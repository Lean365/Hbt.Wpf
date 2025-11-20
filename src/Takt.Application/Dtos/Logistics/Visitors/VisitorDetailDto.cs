// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Visitors
// 文件名称：VisitorDetailDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客详情数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Visitors;

/// <summary>
/// 访客详情数据传输对象
/// </summary>
public class VisitorDetailDto
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

    // VisitorDetail 特有字段
    public long VisitorId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}

/// <summary>
/// 访客详情查询数据传输对象
/// </summary>
public class VisitorDetailQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在姓名、部门、职位中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    public long? VisitorId { get; set; }
    public string? Name { get; set; }
    public string? Department { get; set; }
}

/// <summary>
/// 创建访客详情数据传输对象
/// </summary>
public class VisitorDetailCreateDto
{
    public long VisitorId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}

/// <summary>
/// 更新访客详情数据传输对象
/// </summary>
public class VisitorDetailUpdateDto : VisitorDetailCreateDto
{
    public long Id { get; set; }
}

