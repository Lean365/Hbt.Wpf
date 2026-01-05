//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : InvertedBooleanToVisibilityConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 反转布尔值到可见性转换器（true时隐藏，false时显示）
//===================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 反转布尔值到可见性转换器
/// true 时返回 Collapsed，false 时返回 Visible
/// </summary>
public class InvertedBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        
        // 支持 Visibility 类型的反转
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Collapsed;
        }
        return false;
    }
}

