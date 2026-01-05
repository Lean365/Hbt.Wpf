// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Serials
// 文件名称：ProdSerialScanningDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号扫描记录数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Serials;

/// <summary>
/// 产品序列号扫描记录数据传输对象
/// </summary>
public class ProdSerialScanningDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialScanningDto()
    {
        InboundFullSerialNumber = string.Empty;
        InboundClient = string.Empty;
        InboundIp = string.Empty;
        InboundMachineName = string.Empty;
        InboundLocation = string.Empty;
        InboundOs = string.Empty;
        OutboundFullSerialNumber = string.Empty;
        OutboundClient = string.Empty;
        OutboundIp = string.Empty;
        OutboundMachineName = string.Empty;
        OutboundLocation = string.Empty;
        OutboundOs = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        InboundDate = DateTime.Now;
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
    /// 入库完整序列号
    /// </summary>
    public string InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string InboundOs { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string OutboundOs { get; set; }

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
/// 产品序列号扫描记录查询数据传输对象
/// </summary>
public class ProdSerialScanningQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialScanningQueryDto()
    {
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        Keywords = string.Empty;
        MaterialCode = string.Empty;
        InboundFullSerialNumber = string.Empty;
        OutboundFullSerialNumber = string.Empty;
        InboundDateFrom = monthStart;
        InboundDateTo = monthEnd;
        OutboundDateFrom = monthStart;
        OutboundDateTo = monthEnd;
    }

    /// <summary>
    /// 搜索关键词（支持在入库/出库完整序列号中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 物料代码（通过完整序列号解析）
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期（起始，默认为本月第一天）
    /// </summary>
    public DateTime InboundDateFrom { get; set; }

    /// <summary>
    /// 入库日期（结束，默认为本月最后一天）
    /// </summary>
    public DateTime InboundDateTo { get; set; }

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
/// 入库扫描记录创建数据传输对象
/// </summary>
public class ProdSerialInboundScanningCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialInboundScanningCreateDto()
    {
        InboundFullSerialNumber = string.Empty;
        InboundClient = string.Empty;
        InboundIp = string.Empty;
        InboundMachineName = string.Empty;
        InboundLocation = string.Empty;
        InboundOs = string.Empty;
        InboundDate = DateTime.Now;
    }

    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string InboundOs { get; set; }
}

/// <summary>
/// 出库扫描记录创建数据传输对象
/// </summary>
public class ProdSerialOutboundScanningCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialOutboundScanningCreateDto()
    {
        OutboundNo = string.Empty;
        DestCode = string.Empty;
        DestPort = string.Empty;
        OutboundFullSerialNumber = string.Empty;
        OutboundClient = string.Empty;
        OutboundIp = string.Empty;
        OutboundMachineName = string.Empty;
        OutboundLocation = string.Empty;
        OutboundOs = string.Empty;
        OutboundDate = DateTime.Now;
    }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string OutboundNo { get; set; }

    /// <summary>
    /// 仕向编码
    /// </summary>
    public string DestCode { get; set; }

    /// <summary>
    /// 目的地港口
    /// </summary>
    public string DestPort { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string OutboundOs { get; set; }
}

/// <summary>
/// 产品序列号扫描记录更新数据传输对象
/// </summary>
public class ProdSerialScanningUpdateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialScanningUpdateDto()
    {
        InboundFullSerialNumber = string.Empty;
        InboundClient = string.Empty;
        InboundIp = string.Empty;
        InboundMachineName = string.Empty;
        InboundLocation = string.Empty;
        InboundOs = string.Empty;
        OutboundFullSerialNumber = string.Empty;
        OutboundClient = string.Empty;
        OutboundIp = string.Empty;
        OutboundMachineName = string.Empty;
        OutboundLocation = string.Empty;
        OutboundOs = string.Empty;
        InboundDate = DateTime.Now;
        OutboundDate = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string InboundOs { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string OutboundOs { get; set; }
}

/// <summary>
/// 产品序列号扫描记录导出数据传输对象
/// </summary>
public class ProdSerialScanningExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdSerialScanningExportDto()
    {
        InboundFullSerialNumber = string.Empty;
        InboundClient = string.Empty;
        InboundIp = string.Empty;
        InboundMachineName = string.Empty;
        InboundLocation = string.Empty;
        InboundOs = string.Empty;
        OutboundFullSerialNumber = string.Empty;
        OutboundClient = string.Empty;
        OutboundIp = string.Empty;
        OutboundMachineName = string.Empty;
        OutboundLocation = string.Empty;
        OutboundOs = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        InboundDate = DateTime.Now;
        OutboundDate = DateTime.Now;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string InboundOs { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string OutboundOs { get; set; }

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
}

