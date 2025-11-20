// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine
// 文件名称：DictionaryView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典管理视图（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows.Controls;
using Takt.Fluent.Controls;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine;

public partial class DictionaryView : UserControl
{
    public DictionaryViewModel ViewModel { get; }

    public DictionaryView(DictionaryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
    }

    private void TypeDataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TaktDataGrid dataGrid)
        {
            dataGrid.SelectedItemsCountChanged += TypeDataGrid_SelectedItemsCountChanged;
        }
    }

    private void TypeDataGrid_SelectedItemsCountChanged(object? sender, int count)
    {
        // 主表选中项数量变化处理（如果需要）
    }

    private void DataDataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TaktDataGrid dataGrid)
        {
            dataGrid.SelectedItemsCountChanged += DataDataGrid_SelectedItemsCountChanged;
        }
    }

    private void DataDataGrid_SelectedItemsCountChanged(object? sender, int count)
    {
        // 子表选中项数量变化处理（如果需要）
    }
}

