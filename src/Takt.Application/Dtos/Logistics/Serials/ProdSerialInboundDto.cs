// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
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
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialInboundDto()
    {
        FullSerialNumber = string.Empty;
        MaterialCode = string.Empty;
        SerialNumber = string.Empty;
        InboundNo = string.Empty;
        Warehouse = string.Empty;
        Location = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        InboundDate = DateTime.Now;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 完整序列号
    /// </summary>
    public string FullSerialNumber { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// 入库单号
    /// </summary>
    public string InboundNo { get; set; }
    
    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime InboundDate { get; set; }
    
    /// <summary>
    /// 仓库
    /// </summary>
    public string Warehouse { get; set; }
    
    /// <summary>
    /// 库位
    /// </summary>
    public string Location { get; set; }
    
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
/// 产品序列号入库记录查询数据传输对象
/// </summary>
public class ProdSerialInboundQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialInboundQueryDto()
    {
        var now = DateTime.Now;
        Keywords = string.Empty;
        MaterialCode = string.Empty;
        InboundNo = string.Empty;
        SerialNumber = string.Empty;
        InboundDateFrom = new DateTime(now.Year, now.Month, 1);
        InboundDateTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    /// <summary>
    /// 搜索关键词（支持在入库单号、序列号、完整序列号中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 入库单号
    /// </summary>
    public string InboundNo { get; set; }
    
    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; }
    
    /// <summary>
    /// 入库日期（起始，默认为本月第一天）
    /// </summary>
    public DateTime InboundDateFrom { get; set; }
    
    /// <summary>
    /// 入库日期（结束，默认为本月最后一天）
    /// </summary>
    public DateTime InboundDateTo { get; set; }
}

/// <summary>
/// 创建产品序列号入库记录数据传输对象
/// </summary>
public class ProdSerialInboundCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialInboundCreateDto()
    {
        FullSerialNumber = string.Empty;
        InboundDate = DateTime.Now;
    }

    /// <summary>
    /// 完整序列号
    /// </summary>
    public string FullSerialNumber { get; set; }
    
    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime InboundDate { get; set; }

}

/// <summary>
/// 更新产品序列号入库记录数据传输对象
/// </summary>
public class ProdSerialInboundUpdateDto : ProdSerialInboundCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

