// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：ObjectReferenceToVisibilityConverter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：对象引用相等转可见性转换器（用于比较两个对象引用并转换为 Visibility）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 对象引用相等转可见性转换器
/// 用于比较两个对象引用是否相等，并转换为 Visibility
/// </summary>
public class ObjectReferenceToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
        {
            return Visibility.Collapsed;
        }
        
        var currentItem = values[0];
        var editingItem = values[1];
        
        // 如果两个对象引用相等，返回 Visible，否则返回 Collapsed
        bool isEqual = ReferenceEquals(currentItem, editingItem);
        
        // 如果 parameter 是 "Inverse"，则反转结果
        if (parameter is string param && param == "Inverse")
        {
            isEqual = !isEqual;
        }
        
        return isEqual ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new[] { Binding.DoNothing };
    }
}

