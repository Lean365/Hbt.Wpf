// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logging
// 文件名称：QuartzJobLogDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务日志数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logging;

/// <summary>
/// 任务日志数据传输对象
/// </summary>
public class QuartzJobLogDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public QuartzJobLogDto()
    {
        JobName = string.Empty;
        JobGroup = string.Empty;
        TriggerName = string.Empty;
        TriggerGroup = string.Empty;
        ExecuteResult = "Success";
        ErrorMessage = string.Empty;
        JobParams = string.Empty;
        StartTime = DateTime.Now;
        EndTime = DateTime.Now;
        CreatedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 关联的任务ID
    /// </summary>
    public long QuartzId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// 任务组
    /// </summary>
    public string JobGroup { get; set; }

    /// <summary>
    /// 触发器名称
    /// </summary>
    public string TriggerName { get; set; }

    /// <summary>
    /// 触发器组
    /// </summary>
    public string TriggerGroup { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public int ElapsedTime { get; set; }

    /// <summary>
    /// 执行结果（Success/Failed）
    /// </summary>
    public string ExecuteResult { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 执行参数（JSON格式）
    /// </summary>
    public string JobParams { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 任务日志查询数据传输对象
/// </summary>
public class QuartzJobLogQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public QuartzJobLogQueryDto()
    {
        var now = DateTime.Now;
        Keywords = string.Empty;
        JobName = string.Empty;
        JobGroup = string.Empty;
        ExecuteResult = string.Empty;
        StartTimeFrom = new DateTime(now.Year, now.Month, 1);
        StartTimeTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    /// <summary>
    /// 搜索关键词（支持在任务名称、错误信息中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 关联的任务ID
    /// </summary>
    public long? QuartzId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// 任务组
    /// </summary>
    public string JobGroup { get; set; }

    /// <summary>
    /// 执行结果（Success/Failed）
    /// </summary>
    public string ExecuteResult { get; set; }

    /// <summary>
    /// 开始时间（起始，默认为本月第一天）
    /// </summary>
    public DateTime StartTimeFrom { get; set; }

    /// <summary>
    /// 开始时间（结束，默认为本月最后一天）
    /// </summary>
    public DateTime StartTimeTo { get; set; }
}

/// <summary>
/// 任务日志导出数据传输对象
/// </summary>
public class QuartzJobLogExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public QuartzJobLogExportDto()
    {
        JobName = string.Empty;
        JobGroup = string.Empty;
        TriggerName = string.Empty;
        TriggerGroup = string.Empty;
        ExecuteResult = "Success";
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 关联的任务ID
    /// </summary>
    public long QuartzId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// 任务组
    /// </summary>
    public string JobGroup { get; set; }

    /// <summary>
    /// 触发器名称
    /// </summary>
    public string TriggerName { get; set; }

    /// <summary>
    /// 触发器组
    /// </summary>
    public string TriggerGroup { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public int ElapsedTime { get; set; }

    /// <summary>
    /// 执行结果（Success/Failed）
    /// </summary>
    public string ExecuteResult { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

