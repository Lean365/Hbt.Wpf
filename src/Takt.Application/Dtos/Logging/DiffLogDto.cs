// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logging
// 文件名称：DiffLogDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：差异日志数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logging;

/// <summary>
/// 差异日志数据传输对象
/// </summary>
public class DiffLogDto
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 差异类型
    /// </summary>
    public string DiffType { get; set; } = string.Empty;

    /// <summary>
    /// 业务数据
    /// </summary>
    public string? BusinessData { get; set; }

    /// <summary>
    /// 变更前数据
    /// </summary>
    public string? BeforeData { get; set; }

    /// <summary>
    /// 变更后数据
    /// </summary>
    public string? AfterData { get; set; }

    /// <summary>
    /// 执行SQL
    /// </summary>
    public string? Sql { get; set; }

    /// <summary>
    /// SQL参数
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// 差异时间
    /// </summary>
    public DateTime DiffTime { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public int ElapsedTime { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 差异日志查询数据传输对象
/// </summary>
public class DiffLogQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在表名、差异类型、业务数据、用户名中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 表名
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// 差异类型
    /// </summary>
    public string? DiffType { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 差异时间开始
    /// </summary>
    public DateTime? DiffTimeFrom { get; set; }

    /// <summary>
    /// 差异时间结束
    /// </summary>
    public DateTime? DiffTimeTo { get; set; }
}

