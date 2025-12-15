//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logging
// 文件名称：LoginLog.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：登录日志实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================
using Takt.Common.Enums;
using SqlSugar;

namespace Takt.Domain.Entities.Logging;

/// <summary>
/// 登录日志实体
/// </summary>
/// <remarks>
/// 记录用户登录、登出行为
/// </remarks>
[SugarTable("takt_logging_login_log", "登录日志表")]
[SugarIndex("IX_takt_logging_login_log_username", nameof(LoginLog.Username), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_login_log_login_time", nameof(LoginLog.LoginTime), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logging_login_log_login_status", nameof(LoginLog.LoginStatus), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_login_log_created_time", nameof(LoginLog.CreatedTime), OrderByType.Desc, false)]
public class LoginLog : BaseEntity
{
    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "username", ColumnDescription = "用户名", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 登录时间
    /// </summary>
    [SugarColumn(ColumnName = "login_time", ColumnDescription = "登录时间", IsNullable = false)]
    public DateTime LoginTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 登出时间
    /// </summary>
    [SugarColumn(ColumnName = "logout_time", ColumnDescription = "登出时间", IsNullable = true)]
    public DateTime? LogoutTime { get; set; }

    /// <summary>
    /// 登录IP
    /// </summary>
    [SugarColumn(ColumnName = "login_ip", ColumnDescription = "登录IP", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? LoginIp { get; set; }

    /// <summary>
    /// MAC地址
    /// </summary>
    [SugarColumn(ColumnName = "mac_address", ColumnDescription = "MAC地址", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? MacAddress { get; set; }

    /// <summary>
    /// 机器名称
    /// </summary>
    [SugarColumn(ColumnName = "machine_name", ColumnDescription = "机器名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? MachineName { get; set; }

    /// <summary>
    /// 登录地点
    /// </summary>
    [SugarColumn(ColumnName = "login_location", ColumnDescription = "登录地点", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? LoginLocation { get; set; }

    /// <summary>
    /// 客户端
    /// </summary>
    [SugarColumn(ColumnName = "browser", ColumnDescription = "客户端", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? Client { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [SugarColumn(ColumnName = "os", ColumnDescription = "操作系统", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? Os { get; set; }

    /// <summary>
    /// 操作系统版本
    /// </summary>
    [SugarColumn(ColumnName = "os_version", ColumnDescription = "操作系统版本", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? OsVersion { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    [SugarColumn(ColumnName = "os_architecture", ColumnDescription = "系统架构", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? OsArchitecture { get; set; }

    /// <summary>
    /// CPU信息
    /// </summary>
    [SugarColumn(ColumnName = "cpu_info", ColumnDescription = "CPU信息", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? CpuInfo { get; set; }

    /// <summary>
    /// 物理内存(GB)
    /// </summary>
    [SugarColumn(ColumnName = "total_memory_gb", ColumnDescription = "物理内存GB", ColumnDataType = "decimal", Length = 10, DecimalDigits = 2, IsNullable = true)]
    public decimal? TotalMemoryGb { get; set; }

    /// <summary>
    /// .NET运行时
    /// </summary>
    [SugarColumn(ColumnName = "framework_version", ColumnDescription = "NET运行时", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? FrameworkVersion { get; set; }

    /// <summary>
    /// 是否管理员
    /// 0=是，1=否
    /// </summary>
    [SugarColumn(ColumnName = "is_admin", ColumnDescription = "是否管理员", ColumnDataType = "int", IsNullable = false)]
    public int IsAdmin { get; set; } = 1;

    /// <summary>
    /// 客户端类型
    /// </summary>
    /// <remarks>
    /// Desktop、Web、Mobile、API等
    /// </remarks>
    [SugarColumn(ColumnName = "client_type", ColumnDescription = "客户端类型", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? ClientType { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    [SugarColumn(ColumnName = "client_version", ColumnDescription = "客户端版本", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? ClientVersion { get; set; }

    /// <summary>
    /// 登录状态
    /// </summary>
    /// <remarks>
    /// 0=成功, 1=失败
    /// </remarks>
    [SugarColumn(ColumnName = "login_status", ColumnDescription = "登录状态", ColumnDataType = "int", IsNullable = false)]
    public LoginStatusEnum LoginStatus { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    [SugarColumn(ColumnName = "fail_reason", ColumnDescription = "失败原因", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? FailReason { get; set; }
}

