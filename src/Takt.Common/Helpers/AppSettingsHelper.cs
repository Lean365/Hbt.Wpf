// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Common.Helpers
// 文件名称：AppSettingsHelper.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：应用配置管理帮助类（管理 taktsettings.json）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Takt.Common.Helpers;

/// <summary>
/// 应用配置管理帮助类
/// 管理 taktsettings.json 配置文件，包含所有用户个性化设置
/// </summary>
public static class AppSettingsHelper
{
    private static readonly string SettingsFilePath;
    
    static AppSettingsHelper()
    {
        // 使用符合 Windows 规范的配置目录（AppData\Roaming）
        // 用户个性化配置（主题、语言、字体等）应存储在 Roaming 目录，以便跟随用户账户漫游
        SettingsFilePath = PathHelper.GetRoamingConfigFilePath("taktsettings.json");
    }

    /// <summary>
    /// 应用配置模型
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 主题模式：Light（浅色）、Dark（深色）、System（跟随系统）
        /// </summary>
        [JsonProperty("theme")]
        public string Theme { get; set; } = "Takt365";

        /// <summary>
        /// 语言代码：zh-CN（中文）、en-US（英文）、ja-JP（日文）
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; } = "zh-CN";

        /// <summary>
        /// 字体族（如：Segoe UI、Microsoft YaHei UI）
        /// </summary>
        [JsonProperty("fontFamily")]
        public string FontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// 字体大小（像素）
        /// </summary>
        [JsonProperty("fontSize")]
        public double FontSize { get; set; } = 14.0;

        /// <summary>
        /// 自定义设置（用于存储其他配置项，如登录相关设置）
        /// </summary>
        [JsonProperty("customSettings")]
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public static AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 加载配置失败: {ex.Message}");
        }
        
        return new AppSettings();
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public static void SaveSettings(AppSettings settings)
    {
        try
        {
            var json = JsonConvert.SerializeObject(settings, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                StringEscapeHandling = StringEscapeHandling.Default,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 保存配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取主题模式
    /// </summary>
    public static string GetTheme()
    {
        var settings = LoadSettings();
        return settings.Theme;
    }

    /// <summary>
    /// 保存主题模式
    /// </summary>
    public static void SaveTheme(string theme)
    {
        var settings = LoadSettings();
        settings.Theme = theme;
        SaveSettings(settings);
    }

    /// <summary>
    /// 获取语言代码
    /// </summary>
    public static string GetLanguage()
    {
        var settings = LoadSettings();
        return settings.Language;
    }

    /// <summary>
    /// 保存语言代码
    /// </summary>
    public static void SaveLanguage(string languageCode)
    {
        var settings = LoadSettings();
        settings.Language = languageCode;
        SaveSettings(settings);
    }

    /// <summary>
    /// 获取字体族
    /// </summary>
    public static string GetFontFamily()
    {
        var settings = LoadSettings();
        return settings.FontFamily;
    }

    /// <summary>
    /// 保存字体族
    /// </summary>
    public static void SaveFontFamily(string fontFamily)
    {
        var settings = LoadSettings();
        settings.FontFamily = fontFamily;
        SaveSettings(settings);
    }

    /// <summary>
    /// 获取字体大小
    /// </summary>
    public static double GetFontSize()
    {
        var settings = LoadSettings();
        return settings.FontSize;
    }

    /// <summary>
    /// 保存字体大小
    /// </summary>
    public static void SaveFontSize(double fontSize)
    {
        var settings = LoadSettings();
        settings.FontSize = fontSize;
        SaveSettings(settings);
    }

    /// <summary>
    /// 保存自定义设置项
    /// </summary>
    public static void SaveSetting(string key, string value)
    {
        var settings = LoadSettings();
        settings.CustomSettings[key] = value;
        SaveSettings(settings);
    }

    /// <summary>
    /// 获取自定义设置项
    /// </summary>
    public static string GetSetting(string key, string defaultValue = "")
    {
        var settings = LoadSettings();
        if (settings.CustomSettings.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }
}

