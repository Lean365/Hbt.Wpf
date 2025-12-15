// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktToastWindow.xaml.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：Toast 通知窗口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace Takt.Fluent.Controls;

/// <summary>
/// Toast 通知窗口
/// </summary>
public partial class TaktToastWindow : Window
{
    private readonly DispatcherTimer _closeTimer;

    public TaktToastWindow(Window? owner = null)
    {
        InitializeComponent();
        
        Owner = owner ?? System.Windows.Application.Current.MainWindow;
        
        // 创建关闭定时器
        _closeTimer = new DispatcherTimer();
        _closeTimer.Tick += CloseTimer_Tick;
        
        Loaded += TaktToastWindow_Loaded;
    }

    private void SetWindowPosition()
    {
        // 设置窗口位置（顶部居中，顶端对齐）
        UpdateLayout();
        
        if (Owner != null)
        {
            // 相对于所有者窗口顶部居中
            Left = Owner.Left + (Owner.Width - ActualWidth) / 2;
            Top = Owner.Top + 20; // 距离顶部 20px
        }
        else
        {
            // 相对于工作区顶部居中
            Left = (SystemParameters.WorkArea.Width - ActualWidth) / 2;
            Top = 20; // 距离顶部 20px
        }
    }

    private void TaktToastWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 等待布局完成后再设置位置
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // 设置窗口位置
            SetWindowPosition();
            
            // 播放淡入动画
            var fadeInAnimation = (Storyboard)FindResource("FadeInAnimation");
            fadeInAnimation?.Begin(this);

            // 启动关闭定时器
            var viewModel = (TaktToastViewModel)DataContext;
            if (viewModel != null && viewModel.Duration > 0)
            {
                _closeTimer.Interval = TimeSpan.FromMilliseconds(viewModel.Duration);
                _closeTimer.Start();
            }
        }), DispatcherPriority.Loaded);
    }

    private void CloseTimer_Tick(object? sender, EventArgs e)
    {
        _closeTimer.Stop();
        CloseWithAnimation();
    }

    private void CloseWithAnimation()
    {
        var fadeOutAnimation = (Storyboard)FindResource("FadeOutAnimation");
        if (fadeOutAnimation != null)
        {
            fadeOutAnimation.Completed += (s, e) => Close();
            fadeOutAnimation.Begin(this);
        }
        else
        {
            Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _closeTimer?.Stop();
        base.OnClosed(e);
    }
}

/// <summary>
/// Toast 视图模型
/// </summary>
public partial class TaktToastViewModel : ObservableObject
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
    private int _duration = 5000;

    [ObservableProperty]
    private Brush _borderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
}

