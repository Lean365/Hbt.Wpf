// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Visits
// 文件名称：VisitingCompanyDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Visits;

/// <summary>
/// 来访公司数据传输对象
/// </summary>
public class VisitingCompanyDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public VisitingCompanyDto()
    {
        VisitingCompanyName = string.Empty;
        ReservationsDept = null;
        Contact = null;
        Purpose = null;
        Industry = null;
        VehiclePlate = null;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        VisitStartTime = DateTime.Now;
        VisitEndTime = DateTime.Now;
        IsWelcomeSign = 0;
        IsVehicleNeeded = 1;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    public string VisitingCompanyName { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime VisitStartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime VisitEndTime { get; set; }

    /// <summary>
    /// 预约部门
    /// </summary>
    public string? ReservationsDept { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    public string? Contact { get; set; }

    /// <summary>
    /// 访问目的
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// 预计时长（分钟）
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 公司所属行业
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// 车牌号
    /// </summary>
    public string? VehiclePlate { get; set; }

    /// <summary>
    /// 欢迎牌显示（0=需要）
    /// </summary>
    public int IsWelcomeSign { get; set; }

    /// <summary>
    /// 是否用车（1=不需要）
    /// </summary>
    public int IsVehicleNeeded { get; set; }

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
/// 来访公司查询数据传输对象
/// </summary>
public class VisitingCompanyQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public VisitingCompanyQueryDto()
    {
        var now = DateTime.Now;
        Keywords = string.Empty;
        VisitingCompanyName = string.Empty;
        VisitStartTimeFrom = new DateTime(now.Year, now.Month, 1);
        VisitStartTimeTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    /// <summary>
    /// 搜索关键词（支持在公司名称中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    public string VisitingCompanyName { get; set; }

    /// <summary>
    /// 开始时间（起始，默认为本月第一天）
    /// </summary>
    public DateTime VisitStartTimeFrom { get; set; }

    /// <summary>
    /// 开始时间（结束，默认为本月最后一天）
    /// </summary>
    public DateTime VisitStartTimeTo { get; set; }
}

/// <summary>
/// 创建来访公司数据传输对象
/// </summary>
public class VisitingCompanyCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public VisitingCompanyCreateDto()
    {
        VisitingCompanyName = string.Empty;
        ReservationsDept = null;
        Contact = null;
        Purpose = null;
        Industry = null;
        VehiclePlate = null;
        Remarks = string.Empty;
        IsWelcomeSign = 0;
        IsVehicleNeeded = 1;
    }

    /// <summary>
    /// 公司名称
    /// </summary>
    public string VisitingCompanyName { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime VisitStartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime VisitEndTime { get; set; }

    /// <summary>
    /// 预约部门
    /// </summary>
    public string? ReservationsDept { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    public string? Contact { get; set; }

    /// <summary>
    /// 访问目的
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// 预计时长（分钟）
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 公司所属行业
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// 车牌号
    /// </summary>
    public string? VehiclePlate { get; set; }

    /// <summary>
    /// 欢迎牌显示（0=需要）
    /// </summary>
    public int IsWelcomeSign { get; set; }

    /// <summary>
    /// 是否用车（1=不需要）
    /// </summary>
    public int IsVehicleNeeded { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新来访公司数据传输对象
/// </summary>
public class VisitingCompanyUpdateDto : VisitingCompanyCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

