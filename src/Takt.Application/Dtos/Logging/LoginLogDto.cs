// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logging
// 文件名称：LoginLogDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：登录日志数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Enums;

namespace Takt.Application.Dtos.Logging;

/// <summary>
/// 登录日志数据传输对象
/// </summary>
public class LoginLogDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public LoginLogDto()
    {
        Username = string.Empty;
        LoginIp = string.Empty;
        MacAddress = string.Empty;
        MachineName = string.Empty;
        LoginLocation = string.Empty;
        Client = string.Empty;
        Os = string.Empty;
        OsVersion = string.Empty;
        OsArchitecture = string.Empty;
        CpuInfo = string.Empty;
        FrameworkVersion = string.Empty;
        ClientType = string.Empty;
        ClientVersion = string.Empty;
        FailReason = string.Empty;
        LoginTime = DateTime.Now;
        LogoutTime = DateTime.Now;
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
    /// 登录时间
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// 登出时间
    /// </summary>
    public DateTime LogoutTime { get; set; }

    /// <summary>
    /// 登录IP
    /// </summary>
    public string LoginIp { get; set; }

    /// <summary>
    /// MAC地址
    /// </summary>
    public string MacAddress { get; set; }

    /// <summary>
    /// 机器名称
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// 登录地点
    /// </summary>
    public string LoginLocation { get; set; }

    /// <summary>
    /// 客户端
    /// </summary>
    public string Client { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string Os { get; set; }

    /// <summary>
    /// 操作系统版本
    /// </summary>
    public string OsVersion { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    public string OsArchitecture { get; set; }

    /// <summary>
    /// CPU信息
    /// </summary>
    public string CpuInfo { get; set; }

    /// <summary>
    /// 物理内存(GB)
    /// </summary>
    public decimal? TotalMemoryGb { get; set; }

    /// <summary>
    /// .NET运行时
    /// </summary>
    public string FrameworkVersion { get; set; }

    /// <summary>
    /// 是否管理员
    /// </summary>
    public int IsAdmin { get; set; }

    /// <summary>
    /// 客户端类型
    /// </summary>
    public string ClientType { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string ClientVersion { get; set; }

    /// <summary>
    /// 登录状态
    /// </summary>
    public LoginStatusEnum LoginStatus { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public string FailReason { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 登录日志查询数据传输对象
/// </summary>
public class LoginLogQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public LoginLogQueryDto()
    {
        var now = DateTime.Now;
        Keywords = string.Empty;
        Username = string.Empty;
        LoginIp = string.Empty;
        LoginTimeFrom = new DateTime(now.Year, now.Month, 1);
        LoginTimeTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
    }

    /// <summary>
    /// 搜索关键词（支持在用户名、登录IP、机器名中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 登录IP
    /// </summary>
    public string LoginIp { get; set; }

    /// <summary>
    /// 登录状态
    /// </summary>
    public LoginStatusEnum? LoginStatus { get; set; }

    /// <summary>
    /// 登录时间开始（默认为本月第一天）
    /// </summary>
    public DateTime LoginTimeFrom { get; set; }

    /// <summary>
    /// 登录时间结束（默认为本月最后一天）
    /// </summary>
    public DateTime LoginTimeTo { get; set; }
}

