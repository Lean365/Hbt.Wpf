// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Materials
// 文件名称：ProdMaterial.cs
// 创建时间：2025-10-22
// 创建人：Takt365(Cursor AI)
// 功能描述：生产物料实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Materials;

/// <summary>
/// 生产物料实体
/// 基于SAP物料主数据标准的生产物料信息
/// </summary>
[SugarTable("takt_logistics_prod_material", "生产物料表")]
[SugarIndex("IX_takt_logistics_prod_material_material_code", nameof(ProdMaterial.MaterialCode), OrderByType.Asc, true)]
[SugarIndex("IX_takt_logistics_prod_material_material_type", nameof(ProdMaterial.MaterialType), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_material_material_group", nameof(ProdMaterial.MaterialGroup), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_material_plant", nameof(ProdMaterial.Plant), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_prod_material_created_time", nameof(ProdMaterial.CreatedTime), OrderByType.Desc, false)]
public class ProdMaterial : BaseEntity
{
    /// <summary>
    /// 工厂
    /// </summary>
    [SugarColumn(ColumnName = "plant", ColumnDescription = "工厂", ColumnDataType = "nvarchar", Length = 10, IsNullable = false)]
    public string Plant { get; set; } = string.Empty;

    /// <summary>
    /// 物料编码
    /// </summary>
    [SugarColumn(ColumnName = "material_code", ColumnDescription = "物料编码", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// 行业领域
    /// </summary>
    [SugarColumn(ColumnName = "industry_field", ColumnDescription = "行业领域", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string IndustryField { get; set; } = string.Empty;

    /// <summary>
    /// 物料类型
    /// </summary>
    [SugarColumn(ColumnName = "material_type", ColumnDescription = "物料类型", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string MaterialType { get; set; } = string.Empty;

    /// <summary>
    /// 物料描述
    /// </summary>
    [SugarColumn(ColumnName = "material_description", ColumnDescription = "物料描述", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? MaterialDescription { get; set; }

    /// <summary>
    /// 基本计量单位
    /// </summary>
    [SugarColumn(ColumnName = "base_unit", ColumnDescription = "基本计量单位", ColumnDataType = "nvarchar", Length = 10, IsNullable = false)]
    public string BaseUnit { get; set; } = string.Empty;

    /// <summary>
    /// 产品层次
    /// </summary>
    [SugarColumn(ColumnName = "product_hierarchy", ColumnDescription = "产品层次", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? ProductHierarchy { get; set; }

    /// <summary>
    /// 物料组
    /// </summary>
    [SugarColumn(ColumnName = "material_group", ColumnDescription = "物料组", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string MaterialGroup { get; set; } = string.Empty;

    /// <summary>
    /// 采购组
    /// </summary>
    [SugarColumn(ColumnName = "purchase_group", ColumnDescription = "采购组", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? PurchaseGroup { get; set; }

    /// <summary>
    /// 采购类型
    /// </summary>
    [SugarColumn(ColumnName = "purchase_type", ColumnDescription = "采购类型", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string PurchaseType { get; set; } = string.Empty;

    /// <summary>
    /// 特殊采购类
    /// </summary>
    [SugarColumn(ColumnName = "special_purchase_type", ColumnDescription = "特殊采购类", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? SpecialPurchaseType { get; set; }

    /// <summary>
    /// 散装物料
    /// </summary>
    [SugarColumn(ColumnName = "bulk_material", ColumnDescription = "散装物料", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? BulkMaterial { get; set; }

    /// <summary>
    /// 最小起订量
    /// </summary>
    [SugarColumn(ColumnName = "minimum_order_quantity", ColumnDescription = "最小起订量", IsNullable = false, DefaultValue = "0")]
    public int MinimumOrderQuantity { get; set; } = 0;

    /// <summary>
    /// 舍入值
    /// </summary>
    [SugarColumn(ColumnName = "rounding_value", ColumnDescription = "舍入值", IsNullable = false, DefaultValue = "0")]
    public int RoundingValue { get; set; } = 0;

    /// <summary>
    /// 计划交货时间
    /// </summary>
    [SugarColumn(ColumnName = "planned_delivery_time", ColumnDescription = "计划交货时间", IsNullable = false, DefaultValue = "0")]
    public int PlannedDeliveryTime { get; set; } = 0;

    /// <summary>
    /// 自制生产天数
    /// </summary>
    [SugarColumn(ColumnName = "self_production_days", ColumnDescription = "自制生产天数", ColumnDataType = "decimal", Length = 8, DecimalDigits = 2, IsNullable = false, DefaultValue = "0")]
    public decimal SelfProductionDays { get; set; } = 0;

    /// <summary>
    /// 过账到检验库存
    /// </summary>
    [SugarColumn(ColumnName = "post_to_inspection_stock", ColumnDescription = "过账到检验库存", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? PostToInspectionStock { get; set; }

    /// <summary>
    /// 利润中心
    /// </summary>
    [SugarColumn(ColumnName = "profit_center", ColumnDescription = "利润中心", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string ProfitCenter { get; set; } = string.Empty;

    /// <summary>
    /// 差异码
    /// </summary>
    [SugarColumn(ColumnName = "variance_code", ColumnDescription = "差异码", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? VarianceCode { get; set; }

    /// <summary>
    /// 批次管理
    /// </summary>
    [SugarColumn(ColumnName = "batch_management", ColumnDescription = "批次管理", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? BatchManagement { get; set; }

    /// <summary>
    /// 制造商零件编号
    /// </summary>
    [SugarColumn(ColumnName = "manufacturer_part_number", ColumnDescription = "制造商零件编号", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? ManufacturerPartNumber { get; set; }

    /// <summary>
    /// 制造商
    /// </summary>
    [SugarColumn(ColumnName = "manufacturer", ColumnDescription = "制造商", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? Manufacturer { get; set; }

    /// <summary>
    /// 评估类
    /// </summary>
    [SugarColumn(ColumnName = "evaluation_type", ColumnDescription = "评估类", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string EvaluationType { get; set; } = string.Empty;

    /// <summary>
    /// 移动平均价
    /// </summary>
    [SugarColumn(ColumnName = "moving_average_price", ColumnDescription = "移动平均价", ColumnDataType = "decimal", Length = 13, DecimalDigits = 2, IsNullable = false, DefaultValue = "0")]
    public decimal MovingAveragePrice { get; set; } = 0;

    /// <summary>
    /// 货币
    /// </summary>
    [SugarColumn(ColumnName = "currency", ColumnDescription = "货币", ColumnDataType = "nvarchar", Length = 10, IsNullable = false)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// 价格控制
    /// </summary>
    [SugarColumn(ColumnName = "price_control", ColumnDescription = "价格控制", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string PriceControl { get; set; } = string.Empty;

    /// <summary>
    /// 价格单位
    /// </summary>
    [SugarColumn(ColumnName = "price_unit", ColumnDescription = "价格单位", IsNullable = false, DefaultValue = "1000")]
    public int PriceUnit { get; set; } = 1000;

    /// <summary>
    /// 生产仓储地点
    /// </summary>
    [SugarColumn(ColumnName = "production_storage_location", ColumnDescription = "生产仓储地点", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? ProductionStorageLocation { get; set; }

    /// <summary>
    /// 外部采购仓储地点
    /// </summary>
    [SugarColumn(ColumnName = "external_purchase_storage_location", ColumnDescription = "外部采购仓储地点", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? ExternalPurchaseStorageLocation { get; set; }

    /// <summary>
    /// 仓位
    /// </summary>
    [SugarColumn(ColumnName = "storage_position", ColumnDescription = "仓位", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? StoragePosition { get; set; }

    /// <summary>
    /// 跨工厂物料状态
    /// </summary>
    [SugarColumn(ColumnName = "cross_plant_material_status", ColumnDescription = "跨工厂物料状态", ColumnDataType = "nvarchar", Length = 20, IsNullable = true)]
    public string? CrossPlantMaterialStatus { get; set; }

    /// <summary>
    /// 在库数量
    /// </summary>
    [SugarColumn(ColumnName = "stock_quantity", ColumnDescription = "在库数量", ColumnDataType = "decimal", Length = 13, DecimalDigits = 2, IsNullable = false, DefaultValue = "0")]
    public decimal StockQuantity { get; set; } = 0;

    /// <summary>
    /// HS编码
    /// 国际通用的商品编码（6位数字）
    /// </summary>
    [SugarColumn(ColumnName = "hs_code", ColumnDescription = "HS编码", ColumnDataType = "nvarchar", Length = 6, IsNullable = true)]
    public string? HsCode { get; set; }

    /// <summary>
    /// HS名称
    /// 国际通用的商品名称
    /// </summary>
    [SugarColumn(ColumnName = "hs_name", ColumnDescription = "HS名称", ColumnDataType = "nvarchar", Length = 200, IsNullable = true)]
    public string? HsName { get; set; }

    /// <summary>
    /// 重量
    /// </summary>
    [SugarColumn(ColumnName = "material_weight", ColumnDescription = "重量", ColumnDataType = "decimal", Length = 18, DecimalDigits = 10, IsNullable = true, DefaultValue = "0")]
    public decimal? MaterialWeight { get; set; } = 0;

    /// <summary>
    /// 容积
    /// </summary>
    [SugarColumn(ColumnName = "material_volume", ColumnDescription = "容积", ColumnDataType = "decimal", Length = 13, DecimalDigits = 6, IsNullable = true, DefaultValue = "0")]
    public decimal? MaterialVolume { get; set; } = 0;
}