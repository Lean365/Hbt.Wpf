// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：ILogCleanupService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志清理服务接口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Results;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 日志类型枚举
/// </summary>
public enum LogType
{
    /// <summary>
    /// 操作日志
    /// </summary>
    OperLog = 1,

    /// <summary>
    /// 登录日志
    /// </summary>
    LoginLog = 2,

    /// <summary>
    /// 差异日志
    /// </summary>
    DiffLog = 3,

    /// <summary>
    /// 任务日志
    /// </summary>
    QuartzJobLog = 4,

    /// <summary>
    /// 所有日志类型
    /// </summary>
    All = 0
}

/// <summary>
/// 日志清理时间范围枚举
/// </summary>
public enum LogCleanupRange
{
    /// <summary>
    /// 当天（清理当天的日志）
    /// </summary>
    Today = 0,

    /// <summary>
    /// 7天（清理7天之前的所有日志）
    /// </summary>
    SevenDays = 7,

    /// <summary>
    /// 30天（清理30天之前的所有日志）
    /// </summary>
    ThirtyDays = 30,

    /// <summary>
    /// 全部（清理所有日志）
    /// </summary>
    All = -1
}

/// <summary>
/// 日志清理服务接口
/// 用于定期清理过期的日志文件和数据表日志
/// </summary>
public interface ILogCleanupService
{
    /// <summary>
    /// 清理过期日志
    /// 清理超过指定天数的日志（文本日志和数据表日志）
    /// </summary>
    /// <param name="retentionDays">保留天数，默认7天</param>
    /// <returns>清理结果，包含清理的文件数量和数据表记录数量</returns>
    Task<Result<LogCleanupResult>> CleanupOldLogsAsync(int retentionDays = 7);

    /// <summary>
    /// 清理指定类型和时间范围的日志
    /// </summary>
    /// <param name="logType">日志类型，All表示清理所有类型的日志</param>
    /// <param name="range">清理时间范围</param>
    /// <returns>清理结果，包含清理的文件数量和数据表记录数量</returns>
    Task<Result<LogCleanupResult>> CleanupLogsAsync(LogType logType, LogCleanupRange range);
}

/// <summary>
/// 日志清理结果
/// </summary>
public class LogCleanupResult
{
    /// <summary>
    /// 清理的文本日志文件数量
    /// </summary>
    public int CleanedFileCount { get; set; }

    /// <summary>
    /// 清理的数据表日志记录数量
    /// </summary>
    public int CleanedDatabaseLogCount { get; set; }

    /// <summary>
    /// 清理的文本日志文件总大小（字节）
    /// </summary>
    public long CleanedFileSize { get; set; }
}

