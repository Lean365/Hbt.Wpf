//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : EnumToStringConverter.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 枚举转字符串转换器
//===================================================================

using System.Globalization;
using System.Windows.Data;
using Hbt.Common.Enums;

namespace Hbt.Fluent.Helpers;

/// <summary>
/// 枚举转字符串转换器
/// 将枚举值转换为本地化的显示文本
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        // 处理用户类型
        if (value is UserTypeEnum userType)
        {
            return userType == UserTypeEnum.System 
                ? "系统用户" 
                : "普通用户";
        }

        // 处理用户性别
        if (value is UserGenderEnum userGender)
        {
            return userGender switch
            {
                UserGenderEnum.Male => "男",
                UserGenderEnum.Female => "女",
                _ => "未知"
            };
        }

        // 处理状态
        if (value is StatusEnum status)
        {
            return status == StatusEnum.Normal ? "正常" : "禁用";
        }

        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
