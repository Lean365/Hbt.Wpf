//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ExceptionLog.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-20
// 版本号 : 1.0
// 描述    : 异常日志实体
//===================================================================

using SqlSugar;

namespace Hbt.Domain.Entities.Logging;

/// <summary>
/// 异常日志实体
/// </summary>
/// <remarks>
/// 记录系统运行时的异常信息
/// </remarks>
[SugarTable("hbt_logging_exception_log", "异常日志表")]
[SugarIndex("IX_hbt_logging_exception_log_exception_type", nameof(ExceptionLog.ExceptionType), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logging_exception_log_level", nameof(ExceptionLog.Level), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logging_exception_log_exception_time", nameof(ExceptionLog.ExceptionTime), OrderByType.Desc, false)]
[SugarIndex("IX_hbt_logging_exception_log_created_time", nameof(ExceptionLog.CreatedTime), OrderByType.Desc, false)]
public class ExceptionLog : BaseEntity
{
    /// <summary>
    /// 异常类型
    /// </summary>
    [SugarColumn(ColumnName = "exception_type", ColumnDescription = "异常类型", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// 异常消息
    /// </summary>
    [SugarColumn(ColumnName = "exception_message", ColumnDescription = "异常消息", ColumnDataType = "nvarchar", Length = 2000, IsNullable = false)]
    public string ExceptionMessage { get; set; } = string.Empty;

    /// <summary>
    /// 堆栈跟踪
    /// </summary>
    [SugarColumn(ColumnName = "stack_trace", ColumnDescription = "堆栈跟踪", ColumnDataType = "nvarchar", IsNullable = true)]
    public string? StackTrace { get; set; }

    /// <summary>
    /// 内部异常
    /// </summary>
    [SugarColumn(ColumnName = "inner_exception", ColumnDescription = "内部异常", ColumnDataType = "nvarchar", Length = 2000, IsNullable = true)]
    public string? InnerException { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    /// <remarks>
    /// Error, Fatal, Critical
    /// </remarks>
    [SugarColumn(ColumnName = "level", ColumnDescription = "日志级别", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string Level { get; set; } = "Error";

    /// <summary>
    /// 异常时间
    /// </summary>
    [SugarColumn(ColumnName = "exception_time", ColumnDescription = "异常时间", IsNullable = false)]
    public DateTime ExceptionTime { get; set; } = DateTime.Now;

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
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "username", ColumnDescription = "用户名", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? Username { get; set; }

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
}
