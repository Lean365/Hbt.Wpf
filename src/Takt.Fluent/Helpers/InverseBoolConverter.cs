//===================================================================
// 项目名 : Takt.Fluent
// 文件名 : InverseBoolConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 反向布尔值转换器（将 true 转换为 false，false 转换为 true）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 反向布尔值转换器
/// 用于将布尔值反转（true -> false, false -> true）
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true; // 默认返回 true（启用）
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

