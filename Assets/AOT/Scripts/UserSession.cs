using UnityEngine;

public enum FestivalType
{ 
    MidAutumn, 
    DragonBoat, 
    SpringFestival
}

public static class UserSession
{
    public static bool IsGuest { get; private set; } = true;
    public static int UserId { get; private set; } = -1;
    public static string Username { get; private set; } = string.Empty;
    /// <summary>
    /// 记录当前玩家正在游玩哪个节日展区
    /// </summary>
    public static FestivalType CurrentFestival { get; set; } = FestivalType.MidAutumn;

    // 正常登录成功后调用
    public static void SetUser(int id, string name)
    {
        IsGuest = false;
        UserId = id;
        Username = name;
        Debug.Log($"玩家上线: {Username}, ID: {UserId}");
    }

    // 游客登录时调用
    public static void SetGuest()
    {
        IsGuest = true;
        UserId = -1;
        Username = "游客";
        Debug.Log("游客上线");
    }
}