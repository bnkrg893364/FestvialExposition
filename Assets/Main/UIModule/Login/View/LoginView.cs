using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Module;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

public class LoginView : LoginViewBase
{
    // 定义委托，让 Controller 能够监听到按钮被点击了
    public Action<string, string> OnConfirmLoginClick;
    public Action<string, string, string> OnConfirmRegisterClick;
    public Action OnGuestLoginClick;

    private void Awake()
    {
        InitVariable();
        #region 登录相关
        Btn(_loginBtn, () =>
        {
           _loginPanel.gameObject.Show(); 
        });
        Btn(_loginPanelCloseBtn, () =>
        {
            _loginPanel.gameObject.Hide();
        });
        // 把数据传给 Controller 处理登录逻辑
        Btn(_confirmLoginBtn, () =>
        {
            OnConfirmLoginClick?.Invoke(_loginUserNameInput.text, _loginPasswordInput.text);
        });
        #endregion

        #region 注册相关
        Btn(_registerBtn, () =>
        {
            _registerPanel.gameObject.Show();
        });
        Btn(_registerPanelCloseBtn, () =>
        {
            _registerPanel.gameObject.Hide();
        });
        // 把数据传给 Controller 处理注册逻辑
        Btn(_confirmRegisterBtn, () =>
        {
            OnConfirmRegisterClick?.Invoke(_registerUserNameInput.text, _registerPasswordInput.text, _registerConfirmPasswordInput.text);
        });
        #endregion

        #region 游客登录相关
        Btn(_guestLoginBtn, ()=>
        {
            _guestLoginWarning.gameObject.Show();
        });
        Btn(_guestLoginWarningCloseBtn, () =>
        {
            _guestLoginWarning.gameObject.Hide();
        });
        Btn(_guestLoginEnterBtn, () =>
        {
            OnGuestLoginClick?.Invoke();
        });
        #endregion
        // 测试按钮点击
        // Btn(_loginBtn,()=>{
        //     TestButton.Instance.TestButtonClick(_loginBtn);
        // });
        // transform.GetButton("Start").onClick.AddListener(() =>
        // {
        //     SendMessage(new ModuleMessage(ModuleMessageConst.CMD_CLICKSTARTGAME));
        // });
    }

    // 提供给 Controller 调用的 UI 刷新方法
    public void UpdateLoginTip(string msg, Color color)
    {
        _loginResultText.text = msg;
        _loginResultText.color = color;
    }
    public void UpdateRegisterTip(string msg, Color color)
    {
        _registerResultText.text = msg;
        _registerResultText.color = color;
    }

    public void HideRegisterPanel()
    {
        _registerPanel.gameObject.Hide();
    }
}