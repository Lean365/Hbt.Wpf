//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : MultiplyValueConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-21
// 版本号 : 0.0.1
// 描述    : 数值乘法转换器（用于计算百分比高度）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 数值乘法转换器
/// 将输入值乘以指定的倍数（通常用于计算百分比）
/// </summary>
public class MultiplyValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            // parameter 可以是字符串形式的倍数，如 "0.8" 表示 80%
            if (parameter is string strParam && double.TryParse(strParam, NumberStyles.Float, culture, out var multiplier))
            {
                return doubleValue * multiplier;
            }
            // 如果 parameter 是数字类型
            if (parameter is double doubleParam)
            {
                return doubleValue * doubleParam;
            }
            if (parameter is int intParam)
            {
                return doubleValue * intParam;
            }
        }
        if (value is int intValue)
        {
            if (parameter is string strParam && double.TryParse(strParam, NumberStyles.Float, culture, out var multiplier))
            {
                return (double)intValue * multiplier;
            }
            if (parameter is double doubleParam)
            {
                return (double)intValue * doubleParam;
            }
            if (parameter is int intParam)
            {
                return intValue * intParam;
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
