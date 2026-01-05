// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Services
// 文件名称：QuartzSchedulerManager.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：Quartz调度器管理实现
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Interfaces;
using Takt.Domain.Repositories;
using EntityQuartzJob = Takt.Domain.Entities.Routine.QuartzJob;

namespace Takt.Infrastructure.Services;

/// <summary>
/// Quartz调度器管理实现
/// </summary>
public class QuartzSchedulerManager : IQuartzSchedulerManager
{
    private IScheduler? _scheduler;
    private readonly IBaseRepository<EntityQuartzJob> _quartzRepository;
    private readonly Takt.Common.Logging.ILogDatabaseWriter? _logDatabaseWriter;
    private readonly AppLogManager _appLog;
    private readonly IServiceProvider _serviceProvider;

    public QuartzSchedulerManager(
        IBaseRepository<EntityQuartzJob> quartzRepository,
        Takt.Common.Logging.ILogDatabaseWriter? logDatabaseWriter,
        AppLogManager appLog,
        IServiceProvider serviceProvider)
    {
        _quartzRepository = quartzRepository ?? throw new ArgumentNullException(nameof(quartzRepository));
        _logDatabaseWriter = logDatabaseWriter;
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// 初始化调度器
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _appLog.Information("[QuartzSchedulerManager] 开始初始化Quartz调度器");

            var props = new NameValueCollection
            {
                ["quartz.scheduler.instanceName"] = "TaktQuartzScheduler",
                ["quartz.scheduler.instanceId"] = "AUTO",
                ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz",
                ["quartz.threadPool.threadCount"] = "10"
            };

            var factory = new StdSchedulerFactory(props);
            _scheduler = await factory.GetScheduler();

            // 设置 JobFactory 以便支持依赖注入
            _scheduler.JobFactory = new DependencyInjectionJobFactory(_serviceProvider);

            // 添加任务执行监听器，自动记录执行日志（使用统一的日志保存逻辑）
            var jobListener = new QuartzJobExecutionListener(_quartzRepository, _logDatabaseWriter, _appLog);
            _scheduler.ListenerManager.AddJobListener(jobListener, Quartz.Impl.Matchers.GroupMatcher<JobKey>.AnyGroup());

            _appLog.Information("[QuartzSchedulerManager] Quartz调度器初始化完成，已注册任务执行监听器");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] Quartz调度器初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 启动调度器
    /// </summary>
    public async Task StartAsync()
    {
        if (_scheduler == null)
        {
            await InitializeAsync();
        }

        if (_scheduler != null && !_scheduler.IsStarted)
        {
            await _scheduler.Start();
            _appLog.Information("[QuartzSchedulerManager] Quartz调度器已启动");
        }
    }

    /// <summary>
    /// 停止调度器
    /// </summary>
    public async Task ShutdownAsync()
    {
        if (_scheduler != null && !_scheduler.IsShutdown)
        {
            await _scheduler.Shutdown();
            _appLog.Information("[QuartzSchedulerManager] Quartz调度器已停止");
        }
    }

    /// <summary>
    /// 从数据库加载所有任务并添加到调度器
    /// </summary>
    public async Task LoadJobsFromDatabaseAsync()
    {
        if (_scheduler == null)
        {
            await StartAsync();
        }

        try
        {
            _appLog.Information("[QuartzSchedulerManager] 开始从数据库加载任务");

            var jobs = await _quartzRepository.GetListAsync(
                q => q.IsDeleted == 0 && q.Status == 0, // 只加载未删除且启用状态的任务
                1,
                int.MaxValue
            );

            foreach (var job in jobs.Items)
            {
                try
                {
                    await AddJobAsync(
                        job.JobName,
                        job.JobGroup,
                        job.TriggerName,
                        job.TriggerGroup,
                        job.CronExpression,
                        job.JobClassName,
                        job.JobParams
                    );
                    _appLog.Information("[QuartzSchedulerManager] 成功加载任务：{JobName} ({JobGroup})", job.JobName, job.JobGroup);
                }
                catch (Exception ex)
                {
                    _appLog.Error(ex, "[QuartzSchedulerManager] 加载任务失败：{JobName} ({JobGroup})", job.JobName, job.JobGroup);
                }
            }

            _appLog.Information("[QuartzSchedulerManager] 从数据库加载任务完成，共加载 {Count} 个任务", jobs.Items.Count);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 从数据库加载任务失败");
            throw;
        }
    }

    /// <summary>
    /// 添加任务到调度器
    /// </summary>
    public async Task<bool> AddJobAsync(string jobName, string jobGroup, string triggerName, string triggerGroup, string cronExpression, string jobClassName, string? jobParams = null)
    {
        if (_scheduler == null)
        {
            await StartAsync();
        }

        try
        {
            // 检查任务是否已存在
            var jobKey = new JobKey(jobName, jobGroup);
            if (await _scheduler!.CheckExists(jobKey))
            {
                _appLog.Warning("[QuartzSchedulerManager] 任务已存在：{JobName} ({JobGroup})，将先删除再添加", jobName, jobGroup);
                await DeleteJobAsync(jobName, jobGroup);
            }

            // 获取任务类型
            var jobType = Type.GetType(jobClassName);
            if (jobType == null)
            {
                _appLog.Error("[QuartzSchedulerManager] 无法找到任务类型：{JobClassName}", jobClassName);
                return false;
            }

            // 创建任务
            var job = JobBuilder.Create(jobType)
                .WithIdentity(jobKey)
                .Build();

            // 如果存在任务参数，添加到 JobDataMap
            if (!string.IsNullOrWhiteSpace(jobParams))
            {
                var jobDataMap = new JobDataMap();
                jobDataMap.Put("JobParams", jobParams);
                job = JobBuilder.Create(jobType)
                    .WithIdentity(jobKey)
                    .UsingJobData(jobDataMap)
                    .Build();
            }

            // 创建触发器
            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerName, triggerGroup)
                .WithCronSchedule(cronExpression)
                .Build();

            // 添加任务到调度器
            await _scheduler!.ScheduleJob(job, trigger);

            _appLog.Information("[QuartzSchedulerManager] 成功添加任务：{JobName} ({JobGroup})，Cron表达式：{CronExpression}", jobName, jobGroup, cronExpression);
            return true;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 添加任务失败：{JobName} ({JobGroup})", jobName, jobGroup);
            return false;
        }
    }

    /// <summary>
    /// 暂停任务
    /// </summary>
    public async Task<bool> PauseJobAsync(string jobName, string jobGroup)
    {
        if (_scheduler == null)
        {
            return false;
        }

        try
        {
            var jobKey = new JobKey(jobName, jobGroup);
            await _scheduler.PauseJob(jobKey);
            _appLog.Information("[QuartzSchedulerManager] 成功暂停任务：{JobName} ({JobGroup})", jobName, jobGroup);
            return true;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 暂停任务失败：{JobName} ({JobGroup})", jobName, jobGroup);
            return false;
        }
    }

    /// <summary>
    /// 恢复任务
    /// </summary>
    public async Task<bool> ResumeJobAsync(string jobName, string jobGroup)
    {
        if (_scheduler == null)
        {
            return false;
        }

        try
        {
            var jobKey = new JobKey(jobName, jobGroup);
            await _scheduler.ResumeJob(jobKey);
            _appLog.Information("[QuartzSchedulerManager] 成功恢复任务：{JobName} ({JobGroup})", jobName, jobGroup);
            return true;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 恢复任务失败：{JobName} ({JobGroup})", jobName, jobGroup);
            return false;
        }
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    public async Task<bool> DeleteJobAsync(string jobName, string jobGroup)
    {
        if (_scheduler == null)
        {
            return false;
        }

        try
        {
            var jobKey = new JobKey(jobName, jobGroup);
            var result = await _scheduler.DeleteJob(jobKey);
            if (result)
            {
                _appLog.Information("[QuartzSchedulerManager] 成功删除任务：{JobName} ({JobGroup})", jobName, jobGroup);
            }
            return result;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 删除任务失败：{JobName} ({JobGroup})", jobName, jobGroup);
            return false;
        }
    }

    /// <summary>
    /// 更新任务的Cron表达式
    /// </summary>
    public async Task<bool> UpdateJobCronAsync(string jobName, string jobGroup, string triggerName, string triggerGroup, string cronExpression)
    {
        if (_scheduler == null)
        {
            return false;
        }

        try
        {
            var triggerKey = new TriggerKey(triggerName, triggerGroup);
            var oldTrigger = await _scheduler.GetTrigger(triggerKey);
            if (oldTrigger == null)
            {
                _appLog.Warning("[QuartzSchedulerManager] 触发器不存在：{TriggerName} ({TriggerGroup})", triggerName, triggerGroup);
                return false;
            }

            // 创建新的触发器
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithCronSchedule(cronExpression)
                .Build();

            // 重新调度触发器
            await _scheduler.RescheduleJob(triggerKey, newTrigger);

            _appLog.Information("[QuartzSchedulerManager] 成功更新任务的Cron表达式：{JobName} ({JobGroup})，新表达式：{CronExpression}", jobName, jobGroup, cronExpression);
            return true;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 更新任务Cron表达式失败：{JobName} ({JobGroup})", jobName, jobGroup);
            return false;
        }
    }

    /// <summary>
    /// 立即触发任务执行（不等待计划时间）
    /// </summary>
    public async Task<bool> TriggerJobAsync(string jobName, string jobGroup)
    {
        if (_scheduler == null)
        {
            await StartAsync();
        }

        try
        {
            var jobKey = new JobKey(jobName, jobGroup);
            
            // 检查任务是否已存在于调度器中
            var exists = await _scheduler!.CheckExists(jobKey);
            if (!exists)
            {
                // 任务不存在，尝试从数据库加载
                _appLog.Warning("[QuartzSchedulerManager] 任务不存在于调度器中，尝试从数据库加载：{JobName} ({JobGroup})", jobName, jobGroup);
                
                var job = await _quartzRepository.GetFirstAsync(
                    q => q.JobName == jobName && q.JobGroup == jobGroup && q.IsDeleted == 0);
                
                if (job == null)
                {
                    _appLog.Error("[QuartzSchedulerManager] 数据库中不存在任务：{JobName} ({JobGroup})", jobName, jobGroup);
                    return false;
                }

                // 如果任务被禁用，不能触发
                if (job.Status != 0)
                {
                    _appLog.Warning("[QuartzSchedulerManager] 任务已被禁用，无法触发：{JobName} ({JobGroup})", jobName, jobGroup);
                    return false;
                }

                // 重新加载任务到调度器
                var added = await AddJobAsync(
                    job.JobName,
                    job.JobGroup,
                    job.TriggerName,
                    job.TriggerGroup,
                    job.CronExpression,
                    job.JobClassName,
                    job.JobParams);
                
                if (!added)
                {
                    _appLog.Error("[QuartzSchedulerManager] 从数据库加载任务到调度器失败：{JobName} ({JobGroup})", jobName, jobGroup);
                    return false;
                }
                
                _appLog.Information("[QuartzSchedulerManager] 从数据库重新加载任务成功：{JobName} ({JobGroup})", jobName, jobGroup);
            }

            // 触发任务
            await _scheduler.TriggerJob(jobKey);
            _appLog.Information("[QuartzSchedulerManager] 成功触发任务：{JobName} ({JobGroup})", jobName, jobGroup);
            return true;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 触发任务失败：{JobName} ({JobGroup})", jobName, jobGroup);
            return false;
        }
    }

    /// <summary>
    /// 停止（中断）正在执行的任务
    /// </summary>
    public async Task<bool> InterruptJobAsync(string jobName, string jobGroup)
    {
        if (_scheduler == null)
        {
            return false;
        }

        try
        {
            var jobKey = new JobKey(jobName, jobGroup);
            var interrupted = await _scheduler.Interrupt(jobKey);
            if (interrupted)
            {
                _appLog.Information("[QuartzSchedulerManager] 成功停止任务：{JobName} ({JobGroup})", jobName, jobGroup);
            }
            else
            {
                _appLog.Warning("[QuartzSchedulerManager] 任务未在运行，无法停止：{JobName} ({JobGroup})", jobName, jobGroup);
            }
            return interrupted;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzSchedulerManager] 停止任务失败：{JobName} ({JobGroup})", jobName, jobGroup);
            return false;
        }
    }
}

/// <summary>
/// 依赖注入 JobFactory
/// 用于支持在 Job 中使用依赖注入
/// </summary>
public class DependencyInjectionJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DependencyInjectionJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var jobDetail = bundle.JobDetail;
        var jobType = jobDetail.JobType;

        try
        {
            // 尝试从服务提供者获取实例
            var job = _serviceProvider.GetService(jobType) as IJob;
            if (job != null)
            {
                return job;
            }

            // 如果无法从服务提供者获取，尝试通过构造函数参数解析创建实例
            var constructors = jobType.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"类型 {jobType.FullName} 没有公共构造函数");
            }

            // 选择参数最多的构造函数（通常是最合适的）
            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();
            var constructorArgs = new object[parameters.Length];

            // 从服务提供者解析每个构造函数参数
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                var resolvedService = _serviceProvider.GetService(paramType);
                
                if (resolvedService == null)
                {
                    throw new InvalidOperationException($"无法解析构造函数参数 {paramType.FullName}，类型：{jobType.FullName}");
                }
                
                constructorArgs[i] = resolvedService;
            }

            // 使用解析的参数创建实例
            return (IJob)constructor.Invoke(constructorArgs);
        }
        catch (Exception ex)
        {
            throw new SchedulerException($"无法创建任务实例：{jobType.FullName}。错误：{ex.Message}", ex);
        }
    }

    public void ReturnJob(IJob job)
    {
        // 如果 Job 实现了 IDisposable，在这里可以处理
        if (job is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

