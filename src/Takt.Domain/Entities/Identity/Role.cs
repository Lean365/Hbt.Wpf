// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Identity
// 文件名称：Role.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：角色实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Enums;
using SqlSugar;

namespace Takt.Domain.Entities.Identity;

/// <summary>
/// OIDC角色实体
/// 用于定义系统中的用户角色和权限
/// </summary>
[SugarTable("takt_oidc_role", "角色表")]
[SugarIndex("IX_takt_oidc_role_role_code", nameof(Role.RoleCode), OrderByType.Asc, true)]
[SugarIndex("IX_takt_oidc_role_status", nameof(Role.RoleStatus), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_role_order_num", nameof(Role.OrderNum), OrderByType.Asc, false)]
public class Role : BaseEntity
{
    /// <summary>
    /// 角色名称
    /// 角色的显示名称
    /// </summary>
    [SugarColumn(ColumnName = "role_name", ColumnDescription = "角色名称", ColumnDataType = "nvarchar", Length = 128, IsNullable = false)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 角色编码
    /// 角色的唯一编码，用于程序识别
    /// </summary>
    [SugarColumn(ColumnName = "role_code", ColumnDescription = "角色编码", ColumnDataType = "nvarchar", Length = 10, IsNullable = false)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    /// <remarks>
    /// 角色的详细描述信息
    /// </remarks>
    [SugarColumn(ColumnName = "description", ColumnDescription = "描述", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string? Description { get; set; }

    /// <summary>
    /// 数据范围
    /// </summary>
    /// <remarks>
    /// 1=全部数据, 2=本部门及以下, 3=本部门, 4=仅本人, 5=自定义
    /// </remarks>
    [SugarColumn(ColumnName = "data_scope", ColumnDescription = "数据范围", ColumnDataType = "int", IsNullable = false, DefaultValue = "4")]
    public DataScopeEnum DataScope { get; set; } = DataScopeEnum.Self;

    /// <summary>
    /// 角色用户数
    /// </summary>
    /// <remarks>
    /// 拥有该角色的用户数量，用于统计展示
    /// </remarks>
    [SugarColumn(ColumnName = "user_count", ColumnDescription = "用户数", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int UserCount { get; set; } = 0;

    /// <summary>
    /// 排序号
    /// </summary>
    /// <remarks>
    /// 用于控制角色在列表中的显示顺序，数值越小越靠前
    /// </remarks>
    [SugarColumn(ColumnName = "order_num", ColumnDescription = "排序号", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 状态（0=启用，1=禁用）
    /// </summary>
    [SugarColumn(ColumnName = "role_status", ColumnDescription = "状态", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public StatusEnum RoleStatus { get; set; } = StatusEnum.Normal;

    /// <summary>
    /// 关联用户集合
    /// </summary>
    /// <remarks>
    /// 拥有该角色的所有用户（多对多关系）
    /// </remarks>
    [Navigate(typeof(UserRole), nameof(UserRole.RoleId), nameof(UserRole.UserId))]
    public List<User>? Users { get; set; }

    /// <summary>
    /// 关联菜单集合
    /// </summary>
    /// <remarks>
    /// 该角色拥有的所有菜单权限（多对多关系）
    /// </remarks>
    [Navigate(typeof(RoleMenu), nameof(RoleMenu.RoleId), nameof(RoleMenu.MenuId))]
    public List<Menu>? Menus { get; set; }
}