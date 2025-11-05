// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：LocExtension.cs
// 创建时间：2025-10-30
// 创建人：Hbt365(Cursor AI)
// 功能描述：XAML 本地化标记扩展：{local:Loc Key=Login.Welcome}
// ========================================

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Hbt.Fluent.Services;

namespace Hbt.Fluent.Localization;

/// <summary>
/// 本地化标记扩展：在 XAML 中直接使用键
/// 例如：Text="{local:Loc Key=Login.Welcome}"
/// </summary>
public class LocExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;
    public string? Param0 { get; set; }
    public string? Param1 { get; set; }
    public string? Param0Key { get; set; }
    public string? Param1Key { get; set; }

    public LocExtension() { }

    public LocExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // 如果 Key 为空，直接返回空字符串（设计时或编译时）
        var keyValue = Key ?? string.Empty;
        if (string.IsNullOrWhiteSpace(keyValue))
        {
            return string.Empty;
        }

        // 通过 DI 获取语言服务（编译时可能为 null）
        var lang = App.Services?.GetService(typeof(LanguageService)) as LanguageService;
        if (lang == null)
        {
            // 回退为静态文本：显示键
            return keyValue;
        }

        // 绑定到语言代码变化（INotifyPropertyChanged），使用转换器输出翻译
        try
        {
            var binding = new Binding("CurrentLanguageCode")
            {
                Source = lang,
                Mode = BindingMode.OneWay,
                Converter = new LocValueConverter(lang, keyValue, Param0, Param1, Param0Key, Param1Key)
            };
            return binding.ProvideValue(serviceProvider);
        }
        catch
        {
            // 如果绑定失败（编译时），返回键本身
            return keyValue;
        }
    }
}

internal class LocValueConverter : IValueConverter
{
    private readonly LanguageService _languageService;
    private readonly string _key;
    private readonly string? _param0;
    private readonly string? _param1;
    private readonly string? _param0Key;
    private readonly string? _param1Key;

    public LocValueConverter(LanguageService languageService, string key, string? param0, string? param1, string? param0Key, string? param1Key)
    {
        _languageService = languageService;
        _key = key ?? string.Empty;
        _param0 = param0;
        _param1 = param1;
        _param0Key = param0Key;
        _param1Key = param1Key;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(_key))
        {
            return string.Empty;
        }
        if (_languageService == null)
        {
            return _key;
        }
        var baseText = _languageService.GetTranslation(_key, _key);

        // 解析参数：优先使用 *Key 再使用字面值
        string? p0 = null;
        string? p1 = null;
        if (!string.IsNullOrWhiteSpace(_param0Key))
        {
            p0 = _languageService.GetTranslation(_param0Key!, _param0Key);
        }
        else if (!string.IsNullOrWhiteSpace(_param0))
        {
            p0 = _param0;
        }

        if (!string.IsNullOrWhiteSpace(_param1Key))
        {
            p1 = _languageService.GetTranslation(_param1Key!, _param1Key);
        }
        else if (!string.IsNullOrWhiteSpace(_param1))
        {
            p1 = _param1;
        }

        try
        {
            if (p0 != null && p1 != null)
            {
                return string.Format(baseText, p0, p1);
            }
            if (p0 != null)
            {
                return string.Format(baseText, p0);
            }
            return baseText;
        }
        catch
        {
            return baseText;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
