// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktTextBox.xaml.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义文本框控件，支持三种尺寸（Small、Medium、Large）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;
using Takt.Common.Logging;

namespace Takt.Fluent.Controls;

/// <summary>
/// 文本框尺寸枚举
/// </summary>
public enum TextBoxSize
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
/// 自定义文本框控件
/// </summary>
public partial class TaktTextBox : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktTextBox.xaml", UriKind.Relative);
    private static readonly OperLogManager? _operLog = App.Services?.GetService<OperLogManager>();

    #region 依赖属性

    /// <summary>
    /// 尺寸属性
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(TextBoxSize), typeof(TaktTextBox),
            new PropertyMetadata(TextBoxSize.Medium, OnSizeChanged));

    /// <summary>
    /// 文本属性
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TaktTextBox),
            new PropertyMetadata(string.Empty, OnTextChanged));

    /// <summary>
    /// 提示文本属性
    /// </summary>
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(TaktTextBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 是否只读属性
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(TaktTextBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TaktTextBox),
            new PropertyMetadata(true));

    /// <summary>
    /// 最大长度属性
    /// </summary>
    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(TaktTextBox),
            new PropertyMetadata(0));

    /// <summary>
    /// 文本换行属性
    /// </summary>
    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(TaktTextBox),
            new PropertyMetadata(TextWrapping.NoWrap));

    /// <summary>
    /// 接受回车属性
    /// </summary>
    public static readonly DependencyProperty AcceptsReturnProperty =
        DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(TaktTextBox),
            new PropertyMetadata(false, OnAcceptsReturnChanged));

    /// <summary>
    /// 垂直滚动条可见性属性
    /// </summary>
    public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
        DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(TaktTextBox),
            new PropertyMetadata(ScrollBarVisibility.Auto));

    /// <summary>
    /// 水平滚动条可见性属性
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
        DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(TaktTextBox),
            new PropertyMetadata(ScrollBarVisibility.Auto));

    /// <summary>
    /// 文本对齐方式属性
    /// </summary>
    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(TaktTextBox),
            new PropertyMetadata(TextAlignment.Left));

    /// <summary>
    /// 是否有错误属性（用于显示红色边框）
    /// </summary>
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(TaktTextBox),
            new PropertyMetadata(false));

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取或设置尺寸
    /// </summary>
    public TextBoxSize Size
    {
        get => (TextBoxSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// 获取或设置文本
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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
    /// 获取或设置是否只读
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
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
    /// 获取或设置文本换行
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// 获取或设置是否接受回车
    /// </summary>
    public bool AcceptsReturn
    {
        get => (bool)GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    /// <summary>
    /// 获取或设置垂直滚动条可见性
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// 获取或设置水平滚动条可见性
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// 获取或设置文本对齐方式
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
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

    #region 构造函数

    public TaktTextBox()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        Loaded += TaktTextBox_Loaded;
        UpdateStyle();
    }
    
    private void TaktTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        // 在 Loaded 事件中再次更新样式，确保 XAML 属性绑定已完成
        UpdateStyle();
        
        // 验证内部 TextBox 的绑定是否正确
        var innerTextBox = FindName("InnerTextBox") as TextBox;
        if (innerTextBox != null)
        {
            var binding = BindingOperations.GetBinding(innerTextBox, TextBox.TextProperty);
            if (binding != null)
            {
                _operLog?.Debug("[TaktTextBox] Loaded: 内部TextBox绑定路径={Path}, Mode={Mode}, UpdateSourceTrigger={UpdateSourceTrigger}", 
                    binding.Path?.Path, binding.Mode, binding.UpdateSourceTrigger);
            }
            else
            {
                _operLog?.Warning("[TaktTextBox] Loaded: 内部TextBox没有绑定！");
            }
            
            // 验证 TaktTextBox.Text 的绑定
            var selfBinding = BindingOperations.GetBinding(this, TextProperty);
            if (selfBinding != null)
            {
                _operLog?.Debug("[TaktTextBox] Loaded: TaktTextBox.Text绑定路径={Path}, Mode={Mode}, UpdateSourceTrigger={UpdateSourceTrigger}", 
                    selfBinding.Path?.Path, selfBinding.Mode, selfBinding.UpdateSourceTrigger);
            }
        }
    }
    
    private void UpdateStyle()
    {
        var innerTextBox = FindName("InnerTextBox") as TextBox;
        if (innerTextBox == null) return;
        
        var styleKey = Size switch
        {
            TextBoxSize.Small => "SmallTextBoxStyle",
            TextBoxSize.Medium => "MediumTextBoxStyle",
            TextBoxSize.Large => "LargeTextBoxStyle",
            _ => "MediumTextBoxStyle"
        };
        
        innerTextBox.Style = (Style)Resources[styleKey];
        
        // 如果是多行文本框，清除固定高度限制，允许自适应，并设置顶部对齐
        if (AcceptsReturn)
        {
            innerTextBox.ClearValue(TextBox.HeightProperty);
            innerTextBox.ClearValue(TextBox.MaxHeightProperty);
            innerTextBox.ClearValue(TextBox.MinHeightProperty); // 清除样式中的 MinHeight
            
            // 应用 UserControl 的 MinHeight 到内部 TextBox
            if (!double.IsNaN(MinHeight) && MinHeight > 0)
            {
                innerTextBox.MinHeight = MinHeight;
            }
            
            innerTextBox.VerticalContentAlignment = VerticalAlignment.Top;
            innerTextBox.AcceptsReturn = true;
            innerTextBox.TextWrapping = TextWrapping;
            
            // 确保 UserControl 本身也支持自适应高度
            ClearValue(HeightProperty);
            ClearValue(MaxHeightProperty);
        }
        else
        {
            innerTextBox.AcceptsReturn = false;
        }
    }

    #endregion

    #region 事件处理

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTextBox control)
        {
            var innerTextBox = control.FindName("InnerTextBox") as TextBox;
            if (innerTextBox != null)
            {
                var newText = e.NewValue?.ToString() ?? string.Empty;
                var oldText = e.OldValue?.ToString() ?? string.Empty;
                
                _operLog?.Debug("[TaktTextBox] OnTextChanged: 旧值='{OldValue}', 新值='{NewValue}', 内部TextBox当前值='{CurrentValue}'", 
                    oldText, newText, innerTextBox.Text);
                
                // 只有当值不同时才更新，避免循环更新
                // 注意：这里直接设置 innerTextBox.Text 可能会干扰双向绑定
                // 但由于这是从外部（ViewModel）到内部的更新，是必要的
                if (innerTextBox.Text != newText)
                {
                    // 使用 SetCurrentValue 来避免触发绑定更新循环
                    // 因为这是从 UserControl.Text 到内部 TextBox 的单向同步
                    innerTextBox.SetCurrentValue(TextBox.TextProperty, newText);
                    _operLog?.Debug("[TaktTextBox] OnTextChanged: 已更新内部TextBox.Text='{NewValue}'", newText);
                }
            }
        }
    }
    
    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTextBox control)
        {
            control.UpdateStyle();
        }
    }
    
    private static void OnAcceptsReturnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTextBox control)
        {
            control.UpdateStyle();
        }
    }

    #endregion
}

