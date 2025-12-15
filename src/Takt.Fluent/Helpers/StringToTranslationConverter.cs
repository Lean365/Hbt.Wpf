//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : StringToTranslationConverter.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 将 I18nKey 字符串转换为翻译文本的转换器（支持语言切换）
//===================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Takt.Fluent.Services;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 将 I18nKey（如 "menu.dashboard"）转换为翻译文本的值转换器
/// 支持动态语言切换，通过绑定到 LocalizationNotifyProperty 的 CurrentLanguageCode 来响应语言变化
/// </summary>
public class StringToTranslationConverter : MarkupExtension, IValueConverter, IMultiValueConverter
{
    private static LocalizationNotifyProperty? GetLocalizationNotifyProperty()
    {
        return App.Services?.GetService(typeof(LocalizationNotifyProperty)) as LocalizationNotifyProperty;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 如果 value 是 I18nKey，使用 value；否则使用 parameter（如果提供）
        var key = value as string;
        if (string.IsNullOrWhiteSpace(key) && parameter is string paramKey)
        {
            key = paramKey;
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var notifyProperty = GetLocalizationNotifyProperty();
        if (notifyProperty == null)
        {
            // 如果 LocalizationNotifyProperty 还未初始化，返回键本身
            // 但创建一个绑定，以便在 LocalizationNotifyProperty 初始化后能够更新
            return key;
        }

        // 返回翻译后的文本
        // 如果找不到翻译，GetTranslation 会返回 defaultValue（即 key）
        // 但我们应该优先使用 MenuName 作为后备
        var translation = notifyProperty.GetTranslation(key, null);
        
        // 如果翻译结果是 key 本身（说明没找到翻译），尝试使用 MenuName
        if (translation == key && parameter is string menuName && !string.IsNullOrWhiteSpace(menuName))
        {
            return menuName;
        }
        
        return translation;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // 多值转换：支持同时绑定 I18nKey、MenuName 和 CurrentLanguageCode（用于触发更新）
        // values[0] = I18nKey
        // values[1] = MenuName (后备)
        // values[2] = CurrentLanguageCode (用于触发语言变化时的更新，可能为 null 或 DependencyProperty.UnsetValue)
        if (values == null || values.Length == 0)
        {
            return string.Empty;
        }

        // 安全地获取值，处理 null 和 DependencyProperty.UnsetValue
        var key = values.Length > 0 ? (values[0] as string) : null;
        var menuName = values.Length > 1 ? (values[1] as string) : null;
        // values[2] 是 CurrentLanguageCode，用于触发更新，不需要使用其值
        // 即使 values[2] 为 null 或 DependencyProperty.UnsetValue，也不影响转换

        if (string.IsNullOrWhiteSpace(key))
        {
            return menuName ?? string.Empty;
        }

        var notifyProperty = GetLocalizationNotifyProperty();
        if (notifyProperty == null)
        {
            // 如果 LocalizationNotifyProperty 还未初始化，返回 MenuName 或 key
            return menuName ?? key;
        }

        var translation = notifyProperty.GetTranslation(key, null);
        
        // 如果翻译结果是 key 本身（说明没找到翻译），使用 MenuName 作为后备
        if (translation == key && !string.IsNullOrWhiteSpace(menuName))
        {
            return menuName;
        }
        
        return translation;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

