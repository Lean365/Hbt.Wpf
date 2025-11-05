//===================================================================
// 项目名 : Hbt.Fluent
// 文件名 : ThemeService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-29
// 版本号 : 1.0
// 描述    : 主题切换服务
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用
using System.IO;
using System.Text.Json;

namespace Hbt.Fluent.Services;

/// <summary>
/// 主题服务，用于管理应用主题切换
/// </summary>
public class ThemeService
{
    private const string SettingsFileName = "theme-settings.json";
    private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);

    /// <summary>
    /// 获取当前主题模式
    /// </summary>
    public System.Windows.ThemeMode GetCurrentTheme()
    {
        // 优先从 Application.Current 获取，确保与实际一致
        if (System.Windows.Application.Current != null)
        {
            return System.Windows.Application.Current.ThemeMode;
        }

        // 如果 Application.Current 不可用，从文件读取
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var options = new JsonSerializerOptions
                {
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };
                var settings = JsonSerializer.Deserialize<ThemeSettings>(json, options);
                return settings?.ThemeMode ?? System.Windows.ThemeMode.System;
            }
        }
        catch
        {
            // 如果读取失败，返回默认值
        }
        return System.Windows.ThemeMode.System;
    }

    /// <summary>
    /// 设置主题模式
    /// </summary>
    public void SetTheme(System.Windows.ThemeMode themeMode)
    {
        if (System.Windows.Application.Current == null)
        {
            return;
        }

        // 设置主题
        var previousTheme = System.Windows.Application.Current.ThemeMode;
        System.Windows.Application.Current.ThemeMode = themeMode;
        var actualTheme = System.Windows.Application.Current.ThemeMode;
        
        // 验证主题是否设置成功
        if (actualTheme != themeMode)
        {
            // 主题设置失败，记录但不抛出异常
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Warning("[主题] ThemeMode 设置失败，期望: {ExpectedTheme}，实际: {ActualTheme}", themeMode, actualTheme);
        }
        
        // 保存设置
        try
        {
            var settings = new ThemeSettings { ThemeMode = themeMode };
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // 如果保存失败，忽略错误
        }
    }

    /// <summary>
    /// 初始化主题（应用启动时调用）
    /// </summary>
    public void InitializeTheme()
    {
        if (System.Windows.Application.Current == null)
        {
            return; // Application 还未初始化，稍后再设置
        }

        var theme = GetCurrentTheme();
        System.Windows.Application.Current.ThemeMode = theme;
    }
}

/// <summary>
/// 主题设置数据模型
/// </summary>
internal class ThemeSettings
{
    public System.Windows.ThemeMode ThemeMode { get; set; } = System.Windows.ThemeMode.System;
}
#pragma warning restore WPF0001

