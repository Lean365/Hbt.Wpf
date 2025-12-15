// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Services
// 文件名称：InitializationStatusManager.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：应用程序初始化状态管理器
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;

namespace Takt.Fluent.Services;

/// <summary>
/// 初始化状态枚举
/// </summary>
public enum InitializationStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// 初始化中
    /// </summary>
    Initializing,
    
    /// <summary>
    /// 初始化完成
    /// </summary>
    Completed
}

/// <summary>
/// 应用程序初始化状态管理器
/// 用于在登录页面显示初始化进度
/// </summary>
public static class InitializationStatusManager
{
    private static InitializationStatus _status = InitializationStatus.NotStarted;
    private static string _statusMessage = string.Empty;

    /// <summary>
    /// 初始化状态变化事件
    /// </summary>
    public static event EventHandler<InitializationStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// 当前初始化状态
    /// </summary>
    public static InitializationStatus Status
    {
        get => _status;
        private set
        {
            if (_status != value)
            {
                _status = value;
                OnStatusChanged();
            }
        }
    }

    /// <summary>
    /// 当前状态消息
    /// </summary>
    public static string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnStatusChanged();
            }
        }
    }

    /// <summary>
    /// 更新初始化状态
    /// </summary>
    /// <param name="status">新状态</param>
    /// <param name="message">状态消息</param>
    public static void UpdateStatus(InitializationStatus status, string message = "")
    {
        Status = status;
        StatusMessage = message;
    }

    /// <summary>
    /// 触发状态变化事件
    /// </summary>
    private static void OnStatusChanged()
    {
        StatusChanged?.Invoke(null, new InitializationStatusChangedEventArgs(Status, StatusMessage));
    }

    /// <summary>
    /// 重置状态（用于测试或重新初始化）
    /// </summary>
    public static void Reset()
    {
        _status = InitializationStatus.NotStarted;
        _statusMessage = string.Empty;
    }
}

/// <summary>
/// 初始化状态变化事件参数
/// </summary>
public class InitializationStatusChangedEventArgs : EventArgs
{
    public InitializationStatus Status { get; }
    public string StatusMessage { get; }

    public InitializationStatusChangedEventArgs(InitializationStatus status, string statusMessage)
    {
        Status = status;
        StatusMessage = statusMessage;
    }
}

