//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : IntToBoolConverter.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 整数转布尔值转换器（支持参数反转）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Hbt.Fluent.Helpers;

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            // 如果参数是 "1"，则反转逻辑（1=只读=true）
            if (parameter?.ToString() == "1")
            {
                return intValue == 1; // 1=否（不可编辑）=只读=true
            }
            // 默认：0=是（可编辑）=false，1=否（不可编辑）=true
            return intValue == 1; // 1=否（不可编辑）=只读=true
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (parameter?.ToString() == "1")
            {
                return boolValue ? 1 : 0;
            }
            return boolValue ? 1 : 0;
        }
        return 0;
    }
}

