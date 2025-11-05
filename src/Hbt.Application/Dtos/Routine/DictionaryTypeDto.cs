//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DictionaryTypeDto.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 字典类型数据传输对象
//===================================================================

namespace Hbt.Application.Dtos.Routine;

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
    public int OrderNum { get; set; }
    public int IsBuiltin { get; set; }
    public int TypeStatus { get; set; }
}

/// <summary>
/// 字典类型查询数据传输对象
/// </summary>
public class DictionaryTypeQueryDto : Hbt.Common.Results.PagedQuery
{
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
