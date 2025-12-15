// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Logging
// 文件名称：LogDatabaseWriter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志数据库写入器实现
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
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
    private readonly IBaseRepository<OperLog> _operationLogRepository;
    private readonly IBaseRepository<DiffLog> _diffLogRepository;
    private readonly IBaseRepository<QuartzJobLog> _quartzJobLogRepository;
    private readonly AppLogManager _appLog;

    public LogDatabaseWriter(
        IBaseRepository<OperLog> operationLogRepository,
        IBaseRepository<DiffLog> diffLogRepository,
        IBaseRepository<QuartzJobLog> quartzJobLogRepository,
        AppLogManager appLog)
    {
        _operationLogRepository = operationLogRepository ?? throw new ArgumentNullException(nameof(operationLogRepository));
        _diffLogRepository = diffLogRepository ?? throw new ArgumentNullException(nameof(diffLogRepository));
        _quartzJobLogRepository = quartzJobLogRepository ?? throw new ArgumentNullException(nameof(quartzJobLogRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 保存操作日志到数据库
    /// </summary>
    public async Task SaveOperLogAsync(
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
        string? browser = null)
    {
        try
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(username))
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 用户名为空，使用默认值 'System'");
                username = "Takt365";
            }

            if (string.IsNullOrWhiteSpace(operationType))
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 操作类型为空，使用默认值 'Unknown'");
                operationType = "Unknown";
            }

            if (string.IsNullOrWhiteSpace(operationModule))
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 操作模块为空，使用默认值 'Unknown'");
                operationModule = "Unknown";
            }

            // 确保操作结果有值
            if (string.IsNullOrWhiteSpace(operationResult))
            {
                operationResult = "Success";
            }

            // 自动填充缺失的系统信息字段
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                try
                {
                    ipAddress = Takt.Common.Helpers.SystemInfoHelper.GetLocalIpAddress();
                }
                catch (Exception ex)
                {
                    _appLog.Debug("[LogDatabaseWriter] 获取IP地址失败: {ErrorMessage}", ex.Message);
                }
            }

            if (string.IsNullOrWhiteSpace(userAgent))
            {
                try
                {
                    userAgent = Takt.Common.Helpers.SystemInfoHelper.GetClientName();
                }
                catch (Exception ex)
                {
                    _appLog.Debug("[LogDatabaseWriter] 获取用户代理失败: {ErrorMessage}", ex.Message);
                    userAgent = "WPF Desktop App";
                }
            }

            if (string.IsNullOrWhiteSpace(os))
            {
                try
                {
                    var osType = Takt.Common.Helpers.SystemInfoHelper.GetOsType();
                    var osVersion = Takt.Common.Helpers.SystemInfoHelper.GetOsVersion();
                    os = $"{osType} {osVersion}";
                }
                catch (Exception ex)
                {
                    _appLog.Debug("[LogDatabaseWriter] 获取操作系统信息失败: {ErrorMessage}", ex.Message);
                }
            }

            if (string.IsNullOrWhiteSpace(browser))
            {
                try
                {
                    browser = Takt.Common.Helpers.SystemInfoHelper.GetClientType();
                }
                catch (Exception ex)
                {
                    _appLog.Debug("[LogDatabaseWriter] 获取客户端类型失败: {ErrorMessage}", ex.Message);
                    browser = "Desktop";
                }
            }

            // 如果 RequestMethod 为空，根据操作类型设置合理的值
            if (string.IsNullOrWhiteSpace(requestMethod))
            {
                // 根据操作类型映射到常见的HTTP方法（WPF应用中使用操作类型作为方法）
                requestMethod = operationType switch
                {
                    "Create" => "POST",
                    "Update" => "PUT",
                    "Delete" => "DELETE",
                    "Query" => "GET",
                    "Export" => "GET",
                    "Import" => "POST",
                    _ => operationType // 其他情况使用操作类型本身
                };
            }

            // 使用当前真实时间
            var operationTime = DateTime.Now;

            // 验证并确保 ElapsedTime 正确（毫秒）
            // 确保非负数，如果为负数则记录警告并使用 0
            var validElapsedTime = elapsedTime < 0 ? 0 : elapsedTime;
            if (elapsedTime < 0)
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 执行耗时为负数 ({ElapsedTime}ms)，已修正为 0", elapsedTime);
            }

            var operationLog = new OperLog
            {
                Username = username,
                OperationType = operationType,
                OperationModule = operationModule,
                OperationDesc = operationDesc ?? string.Empty,
                OperationTime = operationTime,
                OperationResult = operationResult,
                IpAddress = ipAddress,
                RequestPath = requestPath,
                RequestMethod = requestMethod,
                RequestParams = Takt.Common.Helpers.DataMaskingHelper.MaskSensitiveJson(requestParams),
                ResponseResult = responseResult,
                ElapsedTime = validElapsedTime,
                UserAgent = userAgent,
                Os = os,
                Browser = browser
            };

            // 记录调试信息，验证所有字段都有真实数据
            _appLog.Debug("[LogDatabaseWriter] 保存操作日志: 用户={Username}, 操作类型={OperationType}, 模块={OperationModule}, " +
                "IP={IpAddress}, 耗时={ElapsedTime}ms, 结果={OperationResult}, 时间={OperationTime}",
                operationLog.Username, operationLog.OperationType, operationLog.OperationModule,
                operationLog.IpAddress ?? "N/A", operationLog.ElapsedTime, operationLog.OperationResult, operationLog.OperationTime);

            _appLog.Debug("[LogDatabaseWriter] 准备调用 _operationLogRepository.CreateAsync");
            var result = await _operationLogRepository.CreateAsync(operationLog);
            
            if (result <= 0)
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 保存操作日志失败: 影响行数={Result}, 用户={Username}, 操作类型={OperationType}", 
                    result, operationLog.Username, operationLog.OperationType);
            }
            else
            {
                _appLog.Information("[LogDatabaseWriter] ✅ 操作日志保存成功: 影响行数={Result}, ID={Id}, 用户={Username}, 操作类型={OperationType}", 
                    result, operationLog.Id, operationLog.Username, operationLog.OperationType);
            }
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常，避免影响业务逻辑
            // 使用统一日志处理（Serilog）
            _appLog.Error(ex, "[LogDatabaseWriter] 保存操作日志失败: {ErrorMessage}", ex.Message);
        }
    }

    /// <summary>
    /// 保存差异日志到数据库
    /// </summary>
    public async Task SaveDiffLogAsync(string tableName, string diffType, string? beforeData, string? afterData, string? sql, string? parameters, int elapsedTime, string? username = null!, string? ipAddress = null)
    {
        try
        {
            _appLog.Debug("[LogDatabaseWriter] 开始保存差异日志: 表={TableName}, 操作={DiffType}, 操作人={Username}", 
                tableName, diffType, username ?? "Takt365");
            
            // 验证必要参数
            if (string.IsNullOrEmpty(tableName))
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 表名为空，无法保存差异日志: 操作={DiffType}", diffType);
                return;
            }
            
            if (string.IsNullOrEmpty(diffType))
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 差异类型为空，无法保存差异日志: 表={TableName}", tableName);
                return;
            }
            
            // 验证并确保 ElapsedTime 正确（毫秒）
            // 确保非负数，如果为负数则记录警告并使用 0
            var validElapsedTime = elapsedTime < 0 ? 0 : elapsedTime;
            if (elapsedTime < 0)
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 执行耗时为负数 ({ElapsedTime}ms)，已修正为 0", elapsedTime);
            }
            
            var diffLog = new DiffLog
            {
                TableName = tableName,
                DiffType = diffType,
                BeforeData = beforeData,
                AfterData = afterData,
                Sql = sql,
                Parameters = parameters,
                ElapsedTime = validElapsedTime,
                DiffTime = DateTime.Now,
                Username = username,
                IpAddress = ipAddress
            };

            _appLog.Debug("[LogDatabaseWriter] 差异日志对象已创建，准备保存到数据库: 表={TableName}, 操作={DiffType}, 耗时={ElapsedTime}ms", 
                tableName, diffType, diffLog.ElapsedTime);

            // 使用 CreateAsync 保存差异日志
            // 注意：保存差异日志本身不应该触发差异日志事件，避免循环
            // 因为差异日志表使用 Insertable，而差异日志事件只在 Updateable 时触发
            var result = await _diffLogRepository.CreateAsync(diffLog);
            
            _appLog.Debug("[LogDatabaseWriter] CreateAsync 调用完成，返回结果: {Result}", result);
            
            // 验证保存结果
            if (result <= 0)
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 保存差异日志失败: 表={TableName}, 操作={DiffType}, 影响行数={Result}", 
                    tableName, diffType, result);
            }
            else
            {
                _appLog.Information("[LogDatabaseWriter] ✅ 差异日志保存成功: 表={TableName}, 操作={DiffType}, 影响行数={Result}, ID={Id}", 
                    tableName, diffType, result, diffLog.Id);
            }
        }
        catch (Exception ex)
        {
            // 记录详细错误信息，包括堆栈跟踪
            _appLog.Error(ex, "[LogDatabaseWriter] ❌ 保存差异日志失败: 表={TableName}, 操作={DiffType}, 错误={ErrorMessage}, 堆栈={StackTrace}", 
                tableName, diffType, ex.Message, ex.StackTrace ?? "Unknown");
            // 不重新抛出异常，让调用者（SqlSugarAop）记录到日志即可
        }
    }

    /// <summary>
    /// 保存任务日志到数据库
    /// 统一日志保存逻辑，与 OperLog 和 DiffLog 保持一致
    /// </summary>
    public async Task SaveQuartzJobLogAsync(
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
        string? jobParams = null)
    {
        try
        {
            _appLog.Debug("[LogDatabaseWriter] 开始保存任务日志: 任务名称={JobName}, 任务组={JobGroup}, 执行结果={ExecuteResult}", 
                jobName, jobGroup, executeResult ?? "Unknown");
            
            // 验证必要参数
            if (string.IsNullOrWhiteSpace(jobName))
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 任务名称为空，无法保存任务日志");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(jobGroup))
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 任务组为空，无法保存任务日志: 任务名称={JobName}", jobName);
                return;
            }
            
            // 验证并确保 ElapsedTime 正确（毫秒）
            var validElapsedTime = elapsedTime < 0 ? 0 : elapsedTime;
            if (elapsedTime < 0)
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 执行耗时为负数 ({ElapsedTime}ms)，已修正为 0", elapsedTime);
            }
            
            // 确保执行结果有值
            if (string.IsNullOrWhiteSpace(executeResult))
            {
                executeResult = "Success";
            }
            
            // 确保开始时间有值
            var validStartTime = startTime == default ? DateTime.Now : startTime;

            // 创建任务日志实体
            var jobLog = new QuartzJobLog
            {
                QuartzId = quartzId,
                JobName = jobName,
                JobGroup = jobGroup,
                TriggerName = triggerName,
                TriggerGroup = triggerGroup,
                StartTime = validStartTime,
                EndTime = endTime,
                ElapsedTime = validElapsedTime,
                ExecuteResult = executeResult,
                ErrorMessage = errorMessage,
                JobParams = jobParams
            };

            _appLog.Debug("[LogDatabaseWriter] 任务日志对象已创建，准备保存到数据库: 任务名称={JobName}, 任务组={JobGroup}, 耗时={ElapsedTime}ms, 结果={ExecuteResult}", 
                jobName, jobGroup, validElapsedTime, executeResult);

            // 使用 CreateAsync 保存任务日志（与 OperLog 和 DiffLog 保持一致）
            var result = await _quartzJobLogRepository.CreateAsync(jobLog);
            
            _appLog.Debug("[LogDatabaseWriter] CreateAsync 调用完成，返回结果: {Result}", result);
            
            // 验证保存结果（与 DiffLog 保持一致的验证逻辑）
            if (result <= 0)
            {
                _appLog.Warning("[LogDatabaseWriter] ⚠️ 保存任务日志失败: 任务名称={JobName}, 任务组={JobGroup}, 影响行数={Result}", 
                    jobName, jobGroup, result);
            }
            else
            {
                _appLog.Information("[LogDatabaseWriter] ✅ 任务日志保存成功: 任务名称={JobName}, 任务组={JobGroup}, 影响行数={Result}, ID={Id}", 
                    jobName, jobGroup, result, jobLog.Id);
            }
        }
        catch (Exception ex)
        {
            // 记录详细错误信息，包括堆栈跟踪（与 DiffLog 保持一致的错误处理逻辑）
            _appLog.Error(ex, "[LogDatabaseWriter] ❌ 保存任务日志失败: 任务名称={JobName}, 任务组={JobGroup}, 错误={ErrorMessage}, 堆栈={StackTrace}", 
                jobName ?? "Unknown", jobGroup ?? "Unknown", ex.Message, ex.StackTrace ?? "Unknown");
            // 不重新抛出异常，避免影响任务执行流程
        }
    }
}

