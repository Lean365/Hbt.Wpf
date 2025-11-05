// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：Visitor.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：访客实体
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

namespace Hbt.Domain.Entities.Logistics.Visitors;

/// <summary>
/// 访客实体
/// 用于管理访客的基本信息
/// </summary>
[SugarTable("hbt_logistics_visitor", "访客表")]
[SugarIndex("IX_hbt_logistics_visitor_company_name", nameof(Visitor.CompanyName), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_visitor_start_time", nameof(Visitor.StartTime), OrderByType.Desc, false)]
[SugarIndex("IX_hbt_logistics_visitor_created_time", nameof(Visitor.CreatedTime), OrderByType.Desc, false)]
public class Visitor : BaseEntity
{
    /// <summary>
    /// 公司名称
    /// </summary>
    [SugarColumn(ColumnName = "company_name", ColumnDescription = "公司名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// 起始时间
    /// </summary>
    [SugarColumn(ColumnName = "start_time", ColumnDescription = "起始时间", ColumnDataType = "datetime", IsNullable = false)]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnName = "end_time", ColumnDescription = "结束时间", ColumnDataType = "datetime", IsNullable = false)]
    public DateTime EndTime { get; set; }
}
