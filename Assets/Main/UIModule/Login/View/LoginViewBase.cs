using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine.UI;

public class LoginViewBase:View
{
    /// <summary>
    /// 登录按钮
    /// </summary>
    protected Button _loginBtn;
    /// <summary>
    /// 注册按钮
    /// </summary>
    protected Button _registerBtn;
    /// <summary>
    /// 游客登录按钮
    /// </summary>
    protected Button _guestLoginBtn;
    
    #region 游客登录相关
    /// <summary>
    /// 游客登录警告面板
    /// </summary>
    protected Button _guestLoginWarning;
    /// <summary>
    /// 关闭游客登录警告面板按钮
    /// </summary>
    protected Button _guestLoginWarningCloseBtn;
    /// <summary>
    /// 确认游客登录按钮
    /// </summary>
    protected Button _guestLoginEnterBtn;
    #endregion

    #region 登录相关
    /// <summary>
    /// 登录面板
    /// </summary>
    protected Button _loginPanel;
    /// <summary>
    /// 关闭登录面板按钮
    /// </summary>
    protected Button _loginPanelCloseBtn;
    /// <summary>
    /// 确认登录按钮
    /// </summary>
    protected Button _confirmLoginBtn;
    /// <summary>
    /// 登录用户名输入框
    /// </summary>
    protected InputField _loginUserNameInput;
    /// <summary>
    /// 登录密码输入框
    /// </summary>
    protected InputField _loginPasswordInput;
    /// <summary>
    /// 登录结果文本
    /// </summary>
    protected Text _loginResultText;
    #endregion

    #region 注册相关
    /// <summary>
    /// 注册面板
    /// </summary>
    protected Button _registerPanel;
    /// <summary>
    /// 关闭注册面板按钮
    /// </summary>
    protected Button _registerPanelCloseBtn;
    /// <summary>
    /// 确认注册按钮
    /// </summary>
    protected Button _confirmRegisterBtn;
    /// <summary>
    /// 注册用户名输入框
    /// </summary>
    protected InputField _registerUserNameInput;
    /// <summary>
    /// 注册密码输入框
    /// </summary>
    protected InputField _registerPasswordInput;
    /// <summary>
    /// 确认注册密码输入框
    /// </summary>
    protected InputField _registerConfirmPasswordInput;
    /// <summary>
    /// 注册结果文本
    /// </summary>
    protected Text _registerResultText;
    #endregion

    protected void InitVariable()
    {
        _loginBtn = transform.Find("Btns/_btn_LoginBtn").GetComponent<Button>();
        _registerBtn = transform.Find("Btns/_btn_RegisterBtn").GetComponent<Button>();
        _guestLoginBtn = transform.Find("Btns/_btn_GuestLoginBtn").GetComponent<Button>();
        
        _guestLoginWarning = transform.Find("_btn_GuestLoginWarning").GetComponent<Button>();
        _guestLoginWarningCloseBtn = transform.Find("_btn_GuestLoginWarning/Bg/_btn_desClose").GetComponent<Button>();
        _guestLoginEnterBtn = transform.Find("_btn_GuestLoginWarning/Bg/_btn_ConfirmGuestLogin").GetComponent<Button>();
        
        _loginPanel = transform.Find("_btn_LoginPanel").GetComponent<Button>();
        _loginPanelCloseBtn = transform.Find("_btn_LoginPanel/Bg/_btn_desClose").GetComponent<Button>();
        _confirmLoginBtn = transform.Find("_btn_LoginPanel/Bg/_btn_ConfirmLogin").GetComponent<Button>();
        _loginUserNameInput = transform.Find("_btn_LoginPanel/Bg/UserName/InputField").GetComponent<InputField>();
        _loginPasswordInput = transform.Find("_btn_LoginPanel/Bg/Password/InputField").GetComponent<InputField>();
        _loginResultText = transform.Find("_btn_LoginPanel/Bg/TextTip").GetComponent<Text>();
        
        _registerPanel = transform.Find("_btn_RegisterPanel").GetComponent<Button>();
        _registerPanelCloseBtn = transform.Find("_btn_RegisterPanel/Bg/_btn_desClose").GetComponent<Button>();
        _confirmRegisterBtn = transform.Find("_btn_RegisterPanel/Bg/_btn_ConfirmRegister").GetComponent<Button>();
        _registerUserNameInput = transform.Find("_btn_RegisterPanel/Bg/UserName/InputField").GetComponent<InputField>();
        _registerPasswordInput = transform.Find("_btn_RegisterPanel/Bg/Password/InputField").GetComponent<InputField>();
        _registerConfirmPasswordInput = transform.Find("_btn_RegisterPanel/Bg/ConfirmPassword/InputField").GetComponent<InputField>();
        _registerResultText = transform.Find("_btn_RegisterPanel/Bg/TextTip").GetComponent<Text>();
    }
}