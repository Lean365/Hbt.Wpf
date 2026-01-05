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
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public DictionaryDataDto()
    {
        TypeCode = string.Empty;
        DataLabel = string.Empty;
        I18nKey = string.Empty;
        DataValue = string.Empty;
        ExtLabel = string.Empty;
        ExtValue = string.Empty;
        CssClass = string.Empty;
        ListClass = string.Empty;
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
    /// 字典类型代码
    /// </summary>
    public string TypeCode { get; set; }
    
    /// <summary>
    /// 数据标签
    /// </summary>
    public string DataLabel { get; set; }
    
    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; }
    
    /// <summary>
    /// 数据值
    /// </summary>
    public string DataValue { get; set; }
    
    /// <summary>
    /// 扩展标签
    /// </summary>
    public string ExtLabel { get; set; }
    
    /// <summary>
    /// 扩展值
    /// </summary>
    public string ExtValue { get; set; }
    
    /// <summary>
    /// CSS类名
    /// </summary>
    public string CssClass { get; set; }
    
    /// <summary>
    /// 列表CSS类名
    /// </summary>
    public string ListClass { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
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
/// 字典数据查询数据传输对象
/// </summary>
public class DictionaryDataQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public DictionaryDataQueryDto()
    {
        Keywords = string.Empty;
        TypeCode = string.Empty;
        DataLabel = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在数据标签、数据值中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 字典类型代码
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// 数据标签
    /// </summary>
    public string DataLabel { get; set; }
}

/// <summary>
/// 创建字典数据传输对象
/// </summary>
public class DictionaryDataCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public DictionaryDataCreateDto()
    {
        TypeCode = string.Empty;
        DataLabel = string.Empty;
        I18nKey = string.Empty;
        DataValue = string.Empty;
        ExtLabel = string.Empty;
        ExtValue = string.Empty;
        CssClass = string.Empty;
        ListClass = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 字典类型代码
    /// </summary>
    public string TypeCode { get; set; }

    /// <summary>
    /// 数据标签
    /// </summary>
    public string DataLabel { get; set; }

    /// <summary>
    /// 国际化键
    /// </summary>
    public string I18nKey { get; set; }

    /// <summary>
    /// 数据值
    /// </summary>
    public string DataValue { get; set; }

    /// <summary>
    /// 扩展标签
    /// </summary>
    public string ExtLabel { get; set; }

    /// <summary>
    /// 扩展值
    /// </summary>
    public string ExtValue { get; set; }

    /// <summary>
    /// CSS类名
    /// </summary>
    public string CssClass { get; set; }

    /// <summary>
    /// 列表CSS类名
    /// </summary>
    public string ListClass { get; set; }

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
/// 更新字典数据传输对象
/// </summary>
public class DictionaryDataUpdateDto : DictionaryDataCreateDto
{
    /// <summary>
    /// 字典数据ID
    /// </summary>
    public long Id { get; set; }
}
