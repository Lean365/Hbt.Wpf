// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktMultiComboBox.xaml.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义多选下拉框控件，支持三种尺寸（Small、Medium、Large）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Takt.Fluent.Helpers;

namespace Takt.Fluent.Controls;

/// <summary>
/// 自定义多选下拉框控件
/// </summary>
public partial class TaktMultiComboBox : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktMultiComboBox.xaml", UriKind.Relative);
    private IList? _selectedItemsInternal;
    private bool _isUpdatingText = false;

    #region 依赖属性

    /// <summary>
    /// 尺寸枚举
    /// </summary>
    public enum ComboBoxSize
    {
        Small,
        Medium,
        Large
    }

    /// <summary>
    /// 尺寸属性
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(ComboBoxSize), typeof(TaktMultiComboBox),
            new PropertyMetadata(ComboBoxSize.Medium, OnSizeChanged));

    /// <summary>
    /// 数据源属性
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TaktMultiComboBox),
            new PropertyMetadata(null, OnItemsSourceChanged));

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktMultiComboBox control)
        {
            control.ApplyFilter();
        }
    }

    /// <summary>
    /// 选中项集合属性
    /// </summary>
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(TaktMultiComboBox),
            new PropertyMetadata(null, OnSelectedItemsChanged));

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktMultiComboBox control)
        {
            control.UpdateDisplayText();
            control.SyncSelectedItemsToListBox();
        }
    }

    /// <summary>
    /// 显示成员路径属性
    /// </summary>
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(TaktMultiComboBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 是否可编辑属性
    /// </summary>
    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(TaktMultiComboBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TaktMultiComboBox),
            new PropertyMetadata(true));

    /// <summary>
    /// 项模板属性
    /// </summary>
    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(TaktMultiComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 项容器样式属性
    /// </summary>
    public static readonly DependencyProperty ItemContainerStyleProperty =
        DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(TaktMultiComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 数据加载命令属性
    /// </summary>
    public static readonly DependencyProperty LoadDataCommandProperty =
        DependencyProperty.Register(nameof(LoadDataCommand), typeof(ICommand), typeof(TaktMultiComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 是否正在加载属性
    /// </summary>
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(TaktMultiComboBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否启用虚拟化属性
    /// </summary>
    public static readonly DependencyProperty IsVirtualizingProperty =
        DependencyProperty.Register(nameof(IsVirtualizing), typeof(bool), typeof(TaktMultiComboBox),
            new PropertyMetadata(true, OnIsVirtualizingChanged));

    /// <summary>
    /// 过滤文本属性
    /// </summary>
    public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(TaktMultiComboBox),
            new PropertyMetadata(string.Empty, OnFilterTextChanged));

    /// <summary>
    /// 过滤后的数据源属性（只读）
    /// </summary>
    private static readonly DependencyPropertyKey FilteredItemsSourcePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FilteredItemsSource), typeof(IEnumerable), typeof(TaktMultiComboBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FilteredItemsSourceProperty = FilteredItemsSourcePropertyKey.DependencyProperty;

    /// <summary>
    /// 是否有错误属性（用于显示红色边框）
    /// </summary>
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(TaktMultiComboBox),
            new PropertyMetadata(false, OnHasErrorChanged));

    /// <summary>
    /// 错误消息文本属性（用于显示在控件下方的错误提示）
    /// </summary>
    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(nameof(HelperText), typeof(string), typeof(TaktMultiComboBox),
            new PropertyMetadata(string.Empty));

    private static void OnIsVirtualizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktMultiComboBox control)
        {
            control.UpdateVirtualization();
        }
    }

    private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktMultiComboBox control)
        {
            control.ApplyFilter();
        }
    }

    private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktMultiComboBox control)
        {
            control.UpdateValidationError();
        }
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktMultiComboBox control)
        {
            control.UpdateStyle();
        }
    }

    #endregion

    #region 属性访问器

    /// <summary>
    /// 获取或设置尺寸
    /// </summary>
    public ComboBoxSize Size
    {
        get => (ComboBoxSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// 获取或设置数据源
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// 获取或设置选中项集合
    /// </summary>
    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// 获取或设置显示成员路径
    /// </summary>
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    /// 获取或设置是否可编辑
    /// </summary>
    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
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
    /// 获取或设置项模板
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// 获取或设置项容器样式
    /// </summary>
    public Style? ItemContainerStyle
    {
        get => (Style?)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
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

    /// <summary>
    /// 获取或设置是否启用虚拟化
    /// </summary>
    public bool IsVirtualizing
    {
        get => (bool)GetValue(IsVirtualizingProperty);
        set => SetValue(IsVirtualizingProperty, value);
    }

    /// <summary>
    /// 获取或设置过滤文本
    /// </summary>
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>
    /// 获取过滤后的数据源（只读）
    /// </summary>
    public IEnumerable? FilteredItemsSource
    {
        get => (IEnumerable?)GetValue(FilteredItemsSourceProperty);
        private set => SetValue(FilteredItemsSourcePropertyKey, value);
    }

    /// <summary>
    /// 获取或设置是否有错误（用于显示红色边框）
    /// </summary>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    /// <summary>
    /// 获取或设置错误消息文本（用于显示在控件下方的错误提示）
    /// </summary>
    public string HelperText
    {
        get => (string)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    #endregion

    #region 事件

    /// <summary>
    /// 选择改变事件
    /// </summary>
    public event SelectionChangedEventHandler? SelectionChanged;

    /// <summary>
    /// 数据加载请求事件
    /// </summary>
    public event EventHandler? DataLoadRequested;

    #endregion

    #region 构造函数

    public TaktMultiComboBox()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        _selectedItemsInternal = new ObservableCollection<object>();
        SelectedItems = _selectedItemsInternal;
        UpdateStyle();
        UpdateVirtualization();
        Loaded += (s, e) => UpdateValidationError();
    }

    /// <summary>
    /// 更新样式（尺寸）
    /// </summary>
    private void UpdateStyle()
    {
        var textBox = FindName("InnerTextBox") as TextBox;
        if (textBox == null) return;

        var styleKey = Size switch
        {
            ComboBoxSize.Small => "SmallTextBoxStyle",
            ComboBoxSize.Medium => "MediumTextBoxStyle",
            ComboBoxSize.Large => "LargeTextBoxStyle",
            _ => "MediumTextBoxStyle"
        };

        var style = Resources[styleKey] as Style;
        if (style != null)
        {
            textBox.Style = style;
        }
    }

    /// <summary>
    /// 更新虚拟化设置
    /// </summary>
    private void UpdateVirtualization()
    {
        var listBox = FindName("ItemsListBox") as ListBox;
        if (listBox == null) return;

        VirtualizingPanel.SetIsVirtualizing(listBox, IsVirtualizing);
        if (IsVirtualizing)
        {
            VirtualizingPanel.SetVirtualizationMode(listBox, VirtualizationMode.Recycling);
            ScrollViewer.SetCanContentScroll(listBox, true);
        }
    }

    /// <summary>
    /// 应用过滤
    /// </summary>
    private void ApplyFilter()
    {
        if (ItemsSource == null)
        {
            FilteredItemsSource = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(FilterText))
        {
            FilteredItemsSource = ItemsSource;
            return;
        }

        var filterText = FilterText.Trim().ToLower();
        var filtered = new List<object>();

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;

            string? displayText = null;

            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                var property = item.GetType().GetProperty(DisplayMemberPath, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    var value = property.GetValue(item);
                    displayText = value?.ToString();
                }
            }
            else
            {
                displayText = item.ToString();
            }

            if (!string.IsNullOrEmpty(displayText) && displayText.ToLower().Contains(filterText))
            {
                filtered.Add(item);
            }
        }

        FilteredItemsSource = filtered;
    }

    /// <summary>
    /// 更新显示文本
    /// </summary>
    private void UpdateDisplayText()
    {
        if (_isUpdatingText) return;

        var textBox = FindName("InnerTextBox") as TextBox;
        if (textBox == null) return;

        if (IsEditable && textBox.IsFocused)
        {
            return;
        }

        if (SelectedItems == null || SelectedItems.Count == 0)
        {
            textBox.Text = string.Empty;
            return;
        }

        var texts = new List<string>();
        foreach (var item in SelectedItems)
        {
            if (item == null) continue;

            string? displayText = GetDisplayText(item);
            if (!string.IsNullOrEmpty(displayText))
            {
                texts.Add(displayText);
            }
        }

        _isUpdatingText = true;
        try
        {
            textBox.Text = string.Join(", ", texts);
        }
        finally
        {
            _isUpdatingText = false;
        }
    }

    /// <summary>
    /// 获取显示文本
    /// </summary>
    private string? GetDisplayText(object? item)
    {
        if (item == null) return null;

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var property = item.GetType().GetProperty(DisplayMemberPath, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                var value = property.GetValue(item);
                return value?.ToString();
            }
        }

        return item.ToString();
    }

    /// <summary>
    /// 同步选中项到 ListBox
    /// </summary>
    private void SyncSelectedItemsToListBox()
    {
        var listBox = FindName("ItemsListBox") as ListBox;
        if (listBox == null || SelectedItems == null) return;

        listBox.SelectedItems.Clear();
        foreach (var item in SelectedItems)
        {
            listBox.SelectedItems.Add(item);
        }
    }

    /// <summary>
    /// 更新验证错误状态
    /// </summary>
    private void UpdateValidationError()
    {
        // 防止重复调用
        if (_isUpdatingValidation) return;
        
        var textBox = FindName("InnerTextBox") as TextBox;
        if (textBox == null) return;

        // 确保控件已加载
        if (!textBox.IsLoaded)
        {
            textBox.Loaded += (s, e) => UpdateValidationError();
            return;
        }

        _isUpdatingValidation = true;

        // 使用 Dispatcher 延迟执行，确保绑定表达式完全创建
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                var bindingExpression = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);

                if (bindingExpression != null)
                {
                    // 先清除所有现有的验证错误，避免重叠
                    Validation.ClearInvalid(bindingExpression);
                    
                    // 等待一帧，确保清除操作完全生效，避免边框重叠
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // 再次清除，确保完全清除（防止其他来源的验证错误）
                            Validation.ClearInvalid(bindingExpression);
                            
                            // 强制更新布局，确保 MaterialDesign 模板的边框状态更新
                            textBox.InvalidateVisual();
                            textBox.UpdateLayout();
                            
                            // 再等待一帧，确保布局更新完成
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    // 根据 HasError 属性手动设置验证状态
                                    if (HasError)
                                    {
                                        Validation.MarkInvalid(bindingExpression, new ValidationError(new HasErrorValidationRule(), bindingExpression, HelperText ?? "验证失败", null));
                                    }
                                }
                                finally
                                {
                                    _isUpdatingValidation = false;
                                }
                            }), System.Windows.Threading.DispatcherPriority.Loaded);
                        }
                        catch
                        {
                            _isUpdatingValidation = false;
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                else
                {
                    _isUpdatingValidation = false;
                }
            }
            catch
            {
                _isUpdatingValidation = false;
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }
    
    private bool _isUpdatingValidation = false;

    #endregion

    #region 事件处理

    /// <summary>
    /// TextBox 鼠标左键按下事件
    /// </summary>
    private void InnerTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var popup = FindName("ItemsPopup") as Popup;
        if (popup != null)
        {
            popup.IsOpen = !popup.IsOpen;
            e.Handled = true;
        }
    }

    /// <summary>
    /// TextBox 获得焦点事件
    /// </summary>
    private void InnerTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var popup = FindName("ItemsPopup") as Popup;
        if (popup != null)
        {
            popup.IsOpen = true;
        }
    }

    /// <summary>
    /// TextBox 文本改变事件
    /// </summary>
    private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingText) return;

        var textBox = sender as TextBox;
        if (textBox == null) return;

        if (IsEditable)
        {
            FilterText = textBox.Text ?? string.Empty;

            var popup = FindName("ItemsPopup") as Popup;
            if (popup != null && !popup.IsOpen)
            {
                popup.IsOpen = true;
            }
        }
    }

    /// <summary>
    /// TextBox 键盘按下事件
    /// </summary>
    private void InnerTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (!IsEditable) return;

        var popup = FindName("ItemsPopup") as Popup;
        var listBox = FindName("ItemsListBox") as ListBox;

        if (popup == null || listBox == null) return;

        switch (e.Key)
        {
            case Key.Down:
                if (!popup.IsOpen)
                {
                    popup.IsOpen = true;
                }
                else
                {
                    listBox.Focus();
                    if (listBox.Items.Count > 0)
                    {
                        listBox.SelectedIndex = 0;
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }
                }
                e.Handled = true;
                break;
            case Key.Up:
                if (popup.IsOpen)
                {
                    listBox.Focus();
                    if (listBox.Items.Count > 0)
                    {
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }
                }
                e.Handled = true;
                break;
            case Key.Escape:
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                    UpdateDisplayText();
                }
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// TextBox 失去焦点事件
    /// </summary>
    private void InnerTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new System.Action(() =>
        {
            var popup = FindName("ItemsPopup") as Popup;
            if (popup != null && !popup.IsKeyboardFocusWithin && !IsKeyboardFocusWithin)
            {
                popup.IsOpen = false;
                UpdateDisplayText();
            }
        }), System.Windows.Threading.DispatcherPriority.Input);
    }

    /// <summary>
    /// Popup 打开事件
    /// </summary>
    private void ItemsPopup_Opened(object sender, EventArgs e)
    {
        if (ItemsSource == null && !IsLoading)
        {
            LoadData();
        }
    }

    /// <summary>
    /// Popup 关闭事件
    /// </summary>
    private void ItemsPopup_Closed(object sender, EventArgs e)
    {
        if (IsEditable)
        {
            FilterText = string.Empty;
            UpdateDisplayText();
        }
    }

    /// <summary>
    /// ListBox 键盘按下事件
    /// </summary>
    private void ItemsListBox_KeyDown(object sender, KeyEventArgs e)
    {
        var listBox = sender as ListBox;
        var popup = FindName("ItemsPopup") as Popup;

        if (listBox == null || popup == null) return;

        switch (e.Key)
        {
            case Key.Escape:
                popup.IsOpen = false;
                UpdateDisplayText();
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// ListBox 选择改变事件
    /// </summary>
    private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox == null || _selectedItemsInternal == null) return;

        _selectedItemsInternal.Clear();
        foreach (var item in listBox.SelectedItems)
        {
            _selectedItemsInternal.Add(item);
        }

        UpdateDisplayText();
        SelectionChanged?.Invoke(this, e);
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
            if (LoadDataCommand != null && LoadDataCommand.CanExecute(null))
            {
                LoadDataCommand.Execute(null);
            }

            DataLoadRequested?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}

