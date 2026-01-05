//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Dtos.Routine
// 文件名 : SettingDto.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 功能描述：系统设置数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 系统设置数据传输对象
/// 用于传输系统设置信息
/// </summary>
public class SettingDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public SettingDto()
    {
        SettingKey = string.Empty;
        SettingValue = string.Empty;
        Category = string.Empty;
        SettingDescription = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 设置ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 设置键
    /// </summary>
    public string SettingKey { get; set; }
    
    /// <summary>
    /// 设置值
    /// </summary>
    public string SettingValue { get; set; }
    
    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 设置描述
    /// </summary>
    public string SettingDescription { get; set; }
    
    /// <summary>
    /// 设置类型
    /// </summary>
    public int SettingType { get; set; }
    
    /// <summary>
    /// 是否内置（0=是，1=否）
    /// </summary>
    public int IsBuiltin { get; set; }
    
    /// <summary>
    /// 是否默认
    /// </summary>
    public int IsDefault { get; set; }
    
    /// <summary>
    /// 是否可编辑（0=是，1=否）
    /// </summary>
    public int IsEditable { get; set; }

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
/// 系统设置查询数据传输对象
/// </summary>
public class SettingQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public SettingQueryDto()
    {
        Keywords = string.Empty;
        SettingKey = string.Empty;
        Category = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在设置键、设置值、描述中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 设置键
    /// </summary>
    public string SettingKey { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; }
}

/// <summary>
/// 创建系统设置数据传输对象
/// </summary>
public class SettingCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public SettingCreateDto()
    {
        SettingKey = string.Empty;
        SettingValue = string.Empty;
        Category = string.Empty;
        SettingDescription = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 设置键
    /// </summary>
    public string SettingKey { get; set; }

    /// <summary>
    /// 设置值
    /// </summary>
    public string SettingValue { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 设置描述
    /// </summary>
    public string SettingDescription { get; set; }

    /// <summary>
    /// 设置类型
    /// </summary>
    public int SettingType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
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
