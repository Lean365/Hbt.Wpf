//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : ProgressWidthConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 进度条宽度转换器
//===================================================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 进度条宽度转换器
/// 将进度百分比转换为宽度（基于固定的最大宽度 200）
/// </summary>
public class ProgressWidthConverter : IValueConverter
{
    private const double MaxWidth = 200.0; // 进度条最大宽度

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double progress && parameter is string maxValueStr && double.TryParse(maxValueStr, out var maxValue))
        {
            // 计算百分比并转换为实际宽度
            var percentage = Math.Max(0, Math.Min(100, progress)) / maxValue;
            return MaxWidth * percentage;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

