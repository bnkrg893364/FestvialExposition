using Assets.Scripts.Common;
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class IconDesViewBase:View {
	protected RawImage _icon;
	protected Text _title;
	protected Text _des;
	protected Button _back;

	protected void InitVariable() {
		_icon  = transform.Find("_raw_icon").GetComponent<RawImage>();
		_title  = transform.Find("_txt_title").GetComponent<Text>();
		_des  = transform.Find("_txt_des").GetComponent<Text>();
		_back  = transform.Find("_btn_back").GetComponent<Button>();
		
	}

}