//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : StringToIconCharConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-30
// 版本号 : 0.0.1
// 描述    : 将字符串图标键转换为 FontAwesome.Sharp IconChar 的转换器
//===================================================================

using System;
using System.Globalization;
using System.Windows.Data;
using FontAwesome.Sharp;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 将字符串（如 "House"、"Lock"）转换为 IconChar 的值转换器
/// </summary>
public class StringToIconCharConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            if (Enum.TryParse<IconChar>(s, true, out var icon))
            {
                return icon;
            }
        }
        return IconChar.Question; // 兜底问号图标
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() ?? string.Empty;
    }
}


