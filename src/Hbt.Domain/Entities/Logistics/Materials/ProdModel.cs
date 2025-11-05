// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：ProdModel.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：产品型号实体
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

using SqlSugar;

namespace Hbt.Domain.Entities.Logistics.Materials;

/// <summary>
/// 产品机种实体
/// 用于管理产品的机种信息
/// </summary>
[SugarTable("hbt_logistics_prod_model", "产品机种表")]
[SugarIndex("IX_hbt_logistics_prod_model_material_code", nameof(ProdModel.MaterialCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_model_model_code", nameof(ProdModel.ModelCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_model_dest_code", nameof(ProdModel.DestCode), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_prod_model_created_time", nameof(ProdModel.CreatedTime), OrderByType.Desc, false)]
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
}