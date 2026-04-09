using Assets.Scripts.Common;
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using Assets.Scripts.Framework.GalaSports.Core;

public class AnswerViewBase:View {

	protected RawImage _bgRawImage;
	protected Button _pull;
	protected Text _title;
	protected Text _tip;
	protected Button _back;

	protected Text _highestScoreText;
	protected Text _currentScoreText;

	protected Button _exitWarningPanel;
	protected Button _closeExitWarningBtn;
	protected Button _confirmExitBtn;

	protected Button _rankEnterBtn;
	protected Button _rankPanel;
	protected Button _rankCloseBtn;
	protected Text _rankTitleText;

	protected void InitVariable() {
		_bgRawImage = transform.Find("answerBg").GetComponent<RawImage>();

		_pull  = transform.Find("_btn_pull").GetComponent<Button>();
		_title  = transform.Find("_txt_title").GetComponent<Text>();
		_tip  = transform.Find("_txt_tip").GetComponent<Text>();
		_back  = transform.Find("_btn_back").GetComponent<Button>();
		
		_highestScoreText = transform.Find("Scores/_text_highestScore").GetComponent<Text>();
		_currentScoreText = transform.Find("Scores/_text_currentScore").GetComponent<Text>();

		_exitWarningPanel = transform.Find("_btn_exitWarning").GetComponent<Button>();
		_closeExitWarningBtn = transform.Find("_btn_exitWarning/Bg/_btn_desClose").GetComponent<Button>();
		_confirmExitBtn = transform.Find("_btn_exitWarning/Bg/_btn_confirmExit").GetComponent<Button>();

		_rankEnterBtn = transform.Find("_btn_rankEnter").GetComponent<Button>();
		_rankPanel = transform.Find("_btn_rankPanel").GetComponent<Button>();
		_rankCloseBtn = transform.Find("_btn_rankPanel/Bg/_btn_desClose").GetComponent<Button>();
		_rankTitleText = transform.Find("_btn_rankPanel/Bg/_text_rankTitle").GetComponent<Text>();
	}
}