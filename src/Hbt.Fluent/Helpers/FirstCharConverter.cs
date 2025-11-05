//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : FirstCharConverter.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 获取字符串首字符转换器
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Hbt.Fluent.Helpers;

public class FirstCharConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return str.Substring(0, 1).ToUpperInvariant();
        }
        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

