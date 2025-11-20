// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：LogCleanupService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志清理服务实现
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Repositories;
using SqlSugar;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 日志清理服务实现
/// 负责清理过期的文本日志文件和数据表日志记录
/// </summary>
public class LogCleanupService : ILogCleanupService
{
    private readonly IBaseRepository<OperationLog> _operationLogRepository;
    private readonly IBaseRepository<LoginLog> _loginLogRepository;
    private readonly IBaseRepository<DiffLog> _diffLogRepository;
    private readonly IBaseRepository<ExceptionLog> _exceptionLogRepository;
    private readonly AppLogManager _appLog;

    public LogCleanupService(
        IBaseRepository<OperationLog> operationLogRepository,
        IBaseRepository<LoginLog> loginLogRepository,
        IBaseRepository<DiffLog> diffLogRepository,
        IBaseRepository<ExceptionLog> exceptionLogRepository,
        AppLogManager appLog)
    {
        _operationLogRepository = operationLogRepository ?? throw new ArgumentNullException(nameof(operationLogRepository));
        _loginLogRepository = loginLogRepository ?? throw new ArgumentNullException(nameof(loginLogRepository));
        _diffLogRepository = diffLogRepository ?? throw new ArgumentNullException(nameof(diffLogRepository));
        _exceptionLogRepository = exceptionLogRepository ?? throw new ArgumentNullException(nameof(exceptionLogRepository));
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

            // 清理异常日志表
            var exceptionLogCount = await _exceptionLogRepository.AsQueryable()
                .Where(x => x.ExceptionTime < cutoffDate && x.IsDeleted == 0)
                .CountAsync();
            
            if (exceptionLogCount > 0)
            {
                var deletedCount = await _exceptionLogRepository.DeleteAsync(x => x.ExceptionTime < cutoffDate && x.IsDeleted == 0);
                totalCount += deletedCount;
                _appLog.Information("清理异常日志记录: {Count} 条", deletedCount);
            }
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "清理数据表日志记录失败");
        }

        return totalCount;
    }
}

