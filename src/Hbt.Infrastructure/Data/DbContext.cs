// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：DbContext.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：数据库上下文
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

using Hbt.Common.Config;
using Serilog;
using SqlSugar;

namespace Hbt.Infrastructure.Data;

/// <summary>
/// 数据库上下文（单例模式，适合桌面应用）
/// </summary>
/// <remarks>
/// 使用 SqlSugarScope 实现单例模式，自动处理线程安全
/// 参考：https://www.donet5.com/home/doc?masterId=1&typeId=1181
/// </remarks>
public class DbContext
{
    private readonly SqlSugarScope _db;
    private readonly ILogger _logger;
    private readonly HbtDatabaseSettings _settings;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="settings">数据库配置</param>
    public DbContext(string connectionString, ILogger logger, HbtDatabaseSettings settings)
    {
        _logger = logger;
        _settings = settings;
        
        // SqlSugarScope：单例模式，自动处理线程安全（适合桌面应用）
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,  // 自动释放连接（官方推荐）
            InitKeyType = InitKeyType.Attribute
        });

        // 配置AOP（雪花ID、审计日志、差异日志）
        SqlSugarAop.ConfigureAop(_db, logger, settings);
    }
    
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
            _logger.Information("开始检查数据库...");
            _db.DbMaintenance.CreateDatabase();
            _logger.Information("✅ 数据库检查完成（自动创建/已存在）");
        }
        catch (Exception ex)
        {
            _logger.Information("数据库已存在或创建失败：{Message}", ex.Message);
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
            _logger.Information("开始创建数据表，共 {Count} 个实体", entityTypes.Length);
            _db.CodeFirst.InitTables(entityTypes);
            _logger.Information("✅ 数据表创建完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "数据表创建失败");
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
        _logger.Information("开启数据库事务...");
        _db.Ado.BeginTran();
        _logger.Information("✅ 事务已开启");
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public void CommitTransaction()
    {
        _db.Ado.CommitTran();
        _logger.Information("✅ 事务已提交");
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public void RollbackTransaction()
    {
        _db.Ado.RollbackTran();
        _logger.Warning("⚠️ 事务已回滚");
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
            _logger.Error(ex, "事务执行失败，回滚");
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
            _logger.Information("✅ 事务已开启");
            await asyncAction();
        });

        if (result.IsSuccess)
        {
            _logger.Information("✅ 事务提交成功");
            return true;
        }
        else
        {
            _logger.Error("❌ 事务执行失败：{ErrorMessage}", result.ErrorMessage);
            _logger.Error("❌ 异常信息：{Exception}", result.ErrorException?.Message);
            return false;
        }
    }

    #endregion
}
