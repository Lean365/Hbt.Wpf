// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Generator.CodeGenComponent
// 文件名称：ImportTableView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：导入表窗口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using Takt.Fluent.ViewModels.Generator;

namespace Takt.Fluent.Views.Generator.CodeGenComponent;

/// <summary>
/// 导入表窗口
/// </summary>
public partial class ImportTableView : Window
{
    public ImportTableViewModel ViewModel { get; }

    public ImportTableView(ImportTableViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        Owner = System.Windows.Application.Current?.MainWindow;

        Loaded += async (s, e) =>
        {
            // 居中窗口
            CenterWindow();
            // 加载表列表
            await ViewModel.LoadAsync();
        };
    }

    private void TablesDataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
    {
        e.Row.Header = e.Row.GetIndex() + 1;
    }

    private void ColumnsDataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
    {
        e.Row.Header = e.Row.GetIndex() + 1;
    }

    private void TablesDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ViewModel != null && sender is System.Windows.Controls.DataGrid dataGrid)
        {
            ViewModel.SelectedTables.Clear();
            foreach (Takt.Domain.Interfaces.TableInfo item in dataGrid.SelectedItems)
            {
                ViewModel.SelectedTables.Add(item);
            }
        }
    }

    /// <summary>
    /// 居中窗口
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            Left = Owner.Left + (Owner.ActualWidth - Width) / 2;
            Top = Owner.Top + (Owner.ActualHeight - Height) / 2;
        }
        else
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }
}

