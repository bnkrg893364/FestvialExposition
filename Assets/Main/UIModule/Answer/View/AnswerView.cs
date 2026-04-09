using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;
using UnityEngine.UI;

public class AnswerView : AnswerViewBase
{
    private List<Toggle> _toggles;

    public Action OnSubmitBtnClicked;
    public Action<int> OnToggleSelected;

    private void Awake()
    {
        InitVariable();

        _back.onClick.AddListener(() => _exitWarningPanel.gameObject.Show());
        Btn(_closeExitWarningBtn, () => _exitWarningPanel.gameObject.Hide());
        Btn(_confirmExitBtn, () =>
        {
            ModuleManager.Instance.GoBack();
        });
        
        _toggles = transform.GetComponentsInChildren<Toggle>(true).ToList();

        // 把 Toggle 的点击事件扔给 Controller
        for (int i = 0; i < _toggles.Count; i++)
        {
            int index = i;
            _toggles[i].onValueChanged.AddListener(isOn =>
            {
                // 如果勾选了，告诉 Controller 选了第几个，+1 是因为选项是 1、2、3、4
                if (isOn) 
                    OnToggleSelected?.Invoke(index + 1); 
            });
        }

        // 把提交按钮的点击事件扔给 Controller
        _pull.onClick.AddListener(() => OnSubmitBtnClicked?.Invoke());


        Btn(_rankEnterBtn, () =>
        {
            _rankPanel.gameObject.Show();
        });
        Btn(_rankCloseBtn, () =>
        {
           _rankPanel.gameObject.Hide(); 
        });
    }

    
    // 下面是提供给Controller调用的UI刷新方法
    /// <summary>
    /// 更新题目文字和四个选项文字
    /// </summary>
    /// <param name="data"></param>
    public void UpdateQuestionUI(DataItem data)
    {
        _title.text = data.Title;
        string[] options = { data.A, data.B, data.C, data.D };
        
        for (int i = 0; i < 4; i++)
        {
            _toggles[i].transform.Find("Label").GetComponent<Text>().text = options[i];
        }
    }

    /// <summary>
    /// 切题时，重置所有 UI 状态
    /// </summary>
    public void ResetUIForNewQuestion()
    {
        _pull.transform.Find("Text").GetComponent<Text>().text = "提交";
        // 取消所有打勾
        _toggles.ForEach(m => m.isOn = false);
        // 隐藏提示框
        _tip.gameObject.SetActive(false);      
    }

    /// <summary>
    /// 显示对错结果
    /// </summary>
    /// <param name="isCorrect"></param>
    /// <param name="msg"></param>
    /// <param name="isLastQuestion"></param>
    public void ShowResult(bool isCorrect, string msg, bool isLastQuestion)
    {
        _tip.gameObject.SetActive(true);
        _tip.text = msg;

        // 如果答对了，要把“提交”按钮的字改掉
        if (isCorrect)
        {
            string btnText = isLastQuestion ? "结束，点击关闭!" : "下一题";
            _pull.transform.Find("Text").GetComponent<Text>().text = btnText;
        }
    }

    /// <summary>
    /// 单纯隐藏提示框
    /// </summary>
    public void HideTip()
    {
        _tip.gameObject.SetActive(false);
    }

    /// <summary>
    /// 更新节日主题背景
    /// </summary>
    public void UpdateTheme(Texture bgTexture, string rankTitle)
    {
        if (bgTexture != null) _bgRawImage.texture = bgTexture;
        _rankTitleText.text = rankTitle;
    }

    /// <summary> 
    /// 更新历史最高分显示 
    /// </summary>
    public void UpdateHighestScoreUI(string msg)
    {
        _highestScoreText.text = $"最高得分：{msg}";
    }

    /// <summary> 
    /// 更新当前实时得分 
    /// </summary>
    public void UpdateCurrentScoreUI(int score)
    {
        _currentScoreText.text = $"当前得分：{score}";
    }

    /// <summary> 
    /// 填充排行榜数据 
    /// </summary>
    public void UpdateLeaderboard(List<string> rankData)
    {
        // 获取挂载了 VerticalLayoutGroup 的父节点
        Transform contextsParent = transform.Find("_btn_rankPanel/Bg/rankContexts");
        
        // 强制刷新前 6 个子物体的内容
        for (int i = 0; i < 6; i++)
        {
            Text txt = contextsParent.GetChild(i).GetComponent<Text>();
            if (i < rankData.Count)
            {
                txt.text = rankData[i];
            }
            else
            {
                txt.text = "暂无";
            }
        }
    }
}