//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Dtos.Routine
// 文件名 : DictionaryDataDto.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 功能描述：字典数据传输对象
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

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
    public string TypeCode { get; set; } = string.Empty;
    public string DataLabel { get; set; } = string.Empty;
    public string I18nKey { get; set; } = string.Empty;
    public string? DataValue { get; set; }
    public string? ExtLabel { get; set; }
    public string? ExtValue { get; set; }
    public string? CssClass { get; set; }
    public string? ListClass { get; set; }
    public int OrderNum { get; set; }
}

/// <summary>
/// 字典数据查询数据传输对象
/// </summary>
public class DictionaryDataQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在数据标签、数据值中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 字典类型代码
    /// </summary>
    public string? TypeCode { get; set; }

    /// <summary>
    /// 数据标签
    /// </summary>
    public string? DataLabel { get; set; }
}

/// <summary>
/// 创建字典数据传输对象
/// </summary>
public class DictionaryDataCreateDto
{
    /// <summary>
    /// 字典类型代码
    /// </summary>
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// 数据标签
    /// </summary>
    public string DataLabel { get; set; } = string.Empty;

    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; } = string.Empty;

    /// <summary>
    /// 数据值
    /// </summary>
    public string? DataValue { get; set; }

    /// <summary>
    /// 扩展标签
    /// </summary>
    public string? ExtLabel { get; set; }

    /// <summary>
    /// 扩展值
    /// </summary>
    public string? ExtValue { get; set; }

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
