// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：InitLogManager.cs
// 创建时间：2025-10-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：初始化日志管理器
// ========================================

using Serilog;
using Serilog.Events;

namespace Hbt.Common.Logging;

/// <summary>
/// 初始化日志管理器
/// 专门用于记录系统启动、数据库初始化、配置加载等初始化过程的日志
/// </summary>
public class InitLogManager : ILogManager
{
    private readonly ILogger _logger;

    public InitLogManager(ILogger logger)
    {
        // 确保 logs 目录存在
        var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }

        // 创建独立的初始化日志器
        _logger = new LoggerConfiguration()
            .WriteTo.File(
                path: Path.Combine(logsDir, "init-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8)
            .CreateLogger();
    }

    /// <summary>
    /// 记录初始化信息
    /// </summary>
    public void Information(string message, params object[] args)
    {
        _logger.Information("[初始化] " + message, args);
    }

    /// <summary>
    /// 记录初始化警告
    /// </summary>
    public void Warning(string message, params object[] args)
    {
        _logger.Warning("[初始化] " + message, args);
    }

    /// <summary>
    /// 记录初始化错误
    /// </summary>
    public void Error(string message, params object[] args)
    {
        _logger.Error("[初始化] " + message, args);
    }

    /// <summary>
    /// 记录初始化错误（带异常）
    /// </summary>
    public void Error(Exception exception, string message, params object[] args)
    {
        _logger.Error(exception, "[初始化] " + message, args);
    }

    /// <summary>
    /// 记录初始化调试信息
    /// </summary>
    public void Debug(string message, params object[] args)
    {
        _logger.Debug("[初始化] " + message, args);
    }
}

