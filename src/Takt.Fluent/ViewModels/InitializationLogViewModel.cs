// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels
// 文件名称：InitializationLogViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：初始化日志窗口的 ViewModel
// 
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险.
// ========================================

using System;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Takt.Fluent.ViewModels;

/// <summary>
/// 初始化日志窗口的 ViewModel
/// </summary>
public partial class InitializationLogViewModel : ObservableObject
{
    private readonly StringBuilder _logBuilder = new();
    
    [ObservableProperty]
    private string _logContent = string.Empty;
    
    /// <summary>
    /// 添加日志条目
    /// </summary>
    public void AppendLog(string message)
    {
        _logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        
        // 限制日志长度（保留最后 5000 行）
        if (_logBuilder.Length > 500000)
        {
            var content = _logBuilder.ToString();
            var lines = content.Split('\n');
            if (lines.Length > 5000)
            {
                _logBuilder.Clear();
                _logBuilder.AppendLine(string.Join("\n", lines, lines.Length - 5000, 5000));
            }
        }
        
        LogContent = _logBuilder.ToString();
    }
}
