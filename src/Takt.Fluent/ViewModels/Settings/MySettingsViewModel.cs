//===================================================================
// 项目名 : Takt.Fluent
// 文件名 : MySettingsViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 2.0
// 描述    : 用户自定义设置视图模型（语言、主题等）
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用
using System.Windows;
using Takt.Application.Dtos.Routine;
using Takt.Common.Helpers;
using Takt.Domain.Interfaces;
using Takt.Fluent.Services;
using TaktMessageManager = Takt.Fluent.Controls.TaktMessageManager;

namespace Takt.Fluent.ViewModels.Settings;

/// <summary>
/// 用户自定义设置视图模型
/// 用于管理用户的个人设置（语言、主题等）
/// </summary>
public partial class MySettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

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

    private readonly ILocalizationManager? _localizationManager;
    private readonly ThemeService? _themeService;

    public MySettingsViewModel(ILocalizationManager? localizationManager = null!, ThemeService? themeService = null)
    {
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _themeService = themeService ?? App.Services?.GetService<ThemeService>();

        // 初始化主题选项
        InitializeThemeOptions();
    }


    private void InitializeThemeOptions()
    {
        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();

        AvailableThemes.Clear();

        // 使用本地化文本（统一使用 common.theme.* 键）
        var systemText = _localizationManager?.GetString("common.theme.system");
        var lightText = _localizationManager?.GetString("common.theme.light");
        var darkText = _localizationManager?.GetString("common.theme.dark");

        AvailableThemes.Add(new ThemeModeOption { Mode = System.Windows.ThemeMode.System, DisplayName = systemText ?? string.Empty });
        AvailableThemes.Add(new ThemeModeOption { Mode = System.Windows.ThemeMode.Light, DisplayName = lightText ?? string.Empty });
        AvailableThemes.Add(new ThemeModeOption { Mode = System.Windows.ThemeMode.Dark, DisplayName = darkText ?? string.Empty });

        // 读取当前主题并同步到 UI
        var mode = _themeService?.GetCurrentTheme() ?? System.Windows.ThemeMode.System;
        var target = AvailableThemes.FirstOrDefault(t => t.Mode == mode) ?? AvailableThemes[0];
        SelectedTheme = target;
        operLog?.Debug("[设置] InitializeThemeOptions - 当前主题: {DisplayName}", target.DisplayName);
    }

    public Task LoadAsync()
    {
        IsLoading = true;

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
                SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Mode == mode) ?? AvailableThemes.First();
            }

            // 加载系统字体列表
            LoadFontFamilies();

            // 加载字体大小列表
            LoadFontSizes();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[设置] 加载设置失败");
        }
        finally
        {
            IsLoading = false;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 保存所有设置命令（统一保存，重启后生效）
    /// </summary>
    [RelayCommand]
    private Task SaveAllSettingsAsync()
    {
        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();

        try
        {
            // 1. 保存主题设置（仅保存到配置，不立即应用）
            if (SelectedTheme != null)
            {
                var themeMode = SelectedTheme.Mode;
                if (themeMode == System.Windows.ThemeMode.None)
                {
                    themeMode = System.Windows.ThemeMode.System;
                }

                string themeString;
                if (themeMode == System.Windows.ThemeMode.Light)
                {
                    themeString = "Light";
                }
                else if (themeMode == System.Windows.ThemeMode.Dark)
                {
                    themeString = "Dark";
                }
                else
                {
                    themeString = "Takt365"; // System
                }

                AppSettingsHelper.SaveTheme(themeString);
                operLog?.Information("[设置] 主题已保存到配置: {Theme}（重启后生效）", themeString);
            }

            // 2. 保存语言设置（仅保存到配置，不立即应用）
            if (SelectedLanguage != null)
            {
                AppSettingsHelper.SaveLanguage(SelectedLanguage.Code);
                operLog?.Information("[设置] 语言已保存到配置: {Language}（重启后生效）", SelectedLanguage.Code);
            }

            // 3. 保存字体设置（仅保存到配置，不立即应用）
            if (SelectedFontFamily != null)
            {
                AppSettingsHelper.SaveFontFamily(SelectedFontFamily.Source);
                operLog?.Information("[设置] 字体已保存到配置: {FontFamily}（重启后生效）", SelectedFontFamily.Source);
            }

            // 4. 保存字体大小设置（仅保存到配置，不立即应用）
            if (SelectedFontSize > 0)
            {
                AppSettingsHelper.SaveFontSize(SelectedFontSize);
                operLog?.Information("[设置] 字体大小已保存到配置: {FontSize}（重启后生效）", SelectedFontSize);
            }

            operLog?.Information("[设置] 所有设置已保存到配置，重启应用后生效");

            // 显示成功消息框，提示重启后生效
            var restartMessage = _localizationManager?.GetString("settings.customize.restartrequired");
            var title = _localizationManager?.GetString("settings.customize.savesuccess");
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                TaktMessageManager.ShowMessageBox(restartMessage ?? string.Empty, title ?? string.Empty, System.Windows.MessageBoxImage.Information);
            });
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[设置] 保存所有设置失败");
            
            // 显示错误消息框
            var errorMessage = _localizationManager?.GetString("common.failed.save");
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                TaktMessageManager.ShowMessageBox(string.Format(errorMessage ?? string.Empty, ex.Message), null, System.Windows.MessageBoxImage.Error);
            });
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

