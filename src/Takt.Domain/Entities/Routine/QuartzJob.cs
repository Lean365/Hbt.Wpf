// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Domain.Entities.Routine
// 文件名称：QuartzJob.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务管理实体
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Routine;

/// <summary>
/// 任务管理实体
/// 用于管理Quartz.NET定时任务的配置信息
/// </summary>
[SugarTable("takt_routine_quartz", "任务管理表")]
[SugarIndex("IX_takt_routine_quartz_job_name", nameof(JobName), OrderByType.Asc, false)]
[SugarIndex("IX_takt_routine_quartz_job_group", nameof(JobGroup), OrderByType.Asc, false)]
[SugarIndex("IX_takt_routine_quartz_status", nameof(Status), OrderByType.Asc, false)]
[SugarIndex("IX_takt_routine_quartz_created_time", nameof(CreatedTime), OrderByType.Desc, false)]
public class QuartzJob : BaseEntity
{
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
    [SugarColumn(ColumnName = "job_group", ColumnDescription = "任务组", ColumnDataType = "nvarchar", Length = 50, IsNullable = false, DefaultValue = "DEFAULT")]
    public string JobGroup { get; set; } = "DEFAULT";

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
    [SugarColumn(ColumnName = "trigger_group", ColumnDescription = "触发器组", ColumnDataType = "nvarchar", Length = 50, IsNullable = false, DefaultValue = "DEFAULT")]
    public string TriggerGroup { get; set; } = "DEFAULT";

    /// <summary>
    /// Cron表达式
    /// 定时任务的执行时间表达式（如：0 0 12 * * ? 表示每天12点执行）
    /// </summary>
    [SugarColumn(ColumnName = "cron_expression", ColumnDescription = "Cron表达式", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// 任务类名
    /// 实现IJob接口的完整类名（包含命名空间）
    /// </summary>
    [SugarColumn(ColumnName = "job_class_name", ColumnDescription = "任务类名", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string JobClassName { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// 任务的描述信息
    /// </summary>
    [SugarColumn(ColumnName = "job_description", ColumnDescription = "任务描述", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? JobDescription { get; set; }

    /// <summary>
    /// 任务状态
    /// 0=启用（任务已启用，将根据日程自动执行）
    /// 1=禁用（任务已禁用，不会执行）
    /// </summary>
    [SugarColumn(ColumnName = "status", ColumnDescription = "任务状态", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int Status { get; set; } = 0;

    /// <summary>
    /// 任务参数
    /// 任务执行时传递的参数（JSON格式）
    /// </summary>
    [SugarColumn(ColumnName = "job_params", ColumnDescription = "任务参数", ColumnDataType = "nvarchar", Length = -1, IsNullable = true)]
    public string? JobParams { get; set; }

    /// <summary>
    /// 最后执行时间
    /// 任务最后一次执行的时间
    /// </summary>
    [SugarColumn(ColumnName = "last_run_time", ColumnDescription = "最后执行时间", IsNullable = true)]
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    /// 下次执行时间
    /// 任务下次计划执行的时间
    /// </summary>
    [SugarColumn(ColumnName = "next_run_time", ColumnDescription = "下次执行时间", IsNullable = true)]
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    /// 执行次数
    /// 任务已执行的次数
    /// </summary>
    [SugarColumn(ColumnName = "run_count", ColumnDescription = "执行次数", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int RunCount { get; set; } = 0;
}

