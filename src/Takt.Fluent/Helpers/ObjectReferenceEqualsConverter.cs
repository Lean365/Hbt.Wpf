// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：ObjectReferenceEqualsConverter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：对象引用相等转换器（用于比较两个对象引用是否相等）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 对象引用相等转换器
/// 用于比较两个对象引用是否相等（使用 ReferenceEquals）
/// 支持 IValueConverter 和 IMultiValueConverter
/// </summary>
public class ObjectReferenceEqualsConverter : IValueConverter, IMultiValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // value 是当前行的 DataContext（即 GenColumnDto）
        // parameter 是 EditingColumn（通过 ConverterParameter 传递）
        if (value == null && parameter == null)
        {
            return true;
        }
        
        if (value == null || parameter == null)
        {
            return false;
        }
        
        // 使用 ReferenceEquals 比较对象引用
        return ReferenceEquals(value, parameter);
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] 是当前行的 DataContext（即 GenColumnDto）
        // values[1] 是 EditingColumn
        if (values == null || values.Length < 2)
        {
            return false;
        }
        
        var currentItem = values[0];
        var editingItem = values[1];
        
        if (currentItem == null && editingItem == null)
        {
            return true;
        }
        
        if (currentItem == null || editingItem == null)
        {
            return false;
        }
        
        // 使用 ReferenceEquals 比较对象引用
        return ReferenceEquals(currentItem, editingItem);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new[] { Binding.DoNothing };
    }
}

