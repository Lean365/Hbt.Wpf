// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：LogCleanupService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志清理服务实现
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 日志清理服务实现
/// 负责清理过期的文本日志文件和数据表日志记录
/// </summary>
public class LogCleanupService : ILogCleanupService
{
    private readonly IBaseRepository<OperLog> _operationLogRepository;
    private readonly IBaseRepository<LoginLog> _loginLogRepository;
    private readonly IBaseRepository<DiffLog> _diffLogRepository;
    private readonly IBaseRepository<QuartzJobLog> _quartzJobLogRepository;
    private readonly AppLogManager _appLog;

    public LogCleanupService(
        IBaseRepository<OperLog> operationLogRepository,
        IBaseRepository<LoginLog> loginLogRepository,
        IBaseRepository<DiffLog> diffLogRepository,
        IBaseRepository<QuartzJobLog> quartzJobLogRepository,
        AppLogManager appLog)
    {
        _operationLogRepository = operationLogRepository ?? throw new ArgumentNullException(nameof(operationLogRepository));
        _loginLogRepository = loginLogRepository ?? throw new ArgumentNullException(nameof(loginLogRepository));
        _diffLogRepository = diffLogRepository ?? throw new ArgumentNullException(nameof(diffLogRepository));
        _quartzJobLogRepository = quartzJobLogRepository ?? throw new ArgumentNullException(nameof(quartzJobLogRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 清理过期日志
    /// 清理超过指定天数的日志（文本日志和数据表日志）
    /// </summary>
    /// <param name="retentionDays">保留天数，默认7天</param>
    /// <returns>清理结果，包含清理的文件数量和数据表记录数量</returns>
    public async Task<Result<LogCleanupResult>> CleanupOldLogsAsync(int retentionDays = 7)
    {
        var result = new LogCleanupResult();
        var cutoffDate = DateTime.Now.AddDays(-retentionDays);

        try
        {
            _appLog.Information("开始清理过期日志，保留天数={RetentionDays}，截止日期={CutoffDate}", retentionDays, cutoffDate);

            // 同时并行清理文本日志文件和数据表日志记录
            var fileCleanupTask = Task.Run(() => CleanupTextLogFiles(cutoffDate));
            var databaseCleanupTask = CleanupDatabaseLogsAsync(cutoffDate);

            // 等待两个任务都完成
            await Task.WhenAll(fileCleanupTask, databaseCleanupTask);

            // 获取清理结果
            var fileCleanupResult = fileCleanupTask.Result;
            result.CleanedFileCount = fileCleanupResult.FileCount;
            result.CleanedFileSize = fileCleanupResult.TotalSize;
            result.CleanedDatabaseLogCount = databaseCleanupTask.Result;

            _appLog.Information("文本日志清理完成，清理文件数={FileCount}，清理大小={FileSize} 字节",
                result.CleanedFileCount, result.CleanedFileSize);
            _appLog.Information("数据表日志清理完成，清理记录数={RecordCount}", result.CleanedDatabaseLogCount);

            _appLog.Information("日志清理完成，总计：文件数={FileCount}，记录数={RecordCount}，文件大小={FileSize} 字节",
                result.CleanedFileCount, result.CleanedDatabaseLogCount, result.CleanedFileSize);

            return Result<LogCleanupResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "清理过期日志失败，保留天数={RetentionDays}", retentionDays);
            return Result<LogCleanupResult>.Fail($"清理过期日志失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 清理文本日志文件
    /// </summary>
    private (int FileCount, long TotalSize) CleanupTextLogFiles(DateTime cutoffDate)
    {
        var fileCount = 0;
        var totalSize = 0L;

        try
        {
            // 使用符合 Windows 规范的日志目录（AppData\Local）
            var logsDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();
            if (!Directory.Exists(logsDir))
            {
                _appLog.Information("日志目录不存在，跳过文本日志清理: {LogsDir}", logsDir);
                return (0, 0);
            }

            // 清理所有日志文件（app-*.txt, oper-*.txt, init-*.txt 等）
            var logFilePatterns = new[] { "app-*.txt", "oper-*.txt", "init-*.txt" };

            foreach (var pattern in logFilePatterns)
            {
                var files = Directory.GetFiles(logsDir, pattern);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        // 检查文件最后修改时间
                        if (fileInfo.LastWriteTime < cutoffDate)
                        {
                            var fileSize = fileInfo.Length;
                            File.Delete(file);
                            fileCount++;
                            totalSize += fileSize;
                            _appLog.Debug("删除过期日志文件: {FileName}, 大小={FileSize} 字节, 修改时间={LastWriteTime}",
                                fileInfo.Name, fileSize, fileInfo.LastWriteTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        _appLog.Warning("删除日志文件失败: {FileName}, 错误: {Error}", file, ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "清理文本日志文件失败");
        }

        return (fileCount, totalSize);
    }

    /// <summary>
    /// 清理数据表日志记录
    /// </summary>
    private async Task<int> CleanupDatabaseLogsAsync(DateTime cutoffDate)
    {
        var totalCount = 0;

        try
        {
            // 清理操作日志表
            var operationLogCount = await _operationLogRepository.AsQueryable()
                .Where(x => x.OperationTime < cutoffDate && x.IsDeleted == 0)
                .CountAsync();

            if (operationLogCount > 0)
            {
                var deletedCount = await _operationLogRepository.DeleteAsync(x => x.OperationTime < cutoffDate && x.IsDeleted == 0);
                totalCount += deletedCount;
                _appLog.Information("清理操作日志记录: {Count} 条", deletedCount);
            }

            // 清理登录日志表
            var loginLogCount = await _loginLogRepository.AsQueryable()
                .Where(x => x.LoginTime < cutoffDate && x.IsDeleted == 0)
                .CountAsync();

            if (loginLogCount > 0)
            {
                var deletedCount = await _loginLogRepository.DeleteAsync(x => x.LoginTime < cutoffDate && x.IsDeleted == 0);
                totalCount += deletedCount;
                _appLog.Information("清理登录日志记录: {Count} 条", deletedCount);
            }

            // 清理差异日志表
            var diffLogCount = await _diffLogRepository.AsQueryable()
                .Where(x => x.CreatedTime < cutoffDate && x.IsDeleted == 0)
                .CountAsync();

            if (diffLogCount > 0)
            {
                var deletedCount = await _diffLogRepository.DeleteAsync(x => x.CreatedTime < cutoffDate && x.IsDeleted == 0);
                totalCount += deletedCount;
                _appLog.Information("清理差异日志记录: {Count} 条", deletedCount);
            }
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "清理数据表日志记录失败");
        }

        return totalCount;
    }

    /// <summary>
    /// 清理指定类型和时间范围的日志
    /// </summary>
    /// <param name="logType">日志类型，All表示清理所有类型的日志</param>
    /// <param name="range">清理时间范围</param>
    /// <returns>清理结果，包含清理的文件数量和数据表记录数量</returns>
    public async Task<Result<LogCleanupResult>> CleanupLogsAsync(LogType logType, LogCleanupRange range)
    {
        var result = new LogCleanupResult();
        DateTime? startDate = null;
        DateTime? endDate = null;

        try
        {
            // 根据时间范围计算日期范围
            if (range == LogCleanupRange.All)
            {
                startDate = DateTime.MinValue; // 清理所有日志
                endDate = DateTime.MaxValue;
            }
            else if (range == LogCleanupRange.Today)
            {
                // 清理当天的日志（今天0点到明天0点）
                startDate = DateTime.Today;
                endDate = DateTime.Today.AddDays(1);
            }
            else
            {
                var days = (int)range;
                // 清理指定天数之前的所有日志（不包括今天）
                startDate = DateTime.MinValue;
                endDate = DateTime.Today.AddDays(-days);
            }

            _appLog.Information("开始清理日志，日志类型={LogType}，时间范围={Range}，开始日期={StartDate}，结束日期={EndDate}", 
                logType, range, startDate, endDate);

            // 如果指定了日志类型，只清理该类型的日志；否则清理所有类型
            var logTypes = logType == LogType.All
                ? new[] { LogType.OperLog, LogType.LoginLog, LogType.DiffLog, LogType.QuartzJobLog }
                : new[] { logType };

            // 清理数据库日志记录
            var databaseCleanupTasks = logTypes.Select(lt => CleanupDatabaseLogsByTypeAsync(lt, startDate.Value, endDate.Value));
            var databaseResults = await Task.WhenAll(databaseCleanupTasks);
            result.CleanedDatabaseLogCount = databaseResults.Sum();

            // 清理文本日志文件（只有清理所有类型时才清理文件）
            if (logType == LogType.All && range != LogCleanupRange.Today)
            {
                // 对于非当天的情况，使用结束日期作为截止日期
                var cutoffDate = endDate.Value == DateTime.MaxValue ? DateTime.MinValue : endDate.Value;
                var fileCleanupResult = await Task.Run(() => CleanupTextLogFiles(cutoffDate));
                result.CleanedFileCount = fileCleanupResult.FileCount;
                result.CleanedFileSize = fileCleanupResult.TotalSize;
            }

            _appLog.Information("日志清理完成，日志类型={LogType}，清理记录数={RecordCount}，文件数={FileCount}，文件大小={FileSize} 字节",
                logType, result.CleanedDatabaseLogCount, result.CleanedFileCount, result.CleanedFileSize);

            return Result<LogCleanupResult>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "清理日志失败，日志类型={LogType}，时间范围={Range}", logType, range);
            return Result<LogCleanupResult>.Fail($"清理日志失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 按类型清理数据库日志记录
    /// </summary>
    private async Task<int> CleanupDatabaseLogsByTypeAsync(LogType logType, DateTime startDate, DateTime endDate)
    {
        try
        {
            return logType switch
            {
                LogType.OperLog => await CleanupOperLogsAsync(startDate, endDate),
                LogType.LoginLog => await CleanupLoginLogsAsync(startDate, endDate),
                LogType.DiffLog => await CleanupDiffLogsAsync(startDate, endDate),
                LogType.QuartzJobLog => await CleanupQuartzJobLogsAsync(startDate, endDate),
                _ => 0
            };
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "清理{LogType}日志失败", logType);
            return 0;
        }
    }

    /// <summary>
    /// 清理操作日志
    /// </summary>
    private async Task<int> CleanupOperLogsAsync(DateTime startDate, DateTime endDate)
    {
        System.Linq.Expressions.Expression<Func<OperLog, bool>> predicate;
        
        if (endDate == DateTime.MaxValue)
        {
            // 清理所有（从开始日期到最大日期）
            predicate = x => x.OperationTime >= startDate && x.IsDeleted == 0;
        }
        else
        {
            // 清理指定日期范围内的日志
            predicate = x => x.OperationTime >= startDate && x.OperationTime < endDate && x.IsDeleted == 0;
        }

        var count = await _operationLogRepository.AsQueryable()
            .Where(predicate)
            .CountAsync();

        if (count > 0)
        {
            var deletedCount = await _operationLogRepository.DeleteAsync(predicate);
            _appLog.Information("清理操作日志记录: {Count} 条", deletedCount);
            return deletedCount;
        }

        return 0;
    }

    /// <summary>
    /// 清理登录日志
    /// </summary>
    private async Task<int> CleanupLoginLogsAsync(DateTime startDate, DateTime endDate)
    {
        System.Linq.Expressions.Expression<Func<LoginLog, bool>> predicate;
        
        if (endDate == DateTime.MaxValue)
        {
            // 清理所有（从开始日期到最大日期）
            predicate = x => x.LoginTime >= startDate && x.IsDeleted == 0;
        }
        else
        {
            // 清理指定日期范围内的日志
            predicate = x => x.LoginTime >= startDate && x.LoginTime < endDate && x.IsDeleted == 0;
        }

        var count = await _loginLogRepository.AsQueryable()
            .Where(predicate)
            .CountAsync();

        if (count > 0)
        {
            var deletedCount = await _loginLogRepository.DeleteAsync(predicate);
            _appLog.Information("清理登录日志记录: {Count} 条", deletedCount);
            return deletedCount;
        }

        return 0;
    }

    /// <summary>
    /// 清理差异日志
    /// </summary>
    private async Task<int> CleanupDiffLogsAsync(DateTime startDate, DateTime endDate)
    {
        System.Linq.Expressions.Expression<Func<DiffLog, bool>> predicate;
        
        if (endDate == DateTime.MaxValue)
        {
            // 清理所有（从开始日期到最大日期）
            predicate = x => x.CreatedTime >= startDate && x.IsDeleted == 0;
        }
        else
        {
            // 清理指定日期范围内的日志
            predicate = x => x.CreatedTime >= startDate && x.CreatedTime < endDate && x.IsDeleted == 0;
        }

        var count = await _diffLogRepository.AsQueryable()
            .Where(predicate)
            .CountAsync();

        if (count > 0)
        {
            var deletedCount = await _diffLogRepository.DeleteAsync(predicate);
            _appLog.Information("清理差异日志记录: {Count} 条", deletedCount);
            return deletedCount;
        }

        return 0;
    }

    /// <summary>
    /// 清理任务日志
    /// </summary>
    private async Task<int> CleanupQuartzJobLogsAsync(DateTime startDate, DateTime endDate)
    {
        System.Linq.Expressions.Expression<Func<QuartzJobLog, bool>> predicate;
        
        if (endDate == DateTime.MaxValue)
        {
            // 清理所有（从开始日期到最大日期）
            predicate = x => x.StartTime >= startDate && x.IsDeleted == 0;
        }
        else
        {
            // 清理指定日期范围内的日志
            predicate = x => x.StartTime >= startDate && x.StartTime < endDate && x.IsDeleted == 0;
        }

        var count = await _quartzJobLogRepository.AsQueryable()
            .Where(predicate)
            .CountAsync();

        if (count > 0)
        {
            var deletedCount = await _quartzJobLogRepository.DeleteAsync(predicate);
            _appLog.Information("清理任务日志记录: {Count} 条", deletedCount);
            return deletedCount;
        }

        return 0;
    }
}

