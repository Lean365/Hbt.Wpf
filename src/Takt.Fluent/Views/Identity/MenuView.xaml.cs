// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
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
using Takt.Application.Dtos.Identity;
using Takt.Fluent.ViewModels.Identity;

namespace Takt.Fluent.Views.Identity;

/// <summary>
/// 菜单管理视图（树形视图）
/// </summary>
public partial class MenuView : UserControl
{
    public MenuViewModel ViewModel { get; }

    public MenuView(MenuViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
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
}

