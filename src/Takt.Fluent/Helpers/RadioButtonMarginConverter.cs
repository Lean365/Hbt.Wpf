//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : RadioButtonMarginConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-28
// 版本号 : 0.0.1
// 描述    : RadioButton 边距转换器（根据 OrderNum 设置边距）
//===================================================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Takt.Common.Models;

namespace Takt.Fluent.Helpers;

/// <summary>
/// RadioButton 边距转换器
/// 根据 OrderNum 设置边距：第一个项（OrderNum=0）无左边距，其他项有左边距
/// </summary>
public class RadioButtonMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SelectOptionModel option)
        {
            // OrderNum 为 0 或 1 的第一个项无左边距
            if (option.OrderNum == 0)
            {
                return new Thickness(0);
            }
            return new Thickness(12, 0, 0, 0);
        }
        
        // 默认返回左边距
        return new Thickness(12, 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

