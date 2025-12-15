// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity.UserComponent
// 文件名称：UserAssignRole.xaml.cs
// 创建时间：2025-11-13
// 创建人：Takt365(Cursor AI)
// 功能描述：用户分配角色窗口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Identity;

namespace Takt.Fluent.Views.Identity.UserComponent;

/// <summary>
/// 用户分配角色窗口
/// </summary>
public partial class UserAssignRole : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private UserAssignRoleViewModel? _viewModel;

    public UserAssignRole(UserAssignRoleViewModel vm, ILocalizationManager? localizationManager = null)
    {
        InitializeComponent();
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _viewModel = vm;
        DataContext = vm;

        if (Owner == null)
        {
            Owner = System.Windows.Application.Current.MainWindow;
        }

        Loaded += (s, e) =>
        {
            // ILocalizationManager 初始化在应用启动时完成，无需在此初始化
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                CalculateAndSetOptimalHeight();
                CenterWindow();
                if (Owner != null)
                {
                    Owner.SizeChanged += Owner_SizeChanged;
                    Owner.LocationChanged += Owner_LocationChanged;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        };

        Unloaded += (_, _) =>
        {
            if (Owner != null)
            {
                Owner.SizeChanged -= Owner_SizeChanged;
                Owner.LocationChanged -= Owner_LocationChanged;
            }
        };

        if (_localizationManager != null)
        {
            _localizationManager.LanguageChanged += (sender, langCode) =>
            {
                // ViewModel 会处理标题更新，这里不需要额外处理
            };
        }
    }

    private void CalculateAndSetOptimalHeight()
    {
        const double minHeight = 500;
        const double maxHeight = 800;

        // 窗口总高度 = 用户信息区域 + 角色列表区域 + 消息区域 + 按钮区域 + 各种 Margin
        const double userInfoHeight = 100;
        const double roleListMinHeight = 300;
        const double messageAreaHeight = 50;
        const double buttonAreaHeight = 52;
        const double windowMargin = 48;
        const double spacing = 32;

        double optimalHeight = userInfoHeight + roleListMinHeight + messageAreaHeight + buttonAreaHeight + windowMargin + spacing;

        Height = Math.Max(minHeight, Math.Min(maxHeight, optimalHeight));
        Width = Math.Max(600, Math.Min(1000, 800));
    }

    private void CenterWindow()
    {
        if (Owner != null)
        {
            var minWidth = Owner.ActualWidth * 0.4;
            var maxWidth = Owner.ActualWidth * 0.6;
            var calculatedWidth = Owner.ActualWidth * 0.5;
            Width = Math.Max(minWidth, Math.Min(calculatedWidth, maxWidth));

            var minHeight = Owner.ActualHeight * 0.7;
            var maxHeight = Owner.ActualHeight;
            var calculatedHeight = Height;
            Height = Math.Max(minHeight, Math.Min(calculatedHeight, maxHeight));

            Left = Owner.Left + (Owner.ActualWidth - Width) / 2;
            Top = Owner.Top + (Owner.ActualHeight - Height) / 2;
        }
        else
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            if (Width == 0 || double.IsNaN(Width))
            {
                Width = Math.Min(800, screenWidth * 0.6);
            }
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Math.Min(600, screenHeight * 0.7);
            }

            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }

    private void Owner_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        CenterWindow();
    }

    private void Owner_LocationChanged(object? sender, EventArgs e)
    {
        CenterWindow();
    }
}

