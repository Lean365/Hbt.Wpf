//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : IntEqualsConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 整数与布尔值互转转换器（用于单选按钮）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

public class IntEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && int.TryParse(parameter?.ToString(), out var target))
        {
            return intValue == target;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && int.TryParse(parameter?.ToString(), out var target))
        {
            return target;
        }

        return Binding.DoNothing;
    }
}


