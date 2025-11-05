//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : LanguageService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-29
// 版本号 : 1.0
// 描述    : WPF语言服务（管理语言切换和翻译）
//===================================================================

using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using Hbt.Application.Dtos.Routine;
using Hbt.Application.Services.Routine;
using ILanguageService = Hbt.Application.Services.Routine.ILanguageService;
using ITranslationService = Hbt.Application.Services.Routine.ITranslationService;

namespace Hbt.Fluent.Services;

/// <summary>
/// WPF 语言服务
/// </summary>
public class LanguageService : System.ComponentModel.INotifyPropertyChanged
{
    private const string SettingsFileName = "language-settings.json";
    private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
    
    private readonly ILanguageService _languageService;
    private readonly ITranslationService _translationService;
    
    private string _currentLanguageCode = "zh-CN";
    private List<LanguageOptionDto> _availableLanguages = new();
    private ConcurrentDictionary<string, string> _translations = new();
    
    /// <summary>
    /// 语言切换事件
    /// </summary>
    public event EventHandler<string>? LanguageChanged;

    public LanguageService(ILanguageService languageService, ITranslationService translationService)
    {
        _languageService = languageService;
        _translationService = translationService;
    }

    /// <summary>
    /// 当前语言代码
    /// </summary>
    public string CurrentLanguageCode => _currentLanguageCode;

    /// <summary>
    /// 可用语言列表
    /// </summary>
    public List<LanguageOptionDto> AvailableLanguages => _availableLanguages;

    /// <summary>
    /// 初始化语言服务
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // 动态加载可用语言列表（从后端服务获取）
            var languagesResult = await _languageService.OptionAsync(false);
            if (languagesResult.Success && languagesResult.Data != null && languagesResult.Data.Any())
            {
                _availableLanguages = languagesResult.Data;
                var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
                operLog?.Information("[语言] 从后端动态加载到 {Count} 种语言", _availableLanguages.Count);
            }
            else
            {
                // 后端服务不可用或无数据，则为空列表（不使用静态后备）
                _availableLanguages = new List<LanguageOptionDto>();
                var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
                operLog?.Warning("[语言] 后端语言服务不可用或无数据，AvailableLanguages 为空");
            }

            // 从配置文件加载当前语言
            LoadLanguageSettings();

            // 如果当前语言在可用列表中，动态加载翻译
            if (_availableLanguages.Any(l => l.Code == _currentLanguageCode))
            {
                await LoadTranslationsAsync(_currentLanguageCode);
                // 即时刷新绑定（即使语言代码未变化）
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
            }
            else if (_availableLanguages.Any())
            {
                // 如果当前语言不可用，使用第一个可用语言
                _currentLanguageCode = _availableLanguages[0].Code;
                SaveLanguageSettings();
                await LoadTranslationsAsync(_currentLanguageCode);
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[语言] 初始化语言服务失败");
            _availableLanguages = new List<LanguageOptionDto>();
        }
    }

    /// <summary>
    /// 设置当前语言
    /// </summary>
    public async Task SetLanguageAsync(string languageCode)
    {
        if (_currentLanguageCode == languageCode)
            return;

        _currentLanguageCode = languageCode;
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
        SaveLanguageSettings();

        // 加载新语言的翻译
        await LoadTranslationsAsync(languageCode);

        // 设置线程文化
        var culture = new CultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // 触发语言切换事件
        LanguageChanged?.Invoke(this, languageCode);
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 获取翻译文本
    /// </summary>
    public string GetTranslation(string key, string? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return defaultValue ?? string.Empty;
        }

        if (_translations.TryGetValue(key, out var value))
        {
            return value;
        }

        // 如果找不到翻译，返回默认值或键本身
        return defaultValue ?? key;
    }

    /// <summary>
    /// 异步获取翻译文本（动态从后端获取）
    /// </summary>
    public async Task<string> GetTranslationAsync(string key, string? defaultValue = null)
    {
        // 先从缓存查找
        if (_translations.TryGetValue(key, out var value))
        {
            return value;
        }

        // 如果缓存中没有，动态从后端获取单个翻译
        try
        {
            var result = await _translationService.GetValueAsync(_currentLanguageCode, key);
            if (result.Success && !string.IsNullOrEmpty(result.Data))
            {
                // 添加到缓存
                _translations[key] = result.Data;
                return result.Data;
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[语言] 动态获取翻译失败 [key: {Key}]", key);
        }

        // 返回默认值或键本身
        return defaultValue ?? key;
    }

    /// <summary>
    /// 刷新翻译缓存（重新从后端加载）
    /// </summary>
    public async Task RefreshTranslationsAsync()
    {
        await LoadTranslationsAsync(_currentLanguageCode);
    }

    /// <summary>
    /// 加载翻译（从后端服务动态获取）
    /// </summary>
    private async Task LoadTranslationsAsync(string languageCode)
    {
        try
        {
            // 创建新的字典，避免并发修改问题
            var newTranslations = new ConcurrentDictionary<string, string>();

            // 优先从后端服务动态获取翻译（模块：Frontend 表示前端翻译）
            var translationsResult = await _translationService.GetTranslationsByModuleAsync("Frontend");
            
            // 如果 Frontend 模块没有数据，尝试获取所有翻译
            if (!translationsResult.Success || translationsResult.Data == null || !translationsResult.Data.Any())
            {
                var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
                operLog?.Debug("[语言] Frontend 模块无翻译数据，尝试获取所有翻译");
                translationsResult = await _translationService.GetTranslationsByModuleAsync(null);
            }
            
            if (translationsResult.Success && translationsResult.Data != null && translationsResult.Data.Any())
            {
                // 后端返回格式：{翻译键: {语言代码: 翻译值}}
                // 提取当前语言的翻译
                foreach (var translationKeyPair in translationsResult.Data)
                {
                    var translationKey = translationKeyPair.Key;
                    var languageTranslations = translationKeyPair.Value;
                    
                    // 获取当前语言的翻译值
                    if (languageTranslations.TryGetValue(languageCode, out var translationValue))
                    {
                        newTranslations[translationKey] = translationValue;
                    }
                    // 如果当前语言没有翻译，尝试使用其他可用语言作为后备
                    else if (languageTranslations.Any())
                    {
                        var fallbackValue = languageTranslations.First().Value;
                        newTranslations[translationKey] = fallbackValue;
                    }
                }
                
                var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
                operLog?.Information("[语言] 从后端动态加载到 {Count} 条翻译（语言: {LanguageCode}）", newTranslations.Count, languageCode);
            }
            else
            {
                // 后端服务不可用或没有翻译数据，使用默认翻译作为后备
                var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
                operLog?.Warning("[语言] 后端翻译服务不可用或没有翻译数据，使用默认翻译");
                var defaultTranslations = GetDefaultTranslations(languageCode);
                foreach (var kvp in defaultTranslations)
                {
                    newTranslations[kvp.Key] = kvp.Value;
                }
            }

            // 原子性替换整个字典，避免并发问题
            _translations = newTranslations;

            // 通知绑定刷新（即使语言代码未改变）
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[语言] 加载翻译失败");
            // 如果加载失败，清空字典（使用新的 ConcurrentDictionary）
            _translations = new ConcurrentDictionary<string, string>();
        }
    }

    /// <summary>
    /// 获取默认翻译（本地后备）
    /// </summary>
    private Dictionary<string, string> GetDefaultTranslations(string languageCode)
    {
        var translations = new Dictionary<string, string>();

        if (languageCode.StartsWith("zh"))
        {
            // 中文翻译
            translations["login.title"] = "登录 - 黑冰台管理系统";
            translations["login.welcome"] = "欢迎登录";
            translations["login.please.input"] = "请输入您的账号信息";
            translations["login.username"] = "请输入用户名";
            translations["login.password"] = "请输入密码";
            translations["login.remember"] = "记住密码";
            translations["login.forgot"] = "忘记密码？";
            translations["login.button"] = "登录";
            translations["login.loading"] = "登录中...";
            translations["login.error"] = "登录失败，请检查用户名和密码";
            translations["system.name"] = "黑冰台管理系统";
            translations["system.slogan"] = "企业级管理平台";
            translations["system.tagline"] = "高效 · 安全 · 智能";
            translations["theme.switch"] = "切换主题";
            translations["language.switch"] = "切换语言";
        }
        else
        {
            // 英文翻译
            translations["login.title"] = "Login - Hbt Management System";
            translations["login.welcome"] = "Welcome";
            translations["login.please.input"] = "Please enter your account information";
            translations["login.username"] = "Username";
            translations["login.password"] = "Password";
            translations["login.remember"] = "Remember me";
            translations["login.forgot"] = "Forgot password?";
            translations["login.button"] = "Login";
            translations["login.loading"] = "Logging in...";
            translations["login.error"] = "Login failed, please check your username and password";
            translations["system.name"] = "Hbt Management System";
            translations["system.slogan"] = "Enterprise Management Platform";
            translations["system.tagline"] = "Efficient · Secure · Intelligent";
            translations["theme.switch"] = "Switch theme";
            translations["language.switch"] = "Switch language";
            translations["close.window"] = "Close window";
        }

        return translations;
    }

    /// <summary>
    /// 保存语言设置
    /// </summary>
    private void SaveLanguageSettings()
    {
        try
        {
            var settings = new LanguageSettings { LanguageCode = _currentLanguageCode };
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // 忽略保存错误
        }
    }

    /// <summary>
    /// 加载语言设置
    /// </summary>
    private void LoadLanguageSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var options = new JsonSerializerOptions();
                var settings = JsonSerializer.Deserialize<LanguageSettings>(json, options);
                if (settings != null && !string.IsNullOrEmpty(settings.LanguageCode))
                {
                    _currentLanguageCode = settings.LanguageCode;
                }
            }
            else
            {
                // 使用系统默认语言
                var systemLang = CultureInfo.CurrentUICulture.Name;
                if (_availableLanguages.Any(l => l.Code == systemLang))
                {
                    _currentLanguageCode = systemLang;
                }
                else if (_availableLanguages.Any())
                {
                    _currentLanguageCode = _availableLanguages[0].Code;
                }
            }
        }
        catch
        {
            // 如果加载失败，使用默认值
            if (_availableLanguages.Any())
            {
                _currentLanguageCode = _availableLanguages[0].Code;
            }
        }
    }
}

/// <summary>
/// 语言设置
/// </summary>
internal class LanguageSettings
{
    public string LanguageCode { get; set; } = "zh-CN";
}

