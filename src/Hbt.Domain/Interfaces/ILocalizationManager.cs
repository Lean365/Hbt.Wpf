//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ILocalizationManager.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 本地化管理器接口（领域层）
//===================================================================

namespace Hbt.Domain.Interfaces;

/// <summary>
/// 本地化管理器接口
/// 提供本地化和语言切换功能
/// </summary>
public interface ILocalizationManager
{
    /// <summary>
    /// 当前语言代码
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// 语言切换事件
    /// </summary>
    event EventHandler<string>? LanguageChanged;

    /// <summary>
    /// 获取默认语言
    /// </summary>
    /// <returns>默认语言代码</returns>
    string GetDefaultLanguage();

    /// <summary>
    /// 初始化（异步预加载数据）
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 获取语言列表
    /// </summary>
    /// <returns>语言列表</returns>
    List<object> GetLanguages();

    /// <summary>
    /// 切换语言
    /// </summary>
    /// <param name="languageCode">语言代码</param>
    void ChangeLanguage(string languageCode);

    /// <summary>
    /// 获取翻译
    /// </summary>
    /// <param name="key">翻译键</param>
    /// <returns>翻译值</returns>
    string GetString(string key);
}

