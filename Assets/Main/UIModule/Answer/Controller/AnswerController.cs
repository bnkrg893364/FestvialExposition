using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class AnswerController : Controller 
{
    public AnswerView answerView;

    // 存题目、存当前进度
    private List<DataItem> _datas = new List<DataItem>();
    private int _index = -1;
    private int _currentSelect = 0;
    private bool _currentRight = false;
    private int _currentScore = 0;
    private int _highestScore = 0;
    private int _wrongAttempts = 0;

    // 【新增】用来保存背景图片的句柄，防止被提前卸载变白块
    private AsyncOperationHandle<Texture> _bgTextureHandle;

    // 模块启动时，框架会自动调用 Start
    public override void Start()
    {
        answerView.OnSubmitBtnClicked = HandleSubmitClick;
        answerView.OnToggleSelected = HandleToggleSelect;

        // 【修改】因为有了 Addressables 异步加载，我们把初始化流程放进异步方法里
        InitProcessAsync().Forget();
    }

    /// <summary>
    /// 全新的异步初始化主流程
    /// </summary>
    private async UniTaskVoid InitProcessAsync()
    {
        // 1. 获取对应的热更资源路径和标题
        string textFileName = "";
        string rankTitle = "";
        string bgResPath = ""; 

        switch (UserSession.CurrentFestival)
        {
            case FestivalType.MidAutumn:
                textFileName = "Timu_MidAutumn"; // 对应 Addressables 里的 Name
                rankTitle = "中秋问答积分榜（只显示前6名）";
                bgResPath = "zhongQiu_Bg"; // 对应 Addressables 里的 Name
                break;
            case FestivalType.DragonBoat:
                textFileName = "Timu_DragonBoat";
                rankTitle = "端午问答积分榜（只显示前6名）";
                bgResPath = "duanWu_Bg";
                break;
            case FestivalType.SpringFestival:
                textFileName = "Timu_Spring";
                rankTitle = "春节问答积分榜（只显示前6名）";
                bgResPath = "chunJie_Bg";
                break;
        }

        // 2. 异步并行加载背景图和题库 (并发加载，速度更快)
        var bgTask = LoadThemeAsync(bgResPath, rankTitle);
        var dataTask = LoadDataAsync(textFileName);
        await UniTask.WhenAll(bgTask, dataTask);

        // 3. 加载排行榜
        List<string> top6 = MySQLManager.GetTop6Leaderboard(UserSession.CurrentFestival);
        answerView.UpdateLeaderboard(top6);

        // 4. 初始化分数显示
        InitScores();

        // 5. 资源都准备好了，开始发卷子！
        StartQuestion();
    }

    /// <summary>
    /// 异步加载背景图并更新 View 主题
    /// </summary>
    private async UniTask LoadThemeAsync(string bgResPath, string rankTitle)
    {
        _bgTextureHandle = Addressables.LoadAssetAsync<Texture>(bgResPath);
        await _bgTextureHandle.Task;

        if (_bgTextureHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Texture bgTex = _bgTextureHandle.Result;
            answerView.UpdateTheme(bgTex, rankTitle);
        }
        else
        {
            Debug.LogError($"热更背景图片加载失败: {bgResPath}");
            // 就算图片加载失败，也要把标题更新上去
            answerView.UpdateTheme(null, rankTitle);
        }
    }

    /// <summary>
    /// 异步数据加载逻辑
    /// </summary>
    private async UniTask LoadDataAsync(string fileName)
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(fileName);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var txt = handle.Result;
            var temp = txt.text.Replace(" ", "").Replace("\n", "");
            var lines = temp.Split(';').ToList();
            lines = lines.FindAll(m => !string.IsNullOrEmpty(m));

            _datas.Clear();

            for (int i = 0; i < lines.Count; i++)
            {
                var v = lines[i].Split('|');
                if(v.Length >= 6)
                {
                    _datas.Add(new DataItem(v[0], v[1], v[2], v[3], v[4], v[5]));
                }
            }
            
            // 【重点】数据已经转成 DataItem 存进列表里了，文本文件可以立刻从内存中释放掉
            Addressables.Release(handle);
        }
        else
        {
            Debug.LogError($"热更题库文件加载失败: {fileName}");
            Addressables.Release(handle);
        }
    }

    private void InitScores()
    {
        _currentScore = 0;
        answerView.UpdateCurrentScoreUI(_currentScore);

        if (UserSession.IsGuest)
        {
            answerView.UpdateHighestScoreUI("未登录");
        }
        else
        {
            _highestScore = MySQLManager.GetHighestScore(UserSession.UserId, UserSession.CurrentFestival);
            if (_highestScore == 0)
                answerView.UpdateHighestScoreUI("未记录");
            else
                answerView.UpdateHighestScoreUI(_highestScore.ToString());
        }
    }

    private void StartQuestion()
    {
        _index = -1;
        NextQuestion();
    }

    private void NextQuestion()
    {
        _index++;
        
        if (_index >= _datas.Count)
        {
            if (!UserSession.IsGuest && _currentScore > _highestScore)
            {
                MySQLManager.UpdateHighestScore(UserSession.UserId, UserSession.CurrentFestival, _currentScore);
                Debug.Log($"新纪录！上传分数：{_currentScore}");
            }
            ModuleManager.Instance.GoBack();
            return;
        }

        _currentRight = false;
        _currentSelect = 0;
        _wrongAttempts = 0;

        answerView.UpdateQuestionUI(_datas[_index]);
        answerView.ResetUIForNewQuestion();
    }

    private void HandleToggleSelect(int selectIndex)
    {
        _currentSelect = selectIndex;
        answerView.HideTip(); 
    }

    private void HandleSubmitClick()
    {
        if (!_currentRight) 
        {
            var currentData = _datas[_index];
            
            if (_currentSelect == currentData.Answer) 
            {
                _currentRight = true;
                int earned = 0;
                if (_wrongAttempts == 0) 
                    earned = 10;
                else if (_wrongAttempts == 1) 
                    earned = 5;
                else if (_wrongAttempts == 2) 
                    earned = 3;
                else 
                    earned = 1;

                _currentScore += earned;
                answerView.UpdateCurrentScoreUI(_currentScore);

                bool isLast = _index >= _datas.Count - 1; 
                answerView.ShowResult(true, $"恭喜正确！(加{earned}分)", isLast);
            }
            else 
            {
                _wrongAttempts++;
                answerView.ShowResult(false, "选择错误！", false);
            }
        }
        else 
        {
            NextQuestion();
        }
    }

    // 【新增】重写 Destroy 方法，用于释放持续显示的热更图片
    public override void Destroy()
    {
        base.Destroy();
        
        // 玩家离开答题模块时，释放背景图片占用的显存
        if (_bgTextureHandle.IsValid())
        {
            Addressables.Release(_bgTextureHandle);
        }
    }
}