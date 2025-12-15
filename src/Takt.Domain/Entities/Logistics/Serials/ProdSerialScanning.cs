// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Serials
// 文件名称：ProdSerialScanning.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号扫描记录实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Serials;

/// <summary>
/// 产品序列号扫描记录实体
/// 记录序列号的扫描操作（入库/出库）
/// </summary>
[SugarTable("takt_logistics_prod_serial_scanning", "产品序列号扫描记录表")]
[SugarIndex("IX_takt_logistics_prod_serial_scanning_inbound_full_serial_number", nameof(ProdSerialScanning.InboundFullSerialNumber), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_scanning_outbound_full_serial_number", nameof(ProdSerialScanning.OutboundFullSerialNumber), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_scanning_created_time", nameof(ProdSerialScanning.CreatedTime), OrderByType.Desc, false)]
public class ProdSerialScanning : BaseEntity
{
    /// <summary>
    /// 入库完整序列号
    /// 入库扫描的完整序列号（包含物料、序列号、数量）
    /// </summary>
    [SugarColumn(ColumnName = "inbound_full_serial_number", ColumnDescription = "入库序列号", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// 入库扫描的日期
    /// </summary>
    [SugarColumn(ColumnName = "inbound_date", ColumnDescription = "入库日期", ColumnDataType = "datetime", IsNullable = true)]
    public DateTime? InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// 入库扫描时的客户端登录用户名称
    /// </summary>
    [SugarColumn(ColumnName = "inbound_client", ColumnDescription = "入库用户", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// 入库扫描时的IP地址
    /// </summary>
    [SugarColumn(ColumnName = "inbound_ip", ColumnDescription = "入库IP", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// 入库扫描时的机器名称
    /// </summary>
    [SugarColumn(ColumnName = "inbound_machine_name", ColumnDescription = "入库机器名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// 入库扫描时的登录地点
    /// </summary>
    [SugarColumn(ColumnName = "inbound_location", ColumnDescription = "入库地点", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// 入库扫描时的操作系统信息
    /// </summary>
    [SugarColumn(ColumnName = "inbound_os", ColumnDescription = "入库OS", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? InboundOs { get; set; }


    /// <summary>
    /// 出库单号
    /// </summary>
    [SugarColumn(ColumnName = "outbound_no", ColumnDescription = "出库单号", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? OutboundNo { get; set; }

    /// <summary>
    /// 仕向编码
    /// 产品的仕向编码（目标市场/规格）
    /// </summary>
    [SugarColumn(ColumnName = "dest_code", ColumnDescription = "仕向编码", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? DestCode { get; set; }

    /// <summary>
    /// 目的地港口
    /// </summary>
    [SugarColumn(ColumnName = "dest_port", ColumnDescription = "目的地港口", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? DestPort { get; set; }

    /// <summary>
    /// 出库日期
    /// 出库扫描的日期
    /// </summary>
    [SugarColumn(ColumnName = "outbound_date", ColumnDescription = "出库日期", ColumnDataType = "datetime", IsNullable = true)]
    public DateTime? OutboundDate { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// 出库扫描的完整序列号（包含物料、序列号、数量）
    /// </summary>
    [SugarColumn(ColumnName = "outbound_full_serial_number", ColumnDescription = "出库序列号", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库用户
    /// 出库扫描时的客户端登录用户名称
    /// </summary>
    [SugarColumn(ColumnName = "outbound_client", ColumnDescription = "出库用户", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// 出库扫描时的IP地址
    /// </summary>
    [SugarColumn(ColumnName = "outbound_ip", ColumnDescription = "出库IP", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// 出库扫描时的机器名称
    /// </summary>
    [SugarColumn(ColumnName = "outbound_machine_name", ColumnDescription = "出库机器名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// 出库扫描时的登录地点
    /// </summary>
    [SugarColumn(ColumnName = "outbound_location", ColumnDescription = "出库地点", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// 出库扫描时的操作系统信息
    /// </summary>
    [SugarColumn(ColumnName = "outbound_os", ColumnDescription = "出库OS", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? OutboundOs { get; set; }
}

