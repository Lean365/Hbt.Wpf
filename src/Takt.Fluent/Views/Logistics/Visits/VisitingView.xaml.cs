// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Logistics.Visits
// 文件名称：VisitingView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：来访公司视图代码后台
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows.Controls;
using Takt.Fluent.Controls;
using Takt.Fluent.ViewModels.Logistics.Visits;

namespace Takt.Fluent.Views.Logistics.Visits;

public partial class VisitingView : UserControl
{
    public VisitingViewModel ViewModel { get; }

    public VisitingView(VisitingViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
    }

    private void VisitingCompanyDataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TaktDataGrid dataGrid)
        {
            dataGrid.SelectedItemsCountChanged += VisitingCompanyDataGrid_SelectedItemsCountChanged;
        }
    }

    private void VisitingCompanyDataGrid_SelectedItemsCountChanged(object? sender, int count)
    {
        // 可以在这里处理选中项数量变化
    }

    private void VisitingEntourageDataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TaktDataGrid dataGrid)
        {
            dataGrid.SelectedItemsCountChanged += VisitingEntourageDataGrid_SelectedItemsCountChanged;
        }
    }

    private void VisitingEntourageDataGrid_SelectedItemsCountChanged(object? sender, int count)
    {
        // 可以在这里处理选中项数量变化
    }
}

