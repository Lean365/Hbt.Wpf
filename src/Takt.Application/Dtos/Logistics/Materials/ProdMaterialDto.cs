// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
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
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialDto()
    {
        Plant = string.Empty;
        MaterialCode = string.Empty;
        IndustryField = string.Empty;
        MaterialType = string.Empty;
        MaterialDescription = string.Empty;
        BaseUnit = string.Empty;
        ProductHierarchy = string.Empty;
        MaterialGroup = string.Empty;
        PurchaseGroup = string.Empty;
        PurchaseType = string.Empty;
        SpecialPurchaseType = string.Empty;
        BulkMaterial = string.Empty;
        PostToInspectionStock = string.Empty;
        ProfitCenter = string.Empty;
        VarianceCode = string.Empty;
        BatchManagement = string.Empty;
        ManufacturerPartNumber = string.Empty;
        Manufacturer = string.Empty;
        EvaluationType = string.Empty;
        Currency = string.Empty;
        PriceControl = string.Empty;
        ProductionStorageLocation = string.Empty;
        ExternalPurchaseStorageLocation = string.Empty;
        StoragePosition = string.Empty;
        CrossPlantMaterialStatus = string.Empty;
        HsCode = string.Empty;
        HsName = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 工厂
    /// </summary>
    public string Plant { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 行业领域
    /// </summary>
    public string IndustryField { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public string MaterialType { get; set; }
    
    /// <summary>
    /// 物料描述
    /// </summary>
    public string MaterialDescription { get; set; }
    
    /// <summary>
    /// 基本单位
    /// </summary>
    public string BaseUnit { get; set; }
    
    /// <summary>
    /// 产品层次
    /// </summary>
    public string ProductHierarchy { get; set; }
    
    /// <summary>
    /// 物料组
    /// </summary>
    public string MaterialGroup { get; set; }
    
    /// <summary>
    /// 采购组
    /// </summary>
    public string PurchaseGroup { get; set; }
    
    /// <summary>
    /// 采购类型
    /// </summary>
    public string PurchaseType { get; set; }
    
    /// <summary>
    /// 特殊采购类型
    /// </summary>
    public string SpecialPurchaseType { get; set; }
    
    /// <summary>
    /// 散装物料
    /// </summary>
    public string BulkMaterial { get; set; }
    
    /// <summary>
    /// 最小订购数量
    /// </summary>
    public int MinimumOrderQuantity { get; set; }
    
    /// <summary>
    /// 舍入值
    /// </summary>
    public int RoundingValue { get; set; }
    
    /// <summary>
    /// 计划交货时间
    /// </summary>
    public int PlannedDeliveryTime { get; set; }
    
    /// <summary>
    /// 自生产天数
    /// </summary>
    public decimal SelfProductionDays { get; set; }
    
    /// <summary>
    /// 过账到检验库存
    /// </summary>
    public string PostToInspectionStock { get; set; }
    
    /// <summary>
    /// 利润中心
    /// </summary>
    public string ProfitCenter { get; set; }
    
    /// <summary>
    /// 差异代码
    /// </summary>
    public string VarianceCode { get; set; }
    
    /// <summary>
    /// 批次管理
    /// </summary>
    public string BatchManagement { get; set; }
    
    /// <summary>
    /// 制造商零件号
    /// </summary>
    public string ManufacturerPartNumber { get; set; }
    
    /// <summary>
    /// 制造商
    /// </summary>
    public string Manufacturer { get; set; }
    
    /// <summary>
    /// 评估类型
    /// </summary>
    public string EvaluationType { get; set; }
    
    /// <summary>
    /// 移动平均价格
    /// </summary>
    public decimal MovingAveragePrice { get; set; }
    
    /// <summary>
    /// 货币
    /// </summary>
    public string Currency { get; set; }
    
    /// <summary>
    /// 价格控制
    /// </summary>
    public string PriceControl { get; set; }
    
    /// <summary>
    /// 价格单位
    /// </summary>
    public int PriceUnit { get; set; }
    
    /// <summary>
    /// 生产存储位置
    /// </summary>
    public string ProductionStorageLocation { get; set; }
    
    /// <summary>
    /// 外部采购存储位置
    /// </summary>
    public string ExternalPurchaseStorageLocation { get; set; }
    
    /// <summary>
    /// 存储位置
    /// </summary>
    public string StoragePosition { get; set; }
    
    /// <summary>
    /// 跨工厂物料状态
    /// </summary>
    public string CrossPlantMaterialStatus { get; set; }
    
    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal StockQuantity { get; set; }
    
    /// <summary>
    /// HS编码
    /// </summary>
    public string HsCode { get; set; }
    
    /// <summary>
    /// HS名称
    /// </summary>
    public string HsName { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 物料体积
    /// </summary>
    public decimal? MaterialVolume { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
    
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
/// 生产物料查询数据传输对象
/// </summary>
public class ProdMaterialQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialQueryDto()
    {
        Keywords = string.Empty;
        MaterialCode = string.Empty;
        MaterialDescription = string.Empty;
        MaterialType = string.Empty;
        Plant = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在物料代码、物料描述中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 物料描述
    /// </summary>
    public string MaterialDescription { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public string MaterialType { get; set; }
    
    /// <summary>
    /// 工厂
    /// </summary>
    public string Plant { get; set; }
}

/// <summary>
/// 创建生产物料数据传输对象
/// </summary>
public class ProdMaterialCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialCreateDto()
    {
        Plant = string.Empty;
        MaterialCode = string.Empty;
        IndustryField = string.Empty;
        MaterialType = string.Empty;
        MaterialDescription = string.Empty;
        BaseUnit = string.Empty;
        ProductHierarchy = string.Empty;
        MaterialGroup = string.Empty;
        PurchaseGroup = string.Empty;
        PurchaseType = string.Empty;
        SpecialPurchaseType = string.Empty;
        BulkMaterial = string.Empty;
        PostToInspectionStock = string.Empty;
        ProfitCenter = string.Empty;
        VarianceCode = string.Empty;
        BatchManagement = string.Empty;
        ManufacturerPartNumber = string.Empty;
        Manufacturer = string.Empty;
        EvaluationType = string.Empty;
        Currency = string.Empty;
        PriceControl = string.Empty;
        ProductionStorageLocation = string.Empty;
        ExternalPurchaseStorageLocation = string.Empty;
        StoragePosition = string.Empty;
        CrossPlantMaterialStatus = string.Empty;
        HsCode = string.Empty;
        HsName = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 工厂
    /// </summary>
    public string Plant { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 行业领域
    /// </summary>
    public string IndustryField { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public string MaterialType { get; set; }
    
    /// <summary>
    /// 物料描述
    /// </summary>
    public string MaterialDescription { get; set; }
    
    /// <summary>
    /// 基本单位
    /// </summary>
    public string BaseUnit { get; set; }
    
    /// <summary>
    /// 产品层次
    /// </summary>
    public string ProductHierarchy { get; set; }
    
    /// <summary>
    /// 物料组
    /// </summary>
    public string MaterialGroup { get; set; }
    
    /// <summary>
    /// 采购组
    /// </summary>
    public string PurchaseGroup { get; set; }
    
    /// <summary>
    /// 采购类型
    /// </summary>
    public string PurchaseType { get; set; }
    
    /// <summary>
    /// 特殊采购类型
    /// </summary>
    public string SpecialPurchaseType { get; set; }
    
    /// <summary>
    /// 散装物料
    /// </summary>
    public string BulkMaterial { get; set; }
    
    /// <summary>
    /// 最小订购数量
    /// </summary>
    public int MinimumOrderQuantity { get; set; }
    
    /// <summary>
    /// 舍入值
    /// </summary>
    public int RoundingValue { get; set; }
    
    /// <summary>
    /// 计划交货时间
    /// </summary>
    public int PlannedDeliveryTime { get; set; }
    
    /// <summary>
    /// 自生产天数
    /// </summary>
    public decimal SelfProductionDays { get; set; }
    
    /// <summary>
    /// 过账到检验库存
    /// </summary>
    public string PostToInspectionStock { get; set; }
    
    /// <summary>
    /// 利润中心
    /// </summary>
    public string ProfitCenter { get; set; }
    
    /// <summary>
    /// 差异代码
    /// </summary>
    public string VarianceCode { get; set; }
    
    /// <summary>
    /// 批次管理
    /// </summary>
    public string BatchManagement { get; set; }
    
    /// <summary>
    /// 制造商零件号
    /// </summary>
    public string ManufacturerPartNumber { get; set; }
    
    /// <summary>
    /// 制造商
    /// </summary>
    public string Manufacturer { get; set; }
    
    /// <summary>
    /// 评估类型
    /// </summary>
    public string EvaluationType { get; set; }
    
    /// <summary>
    /// 移动平均价格
    /// </summary>
    public decimal MovingAveragePrice { get; set; }
    
    /// <summary>
    /// 货币
    /// </summary>
    public string Currency { get; set; }
    
    /// <summary>
    /// 价格控制
    /// </summary>
    public string PriceControl { get; set; }
    
    /// <summary>
    /// 价格单位
    /// </summary>
    public int PriceUnit { get; set; }
    
    /// <summary>
    /// 生产存储位置
    /// </summary>
    public string ProductionStorageLocation { get; set; }
    
    /// <summary>
    /// 外部采购存储位置
    /// </summary>
    public string ExternalPurchaseStorageLocation { get; set; }
    
    /// <summary>
    /// 存储位置
    /// </summary>
    public string StoragePosition { get; set; }
    
    /// <summary>
    /// 跨工厂物料状态
    /// </summary>
    public string CrossPlantMaterialStatus { get; set; }
    
    /// <summary>
    /// HS编码
    /// </summary>
    public string HsCode { get; set; }
    
    /// <summary>
    /// HS名称
    /// </summary>
    public string HsName { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 物料体积
    /// </summary>
    public decimal? MaterialVolume { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新生产物料数据传输对象
/// </summary>
public class ProdMaterialUpdateDto : ProdMaterialCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 生产物料统计数据传输对象
/// </summary>
public class ProdMaterialStatisticDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialStatisticDto()
    {
        Plant = string.Empty;
        DimensionValue = string.Empty;
    }

    /// <summary>
    /// 工厂
    /// </summary>
    public string Plant { get; set; }

    /// <summary>
    /// 统计维度值（如：MaterialType值、PurchaseGroup值等）
    /// </summary>
    public string DimensionValue { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// 生产物料统计查询参数
/// </summary>
public class ProdMaterialStatisticQueryDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialStatisticQueryDto()
    {
        Plant = string.Empty;
        Dimension = "MaterialType";
    }

    /// <summary>
    /// 工厂（为空则统计所有工厂）
    /// </summary>
    public string Plant { get; set; }

    /// <summary>
    /// 统计维度：MaterialType（物料类型）、PurchaseGroup（采购组）、PurchaseType（采购类型）、PostToInspectionStock（过账到检验库存）、ExternalPurchaseStorageLocation（外部采购存储位置）、All（同时统计所有维度）
    /// </summary>
    public string Dimension { get; set; }
}

/// <summary>
/// 生产物料统计结果（包含所有维度的统计清单）
/// </summary>
public class ProdMaterialStatisticResultDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialStatisticResultDto()
    {
        TotalCount = 0;
        MaterialTypeStatistics = new List<ProdMaterialStatisticDto>();
        PurchaseGroupStatistics = new List<ProdMaterialStatisticDto>();
        PurchaseTypeStatistics = new List<ProdMaterialStatisticDto>();
        PostToInspectionStockStatistics = new List<ProdMaterialStatisticDto>();
        ExternalPurchaseStorageLocationStatistics = new List<ProdMaterialStatisticDto>();
    }

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 按物料类型统计清单
    /// </summary>
    public List<ProdMaterialStatisticDto> MaterialTypeStatistics { get; set; }

    /// <summary>
    /// 按采购组统计清单
    /// </summary>
    public List<ProdMaterialStatisticDto> PurchaseGroupStatistics { get; set; }

    /// <summary>
    /// 按采购类型统计清单
    /// </summary>
    public List<ProdMaterialStatisticDto> PurchaseTypeStatistics { get; set; }

    /// <summary>
    /// 按过账到检验库存统计清单
    /// </summary>
    public List<ProdMaterialStatisticDto> PostToInspectionStockStatistics { get; set; }

    /// <summary>
    /// 按外部采购存储位置统计清单
    /// </summary>
    public List<ProdMaterialStatisticDto> ExternalPurchaseStorageLocationStatistics { get; set; }
}

/// <summary>
/// 生产物料导出数据传输对象
/// </summary>
public class ProdMaterialExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialExportDto()
    {
        Plant = string.Empty;
        MaterialCode = string.Empty;
        IndustryField = string.Empty;
        MaterialType = string.Empty;
        MaterialDescription = string.Empty;
        BaseUnit = string.Empty;
        ProductHierarchy = string.Empty;
        MaterialGroup = string.Empty;
        PurchaseGroup = string.Empty;
        PurchaseType = string.Empty;
        SpecialPurchaseType = string.Empty;
        BulkMaterial = string.Empty;
        PostToInspectionStock = string.Empty;
        ProfitCenter = string.Empty;
        VarianceCode = string.Empty;
        BatchManagement = string.Empty;
        ManufacturerPartNumber = string.Empty;
        Manufacturer = string.Empty;
        EvaluationType = string.Empty;
        Currency = string.Empty;
        PriceControl = string.Empty;
        ProductionStorageLocation = string.Empty;
        ExternalPurchaseStorageLocation = string.Empty;
        StoragePosition = string.Empty;
        CrossPlantMaterialStatus = string.Empty;
        HsCode = string.Empty;
        HsName = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 工厂
    /// </summary>
    public string Plant { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 行业领域
    /// </summary>
    public string IndustryField { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public string MaterialType { get; set; }
    
    /// <summary>
    /// 物料描述
    /// </summary>
    public string MaterialDescription { get; set; }
    
    /// <summary>
    /// 基本单位
    /// </summary>
    public string BaseUnit { get; set; }
    
    /// <summary>
    /// 产品层次
    /// </summary>
    public string ProductHierarchy { get; set; }
    
    /// <summary>
    /// 物料组
    /// </summary>
    public string MaterialGroup { get; set; }
    
    /// <summary>
    /// 采购组
    /// </summary>
    public string PurchaseGroup { get; set; }
    
    /// <summary>
    /// 采购类型
    /// </summary>
    public string PurchaseType { get; set; }
    
    /// <summary>
    /// 特殊采购类型
    /// </summary>
    public string SpecialPurchaseType { get; set; }
    
    /// <summary>
    /// 散装物料
    /// </summary>
    public string BulkMaterial { get; set; }
    
    /// <summary>
    /// 最小订购数量
    /// </summary>
    public int MinimumOrderQuantity { get; set; }
    
    /// <summary>
    /// 舍入值
    /// </summary>
    public int RoundingValue { get; set; }
    
    /// <summary>
    /// 计划交货时间
    /// </summary>
    public int PlannedDeliveryTime { get; set; }
    
    /// <summary>
    /// 自生产天数
    /// </summary>
    public decimal SelfProductionDays { get; set; }
    
    /// <summary>
    /// 过账到检验库存
    /// </summary>
    public string PostToInspectionStock { get; set; }
    
    /// <summary>
    /// 利润中心
    /// </summary>
    public string ProfitCenter { get; set; }
    
    /// <summary>
    /// 差异代码
    /// </summary>
    public string VarianceCode { get; set; }
    
    /// <summary>
    /// 批次管理
    /// </summary>
    public string BatchManagement { get; set; }
    
    /// <summary>
    /// 制造商零件号
    /// </summary>
    public string ManufacturerPartNumber { get; set; }
    
    /// <summary>
    /// 制造商
    /// </summary>
    public string Manufacturer { get; set; }
    
    /// <summary>
    /// 评估类型
    /// </summary>
    public string EvaluationType { get; set; }
    
    /// <summary>
    /// 移动平均价格
    /// </summary>
    public decimal MovingAveragePrice { get; set; }
    
    /// <summary>
    /// 货币
    /// </summary>
    public string Currency { get; set; }
    
    /// <summary>
    /// 价格控制
    /// </summary>
    public string PriceControl { get; set; }
    
    /// <summary>
    /// 价格单位
    /// </summary>
    public int PriceUnit { get; set; }
    
    /// <summary>
    /// 生产存储位置
    /// </summary>
    public string ProductionStorageLocation { get; set; }
    
    /// <summary>
    /// 外部采购存储位置
    /// </summary>
    public string ExternalPurchaseStorageLocation { get; set; }
    
    /// <summary>
    /// 存储位置
    /// </summary>
    public string StoragePosition { get; set; }
    
    /// <summary>
    /// 跨工厂物料状态
    /// </summary>
    public string CrossPlantMaterialStatus { get; set; }
    
    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal StockQuantity { get; set; }
    
    /// <summary>
    /// HS编码
    /// </summary>
    public string HsCode { get; set; }
    
    /// <summary>
    /// HS名称
    /// </summary>
    public string HsName { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 物料体积
    /// </summary>
    public decimal? MaterialVolume { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 生产物料导入模板数据传输对象
/// </summary>
public class ProdMaterialTemplateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialTemplateDto()
    {
        Plant = string.Empty;
        MaterialCode = string.Empty;
        IndustryField = string.Empty;
        MaterialType = string.Empty;
        MaterialDescription = string.Empty;
        BaseUnit = string.Empty;
        ProductHierarchy = string.Empty;
        MaterialGroup = string.Empty;
        PurchaseGroup = string.Empty;
        PurchaseType = string.Empty;
        SpecialPurchaseType = string.Empty;
        BulkMaterial = string.Empty;
        PostToInspectionStock = string.Empty;
        ProfitCenter = string.Empty;
        VarianceCode = string.Empty;
        BatchManagement = string.Empty;
        ManufacturerPartNumber = string.Empty;
        Manufacturer = string.Empty;
        EvaluationType = string.Empty;
        Currency = string.Empty;
        PriceControl = string.Empty;
        ProductionStorageLocation = string.Empty;
        ExternalPurchaseStorageLocation = string.Empty;
        StoragePosition = string.Empty;
        CrossPlantMaterialStatus = string.Empty;
        HsCode = string.Empty;
        HsName = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 工厂
    /// </summary>
    public string Plant { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 行业领域
    /// </summary>
    public string IndustryField { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public string MaterialType { get; set; }
    
    /// <summary>
    /// 物料描述
    /// </summary>
    public string MaterialDescription { get; set; }
    
    /// <summary>
    /// 基本单位
    /// </summary>
    public string BaseUnit { get; set; }
    
    /// <summary>
    /// 产品层次
    /// </summary>
    public string ProductHierarchy { get; set; }
    
    /// <summary>
    /// 物料组
    /// </summary>
    public string MaterialGroup { get; set; }
    
    /// <summary>
    /// 采购组
    /// </summary>
    public string PurchaseGroup { get; set; }
    
    /// <summary>
    /// 采购类型
    /// </summary>
    public string PurchaseType { get; set; }
    
    /// <summary>
    /// 特殊采购类型
    /// </summary>
    public string SpecialPurchaseType { get; set; }
    
    /// <summary>
    /// 散装物料
    /// </summary>
    public string BulkMaterial { get; set; }
    
    /// <summary>
    /// 最小订购数量
    /// </summary>
    public int MinimumOrderQuantity { get; set; }
    
    /// <summary>
    /// 舍入值
    /// </summary>
    public int RoundingValue { get; set; }
    
    /// <summary>
    /// 计划交货时间
    /// </summary>
    public int PlannedDeliveryTime { get; set; }
    
    /// <summary>
    /// 自生产天数
    /// </summary>
    public decimal SelfProductionDays { get; set; }
    
    /// <summary>
    /// 过账到检验库存
    /// </summary>
    public string PostToInspectionStock { get; set; }
    
    /// <summary>
    /// 利润中心
    /// </summary>
    public string ProfitCenter { get; set; }
    
    /// <summary>
    /// 差异代码
    /// </summary>
    public string VarianceCode { get; set; }
    
    /// <summary>
    /// 批次管理
    /// </summary>
    public string BatchManagement { get; set; }
    
    /// <summary>
    /// 制造商零件号
    /// </summary>
    public string ManufacturerPartNumber { get; set; }
    
    /// <summary>
    /// 制造商
    /// </summary>
    public string Manufacturer { get; set; }
    
    /// <summary>
    /// 评估类型
    /// </summary>
    public string EvaluationType { get; set; }
    
    /// <summary>
    /// 移动平均价格
    /// </summary>
    public decimal MovingAveragePrice { get; set; }
    
    /// <summary>
    /// 货币
    /// </summary>
    public string Currency { get; set; }
    
    /// <summary>
    /// 价格控制
    /// </summary>
    public string PriceControl { get; set; }
    
    /// <summary>
    /// 价格单位
    /// </summary>
    public int PriceUnit { get; set; }
    
    /// <summary>
    /// 生产存储位置
    /// </summary>
    public string ProductionStorageLocation { get; set; }
    
    /// <summary>
    /// 外部采购存储位置
    /// </summary>
    public string ExternalPurchaseStorageLocation { get; set; }
    
    /// <summary>
    /// 存储位置
    /// </summary>
    public string StoragePosition { get; set; }
    
    /// <summary>
    /// 跨工厂物料状态
    /// </summary>
    public string CrossPlantMaterialStatus { get; set; }
    
    /// <summary>
    /// HS编码
    /// </summary>
    public string HsCode { get; set; }
    
    /// <summary>
    /// HS名称
    /// </summary>
    public string HsName { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 物料体积
    /// </summary>
    public decimal? MaterialVolume { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 生产物料导入数据传输对象
/// </summary>
public class ProdMaterialImportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdMaterialImportDto()
    {
        Plant = string.Empty;
        MaterialCode = string.Empty;
        IndustryField = string.Empty;
        MaterialType = string.Empty;
        MaterialDescription = string.Empty;
        BaseUnit = string.Empty;
        ProductHierarchy = string.Empty;
        MaterialGroup = string.Empty;
        PurchaseGroup = string.Empty;
        PurchaseType = string.Empty;
        SpecialPurchaseType = string.Empty;
        BulkMaterial = string.Empty;
        PostToInspectionStock = string.Empty;
        ProfitCenter = string.Empty;
        VarianceCode = string.Empty;
        BatchManagement = string.Empty;
        ManufacturerPartNumber = string.Empty;
        Manufacturer = string.Empty;
        EvaluationType = string.Empty;
        Currency = string.Empty;
        PriceControl = string.Empty;
        ProductionStorageLocation = string.Empty;
        ExternalPurchaseStorageLocation = string.Empty;
        StoragePosition = string.Empty;
        CrossPlantMaterialStatus = string.Empty;
        HsCode = string.Empty;
        HsName = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 工厂
    /// </summary>
    public string Plant { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; }
    
    /// <summary>
    /// 行业领域
    /// </summary>
    public string IndustryField { get; set; }
    
    /// <summary>
    /// 物料类型
    /// </summary>
    public string MaterialType { get; set; }
    
    /// <summary>
    /// 物料描述
    /// </summary>
    public string MaterialDescription { get; set; }
    
    /// <summary>
    /// 基本单位
    /// </summary>
    public string BaseUnit { get; set; }
    
    /// <summary>
    /// 产品层次
    /// </summary>
    public string ProductHierarchy { get; set; }
    
    /// <summary>
    /// 物料组
    /// </summary>
    public string MaterialGroup { get; set; }
    
    /// <summary>
    /// 采购组
    /// </summary>
    public string PurchaseGroup { get; set; }
    
    /// <summary>
    /// 采购类型
    /// </summary>
    public string PurchaseType { get; set; }
    
    /// <summary>
    /// 特殊采购类型
    /// </summary>
    public string SpecialPurchaseType { get; set; }
    
    /// <summary>
    /// 散装物料
    /// </summary>
    public string BulkMaterial { get; set; }
    
    /// <summary>
    /// 最小订购数量
    /// </summary>
    public int MinimumOrderQuantity { get; set; }
    
    /// <summary>
    /// 舍入值
    /// </summary>
    public int RoundingValue { get; set; }
    
    /// <summary>
    /// 计划交货时间
    /// </summary>
    public int PlannedDeliveryTime { get; set; }
    
    /// <summary>
    /// 自生产天数
    /// </summary>
    public decimal SelfProductionDays { get; set; }
    
    /// <summary>
    /// 过账到检验库存
    /// </summary>
    public string PostToInspectionStock { get; set; }
    
    /// <summary>
    /// 利润中心
    /// </summary>
    public string ProfitCenter { get; set; }
    
    /// <summary>
    /// 差异代码
    /// </summary>
    public string VarianceCode { get; set; }
    
    /// <summary>
    /// 批次管理
    /// </summary>
    public string BatchManagement { get; set; }
    
    /// <summary>
    /// 制造商零件号
    /// </summary>
    public string ManufacturerPartNumber { get; set; }
    
    /// <summary>
    /// 制造商
    /// </summary>
    public string Manufacturer { get; set; }
    
    /// <summary>
    /// 评估类型
    /// </summary>
    public string EvaluationType { get; set; }
    
    /// <summary>
    /// 移动平均价格
    /// </summary>
    public decimal MovingAveragePrice { get; set; }
    
    /// <summary>
    /// 货币
    /// </summary>
    public string Currency { get; set; }
    
    /// <summary>
    /// 价格控制
    /// </summary>
    public string PriceControl { get; set; }
    
    /// <summary>
    /// 价格单位
    /// </summary>
    public int PriceUnit { get; set; }
    
    /// <summary>
    /// 生产存储位置
    /// </summary>
    public string ProductionStorageLocation { get; set; }
    
    /// <summary>
    /// 外部采购存储位置
    /// </summary>
    public string ExternalPurchaseStorageLocation { get; set; }
    
    /// <summary>
    /// 存储位置
    /// </summary>
    public string StoragePosition { get; set; }
    
    /// <summary>
    /// 跨工厂物料状态
    /// </summary>
    public string CrossPlantMaterialStatus { get; set; }
    
    /// <summary>
    /// HS编码
    /// </summary>
    public string HsCode { get; set; }
    
    /// <summary>
    /// HS名称
    /// </summary>
    public string HsName { get; set; }
    
    /// <summary>
    /// 物料重量
    /// </summary>
    public decimal? MaterialWeight { get; set; }
    
    /// <summary>
    /// 物料体积
    /// </summary>
    public decimal? MaterialVolume { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

