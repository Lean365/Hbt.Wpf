//===================================================================
// 项目名 : Takt.Fluent
// 文件名 : MySettingsViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 2.0
// 描述    : 用户自定义设置视图模型（语言、主题等）
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用
using Takt.Application.Dtos.Routine;
using Takt.Common.Helpers;
using Takt.Domain.Interfaces;
using Takt.Fluent.Services;

namespace Takt.Fluent.ViewModels.Settings;

/// <summary>
/// 用户自定义设置视图模型
/// 用于管理用户的个人设置（语言、主题等）
/// </summary>
public partial class MySettingsViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    // 语言设置
    [ObservableProperty]
    private ObservableCollection<LanguageOptionDto> _availableLanguages = new();

    [ObservableProperty]
    private LanguageOptionDto? _selectedLanguage;

    // 主题设置
    [ObservableProperty]
    private ThemeModeOption? _selectedTheme;

    [ObservableProperty]
    private ObservableCollection<ThemeModeOption> _availableThemes = new();

    // 字体设置
    [ObservableProperty]
    private FontFamily? _selectedFontFamily;

    [ObservableProperty]
    private ObservableCollection<FontFamily> _availableFontFamilies = new();

    // 字体大小设置
    [ObservableProperty]
    private double _selectedFontSize;

    [ObservableProperty]
    private ObservableCollection<double> _availableFontSizes = new();

    private bool _isUpdatingTheme = false; // 防止循环更新
    private bool _isInitialized = false; // 防止重复初始化

    private readonly ILocalizationManager? _localizationManager;
    private readonly ThemeService? _themeService;

    public MySettingsViewModel(ILocalizationManager? localizationManager = null, ThemeService? themeService = null)
    {
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _themeService = themeService ?? App.Services?.GetService<ThemeService>();

        // 初始化主题选项
        InitializeThemeOptions();

        // 订阅语言变化事件，更新主题选项的显示名称
        if (_localizationManager != null)
        {
            _localizationManager.LanguageChanged += OnLanguageChanged;
        }

        if (_themeService != null && !_isInitialized)
        {
            _themeService.ThemeChanged += OnThemeServiceThemeChanged;
            _isInitialized = true;
        }
    }

    private void OnLanguageChanged(object? sender, string languageCode)
    {
        // 语言变化时，更新主题选项的显示名称
        InitializeThemeOptions();

        // 保持当前选中的主题
        if (_themeService != null && AvailableThemes.Any())
        {
            var mode = _themeService.GetCurrentTheme();
            _isUpdatingTheme = true;
            try
            {
                SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Mode == mode) ?? AvailableThemes.First();
            }
            finally
            {
                _isUpdatingTheme = false;
            }
        }
    }

    private string GetTranslation(string key, string defaultValue)
    {
        if (_localizationManager == null)
        {
            return defaultValue;
        }
        var translation = _localizationManager.GetString(key);
        return (translation == key) ? defaultValue : translation;
    }

    private void OnThemeServiceThemeChanged(object? sender, System.Windows.ThemeMode mode)
    {
        if (_isUpdatingTheme)
        {
            return;
        }

        if (AvailableThemes.Count == 0)
        {
            return;
        }

        var target = AvailableThemes.FirstOrDefault(t => t.Mode == mode);
        if (target != null && (SelectedTheme == null || SelectedTheme.Mode != mode))
        {
            _isUpdatingTheme = true;
            try
            {
                SelectedTheme = target;
            }
            finally
            {
                _isUpdatingTheme = false;
            }
        }
    }

    private void InitializeThemeOptions()
    {
        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();

        AvailableThemes.Clear();

        // 使用本地化文本
        var systemText = GetTranslation("Settings.Customize.ThemeMode.System", "跟随系统");
        var lightText = GetTranslation("Settings.Customize.ThemeMode.Light", "浅色");
        var darkText = GetTranslation("Settings.Customize.ThemeMode.Dark", "深色");

        AvailableThemes.Add(new ThemeModeOption { Mode = System.Windows.ThemeMode.System, DisplayName = systemText });
        AvailableThemes.Add(new ThemeModeOption { Mode = System.Windows.ThemeMode.Light, DisplayName = lightText });
        AvailableThemes.Add(new ThemeModeOption { Mode = System.Windows.ThemeMode.Dark, DisplayName = darkText });

        // 读取当前主题并同步到 UI
        _isUpdatingTheme = true;
        try
        {
            var mode = _themeService?.GetCurrentTheme() ?? System.Windows.ThemeMode.System;
            var target = AvailableThemes.FirstOrDefault(t => t.Mode == mode) ?? AvailableThemes[0];
            SelectedTheme = target;
            operLog?.Debug("[设置] InitializeThemeOptions - 当前主题: {DisplayName}", target.DisplayName);
        }
        finally
        {
            _isUpdatingTheme = false;
        }
    }

    public Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            // 加载可用语言列表
            if (_localizationManager != null)
            {
                var languageObjects = _localizationManager.GetLanguages();
                AvailableLanguages.Clear();
                foreach (var langObj in languageObjects)
                {
                    // ILocalizationManager.GetLanguages() 返回 LanguageItem 对象
                    if (langObj is Takt.Infrastructure.Services.LanguageItem langItem)
                    {
                        AvailableLanguages.Add(new LanguageOptionDto
                        {
                            Code = langItem.Code,
                            Name = langItem.Name,
                            DataValue = langItem.DataValue,
                            DataLabel = langItem.DataLabel,
                            OrderNum = langItem.OrderNum
                        });
                    }
                }

                // 设置当前选中的语言
                var currentLanguageCode = _localizationManager.CurrentLanguage;
                SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == currentLanguageCode);
            }

            if (_themeService != null && AvailableThemes.Any())
            {
                var mode = _themeService.GetCurrentTheme();
                _isUpdatingTheme = true;
                try
                {
                    SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Mode == mode) ?? AvailableThemes.First();
                }
                finally
                {
                    _isUpdatingTheme = false;
                }
            }

            // 加载系统字体列表
            LoadFontFamilies();

            // 加载字体大小列表
            LoadFontSizes();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            var errorKey = GetTranslation("Settings.Customize.LoadFailed", "加载设置失败：{0}");
            ErrorMessage = string.Format(errorKey, ex.Message);
        }
        finally
        {
            IsLoading = false;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveLanguageAsync()
    {
        if (_localizationManager == null || SelectedLanguage == null)
        {
            ErrorMessage = GetTranslation("Settings.Customize.LanguageNotSelected", "本地化管理器未初始化或未选择语言");
            return;
        }

        try
        {
            // ILocalizationManager.ChangeLanguage 是同步方法，但为了保持异步签名，使用 Task.Run
            await Task.Run(() => _localizationManager.ChangeLanguage(SelectedLanguage.Code));

            // 使用本地化文本
            var successKey = GetTranslation("Settings.Customize.LanguageChanged", "语言已切换为 {0}");
            SuccessMessage = string.Format(successKey, SelectedLanguage.Name);
            ErrorMessage = null;

            // 3秒后清除成功消息
            _ = Task.Delay(3000).ContinueWith(_ =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    SuccessMessage = null;
                });
            });
        }
        catch (Exception ex)
        {
            var errorKey = GetTranslation("Settings.Customize.LanguageChangeFailed", "切换语言失败：{0}");
            ErrorMessage = string.Format(errorKey, ex.Message);
            SuccessMessage = null;
        }
    }

    /// <summary>
    /// 当 SelectedTheme 改变时自动应用主题（与 Wpf.Ui.Gallery 完全一致，纯粹的主题切换功能）
    /// </summary>
    partial void OnSelectedThemeChanged(ThemeModeOption? oldValue, ThemeModeOption? newValue)
    {
        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();

        // 防止初始化时的触发和循环更新
        if (_isUpdatingTheme || newValue == null)
        {
            if (_isUpdatingTheme)
            {
                operLog?.Debug("[设置] 主题变更被忽略（正在更新中）: {OldValue} -> {NewValue}",
                    oldValue?.DisplayName ?? "null", newValue?.DisplayName ?? "null");
            }
            return;
        }

        try
        {
            var themeMode = newValue.Mode;
            if (themeMode == System.Windows.ThemeMode.None)
            {
                themeMode = System.Windows.ThemeMode.System;
            }

            _isUpdatingTheme = true;
            try
            {
                _themeService?.SetTheme(themeMode);
            }
            finally
            {
                _isUpdatingTheme = false;
            }
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[设置] 切换主题时发生异常");
            var errorKey = GetTranslation("Settings.Customize.ThemeChangeFailed", "切换主题失败：{0}");
            ErrorMessage = string.Format(errorKey, ex.Message);
            SuccessMessage = null;
        }
        finally
        {
            // 主题切换完成后，自动清除提示
            _ = Task.Delay(3000).ContinueWith(_ =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    SuccessMessage = null;
                });
            });
        }
    }

    /// <summary>
    /// 保存字体设置命令
    /// </summary>
    [RelayCommand]
    private Task SaveFontFamilyAsync()
    {
        if (SelectedFontFamily == null)
        {
            ErrorMessage = GetTranslation("Settings.Customize.FontFamilyNotSelected", "请选择字体");
            return Task.CompletedTask;
        }

        try
        {
            // 保存字体设置
            AppSettingsHelper.SaveFontFamily(SelectedFontFamily.Source);

            // 应用字体设置（更新全局字体资源）
            System.Windows.Application.Current.Resources["ApplicationFontFamily"] = SelectedFontFamily;

            var successKey = GetTranslation("Settings.Customize.FontFamilyChanged", "字体已切换为 {0}");
            SuccessMessage = string.Format(successKey, SelectedFontFamily.Source);
            ErrorMessage = null;

            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Information("[设置] 字体已保存并应用: {FontFamily}", SelectedFontFamily.Source);

            // 3秒后清除成功消息
            _ = Task.Delay(3000).ContinueWith(_ =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    SuccessMessage = null;
                });
            });
        }
        catch (Exception ex)
        {
            var errorKey = GetTranslation("Settings.Customize.FontFamilyChangeFailed", "切换字体失败：{0}");
            ErrorMessage = string.Format(errorKey, ex.Message);
            SuccessMessage = null;

            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[设置] 保存字体失败");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 保存字体大小设置命令
    /// </summary>
    [RelayCommand]
    private Task SaveFontSizeAsync()
    {
        if (SelectedFontSize <= 0)
        {
            ErrorMessage = GetTranslation("Settings.Customize.FontSizeNotSelected", "请选择字体大小");
            return Task.CompletedTask;
        }

        try
        {
            // 保存字体大小设置
            AppSettingsHelper.SaveFontSize(SelectedFontSize);

            // 应用字体大小设置（更新全局字体大小资源）
            System.Windows.Application.Current.Resources["ApplicationFontSize"] = SelectedFontSize;

            var successKey = GetTranslation("Settings.Customize.FontSizeChanged", "字体大小已切换为 {0}");
            SuccessMessage = string.Format(successKey, SelectedFontSize);
            ErrorMessage = null;

            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Information("[设置] 字体大小已保存并应用: {FontSize}", SelectedFontSize);

            // 3秒后清除成功消息
            _ = Task.Delay(3000).ContinueWith(_ =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    SuccessMessage = null;
                });
            });
        }
        catch (Exception ex)
        {
            var errorKey = GetTranslation("Settings.Customize.FontSizeChangeFailed", "切换字体大小失败：{0}");
            ErrorMessage = string.Format(errorKey, ex.Message);
            SuccessMessage = null;

            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[设置] 保存字体大小失败");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 加载字体大小列表
    /// </summary>
    private void LoadFontSizes()
    {
        try
        {
            AvailableFontSizes.Clear();

            // 常用字体大小列表
            var fontSizes = new[] { 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 18.0, 20.0, 24.0 };

            foreach (var size in fontSizes)
            {
                AvailableFontSizes.Add(size);
            }

            // 设置当前选中的字体大小
            var savedFontSize = AppSettingsHelper.GetFontSize();
            if (savedFontSize > 0)
            {
                if (AvailableFontSizes.Contains(savedFontSize))
                {
                    SelectedFontSize = savedFontSize;
                }
                else
                {
                    // 查找最接近的字体大小
                    var closestSize = AvailableFontSizes.FirstOrDefault(s => s >= savedFontSize);
                    SelectedFontSize = closestSize > 0 ? closestSize : AvailableFontSizes[4]; // 默认 14
                }
            }
            else
            {
                // 默认字体大小：14
                SelectedFontSize = 14.0;
            }

            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Debug("[设置] 加载字体大小列表完成，当前选中: {FontSize}", SelectedFontSize);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[设置] 加载字体大小列表失败");
        }
    }

    /// <summary>
    /// 加载系统字体列表（仅包含 Segoe UI 变体）
    /// </summary>
    private void LoadFontFamilies()
    {
        try
        {
            AvailableFontFamilies.Clear();

            // 包含所有 Segoe UI 变体（按显示顺序）
            var segoeUIFonts = new[]
            {
                "Segoe UI",              // Normal (400) - 正文文本、默认字体
                "Segoe UI Light",        // Light (300) - 标题、大字号显示
                "Segoe UI Semilight",    // 350 - 副标题、强调文本
                "Segoe UI Semibold",     // Semibold (600) - 按钮文字、重要标签
                "Segoe UI Bold",         // Bold (700) - 主要标题、重要提示
                "Segoe UI Black",        // Black (900) - 大型展示性文字
                "Segoe UI Variable",     // 可变字体（Windows 11+）
                "Segoe UI Historic",     // 支持历史字符
                "Segoe UI Symbol",       // 符号和图标
                "Segoe UI Emoji"         // 表情符号
            };

            // 获取所有系统字体
            var allFonts = Fonts.SystemFontFamilies.ToList();

            // 只添加指定的 Segoe UI 字体变体
            // 注意：FontFamily.Source 可能包含完整路径或字体族名称，需要灵活匹配
            foreach (var fontName in segoeUIFonts)
            {
                // 尝试精确匹配
                var font = allFonts.FirstOrDefault(f =>
                    f.Source.Equals(fontName, StringComparison.OrdinalIgnoreCase) ||
                    f.Source.EndsWith(fontName, StringComparison.OrdinalIgnoreCase) ||
                    f.FamilyNames.Values.Any(name => name.Equals(fontName, StringComparison.OrdinalIgnoreCase)));

                if (font != null && !AvailableFontFamilies.Contains(font))
                {
                    AvailableFontFamilies.Add(font);
                }
            }

            // 如果匹配到的字体少于预期，尝试查找所有包含 "Segoe UI" 的字体
            if (AvailableFontFamilies.Count < segoeUIFonts.Length)
            {
                var log = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
                log?.Warning("[设置] 只找到 {FoundCount} 个字体，预期 {ExpectedCount} 个。尝试查找所有 Segoe UI 字体变体",
                    AvailableFontFamilies.Count, segoeUIFonts.Length);

                // 查找所有以 "Segoe UI" 开头的字体族
                var allSegoeUIFonts = allFonts.Where(f =>
                    f.Source.StartsWith("Segoe UI", StringComparison.OrdinalIgnoreCase) ||
                    f.FamilyNames.Values.Any(name => name.StartsWith("Segoe UI", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // 添加未包含的字体
                foreach (var font in allSegoeUIFonts)
                {
                    if (!AvailableFontFamilies.Contains(font))
                    {
                        AvailableFontFamilies.Add(font);
                    }
                }

                // 按预定义的顺序排序
                var sortedFonts = new List<FontFamily>();
                foreach (var fontName in segoeUIFonts)
                {
                    var font = AvailableFontFamilies.FirstOrDefault(f =>
                        f.Source.Contains(fontName, StringComparison.OrdinalIgnoreCase) ||
                        f.FamilyNames.Values.Any(name => name.Contains(fontName, StringComparison.OrdinalIgnoreCase)));
                    if (font != null)
                    {
                        sortedFonts.Add(font);
                    }
                }

                // 添加未匹配的字体到末尾
                foreach (var font in AvailableFontFamilies)
                {
                    if (!sortedFonts.Contains(font))
                    {
                        sortedFonts.Add(font);
                    }
                }

                AvailableFontFamilies.Clear();
                foreach (var font in sortedFonts)
                {
                    AvailableFontFamilies.Add(font);
                }
            }

            // 设置当前选中的字体
            var savedFontFamily = AppSettingsHelper.GetFontFamily();
            if (!string.IsNullOrWhiteSpace(savedFontFamily))
            {
                SelectedFontFamily = AvailableFontFamilies
                    .FirstOrDefault(f => f.Source.Equals(savedFontFamily, StringComparison.OrdinalIgnoreCase));
            }

            // 如果没有保存的字体或找不到，使用默认字体（优先使用 Segoe UI）
            if (SelectedFontFamily == null)
            {
                SelectedFontFamily = AvailableFontFamilies
                    .FirstOrDefault(f => f.Source.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
                    ?? AvailableFontFamilies.FirstOrDefault();
            }

            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Debug("[设置] 加载字体列表完成，共 {Count} 个字体，当前选中: {FontFamily}",
                AvailableFontFamilies.Count, SelectedFontFamily?.Source ?? "null");
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[设置] 加载字体列表失败");
        }
    }

    private bool _disposed = false;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源的实现
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // 安全地取消订阅事件
            try
            {
                // 尝试在 UI 线程中取消订阅，但如果应用已关闭则忽略异常
                var app = System.Windows.Application.Current;
                if (app?.Dispatcher != null && !app.Dispatcher.HasShutdownStarted)
                {
                    if (app.Dispatcher.CheckAccess())
                    {
                        // 当前在 UI 线程，直接取消订阅
                        UnsubscribeEvents();
                    }
                    else
                    {
                        // 不在 UI 线程，尝试异步调用（如果失败则忽略）
                        try
                        {
                            app.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    UnsubscribeEvents();
                                }
                                catch
                                {
                                    // 忽略取消订阅时的异常
                                }
                            }), System.Windows.Threading.DispatcherPriority.Normal);
                        }
                        catch
                        {
                            // Dispatcher 可能已关闭，直接尝试取消订阅
                            try
                            {
                                UnsubscribeEvents();
                            }
                            catch
                            {
                                // 忽略所有异常
                            }
                        }
                    }
                }
                else
                {
                    // Application 已关闭或 Dispatcher 不可用，直接尝试取消订阅（可能失败，但可以忽略）
                    try
                    {
                        UnsubscribeEvents();
                    }
                    catch
                    {
                        // 忽略异常，因为应用可能已经关闭
                    }
                }
            }
            catch
            {
                // 忽略所有异常，确保 Dispose 不会抛出
            }
        }

        _disposed = true;
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (_themeService != null)
        {
            try
            {
                _themeService.ThemeChanged -= OnThemeServiceThemeChanged;
            }
            catch
            {
                // 忽略取消订阅时的异常
            }
        }

        if (_localizationManager != null)
        {
            try
            {
                _localizationManager.LanguageChanged -= OnLanguageChanged;
            }
            catch
            {
                // 忽略取消订阅时的异常
            }
        }
    }

    /// <summary>
    /// 析构函数（作为备用，但优先使用 Dispose）
    /// </summary>
    ~MySettingsViewModel()
    {
        Dispose(false);
    }
}

/// <summary>
/// 主题模式选项
/// </summary>
public partial class ThemeModeOption : ObservableObject
{
    [ObservableProperty]
    private System.Windows.ThemeMode _mode;

    [ObservableProperty]
    private string _displayName = string.Empty;
}
#pragma warning restore WPF0001

