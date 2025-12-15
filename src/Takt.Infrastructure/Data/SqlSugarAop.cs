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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SqlSugar;
using System.Diagnostics;
using Takt.Common.Config;
using Takt.Common.Logging;

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
    private static AppLogManager? _appLog;
    private static bool _isDiffLogEnabled = true;  // 差异日志启用标志，启动时可以临时禁用
    private static Func<ISqlSugarClient>? _getDbFunc; // 获取 DbContext 的 Db 实例的委托，确保使用同一个实例

    /// <summary>
    /// 设置日志数据库写入器（用于后续设置，避免循环依赖）
    /// </summary>
    public static void SetLogDatabaseWriter(ILogDatabaseWriter? logDatabaseWriter)
    {
        _logDatabaseWriter = logDatabaseWriter;
    }

    /// <summary>
    /// 设置应用程序日志管理器（用于统一日志处理）
    /// </summary>
    public static void SetAppLogManager(AppLogManager? appLog)
    {
        _appLog = appLog;
        // 立即测试日志是否正常工作
        if (_appLog != null)
        {
            _appLog.Information("✅ SqlSugarAop._appLog 已成功设置，日志将记录到 app-.txt 文件");
            WriteDiagnosticLog("✅ [SqlSugarAop] SetAppLogManager: _appLog 已成功设置");
            Debug.WriteLine("✅ [SqlSugarAop] SetAppLogManager: _appLog 已成功设置");
        }
        else
        {
            WriteDiagnosticLog("⚠️ [SqlSugarAop] SetAppLogManager: _appLog 为 null，差异日志将无法记录到日志文件");
            Debug.WriteLine("⚠️ [SqlSugarAop] SetAppLogManager: _appLog 为 null，差异日志将无法记录到日志文件");
        }
    }

    /// <summary>
    /// 写入诊断日志到文件
    /// </summary>
    private static void WriteDiagnosticLog(string message)
    {
        try
        {
            var logDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();
            var logFile = Path.Combine(logDir, "diagnostic.log");
            var now = DateTime.Now;
            var logMessage = $"{now:yyyy-MM-dd HH:mm:ss.fff zzz} [DBG] {message}\r\n";
            File.AppendAllText(logFile, logMessage);
            // 同时输出到 Debug，确保与 TimestampedDebug 一致
            System.Diagnostics.Debug.WriteLine(message);
        }
        catch
        {
            // 忽略文件写入错误
        }
    }

    /// <summary>
    /// 启用或禁用差异日志（用于启动时临时禁用，避免连接冲突）
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public static void SetDiffLogEnabled(bool enabled)
    {
        var oldValue = _isDiffLogEnabled;
        _isDiffLogEnabled = enabled;
        WriteDiagnosticLog($"🟢 [SetDiffLogEnabled] 差异日志标志已更改: {oldValue} -> {enabled}，调用堆栈: {Environment.StackTrace?.Split('\n').Take(3).LastOrDefault() ?? "Unknown"}");
        Debug.WriteLine($"🟢 [SetDiffLogEnabled] 差异日志标志已更改: {oldValue} -> {enabled}");
        _appLog?.Information("🟢 [SetDiffLogEnabled] 差异日志标志已更改: {0} -> {1}", oldValue, enabled);
    }

    /// <summary>
    /// 配置SqlSugar AOP
    /// </summary>
    /// <param name="db">数据库客户端实例（来自 DbContext.Db）</param>
    /// <param name="getDbFunc">获取 DbContext 的 Db 实例的委托，确保 CompleteUpdateableFunc 中使用同一个实例</param>
    public static void ConfigureAop(ISqlSugarClient db, Func<ISqlSugarClient> getDbFunc, ILogger? logger, HbtDatabaseSettings settings, ILogDatabaseWriter? logDatabaseWriter = null!, AppLogManager? appLog = null)
    {
        // 立即输出诊断信息，确认 ConfigureAop 被调用
        WriteDiagnosticLog("🔵 [SqlSugarAop] ConfigureAop 方法被调用");
        WriteDiagnosticLog($"🔵 [SqlSugarAop] appLog 参数: {(appLog != null ? "不为 null" : "为 null")}");
        WriteDiagnosticLog($"🔵 [SqlSugarAop] _appLog 静态字段: {(_appLog != null ? "不为 null" : "为 null")}");
        WriteDiagnosticLog($"🔵 [SqlSugarAop] EnableDiffLog: {settings.EnableDiffLog}");

        Debug.WriteLine("🔵 [SqlSugarAop] ConfigureAop 方法被调用");
        Debug.WriteLine($"🔵 [SqlSugarAop] appLog 参数: {(appLog != null ? "不为 null" : "为 null")}");
        Debug.WriteLine($"🔵 [SqlSugarAop] _appLog 静态字段: {(_appLog != null ? "不为 null" : "为 null")}");
        Debug.WriteLine($"🔵 [SqlSugarAop] EnableDiffLog: {settings.EnableDiffLog}");

        // 保存获取 DbContext 的 Db 实例的委托，确保 CompleteUpdateableFunc 中使用同一个实例
        _getDbFunc = getDbFunc ?? throw new ArgumentNullException(nameof(getDbFunc), "getDbFunc 不能为 null，必须提供获取 DbContext.Db 的委托");

        // 保存应用程序日志管理器引用（如果提供）
        if (appLog != null)
        {
            _appLog = appLog;
            WriteDiagnosticLog("🔵 [SqlSugarAop] _appLog 已从参数设置");
            Debug.WriteLine("🔵 [SqlSugarAop] _appLog 已从参数设置");
            _appLog.Information("🔵 [SqlSugarAop] ConfigureAop 方法被调用，开始配置 AOP");
        }
        else
        {
            WriteDiagnosticLog("⚠️ [SqlSugarAop] appLog 参数为 null，尝试使用静态字段 _appLog");
            Debug.WriteLine("⚠️ [SqlSugarAop] appLog 参数为 null，尝试使用静态字段 _appLog");
            if (_appLog != null)
            {
                _appLog.Information("🔵 [SqlSugarAop] ConfigureAop 方法被调用（使用静态字段 _appLog），开始配置 AOP");
            }
            else
            {
                WriteDiagnosticLog("❌ [SqlSugarAop] appLog 参数和静态字段 _appLog 都为 null，无法记录日志");
                Debug.WriteLine("❌ [SqlSugarAop] appLog 参数和静态字段 _appLog 都为 null，无法记录日志");
            }
        }

        // 配置雪花ID WorkId（仅设置，不处理生成）
        if (settings.EnableSnowflakeId)
        {
            SnowFlakeSingle.WorkId = settings.SnowflakeWorkerId;
            _appLog?.Information("雪花ID配置完成，WorkId: {0}", settings.SnowflakeWorkerId);
        }

        // 保存日志数据库写入器引用（如果提供）
        if (logDatabaseWriter != null)
        {
            _logDatabaseWriter = logDatabaseWriter;
            WriteDiagnosticLog("🔵 [SqlSugarAop] _logDatabaseWriter 已从参数设置");
            Debug.WriteLine("🔵 [SqlSugarAop] _logDatabaseWriter 已从参数设置");
        }
        else
        {
            WriteDiagnosticLog("⚠️ [SqlSugarAop] logDatabaseWriter 参数为 null，_logDatabaseWriter 仍为 null");
            Debug.WriteLine("⚠️ [SqlSugarAop] logDatabaseWriter 参数为 null，_logDatabaseWriter 仍为 null");
        }

        // 配置差异日志
        if (settings.EnableDiffLog)
        {
            WriteDiagnosticLog($"🔵 [SqlSugarAop] 开始配置差异日志，_appLog: {(_appLog != null ? "不为 null" : "为 null")}");
            Debug.WriteLine($"🔵 [SqlSugarAop] 开始配置差异日志，_appLog: {(_appLog != null ? "不为 null" : "为 null")}");
            if (_appLog == null)
            {
                WriteDiagnosticLog("⚠️ [SqlSugarAop] 差异日志已启用，但 _appLog 为 null，差异日志将无法记录到日志文件");
                Debug.WriteLine("⚠️ [SqlSugarAop] 差异日志已启用，但 _appLog 为 null，差异日志将无法记录到日志文件");
            }
            ConfigureDiffLog(db);
            WriteDiagnosticLog("🔵 [SqlSugarAop] 差异日志配置完成");
            Debug.WriteLine("🔵 [SqlSugarAop] 差异日志配置完成");
        }
        else
        {
            WriteDiagnosticLog("🔵 [SqlSugarAop] 差异日志已禁用，跳过配置");
            Debug.WriteLine("🔵 [SqlSugarAop] 差异日志已禁用，跳过配置");
        }

        // 配置SQL执行日志
        if (settings.EnableSqlLog && _appLog != null)
        {
            ConfigureSqlLog(db, settings);
        }
    }

    /// <summary>
    /// 配置差异日志（严格按照SqlSugar官方文档）
    /// </summary>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/doc?masterId=1&typeId=1204
    /// </remarks>
    private static void ConfigureDiffLog(ISqlSugarClient db)
    {
        // 注册差异日志事件
        var dbHashCode = db.GetHashCode();
        WriteDiagnosticLog($"🔵 [ConfigureDiffLog] 注册 OnDiffLogEvent 到 db 实例，哈希值: {dbHashCode}");
        Debug.WriteLine($"🔵 [ConfigureDiffLog] 注册 OnDiffLogEvent 到 db 实例，哈希值: {dbHashCode}");
        _appLog?.Information("🔵 [ConfigureDiffLog] 注册 OnDiffLogEvent 到 db 实例，哈希值: {0}", dbHashCode);

        // 创建差异日志事件处理器并保存为静态委托
        // 直接使用 lambda 表达式，让编译器推断类型
        db.Aop.OnDiffLogEvent = (diffLog) =>
        {
            // 立即记录日志，确认事件被触发（即使后续会 return）
            WriteDiagnosticLog("🟠 [OnDiffLogEvent] 差异日志事件被触发！");
            Debug.WriteLine("🟠 [OnDiffLogEvent] 差异日志事件被触发！");
            _appLog?.Information("🟠 [OnDiffLogEvent] 差异日志事件被触发！");

            try
            {
                // 如果差异日志被禁用，直接返回
                if (!_isDiffLogEnabled)
                {
                    WriteDiagnosticLog("🟠 [OnDiffLogEvent] 差异日志已禁用，跳过处理");
                    Debug.WriteLine("🟠 [OnDiffLogEvent] 差异日志已禁用，跳过处理");
                    return;
                }

                if (diffLog == null)
                {
                    WriteDiagnosticLog("🟠 [OnDiffLogEvent] diffLog 为 null，跳过处理");
                    Debug.WriteLine("🟠 [OnDiffLogEvent] diffLog 为 null，跳过处理");
                    return;
                }
                // 解析表名
                string tableName = "Unknown";
                if (!string.IsNullOrEmpty(diffLog.Sql))
                {
                    var sqlUpper = diffLog.Sql.ToUpper().Trim();
                    if (sqlUpper.StartsWith("UPDATE "))
                    {
                        var parts = diffLog.Sql.Substring(7).Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            tableName = parts[0].Trim('[', ']', '`', '"');
                        }
                    }
                    else if (sqlUpper.StartsWith("INSERT INTO "))
                    {
                        var parts = diffLog.Sql.Substring(12).Split(new[] { ' ', '\t', '\n', '\r', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            tableName = parts[0].Trim('[', ']', '`', '"');
                        }
                    }
                    else if (sqlUpper.StartsWith("DELETE FROM "))
                    {
                        var parts = diffLog.Sql.Substring(12).Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            tableName = parts[0].Trim('[', ']', '`', '"');
                        }
                    }
                }

                // 跳过日志表本身的操作
                var tableNameLower = tableName.ToLower();
                if (tableNameLower == "takt_logging_diff_log" ||
                    tableNameLower == "takt_logging_oper_log" ||
                    tableNameLower == "takt_logging_login_log")
                {
                    return;
                }

                // 准备差异日志数据
                var diffType = diffLog.DiffType.ToString();

                // 格式化 BeforeData 和 AfterData，转换为易读的键值对格式（已自动脱敏）
                var beforeData = FormatDiffData(diffLog.BeforeData);
                var afterData = FormatDiffData(diffLog.AfterData);

                var sql = diffLog.Sql;
                var parameters = diffLog.Parameters != null ? JsonConvert.SerializeObject(diffLog.Parameters) : null;
                // 计算执行耗时（毫秒），确保精度和范围正确
                var elapsedTime = diffLog.Time?.TotalMilliseconds ?? 0;
                // 确保非负数，并四舍五入到最近的整数
                elapsedTime = Math.Max(0, Math.Round(elapsedTime));
                var elapsedTimeInt = elapsedTime > int.MaxValue ? int.MaxValue : (int)elapsedTime;

                // 获取当前用户名和IP地址
                string? username = "Takt365";
                string? ipAddress = null;
                try
                {
                    var userContext = Takt.Common.Context.UserContext.Current;
                    if (userContext?.IsAuthenticated == true && !string.IsNullOrEmpty(userContext.Username))
                    {
                        username = userContext.Username;
                    }
                    ipAddress = Takt.Common.Helpers.SystemInfoHelper.GetLocalIpAddress();
                }
                catch
                {
                }

                // 异步保存到数据库
                if (_logDatabaseWriter != null)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                            if (_logDatabaseWriter != null)
                            {
                                await _logDatabaseWriter.SaveDiffLogAsync(
                                    tableName ?? "Unknown",
                                    diffType ?? "Unknown",
                                    beforeData,
                                    afterData,
                                    sql,
                                    parameters,
                                    elapsedTimeInt,
                                    username ?? "Takt365",
                                    ipAddress
                                ).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            _appLog?.Error(ex, "保存差异日志失败：表={0}, 操作={1}", tableName ?? "Unknown", diffType ?? "Unknown");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _appLog?.Error(ex, "处理差异日志失败: {0}", ex.Message);
            }
        };

        // 注意：不再保存事件处理器为静态委托
        // 改为在 CompleteUpdateableFunc 中直接获取 Updateable 使用的 ISqlSugarClient 实例并注册事件

        // 配置全局 Complete 函数，自动启用差异日志
        WriteDiagnosticLog("🔵 [ConfigureDiffLog] 准备设置 StaticConfig.CompleteUpdateableFunc");
        Debug.WriteLine("🔵 [ConfigureDiffLog] 准备设置 StaticConfig.CompleteUpdateableFunc");
        _appLog?.Information("🔵 [ConfigureDiffLog] 准备设置 StaticConfig.CompleteUpdateableFunc");

        StaticConfig.CompleteUpdateableFunc = it =>
        {
            var entityType = it.GetType().GetGenericArguments().FirstOrDefault();
            WriteDiagnosticLog($"🟡 [CompleteUpdateableFunc] 被调用，实体类型: {entityType?.Name ?? "Unknown"}, _isDiffLogEnabled: {_isDiffLogEnabled}");
            Debug.WriteLine($"🟡 [CompleteUpdateableFunc] 被调用，实体类型: {entityType?.Name ?? "Unknown"}, _isDiffLogEnabled: {_isDiffLogEnabled}");
            _appLog?.Information("🟡 [CompleteUpdateableFunc] 被调用，实体类型: {0}, _isDiffLogEnabled: {1}", entityType?.Name ?? "Unknown", _isDiffLogEnabled);

            if (!_isDiffLogEnabled) return;

            if (entityType != null)
            {
                if (entityType == typeof(Takt.Domain.Entities.Logging.DiffLog) ||
                    entityType == typeof(Takt.Domain.Entities.Logging.OperLog) ||
                    entityType == typeof(Takt.Domain.Entities.Logging.LoginLog))
                {
                    WriteDiagnosticLog($"🟡 [CompleteUpdateableFunc] 跳过日志表实体: {entityType.Name}");
                    Debug.WriteLine($"🟡 [CompleteUpdateableFunc] 跳过日志表实体: {entityType.Name}");
                    return;
                }
            }

            // 关键修复：统一使用 DbContext 的 Db 实例
            // 由于 DbContext 是单例，直接通过 _getDbFunc 获取 DbContext.Db 并注册事件
            // 这样确保事件注册在 BaseRepository 实际使用的实例上
            if (_getDbFunc == null)
            {
                WriteDiagnosticLog("❌ [CompleteUpdateableFunc] _getDbFunc 为 null，无法获取 DbContext.Db 实例");
                Debug.WriteLine("❌ [CompleteUpdateableFunc] _getDbFunc 为 null，无法获取 DbContext.Db 实例");
                return;
            }

            try
            {
                // 获取 DbContext 的 Db 实例（与 BaseRepository 使用同一个实例，因为 DbContext 是单例）
                var dbContextDb = _getDbFunc();
                if (dbContextDb == null)
                {
                    WriteDiagnosticLog("❌ [CompleteUpdateableFunc] DbContext.Db 为 null");
                    Debug.WriteLine("❌ [CompleteUpdateableFunc] DbContext.Db 为 null");
                    return;
                }

                var dbContextDbHash = dbContextDb.GetHashCode();
                WriteDiagnosticLog($"🟡 [CompleteUpdateableFunc] 获取到 DbContext.Db 实例，哈希: {dbContextDbHash}");
                Debug.WriteLine($"🟡 [CompleteUpdateableFunc] 获取到 DbContext.Db 实例，哈希: {dbContextDbHash}");

                // 直接在 DbContext.Db 实例上注册 OnDiffLogEvent（使用与 ConfigureDiffLog 中相同的处理器）
                // 由于 DbContext 是单例，BaseRepository 使用的 _dbContext.Db 就是这个实例
                dbContextDb.Aop.OnDiffLogEvent = (diffLog) =>
                {
                    // 立即记录日志，确认事件被触发（即使后续会 return）
                    WriteDiagnosticLog("🟠 [OnDiffLogEvent] 差异日志事件被触发！");
                    Debug.WriteLine("🟠 [OnDiffLogEvent] 差异日志事件被触发！");
                    _appLog?.Information("🟠 [OnDiffLogEvent] 差异日志事件被触发！");

                    try
                    {
                        // 如果差异日志被禁用，直接返回
                        if (!_isDiffLogEnabled)
                        {
                            WriteDiagnosticLog("🟠 [OnDiffLogEvent] 差异日志已禁用，跳过处理");
                            Debug.WriteLine("🟠 [OnDiffLogEvent] 差异日志已禁用，跳过处理");
                            return;
                        }

                        if (diffLog == null)
                        {
                            WriteDiagnosticLog("🟠 [OnDiffLogEvent] diffLog 为 null，跳过处理");
                            Debug.WriteLine("🟠 [OnDiffLogEvent] diffLog 为 null，跳过处理");
                            return;
                        }

                        // 解析表名
                        string tableName = "Unknown";
                        if (!string.IsNullOrEmpty(diffLog.Sql))
                        {
                            var sqlUpper = diffLog.Sql.ToUpper().Trim();
                            if (sqlUpper.StartsWith("UPDATE "))
                            {
                                var parts = diffLog.Sql.Substring(7).Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length > 0)
                                {
                                    tableName = parts[0].Trim('[', ']', '`', '"');
                                }
                            }
                            else if (sqlUpper.StartsWith("INSERT INTO "))
                            {
                                var parts = diffLog.Sql.Substring(12).Split(new[] { ' ', '\t', '\n', '\r', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length > 0)
                                {
                                    tableName = parts[0].Trim('[', ']', '`', '"');
                                }
                            }
                            else if (sqlUpper.StartsWith("DELETE FROM "))
                            {
                                var parts = diffLog.Sql.Substring(12).Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length > 0)
                                {
                                    tableName = parts[0].Trim('[', ']', '`', '"');
                                }
                            }
                        }

                        // 跳过日志表本身的操作
                        var tableNameLower = tableName.ToLower();
                        if (tableNameLower == "takt_logging_diff_log" ||
                            tableNameLower == "takt_logging_oper_log" ||
                            tableNameLower == "takt_logging_login_log")
                        {
                            return;
                        }

                        // 准备差异日志数据
                        var diffType = diffLog.DiffType.ToString();

                        // 格式化 BeforeData 和 AfterData，转换为易读的键值对格式（已自动脱敏）
                        var beforeData = FormatDiffData(diffLog.BeforeData);
                        var afterData = FormatDiffData(diffLog.AfterData);

                        var sql = diffLog.Sql;
                        var parameters = diffLog.Parameters != null ? JsonConvert.SerializeObject(diffLog.Parameters) : null;
                        // 计算执行耗时（毫秒），确保精度和范围正确
                        var elapsedTime = diffLog.Time?.TotalMilliseconds ?? 0;
                        // 确保非负数，并四舍五入到最近的整数
                        elapsedTime = Math.Max(0, Math.Round(elapsedTime));
                        var elapsedTimeInt = elapsedTime > int.MaxValue ? int.MaxValue : (int)elapsedTime;

                        // 获取当前用户名和IP地址
                        string? username = "Takt365";
                        string? ipAddress = null;
                        try
                        {
                            var userContext = Takt.Common.Context.UserContext.Current;
                            if (userContext?.IsAuthenticated == true && !string.IsNullOrEmpty(userContext.Username))
                            {
                                username = userContext.Username;
                            }
                            ipAddress = Takt.Common.Helpers.SystemInfoHelper.GetLocalIpAddress();
                        }
                        catch
                        {
                        }

                        // 异步保存到数据库
                        if (_logDatabaseWriter != null)
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await Task.Delay(500).ConfigureAwait(false);
                                    if (_logDatabaseWriter != null)
                                    {
                                        await _logDatabaseWriter.SaveDiffLogAsync(
                                            tableName ?? "Unknown",
                                            diffType ?? "Unknown",
                                            beforeData,
                                            afterData,
                                            sql,
                                            parameters,
                                            elapsedTimeInt,
                                            username ?? "Takt365",
                                            ipAddress
                                        ).ConfigureAwait(false);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _appLog?.Error(ex, "保存差异日志失败：表={0}, 操作={1}", tableName ?? "Unknown", diffType ?? "Unknown");
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _appLog?.Error(ex, "处理差异日志失败: {0}", ex.Message);
                    }
                };

                WriteDiagnosticLog($"✅ [CompleteUpdateableFunc] 已在 DbContext.Db 实例上注册 OnDiffLogEvent，实例哈希: {dbContextDbHash}");
                Debug.WriteLine($"✅ [CompleteUpdateableFunc] 已在 DbContext.Db 实例上注册 OnDiffLogEvent，实例哈希: {dbContextDbHash}");
            }
            catch (Exception ex)
            {
                WriteDiagnosticLog($"⚠️ [CompleteUpdateableFunc] 注册 OnDiffLogEvent 时出错: {ex.Message}");
                Debug.WriteLine($"⚠️ [CompleteUpdateableFunc] 注册 OnDiffLogEvent 时出错: {ex.Message}");
            }

            var method = it.GetType().GetMethod("EnableDiffLogEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                try
                {
                    method.Invoke(it, new object?[] { null });
                    WriteDiagnosticLog($"🟡 [CompleteUpdateableFunc] EnableDiffLogEvent 调用成功，实体类型: {entityType?.Name ?? "Unknown"}");
                    Debug.WriteLine($"🟡 [CompleteUpdateableFunc] EnableDiffLogEvent 调用成功，实体类型: {entityType?.Name ?? "Unknown"}");
                    _appLog?.Information("🟡 [CompleteUpdateableFunc] EnableDiffLogEvent 调用成功，实体类型: {0}", entityType?.Name ?? "Unknown");
                }
                catch (Exception ex)
                {
                    WriteDiagnosticLog($"❌ [CompleteUpdateableFunc] EnableDiffLogEvent 调用失败: {ex.Message}");
                    Debug.WriteLine($"❌ [CompleteUpdateableFunc] EnableDiffLogEvent 调用失败: {ex.Message}");
                    _appLog?.Error(ex, "❌ [CompleteUpdateableFunc] EnableDiffLogEvent 调用失败");
                }
            }
            else
            {
                WriteDiagnosticLog($"❌ [CompleteUpdateableFunc] 无法找到 EnableDiffLogEvent 方法");
                Debug.WriteLine($"❌ [CompleteUpdateableFunc] 无法找到 EnableDiffLogEvent 方法");
            }
        };

        StaticConfig.CompleteInsertableFunc = it =>
        {
            if (!_isDiffLogEnabled) return;

            var entityType = it.GetType().GetGenericArguments().FirstOrDefault();
            if (entityType != null)
            {
                if (entityType == typeof(Takt.Domain.Entities.Logging.DiffLog) ||
                    entityType == typeof(Takt.Domain.Entities.Logging.OperLog) ||
                    entityType == typeof(Takt.Domain.Entities.Logging.LoginLog))
                {
                    return;
                }
            }

            var method = it.GetType().GetMethod("EnableDiffLogEvent");
            method?.Invoke(it, new object?[] { null });
        };

        StaticConfig.CompleteDeleteableFunc = it =>
        {
            if (!_isDiffLogEnabled) return;

            var entityType = it.GetType().GetGenericArguments().FirstOrDefault();
            if (entityType != null)
            {
                if (entityType == typeof(Takt.Domain.Entities.Logging.DiffLog) ||
                    entityType == typeof(Takt.Domain.Entities.Logging.OperLog) ||
                    entityType == typeof(Takt.Domain.Entities.Logging.LoginLog))
                {
                    return;
                }
            }

            var method = it.GetType().GetMethod("EnableDiffLogEvent");
            method?.Invoke(it, new object?[] { null });
        };
    }

    /// <summary>
    /// 配置SQL执行日志（严格按照SqlSugar官方文档）
    /// </summary>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/doc?masterId=1&typeId=1204
    /// </remarks>
    private static void ConfigureSqlLog(ISqlSugarClient db, HbtDatabaseSettings settings)
    {
        // OnLogExecuting：SQL执行前事件
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            // 获取原生SQL（官方推荐，性能OK）
            var nativeSql = UtilMethods.GetNativeSql(sql, pars);
            _appLog?.Information("【SQL执行】{0}", nativeSql);
        };

        // OnLogExecuted：SQL执行完事件
        db.Aop.OnLogExecuted = (sql, pars) =>
        {
            try
            {
                _appLog?.Information("【SQL执行完成】");

                // 获取SQL执行时间
                var elapsed = db.Ado.SqlExecutionTime;
                _appLog?.Information("【SQL耗时】{0}ms", elapsed.TotalMilliseconds);

                // 慢查询警告
                if (elapsed.TotalMilliseconds > settings.SlowQueryThreshold)
                {
                    var nativeSql = UtilMethods.GetNativeSql(sql, pars);
                    _appLog?.Warning("【慢查询警告】耗时: {0}ms, 阈值: {1}ms",
                        elapsed.TotalMilliseconds, settings.SlowQueryThreshold);
                }
            }
            catch (Exception ex)
            {
                _appLog?.Error(ex, "OnLogExecuted 事件处理异常");
            }
        };

        // OnError：SQL报错事件
        db.Aop.OnError = (exp) =>
        {
            // 获取原生SQL（官方推荐）
            var nativeSql = exp.Parametres != null
                ? UtilMethods.GetNativeSql(exp.Sql, (SugarParameter[])exp.Parametres)
                : exp.Sql;
            _appLog?.Error("【SQL错误】{0}, SQL: {1}", exp.Message, nativeSql);
        };
    }

    /// <summary>
    /// 格式化差异数据，将 SqlSugar 的复杂结构转换为易读的键值对格式
    /// </summary>
    /// <param name="diffData">SqlSugar 差异数据对象</param>
    /// <returns>格式化后的 JSON 字符串，如果输入为 null 则返回 null</returns>
    private static string? FormatDiffData(object? diffData)
    {
        if (diffData == null)
            return null;

        try
        {
            // 先序列化为 JSON，然后解析为 JToken
            var jsonString = JsonConvert.SerializeObject(diffData);
            var token = JToken.Parse(jsonString);

            // 如果是数组格式（SqlSugar 的标准格式）
            if (token.Type == JTokenType.Array && token.Count() > 0)
            {
                var result = new Dictionary<string, object?>();

                foreach (var item in token)
                {
                    // 提取 Columns 数组
                    if (item["Columns"] is JArray columns)
                    {
                        foreach (var column in columns)
                        {
                            if (column["ColumnName"] != null)
                            {
                                var name = column["ColumnName"]!.ToString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    // 获取 Value，如果不存在或为 null，则使用 null
                                    var value = column["Value"];
                                    if (value != null)
                                    {
                                        // 处理不同类型的值
                                        if (value.Type == JTokenType.Null)
                                        {
                                            result[name] = null;
                                        }
                                        else if (value.Type == JTokenType.String)
                                        {
                                            var stringValue = value.ToString();
                                            // 使用统一的脱敏工具类进行脱敏处理
                                            result[name] = Takt.Common.Helpers.DataMaskingHelper.MaskSensitiveField(name, stringValue);
                                        }
                                        else if (value.Type == JTokenType.Integer)
                                        {
                                            result[name] = value.ToObject<long>();
                                        }
                                        else if (value.Type == JTokenType.Float)
                                        {
                                            result[name] = value.ToObject<double>();
                                        }
                                        else if (value.Type == JTokenType.Boolean)
                                        {
                                            result[name] = value.ToObject<bool>();
                                        }
                                        else
                                        {
                                            result[name] = value.ToString();
                                        }
                                    }
                                    else
                                    {
                                        result[name] = null;
                                    }
                                }
                            }
                        }
                    }
                }

                // 如果提取到了数据，序列化为易读的 JSON
                if (result.Count > 0)
                {
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.None,  // 不缩进，节省空间
                        StringEscapeHandling = StringEscapeHandling.Default
                    };
                    return JsonConvert.SerializeObject(result, settings);
                }
            }

            // 如果不是预期的格式，返回原始序列化结果
            return jsonString;
        }
        catch (Exception ex)
        {
            // 如果格式化失败，记录错误并返回原始序列化结果
            _appLog?.Warning("格式化差异数据失败: {0}, 使用原始数据", ex.Message);
            try
            {
                return JsonConvert.SerializeObject(diffData);
            }
            catch
            {
                return diffData.ToString();
            }
        }
    }

}

