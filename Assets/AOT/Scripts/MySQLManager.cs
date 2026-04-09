using System;
using MySqlConnector;
using UnityEngine;
using System.Collections.Generic;

// 定义登录和注册的状态枚举
public enum LoginState
{ 
    Success, 
    UserNotExist,
    PasswordError,
    DbError
}
public enum RegisterState
{ 
    Success, 
    UserExist, 
    DbError
}

public static class MySQLManager
{
    // 连接配置
    private static string connStr = "server=127.0.0.1;port=3306;user=root;password=123456;database=festival_exposition;charset=utf8mb4;";
    
    //out int userId，用来把数据库里的 ID 传回给 Controller
    public static LoginState Login(string username, string password, out int userId)
    {   
        // 默认游客登录
        userId = -1; 
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                // 同时查出 id 和 password
                string sql = "SELECT id, password FROM users WHERE username = @user";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    
                    // 使用 DataReader 来读取多列数据
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) 
                        {
                            // 账号不存在
                            return LoginState.UserNotExist;
                        }

                        // 读到了，开始核对密码
                        string dbPassword = reader["password"].ToString();
                        if (dbPassword == password)
                        {
                            userId = Convert.ToInt32(reader["id"]); // 把 ID 提取出来
                            return LoginState.Success;
                        }
                        // 密码错误
                        return LoginState.PasswordError; 
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("数据库登录异常: " + e.Message);
                return LoginState.DbError;
            }
        }
    }

    public static RegisterState Register(string username, string password)
    {
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                // 1. 先检查用户名是否被占用
                string checkSql = "SELECT COUNT(*) FROM users WHERE username = @user";
                using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@user", username);
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        return RegisterState.UserExist;
                }

                // 2. 插入新用户
                string insertSql = "INSERT INTO users (username, password) VALUES (@user, @pass)";
                using (MySqlCommand insertCmd = new MySqlCommand(insertSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@user", username);
                    insertCmd.Parameters.AddWithValue("@pass", password);
                    insertCmd.ExecuteNonQuery();

                    // 新用户注册成功后，在分数表里创建一行初始数据
                    long newUserId = insertCmd.LastInsertedId;
                    string scoreSql = "INSERT INTO festival_scores (user_id) VALUES (@userId)";
                    using (MySqlCommand scoreCmd = new MySqlCommand(scoreSql, conn))
                    {
                        scoreCmd.Parameters.AddWithValue("@userId", newUserId);
                        scoreCmd.ExecuteNonQuery();
                    }
                }
                return RegisterState.Success;
            }
            catch (Exception e)
            {
                Debug.LogError("数据库注册异常: " + e.Message);
                return RegisterState.DbError;
            }
        }
    }

    /// <summary> 
    /// 根据节日类型获取对应的数据库字段名 
    /// </summary>
    public static string GetScoreColumn(FestivalType type)
    {
        return type switch
        {
            FestivalType.MidAutumn => "mid_autumn_score",
            FestivalType.DragonBoat => "dragon_boat_score",
            FestivalType.SpringFestival => "spring_festival_score",
            _ => "mid_autumn_score"
        };
    }

    /// <summary> 
    /// 获取指定玩家、指定节日的最高分 
    /// </summary>
    public static int GetHighestScore(int userId, FestivalType type)
    {
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                string column = GetScoreColumn(type);
                // 使用字符串拼接列名
                string sql = $"SELECT {column} FROM festival_scores WHERE user_id = @userId";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("获取最高分失败: " + e.Message);
                return 0;
            }
        }
    }

    /// <summary> 
    /// 上传新最高分 
    /// </summary>
    public static void UpdateHighestScore(int userId, FestivalType type, int newScore)
    {
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                string column = GetScoreColumn(type);
                string sql = $"UPDATE festival_scores SET {column} = @score WHERE user_id = @userId";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@score", newScore);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("更新最高分失败: " + e.Message);
            }
        }
    }

    /// <summary> 
    /// 获取排行榜
    /// </summary>
    public static List<string> GetTop6Leaderboard(FestivalType type)
    {
        List<string> rankList = new List<string>();
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                string column = GetScoreColumn(type);
                // 连表查询：查出分数大于0的用户，按分数倒序排列，取前6个
                string sql = $@"
                    SELECT u.username, f.{column} as score 
                    FROM festival_scores f
                    JOIN users u ON f.user_id = u.id
                    WHERE f.{column} > 0
                    ORDER BY f.{column} DESC
                    LIMIT 6";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["username"].ToString();
                            string score = reader["score"].ToString();
                            rankList.Add($"{name}  得分：{score}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("获取排行榜失败: " + e.Message);
            }
        }
        return rankList;
    }
}