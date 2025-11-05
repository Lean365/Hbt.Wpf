//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : MenuService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-21
// 版本号 : 1.0
// 描述    : 菜单服务实现
//===================================================================

using Hbt.Application.Dtos.Identity;
using Hbt.Common.Enums;
using Hbt.Common.Logging;
using Hbt.Common.Results;
using Hbt.Domain.Entities.Identity;
using Hbt.Domain.Repositories;
using Mapster;

namespace Hbt.Application.Services.Identity;

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

    public MenuService(
        IBaseRepository<Menu> menuRepository,
        IBaseRepository<User> userRepository,
        IBaseRepository<Role> roleRepository,
        IBaseRepository<UserRole> userRoleRepository,
        IBaseRepository<RoleMenu> roleMenuRepository,
        AppLogManager logger)
    {
        _menuRepository = menuRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _roleMenuRepository = roleMenuRepository;
        _logger = logger;
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
    /// <param name="roleIds">角色ID列表</param>
    /// <returns>菜单树</returns>
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
    /// <returns>完整菜单树</returns>
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
    /// <param name="userId">用户ID</param>
    /// <returns>权限码列表</returns>
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
    /// 获取菜单列表（分页）
    /// </summary>
    public async Task<Result<PagedResult<MenuDto>>> GetMenuPagedListAsync(int pageIndex, int pageSize, string? keyword = null)
    {
        try
        {
            // 构建查询条件表达式
            System.Linq.Expressions.Expression<Func<Menu, bool>>? condition = null;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                condition = m => m.IsDeleted == 0 && 
                    (m.MenuName.Contains(keyword) || 
                    m.MenuCode.Contains(keyword) ||
                    (m.PermCode != null && m.PermCode.Contains(keyword)));
            }
            else
            {
                condition = m => m.IsDeleted == 0;
            }

            var result = await _menuRepository.GetListAsync(
                condition: condition,
                pageIndex: pageIndex,
                pageSize: pageSize,
                orderByExpression: m => m.OrderNum);

            var dtos = result.Items.Select(m => m.Adapt<MenuDto>()).ToList();

            return Result<PagedResult<MenuDto>>.Ok(new PagedResult<MenuDto>
            {
                Items = dtos,
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取菜单分页列表失败");
            return Result<PagedResult<MenuDto>>.Fail($"获取菜单列表失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取菜单详情
    /// </summary>
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
    /// 创建菜单
    /// </summary>
    public async Task<Result<MenuDto>> CreateMenuAsync(MenuCreateDto dto)
    {
        try
        {
            // 1. 验证菜单编码唯一性
            var existing = await _menuRepository.GetFirstAsync(m => m.MenuCode == dto.MenuCode && m.IsDeleted == 0);
            if (existing != null)
                return Result<MenuDto>.Fail($"菜单编码 {dto.MenuCode} 已存在");

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
            await _menuRepository.CreateAsync(menu);

            _logger.Information("创建菜单成功：{MenuName}({MenuCode})", menu.MenuName, menu.MenuCode);

            return Result<MenuDto>.Ok(menu.Adapt<MenuDto>());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "创建菜单失败");
            return Result<MenuDto>.Fail($"创建菜单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 批量创建菜单
    /// </summary>
    public async Task<Result<List<MenuDto>>> CreateMenuBatchAsync(List<MenuCreateDto> dtos)
    {
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

            _logger.Information("批量创建菜单成功，共 {Count} 个", createdMenus.Count);

            var menuDtos = createdMenus.Select(m => m.Adapt<MenuDto>()).ToList();
            return Result<List<MenuDto>>.Ok(menuDtos);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "批量创建菜单失败");
            return Result<List<MenuDto>>.Fail($"批量创建菜单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 更新菜单
    /// </summary>
    public async Task<Result<MenuDto>> UpdateMenuAsync(long menuId, MenuUpdateDto dto)
    {
        try
        {
            // 1. 获取菜单
            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null || menu.IsDeleted == 1)
                return Result<MenuDto>.Fail("菜单不存在");

            // 2. 更新字段（只更新非null的字段）
            if (!string.IsNullOrEmpty(dto.MenuName))
                menu.MenuName = dto.MenuName;
            if (!string.IsNullOrEmpty(dto.MenuCode))
            {
                // 验证编码唯一性
                var existing = await _menuRepository.GetFirstAsync(m => m.MenuCode == dto.MenuCode && m.Id != menuId && m.IsDeleted == 0);
                if (existing != null)
                    return Result<MenuDto>.Fail($"菜单编码 {dto.MenuCode} 已被其他菜单使用");
                menu.MenuCode = dto.MenuCode;
            }
            if (dto.I18nKey != null) menu.I18nKey = dto.I18nKey;
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
            await _menuRepository.UpdateAsync(menu);

            _logger.Information("更新菜单成功：{MenuName}({MenuId})", menu.MenuName, menuId);

            return Result<MenuDto>.Ok(menu.Adapt<MenuDto>());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "更新菜单失败，菜单ID: {MenuId}", menuId);
            return Result<MenuDto>.Fail($"更新菜单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 更新菜单状态（DTO）
    /// </summary>
    public async Task<Result> UpdateMenuStatusAsync(MenuStatusDto dto)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(dto.Id);
            if (menu == null || menu.IsDeleted == 1)
                return Result.Fail("菜单不存在");

            await _menuRepository.StatusAsync(dto.Id, (int)dto.Status);

            _logger.Information("更新菜单状态成功：{MenuName}({MenuId}), 状态: {Status}", menu.MenuName, dto.Id, (int)dto.Status);

            return Result.Ok("更新状态成功");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "更新菜单状态失败，菜单ID: {MenuId}", dto.Id);
            return Result.Fail($"更新状态失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 调整菜单排序（DTO）
    /// </summary>
    public async Task<Result> UpdateMenuOrderAsync(MenuOrderDto dto)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(dto.Id);
            if (menu == null || menu.IsDeleted == 1)
                return Result.Fail("菜单不存在");

            menu.OrderNum = dto.OrderNum;
            await _menuRepository.UpdateAsync(menu);

            _logger.Information("调整菜单排序成功：{MenuName}({MenuId}), 排序号: {OrderNum}", menu.MenuName, dto.Id, dto.OrderNum);

            return Result.Ok("调整排序成功");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "调整菜单排序失败，菜单ID: {MenuId}", dto.Id);
            return Result.Fail($"调整排序失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    public async Task<Result> DeleteMenuAsync(long menuId)
    {
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

            await _menuRepository.DeleteAsync(menuId);

            _logger.Information("删除菜单成功：{MenuName}({MenuId})", menu.MenuName, menuId);

            return Result.Ok("删除成功");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "删除菜单失败，菜单ID: {MenuId}", menuId);
            return Result.Fail($"删除失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除菜单
    /// </summary>
    public async Task<Result> DeleteMenuBatchAsync(List<long> menuIds)
    {
        try
        {
            var deletedCount = 0;
            var failedCount = 0;

            foreach (var menuId in menuIds)
            {
                var result = await DeleteMenuAsync(menuId);
                if (result.Success)
                    deletedCount++;
                else
                    failedCount++;
            }

            _logger.Information("批量删除菜单完成：成功 {DeletedCount} 个，失败 {FailedCount} 个", deletedCount, failedCount);

            return Result.Ok($"删除完成：成功 {deletedCount} 个，失败 {failedCount} 个");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "批量删除菜单失败");
            return Result.Fail($"批量删除失败：{ex.Message}");
        }
    }

    #endregion

    #region 导入导出

    /// <summary>
    /// 导出菜单到Excel
    /// </summary>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(List<long>? menuIds = null, string? sheetName = null, string? fileName = null)
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
            var bytes = Hbt.Common.Helpers.ExcelHelper.ExportToExcel(menuDtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {menuDtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导出菜单到Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "Menus";
        fileName ??= $"菜单导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = Hbt.Common.Helpers.ExcelHelper.ExportTemplate<MenuDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    // JSON 导出已不支持

    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "Menus";
            var menuDtos = Hbt.Common.Helpers.ExcelHelper.ImportFromExcel<MenuDto>(fileStream, sheetName);
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
                        var createDto = dto.Adapt<MenuCreateDto>();
                        var entity = createDto.Adapt<Menu>();
                        if (dto.ParentId == 0) entity.ParentId = null;
                        await _menuRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        existing.MenuName = dto.MenuName;
                        existing.I18nKey = dto.I18nKey;
                        existing.PermCode = dto.PermCode;
                        existing.MenuType = dto.MenuType;
                        existing.ParentId = dto.ParentId == 0 ? null : dto.ParentId;
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
            return Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "从Excel导入菜单失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    #endregion
}

