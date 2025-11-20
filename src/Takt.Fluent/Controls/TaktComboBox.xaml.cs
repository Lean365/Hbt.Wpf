// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktComboBox.xaml.cs
// 创建时间：2025-01-XX
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义下拉框控件，支持三种尺寸（Small、Medium、Large）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Takt.Fluent.Controls;

/// <summary>
/// 下拉框尺寸枚举
/// </summary>
public enum ComboBoxSize
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
/// 自定义下拉框控件
/// </summary>
public partial class TaktComboBox : UserControl
{
    private static readonly Uri resourceLocator = new("/Takt.Fluent;component/Controls/TaktComboBox.xaml", UriKind.Relative);

    #region 依赖属性

    /// <summary>
    /// 尺寸属性
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(ComboBoxSize), typeof(TaktComboBox),
            new PropertyMetadata(ComboBoxSize.Medium, OnSizeChanged));

    /// <summary>
    /// 数据源属性
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TaktComboBox),
            new PropertyMetadata(null, OnItemsSourceChanged));

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktComboBox control)
        {
            control.ApplyFilter();
        }
    }

    /// <summary>
    /// 选中项属性
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TaktComboBox),
            new PropertyMetadata(null, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktComboBox control)
        {
            control.UpdateMultiSelectText();
        }
    }

    /// <summary>
    /// 选中值属性
    /// </summary>
    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(nameof(SelectedValue), typeof(object), typeof(TaktComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 选中值路径属性
    /// </summary>
    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(TaktComboBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 显示成员路径属性
    /// </summary>
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(TaktComboBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 是否可编辑属性
    /// </summary>
    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(TaktComboBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否只读属性
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(TaktComboBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否启用属性
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TaktComboBox),
            new PropertyMetadata(true));

    /// <summary>
    /// 项模板属性
    /// </summary>
    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(TaktComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 项容器样式属性
    /// </summary>
    public static readonly DependencyProperty ItemContainerStyleProperty =
        DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(TaktComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 数据加载命令属性
    /// </summary>
    public static readonly DependencyProperty LoadDataCommandProperty =
        DependencyProperty.Register(nameof(LoadDataCommand), typeof(ICommand), typeof(TaktComboBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 是否正在加载属性
    /// </summary>
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(TaktComboBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否启用虚拟化属性
    /// </summary>
    public static readonly DependencyProperty IsVirtualizingProperty =
        DependencyProperty.Register(nameof(IsVirtualizing), typeof(bool), typeof(TaktComboBox),
            new PropertyMetadata(true, OnIsVirtualizingChanged));

    /// <summary>
    /// 选择模式属性（true=多选，false=单选，默认false）
    /// </summary>
    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(bool), typeof(TaktComboBox),
            new PropertyMetadata(false, OnSelectionModeChanged));

    /// <summary>
    /// 过滤文本属性
    /// </summary>
    public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(TaktComboBox),
            new PropertyMetadata(string.Empty, OnFilterTextChanged));

    /// <summary>
    /// 过滤后的数据源属性（只读）
    /// </summary>
    private static readonly DependencyPropertyKey FilteredItemsSourcePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FilteredItemsSource), typeof(IEnumerable), typeof(TaktComboBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FilteredItemsSourceProperty = FilteredItemsSourcePropertyKey.DependencyProperty;

    /// <summary>
    /// 选中项集合属性（多选时使用）
    /// </summary>
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(TaktComboBox),
            new PropertyMetadata(null, OnSelectedItemsChanged));

    /// <summary>
    /// 是否有错误属性（用于显示红色边框）
    /// </summary>
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(TaktComboBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 提示文本属性
    /// </summary>
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(TaktComboBox),
            new PropertyMetadata(string.Empty));

    private static void OnIsVirtualizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktComboBox control)
        {
            control.UpdateVirtualization();
        }
    }

    private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktComboBox control)
        {
            control.UpdateStyle();
            control.UpdateSelectionMode();
            control.UpdateMultiSelectText();
        }
    }

    private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktComboBox control)
        {
            control.ApplyFilter();
        }
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktComboBox control)
        {
            control.UpdateMultiSelectText();
            // 同步到 ListBox
            var listBox = control.FindName("MultiSelectListBox") as ListBox;
            if (listBox != null && control.SelectedItems != null)
            {
                listBox.SelectedItems.Clear();
                foreach (var item in control.SelectedItems)
                {
                    listBox.SelectedItems.Add(item);
                }
            }
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
    /// 获取或设置选中项
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// 获取或设置选中值
    /// </summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>
    /// 获取或设置选中值路径
    /// </summary>
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
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
    /// 获取或设置选择模式（true=多选，false=单选，默认false）
    /// </summary>
    public bool SelectionMode
    {
        get => (bool)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
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
    /// 获取或设置选中项集合（多选时使用）
    /// </summary>
    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
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
    /// 获取或设置提示文本
    /// </summary>
    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
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

    private IList? _selectedItemsInternal;

    public TaktComboBox()
    {
        System.Windows.Application.LoadComponent(this, resourceLocator);
        _selectedItemsInternal = new ObservableCollection<object>();
        SelectedItems = _selectedItemsInternal;
        UpdateStyle();
        UpdateVirtualization();
        UpdateSelectionMode();
        ApplyFilter();
    }
    
    private void UpdateStyle()
    {
        var singleSelectTextBox = FindName("SingleSelectTextBox") as TextBox;
        var multiSelectTextBox = FindName("MultiSelectTextBox") as TextBox;
        
        var styleKey = Size switch
        {
            ComboBoxSize.Small => "SmallTextBoxStyle",
            ComboBoxSize.Medium => "MediumTextBoxStyle",
            ComboBoxSize.Large => "LargeTextBoxStyle",
            _ => "MediumTextBoxStyle"
        };
        
        if (singleSelectTextBox != null)
        {
            singleSelectTextBox.Style = (Style)Resources[styleKey];
        }
        
        if (multiSelectTextBox != null)
        {
            multiSelectTextBox.Style = (Style)Resources[styleKey];
        }
    }
    
    private void UpdateVirtualization()
    {
        var singleSelectListBox = FindName("SingleSelectListBox") as ListBox;
        if (singleSelectListBox != null)
        {
            VirtualizingPanel.SetIsVirtualizing(singleSelectListBox, IsVirtualizing);
            if (IsVirtualizing)
            {
                VirtualizingPanel.SetVirtualizationMode(singleSelectListBox, VirtualizationMode.Recycling);
                ScrollViewer.SetCanContentScroll(singleSelectListBox, true);
            }
        }

        var multiSelectListBox = FindName("MultiSelectListBox") as ListBox;
        if (multiSelectListBox != null)
        {
            VirtualizingPanel.SetIsVirtualizing(multiSelectListBox, IsVirtualizing);
            if (IsVirtualizing)
            {
                VirtualizingPanel.SetVirtualizationMode(multiSelectListBox, VirtualizationMode.Recycling);
                ScrollViewer.SetCanContentScroll(multiSelectListBox, true);
            }
        }
    }

    private void UpdateSelectionMode()
    {
        var singleSelectListBox = FindName("SingleSelectListBox") as ListBox;
        if (singleSelectListBox != null)
        {
            singleSelectListBox.SelectionMode = System.Windows.Controls.SelectionMode.Single;
        }

        var multiSelectListBox = FindName("MultiSelectListBox") as ListBox;
        if (multiSelectListBox != null)
        {
            multiSelectListBox.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;
        }
    }

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

        var filterText = FilterText.ToLowerInvariant();
        var filtered = new List<object>();

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;

            string? displayText = null;

            // 如果有 DisplayMemberPath，使用它
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

            if (displayText != null && displayText.ToLowerInvariant().Contains(filterText))
            {
                filtered.Add(item);
            }
        }

        FilteredItemsSource = filtered;
    }

    private void UpdateMultiSelectText()
    {
        if (SelectionMode)
        {
            // 多选模式
            var multiSelectTextBox = FindName("MultiSelectTextBox") as TextBox;
            if (multiSelectTextBox == null) return;

            if (SelectedItems == null || SelectedItems.Count == 0)
            {
                multiSelectTextBox.Text = string.Empty;
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

            multiSelectTextBox.Text = string.Join(", ", texts);
        }
        else
        {
            // 单选模式
            var singleSelectTextBox = FindName("SingleSelectTextBox") as TextBox;
            if (singleSelectTextBox == null) return;

            if (SelectedItem == null)
            {
                singleSelectTextBox.Text = string.Empty;
                return;
            }

            singleSelectTextBox.Text = GetDisplayText(SelectedItem) ?? string.Empty;
        }
    }

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

    #endregion

    #region 事件处理

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktComboBox control)
        {
            control.UpdateStyle();
        }
    }
    
    private void SingleSelectTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var popup = FindName("SingleSelectPopup") as Popup;
        if (popup != null)
        {
            popup.IsOpen = !popup.IsOpen;
            e.Handled = true;
        }
    }

    private void SingleSelectTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var popup = FindName("SingleSelectPopup") as Popup;
        if (popup != null)
        {
            popup.IsOpen = true;
        }
    }

    private void SingleSelectPopup_Opened(object sender, EventArgs e)
    {
        // 弹出框打开时，如果数据源为空且未在加载中，触发数据加载
        if (ItemsSource == null && !IsLoading)
        {
            LoadData();
        }

        // 聚焦到搜索框
        var filterTextBox = FindName("SingleFilterTextBox") as TextBox;
        if (filterTextBox != null)
        {
            filterTextBox.Focus();
            filterTextBox.SelectAll();
        }
    }

    private void SingleSelectPopup_Closed(object sender, EventArgs e)
    {
        // 关闭时清空过滤文本
        FilterText = string.Empty;
    }

    private void SingleSelectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectionMode) return;

        UpdateMultiSelectText();
        
        // 选择后关闭弹出框
        var popup = FindName("SingleSelectPopup") as Popup;
        if (popup != null && SelectedItem != null)
        {
            popup.IsOpen = false;
        }
        
        SelectionChanged?.Invoke(this, e);
    }

    private void MultiSelectTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var popup = FindName("MultiSelectPopup") as Popup;
        if (popup != null)
        {
            popup.IsOpen = !popup.IsOpen;
            e.Handled = true;
        }
    }

    private void MultiSelectTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var popup = FindName("MultiSelectPopup") as Popup;
        if (popup != null)
        {
            popup.IsOpen = true;
        }
    }

    private void MultiSelectPopup_Opened(object sender, EventArgs e)
    {
        // 弹出框打开时，如果数据源为空且未在加载中，触发数据加载
        if (ItemsSource == null && !IsLoading)
        {
            LoadData();
        }

        // 聚焦到搜索框
        var filterTextBox = FindName("MultiFilterTextBox") as TextBox;
        if (filterTextBox != null)
        {
            filterTextBox.Focus();
            filterTextBox.SelectAll();
        }
    }

    private void MultiSelectPopup_Closed(object sender, EventArgs e)
    {
        // 关闭时清空过滤文本
        FilterText = string.Empty;
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void MultiSelectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!SelectionMode) return;

        var listBox = sender as ListBox;
        if (listBox == null || _selectedItemsInternal == null) return;

        // 更新 SelectedItems
        _selectedItemsInternal.Clear();
        foreach (var item in listBox.SelectedItems)
        {
            _selectedItemsInternal.Add(item);
        }

        UpdateMultiSelectText();
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

