//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : DbSeedRbac.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 种子数据初始化服务（军用级安全 - BouncyCastle，使用 BaseRepository）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//===================================================================

using Takt.Common.Enums;
using Takt.Common.Logging;
using Takt.Common.Security;
using Takt.Domain.Entities.Identity;
using Takt.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Takt.Infrastructure.Data;

/// <summary>
/// RBAC 种子数据初始化服务（用户/角色/菜单/按钮等）
/// 使用 BouncyCastle 的 Argon2（军用级密码哈希算法）进行密码加密
/// 使用 BaseRepository 自动填充审计字段和处理雪花ID
/// </summary>
public class DbSeedRbac
{
    private readonly DbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly InitLogManager _initLog;
    private readonly DbSeedMenu _seedMenu;
    private readonly IBaseRepository<Role> _roleRepository;
    private readonly IBaseRepository<User> _userRepository;
    private readonly IBaseRepository<RoleMenu> _roleMenuRepository;
    private readonly IBaseRepository<UserRole> _userRoleRepository;

    public DbSeedRbac(
        DbContext dbContext, 
        IConfiguration configuration, 
        InitLogManager initLog,
        IBaseRepository<Menu> menuRepository,
        IBaseRepository<Role> roleRepository,
        IBaseRepository<User> userRepository,
        IBaseRepository<RoleMenu> roleMenuRepository,
        IBaseRepository<UserRole> userRoleRepository)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _initLog = initLog;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _roleMenuRepository = roleMenuRepository;
        _userRoleRepository = userRoleRepository;
        _seedMenu = new DbSeedMenu(initLog, menuRepository);
    }

    /// <summary>
    /// 初始化种子数据（使用同步事务避免死锁）
    /// </summary>
    /// <remarks>
    /// ⚠️ 重要：SqlSugar 的 UseTranAsync 在桌面应用（WPF）中容易死锁
    /// 使用同步事务 + 同步方法是最稳定的方案
    /// 使用 BaseRepository 自动填充审计字段（CreatedBy、CreatedTime、UpdatedBy、UpdatedTime）
    /// 并根据配置自动处理雪花ID
    /// </remarks>
#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符。故意使用同步方法避免死锁
    public async Task InitializeAsync()
    {
        try
        {
            // 检查是否启用种子数据
            var enableSeedData = bool.Parse(_configuration["DatabaseSettings:EnableSeedData"] ?? "false");
            
            if (!enableSeedData)
            {
                _initLog.Information("种子数据功能已禁用，跳过初始化");
                return;
            }

            _initLog.Information("开始初始化种子数据（使用 BouncyCastle 军用级加密 + BaseRepository）..");

            // ✅ 使用同步事务（避免死锁）
            _dbContext.BeginTransaction();
            
            try
            {
                // 1. 创建系统菜单
                _initLog.Information("步骤 1/6: 开始创建系统菜单..");
                var menus = _seedMenu.CreateSystemMenus();
                _initLog.Information("步骤 1/6: 系统菜单创建完成，共 {Count} 条", menus.Count);

                // 2. 创建系统角色
                _initLog.Information("步骤 2/6: 开始创建系统角色..");
                var superRole = CreateSuperRole();
                var guestRole = CreateGuestRole();
                _initLog.Information("步骤 2/6: 系统角色创建完成");

                // 3. 创建系统用户
                _initLog.Information("步骤 3/6: 开始创建系统用户..");
                var superUser = CreateSuperUser();
                var guestUser = CreateGuestUser();
                _initLog.Information("步骤 3/6: 系统用户创建完成");

                // 4. 关联角色和菜单
                _initLog.Information("步骤 4/6: 开始关联角色和菜单..");
                AssignRoleMenus(superRole, menus, true); // 超级角色拥有所有菜单
                AssignRoleMenus(guestRole, menus, false); // 来宾角色只有基础菜单
                _initLog.Information("步骤 4/6: 角色菜单关联完成");

                // 5. 关联用户和角色
                _initLog.Information("步骤 5/6: 开始关联用户和角色..");
                AssignUserRole(superUser, superRole);
                AssignUserRole(guestUser, guestRole);
                _initLog.Information("步骤 5/6: 用户角色关联完成");

                // 6. 更新角色用户数
                _initLog.Information("步骤 6/6: 开始更新角色用户数..");
                UpdateRoleUserCount(superRole);
                UpdateRoleUserCount(guestRole);
                _initLog.Information("步骤 6/6: 角色用户数更新完成");

                // 提交事务
                _dbContext.CommitTransaction();

                _initLog.Information("========================================");
                _initLog.Information("✅ 种子数据初始化完成！");
                _initLog.Information("超级用户账号：super");
                _initLog.Information("超级用户密码：Super@123456");
                _initLog.Information("来宾用户账号：guest");
                _initLog.Information("来宾用户密码：Guest@123456");
                _initLog.Information("========================================");
            }
            catch (Exception ex)
            {
                // 回滚事务
                _dbContext.RollbackTransaction();
                _initLog.Error(ex, "❌ 种子数据初始化失败，已回滚事务");
                throw;
            }
        }
        catch (Exception ex)
        {
            _initLog.Error(ex, "种子数据初始化失败");
            throw;
        }
    }
#pragma warning restore CS1998

    /// <summary>
    /// 创建超级角色（使用 BaseRepository 同步方法，存在则更新，不存在则创建）
    /// </summary>
    private Role CreateSuperRole()
    {
        var superRole = _roleRepository.GetFirst(r => r.RoleCode == "super");

        if (superRole == null)
        {
            _initLog.Information("创建超级角色..");
            superRole = new Role
            {
                RoleCode = "super",
                RoleName = "超级角色",
                Description = "系统超级角色，拥有所有权限",
                RoleStatus = StatusEnum.Normal,  // 0=启用
                DataScope = DataScopeEnum.All,  // 1=全部数据
                UserCount = 0,
                OrderNum = 1
            };
            _roleRepository.Create(superRole, "Takt365");
            _initLog.Information("✅ 创建超级角色，ID: {RoleId}", superRole.Id);
        }
        else
        {
            // 更新所有字段
            superRole.RoleName = "超级角色";
            superRole.Description = "系统超级角色，拥有所有权限";
            superRole.RoleStatus = StatusEnum.Normal;
            superRole.DataScope = DataScopeEnum.All;
            superRole.UserCount = 0;
            superRole.OrderNum = 1;
            _roleRepository.Update(superRole, "Takt365");
            _initLog.Information("✅ 更新超级角色");
        }

        return superRole!;
    }

    /// <summary>
    /// 创建来宾角色（使用 BaseRepository 同步方法，存在则更新，不存在则创建）
    /// </summary>
    private Role CreateGuestRole()
    {
        var guestRole = _roleRepository.GetFirst(r => r.RoleCode == "guest");

        if (guestRole == null)
        {
            _initLog.Information("创建来宾角色..");
            guestRole = new Role
            {
                RoleCode = "guest",
                RoleName = "来宾角色",
                Description = "来宾用户角色，拥有基础查看权限",
                RoleStatus = StatusEnum.Normal,  // 0=启用
                DataScope = DataScopeEnum.Self,  // 4=仅本人
                UserCount = 0,
                OrderNum = 2
            };
            _roleRepository.Create(guestRole, "Takt365");
            _initLog.Information("✅ 创建来宾角色");
        }
        else
        {
            // 更新所有字段
            guestRole.RoleName = "来宾角色";
            guestRole.Description = "来宾用户角色，拥有基础查看权限";
            guestRole.RoleStatus = StatusEnum.Normal;
            guestRole.DataScope = DataScopeEnum.Self;
            guestRole.UserCount = 0;
            guestRole.OrderNum = 2;
            _roleRepository.Update(guestRole, "Takt365");
            _initLog.Information("✅ 更新来宾角色");
        }

        return guestRole!;
    }

    /// <summary>
    /// 创建超级用户（使用 BaseRepository 同步方法，存在则更新，不存在则创建）
    /// </summary>
    private User CreateSuperUser()
    {
        var superUser = _userRepository.GetFirst(u => u.Username == "admin");

        if (superUser == null)
        {
            _initLog.Information("创建超级用户（使用 BouncyCastle Argon2）..");
            
            // 使用军用级安全帮助类加密密码
            var hashedPassword = SecurityHelper.HashPassword("Hbt@123");
            
            superUser = new User
            {
                Username = "admin",
                Password = hashedPassword,
                RealName = "超级用户",
                Nickname = "William Henry Gates III",
                Email = "admin@Takt.com",
                Phone = "13800138001",
                Avatar = "assets/avatar.png", // 默认头像路径
                UserType = UserTypeEnum.System,  // 0=系统用户
                UserGender = UserGenderEnum.Unknown,     // 0=未知
                UserStatus = StatusEnum.Normal      // 0=启用
            };
            _userRepository.Create(superUser, "Takt365");
            _initLog.Information("✅ 创建超级用户，ID: {UserId}", superUser.Id);
        }
        else
        {
            // 更新所有字段（包括密码，重置为默认密码）
            superUser.RealName = "超级用户";    
            superUser.Nickname = "William Henry Gates III";
            superUser.Email = "admin@Takt.com";
            superUser.Phone = "13800138001";
            superUser.Avatar = string.IsNullOrWhiteSpace(superUser.Avatar) ? "assets/avatar.png" : superUser.Avatar; // 如果为空则使用默认头像
            superUser.UserType = UserTypeEnum.System;
            superUser.UserGender = UserGenderEnum.Unknown;
            superUser.UserStatus = StatusEnum.Normal;
            // 更新密码，重置为默认密码
            superUser.Password = SecurityHelper.HashPassword("Hbt@123");
            _userRepository.Update(superUser, "Takt365");
            _initLog.Information("✅ 更新超级用户（密码已重置为默认密码）");
        }

        return superUser!;
    }

    /// <summary>
    /// 创建来宾用户（使用 BaseRepository 同步方法，存在则更新，不存在则创建）
    /// </summary>
    private User CreateGuestUser()
    {
        var guestUser = _userRepository.GetFirst(u => u.Username == "guest");

        if (guestUser == null)
        {
            _initLog.Information("创建来宾用户（使用 BouncyCastle Argon2）..");
            
            // 使用军用级安全帮助类加密密码
            var hashedPassword = SecurityHelper.HashPassword("Hbt@123");
            
            guestUser = new User
            {
                Username = "guest",
                Password = hashedPassword,
                RealName = "来宾用户",
                Nickname = "Paul Gardner Allen",
                Email = "guest@Takt.com",
                Phone = "13800138002",
                Avatar = "assets/avatar.png", // 默认头像路径
                UserType = UserTypeEnum.Normal,  // 1=普通用户
                UserGender = UserGenderEnum.Unknown,     // 0=未知
                UserStatus = StatusEnum.Normal      // 0=启用
            };
            _userRepository.Create(guestUser, "Takt365");
            _initLog.Information("✅ 创建来宾用户");
        }
        else
        {
            // 更新所有字段（包括密码，重置为默认密码）
            guestUser.RealName = "来宾用户";
            guestUser.Nickname = "Paul Gardner Allen";
            guestUser.Email = "guest@Takt.com";
            guestUser.Phone = "13800138002";
            guestUser.Avatar = string.IsNullOrWhiteSpace(guestUser.Avatar) ? "assets/avatar.png" : guestUser.Avatar; // 如果为空则使用默认头像
            guestUser.UserType = UserTypeEnum.Normal;
            guestUser.UserGender = UserGenderEnum.Unknown;
            guestUser.UserStatus = StatusEnum.Normal;
            // 更新密码，重置为默认密码
            guestUser.Password = SecurityHelper.HashPassword("Hbt@123");
            _userRepository.Update(guestUser, "Takt365");
            _initLog.Information("✅ 更新来宾用户（密码已重置为默认密码）");
        }

        return guestUser!;
    }

    /// <summary>
    /// 关联角色和菜单（使用 BaseRepository 同步方法，存在则跳过，不存在则创建）
    /// </summary>
    private void AssignRoleMenus(Role role, List<Menu> menus, bool isSuperRole)
    {
        if (isSuperRole)
        {
            _initLog.Information("为超级角色分配所有菜单权限..");
        }
        else
        {
            _initLog.Information("为来宾角色分配基础菜单权限..");
        }

        var createdCount = 0;
        var existingCount = 0;
        
        foreach (var menu in menus)
        {
            // 来宾角色只分配基础菜单（目录和菜单类型，排除按钮和API）
            if (!isSuperRole && (menu.MenuType == MenuTypeEnum.Button || menu.MenuType == MenuTypeEnum.Api))
            {
                continue; // 跳过按钮和API权限
            }

            // 来宾角色只分配可见的菜单
            if (!isSuperRole && menu.IsVisible == VisibilityEnum.Invisible)
            {
                continue; // 跳过不可见的菜单
            }

            var existingRelation = _roleMenuRepository.GetFirst(rm => rm.RoleId == role.Id && rm.MenuId == menu.Id);

            if (existingRelation == null)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = role.Id,
                    MenuId = menu.Id
                };
                _roleMenuRepository.Create(roleMenu, "Takt365");
                createdCount++;
            }
            else
            {
                existingCount++;
            }
        }

        var roleType = isSuperRole ? "超级角色" : "来宾角色";
        _initLog.Information("✅ {RoleType}菜单关联完成：新增 {Created} 个，已存在 {Existing} 个", roleType, createdCount, existingCount);
    }

    /// <summary>
    /// 关联用户和角色（使用 BaseRepository 同步方法，存在则跳过，不存在则创建）
    /// </summary>
    private void AssignUserRole(User user, Role role)
    {
        var existingRelation = _userRoleRepository.GetFirst(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

        if (existingRelation == null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            _userRoleRepository.Create(userRole, "Takt365");
            _initLog.Information("✅ 创建用户角色关联，用户ID: {UserId}, 角色ID: {RoleId}", user.Id, role.Id);
        }
        else
        {
            _initLog.Information("✅ 用户角色关联已存在");
        }
    }

    /// <summary>
    /// 更新角色用户数（使用 BaseRepository 同步方法）
    /// </summary>
    private void UpdateRoleUserCount(Role role)
    {
        var userCount = _userRoleRepository.AsQueryable()
            .Where(ur => ur.RoleId == role.Id)
            .Count();
        role.UserCount = userCount;
        _roleRepository.Update(role, "Takt365");
        _initLog.Information("✅ 更新角色 {RoleName} 用户数：{UserCount}", role.RoleName, userCount);
    }
}
