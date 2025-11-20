// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logging
// 文件名称：ExceptionLogDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：异常日志数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logging;

/// <summary>
/// 异常日志数据传输对象
/// </summary>
public class ExceptionLogDto
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 异常类型
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// 异常消息
    /// </summary>
    public string ExceptionMessage { get; set; } = string.Empty;

    /// <summary>
    /// 堆栈跟踪
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// 内部异常
    /// </summary>
    public string? InnerException { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public string Level { get; set; } = "Error";

    /// <summary>
    /// 异常时间
    /// </summary>
    public DateTime ExceptionTime { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 异常日志查询数据传输对象
/// </summary>
public class ExceptionLogQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在异常类型、异常消息、用户名、请求路径中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 异常类型
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// 异常时间开始
    /// </summary>
    public DateTime? ExceptionTimeFrom { get; set; }

    /// <summary>
    /// 异常时间结束
    /// </summary>
    public DateTime? ExceptionTimeTo { get; set; }
}

