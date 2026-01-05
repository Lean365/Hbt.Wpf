// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：RowNumberConverter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：行号转换器（用于 DataGrid 显示行号）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 行号转换器
/// 用于在 DataGrid 中显示行号（从1开始）
/// </summary>
public class RowNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataGridRow row && row.DataContext != null)
        {
            var dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;
            if (dataGrid != null)
            {
                var index = dataGrid.Items.IndexOf(row.DataContext);
                return index >= 0 ? index + 1 : 0;
            }
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

