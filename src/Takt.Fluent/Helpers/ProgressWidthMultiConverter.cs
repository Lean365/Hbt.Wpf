//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : ProgressWidthMultiConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-27
// 版本号 : 0.0.1
// 描述    : 进度条宽度多值转换器，基于父容器实际宽度计算
//===================================================================

using System;
using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 进度条宽度多值转换器
/// 将进度百分比和父容器宽度转换为实际宽度
/// </summary>
public class ProgressWidthMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return 0.0;
        
        // values[0] = 进度百分比 (double)
        // values[1] = 父容器实际宽度 (double)
        // parameter = 最大百分比值 (通常是 "100")
        
        if (values[0] is double progress && values[1] is double containerWidth)
        {
            double maxValue = 100.0;
            if (parameter is string maxValueStr && double.TryParse(maxValueStr, out var parsedMax))
            {
                maxValue = parsedMax;
            }
            
            // 计算百分比并转换为实际宽度
            var percentage = Math.Max(0, Math.Min(maxValue, progress)) / maxValue;
            return Math.Max(0, containerWidth * percentage);
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

