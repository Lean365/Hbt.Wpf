// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktRadioButton.xaml.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义单选按钮控件，支持三种尺寸（Small、Medium、Large）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Takt.Fluent.Controls;

/// <summary>
/// 单选按钮尺寸枚举
/// </summary>
public enum RadioButtonSize
{
    /// <summary>
    /// 小尺寸：字体14px，内边距8px
    /// </summary>
    Small,
    
    /// <summary>
    /// 中等尺寸：字体14px，内边距10px（默认）
    /// </summary>
    Medium,
    
    /// <summary>
    /// 大尺寸：字体14px，内边距12px
    /// </summary>
    Large
}

/// <summary>
/// 自定义单选按钮控件
/// </summary>
public partial class TaktRadioButton : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktRadioButton.xaml", UriKind.Relative);

    #region 依赖属性

    /// <summary>
    /// 尺寸属性
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(RadioButtonSize), typeof(TaktRadioButton),
            new PropertyMetadata(RadioButtonSize.Medium, OnSizeChanged));

    /// <summary>
    /// 内容属性
    /// </summary>
    public new static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(TaktRadioButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 组名属性
    /// </summary>
    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(TaktRadioButton),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 是否选中属性
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(TaktRadioButton),
            new PropertyMetadata(false, OnIsCheckedChanged));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TaktRadioButton),
            new PropertyMetadata(true));

    /// <summary>
    /// 命令属性
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TaktRadioButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 命令参数属性
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(TaktRadioButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 数据加载命令属性
    /// </summary>
    public static readonly DependencyProperty LoadDataCommandProperty =
        DependencyProperty.Register(nameof(LoadDataCommand), typeof(ICommand), typeof(TaktRadioButton),
            new PropertyMetadata(null));

    /// <summary>
    /// 是否正在加载属性
    /// </summary>
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(TaktRadioButton),
            new PropertyMetadata(false));

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取或设置尺寸
    /// </summary>
    public RadioButtonSize Size
    {
        get => (RadioButtonSize)GetValue(SizeProperty);
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
    /// 获取或设置组名
    /// </summary>
    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
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
    /// 获取或设置数据加载命令
    /// </summary>
    public ICommand? LoadDataCommand
    {
        get => (ICommand?)GetValue(LoadDataCommandProperty);
        set => SetValue(LoadDataCommandProperty, value);
    }

    /// <summary>
    /// 获取或设置是否正在加载
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
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

    /// <summary>
    /// 数据加载请求事件
    /// </summary>
    public event EventHandler? DataLoadRequested;

    #endregion

    #region 构造函数

    public TaktRadioButton()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        UpdateStyle();
    }
    
    private void UpdateStyle()
    {
        var innerRadioButton = FindName("InnerRadioButton") as RadioButton;
        if (innerRadioButton == null) return;
        
        var styleKey = Size switch
        {
            RadioButtonSize.Small => "SmallRadioButtonStyle",
            RadioButtonSize.Medium => "MediumRadioButtonStyle",
            RadioButtonSize.Large => "LargeRadioButtonStyle",
            _ => "MediumRadioButtonStyle"
        };
        
        innerRadioButton.Style = (Style)Resources[styleKey];
    }

    #endregion

    #region 事件处理

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktRadioButton control)
        {
            control.UpdateStyle();
        }
    }
    
    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktRadioButton control)
        {
            var innerRadioButton = control.FindName("InnerRadioButton") as RadioButton;
            if (innerRadioButton != null)
            {
                innerRadioButton.IsChecked = e.NewValue as bool?;
            }
        }
    }
    
    private void InnerRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        IsChecked = true;
        Checked?.Invoke(this, e);
    }
    
    private void InnerRadioButton_Unchecked(object sender, RoutedEventArgs e)
    {
        IsChecked = false;
        Unchecked?.Invoke(this, e);
    }
    
    private void InnerRadioButton_Loaded(object sender, RoutedEventArgs e)
    {
        // 当控件加载时，如果内容为空且未在加载中，触发数据加载
        if (Content == null && !IsLoading)
        {
            LoadData();
        }
    }
    
    /// <summary>
    /// 加载数据
    /// </summary>
    public void LoadData()
    {
        if (IsLoading) return;
        
        IsLoading = true;
        
        try
        {
            // 执行命令
            if (LoadDataCommand != null && LoadDataCommand.CanExecute(null))
            {
                LoadDataCommand.Execute(null);
            }
            
            // 触发事件
            DataLoadRequested?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}

