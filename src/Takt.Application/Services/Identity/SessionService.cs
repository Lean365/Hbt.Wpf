// ========================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Services.Identity
// 文件名 : SessionService.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：会话服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Results;
using Takt.Domain.Entities.Identity;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 会话服务实现
/// </summary>
public class SessionService : ISessionService
{
    private readonly IBaseRepository<UserSession> _sessionRepository;

    public SessionService(IBaseRepository<UserSession> sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// 创建会话
    /// </summary>
    public async Task<Result<string>> CreateSessionAsync(
        long userId,
        string username,
        string realName,
        long roleId,
        string roleName,
        string? loginIp = null!, 
        string? clientInfo = null!, 
        int sessionTimeout = 43200)
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString("N");

            var session = new UserSession
            {
                SessionId = sessionId,
                UserId = userId,
                Username = username,
                RealName = realName,
                RoleId = roleId,
                RoleName = roleName,
                LoginTime = DateTime.Now,
                LastActivityTime = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(sessionTimeout),
                LoginIp = loginIp,
                ClientInfo = clientInfo ?? "Desktop App",
                IsActive = 0  // 布尔字段：0=是（活跃）
            };

            await _sessionRepository.CreateAsync(session);

            return Result<string>.Ok(sessionId, "会话创建成功");
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"创建会话失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取会话信息
    /// </summary>
    public async Task<Result<UserSession>> GetSessionAsync(string sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetFirstAsync(x => 
                x.SessionId == sessionId && 
                x.IsDeleted == 0);

            if (session == null)
                return Result<UserSession>.Fail("会话不存在");

            return Result<UserSession>.Ok(session);
        }
        catch (Exception ex)
        {
            return Result<UserSession>.Fail($"获取会话失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 验证会话是否有效
    /// </summary>
    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetFirstAsync(x => 
                x.SessionId == sessionId && 
                x.IsActive == 0 &&  // 布尔字段：0=是（活跃）
                x.IsDeleted == 0);  // 0=否（未删除），1=是（已删除）

            if (session == null)
                return false;

            // 检查是否过期
            if (DateTime.Now > session.ExpiresAt)
            {
                // 标记为过期
                session.IsActive = 1;  // 布尔字段：1=否（已失效）
                session.LogoutTime = DateTime.Now;
                session.LogoutReason = 2; // 超时
                await _sessionRepository.UpdateAsync(session);
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 刷新会话活动时间
    /// </summary>
    public async Task<Result> RefreshSessionAsync(string sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetFirstAsync(x => 
                x.SessionId == sessionId && 
                x.IsActive == 0 &&  // 布尔字段：0=是（活跃）
                x.IsDeleted == 0);  // 0=否（未删除），1=是（已删除）

            if (session == null)
                return Result.Fail("会话不存在");

            session.LastActivityTime = DateTime.Now;
            await _sessionRepository.UpdateAsync(session);

            return Result.Ok("会话刷新成功");
        }
        catch (Exception ex)
        {
            return Result.Fail($"刷新会话失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 销毁会话（登出）
    /// </summary>
    public async Task<Result> DestroySessionAsync(string sessionId, int logoutReason = 1)
    {
        try
        {
            var session = await _sessionRepository.GetFirstAsync(x => 
                x.SessionId == sessionId && 
                x.IsDeleted == 0);

            if (session == null)
                return Result.Fail("会话不存在");

            session.IsActive = 1;  // 布尔字段：1=否（已失效）
            session.LogoutTime = DateTime.Now;
            session.LogoutReason = logoutReason;
            await _sessionRepository.UpdateAsync(session);

            return Result.Ok("会话已销毁");
        }
        catch (Exception ex)
        {
            return Result.Fail($"销毁会话失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 清理过期会话
    /// </summary>
    public async Task<int> CleanExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await _sessionRepository.AsQueryable()
                .Where(x => x.IsActive == 0 && x.ExpiresAt < DateTime.Now && x.IsDeleted == 0)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.IsActive = 1;  // 布尔字段：1=否（已失效）
                session.LogoutTime = DateTime.Now;
                session.LogoutReason = 2; // 超时
                await _sessionRepository.UpdateAsync(session);
            }

            return expiredSessions.Count;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 强制下线用户的所有会话
    /// </summary>
    public async Task<Result> ForceLogoutUserAsync(long userId)
    {
        try
        {
            var activeSessions = await _sessionRepository.AsQueryable()
                .Where(x => x.UserId == userId && x.IsActive == 0 && x.IsDeleted == 0)
                .ToListAsync();

            foreach (var session in activeSessions)
            {
                session.IsActive = 1;  // 布尔字段：1=否（已失效）
                session.LogoutTime = DateTime.Now;
                session.LogoutReason = 3; // 强制下线
                await _sessionRepository.UpdateAsync(session);
            }

            return Result.Ok($"已强制下线 {activeSessions.Count} 个会话");
        }
        catch (Exception ex)
        {
            return Result.Fail($"强制下线失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户的所有活跃会话
    /// </summary>
    public async Task<Result<List<UserSession>>> GetUserActiveSessionsAsync(long userId)
    {
        try
        {
            var sessions = await _sessionRepository.AsQueryable()
                .Where(x => x.UserId == userId && x.IsActive == 0 && x.IsDeleted == 0)
                .ToListAsync();

            return Result<List<UserSession>>.Ok(sessions);
        }
        catch (Exception ex)
        {
            return Result<List<UserSession>>.Fail($"获取会话列表失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取所有在线用户
    /// </summary>
    public async Task<Result<List<UserSession>>> GetAllOnlineUsersAsync()
    {
        try
        {
            var sessions = await _sessionRepository.AsQueryable()
                .Where(x => x.IsActive == 0 && x.ExpiresAt > DateTime.Now && x.IsDeleted == 0)
                .OrderByDescending(x => x.LoginTime)
                .ToListAsync();

            return Result<List<UserSession>>.Ok(sessions);
        }
        catch (Exception ex)
        {
            return Result<List<UserSession>>.Fail($"获取在线用户失败：{ex.Message}");
        }
    }
}

