// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：BaseRepository.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：通用仓储实现
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

using System.Linq.Expressions;
using SqlSugar;
using Hbt.Common.Results;
using Hbt.Common.Context;
using Hbt.Common.Helpers;
using Hbt.Domain.Entities;
using Hbt.Domain.Repositories;
using Hbt.Infrastructure.Data;

namespace Hbt.Infrastructure.Repositories;

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
    /// 获取SqlSugar客户端
    /// </summary>
    public ISqlSugarClient SqlSugarClient => _dbContext.Db;

    /// <summary>
    /// 获取SimpleClient对象
    /// </summary>
    public SimpleClient<TEntity> SimpleClient => new SimpleClient<TEntity>(_dbContext.Db);

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
        Expression<Func<TEntity, bool>>? condition = null,
        int pageIndex = 1,
        int pageSize = 20,
        Expression<Func<TEntity, object>>? orderByExpression = null,
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

        int totalNum = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, totalNum);
        totalNum = await query.CountAsync();

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
    public async Task<TEntity?> GetByIdAsync(object id)
    {
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
                : "System";
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
    public async Task<int> CreateRangeAsync(List<TEntity> entities)
    {
        // 填充审计字段（从当前登录用户上下文获取）
        var currentUser = UserContext.Current.IsAuthenticated 
            ? UserContext.Current.Username 
            : "System";
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
                : "System";
            baseEntity.UpdatedBy = currentUser;
            baseEntity.UpdatedTime = DateTime.Now;
        }
        return await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
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
        return _dbContext.Db.Updateable(entity).ExecuteCommand();
    }

    /// <summary>
    /// 批量更新
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>影响行数</returns>
    public async Task<int> UpdateRangeAsync(List<TEntity> entities)
    {
        // 填充审计字段（从当前登录用户上下文获取）
        var currentUser = UserContext.Current.IsAuthenticated 
            ? UserContext.Current.Username 
            : "System";
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
    public async Task<int> DeleteAsync(object id)
    {
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated 
                ? UserContext.Current.Username 
                : "System";
            return await _dbContext.Db.Updateable<TEntity>()
                .SetColumns("is_deleted", 1)
                .SetColumns("deleted_by", currentUser)
                .SetColumns("deleted_time", DateTime.Now)
                .SetColumns("updated_by", currentUser)
                .SetColumns("updated_time", DateTime.Now)
                .Where($"id = {id}")
                .ExecuteCommandAsync();
        }
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
                : "System";
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
                : "System";
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
    public async Task<int> DeleteRangeAsync(List<object> ids)
    {
        if (typeof(TEntity).IsSubclassOf(typeof(BaseEntity)))
        {
            // 填充审计字段（从当前登录用户上下文获取）
            var currentUser = UserContext.Current.IsAuthenticated 
                ? UserContext.Current.Username 
                : "System";
            return await _dbContext.Db.Updateable<TEntity>()
                .SetColumns("is_deleted", 1)
                .SetColumns("deleted_by", currentUser)
                .SetColumns("deleted_time", DateTime.Now)
                .SetColumns("updated_by", currentUser)
                .SetColumns("updated_time", DateTime.Now)
                .Where($"id IN ({string.Join(",", ids)})")
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
    public async Task<int> StatusAsync(object id, int status)
    {
        return await _dbContext.Db.Updateable<TEntity>()
            .SetColumns("status", status)
            .SetColumns("updated_time", DateTime.Now)
            .Where($"id = {id}")
            .ExecuteCommandAsync();
    }

    #endregion
}
