// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Jobs
// 文件名称：GenericServiceJob.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：通用服务任务 - 通过 JobParams 配置要执行的服务和方法
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using Takt.Common.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Takt.Infrastructure.Jobs;

/// <summary>
/// 通用服务任务
/// 通过 JobParams 配置要执行的服务接口名和方法名，实现动态调用
/// 
/// 严格按照 QuartzJob 实体实现：
/// 1. 从 context.JobDetail.JobDataMap.GetString("JobParams") 获取配置（由 QuartzSchedulerManager 从数据库的 QuartzJob.JobParams 字段设置）
/// 2. 解析 JSON 配置，获取 ServiceType、MethodName、Parameters
/// 3. 通过依赖注入获取服务实例，动态调用指定方法
/// 
/// JobParams JSON 格式示例：
/// {
///   "ServiceType": "Takt.Application.Services.Logging.ILogCleanupService, Takt.Application",
///   "MethodName": "CleanupOldLogsAsync",
///   "Parameters": {
///     "retentionDays": 7
///   }
/// }
/// </summary>
public class GenericServiceJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLogManager _appLog;

    public GenericServiceJob(
        IServiceProvider serviceProvider,
        AppLogManager appLog)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 执行任务
    /// 
    /// 执行流程：
    /// 1. 从 Quartz JobDataMap 获取 JobParams（由 QuartzSchedulerManager 从数据库 QuartzJob.JobParams 字段设置）
    /// 2. 解析 JSON 配置，获取 ServiceType、MethodName、Parameters
    /// 3. 通过反射获取服务类型，从依赖注入容器获取服务实例
    /// 4. 动态调用指定的方法，支持异步方法
    /// 5. 记录执行日志和错误信息
    /// </summary>
    /// <param name="context">任务执行上下文</param>
    public async Task Execute(IJobExecutionContext context)
    {
        var startTime = DateTime.Now;
        var jobKey = context.JobDetail.Key;
        _appLog.Information("[GenericServiceJob] 开始执行通用服务任务：{JobName} ({JobGroup})，执行时间：{StartTime}",
            jobKey.Name, jobKey.Group, startTime);

        try
        {
            // 获取任务参数
            var jobParamsJson = context.JobDetail.JobDataMap.GetString("JobParams");
            if (string.IsNullOrWhiteSpace(jobParamsJson))
            {
                throw new JobExecutionException("JobParams 参数为空，无法执行任务");
            }

            // 解析任务参数
            var jobParams = JsonConvert.DeserializeObject<GenericJobParams>(jobParamsJson, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });
            
            // 确保 Parameters 字典中的值如果是 JToken，转换为实际对象
            if (jobParams?.Parameters != null)
            {
                var processedParams = new Dictionary<string, object?>();
                foreach (var kvp in jobParams.Parameters)
                {
                    if (kvp.Value is JToken token)
                    {
                        processedParams[kvp.Key] = token.ToObject<object>();
                    }
                    else
                    {
                        processedParams[kvp.Key] = kvp.Value;
                    }
                }
                jobParams.Parameters = processedParams;
            }
            if (jobParams == null)
            {
                throw new JobExecutionException("JobParams 参数解析失败");
            }

            if (string.IsNullOrWhiteSpace(jobParams.ServiceType))
            {
                throw new JobExecutionException("ServiceType 不能为空");
            }

            if (string.IsNullOrWhiteSpace(jobParams.MethodName))
            {
                throw new JobExecutionException("MethodName 不能为空");
            }

            // 获取服务类型
            var serviceType = Type.GetType(jobParams.ServiceType);
            if (serviceType == null)
            {
                throw new JobExecutionException($"无法找到服务类型：{jobParams.ServiceType}");
            }

            // 从服务容器获取服务实例
            var serviceInstance = _serviceProvider.GetService(serviceType);
            if (serviceInstance == null)
            {
                throw new JobExecutionException($"无法从服务容器获取服务实例：{jobParams.ServiceType}");
            }

            // 获取方法信息（支持方法重载，根据参数名称匹配）
            System.Reflection.MethodInfo? methodInfo = null;
            
            var methods = serviceType.GetMethods()
                .Where(m => m.Name == jobParams.MethodName)
                .ToList();

            if (methods.Count == 0)
            {
                throw new JobExecutionException($"服务类型 {serviceType.Name} 中找不到方法：{jobParams.MethodName}");
            }
            
            if (methods.Count == 1)
            {
                methodInfo = methods[0];
            }
            else
            {
                // 如果有多个重载方法，根据参数名称匹配
                if (jobParams.Parameters != null && jobParams.Parameters.Any())
                {
                    var paramNames = jobParams.Parameters.Keys.ToList();
                    methodInfo = methods.FirstOrDefault(m =>
                    {
                        var methodParams = m.GetParameters();
                        return methodParams.Length == paramNames.Count &&
                               methodParams.All(p => paramNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                    });
                }
                else
                {
                    // 没有参数，查找无参数的方法
                    methodInfo = methods.FirstOrDefault(m => m.GetParameters().Length == 0);
                }

                // 如果还是找不到，使用第一个匹配名称的方法
                methodInfo ??= methods.FirstOrDefault();
            }

            // 确保 methodInfo 不为空
            if (methodInfo == null)
            {
                throw new JobExecutionException($"无法确定要调用的方法：{jobParams.MethodName}");
            }

            // 准备方法参数
            object?[]? methodArgs = null;
            if (jobParams.Parameters != null && jobParams.Parameters.Any())
            {
                var paramInfos = methodInfo.GetParameters();
                methodArgs = new object?[paramInfos.Length];

                for (int i = 0; i < paramInfos.Length; i++)
                {
                    var paramInfo = paramInfos[i];
                    if (jobParams.Parameters.TryGetValue(paramInfo.Name!, out var paramValue))
                    {
                        // 转换参数类型
                        methodArgs[i] = ConvertParameter(paramValue, paramInfo.ParameterType);
                    }
                    else if (!paramInfo.HasDefaultValue)
                    {
                        throw new JobExecutionException($"缺少必需参数：{paramInfo.Name}");
                    }
                }
            }

            // 调用方法
            var result = methodInfo.Invoke(serviceInstance, methodArgs);

            // 如果方法是异步的，等待执行完成
            if (result is Task task)
            {
                await task;
            }

            var endTime = DateTime.Now;
            var elapsedTime = (int)(endTime - startTime).TotalMilliseconds;
            _appLog.Information("[GenericServiceJob] 通用服务任务执行完成：{JobName} ({JobGroup})，执行时间：{EndTime}，耗时：{ElapsedTime} 毫秒",
                jobKey.Name, jobKey.Group, endTime, elapsedTime);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[GenericServiceJob] 通用服务任务执行失败：{JobName} ({JobGroup})", jobKey.Name, jobKey.Group);
            throw new JobExecutionException($"通用服务任务执行失败：{ex.Message}", ex, false);
        }
    }

    /// <summary>
    /// 转换参数类型
    /// </summary>
    private object? ConvertParameter(object? value, Type targetType)
    {
        if (value == null)
        {
            return null;
        }

        // 如果类型匹配，直接返回
        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        // 如果是 JToken，尝试转换
        if (value is JToken jToken)
        {
            try
            {
                // 如果目标类型是简单类型，直接转换
                if (targetType == typeof(int) && jToken.Type == JTokenType.Integer)
                {
                    return jToken.ToObject<int>();
                }
                else if (targetType == typeof(long) && jToken.Type == JTokenType.Integer)
                {
                    return jToken.ToObject<long>();
                }
                else if (targetType == typeof(double) && jToken.Type == JTokenType.Float)
                {
                    return jToken.ToObject<double>();
                }
                else if (targetType == typeof(decimal) && (jToken.Type == JTokenType.Float || jToken.Type == JTokenType.Integer))
                {
                    return jToken.ToObject<decimal>();
                }
                else if (targetType == typeof(string) && jToken.Type == JTokenType.String)
                {
                    return jToken.ToString();
                }
                else if (targetType == typeof(bool) && (jToken.Type == JTokenType.Boolean))
                {
                    return jToken.ToObject<bool>();
                }
                else if (targetType == typeof(DateTime) && jToken.Type == JTokenType.Date)
                {
                    return jToken.ToObject<DateTime>();
                }
                
                // 尝试反序列化为目标类型
                return jToken.ToObject(targetType);
            }
            catch
            {
                // 如果转换失败，尝试直接获取值
                if (jToken.Type == JTokenType.Integer)
                {
                    try { return jToken.ToObject<int>(); } catch { }
                    try { return jToken.ToObject<long>(); } catch { }
                    try { return jToken.ToObject<double>(); } catch { }
                    try { return jToken.ToObject<decimal>(); } catch { }
                    return null;
                }
                
                return jToken.Type switch
                {
                    JTokenType.String => jToken.ToString(),
                    JTokenType.Boolean => jToken.ToObject<bool>(),
                    _ => null
                };
            }
        }
        
        // 如果是字符串且是 JSON 格式，尝试解析
        if (value is string jsonString)
        {
            try
            {
                var token = JToken.Parse(jsonString);
                return token.ToObject(targetType);
            }
            catch
            {
                // 不是 JSON 格式，继续后续处理
            }
        }

        // 尝试类型转换
        if (targetType.IsEnum && value is string enumString)
        {
            return Enum.Parse(targetType, enumString);
        }

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            // 如果转换失败，返回原值
            return value;
        }
    }

    /// <summary>
    /// 通用任务参数模型
    /// </summary>
    private class GenericJobParams
    {
        /// <summary>
        /// 服务类型（完整类名，包含命名空间和程序集）
        /// 示例：Takt.Application.Services.Logging.ILogCleanupService, Takt.Application
        /// </summary>
        public string ServiceType { get; set; } = string.Empty;

        /// <summary>
        /// 方法名
        /// 示例：CleanupOldLogsAsync
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// 方法参数（字典，key 为参数名，value 为参数值）
        /// 示例：{ "retentionDays": 7 }
        /// </summary>
        public Dictionary<string, object?>? Parameters { get; set; }
    }
}

