//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : SqlSugarAop.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-20
// 版本号 : 1.0
// 描述    : SqlSugar AOP配置（审计日志、差异日志）
//===================================================================

using Hbt.Common.Config;
using Hbt.Domain.Entities;
using Hbt.Domain.Entities.Logging;
using Serilog;
using SqlSugar;
using System.Text.Json;

namespace Hbt.Infrastructure.Data;

/// <summary>
/// SqlSugar AOP配置
/// </summary>
/// <remarks>
/// 统一配置审计字段、差异日志
/// 雪花ID由SqlSugar自动处理，无需AOP配置
/// </remarks>
public static class SqlSugarAop
{
    /// <summary>
    /// 配置SqlSugar AOP
    /// </summary>
    public static void ConfigureAop(ISqlSugarClient db, ILogger? logger, HbtDatabaseSettings settings)
    {
        // 配置雪花ID WorkId（仅设置，不处理生成）
        if (settings.EnableSnowflakeId)
        {
            SnowFlakeSingle.WorkId = settings.SnowflakeWorkerId;
            logger?.Information("雪花ID配置完成，WorkId: {WorkId}", settings.SnowflakeWorkerId);
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
    /// ⚠️ 注意：为避免 DataReader 冲突，只记录到日志文件，不写入数据库
    /// </remarks>
    private static void ConfigureDiffLog(ISqlSugarClient db, ILogger logger)
    {
        // OnDiffLogEvent：差异日志事件
        db.Aop.OnDiffLogEvent = (diffLog) =>
        {
            try
            {
                // 创建差异日志实体
                var diffLogEntity = new DiffLog
                {
                    TableName = diffLog.BusinessData?.ToString() ?? "Unknown",
                    DiffType = diffLog.DiffType.ToString(),
                    BusinessData = diffLog.BusinessData?.ToString(),
                    BeforeData = JsonSerializer.Serialize(diffLog.BeforeData),
                    AfterData = JsonSerializer.Serialize(diffLog.AfterData),
                    Sql = diffLog.Sql,
                    Parameters = JsonSerializer.Serialize(diffLog.Parameters),
                    ElapsedTime = (int)(diffLog.Time?.TotalMilliseconds ?? 0),
                    DiffTime = DateTime.Now,
                    Username = "System"  // TODO: 从上下文获取
                };

                // ⚠️ 只记录到日志文件，不写入数据库
                // 原因：在事务中执行 UPDATE 后，异步插入 DiffLog 会导致 DataReader 冲突
                // 如需写入数据库，应该在业务层手动记录，而不是在 AOP 中自动记录
                logger.Information("【数据差异日志】表:{TableName}, 操作:{DiffType}, 业务数据:{BusinessData}", 
                    diffLogEntity.TableName, diffLogEntity.DiffType, diffLogEntity.BusinessData);
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

