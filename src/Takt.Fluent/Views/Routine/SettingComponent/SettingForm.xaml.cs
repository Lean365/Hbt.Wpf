// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine.SettingComponent
// 文件名称：SettingForm.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：系统设置表单窗口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine.SettingComponent;

/// <summary>
/// 系统设置表单窗口
/// </summary>
public partial class SettingForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private SettingFormViewModel? _viewModel;

    /// <summary>
    /// 初始化系统设置表单窗口
    /// </summary>
    /// <param name="vm">系统设置表单视图模型</param>
    /// <param name="languageService">语言服务</param>
    public SettingForm(SettingFormViewModel vm, ILocalizationManager? localizationManager = null)
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

        // ILocalizationManager 初始化在应用启动时完成，无需在此初始化
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

        // 订阅语言变化事件（通过 ILocalizationManager）
        if (_localizationManager != null)
        {
            _localizationManager.LanguageChanged += (sender, langCode) =>
            {
                // ViewModel 会处理标题更新，这里不需要额外处理
            };
        }

        // 设置保存成功回调
        vm.SaveSuccessCallback = () => Close();
    }

    /// <summary>
    /// 计算并设置最佳窗口高度
    /// </summary>
    /// <remarks>
    /// 统计字段数量，计算实际高度，确保所有字段都能显示
    /// </remarks>
    private void CalculateAndSetOptimalHeight()
    {
        if (_viewModel == null) return;

        // 计算内容高度
        double contentHeight = CalculateContentHeight();

        // 窗口总高度 = 内容高度 + ScrollViewer 边框 + 按钮区域 + 各种 Margin
        const double scrollViewerBorder = 2; // ScrollViewer 边框
        const double buttonAreaHeight = 52; // 按钮区域高度（按钮高度约 36px + 按钮间距 + StackPanel 内边距）
        const double windowMargin = 48; // 窗口 Margin="24"（上下各24，共48）
        const double buttonMargin = 20; // 按钮区域顶部 Margin="0,20,0,0"

        double optimalHeight = contentHeight + scrollViewerBorder + buttonAreaHeight + windowMargin + buttonMargin;

        // 添加额外的缓冲空间，确保所有内容都能完整显示（考虑实际渲染、边框、内边距、标题栏等）
        const double extraBuffer = 48;

        // 设置最小和最大高度限制，确保所有内容都能显示
        const double minHeight = 500;
        const double maxHeight = 1000;
        Height = Math.Max(minHeight, Math.Min(maxHeight, optimalHeight + extraBuffer));
    }

    /// <summary>
    /// 计算内容高度
    /// </summary>
    /// <returns>内容高度（像素）</returns>
    private double CalculateContentHeight()
    {
        // 统计字段数量
        int normalFieldCount = GetNormalFieldCount(); // 普通字段：设置键、设置值、分类、排序号、设置类型 = 5个
        double normalFieldsHeight = CalculateHeightByFieldCount(normalFieldCount);

        // 特殊字段高度：设置描述（MinHeight=80）、备注（MinHeight=80）
        const double descriptionTextBoxHeight = 80; // 设置描述文本框的 MinHeight
        const double descriptionErrorTextHeight = 24; // 错误文本区域（MinHeight=20 + Margin Top=4）
        const double descriptionFieldHeight = descriptionTextBoxHeight + descriptionErrorTextHeight + 56; // 包含 StackPanel 的 MinHeight=56

        const double remarksTextBoxHeight = 80; // 备注文本框的 MinHeight
        const double remarksFieldHeight = remarksTextBoxHeight + 56; // 包含 StackPanel 的 MinHeight=56

        // StackPanel Margin="16"（上下各16，共32）
        const double stackPanelMargin = 32;

        // 字段间距：每个字段 StackPanel 的 Margin="0,0,0,8"
        // 普通字段有 5 个，间距 4 个（5-1）
        const int normalFieldCountConst = 5;
        const double normalFieldsSpacing = (normalFieldCountConst - 1) * 8.0;
        // 设置描述和备注之间有 1 个间距
        const double specialFieldsSpacing = 8.0;

        double totalHeight = normalFieldsHeight + descriptionFieldHeight + remarksFieldHeight + normalFieldsSpacing + specialFieldsSpacing + stackPanelMargin;

        // 添加缓冲空间
        const double buffer = 24.0;
        totalHeight += buffer;

        return totalHeight;
    }

    /// <summary>
    /// 统计普通字段数量
    /// </summary>
    /// <returns>字段数量</returns>
    /// <remarks>
    /// 普通字段：设置键、设置值、分类、排序号、设置类型 = 5个
    /// </remarks>
    private int GetNormalFieldCount()
    {
        return 5; // 设置键、设置值、分类、排序号、设置类型
    }

    /// <summary>
    /// 根据字段数量计算高度
    /// </summary>
    /// <param name="fieldCount">字段数量</param>
    /// <returns>字段区域高度（像素）</returns>
    /// <remarks>
    /// 使用统一的字段高度和间距计算逻辑
    /// 每个字段 StackPanel：MinHeight=56（Grid 32px + 错误文本区域 24px）
    /// 字段间距：每个字段 StackPanel 的 Margin="0,0,0,8"
    /// </remarks>
    private double CalculateHeightByFieldCount(int fieldCount)
    {
        const double fieldHeight = 56; // 每个字段 StackPanel 的 MinHeight（Grid 32 + 错误文本 24）
        const double fieldSpacing = 8; // 字段之间的间距（每个字段 StackPanel 的 Margin="0,0,0,8"）

        if (fieldCount <= 0)
        {
            return 0;
        }

        // 字段高度总和
        double fieldsHeight = fieldCount * fieldHeight;

        // 字段间距总和（n个字段有 n-1 个间距）
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
            // 相对于父窗口居中
            // 窗口宽度限制：最小为父窗口的40%，最大为父窗口的60%
            var minWidth = Owner.ActualWidth * 0.4;
            var maxWidth = Owner.ActualWidth * 0.6;
            // 使用50%作为默认宽度，但限制在40%-60%之间
            var calculatedWidth = Owner.ActualWidth * 0.5;
            Width = Math.Max(minWidth, Math.Min(calculatedWidth, maxWidth));

            // 窗口高度限制：最小为父窗口的70%，最大为父窗口的100%
            var minHeight = Owner.ActualHeight * 0.7;
            var maxHeight = Owner.ActualHeight;
            var calculatedHeight = CalculateOptimalHeight();
            // 确保高度在最小和最大限制之间，优先使用计算出的最佳高度
            Height = Math.Max(minHeight, Math.Min(calculatedHeight, maxHeight));

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
                Width = Math.Min(800, screenWidth * 0.6);
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
    /// 计算最佳高度（不包含父窗口限制）
    /// </summary>
    /// <returns>计算出的最佳窗口高度（像素）</returns>
    /// <remarks>
    /// 计算逻辑与 CalculateAndSetOptimalHeight 相同，但不设置窗口高度
    /// 用于在父窗口大小变化时获取计算值，然后与父窗口限制进行比较
    /// </remarks>
    private double CalculateOptimalHeight()
    {
        if (_viewModel == null) return 600;

        // 计算内容高度
        double contentHeight = CalculateContentHeight();

        const double scrollViewerBorder = 2;
        const double buttonAreaHeight = 52;
        const double windowMargin = 48;
        const double buttonMargin = 20;

        double optimalHeight = contentHeight + scrollViewerBorder + buttonAreaHeight + windowMargin + buttonMargin;

        const double extraBuffer = 48;

        const double minHeight = 500;
        const double maxHeight = 1000;
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

