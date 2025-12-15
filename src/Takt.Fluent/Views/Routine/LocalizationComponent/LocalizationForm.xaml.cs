// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine.LocalizationComponent
// 文件名称：LocalizationForm.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：翻译表单窗口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine.LocalizationComponent;

/// <summary>
/// 翻译表单窗口
/// </summary>
public partial class LocalizationForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private LocalizationFormViewModel? _viewModel;

    /// <summary>
    /// 初始化翻译表单窗口
    /// </summary>
    /// <param name="vm">翻译表单视图模型</param>
    /// <param name="localizationManager">本地化管理器</param>
    public LocalizationForm(LocalizationFormViewModel vm, ILocalizationManager? localizationManager = null)
    {
        InitializeComponent();
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _viewModel = vm;
        DataContext = vm;

        // 设置 Owner（如果还没有设置）
        if (Owner == null)
        {
            Owner = System.Windows.Application.Current.MainWindow;
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

        // 设置保存成功回调
        vm.SaveSuccessCallback = () => Close();
    }

    /// <summary>
    /// 计算并设置最佳窗口高度
    /// </summary>
    private void CalculateAndSetOptimalHeight()
    {
        if (_viewModel == null) return;

        double contentHeight = CalculateContentHeight();

        const double scrollViewerBorder = 2;
        const double buttonAreaHeight = 52;
        const double windowMargin = 48;
        const double buttonMargin = 20;

        double optimalHeight = contentHeight + scrollViewerBorder + buttonAreaHeight + windowMargin + buttonMargin;

        const double extraBuffer = 48;
        const double minHeight = 600;
        const double maxHeight = 1200;
        Height = Math.Max(minHeight, Math.Min(maxHeight, optimalHeight + extraBuffer));
    }

    /// <summary>
    /// 计算内容高度
    /// </summary>
    private double CalculateContentHeight()
    {
        // 统计字段数量：语言代码、翻译键、翻译值、模块、描述、排序号、备注 = 7个
        int normalFieldCount = 5; // 语言代码、翻译键、翻译值、模块、排序号
        double normalFieldsHeight = CalculateHeightByFieldCount(normalFieldCount);

        // 描述和备注字段高度
        const double descriptionTextBoxHeight = 80;
        const double descriptionFieldHeight = descriptionTextBoxHeight + 56;

        const double remarksTextBoxHeight = 80;
        const double remarksFieldHeight = remarksTextBoxHeight + 56;

        const double stackPanelMargin = 32;
        double normalFieldsSpacing = (normalFieldCount - 1) * 8.0;
        const double specialFieldsSpacing = 8.0 * 2; // 描述和备注之间的间距

        double totalHeight = normalFieldsHeight + descriptionFieldHeight + remarksFieldHeight + normalFieldsSpacing + specialFieldsSpacing + stackPanelMargin;

        const double buffer = 24.0;
        totalHeight += buffer;

        return totalHeight;
    }

    /// <summary>
    /// 根据字段数量计算高度
    /// </summary>
    private double CalculateHeightByFieldCount(int fieldCount)
    {
        const double fieldHeight = 56;
        const double fieldSpacing = 8;

        if (fieldCount <= 0)
        {
            return 0;
        }

        double fieldsHeight = fieldCount * fieldHeight;
        double fieldsSpacing = (fieldCount - 1) * fieldSpacing;

        return fieldsHeight + fieldsSpacing;
    }

    /// <summary>
    /// 居中窗口到父窗口或屏幕
    /// </summary>
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
            var calculatedHeight = CalculateOptimalHeight();
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
                Height = Math.Min(700, screenHeight * 0.7);
            }

            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }

    /// <summary>
    /// 计算最佳高度（不包含父窗口限制）
    /// </summary>
    private double CalculateOptimalHeight()
    {
        if (_viewModel == null) return 700;

        double contentHeight = CalculateContentHeight();

        const double scrollViewerBorder = 2;
        const double buttonAreaHeight = 52;
        const double windowMargin = 48;
        const double buttonMargin = 20;

        double optimalHeight = contentHeight + scrollViewerBorder + buttonAreaHeight + windowMargin + buttonMargin;

        const double extraBuffer = 48;
        const double minHeight = 600;
        const double maxHeight = 1200;
        return Math.Max(minHeight, Math.Min(maxHeight, optimalHeight + extraBuffer));
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

