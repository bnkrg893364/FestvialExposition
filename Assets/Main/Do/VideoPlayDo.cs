using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayDo : DoBase
{
    private VideoPlayer _video;


    private void Awake()
    {
        _video = transform.GetComponent<VideoPlayer>();
    }

    /// <summary>
    /// 暂停视频
    /// </summary>
    public void Pause()
    {
        _video.Pause();
    }

    private void OnMouseDown()
    {
        // 点击在了UI元素上，不处理
        if(IsPointerOverUI()) return;


        if (!_video.isPlaying)
        {
            //播放之前，把场景里其他所有视频全停掉
            GameObject.FindObjectsOfType<VideoPlayDo>().ToList().ForEach(m => m.Pause());
            //播放当前视频
            _video.Play();
            //暂停背景音乐
            AudioManager.Instance?.PauseBg();
        }
        else
        {
            //暂停当前视频
            _video.Pause();
            //播放背景音乐
            AudioManager.Instance?.PlayBg();
        }
    }
    
}