// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity.UserComponent
// 文件名称：UserProfile.xaml.cs
// 创建时间：2025-10-31
// 创建人：Takt365(Cursor AI)
// 功能描述：用户信息窗体
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using Takt.Fluent.ViewModels.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Takt.Fluent.Views.Identity.UserComponent;

/// <summary>
/// UserProfile.xaml 的交互逻辑
/// </summary>
public partial class UserProfile : Window
{
    public UserProfileViewModel ViewModel { get; }

    public UserProfile(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;

        // 设置 Owner（如果还没有设置）
        if (Owner == null)
        {
            Owner = System.Windows.Application.Current.MainWindow;
        }

        Loaded += (_, _) =>
        {
            CenterWindow();
            
            if (Owner != null)
            {
                Owner.SizeChanged += Owner_SizeChanged;
                Owner.LocationChanged += Owner_LocationChanged;
            }
        };

        Unloaded += (_, _) =>
        {
            if (Owner != null)
            {
                Owner.SizeChanged -= Owner_SizeChanged;
                Owner.LocationChanged -= Owner_LocationChanged;
            }
        };
    }

    /// <summary>
    /// 居中窗口到父窗口或屏幕
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            // 相对于父窗口居中
            Width = Owner.ActualWidth * 0.7;
            Height = Owner.ActualHeight * 0.7;

            // 计算居中位置
            Left = Owner.Left + (Owner.ActualWidth - Width) / 2;
            Top = Owner.Top + (Owner.ActualHeight - Height) / 2;
        }
        else
        {
            // 相对于屏幕居中
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            
            // 设置默认大小
            if (Width == 0 || double.IsNaN(Width))
            {
                Width = Math.Min(800, screenWidth * 0.7);
            }
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Math.Min(600, screenHeight * 0.7);
            }
            
            // 居中到屏幕
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }

    /// <summary>
    /// 父窗口大小变化事件处理
    /// </summary>
    private void Owner_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        CenterWindow();
    }

    /// <summary>
    /// 父窗口位置变化事件处理
    /// </summary>
    private void Owner_LocationChanged(object? sender, EventArgs e)
    {
        CenterWindow();
    }
}
