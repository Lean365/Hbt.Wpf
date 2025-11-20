// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Routine
// 文件名称：TranslationValueItemDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：翻译值项数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 翻译值项数据传输对象
/// 用于从表显示某个翻译键在所有语言下的翻译值
/// </summary>
public class TranslationValueItemDto
{
    /// <summary>
    /// 语言代码
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// 语言名称
    /// </summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>
    /// 翻译值
    /// </summary>
    public string TranslationValue { get; set; } = string.Empty;

    /// <summary>
    /// 翻译ID（如果存在）
    /// </summary>
    public long? TranslationId { get; set; }
}

