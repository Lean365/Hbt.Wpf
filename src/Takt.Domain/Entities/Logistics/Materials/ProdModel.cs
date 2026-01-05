// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Materials
// 文件名称：ProdModel.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：产品型号实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Materials;

/// <summary>
/// 产品机种实体
/// 用于管理产品的机种信息
/// </summary>
[SugarTable("takt_logistics_prod_model", "产品机种表")]
[SugarIndex("IX_takt_logistics_prod_model_material_code", nameof(ProdModel.MaterialCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_model_model_code", nameof(ProdModel.ModelCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_model_dest_code", nameof(ProdModel.DestCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_model_created_time", nameof(ProdModel.CreatedTime), OrderByType.Desc, false)]
public class ProdModel : BaseEntity
{
    /// <summary>
    /// 物料编码
    /// </summary>
    [SugarColumn(ColumnName = "material_code", ColumnDescription = "物料编码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// 机种编码
    /// </summary>
    [SugarColumn(ColumnName = "model_code", ColumnDescription = "机种编码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string ModelCode { get; set; } = string.Empty;

    /// <summary>
    /// 仕向编码
    /// </summary>
    [SugarColumn(ColumnName = "dest_code", ColumnDescription = "仕向编码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string DestCode { get; set; } = string.Empty;

    /// <summary>
    /// 入库记录（导航属性）
    /// 通过物料编码关联
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(Takt.Domain.Entities.Logistics.Serials.ProdSerialInbound.MaterialCode), nameof(MaterialCode))]
    public List<Takt.Domain.Entities.Logistics.Serials.ProdSerialInbound>? InboundRecords { get; set; }

    /// <summary>
    /// 出库记录（导航属性）
    /// 通过物料编码关联
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(Takt.Domain.Entities.Logistics.Serials.ProdSerialOutbound.MaterialCode), nameof(MaterialCode))]
    public List<Takt.Domain.Entities.Logistics.Serials.ProdSerialOutbound>? OutboundRecords { get; set; }
}