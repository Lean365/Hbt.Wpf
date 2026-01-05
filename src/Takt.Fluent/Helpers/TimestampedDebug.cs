// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：TimestampedDebug.cs
// 创建时间：2025-12-10
// 创建人：Takt365(Cursor AI)
// 功能描述：带时间戳的 Debug 输出辅助类
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Diagnostics;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 带时间戳的 Debug 输出辅助类
/// 用于在 Debug 输出中添加时间戳，便于分析性能瓶颈
/// </summary>
public static class TimestampedDebug
{
    private static readonly object _lock = new object();
    private static DateTime _startTime = DateTime.Now;
    private static DateTime _lastTime = DateTime.Now;

    /// <summary>
    /// 重置起始时间
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _startTime = DateTime.Now;
            _lastTime = _startTime;
        }
    }

    /// <summary>
    /// 输出带时间戳的调试信息
    /// 格式：[HH:mm:ss.fff +相对时间ms] 消息
    /// </summary>
    public static void WriteLine(string message)
    {
        lock (_lock)
        {
            var now = DateTime.Now;
            var elapsedFromStart = (now - _startTime).TotalMilliseconds;
            var elapsedFromLast = (now - _lastTime).TotalMilliseconds;
            _lastTime = now;

            var timestamp = now.ToString("HH:mm:ss.fff");
            var relativeTime = elapsedFromStart.ToString("F0").PadLeft(6, ' ');
            var timeSinceLast = elapsedFromLast > 0 ? $"+{elapsedFromLast:F0}ms" : "  +0ms";

            Debug.WriteLine($"[{timestamp} | {relativeTime}ms {timeSinceLast}] {message}");
        }
    }

    /// <summary>
    /// 输出带时间戳的格式化调试信息
    /// </summary>
    public static void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args));
    }

    /// <summary>
    /// 输出带时间戳的错误信息
    /// </summary>
    public static void WriteLineError(string message)
    {
        WriteLine($"❌ {message}");
    }

    /// <summary>
    /// 输出带时间戳的成功信息
    /// </summary>
    public static void WriteLineSuccess(string message)
    {
        WriteLine($"✓ {message}");
    }

    /// <summary>
    /// 输出带时间戳的警告信息
    /// </summary>
    public static void WriteLineWarning(string message)
    {
        WriteLine($"⚠️ {message}");
    }
}

