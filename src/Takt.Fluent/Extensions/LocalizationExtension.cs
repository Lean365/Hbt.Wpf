// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Extensions
// 文件名称：LocalizationExtension.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：XAML 本地化标记扩展：{local:Localization Key=Login.Welcome}
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using MaterialDesignThemes.Wpf;
using Takt.Fluent.Services;

namespace Takt.Fluent.Extensions;

/// <summary>
/// 本地化标记扩展：在 XAML 中直接使用键
/// 例如：Text="{local:Localization Key=Login.Welcome}"
/// </summary>
public class LocalizationExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;
    public string? Param0 { get; set; }
    public string? Param1 { get; set; }
    public string? Param0Key { get; set; }
    public string? Param1Key { get; set; }

    public LocalizationExtension() { }

    public LocalizationExtension(string key)
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

        // 通过 DI 获取本地化属性通知类（编译时可能为 null）
        var adapter = App.Services?.GetService(typeof(LocalizationNotifyProperty)) as LocalizationNotifyProperty;

        // 检查目标属性类型，避免绑定到 Name 等只读属性
        if (serviceProvider is IProvideValueTarget provideValueTarget)
        {
            var targetProperty = provideValueTarget.TargetProperty;
            
            // 如果是 DependencyProperty，检查属性名称
            if (targetProperty is DependencyProperty dp)
            {
                var propertyName = dp.Name;
                
                // 如果是 Name 属性，直接返回原始键值字符串（MaterialDesign 内部会绑定到 AutomationProperties.Name）
                if (propertyName == "Name")
                {
                    return keyValue;
                }
            }
            
            // 如果是 PropertyInfo，也检查属性名称
            if (targetProperty is System.Reflection.PropertyInfo propInfo)
            {
                if (propInfo.Name == "Name")
                {
                    return keyValue;
                }
            }
        }

        // 如果适配器不可用（应用启动早期），创建一个延迟绑定
        if (adapter == null)
        {
            // 创建一个 Binding，绑定到 DelayedLocalizationAdapterSource
            var binding = new Binding
            {
                Source = DelayedLocalizationAdapterSource.Instance,
                Path = new System.Windows.PropertyPath($"Translations[{keyValue}]"),
                Mode = BindingMode.OneWay,
                FallbackValue = keyValue,
                TargetNullValue = keyValue
            };
            
            // 如果有参数，需要创建一个 MultiBinding
            if (!string.IsNullOrWhiteSpace(Param0) || !string.IsNullOrWhiteSpace(Param1) ||
                !string.IsNullOrWhiteSpace(Param0Key) || !string.IsNullOrWhiteSpace(Param1Key))
            {
                var multiBinding = new MultiBinding
                {
                    Converter = new LocalizationValueConverter(keyValue, Param0, Param1, Param0Key, Param1Key),
                    Mode = BindingMode.OneWay,
                    FallbackValue = keyValue,
                    TargetNullValue = keyValue
                };
                
                // 主绑定（翻译值）
                multiBinding.Bindings.Add(binding);
                
                // 参数绑定
                if (!string.IsNullOrWhiteSpace(Param0Key))
                {
                    var paramBinding = new Binding
                    {
                        Source = DelayedLocalizationAdapterSource.Instance,
                        Path = new System.Windows.PropertyPath($"Translations[{Param0Key}]"),
                        Mode = BindingMode.OneWay,
                        FallbackValue = Param0Key,
                        TargetNullValue = Param0Key
                    };
                    multiBinding.Bindings.Add(paramBinding);
                }
                else if (!string.IsNullOrWhiteSpace(Param0))
                {
                    multiBinding.Bindings.Add(new Binding { Source = Param0, Mode = BindingMode.OneWay });
                }
                
                if (!string.IsNullOrWhiteSpace(Param1Key))
                {
                    var paramBinding = new Binding
                    {
                        Source = DelayedLocalizationAdapterSource.Instance,
                        Path = new System.Windows.PropertyPath($"Translations[{Param1Key}]"),
                        Mode = BindingMode.OneWay,
                        FallbackValue = Param1Key,
                        TargetNullValue = Param1Key
                    };
                    multiBinding.Bindings.Add(paramBinding);
                }
                else if (!string.IsNullOrWhiteSpace(Param1))
                {
                    multiBinding.Bindings.Add(new Binding { Source = Param1, Mode = BindingMode.OneWay });
                }
                
                return multiBinding.ProvideValue(serviceProvider);
            }
            
            return binding.ProvideValue(serviceProvider);
        }

        // 适配器可用，直接获取翻译值
        string translation;
        if (!string.IsNullOrWhiteSpace(Param0Key) || !string.IsNullOrWhiteSpace(Param1Key) ||
            !string.IsNullOrWhiteSpace(Param0) || !string.IsNullOrWhiteSpace(Param1))
        {
            var param0Value = !string.IsNullOrWhiteSpace(Param0Key)
                ? adapter.GetString(Param0Key)
                : Param0 ?? string.Empty;
            var param1Value = !string.IsNullOrWhiteSpace(Param1Key)
                ? adapter.GetString(Param1Key)
                : Param1 ?? string.Empty;
            
            translation = adapter.GetString(keyValue, param0Value, param1Value);
        }
        else
        {
            translation = adapter.GetString(keyValue);
        }

        return translation;
    }

    /// <summary>
    /// 延迟本地化属性通知源
    /// 在 LocalizationNotifyProperty 未初始化时提供占位绑定源
    /// </summary>
    private class DelayedLocalizationAdapterSource
    {
        public static readonly DelayedLocalizationAdapterSource Instance = new();

        private LocalizationNotifyProperty? _adapter;
        private readonly Dictionary<string, string> _translations = new();

        public Dictionary<string, string> Translations => _translations;

        /// <summary>
        /// 获取本地化属性通知类实例（用于延迟绑定）
        /// </summary>
        public LocalizationNotifyProperty? Adapter => _adapter;

        private DelayedLocalizationAdapterSource()
        {
            // 尝试获取本地化属性通知类（如果已初始化）
            if (App.Services != null)
            {
                _adapter = App.Services.GetService(typeof(LocalizationNotifyProperty)) as LocalizationNotifyProperty;
            }

            // 订阅适配器的语言变化事件
            if (_adapter != null)
            {
                _adapter.PropertyChanged += OnAdapterPropertyChanged;
                UpdateTranslations();
            }
            else
            {
                // 如果适配器还未初始化，定期检查
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                timer.Tick += (s, e) =>
                {
                    if (App.Services != null)
                    {
                        var newAdapter = App.Services.GetService(typeof(LocalizationNotifyProperty)) as LocalizationNotifyProperty;
                        if (newAdapter != null && newAdapter != _adapter)
                        {
                            _adapter = newAdapter;
                            _adapter.PropertyChanged += OnAdapterPropertyChanged;
                            UpdateTranslations();
                            timer.Stop();
                        }
                    }
                };
                timer.Start();
            }
        }

        private void OnAdapterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizationNotifyProperty.CurrentLanguageCode))
            {
                UpdateTranslations();
            }
        }

        private void UpdateTranslations()
        {
            if (_adapter == null) return;

            // 这里无法预知所有需要翻译的键，所以不预先加载
            // 而是在 LocalizationValueConverter 中按需获取
        }
    }

    /// <summary>
    /// 本地化值转换器
    /// </summary>
    private class LocalizationValueConverter : IMultiValueConverter
    {
        private readonly string _key;
        private readonly string? _param0;
        private readonly string? _param1;
        private readonly string? _param0Key;
        private readonly string? _param1Key;

        public LocalizationValueConverter(string key, string? param0 = null!, string? param1 = null!, 
            string? param0Key = null!, string? param1Key = null)
        {
            _key = key;
            _param0 = param0;
            _param1 = param1;
            _param0Key = param0Key;
            _param1Key = param1Key;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return _key; // 回退到键值
            }

            // 尝试获取本地化属性通知类
            LocalizationNotifyProperty? adapter = null;
            if (App.Services != null)
            {
                adapter = App.Services.GetService(typeof(LocalizationNotifyProperty)) as LocalizationNotifyProperty;
            }

            // 如果服务不可用，使用延迟绑定源
            if (adapter == null)
            {
                adapter = DelayedLocalizationAdapterSource.Instance.Adapter;
            }

            if (adapter == null)
            {
                return _key; // 回退到键值
            }

            // 处理参数
            string? param0Value = null;
            string? param1Value = null;

            if (values.Length > 1 && values[1] != null)
            {
                param0Value = values[1].ToString();
            }
            else if (!string.IsNullOrWhiteSpace(_param0Key))
            {
                param0Value = adapter.GetString(_param0Key);
            }
            else if (!string.IsNullOrWhiteSpace(_param0))
            {
                param0Value = _param0;
            }

            if (values.Length > 2 && values[2] != null)
            {
                param1Value = values[2].ToString();
            }
            else if (!string.IsNullOrWhiteSpace(_param1Key))
            {
                param1Value = adapter.GetString(_param1Key);
            }
            else if (!string.IsNullOrWhiteSpace(_param1))
            {
                param1Value = _param1;
            }

            // 获取翻译值
            if (param0Value != null && param1Value != null)
            {
                return adapter.GetString(_key, param0Value, param1Value);
            }
            else if (param0Value != null)
            {
                return adapter.GetString(_key, param0Value);
            }
            else
            {
                return adapter.GetString(_key);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

