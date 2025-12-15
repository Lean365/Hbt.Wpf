//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : ISessionService.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 版本号 : 0.0.1
// 描述    : 会话服务接口
//===================================================================

using Takt.Common.Results;
using Takt.Domain.Entities.Identity;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 会话服务接口
/// 管理用户登录会话
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// 创建会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="username">用户名</param>
    /// <param name="realName">真实姓名</param>
    /// <param name="roleId">角色ID</param>
    /// <param name="roleName">角色名称</param>
    /// <param name="loginIp">登录IP</param>
    /// <param name="clientInfo">客户端信息</param>
    /// <param name="sessionTimeout">会话超时时间（分钟），默认30天</param>
    /// <returns>会话ID</returns>
    Task<Result<string>> CreateSessionAsync(
        long userId,
        string username,
        string realName,
        long roleId,
        string roleName,
        string? loginIp = null!, 
        string? clientInfo = null!, 
        int sessionTimeout = 43200);

    /// <summary>
    /// 获取会话信息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>会话信息</returns>
    Task<Result<UserSession>> GetSessionAsync(string sessionId);

    /// <summary>
    /// 验证会话是否有效
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>是否有效</returns>
    Task<bool> ValidateSessionAsync(string sessionId);

    /// <summary>
    /// 刷新会话活动时间
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>操作结果</returns>
    Task<Result> RefreshSessionAsync(string sessionId);

    /// <summary>
    /// 销毁会话（登出）
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="logoutReason">登出原因</param>
    /// <returns>操作结果</returns>
    Task<Result> DestroySessionAsync(string sessionId, int logoutReason = 1);

    /// <summary>
    /// 清理过期会话
    /// </summary>
    /// <returns>清理数量</returns>
    Task<int> CleanExpiredSessionsAsync();

    /// <summary>
    /// 强制下线用户的所有会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>操作结果</returns>
    Task<Result> ForceLogoutUserAsync(long userId);

    /// <summary>
    /// 获取用户的所有活跃会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>会话列表</returns>
    Task<Result<List<UserSession>>> GetUserActiveSessionsAsync(long userId);

    /// <summary>
    /// 获取所有在线用户
    /// </summary>
    /// <returns>在线用户列表</returns>
    Task<Result<List<UserSession>>> GetAllOnlineUsersAsync();
}

