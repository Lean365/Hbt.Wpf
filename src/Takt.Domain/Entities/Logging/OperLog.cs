//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logging
// 文件名称：OperLog.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：操作日志实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logging;

/// <summary>
/// 操作日志实体
/// </summary>
/// <remarks>
/// 记录用户的业务操作行为
/// </remarks>
[SugarTable("takt_logging_oper_log", "操作日志表")]
[SugarIndex("IX_takt_logging_oper_log_username", nameof(OperLog.Username), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_oper_log_operation_type", nameof(OperLog.OperationType), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_oper_log_operation_module", nameof(OperLog.OperationModule), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_oper_log_operation_time", nameof(OperLog.OperationTime), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logging_oper_log_created_time", nameof(OperLog.CreatedTime), OrderByType.Desc, false)]
public class OperLog : BaseEntity
{
    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "username", ColumnDescription = "用户名", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型
    /// </summary>
    /// <remarks>
    /// Create, Update, Delete, Export, Import, Query
    /// </remarks>
    [SugarColumn(ColumnName = "operation_type", ColumnDescription = "操作类型", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// 操作模块
    /// </summary>
    [SugarColumn(ColumnName = "operation_module", ColumnDescription = "操作模块", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string OperationModule { get; set; } = string.Empty;

    /// <summary>
    /// 操作描述
    /// </summary>
    [SugarColumn(ColumnName = "operation_desc", ColumnDescription = "操作描述", ColumnDataType = "nvarchar", Length = -1, IsNullable = true)]
    public string? OperationDesc { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [SugarColumn(ColumnName = "operation_time", ColumnDescription = "操作时间", IsNullable = false)]
    public DateTime OperationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 请求路径
    /// </summary>
    [SugarColumn(ColumnName = "request_path", ColumnDescription = "请求路径", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? RequestPath { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    [SugarColumn(ColumnName = "request_method", ColumnDescription = "请求方法", ColumnDataType = "nvarchar", Length = 10, IsNullable = true)]
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    [SugarColumn(ColumnName = "request_params", ColumnDescription = "请求参数", ColumnDataType = "nvarchar", Length = -1, IsNullable = true)]
    public string? RequestParams { get; set; }

    /// <summary>
    /// 响应结果
    /// </summary>
    [SugarColumn(ColumnName = "response_result", ColumnDescription = "响应结果", ColumnDataType = "nvarchar", Length = -1, IsNullable = true)]
    public string? ResponseResult { get; set; }

    /// <summary>
    /// 执行耗时
    /// </summary>
    /// <remarks>
    /// 单位：毫秒
    /// </remarks>
    [SugarColumn(ColumnName = "elapsed_time", ColumnDescription = "执行耗时", ColumnDataType = "int", IsNullable = false)]
    public int ElapsedTime { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [SugarColumn(ColumnName = "ip_address", ColumnDescription = "IP地址", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    [SugarColumn(ColumnName = "user_agent", ColumnDescription = "用户代理", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [SugarColumn(ColumnName = "os", ColumnDescription = "操作系统", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? Os { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    [SugarColumn(ColumnName = "browser", ColumnDescription = "浏览器", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? Browser { get; set; }

    /// <summary>
    /// 操作结果
    /// </summary>
    /// <remarks>
    /// Success, Failed
    /// </remarks>
    [SugarColumn(ColumnName = "operation_result", ColumnDescription = "操作结果", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string OperationResult { get; set; } = "Success";
}
