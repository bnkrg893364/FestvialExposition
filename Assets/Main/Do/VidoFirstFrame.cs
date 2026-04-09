using UnityEngine;
using UnityEngine.Video;
using Cysharp.Threading.Tasks;

/// <summary>
/// 获取视频第一帧画面，用作视频封面
/// </summary>
public class VideoFirstFrame : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        await UniTask.Yield(destroyCancellationToken); 
        // 暂停视频，留下第一帧当封面
        this.gameObject.GetComponent<VideoPlayer>().Pause();
    }
}