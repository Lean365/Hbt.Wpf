//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Identity
// 文件名称：UserRole.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：用户角色关联实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Identity;

/// <summary>
/// 用户角色关联实体
/// </summary>
/// <remarks>
/// 用户和角色的多对多关系中间表
/// </remarks>
[SugarTable("takt_oidc_user_role", "用户角色关联表")]
[SugarIndex("IX_takt_oidc_user_role_user_id", nameof(UserRole.UserId), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_user_role_role_id", nameof(UserRole.RoleId), OrderByType.Asc, false)]
public class UserRole : BaseEntity
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnName = "user_id", ColumnDescription = "用户ID", IsNullable = false)]
    public long UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnName = "role_id", ColumnDescription = "角色ID", IsNullable = false)]
    public long RoleId { get; set; }
}

