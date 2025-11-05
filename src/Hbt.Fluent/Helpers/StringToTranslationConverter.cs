//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : StringToTranslationConverter.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 将 I18nKey 字符串转换为翻译文本的转换器（支持语言切换）
//===================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Hbt.Fluent.Services;

namespace Hbt.Fluent.Helpers;

/// <summary>
/// 将 I18nKey（如 "menu.dashboard"）转换为翻译文本的值转换器
/// 支持动态语言切换，通过绑定到 LanguageService 的 CurrentLanguageCode 来响应语言变化
/// </summary>
public class StringToTranslationConverter : MarkupExtension, IValueConverter, IMultiValueConverter
{
    private static LanguageService? GetLanguageService()
    {
        return App.Services?.GetService(typeof(LanguageService)) as LanguageService;
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

        var languageService = GetLanguageService();
        if (languageService == null)
        {
            // 如果 LanguageService 还未初始化，返回键本身
            // 但创建一个绑定，以便在 LanguageService 初始化后能够更新
            return key;
        }

        // 返回翻译后的文本
        // 如果找不到翻译，GetTranslation 会返回 defaultValue（即 key）
        // 但我们应该优先使用 MenuName 作为后备
        var translation = languageService.GetTranslation(key, null);
        
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
        // 多值转换：支持同时绑定 I18nKey 和 MenuName（作为后备）
        if (values == null || values.Length == 0)
        {
            return string.Empty;
        }

        var key = values[0] as string;
        var menuName = values.Length > 1 ? values[1] as string : null;

        if (string.IsNullOrWhiteSpace(key))
        {
            return menuName ?? string.Empty;
        }

        var languageService = GetLanguageService();
        if (languageService == null)
        {
            return menuName ?? key;
        }

        var translation = languageService.GetTranslation(key, null);
        
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

