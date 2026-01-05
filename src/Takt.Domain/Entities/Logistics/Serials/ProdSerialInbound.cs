// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Serials
// 文件名称：ProdSerialInbound.cs
// 创建时间：2025-10-22
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号入库记录实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Serials;

/// <summary>
/// 产品序列号入库记录实体
/// 记录序列号的入库信息
/// </summary>
[SugarTable("takt_logistics_prod_serial_inbound", "产品序列号入库记录表")]
[SugarIndex("IX_takt_logistics_prod_serial_inbound_full_serial_number", nameof(ProdSerialInbound.FullSerialNumber), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_inbound_material_code", nameof(ProdSerialInbound.MaterialCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_inbound_inbound_no", nameof(ProdSerialInbound.InboundNo), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_inbound_inbound_date", nameof(ProdSerialInbound.InboundDate), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_inbound_created_time", nameof(ProdSerialInbound.CreatedTime), OrderByType.Desc, false)]
public class ProdSerialInbound : BaseEntity
{

    /// <summary>
    /// 完整序列号
    /// 包含物料、序列号、数量的完整序列号
    /// </summary>
    [SugarColumn(ColumnName = "full_serial_number", ColumnDescription = "完整序列号", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string FullSerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 物料编码
    /// 从完整序列号中提取的物料编码
    /// </summary>
    [SugarColumn(ColumnName = "material_code", ColumnDescription = "物料编码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// 真正序列号
    /// 从完整序列号中提取的真正序列号
    /// </summary>
    [SugarColumn(ColumnName = "serial_number", ColumnDescription = "真正序列号", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// 从完整序列号中提取的数量
    /// </summary>
    [SugarColumn(ColumnName = "quantity", ColumnDescription = "数量", ColumnDataType = "decimal", Length = 18, DecimalDigits = 2, IsNullable = false, DefaultValue = "0")]
    public decimal Quantity { get; set; } = 0;


    /// <summary>
    /// 入库单号
    /// </summary>
    [SugarColumn(ColumnName = "inbound_no", ColumnDescription = "入库单号", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string InboundNo { get; set; } = string.Empty;

    /// <summary>
    /// 入库日期
    /// </summary>
    [SugarColumn(ColumnName = "inbound_date", ColumnDescription = "入库日期", ColumnDataType = "datetime", IsNullable = false)]
    public DateTime InboundDate { get; set; }

    /// <summary>
    /// 仓库
    /// </summary>
    [SugarColumn(ColumnName = "warehouse", ColumnDescription = "仓库", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? Warehouse { get; set; }

    /// <summary>
    /// 库位
    /// </summary>
    [SugarColumn(ColumnName = "location", ColumnDescription = "库位", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? Location { get; set; }
}
