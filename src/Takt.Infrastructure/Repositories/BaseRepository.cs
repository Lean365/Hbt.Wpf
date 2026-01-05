// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.Repositories
// 文件名称：BaseRepository.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：通用仓储实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;
using System.Linq.Expressions;
using Takt.Common.Context;
using Takt.Common.Results;
using Takt.Domain.Entities;
using Takt.Domain.Repositories;
using Takt.Infrastructure.Data;

namespace Takt.Infrastructure.Repositories;

/// <summary>
/// 通用仓储实现
/// 实现所有实体的通用数据访问操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
{
    protected readonly DbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public BaseRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取查询对象
    /// </summary>
    /// <returns>查询对象</returns>
    public ISugarQueryable<TEntity> AsQueryable()
    {
        return _dbContext.Db.Queryable<TEntity>();
    }

    #region 事务管理（通过内部DbContext）

    /// <summary>
    /// 使用事务执行操作（同步）
    /// </summary>
    /// <param name="action">事务内的操作</param>
    protected void UseTransaction(Action action)
    {
        _dbContext.UseTransaction(action);
    }

    /// <summary>
    /// 使用事务执行操作（异步）
    /// </summary>
    /// <param name="asyncAction">事务内的异步操作</param>
    /// <returns>是否成功</returns>
    protected async Task<bool> UseTransactionAsync(Func<Task> asyncAction)
    {
        return await _dbContext.UseTransactionAsync(asyncAction);
    }

    #endregion

    #region 查询操作

    /// <summary>
    /// 获取分页列表
    /// </summary>
    /// <param name="condition">查询条件</param>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="orderByExpression">排序表达式</param>
    /// <param name="orderByType">排序类型</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResult<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? condition = null!, 
        int pageIndex = 1,
        int pageSize = 20,
        Expression<Func<TEntity, object>>? orderByExpression = null!, 
        OrderByType orderByType = OrderByType.Desc)
    {
        var query = _dbContext.Db.Queryable<TEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        if (orderByExpression != null)
        {
            query = orderByType == OrderByType.Asc
                ? query.OrderBy(orderByExpression)
                : query.OrderByDescending(orderByExpression);
        }

        // SqlSugar 的分页查询：先获取总数，再获取分页数据
        int totalNum = await query.CountAsync();
        var items = await query.ToPageListAsync(pageIndex, pageSize);

        return new PagedResult<TEntity>
        {
            Items = items,
            TotalNum = totalNum,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns>实体</returns>
    /// <remarks>
    /// 使用 SqlSugar 的 In 方法，自动识别主键字段
    /// 参考：https://www.donet5.com/home/Doc?typeId=1187
    /// </remarks>
    public async Task<TEntity?> GetByIdAsync(object id)
    {
        // SqlSugar 的 In 方法会自动识别主键字段，无需区分 BaseEntity
        return await _dbContext.Db.Queryable<TEntity>()
            .In(id)
            .FirstAsync();
    }

    /// <summary>
    /// 获取第一个符合条件的实体（异步）
    /// </summary>
    /// <param name="condition">查询条件</param>
    /// <returns>实体</returns>
    public async Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> condition)
    {
        return await _dbContext.Db.Queryable<TEntity>()
            .Where(condition)
            .FirstAsync();
    }

    /// <summary>
    /// 获取第一个符合条件的实体（同步，用于事务内避免死锁）
    /// </summary>
    /// <param name="condition">查询条件</param>
    /// <returns>实体</returns>
    public TEntity? GetFirst(Expression<Func<TEntity, bool>> condition)
    {
        return _dbContext.Db.Queryable<TEntity>()
            .Where(condition)
            .First();
    }

    /// <summary>
    /// 获取符合条件的实体数量
    /// </summary>
    /// <param name="condition">查询条件</param>
    /// <returns>数量</returns>
    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? condition = null)
    {
        var query = _dbContext.Db.Queryable<TEntity>();
        if (condition != null)
        {
            query = query.Where(condition);
        }
        return await query.CountAsync();
    }

    /// <summary>
    /// 获取最大值
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <param name="selector">字段选择器</param>
    /// <param name="condition">查询条件（可选）</param>
    /// <returns>最大值，如果没有数据则返回 null</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=1187
    /// </remarks>
    public async Task<TResult?> GetMaxAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct
    {
        var query = _dbContext.Db.Queryable<TEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query.MaxAsync(selector);
    }

    /// <summary>
    /// 获取最小值
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <param name="selector">字段选择器</param>
    /// <param name="condition">查询条件（可选）</param>
    /// <returns>最小值，如果没有数据则返回 null</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=1187
    /// </remarks>
    public async Task<TResult?> GetMinAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct
    {
        var query = _dbContext.Db.Queryable<TEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query.MinAsync(selector);
    }

    /// <summary>
    /// 求和
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <param name="selector">字段选择器</param>
    /// <param name="condition">查询条件（可选）</param>
    /// <returns>求和结果，如果没有数据则返回 null</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=1187
    /// </remarks>
    public async Task<TResult?> GetSumAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct
    {
        var query = _dbContext.Db.Queryable<TEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query.SumAsync(selector);
    }

    /// <summary>
    /// 获取平均值
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <param name="selector">字段选择器</param>
    /// <param name="condition">查询条件（可选）</param>
    /// <returns>平均值，如果没有数据则返回 null</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=1187
    /// </remarks>
    public async Task<TResult?> GetAverageAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct
    {
        var query = _dbContext.Db.Queryable<TEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query.AvgAsync(selector);
    }

    /// <summary>
    /// 获取中位数
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <param name="selector">字段选择器</param>
    /// <param name="condition">查询条件（可选）</param>
    /// <returns>中位数，如果没有数据则返回 null</returns>
    /// <remarks>
    /// 根据数据库类型自动选择相应的中位数函数：
    /// - SQL Server 2012+: PERCENTILE_CONT(0.5)
    /// - Oracle 10g+: MEDIAN()
    /// - PostgreSQL 9.4+: PERCENTILE_CONT(0.5)
    /// - MySQL/SQLite: 使用窗口函数手动计算
    /// </remarks>
    public async Task<TResult?> GetMedianAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct
    {
        // 获取数据库类型
        var dbType = _dbContext.Db.CurrentConnectionConfig.DbType;

        // 获取表名
        var tableName = _dbContext.Db.EntityMaintenance.GetTableName<TEntity>();

        // 获取字段名（从表达式中提取）
        if (selector.Body is not System.Linq.Expressions.MemberExpression memberExpression)
        {
            throw new ArgumentException("选择器必须是成员访问表达式", nameof(selector));
        }

        // 获取实体信息
        var entityInfo = _dbContext.Db.EntityMaintenance.GetEntityInfo<TEntity>();
        var columnInfo = entityInfo.Columns.FirstOrDefault(c => c.PropertyName == memberExpression.Member.Name);
        if (columnInfo is null)
        {
            throw new ArgumentException($"找不到属性 {memberExpression.Member.Name} 对应的数据库列", nameof(selector));
        }

        var columnName = columnInfo.DbColumnName;

        // 构建查询以获取 WHERE 条件和参数
        var query = _dbContext.Db.Queryable<TEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        // 获取 SQL 和参数
        var sqlInfo = query.ToSql();
        var whereSql = sqlInfo.Key; // ToSql() 返回 KeyValuePair<string, List<SugarParameter>>，Key 是 SQL
        var sqlParams = sqlInfo.Value; // Value 是参数列表

        // 从 SQL 中提取 WHERE 子句（去掉 SELECT 和 FROM 部分）
        var whereClause = string.Empty;
        if (!string.IsNullOrWhiteSpace(whereSql))
        {
            var whereIndex = whereSql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
            if (whereIndex >= 0)
            {
                whereClause = whereSql[whereIndex..];
            }
        }
        else if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            whereClause = "WHERE is_deleted = 0";
        }

        // 根据数据库类型构建中位数查询 SQL
        var medianSql = dbType switch
        {
            DbType.SqlServer => $@"
                    SELECT PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY [{columnName}]) AS Median
                    FROM [{tableName}]
                    {whereClause}",
            DbType.Oracle => $@"
                    SELECT MEDIAN([{columnName}]) AS Median
                    FROM [{tableName}]
                    {whereClause}",
            DbType.PostgreSQL => $@"
                    SELECT PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY ""{columnName}"") AS Median
                    FROM ""{tableName}""
                    {whereClause}",
            DbType.MySql or DbType.Sqlite => $@"
                    SELECT AVG([{columnName}]) AS Median
                    FROM (
                        SELECT [{columnName}],
                               ROW_NUMBER() OVER (ORDER BY [{columnName}]) AS row_num,
                               COUNT(*) OVER () AS total_rows
                        FROM [{tableName}]
                        {whereClause}
                    ) AS ordered_values
                    WHERE row_num IN (
                        FLOOR((total_rows + 1) / 2),
                        CEIL((total_rows + 1) / 2)
                    )",
            _ => $@"
                    SELECT AVG([{columnName}]) AS Median
                    FROM (
                        SELECT [{columnName}],
                               ROW_NUMBER() OVER (ORDER BY [{columnName}]) AS row_num,
                               COUNT(*) OVER () AS total_rows
                        FROM [{tableName}]
                        {whereClause}
                    ) AS ordered_values
                    WHERE row_num IN (
                        FLOOR((total_rows + 1) / 2),
                        CEIL((total_rows + 1) / 2)
                    )"
        };

        // 执行查询
        var result = await _dbContext.Db.Ado.SqlQuerySingleAsync<TResult?>(medianSql, sqlParams);
        return result;
    }

    #endregion

    #region 新增操作

    /// <summary>
    /// 新增实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>影响行数</returns>
    public async Task<int> CreateAsync(TEntity entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated
                ? UserContext.Current.Username
                : "Takt365";
            baseEntity.CreatedBy = currentUser;
            baseEntity.CreatedTime = DateTime.Now;
            baseEntity.UpdatedBy = currentUser;
            baseEntity.UpdatedTime = DateTime.Now;
            baseEntity.IsDeleted = 0;

            // 根据配置判断是否使用雪花ID
            if (_dbContext.EnableSnowflakeId)
            {
                // 使用SqlSugar标准方法：ExecuteReturnSnowflakeId() 自动生成雪花ID
                var snowflakeId = await _dbContext.Db.Insertable(entity).ExecuteReturnSnowflakeIdAsync();
                baseEntity.Id = snowflakeId;
                return 1;
            }
            else
            {
                // 不使用雪花ID时，使用自增ID，需要返回生成的ID并赋值
                var insertedId = await _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentityAsync();
                baseEntity.Id = insertedId;
                return 1;
            }
        }
        return await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 新增实体（指定用户名）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    public async Task<int> CreateAsync(TEntity entity, string? userName)
    {
        if (entity is BaseEntity baseEntity)
        {
            // 填充审计字段
            baseEntity.CreatedBy = userName;
            baseEntity.CreatedTime = DateTime.Now;
            baseEntity.UpdatedBy = userName;
            baseEntity.UpdatedTime = DateTime.Now;
            baseEntity.IsDeleted = 0;

            // 根据配置判断是否使用雪花ID
            if (_dbContext.EnableSnowflakeId)
            {
                // 使用SqlSugar标准方法：ExecuteReturnSnowflakeId() 自动生成雪花ID
                var snowflakeId = await _dbContext.Db.Insertable(entity).ExecuteReturnSnowflakeIdAsync();
                baseEntity.Id = snowflakeId;
                return 1;
            }
            else
            {
                // 不使用雪花ID时，使用自增ID，需要返回生成的ID并赋值
                var insertedId = await _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentityAsync();
                baseEntity.Id = insertedId;
                return 1;
            }
        }
        return await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 新增实体（同步方法，用于事务内避免死锁）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    public int Create(TEntity entity, string? userName)
    {
        if (entity is BaseEntity baseEntity)
        {
            // 填充审计字段
            baseEntity.CreatedBy = userName;
            baseEntity.CreatedTime = DateTime.Now;
            baseEntity.UpdatedBy = userName;
            baseEntity.UpdatedTime = DateTime.Now;
            baseEntity.IsDeleted = 0;

            // 根据配置判断是否使用雪花ID
            if (_dbContext.EnableSnowflakeId)
            {
                // 使用SqlSugar标准方法：ExecuteReturnSnowflakeId() 自动生成雪花ID
                var snowflakeId = _dbContext.Db.Insertable(entity).ExecuteReturnSnowflakeId();
                baseEntity.Id = snowflakeId;
                return 1;
            }
            else
            {
                // 不使用雪花ID时，使用自增ID，需要返回生成的ID并赋值
                var insertedId = _dbContext.Db.Insertable(entity).ExecuteReturnBigIdentity();
                baseEntity.Id = insertedId;
                return 1;
            }
        }
        return _dbContext.Db.Insertable(entity).ExecuteCommand();
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>影响行数</returns>
    public async Task<int> CreateBatchAsync(List<TEntity> entities)
    {
        // 填充审计字段（从当前登录用户上下文获取）
        var currentUser = UserContext.Current.IsAuthenticated
            ? UserContext.Current.Username
            : "Takt365";
        foreach (var entity in entities)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreatedBy = currentUser;
                baseEntity.CreatedTime = DateTime.Now;
                baseEntity.UpdatedBy = currentUser;
                baseEntity.UpdatedTime = DateTime.Now;
                baseEntity.IsDeleted = 0;
            }
        }

        // 根据配置判断是否使用雪花ID
        if (_dbContext.EnableSnowflakeId && entities.Count > 0 && entities[0] is BaseEntity)
        {
            // 使用SqlSugar标准方法：ExecuteReturnSnowflakeIdList() 批量生成雪花ID
            var snowflakeIds = await _dbContext.Db.Insertable(entities).ExecuteReturnSnowflakeIdListAsync();
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] is BaseEntity baseEntity)
                {
                    baseEntity.Id = snowflakeIds[i];
                }
            }
            return entities.Count;
        }
        return await _dbContext.Db.Insertable(entities).ExecuteCommandAsync();
    }

    #endregion

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

    #region 更新操作


    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>影响行数</returns>
    public async Task<int> UpdateAsync(TEntity entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated
                ? UserContext.Current.Username
                : "Takt365";
            baseEntity.UpdatedBy = currentUser;
            baseEntity.UpdatedTime = DateTime.Now;
        }

        // 添加诊断日志，确认 db 实例
        var entityType = typeof(TEntity).Name;
        var dbHashCode = _dbContext.Db.GetHashCode();
        System.Diagnostics.Debug.WriteLine($"🔵 [BaseRepository.UpdateAsync] 准备创建 Updateable 对象，实体类型: {entityType}, db 实例哈希: {dbHashCode}");
        WriteDiagnosticLog($"🔵 [BaseRepository.UpdateAsync] 准备创建 Updateable 对象，实体类型: {entityType}, db 实例哈希: {dbHashCode}");

        // 差异日志通过 StaticConfig.CompleteUpdateableFunc 自动启用
        var result = await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();

        System.Diagnostics.Debug.WriteLine($"🔵 [BaseRepository.UpdateAsync] ExecuteCommandAsync 执行完成，影响行数: {result}，实体类型: {entityType}");
        WriteDiagnosticLog($"🔵 [BaseRepository.UpdateAsync] ExecuteCommandAsync 执行完成，影响行数: {result}，实体类型: {entityType}");

        return result;
    }


    /// <summary>
    /// 更新实体（指定用户名）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    public async Task<int> UpdateAsync(TEntity entity, string? userName)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.UpdatedBy = userName;
            baseEntity.UpdatedTime = DateTime.Now;
        }

        // 差异日志通过 StaticConfig.CompleteUpdateableFunc 自动启用
        return await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新实体（同步方法，用于事务内避免死锁）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    public int Update(TEntity entity, string? userName)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.UpdatedBy = userName;
            baseEntity.UpdatedTime = DateTime.Now;
        }

        // 注意：差异日志通过 StaticConfig.CompleteUpdateableFunc 自动启用
        // 如果 GetDiffTable() 在启动时出现连接冲突，会由 SqlSugar 内部处理
        // 这里直接执行，让 SqlSugar 自动处理连接管理
        return _dbContext.Db.Updateable(entity).ExecuteCommand();
    }

    /// <summary>
    /// 批量更新
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>影响行数</returns>
    public async Task<int> UpdateBatchAsync(List<TEntity> entities)
    {
        // 填充审计字段（从当前登录用户上下文获取）
        var currentUser = UserContext.Current.IsAuthenticated
            ? UserContext.Current.Username
            : "Takt365";
        foreach (var entity in entities)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.UpdatedBy = currentUser;
                baseEntity.UpdatedTime = DateTime.Now;
            }
        }
        return await _dbContext.Db.Updateable(entities).ExecuteCommandAsync();
    }

    #endregion

    #region 删除操作

    /// <summary>
    /// 根据主键删除（逻辑删除）
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns>影响行数</returns>
    /// <remarks>
    /// 使用 SqlSugar 的参数化查询，避免 SQL 注入风险
    /// 参考：https://www.donet5.com/home/Doc?typeId=1191
    /// </remarks>
    public async Task<int> DeleteAsync(object id)
    {
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated
                ? UserContext.Current.Username
                : "Takt365";
            // 使用参数化查询，避免 SQL 注入
            // 获取主键列名
            var entityInfo = _dbContext.Db.EntityMaintenance.GetEntityInfo<TEntity>();
            var primaryKeyColumn = entityInfo.Columns.FirstOrDefault(c => c.IsPrimarykey);
            var primaryKeyColumnName = primaryKeyColumn?.DbColumnName ?? "id";

            // 使用不同的参数名，避免与 SetColumns 内部参数冲突
            // 使用 @entityId 作为参数名，而不是 @id
            var idValue = Convert.ToInt64(id);
            return await _dbContext.Db.Updateable<TEntity>()
                .SetColumns("is_deleted", 1)
                .SetColumns("deleted_by", currentUser)
                .SetColumns("deleted_time", DateTime.Now)
                .SetColumns("updated_by", currentUser)
                .SetColumns("updated_time", DateTime.Now)
                .Where($"{primaryKeyColumnName} = @entityId", new { entityId = idValue })
                .ExecuteCommandAsync();
        }
        // 对于非 BaseEntity，使用 In 方法（SqlSugar 会自动识别主键）
        return await _dbContext.Db.Deleteable<TEntity>().In(id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除实体（逻辑删除）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>影响行数</returns>
    public async Task<int> DeleteAsync(TEntity entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated
                ? UserContext.Current.Username
                : "Takt365";
            baseEntity.IsDeleted = 1;
            baseEntity.DeletedBy = currentUser;
            baseEntity.DeletedTime = DateTime.Now;
            baseEntity.UpdatedBy = currentUser;
            baseEntity.UpdatedTime = DateTime.Now;
            return await UpdateAsync(entity);
        }
        return await _dbContext.Db.Deleteable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据条件删除（逻辑删除）
    /// </summary>
    /// <param name="condition">删除条件</param>
    /// <returns>影响行数</returns>
    public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> condition)
    {
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated
                ? UserContext.Current.Username
                : "Takt365";
            return await _dbContext.Db.Updateable<TEntity>()
                .SetColumns("is_deleted", 1)
                .SetColumns("deleted_by", currentUser)
                .SetColumns("deleted_time", DateTime.Now)
                .SetColumns("updated_by", currentUser)
                .SetColumns("updated_time", DateTime.Now)
                .Where(condition)
                .ExecuteCommandAsync();
        }
        return await _dbContext.Db.Deleteable<TEntity>().Where(condition).ExecuteCommandAsync();
    }

    /// <summary>
    /// 批量删除（逻辑删除）
    /// </summary>
    /// <param name="ids">主键列表</param>
    /// <returns>影响行数</returns>
    /// <remarks>
    /// 使用 SqlSugar 的参数化查询，避免 SQL 注入风险
    /// 参考：https://www.donet5.com/home/Doc?typeId=1195
    /// </remarks>
    public async Task<int> DeleteBatchAsync(List<object> ids)
    {
        if (ids == null || ids.Count == 0)
            return 0;

        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated
                ? UserContext.Current.Username
                : "Takt365";
            // 将 object 列表转换为 long 列表（BaseEntity 的主键是 long 类型）
            var longIds = ids.Select(id => Convert.ToInt64(id)).ToList();
            // 使用参数化查询，避免 SQL 注入
            // 获取主键列名
            var entityInfo = _dbContext.Db.EntityMaintenance.GetEntityInfo<TEntity>();
            var primaryKeyColumn = entityInfo.Columns.FirstOrDefault(c => c.IsPrimarykey);
            var primaryKeyColumnName = primaryKeyColumn?.DbColumnName ?? "id";

            // 构建 IN 查询的参数化 SQL
            var placeholders = string.Join(",", longIds.Select((_, index) => $"@id{index}"));
            var parameters = longIds.Select((id, index) => new SugarParameter($"@id{index}", id)).ToList();

            return await _dbContext.Db.Updateable<TEntity>()
                .SetColumns("is_deleted", 1)
                .SetColumns("deleted_by", currentUser)
                .SetColumns("deleted_time", DateTime.Now)
                .SetColumns("updated_by", currentUser)
                .SetColumns("updated_time", DateTime.Now)
                .Where($"{primaryKeyColumnName} IN ({placeholders})", parameters.ToArray())
                .ExecuteCommandAsync();
        }
        return await _dbContext.Db.Deleteable<TEntity>().In(ids).ExecuteCommandAsync();
    }

    #endregion

    #region 状态操作

    /// <summary>
    /// 修改实体状态
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="status">新状态值</param>
    /// <returns>影响行数</returns>
    /// <remarks>
    /// 使用 SqlSugar 的参数化查询，避免 SQL 注入风险
    /// 参考：https://www.donet5.com/home/Doc?typeId=1193
    /// </remarks>
    public async Task<int> StatusAsync(object id, int status)
    {
        // 获取主键列名
        var entityInfo = _dbContext.Db.EntityMaintenance.GetEntityInfo<TEntity>();
        var primaryKeyColumn = entityInfo.Columns.FirstOrDefault(c => c.IsPrimarykey);
        var primaryKeyColumnName = primaryKeyColumn?.DbColumnName ?? "id";

        // 使用参数化查询，避免 SQL 注入
        // 注意：使用不同的参数名（@entityId）避免与差异日志功能中的 @id 参数冲突
        var idValue = Convert.ToInt64(id);
        return await _dbContext.Db.Updateable<TEntity>()
            .SetColumns("status", status)
            .SetColumns("updated_time", DateTime.Now)
            .Where($"{primaryKeyColumnName} = @entityId", new { entityId = idValue })
            .ExecuteCommandAsync();
    }

    #endregion

    #region 树形查询操作

    /// <summary>
    /// 构建树形结构（基于主键）
    /// </summary>
    /// <typeparam name="TTreeEntity">树形实体类型（必须包含 Child 属性和 ParentId 属性）</typeparam>
    /// <param name="childSelector">子节点集合选择器</param>
    /// <param name="parentIdSelector">父级ID选择器</param>
    /// <param name="rootParentId">根节点的父级ID值（通常为 0 或 null）</param>
    /// <param name="condition">查询条件（可选）</param>
    /// <returns>树形结构列表</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=2311
    /// 适用于基于主键的树形结构，如 Menu 的 ParentId
    /// </remarks>
    public async Task<List<TTreeEntity>> GetTreeListAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, IEnumerable<TTreeEntity>>> childSelector,
        Expression<Func<TTreeEntity, object>> parentIdSelector,
        object rootParentId,
        Expression<Func<TTreeEntity, bool>>? condition = null) where TTreeEntity : class, new()
    {
        var query = _dbContext.Db.Queryable<TTreeEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TTreeEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        // 应用查询条件
        if (condition != null)
        {
            query = query.Where(condition);
        }

        // 使用 SqlSugar 的 ToTree 方法构建树形结构
        // 注意：SqlSugar 的 ToTreeAsync 需要 IEnumerable<object> 类型，需要进行类型转换
        var childSelectorObject = Expression.Lambda<Func<TTreeEntity, IEnumerable<object>>>(
            Expression.Convert(childSelector.Body, typeof(IEnumerable<object>)),
            childSelector.Parameters);
        var treeList = await query.ToTreeAsync(childSelectorObject, parentIdSelector, rootParentId);
        return treeList;
    }

    /// <summary>
    /// 构建树形结构（基于编码）
    /// </summary>
    /// <typeparam name="TTreeEntity">树形实体类型（必须包含 Child 属性、Code 属性和 ParentCode 属性）</typeparam>
    /// <param name="childSelector">子节点集合选择器</param>
    /// <param name="codeSelector">编码选择器</param>
    /// <param name="parentCodeSelector">父级编码选择器</param>
    /// <param name="rootParentCode">根节点的父级编码值（通常为 null 或空字符串）</param>
    /// <param name="condition">查询条件（可选）</param>
    /// <returns>树形结构列表</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=2311
    /// 适用于基于编码的树形结构，如组织架构的 Code 和 ParentCode
    /// </remarks>
    public async Task<List<TTreeEntity>> GetTreeListByCodeAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, IEnumerable<TTreeEntity>>> childSelector,
        Expression<Func<TTreeEntity, object>> codeSelector,
        Expression<Func<TTreeEntity, object>> parentCodeSelector,
        string? rootParentCode = null!, 
        Expression<Func<TTreeEntity, bool>>? condition = null) where TTreeEntity : class, new()
    {
        var query = _dbContext.Db.Queryable<TTreeEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TTreeEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        // 应用查询条件
        if (condition != null)
        {
            query = query.Where(condition);
        }

        // 使用 SqlSugar 的 ToTree 方法构建树形结构（基于编码）
        // 注意：SqlSugar 的 ToTreeAsync 需要 IEnumerable<object> 类型，需要进行类型转换
        var childSelectorObject = Expression.Lambda<Func<TTreeEntity, IEnumerable<object>>>(
            Expression.Convert(childSelector.Body, typeof(IEnumerable<object>)),
            childSelector.Parameters);
        var treeList = await query.ToTreeAsync(childSelectorObject, parentCodeSelector, rootParentCode ?? string.Empty);
        return treeList;
    }

    /// <summary>
    /// 查询所有上级节点（基于主键）
    /// </summary>
    /// <typeparam name="TTreeEntity">树形实体类型（必须包含 ParentId 属性）</typeparam>
    /// <param name="parentIdSelector">父级ID选择器</param>
    /// <param name="nodeId">目标节点的主键值</param>
    /// <returns>所有上级节点列表（从直接父级到根节点）</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=2311
    /// </remarks>
    public async Task<List<TTreeEntity>> GetParentListAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> parentIdSelector,
        object nodeId) where TTreeEntity : class, new()
    {
        var query = _dbContext.Db.Queryable<TTreeEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TTreeEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        // 使用 SqlSugar 的 ToParentList 方法查询所有上级节点
        var parentList = await query.ToParentListAsync(parentIdSelector, nodeId);
        return parentList;
    }

    /// <summary>
    /// 查询所有上级节点（基于编码）
    /// </summary>
    /// <typeparam name="TTreeEntity">树形实体类型（必须包含 Code 属性和 ParentCode 属性）</typeparam>
    /// <param name="codeSelector">编码选择器</param>
    /// <param name="parentCodeSelector">父级编码选择器</param>
    /// <param name="nodeCode">目标节点的编码值</param>
    /// <returns>所有上级节点列表（从直接父级到根节点）</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=2311
    /// </remarks>
    public async Task<List<TTreeEntity>> GetParentListByCodeAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> codeSelector,
        Expression<Func<TTreeEntity, object>> parentCodeSelector,
        string nodeCode) where TTreeEntity : class, new()
    {
        var query = _dbContext.Db.Queryable<TTreeEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TTreeEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        // 使用 SqlSugar 的 ToParentList 方法查询所有上级节点（基于编码）
        var parentList = await query.ToParentListAsync(parentCodeSelector, nodeCode);
        return parentList;
    }

    /// <summary>
    /// 查询所有下级节点（基于主键）
    /// </summary>
    /// <typeparam name="TTreeEntity">树形实体类型（必须包含 ParentId 属性）</typeparam>
    /// <param name="parentIdSelector">父级ID选择器</param>
    /// <param name="nodeId">目标节点的主键值</param>
    /// <returns>所有下级节点列表（包含直接子节点和所有后代节点）</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=2311
    /// </remarks>
    public async Task<List<TTreeEntity>> GetChildListAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> parentIdSelector,
        object nodeId) where TTreeEntity : class, new()
    {
        var query = _dbContext.Db.Queryable<TTreeEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TTreeEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        // 使用 SqlSugar 的 ToChildList 方法查询所有下级节点
        var childList = await query.ToChildListAsync(parentIdSelector, nodeId);
        return childList;
    }

    /// <summary>
    /// 查询所有下级节点（基于编码）
    /// </summary>
    /// <typeparam name="TTreeEntity">树形实体类型（必须包含 Code 属性和 ParentCode 属性）</typeparam>
    /// <param name="codeSelector">编码选择器</param>
    /// <param name="parentCodeSelector">父级编码选择器</param>
    /// <param name="nodeCode">目标节点的编码值</param>
    /// <returns>所有下级节点列表（包含直接子节点和所有后代节点）</returns>
    /// <remarks>
    /// 参考：https://www.donet5.com/home/Doc?typeId=2311
    /// </remarks>
    public async Task<List<TTreeEntity>> GetChildListByCodeAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> codeSelector,
        Expression<Func<TTreeEntity, object>> parentCodeSelector,
        string nodeCode) where TTreeEntity : class, new()
    {
        var query = _dbContext.Db.Queryable<TTreeEntity>();

        // 如果实体继承自BaseEntity，自动过滤已删除的记录
        if (typeof(TTreeEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            query = query.Where("is_deleted = 0");
        }

        // 使用 SqlSugar 的 ToChildList 方法查询所有下级节点（基于编码）
        var childList = await query.ToChildListAsync(parentCodeSelector, nodeCode);
        return childList;
    }

    #endregion

    #region 原始SQL查询

    /// <summary>
    /// 执行原始SQL查询（返回动态类型）
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parameters">SQL参数（可选）</param>
    /// <returns>查询结果列表</returns>
    /// <remarks>
    /// 用于执行自定义SQL脚本，如字典类型的SQL脚本数据源
    /// 注意：SQL语句应该经过验证，避免SQL注入风险
    /// </remarks>
    public async Task<List<dynamic>> ExecuteSqlAsync(string sql, object? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL语句不能为空", nameof(sql));

        if (parameters != null)
        {
            return await _dbContext.Db.Ado.SqlQueryAsync<dynamic>(sql, parameters);
        }
        else
        {
            return await _dbContext.Db.Ado.SqlQueryAsync<dynamic>(sql);
        }
    }

    #endregion
}
