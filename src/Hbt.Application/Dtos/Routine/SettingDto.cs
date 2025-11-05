//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : SettingDto.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 系统设置数据传输对象
//===================================================================

namespace Hbt.Application.Dtos.Routine;

/// <summary>
/// 系统设置数据传输对象
/// 用于传输系统设置信息
/// </summary>
public class SettingDto
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

    // Setting 特有字段
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int OrderNum { get; set; }
    public string? SettingDescription { get; set; }
    public int SettingType { get; set; }
    public int IsBuiltin { get; set; }
    public int IsDefault { get; set; }
    public int IsEditable { get; set; }
}

/// <summary>
/// 系统设置查询数据传输对象
/// </summary>
public class SettingQueryDto : Hbt.Common.Results.PagedQuery
{
    /// <summary>
    /// 设置键
    /// </summary>
    public string? SettingKey { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// 创建系统设置数据传输对象
/// </summary>
public class SettingCreateDto
{
    /// <summary>
    /// 设置键
    /// </summary>
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// 设置值
    /// </summary>
    public string SettingValue { get; set; } = string.Empty;

    /// <summary>
    /// 分类
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 设置描述
    /// </summary>
    public string? SettingDescription { get; set; }

    /// <summary>
    /// 设置类型
    /// </summary>
    public int SettingType { get; set; }
}

/// <summary>
/// 更新系统设置数据传输对象
/// </summary>
public class SettingUpdateDto : SettingCreateDto
{
    /// <summary>
    /// 设置ID
    /// </summary>
    public long Id { get; set; }
}
