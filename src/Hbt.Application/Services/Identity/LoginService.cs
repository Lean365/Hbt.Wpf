// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：LoginService.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：登录服务实现
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

using Hbt.Application.Dtos.Identity;
using Hbt.Common.Helpers;
using Hbt.Common.Logging;
using Hbt.Common.Results;
using Hbt.Common.Security;
using Hbt.Domain.Entities.Identity;
using Hbt.Domain.Entities.Logging;
using Hbt.Domain.Repositories;

namespace Hbt.Application.Services.Identity;

/// <summary>
/// 登录服务实现
/// 实现用户认证相关的业务逻辑
/// </summary>
public class LoginService : ILoginService
{
    private readonly IBaseRepository<User> _userRepository;
    private readonly IBaseRepository<UserRole> _userRoleRepository;
    private readonly IBaseRepository<Role> _roleRepository;
    private readonly IBaseRepository<LoginLog> _loginLogRepository;
    private readonly IMenuService _menuService;
    private readonly OperLogManager _operLog;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="userRoleRepository">用户角色仓储</param>
    /// <param name="roleRepository">角色仓储</param>
    /// <param name="loginLogRepository">登录日志仓储</param>
    /// <param name="menuService">菜单服务</param>
    /// <param name="operLog">操作日志管理器</param>
    public LoginService(
        IBaseRepository<User> userRepository,
        IBaseRepository<UserRole> userRoleRepository,
        IBaseRepository<Role> roleRepository,
        IBaseRepository<LoginLog> loginLogRepository,
        IMenuService menuService,
        OperLogManager operLog)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _loginLogRepository = loginLogRepository;
        _menuService = menuService;
        _operLog = operLog;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="dto">登录DTO</param>
    /// <returns>登录结果</returns>
    public async Task<Result<LoginResultDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            // 1. 查询用户（包含角色信息）
            var user = await _userRepository.GetFirstAsync(x => x.Username == dto.Username && x.IsDeleted == 0);
            if (user == null)
                return Result<LoginResultDto>.Fail("用户名或密码错误");

            // 2. 验证密码（使用 BouncyCastle Argon2id）
            if (!SecurityHelper.VerifyPassword(dto.Password, user.Password))
                return Result<LoginResultDto>.Fail("用户名或密码错误");

            // 3. 检查用户状态
            if (user.UserStatus != (int)Hbt.Common.Enums.StatusEnum.Normal)
                return Result<LoginResultDto>.Fail("用户已被禁用");

            // 4. 获取用户角色信息
            var userRole = await _userRoleRepository.GetFirstAsync(x => x.UserId == user.Id && x.IsDeleted == 0);
            if (userRole == null)
                return Result<LoginResultDto>.Fail("用户未分配角色，请联系管理员");

            var role = await _roleRepository.GetByIdAsync(userRole.RoleId);
            if (role == null || role.RoleStatus != (int)Hbt.Common.Enums.StatusEnum.Normal)
                return Result<LoginResultDto>.Fail("用户角色不存在或已被禁用");

            // 5. 获取用户菜单树和权限（使用 MenuService）
            var userMenuResult = await _menuService.GetUserMenuTreeAsync(user.Id);
            if (!userMenuResult.Success || userMenuResult.Data == null)
            {
                _operLog.Warning("用户 {Username} 获取菜单树失败：{Message}", user.Username, userMenuResult.Message);
                return Result<LoginResultDto>.Fail($"获取菜单权限失败：{userMenuResult.Message}");
            }

            // 6. 记录登录日志到数据库（完整客户端信息）
            var loginLog = new LoginLog
            {
                Username = user.Username,
                LoginTime = DateTime.Now,
                LoginIp = SystemInfoHelper.GetLocalIpAddress(),
                MacAddress = SystemInfoHelper.GetMacAddress(),
                MachineName = SystemInfoHelper.GetMachineName(),
                LoginLocation = $"{SystemInfoHelper.GetMachineName()}@{SystemInfoHelper.GetOsType()}",
                Browser = "Avalonia Desktop App",
                Os = SystemInfoHelper.GetOsDescription(),
                OsVersion = SystemInfoHelper.GetOsVersion(),
                OsArchitecture = SystemInfoHelper.GetOsArchitecture(),
                CpuInfo = SystemInfoHelper.GetCpuInfo(),
                TotalMemoryGb = (decimal)SystemInfoHelper.GetTotalMemoryGb(),
                FrameworkVersion = SystemInfoHelper.GetFrameworkVersion(),
                IsAdmin = SystemInfoHelper.IsCurrentUserAdmin() ? 0 : 1,  // 布尔字段：0=是，1=否
                ClientType = "Desktop",
                ClientVersion = typeof(Hbt.Application.Services.Identity.LoginService).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                LoginStatus = Hbt.Common.Enums.LoginStatusEnum.Success
            };
            await _loginLogRepository.CreateAsync(loginLog);

            // 8. 记录登录日志到文件
            _operLog.Login(user.Username, user.RealName ?? "", true);

            // 9. 生成登录结果（包含菜单树和权限）
            var loginResult = new LoginResultDto
            {
                UserId = user.Id,
                Username = user.Username,
                RealName = user.RealName ?? string.Empty,
                RoleId = role.Id,
                RoleName = role.RoleName,
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                ExpiresAt = DateTime.MaxValue,
                Menus = userMenuResult.Data.Menus,
                Permissions = userMenuResult.Data.Permissions
            };

            _operLog.Information("用户 {Username} 登录成功，获得 {MenuCount} 个菜单，{PermCount} 个权限",
                user.Username, loginResult.Menus.Count, loginResult.Permissions.Count);

            return Result<LoginResultDto>.Ok(loginResult, "登录成功");
        }
        catch (Exception ex)
        {
            // 先记录文件日志（确保日志被记录）
            _operLog.Login(dto.Username, "", false);
            _operLog.Error(ex, "用户登录失败：{Username}", dto.Username);
            
            // 尝试记录到数据库（如果失败不影响文件日志）
            try
            {
                var loginLog = new LoginLog
                {
                    Username = dto.Username,
                    LoginTime = DateTime.Now,
                    LoginIp = SystemInfoHelper.GetLocalIpAddress(),
                    MacAddress = SystemInfoHelper.GetMacAddress(),
                    MachineName = SystemInfoHelper.GetMachineName(),
                    LoginLocation = $"{SystemInfoHelper.GetMachineName()}@{SystemInfoHelper.GetOsType()}",
                    Browser = "Avalonia Desktop App",
                    Os = SystemInfoHelper.GetOsDescription(),
                    OsVersion = SystemInfoHelper.GetOsVersion(),
                    OsArchitecture = SystemInfoHelper.GetOsArchitecture(),
                    CpuInfo = SystemInfoHelper.GetCpuInfo(),
                    TotalMemoryGb = (decimal)SystemInfoHelper.GetTotalMemoryGb(),
                    FrameworkVersion = SystemInfoHelper.GetFrameworkVersion(),
                    IsAdmin = SystemInfoHelper.IsCurrentUserAdmin() ? 0 : 1,  // 布尔字段：0=是，1=否
                    ClientType = "Desktop",
                    ClientVersion = typeof(Hbt.Application.Services.Identity.LoginService).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                    LoginStatus = Hbt.Common.Enums.LoginStatusEnum.Failed,
                    FailReason = ex.Message
                };
                await _loginLogRepository.CreateAsync(loginLog);
            }
            catch (Exception dbEx)
            {
                _operLog.Error(dbEx, "登录失败日志写入数据库失败：{Username}", dto.Username);
            }
            
            return Result<LoginResultDto>.Fail($"登录失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>操作结果</returns>
    public Task<Result> LogoutAsync(long userId)
    {
        try
        {
            _operLog.Information("用户登出，用户ID：{UserId}", userId);
            return Task.FromResult(Result.Ok("登出成功"));
        }
        catch (Exception ex)
        {
            _operLog.Error(ex, "用户登出失败");
            return Task.FromResult(Result.Fail($"登出失败：{ex.Message}"));
        }
    }
}
