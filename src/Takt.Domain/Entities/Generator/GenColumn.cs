// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Domain.Entities.Generator
// 文件名称：GenColumn.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成列配置实体（参照 OpenAuth.Net BuilderTableColumn）
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Generator;

/// <summary>
/// 代码生成列配置实体
/// 用于存储代码生成的列级配置信息（包括字段顺序）
/// </summary>
[SugarTable("takt_generator_column", "代码生成列配置")]
[SugarIndex("IX_takt_generator_column_table_name", nameof(GenColumn.TableName), OrderByType.Asc, false)]
[SugarIndex("IX_takt_generator_column_name", nameof(GenColumn.ColumnName), OrderByType.Asc, false)]
public class GenColumn : BaseEntity
{
    // ========== 基本信息 ==========
    /// <summary>
    /// 表名
    /// </summary>
    [SugarColumn(ColumnName = "table_name", ColumnDescription = "表名", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 列名（数据库列名）
    /// </summary>
    [SugarColumn(ColumnName = "column_name", ColumnDescription = "列名", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// 列描述
    /// </summary>
    [SugarColumn(ColumnName = "column_description", ColumnDescription = "列描述", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? ColumnDescription { get; set; }

    /// <summary>
    /// 库列类型
    /// </summary>
    [SugarColumn(ColumnName = "column_data_type", ColumnDescription = "库列类型", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? ColumnDataType { get; set; }

    /// <summary>
    /// 属性名称（C#属性名）
    /// </summary>
    [SugarColumn(ColumnName = "property_name", ColumnDescription = "属性名称", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? PropertyName { get; set; }

    /// <summary>
    /// C#类型
    /// </summary>
    [SugarColumn(ColumnName = "data_type", ColumnDescription = "C#类型", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? DataType { get; set; }

    // ========== 列属性 ==========
    /// <summary>
    /// 可空（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_nullable", ColumnDescription = "可空", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsNullable { get; set; } = 1;

    /// <summary>
    /// 主键（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_primary_key", ColumnDescription = "主键", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsPrimaryKey { get; set; } = 1;

    /// <summary>
    /// 自增（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_identity", ColumnDescription = "自增", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsIdentity { get; set; } = 1;

    /// <summary>
    /// 列长度
    /// </summary>
    [SugarColumn(ColumnName = "length", ColumnDescription = "列长度", ColumnDataType = "int", IsNullable = true)]
    public int? Length { get; set; }

    /// <summary>
    /// 精度（小数位数，用于decimal类型）
    /// </summary>
    [SugarColumn(ColumnName = "decimal_places", ColumnDescription = "精度", ColumnDataType = "int", IsNullable = true)]
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    [SugarColumn(ColumnName = "default_value", ColumnDescription = "默认值", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? DefaultValue { get; set; }

    // ========== 排序 ==========
    /// <summary>
    /// 库列排序
    /// </summary>
    [SugarColumn(ColumnName = "order_num", ColumnDescription = "库列排序", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int OrderNum { get; set; }

    // ========== 生成控制 ==========
    /// <summary>
    /// 查询（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_query", ColumnDescription = "查询", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsQuery { get; set; } = 1;

    /// <summary>
    /// 查询方式（Like=模糊查询，Equal=精确查询，Between=范围查询等）
    /// </summary>
    [SugarColumn(ColumnName = "query_type", ColumnDescription = "查询方式", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? QueryType { get; set; }

    /// <summary>
    /// 创建（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_create", ColumnDescription = "创建", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int IsCreate { get; set; } = 0;

    /// <summary>
    /// 更新（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_update", ColumnDescription = "更新", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int IsUpdate { get; set; } = 0;

    /// <summary>
    /// 删除（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_delete", ColumnDescription = "删除", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int IsDelete { get; set; } = 0;

    /// <summary>
    /// 列表（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_list", ColumnDescription = "列表", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int IsList { get; set; } = 0;

    /// <summary>
    /// 导出（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_export", ColumnDescription = "导出", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsExport { get; set; } = 1;

    /// <summary>
    /// 排序（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_sort", ColumnDescription = "排序", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsSort { get; set; } = 1;

    /// <summary>
    /// 必填（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_required", ColumnDescription = "必填", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsRequired { get; set; } = 1;

    /// <summary>
    /// 表单显示（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_form", ColumnDescription = "表单显示", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int IsForm { get; set; } = 0;

    // ========== UI相关 ==========
    /// <summary>
    /// 表单类型
    /// </summary>
    [SugarColumn(ColumnName = "form_control_type", ColumnDescription = "表单类型", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? FormControlType { get; set; }

    /// <summary>
    /// 字典类型（用于下拉框等）
    /// </summary>
    [SugarColumn(ColumnName = "dict_type", ColumnDescription = "字典类型", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? DictType { get; set; }

    // ========== 导航属性 ==========
    /// <summary>
    /// 关联的表配置
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(GenColumn.TableName), nameof(GenTable.TableName))]
    public GenTable? Table { get; set; }
}
