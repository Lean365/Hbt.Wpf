// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：LogDto.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：日志数据传输对象
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

namespace Hbt.Application.Dtos.Logging;

/// <summary>
/// 日志数据传输对象
/// 用于传输日志信息
/// </summary>
public class LogDto
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 日志级别
    /// </summary>
    public string Level { get; set; } = string.Empty;
    
    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 异常信息
    /// </summary>
    public string? Exception { get; set; }
    
    /// <summary>
    /// 属性信息
    /// </summary>
    public string? Properties { get; set; }
    
    /// <summary>
    /// 日志来源
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// 请求ID
    /// </summary>
    public string? RequestId { get; set; }
    
    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 日志查询数据传输对象
/// 用于查询日志
/// </summary>
public class LogQueryDto
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public string? Level { get; set; }
    
    /// <summary>
    /// 日志来源
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// 搜索关键字
    /// </summary>
    public string? Keyword { get; set; }
    
    /// <summary>
    /// 页码
    /// </summary>
    public int PageIndex { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 日志统计数据传输对象
/// 用于日志统计信息
/// </summary>
public class LogStatisticsDto
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public string Level { get; set; } = string.Empty;
    
    /// <summary>
    /// 数量
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; }
}
