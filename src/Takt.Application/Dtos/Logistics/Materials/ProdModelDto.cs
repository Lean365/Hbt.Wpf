// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
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

    // ProdModel 特有字段
    public string MaterialCode { get; set; } = string.Empty;
    public string ModelCode { get; set; } = string.Empty;
    public string DestCode { get; set; } = string.Empty;
}

/// <summary>
/// 产品机种查询数据传输对象
/// </summary>
public class ProdModelQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在物料代码、机种代码、目标代码中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    public string? MaterialCode { get; set; }
    public string? ModelCode { get; set; }
    public string? DestCode { get; set; }
}

/// <summary>
/// 创建产品机种数据传输对象
/// </summary>
public class ProdModelCreateDto
{
    public string MaterialCode { get; set; } = string.Empty;
    public string ModelCode { get; set; } = string.Empty;
    public string DestCode { get; set; } = string.Empty;
}

/// <summary>
/// 更新产品机种数据传输对象
/// </summary>
public class ProdModelUpdateDto : ProdModelCreateDto
{
    public long Id { get; set; }
}

