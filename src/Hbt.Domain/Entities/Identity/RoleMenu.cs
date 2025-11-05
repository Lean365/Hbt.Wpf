//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : RoleMenu.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-20
// 版本号 : 1.0
// 描述    : 角色菜单关联实体
//===================================================================

using SqlSugar;

namespace Hbt.Domain.Entities.Identity;

/// <summary>
/// 角色菜单关联实体
/// </summary>
/// <remarks>
/// 角色和菜单的多对多关系中间表
/// </remarks>
[SugarTable("hbt_oidc_role_menu", "角色菜单关联表")]
[SugarIndex("IX_hbt_oidc_role_menu_role_id", nameof(RoleMenu.RoleId), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_oidc_role_menu_menu_id", nameof(RoleMenu.MenuId), OrderByType.Asc, false)]
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

