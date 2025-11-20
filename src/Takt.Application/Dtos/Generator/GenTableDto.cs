// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Generator
// 文件名称：GenTableDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表配置数据传输对象
// 
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Results;

namespace Takt.Application.Dtos.Generator;

/// <summary>
/// 代码生成表配置数据传输对象
/// </summary>
public class GenTableDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 库表名称
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 库表描述
    /// </summary>
    public string? TableDescription { get; set; }

    /// <summary>
    /// 实体类名称
    /// </summary>
    public string? ClassName { get; set; }


    /// <summary>
    /// 子表名称
    /// </summary>
    public string? DetailTableName { get; set; }

    /// <summary>
    /// 子表关联字段
    /// </summary>
    public string? DetailRelationField { get; set; }

    /// <summary>
    /// 树编码字段
    /// </summary>
    public string? TreeCodeField { get; set; }

    /// <summary>
    /// 树父编码字段
    /// </summary>
    public string? TreeParentCodeField { get; set; }

    /// <summary>
    /// 树名称字段
    /// </summary>
    public string? TreeNameField { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 生成模板类型（CRUD、MasterDetail、Tree）
    /// </summary>
    public string? TemplateType { get; set; }

    /// <summary>
    /// 命名空间前缀（用于生成命名空间，如：Takt）
    /// </summary>
    public string? GenNamespacePrefix { get; set; }

    /// <summary>
    /// 生成业务名称
    /// </summary>
    public string? GenBusinessName { get; set; }

    /// <summary>
    /// 生成模块名称
    /// </summary>
    public string? GenModuleName { get; set; }

    /// <summary>
    /// 生成功能名
    /// </summary>
    public string? GenFunctionName { get; set; }

    /// <summary>
    /// 生成方式（zip=压缩包，path=自定义路径）
    /// </summary>
    public string? GenType { get; set; }

    /// <summary>
    /// 生成功能（多个功能用逗号分隔）
    /// </summary>
    public string? GenFunctions { get; set; }

    /// <summary>
    /// 代码生成路径
    /// </summary>
    public string? GenPath { get; set; }

    /// <summary>
    /// 其它生成选项
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// 上级菜单名称
    /// </summary>
    public string? ParentMenuName { get; set; }

    /// <summary>
    /// 权限前缀
    /// </summary>
    public string? PermissionPrefix { get; set; }

    /// <summary>
    /// 是否有表（0=有表，1=无表）
    /// </summary>
    public int IsDatabaseTable { get; set; }

    /// <summary>
    /// 是否生成菜单（0=是，1=否）
    /// </summary>
    public int IsGenMenu { get; set; }

    /// <summary>
    /// 是否生成翻译（0=是，1=否）
    /// </summary>
    public int IsGenTranslation { get; set; }

    /// <summary>
    /// 是否生成代码（0=是，1=否）
    /// </summary>
    public int IsGenCode { get; set; }

    /// <summary>
    /// 默认排序字段
    /// </summary>
    public string? DefaultSortField { get; set; }

    /// <summary>
    /// 默认排序（ASC=正序，DESC=倒序）
    /// </summary>
    public string? DefaultSortOrder { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedBy { get; set; }

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
    public string? DeletedBy { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedTime { get; set; }
}

/// <summary>
/// 代码生成表配置查询数据传输对象
/// </summary>
public class GenTableQueryDto : PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在库表名称、库表描述中搜索）
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// 库表名称
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// 库表描述
    /// </summary>
    public string? TableDescription { get; set; }

    /// <summary>
    /// 生成模板类型
    /// </summary>
    public string? TemplateType { get; set; }
}

/// <summary>
/// 创建代码生成表配置数据传输对象
/// </summary>
public class GenTableCreateDto
{
    /// <summary>
    /// 库表名称
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 库表描述
    /// </summary>
    public string? TableDescription { get; set; }

    /// <summary>
    /// 实体类名称
    /// </summary>
    public string? ClassName { get; set; }


    /// <summary>
    /// 子表名称
    /// </summary>
    public string? DetailTableName { get; set; }

    /// <summary>
    /// 子表关联字段
    /// </summary>
    public string? DetailRelationField { get; set; }

    /// <summary>
    /// 树编码字段
    /// </summary>
    public string? TreeCodeField { get; set; }

    /// <summary>
    /// 树父编码字段
    /// </summary>
    public string? TreeParentCodeField { get; set; }

    /// <summary>
    /// 树名称字段
    /// </summary>
    public string? TreeNameField { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 生成模板类型（CRUD、MasterDetail、Tree）
    /// </summary>
    public string? TemplateType { get; set; }

    /// <summary>
    /// 命名空间前缀（用于生成命名空间，如：Takt）
    /// </summary>
    public string? GenNamespacePrefix { get; set; }

    /// <summary>
    /// 生成业务名称
    /// </summary>
    public string? GenBusinessName { get; set; }

    /// <summary>
    /// 生成模块名称
    /// </summary>
    public string? GenModuleName { get; set; }

    /// <summary>
    /// 生成功能名
    /// </summary>
    public string? GenFunctionName { get; set; }

    /// <summary>
    /// 生成方式（zip=压缩包，path=自定义路径）
    /// </summary>
    public string? GenType { get; set; }

    /// <summary>
    /// 生成功能（多个功能用逗号分隔）
    /// </summary>
    public string? GenFunctions { get; set; }

    /// <summary>
    /// 代码生成路径
    /// </summary>
    public string? GenPath { get; set; }

    /// <summary>
    /// 其它生成选项
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// 上级菜单名称
    /// </summary>
    public string? ParentMenuName { get; set; }

    /// <summary>
    /// 权限前缀
    /// </summary>
    public string? PermissionPrefix { get; set; }

    /// <summary>
    /// 是否有表（0=有表，1=无表）
    /// </summary>
    public int IsDatabaseTable { get; set; } = 1;

    /// <summary>
    /// 是否生成菜单（0=是，1=否）
    /// </summary>
    public int IsGenMenu { get; set; } = 1;

    /// <summary>
    /// 是否生成翻译（0=是，1=否）
    /// </summary>
    public int IsGenTranslation { get; set; } = 1;

    /// <summary>
    /// 是否生成代码（0=是，1=否）
    /// </summary>
    public int IsGenCode { get; set; } = 1;

    /// <summary>
    /// 默认排序字段
    /// </summary>
    public string? DefaultSortField { get; set; }

    /// <summary>
    /// 默认排序（ASC=正序，DESC=倒序）
    /// </summary>
    public string? DefaultSortOrder { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// 更新代码生成表配置数据传输对象
/// </summary>
public class GenTableUpdateDto : GenTableCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

