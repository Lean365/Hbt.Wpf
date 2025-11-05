//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DictionaryData.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 字典数据实体
//===================================================================

using SqlSugar;

namespace Hbt.Domain.Entities.Routine;

/// <summary>
/// 字典数据实体
/// 用于存储具体的字典数据项
/// </summary>
[SugarTable("hbt_routine_dictionary_data", "字典数据表")]
[SugarIndex("IX_hbt_routine_dictionary_data_code", nameof(DataCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_routine_dictionary_data_type_code", nameof(TypeCode), OrderByType.Asc, false)]
public class DictionaryData : BaseEntity
{
    /// <summary>
    /// 字典类型代码
    /// 关联的字典类型代码（避免硬依赖主键Id）
    /// </summary>
    [SugarColumn(ColumnName = "type_code", ColumnDescription = "字典类型代码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// 数据代码
    /// 字典数据的唯一标识
    /// </summary>
    [SugarColumn(ColumnName = "data_code", ColumnDescription = "数据代码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string DataCode { get; set; } = string.Empty;

    /// <summary>
    /// 数据名称
    /// 字典数据的显示名称
    /// </summary>
    [SugarColumn(ColumnName = "data_name", ColumnDescription = "数据名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string DataName { get; set; } = string.Empty;

    /// <summary>
    /// 数据值
    /// 字典数据的实际值
    /// </summary>
    [SugarColumn(ColumnName = "data_value", ColumnDescription = "数据值", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? DataValue { get; set; }

    /// <summary>
    /// 扩展数据
    /// JSON格式的扩展信息
    /// </summary>
    [SugarColumn(ColumnName = "extended_data", ColumnDescription = "扩展数据", ColumnDataType = "nvarchar", Length = 2000, IsNullable = true)]
    public string? ExtendedData { get; set; }

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

