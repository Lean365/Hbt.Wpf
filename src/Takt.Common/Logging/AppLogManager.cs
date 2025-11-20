// ========================================
// 项目名称：Takt.Wpf
// 文件名称：AppLogManager.cs
// 创建时间：2025-10-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：应用程序日志管理器
// ========================================

using Serilog;
using Serilog.Events;

namespace Takt.Common.Logging;

/// <summary>
/// 应用程序日志管理器
/// 专门用于记录程序运行过程中的一般日志
/// </summary>
public class AppLogManager : ILogManager
{
    private readonly ILogger _appLogger;

    public AppLogManager(ILogger logger)
    {
        // 使用符合 Windows 规范的日志目录（AppData\Local）
        var logsDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();
        
        // 调试信息：输出路径信息
        Console.WriteLine($"[AppLogManager] LogsDir: {logsDir}");

        // 创建独立的应用程序日志器
        _appLogger = new LoggerConfiguration()
            .WriteTo.File(
                path: Path.Combine(logsDir, "app-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8)
            .CreateLogger();
            
        Console.WriteLine($"[AppLogManager] 应用程序日志器创建完成");
    }

    /// <summary>
    /// 记录应用程序信息
    /// </summary>
    public void Information(string message, params object[] args)
    {
        _appLogger.Information("[程序] " + message, args);
    }

    /// <summary>
    /// 记录应用程序警告
    /// </summary>
    public void Warning(string message, params object[] args)
    {
        _appLogger.Warning("[程序] " + message, args);
    }

    /// <summary>
    /// 记录应用程序错误
    /// </summary>
    public void Error(string message, params object[] args)
    {
        _appLogger.Error("[程序] " + message, args);
    }

    /// <summary>
    /// 记录应用程序错误（带异常）
    /// </summary>
    public void Error(Exception exception, string message, params object[] args)
    {
        _appLogger.Error(exception, "[程序] " + message, args);
    }

    /// <summary>
    /// 记录应用程序调试信息
    /// </summary>
    public void Debug(string message, params object[] args)
    {
        _appLogger.Debug("[程序] " + message, args);
    }
}

