//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : IntToBoolConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 整数转布尔值转换器（支持参数反转）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            // 新规则：0=是（true），1=否（false）
            // 如果参数是 "1" 或 "Inverse"，则反转逻辑（用于特殊情况）
            var paramStr = parameter?.ToString();
            if (paramStr == "1" || paramStr == "Inverse")
            {
                return intValue == 1; // 反转：1=true
            }
            // 默认：0=是=true，1=否=false
            return intValue == 0; // 0=是=true
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // 新规则：true=0（是），false=1（否）
            if (parameter?.ToString() == "1")
            {
                return boolValue ? 1 : 0; // 反转：true=1
            }
            return boolValue ? 0 : 1; // 默认：true=0（是），false=1（否）
        }
        return 1; // 默认返回 1（否）
    }
}

