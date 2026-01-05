// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Common.Helpers
// 文件名称：PathHelper.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：路径辅助类，提供符合 Windows 规范的应用程序路径（日志、配置等）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Common.Helpers;

/// <summary>
/// 路径辅助类
/// 提供符合 Windows 规范的应用程序路径（日志、配置等）
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// 公司名称（用于构建 AppData 路径）
    /// </summary>
    private const string CompanyName = "Takt";

    /// <summary>
    /// 应用程序名称（用于构建 AppData 路径）
    /// </summary>
    private const string ApplicationName = "Takt SMEs";

    #region 日志路径

    /// <summary>
    /// 获取标准的日志目录路径
    /// 路径格式：C:\Users\{UserName}\AppData\Local\{CompanyName}\{ApplicationName}\Logs
    /// </summary>
    /// <returns>日志目录的完整路径</returns>
    /// <remarks>
    /// 使用 Environment.SpecialFolder.LocalApplicationData 获取 AppData\Local 目录
    /// 这是 Windows 应用程序存储本地数据（如日志）的标准位置
    /// </remarks>
    public static string GetLogDirectory()
    {
        // 获取当前用户的 AppData\Local 目录
        var appDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // 构建规范的日志目录路径
        // 格式：...\AppData\Local\{公司名}\{应用名}\Logs\
        var logDirectory = Path.Combine(appDataLocalPath, CompanyName, ApplicationName, "Logs");

        // 确保目录存在
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        return logDirectory;
    }

    /// <summary>
    /// 获取指定日志文件的完整路径
    /// </summary>
    /// <param name="logFileName">日志文件名（支持 Serilog 的日期占位符，如 "app-.txt"）</param>
    /// <returns>日志文件的完整路径</returns>
    public static string GetLogFilePath(string logFileName)
    {
        return Path.Combine(GetLogDirectory(), logFileName);
    }

    #endregion

    #region 配置路径

    /// <summary>
    /// 获取用户漫游配置目录路径
    /// 路径格式：C:\Users\{UserName}\AppData\Roaming\{CompanyName}\{ApplicationName}
    /// </summary>
    /// <returns>用户漫游配置目录的完整路径</returns>
    /// <remarks>
    /// 使用 Environment.SpecialFolder.ApplicationData 获取 AppData\Roaming 目录
    /// 适用于用户个性化配置（主题、语言、窗口位置等），这些配置会跟随用户账户漫游
    /// </remarks>
    public static string GetRoamingConfigDirectory()
    {
        // 获取当前用户的 AppData\Roaming 目录
        var appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // 构建规范的配置目录路径
        // 格式：...\AppData\Roaming\{公司名}\{应用名}\
        var configDirectory = Path.Combine(appDataRoamingPath, CompanyName, ApplicationName);

        // 确保目录存在
        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        return configDirectory;
    }

    /// <summary>
    /// 获取用户本地配置目录路径
    /// 路径格式：C:\Users\{UserName}\AppData\Local\{CompanyName}\{ApplicationName}
    /// </summary>
    /// <returns>用户本地配置目录的完整路径</returns>
    /// <remarks>
    /// 使用 Environment.SpecialFolder.LocalApplicationData 获取 AppData\Local 目录
    /// 适用于本地缓存配置、临时设置等，这些配置不会在域网络中漫游
    /// </remarks>
    public static string GetLocalConfigDirectory()
    {
        // 获取当前用户的 AppData\Local 目录
        var appDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // 构建规范的配置目录路径
        // 格式：...\AppData\Local\{公司名}\{应用名}\
        var configDirectory = Path.Combine(appDataLocalPath, CompanyName, ApplicationName);

        // 确保目录存在
        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        return configDirectory;
    }

    /// <summary>
    /// 获取用户漫游配置文件的完整路径
    /// </summary>
    /// <param name="configFileName">配置文件名（如 "taktsettings.json"）</param>
    /// <returns>配置文件的完整路径</returns>
    public static string GetRoamingConfigFilePath(string configFileName)
    {
        return Path.Combine(GetRoamingConfigDirectory(), configFileName);
    }

    /// <summary>
    /// 获取用户本地配置文件的完整路径
    /// </summary>
    /// <param name="configFileName">配置文件名（如 "cache.json"）</param>
    /// <returns>配置文件的完整路径</returns>
    public static string GetLocalConfigFilePath(string configFileName)
    {
        return Path.Combine(GetLocalConfigDirectory(), configFileName);
    }

    #endregion

    #region 代码模板路径

    /// <summary>
    /// 获取默认代码模板目录路径（应用目录）
    /// 路径格式：{AppDirectory}\Assets\Generator
    /// </summary>
    /// <returns>默认代码模板目录的完整路径</returns>
    /// <remarks>
    /// 默认模板是应用的一部分，存储在应用目录下，通常是只读的
    /// 如果应用安装在 Program Files，可能需要管理员权限才能修改
    /// </remarks>
    public static string GetDefaultTemplateDirectory()
    {
        // 获取应用目录
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // 构建默认模板目录路径
        // 格式：{应用目录}\Assets\Generator\
        var templateDirectory = Path.Combine(appDirectory, "Assets", "Generator");

        // 确保目录存在
        if (!Directory.Exists(templateDirectory))
        {
            Directory.CreateDirectory(templateDirectory);
        }

        return templateDirectory;
    }

    /// <summary>
    /// 获取用户自定义代码模板目录路径（AppData\Roaming）
    /// 路径格式：C:\Users\{UserName}\AppData\Roaming\{CompanyName}\{ApplicationName}\Templates
    /// </summary>
    /// <returns>用户自定义代码模板目录的完整路径</returns>
    /// <remarks>
    /// 用户自定义模板存储在 AppData\Roaming 目录，用户可以编辑和自定义
    /// 这些模板会跟随用户账户漫游，适合存储用户自定义的代码生成模板
    /// </remarks>
    public static string GetUserTemplateDirectory()
    {
        // 获取用户漫游配置目录
        var roamingConfigDir = GetRoamingConfigDirectory();

        // 构建用户模板目录路径
        // 格式：...\AppData\Roaming\{公司名}\{应用名}\Templates\
        var templateDirectory = Path.Combine(roamingConfigDir, "Templates");

        // 确保目录存在
        if (!Directory.Exists(templateDirectory))
        {
            Directory.CreateDirectory(templateDirectory);
        }

        return templateDirectory;
    }

    /// <summary>
    /// 获取代码模板目录路径（优先使用用户自定义模板，如果不存在则使用默认模板）
    /// </summary>
    /// <param name="templateType">模板类型（如 "CRUD"、"MasterDetail"、"Tree"）</param>
    /// <returns>模板目录的完整路径</returns>
    /// <remarks>
    /// 优先查找用户自定义模板目录，如果不存在则使用默认模板目录
    /// 这样用户可以通过自定义模板覆盖默认模板
    /// </remarks>
    public static string GetTemplateDirectory(string? templateType = null)
    {
        // 优先使用用户自定义模板目录
        var userTemplateDir = GetUserTemplateDirectory();
        if (!string.IsNullOrWhiteSpace(templateType))
        {
            userTemplateDir = Path.Combine(userTemplateDir, templateType);
        }

        // 如果用户模板目录存在且有文件，则使用用户模板
        if (Directory.Exists(userTemplateDir) && Directory.GetFiles(userTemplateDir, "*.sbn", SearchOption.TopDirectoryOnly).Length > 0)
        {
            return userTemplateDir;
        }

        // 否则使用默认模板目录
        var defaultTemplateDir = GetDefaultTemplateDirectory();
        if (!string.IsNullOrWhiteSpace(templateType))
        {
            defaultTemplateDir = Path.Combine(defaultTemplateDir, templateType);
        }

        return defaultTemplateDir;
    }

    /// <summary>
    /// 获取指定模板文件的完整路径（优先查找用户自定义模板）
    /// </summary>
    /// <param name="templateFileName">模板文件名（如 "Entity.sbn"）</param>
    /// <param name="templateType">模板类型（如 "CRUD"、"MasterDetail"、"Tree"）</param>
    /// <returns>模板文件的完整路径，如果不存在则返回 null</returns>
    public static string? GetTemplateFilePath(string templateFileName, string? templateType = null)
    {
        // 优先查找用户自定义模板
        var userTemplateDir = GetUserTemplateDirectory();
        if (!string.IsNullOrWhiteSpace(templateType))
        {
            userTemplateDir = Path.Combine(userTemplateDir, templateType);
        }
        var userTemplatePath = Path.Combine(userTemplateDir, templateFileName);
        if (File.Exists(userTemplatePath))
        {
            return userTemplatePath;
        }

        // 查找默认模板
        var defaultTemplateDir = GetDefaultTemplateDirectory();
        if (!string.IsNullOrWhiteSpace(templateType))
        {
            defaultTemplateDir = Path.Combine(defaultTemplateDir, templateType);
        }
        var defaultTemplatePath = Path.Combine(defaultTemplateDir, templateFileName);
        if (File.Exists(defaultTemplatePath))
        {
            return defaultTemplatePath;
        }

        // 如果都不存在，返回 null
        return null;
    }

    #endregion
}

