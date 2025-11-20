// ========================================
// 项目名称：Takt.Wpf
// 文件名称：OperLogManager.cs
// 创建时间：2025-10-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：操作日志管理器
// ========================================

using Serilog;
using Serilog.Events;

namespace Takt.Common.Logging;

/// <summary>
/// 操作日志管理器
/// 专门用于记录用户操作日志（登录、创建、更新、删除等）
/// 同时记录到文件（Serilog）和数据库（通过 ILogDatabaseWriter 接口）
/// </summary>
public class OperLogManager : ILogManager
{
    private readonly ILogger _operLogger;
    private readonly ILogDatabaseWriter? _logDatabaseWriter;

    public OperLogManager(ILogger logger, ILogDatabaseWriter? logDatabaseWriter = null)
    {
        // 使用符合 Windows 规范的日志目录（AppData\Local）
        var logsDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();

        // 创建独立的操作日志器
        // 设置最小日志级别为 Debug，确保所有调试日志都能被记录
        _operLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()  // 设置最小日志级别为 Debug
            .WriteTo.File(
                path: Path.Combine(logsDir, "oper-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)  // 文件输出也接受 Debug 级别
            .CreateLogger();

        _logDatabaseWriter = logDatabaseWriter;
    }

    /// <summary>
    /// 记录操作信息
    /// </summary>
    public void Information(string message, params object[] args)
    {
        _operLogger.Information("[操作] " + message, args);
    }

    /// <summary>
    /// 记录操作警告
    /// </summary>
    public void Warning(string message, params object[] args)
    {
        _operLogger.Warning("[操作] " + message, args);
    }

    /// <summary>
    /// 记录操作错误
    /// </summary>
    public void Error(string message, params object[] args)
    {
        _operLogger.Error("[操作] " + message, args);
    }

    /// <summary>
    /// 记录操作错误（带异常）
    /// </summary>
    public void Error(Exception exception, string message, params object[] args)
    {
        // 记录到文件
        _operLogger.Error(exception, "[操作] " + message, args);

        // 保存到数据库（异步，不阻塞）
        _ = SaveExceptionLogAsync(exception, message, args);
    }

    /// <summary>
    /// 异步保存异常日志到数据库
    /// </summary>
    private async Task SaveExceptionLogAsync(Exception exception, string message, params object[] args)
    {
        if (_logDatabaseWriter == null)
            return;

        try
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            var exceptionType = exception.GetType().FullName ?? exception.GetType().Name;
            var exceptionMessage = formattedMessage.Length > 2000 ? formattedMessage.Substring(0, 2000) : formattedMessage;
            var stackTrace = exception.StackTrace;
            var innerException = exception.InnerException?.ToString();
            if (innerException != null && innerException.Length > 2000)
            {
                innerException = innerException.Substring(0, 2000);
            }

            await _logDatabaseWriter.SaveExceptionLogAsync(
                exceptionType,
                exceptionMessage,
                stackTrace,
                innerException,
                "Error",
                GetCurrentUsername(),
                GetCurrentIpAddress()
            );
        }
        catch (Exception ex)
        {
            // 如果保存到数据库失败，只记录到文件，不抛出异常
            _operLogger.Error(ex, "保存异常日志到数据库失败");
        }
    }

    /// <summary>
    /// 获取当前用户名（从上下文获取，如果无法获取则返回 "System"）
    /// </summary>
    private string GetCurrentUsername()
    {
        try
        {
            var userContext = Takt.Common.Context.UserContext.Current;
            if (userContext.IsAuthenticated && !string.IsNullOrEmpty(userContext.Username))
            {
                return userContext.Username;
            }
        }
        catch
        {
            // 忽略异常
        }
        return "System";
    }

    /// <summary>
    /// 获取当前IP地址
    /// </summary>
    private string? GetCurrentIpAddress()
    {
        try
        {
            return Takt.Common.Helpers.SystemInfoHelper.GetLocalIpAddress();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取操作系统信息
    /// </summary>
    private string? GetOsInfo()
    {
        try
        {
            var osType = Takt.Common.Helpers.SystemInfoHelper.GetOsType();
            var osVersion = Takt.Common.Helpers.SystemInfoHelper.GetOsVersion();
            return $"{osType} {osVersion}";
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取客户端信息（用户代理）
    /// </summary>
    private string? GetUserAgent()
    {
        try
        {
            return Takt.Common.Helpers.SystemInfoHelper.GetClientName();
        }
        catch
        {
            return "WPF Desktop App";
        }
    }

    /// <summary>
    /// 获取客户端类型（浏览器字段）
    /// </summary>
    private string? GetBrowser()
    {
        try
        {
            return Takt.Common.Helpers.SystemInfoHelper.GetClientType();
        }
        catch
        {
            return "Desktop";
        }
    }

    /// <summary>
    /// 记录操作调试信息
    /// </summary>
    public void Debug(string message, params object[] args)
    {
        _operLogger.Debug("[操作] " + message, args);
    }

    /// <summary>
    /// 记录用户登录
    /// </summary>
    public void Login(string username, string realName, bool success, string ip = "")
    {
        if (success)
            _operLogger.Information("[操作-登录] 用户登录成功：{Username} ({RealName}) IP:{IP}", username, realName, ip);
        else
            _operLogger.Warning("[操作-登录] 用户登录失败：{Username} IP:{IP}", username, ip);
    }

    /// <summary>
    /// 记录用户登出
    /// </summary>
    public void Logout(string username, string realName)
    {
        _operLogger.Information("[操作-登出] 用户登出：{Username} ({RealName})", username, realName);
    }

    /// <summary>
    /// 记录创建操作
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="operatorName">操作人</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    public void Create(string entityName, string entityId, string operatorName, string? requestPath = null, string? requestParams = null, int elapsedTime = 0)
    {
        // 记录到文件
        _operLogger.Information("[操作-创建] {Operator} 创建了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);

        // 保存到数据库（异步，不阻塞）
        var requestParamsJson = requestParams ?? System.Text.Json.JsonSerializer.Serialize(new { EntityId = entityId });
        _ = SaveOperationLogAsync("Create", entityName, operatorName, $"创建了 {entityName} (ID:{entityId})", requestPath, requestParamsJson, null, elapsedTime);
    }

    /// <summary>
    /// 记录更新操作
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="operatorName">操作人</param>
    /// <param name="changes">变更内容</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    public void Update(string entityName, string entityId, string operatorName, string changes = "", string? requestPath = null, string? requestParams = null, int elapsedTime = 0)
    {
        // 记录到文件
        _operLogger.Information("[操作-更新] {Operator} 更新了 {EntityName} (ID:{EntityId}) 变更:{Changes}", 
            operatorName, entityName, entityId, changes);

        // 保存到数据库（异步，不阻塞）
        var desc = string.IsNullOrEmpty(changes) 
            ? $"更新了 {entityName} (ID:{entityId})" 
            : $"更新了 {entityName} (ID:{entityId}) 变更:{changes}";
        var requestParamsJson = requestParams ?? System.Text.Json.JsonSerializer.Serialize(new { EntityId = entityId, Changes = changes });
        _ = SaveOperationLogAsync("Update", entityName, operatorName, desc, requestPath, requestParamsJson, null, elapsedTime);
    }

    /// <summary>
    /// 记录删除操作
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="operatorName">操作人</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    public void Delete(string entityName, string entityId, string operatorName, string? requestPath = null, string? requestParams = null, int elapsedTime = 0)
    {
        // 记录到文件
        _operLogger.Information("[操作-删除] {Operator} 删除了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);

        // 保存到数据库（异步，不阻塞）
        var requestParamsJson = requestParams ?? System.Text.Json.JsonSerializer.Serialize(new { EntityId = entityId });
        _ = SaveOperationLogAsync("Delete", entityName, operatorName, $"删除了 {entityName} (ID:{entityId})", requestPath, requestParamsJson, null, elapsedTime);
    }

    /// <summary>
    /// 异步保存操作日志到数据库
    /// </summary>
    private async Task SaveOperationLogAsync(
        string operationType, 
        string operationModule, 
        string operatorName, 
        string operationDesc,
        string? requestPath = null,
        string? requestParams = null,
        string? responseResult = null,
        int elapsedTime = 0)
    {
        if (_logDatabaseWriter == null)
            return;

        try
        {
            var desc = operationDesc.Length > 500 ? operationDesc.Substring(0, 500) : operationDesc;
            await _logDatabaseWriter.SaveOperationLogAsync(
                operatorName,
                operationType,
                operationModule,
                desc,
                "Success",
                GetCurrentIpAddress(),
                requestPath,
                operationType, // RequestMethod 使用操作类型
                requestParams,
                responseResult,
                elapsedTime,
                GetUserAgent(),
                GetOsInfo(),
                GetBrowser()
            );
        }
        catch (Exception ex)
        {
            // 如果保存到数据库失败，只记录到文件，不抛出异常
            _operLogger.Error(ex, "保存操作日志到数据库失败");
        }
    }

    /// <summary>
    /// 记录查询操作
    /// </summary>
    public void Query(string entityName, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-查询] {Operator} 查询了 {EntityName} 条件:{Condition}", 
            operatorName, entityName, condition);
    }

    /// <summary>
    /// 记录导出操作
    /// </summary>
    public void Export(string entityName, int count, string operatorName)
    {
        _operLogger.Information("[操作-导出] {Operator} 导出了 {Count} 条 {EntityName} 数据", 
            operatorName, count, entityName);
    }

    /// <summary>
    /// 记录导入操作
    /// </summary>
    public void Import(string entityName, int count, string operatorName)
    {
        _operLogger.Information("[操作-导入] {Operator} 导入了 {Count} 条 {EntityName} 数据", 
            operatorName, count, entityName);
    }

    /// <summary>
    /// 记录批量创建操作
    /// </summary>
    public void BatchCreate(string entityName, int count, string operatorName)
    {
        _operLogger.Information("[操作-批量创建] {Operator} 批量创建了 {Count} 条 {EntityName} 数据", 
            operatorName, count, entityName);
    }

    /// <summary>
    /// 记录批量更新操作
    /// </summary>
    public void BatchUpdate(string entityName, int count, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-批量更新] {Operator} 批量更新了 {Count} 条 {EntityName} 数据 条件:{Condition}", 
            operatorName, count, entityName, condition);
    }

    /// <summary>
    /// 记录批量删除操作
    /// </summary>
    public void BatchDelete(string entityName, int count, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-批量删除] {Operator} 批量删除了 {Count} 条 {EntityName} 数据 条件:{Condition}", 
            operatorName, count, entityName, condition);
    }

    /// <summary>
    /// 记录复制操作
    /// </summary>
    public void Copy(string entityName, string sourceId, string targetId, string operatorName)
    {
        _operLogger.Information("[操作-复制] {Operator} 复制了 {EntityName} 从 {SourceId} 到 {TargetId}", 
            operatorName, entityName, sourceId, targetId);
    }

    /// <summary>
    /// 记录移动操作
    /// </summary>
    public void Move(string entityName, string sourceId, string targetId, string operatorName)
    {
        _operLogger.Information("[操作-移动] {Operator} 移动了 {EntityName} 从 {SourceId} 到 {TargetId}", 
            operatorName, entityName, sourceId, targetId);
    }

    /// <summary>
    /// 记录打印操作
    /// </summary>
    public void Print(string entityName, string entityId, string operatorName, string printType = "")
    {
        _operLogger.Information("[操作-打印] {Operator} 打印了 {EntityName} (ID:{EntityId}) 类型:{PrintType}", 
            operatorName, entityName, entityId, printType);
    }

    /// <summary>
    /// 记录预览操作
    /// </summary>
    public void Preview(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-预览] {Operator} 预览了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录详情查看操作
    /// </summary>
    public void Detail(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-详情] {Operator} 查看了 {EntityName} (ID:{EntityId}) 详情", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录锁定操作
    /// </summary>
    public void Lock(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-锁定] {Operator} 锁定了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录解锁操作
    /// </summary>
    public void Unlock(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-解锁] {Operator} 解锁了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录启用操作
    /// </summary>
    public void Enable(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-启用] {Operator} 启用了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录禁用操作
    /// </summary>
    public void Disable(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-禁用] {Operator} 禁用了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录提交操作
    /// </summary>
    public void Submit(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-提交] {Operator} 提交了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录撤回操作
    /// </summary>
    public void Recall(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-撤回] {Operator} 撤回了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录审核操作
    /// </summary>
    public void Approve(string entityName, string entityId, string operatorName, bool approved, string reason = "")
    {
        var status = approved ? "通过" : "拒绝";
        _operLogger.Information("[操作-审核] {Operator} {Status}了 {EntityName} (ID:{EntityId}) 原因:{Reason}", 
            operatorName, status, entityName, entityId, reason);
    }

    /// <summary>
    /// 记录发送操作
    /// </summary>
    public void Send(string entityName, string entityId, string operatorName, string recipient = "")
    {
        _operLogger.Information("[操作-发送] {Operator} 发送了 {EntityName} (ID:{EntityId}) 接收方:{Recipient}", 
            operatorName, entityName, entityId, recipient);
    }

    /// <summary>
    /// 记录启动操作
    /// </summary>
    public void Start(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-启动] {Operator} 启动了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录停止操作
    /// </summary>
    public void Stop(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-停止] {Operator} 停止了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录重启操作
    /// </summary>
    public void Restart(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-重启] {Operator} 重启了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录收藏操作
    /// </summary>
    public void Favorite(string entityName, string entityId, string operatorName, bool favorited)
    {
        var action = favorited ? "收藏" : "取消收藏";
        _operLogger.Information("[操作-{Action}] {Operator} {Action}了 {EntityName} (ID:{EntityId})", 
            action, operatorName, action, entityName, entityId);
    }

    /// <summary>
    /// 记录点赞操作
    /// </summary>
    public void Like(string entityName, string entityId, string operatorName, bool liked)
    {
        var action = liked ? "点赞" : "取消点赞";
        _operLogger.Information("[操作-{Action}] {Operator} {Action}了 {EntityName} (ID:{EntityId})", 
            action, operatorName, action, entityName, entityId);
    }

    /// <summary>
    /// 记录评论操作
    /// </summary>
    public void Comment(string entityName, string entityId, string operatorName, string comment = "")
    {
        _operLogger.Information("[操作-评论] {Operator} 评论了 {EntityName} (ID:{EntityId}) 内容:{Comment}", 
            operatorName, entityName, entityId, comment);
    }

    /// <summary>
    /// 记录分享操作
    /// </summary>
    public void Share(string entityName, string entityId, string operatorName, string platform = "")
    {
        _operLogger.Information("[操作-分享] {Operator} 分享了 {EntityName} (ID:{EntityId}) 平台:{Platform}", 
            operatorName, entityName, entityId, platform);
    }

    /// <summary>
    /// 记录订阅操作
    /// </summary>
    public void Subscribe(string entityName, string entityId, string operatorName, bool subscribed)
    {
        var action = subscribed ? "订阅" : "取消订阅";
        _operLogger.Information("[操作-{Action}] {Operator} {Action}了 {EntityName} (ID:{EntityId})", 
            action, operatorName, action, entityName, entityId);
    }

    /// <summary>
    /// 记录刷新操作
    /// </summary>
    public void Refresh(string entityName, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-刷新] {Operator} 刷新了 {EntityName} 条件:{Condition}", 
            operatorName, entityName, condition);
    }

    /// <summary>
    /// 记录归档操作
    /// </summary>
    public void Archive(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-归档] {Operator} 归档了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录恢复操作
    /// </summary>
    public void Restore(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-恢复] {Operator} 恢复了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录通知操作
    /// </summary>
    public void Notify(string entityName, string entityId, string operatorName, string message = "")
    {
        _operLogger.Information("[操作-通知] {Operator} 通知了 {EntityName} (ID:{EntityId}) 消息:{Message}", 
            operatorName, entityName, entityId, message);
    }

    /// <summary>
    /// 记录附件操作
    /// </summary>
    public void Attach(string entityName, string entityId, string operatorName, string fileName)
    {
        _operLogger.Information("[操作-附件] {Operator} 为 {EntityName} (ID:{EntityId}) 添加了附件 {FileName}", 
            operatorName, entityName, entityId, fileName);
    }
}

