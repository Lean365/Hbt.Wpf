//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Routine
// 文件名 : DictionaryData.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-17
// 功能描述：字典数据实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Routine;

/// <summary>
/// 字典数据实体
/// 用于存储具体的字典数据项
/// </summary>
[SugarTable("takt_routine_dictionary_data", "字典数据表")]
[SugarIndex("IX_takt_routine_dictionary_data_type_code", nameof(TypeCode), OrderByType.Asc, false)]
public class DictionaryData : BaseEntity
{
    /// <summary>
    /// 字典类型代码
    /// 关联的字典类型代码（避免硬依赖主键Id）
    /// </summary>
    [SugarColumn(ColumnName = "type_code", ColumnDescription = "字典类型代码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// 数据标签
    /// 字典数据的显示名称
    /// </summary>
    [SugarColumn(ColumnName = "data_label", ColumnDescription = "数据标签", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string DataLabel { get; set; } = string.Empty;

    /// <summary>
    /// 国际化键
    /// 用于多语言翻译
    /// </summary>
    /// <remarks>
    /// 用于多语言翻译，如：dictionary.common.gender.male
    /// </remarks>
    [SugarColumn(ColumnName = "i18n_key", ColumnDescription = "国际化键", ColumnDataType = "nvarchar", Length = 64, IsNullable = false)]
    public string I18nKey { get; set; } = string.Empty;

    /// <summary>
    /// 数据值
    /// 字典数据的实际值
    /// </summary>
    [SugarColumn(ColumnName = "data_value", ColumnDescription = "数据值", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? DataValue { get; set; }

    /// <summary>
    /// 扩展标签
    /// 扩展信息的标签
    /// </summary>
    [SugarColumn(ColumnName = "ext_label", ColumnDescription = "扩展标签", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? ExtLabel { get; set; }

    /// <summary>
    /// 扩展值
    /// 扩展信息的实际值
    /// </summary>
    [SugarColumn(ColumnName = "ext_value", ColumnDescription = "扩展值", ColumnDataType = "nvarchar", Length = 2000, IsNullable = true)]
    public string? ExtValue { get; set; }

    /// <summary>
    /// CSS类名
    /// 用于控制显示的样式
    /// </summary>
    [SugarColumn(ColumnName = "css_class", ColumnDescription = "CSS类名", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? CssClass { get; set; }

    /// <summary>
    /// 列表CSS类名
    /// 用于控制列表显示的样式
    /// </summary>
    [SugarColumn(ColumnName = "list_css_class", ColumnDescription = "列表CSS类名", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? ListClass { get; set; }

    /// <summary>
    /// 排序号
    /// 用于控制显示顺序
    /// </summary>
    [SugarColumn(ColumnName = "order_num", ColumnDescription = "排序号", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int OrderNum { get; set; } = 0;

    // 注意：为降低耦合度，此处直接保存 TypeCode，不通过 Id 导航
}

