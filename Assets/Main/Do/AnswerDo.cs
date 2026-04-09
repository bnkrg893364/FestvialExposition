using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Module;
using UnityEngine;

public class AnswerDo : DoBase
{
    public FestivalType festivalType;
    
    private void OnMouseDown()
    {
        if (!IsPointerOverUI())
        {
            UserSession.CurrentFestival = festivalType;
            ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_ANSWER);
        }
    }
}