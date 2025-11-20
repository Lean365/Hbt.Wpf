//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Dtos.Routine
// 文件名 : DictionaryTypeDto.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 功能描述：字典类型数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 字典类型数据传输对象
/// 用于传输字典类型信息
/// </summary>
public class DictionaryTypeDto
{
    // 继承自 BaseEntity
    public long Id { get; set; }
    public string? Remarks { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedTime { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedTime { get; set; }
    public int IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedTime { get; set; }

    // DictionaryType 特有字段
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public int DataSource { get; set; } = 0;
    public string? SqlScript { get; set; }
    public int OrderNum { get; set; }
    public int IsBuiltin { get; set; }
    public int TypeStatus { get; set; }
}

/// <summary>
/// 字典类型查询数据传输对象
/// </summary>
public class DictionaryTypeQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在类型代码、类型名称中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 类型代码
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    public string? TypeName { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int? TypeStatus { get; set; }
}

/// <summary>
/// 创建字典类型数据传输对象
/// </summary>
public class DictionaryTypeCreateDto
{
    /// <summary>
    /// 类型代码
    /// </summary>
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// 类型名称
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 数据源（0=系统，1=SQL脚本）
    /// </summary>
    public int DataSource { get; set; } = 0;

    /// <summary>
    /// SQL脚本（当数据源为SQL脚本时使用）
    /// </summary>
    public string? SqlScript { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
}

/// <summary>
/// 更新字典类型数据传输对象
/// </summary>
public class DictionaryTypeUpdateDto : DictionaryTypeCreateDto
{
    /// <summary>
    /// 字典类型ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int TypeStatus { get; set; }
}
