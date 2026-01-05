// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity
// 文件名称：MenuView.xaml.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：菜单管理视图（树形视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Takt.Application.Dtos.Identity;
using Takt.Fluent.ViewModels.Identity;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.Views.Identity;

/// <summary>
/// 菜单管理视图（树形视图）
/// </summary>
public partial class MenuView : UserControl
{
    public MenuViewModel ViewModel { get; }
    
    private readonly ILocalizationManager? _localizationManager;
    
    // 查询栏显隐状态
    public bool ShowQueryArea
    {
        get => (bool)GetValue(ShowQueryAreaProperty);
        set => SetValue(ShowQueryAreaProperty, value);
    }
    
    public static readonly DependencyProperty ShowQueryAreaProperty =
        DependencyProperty.Register(nameof(ShowQueryArea), typeof(bool), typeof(MenuView),
            new PropertyMetadata(true));
    
    // 列设置面板显隐状态
    public bool IsColumnPanelOpen
    {
        get => (bool)GetValue(IsColumnPanelOpenProperty);
        set => SetValue(IsColumnPanelOpenProperty, value);
    }
    
    public static readonly DependencyProperty IsColumnPanelOpenProperty =
        DependencyProperty.Register(nameof(IsColumnPanelOpen), typeof(bool), typeof(MenuView),
            new PropertyMetadata(false));
    
    // 树形视图展开/收缩状态
    private bool _isExpanded = true;

    public MenuView(MenuViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        _localizationManager = App.Services?.GetService<ILocalizationManager>();
        
        // 在 Loaded 事件中初始化展开/收缩按钮状态
        Loaded += (s, e) => UpdateExpandCollapseButton();
    }

    /// <summary>
    /// 树形视图选中项变更事件
    /// </summary>
    private void MenuTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is MenuDto menu)
        {
            ViewModel.SelectedMenu = menu;
        }
        else
        {
            ViewModel.SelectedMenu = null;
        }
    }

    /// <summary>
    /// 树形视图项选中事件
    /// </summary>
    private void MenuTreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem treeViewItem && treeViewItem.DataContext is MenuDto menu)
        {
            ViewModel.SelectedMenu = menu;
        }
    }
    
    /// <summary>
    /// 切换查询栏显隐
    /// </summary>
    private void ToggleQueryAreaButton_Click(object sender, RoutedEventArgs e)
    {
        ShowQueryArea = !ShowQueryArea;
    }
    
    /// <summary>
    /// 切换列设置面板显隐
    /// </summary>
    private void ColumnToggleButton_Click(object sender, RoutedEventArgs e)
    {
        IsColumnPanelOpen = !IsColumnPanelOpen;
    }
    
    /// <summary>
    /// 列设置面板关闭事件
    /// </summary>
    private void ColumnChooserPopup_Closed(object sender, EventArgs e)
    {
        IsColumnPanelOpen = false;
    }
    
    /// <summary>
    /// 展开/收缩按钮点击事件
    /// </summary>
    private void ExpandCollapseButton_Click(object sender, RoutedEventArgs e)
    {
        _isExpanded = !_isExpanded;
        
        if (_isExpanded)
        {
            // 展开所有节点
            ExpandAllTreeViewItems(MenuTreeView);
        }
        else
        {
            // 收缩所有节点
            CollapseAllTreeViewItems(MenuTreeView);
        }
        
        UpdateExpandCollapseButton();
    }
    
    /// <summary>
    /// 更新展开/收缩按钮状态
    /// </summary>
    private void UpdateExpandCollapseButton()
    {
        if (ExpandCollapseIcon == null || ExpandCollapseButton == null) return;
        
        if (_isExpanded)
        {
            ExpandCollapseIcon.Kind = PackIconKind.ChevronUp;
            ExpandCollapseButton.ToolTip = _localizationManager?.GetString("common.collapseall");
        }
        else
        {
            ExpandCollapseIcon.Kind = PackIconKind.ChevronDown;
            ExpandCollapseButton.ToolTip = _localizationManager?.GetString("common.expandall");
        }
    }
    
    /// <summary>
    /// 展开所有树形视图项
    /// </summary>
    private void ExpandAllTreeViewItems(ItemsControl itemsControl)
    {
        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
            {
                treeViewItem.IsExpanded = true;
                ExpandAllTreeViewItems(treeViewItem);
            }
        }
    }
    
    /// <summary>
    /// 收缩所有树形视图项
    /// </summary>
    private void CollapseAllTreeViewItems(ItemsControl itemsControl)
    {
        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
            {
                treeViewItem.IsExpanded = false;
                CollapseAllTreeViewItems(treeViewItem);
            }
        }
    }
}

