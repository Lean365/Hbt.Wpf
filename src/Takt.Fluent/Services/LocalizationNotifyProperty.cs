// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Services
// 文件名称：LocalizationNotifyProperty.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：本地化属性通知类（包装 ILocalizationManager，实现 INotifyPropertyChanged）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.ComponentModel;
using System.Threading;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.Services;

/// <summary>
/// 本地化属性通知类
/// 包装 ILocalizationManager，实现 INotifyPropertyChanged，供 WPF 绑定使用
/// </summary>
public class LocalizationNotifyProperty : INotifyPropertyChanged
{
    private readonly ILocalizationManager _localizationManager;
    private readonly SynchronizationContext? _uiContext;

    /// <summary>
    /// 当前语言代码（可绑定属性）
    /// </summary>
    public string CurrentLanguageCode
    {
        get => _localizationManager.CurrentLanguage;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public LocalizationNotifyProperty(ILocalizationManager localizationManager)
    {
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        
        // 捕获 UI 线程的 SynchronizationContext（如果可用）
        _uiContext = SynchronizationContext.Current;
        
        // 订阅语言切换事件，触发 PropertyChanged
        _localizationManager.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>
    /// 语言切换事件处理
    /// </summary>
    private void OnLanguageChanged(object? sender, string languageCode)
    {
        // 确保在 UI 线程上触发 PropertyChanged
        if (_uiContext != null && _uiContext != SynchronizationContext.Current)
        {
            // 在 UI 线程上执行
            _uiContext.Post(_ =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
            }, null);
        }
        else
        {
            // 如果已经在 UI 线程上，直接触发
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
        }
    }

    /// <summary>
    /// 属性变更事件
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 获取翻译文本（支持参数替换）
    /// </summary>
    public string GetString(string key, params string[] args)
    {
        var translation = _localizationManager.GetString(key);
        
        // 如果提供了参数，进行字符串格式化替换
        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(translation, args);
            }
            catch
            {
                // 格式化失败，返回原始翻译
                return translation;
            }
        }
        
        return translation;
    }

    /// <summary>
    /// 获取翻译文本（兼容 LanguageService.GetTranslation）
    /// </summary>
    public string GetTranslation(string key, string? defaultValue = null)
    {
        var translation = _localizationManager.GetString(key);
        
        // 如果找不到翻译，返回默认值或键本身
        if (translation == key && defaultValue != null)
        {
            return defaultValue;
        }
        
        return translation;
    }

    /// <summary>
    /// 切换语言
    /// </summary>
    public void ChangeLanguage(string languageCode)
    {
        _localizationManager.ChangeLanguage(languageCode);
        // PropertyChanged 会在 LanguageChanged 事件处理中触发
    }
}

