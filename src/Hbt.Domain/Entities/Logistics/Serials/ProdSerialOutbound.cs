// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：ProdSerialOutbound.cs
// 创建时间：2025-10-22
// 创建人：Hbt365(Cursor AI)
// 功能描述：产品序列号出库记录实体
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// ========================================

using SqlSugar;

namespace Hbt.Domain.Entities.Logistics.Serials;

/// <summary>
/// 产品序列号出库记录实体
/// 记录序列号的出库信息（包含仕向地、港口信息）
/// </summary>
[SugarTable("hbt_logistics_prod_serial_outbound", "产品序列号出库记录表")]
[SugarIndex("IX_hbt_logistics_prod_serial_outbound_full_serial_number", nameof(ProdSerialOutbound.FullSerialNumber), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_outbound_material_code", nameof(ProdSerialOutbound.MaterialCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_outbound_outbound_no", nameof(ProdSerialOutbound.OutboundNo), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_outbound_outbound_date", nameof(ProdSerialOutbound.OutboundDate), OrderByType.Desc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_outbound_destination", nameof(ProdSerialOutbound.Destination), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_outbound_created_time", nameof(ProdSerialOutbound.CreatedTime), OrderByType.Desc, false)]
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
    /// 仕向地
    /// 产品的目标市场/地区
    /// </summary>
    [SugarColumn(ColumnName = "destination", ColumnDescription = "仕向地", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? Destination { get; set; }

    /// <summary>
    /// 仕向编码
    /// 产品的仕向编码（目标市场/规格）
    /// </summary>
    [SugarColumn(ColumnName = "dest_code", ColumnDescription = "仕向编码", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? DestCode { get; set; }


    /// <summary>
    /// 目的地港口
    /// </summary>
    [SugarColumn(ColumnName = "destination_port", ColumnDescription = "目的地港口", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? DestinationPort { get; set; }

    /// <summary>
    /// 出库员
    /// </summary>
    [SugarColumn(ColumnName = "operator", ColumnDescription = "出库员", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? Operator { get; set; }


    /// <summary>
    /// 关联的产品序列号信息（导航属性）
    /// 通过物料编码关联到主表
    /// </summary>
    [Navigate(NavigateType.ManyToOne, nameof(MaterialCode), nameof(ProdSerial.MaterialCode))]
    public ProdSerial? Serial { get; set; }
}
