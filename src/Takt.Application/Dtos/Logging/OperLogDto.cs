// ========================================
// 项目名称：节拍（Takt）中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logging
// 文件名称：OperLogDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：操作日志数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logging;

/// <summary>
/// 操作日志数据传输对象
/// </summary>
public class OperLogDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public OperLogDto()
    {
        Username = string.Empty;
        OperationType = string.Empty;
        OperationModule = string.Empty;
        OperationDesc = string.Empty;
        RequestPath = string.Empty;
        RequestMethod = string.Empty;
        RequestParams = string.Empty;
        ResponseResult = string.Empty;
        IpAddress = string.Empty;
        UserAgent = string.Empty;
        Os = string.Empty;
        Browser = string.Empty;
        OperationResult = "Success";
    }

    /// <summary>
    /// 日志ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string OperationType { get; set; }

    /// <summary>
    /// 操作模块
    /// </summary>
    public string OperationModule { get; set; }

    /// <summary>
    /// 操作描述
    /// </summary>
    public string OperationDesc { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime OperationTime { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string RequestPath { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string RequestMethod { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    public string RequestParams { get; set; }

    /// <summary>
    /// 响应结果
    /// </summary>
    public string ResponseResult { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public int ElapsedTime { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string Os { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    public string Browser { get; set; }

    /// <summary>
    /// 操作结果
    /// </summary>
    public string OperationResult { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 操作日志查询数据传输对象
/// </summary>
public class OperLogQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public OperLogQueryDto()
    {
        var now = DateTime.Now;
        Keywords = string.Empty;
        Username = string.Empty;
        OperationType = string.Empty;
        OperationModule = string.Empty;
        OperationTimeFrom = new DateTime(now.Year, now.Month, 1);
        OperationTimeTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    /// <summary>
    /// 搜索关键词（支持在用户名、操作类型、操作模块、操作描述中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string OperationType { get; set; }

    /// <summary>
    /// 操作模块
    /// </summary>
    public string OperationModule { get; set; }

    /// <summary>
    /// 操作时间开始（默认为本月第一天）
    /// </summary>
    public DateTime OperationTimeFrom { get; set; }

    /// <summary>
    /// 操作时间结束（默认为本月最后一天）
    /// </summary>
    public DateTime OperationTimeTo { get; set; }
}

