// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktTransfer.xaml.cs
// 创建时间：2025-11-13
// 创建人：Takt365(Cursor AI)
// 功能描述：自定义穿梭框控件
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Takt.Fluent.Controls;

/// <summary>
/// 自定义穿梭框控件
/// 用于在两个列表之间移动项目
/// </summary>
public partial class TaktTransfer : UserControl
{
    public static readonly DependencyProperty LeftItemsSourceProperty =
        DependencyProperty.Register(nameof(LeftItemsSource), typeof(IEnumerable), typeof(TaktTransfer),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty RightItemsSourceProperty =
        DependencyProperty.Register(nameof(RightItemsSource), typeof(IEnumerable), typeof(TaktTransfer),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(TaktTransfer),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(TaktTransfer),
            new PropertyMetadata(null));

    public static readonly DependencyProperty LeftTitleProperty =
        DependencyProperty.Register(nameof(LeftTitle), typeof(string), typeof(TaktTransfer),
            new PropertyMetadata("未分配"));

    public static readonly DependencyProperty RightTitleProperty =
        DependencyProperty.Register(nameof(RightTitle), typeof(string), typeof(TaktTransfer),
            new PropertyMetadata("已分配"));

    public static readonly DependencyProperty MoveRightToolTipProperty =
        DependencyProperty.Register(nameof(MoveRightToolTip), typeof(string), typeof(TaktTransfer),
            new PropertyMetadata("添加到已分配"));

    public static readonly DependencyProperty MoveAllRightToolTipProperty =
        DependencyProperty.Register(nameof(MoveAllRightToolTip), typeof(string), typeof(TaktTransfer),
            new PropertyMetadata("全部添加到已分配"));

    public static readonly DependencyProperty MoveLeftToolTipProperty =
        DependencyProperty.Register(nameof(MoveLeftToolTip), typeof(string), typeof(TaktTransfer),
            new PropertyMetadata("从已分配移除"));

    public static readonly DependencyProperty MoveAllLeftToolTipProperty =
        DependencyProperty.Register(nameof(MoveAllLeftToolTip), typeof(string), typeof(TaktTransfer),
            new PropertyMetadata("全部从已分配移除"));

    public static readonly DependencyProperty CanMoveRightProperty =
        DependencyProperty.Register(nameof(CanMoveRight), typeof(bool), typeof(TaktTransfer),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CanMoveAllRightProperty =
        DependencyProperty.Register(nameof(CanMoveAllRight), typeof(bool), typeof(TaktTransfer),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CanMoveLeftProperty =
        DependencyProperty.Register(nameof(CanMoveLeft), typeof(bool), typeof(TaktTransfer),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CanMoveAllLeftProperty =
        DependencyProperty.Register(nameof(CanMoveAllLeft), typeof(bool), typeof(TaktTransfer),
            new PropertyMetadata(false));

    public IEnumerable? LeftItemsSource
    {
        get => (IEnumerable?)GetValue(LeftItemsSourceProperty);
        set => SetValue(LeftItemsSourceProperty, value);
    }

    public IEnumerable? RightItemsSource
    {
        get => (IEnumerable?)GetValue(RightItemsSourceProperty);
        set => SetValue(RightItemsSourceProperty, value);
    }

    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public string LeftTitle
    {
        get => (string)GetValue(LeftTitleProperty);
        set => SetValue(LeftTitleProperty, value);
    }

    public string RightTitle
    {
        get => (string)GetValue(RightTitleProperty);
        set => SetValue(RightTitleProperty, value);
    }

    public string MoveRightToolTip
    {
        get => (string)GetValue(MoveRightToolTipProperty);
        set => SetValue(MoveRightToolTipProperty, value);
    }

    public string MoveAllRightToolTip
    {
        get => (string)GetValue(MoveAllRightToolTipProperty);
        set => SetValue(MoveAllRightToolTipProperty, value);
    }

    public string MoveLeftToolTip
    {
        get => (string)GetValue(MoveLeftToolTipProperty);
        set => SetValue(MoveLeftToolTipProperty, value);
    }

    public string MoveAllLeftToolTip
    {
        get => (string)GetValue(MoveAllLeftToolTipProperty);
        set => SetValue(MoveAllLeftToolTipProperty, value);
    }

    public bool CanMoveRight
    {
        get => (bool)GetValue(CanMoveRightProperty);
        set => SetValue(CanMoveRightProperty, value);
    }

    public bool CanMoveAllRight
    {
        get => (bool)GetValue(CanMoveAllRightProperty);
        set => SetValue(CanMoveAllRightProperty, value);
    }

    public bool CanMoveLeft
    {
        get => (bool)GetValue(CanMoveLeftProperty);
        set => SetValue(CanMoveLeftProperty, value);
    }

    public bool CanMoveAllLeft
    {
        get => (bool)GetValue(CanMoveAllLeftProperty);
        set => SetValue(CanMoveAllLeftProperty, value);
    }

    public TaktTransfer()
    {
        InitializeComponent();
        Loaded += TaktTransfer_Loaded;
    }

    private void TaktTransfer_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStates();
        
        if (LeftListBox != null)
        {
            LeftListBox.SelectionChanged += LeftListBox_SelectionChanged;
        }
        
        if (RightListBox != null)
        {
            RightListBox.SelectionChanged += RightListBox_SelectionChanged;
        }
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTransfer transfer)
        {
            transfer.UpdateButtonStates();
            
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= transfer.ItemsSource_CollectionChanged;
            }
            
            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += transfer.ItemsSource_CollectionChanged;
            }
        }
    }

    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateButtonStates();
    }

    private void LeftListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateButtonStates();
    }

    private void RightListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (LeftListBox == null || RightListBox == null)
        {
            return;
        }

        var leftSelectedCount = LeftListBox.SelectedItems.Count;
        var leftTotalCount = LeftListBox.Items.Count;
        var rightSelectedCount = RightListBox.SelectedItems.Count;
        var rightTotalCount = RightListBox.Items.Count;

        CanMoveRight = leftSelectedCount > 0;
        CanMoveAllRight = leftTotalCount > 0;
        CanMoveLeft = rightSelectedCount > 0;
        CanMoveAllLeft = rightTotalCount > 0;
    }

    private void MoveRightButton_Click(object sender, RoutedEventArgs e)
    {
        if (LeftListBox == null || RightListBox == null || LeftItemsSource == null || RightItemsSource == null)
        {
            return;
        }

        var selectedItems = LeftListBox.SelectedItems.Cast<object>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        MoveItems(LeftItemsSource, RightItemsSource, selectedItems);
        LeftListBox.SelectedItems.Clear();
    }

    private void MoveAllRightButton_Click(object sender, RoutedEventArgs e)
    {
        if (LeftListBox == null || RightListBox == null || LeftItemsSource == null || RightItemsSource == null)
        {
            return;
        }

        var allItems = LeftListBox.Items.Cast<object>().ToList();
        MoveItems(LeftItemsSource, RightItemsSource, allItems);
    }

    private void MoveLeftButton_Click(object sender, RoutedEventArgs e)
    {
        if (LeftListBox == null || RightListBox == null || LeftItemsSource == null || RightItemsSource == null)
        {
            return;
        }

        var selectedItems = RightListBox.SelectedItems.Cast<object>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        MoveItems(RightItemsSource, LeftItemsSource, selectedItems);
        RightListBox.SelectedItems.Clear();
    }

    private void MoveAllLeftButton_Click(object sender, RoutedEventArgs e)
    {
        if (LeftListBox == null || RightListBox == null || LeftItemsSource == null || RightItemsSource == null)
        {
            return;
        }

        var allItems = RightListBox.Items.Cast<object>().ToList();
        MoveItems(RightItemsSource, LeftItemsSource, allItems);
    }

    private static void MoveItems(IEnumerable source, IEnumerable target, IList<object> items)
    {
        if (source is IList sourceList && target is IList targetList)
        {
            foreach (var item in items)
            {
                if (sourceList.Contains(item))
                {
                    sourceList.Remove(item);
                    if (!targetList.Contains(item))
                    {
                        targetList.Add(item);
                    }
                }
            }
        }
    }
}

