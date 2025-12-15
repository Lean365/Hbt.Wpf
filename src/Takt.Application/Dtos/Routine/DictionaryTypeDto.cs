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
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public DictionaryTypeDto()
    {
        TypeCode = string.Empty;
        TypeName = string.Empty;
        SqlScript = string.Empty;
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
    /// 类型代码
    /// </summary>
    public string TypeCode { get; set; }
    
    /// <summary>
    /// 类型名称
    /// </summary>
    public string TypeName { get; set; }
    
    /// <summary>
    /// 数据源（0=系统，1=SQL脚本）
    /// </summary>
    public int DataSource { get; set; } = 0;
    
    /// <summary>
    /// SQL脚本（当数据源为SQL脚本时使用）
    /// </summary>
    public string SqlScript { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 是否内置（0=否，1=是）
    /// </summary>
    public int IsBuiltin { get; set; }
    
    /// <summary>
    /// 类型状态（0=启用，1=禁用）
    /// </summary>
    public int TypeStatus { get; set; }
    
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
/// 字典类型查询数据传输对象
/// </summary>
public class DictionaryTypeQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public DictionaryTypeQueryDto()
    {
        Keywords = string.Empty;
        TypeCode = string.Empty;
        TypeName = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在类型代码、类型名称中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 类型代码
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    public string TypeName { get; set; }

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
    /// 构造函数：初始化默认值
    /// </summary>
    public DictionaryTypeCreateDto()
    {
        TypeCode = string.Empty;
        TypeName = string.Empty;
        SqlScript = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 类型代码
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// 数据源（0=系统，1=SQL脚本）
    /// </summary>
    public int DataSource { get; set; } = 0;

    /// <summary>
    /// SQL脚本（当数据源为SQL脚本时使用）
    /// </summary>
    public string SqlScript { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
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
