// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Services
// 文件名称：LogCleanupBackgroundService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志清理后台服务（每月1号0点执行，只保留最近7天的日志）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Microsoft.Extensions.Hosting;
using Takt.Application.Services.Logging;
using Takt.Common.Logging;

namespace Takt.Infrastructure.Services;

/// <summary>
/// 日志清理后台服务
/// 每月1号0点执行，只保留最近7天的所有日志（文本日志和数据表日志）
/// </summary>
public class LogCleanupBackgroundService : BackgroundService
{
    private readonly ILogCleanupService _logCleanupService;
    private readonly AppLogManager _appLog;

    public LogCleanupBackgroundService(
        ILogCleanupService logCleanupService,
        AppLogManager appLog)
    {
        _logCleanupService = logCleanupService ?? throw new ArgumentNullException(nameof(logCleanupService));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _appLog.Information("[LogCleanupBackgroundService] 日志清理后台服务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                
                // 计算下次执行时间：每月1号0点
                var nextRunTime = GetNextRunTime(now);
                var delay = nextRunTime - now;

                _appLog.Information("[LogCleanupBackgroundService] 下次执行时间: {NextRunTime}, 延迟: {Delay} 小时", 
                    nextRunTime, delay.TotalHours);

                // 等待到下次执行时间
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                // 执行日志清理
                _appLog.Information("[LogCleanupBackgroundService] 开始执行日志清理任务");
                var result = await _logCleanupService.CleanupOldLogsAsync(retentionDays: 7);
                
                if (result.Success && result.Data != null)
                {
                    _appLog.Information("[LogCleanupBackgroundService] 日志清理完成，清理文件数={FileCount}，清理记录数={RecordCount}，清理大小={FileSize} 字节",
                        result.Data.CleanedFileCount, result.Data.CleanedDatabaseLogCount, result.Data.CleanedFileSize);
                }
                else
                {
                    _appLog.Warning("[LogCleanupBackgroundService] 日志清理失败: {Message}", result.Message);
                }
            }
            catch (OperationCanceledException)
            {
                _appLog.Information("[LogCleanupBackgroundService] 日志清理后台服务已取消");
                break;
            }
            catch (Exception ex)
            {
                _appLog.Error(ex, "[LogCleanupBackgroundService] 日志清理后台服务执行异常");
                
                // 发生异常后，等待1小时再重试
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _appLog.Information("[LogCleanupBackgroundService] 日志清理后台服务已停止");
    }

    /// <summary>
    /// 计算下次执行时间（每月1号0点）
    /// </summary>
    private DateTime GetNextRunTime(DateTime now)
    {
        // 如果今天是1号且当前时间已过0点，则下个月1号0点执行
        // 如果今天是1号但还没到0点，则今天0点执行
        // 如果今天不是1号，则本月1号0点执行（如果已过）或下个月1号0点执行

        var thisMonthFirstDay = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
        
        if (now.Day == 1 && now.Hour >= 0)
        {
            // 今天是1号且已过0点，下个月1号0点执行
            return thisMonthFirstDay.AddMonths(1);
        }
        else if (now < thisMonthFirstDay)
        {
            // 还没到本月1号0点，本月1号0点执行
            return thisMonthFirstDay;
        }
        else
        {
            // 已过本月1号0点，下个月1号0点执行
            return thisMonthFirstDay.AddMonths(1);
        }
    }
}

