// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Materials
// 文件名称：ProdMaterialDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：生产物料数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Materials;

/// <summary>
/// 生产物料数据传输对象
/// </summary>
public class ProdMaterialDto
{
    // 继承自 BaseEntity
    public long Id { get; set; }
    public string? Remarks { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedTime { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedTime { get; set; }
    public int IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedTime { get; set; }

    // ProdMaterial 特有字段
    public string Plant { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string IndustryField { get; set; } = string.Empty;
    public string MaterialType { get; set; } = string.Empty;
    public string? MaterialDescription { get; set; }
    public string BaseUnit { get; set; } = string.Empty;
    public string? ProductHierarchy { get; set; }
    public string MaterialGroup { get; set; } = string.Empty;
    public string? PurchaseGroup { get; set; }
    public string PurchaseType { get; set; } = string.Empty;
    public string? SpecialPurchaseType { get; set; }
    public string? BulkMaterial { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public int RoundingValue { get; set; }
    public int PlannedDeliveryTime { get; set; }
    public decimal SelfProductionDays { get; set; }
    public string? PostToInspectionStock { get; set; }
    public string ProfitCenter { get; set; } = string.Empty;
    public string? VarianceCode { get; set; }
    public string? BatchManagement { get; set; }
    public string? ManufacturerPartNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string EvaluationType { get; set; } = string.Empty;
    public decimal MovingAveragePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PriceControl { get; set; } = string.Empty;
    public int PriceUnit { get; set; }
    public string? ProductionStorageLocation { get; set; }
    public string? ExternalPurchaseStorageLocation { get; set; }
    public string? StoragePosition { get; set; }
    public string? CrossPlantMaterialStatus { get; set; }
    public decimal StockQuantity { get; set; }
    public string? HsCode { get; set; }
    public string? HsName { get; set; }
    public decimal? MaterialWeight { get; set; }
    public decimal? MaterialVolume { get; set; }
}

/// <summary>
/// 生产物料查询数据传输对象
/// </summary>
public class ProdMaterialQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在物料代码、物料描述中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    public string? MaterialCode { get; set; }
    public string? MaterialDescription { get; set; }
    public string? MaterialType { get; set; }
    public string? Plant { get; set; }
}

/// <summary>
/// 创建生产物料数据传输对象
/// </summary>
public class ProdMaterialCreateDto
{
    public string Plant { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string IndustryField { get; set; } = string.Empty;
    public string MaterialType { get; set; } = string.Empty;
    public string? MaterialDescription { get; set; }
    public string BaseUnit { get; set; } = string.Empty;
    public string? ProductHierarchy { get; set; }
    public string MaterialGroup { get; set; } = string.Empty;
    public string? PurchaseGroup { get; set; }
    public string PurchaseType { get; set; } = string.Empty;
    public string? SpecialPurchaseType { get; set; }
    public string? BulkMaterial { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public int RoundingValue { get; set; }
    public int PlannedDeliveryTime { get; set; }
    public decimal SelfProductionDays { get; set; }
    public string? PostToInspectionStock { get; set; }
    public string ProfitCenter { get; set; } = string.Empty;
    public string? VarianceCode { get; set; }
    public string? BatchManagement { get; set; }
    public string? ManufacturerPartNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string EvaluationType { get; set; } = string.Empty;
    public decimal MovingAveragePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PriceControl { get; set; } = string.Empty;
    public int PriceUnit { get; set; }
    public string? ProductionStorageLocation { get; set; }
    public string? ExternalPurchaseStorageLocation { get; set; }
    public string? StoragePosition { get; set; }
    public string? CrossPlantMaterialStatus { get; set; }
    public string? HsCode { get; set; }
    public string? HsName { get; set; }
    public decimal? MaterialWeight { get; set; }
    public decimal? MaterialVolume { get; set; }
}

/// <summary>
/// 更新生产物料数据传输对象
/// </summary>
public class ProdMaterialUpdateDto : ProdMaterialCreateDto
{
    public long Id { get; set; }
}

