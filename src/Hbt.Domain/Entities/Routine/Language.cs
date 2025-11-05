//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : Language.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 语言实体
//===================================================================

using SqlSugar;

namespace Hbt.Domain.Entities.Routine;

/// <summary>
/// 语言实体
/// 用于管理系统支持的语言列表
/// </summary>
[SugarTable("hbt_routine_language", "语言表")]
[SugarIndex("IX_hbt_routine_language_code", nameof(Language.LanguageCode), OrderByType.Asc, true)]
public class Language : BaseEntity
{
    /// <summary>
    /// 语言代码
    /// 标准语言代码，如：zh-CN, en-US
    /// </summary>
    [SugarColumn(ColumnName = "language_code", ColumnDescription = "语言代码", ColumnDataType = "nvarchar", Length = 10, IsNullable = false)]
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// 语言名称
    /// 语言的显示名称
    /// </summary>
    [SugarColumn(ColumnName = "language_name", ColumnDescription = "语言名称", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>
    /// 本地化名称
    /// 该语言的自称（例如：中文、English）
    /// </summary>
    [SugarColumn(ColumnName = "native_name", ColumnDescription = "本地化名称", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? NativeName { get; set; }

    /// <summary>
    /// 语言图标
    /// 语言标识图标（如：🇨🇳, 🇺🇸）
    /// </summary>
    [SugarColumn(ColumnName = "language_icon", ColumnDescription = "语言图标", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? LanguageIcon { get; set; }

    /// <summary>
    /// 是否默认
    /// 0=是，1=否
    /// </summary>
    [SugarColumn(ColumnName = "is_default", ColumnDescription = "是否默认", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsDefault { get; set; } = 1;

    /// <summary>
    /// 排序号
    /// 用于控制显示顺序
    /// </summary>
    [SugarColumn(ColumnName = "order_num", ColumnDescription = "排序号", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 是否内置
    /// 0=是，1=否（内置数据不可删除）
    /// </summary>
    [SugarColumn(ColumnName = "is_builtin", ColumnDescription = "是否内置", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsBuiltin { get; set; } = 1;

    /// <summary>
    /// 语言状态
    /// 0=启用，1=禁用
    /// </summary>
    [SugarColumn(ColumnName = "language_status", ColumnDescription = "语言状态", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int LanguageStatus { get; set; } = 0;

    /// <summary>
    /// 关联的翻译集合
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(Translation.LanguageCode))]
    public List<Translation>? Translations { get; set; }
}

