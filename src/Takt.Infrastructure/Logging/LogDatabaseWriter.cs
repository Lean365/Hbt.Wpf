// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Logging
// 文件名称：LogDatabaseWriter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志数据库写入器实现
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Logging;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Logging;

/// <summary>
/// 日志数据库写入器实现
/// 负责将日志保存到数据库
/// </summary>
public class LogDatabaseWriter : ILogDatabaseWriter
{
    private readonly IBaseRepository<OperationLog> _operationLogRepository;
    private readonly IBaseRepository<ExceptionLog> _exceptionLogRepository;
    private readonly IBaseRepository<DiffLog> _diffLogRepository;

    public LogDatabaseWriter(
        IBaseRepository<OperationLog> operationLogRepository,
        IBaseRepository<ExceptionLog> exceptionLogRepository,
        IBaseRepository<DiffLog> diffLogRepository)
    {
        _operationLogRepository = operationLogRepository ?? throw new ArgumentNullException(nameof(operationLogRepository));
        _exceptionLogRepository = exceptionLogRepository ?? throw new ArgumentNullException(nameof(exceptionLogRepository));
        _diffLogRepository = diffLogRepository ?? throw new ArgumentNullException(nameof(diffLogRepository));
    }

    /// <summary>
    /// 保存操作日志到数据库
    /// </summary>
    public async Task SaveOperationLogAsync(
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
        string? browser = null)
    {
        try
        {
            var operationLog = new OperationLog
            {
                Username = username,
                OperationType = operationType,
                OperationModule = operationModule,
                OperationDesc = operationDesc,
                OperationTime = DateTime.Now,
                OperationResult = operationResult,
                IpAddress = ipAddress,
                RequestPath = requestPath,
                RequestMethod = requestMethod,
                RequestParams = requestParams,
                ResponseResult = responseResult,
                ElapsedTime = elapsedTime,
                UserAgent = userAgent,
                Os = os,
                Browser = browser
            };

            await _operationLogRepository.CreateAsync(operationLog);
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常，避免影响业务逻辑
            // 错误会通过 Serilog 记录到文件
            System.Diagnostics.Debug.WriteLine($"[LogDatabaseWriter] 保存操作日志失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存异常日志到数据库
    /// </summary>
    public async Task SaveExceptionLogAsync(string exceptionType, string exceptionMessage, string? stackTrace, string? innerException, string level = "Error", string? username = null, string? ipAddress = null)
    {
        try
        {
            var exceptionLog = new ExceptionLog
            {
                ExceptionType = exceptionType,
                ExceptionMessage = exceptionMessage.Length > 2000 ? exceptionMessage.Substring(0, 2000) : exceptionMessage,
                StackTrace = stackTrace,
                InnerException = innerException != null && innerException.Length > 2000 
                    ? innerException.Substring(0, 2000) 
                    : innerException,
                Level = level,
                ExceptionTime = DateTime.Now,
                Username = username,
                IpAddress = ipAddress
            };

            await _exceptionLogRepository.CreateAsync(exceptionLog);
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常，避免影响业务逻辑
            // 错误会通过 Serilog 记录到文件
            System.Diagnostics.Debug.WriteLine($"[LogDatabaseWriter] 保存异常日志失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存差异日志到数据库
    /// </summary>
    public async Task SaveDiffLogAsync(string tableName, string diffType, string? businessData, string? beforeData, string? afterData, string? sql, string? parameters, int elapsedTime, string? username = null, string? ipAddress = null)
    {
        try
        {
            var diffLog = new DiffLog
            {
                TableName = tableName,
                DiffType = diffType,
                BusinessData = businessData,
                BeforeData = beforeData,
                AfterData = afterData,
                Sql = sql,
                Parameters = parameters,
                ElapsedTime = elapsedTime,
                DiffTime = DateTime.Now,
                Username = username,
                IpAddress = ipAddress
            };

            await _diffLogRepository.CreateAsync(diffLog);
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常，避免影响业务逻辑
            // 错误会通过 Serilog 记录到文件
            System.Diagnostics.Debug.WriteLine($"[LogDatabaseWriter] 保存差异日志失败: {ex.Message}");
        }
    }
}

