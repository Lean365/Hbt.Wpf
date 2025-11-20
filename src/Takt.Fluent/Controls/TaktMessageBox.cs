// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktMessageBox.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：统一的消息框静态类
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.Controls;

/// <summary>
/// 统一的消息框组件
/// </summary>
public static class TaktMessageBox
{
    /// <summary>
    /// 显示信息消息框
    /// </summary>
    public static MessageBoxResult Show(string message, string? title = null, MessageBoxButton button = MessageBoxButton.OK, Window? owner = null)
    {
        return Show(message, title, MessageBoxImage.Information, button, owner);
    }

    /// <summary>
    /// 显示消息框
    /// </summary>
    public static MessageBoxResult Show(string message, string? title, MessageBoxImage icon, MessageBoxButton button = MessageBoxButton.OK, Window? owner = null)
    {
        // 获取本地化管理器
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        
        // 如果没有指定标题，使用默认标题
        if (string.IsNullOrWhiteSpace(title))
        {
            title = GetDefaultTitle(icon, localizationManager);
        }

        // 创建窗口和视图模型
        var window = new TaktMessageBoxWindow();
        var viewModel = new TaktMessageBoxViewModel(owner ?? System.Windows.Application.Current.MainWindow)
        {
            Title = title,
            Message = message,
            IconKind = GetIconKind(icon),
            IconBrush = GetIconBrush(icon)
        };

        // 设置按钮
        SetupButtons(viewModel, button, localizationManager);

        // 设置窗口属性
        window.DataContext = viewModel;
        window.Owner = owner ?? System.Windows.Application.Current.MainWindow;
        
        // 显示对话框
        window.ShowDialog();

        return viewModel.Result;
    }

    /// <summary>
    /// 显示信息消息框
    /// </summary>
    public static MessageBoxResult Information(string message, string? title = null, Window? owner = null)
    {
        return Show(message, title, MessageBoxImage.Information, MessageBoxButton.OK, owner);
    }

    /// <summary>
    /// 显示警告消息框
    /// </summary>
    public static MessageBoxResult Warning(string message, string? title = null, Window? owner = null)
    {
        return Show(message, title, MessageBoxImage.Warning, MessageBoxButton.OK, owner);
    }

    /// <summary>
    /// 显示错误消息框
    /// </summary>
    public static MessageBoxResult Error(string message, string? title = null, Window? owner = null)
    {
        return Show(message, title, MessageBoxImage.Error, MessageBoxButton.OK, owner);
    }

    /// <summary>
    /// 显示确认消息框
    /// </summary>
    public static MessageBoxResult Question(string message, string? title = null, Window? owner = null)
    {
        return Show(message, title, MessageBoxImage.Question, MessageBoxButton.YesNo, owner);
    }

    /// <summary>
    /// 获取默认标题
    /// </summary>
    private static string GetDefaultTitle(MessageBoxImage icon, ILocalizationManager? localizationManager)
    {
        var key = icon switch
        {
            MessageBoxImage.Information => "common.messageBox.information",
            MessageBoxImage.Warning => "common.messageBox.warning",
            MessageBoxImage.Error => "common.messageBox.error",
            MessageBoxImage.Question => "common.messageBox.question",
            _ => "common.messageBox.information"
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
    /// 设置按钮
    /// </summary>
    private static void SetupButtons(TaktMessageBoxViewModel viewModel, MessageBoxButton button, ILocalizationManager? localizationManager)
    {
        viewModel.ShowOkButton = false;
        viewModel.ShowYesButton = false;
        viewModel.ShowNoButton = false;
        viewModel.ShowCancelButton = false;

        switch (button)
        {
            case MessageBoxButton.OK:
                viewModel.ShowOkButton = true;
                viewModel.OkButtonText = localizationManager?.GetString("common.button.ok") ?? "确定";
                break;

            case MessageBoxButton.OKCancel:
                viewModel.ShowOkButton = true;
                viewModel.ShowCancelButton = true;
                viewModel.OkButtonText = localizationManager?.GetString("common.button.ok") ?? "确定";
                viewModel.CancelButtonText = localizationManager?.GetString("common.button.cancel") ?? "取消";
                break;

            case MessageBoxButton.YesNo:
                viewModel.ShowYesButton = true;
                viewModel.ShowNoButton = true;
                viewModel.YesButtonText = localizationManager?.GetString("common.button.yes") ?? "是";
                viewModel.NoButtonText = localizationManager?.GetString("common.button.no") ?? "否";
                break;

            case MessageBoxButton.YesNoCancel:
                viewModel.ShowYesButton = true;
                viewModel.ShowNoButton = true;
                viewModel.ShowCancelButton = true;
                viewModel.YesButtonText = localizationManager?.GetString("common.button.yes") ?? "是";
                viewModel.NoButtonText = localizationManager?.GetString("common.button.no") ?? "否";
                viewModel.CancelButtonText = localizationManager?.GetString("common.button.cancel") ?? "取消";
                break;
        }
    }
}

