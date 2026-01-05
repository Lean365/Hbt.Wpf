// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktToggleButton.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义切换按钮控件，支持三种尺寸（Small、Medium、Large）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Takt.Fluent.Controls;

/// <summary>
/// 切换按钮尺寸枚举
/// </summary>
public enum ToggleButtonSize
{
    /// <summary>
    /// 小尺寸：高度32px，内边距上下6px，左右12px
    /// </summary>
    Small,
    
    /// <summary>
    /// 中等尺寸：高度36px，内边距上下10px，左右16px（默认）
    /// </summary>
    Medium,
    
    /// <summary>
    /// 大尺寸：高度40px，内边距上下12px，左右20px
    /// </summary>
    Large
}

/// <summary>
/// 自定义切换按钮控件
/// </summary>
public partial class TaktToggleButton : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktToggleButton.xaml", UriKind.Relative);

    #region 依赖属性

    /// <summary>
    /// 尺寸属性
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(ToggleButtonSize), typeof(TaktToggleButton),
            new PropertyMetadata(ToggleButtonSize.Medium, OnSizeChanged));

    /// <summary>
    /// 内容属性
    /// </summary>
    public new static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(TaktToggleButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 是否选中属性
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(TaktToggleButton),
            new PropertyMetadata(false, OnIsCheckedChanged));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TaktToggleButton),
            new PropertyMetadata(true));

    /// <summary>
    /// 命令属性
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TaktToggleButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 命令参数属性
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(TaktToggleButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 工具提示属性
    /// </summary>
    public new static readonly DependencyProperty ToolTipProperty =
        DependencyProperty.Register(nameof(ToolTip), typeof(object), typeof(TaktToggleButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 左侧文字标签属性（未选中状态）
    /// </summary>
    public static readonly DependencyProperty OffLabelProperty =
        DependencyProperty.Register(nameof(OffLabel), typeof(string), typeof(TaktToggleButton),
            new PropertyMetadata("关闭"));

    /// <summary>
    /// 右侧文字标签属性（选中状态）
    /// </summary>
    public static readonly DependencyProperty OnLabelProperty =
        DependencyProperty.Register(nameof(OnLabel), typeof(string), typeof(TaktToggleButton),
            new PropertyMetadata("开启"));

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取或设置尺寸
    /// </summary>
    public ToggleButtonSize Size
    {
        get => (ToggleButtonSize)GetValue(SizeProperty);
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

    /// <summary>
    /// 获取或设置左侧文字标签（未选中状态）
    /// </summary>
    public string OffLabel
    {
        get => (string)GetValue(OffLabelProperty);
        set => SetValue(OffLabelProperty, value);
    }

    /// <summary>
    /// 获取或设置右侧文字标签（选中状态）
    /// </summary>
    public string OnLabel
    {
        get => (string)GetValue(OnLabelProperty);
        set => SetValue(OnLabelProperty, value);
    }

    #endregion

    #region 构造函数

    public TaktToggleButton()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        Loaded += TaktToggleButton_Loaded;
        UpdateStyle();
    }
    
    private void TaktToggleButton_Loaded(object sender, RoutedEventArgs e)
    {
        // 在 Loaded 事件中再次更新样式，确保 XAML 属性绑定已完成
        UpdateStyle();
    }
    
    private void UpdateStyle()
    {
        var innerToggleButton = FindName("InnerToggleButton") as ToggleButton;
        if (innerToggleButton == null) return;
        
        var styleKey = Size switch
        {
            ToggleButtonSize.Small => "SmallToggleButtonStyle",
            ToggleButtonSize.Medium => "MediumToggleButtonStyle",
            ToggleButtonSize.Large => "LargeToggleButtonStyle",
            _ => "MediumToggleButtonStyle"
        };
        
        innerToggleButton.Style = (Style)Resources[styleKey];
    }

    #endregion

    #region 事件处理

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktToggleButton control)
        {
            control.UpdateStyle();
        }
    }
    
    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 可以在这里添加选中状态改变的逻辑
    }

    #endregion
}

