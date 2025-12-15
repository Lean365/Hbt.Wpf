// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：ResourceFileLocalizationHelper.cs
// 创建时间：2025-12-12
// 创建人：Takt365(Cursor AI)
// 功能描述：资源文件本地化辅助类（用于数据库连接失败时的本地化）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Globalization;
using System.Resources;
using System.Reflection;
using Takt.Common.Helpers;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 资源文件本地化辅助类
/// 用于在数据库连接失败时从资源文件读取本地化字符串
/// </summary>
public static class ResourceFileLocalizationHelper
{
    private static ResourceManager? _resourceManager;

    /// <summary>
    /// 获取资源管理器（延迟初始化）
    /// </summary>
    private static ResourceManager ResourceManager
    {
        get
        {
            if (_resourceManager == null)
            {
                _resourceManager = new ResourceManager(
                    "Takt.Fluent.Resources.Localization.Resources",
                    Assembly.GetExecutingAssembly());
            }
            return _resourceManager;
        }
    }

    /// <summary>
    /// 获取当前系统语言代码
    /// </summary>
    private static string GetCurrentLanguageCode()
    {
        // 使用 SystemInfoHelper 获取系统语言代码
        var languageCode = SystemInfoHelper.GetSystemLanguageCode();
        
        // 如果当前语言不在支持列表中，默认使用中文
        if (languageCode != "zh-CN" && languageCode != "en-US" && languageCode != "ja-JP")
        {
            // 检查是否以支持的语言开头（如 zh, en, ja）
            if (languageCode.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                return "zh-CN";
            else if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return "en-US";
            else if (languageCode.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
                return "ja-JP";
            else
                return "zh-CN"; // 默认使用中文
        }
        
        return languageCode;
    }

    /// <summary>
    /// 从资源文件获取本地化字符串
    /// </summary>
    /// <param name="key">翻译键</param>
    /// <returns>翻译值，如果找不到则返回键本身</returns>
    public static string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        try
        {
            var culture = new CultureInfo(GetCurrentLanguageCode());
            var value = ResourceManager.GetString(key, culture);
            
            // 如果找到了值，进行参数替换
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            
            // 如果找不到，尝试从默认资源文件读取（通常是第一个资源文件）
            value = ResourceManager.GetString(key);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            
            // 如果还是找不到，返回键本身
            return key;
        }
        catch
        {
            // 发生任何异常时，返回键本身
            return key;
        }
    }

    /// <summary>
    /// 从资源文件获取本地化字符串，并格式化参数
    /// </summary>
    /// <param name="key">翻译键</param>
    /// <param name="args">格式化参数</param>
    /// <returns>翻译值，如果找不到则返回键本身</returns>
    public static string GetString(string key, params object[] args)
    {
        var value = GetString(key);
        
        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(value, args);
            }
            catch
            {
                // 格式化失败时，返回原始值
                return value;
            }
        }
        
        return value;
    }
}

