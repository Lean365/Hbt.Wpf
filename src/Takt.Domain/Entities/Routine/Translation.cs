//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Routine
// 文件名 : Translation.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-17
// 功能描述：翻译实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Routine;

/// <summary>
/// 翻译实体
/// 用于存储多语言翻译内容
/// </summary>
[SugarTable("takt_routine_translation", "翻译表")]
[SugarIndex("IX_takt_routine_translation_language_code", nameof(LanguageCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_routine_translation_key", nameof(TranslationKey), OrderByType.Asc, false)]
[SugarIndex("IX_takt_routine_translation_module", nameof(Module), OrderByType.Asc, false)]
[SugarIndex("IX_takt_routine_translation_composite", nameof(TranslationKey), OrderByType.Asc, nameof(LanguageCode), OrderByType.Asc, true)]
public class Translation : BaseEntity
{
    /// <summary>
    /// 语言代码
    /// 标准语言代码（如：zh-CN、en-US）
    /// </summary>
    [SugarColumn(ColumnName = "language_code", ColumnDescription = "语言代码", ColumnDataType = "nvarchar", Length = 10, IsNullable = false)]
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// 翻译键
    /// 唯一标识翻译内容的键，如：Login.Welcome
    /// </summary>
    [SugarColumn(ColumnName = "translation_key", ColumnDescription = "翻译键", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string TranslationKey { get; set; } = string.Empty;

    /// <summary>
    /// 翻译值
    /// 该键在指定语言下的翻译内容
    /// </summary>
    [SugarColumn(ColumnName = "translation_value", ColumnDescription = "翻译值", ColumnDataType = "nvarchar", Length = 2000, IsNullable = false)]
    public string TranslationValue { get; set; } = string.Empty;

    /// <summary>
    /// 模块
    /// 所属模块，如：Login, Common, Dashboard
    /// </summary>
    [SugarColumn(ColumnName = "module", ColumnDescription = "模块", ColumnDataType = "nvarchar", Length = 50, IsNullable = false, DefaultValue = "Frontend")]
    public string Module { get; set; } = "Frontend";

    /// <summary>
    /// 描述
    /// 翻译内容的描述信息
    /// </summary>
    [SugarColumn(ColumnName = "description", ColumnDescription = "描述", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 排序号
    /// 用于控制显示顺序
    /// </summary>
    [SugarColumn(ColumnName = "order_num", ColumnDescription = "排序号", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int OrderNum { get; set; } = 0;

    // 注意：为避免跨库/跨上下文 Join 带来的不必要查询，翻译表直接保存 LanguageCode。
}

