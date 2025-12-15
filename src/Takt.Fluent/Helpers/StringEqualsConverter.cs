//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : StringEqualsConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-XX
// 版本号 : 0.0.1
// 描述    : 字符串相等转换器（用于RadioButton绑定）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 字符串相等转换器
/// 用于RadioButton绑定：当绑定的字符串值等于parameter时，IsChecked为true
/// 当IsChecked为true时，将值设置为parameter
/// </summary>
public class StringEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && parameter is string param)
        {
            return str == param;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string param)
        {
            return param;
        }
        return null!; // 允许返回 null，IValueConverter.ConvertBack 可以返回 null
    }
}

