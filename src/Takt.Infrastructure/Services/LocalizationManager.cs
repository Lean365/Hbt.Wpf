//=======================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.Services
// 文件名 : LocalizationManager.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 本地化管理器实现（基础设施层）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//=======================================

using Takt.Application.Services.Routine;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Models;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Interfaces;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Services;

/// <summary>
/// 本地化管理器
/// </summary>
public class LocalizationManager : ILocalizationManager
{
    private string _currentLanguage;
    private readonly Dictionary<string, Dictionary<string, string>> _resources;
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
            _appLog.Information("[LocalizationManager] 步骤1: 开始获取语言列表（异步加载）..");
            _appLog.Information("[LocalizationManager] 检查 _languageRepository 是否为 null: {IsNull}", _languageRepository == null);

            if (_languageRepository == null)
            {
                _appLog.Error("[LocalizationManager] 语言仓储为 null，无法查询");
                return;
            }

            _appLog.Information("[LocalizationManager] 开始构建查询..");

            List<Language> languages = new List<Language>();
            try
            {
                _appLog.Information("[LocalizationManager] 步骤1.1: 调用 AsQueryable()..");
                var query = _languageRepository.AsQueryable();

                _appLog.Information("[LocalizationManager] 步骤1.2: 调用 Where()..");
                query = query.Where(x => x.IsDeleted == 0 && x.LanguageStatus == 0);

                _appLog.Information("[LocalizationManager] 步骤1.3: 调用 OrderBy()..");
                query = query.OrderBy(x => x.OrderNum);

                _appLog.Information("[LocalizationManager] 步骤1.4: 调用 ToListAsync()（带超时保护，后台线程执行）..");
                
                // 使用 CancellationTokenSource 设置超时（10秒）
                // 使用 ConfigureAwait(false) 确保不在 UI 线程上等待
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    try
                    {
                        languages = await query.ToListAsync(cts.Token).ConfigureAwait(false);
                        _appLog.Information("[LocalizationManager] 步骤1.5: ToListAsync() 返回成功，获取到 {Count} 条记录", languages.Count);
                    }
                    catch (OperationCanceledException)
                    {
                        _appLog.Error("[LocalizationManager] 数据库查询超时（10秒），可能原因：1.数据库连接失败 2.表不存在 3.网络问题");
                        throw new TimeoutException("数据库查询超时，请检查数据库连接和表是否存在");
                    }
                }
            }
            catch (Exception dbEx)
            {
                _appLog.Error(dbEx, "[LocalizationManager] 数据库查询失败: {Message}", dbEx.Message);
                // 不抛出异常，返回空列表，允许应用继续启动
                _appLog.Warning("[LocalizationManager] 返回空语言列表，应用将继续启动");
                languages = new List<Language>();
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
                    AppSettingsHelper.SaveLanguage(_currentLanguage);
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
            _appLog.Information("[LocalizationManager] 步骤3: 开始加载选中语言的翻译（后台线程执行）..");
            // 使用 ConfigureAwait(false) 确保不在 UI 线程上等待
            var translations = await _translationRepository.AsQueryable()
                .Where(x => x.IsDeleted == 0 && x.LanguageCode == selectedLanguage.LanguageCode)
                .ToListAsync()
                .ConfigureAwait(false);

            _appLog.Information("[LocalizationManager] 步骤3完成: 加载到 {Count} 条翻译", translations.Count);

            // 构建内存翻译数据结构
            if (!_resources.ContainsKey(_currentLanguage))
            {
                _resources[_currentLanguage] = new Dictionary<string, string>();
            }

            foreach (var translation in translations)
            {
                // 确保 TranslationKey 不为 null，如果为 null 则跳过
                if (string.IsNullOrEmpty(translation.TranslationKey))
                {
                    continue;
                }
                
                // 如果 TranslationValue 为 null，使用 TranslationKey 作为后备值（保证字典中存储的值不为 null）
                _resources[_currentLanguage][translation.TranslationKey] = 
                    string.IsNullOrEmpty(translation.TranslationValue) 
                        ? translation.TranslationKey 
                        : translation.TranslationValue;
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
    /// 保证永远不会返回 null：如果找不到翻译或翻译值为 null，返回 key 本身
    /// 翻译键会自动转换为小写以匹配种子数据
    /// </summary>
    public string GetString(string key)
    {
        // 防御性编程：确保 key 不为 null
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        // 将翻译键转换为小写，以匹配种子数据
        var normalizedKey = key.ToLower();

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
            _resources[_currentLanguage].ContainsKey(normalizedKey))
        {
            var translationValue = _resources[_currentLanguage][normalizedKey];
            // 如果翻译值为 null 或空字符串，返回 key 本身作为后备
            return string.IsNullOrEmpty(translationValue) ? key : translationValue;
        }

        // 如果找不到，返回键名（永远不会返回 null）
        return key;
    }

    /// <summary>
    /// 获取默认语言（从本地配置或系统语言获取）
    /// </summary>
    public string GetDefaultLanguage()
    {
        try
        {
            // 1. 优先读取配置文件
            var localLanguage = AppSettingsHelper.GetLanguage();
            if (!string.IsNullOrWhiteSpace(localLanguage))
            {
                _appLog.Information("[LocalizationManager] 从配置文件获取语言: {Language}", localLanguage);
                return localLanguage;
            }

            // 2. 如果配置文件中没有，获取系统语言
            var systemLanguageCode = SystemInfoHelper.GetSystemLanguageCode();
            var mappedLanguage = MapSystemLanguageToAppLanguage(systemLanguageCode);

            _appLog.Information("[LocalizationManager] 系统语言: {SystemLanguage}，映射为: {MappedLanguage}", systemLanguageCode, mappedLanguage);

            // 3. 保存到配置文件
            AppSettingsHelper.SaveLanguage(mappedLanguage);

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

        var normalizedCode = systemLanguageCode.ToLower();

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

        // 保存到配置文件（用户个性化设置）
        AppSettingsHelper.SaveLanguage(languageCode);
        _appLog.Information("[LocalizationManager] 已保存语言到配置文件");

        // 检查该语言的翻译是否已加载
        bool translationsLoaded = _resources.ContainsKey(languageCode);
        _appLog.Information("[LocalizationManager] 该语言翻译已加载: {Loaded}", translationsLoaded);

        // 先触发语言切换事件，让 UI 立即响应（即使翻译数据尚未加载）
        // 确保在 UI 线程上触发，避免 CollectionView 线程错误
        _appLog.Information("[LocalizationManager] 触发语言切换事件（立即响应）");
        InvokeLanguageChangedOnUIThread(languageCode);

        if (translationsLoaded)
        {
            var translationCount = _resources[languageCode].Count;
            _appLog.Information("[LocalizationManager] 该语言翻译缓存条目数: {Count}", translationCount);
            _appLog.Information("[LocalizationManager] ========== 语言切换完成 ==========");
        }
        else
        {
            _appLog.Information("[LocalizationManager] 该语言翻译尚未加载，开始后台异步加载: {Language}", languageCode);
            // 在后台线程异步加载翻译数据，避免阻塞 UI 线程
            // 使用 Task.Factory.StartNew 并指定 TaskCreationOptions 确保立即执行，避免线程池调度延迟
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    var startTime = DateTime.Now;
                    _appLog.Information("[LocalizationManager] 异步任务实际开始执行: {Language}, 时间: {Time}", languageCode, startTime);
                    
                    await LoadLanguageTranslationsAsync(languageCode).ConfigureAwait(false);
                    
                    var endTime = DateTime.Now;
                    var elapsed = (endTime - startTime).TotalMilliseconds;
                    _appLog.Information("[LocalizationManager] 翻译加载完成，条目数: {Count}, 耗时: {Elapsed}ms", 
                        _resources.ContainsKey(languageCode) ? _resources[languageCode].Count : 0, elapsed);
                    
                    // 翻译加载完成后，再次触发事件更新 UI
                    // 确保在 UI 线程上触发事件，避免 CollectionView 线程错误
                    _appLog.Information("[LocalizationManager] 翻译加载完成，触发 UI 更新事件");
                    InvokeLanguageChangedOnUIThread(languageCode);

                    _appLog.Information("[LocalizationManager] ========== 语言切换完成（翻译已加载） ==========");
                }
                catch (Exception ex)
                {
                    _appLog.Error(ex, "[LocalizationManager] 异步加载翻译失败: {Language}", languageCode);
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default).Unwrap();
        }
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
                // 确保 TranslationKey 不为 null，如果为 null 则跳过
                if (string.IsNullOrEmpty(translation.TranslationKey))
                {
                    continue;
                }
                
                // 如果 TranslationValue 为 null，使用 TranslationKey 作为后备值（保证字典中存储的值不为 null）
                _resources[languageCode][translation.TranslationKey] = 
                    string.IsNullOrEmpty(translation.TranslationValue) 
                        ? translation.TranslationKey 
                        : translation.TranslationValue;
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
            _appLog.Information("[LocalizationManager] 初始化进行中，等待语言列表..");

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

    /// <summary>
    /// 安全触发 LanguageChanged 事件
    /// Infrastructure 层不处理 UI 线程切换，由订阅者（如 LocalizationAdapter）负责
    /// </summary>
    private void InvokeLanguageChangedOnUIThread(string languageCode)
    {
        try
        {
            // Infrastructure 层直接触发事件，不处理线程切换
            // 订阅者（如 LocalizationAdapter）应该自己处理线程切换
            LanguageChanged?.Invoke(this, languageCode);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[LocalizationManager] 触发 LanguageChanged 事件时发生异常");
        }
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
        get => DataValue;
        set => DataValue = value;
    }

    /// <summary>
    /// 设置选项标签（兼容旧代码）
    /// </summary>
    public string Name
    {
        get => DataLabel;
        set => DataLabel = value;
    }

    /// <summary>
    /// 重写 ToString 方法，用于 ComboBox 显示
    /// </summary>
    public override string ToString()
    {
        return DataLabel ?? Name ?? Code ?? string.Empty;
    }
}

