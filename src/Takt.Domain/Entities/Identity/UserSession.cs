//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Identity
// 文件名称：UserSession.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：用户会话实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Identity;

/// <summary>
/// 用户会话实体
/// </summary>
/// <remarks>
/// 记录用户登录会话信息，支持多用户并发
/// </remarks>
[SugarTable("takt_oidc_user_session", "用户会话表")]
[SugarIndex("IX_takt_oidc_user_session_session_id", nameof(UserSession.SessionId), OrderByType.Asc, true)]
[SugarIndex("IX_takt_oidc_user_session_user_id", nameof(UserSession.UserId), OrderByType.Asc, false)]
[SugarIndex("IX_takt_oidc_user_session_expires_at", nameof(UserSession.ExpiresAt), OrderByType.Desc, false)]
[SugarIndex("IX_takt_oidc_user_session_is_active", nameof(UserSession.IsActive), OrderByType.Asc, false)]
public class UserSession : BaseEntity
{
    /// <summary>
    /// 会话ID
    /// </summary>
    /// <remarks>
    /// 全局唯一标识符，用于标识用户会话
    /// </remarks>
    [SugarColumn(ColumnName = "session_id", ColumnDescription = "会话ID", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnName = "user_id", ColumnDescription = "用户ID", IsNullable = false)]
    public long UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "username", ColumnDescription = "用户名", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [SugarColumn(ColumnName = "real_name", ColumnDescription = "真实姓名", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? RealName { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnName = "role_id", ColumnDescription = "角色ID", IsNullable = false)]
    public long RoleId { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    [SugarColumn(ColumnName = "role_name", ColumnDescription = "角色名称", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 登录时间
    /// </summary>
    [SugarColumn(ColumnName = "login_time", ColumnDescription = "登录时间", IsNullable = false)]
    public DateTime LoginTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 过期时间
    /// </summary>
    [SugarColumn(ColumnName = "expires_at", ColumnDescription = "过期时间", IsNullable = false)]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    [SugarColumn(ColumnName = "last_activity_time", ColumnDescription = "最后活动时间", IsNullable = false)]
    public DateTime LastActivityTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 登录IP
    /// </summary>
    [SugarColumn(ColumnName = "login_ip", ColumnDescription = "登录IP", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? LoginIp { get; set; }

    /// <summary>
    /// 客户端信息
    /// </summary>
    [SugarColumn(ColumnName = "client_info", ColumnDescription = "客户端信息", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? ClientInfo { get; set; }

        /// <summary>
        /// 客户端快照（JSON）
        /// </summary>
        /// <remarks>
        /// 记录登录时通过 SystemInfoHelper 获取到的尽可能完整的客户端信息快照（键值对/列表）
        /// 包含但不限于：IP/MAC/OS/架构/CPU/内存/磁盘/已安装软件/网络适配器等
        /// </remarks>
        [SugarColumn(ColumnName = "client_snapshot", ColumnDescription = "客户端快照(JSON)", ColumnDataType = "nvarchar", Length = int.MaxValue, IsNullable = true)]
        public string? ClientSnapshot { get; set; }

        /// <summary>
        /// 操作系统描述
        /// </summary>
        [SugarColumn(ColumnName = "os_description", ColumnDescription = "操作系统描述", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
        public string? OsDescription { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        [SugarColumn(ColumnName = "os_version", ColumnDescription = "操作系统版本", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
        public string? OsVersion { get; set; }

        /// <summary>
        /// 操作系统类型（Windows/Linux/macOS）
        /// </summary>
        [SugarColumn(ColumnName = "os_type", ColumnDescription = "操作系统类型", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
        public string? OsType { get; set; }

        /// <summary>
        /// 操作系统架构（X64/X86/Arm64/Arm）
        /// </summary>
        [SugarColumn(ColumnName = "os_arch", ColumnDescription = "操作系统架构", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
        public string? OsArchitecture { get; set; }

        /// <summary>
        /// 机器名称
        /// </summary>
        [SugarColumn(ColumnName = "machine_name", ColumnDescription = "机器名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
        public string? MachineName { get; set; }

        /// <summary>
        /// MAC地址
        /// </summary>
        [SugarColumn(ColumnName = "mac_address", ColumnDescription = "MAC地址", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
        public string? MacAddress { get; set; }

        /// <summary>
        /// .NET 运行时版本
        /// </summary>
        [SugarColumn(ColumnName = "framework_version", ColumnDescription = ".NET运行时版本", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
        public string? FrameworkVersion { get; set; }

        /// <summary>
        /// 进程架构
        /// </summary>
        [SugarColumn(ColumnName = "process_arch", ColumnDescription = "进程架构", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
        public string? ProcessArchitecture { get; set; }

    /// <summary>
    /// 是否活跃
    /// 0=是（活跃），1=否（已失效）
    /// </summary>
    [SugarColumn(ColumnName = "is_active", ColumnDescription = "是否活跃", ColumnDataType = "int", IsNullable = false)]
    public int IsActive { get; set; } = 0;

    /// <summary>
    /// 登出时间
    /// </summary>
    [SugarColumn(ColumnName = "logout_time", ColumnDescription = "登出时间", IsNullable = true)]
    public DateTime? LogoutTime { get; set; }

    /// <summary>
    /// 登出原因
    /// </summary>
    /// <remarks>
    /// 1=主动登出, 2=超时, 3=强制下线, 4=账号异常
    /// </remarks>
    [SugarColumn(ColumnName = "logout_reason", ColumnDescription = "登出原因", ColumnDataType = "int", IsNullable = true)]
    public int? LogoutReason { get; set; }
}

