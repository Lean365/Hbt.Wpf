// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Domain.Entities.Generator
// 文件名称：GenTable.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表配置实体（参照 OpenAuth.Net BuilderTable，适配当前项目）
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Generator;

/// <summary>
/// 代码生成表配置实体
/// 用于存储代码生成的表级配置信息
/// </summary>
[SugarTable("takt_generator_table", "代码生成表配置")]
[SugarIndex("IX_takt_generator_table_name", nameof(GenTable.TableName), OrderByType.Asc, true)]
public class GenTable : BaseEntity
{
    // ========== 基本信息 ==========
    /// <summary>
    /// 库表名称
    /// </summary>
    [SugarColumn(ColumnName = "table_name", ColumnDescription = "库表名称", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 库表描述
    /// </summary>
    [SugarColumn(ColumnName = "table_description", ColumnDescription = "库表描述", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? TableDescription { get; set; }

    /// <summary>
    /// 实体类名称
    /// </summary>
    [SugarColumn(ColumnName = "class_name", ColumnDescription = "实体类名称", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? ClassName { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [SugarColumn(ColumnName = "author", ColumnDescription = "作者", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? Author { get; set; }

    // ========== 生成信息 ==========
    /// <summary>
    /// 生成模板类型（CRUD、MasterDetail、Tree）
    /// </summary>
    [SugarColumn(ColumnName = "template_type", ColumnDescription = "生成模板类型", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? TemplateType { get; set; }

    /// <summary>
    /// 子表名称
    /// </summary>
    [SugarColumn(ColumnName = "detail_table_name", ColumnDescription = "子表名称", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? DetailTableName { get; set; }

    /// <summary>
    /// 子表关联字段
    /// </summary>
    [SugarColumn(ColumnName = "detail_relation_field", ColumnDescription = "子表关联字段", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? DetailRelationField { get; set; }

    /// <summary>
    /// 树编码字段
    /// </summary>
    [SugarColumn(ColumnName = "tree_code_field", ColumnDescription = "树编码字段", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? TreeCodeField { get; set; }

    /// <summary>
    /// 树父编码字段
    /// </summary>
    [SugarColumn(ColumnName = "tree_parent_code_field", ColumnDescription = "树父编码字段", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? TreeParentCodeField { get; set; }

    /// <summary>
    /// 树名称字段
    /// </summary>
    [SugarColumn(ColumnName = "tree_name_field", ColumnDescription = "树名称字段", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? TreeNameField { get; set; }

    /// <summary>
    /// 命名空间前缀（用于生成命名空间，如：Takt）
    /// </summary>
    [SugarColumn(ColumnName = "gen_namespace_prefix", ColumnDescription = "命名空间前缀", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? GenNamespacePrefix { get; set; }

    /// <summary>
    /// 生成业务名称
    /// </summary>
    [SugarColumn(ColumnName = "gen_business_name", ColumnDescription = "生成业务名称", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? GenBusinessName { get; set; }

    /// <summary>
    /// 生成模块名称
    /// </summary>
    [SugarColumn(ColumnName = "gen_module_name", ColumnDescription = "生成模块名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? GenModuleName { get; set; }

    /// <summary>
    /// 生成功能名
    /// </summary>
    [SugarColumn(ColumnName = "gen_function_name", ColumnDescription = "生成功能名", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? GenFunctionName { get; set; }

    /// <summary>
    /// 生成方式（zip=压缩包，path=自定义路径）
    /// </summary>
    [SugarColumn(ColumnName = "gen_type", ColumnDescription = "生成方式", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? GenType { get; set; }

    /// <summary>
    /// 代码生成路径
    /// </summary>
    [SugarColumn(ColumnName = "gen_path", ColumnDescription = "代码生成路径", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? GenPath { get; set; }

    /// <summary>
    /// 生成功能（多个功能用逗号分隔：Create,Update,Delete,View,Preview,Import,Export等）
    /// </summary>
    [SugarColumn(ColumnName = "gen_functions", ColumnDescription = "生成功能", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? GenFunctions { get; set; }

    /// <summary>
    /// 上级菜单名称
    /// </summary>
    [SugarColumn(ColumnName = "parent_menu_name", ColumnDescription = "上级菜单名称", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? ParentMenuName { get; set; }

    /// <summary>
    /// 权限前缀
    /// </summary>
    [SugarColumn(ColumnName = "permission_prefix", ColumnDescription = "权限前缀", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? PermissionPrefix { get; set; }

    /// <summary>
    /// 是否有表（0=有表，1=无表）
    /// </summary>
    [SugarColumn(ColumnName = "is_database_table", ColumnDescription = "是否有表", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsDatabaseTable { get; set; } = 1;

    /// <summary>
    /// 是否生成菜单（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_gen_menu", ColumnDescription = "是否生成菜单", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsGenMenu { get; set; } = 1;

    /// <summary>
    /// 是否生成翻译（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_gen_translation", ColumnDescription = "是否生成翻译", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsGenTranslation { get; set; } = 1;

    /// <summary>
    /// 是否生成代码（0=是，1=否）
    /// </summary>
    [SugarColumn(ColumnName = "is_gen_code", ColumnDescription = "是否生成代码", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsGenCode { get; set; } = 1;

    /// <summary>
    /// 默认排序字段
    /// </summary>
    [SugarColumn(ColumnName = "default_sort_field", ColumnDescription = "默认排序字段", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? DefaultSortField { get; set; }

    /// <summary>
    /// 默认排序（ASC=正序，DESC=倒序）
    /// </summary>
    [SugarColumn(ColumnName = "default_sort_order", ColumnDescription = "默认排序", ColumnDataType = "nvarchar", Length = 10, IsNullable = true)]
    public string? DefaultSortOrder { get; set; }

    /// <summary>
    /// 其它生成选项
    /// </summary>
    [SugarColumn(ColumnName = "options", ColumnDescription = "其它生成选项", ColumnDataType = "nvarchar", Length = 1000, IsNullable = true)]
    public string? Options { get; set; }

    // ========== 导航属性 ==========
    /// <summary>
    /// 关联的列配置集合
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(GenColumn.TableName), nameof(GenTable.TableName))]
    public List<GenColumn>? Columns { get; set; }
}
