// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Domain.Entities.Logging
// 文件名称：QuartzJobLog.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务日志实体
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logging;

/// <summary>
/// 任务日志实体
/// 记录Quartz.NET定时任务的执行日志
/// </summary>
[SugarTable("takt_logging_quartz_log", "任务日志表")]
[SugarIndex("IX_takt_logging_quartz_log_quartz_id", nameof(QuartzId), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_quartz_log_job_name", nameof(JobName), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_quartz_log_job_group", nameof(JobGroup), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_quartz_log_execute_result", nameof(ExecuteResult), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_quartz_log_start_time", nameof(StartTime), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logging_quartz_log_created_time", nameof(CreatedTime), OrderByType.Desc, false)]
public class QuartzJobLog : BaseEntity
{
    /// <summary>
    /// 关联的任务ID
    /// 关联到QuartzJob表的主键ID
    /// </summary>
    [SugarColumn(ColumnName = "quartz_id", ColumnDescription = "任务ID", IsNullable = false)]
    public long QuartzId { get; set; }

    /// <summary>
    /// 任务名称
    /// Quartz Job的唯一标识名称
    /// </summary>
    [SugarColumn(ColumnName = "job_name", ColumnDescription = "任务名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务组
    /// Quartz Job所属的组
    /// </summary>
    [SugarColumn(ColumnName = "job_group", ColumnDescription = "任务组", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string JobGroup { get; set; } = string.Empty;

    /// <summary>
    /// 触发器名称
    /// Quartz Trigger的唯一标识名称
    /// </summary>
    [SugarColumn(ColumnName = "trigger_name", ColumnDescription = "触发器名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string TriggerName { get; set; } = string.Empty;

    /// <summary>
    /// 触发器组
    /// Quartz Trigger所属的组
    /// </summary>
    [SugarColumn(ColumnName = "trigger_group", ColumnDescription = "触发器组", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string TriggerGroup { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间
    /// 任务开始执行的时间
    /// </summary>
    [SugarColumn(ColumnName = "start_time", ColumnDescription = "开始时间", IsNullable = false)]
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 结束时间
    /// 任务执行完成的时间
    /// </summary>
    [SugarColumn(ColumnName = "end_time", ColumnDescription = "结束时间", IsNullable = true)]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 执行耗时
    /// 任务执行的耗时（单位：毫秒）
    /// </summary>
    [SugarColumn(ColumnName = "elapsed_time", ColumnDescription = "执行耗时", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int ElapsedTime { get; set; } = 0;

    /// <summary>
    /// 执行结果
    /// Success=成功，Failed=失败
    /// </summary>
    [SugarColumn(ColumnName = "execute_result", ColumnDescription = "执行结果", ColumnDataType = "nvarchar", Length = 20, IsNullable = false, DefaultValue = "Success")]
    public string ExecuteResult { get; set; } = "Success";

    /// <summary>
    /// 错误信息
    /// 任务执行失败时的错误信息
    /// </summary>
    [SugarColumn(ColumnName = "error_message", ColumnDescription = "错误信息", ColumnDataType = "nvarchar", Length = -1, IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 执行参数
    /// 任务执行时传递的参数（JSON格式）
    /// </summary>
    [SugarColumn(ColumnName = "job_params", ColumnDescription = "执行参数", ColumnDataType = "nvarchar", Length = -1, IsNullable = true)]
    public string? JobParams { get; set; }
}

