//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ThemeManager.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 主题管理器（用于管理用户主题选择并持久化）
//===================================================================

namespace Hbt.Common.Helpers;

/// <summary>
/// 主题管理器
/// 用于管理和持久化用户的主题选择
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// 主题常量
    /// </summary>
    public static class Theme
    {
        public const string Light = "Light";
        public const string Dark = "Dark";
        public const string Default = Light;
    }

    /// <summary>
    /// 获取用户选择的主题
    /// </summary>
    public static string GetTheme()
    {
        var theme = LocalConfigHelper.GetTheme();
        if (string.IsNullOrWhiteSpace(theme))
        {
            // 如果本地没有保存，返回默认主题
            return Theme.Default;
        }
        return theme;
    }

    /// <summary>
    /// 保存用户选择的主题
    /// </summary>
    public static void SaveTheme(string theme)
    {
        LocalConfigHelper.SaveTheme(theme);
    }

    /// <summary>
    /// 切换主题
    /// </summary>
    public static string ToggleTheme()
    {
        var currentTheme = GetTheme();
        var newTheme = currentTheme == Theme.Light ? Theme.Dark : Theme.Light;
        SaveTheme(newTheme);
        return newTheme;
    }
}

