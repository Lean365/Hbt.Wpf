// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Serials
// 文件名称：ProdSerialInboundDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号入库记录数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Serials;

/// <summary>
/// 产品序列号入库记录数据传输对象
/// </summary>
public class ProdSerialInboundDto
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

    // ProdSerialInbound 特有字段
    public string FullSerialNumber { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string InboundNo { get; set; } = string.Empty;
    public DateTime InboundDate { get; set; }
    public string? Warehouse { get; set; }
    public string? Location { get; set; }
    public string? Operator { get; set; }
}

/// <summary>
/// 产品序列号入库记录查询数据传输对象
/// </summary>
public class ProdSerialInboundQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在入库单号、序列号、完整序列号中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    public string? MaterialCode { get; set; }
    public string? InboundNo { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? InboundDateFrom { get; set; }
    public DateTime? InboundDateTo { get; set; }
}

/// <summary>
/// 创建产品序列号入库记录数据传输对象
/// </summary>
public class ProdSerialInboundCreateDto
{
    public string FullSerialNumber { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string InboundNo { get; set; } = string.Empty;
    public DateTime InboundDate { get; set; }
    public string? Warehouse { get; set; }
    public string? Location { get; set; }
    public string? Operator { get; set; }
}

/// <summary>
/// 更新产品序列号入库记录数据传输对象
/// </summary>
public class ProdSerialInboundUpdateDto : ProdSerialInboundCreateDto
{
    public long Id { get; set; }
}

