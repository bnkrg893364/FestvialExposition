using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Framework.GalaSports.Core;

public class BackgroundUpdateMonitor : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject updateAlertPanel;
    public Text countdownText;          // 面板上的倒计时文字

    [Header("监视设置")]
    public float checkIntervalMinutes = 5f; // 每隔一段静默检查一次

    private CancellationTokenSource _monitorCts;

    void Start()
    {
        updateAlertPanel.SetActive(false);
        _monitorCts = new CancellationTokenSource();
        
        // 游戏启动后，开启后台轮回任务
        StartPolling(_monitorCts.Token).Forget();
    }

    private async UniTaskVoid StartPolling(CancellationToken token)
    {
        // 刚进游戏先别查，让玩家先玩一会儿，延迟一段时间后再开始第一次检查
        await UniTask.Delay(TimeSpan.FromMinutes(0.1), cancellationToken: token);

        while (!token.IsCancellationRequested)
        {
            // 静默检查更新
            bool hasUpdate = await CheckUpdateSilent();

            if (hasUpdate)
            {
                // 解锁鼠标锁定
                // 强制解锁鼠标并显示鼠标指针，防止跨场景后鼠标依然锁定
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                // 发现更新！触发强制重启机制，跳出死循环
                await TriggerForceReboot();
                break; 
            }

            // 没发现更新，休眠指定的分钟数后，再查下一次
            await UniTask.Delay(TimeSpan.FromMinutes(checkIntervalMinutes), cancellationToken: token);
        }
    }

    /// <summary>
    /// 静默检查目录更新
    /// </summary>
    private async UniTask<bool> CheckUpdateSilent()
    {
        bool hasUpdate = false;
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
        
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            if (checkHandle.Result != null && checkHandle.Result.Count > 0)
            {
                hasUpdate = true; // 发现服务器有新的资源目录
            }
        }
        
        Addressables.Release(checkHandle);
        return hasUpdate;
    }

    /// <summary>
    /// 触发强制退回初始场景的逻辑
    /// </summary>
    private async UniTask TriggerForceReboot()
    {
        updateAlertPanel.SetActive(true);

        // 冻结游戏时间（如果有3D角色在跑、或者有视频在放，直接全部暂停）
        Time.timeScale = 0f;

        // 倒计时 3 秒
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = $"有更新内容，系统将在 {i} 秒后强制返回主界面进行更新";
            }
            // 因为 Time.timeScale 是 0，必须用 ignoreTimeScale: true 才能正常倒数
            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true);
        }

        // 恢复时间
        Time.timeScale = 1f;

        // 销毁当前所有的 MVC 模块，防止 UI 残留
        ModuleManager.Instance.DestroyAllModule(); 

        // 重新加载热更新场景
        SceneManager.LoadScene("InitialScene"); 
    }

    private void OnDestroy()
    {
        if (_monitorCts != null)
        {
            _monitorCts.Cancel();
            _monitorCts.Dispose();
            _monitorCts = null;
        }
    }
}