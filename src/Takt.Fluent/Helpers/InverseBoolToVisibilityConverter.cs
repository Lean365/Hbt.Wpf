//===================================================================
// 项目名 : Takt.Fluent
// 文件名 : InverseBoolToVisibilityConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 反向布尔值到可见性转换器（true -> Collapsed, false -> Visible）
//===================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 反向布尔值到可见性转换器
/// 用于将布尔值反转并转换为可见性（true -> Collapsed, false -> Visible）
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible; // 默认可见
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return false;
    }
}
