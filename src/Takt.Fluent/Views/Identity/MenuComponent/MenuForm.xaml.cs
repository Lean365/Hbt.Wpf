// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity.MenuComponent
// 文件名称：MenuForm.xaml.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：菜单表单窗口（新建/编辑菜单）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Identity;

namespace Takt.Fluent.Views.Identity.MenuComponent;

/// <summary>
/// 菜单表单窗口（新建/编辑菜单）
/// </summary>
public partial class MenuForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private MenuFormViewModel? _viewModel;

    /// <summary>
    /// 初始化菜单表单窗口
    /// </summary>
    /// <param name="vm">菜单表单视图模型</param>
    /// <param name="localizationManager">本地化管理器</param>
    public MenuForm(MenuFormViewModel vm, ILocalizationManager? localizationManager = null)
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

        // 基本信息：菜单名称、菜单编码、国际化键、权限码、菜单类型、父级菜单ID = 6个字段
        double basicInfoHeight = 6 * 56; // 6个字段，每个字段56px

        // 路由信息：路由路径、图标、组件路径 = 3个字段
        double routeInfoHeight = 3 * 56; // 3个字段，每个字段56px

        // 状态信息：是否外链、是否缓存、是否可见、排序号、菜单状态 = 5个字段
        double statusInfoHeight = 5 * 56; // 5个字段，每个字段56px

        // 备注信息：固定高度200px的文本框
        const double remarksInfoHeight = 200 + 32 + 24; // 文本框高度 + StackPanel Margin + 错误文本

        double maxTabContentHeight = Math.Max(Math.Max(Math.Max(basicInfoHeight, routeInfoHeight), statusInfoHeight), remarksInfoHeight);

        const double tabControlHeaderHeight = 52;
        const double buttonAreaHeight = 52;
        const double windowMargin = 48;
        const double tabControlMargin = 16;
        const double buttonMargin = 20;
        const double extraBuffer = 48;

        double optimalHeight = maxTabContentHeight + tabControlHeaderHeight + buttonAreaHeight + windowMargin + tabControlMargin + buttonMargin;

        const double minHeight = 600;
        const double maxHeight = 1200;
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

