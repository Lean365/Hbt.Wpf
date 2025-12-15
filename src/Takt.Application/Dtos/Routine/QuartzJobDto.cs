// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Routine
// 文件名称：QuartzJobDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 任务数据传输对象
/// </summary>
public class QuartzJobDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public QuartzJobDto()
    {
        JobName = string.Empty;
        JobGroup = "DEFAULT";
        TriggerName = string.Empty;
        TriggerGroup = "DEFAULT";
        CronExpression = string.Empty;
        JobClassName = string.Empty;
        JobDescription = string.Empty;
        JobParams = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        LastRunTime = DateTime.Now;
        NextRunTime = DateTime.Now;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

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
    /// Cron表达式
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    /// 任务类名
    /// </summary>
    public string JobClassName { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string JobDescription { get; set; }

    /// <summary>
    /// 任务状态（0=启用，1=禁用，2=运行中，3=暂停）
    /// </summary>
    public int Status { get; set; } = 0;

    /// <summary>
    /// 任务参数（JSON格式）
    /// </summary>
    public string JobParams { get; set; }

    /// <summary>
    /// 最后执行时间
    /// </summary>
    public DateTime LastRunTime { get; set; }

    /// <summary>
    /// 下次执行时间
    /// </summary>
    public DateTime NextRunTime { get; set; }

    /// <summary>
    /// 执行次数
    /// </summary>
    public int RunCount { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdatedBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }

    /// <summary>
    /// 是否删除（0=否，1=是）
    /// </summary>
    public int IsDeleted { get; set; }

    /// <summary>
    /// 删除人
    /// </summary>
    public string DeletedBy { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime DeletedTime { get; set; }
}

/// <summary>
/// 任务查询数据传输对象
/// </summary>
public class QuartzJobQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public QuartzJobQueryDto()
    {
        Keywords = string.Empty;
        JobName = string.Empty;
        JobGroup = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在任务名称、任务描述、任务类名中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// 任务组
    /// </summary>
    public string JobGroup { get; set; }

    /// <summary>
    /// 任务状态（0=启用，1=禁用，2=运行中，3=暂停）
    /// </summary>
    public int? Status { get; set; }
}

/// <summary>
/// 创建任务数据传输对象
/// </summary>
public class QuartzJobCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public QuartzJobCreateDto()
    {
        JobName = string.Empty;
        JobGroup = "DEFAULT";
        TriggerName = string.Empty;
        TriggerGroup = "DEFAULT";
        CronExpression = string.Empty;
        JobClassName = string.Empty;
        JobDescription = string.Empty;
        JobParams = string.Empty;
        Remarks = string.Empty;
    }

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
    /// Cron表达式
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    /// 任务类名
    /// </summary>
    public string JobClassName { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string JobDescription { get; set; }

    /// <summary>
    /// 任务参数（JSON格式）
    /// </summary>
    public string JobParams { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新任务数据传输对象
/// </summary>
public class QuartzJobUpdateDto : QuartzJobCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 任务状态（0=启用，1=禁用，2=运行中，3=暂停）
    /// </summary>
    public int Status { get; set; } = 0;
}

/// <summary>
/// 任务导出数据传输对象
/// </summary>
public class QuartzJobExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public QuartzJobExportDto()
    {
        JobName = string.Empty;
        JobGroup = "DEFAULT";
        TriggerName = string.Empty;
        TriggerGroup = "DEFAULT";
        CronExpression = string.Empty;
        JobClassName = string.Empty;
        JobDescription = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        LastRunTime = DateTime.Now;
        NextRunTime = DateTime.Now;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

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
    /// Cron表达式
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    /// 任务类名
    /// </summary>
    public string JobClassName { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string JobDescription { get; set; }

    /// <summary>
    /// 任务状态（0=启用，1=禁用，2=运行中，3=暂停）
    /// </summary>
    public int Status { get; set; } = 0;

    /// <summary>
    /// 最后执行时间
    /// </summary>
    public DateTime LastRunTime { get; set; }

    /// <summary>
    /// 下次执行时间
    /// </summary>
    public DateTime NextRunTime { get; set; }

    /// <summary>
    /// 执行次数
    /// </summary>
    public int RunCount { get; set; } = 0;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdatedBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }
}

/// <summary>
/// 任务状态更新 DTO（启用/禁用）
/// </summary>
public class QuartzJobStatusDto
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 新状态（0=启用，1=禁用）
    /// </summary>
    public int Status { get; set; }
}

