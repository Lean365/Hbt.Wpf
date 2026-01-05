// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Serials
// 文件名称：ProdSerialOutboundDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号出库记录数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Serials;

/// <summary>
/// 产品序列号出库记录数据传输对象
/// </summary>
public class ProdSerialOutboundDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialOutboundDto()
    {
        FullSerialNumber = string.Empty;
        MaterialCode = string.Empty;
        SerialNumber = string.Empty;
        OutboundNo = string.Empty;
        DestCode = string.Empty;
        DestPort = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        OutboundDate = DateTime.Now;
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
    /// 出库单号
    /// </summary>
    public string OutboundNo { get; set; }
    
    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }
    
    /// <summary>
    /// 目标代码
    /// </summary>
    public string DestCode { get; set; }
    
    /// <summary>
    /// 目的港
    /// </summary>
    public string DestPort { get; set; }
    
    /// <summary>
    /// 重量
    /// 出库货物的重量（单位：千克）
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 体积
    /// 出库货物的体积（单位：立方米）
    /// </summary>
    public decimal? Volume { get; set; }
    
    /// <summary>
    /// 箱数
    /// 出库货物的箱数
    /// </summary>
    public int? CarQuantity { get; set; }
    
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
/// 产品序列号出库记录查询数据传输对象
/// </summary>
public class ProdSerialOutboundQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialOutboundQueryDto()
    {
        var now = DateTime.Now;
        Keywords = string.Empty;
        MaterialCode = string.Empty;
        OutboundNo = string.Empty;
        SerialNumber = string.Empty;
        OutboundDateFrom = new DateTime(now.Year, now.Month, 1);
        OutboundDateTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    /// <summary>
    /// 搜索关键词（支持在出库单号、序列号、完整序列号中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 出库单号
    /// </summary>
    public string OutboundNo { get; set; }
    
    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; }
    
    /// <summary>
    /// 出库日期（起始，默认为本月第一天）
    /// </summary>
    public DateTime OutboundDateFrom { get; set; }
    
    /// <summary>
    /// 出库日期（结束，默认为本月最后一天）
    /// </summary>
    public DateTime OutboundDateTo { get; set; }
}

/// <summary>
/// 创建产品序列号出库记录数据传输对象
/// </summary>
public class ProdSerialOutboundCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialOutboundCreateDto()
    {
        FullSerialNumber = string.Empty;
        DestCode = string.Empty;
        OutboundNo = string.Empty;
        DestPort = string.Empty;
        OutboundDate = DateTime.Now;
    }

    /// <summary>
    /// 完整序列号
    /// </summary>
    public string FullSerialNumber { get; set; }    

    /// <summary>
    /// 仕向编码
    /// </summary>
    public string DestCode { get; set; }
    
    /// <summary>
    /// 出库单号
    /// </summary>
    public string OutboundNo { get; set; }
    
    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }   
    
    /// <summary>
    /// 目的港
    /// </summary>
    public string DestPort { get; set; }

}

/// <summary>
/// 更新产品序列号出库记录数据传输对象
/// </summary>
public class ProdSerialOutboundUpdateDto : ProdSerialOutboundCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 产品序列号出库统计数据传输对象
/// </summary>
public class ProdSerialOutboundStatisticDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialOutboundStatisticDto()
    {
        Period = string.Empty;
        DestCode = string.Empty;
        DestPort = string.Empty;
    }

    /// <summary>
    /// 统计维度（年或月）
    /// 格式：年统计为 "2025"，月统计为 "2025-01"
    /// </summary>
    public string Period { get; set; }

    /// <summary>
    /// 仕向编码（DestCode）
    /// </summary>
    public string DestCode { get; set; }

    /// <summary>
    /// 目的地港口（DestPort）
    /// </summary>
    public string DestPort { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// 产品序列号出库统计查询参数
/// </summary>
public class ProdSerialOutboundStatisticQueryDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialOutboundStatisticQueryDto()
    {
        var now = DateTime.Now;
        StatisticType = "Year";
        Dimension = "DestCode";
        StartDate = new DateTime(now.Year, now.Month, 1);
        EndDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    /// <summary>
    /// 统计类型：Year（按年）或 Month（按月）或 Both（同时返回按年和按月）
    /// </summary>
    public string StatisticType { get; set; }

    /// <summary>
    /// 统计维度：DestCode（按仕向编码）或 DestPort（按目的地港口）或 Both（同时统计）
    /// </summary>
    public string Dimension { get; set; }

    /// <summary>
    /// 起始日期（默认为本月第一天）
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 结束日期（默认为本月最后一天）
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// 产品序列号出库统计结果（包含按年和按月统计清单）
/// </summary>
public class ProdSerialOutboundStatisticResultDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialOutboundStatisticResultDto()
    {
        YearStatistics = new List<ProdSerialOutboundStatisticDto>();
        MonthStatistics = new List<ProdSerialOutboundStatisticDto>();
    }

    /// <summary>
    /// 近年统计清单（按年统计）
    /// </summary>
    public List<ProdSerialOutboundStatisticDto> YearStatistics { get; set; }

    /// <summary>
    /// 按月统计清单（按月统计）
    /// </summary>
    public List<ProdSerialOutboundStatisticDto> MonthStatistics { get; set; }
}

