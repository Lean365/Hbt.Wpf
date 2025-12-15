// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktMessageManager.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：统一的消息管理器
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using Takt.Fluent.Views;
using Takt.Fluent.ViewModels;

namespace Takt.Fluent.Controls;

/// <summary>
/// 消息显示类型
/// </summary>
public enum MessageDisplayType
{
    /// <summary>
    /// Toast 通知（自动消失）
    /// </summary>
    Toast,
    
    /// <summary>
    /// 消息框（需要点击确定/取消）
    /// </summary>
    MessageBox
}

/// <summary>
/// 统一的消息管理器
/// 提供三种消息显示方式：
/// A. 弹出自动消失提示框（10秒），顶端对齐
/// B. 需要人工确认的消息框，始终在当前视口的中心居中位置
/// C. 直接显示在状态栏的消息文本（颜色，自动消失（10秒））
/// </summary>
public static class TaktMessageManager
{
    /// <summary>
    /// 【类型A】显示弹出自动消失提示框（10秒），顶端对齐
    /// 适用于：需要突出显示的重要提示信息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题，可选</param>
    /// <param name="icon">图标类型</param>
    /// <param name="duration">显示时长（毫秒），默认 10000（10秒）</param>
    /// <param name="owner">所有者窗口，可选</param>
    public static void ShowToastWindow(string message, string? title = null!, MessageBoxImage icon = MessageBoxImage.Information, int duration = 10000, Window? owner = null)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var viewModel = new TaktToastViewModel
            {
                Message = message,
                Title = title ?? GetDefaultTitle(icon),
                IconKind = GetIconKind(icon),
                IconBrush = GetIconBrush(icon),
                BorderBrush = GetBorderBrush(icon),
                Duration = duration
            };
            
            var toastWindow = new TaktToastWindow(owner ?? System.Windows.Application.Current.MainWindow)
            {
                DataContext = viewModel
            };
            toastWindow.Show();
        });
    }

    /// <summary>
    /// 【类型C】显示状态栏消息（自动消失，10秒）
    /// 适用于：常规操作结果提示（成功/失败）
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="icon">图标类型</param>
    /// <param name="duration">显示时长（毫秒），默认 10000（10秒）</param>
    public static void ShowStatusBarMessage(string message, MessageBoxImage icon = MessageBoxImage.Information, int duration = 10000)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            // 尝试获取 MainWindow 的 ViewModel
            var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
            if (mainWindow != null && mainWindow.ViewModel != null)
            {
                // 在状态栏显示消息
                var iconKind = GetIconKind(icon);
                var iconBrush = GetIconBrush(icon);
                mainWindow.ViewModel.ShowStatusBarMessage(message, iconKind, iconBrush, duration);
            }
        });
    }

    /// <summary>
    /// 【已废弃】显示 Toast 通知（保留用于向后兼容）
    /// </summary>
    [Obsolete("请使用 ShowStatusBarMessage 或 ShowToastWindow")]
    public static void ShowToast(string message, string? title = null!, MessageBoxImage icon = MessageBoxImage.Information, int duration = 5000, Window? owner = null)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            // 尝试获取 MainWindow 的 ViewModel
            var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
            if (mainWindow != null && mainWindow.ViewModel != null)
            {
                // 在状态栏显示消息
                var iconKind = GetIconKind(icon);
                var iconBrush = GetIconBrush(icon);
                mainWindow.ViewModel.ShowStatusBarMessage(message, iconKind, iconBrush, duration);
            }
            else
            {
                // 如果 MainWindow 不可用，回退到独立窗口（用于登录窗口等场景）
                var viewModel = new TaktToastViewModel
                {
                    Message = message,
                    Title = title ?? GetDefaultTitle(icon),
                    IconKind = GetIconKind(icon),
                    IconBrush = GetIconBrush(icon),
                    BorderBrush = GetBorderBrush(icon),
                    Duration = duration
                };
                
                var toastWindow = new TaktToastWindow(owner ?? System.Windows.Application.Current.MainWindow)
                {
                    DataContext = viewModel
                };
                toastWindow.Show();
            }
        });
    }

    /// <summary>
    /// 【类型B】显示消息框（需要点击确定/取消），始终在当前视口的中心居中位置
    /// 适用于：删除操作前的确认、重要操作前的确认等
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题，可选</param>
    /// <param name="icon">图标类型</param>
    /// <param name="button">按钮类型</param>
    /// <param name="owner">所有者窗口，可选</param>
    /// <returns>用户选择的结果</returns>
    public static MessageBoxResult ShowMessageBox(string message, string? title = null!, MessageBoxImage icon = MessageBoxImage.Information, MessageBoxButton button = MessageBoxButton.OK, Window? owner = null)
    {
        return TaktMessageBox.Show(message, title, icon, button, owner);
    }

    /// <summary>
    /// 根据 Result 对象显示消息（状态栏，自动消失，10秒）
    /// 适用于：登录、CURD操作等常规操作的结果提示
    /// </summary>
    /// <param name="result">操作结果</param>
    public static void ShowResult(Result result)
    {
        if (result.Success)
        {
            ShowSuccess(result.Message);
        }
        else
        {
            ShowError(result.Message);
        }
    }

    /// <summary>
    /// 根据 Result 对象显示消息（状态栏，自动消失，10秒）
    /// 适用于：登录、CURD操作等常规操作的结果提示
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="result">操作结果</param>
    /// <returns>如果成功返回 true，否则返回 false</returns>
    public static bool ShowResult<T>(Result<T> result)
    {
        if (result.Success)
        {
            ShowSuccess(result.Message);
            return true;
        }
        else
        {
            ShowError(result.Message);
            return false;
        }
    }

    /// <summary>
    /// 【类型C】显示成功消息（状态栏，自动消失，10秒）
    /// 适用于：登录成功、CURD操作成功等常规操作
    /// </summary>
    /// <param name="message">消息内容</param>
    public static void ShowSuccess(string message)
    {
        ShowStatusBarMessage(message, MessageBoxImage.Information, 10000);
    }

    /// <summary>
    /// 【类型C】显示错误消息（状态栏，自动消失，10秒）
    /// 适用于：登录失败、CURD操作失败等常规操作
    /// </summary>
    /// <param name="message">消息内容</param>
    public static void ShowError(string message)
    {
        ShowStatusBarMessage(message, MessageBoxImage.Error, 10000);
    }

    /// <summary>
    /// 【类型C】显示警告消息（状态栏，自动消失，10秒）
    /// </summary>
    /// <param name="message">消息内容</param>
    public static void ShowWarning(string message)
    {
        ShowStatusBarMessage(message, MessageBoxImage.Warning, 10000);
    }

    /// <summary>
    /// 【类型C】显示信息消息（状态栏，自动消失，10秒）
    /// </summary>
    /// <param name="message">消息内容</param>
    public static void ShowInformation(string message)
    {
        ShowStatusBarMessage(message, MessageBoxImage.Information, 10000);
    }

    /// <summary>
    /// 【类型B】显示确认消息（手动确认框，需要用户点击确定或取消），始终在当前视口的中心居中位置
    /// 适用于：删除操作前的确认、重要操作前的确认等
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="title">标题，可选，默认为"确认"</param>
    /// <param name="owner">所有者窗口，可选</param>
    /// <returns>用户选择的结果（Yes/No）</returns>
    public static MessageBoxResult ShowQuestion(string message, string? title = null!, Window? owner = null)
    {
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        var defaultTitle = localizationManager?.GetString("common.messagebox.question") ?? "确认";
        
        return ShowMessageBox(message, title ?? defaultTitle, MessageBoxImage.Question, MessageBoxButton.YesNo, owner);
    }

    /// <summary>
    /// 【类型B】显示确认删除消息（手动确认框），始终在当前视口的中心居中位置
    /// 适用于：删除操作前的确认
    /// </summary>
    /// <param name="message">确认消息内容，如果为空则使用默认消息</param>
    /// <param name="owner">所有者窗口，可选</param>
    /// <returns>如果用户选择 Yes 返回 true，否则返回 false</returns>
    public static bool ShowDeleteConfirm(string? message = null!, Window? owner = null)
    {
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        
        var defaultMessage = localizationManager?.GetString("common.messagebox.confirmdeletemessage") ?? "确定要删除这条记录吗？";
        var defaultTitle = localizationManager?.GetString("common.messagebox.confirmdelete") ?? "确认删除";
        
        var confirmMessage = string.IsNullOrWhiteSpace(message) 
            ? defaultMessage 
            : message;
        var result = ShowQuestion(confirmMessage, defaultTitle, owner);
        return result == MessageBoxResult.Yes;
    }

    /// <summary>
    /// 获取默认标题
    /// </summary>
    private static string GetDefaultTitle(MessageBoxImage icon)
    {
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        var key = icon switch
        {
            MessageBoxImage.Information => "common.messagebox.information",
            MessageBoxImage.Warning => "common.messagebox.warning",
            MessageBoxImage.Error => "common.messagebox.error",
            MessageBoxImage.Question => "common.messagebox.question",
            _ => "common.messagebox.information"
        };

        return localizationManager?.GetString(key) ?? icon switch
        {
            MessageBoxImage.Information => "信息",
            MessageBoxImage.Warning => "警告",
            MessageBoxImage.Error => "错误",
            MessageBoxImage.Question => "确认",
            _ => "信息"
        };
    }

    /// <summary>
    /// 获取图标类型
    /// </summary>
    private static PackIconKind GetIconKind(MessageBoxImage icon)
    {
        return icon switch
        {
            MessageBoxImage.Information => PackIconKind.Information,
            MessageBoxImage.Warning => PackIconKind.Alert,
            MessageBoxImage.Error => PackIconKind.AlertCircle,
            MessageBoxImage.Question => PackIconKind.HelpCircle,
            _ => PackIconKind.Information
        };
    }

    /// <summary>
    /// 获取图标颜色
    /// </summary>
    private static Brush GetIconBrush(MessageBoxImage icon)
    {
        return icon switch
        {
            MessageBoxImage.Information => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
            MessageBoxImage.Warning => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
            MessageBoxImage.Error => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
            MessageBoxImage.Question => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
            _ => new SolidColorBrush(Color.FromRgb(33, 150, 243))
        };
    }

    /// <summary>
    /// 获取边框颜色（用于区分成功/失败）
    /// </summary>
    private static Brush GetBorderBrush(MessageBoxImage icon)
    {
        return icon switch
        {
            MessageBoxImage.Information => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green - 成功
            MessageBoxImage.Warning => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange - 警告
            MessageBoxImage.Error => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red - 错误
            MessageBoxImage.Question => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue - 询问
            _ => new SolidColorBrush(Color.FromRgb(200, 200, 200)) // Gray - 默认
        };
    }
}

