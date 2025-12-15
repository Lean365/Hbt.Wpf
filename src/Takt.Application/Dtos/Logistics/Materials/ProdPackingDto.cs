// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Materials
// 文件名称：ProdPackingDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：包装信息数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Materials;

/// <summary>
/// 包装信息数据传输对象
/// </summary>
public class ProdPackingDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdPackingDto()
    {
        MaterialCode = string.Empty;
        PackingType = "VERP";
        PackingUnit = "CAR";
        WeightUnit = "KG";
        VolumeUnit = "M3";
        SizeDimension = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 包装类型
    /// </summary>
    public string PackingType { get; set; }

    /// <summary>
    /// 包装单位
    /// </summary>
    public string PackingUnit { get; set; }

    /// <summary>
    /// 毛重
    /// </summary>
    public decimal? GrossWeight { get; set; }

    /// <summary>
    /// 净重
    /// </summary>
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// 重量单位
    /// </summary>
    public string WeightUnit { get; set; }

    /// <summary>
    /// 业务量（容积）
    /// 一个包装单位的体积
    /// </summary>
    public decimal? BusinessVolume { get; set; }

    /// <summary>
    /// 体积单位
    /// </summary>
    public string VolumeUnit { get; set; }

    /// <summary>
    /// 大小/量纲
    /// </summary>
    public string SizeDimension { get; set; }

    /// <summary>
    /// 每包装数量
    /// </summary>
    public decimal? QuantityPerPacking { get; set; }

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
}

/// <summary>
/// 包装信息查询数据传输对象
/// </summary>
public class ProdPackingQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdPackingQueryDto()
    {
        Keywords = string.Empty;
        MaterialCode = string.Empty;
        PackingType = string.Empty;
        PackingUnit = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在物料编码中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 包装类型
    /// </summary>
    public string PackingType { get; set; }

    /// <summary>
    /// 包装单位
    /// </summary>
    public string PackingUnit { get; set; }
}

/// <summary>
/// 创建包装信息数据传输对象
/// </summary>
public class ProdPackingCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdPackingCreateDto()
    {
        MaterialCode = string.Empty;
        PackingType = "VERP";
        PackingUnit = "CAR";
        WeightUnit = "KG";
        VolumeUnit = "M3";
        SizeDimension = string.Empty;
    }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 包装类型
    /// </summary>
    public string PackingType { get; set; }

    /// <summary>
    /// 包装单位
    /// </summary>
    public string PackingUnit { get; set; }

    /// <summary>
    /// 毛重
    /// </summary>
    public decimal? GrossWeight { get; set; }

    /// <summary>
    /// 净重
    /// </summary>
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// 重量单位
    /// </summary>
    public string WeightUnit { get; set; }

    /// <summary>
    /// 业务量（容积）
    /// 一个包装单位的体积
    /// </summary>
    public decimal? BusinessVolume { get; set; }

    /// <summary>
    /// 体积单位
    /// </summary>
    public string VolumeUnit { get; set; }

    /// <summary>
    /// 大小/量纲
    /// </summary>
    public string SizeDimension { get; set; }

    /// <summary>
    /// 每包装数量
    /// </summary>
    public decimal? QuantityPerPacking { get; set; }
}

/// <summary>
/// 更新包装信息数据传输对象
/// </summary>
public class ProdPackingUpdateDto : ProdPackingCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 包装信息导出数据传输对象
/// </summary>
public class ProdPackingExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public ProdPackingExportDto()
    {
        MaterialCode = string.Empty;
        PackingType = "VERP";
        PackingUnit = "CAR";
        WeightUnit = "KG";
        VolumeUnit = "M3";
        SizeDimension = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string MaterialCode { get; set; }

    /// <summary>
    /// 包装类型
    /// </summary>
    public string PackingType { get; set; }

    /// <summary>
    /// 包装单位
    /// </summary>
    public string PackingUnit { get; set; }

    /// <summary>
    /// 毛重
    /// </summary>
    public decimal? GrossWeight { get; set; }

    /// <summary>
    /// 净重
    /// </summary>
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// 重量单位
    /// </summary>
    public string WeightUnit { get; set; }

    /// <summary>
    /// 业务量（容积）
    /// 一个包装单位的体积
    /// </summary>
    public decimal? BusinessVolume { get; set; }

    /// <summary>
    /// 体积单位
    /// </summary>
    public string VolumeUnit { get; set; }

    /// <summary>
    /// 大小/量纲
    /// </summary>
    public string SizeDimension { get; set; }

    /// <summary>
    /// 每包装数量
    /// </summary>
    public decimal? QuantityPerPacking { get; set; }

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
}
