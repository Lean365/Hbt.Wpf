// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Materials
// 文件名称：ProdPacking.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：包装信息实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Materials;

/// <summary>
/// 包装信息实体
/// 记录物料的包装相关信息，与物料通过物料编码关联
/// </summary>
[SugarTable("takt_logistics_prod_packing", "包装信息表")]
[SugarIndex("IX_takt_logistics_prod_packing_material_code", nameof(MaterialCode), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_packing_created_time", nameof(CreatedTime), OrderByType.Desc, false)]
public class ProdPacking : BaseEntity
{
    /// <summary>
    /// 物料编码
    /// 关联到物料表的物料编码
    /// </summary>
    [SugarColumn(ColumnName = "material_code", ColumnDescription = "物料编码", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// 包装类型
    /// 包装物的类型（如：箱、托盘、袋、桶等）
    /// </summary>
    [SugarColumn(ColumnName = "packing_type", ColumnDescription = "包装类型", ColumnDataType = "nvarchar", Length = 50, IsNullable = false, DefaultValue = "VERP")]
    public string PackingType { get; set; } = "VERP";

    /// <summary>
    /// 包装单位
    /// 包装的计量单位（CAR 表示 Carton，即卡通箱；其他如：个、件等）
    /// </summary>
    [SugarColumn(ColumnName = "packing_unit", ColumnDescription = "包装单位", ColumnDataType = "nvarchar", Length = 20, IsNullable = false, DefaultValue = "CAR")]
    public string PackingUnit { get; set; } = "CAR";


    /// <summary>
    /// 毛重
    /// 包含包装物的总重量（单位：千克）
    /// </summary>
    [SugarColumn(ColumnName = "gross_weight", ColumnDescription = "毛重", ColumnDataType = "decimal", Length = 18, DecimalDigits = 10, IsNullable = true, DefaultValue = "0")]
    public decimal? GrossWeight { get; set; }

    /// <summary>
    /// 净重
    /// 不含包装物的净重量（单位：千克）
    /// </summary>
    [SugarColumn(ColumnName = "net_weight", ColumnDescription = "净重", ColumnDataType = "decimal", Length = 18, DecimalDigits = 10, IsNullable = true, DefaultValue = "0")]
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// 重量单位
    /// 重量的计量单位（如：KG、G、T等）
    /// </summary>
    [SugarColumn(ColumnName = "weight_unit", ColumnDescription = "重量单位", ColumnDataType = "nvarchar", Length = 10, IsNullable = false, DefaultValue = "KG")]
    public string WeightUnit { get; set; } = "KG";

    /// <summary>
    /// 业务量（容积）
    /// 一个包装单位的体积（单位：立方米）
    /// </summary>
    [SugarColumn(ColumnName = "business_volume", ColumnDescription = "业务量", ColumnDataType = "decimal", Length = 18, DecimalDigits = 6, IsNullable = true, DefaultValue = "0")]
    public decimal? BusinessVolume { get; set; }

    /// <summary>
    /// 体积单位
    /// 体积的计量单位（如：M3、L、ML等）
    /// </summary>
    [SugarColumn(ColumnName = "volume_unit", ColumnDescription = "体积单位", ColumnDataType = "nvarchar", Length = 10, IsNullable = false, DefaultValue = "M3")]
    public string VolumeUnit { get; set; } = "M3";

    /// <summary>
    /// 大小/量纲
    /// 尺寸量纲或大小规格
    /// </summary>
    [SugarColumn(ColumnName = "size_dimension", ColumnDescription = "大小/量纲", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? SizeDimension { get; set; }


    /// <summary>
    /// 每包装数量
    /// 一个包装包含的基本单位数量
    /// </summary>
    [SugarColumn(ColumnName = "quantity_per_packing", ColumnDescription = "每包装数量", ColumnDataType = "decimal", Length = 18, DecimalDigits = 2, IsNullable = true, DefaultValue = "0")]
    public decimal? QuantityPerPacking { get; set; }


}
