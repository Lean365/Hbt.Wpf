// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logistics.Visits
// 文件名称：VisitingCompany.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// 
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logistics.Visits;

/// <summary>
/// 随行人员实体
/// 用于管理随行人员的基本信息
/// </summary>
[SugarTable("takt_logistics_visits_visiting_company", "来访公司信息")]
[SugarIndex("IX_takt_logistics_visits_visiting_company_visiting_company_name", nameof(VisitingCompany.VisitingCompanyName), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logistics_visits_visiting_company_visit_start_time", nameof(VisitingCompany.VisitStartTime), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logistics_visits_visiting_company_created_time", nameof(VisitingCompany.CreatedTime), OrderByType.Desc, false)]
public class VisitingCompany : BaseEntity
{
    /// <summary>
    /// 公司名称
    /// </summary>
    [SugarColumn(ColumnName = "visiting_company", ColumnDescription = "公司名称", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string VisitingCompanyName { get; set; } = string.Empty;

    /// <summary>
    /// 起始时间
    /// </summary>
    [SugarColumn(ColumnName = "visit_start_time", ColumnDescription = "起始时间", ColumnDataType = "datetime", IsNullable = false)]
    public DateTime VisitStartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnName = "visit_end_time", ColumnDescription = "结束时间", ColumnDataType = "datetime", IsNullable = false)]
    public DateTime VisitEndTime { get; set; }

    /// <summary>
    /// 预约部门
    /// </summary>
    [SugarColumn(ColumnName = "reservations_dept", ColumnDescription = "预约部门", ColumnDataType = "nvarchar", Length = 128, IsNullable = true)]
    public string? ReservationsDept { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    [SugarColumn(ColumnName = "contact", ColumnDescription = "联系人", ColumnDataType = "nvarchar", Length = 100, IsNullable = true)]
    public string? Contact { get; set; }

    /// <summary>
    /// 访问目的
    /// </summary>
    [SugarColumn(ColumnName = "purpose", ColumnDescription = "访问目的", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? Purpose { get; set; }

    /// <summary>
    /// 预计时长（分钟）
    /// </summary>
    [SugarColumn(ColumnName = "duration", ColumnDescription = "预计时长", IsNullable = true)]
    public int? Duration { get; set; }

    /// <summary>
    /// 公司所属行业
    /// </summary>
    [SugarColumn(ColumnName = "industry", ColumnDescription = "所属行业", ColumnDataType = "nvarchar", Length = 128, IsNullable = true)]
    public string? Industry { get; set; }

    /// <summary>
    /// 车牌号
    /// </summary>
    [SugarColumn(ColumnName = "vehicle_plate", ColumnDescription = "车牌号", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? VehiclePlate { get; set; }

    /// <summary>
    /// 欢迎牌显示（0=需要）
    /// </summary>
    [SugarColumn(ColumnName = "is_welcome_sign", ColumnDescription = "欢迎牌显示", IsNullable = false)]
    public int IsWelcomeSign { get; set; } = 0;

    /// <summary>
    /// 是否用车（1=不需要）
    /// </summary>
    [SugarColumn(ColumnName = "is_vehicle_needed", ColumnDescription = "是否用车", IsNullable = false)]
    public int IsVehicleNeeded { get; set; } = 1;
}
