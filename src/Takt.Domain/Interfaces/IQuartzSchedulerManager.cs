// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Domain.Interfaces
// 文件名称：IQuartzSchedulerManager.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：Quartz调度器管理接口
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Domain.Interfaces;

/// <summary>
/// Quartz调度器管理接口
/// 用于管理Quartz.NET任务调度器，包括任务的启动、停止、添加、删除等操作
/// </summary>
public interface IQuartzSchedulerManager
{
    /// <summary>
    /// 初始化调度器
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 启动调度器
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// 停止调度器
    /// </summary>
    Task ShutdownAsync();

    /// <summary>
    /// 从数据库加载所有任务并添加到调度器
    /// </summary>
    Task LoadJobsFromDatabaseAsync();

    /// <summary>
    /// 添加任务到调度器
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <param name="triggerName">触发器名称</param>
    /// <param name="triggerGroup">触发器组</param>
    /// <param name="cronExpression">Cron表达式</param>
    /// <param name="jobClassName">任务类名（完整类名）</param>
    /// <param name="jobParams">任务参数（JSON格式，可选）</param>
    /// <returns>是否成功</returns>
    Task<bool> AddJobAsync(string jobName, string jobGroup, string triggerName, string triggerGroup, string cronExpression, string jobClassName, string? jobParams = null);

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <returns>是否成功</returns>
    Task<bool> PauseJobAsync(string jobName, string jobGroup);

    /// <summary>
    /// 恢复任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <returns>是否成功</returns>
    Task<bool> ResumeJobAsync(string jobName, string jobGroup);

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteJobAsync(string jobName, string jobGroup);

    /// <summary>
    /// 更新任务的Cron表达式
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <param name="triggerName">触发器名称</param>
    /// <param name="triggerGroup">触发器组</param>
    /// <param name="cronExpression">新的Cron表达式</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateJobCronAsync(string jobName, string jobGroup, string triggerName, string triggerGroup, string cronExpression);

    /// <summary>
    /// 立即触发任务执行（不等待计划时间）
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <returns>是否成功</returns>
    Task<bool> TriggerJobAsync(string jobName, string jobGroup);

    /// <summary>
    /// 停止（中断）正在执行的任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <returns>是否成功</returns>
    Task<bool> InterruptJobAsync(string jobName, string jobGroup);
}

