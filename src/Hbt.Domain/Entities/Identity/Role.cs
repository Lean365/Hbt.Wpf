// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：Role.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：角色实体
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

using Hbt.Common.Enums;
using SqlSugar;

namespace Hbt.Domain.Entities.Identity;

/// <summary>
/// OIDC角色实体
/// 用于定义系统中的用户角色和权限
/// </summary>
[SugarTable("hbt_oidc_role", "角色表")]
[SugarIndex("IX_hbt_oidc_role_role_code", nameof(Role.RoleCode), OrderByType.Asc, true)]
[SugarIndex("IX_hbt_oidc_role_status", nameof(Role.RoleStatus), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_oidc_role_order_num", nameof(Role.OrderNum), OrderByType.Asc, false)]
public class Role : BaseEntity
{
    /// <summary>
    /// 角色名称
    /// 角色的显示名称
    /// </summary>
    [SugarColumn(ColumnName = "role_name", ColumnDescription = "角色名称", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 角色编码
    /// 角色的唯一编码，用于程序识别
    /// </summary>
    [SugarColumn(ColumnName = "role_code", ColumnDescription = "角色编码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    /// <remarks>
    /// 角色的详细描述信息
    /// </remarks>
    [SugarColumn(ColumnName = "description", ColumnDescription = "描述", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
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