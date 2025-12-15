//===================================================================
// 项目名 : Takt.Fluent
// 文件名 : ThemeService.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-29
// 版本号 : 4.0
// 描述    : 主题切换服务（使用 WPF-UI 的 ApplicationThemeManager，与 Wpf.Ui.Gallery 完全一致）
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用
using System.IO;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Media;
using Takt.Common.Helpers;

namespace Takt.Fluent.Services;

/// <summary>
/// ThemeMode JSON 转换器（处理 ThemeMode 结构体的序列化/反序列化）
/// </summary>
internal class ThemeModeJsonConverter : JsonConverter<System.Windows.ThemeMode>
{
    public override System.Windows.ThemeMode ReadJson(JsonReader reader, Type objectType, System.Windows.ThemeMode existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var value = reader.Value?.ToString();
            return ParseThemeModeString(value);
        }
        else if (reader.TokenType == JsonToken.StartObject)
        {
            // 处理 { "Value": "Dark" } 格式（向后兼容旧格式）
            var obj = JObject.Load(reader);
            if (obj["Value"] != null)
            {
                var value = obj["Value"]!.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    return ParseThemeModeString(value);
                }
            }
        }
        
        return System.Windows.ThemeMode.System;
    }
    
    private static System.Windows.ThemeMode ParseThemeModeString(string? value)
    {
        return value switch
        {
            "Light" => System.Windows.ThemeMode.Light,
            "Dark" => System.Windows.ThemeMode.Dark,
            "Takt365" => System.Windows.ThemeMode.System,
            _ => System.Windows.ThemeMode.System
        };
    }

    public override void WriteJson(JsonWriter writer, System.Windows.ThemeMode value, JsonSerializer serializer)
    {
        string stringValue;
        if (value == System.Windows.ThemeMode.Light)
        {
            stringValue = "Light";
        }
        else if (value == System.Windows.ThemeMode.Dark)
        {
            stringValue = "Dark";
        }
        else if (value == System.Windows.ThemeMode.System)
        {
            stringValue = "Takt365";
        }
        else
        {
            stringValue = "Takt365";
        }
        writer.WriteValue(stringValue);
    }
}

/// <summary>
/// 主题服务，用于管理应用主题切换（基于 WPF-UI ApplicationThemeManager，与 Wpf.Ui.Gallery 完全一致）
/// </summary>
public class ThemeService
{
    // 内存缓存，确保 GetCurrentTheme() 立即返回最新值
    private static System.Windows.ThemeMode? _cachedThemeMode = null;
    private static readonly object _cacheLock = new object();
    private readonly PaletteHelper _paletteHelper = new();

    public event EventHandler<System.Windows.ThemeMode>? ThemeChanged;

    public ThemeService()
    {
        // PaletteHelper 自动管理主题
    }

    /// <summary>
    /// 获取当前主题模式（优先从内存缓存读取，确保立即返回最新值）
    /// 因为用户可能选择了 System 模式，但实际应用的是 Light 或 Dark
    /// </summary>
    public System.Windows.ThemeMode GetCurrentTheme()
    {
        lock (_cacheLock)
        {
            // 如果缓存存在，直接返回
            if (_cachedThemeMode.HasValue)
            {
                return _cachedThemeMode.Value;
            }
            
            // 否则从文件读取并更新缓存
            var themeMode = LoadThemeFromFile();
            _cachedThemeMode = themeMode;
            return themeMode;
        }
    }

    /// <summary>
    /// 设置主题模式，并应用到 Material Design Palette
    /// </summary>
    public void SetTheme(System.Windows.ThemeMode themeMode)
    {
        try
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Debug("[主题] 设置主题: {ThemeMode}", themeMode);

            var actualMode = ResolveActualThemeMode(themeMode);

            ApplyPaletteTheme(actualMode);
            UpdateLegacyBrushes(actualMode);

            lock (_cacheLock)
            {
                _cachedThemeMode = themeMode;
            }

            SaveThemeToFile(themeMode);

            operLog?.Debug("[主题] 主题设置完成，缓存已更新: {ThemeMode}, 实际应用主题: {ActualMode}", themeMode, actualMode);

            ThemeChanged?.Invoke(this, actualMode);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[主题] 设置主题失败: {ThemeMode}", themeMode.ToString());
        }
    }

    /// <summary>
    /// 初始化主题（应用启动时调用）
    /// </summary>
    public void InitializeTheme()
    {
        try
        {
            var theme = LoadThemeFromFile();
            
            // 初始化缓存
            lock (_cacheLock)
            {
                _cachedThemeMode = theme;
            }
            
            SetTheme(theme);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[主题] 初始化主题失败");
        }
    }

    /// <summary>
    /// 从配置文件加载主题设置
    /// </summary>
    private static System.Windows.ThemeMode LoadThemeFromFile()
    {
        try
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Debug("[主题] 开始从 taktsettings.json 加载主题");
            
            var themeString = AppSettingsHelper.GetTheme();
            if (!string.IsNullOrWhiteSpace(themeString))
            {
                var themeMode = ParseThemeModeString(themeString);
                operLog?.Debug("[主题] 从配置文件加载主题: {ThemeMode}", themeMode);
                
                // 验证主题模式是否有效（排除 None 值）
                if (themeMode == System.Windows.ThemeMode.None)
                {
                    operLog?.Warning("[主题] 配置文件中的主题为 None，使用默认值 System");
                    return System.Windows.ThemeMode.System;
                }
                
                return themeMode;
            }
            else
            {
                operLog?.Debug("[主题] 配置文件中没有主题设置，使用默认值 System");
            }
        }
        catch (Exception ex)
        {
            // 如果读取失败，记录错误并返回默认值
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Warning("[主题] 从配置文件加载主题失败，使用默认值 System: {Exception}", ex.Message);
        }
        return System.Windows.ThemeMode.System;
    }
    
    /// <summary>
    /// 解析主题模式字符串
    /// </summary>
    private static System.Windows.ThemeMode ParseThemeModeString(string? value)
    {
        return value switch
        {
            "Light" => System.Windows.ThemeMode.Light,
            "Dark" => System.Windows.ThemeMode.Dark,
            "Takt365" => System.Windows.ThemeMode.System,
            _ => System.Windows.ThemeMode.System
        };
    }
    
    /// <summary>
    /// 清除缓存（用于调试或强制重新加载）
    /// </summary>
    public static void ClearCache()
    {
        lock (_cacheLock)
        {
            _cachedThemeMode = null;
        }
    }

    /// <summary>
    /// 保存主题设置到配置文件
    /// </summary>
    private static void SaveThemeToFile(System.Windows.ThemeMode themeMode)
    {
        try
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Debug("[主题] 开始保存主题到 taktsettings.json: {ThemeMode}", themeMode);
            
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
                themeString = "Takt365";
            }
            
            AppSettingsHelper.SaveTheme(themeString);
            operLog?.Debug("[主题] 主题已成功保存到配置文件: {ThemeMode}", themeMode);
        }
        catch (Exception ex)
        {
            // 记录保存失败的错误
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[主题] 保存主题到配置文件失败: {ThemeMode}", themeMode);
        }
    }

    public System.Windows.ThemeMode GetAppliedThemeMode()
    {
        var requested = GetCurrentTheme();
        return ResolveActualThemeMode(requested);
    }

    private void ApplyPaletteTheme(System.Windows.ThemeMode mode)
    {
        var theme = _paletteHelper.GetTheme();
        if (mode == System.Windows.ThemeMode.Dark)
        {
            theme.SetBaseTheme(BaseTheme.Dark);
        }
        else
        {
            theme.SetBaseTheme(BaseTheme.Light);
        }
        _paletteHelper.SetTheme(theme);
    }

    private static System.Windows.ThemeMode ResolveActualThemeMode(System.Windows.ThemeMode requested)
    {
        if (requested == System.Windows.ThemeMode.System)
        {
            return DetectSystemTheme();
        }

        return requested == System.Windows.ThemeMode.Dark
            ? System.Windows.ThemeMode.Dark
            : System.Windows.ThemeMode.Light;
    }

    private static System.Windows.ThemeMode DetectSystemTheme()
    {
        try
        {
            using var personalizeKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (personalizeKey != null)
            {
                var appsUseLightTheme = personalizeKey.GetValue("AppsUseLightTheme");
                if (appsUseLightTheme is int lightValue)
                {
                    return lightValue == 0 ? System.Windows.ThemeMode.Dark : System.Windows.ThemeMode.Light;
                }
            }
        }
        catch
        {
            // ignore
        }

        return System.Windows.ThemeMode.Light;
    }

    private static void UpdateLegacyBrushes(System.Windows.ThemeMode mode)
    {
        var resource = System.Windows.Application.Current?.Resources;
        if (resource == null)
        {
            return;
        }

        var isDark = mode == System.Windows.ThemeMode.Dark;

        static SolidColorBrush Brush(string hex) => new((Color)ColorConverter.ConvertFromString(hex));

        // 基础层
        resource["ApplicationBackgroundBrush"] = Brush(isDark ? "#FF121212" : "#FFF5F5F5");
        resource["LayerFillColorDefaultBrush"] = Brush(isDark ? "#FF1E1E1E" : "#FFFFFFFF");
        resource["LayerFillColorAltBrush"] = Brush(isDark ? "#FF252525" : "#FFF7F7F7");
        resource["CardBackgroundFillColorDefaultBrush"] = Brush(isDark ? "#FF1E1E1E" : "#FFFFFFFF");
        resource["CardStrokeColorDefaultBrush"] = Brush(isDark ? "#FF2F2F2F" : "#FFE0E0E0");

        // 控件背景
        resource["ControlFillColorDefaultBrush"] = Brush(isDark ? "#FF242424" : "#FFFFFFFF");
        resource["ControlFillColorSecondaryBrush"] = Brush(isDark ? "#FF2D2D2D" : "#FFF2F2F2");
        resource["ControlFillColorTertiaryBrush"] = Brush(isDark ? "#FF363636" : "#FFE6E6E6");
        resource["ControlFillColorDisabledBrush"] = Brush(isDark ? "#FF3C3C3C" : "#FFDDDDDD");
        resource["ControlAltFillColorQuarternaryBrush"] = Brush(isDark ? "#FF3B3B3B" : "#FFEDEDED");
        resource["ControlStrokeColorDefaultBrush"] = Brush(isDark ? "#FF3D3D3D" : "#FFCCCCCC");
        resource["ControlStrokeColorSecondaryBrush"] = Brush(isDark ? "#FF474747" : "#FFBDBDBD");

        // 文本
        resource["TextFillColorPrimaryBrush"] = Brush(isDark ? "#FFECECEC" : "#FF1F1F1F");
        resource["TextFillColorSecondaryBrush"] = Brush(isDark ? "#FFBDBDBD" : "#FF616161");
        resource["TextFillColorDisabledBrush"] = Brush(isDark ? "#FF6F6F6F" : "#FF9E9E9E");
        resource["SystemFillColorCriticalBrush"] = Brush(isDark ? "#FFFF5C5C" : "#FFD92D20");

        // Accent 颜色（蓝色系）
        var accent = isDark ? "#FF80CBC4" : "#FF009688";
        var accentLight = isDark ? "#FFB2DFDB" : "#FF4DB6AC";
        var accentDark = isDark ? "#FF4F9A94" : "#FF00796B";

        resource["AccentFillColorDefaultBrush"] = Brush(accent);
        resource["AccentFillColorSecondaryBrush"] = Brush(accentLight);
        resource["AccentFillColorTertiaryBrush"] = Brush(accentDark);
        resource["AccentFillColorControlBrush"] = Brush(accent);

        resource["AccentTextFillColorPrimaryBrush"] = Brush(isDark ? "#FF102A27" : "#FFFFFFFF");
        resource["AccentTextFillColorSecondaryBrush"] = Brush(isDark ? "#FF1F3B38" : "#FFE0F2F1");
        resource["TextOnAccentFillColorPrimaryBrush"] = Brush("#FFFFFFFF");
        resource["TextOnAccentFillColorSecondaryBrush"] = Brush("#FFE0E0E0");

        // 语义颜色（Fluent Design）
        // Success - 绿色
        resource["SuccessFillColorDefaultBrush"] = Brush(isDark ? "#FF54B054" : "#FF107C10");
        resource["SuccessFillColorSecondaryBrush"] = Brush(isDark ? "#FF7ACF7A" : "#FF6FBD6F");
        resource["SuccessTextFillColorPrimaryBrush"] = Brush("#FFFFFFFF");
        
        // Warning - 橙色/琥珀色
        resource["WarningFillColorDefaultBrush"] = Brush(isDark ? "#FFFFB900" : "#FFFF8C00");
        resource["WarningFillColorSecondaryBrush"] = Brush(isDark ? "#FFFFCC4D" : "#FFFFAA33");
        resource["WarningTextFillColorPrimaryBrush"] = Brush("#FFFFFFFF");
        
        // Info - 蓝色
        resource["InfoFillColorDefaultBrush"] = Brush(isDark ? "#FF3AA0F3" : "#FF0078D4");
        resource["InfoFillColorSecondaryBrush"] = Brush(isDark ? "#FF5FB3F5" : "#FF3399FF");
        resource["InfoTextFillColorPrimaryBrush"] = Brush("#FFFFFFFF");
        
        // Info Variant - 紫色/靛蓝色（用于随行人员）
        resource["InfoVariantFillColorDefaultBrush"] = Brush(isDark ? "#FF8B7DD8" : "#FF6366F1");
        resource["InfoVariantFillColorSecondaryBrush"] = Brush(isDark ? "#FFA897E3" : "#FF8184F3");
        resource["InfoVariantTextFillColorPrimaryBrush"] = Brush("#FFFFFFFF");

        // 其他常用资源
        resource["ControlStrongFillColorDefaultBrush"] = Brush(isDark ? "#FF3F3F3F" : "#FFE4E4E4");
        resource["ControlStrongStrokeColorDefaultBrush"] = Brush(isDark ? "#FF565656" : "#FFA6A6A6");
        resource["FocusStrokeColorOuterBrush"] = Brush(isDark ? "#FF64B5F6" : "#FF1976D2");
    }
}

/// <summary>
/// 主题设置数据模型
/// </summary>
internal class ThemeSettings
{
    [JsonConverter(typeof(ThemeModeJsonConverter))]
    public System.Windows.ThemeMode ThemeMode { get; set; } = System.Windows.ThemeMode.System;
}
#pragma warning restore WPF0001

