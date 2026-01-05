//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Identity
// 文件名 : Menu.cs
// 创建人：Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：菜单实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;
using Takt.Common.Enums;

namespace Takt.Domain.Entities.Identity;

/// <summary>
/// OIDC菜单实体
/// </summary>
/// <remarks>
/// 定义系统菜单和权限（目录、菜单、按钮、API）
/// </remarks>
[SugarTable("takt_oidc_menu", "菜单表")]
[SugarIndex("IX_takt_oidc_menu_menu_code", nameof(Menu.MenuCode), OrderByType.Asc, true)]
[SugarIndex("IX_takt_oidc_menu_parent_id", nameof(Menu.ParentId), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_menu_menu_type", nameof(Menu.MenuType), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_menu_status", nameof(Menu.MenuStatus), OrderByType.Asc, false)]
public class Menu : BaseEntity
{
    /// <summary>
    /// 菜单名称
    /// </summary>
    [SugarColumn(ColumnName = "menu_name", ColumnDescription = "菜单名称", ColumnDataType = "nvarchar", Length = 40, IsNullable = false)]
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 菜单编码
    /// </summary>
    /// <remarks>
    /// 唯一标识，如：user_management, system_settings
    /// </remarks>
    [SugarColumn(ColumnName = "menu_code", ColumnDescription = "菜单编码", ColumnDataType = "nvarchar", Length = 40, IsNullable = false)]
    public string MenuCode { get; set; } = string.Empty;

    /// <summary>
    /// 国际化键
    /// </summary>
    /// <remarks>
    /// 用于多语言翻译，如：menu.user.management
    /// </remarks>
    [SugarColumn(ColumnName = "i18n_key", ColumnDescription = "国际化键", ColumnDataType = "nvarchar", Length = 64, IsNullable = false)]
    public string? I18nKey { get; set; }

    /// <summary>
    /// 权限码
    /// </summary>
    /// <remarks>
    /// 后端权限验证标识，如：user:list, user:add, user:edit, user:delete
    /// </remarks>
    [SugarColumn(ColumnName = "perm_code", ColumnDescription = "权限码", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string? PermCode { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    /// <remarks>
    /// 0=目录, 1=菜单, 2=按钮, 3=API
    /// </remarks>
    [SugarColumn(ColumnName = "menu_type", ColumnDescription = "菜单类型", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public MenuTypeEnum MenuType { get; set; } = MenuTypeEnum.Directory;

    /// <summary>
    /// 父级菜单ID
    /// </summary>
    [SugarColumn(ColumnName = "parent_id", ColumnDescription = "父级ID", IsNullable = true)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    /// <remarks>
    /// 前端路由路径或API路径
    /// </remarks>
    [SugarColumn(ColumnName = "route_path", ColumnDescription = "路由路径", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? RoutePath { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(ColumnName = "icon", ColumnDescription = "图标", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    /// <remarks>
    /// 前端组件路径
    /// </remarks>
    [SugarColumn(ColumnName = "component", ColumnDescription = "组件路径", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? Component { get; set; }

    /// <summary>
    /// 是否外链（0=外链，1=不是外链）
    /// </summary>
    [SugarColumn(ColumnName = "is_external", ColumnDescription = "是否外链", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public ExternalEnum IsExternal { get; set; } = ExternalEnum.NotExternal;

    /// <summary>
    /// 是否缓存（0=缓存，1=不缓存）
    /// </summary>
    [SugarColumn(ColumnName = "is_cache", ColumnDescription = "是否缓存", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public CacheEnum IsCache { get; set; } = CacheEnum.NoCache;

    /// <summary>
    /// 是否可见（0=可见，1=不可见）
    /// </summary>
    [SugarColumn(ColumnName = "is_visible", ColumnDescription = "是否可见", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public VisibilityEnum IsVisible { get; set; } = VisibilityEnum.Visible;

    /// <summary>
    /// 排序号
    /// </summary>
    [SugarColumn(ColumnName = "order_num", ColumnDescription = "排序号", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 状态（0=启用，1=禁用）
    /// </summary>
    [SugarColumn(ColumnName = "menu_status", ColumnDescription = "状态", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public StatusEnum MenuStatus { get; set; } = StatusEnum.Normal;

    /// <summary>
    /// 关联角色集合
    /// </summary>
    /// <remarks>
    /// 拥有该菜单权限的所有角色（多对多关系）
    /// </remarks>
    [Navigate(typeof(RoleMenu), nameof(RoleMenu.MenuId), nameof(RoleMenu.RoleId))]
    public List<Role>? Roles { get; set; }
}

