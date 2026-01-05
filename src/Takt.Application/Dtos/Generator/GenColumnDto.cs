// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Generator
// 文件名称：GenColumnDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成列配置数据传输对象
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Results;

namespace Takt.Application.Dtos.Generator;

/// <summary>
/// 代码生成列配置数据传输对象
/// </summary>
public class GenColumnDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public GenColumnDto()
    {
        TableName = string.Empty;
        ColumnName = string.Empty;
        PropertyName = string.Empty;
        ColumnDescription = string.Empty;
        DataType = string.Empty;
        ColumnDataType = string.Empty;
        DefaultValue = string.Empty;
        QueryType = string.Empty;
        FormControlType = string.Empty;
        DictType = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 列名（数据库列名）
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 属性名称（C#属性名）
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 数据类型（C#类型）
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 列数据类型（数据库类型）
    /// </summary>
    public string ColumnDataType { get; set; }

    /// <summary>
    /// 可空（0=是，1=否）
    /// </summary>
    public int IsNullable { get; set; }

    /// <summary>
    /// 主键（0=是，1=否）
    /// </summary>
    public int IsPrimaryKey { get; set; }

    /// <summary>
    /// 自增（0=是，1=否）
    /// </summary>
    public int IsIdentity { get; set; }

    /// <summary>
    /// 列长度
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// 精度（小数位数，用于decimal类型）
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// 排序顺序（用于控制字段在DTO中的顺序）
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 查询（0=是，1=否）
    /// </summary>
    public int IsQuery { get; set; }

    /// <summary>
    /// 查询方式（Like=模糊查询，Equal=精确查询，Between=范围查询等）
    /// </summary>
    public string QueryType { get; set; }

    /// <summary>
    /// 创建（0=是，1=否）
    /// </summary>
    public int IsCreate { get; set; }

    /// <summary>
    /// 更新（0=是，1=否）
    /// </summary>
    public int IsUpdate { get; set; }

    /// <summary>
    /// 删除（0=是，1=否）
    /// </summary>
    public int IsDelete { get; set; }

    /// <summary>
    /// 列表（0=是，1=否）
    /// </summary>
    public int IsList { get; set; }

    /// <summary>
    /// 导出（0=是，1=否）
    /// </summary>
    public int IsExport { get; set; }

    /// <summary>
    /// 排序（0=是，1=否）
    /// </summary>
    public int IsSort { get; set; }

    /// <summary>
    /// 必填（0=是，1=否）
    /// </summary>
    public int IsRequired { get; set; }

    /// <summary>
    /// 表单显示（0=是，1=否）
    /// </summary>
    public int IsForm { get; set; }

    /// <summary>
    /// 表单显示类型（TextBox、ComboBox、DatePicker、CheckBox等）
    /// </summary>
    public string FormControlType { get; set; }

    /// <summary>
    /// 字典类型（用于下拉框等）
    /// </summary>
    public string DictType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdatedBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }

    /// <summary>
    /// 是否删除（0=否，1=是）
    /// </summary>
    public int IsDeleted { get; set; }

    /// <summary>
    /// 删除人
    /// </summary>
    public string DeletedBy { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime DeletedTime { get; set; }
}

/// <summary>
/// 代码生成列配置查询数据传输对象
/// </summary>
public class GenColumnQueryDto : PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public GenColumnQueryDto()
    {
        Keywords = string.Empty;
        TableName = string.Empty;
        ColumnName = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在列名、列描述中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 列名
    /// </summary>
    public string ColumnName { get; set; }
}

/// <summary>
/// 创建代码生成列配置数据传输对象
/// </summary>
public class GenColumnCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public GenColumnCreateDto()
    {
        TableName = string.Empty;
        ColumnName = string.Empty;
        ColumnDescription = string.Empty;
        ColumnDataType = string.Empty;
        PropertyName = string.Empty;
        DataType = string.Empty;
        DefaultValue = string.Empty;
        QueryType = string.Empty;
        FormControlType = string.Empty;
        DictType = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 列名
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 库列类型
    /// </summary>
    public string ColumnDataType { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// C#类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 可空（0=是，1=否）
    /// </summary>
    public int IsNullable { get; set; } = 1;

    /// <summary>
    /// 主键（0=是，1=否）
    /// </summary>
    public int IsPrimaryKey { get; set; } = 1;

    /// <summary>
    /// 自增（0=是，1=否）
    /// </summary>
    public int IsIdentity { get; set; } = 1;

    /// <summary>
    /// 长度
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// 精度
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// 库列排序
    /// </summary>
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 查询（0=是，1=否）
    /// </summary>
    public int IsQuery { get; set; } = 1;

    /// <summary>
    /// 查询方式
    /// </summary>
    public string QueryType { get; set; }

    /// <summary>
    /// 创建（0=是，1=否）
    /// </summary>
    public int IsCreate { get; set; } = 0;

    /// <summary>
    /// 更新（0=是，1=否）
    /// </summary>
    public int IsUpdate { get; set; } = 0;

    /// <summary>
    /// 删除（0=是，1=否）
    /// </summary>
    public int IsDelete { get; set; } = 0;

    /// <summary>
    /// 列表（0=是，1=否）
    /// </summary>
    public int IsList { get; set; } = 0;

    /// <summary>
    /// 导出（0=是，1=否）
    /// </summary>
    public int IsExport { get; set; } = 1;

    /// <summary>
    /// 排序（0=是，1=否）
    /// </summary>
    public int IsSort { get; set; } = 1;

    /// <summary>
    /// 必填（0=是，1=否）
    /// </summary>
    public int IsRequired { get; set; } = 1;

    /// <summary>
    /// 表单显示（0=是，1=否）
    /// </summary>
    public int IsForm { get; set; } = 0;

    /// <summary>
    /// 表单类型
    /// </summary>
    public string FormControlType { get; set; }

    /// <summary>
    /// 字典类型
    /// </summary>
    public string DictType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新代码生成列配置数据传输对象
/// </summary>
public class GenColumnUpdateDto : GenColumnCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 代码生成列配置导出数据传输对象
/// </summary>
public class GenColumnExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public GenColumnExportDto()
    {
        TableName = string.Empty;
        ColumnName = string.Empty;
        PropertyName = string.Empty;
        ColumnDescription = string.Empty;
        DataType = string.Empty;
        ColumnDataType = string.Empty;
        DefaultValue = string.Empty;
        QueryType = string.Empty;
        FormControlType = string.Empty;
        DictType = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 列名（数据库列名）
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 属性名称（C#属性名）
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 数据类型（C#类型）
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 列数据类型（数据库类型）
    /// </summary>
    public string ColumnDataType { get; set; }

    /// <summary>
    /// 可空（0=是，1=否）
    /// </summary>
    public int IsNullable { get; set; }

    /// <summary>
    /// 主键（0=是，1=否）
    /// </summary>
    public int IsPrimaryKey { get; set; }

    /// <summary>
    /// 自增（0=是，1=否）
    /// </summary>
    public int IsIdentity { get; set; }

    /// <summary>
    /// 列长度
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// 精度（小数位数，用于decimal类型）
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// 排序顺序（用于控制字段在DTO中的顺序）
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 查询（0=是，1=否）
    /// </summary>
    public int IsQuery { get; set; }

    /// <summary>
    /// 查询方式（Like=模糊查询，Equal=精确查询，Between=范围查询等）
    /// </summary>
    public string QueryType { get; set; }

    /// <summary>
    /// 创建（0=是，1=否）
    /// </summary>
    public int IsCreate { get; set; }

    /// <summary>
    /// 更新（0=是，1=否）
    /// </summary>
    public int IsUpdate { get; set; }

    /// <summary>
    /// 删除（0=是，1=否）
    /// </summary>
    public int IsDelete { get; set; }

    /// <summary>
    /// 列表（0=是，1=否）
    /// </summary>
    public int IsList { get; set; }

    /// <summary>
    /// 导出（0=是，1=否）
    /// </summary>
    public int IsExport { get; set; }

    /// <summary>
    /// 排序（0=是，1=否）
    /// </summary>
    public int IsSort { get; set; }

    /// <summary>
    /// 必填（0=是，1=否）
    /// </summary>
    public int IsRequired { get; set; }

    /// <summary>
    /// 表单显示（0=是，1=否）
    /// </summary>
    public int IsForm { get; set; }

    /// <summary>
    /// 表单显示类型（TextBox、ComboBox、DatePicker、CheckBox等）
    /// </summary>
    public string FormControlType { get; set; }

    /// <summary>
    /// 字典类型（用于下拉框等）
    /// </summary>
    public string DictType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 代码生成列配置导入模板数据传输对象
/// </summary>
public class GenColumnTemplateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public GenColumnTemplateDto()
    {
        TableName = string.Empty;
        ColumnName = string.Empty;
        ColumnDescription = string.Empty;
        ColumnDataType = string.Empty;
        PropertyName = string.Empty;
        DataType = string.Empty;
        DefaultValue = string.Empty;
        QueryType = string.Empty;
        FormControlType = string.Empty;
        DictType = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 列名
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 库列类型
    /// </summary>
    public string ColumnDataType { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// C#类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 可空（0=是，1=否）
    /// </summary>
    public int IsNullable { get; set; }

    /// <summary>
    /// 主键（0=是，1=否）
    /// </summary>
    public int IsPrimaryKey { get; set; }

    /// <summary>
    /// 自增（0=是，1=否）
    /// </summary>
    public int IsIdentity { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// 精度
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// 库列排序
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 查询（0=是，1=否）
    /// </summary>
    public int IsQuery { get; set; }

    /// <summary>
    /// 查询方式
    /// </summary>
    public string QueryType { get; set; }

    /// <summary>
    /// 创建（0=是，1=否）
    /// </summary>
    public int IsCreate { get; set; }

    /// <summary>
    /// 更新（0=是，1=否）
    /// </summary>
    public int IsUpdate { get; set; }

    /// <summary>
    /// 删除（0=是，1=否）
    /// </summary>
    public int IsDelete { get; set; }

    /// <summary>
    /// 列表（0=是，1=否）
    /// </summary>
    public int IsList { get; set; }

    /// <summary>
    /// 导出（0=是，1=否）
    /// </summary>
    public int IsExport { get; set; }

    /// <summary>
    /// 排序（0=是，1=否）
    /// </summary>
    public int IsSort { get; set; }

    /// <summary>
    /// 必填（0=是，1=否）
    /// </summary>
    public int IsRequired { get; set; }

    /// <summary>
    /// 表单显示（0=是，1=否）
    /// </summary>
    public int IsForm { get; set; }

    /// <summary>
    /// 表单类型
    /// </summary>
    public string FormControlType { get; set; }

    /// <summary>
    /// 字典类型
    /// </summary>
    public string DictType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 代码生成列配置导入数据传输对象
/// </summary>
public class GenColumnImportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public GenColumnImportDto()
    {
        TableName = string.Empty;
        ColumnName = string.Empty;
        ColumnDescription = string.Empty;
        ColumnDataType = string.Empty;
        PropertyName = string.Empty;
        DataType = string.Empty;
        DefaultValue = string.Empty;
        QueryType = string.Empty;
        FormControlType = string.Empty;
        DictType = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 列名
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 列描述
    /// </summary>
    public string ColumnDescription { get; set; }

    /// <summary>
    /// 库列类型
    /// </summary>
    public string ColumnDataType { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// C#类型
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 可空（0=是，1=否）
    /// </summary>
    public int IsNullable { get; set; }

    /// <summary>
    /// 主键（0=是，1=否）
    /// </summary>
    public int IsPrimaryKey { get; set; }

    /// <summary>
    /// 自增（0=是，1=否）
    /// </summary>
    public int IsIdentity { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// 精度
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// 库列排序
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 查询（0=是，1=否）
    /// </summary>
    public int IsQuery { get; set; }

    /// <summary>
    /// 查询方式
    /// </summary>
    public string QueryType { get; set; }

    /// <summary>
    /// 创建（0=是，1=否）
    /// </summary>
    public int IsCreate { get; set; }

    /// <summary>
    /// 更新（0=是，1=否）
    /// </summary>
    public int IsUpdate { get; set; }

    /// <summary>
    /// 删除（0=是，1=否）
    /// </summary>
    public int IsDelete { get; set; }

    /// <summary>
    /// 列表（0=是，1=否）
    /// </summary>
    public int IsList { get; set; }

    /// <summary>
    /// 导出（0=是，1=否）
    /// </summary>
    public int IsExport { get; set; }

    /// <summary>
    /// 排序（0=是，1=否）
    /// </summary>
    public int IsSort { get; set; }

    /// <summary>
    /// 必填（0=是，1=否）
    /// </summary>
    public int IsRequired { get; set; }

    /// <summary>
    /// 表单显示（0=是，1=否）
    /// </summary>
    public int IsForm { get; set; }

    /// <summary>
    /// 表单类型
    /// </summary>
    public string FormControlType { get; set; }

    /// <summary>
    /// 字典类型
    /// </summary>
    public string DictType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

