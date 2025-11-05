// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：ProdSerial.cs
// 创建时间：2025-10-22
// 创建人：Hbt365(Cursor AI)
// 功能描述：产品序列号实体（主表）
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// ========================================

using SqlSugar;

namespace Hbt.Domain.Entities.Logistics.Serials;

/// <summary>
/// 产品序列号实体（主表）
/// 包含物料、机种、仕向信息
/// </summary>
[SugarTable("hbt_logistics_prod_serial", "产品序列号主表")]
[SugarIndex("IX_hbt_logistics_prod_serial_material_code", nameof(ProdSerial.MaterialCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_model_code", nameof(ProdSerial.ModelCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_dest_code", nameof(ProdSerial.DestCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_serial_created_time", nameof(ProdSerial.CreatedTime), OrderByType.Desc, false)]
public class ProdSerial : BaseEntity
{

    /// <summary>
    /// 物料编码
    /// 成品物料编码
    /// </summary>
    [SugarColumn(ColumnName = "material_code", ColumnDescription = "物料编码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// 机种编码
    /// 产品机种编码
    /// </summary>
    [SugarColumn(ColumnName = "model_code", ColumnDescription = "机种编码", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string ModelCode { get; set; } = string.Empty;

    /// <summary>
    /// 仕向编码
    /// 产品的仕向编码（目标市场/规格）
    /// </summary>
    [SugarColumn(ColumnName = "dest_code", ColumnDescription = "仕向编码", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? DestCode { get; set; }





    /// <summary>
    /// 入库记录（导航属性）
    /// 通过物料编码关联
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ProdSerialInbound.MaterialCode), nameof(MaterialCode))]
    public List<ProdSerialInbound>? InboundRecords { get; set; }

    /// <summary>
    /// 出库记录（导航属性）
    /// 通过物料编码关联
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ProdSerialOutbound.MaterialCode), nameof(MaterialCode))]
    public List<ProdSerialOutbound>? OutboundRecords { get; set; }
}
