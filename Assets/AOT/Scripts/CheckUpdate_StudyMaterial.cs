using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CheckUpdate_StudyMaterial : MonoBehaviour
{
    [Serializable]
    private class CheckCatalogsClass
    {
        public List<string> checkCataLogs = new List<string>();
    }

    private CheckCatalogsClass m_checkCatalogsClass = new CheckCatalogsClass();
    private string downloadCatalogs = "DownloadCatalogs";
    private bool isNeedDownload => m_checkCatalogsClass != null && m_checkCatalogsClass.checkCataLogs != null && m_checkCatalogsClass.checkCataLogs.Count > 0;

    //private List<object> locatorKeys = new List<object>(100);

    // Start is called before the first frame update
    void Start()
    {
        // 强制清空本地所有的 AssetBundle 缓存
        Caching.ClearCache();
        // 清除上一次的持久化数据
        PlayerPrefs.DeleteKey(downloadCatalogs);
        PlayerPrefs.Save();

        // 1.检查目录更新
        //Addressables.CheckForCatalogUpdates();
        // 2.更新资源目录
        //Addressables.UpdateCatalogs();
        // 3.获取资源定位器的Key
        //IResourceLocator locator;
        //locator.Key
        // 4.下载对应资源
        //Addressables.DownloadDependenciesAsync();
        //StartCoroutine(CheckCataUpdate_Old());
        //StartCoroutine(StartJobs());
        
        // 更新流程一般是先下载游戏资源，再更新资源目录，而HybirdCLR流程与之不同，需要修改

        // 修改：
        // 2.更新目录，并将要更新的目录数据持久化保存到本地
        // 检查服务器目录更新，再检查本地目录更新
        // 资源下载完毕后再删除保存的本地目录

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator StartJobs()
    {
        yield return CheckCataUpdate_New();

        yield return LoadCube();

        yield return new WaitForSeconds(3);

        yield return LoadScene();
    }

    // 1.检查目录更新(修改后的方法)
    private IEnumerator CheckCataUpdate_New()
    {
        // false 手动释放句柄
        AsyncOperationHandle<List<string>> checkCataLogsHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkCataLogsHandle;

        if (checkCataLogsHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("检查目录更新成功！");
            m_checkCatalogsClass.checkCataLogs= checkCataLogsHandle.Result;

            if (isNeedDownload)
            {
                // 检查服务器更新，并将数据保存到本地
                Debug.Log("服务器有资源需要更新！");
                string jsonStr = JsonUtility.ToJson(m_checkCatalogsClass);
                PlayerPrefs.SetString(downloadCatalogs, jsonStr);
                
            }
            else
            {
                // 检查上次保存的本地更新
                if (PlayerPrefs.HasKey(downloadCatalogs))
                {
                    Debug.Log("继续完成未更新的资源！");
                    string strJson = PlayerPrefs.GetString(downloadCatalogs);
                    JsonUtility.FromJsonOverwrite(strJson, m_checkCatalogsClass);
                }
            }

            if (isNeedDownload)
            {
                Debug.Log("需要下载的资源个数为" + m_checkCatalogsClass.checkCataLogs.Count);

                yield return UpdateCata(m_checkCatalogsClass.checkCataLogs);

            }
            else
            {
                Debug.Log("所有资源全部下载完成！");
            }
        }
        else
        {
            Debug.Log("检查目录更新失败！");
        }
        Addressables.Release(checkCataLogsHandle);
    }

    // 1.检查目录更新
    private IEnumerator CheckCataUpdate_Old()
    {
        // false 手动释放句柄
        AsyncOperationHandle<List<string>> checkCataLogsHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkCataLogsHandle;

        if (checkCataLogsHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("检查目录更新成功！");
            List<string> checkCataLogs = checkCataLogsHandle.Result;

            if (checkCataLogs != null && checkCataLogs.Count > 0)
            {
                // 需要更新目录
                Debug.Log("需要更新目录，更新资源个数为："+checkCataLogs.Count);
                StartCoroutine(UpdateCata(checkCataLogs));
            }
            else
            {
                // 无需更新目录
                Debug.Log("无需更新目录!");
            }

        }
        else
        {
            Debug.Log("检查目录更新失败！");
        }
        Addressables.Release(checkCataLogsHandle);
    }
    
    // 2.更新资源目录
    private IEnumerator UpdateCata(List<string> cataLogs)
    {
        AsyncOperationHandle<List<IResourceLocator>> updateCataLogsHandle =  Addressables.UpdateCatalogs(cataLogs,false);
        yield return updateCataLogsHandle;

        if (updateCataLogsHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("资源目录更新成功！");
            //List<IResourceLocator> resourceLocators = updateCataLogsHandle.Result;
            Addressables.Release(updateCataLogsHandle);

            //GetResourceLocatorKeys(resourceLocators);

            yield return DownResources();

        }
        else
        {
            Debug.Log("资源目录更新失败！");
        }
    }

    //// 3.获取资源定位器的Key
    //private void GetResourceLocatorKeys(List<IResourceLocator> resourceLocators)
    //{
    //    // 清空上一次的Key
    //    locatorKeys.Clear();

    //    foreach (var locator in resourceLocators)
    //    {
    //        locatorKeys.AddRange(locator.Keys);
    //    }
    //}

    // 4.下载对应资源
    private IEnumerator DownResources()
    {
        // 判断是否需要下载
        AsyncOperationHandle<long> downloadSizeHandle = Addressables.GetDownloadSizeAsync("HotUpdate");
        yield return downloadSizeHandle;

        if (downloadSizeHandle.Status == AsyncOperationStatus.Succeeded && downloadSizeHandle.Result > 0)
        {
            long sizeInBytes = downloadSizeHandle.Result;
            string sizeText;
            if (sizeInBytes < 1024)
                sizeText = $"{sizeInBytes} B";
            else if (sizeInBytes < 1024 * 1024)
                sizeText = $"{(sizeInBytes / 1024f):F2} KB";
            else if (sizeInBytes < 1024 * 1024 * 1024)
                sizeText = $"{(sizeInBytes / (1024f * 1024f)):F2} MB";
            else
                sizeText = $"{(sizeInBytes / (1024f * 1024f * 1024f)):F2} GB";

            Debug.Log("需要下载资源，大小为：" + sizeText);

            //  下载资源
            // Addressables.MergeMode 资源合并方式参数，重要，不要遗漏！
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(new[] {"HotUpdate"},Addressables.MergeMode.Union,false);
            yield return downloadHandle;

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("下载成功！");
                PlayerPrefs.DeleteKey(downloadCatalogs);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("下载失败！");
            }
            Addressables.Release(downloadHandle);
        }
        else
        {
            Debug.Log("不需要下载资源！");
        }
        Addressables.Release(downloadSizeHandle);
    }

    /// <summary>
    /// 目标：通过加载热更新的游戏物体，来执行热更新的代码
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadCube()
    {
        // 先加载热更新的程序集
        AsyncOperationHandle<TextAsset> dllHandle = Addressables.LoadAssetAsync<TextAsset>("HotUpdate.dll");
        dllHandle.WaitForCompletion();

        var dll = dllHandle.Result;

        // 加载程序集
        Assembly.Load(dll.bytes);

        // 加载游戏资源
        AsyncOperationHandle<GameObject> gameObjectHandle = Addressables.LoadAssetAsync<GameObject>("Cube");
        yield return gameObjectHandle;

        if (gameObjectHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("游戏物体加载成功！");
            GameObject cube = gameObjectHandle.Result;
            Instantiate(cube);
        }
        else
        {
            Debug.Log("游戏物体加载失败！");

        }
        
    }

    // 加载场景
    private IEnumerator LoadScene()
    {
        var sceneHandle = Addressables.LoadSceneAsync("TestHotUpdateScene");
        yield return sceneHandle;

        // 如果热更场景成功加载，则会跳转对应场景，因此以下逻辑判断场景加载失败的情况
        if (sceneHandle.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log("场景加载失败！");
            yield break;
        }

        Debug.Log("场景加载成功！");
    }
}
