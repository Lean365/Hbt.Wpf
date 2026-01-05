// ========================================
// 项目名称：Takt.Wpf
// 文件名称：ILogDatabaseWriter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志数据库写入器接口
// ========================================

namespace Takt.Common.Logging;

/// <summary>
/// 日志数据库写入器接口
/// 用于将日志保存到数据库，由 Infrastructure 层实现
/// </summary>
public interface ILogDatabaseWriter
{
    /// <summary>
    /// 保存操作日志到数据库
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="operationType">操作类型（Create/Update/Delete等）</param>
    /// <param name="operationModule">操作模块（实体名称）</param>
    /// <param name="operationDesc">操作描述</param>
    /// <param name="operationResult">操作结果（Success/Failed）</param>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="requestPath">请求路径（WPF中为视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestMethod">请求方法（WPF中为操作类型，如 "Create", "Update", "Delete"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="responseResult">响应结果（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    /// <param name="userAgent">用户代理（WPF中为客户端信息）</param>
    /// <param name="os">操作系统</param>
    /// <param name="browser">浏览器（WPF中为客户端类型）</param>
    Task SaveOperLogAsync(
        string username, 
        string operationType, 
        string operationModule, 
        string operationDesc, 
        string operationResult = "Success", 
        string? ipAddress = null!, 
        string? requestPath = null!, 
        string? requestMethod = null!, 
        string? requestParams = null!, 
        string? responseResult = null!, 
        int elapsedTime = 0,
        string? userAgent = null!, 
        string? os = null!, 
        string? browser = null);

    /// <summary>
    /// 保存差异日志到数据库
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="diffType">差异类型（insert/update/delete）</param>
    /// <param name="beforeData">变更前数据（JSON，已脱敏）</param>
    /// <param name="afterData">变更后数据（JSON，已脱敏）</param>
    /// <param name="sql">执行SQL</param>
    /// <param name="parameters">SQL参数（JSON）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    /// <param name="username">用户名</param>
    /// <param name="ipAddress">IP地址</param>
    Task SaveDiffLogAsync(string tableName, string diffType, string? beforeData, string? afterData, string? sql, string? parameters, int elapsedTime, string? username = null!, string? ipAddress = null);

    /// <summary>
    /// 保存任务日志到数据库
    /// </summary>
    /// <param name="quartzId">关联的任务ID</param>
    /// <param name="jobName">任务名称</param>
    /// <param name="jobGroup">任务组</param>
    /// <param name="triggerName">触发器名称</param>
    /// <param name="triggerGroup">触发器组</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间（可选）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    /// <param name="executeResult">执行结果（Success/Failed）</param>
    /// <param name="errorMessage">错误信息（可选）</param>
    /// <param name="jobParams">执行参数（JSON格式，可选）</param>
    Task SaveQuartzJobLogAsync(
        long quartzId,
        string jobName,
        string jobGroup,
        string triggerName,
        string triggerGroup,
        DateTime startTime,
        DateTime? endTime,
        int elapsedTime,
        string executeResult,
        string? errorMessage = null!, 
        string? jobParams = null);
}

