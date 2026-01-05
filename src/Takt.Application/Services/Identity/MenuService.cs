//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Services.Identity
// 文件名 : MenuService.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-21
// 功能描述：菜单服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Takt.Application.Dtos.Identity;
using Takt.Common.Enums;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Identity;
using Takt.Domain.Repositories;
using Mapster;
using SqlSugar;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 菜单服务实现
/// </summary>
public class MenuService : IMenuService
{
    private readonly IBaseRepository<Menu> _menuRepository;
    private readonly IBaseRepository<User> _userRepository;
    private readonly IBaseRepository<Role> _roleRepository;
    private readonly IBaseRepository<UserRole> _userRoleRepository;
    private readonly IBaseRepository<RoleMenu> _roleMenuRepository;
    private readonly AppLogManager _logger;
    private readonly OperLogManager? _operLog;

    public MenuService(
        IBaseRepository<Menu> menuRepository,
        IBaseRepository<User> userRepository,
        IBaseRepository<Role> roleRepository,
        IBaseRepository<UserRole> userRoleRepository,
        IBaseRepository<RoleMenu> roleMenuRepository,
        AppLogManager logger,
        OperLogManager? operLog = null)
    {
        _menuRepository = menuRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _roleMenuRepository = roleMenuRepository;
        _logger = logger;
        _operLog = operLog;
    }

    /// <summary>
    /// 根据用户ID获取菜单树
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>菜单树</returns>
    public async Task<Result<UserMenuTreeDto>> GetUserMenuTreeAsync(long userId)
    {
        try
        {
            // 1. 获取用户信息
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<UserMenuTreeDto>.Fail("用户不存在");

            if (user.UserStatus != StatusEnum.Normal)
                return Result<UserMenuTreeDto>.Fail("用户已被禁用");

            // 2. 获取用户的所有角色
            var userRoles = await _userRoleRepository.AsQueryable()
                .Where(ur => ur.UserId == userId && ur.IsDeleted == 0)
                .ToListAsync();

            if (userRoles == null || !userRoles.Any())
                return Result<UserMenuTreeDto>.Fail("用户未分配角色");

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // 3. 验证角色状态
            var roles = await _roleRepository.AsQueryable()
                .Where(r => roleIds.Contains(r.Id) && r.IsDeleted == 0 && r.RoleStatus == StatusEnum.Normal)
                .ToListAsync();

            if (roles == null || !roles.Any())
                return Result<UserMenuTreeDto>.Fail("用户的角色不存在或已被禁用");

            var validRoleIds = roles.Select(r => r.Id).ToList();

            // 4. 获取角色关联的所有菜单
            var roleMenus = await _roleMenuRepository.AsQueryable()
                .Where(rm => validRoleIds.Contains(rm.RoleId) && rm.IsDeleted == 0)
                .ToListAsync();

            if (roleMenus == null || !roleMenus.Any())
            {
                return Result<UserMenuTreeDto>.Ok(new UserMenuTreeDto
                {
                    UserId = userId,
                    Username = user.Username,
                    RoleIds = validRoleIds,
                    Menus = new List<MenuDto>(),
                    Permissions = new List<string>()
                });
            }

            var menuIds = roleMenus.Select(rm => rm.MenuId).Distinct().ToList();
            _logger.Information("用户 {Username}({UserId}) 角色关联的菜单ID数量: {MenuIdCount}", user.Username, userId, menuIds.Count);

            // 5. 获取所有菜单信息（包含被引用的父级菜单）
            var allMenus = await _menuRepository.AsQueryable()
                .Where(m => menuIds.Contains(m.Id) && m.IsDeleted == 0 && m.MenuStatus == StatusEnum.Normal)
                .OrderBy(m => m.OrderNum)
                .ToListAsync();

            _logger.Information("用户 {Username}({UserId}) 查询到的菜单数量: {TotalMenuCount}，菜单类型分布: 目录={DirectoryCount}, 菜单={MenuTypeCount}, 按钮={ButtonCount}, API={ApiCount}",
                user.Username, userId, allMenus?.Count ?? 0,
                allMenus?.Count(m => m.MenuType == MenuTypeEnum.Directory) ?? 0,
                allMenus?.Count(m => m.MenuType == MenuTypeEnum.Menu) ?? 0,
                allMenus?.Count(m => m.MenuType == MenuTypeEnum.Button) ?? 0,
                allMenus?.Count(m => m.MenuType == MenuTypeEnum.Api) ?? 0);

            if (allMenus == null || !allMenus.Any())
            {
                return Result<UserMenuTreeDto>.Ok(new UserMenuTreeDto
                {
                    UserId = userId,
                    Username = user.Username,
                    RoleIds = validRoleIds,
                    Menus = new List<MenuDto>(),
                    Permissions = new List<string>()
                });
            }

            // 6. 递归加载所有父级菜单（确保菜单树完整）
            var allMenusWithParents = await LoadParentMenusAsync(allMenus);
            _logger.Information("用户 {Username}({UserId}) 加载父级菜单后总数: {TotalCount}", user.Username, userId, allMenusWithParents.Count);

            // 7. 转换为DTO，统一处理ParentId：null -> 0
            var menuDtos = allMenusWithParents
                .Select(m =>
                {
                    var dto = m.Adapt<MenuDto>();
                    // 统一规范：null 转换为 0（顶级菜单）
                    if (dto.ParentId == null)
                    {
                        dto.ParentId = 0;
                    }
                    return dto;
                })
                .ToList();

            // 8. 构建菜单树（只返回目录和菜单，不返回按钮）
            var nonButtonMenus = menuDtos.Where(m => m.MenuType != MenuTypeEnum.Button).ToList();
            _logger.Information("用户 {Username}({UserId}) 过滤按钮后菜单数量: {NonButtonCount}", user.Username, userId, nonButtonMenus.Count);
            
            // 调试：输出菜单的 ParentId 分布
            var parentIdGroups = nonButtonMenus.GroupBy(m => m.ParentId ?? 0).ToList();
            foreach (var group in parentIdGroups)
            {
                _logger.Information("用户 {Username}({UserId}) ParentId={ParentId} 的菜单数量: {Count}, 菜单名称: {MenuNames}",
                    user.Username, userId, group.Key, group.Count(), 
                    string.Join(", ", group.Select(m => $"{m.MenuName}({m.Id})")));
            }
            
            // 调试：输出顶级菜单（ParentId=0）的详细信息
            var topLevelMenus = nonButtonMenus.Where(m => (m.ParentId ?? 0) == 0).ToList();
            _logger.Information("用户 {Username}({UserId}) 顶级菜单（ParentId=0）数量: {Count}, 菜单名称: {MenuNames}",
                user.Username, userId, topLevelMenus.Count,
                string.Join(", ", topLevelMenus.Select(m => $"{m.MenuName}({m.Id})")));
            
            var menuTree = BuildMenuTree(nonButtonMenus);

            // 9. 提取所有权限码（包括按钮权限）
            var permissions = allMenusWithParents
                .Where(m => !string.IsNullOrEmpty(m.PermCode))
                .Select(m => m.PermCode!)
                .Distinct()
                .ToList();

            // 统计菜单总数（包括所有子节点）
            int totalMenuCount = CountMenuNodes(menuTree);

            var result = new UserMenuTreeDto
            {
                UserId = userId,
                Username = user.Username,
                RoleIds = validRoleIds,
                Menus = menuTree,
                Permissions = permissions
            };

            _logger.Information("用户 {Username}({UserId}) 获取菜单树成功，顶级菜单 {TopLevelCount} 个，总菜单 {TotalCount} 个，权限 {PermCount} 个",
                user.Username, userId, menuTree.Count, totalMenuCount, permissions.Count);

            return Result<UserMenuTreeDto>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取用户菜单树失败，用户ID: {UserId}", userId);
            return Result<UserMenuTreeDto>.Fail($"获取菜单树失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 根据角色ID列表获取菜单树
    /// </summary>
    /// <param name="roleIds">角色ID列表，不能为空</param>
    /// <returns>包含菜单树的结果对象，成功时返回菜单树列表，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 返回的菜单树只包含目录和菜单类型，不包含按钮类型
    /// </remarks>
    public async Task<Result<List<MenuDto>>> GetMenuTreeByRolesAsync(List<long> roleIds)
    {
        try
        {
            if (roleIds == null || !roleIds.Any())
                return Result<List<MenuDto>>.Fail("角色ID列表不能为空");

            // 1. 获取角色关联的所有菜单
            var roleMenus = await _roleMenuRepository.AsQueryable()
                .Where(rm => roleIds.Contains(rm.RoleId) && rm.IsDeleted == 0)
                .ToListAsync();

            if (roleMenus == null || !roleMenus.Any())
                return Result<List<MenuDto>>.Ok(new List<MenuDto>());

            var menuIds = roleMenus.Select(rm => rm.MenuId).Distinct().ToList();

            // 2. 获取所有菜单信息
            var allMenus = await _menuRepository.AsQueryable()
                .Where(m => menuIds.Contains(m.Id) && m.IsDeleted == 0 && m.MenuStatus == StatusEnum.Normal)
                .OrderBy(m => m.OrderNum)
                .ToListAsync();

            if (allMenus == null || !allMenus.Any())
                return Result<List<MenuDto>>.Ok(new List<MenuDto>());

            // 3. 递归加载所有父级菜单
            var allMenusWithParents = await LoadParentMenusAsync(allMenus);

            // 4. 转换为DTO并构建树，统一处理ParentId：null -> 0
            var menuDtos = allMenusWithParents
                .Where(m => m.MenuType != MenuTypeEnum.Button) // 不包含按钮
                .Select(m =>
                {
                    var dto = m.Adapt<MenuDto>();
                    // 统一规范：null 转换为 0（顶级菜单）
                    if (dto.ParentId == null)
                    {
                        dto.ParentId = 0;
                    }
                    return dto;
                })
                .ToList();

            var menuTree = BuildMenuTree(menuDtos);

            return Result<List<MenuDto>>.Ok(menuTree);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "根据角色获取菜单树失败");
            return Result<List<MenuDto>>.Fail($"获取菜单树失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取所有菜单树（管理员）
    /// </summary>
    /// <returns>包含完整菜单树的结果对象，成功时返回菜单树列表，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 返回所有正常状态的菜单，只包含目录和菜单类型，不包含按钮类型
    /// </remarks>
    public async Task<Result<List<MenuDto>>> GetAllMenuTreeAsync()
    {
        try
        {
            // 获取所有正常状态的菜单
            var allMenus = await _menuRepository.AsQueryable()
                .Where(m => m.IsDeleted == 0 && m.MenuStatus == StatusEnum.Normal)
                .OrderBy(m => m.OrderNum)
                .ToListAsync();

            if (allMenus == null || !allMenus.Any())
                return Result<List<MenuDto>>.Ok(new List<MenuDto>());

            // 转换为DTO并构建树（排除按钮类型），统一处理ParentId：null -> 0
            var menuDtos = allMenus
                .Where(m => m.MenuType != MenuTypeEnum.Button)
                .Select(m =>
                {
                    var dto = m.Adapt<MenuDto>();
                    // 统一规范：null 转换为 0（顶级菜单）
                    if (dto.ParentId == null)
                    {
                        dto.ParentId = 0;
                    }
                    return dto;
                })
                .ToList();
            var menuTree = BuildMenuTree(menuDtos);

            return Result<List<MenuDto>>.Ok(menuTree);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取所有菜单树失败");
            return Result<List<MenuDto>>.Fail($"获取菜单树失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 根据用户ID获取权限码列表
    /// </summary>
    /// <param name="userId">用户ID，必须大于0</param>
    /// <returns>包含权限码列表的结果对象，成功时返回权限码列表，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 返回用户所有角色关联的菜单权限码（包括按钮权限）
    /// </remarks>
    public async Task<Result<List<string>>> GetUserPermissionsAsync(long userId)
    {
        try
        {
            // 1. 获取用户的所有角色
            var userRoles = await _userRoleRepository.AsQueryable()
                .Where(ur => ur.UserId == userId && ur.IsDeleted == 0)
                .ToListAsync();

            if (userRoles == null || !userRoles.Any())
                return Result<List<string>>.Ok(new List<string>());

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            // 2. 获取角色关联的所有菜单
            var roleMenus = await _roleMenuRepository.AsQueryable()
                .Where(rm => roleIds.Contains(rm.RoleId) && rm.IsDeleted == 0)
                .ToListAsync();

            if (roleMenus == null || !roleMenus.Any())
                return Result<List<string>>.Ok(new List<string>());

            var menuIds = roleMenus.Select(rm => rm.MenuId).Distinct().ToList();

            // 3. 获取所有菜单的权限码
            var menus = await _menuRepository.AsQueryable()
                .Where(m => menuIds.Contains(m.Id) && m.IsDeleted == 0 && m.MenuStatus == StatusEnum.Normal)
                .ToListAsync();

            var permissions = menus
                .Where(m => !string.IsNullOrEmpty(m.PermCode))
                .Select(m => m.PermCode!)
                .Distinct()
                .ToList();

            return Result<List<string>>.Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取用户权限码失败，用户ID: {UserId}", userId);
            return Result<List<string>>.Fail($"获取权限码失败：{ex.Message}");
        }
    }

    #region 私有方法

    /// <summary>
    /// 递归加载父级菜单
    /// </summary>
    /// <param name="menus">当前菜单列表</param>
    /// <returns>包含所有父级菜单的完整列表</returns>
    private async Task<List<Menu>> LoadParentMenusAsync(List<Menu> menus)
    {
        var result = new List<Menu>(menus);
        var menuIds = new HashSet<long>(menus.Select(m => m.Id));

        // 获取所有父级ID
        var parentIds = menus
            .Where(m => m.ParentId.HasValue && m.ParentId.Value > 0)
            .Select(m => m.ParentId!.Value)
            .Distinct()
            .Where(pid => !menuIds.Contains(pid)) // 排除已加载的
            .ToList();

        if (!parentIds.Any())
            return result;

        // 批量加载父级菜单
        var parentMenus = await _menuRepository.AsQueryable()
            .Where(m => parentIds.Contains(m.Id) && m.IsDeleted == 0 && m.MenuStatus == StatusEnum.Normal)
            .ToListAsync();

        if (parentMenus != null && parentMenus.Any())
        {
            result.AddRange(parentMenus);
            // 递归加载上级的父级
            var grandParents = await LoadParentMenusAsync(parentMenus);
            foreach (var gp in grandParents)
            {
                if (!result.Any(m => m.Id == gp.Id))
                    result.Add(gp);
            }
        }

        return result;
    }

    /// <summary>
    /// 构建菜单树
    /// </summary>
    /// <param name="menus">扁平菜单列表</param>
    /// <param name="parentId">父级ID（0 表示顶级菜单）</param>
    /// <returns>菜单树</returns>
    private List<MenuDto> BuildMenuTree(List<MenuDto> menus, long parentId = 0)
    {
        // 统一规范：ParentId 为 0 表示顶级菜单
        return menus
            .Where(m => (m.ParentId ?? 0) == parentId)
            .OrderBy(m => m.OrderNum)
            .Select(m =>
            {
                m.Children = BuildMenuTree(menus, m.Id);
                return m;
            })
            .ToList();
    }

    /// <summary>
    /// 递归统计菜单树中的总节点数
    /// </summary>
    /// <param name="menuTree">菜单树</param>
    /// <returns>总节点数</returns>
    private int CountMenuNodes(List<MenuDto> menuTree)
    {
        int count = 0;
        foreach (var menu in menuTree)
        {
            count++; // 当前节点
            if (menu.Children != null && menu.Children.Any())
            {
                count += CountMenuNodes(menu.Children); // 递归统计子节点
            }
        }
        return count;
    }

    // 使用 Mapster 直接映射，移除手写映射方法

    // DTO->实体改用 Mapster，手写方法移除

    #endregion

    #region CRUD 操作

    /// <summary>
    /// 查询菜单列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、菜单名称、菜单编码等筛选条件</param>
    /// <returns>包含分页菜单列表的结果对象，成功时返回菜单列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在菜单名称、菜单编码、权限码、国际化键中搜索）
    /// 支持按菜单名称、菜单编码、排序号、创建时间排序，默认按排序号升序
    /// </remarks>
    public async Task<Result<PagedResult<MenuDto>>> GetListAsync(MenuQueryDto query)
    {
        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<Menu, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Asc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "menuname":
                        orderByExpression = m => m.MenuName;
                        break;
                    case "menucode":
                        orderByExpression = m => m.MenuCode;
                        break;
                    case "ordernum":
                        orderByExpression = m => m.OrderNum;
                        break;
                    case "createdtime":
                        orderByExpression = m => m.CreatedTime;
                        break;
                    default:
                        orderByExpression = m => m.OrderNum; // 默认按排序号
                        break;
                }
            }
            else
            {
                orderByExpression = m => m.OrderNum; // 默认按排序号升序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "desc")
            {
                orderByType = SqlSugar.OrderByType.Desc;
            }
            
            // 使用真实的数据库查询
            var result = await _menuRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var menuDtos = result.Items.Adapt<List<MenuDto>>();

            var pagedResult = new PagedResult<MenuDto>
            {
                Items = menuDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<MenuDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "查询菜单列表失败");
            return Result<PagedResult<MenuDto>>.Fail($"查询菜单数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取菜单详情
    /// </summary>
    /// <param name="menuId">菜单ID，必须大于0</param>
    /// <returns>包含菜单详情的结果对象，成功时返回菜单DTO，失败时返回错误信息（如菜单不存在）</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// </remarks>
    public async Task<Result<MenuDto>> GetMenuByIdAsync(long menuId)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
                return Result<MenuDto>.Fail("菜单不存在");

            return Result<MenuDto>.Ok(menu.Adapt<MenuDto>());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取菜单详情失败，菜单ID: {MenuId}", menuId);
            return Result<MenuDto>.Fail($"获取菜单详情失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 创建新菜单
    /// </summary>
    /// <param name="dto">创建菜单数据传输对象，包含菜单名称、菜单编码、菜单类型等菜单信息</param>
    /// <returns>包含新菜单信息的结果对象，成功时返回菜单DTO，失败时返回错误信息（如菜单编码已存在、父级菜单不存在）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result<MenuDto>> CreateMenuAsync(MenuCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 1. 验证菜单编码唯一性
            var existing = await _menuRepository.GetFirstAsync(m => m.MenuCode == dto.MenuCode && m.IsDeleted == 0);
            if (existing != null)
                return Result<MenuDto>.Fail($"菜单编码 {dto.MenuCode} 已存在");

            // 验证菜单名称唯一性
            var existsByName = await _menuRepository.GetFirstAsync(m => m.MenuName == dto.MenuName && m.IsDeleted == 0);
            if (existsByName != null)
                return Result<MenuDto>.Fail($"菜单名称 {dto.MenuName} 已存在");

            // 验证国际化键唯一性（如果提供了国际化键）
            if (!string.IsNullOrWhiteSpace(dto.I18nKey))
            {
                var existsByI18nKey = await _menuRepository.GetFirstAsync(m => m.I18nKey == dto.I18nKey && m.IsDeleted == 0);
                if (existsByI18nKey != null)
                    return Result<MenuDto>.Fail($"国际化键 {dto.I18nKey} 已被其他菜单使用");
            }

            // 2. 如果有父级菜单，验证父级存在
            if (dto.ParentId > 0)
            {
                var parent = await _menuRepository.GetByIdAsync(dto.ParentId);
                if (parent == null || parent.IsDeleted == 1)
                    return Result<MenuDto>.Fail("父级菜单不存在");
            }

            // 3. 创建菜单
            var menu = dto.Adapt<Menu>();
            if (dto.ParentId == 0) menu.ParentId = null;
            var result = await _menuRepository.CreateAsync(menu);
            var response = result > 0 ? Result<MenuDto>.Ok(menu.Adapt<MenuDto>()) : Result<MenuDto>.Fail("创建菜单失败");

            _operLog?.LogCreate("Menu", menu.Id.ToString(), "Identity.MenuView", 
                dto, response, stopwatch);

            _logger.Information("创建菜单成功：{MenuName}({MenuCode})", menu.MenuName, menu.MenuCode);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "创建菜单失败");
            return Result<MenuDto>.Fail($"创建菜单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 批量创建菜单
    /// </summary>
    /// <param name="dtos">创建菜单数据传输对象列表，不能为空</param>
    /// <returns>包含已创建菜单列表的结果对象，成功时返回菜单DTO列表，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 如果某个菜单编码已存在，会跳过该菜单继续处理其他菜单
    /// </remarks>
    public async Task<Result<List<MenuDto>>> CreateMenuBatchAsync(List<MenuCreateDto> dtos)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var createdMenus = new List<Menu>();

            foreach (var dto in dtos)
            {
                // 验证菜单编码唯一性
                var existing = await _menuRepository.GetFirstAsync(m => m.MenuCode == dto.MenuCode && m.IsDeleted == 0);
                if (existing != null)
                {
                    _logger.Warning("菜单编码 {MenuCode} 已存在，跳过", dto.MenuCode);
                    continue;
                }

                var menu = dto.Adapt<Menu>();
                if (dto.ParentId == 0) menu.ParentId = null;
                await _menuRepository.CreateAsync(menu);
                createdMenus.Add(menu);
            }

            var menuDtos = createdMenus.Select(m => m.Adapt<MenuDto>()).ToList();
            var response = Result<List<MenuDto>>.Ok(menuDtos);

            // 批量创建时，为每个创建的菜单记录操作日志
            if (createdMenus.Count > 0 && _operLog != null)
            {
                foreach (var menu in createdMenus)
                {
                    _operLog.LogCreate("Menu", menu.Id.ToString(), "Identity.MenuView", 
                        new { MenuName = menu.MenuName, MenuCode = menu.MenuCode, MenuType = menu.MenuType }, response, stopwatch);
                }
            }

            _logger.Information("批量创建菜单成功，共 {Count} 个", createdMenus.Count);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "批量创建菜单失败");
            return Result<List<MenuDto>>.Fail($"批量创建菜单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 更新菜单信息
    /// </summary>
    /// <param name="menuId">菜单ID，必须大于0</param>
    /// <param name="dto">更新菜单数据传输对象，包含要更新的字段信息</param>
    /// <returns>包含更新后菜单信息的结果对象，成功时返回菜单DTO，失败时返回错误信息（如菜单不存在、菜单编码已被使用、父级菜单不存在、循环引用）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、变更内容、操作时间、请求参数、执行耗时等信息
    /// 注意：不能将菜单的父级设置为自己（防止循环引用）
    /// </remarks>
    public async Task<Result<MenuDto>> UpdateMenuAsync(long menuId, MenuUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 1. 获取菜单
            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null || menu.IsDeleted == 1)
                return Result<MenuDto>.Fail("菜单不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldMenu = menu.Adapt<MenuUpdateDto>();

            // 2. 更新字段（只更新非null的字段）
            if (!string.IsNullOrEmpty(dto.MenuName))
            {
                // 验证菜单名称唯一性（如果菜单名称有变化）
                if (menu.MenuName != dto.MenuName)
                {
                    var existsByName = await _menuRepository.GetFirstAsync(m => m.MenuName == dto.MenuName && m.Id != menuId && m.IsDeleted == 0);
                    if (existsByName != null)
                        return Result<MenuDto>.Fail($"菜单名称 {dto.MenuName} 已被其他菜单使用");
                }
                menu.MenuName = dto.MenuName;
            }
            if (!string.IsNullOrEmpty(dto.MenuCode))
            {
                // 验证编码唯一性
                var existing = await _menuRepository.GetFirstAsync(m => m.MenuCode == dto.MenuCode && m.Id != menuId && m.IsDeleted == 0);
                if (existing != null)
                    return Result<MenuDto>.Fail($"菜单编码 {dto.MenuCode} 已被其他菜单使用");
                menu.MenuCode = dto.MenuCode;
            }
            if (dto.I18nKey != null)
            {
                // 验证国际化键唯一性（如果国际化键有变化）
                if (menu.I18nKey != dto.I18nKey && !string.IsNullOrWhiteSpace(dto.I18nKey))
                {
                    var existsByI18nKey = await _menuRepository.GetFirstAsync(m => m.I18nKey == dto.I18nKey && m.Id != menuId && m.IsDeleted == 0);
                    if (existsByI18nKey != null)
                        return Result<MenuDto>.Fail($"国际化键 {dto.I18nKey} 已被其他菜单使用");
                }
                menu.I18nKey = dto.I18nKey;
            }
            if (dto.PermCode != null) menu.PermCode = dto.PermCode;
            menu.MenuType = dto.MenuType;
            // 验证父级菜单
            if (dto.ParentId > 0)
            {
                var parent = await _menuRepository.GetByIdAsync(dto.ParentId);
                if (parent == null || parent.IsDeleted == 1)
                    return Result<MenuDto>.Fail("父级菜单不存在");
                
                // 防止循环引用
                if (dto.ParentId == menuId)
                    return Result<MenuDto>.Fail("不能将菜单的父级设置为自己");
                
                menu.ParentId = dto.ParentId;
            }
            else
            {
                menu.ParentId = null;
            }
            if (dto.RoutePath != null) menu.RoutePath = dto.RoutePath;
            if (dto.Icon != null) menu.Icon = dto.Icon;
            if (dto.Component != null) menu.Component = dto.Component;
            menu.IsExternal = dto.IsExternal;
            menu.IsCache = dto.IsCache;
            menu.IsVisible = dto.IsVisible;
            menu.OrderNum = dto.OrderNum;
            menu.MenuStatus = dto.MenuStatus;

            // 3. 保存更新
            var result = await _menuRepository.UpdateAsync(menu);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldMenu.MenuName != menu.MenuName) changeList.Add($"MenuName: {oldMenu.MenuName} -> {menu.MenuName}");
            if (oldMenu.MenuCode != menu.MenuCode) changeList.Add($"MenuCode: {oldMenu.MenuCode} -> {menu.MenuCode}");
            if (oldMenu.MenuType != menu.MenuType) changeList.Add($"MenuType: {oldMenu.MenuType} -> {menu.MenuType}");
            if (oldMenu.ParentId != menu.ParentId) changeList.Add($"ParentId: {oldMenu.ParentId} -> {menu.ParentId}");
            if (oldMenu.MenuStatus != menu.MenuStatus) changeList.Add($"MenuStatus: {oldMenu.MenuStatus} -> {menu.MenuStatus}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result<MenuDto>.Ok(menu.Adapt<MenuDto>()) : Result<MenuDto>.Fail("更新菜单失败");

            _operLog?.LogUpdate("Menu", menuId.ToString(), "Identity.MenuView", changes, dto, oldMenu, response, stopwatch);

            _logger.Information("更新菜单成功：{MenuName}({MenuId})", menu.MenuName, menuId);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "更新菜单失败，菜单ID: {MenuId}", menuId);
            return Result<MenuDto>.Fail($"更新菜单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 更新菜单状态（DTO方式）
    /// </summary>
    /// <param name="dto">菜单状态数据传输对象，包含菜单ID和状态值</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如菜单不存在）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result> UpdateMenuStatusAsync(MenuStatusDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var menu = await _menuRepository.GetByIdAsync(dto.Id);
            if (menu == null || menu.IsDeleted == 1)
                return Result.Fail("菜单不存在");

            var oldStatus = menu.MenuStatus;
            var result = await _menuRepository.StatusAsync(dto.Id, (int)dto.Status);
            var response = result > 0 ? Result.Ok("更新状态成功") : Result.Fail("更新状态失败");

            _operLog?.LogUpdate("Menu", dto.Id.ToString(), "Identity.MenuView", $"修改状态为 {dto.Status}",
                new { MenuId = dto.Id, OldStatus = oldStatus, NewStatus = dto.Status }, response, stopwatch);

            if (result > 0)
            {
                _logger.Information("更新菜单状态成功：{MenuName}({MenuId}), 状态: {Status}", menu.MenuName, dto.Id, (int)dto.Status);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "更新菜单状态失败，菜单ID: {MenuId}", dto.Id);
            return Result.Fail($"更新状态失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 调整菜单排序（DTO方式）
    /// </summary>
    /// <param name="dto">菜单排序数据传输对象，包含菜单ID和排序号</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如菜单不存在）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result> UpdateMenuOrderAsync(MenuOrderDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var menu = await _menuRepository.GetByIdAsync(dto.Id);
            if (menu == null || menu.IsDeleted == 1)
                return Result.Fail("菜单不存在");

            var oldOrderNum = menu.OrderNum;
            menu.OrderNum = dto.OrderNum;
            var result = await _menuRepository.UpdateAsync(menu);
            var response = result > 0 ? Result.Ok("调整排序成功") : Result.Fail("调整排序失败");

            _operLog?.LogUpdate("Menu", dto.Id.ToString(), "Identity.MenuView", $"调整排序号: {oldOrderNum} -> {dto.OrderNum}",
                new { MenuId = dto.Id, OldOrderNum = oldOrderNum, NewOrderNum = dto.OrderNum }, response, stopwatch);

            if (result > 0)
            {
                _logger.Information("调整菜单排序成功：{MenuName}({MenuId}), 排序号: {OrderNum}", menu.MenuName, dto.Id, dto.OrderNum);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "调整菜单排序失败，菜单ID: {MenuId}", dto.Id);
            return Result.Fail($"调整排序失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    /// <param name="menuId">菜单ID，必须大于0</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如菜单不存在、菜单下有子菜单、菜单正被角色使用）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 注意：如果菜单下有子菜单或正被角色使用，则不允许删除
    /// </remarks>
    public async Task<Result> DeleteMenuAsync(long menuId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null || menu.IsDeleted == 1)
                return Result.Fail("菜单不存在");

            // 检查是否有子菜单
            var hasChildren = await _menuRepository.GetCountAsync(m => m.ParentId == menuId && m.IsDeleted == 0) > 0;
            if (hasChildren)
                return Result.Fail("该菜单下还有子菜单，无法删除");

            // 检查是否被角色使用
            var roleMenuCount = await _roleMenuRepository.GetCountAsync(rm => rm.MenuId == menuId && rm.IsDeleted == 0);
            if (roleMenuCount > 0)
                return Result.Fail($"该菜单正被 {roleMenuCount} 个角色使用，无法删除");

            var result = await _menuRepository.DeleteAsync(menuId);
            var response = result > 0 ? Result.Ok("删除成功") : Result.Fail("删除失败");

            _operLog?.LogDelete("Menu", menuId.ToString(), "Identity.MenuView", 
                new { MenuId = menuId, MenuName = menu.MenuName }, response, stopwatch);

            _logger.Information("删除菜单成功：{MenuName}({MenuId})", menu.MenuName, menuId);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "删除菜单失败，菜单ID: {MenuId}", menuId);
            return Result.Fail($"删除失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除菜单
    /// </summary>
    /// <param name="menuIds">菜单ID列表，不能为空</param>
    /// <returns>操作结果对象，成功时返回删除统计信息（成功和失败数量），失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 会逐个删除菜单，如果某个菜单删除失败，会继续处理其他菜单
    /// </remarks>
    public async Task<Result> DeleteBatchAsync(List<long> menuIds)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var deletedCount = 0;
            var failedCount = 0;
            var deletedMenuInfos = new List<object>();

            foreach (var menuId in menuIds)
            {
                var menu = await _menuRepository.GetByIdAsync(menuId);
                if (menu == null || menu.IsDeleted == 1)
                {
                    failedCount++;
                    continue;
                }

                // 检查是否有子菜单
                var hasChildren = await _menuRepository.GetCountAsync(m => m.ParentId == menuId && m.IsDeleted == 0) > 0;
                if (hasChildren)
                {
                    failedCount++;
                    continue;
                }

                // 检查是否被角色使用
                var roleMenuCount = await _roleMenuRepository.GetCountAsync(rm => rm.MenuId == menuId && rm.IsDeleted == 0);
                if (roleMenuCount > 0)
                {
                    failedCount++;
                    continue;
                }

                var result = await _menuRepository.DeleteAsync(menuId);
                if (result > 0)
                {
                    deletedCount++;
                    deletedMenuInfos.Add(new { Id = menuId, MenuName = menu.MenuName });
                    
                    // 为每个删除的菜单记录操作日志
                    _operLog?.LogDelete("Menu", menuId.ToString(), "Identity.MenuView", 
                        new { Id = menuId, MenuName = menu.MenuName }, Result.Ok(), stopwatch);
                }
                else
                {
                    failedCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {deletedCount} 个，失败 {failedCount} 个");

            // 批量操作整体记录一条导入类型的日志
            _operLog?.LogImport("Menu", deletedCount, "Identity.MenuView", 
                new { Total = menuIds.Count, Success = deletedCount, Fail = failedCount, DeletedMenus = deletedMenuInfos }, response, stopwatch);

            _logger.Information("批量删除菜单完成：成功 {DeletedCount} 个，失败 {FailedCount} 个", deletedCount, failedCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "批量删除菜单失败");
            return Result.Fail($"批量删除失败：{ex.Message}");
        }
    }

    #endregion

    #region 导入导出

    /// <summary>
    /// 导出菜单到Excel
    /// </summary>
    /// <param name="menuIds">菜单ID列表，可选，为空则导出全部菜单</param>
    /// <param name="sheetName">工作表名称，可选，默认为 "Menus"</param>
    /// <param name="fileName">文件名，可选，默认为 "菜单导出_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容的结果对象，成功时返回文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于导出，不会记录操作日志
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(List<long>? menuIds = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            List<Menu> menus;
            if (menuIds != null && menuIds.Any())
            {
                menus = await _menuRepository.AsQueryable()
                    .Where(m => menuIds.Contains(m.Id) && m.IsDeleted == 0)
                    .OrderBy(m => m.OrderNum)
                    .ToListAsync();
            }
            else
            {
                menus = await _menuRepository.AsQueryable()
                    .Where(m => m.IsDeleted == 0)
                    .OrderBy(m => m.OrderNum)
                    .ToListAsync();
            }

            var menuDtos = menus.Select(m => m.Adapt<MenuDto>()).ToList();
            sheetName ??= "Menus";
            fileName ??= $"菜单导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = Takt.Common.Helpers.ExcelHelper.ExportToExcel(menuDtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {menuDtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导出菜单到Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出菜单 Excel 模板（仅表头，双行表头）
    /// </summary>
    /// <param name="sheetName">工作表名称，可选，默认为 "Menus"</param>
    /// <param name="fileName">文件名，可选，默认为 "菜单导入模板_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容的结果对象，成功时返回文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于导出模板，不会记录操作日志
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "Menus";
        fileName ??= $"菜单导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = Takt.Common.Helpers.ExcelHelper.ExportTemplate<MenuDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    // JSON 导出已不支持

    /// <summary>
    /// 从 Excel 导入菜单
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选，默认为 "Menus"</param>
    /// <returns>包含成功和失败数量的结果对象，成功时返回导入统计信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 如果菜单编码已存在，会更新该菜单；如果不存在，会创建新菜单
    /// </remarks>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "Menus";
            var menuDtos = Takt.Common.Helpers.ExcelHelper.ImportFromExcel<MenuDto>(fileStream, sheetName);
            if (menuDtos == null || !menuDtos.Any()) return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0, fail = 0;
            foreach (var dto in menuDtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.MenuCode)) { fail++; continue; }
                    var existing = await _menuRepository.GetFirstAsync(m => m.MenuCode == dto.MenuCode && m.IsDeleted == 0);
                    if (existing == null)
                    {
                        // 验证菜单名称唯一性
                        if (!string.IsNullOrWhiteSpace(dto.MenuName))
                        {
                            var existsByName = await _menuRepository.GetFirstAsync(m => m.MenuName == dto.MenuName && m.IsDeleted == 0);
                            if (existsByName != null) { fail++; continue; }
                        }

                        // 验证国际化键唯一性（如果提供了国际化键）
                        if (!string.IsNullOrWhiteSpace(dto.I18nKey))
                        {
                            var existsByI18nKey = await _menuRepository.GetFirstAsync(m => m.I18nKey == dto.I18nKey && m.IsDeleted == 0);
                            if (existsByI18nKey != null) { fail++; continue; }
                        }

                        var createDto = dto.Adapt<MenuCreateDto>();
                        var entity = createDto.Adapt<Menu>();
                        if (dto.ParentId == 0) entity.ParentId = null;
                        await _menuRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        // 验证菜单名称唯一性（如果菜单名称有变化）
                        if (existing.MenuName != dto.MenuName && !string.IsNullOrWhiteSpace(dto.MenuName))
                        {
                            var existsByName = await _menuRepository.GetFirstAsync(m => m.MenuName == dto.MenuName && m.Id != existing.Id && m.IsDeleted == 0);
                            if (existsByName != null) { fail++; continue; }
                        }

                        // 验证国际化键唯一性（如果国际化键有变化）
                        if (existing.I18nKey != dto.I18nKey && !string.IsNullOrWhiteSpace(dto.I18nKey))
                        {
                            var existsByI18nKey = await _menuRepository.GetFirstAsync(m => m.I18nKey == dto.I18nKey && m.Id != existing.Id && m.IsDeleted == 0);
                            if (existsByI18nKey != null) { fail++; continue; }
                        }

                        existing.MenuName = dto.MenuName;
                        existing.I18nKey = dto.I18nKey;
                        existing.PermCode = dto.PermCode;
                        existing.MenuType = dto.MenuType;
                        existing.ParentId = dto.ParentId == 0 ? null! : dto.ParentId;
                        existing.RoutePath = dto.RoutePath;
                        existing.Icon = dto.Icon;
                        existing.Component = dto.Component;
                        existing.IsExternal = dto.IsExternal;
                        existing.IsCache = dto.IsCache;
                        existing.IsVisible = dto.IsVisible;
                        existing.OrderNum = dto.OrderNum;
                        existing.MenuStatus = dto.MenuStatus;
                        existing.Remarks = dto.Remarks;
                        await _menuRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
            
            _operLog?.LogImport("Menu", success, "Identity.MenuView", 
                new { Total = menuDtos.Count, Success = success, Fail = fail, Items = menuDtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "从Excel导入菜单失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    #endregion

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<Menu, bool>> QueryExpression(MenuQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Menu>()
            .And(m => m.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), m => m.MenuName.Contains(query.Keywords!) || 
                                                               m.MenuCode.Contains(query.Keywords!) ||
                                                               (m.PermCode != null && m.PermCode.Contains(query.Keywords!)) ||
                                                               (m.I18nKey != null && m.I18nKey.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.MenuName), m => m.MenuName.Contains(query.MenuName!))
            .AndIF(!string.IsNullOrEmpty(query.MenuCode), m => m.MenuCode.Contains(query.MenuCode!))
            .AndIF(!string.IsNullOrEmpty(query.PermCode), m => m.PermCode != null && m.PermCode.Contains(query.PermCode!))
            .AndIF(query.MenuType.HasValue, m => m.MenuType == query.MenuType!.Value)
            .AndIF(query.IsExternal.HasValue, m => m.IsExternal == query.IsExternal!.Value)
            .ToExpression();
    }
}

