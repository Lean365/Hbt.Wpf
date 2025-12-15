// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktMessageBoxWindow.xaml.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：统一的消息框窗口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace Takt.Fluent.Controls;

/// <summary>
/// 消息框窗口
/// </summary>
public partial class TaktMessageBoxWindow : Window
{
    public TaktMessageBoxWindow()
    {
        InitializeComponent();
    }
}

/// <summary>
/// 消息框视图模型
/// </summary>
public partial class TaktMessageBoxViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private PackIconKind _iconKind = PackIconKind.Information;

    [ObservableProperty]
    private Brush _iconBrush = Brushes.Blue;

    [ObservableProperty]
    private bool _showOkButton = false;

    [ObservableProperty]
    private bool _showYesButton = false;

    [ObservableProperty]
    private bool _showNoButton = false;

    [ObservableProperty]
    private bool _showCancelButton = false;

    [ObservableProperty]
    private string _okButtonText = "确定";

    [ObservableProperty]
    private string _yesButtonText = "是";

    [ObservableProperty]
    private string _noButtonText = "否";

    [ObservableProperty]
    private string _cancelButtonText = "取消";

    [ObservableProperty]
    private MessageBoxResult _result = MessageBoxResult.None;

    private Window? _parentWindow;

    public TaktMessageBoxViewModel(Window? parentWindow = null)
    {
        _parentWindow = parentWindow;
    }

    [RelayCommand]
    private void Ok()
    {
        Result = MessageBoxResult.OK;
        CloseWindow();
    }

    [RelayCommand]
    private void Yes()
    {
        Result = MessageBoxResult.Yes;
        CloseWindow();
    }

    [RelayCommand]
    private void No()
    {
        Result = MessageBoxResult.No;
        CloseWindow();
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = MessageBoxResult.Cancel;
        CloseWindow();
    }

    private void CloseWindow()
    {
        // 查找父窗口并关闭
        var window = System.Windows.Application.Current.Windows.OfType<TaktMessageBoxWindow>()
            .FirstOrDefault(w => w.DataContext == this);
        window?.Close();
    }
}

