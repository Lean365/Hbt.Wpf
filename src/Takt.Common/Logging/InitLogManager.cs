// ========================================
// 项目名称：Takt.Wpf
// 文件名称：InitLogManager.cs
// 创建时间：2025-10-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：初始化日志管理器
// ========================================

using System;
using System.Text.RegularExpressions;
using Serilog;
using Serilog.Events;

namespace Takt.Common.Logging;

/// <summary>
/// 初始化日志管理器
/// 专门用于记录系统启动、数据库初始化、配置加载等初始化过程的日志
/// </summary>
public class InitLogManager : ILogManager
{
    private readonly ILogger _logger;
    
    /// <summary>
    /// 日志输出事件（用于实时显示在UI中）
    /// </summary>
    public static event EventHandler<string>? LogOutput;

    public InitLogManager(ILogger logger)
    {
        // 使用符合 Windows 规范的日志目录（AppData\Local）
        var logsDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();

        // 创建独立的初始化日志器
        _logger = new LoggerConfiguration()
            .WriteTo.File(
                path: Path.Combine(logsDir, "init-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 8 * 1024 * 1024,  // 单个文件最大 8MB
                rollOnFileSizeLimit: true,  // 达到文件大小限制时自动滚动
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8)
            .CreateLogger();
    }
    
    /// <summary>
    /// 触发日志输出事件
    /// </summary>
    private void OnLogOutput(string message)
    {
        LogOutput?.Invoke(this, message);
    }

    /// <summary>
    /// 记录初始化信息
    /// </summary>
    public void Information(string message, params object[] args)
    {
        _logger.Information("[初始化] " + message, args);
        
        // 对于 UI 输出，格式化消息
        // Serilog 使用结构化日志格式（如 {Count}），我们需要手动替换
        string formattedMessage = FormatMessage(message, args);
        OnLogOutput(formattedMessage);
    }
    
    /// <summary>
    /// 格式化消息（支持 Serilog 结构化日志格式和标准格式）
    /// </summary>
    private string FormatMessage(string message, object[] args)
    {
        if (args == null || args.Length == 0)
        {
            return message;
        }
        
        // 简单替换：将所有 {PropertyName} 替换为对应的参数值
        string result = message;
        var matches = Regex.Matches(result, @"\{[^}]+\}");
        for (int i = 0; i < matches.Count && i < args.Length; i++)
        {
            result = result.Replace(matches[i].Value, args[i]?.ToString() ?? "");
        }
        return result;
    }

    /// <summary>
    /// 记录初始化警告
    /// </summary>
    public void Warning(string message, params object[] args)
    {
        _logger.Warning("[初始化] " + message, args);
        string formattedMessage = FormatMessage(message, args);
        OnLogOutput($"⚠️ {formattedMessage}");
    }

    /// <summary>
    /// 记录初始化错误
    /// </summary>
    public void Error(string message, params object[] args)
    {
        _logger.Error("[初始化] " + message, args);
        string formattedMessage = FormatMessage(message, args);
        OnLogOutput($"❌ {formattedMessage}");
    }

    /// <summary>
    /// 记录初始化错误（带异常）
    /// </summary>
    public void Error(Exception exception, string message, params object[] args)
    {
        _logger.Error(exception, "[初始化] " + message, args);
        string formattedMessage = FormatMessage(message, args);
        OnLogOutput($"❌ {formattedMessage}: {exception.Message}");
    }

    /// <summary>
    /// 记录初始化调试信息
    /// </summary>
    public void Debug(string message, params object[] args)
    {
        _logger.Debug("[初始化] " + message, args);
        string formattedMessage = FormatMessage(message, args);
        OnLogOutput(formattedMessage);
    }
}

