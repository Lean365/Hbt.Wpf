// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Logistics.Visits.VisitsComponent
// 文件名称：VisitingForm.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：来访公司表单窗口（新建/编辑来访公司）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using System.Windows.Controls;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Logistics.Visits;

namespace Takt.Fluent.Views.Logistics.Visits.VisitsComponent;

/// <summary>
/// 来访公司表单窗口（新建/编辑来访公司）
/// </summary>
public partial class VisitingForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private VisitingFormViewModel? _viewModel;

    /// <summary>
    /// 初始化来访公司表单窗口
    /// </summary>
    /// <param name="vm">来访公司表单视图模型</param>
    /// <param name="localizationManager">本地化管理器</param>
    public VisitingForm(VisitingFormViewModel vm, ILocalizationManager? localizationManager = null)
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

        Closing += EntourageForm_Closing;

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
        const int fieldCount = 4; // 公司名称、开始时间、结束时间、备注

        // 每个字段 StackPanel：MinHeight=56（Grid 32px + 错误文本区域 24px）
        const double fieldHeight = 56;
        const double fieldSpacing = 8; // 字段之间的间距
        const double stackPanelMargin = 32; // StackPanel Margin="16"（上下各16，共32）
        const double scrollViewerMargin = 16; // ScrollViewer 底部 Margin="0,0,0,16"
        const double buttonAreaHeight = 52; // 按钮区域高度
        const double buttonMargin = 20; // 按钮区域顶部 Margin="0,20,0,0"
        const double windowMargin = 48; // 窗口 Margin="24"（上下各24，共48）

        // 备注字段特殊处理（MinHeight=120）
        const double remarksFieldHeight = 120;
        const double normalFieldHeight = fieldHeight;
        const double normalFieldCount = fieldCount - 1; // 除了备注字段

        double fieldsHeight = (normalFieldCount * normalFieldHeight) + remarksFieldHeight;
        double fieldsSpacing = (fieldCount - 1) * fieldSpacing;
        const double buffer = 24;

        double contentHeight = fieldsHeight + fieldsSpacing + stackPanelMargin + buffer;
        double optimalHeight = contentHeight + scrollViewerMargin + buttonAreaHeight + buttonMargin + windowMargin;

        // 设置最小和最大高度限制
        const double minHeight = 400;
        const double maxHeight = 800;
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
                Width = Math.Min(600, screenWidth * 0.5);
            }
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Math.Min(500, screenHeight * 0.6);
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
    /// 窗口关闭事件处理（处理取消操作时的清理逻辑）
    /// </summary>
    private void EntourageForm_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // 如果窗口是通过取消按钮关闭的（DialogResult 为 false 或 null），执行清理逻辑
        if (_viewModel != null && (DialogResult == false || DialogResult == null))
        {
            // 调用 ViewModel 的取消清理逻辑
            _viewModel.OnCancel();
        }
    }

    /// <summary>
    /// 计算最佳高度（不包含父窗口限制）
    /// </summary>
    private double CalculateOptimalHeight()
    {
        if (_viewModel == null) return 500;

        const int fieldCount = 4;
        const double fieldHeight = 56;
        const double fieldSpacing = 8;
        const double stackPanelMargin = 32;
        const double scrollViewerMargin = 16;
        const double buttonAreaHeight = 52;
        const double buttonMargin = 20;
        const double windowMargin = 48;

        const double remarksFieldHeight = 120;
        const double normalFieldHeight = fieldHeight;
        const double normalFieldCount = fieldCount - 1;

        double fieldsHeight = (normalFieldCount * normalFieldHeight) + remarksFieldHeight;
        double fieldsSpacing = (fieldCount - 1) * fieldSpacing;
        const double buffer = 24;

        double contentHeight = fieldsHeight + fieldsSpacing + stackPanelMargin + buffer;
        double optimalHeight = contentHeight + scrollViewerMargin + buttonAreaHeight + buttonMargin + windowMargin;

        const double minHeight = 400;
        const double maxHeight = 800;
        return Math.Max(minHeight, Math.Min(maxHeight, optimalHeight));
    }
}

