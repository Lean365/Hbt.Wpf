//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : SqlSugarAop.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : SqlSugar AOP配置（审计日志、差异日志）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//===================================================================

using Takt.Common.Config;
using Takt.Common.Logging;
using Takt.Domain.Entities;
using Takt.Domain.Entities.Logging;
using Serilog;
using SqlSugar;
using System.Text.Json;

namespace Takt.Infrastructure.Data;

/// <summary>
/// SqlSugar AOP配置
/// </summary>
/// <remarks>
/// 统一配置审计字段、差异日志
/// 雪花ID由SqlSugar自动处理，无需AOP配置
/// </remarks>
public static class SqlSugarAop
{
    private static ILogDatabaseWriter? _logDatabaseWriter;

    /// <summary>
    /// 设置日志数据库写入器（用于后续设置，避免循环依赖）
    /// </summary>
    public static void SetLogDatabaseWriter(ILogDatabaseWriter? logDatabaseWriter)
    {
        _logDatabaseWriter = logDatabaseWriter;
    }

    /// <summary>
    /// 配置SqlSugar AOP
    /// </summary>
    public static void ConfigureAop(ISqlSugarClient db, ILogger? logger, HbtDatabaseSettings settings, ILogDatabaseWriter? logDatabaseWriter = null)
    {
        // 配置雪花ID WorkId（仅设置，不处理生成）
        if (settings.EnableSnowflakeId)
        {
            SnowFlakeSingle.WorkId = settings.SnowflakeWorkerId;
            logger?.Information("雪花ID配置完成，WorkId: {WorkId}", settings.SnowflakeWorkerId);
        }

        // 保存日志数据库写入器引用（如果提供）
        if (logDatabaseWriter != null)
        {
            _logDatabaseWriter = logDatabaseWriter;
        }

        // 配置差异日志
        if (settings.EnableDiffLog && logger != null)
        {
            ConfigureDiffLog(db, logger);
        }

        // 配置SQL执行日志
        if (settings.EnableSqlLog && logger != null)
        {
            ConfigureSqlLog(db, logger, settings);
        }
    }

    /// <summary>
    /// 配置差异日志（严格按照SqlSugar官方文档）
    /// </summary>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/doc?masterId=1&typeId=1204
    /// 差异日志需要在更新时调用 .EnableDiffLogEvent()
    /// 这里只配置差异日志的处理事件
    /// 通过异步方式保存到数据库，避免 DataReader 冲突
    /// </remarks>
    private static void ConfigureDiffLog(ISqlSugarClient db, ILogger logger)
    {
        // OnDiffLogEvent：差异日志事件
        db.Aop.OnDiffLogEvent = (diffLog) =>
        {
            try
            {
                // 创建差异日志实体
                var tableName = diffLog.BusinessData?.ToString() ?? "Unknown";
                var diffType = diffLog.DiffType.ToString();
                var businessData = diffLog.BusinessData?.ToString();
                var beforeData = diffLog.BeforeData != null ? JsonSerializer.Serialize(diffLog.BeforeData) : null;
                var afterData = diffLog.AfterData != null ? JsonSerializer.Serialize(diffLog.AfterData) : null;
                var sql = diffLog.Sql;
                var parameters = diffLog.Parameters != null ? JsonSerializer.Serialize(diffLog.Parameters) : null;
                var elapsedTime = (int)(diffLog.Time?.TotalMilliseconds ?? 0);

                // 记录到文件
                logger.Information("【数据差异日志】表:{TableName}, 操作:{DiffType}, 业务数据:{BusinessData}", 
                    tableName, diffType, businessData);

                // 异步保存到数据库（不阻塞，避免 DataReader 冲突）
                if (_logDatabaseWriter != null)
                {
                    // 使用 Task.Run 在后台线程执行，避免阻塞当前事务
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _logDatabaseWriter.SaveDiffLogAsync(
                                tableName,
                                diffType,
                                businessData,
                                beforeData,
                                afterData,
                                sql,
                                parameters,
                                elapsedTime,
                                "System",  // TODO: 从上下文获取用户名
                                null       // TODO: 从上下文获取IP地址
                            );
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "异步保存差异日志到数据库失败");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "记录差异日志失败");
            }
        };

        // 使用静态配置自动启用差异日志（官方推荐方式）
        StaticConfig.CompleteUpdateableFunc = it =>
        {
            // 自动为所有Updateable操作启用差异日志
            var method = it.GetType().GetMethod("EnableDiffLogEvent");
            if (method != null)
            {
                method.Invoke(it, new object?[] { null });
            }
        };
    }

    /// <summary>
    /// 配置SQL执行日志（严格按照SqlSugar官方文档）
    /// </summary>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/doc?masterId=1&typeId=1204
    /// </remarks>
    private static void ConfigureSqlLog(ISqlSugarClient db, ILogger logger, HbtDatabaseSettings settings)
    {
        // OnLogExecuting：SQL执行前事件
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            // 获取原生SQL（官方推荐，性能OK）
            var nativeSql = UtilMethods.GetNativeSql(sql, pars);
            logger.Information("【SQL执行】{Sql}", nativeSql);
        };

        // OnLogExecuted：SQL执行完事件
        db.Aop.OnLogExecuted = (sql, pars) =>
        {
            try
            {
                logger.Information("【SQL执行完成】");
                
                // 获取SQL执行时间
                var elapsed = db.Ado.SqlExecutionTime;
                logger.Information("【SQL耗时】{Elapsed}ms", elapsed.TotalMilliseconds);
                
                // 慢查询警告
                if (elapsed.TotalMilliseconds > settings.SlowQueryThreshold)
                {
                    var nativeSql = UtilMethods.GetNativeSql(sql, pars);
                    logger.Warning("【慢查询警告】耗时: {Elapsed}ms, 阈值: {Threshold}ms", 
                        elapsed.TotalMilliseconds, settings.SlowQueryThreshold);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "OnLogExecuted 事件处理异常");
            }
        };

        // OnError：SQL报错事件
        db.Aop.OnError = (exp) =>
        {
            // 获取原生SQL（官方推荐）
            var nativeSql = exp.Parametres != null 
                ? UtilMethods.GetNativeSql(exp.Sql, (SugarParameter[])exp.Parametres)
                : exp.Sql;
            logger.Error("【SQL错误】{Error}, SQL: {Sql}", exp.Message, nativeSql);
        };
    }
}

