//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : StringToBoolConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-XX
// 版本号 : 0.0.1
// 描述    : 字符串到布尔值转换器（非空字符串转换为 true）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 字符串到布尔值转换器
/// 将非空字符串转换为 true，空字符串转换为 false
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

