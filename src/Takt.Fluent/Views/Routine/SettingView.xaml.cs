// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine
// 文件名称：SettingView.xaml.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：系统设置管理视图
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows.Controls;
using Takt.Fluent.Controls;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine;

public partial class SettingView : UserControl
{
    public SettingViewModel ViewModel { get; }

    public SettingView(SettingViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
    }

    private void DataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TaktDataGrid dataGrid)
        {
            dataGrid.SelectedItemsCountChanged += DataGrid_SelectedItemsCountChanged;
            // 初始化选中数量
            ViewModel.SelectedItemsCount = dataGrid.SelectedItemsCount;
        }
    }

    private void DataGrid_SelectedItemsCountChanged(object? sender, int count)
    {
        if (ViewModel != null)
        {
            ViewModel.SelectedItemsCount = count;
        }
    }
}
