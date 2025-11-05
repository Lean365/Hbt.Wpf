//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DictionaryType.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 字典类型实体
//===================================================================

using SqlSugar;

namespace Hbt.Domain.Entities.Routine;

/// <summary>
/// 字典类型实体
/// 用于定义数据字典的分类
/// </summary>
[SugarTable("hbt_routine_dictionary_type", "字典类型表")]
[SugarIndex("IX_hbt_routine_dictionary_type_code", nameof(TypeCode), OrderByType.Asc, true)]
public class DictionaryType : BaseEntity
{
    /// <summary>
    /// 类型代码
    /// 字典类型的唯一标识
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
    /// 字典类型状态
    /// 0=启用，1=禁用
    /// </summary>
    [SugarColumn(ColumnName = "type_status", ColumnDescription = "字典类型状态", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int TypeStatus { get; set; } = 0;

    /// <summary>
    /// 关联的字典数据集合
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(DictionaryData.TypeCode))]
    public List<DictionaryData>? DictionaryDataList { get; set; }
}

