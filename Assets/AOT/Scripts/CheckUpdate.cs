using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class CheckUpdate : MonoBehaviour
{
    [Header("UI引用")]
    public Text _checkUpdateText;
    public Text _loadingText;
    public Image _confirmDownloadPanel;
    public Text _downloadSizeText;
    public Button _confirmDownloadBtn;

    [Serializable]
    private class CheckCatalogsClass
    {
        public List<string> checkCataLogs = new List<string>();
    }

    private CheckCatalogsClass m_checkCatalogsClass = new CheckCatalogsClass();
    private string downloadCatalogs = "DownloadCatalogs";
    private bool isNeedDownload => m_checkCatalogsClass != null && m_checkCatalogsClass.checkCataLogs != null && m_checkCatalogsClass.checkCataLogs.Count > 0;

    private CancellationTokenSource _textAnimCts; 
    private bool isWaitingForConfirm = false;
    
    // 用来区分当前是“全新更新”还是“断点续传”，从而显示不同的UI文案
    private bool isResuming = false;

    void Start()
    {
        _checkUpdateText.gameObject.SetActive(true);
        _loadingText.gameObject.SetActive(false);
        _confirmDownloadPanel.gameObject.SetActive(false);
        _confirmDownloadBtn.onClick.AddListener(() => isWaitingForConfirm = false);

        StartJobs().Forget();
    }

    private async UniTaskVoid StartJobs()
    {
        _textAnimCts = new CancellationTokenSource();
        PlayTextAnimation(_checkUpdateText, "检查服务器资源更新中", _textAnimCts.Token).Forget();
        
        float startTime = Time.realtimeSinceStartup;

        // 1. 检查服务器目录 (会在这里处理断点续传的标志判断)
        await CheckCataUpdate_New();

        // 2. 如果发现有要更新/继续下载的目录，必须在获取文件大小之前，把它更新到内存里
        if (isNeedDownload)
        {
            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(m_checkCatalogsClass.checkCataLogs, false);
            await updateHandle.Task;
            Addressables.Release(updateHandle);
        }

        // 保证检查更新的动画至少显示 5 秒
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < 5f) 
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5f - elapsed), ignoreTimeScale: true);
        }

        // 3. 计算实际体积并弹窗
        bool actuallyNeedsToDownload = false;
        if (isNeedDownload || isResuming)
        {
            actuallyNeedsToDownload = await PrepareDownloadUI();
        }

        // 准备切换 UI 状态
        if (_textAnimCts != null)
        {
            _textAnimCts.Cancel();
            _textAnimCts.Dispose(); 
            _textAnimCts = new CancellationTokenSource(); 
        }

        _checkUpdateText.gameObject.SetActive(false);
        _loadingText.gameObject.SetActive(true);

        // 4. 下载阶段
        if (actuallyNeedsToDownload)
        {
            PlayTextAnimation(_loadingText, "下载中", _textAnimCts.Token).Forget();
            float downloadStartTime = Time.realtimeSinceStartup;
            
            await DownResources();
            
            float downloadElapsed = Time.realtimeSinceStartup - downloadStartTime;
            if (downloadElapsed < 2f) 
                await UniTask.Delay(TimeSpan.FromSeconds(2f - downloadElapsed), ignoreTimeScale: true);

            if (_textAnimCts != null)
            {
                _textAnimCts.Cancel(); 
                _textAnimCts.Dispose();
                _textAnimCts = new CancellationTokenSource(); 
            }
        }

        // 5. 加载阶段 (加载DLL)
        PlayTextAnimation(_loadingText, "加载中", _textAnimCts.Token).Forget();
        float loadStartTime = Time.realtimeSinceStartup;

        await LoadHotUpdateAssemblies();      

        float loadElapsed = Time.realtimeSinceStartup - loadStartTime;
        if (loadElapsed < 2f) 
            await UniTask.Delay(TimeSpan.FromSeconds(2f - loadElapsed), ignoreTimeScale: true);

        // 6. 进主场景
        if (_textAnimCts != null)
        {
            _textAnimCts.Cancel(); 
            _textAnimCts.Dispose();
            _textAnimCts = null; 
        }
        await LoadMainScene();
    }

    private async UniTask PlayTextAnimation(Text targetText, string baseContent, CancellationToken token)
    {
        int dotCount = 0;
        while (!token.IsCancellationRequested)
        {
            dotCount = (dotCount + 1) % 7; 
            targetText.text = baseContent + new string('·', dotCount);
            
            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token).SuppressCancellationThrow();
            if (isCanceled) break; 
        }
    }

    private async UniTask CheckCataUpdate_New()
    {
        isResuming = false; // 每次检查前重置标志位
        
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            m_checkCatalogsClass.checkCataLogs = checkHandle.Result;
            
            if (isNeedDownload)
            {
                // 有全新的服务器资源需要更新，把字典存到本地
                string jsonStr = JsonUtility.ToJson(m_checkCatalogsClass);
                PlayerPrefs.SetString(downloadCatalogs, jsonStr);
                PlayerPrefs.Save();
            }
            else
            {
                // 没有新资源，但我们要检查上次是不是有下载到一半的文件（断点续传）
                if (PlayerPrefs.HasKey(downloadCatalogs))
                {
                    isResuming = true; // 标记为续传状态
                    string strJson = PlayerPrefs.GetString(downloadCatalogs);
                    JsonUtility.FromJsonOverwrite(strJson, m_checkCatalogsClass);
                }
            }
        }
        Addressables.Release(checkHandle);
    }

    private async UniTask<bool> PrepareDownloadUI()
    {
        bool willDownload = false; 
        
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(new[] { "HotUpdate" });
        await sizeHandle.Task;

        if (sizeHandle.Result > 0)
        {
            willDownload = true; 
            _confirmDownloadPanel.gameObject.SetActive(true);
            string sizeStr = FormatBytes(sizeHandle.Result);
            
            // 根据标志位显示不同提示文本
            if (isResuming)
                _downloadSizeText.text = "检测到尚未更新完毕的资源文件，是否继续？";
            else
                _downloadSizeText.text = $"共计更新 {sizeStr} 大小资源文件，是否继续？";

            isWaitingForConfirm = true;
            await UniTask.WaitWhile(() => isWaitingForConfirm);
            
            _confirmDownloadPanel.gameObject.SetActive(false);
        }
        Addressables.Release(sizeHandle);
        
        return willDownload; 
    }

    private async UniTask DownResources()
    {
        AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(new[] { "HotUpdate" }, Addressables.MergeMode.Union, false);
        await downloadHandle.Task;

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            PlayerPrefs.DeleteKey(downloadCatalogs);
            PlayerPrefs.Save();
        }
        Addressables.Release(downloadHandle);
    }

    private async UniTask LoadHotUpdateAssemblies()
    {
        
        var hotDllHandle = Addressables.LoadAssetAsync<TextAsset>("HotUpdate.dll");
        await hotDllHandle.Task;
        
        if (hotDllHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Assembly hotAss = Assembly.Load(hotDllHandle.Result.bytes);
            Debug.Log("热更新程序集加载成功");
        }
    }

    private async UniTask LoadMainScene()
    {
        await SceneManager.LoadSceneAsync("Scenes/Main").ToUniTask();
    }

    private string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{(bytes / 1024f):F2} KB";
        return $"{(bytes / (1024f * 1024f)):F2} MB";
    }

    private void OnDestroy()
    {
        if (_textAnimCts != null)
        {
            _textAnimCts.Cancel();
            _textAnimCts.Dispose();
            _textAnimCts = null;
        }
    }
}