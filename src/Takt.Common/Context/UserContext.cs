// ========================================
// 项目名称：Takt.Wpf
// 文件名称：UserContext.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：用户上下文
// 
// 版权信息：
// Copyright (c) 2025 Takt All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

namespace Takt.Common.Context;

/// <summary>
/// 用户上下文
/// 管理多用户登录信息
/// </summary>
public class UserContext
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, UserContext> _users = new();
    private static readonly object _lock = new object();
    private static UserContext? _currentUser;

    /// <summary>
    /// 获取当前用户上下文实例
    /// </summary>
    /// <remarks>
    /// 对于多用户系统，需要先调用 SetCurrent 设置当前用户
    /// </remarks>
    public static UserContext Current
    {
        get
        {
            if (_currentUser == null)
            {
                lock (_lock)
                {
                    _currentUser ??= new UserContext();
                }
            }
            return _currentUser;
        }
    }

    /// <summary>
    /// 设置当前用户
    /// </summary>
    /// <param name="userId">用户ID</param>
    public static void SetCurrent(long userId)
    {
        if (_users.TryGetValue(userId, out var userContext))
        {
            _currentUser = userContext;
        }
    }

    /// <summary>
    /// 添加用户到上下文
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户上下文</returns>
    public static UserContext AddUser(long userId)
    {
        return _users.GetOrAdd(userId, _ => new UserContext());
    }

    /// <summary>
    /// 移除用户上下文
    /// </summary>
    /// <param name="userId">用户ID</param>
    public static void RemoveUser(long userId)
    {
        _users.TryRemove(userId, out _);
        if (_currentUser?.UserId == userId)
        {
            _currentUser = null;
        }
    }

    /// <summary>
    /// 获取所有在线用户
    /// </summary>
    public static List<UserContext> GetAllUsers()
    {
        return _users.Values.Where(u => u.IsAuthenticated).ToList();
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; private set; } = string.Empty;

    /// <summary>
    /// 角色ID
    /// </summary>
    public long? RoleId { get; private set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; private set; } = string.Empty;

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; private set; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; private set; } = string.Empty;

    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; private set; } = string.Empty;

    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime LoginTime { get; private set; }

    /// <summary>
    /// 是否已登录
    /// </summary>
    public bool IsAuthenticated { get; private set; }

    /// <summary>
    /// 设置登录信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="username">用户名</param>
    /// <param name="realName">真实姓名</param>
    /// <param name="roleId">角色ID</param>
    /// <param name="roleName">角色名称</param>
    /// <param name="sessionId">会话ID</param>
    /// <param name="accessToken">访问令牌</param>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="expiresAt">令牌过期时间</param>
    public void SetLoginInfo(
        long userId, 
        string username, 
        string realName, 
        long? roleId, 
        string roleName,
        string sessionId,
        string accessToken,
        string refreshToken,
        DateTime expiresAt)
    {
        UserId = userId;
        Username = username;
        RealName = realName;
        RoleId = roleId;
        RoleName = roleName;
        SessionId = sessionId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        LoginTime = DateTime.Now;
        IsAuthenticated = true;
    }

    /// <summary>
    /// 清除登录信息
    /// </summary>
    public void Clear()
    {
        UserId = 0;
        Username = string.Empty;
        RealName = string.Empty;
        RoleId = null;
        RoleName = string.Empty;
        SessionId = string.Empty;
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        ExpiresAt = DateTime.MinValue;
        LoginTime = DateTime.MinValue;
        IsAuthenticated = false;
    }

    /// <summary>
    /// 检查令牌是否过期
    /// </summary>
    /// <returns>已过期返回true</returns>
    public bool IsTokenExpired()
    {
        return DateTime.Now >= ExpiresAt;
    }

    /// <summary>
    /// 私有构造函数
    /// </summary>
    private UserContext()
    {
    }
}

