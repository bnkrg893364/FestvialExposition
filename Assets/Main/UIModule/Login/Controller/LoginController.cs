using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Module;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoginController : Controller
{
    public LoginView loginView;
    
    // 框架初始化时会调用 Start
    public override void Start()
    {
        // 订阅 View 层的点击事件
        loginView.OnConfirmLoginClick = HandleLogin;
        loginView.OnConfirmRegisterClick = HandleRegister;
        loginView.OnGuestLoginClick = HandleGuestLogin;
    }

    private async void HandleLogin(string user, string pass)
    {
        // 去掉首尾空格
        user = user.Trim(); 

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            loginView.UpdateLoginTip("账号或密码不能为空", Color.red);
            await UniTask.Delay(3000);
            loginView.UpdateLoginTip("", Color.white); // 3秒后清空
            return;
        }

        // 准备一个变量来接收数据库传回来的 ID
        int loggedInUserId;

        // 调用 Model 层查数据库
        LoginState state = MySQLManager.Login(user, pass, out loggedInUserId);

        switch (state)
        {
            case LoginState.UserNotExist:
                loginView.UpdateLoginTip("不存在用户名", Color.red);
                await UniTask.Delay(3000);
                loginView.UpdateLoginTip("", Color.white);
                break;

            case LoginState.PasswordError:
                loginView.UpdateLoginTip("密码错误", Color.red);
                await UniTask.Delay(3000);
                loginView.UpdateLoginTip("", Color.white);
                break;

            case LoginState.Success:
                loginView.UpdateLoginTip("登录成功", Color.green);

                // 设置玩家令牌
                UserSession.SetUser(loggedInUserId, user);

                await UniTask.Delay(3000);
                loginView.UpdateLoginTip("", Color.white);
                ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_HOME, true);
                break;

            case LoginState.DbError:
                loginView.UpdateLoginTip("数据库连接失败", Color.red);
                await UniTask.Delay(3000);
                loginView.UpdateLoginTip("", Color.white);
                break;
        }
    }

    private async void HandleRegister(string user, string pass, string confirmPass)
    {
        user = user.Trim();

        if (pass != confirmPass)
        {
            loginView.UpdateRegisterTip("密码前后不一致", Color.red);
            await UniTask.Delay(3000);
            loginView.UpdateRegisterTip("", Color.white);
            return;
        }

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            loginView.UpdateRegisterTip("账号或密码不能为空", Color.red);
            await UniTask.Delay(3000);
            loginView.UpdateRegisterTip("", Color.white);
            return;
        }

        RegisterState state = MySQLManager.Register(user, pass);

        switch (state)
        {
            case RegisterState.UserExist:
                loginView.UpdateRegisterTip("已存在用户名", Color.red);
                await UniTask.Delay(3000);
                loginView.UpdateRegisterTip("", Color.white);
                break;

            case RegisterState.Success:
                loginView.UpdateRegisterTip("注册成功，3秒后自动返回登录界面", Color.green);
                await UniTask.Delay(3000);
                loginView.UpdateRegisterTip("", Color.white);
                loginView.HideRegisterPanel();
                break;
        }
    }

    private void HandleGuestLogin()
    {
        // 设置游客令牌
        UserSession.SetGuest(); 
        ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_HOME, true);
    }

    // public override void OnMessage(ModuleMessage message)
    // {
    //     switch (message.Name)
    //     {
    //         case ModuleMessageConst.CMD_CLICKSTARTGAME:
    //             loginView.Show();
    //             break;
    //     }
    // }
}