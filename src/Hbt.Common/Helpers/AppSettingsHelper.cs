//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : AppSettingsHelper.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 应用设置管理帮助类（集中管理语言和主题设置）
//===================================================================

namespace Hbt.Common.Helpers;

/// <summary>
/// 应用设置管理帮助类
/// 提供统一的接口管理用户的语言和主题设置
/// </summary>
public static class AppSettingsHelper
{
    /// <summary>
    /// 获取用户语言设置（优先本地配置，其次系统语言）
    /// </summary>
    public static string GetUserLanguage()
    {
        try
        {
            // 1. 优先读取本地用户配置
            var localLanguage = LocalConfigHelper.GetLanguage();
            if (!string.IsNullOrWhiteSpace(localLanguage))
            {
                System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 读取到本地用户配置语言：{localLanguage}");
                return localLanguage;
            }

            // 2. 如果没有本地配置，获取系统语言
            var systemLanguageCode = SystemInfoHelper.GetSystemLanguageCode();
            var mappedLanguage = MapSystemLanguageToAppLanguage(systemLanguageCode);
            
            System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 系统语言：{systemLanguageCode}，映射为：{mappedLanguage}");
            
            // 3. 保存到本地配置以便下次使用
            LocalConfigHelper.SaveLanguage(mappedLanguage);
            System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 保存系统语言到本地配置：{mappedLanguage}");
            
            return mappedLanguage;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 获取用户语言失败：{ex.Message}");
            return "zh-CN"; // 默认返回中文
        }
    }

    /// <summary>
    /// 保存用户语言设置
    /// </summary>
    public static void SaveUserLanguage(string languageCode)
    {
        LocalConfigHelper.SaveLanguage(languageCode);
        System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 保存用户语言设置：{languageCode}");
    }

    /// <summary>
    /// 获取用户主题设置
    /// </summary>
    public static string GetUserTheme()
    {
        return ThemeManager.GetTheme();
    }

    /// <summary>
    /// 保存用户主题设置
    /// </summary>
    public static void SaveUserTheme(string theme)
    {
        ThemeManager.SaveTheme(theme);
        System.Diagnostics.Debug.WriteLine($"[AppSettingsHelper] 保存用户主题设置：{theme}");
    }

    /// <summary>
    /// 将系统语言映射到应用支持的语言
    /// </summary>
    private static string MapSystemLanguageToAppLanguage(string systemLanguageCode)
    {
        if (string.IsNullOrEmpty(systemLanguageCode))
        {
            return "zh-CN"; // 默认为中文
        }

        var normalizedCode = systemLanguageCode.ToLowerInvariant();

        // 中文相关：zh-CN, zh-Hans, zh-TW, zh-Hant 等都映射到 zh-CN
        if (normalizedCode.StartsWith("zh"))
        {
            return "zh-CN";
        }
        
        // 日文相关：ja, ja-JP 都映射到 ja-JP
        if (normalizedCode.StartsWith("ja"))
        {
            return "ja-JP";
        }
        
        // 其他所有语言都映射到 en-US
        return "en-US";
    }
}

