using System;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

public class IconDesView : IconDesViewBase
{
    private void Awake()
    {
        InitVariable();
        Btn(_back, () => ModuleManager.Instance.GoBack());
    }

    public void SetData(string title, string c, Texture icon)
    {
        _title.text = title;
        _des.text = c;
        var component = _icon.GetComponent<RectTransform>();
        var num1 = component.GetWidth() / component.GetHeight();
		if(icon==null) return;
        var width = (double)icon.width;
        var height = (double)icon.height;
        var num2 = (float)(width / height);
        if (num1 > num2)
            component.SetWidth(num2 * component.GetHeight());
        else
            component.SetHeight(component.GetWidth() / num2);
        _icon.texture = icon;
    }
}