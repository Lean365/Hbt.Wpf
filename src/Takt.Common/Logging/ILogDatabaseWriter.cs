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
    Task SaveOperationLogAsync(
        string username, 
        string operationType, 
        string operationModule, 
        string operationDesc, 
        string operationResult = "Success", 
        string? ipAddress = null,
        string? requestPath = null,
        string? requestMethod = null,
        string? requestParams = null,
        string? responseResult = null,
        int elapsedTime = 0,
        string? userAgent = null,
        string? os = null,
        string? browser = null);

    /// <summary>
    /// 保存异常日志到数据库
    /// </summary>
    /// <param name="exceptionType">异常类型</param>
    /// <param name="exceptionMessage">异常消息</param>
    /// <param name="stackTrace">堆栈跟踪</param>
    /// <param name="innerException">内部异常</param>
    /// <param name="level">日志级别</param>
    /// <param name="username">用户名</param>
    /// <param name="ipAddress">IP地址</param>
    Task SaveExceptionLogAsync(string exceptionType, string exceptionMessage, string? stackTrace, string? innerException, string level = "Error", string? username = null, string? ipAddress = null);

    /// <summary>
    /// 保存差异日志到数据库
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="diffType">差异类型（insert/update/delete）</param>
    /// <param name="businessData">业务数据</param>
    /// <param name="beforeData">变更前数据（JSON）</param>
    /// <param name="afterData">变更后数据（JSON）</param>
    /// <param name="sql">执行SQL</param>
    /// <param name="parameters">SQL参数（JSON）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    /// <param name="username">用户名</param>
    /// <param name="ipAddress">IP地址</param>
    Task SaveDiffLogAsync(string tableName, string diffType, string? businessData, string? beforeData, string? afterData, string? sql, string? parameters, int elapsedTime, string? username = null, string? ipAddress = null);
}

