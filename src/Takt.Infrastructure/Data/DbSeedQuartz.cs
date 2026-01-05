// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedQuartz.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：Quartz任务种子数据初始化服务
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Quartz任务种子数据初始化服务
/// 初始化系统默认的定时任务
/// </summary>
public class DbSeedQuartz
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<QuartzJob> _quartzRepository;

    public DbSeedQuartz(
        InitLogManager initLog,
        IBaseRepository<QuartzJob> quartzRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _quartzRepository = quartzRepository ?? throw new ArgumentNullException(nameof(quartzRepository));
    }

    /// <summary>
    /// 创建或更新任务的辅助方法
    /// </summary>
    private QuartzJob CreateOrUpdateQuartz(string jobName, Action<QuartzJob> configureQuartz)
    {
        var existingQuartz = _quartzRepository.GetFirst(q => q.JobName == jobName && q.IsDeleted == 0);
        var isNew = existingQuartz == null;

        var quartz = existingQuartz ?? new QuartzJob();

        configureQuartz(quartz);

        // 确保 JobName 被设置
        quartz.JobName = jobName;

        if (isNew)
        {
            _quartzRepository.Create(quartz, "Takt365");
            _initLog.Information($"✅ 创建Quartz任务：{quartz.JobName}");
        }
        else
        {
            _quartzRepository.Update(quartz, "Takt365");
            _initLog.Information($"✅ 更新Quartz任务：{quartz.JobName}");
        }

        return quartz;
    }

    /// <summary>
    /// 初始化Quartz任务种子数据
    /// </summary>
    public void Run()
    {
        _initLog.Information("开始初始化Quartz任务种子数据（存在则跳过，不存在则创建）..");

        // 1. 序列号管理器任务（每晚0点执行）- 使用通用 Job 类
        CreateOrUpdateQuartz("SerialsManagerJob", q =>
        {
            q.JobName = "SerialsManagerJob";
            q.JobGroup = "DEFAULT";
            q.TriggerName = "SerialsManagerTrigger";
            q.TriggerGroup = "DEFAULT";
            q.CronExpression = "0 0 0 * * ?"; // 每晚0点执行
            q.JobClassName = "Takt.Infrastructure.Jobs.GenericServiceJob, Takt.Infrastructure";
            q.JobDescription = "序列号管理器任务，每晚0点执行，用于处理序列号相关的定时任务";
            q.Status = 0; // 启用
            q.JobParams = "{\"ServiceType\":\"Takt.Domain.Interfaces.ISerialsManager, Takt.Domain\",\"MethodName\":\"InitializeAsync\",\"Parameters\":{}}";
        });

        // 2. 日志清理任务（每星期天0点执行）- 使用通用 Job 类
        CreateOrUpdateQuartz("LogCleanupJob", q =>
        {
            q.JobName = "LogCleanupJob";
            q.JobGroup = "DEFAULT";
            q.TriggerName = "LogCleanupTrigger";
            q.TriggerGroup = "DEFAULT";
            q.CronExpression = "0 0 0 ? * SUN"; // 每星期天0点执行
            q.JobClassName = "Takt.Infrastructure.Jobs.GenericServiceJob, Takt.Infrastructure";
            q.JobDescription = "日志清理任务，每星期天0点执行，清理超过7天的日志";
            q.Status = 0; // 启用
            q.JobParams = "{\"ServiceType\":\"Takt.Application.Services.Logging.ILogCleanupService, Takt.Application\",\"MethodName\":\"CleanupOldLogsAsync\",\"Parameters\":{\"retentionDays\":7}}";
        });

        _initLog.Information("Quartz任务种子数据初始化完成");
    }
}

