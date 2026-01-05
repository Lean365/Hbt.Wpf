//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : SubtractValueConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 数值减法转换器（用于从宽度中减去边距）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 数值减法转换器
/// 从输入值中减去指定的数值
/// </summary>
public class SubtractValueConverter : IValueConverter
{
    public double Subtract { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return Math.Max(0, doubleValue - Subtract);
        }
        if (value is int intValue)
        {
            return Math.Max(0, (double)intValue - Subtract);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

