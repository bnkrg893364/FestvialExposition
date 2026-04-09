using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Module;
using UnityEngine;

public class GameStart : MonoBehaviour
{ 
    private void Awake()
    {
        
        // 游戏开始进入的模块
        RoomLoop.StartModuleName = ModuleConfig.MODULE_LOGIN;
            
        // 进入模块时调用
        ModuleManager.Instance.EnterModuleCb(() => Player._.Pause());
        // 退出所有模块时调用
        ModuleManager.Instance.ExitAllModuleCb(() => Player._.ReStart());
            
        gameObject.AddComponent<GameMain>();
    }
}