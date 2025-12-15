// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：TreeViewItemLevelConverter.cs
// 创建时间：2025-12-04
// 创建人：Takt365(Cursor AI)
// 功能描述：TreeViewItem 层级深度转换器，用于计算缩进（每级2px）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;

namespace Takt.Fluent.Helpers;

/// <summary>
/// TreeViewItem 层级深度转换器
/// 计算 TreeViewItem 的层级深度，用于设置缩进
/// 父菜单项：8px，一级子菜单：8+2px，二级子菜单：8+4px，三级子菜单：8+6px
/// </summary>
public class TreeViewItemLevelConverter : IValueConverter, IMultiValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TreeViewItem item)
        {
            return CalculateMargin(item);
        }
        
        return new Thickness(8, 2, 0, 2);
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values != null && values.Length > 0 && values[0] is TreeViewItem item)
        {
            return CalculateMargin(item);
        }
        
        return new Thickness(8, 2, 0, 2);
    }

    private Thickness CalculateMargin(TreeViewItem item)
    {
        int level = 0;
        DependencyObject? parent = LogicalTreeHelper.GetParent(item);
        
        while (parent != null)
        {
            if (parent is TreeViewItem)
            {
                level++;
            }
            parent = LogicalTreeHelper.GetParent(parent);
        }
        
        // 父菜单项：8px
        // 一级子菜单：8px + 2px = 10px
        // 二级子菜单：8px + 4px = 12px
        // 三级子菜单：8px + 6px = 14px
        // 公式：8 + (level * 2)
        double leftMargin = 8 + (level * 2);
        return new Thickness(leftMargin, 2, 0, 2);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

