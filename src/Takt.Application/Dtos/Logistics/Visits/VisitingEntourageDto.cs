// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Visits
// 文件名称：VisitingEntourageDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：来访成员详情数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Visits;

/// <summary>
/// 来访成员详情数据传输对象
/// </summary>
public class VisitingEntourageDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public VisitingEntourageDto()
    {
        VisitDept = string.Empty;
        VisitingMembers = string.Empty;
        VisitPost = string.Empty;
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
    /// 来访公司ID
    /// </summary>
    public long VisitingCompanyId { get; set; }

    /// <summary>
    /// 来访部门
    /// </summary>
    public string VisitDept { get; set; }

    /// <summary>
    /// 来访职务
    /// </summary>
    public string VisitPost { get; set; }

    /// <summary>
    /// 来访成员
    /// </summary>
    public string VisitingMembers { get; set; }

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
/// 来访成员详情查询数据传输对象
/// </summary>
public class VisitingEntourageQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public VisitingEntourageQueryDto()
    {
        Keywords = string.Empty;
        VisitingMembers = string.Empty;
        VisitDept = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在姓名、部门、职位中搜索）
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// 来访公司ID
    /// </summary>
    public long? VisitingCompanyId { get; set; }

    /// <summary>
    /// 来访成员
    /// </summary>
    public string VisitingMembers { get; set; }

    /// <summary>
    /// 来访部门
    /// </summary>
    public string VisitDept { get; set; }
}

/// <summary>
/// 创建来访成员详情数据传输对象
/// </summary>
public class VisitingEntourageCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public VisitingEntourageCreateDto()
    {
        VisitDept = string.Empty;
        VisitingMembers = string.Empty;
        VisitPost = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 来访公司ID
    /// </summary>
    public long VisitingCompanyId { get; set; }

    /// <summary>
    /// 来访部门
    /// </summary>
    public string VisitDept { get; set; }

    /// <summary>
    /// 来访职务
    /// </summary>
    public string VisitPost { get; set; }

    /// <summary>
    /// 来访成员
    /// </summary>
    public string VisitingMembers { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新来访成员详情数据传输对象
/// </summary>
public class VisitingEntourageUpdateDto : VisitingEntourageCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

