// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Dtos.Identity
// 文件名称：MenuDto.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：菜单数据传输对象
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Enums;
using Takt.Common.Results;

namespace Takt.Application.Dtos.Identity;

/// <summary>
/// 菜单数据传输对象
/// </summary>
public class MenuDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public MenuDto()
    {
        MenuName = string.Empty;
        MenuCode = string.Empty;
        I18nKey = string.Empty;
        PermCode = string.Empty;
        RoutePath = string.Empty;
        Icon = string.Empty;
        Component = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 菜单ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; }

    /// <summary>
    /// 权限码
    /// </summary>
    public string PermCode { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    /// <remarks>
    /// 0=目录, 1=菜单, 2=按钮, 3=API
    /// </remarks>
    public MenuTypeEnum MenuType { get; set; }

    /// <summary>
    /// 父级菜单ID
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string RoutePath { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 是否外链（0=外链，1=不是外链）
    /// </summary>
    public ExternalEnum IsExternal { get; set; } = ExternalEnum.NotExternal;

    /// <summary>
    /// 是否缓存（0=缓存，1=不缓存）
    /// </summary>
    public CacheEnum IsCache { get; set; } = CacheEnum.NoCache;

    /// <summary>
    /// 是否可见（0=可见，1=不可见）
    /// </summary>
    public VisibilityEnum IsVisible { get; set; } = VisibilityEnum.Visible;

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 菜单状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum MenuStatus { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdatedBy { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }
    /// <summary>
    /// 是否删除
    /// </summary>
    public int IsDeleted { get; set; }
    /// <summary>
    /// 删除人
    /// </summary>
    public string DeletedBy { get; set; }
    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime DeletedTime { get; set; }

    /// <summary>
    /// 子菜单列表
    /// </summary>
    public List<MenuDto>? Children { get; set; }
}

/// <summary>
/// 用户菜单树响应对象
/// </summary>
public class UserMenuTreeDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public UserMenuTreeDto()
    {
        Username = string.Empty;
        RoleIds = new List<long>();
        Menus = new List<MenuDto>();
        Permissions = new List<string>();
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 角色ID列表
    /// </summary>
    public List<long> RoleIds { get; set; }

    /// <summary>
    /// 菜单树
    /// </summary>
    public List<MenuDto> Menus { get; set; }

    /// <summary>
    /// 权限码列表
    /// </summary>
    public List<string> Permissions { get; set; }
}

/// <summary>
/// 菜单查询数据传输对象
/// 用于查询菜单信息
/// </summary>
public class MenuQueryDto : PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public MenuQueryDto()
    {
        Keywords = string.Empty;
        MenuName = string.Empty;
        MenuCode = string.Empty;
        PermCode = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在菜单名称、菜单编码、权限码、国际化键中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 权限码
    /// </summary>
    public string PermCode { get; set; }

    /// <summary>
    /// 菜单类型（0=目录, 1=菜单, 2=按钮, 3=API）
    /// </summary>
    public MenuTypeEnum? MenuType { get; set; }


    /// <summary>
    /// 是否外链（0=否，1=是）
    /// </summary>
    public ExternalEnum? IsExternal { get; set; }

    /// <summary>
    /// 是否缓存（0=否，1=是）
    /// </summary>
    public CacheEnum? IsCache { get; set; }

    /// <summary>
    /// 是否可见（0=否，1=是）
    /// </summary>
    public VisibilityEnum? IsVisible { get; set; }

    /// <summary>
    /// 菜单状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum? MenuStatus { get; set; }
}

/// <summary>
/// 创建菜单DTO
/// </summary>
public class MenuCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public MenuCreateDto()
    {
        MenuName = string.Empty;
        MenuCode = string.Empty;
        I18nKey = string.Empty;
        PermCode = string.Empty;
        RoutePath = string.Empty;
        Icon = string.Empty;
        Component = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; }

    /// <summary>
    /// 权限码
    /// </summary>
    public string PermCode { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public MenuTypeEnum MenuType { get; set; }

    /// <summary>
    /// 父级菜单ID（0表示顶级菜单）
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string RoutePath { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 是否外链（0=外链，1=不是外链）
    /// </summary>
    public ExternalEnum IsExternal { get; set; } = ExternalEnum.NotExternal;

    /// <summary>
    /// 是否缓存（0=缓存，1=不缓存）
    /// </summary>
    public CacheEnum IsCache { get; set; } = CacheEnum.NoCache;

    /// <summary>
    /// 是否可见（0=可见，1=不可见）
    /// </summary>
    public VisibilityEnum IsVisible { get; set; } = VisibilityEnum.Visible;

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 菜单状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum MenuStatus { get; set; } = StatusEnum.Normal;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新菜单DTO
/// </summary>
public class MenuUpdateDto : MenuCreateDto
{
    /// <summary>
    /// 菜单ID
    /// </summary>
    public long Id { get; set; }

}

/// <summary>
/// 菜单状态更新 DTO（启用/禁用）
/// </summary>
public class MenuStatusDto
{
    /// <summary>
    /// 菜单ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 新状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum Status { get; set; }
}

/// <summary>
/// 菜单排序 DTO
/// </summary>
public class MenuOrderDto
{
    /// <summary>
    /// 菜单ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 新的排序号
    /// </summary>
    public int OrderNum { get; set; }
}

/// <summary>
/// 菜单导出数据传输对象
/// </summary>
public class MenuExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public MenuExportDto()
    {
        MenuName = string.Empty;
        MenuCode = string.Empty;
        I18nKey = string.Empty;
        PermCode = string.Empty;
        RoutePath = string.Empty;
        Icon = string.Empty;
        Component = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 菜单ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; }

    /// <summary>
    /// 权限码
    /// </summary>
    public string PermCode { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    /// <remarks>
    /// 0=目录, 1=菜单, 2=按钮, 3=API
    /// </remarks>
    public MenuTypeEnum MenuType { get; set; }

    /// <summary>
    /// 父级菜单ID
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string RoutePath { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 是否外链（0=外链，1=不是外链）
    /// </summary>
    public ExternalEnum IsExternal { get; set; }

    /// <summary>
    /// 是否缓存（0=缓存，1=不缓存）
    /// </summary>
    public CacheEnum IsCache { get; set; }

    /// <summary>
    /// 是否可见（0=可见，1=不可见）
    /// </summary>
    public VisibilityEnum IsVisible { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 菜单状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum MenuStatus { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 菜单导入模板数据传输对象
/// </summary>
public class MenuTemplateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public MenuTemplateDto()
    {
        MenuName = string.Empty;
        MenuCode = string.Empty;
        I18nKey = string.Empty;
        PermCode = string.Empty;
        RoutePath = string.Empty;
        Icon = string.Empty;
        Component = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; }

    /// <summary>
    /// 权限码
    /// </summary>
    public string PermCode { get; set; }

    /// <summary>
    /// 菜单类型（0=目录, 1=菜单, 2=按钮, 3=API）
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    /// 父级菜单ID（0表示顶级菜单）
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string RoutePath { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 是否外链（0=外链，1=不是外链）
    /// </summary>
    public int IsExternal { get; set; }

    /// <summary>
    /// 是否缓存（0=缓存，1=不缓存）
    /// </summary>
    public int IsCache { get; set; }

    /// <summary>
    /// 是否可见（0=可见，1=不可见）
    /// </summary>
    public int IsVisible { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 菜单状态（0=启用，1=禁用）
    /// </summary>
    public int MenuStatus { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 菜单导入数据传输对象
/// </summary>
public class MenuImportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public MenuImportDto()
    {
        MenuName = string.Empty;
        MenuCode = string.Empty;
        I18nKey = string.Empty;
        PermCode = string.Empty;
        RoutePath = string.Empty;
        Icon = string.Empty;
        Component = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; }

    /// <summary>
    /// 权限码
    /// </summary>
    public string PermCode { get; set; }

    /// <summary>
    /// 菜单类型（0=目录, 1=菜单, 2=按钮, 3=API）
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    /// 父级菜单ID（0表示顶级菜单）
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string RoutePath { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 是否外链（0=外链，1=不是外链）
    /// </summary>
    public int IsExternal { get; set; }

    /// <summary>
    /// 是否缓存（0=缓存，1=不缓存）
    /// </summary>
    public int IsCache { get; set; }

    /// <summary>
    /// 是否可见（0=可见，1=不可见）
    /// </summary>
    public int IsVisible { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 菜单状态（0=启用，1=禁用）
    /// </summary>
    public int MenuStatus { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

