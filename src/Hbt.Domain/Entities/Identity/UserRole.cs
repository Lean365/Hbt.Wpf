//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : UserRole.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-20
// 版本号 : 1.0
// 描述    : 用户角色关联实体
//===================================================================

using SqlSugar;

namespace Hbt.Domain.Entities.Identity;

/// <summary>
/// 用户角色关联实体
/// </summary>
/// <remarks>
/// 用户和角色的多对多关系中间表
/// </remarks>
[SugarTable("hbt_oidc_user_role", "用户角色关联表")]
[SugarIndex("IX_hbt_oidc_user_role_user_id", nameof(UserRole.UserId), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_oidc_user_role_role_id", nameof(UserRole.RoleId), OrderByType.Asc, false)]
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

