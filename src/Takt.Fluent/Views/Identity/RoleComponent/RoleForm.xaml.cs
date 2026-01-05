// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity.RoleComponent
// 文件名称：RoleForm.xaml.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：角色表单窗口（新建/编辑角色）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Identity;

namespace Takt.Fluent.Views.Identity.RoleComponent;

/// <summary>
/// 角色表单窗口（新建/编辑角色）
/// </summary>
public partial class RoleForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private RoleFormViewModel? _viewModel;

    /// <summary>
    /// 初始化角色表单窗口
    /// </summary>
    /// <param name="vm">角色表单视图模型</param>
    /// <param name="languageService">语言服务</param>
    public RoleForm(RoleFormViewModel vm, ILocalizationManager? localizationManager = null)
    {
        InitializeComponent();
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _viewModel = vm;
        DataContext = vm;
        
        // 设置 Owner（如果还没有设置）
        if (Owner == null)
        {
            Owner = System.Windows.Application.Current?.MainWindow;
        }
        
        // ILocalizationManager 初始化在应用启动时完成，无需在此初始化
        Loaded += (s, e) =>
        {
            // 延迟计算高度
            _ = Dispatcher.BeginInvoke(new System.Action(() =>
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
        
        // 订阅语言变化事件（通过 ILocalizationManager）
        if (_localizationManager != null)
        {
            _localizationManager.LanguageChanged += (sender, langCode) =>
            {
                // ViewModel 会处理标题更新，这里不需要额外处理
            };
        }
    }

    /// <summary>
    /// 计算并设置最佳窗口高度
    /// </summary>
    private void CalculateAndSetOptimalHeight()
    {
        if (_viewModel == null) return;

        // 基本信息：角色名称、角色编码、角色描述 = 3个字段
        double basicInfoHeight = 3 * 56; // 3个字段，每个字段56px

        // 状态信息：数据范围、排序号、状态 = 3个字段
        double statusInfoHeight = 3 * 56; // 3个字段，每个字段56px

        // 备注信息：固定高度200px的文本框
        const double remarksInfoHeight = 200 + 32 + 24; // 文本框高度 + StackPanel Margin + 错误文本

        double maxTabContentHeight = Math.Max(Math.Max(basicInfoHeight, statusInfoHeight), remarksInfoHeight);

        const double tabControlHeaderHeight = 52;
        const double buttonAreaHeight = 52;
        const double windowMargin = 48;
        const double tabControlMargin = 16;
        const double buttonMargin = 20;
        const double extraBuffer = 48;

        double optimalHeight = maxTabContentHeight + tabControlHeaderHeight + buttonAreaHeight + windowMargin + tabControlMargin + buttonMargin;

        const double minHeight = 500;
        const double maxHeight = 1000;
        Height = Math.Max(minHeight, Math.Min(maxHeight, optimalHeight + extraBuffer));
    }

    /// <summary>
    /// 居中窗口
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            Left = Owner.Left + (Owner.Width - Width) / 2;
            Top = Owner.Top + (Owner.Height - Height) / 2;
        }
        else
        {
            // 如果没有 Owner，居中到屏幕
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
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

