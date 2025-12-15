// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Services
// 文件名称：QuartzJobExecutionListener.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：Quartz任务执行监听器，自动记录任务执行日志到数据库
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Quartz;
using Takt.Common.Logging;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;
using EntityQuartzJob = Takt.Domain.Entities.Routine.QuartzJob;

namespace Takt.Infrastructure.Services;

/// <summary>
/// Quartz任务执行监听器
/// 自动记录任务执行日志到数据库表
/// </summary>
public class QuartzJobExecutionListener : IJobListener
{
    private readonly IBaseRepository<EntityQuartzJob> _quartzRepository;
    private readonly Takt.Common.Logging.ILogDatabaseWriter? _logDatabaseWriter;
    private readonly AppLogManager _appLog;
    
    // 用于存储任务执行开始时间
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> _jobStartTimes = new();
    
    // 用于存储任务执行前的状态，以便执行完成后恢复
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> _jobPreviousStatus = new();

    public QuartzJobExecutionListener(
        IBaseRepository<EntityQuartzJob> quartzRepository,
        Takt.Common.Logging.ILogDatabaseWriter? logDatabaseWriter,
        AppLogManager appLog)
    {
        _quartzRepository = quartzRepository ?? throw new ArgumentNullException(nameof(quartzRepository));
        _logDatabaseWriter = logDatabaseWriter;
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    public string Name => "QuartzJobExecutionListener";

    /// <summary>
    /// 任务执行前触发
    /// </summary>
    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;
            var startTime = context.FireTimeUtc.LocalDateTime;
            
            // 存储开始时间，供JobWasExecuted使用
            var jobIdentifier = $"{jobKey.Name}_{jobKey.Group}_{triggerKey.Name}_{triggerKey.Group}";
            _jobStartTimes[jobIdentifier] = startTime;

            // 查询任务实体，保存当前状态并设置为运行中（2）
            var quartzJob = _quartzRepository.GetFirst(j => 
                j.JobName == jobKey.Name && 
                j.JobGroup == jobKey.Group && 
                j.IsDeleted == 0);
            
            if (quartzJob != null)
            {
                // 保存任务执行前的状态（正常情况下应该是启用0，但可能是其他状态如立即运行）
                _jobPreviousStatus[jobIdentifier] = quartzJob.Status;
                
                // 只有当状态不是运行中（2）时，才设置为运行中（2）
                // 注意：只有状态为启用（0）的任务才会被执行，但为了安全起见，这里仍然检查
                if (quartzJob.Status != 2)
                {
                    quartzJob.Status = 2; // 设置为运行中
                    _quartzRepository.Update(quartzJob, "System");
                    _appLog.Debug("[QuartzJobExecutionListener] 任务状态已设置为运行中（2）：{JobName} ({JobGroup})，执行前状态：{PreviousStatus}", 
                        jobKey.Name, jobKey.Group, _jobPreviousStatus[jobIdentifier]);
                }
            }

            _appLog.Debug("[QuartzJobExecutionListener] 任务开始执行：{JobName} ({JobGroup}), 触发器：{TriggerName} ({TriggerGroup}), 执行时间：{StartTime}",
                jobKey.Name, jobKey.Group, triggerKey.Name, triggerKey.Group, startTime);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzJobExecutionListener] 记录任务开始执行信息失败");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 任务执行后被否决时触发
    /// </summary>
    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;
            
            _appLog.Warning("[QuartzJobExecutionListener] 任务执行被否决：{JobName} ({JobGroup}), 触发器：{TriggerName} ({TriggerGroup})",
                jobKey.Name, jobKey.Group, triggerKey.Name, triggerKey.Group);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzJobExecutionListener] 处理任务执行被否决事件失败");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 任务执行完成后触发（无论成功还是失败）
    /// </summary>
    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;
            var jobIdentifier = $"{jobKey.Name}_{jobKey.Group}_{triggerKey.Name}_{triggerKey.Group}";

            // 获取开始时间
            if (!_jobStartTimes.TryRemove(jobIdentifier, out var startTime))
            {
                startTime = context.FireTimeUtc.LocalDateTime;
            }

            var endTime = DateTime.Now;
            var elapsedTime = (int)(endTime - startTime).TotalMilliseconds;

            // 确定执行结果
            var executeResult = jobException == null ? "Success" : "Failed";
            var errorMessage = jobException?.GetBaseException().Message;

            // 获取任务参数
            var jobParams = context.JobDetail.JobDataMap.GetString("JobParams");

            // 从数据库查询QuartzJob实体以获取QuartzId
            var quartzJob = _quartzRepository.GetFirst(j => 
                j.JobName == jobKey.Name && 
                j.JobGroup == jobKey.Group && 
                j.IsDeleted == 0);

            // 创建任务日志记录
            var jobLog = new QuartzJobLog
            {
                QuartzId = quartzJob?.Id ?? 0, // 如果找不到任务，设置为0
                JobName = jobKey.Name,
                JobGroup = jobKey.Group,
                TriggerName = triggerKey.Name,
                TriggerGroup = triggerKey.Group,
                StartTime = startTime,
                EndTime = endTime,
                ElapsedTime = elapsedTime,
                ExecuteResult = executeResult,
                ErrorMessage = errorMessage,
                JobParams = jobParams
            };

            // 保存到数据库（使用统一日志保存逻辑，通过 LogDatabaseWriter）
            if (_logDatabaseWriter != null)
            {
                try
                {
                    await _logDatabaseWriter.SaveQuartzJobLogAsync(
                        quartzId: jobLog.QuartzId,
                        jobName: jobLog.JobName,
                        jobGroup: jobLog.JobGroup,
                        triggerName: jobLog.TriggerName,
                        triggerGroup: jobLog.TriggerGroup,
                        startTime: jobLog.StartTime,
                        endTime: jobLog.EndTime,
                        elapsedTime: jobLog.ElapsedTime,
                        executeResult: jobLog.ExecuteResult,
                        errorMessage: jobLog.ErrorMessage,
                        jobParams: jobLog.JobParams);
                    _appLog.Debug("[QuartzJobExecutionListener] 任务日志已通过 LogDatabaseWriter 保存");
                }
                catch (Exception ex)
                {
                    _appLog.Error(ex, "[QuartzJobExecutionListener] 通过 LogDatabaseWriter 保存任务日志失败: 任务名称={JobName}, 任务组={JobGroup}", 
                        jobKey.Name, jobKey.Group);
                    // 不重新抛出异常，避免影响任务执行流程
                }
            }
            else
            {
                _appLog.Warning("[QuartzJobExecutionListener] ⚠️ LogDatabaseWriter 未注入，无法保存任务日志: 任务名称={JobName}, 任务组={JobGroup}", 
                    jobKey.Name, jobKey.Group);
            }

            // 更新QuartzJob表的执行信息
            if (quartzJob != null)
            {
                quartzJob.LastRunTime = endTime;
                quartzJob.RunCount += 1;
                
                // 计算下次执行时间
                var nextFireTime = context.NextFireTimeUtc?.LocalDateTime;
                if (nextFireTime.HasValue)
                {
                    quartzJob.NextRunTime = nextFireTime.Value;
                }

                // 恢复任务状态：执行完成后，如果当前状态是运行中（2），则恢复为启用（0）
                // 注意：任务执行过程中，如果被手动暂停或禁用，状态会被改变，这里不会覆盖
                if (quartzJob.Status == 2)
                {
                    // 任务执行前应该是启用（0）或运行中（2），执行完成后恢复为启用（0）
                    var previousStatus = _jobPreviousStatus.TryRemove(jobIdentifier, out var status) ? status : 0;
                    quartzJob.Status = 0; // 恢复为启用状态
                    _appLog.Debug("[QuartzJobExecutionListener] 任务执行完成，状态已恢复为启用（0），任务：{JobName} ({JobGroup})，执行前状态：{PreviousStatus}", 
                        jobKey.Name, jobKey.Group, previousStatus);
                }
                else
                {
                    // 如果状态不是运行中（2），说明任务在执行过程中被手动暂停或禁用，保持当前状态
                    _jobPreviousStatus.TryRemove(jobIdentifier, out _); // 清理记录
                    _appLog.Debug("[QuartzJobExecutionListener] 任务执行完成，保持当前状态：{Status}，任务：{JobName} ({JobGroup})", 
                        quartzJob.Status, jobKey.Name, jobKey.Group);
                }

                await Task.Run(() => _quartzRepository.Update(quartzJob, "System"));
            }

            if (jobException == null)
            {
                _appLog.Information("[QuartzJobExecutionListener] 任务执行成功：{JobName} ({JobGroup}), 耗时：{ElapsedTime} 毫秒",
                    jobKey.Name, jobKey.Group, elapsedTime);
            }
            else
            {
                _appLog.Error(jobException, "[QuartzJobExecutionListener] 任务执行失败：{JobName} ({JobGroup}), 耗时：{ElapsedTime} 毫秒, 错误：{ErrorMessage}",
                    jobKey.Name, jobKey.Group, elapsedTime, errorMessage ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[QuartzJobExecutionListener] 记录任务执行日志失败");
        }
    }
}

