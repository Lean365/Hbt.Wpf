// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Materials
// 文件名称：ProdModelDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品机种数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Materials;

/// <summary>
/// 产品机种数据传输对象
/// </summary>
public class ProdModelDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdModelDto()
    {
        MaterialCode = string.Empty;
        ModelCode = string.Empty;
        DestCode = string.Empty;
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
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 机种代码
    /// </summary>
    public string ModelCode { get; set; }
    
    /// <summary>
    /// 目标代码
    /// </summary>
    public string DestCode { get; set; }
    
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
/// 产品机种查询数据传输对象
/// </summary>
public class ProdModelQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdModelQueryDto()
    {
        Keywords = string.Empty;
        MaterialCode = string.Empty;
        ModelCode = string.Empty;
        DestCode = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在物料代码、机种代码、目标代码中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 机种代码
    /// </summary>
    public string ModelCode { get; set; }
    
    /// <summary>
    /// 目标代码
    /// </summary>
    public string DestCode { get; set; }
}

/// <summary>
/// 创建产品机种数据传输对象
/// </summary>
public class ProdModelCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdModelCreateDto()
    {
        MaterialCode = string.Empty;
        ModelCode = string.Empty;
        DestCode = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 机种代码
    /// </summary>
    public string ModelCode { get; set; }
    
    /// <summary>
    /// 目标代码
    /// </summary>
    public string DestCode { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新产品机种数据传输对象
/// </summary>
public class ProdModelUpdateDto : ProdModelCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

