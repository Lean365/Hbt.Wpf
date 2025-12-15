// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Serials
// 文件名称：ProdSerialOutbound.cs
// 创建时间：2025-10-22
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号出库记录实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Serials;

/// <summary>
/// 产品序列号出库记录实体
/// 记录序列号的出库信息（包含仕向地、港口信息）
/// </summary>
[SugarTable("takt_logistics_prod_serial_outbound", "产品序列号出库记录表")]
[SugarIndex("IX_takt_logistics_prod_serial_outbound_full_serial_number", nameof(ProdSerialOutbound.FullSerialNumber), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_outbound_material_code", nameof(ProdSerialOutbound.MaterialCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_outbound_outbound_no", nameof(ProdSerialOutbound.OutboundNo), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_outbound_outbound_date", nameof(ProdSerialOutbound.OutboundDate), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logistics_prod_serial_outbound_created_time", nameof(ProdSerialOutbound.CreatedTime), OrderByType.Desc, false)]
public class ProdSerialOutbound : BaseEntity
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
    /// 出库单号
    /// </summary>
    [SugarColumn(ColumnName = "outbound_no", ColumnDescription = "出库单号", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string OutboundNo { get; set; } = string.Empty;

    /// <summary>
    /// 出库日期
    /// </summary>
    [SugarColumn(ColumnName = "outbound_date", ColumnDescription = "出库日期", ColumnDataType = "datetime", IsNullable = false)]
    public DateTime OutboundDate { get; set; }

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
    /// 重量
    /// 出库货物的重量（单位：千克）
    /// </summary>
    [SugarColumn(ColumnName = "weight", ColumnDescription = "重量", ColumnDataType = "decimal", Length = 18, DecimalDigits = 10, IsNullable = true, DefaultValue = "0")]
    public decimal? Weight { get; set; }

    /// <summary>
    /// 体积
    /// 出库货物的体积（单位：立方米）
    /// </summary>
    [SugarColumn(ColumnName = "volume", ColumnDescription = "体积", ColumnDataType = "decimal", Length = 18, DecimalDigits = 6, IsNullable = true, DefaultValue = "0")]
    public decimal? Volume { get; set; }

    /// <summary>
    /// 箱数（卡通箱数量）
    /// 出库货物的卡通箱数量（Car 表示 Carton，即卡通箱）
    /// </summary>
    [SugarColumn(ColumnName = "car_quantity", ColumnDescription = "箱数", ColumnDataType = "int", IsNullable = true, DefaultValue = "0")]
    public int? CarQuantity { get; set; }
}
