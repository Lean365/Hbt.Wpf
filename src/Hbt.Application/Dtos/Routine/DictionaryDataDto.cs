//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DictionaryDataDto.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 字典数据传输对象
//===================================================================

namespace Hbt.Application.Dtos.Routine;

/// <summary>
/// 字典数据传输对象
/// 用于传输字典数据信息
/// </summary>
public class DictionaryDataDto
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

    // DictionaryData 特有字段
    public string TypeCode { get; set; }
    public string DataCode { get; set; } = string.Empty;
    public string DataName { get; set; } = string.Empty;
    public string? DataValue { get; set; }
    public string? ExtendedData { get; set; }
    public string? CssClass { get; set; }
    public string? ListClass { get; set; }
    public int OrderNum { get; set; }
}

/// <summary>
/// 字典数据查询数据传输对象
/// </summary>
public class DictionaryDataQueryDto : Hbt.Common.Results.PagedQuery
{
    /// <summary>
    /// 字典类型ID
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// 数据代码
    /// </summary>
    public string? DataCode { get; set; }

    /// <summary>
    /// 数据名称
    /// </summary>
    public string? DataName { get; set; }
}

/// <summary>
/// 创建字典数据传输对象
/// </summary>
public class DictionaryDataCreateDto
{
    /// <summary>
    /// 字典类型ID
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// 数据代码
    /// </summary>
    public string DataCode { get; set; } = string.Empty;

    /// <summary>
    /// 数据名称
    /// </summary>
    public string DataName { get; set; } = string.Empty;

    /// <summary>
    /// 数据值
    /// </summary>
    public string? DataValue { get; set; }

    /// <summary>
    /// 扩展数据
    /// </summary>
    public string? ExtendedData { get; set; }

    /// <summary>
    /// CSS类名
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// 列表CSS类名
    /// </summary>
    public string? ListClass { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
}

/// <summary>
/// 更新字典数据传输对象
/// </summary>
public class DictionaryDataUpdateDto : DictionaryDataCreateDto
{
    /// <summary>
    /// 字典数据ID
    /// </summary>
    public long Id { get; set; }
}
