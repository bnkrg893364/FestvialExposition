using Assets.Scripts.Common;
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class HomeViewBase:View {
	/// <summary>
	/// 进入游戏程序按钮
	/// </summary>
	protected Button _enterBtn;
	/// <summary>
	/// 进入游戏说明按钮
	/// </summary>
	protected Button _desBtn;
	/// <summary>
	/// 退出游戏程序按钮
	/// </summary>
	protected Button _exitBtn;
	/// <summary>
	/// 游戏说明按钮（本质是一个面板，上面挂有Button组件，占满整个屏幕，防止与其他UI交互）
	/// </summary>
	protected Button _des;
	/// <summary>
	/// 退出游戏说明按钮
	/// </summary>
	protected Button _desClose;

	protected void InitVariable() {
		_enterBtn  = transform.Find("Image/_btn_EnterBtn").GetComponent<Button>();
		_desBtn  = transform.Find("Image/_btn_desBtn").GetComponent<Button>();
		_exitBtn  = transform.Find("Image/_btn_ExitBtn").GetComponent<Button>();
		_des  = transform.Find("_btn_des").GetComponent<Button>();
		_desClose  = transform.Find("_btn_des/Bg/_btn_desClose").GetComponent<Button>();
		
	}

}