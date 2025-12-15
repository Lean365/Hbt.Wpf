//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : StringToIntEqualsConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-28
// 版本号 : 0.0.1
// 描述    : 字符串转整数相等转换器（用于RadioButton绑定字典数据）
//===================================================================

using System.Globalization;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 字符串转整数相等转换器
/// 用于RadioButton绑定：当字符串值（字典的DataValue）转换为整数后等于绑定的整数时，IsChecked为true
/// 当IsChecked为true时，将绑定的整数设置为转换后的值
/// </summary>
public class StringToIntEqualsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length != 2)
            return false;

        // values[0] 是 SelectOptionModel.DataValue (字符串形式的枚举值)
        // values[1] 是 ViewModel 的 UserType/UserGender/UserStatus (整数)
        if (values[0] is string strValue && values[1] is int intValue)
        {
            if (int.TryParse(strValue, out var parsedValue))
            {
                return parsedValue == intValue;
            }
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // ConvertBack 需要返回两个值：SelectOptionModel.DataValue 和更新后的整数
        // 但 MultiBinding 的 ConvertBack 比较复杂，我们使用另一种方式
        throw new NotImplementedException("StringToIntEqualsConverter 不支持 ConvertBack，请使用 Command 或事件处理");
    }
}

