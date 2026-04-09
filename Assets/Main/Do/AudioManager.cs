using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频管理器
/// </summary>
public class AudioManager : MonoBehaviour
{
    //单例模式
    public static AudioManager Instance;
    private AudioSource BgAudio;

    private void Awake()
    {
        if (AudioManager.Instance == null)
        {
            AudioManager.Instance = this;
            DontDestroyOnLoad(this.gameObject);
            BgAudio = transform.GetComponent<AudioSource>();
        }
        else
        {
            Destroy(this.gameObject);
            Debug.LogError("禁止创建多个单例模式对象！"+this.name);
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBg()
    {
        //避免对UnityObject使用?.判空方法，改为if判断
        //BgAudio?.Play();
        if (BgAudio != null)
        {
            BgAudio.Play();
        }
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBg()
    {
        //BgAudio?.Pause();
        if (BgAudio != null)
        {
            BgAudio.Pause();
        }
    }
    
}