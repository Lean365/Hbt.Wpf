//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Identity
// 文件名称：RoleMenu.cs
// 创建人：Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：角色菜单关联实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Identity;

/// <summary>
/// 角色菜单关联实体
/// </summary>
/// <remarks>
/// 角色和菜单的多对多关系中间表
/// </remarks>
[SugarTable("takt_oidc_role_menu", "角色菜单关联表")]
[SugarIndex("IX_takt_oidc_role_menu_role_id", nameof(RoleMenu.RoleId), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_role_menu_menu_id", nameof(RoleMenu.MenuId), OrderByType.Asc, false)]
public class RoleMenu : BaseEntity
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnName = "role_id", ColumnDescription = "角色ID", IsNullable = false)]
    public long RoleId { get; set; }

    /// <summary>
    /// 菜单ID
    /// </summary>
    [SugarColumn(ColumnName = "menu_id", ColumnDescription = "菜单ID", IsNullable = false)]
    public long MenuId { get; set; }
}

