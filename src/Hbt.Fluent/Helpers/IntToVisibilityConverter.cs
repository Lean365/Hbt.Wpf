//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : IntToVisibilityConverter.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 整数转可见性转换器（支持参数匹配）
//===================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hbt.Fluent.Helpers;

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter != null)
        {
            var paramValue = int.Parse(parameter.ToString() ?? "0", culture);
            return intValue == paramValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible ? 1 : 0;
        }
        return 0;
    }
}

