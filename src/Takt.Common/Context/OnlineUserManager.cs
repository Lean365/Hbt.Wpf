//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : OnlineUserManager.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 版本号 : 0.0.1
// 描述    : 在线用户管理器
//===================================================================

namespace Takt.Common.Context;

/// <summary>
/// 在线用户管理器（内存层）
/// </summary>
/// <remarks>
/// 管理内存中的在线用户，配合 SessionService（数据库层）使用
/// </remarks>
public static class OnlineUserManager
{
    /// <summary>
    /// 获取内存中的在线用户数量
    /// </summary>
    public static int GetOnlineCount()
    {
        return UserContext.GetAllUsers().Count;
    }

    /// <summary>
    /// 获取内存中的在线用户列表
    /// </summary>
    public static List<UserInfo> GetOnlineUsers()
    {
        return UserContext.GetAllUsers()
            .Select(u => new UserInfo
            {
                UserId = u.UserId,
                Username = u.Username,
                RealName = u.RealName,
                RoleName = u.RoleName,
                SessionId = u.SessionId,
                LoginTime = u.LoginTime
            })
            .OrderByDescending(u => u.LoginTime)
            .ToList();
    }

    /// <summary>
    /// 检查用户是否在内存中
    /// </summary>
    public static bool IsUserOnline(long userId)
    {
        return UserContext.GetAllUsers().Any(u => u.UserId == userId);
    }

    /// <summary>
    /// 从内存中移除用户
    /// </summary>
    public static void RemoveUser(long userId)
    {
        UserContext.RemoveUser(userId);
    }
}

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo
{
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
}

