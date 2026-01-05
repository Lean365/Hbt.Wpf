// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbContext.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：数据库上下文
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Config;
using Takt.Common.Logging;
using Serilog;
using SqlSugar;
using System.Diagnostics;

namespace Takt.Infrastructure.Data;

/// <summary>
/// 数据库上下文（单例模式，适合桌面应用）
/// </summary>
/// <remarks>
/// 使用 SqlSugarScope 实现单例模式，自动处理线程安全
/// 参考：https://www.donet5.com/home/doc?masterId=1&typeId=1181
/// 
/// 重要：此类型必须通过依赖注入容器（Autofac）创建，且注册为 SingleInstance
/// 构造函数中包含单例验证逻辑，确保全局只有一个实例
/// </remarks>
public class DbContext
{
    private static DbContext? _instance;
    private static readonly object _lock = new object();
    
    private readonly SqlSugarScope _db;
    private readonly ILogger _logger;  // 保留用于向后兼容，但实际使用 AppLogManager
    private readonly AppLogManager _appLog;
    private readonly HbtDatabaseSettings _settings;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="logger">日志记录器（保留用于向后兼容）</param>
    /// <param name="settings">数据库配置</param>
    /// <param name="logDatabaseWriter">日志数据库写入器（可选）</param>
    /// <param name="appLog">应用程序日志管理器（可选）</param>
    public DbContext(string connectionString, ILogger logger, HbtDatabaseSettings settings, ILogDatabaseWriter? logDatabaseWriter = null!, AppLogManager? appLog = null)
    {
        // 单例验证：确保全局只有一个 DbContext 实例
        lock (_lock)
        {
            if (_instance != null)
            {
                var errorMsg = $"❌ [DbContext] 检测到多个 DbContext 实例！当前实例哈希: {_instance.GetHashCode()}, 新实例哈希: {GetHashCode()}。DbContext 必须注册为 SingleInstance，只能有一个实例。";
                WriteDiagnosticLog(errorMsg);
                Debug.WriteLine(errorMsg);
                throw new InvalidOperationException("DbContext 必须注册为 SingleInstance，全局只能有一个实例。请检查 Autofac 注册配置，确保使用 .SingleInstance()。");
            }
            _instance = this;
            WriteDiagnosticLog($"✅ [DbContext] 单例实例已创建，实例哈希: {GetHashCode()}");
            Debug.WriteLine($"✅ [DbContext] 单例实例已创建，实例哈希: {GetHashCode()}");
        }
        
        Debug.WriteLine("🔴 [DbContext] 构造函数被调用");
        Debug.WriteLine($"🔴 [DbContext] appLog 参数: {(appLog != null ? "不为 null" : "为 null")}");
        
        // 同时写入文件，确保能看到
        WriteDiagnosticLog("🔴 [DbContext] 构造函数被调用");
        WriteDiagnosticLog($"🔴 [DbContext] appLog 参数: {(appLog != null ? "不为 null" : "为 null")}");
        
        _logger = logger;
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog), "AppLogManager 不能为 null");
        _settings = settings;
        
        Debug.WriteLine("🔴 [DbContext] 开始创建 SqlSugarScope");
        WriteDiagnosticLog("🔴 [DbContext] 开始创建 SqlSugarScope");
        
        // SqlSugarScope：单例模式，自动处理线程安全（适合桌面应用）
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,  // 自动释放连接（官方推荐）
            InitKeyType = InitKeyType.Attribute
        });

        Debug.WriteLine("🔴 [DbContext] SqlSugarScope 创建完成，准备调用 ConfigureAop");
        WriteDiagnosticLog("🔴 [DbContext] SqlSugarScope 创建完成，准备调用 ConfigureAop");
        
        // 配置AOP（雪花ID、审计日志、差异日志）
        // 传入 _db 和获取 _db 的委托，确保 CompleteUpdateableFunc 中使用同一个实例
        SqlSugarAop.ConfigureAop(_db, () => _db, logger, settings, logDatabaseWriter, appLog);
        
        Debug.WriteLine("🔴 [DbContext] ConfigureAop 调用完成");
        WriteDiagnosticLog("🔴 [DbContext] ConfigureAop 调用完成");
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
    /// 获取当前 DbContext 单例实例（如果已创建）
    /// </summary>
    /// <remarks>
    /// 此属性用于验证单例模式是否正确工作
    /// 正常情况下，应该通过依赖注入获取 DbContext 实例，而不是使用此属性
    /// </remarks>
    public static DbContext? Instance => _instance;
    
    /// <summary>
    /// SqlSugar数据库客户端（单例模式）
    /// </summary>
    public SqlSugarScope Db => _db;

    /// <summary>
    /// 是否启用雪花ID
    /// </summary>
    public bool EnableSnowflakeId => _settings.EnableSnowflakeId;

    /// <summary>
    /// 获取SqlSugar客户端
    /// </summary>
    /// <returns>SqlSugar客户端实例</returns>
    public ISqlSugarClient GetClient()
    {
        return _db;
    }

    #region 数据库初始化

    /// <summary>
    /// 确保数据库已创建
    /// </summary>
    /// <remarks>
    /// SqlSugar 官方方法：DbMaintenance.CreateDatabase()
    /// 参考：https://www.donet5.com/home/doc?masterId=1&typeId=1181
    /// </remarks>
    public void EnsureDatabaseCreated()
    {
        try
        {
            _appLog.Information("开始检查数据库..");
            _db.DbMaintenance.CreateDatabase();
            _appLog.Information("✅ 数据库检查完成（自动创建/已存在）");
        }
        catch (Exception ex)
        {
            _appLog.Information("数据库已存在或创建失败：{0}", ex.Message ?? "Unknown");
        }
    }

    /// <summary>
    /// 初始化数据表（CodeFirst）
    /// </summary>
    /// <param name="entityTypes">实体类型数组</param>
    public void InitializeTables(params Type[] entityTypes)
    {
        try
        {
            _appLog.Information("开始创建数据表，共 {0} 个实体", entityTypes.Length);
            _db.CodeFirst.InitTables(entityTypes);
            _appLog.Information("✅ 数据表创建完成");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "数据表创建失败");
            throw;
        }
    }

    #endregion

    #region 事务管理

    /// <summary>
    /// 开启事务
    /// </summary>
    public void BeginTransaction()
    {
        _appLog.Information("开启数据库事务..");
        _db.Ado.BeginTran();
        _appLog.Information("✅ 事务已开启");
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public void CommitTransaction()
    {
        _db.Ado.CommitTran();
        _appLog.Information("✅ 事务已提交");
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public void RollbackTransaction()
    {
        _db.Ado.RollbackTran();
        _appLog.Warning("⚠️ 事务已回滚");
    }

    /// <summary>
    /// 使用事务执行操作（同步）
    /// </summary>
    /// <param name="action">事务内的操作</param>
    public void UseTransaction(Action action)
    {
        try
        {
            BeginTransaction();
            action();
            CommitTransaction();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "事务执行失败，回滚");
            RollbackTransaction();
            throw;
        }
    }

    /// <summary>
    /// 使用事务执行操作（异步，SqlSugar官方方法）
    /// </summary>
    /// <param name="asyncAction">事务内的异步操作</param>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=1183 第7节
    /// </remarks>
    public async Task<bool> UseTransactionAsync(Func<Task> asyncAction)
    {
        var result = await _db.Ado.UseTranAsync(async () =>
        {
            _appLog.Information("✅ 事务已开启");
            await asyncAction();
        });

        if (result.IsSuccess)
        {
            _appLog.Information("✅ 事务提交成功");
            return true;
        }
        else
        {
            _appLog.Error("❌ 事务执行失败：{0}", result.ErrorMessage ?? "Unknown");
            _appLog.Error("❌ 异常信息：{0}", result.ErrorException?.Message ?? "Unknown");
            return false;
        }
    }

    #endregion

    #region 健康检查

    /// <summary>
    /// 检查数据库连接是否可用
    /// </summary>
    /// <returns>如果连接可用返回 true，否则返回 false</returns>
    public bool CheckConnection()
    {
        try
        {
            // 执行一个简单的查询来测试连接
            var result = _db.Ado.GetDataTable("SELECT 1");
            return result != null;
        }
        catch (Exception ex)
        {
            _appLog.Warning("数据库连接检查失败：{0}", ex.Message ?? "Unknown");
            return false;
        }
    }

    /// <summary>
    /// 异步检查数据库连接是否可用
    /// </summary>
    /// <returns>如果连接可用返回 true，否则返回 false</returns>
    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            // 执行一个简单的查询来测试连接
            await _db.Ado.GetDataTableAsync("SELECT 1");
            return true;
        }
        catch (Exception ex)
        {
            _appLog.Warning("数据库连接检查失败：{0}", ex.Message ?? "Unknown");
            return false;
        }
    }

    #endregion
}
