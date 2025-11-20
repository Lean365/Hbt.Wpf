// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktCheckBox.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义复选框控件，支持三种尺寸（Small、Medium、Large）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Takt.Fluent.Controls;

/// <summary>
/// 复选框尺寸枚举
/// </summary>
public enum CheckBoxSize
{
    /// <summary>
    /// 小尺寸：字体13px，内边距上下6px，左右8px
    /// </summary>
    Small,
    
    /// <summary>
    /// 中等尺寸：字体14px，内边距上下8px，左右10px（默认）
    /// </summary>
    Medium,
    
    /// <summary>
    /// 大尺寸：字体14px，内边距上下10px，左右12px
    /// </summary>
    Large
}

/// <summary>
/// 自定义复选框控件
/// </summary>
public partial class TaktCheckBox : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktCheckBox.xaml", UriKind.Relative);

    #region 依赖属性

    /// <summary>
    /// 尺寸属性
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(CheckBoxSize), typeof(TaktCheckBox),
            new PropertyMetadata(CheckBoxSize.Medium, OnSizeChanged));

    /// <summary>
    /// 内容属性
    /// </summary>
    public new static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(TaktCheckBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 是否选中属性
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(TaktCheckBox),
            new PropertyMetadata(false, OnIsCheckedChanged));

    /// <summary>
    /// 是否三态属性
    /// </summary>
    public static readonly DependencyProperty IsThreeStateProperty =
        DependencyProperty.Register(nameof(IsThreeState), typeof(bool), typeof(TaktCheckBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TaktCheckBox),
            new PropertyMetadata(true));

    /// <summary>
    /// 命令属性
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TaktCheckBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 命令参数属性
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(TaktCheckBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 工具提示属性
    /// </summary>
    public new static readonly DependencyProperty ToolTipProperty =
        DependencyProperty.Register(nameof(ToolTip), typeof(object), typeof(TaktCheckBox),
            new PropertyMetadata(null));

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取或设置尺寸
    /// </summary>
    public CheckBoxSize Size
    {
        get => (CheckBoxSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// 获取或设置内容
    /// </summary>
    public new object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// 获取或设置是否选中
    /// </summary>
    public bool? IsChecked
    {
        get => (bool?)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// 获取或设置是否三态
    /// </summary>
    public bool IsThreeState
    {
        get => (bool)GetValue(IsThreeStateProperty);
        set => SetValue(IsThreeStateProperty, value);
    }

    /// <summary>
    /// 获取或设置是否启用
    /// </summary>
    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// 获取或设置命令
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// 获取或设置命令参数
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// 获取或设置工具提示
    /// </summary>
    public new object? ToolTip
    {
        get => GetValue(ToolTipProperty);
        set => SetValue(ToolTipProperty, value);
    }

    #endregion

    #region 构造函数

    public TaktCheckBox()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        Loaded += TaktCheckBox_Loaded;
        UpdateStyle();
    }
    
    private void TaktCheckBox_Loaded(object sender, RoutedEventArgs e)
    {
        // 在 Loaded 事件中再次更新样式，确保 XAML 属性绑定已完成
        UpdateStyle();
    }
    
    private void UpdateStyle()
    {
        var innerCheckBox = FindName("InnerCheckBox") as CheckBox;
        if (innerCheckBox == null) return;
        
        var styleKey = Size switch
        {
            CheckBoxSize.Small => "SmallCheckBoxStyle",
            CheckBoxSize.Medium => "MediumCheckBoxStyle",
            CheckBoxSize.Large => "LargeCheckBoxStyle",
            _ => "MediumCheckBoxStyle"
        };
        
        innerCheckBox.Style = (Style)Resources[styleKey];
    }

    #endregion

    #region 事件

    /// <summary>
    /// 选中事件
    /// </summary>
    public event RoutedEventHandler? Checked;

    /// <summary>
    /// 取消选中事件
    /// </summary>
    public event RoutedEventHandler? Unchecked;

    #endregion

    #region 事件处理

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktCheckBox control)
        {
            control.UpdateStyle();
        }
    }
    
    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 可以在这里添加选中状态改变的逻辑
    }

    private void InnerCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Checked?.Invoke(this, e);
    }

    private void InnerCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        Unchecked?.Invoke(this, e);
    }

    #endregion
}

