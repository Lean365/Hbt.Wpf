// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Identity
// 文件名称：User.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：用户实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Enums;
using SqlSugar;

namespace Takt.Domain.Entities.Identity;

/// <summary>
/// OIDC用户实体
/// 用于存储系统用户的基本信息和认证相关数据
/// </summary>
[SugarTable("takt_oidc_user", "用户表")]
[SugarIndex("IX_takt_oidc_user_username", nameof(User.Username), OrderByType.Asc, true)]
[SugarIndex("IX_takt_oidc_user_email", nameof(User.Email), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_user_phone", nameof(User.Phone), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_user_user_type", nameof(User.UserType), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_user_status", nameof(User.UserStatus), OrderByType.Asc, false)]
public class User : BaseEntity
{
    /// <summary>
    /// 用户名
    /// 用于登录的唯一标识符
    /// </summary>
    [SugarColumn(ColumnName = "username", ColumnDescription = "用户名", ColumnDataType = "nvarchar", Length = 10, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// 用户登录密码，建议加密存储
    /// </summary>
    [SugarColumn(ColumnName = "password", ColumnDescription = "密码", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱地址
    /// 用户邮箱，可用于找回密码等操作
    /// </summary>
    [SugarColumn(ColumnName = "email", ColumnDescription = "邮箱", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string? Email { get; set; }

    /// <summary>
    /// 手机号码
    /// 用户手机号，可用于短信验证等操作
    /// </summary>
    [SugarColumn(ColumnName = "phone", ColumnDescription = "手机号", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string? Phone { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    /// <remarks>
    /// 用户的真实姓名
    /// </remarks>
        [SugarColumn(ColumnName = "real_name", ColumnDescription = "真实姓名", ColumnDataType = "nvarchar", Length = 128, IsNullable = false)]
        public string? RealName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    /// <remarks>
    /// 用户的昵称，用于显示
    /// </remarks>
    [SugarColumn(ColumnName = "nickname", ColumnDescription = "昵称", ColumnDataType = "nvarchar", Length = 128, IsNullable = false)]
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// 用户类型
    /// </summary>
    /// <remarks>
    /// 0=系统用户, 1=普通用户
    /// </remarks>
    [SugarColumn(ColumnName = "user_type", ColumnDescription = "用户类型", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public UserTypeEnum UserType { get; set; } = UserTypeEnum.System;

    /// <summary>
    /// 用户性别
    /// </summary>
    /// <remarks>
    /// 0=未知, 1=男, 2=女
    /// </remarks>
    [SugarColumn(ColumnName = "user_gender", ColumnDescription = "性别", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public UserGenderEnum UserGender { get; set; } = UserGenderEnum.Unknown;

    /// <summary>
    /// 用户头像
    /// </summary>
    /// <remarks>
    /// 存储头像文件的路径或 URL
    /// </remarks>
    [SugarColumn(ColumnName = "avatar", ColumnDescription = "用户头像", ColumnDataType = "nvarchar", Length = 256, IsNullable = true)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 状态（0=启用，1=禁用）
    /// </summary>
    [SugarColumn(ColumnName = "user_status", ColumnDescription = "状态", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public StatusEnum UserStatus { get; set; } = StatusEnum.Normal;

    /// <summary>
    /// 关联角色集合
    /// </summary>
    /// <remarks>
    /// 用户拥有的所有角色（多对多关系）
    /// </remarks>
    [Navigate(typeof(UserRole), nameof(UserRole.UserId), nameof(UserRole.RoleId))]
    public List<Role>? Roles { get; set; }
}