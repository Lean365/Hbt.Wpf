//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : EmptyToVisibilityConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 空值到可见性转换器（参照WPFGallery实现）
//===================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

public class EmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isEmpty = value == null || string.IsNullOrWhiteSpace(value.ToString());
        
        // 如果 parameter 为 "Invert"，则反转逻辑（用于占位符：空值显示，有值隐藏）
        if (parameter?.ToString() == "Invert")
        {
            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
        
        // 默认逻辑（用于错误消息等：空值隐藏，有值显示）
        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
