//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : TranslationDto.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 翻译数据传输对象
//===================================================================

namespace Hbt.Application.Dtos.Routine;

/// <summary>
/// 翻译数据传输对象
/// 用于传输翻译信息
/// </summary>
public class TranslationDto
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

    // Translation 特有字段
    public string LanguageCode { get; set; }
    public string TranslationKey { get; set; } = string.Empty;
    public string TranslationValue { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? Description { get; set; }
    public int OrderNum { get; set; }
}

/// <summary>
/// 翻译查询数据传输对象
/// </summary>
public class TranslationQueryDto : Hbt.Common.Results.PagedQuery
{
    /// <summary>
    /// 语言ID
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// 翻译键
    /// </summary>
    public string? TranslationKey { get; set; }

    /// <summary>
    /// 模块
    /// </summary>
    public string? Module { get; set; }
}

/// <summary>
/// 创建翻译数据传输对象
/// </summary>
public class TranslationCreateDto
{
    /// <summary>
    /// 语言ID
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; } = string.Empty;

    /// <summary>
    /// 翻译值
    /// </summary>
    public string TranslationValue { get; set; } = string.Empty;

    /// <summary>
    /// 模块
    /// </summary>
    public string? Module { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
}

/// <summary>
/// 更新翻译数据传输对象
/// </summary>
public class TranslationUpdateDto : TranslationCreateDto
{
    /// <summary>
    /// 翻译ID
    /// </summary>
    public long Id { get; set; }
}
