//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Routine
// 文件名 : DictionaryType.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-17
// 功能描述：字典类型实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Routine;

/// <summary>
/// 字典类型实体
/// 用于定义数据字典的分类
/// </summary>
[SugarTable("takt_routine_dictionary_type", "字典类型表")]
[SugarIndex("IX_takt_routine_dictionary_type_code", nameof(TypeCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_routine_dictionary_type_data_source", nameof(DataSource), OrderByType.Asc, false)]
public class DictionaryType : BaseEntity
{
    /// <summary>
    /// 类型代码
    /// 字典类型的唯一标识（在同一数据源下唯一）
    /// </summary>
    [SugarColumn(ColumnName = "type_code", ColumnDescription = "类型代码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// 类型名称
    /// 字典类型的显示名称
    /// </summary>
    [SugarColumn(ColumnName = "type_name", ColumnDescription = "类型名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 数据源
    /// 数据来源标识：0=系统，1=SQL脚本
    /// </summary>
    [SugarColumn(ColumnName = "data_source", ColumnDescription = "数据源", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int DataSource { get; set; } = 0;

    /// <summary>
    /// SQL脚本
    /// 当数据源为SQL脚本时，存储SQL查询语句
    /// </summary>
    [SugarColumn(ColumnName = "sql_script", ColumnDescription = "SQL脚本", ColumnDataType = "nvarchar", Length = -1, IsNullable = true)]
    public string? SqlScript { get; set; }

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
    [SugarColumn(ColumnName = "is_builtin", ColumnDescription = "内置", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsBuiltin { get; set; } = 1;

    /// <summary>
    /// 字典类型状态
    /// 0=启用，1=禁用
    /// </summary>
    [SugarColumn(ColumnName = "type_status", ColumnDescription = "状态", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int TypeStatus { get; set; } = 0;

    /// <summary>
    /// 关联的字典数据集合
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(DictionaryData.TypeCode))]
    public List<DictionaryData>? DictionaryDataList { get; set; }
}

