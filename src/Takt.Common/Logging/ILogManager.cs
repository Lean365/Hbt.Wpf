// ========================================
// 项目名称：Takt.Wpf
// 文件名称：ILogManager.cs
// 创建时间：2025-10-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：日志管理器接口
// ========================================

namespace Takt.Common.Logging;

/// <summary>
/// 日志管理器接口
/// 提供统一的日志管理功能
/// </summary>
public interface ILogManager
{
    /// <summary>
    /// 记录信息日志
    /// </summary>
    void Information(string message, params object[] args);

    /// <summary>
    /// 记录警告日志
    /// </summary>
    void Warning(string message, params object[] args);

    /// <summary>
    /// 记录错误日志
    /// </summary>
    void Error(string message, params object[] args);

    /// <summary>
    /// 记录错误日志（带异常）
    /// </summary>
    void Error(Exception exception, string message, params object[] args);

    /// <summary>
    /// 记录调试日志
    /// </summary>
    void Debug(string message, params object[] args);
}
