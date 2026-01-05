// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine.DictionaryComponent
// 文件名称：DictionaryForm.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典表单窗口（新建/编辑字典类型和字典数据）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine.DictionaryComponent;

/// <summary>
/// 字典表单窗口（新建/编辑字典类型和字典数据）
/// </summary>
public partial class DictionaryForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private DictionaryFormViewModel? _viewModel;

    /// <summary>
    /// 初始化字典表单窗口
    /// </summary>
    /// <param name="vm">字典表单视图模型</param>
    /// <param name="localizationManager">本地化管理器</param>
    public DictionaryForm(DictionaryFormViewModel vm, ILocalizationManager? localizationManager = null)
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

        Loaded += (s, e) =>
        {
            // 延迟计算高度，确保 UI 已完全渲染
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                // 计算并设置最佳窗口高度
                CalculateAndSetOptimalHeight();

                // 居中窗口
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
    }

    /// <summary>
    /// 计算并设置最佳窗口高度
    /// </summary>
    private void CalculateAndSetOptimalHeight()
    {
        if (_viewModel == null) return;

        // 计算字段数量
        const int fieldCount = 5; // 类型代码、类型名称、排序号、类型状态、备注

        // 每个字段 StackPanel：MinHeight=56
        const double fieldHeight = 56;
        const double fieldSpacing = 8; // 字段之间的间距
        const double stackPanelMargin = 32; // StackPanel Margin="16"（上下各16，共32）
        const double scrollViewerMargin = 16; // ScrollViewer 底部 Margin="0,0,0,16"
        const double buttonAreaHeight = 52; // 按钮区域高度
        const double buttonMargin = 20; // 按钮区域顶部 Margin="0,20,0,0"
        const double windowMargin = 48; // 窗口 Margin="24"（上下各24，共48）

        // 备注字段特殊处理（MinHeight=80）
        const double remarksFieldHeight = 80;
        const double normalFieldHeight = fieldHeight;
        const double normalFieldCount = fieldCount - 1; // 除了备注字段

        double fieldsHeight = (normalFieldCount * normalFieldHeight) + remarksFieldHeight;
        double fieldsSpacing = (fieldCount - 1) * fieldSpacing;
        const double buffer = 24;

        double contentHeight = fieldsHeight + fieldsSpacing + stackPanelMargin + buffer;
        double optimalHeight = contentHeight + scrollViewerMargin + buttonAreaHeight + buttonMargin + windowMargin;

        // 设置最小和最大高度限制
        const double minHeight = 500;
        const double maxHeight = 900;
        Height = Math.Max(minHeight, Math.Min(maxHeight, optimalHeight));
    }

    /// <summary>
    /// 居中窗口到父窗口或屏幕
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            // 相对于父窗口居中，默认大小为父窗口的95%
            Width = Owner.ActualWidth * 0.95;
            Height = Owner.ActualHeight * 0.95;

            // 计算居中位置
            Left = Owner.Left + (Owner.ActualWidth - Width) / 2;
            Top = Owner.Top + (Owner.ActualHeight - Height) / 2;
        }
        else
        {
            // 相对于屏幕居中
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            if (Width == 0 || double.IsNaN(Width))
            {
                Width = Math.Min(700, screenWidth * 0.5);
            }
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Math.Min(600, screenHeight * 0.6);
            }

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

    /// <summary>
    /// 计算最佳高度（不包含父窗口限制）
    /// </summary>
    private double CalculateOptimalHeight()
    {
        if (_viewModel == null) return 600;

        const int fieldCount = 5;
        const double fieldHeight = 56;
        const double fieldSpacing = 8;
        const double stackPanelMargin = 32;
        const double scrollViewerMargin = 16;
        const double buttonAreaHeight = 52;
        const double buttonMargin = 20;
        const double windowMargin = 48;

        const double remarksFieldHeight = 80;
        const double normalFieldHeight = fieldHeight;
        const double normalFieldCount = fieldCount - 1;

        double fieldsHeight = (normalFieldCount * normalFieldHeight) + remarksFieldHeight;
        double fieldsSpacing = (fieldCount - 1) * fieldSpacing;
        const double buffer = 24;

        double contentHeight = fieldsHeight + fieldsSpacing + stackPanelMargin + buffer;
        double optimalHeight = contentHeight + scrollViewerMargin + buttonAreaHeight + buttonMargin + windowMargin;

        const double minHeight = 500;
        const double maxHeight = 900;
        return Math.Max(minHeight, Math.Min(maxHeight, optimalHeight));
    }
}

