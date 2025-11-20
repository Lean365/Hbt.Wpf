// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Visitors
// 文件名称：Visitor.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：访客实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// 
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Visitors;

/// <summary>
/// 访客实体
/// 用于管理访客的基本信息
/// </summary>
[SugarTable("takt_logistics_visitor", "访客表")]
[SugarIndex("IX_takt_logistics_visitor_company_name", nameof(Visitor.CompanyName), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_visitor_start_time", nameof(Visitor.StartTime), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logistics_visitor_created_time", nameof(Visitor.CreatedTime), OrderByType.Desc, false)]
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
