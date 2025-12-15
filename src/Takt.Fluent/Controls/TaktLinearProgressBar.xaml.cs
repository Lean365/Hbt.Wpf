// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktLinearProgressBar.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：基于 MaterialDesignLinearProgressBar 的线性进度条控件
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;

namespace Takt.Fluent.Controls;

/// <summary>
/// 基于 MaterialDesignLinearProgressBar 的线性进度条控件
/// </summary>
public partial class TaktLinearProgressBar : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktLinearProgressBar.xaml", UriKind.Relative);

    #region 依赖属性

    /// <summary>
    /// 进度值属性
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(TaktLinearProgressBar),
            new PropertyMetadata(0.0, OnValueChanged));

    /// <summary>
    /// 最小值属性
    /// </summary>
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(TaktLinearProgressBar),
            new PropertyMetadata(0.0));

    /// <summary>
    /// 最大值属性
    /// </summary>
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(TaktLinearProgressBar),
            new PropertyMetadata(100.0));

    /// <summary>
    /// 是否不确定进度属性
    /// </summary>
    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(
            nameof(IsIndeterminate),
            typeof(bool),
            typeof(TaktLinearProgressBar),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(
            nameof(IsEnabled),
            typeof(bool),
            typeof(TaktLinearProgressBar),
            new PropertyMetadata(true));

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取或设置进度值
    /// </summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// 获取或设置最小值
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// 获取或设置最大值
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// 获取或设置是否不确定进度（显示动画加载效果）
    /// </summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// 获取或设置是否启用
    /// </summary>
    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 TaktLinearProgressBar 的新实例
    /// </summary>
    public TaktLinearProgressBar()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        // 设置默认高度为 4
        Height = 4.0;
    }

    #endregion

    #region 事件处理

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktLinearProgressBar control)
        {
            var newValue = (double)e.NewValue;
            // 确保值在有效范围内
            if (newValue < control.Minimum)
                control.Value = control.Minimum;
            else if (newValue > control.Maximum)
                control.Value = control.Maximum;
        }
    }

    #endregion
}

