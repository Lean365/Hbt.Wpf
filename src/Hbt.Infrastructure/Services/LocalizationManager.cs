//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : LocalizationManager.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 本地化管理器实现（基础设施层）
//===================================================================

using System.Linq;
using Hbt.Application.Services.Routine;
using Hbt.Common.Constants;
using Hbt.Common.Helpers;
using Hbt.Common.Logging;
using Hbt.Common.Models;
using Hbt.Domain.Entities.Routine;
using Hbt.Domain.Interfaces;
using Hbt.Domain.Repositories;

namespace Hbt.Infrastructure.Services;

/// <summary>
/// 本地化管理器
/// </summary>
public class LocalizationManager : ILocalizationManager
{
    private string _currentLanguage;
    private readonly Dictionary<string, Dictionary<string, string>> _resources;
    private readonly ITranslationService? _translationService;
    private readonly ILanguageService? _languageService;
    private readonly ISettingService? _settingService;
    private readonly IBaseRepository<Language>? _languageRepository;
    private readonly IBaseRepository<Translation>? _translationRepository;
    private readonly AppLogManager _appLog;
    private List<LanguageItem> _cachedLanguages = new List<LanguageItem>();
    private bool _isInitialized = false;

    /// <summary>
    /// 语言切换事件
    /// </summary>
    public event EventHandler<string>? LanguageChanged;

    /// <summary>
    /// 当前语言代码
    /// </summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// 构造函数
    /// </summary>
    public LocalizationManager(
        IBaseRepository<Language> languageRepository, 
        IBaseRepository<Translation> translationRepository, 
        ISettingService settingService,
        AppLogManager appLog)
    {
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
        _settingService = settingService;
        _appLog = appLog;
        _resources = new Dictionary<string, Dictionary<string, string>>();

        // 从本地配置或系统语言获取默认语言
        _currentLanguage = GetDefaultLanguage();

        _appLog.Information("LocalizationManager 构造函数完成，当前语言：{CurrentLanguage}", _currentLanguage);
    }

    /// <summary>
    /// 初始化（按正确顺序：1.获取语言列表 2.确定选中项 3.加载选中语言的翻译）
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_translationRepository == null || _languageRepository == null)
        {
            _appLog.Warning("[LocalizationManager] 仓储未初始化");
            return;
        }

        try
        {
            // 步骤1: 先动态获取语言列表（异步加载，符合微软规范）
            _appLog.Information("[LocalizationManager] 步骤1: 开始获取语言列表（异步加载）...");
            _appLog.Information("[LocalizationManager] 检查 _languageRepository 是否为 null: {IsNull}", _languageRepository == null);
            
            if (_languageRepository == null)
            {
                _appLog.Error("[LocalizationManager] 语言仓储为 null，无法查询");
                return;
            }
            
            _appLog.Information("[LocalizationManager] 开始构建查询...");
            
            List<Language> languages;
            try
            {
                _appLog.Information("[LocalizationManager] 步骤1.1: 调用 AsQueryable()...");
                var query = _languageRepository.AsQueryable();
                
                _appLog.Information("[LocalizationManager] 步骤1.2: 调用 Where()...");
                query = query.Where(x => x.IsDeleted == 0 && x.LanguageStatus == 0);
                
                _appLog.Information("[LocalizationManager] 步骤1.3: 调用 OrderBy()...");
                query = query.OrderBy(x => x.OrderNum);
                
                _appLog.Information("[LocalizationManager] 步骤1.4: 调用 ToListAsync()...");
                languages = await query.ToListAsync();
                _appLog.Information("[LocalizationManager] 步骤1.5: ToListAsync() 返回成功");
            }
            catch (Exception dbEx)
            {
                _appLog.Error(dbEx, "[LocalizationManager] 数据库查询失败");
                throw; // 重新抛出异常
            }
            
            _appLog.Information("[LocalizationManager] 步骤1完成: 获取到 {Count} 种语言", languages.Count);
            
            // ⚠️ 立即缓存语言列表（即使在后续步骤失败也能使用）
            _cachedLanguages = languages.Select(lang => new LanguageItem
            {
                Code = lang.LanguageCode,
                Name = lang.LanguageName,
                Icon = lang.LanguageIcon,
                OrderNum = lang.OrderNum
            }).OrderBy(l => l.OrderNum).ToList();
            _appLog.Information("[LocalizationManager] 语言列表已缓存，共 {Count} 种语言", _cachedLanguages.Count);

            // 步骤2: 根据系统默认语言，确定语言列表的选中项
            _appLog.Information("[LocalizationManager] 步骤2: 当前默认语言为: {Language}", _currentLanguage);
            var selectedLanguage = languages.FirstOrDefault(l => l.LanguageCode == _currentLanguage);
            
            if (selectedLanguage == null)
            {
                _appLog.Warning("[LocalizationManager] 默认语言 {DefaultLanguage} 不在语言列表中，使用第一种语言", _currentLanguage);
                selectedLanguage = languages.FirstOrDefault();
                if (selectedLanguage != null)
                {
                    _currentLanguage = selectedLanguage.LanguageCode;
                    LocalConfigHelper.SaveLanguage(_currentLanguage);
                }
                else
                {
                    _appLog.Warning("[LocalizationManager] 语言列表为空，无法初始化");
                    return;
                }
            }

            _appLog.Information("[LocalizationManager] 步骤2完成: 选中语言 {SelectedLanguage}，语言ID: {LanguageId}", 
                selectedLanguage.LanguageCode, selectedLanguage.Id);

            // 步骤3: 根据选中项来动态获取翻译
            _appLog.Information("[LocalizationManager] 步骤3: 开始加载选中语言的翻译...");
            var translations = await _translationRepository.AsQueryable()
                .Where(x => x.IsDeleted == 0 && x.LanguageCode == selectedLanguage.LanguageCode)
                .ToListAsync();

            _appLog.Information("[LocalizationManager] 步骤3完成: 加载到 {Count} 条翻译", translations.Count);

            // 构建内存翻译数据结构
            if (!_resources.ContainsKey(_currentLanguage))
            {
                _resources[_currentLanguage] = new Dictionary<string, string>();
            }

            foreach (var translation in translations)
            {
                _resources[_currentLanguage][translation.TranslationKey] = translation.TranslationValue;
            }

            _appLog.Information("[LocalizationManager] 初始化完成，当前语言: {Language}，共 {Count} 条翻译已缓存", 
                _currentLanguage, translations.Count);
            
            _isInitialized = true;
            _appLog.Information("[LocalizationManager] 初始化标志已设置");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[LocalizationManager] 初始化失败");
        }
    }


    /// <summary>
    /// 获取本地化字符串（从内存读取，数据由 InitializeAsync 预加载）
    /// </summary>
    public string GetString(string key)
    {
        // 如果尚未初始化，等待一小段时间
        if (!_isInitialized && _resources.Count == 0)
        {
            int maxRetries = 50; // 最多等待 5 秒（50 * 100ms）
            int retryCount = 0;
            while (!_isInitialized && _resources.Count == 0 && retryCount < maxRetries)
            {
                Thread.Sleep(100); // 等待 100ms
                retryCount++;
            }
        }

        // 从已加载的翻译中查找（纯内存操作，同步）
        if (_resources.ContainsKey(_currentLanguage) &&
            _resources[_currentLanguage].ContainsKey(key))
        {
            return _resources[_currentLanguage][key];
        }

        // 如果找不到，返回键名
        return key;
    }

    /// <summary>
    /// 获取默认语言（从本地配置或系统语言获取）
    /// </summary>
    public string GetDefaultLanguage()
    {
        try
        {
            // 1. 优先读取本地用户配置
            var localLanguage = LocalConfigHelper.GetLanguage();
            if (!string.IsNullOrWhiteSpace(localLanguage))
            {
                _appLog.Information("[LocalizationManager] 从本地配置获取语言: {Language}", localLanguage);
                return localLanguage;
            }

            // 2. 如果没有本地配置，获取系统语言
            var systemLanguageCode = SystemInfoHelper.GetSystemLanguageCode();
            var mappedLanguage = MapSystemLanguageToAppLanguage(systemLanguageCode);
            
            _appLog.Information("[LocalizationManager] 系统语言: {SystemLanguage}，映射为: {MappedLanguage}", systemLanguageCode, mappedLanguage);
            
            // 3. 保存到本地配置
            LocalConfigHelper.SaveLanguage(mappedLanguage);
            
            return mappedLanguage;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[LocalizationManager] 获取默认语言失败");
            return "zh-CN"; // 默认返回中文
        }
    }

    /// <summary>
    /// 将系统语言映射到应用支持的语言
    /// </summary>
    private string MapSystemLanguageToAppLanguage(string systemLanguageCode)
    {
        if (string.IsNullOrEmpty(systemLanguageCode))
        {
            return "zh-CN";
        }

        var normalizedCode = systemLanguageCode.ToLowerInvariant();

        // 中文相关
        if (normalizedCode.StartsWith("zh"))
        {
            return "zh-CN";
        }
        
        // 日文相关
        if (normalizedCode.StartsWith("ja"))
        {
            return "ja-JP";
        }
        
        // 其他语言都映射到英文
        return "en-US";
    }

    /// <summary>
    /// 切换语言
    /// </summary>
    public void ChangeLanguage(string languageCode)
    {
        _appLog.Information("[LocalizationManager] ========== 开始切换语言 ==========");
        _appLog.Information("[LocalizationManager] 目标语言代码: {LanguageCode}", languageCode);
        _appLog.Information("[LocalizationManager] 当前语言代码: {CurrentLanguage}", _currentLanguage);
        _appLog.Information("[LocalizationManager] 可用语言缓存数量: {Count}", _resources.Count);
        
        _currentLanguage = languageCode;
        
        // 保存到本地配置（用户个性化设置）
        LocalConfigHelper.SaveLanguage(languageCode);
        _appLog.Information("[LocalizationManager] 已保存语言到本地配置");
        
        // 检查该语言的翻译是否已加载
        bool translationsLoaded = _resources.ContainsKey(languageCode);
        _appLog.Information("[LocalizationManager] 该语言翻译已加载: {Loaded}", translationsLoaded);
        
        if (translationsLoaded)
        {
            var translationCount = _resources[languageCode].Count;
            _appLog.Information("[LocalizationManager] 该语言翻译缓存条目数: {Count}", translationCount);
        }
        else
        {
            _appLog.Information("[LocalizationManager] 该语言翻译尚未加载，开始同步加载: {Language}", languageCode);
            try
            {
                // 同步加载翻译数据，确保数据可用
                LoadLanguageTranslationsAsync(languageCode).GetAwaiter().GetResult();
                _appLog.Information("[LocalizationManager] 翻译加载完成，条目数: {Count}", _resources.ContainsKey(languageCode) ? _resources[languageCode].Count : 0);
            }
            catch (Exception ex)
            {
                _appLog.Error(ex, "[LocalizationManager] 同步加载翻译失败: {Language}", languageCode);
            }
        }

        // 触发语言切换事件
        _appLog.Information("[LocalizationManager] 触发语言切换事件");
        LanguageChanged?.Invoke(this, languageCode);
        _appLog.Information("[LocalizationManager] ========== 语言切换完成 ==========");
    }

    /// <summary>
    /// 加载指定语言的翻译数据
    /// </summary>
    private async Task LoadLanguageTranslationsAsync(string languageCode)
    {
        if (_translationRepository == null || _languageRepository == null)
        {
            return;
        }

        try
        {
            _appLog.Information("[LocalizationManager] 开始加载语言翻译: {Language}", languageCode);

            // 1. 获取语言ID
            var languages = await _languageRepository.AsQueryable()
                .Where(x => x.IsDeleted == 0 && x.LanguageCode == languageCode)
                .ToListAsync();
            
            var language = languages.FirstOrDefault();

            if (language == null)
            {
                _appLog.Warning("[LocalizationManager] 语言不存在: {Language}", languageCode);
                return;
            }

            // 2. 加载该语言的翻译
            var translations = await _translationRepository.AsQueryable()
                .Where(x => x.IsDeleted == 0 && x.LanguageCode == language.LanguageCode)
                .ToListAsync();

            // 3. 构建内存翻译数据
            if (!_resources.ContainsKey(languageCode))
            {
                _resources[languageCode] = new Dictionary<string, string>();
            }

            foreach (var translation in translations)
            {
                _resources[languageCode][translation.TranslationKey] = translation.TranslationValue;
            }

            _appLog.Information("[LocalizationManager] 语言翻译加载完成: {Language}，共 {Count} 条", languageCode, translations.Count);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[LocalizationManager] 加载语言翻译失败: {Language}", languageCode);
        }
    }

    /// <summary>
    /// 获取所有可用的语言列表（从缓存读取，数据由 InitializeAsync 异步预加载）
    /// 符合微软规范：同步方法不执行 I/O 操作，纯内存读取
    /// </summary>
    public List<object> GetLanguages()
    {
        // 如果尚未初始化，等待一小段时间（最多 5 秒）
        if (!_isInitialized && _cachedLanguages.Count == 0)
        {
            _appLog.Information("[LocalizationManager] 初始化进行中，等待语言列表...");
            
            int maxRetries = 50; // 最多等待 5 秒（50 * 100ms）
            int retryCount = 0;
            while (!_isInitialized && _cachedLanguages.Count == 0 && retryCount < maxRetries)
            {
                Thread.Sleep(100); // 等待 100ms
                retryCount++;
            }
            
            if (_cachedLanguages.Count == 0)
            {
                _appLog.Warning("[LocalizationManager] 等待超时，语言列表仍未初始化");
                return new List<object>();
            }
        }

        if (_cachedLanguages.Count == 0)
        {
            _appLog.Warning("[LocalizationManager] 语言列表为空");
            return new List<object>();
        }

        _appLog.Information("[LocalizationManager] 从缓存读取语言列表，共 {Count} 种语言", _cachedLanguages.Count);
        return _cachedLanguages.Cast<object>().ToList();
    }
}

/// <summary>
/// 语言项
/// </summary>
public class LanguageItem : SelectOptionModel<string>
{
    /// <summary>
    /// 语言图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 设置选项值（兼容旧代码）
    /// </summary>
    public string Code
    {
        get => Value;
        set => Value = value;
    }

    /// <summary>
    /// 设置选项标签（兼容旧代码）
    /// </summary>
    public string Name
    {
        get => Label;
        set => Label = value;
    }

    /// <summary>
    /// 重写 ToString 方法，用于 ComboBox 显示
    /// </summary>
    public override string ToString()
    {
        return Label ?? Name ?? Code ?? string.Empty;
    }
}

