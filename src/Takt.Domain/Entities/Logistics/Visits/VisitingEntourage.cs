// ========================================
// 项目名称：Takt.Wpf
// 文件名称：VisitingEntourage.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员详情实体
// 
// 版权信息：
// Copyright (c) 2025 Takt All rights reserved.
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

namespace Takt.Domain.Entities.Logistics.Visits;

/// <summary>
/// 随行人员详情实体
/// 随行人员的详细信息
/// </summary>
[SugarTable("takt_logistics_visits_visiting_entourage", "随行人员详情表")]
[SugarIndex("IX_takt_logistics_visits_visiting_entourage_visiting_company_id", nameof(VisitingEntourage.VisitingCompanyId), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_visits_visiting_entourage_visiting_members", nameof(VisitingEntourage.VisitingMembers), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_visits_visiting_entourage_created_time", nameof(VisitingEntourage.CreatedTime), OrderByType.Desc, false)]
public class VisitingEntourage : BaseEntity
{
    /// <summary>
    /// 来访公司ID
    /// 关联的来访公司主表ID
    /// </summary>
    [SugarColumn(ColumnName = "visiting_company_id", ColumnDescription = "来访公司ID", IsNullable = false)]
    public long VisitingCompanyId { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    [SugarColumn(ColumnName = "visit_dept", ColumnDescription = "来访部门", ColumnDataType = "nvarchar", Length = 128, IsNullable = false)]
    public string VisitDept { get; set; } = string.Empty;

    /// <summary>
    /// 职务
    /// </summary>
    [SugarColumn(ColumnName = "visit_post", ColumnDescription = "来访职务", ColumnDataType = "nvarchar", Length = 128, IsNullable = false)]
    public string VisitPost { get; set; } = string.Empty;
    
    /// <summary>
    /// 姓名
    /// </summary>
    [SugarColumn(ColumnName = "visiting_members", ColumnDescription = "来访成员", ColumnDataType = "nvarchar", Length = 200, IsNullable = false)]
    public string VisitingMembers { get; set; } = string.Empty;



    /// <summary>
    /// 关联的随行人员主表（导航属性）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(VisitingCompanyId))]
    public VisitingCompany? EntourageNavigation { get; set; }
}
