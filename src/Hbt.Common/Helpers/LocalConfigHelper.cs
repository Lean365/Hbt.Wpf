//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : LocalConfigHelper.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 本地配置管理帮助类（用于存储用户个性化设置）
//===================================================================

using System.IO;
using System.Text.Json;

namespace Hbt.Common.Helpers;

/// <summary>
/// 本地配置管理帮助类
/// 用于存储和读取用户的个性化设置（语言、主题等）
/// </summary>
public static class LocalConfigHelper
{
    private static readonly string ConfigFilePath;
    
    static LocalConfigHelper()
    {
        // 使用 LocalApplicationData 目录存储用户配置
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var configDirectory = Path.Combine(appDataPath, "HbtWpf");
        Directory.CreateDirectory(configDirectory);
        ConfigFilePath = Path.Combine(configDirectory, "user-config.json");
    }

    /// <summary>
    /// 用户配置模型
    /// </summary>
    public class UserConfig
    {
        public string Language { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }

    /// <summary>
    /// 保存用户配置
    /// </summary>
    public static void SaveConfig(UserConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LocalConfigHelper] 保存配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 加载用户配置
    /// </summary>
    public static UserConfig LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<UserConfig>(json);
                return config ?? new UserConfig();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LocalConfigHelper] 加载配置失败: {ex.Message}");
        }
        
        return new UserConfig();
    }

    /// <summary>
    /// 保存单个设置项
    /// </summary>
    public static void SaveSetting(string key, string value)
    {
        var config = LoadConfig();
        config.CustomSettings[key] = value;
        SaveConfig(config);
    }

    /// <summary>
    /// 获取单个设置项
    /// </summary>
    public static string GetSetting(string key, string defaultValue = "")
    {
        var config = LoadConfig();
        if (config.CustomSettings.TryGetValue(key, out var value) && value is string strValue)
        {
            return strValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// 保存用户语言设置
    /// </summary>
    public static void SaveLanguage(string languageCode)
    {
        var config = LoadConfig();
        config.Language = languageCode;
        SaveConfig(config);
    }

    /// <summary>
    /// 获取用户语言设置
    /// </summary>
    public static string GetLanguage()
    {
        var config = LoadConfig();
        return config.Language;
    }

    /// <summary>
    /// 保存用户主题设置
    /// </summary>
    public static void SaveTheme(string theme)
    {
        var config = LoadConfig();
        config.Theme = theme;
        SaveConfig(config);
    }

    /// <summary>
    /// 获取用户主题设置
    /// </summary>
    public static string GetTheme()
    {
        var config = LoadConfig();
        return config.Theme;
    }
}

