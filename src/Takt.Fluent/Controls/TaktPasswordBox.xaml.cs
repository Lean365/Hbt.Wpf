// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktPasswordBox.xaml.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义密码框控件，支持三种尺寸（Small、Medium、Large）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Takt.Fluent.Controls;

/// <summary>
/// 密码框尺寸枚举
/// </summary>
public enum PasswordBoxSize
{
    /// <summary>
    /// 小尺寸：高度32px，行高32px，内边距上下8px，左右12px
    /// </summary>
    Small,
    
    /// <summary>
    /// 中等尺寸：高度36px，行高36px，内边距上下10px，左右16px（默认）
    /// </summary>
    Medium,
    
    /// <summary>
    /// 大尺寸：高度40px，行高40px，内边距上下12px，左右20px
    /// </summary>
    Large
}

/// <summary>
/// 自定义密码框控件
/// </summary>
public partial class TaktPasswordBox : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktPasswordBox.xaml", UriKind.Relative);
    private bool _isUpdatingPassword;

    #region 依赖属性

    /// <summary>
    /// 尺寸属性
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(PasswordBoxSize), typeof(TaktPasswordBox),
            new PropertyMetadata(PasswordBoxSize.Medium, OnSizeChanged));

    /// <summary>
    /// 密码属性
    /// </summary>
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(nameof(Password), typeof(string), typeof(TaktPasswordBox),
            new PropertyMetadata(string.Empty, OnPasswordChanged));

    /// <summary>
    /// 提示文本属性
    /// </summary>
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(TaktPasswordBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TaktPasswordBox),
            new PropertyMetadata(true));

    /// <summary>
    /// 最大长度属性
    /// </summary>
    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(TaktPasswordBox),
            new PropertyMetadata(0));

    /// <summary>
    /// 是否有错误属性（用于显示红色边框）
    /// </summary>
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(TaktPasswordBox),
            new PropertyMetadata(false));

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取或设置尺寸
    /// </summary>
    public PasswordBoxSize Size
    {
        get => (PasswordBoxSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// 获取或设置密码
    /// </summary>
    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    /// <summary>
    /// 获取或设置提示文本
    /// </summary>
    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
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
    /// 获取或设置最大长度
    /// </summary>
    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>
    /// 获取或设置是否有错误（用于显示红色边框）
    /// </summary>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    #endregion

    #region 事件

    /// <summary>
    /// 密码改变事件
    /// </summary>
    public event RoutedEventHandler? PasswordChanged;

    #endregion

    #region 构造函数

    public TaktPasswordBox()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        UpdateStyle();
    }
    
    private void UpdateStyle()
    {
        var innerPasswordBox = FindName("InnerPasswordBox") as PasswordBox;
        if (innerPasswordBox == null) return;
        
        var styleKey = Size switch
        {
            PasswordBoxSize.Small => "SmallPasswordBoxStyle",
            PasswordBoxSize.Medium => "MediumPasswordBoxStyle",
            PasswordBoxSize.Large => "LargePasswordBoxStyle",
            _ => "MediumPasswordBoxStyle"
        };
        
        innerPasswordBox.Style = (Style)Resources[styleKey];
    }

    #endregion

    #region 事件处理

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktPasswordBox control && !control._isUpdatingPassword)
        {
            var innerPasswordBox = control.FindName("InnerPasswordBox") as PasswordBox;
            if (innerPasswordBox != null)
            {
                control._isUpdatingPassword = true;
                innerPasswordBox.Password = e.NewValue?.ToString() ?? string.Empty;
                control._isUpdatingPassword = false;
            }
        }
    }
    
    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktPasswordBox control)
        {
            control.UpdateStyle();
        }
    }
    

    private void InnerPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingPassword) return;

        var innerPasswordBox = FindName("InnerPasswordBox") as PasswordBox;
        if (innerPasswordBox != null)
        {
            _isUpdatingPassword = true;
            Password = innerPasswordBox.Password ?? string.Empty;
            _isUpdatingPassword = false;

            PasswordChanged?.Invoke(this, e);
        }
    }

    #endregion
}

