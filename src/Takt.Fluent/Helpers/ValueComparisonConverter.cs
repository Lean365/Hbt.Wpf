// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：ValueComparisonConverter.cs
// 创建时间：2025-01-27
// 创建人：Takt365(Cursor AI)
// 功能描述：值比较转换器，用于判断值是否小于指定值
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 值比较转换器
/// </summary>
public class ValueComparisonConverter : IValueConverter
{
    public string Comparison { get; set; } = "LessThan";
    public double ComparisonValue { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return false;

        double doubleValue;
        if (value is double d)
        {
            doubleValue = d;
        }
        else if (value is int i)
        {
            doubleValue = i;
        }
        else if (double.TryParse(value.ToString(), out double parsed))
        {
            doubleValue = parsed;
        }
        else
        {
            return false;
        }

        return Comparison switch
        {
            "LessThan" => doubleValue < ComparisonValue,
            "LessThanOrEqual" => doubleValue <= ComparisonValue,
            "GreaterThan" => doubleValue > ComparisonValue,
            "GreaterThanOrEqual" => doubleValue >= ComparisonValue,
            "Equal" => Math.Abs(doubleValue - ComparisonValue) < 0.01,
            _ => false
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
