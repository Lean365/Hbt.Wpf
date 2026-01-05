//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Dtos.Routine
// 文件名 : LanguageDto.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 功能描述：语言数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 语言数据传输对象
/// 用于传输语言信息
/// </summary>
public class LanguageDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public LanguageDto()
    {
        LanguageCode = string.Empty;
        LanguageName = string.Empty;
        NativeName = string.Empty;
        LanguageIcon = string.Empty;
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
    /// 语言代码
    /// </summary>
    public string LanguageCode { get; set; }
    
    /// <summary>
    /// 语言名称
    /// </summary>
    public string LanguageName { get; set; }
    
    /// <summary>
    /// 本地化名称
    /// </summary>
    public string NativeName { get; set; }
    
    /// <summary>
    /// 语言图标
    /// </summary>
    public string LanguageIcon { get; set; }
    
    /// <summary>
    /// 是否默认（0=否，1=是）
    /// </summary>
    public int IsDefault { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 是否内置（0=否，1=是）
    /// </summary>
    public int IsBuiltin { get; set; }
    
    /// <summary>
    /// 语言状态（0=启用，1=禁用）
    /// </summary>
    public int LanguageStatus { get; set; }
    
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
/// 语言查询数据传输对象
/// </summary>
public class LanguageQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public LanguageQueryDto()
    {
        Keywords = string.Empty;
        LanguageCode = string.Empty;
        LanguageName = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在语言代码、语言名称、本地名称中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 语言代码
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// 语言名称
    /// </summary>
    public string LanguageName { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int? LanguageStatus { get; set; }
}

/// <summary>
/// 创建语言数据传输对象
/// </summary>
public class LanguageCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public LanguageCreateDto()
    {
        LanguageCode = string.Empty;
        LanguageName = string.Empty;
        NativeName = string.Empty;
        LanguageIcon = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 语言代码
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// 语言名称
    /// </summary>
    public string LanguageName { get; set; }

    /// <summary>
    /// 本地化名称
    /// </summary>
    public string NativeName { get; set; }

    /// <summary>
    /// 语言图标
    /// </summary>
    public string LanguageIcon { get; set; }

    /// <summary>
    /// 是否默认（0=否，1=是）
    /// </summary>
    public int IsDefault { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 是否内置（0=否，1=是）
    /// </summary>
    public int IsBuiltin { get; set; }

    /// <summary>
    /// 语言状态（0=启用，1=禁用）
    /// </summary>
    public int LanguageStatus { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新语言数据传输对象
/// </summary>
public class LanguageUpdateDto : LanguageCreateDto
{
    /// <summary>
    /// 语言ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 语言状态（0=启用，1=禁用）
    /// </summary>
    public new int LanguageStatus { get; set; }
}

/// <summary>
/// 语言选项DTO
/// 用于下拉列表等UI组件
/// </summary>
public class LanguageOptionDto : Takt.Common.Models.SelectOptionModel<string>
{
    /// <summary>
    /// 语言ID（保留用于后端业务逻辑）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 语言代码（如：zh-CN, en-US）
    /// 映射到父类的 Value 属性
    /// </summary>
    public string Code
    {
        get => DataValue;
        set => DataValue = value;
    }

    /// <summary>
    /// 语言名称
    /// 映射到父类的 Label 属性
    /// </summary>
    public string Name
    {
        get => DataLabel;
        set => DataLabel = value;
    }
}