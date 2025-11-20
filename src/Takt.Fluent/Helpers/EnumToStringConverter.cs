//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : EnumToStringConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 枚举转字符串转换器
//===================================================================

using System.Globalization;
using System.Windows.Data;
using Takt.Common.Enums;

namespace Takt.Fluent.Helpers;

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

        // 处理菜单类型
        if (value is MenuTypeEnum menuType)
        {
            return menuType switch
            {
                MenuTypeEnum.Directory => "目录",
                MenuTypeEnum.Menu => "菜单",
                MenuTypeEnum.Button => "按钮",
                MenuTypeEnum.Api => "API",
                _ => menuType.ToString()
            };
        }

        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
