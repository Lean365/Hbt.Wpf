// ========================================
// 项目名称：Takt.Wpf
// 文件名称：LogTypeEnum.cs
// 创建时间：2025-10-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：日志类型枚举
// ========================================

namespace Takt.Common.Enums;

/// <summary>
/// 日志类型枚举
/// </summary>
public enum LogTypeEnum
{
    /// <summary>
    /// 系统初始化日志
    /// </summary>
    Init = 1,

    /// <summary>
    /// 程序运行日志
    /// </summary>
    Application = 2,

    /// <summary>
    /// 用户操作日志
    /// </summary>
    Operation = 3,

    /// <summary>
    /// 数据库操作日志
    /// </summary>
    Database = 4,

    /// <summary>
    /// 安全审计日志
    /// </summary>
    Security = 5,

    /// <summary>
    /// 性能监控日志
    /// </summary>
    Performance = 6
}

/// <summary>
/// 日志级别枚举
/// </summary>
public enum LogLevelEnum
{
    /// <summary>
    /// 调试
    /// </summary>
    Debug = 1,

    /// <summary>
    /// 信息
    /// </summary>
    Information = 2,

    /// <summary>
    /// 警告
    /// </summary>
    Warning = 3,

    /// <summary>
    /// 错误
    /// </summary>
    Error = 4,

    /// <summary>
    /// 致命
    /// </summary>
    Fatal = 5
}

