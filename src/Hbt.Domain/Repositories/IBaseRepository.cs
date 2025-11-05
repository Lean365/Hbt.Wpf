// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：IBaseRepository.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：通用仓储接口
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

namespace Hbt.Domain.Repositories;

/// <summary>
/// 通用仓储接口
/// 定义所有实体的通用数据访问操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IBaseRepository<TEntity> where TEntity : class, new()
{
    /// <summary>
    /// 获取SqlSugar客户端
    /// </summary>
    ISqlSugarClient SqlSugarClient { get; }

    /// <summary>
    /// 获取SimpleClient对象
    /// </summary>
    SimpleClient<TEntity> SimpleClient { get; }

    /// <summary>
    /// 获取查询对象
    /// </summary>
    /// <returns>查询对象</returns>
    ISugarQueryable<TEntity> AsQueryable();

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
    Task<PagedResult<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? condition = null,
        int pageIndex = 1,
        int pageSize = 20,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        OrderByType orderByType = OrderByType.Desc);

    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns>实体</returns>
    Task<TEntity?> GetByIdAsync(object id);

    /// <summary>
    /// 获取第一个符合条件的实体（异步）
    /// </summary>
    /// <param name="condition">查询条件</param>
    /// <returns>实体</returns>
    Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> condition);

    /// <summary>
    /// 获取第一个符合条件的实体（同步，用于事务内避免死锁）
    /// </summary>
    /// <param name="condition">查询条件</param>
    /// <returns>实体</returns>
    TEntity? GetFirst(Expression<Func<TEntity, bool>> condition);

    /// <summary>
    /// 获取符合条件的实体数量
    /// </summary>
    /// <param name="condition">查询条件</param>
    /// <returns>数量</returns>
    Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? condition = null);

    #endregion

    #region 新增操作

    /// <summary>
    /// 新增实体（异步）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>影响行数</returns>
    Task<int> CreateAsync(TEntity entity);

    /// <summary>
    /// 新增实体（异步，指定用户名）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    Task<int> CreateAsync(TEntity entity, string? userName);

    /// <summary>
    /// 新增实体（同步，用于事务内避免死锁）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    int Create(TEntity entity, string? userName);

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>影响行数</returns>
    Task<int> CreateRangeAsync(List<TEntity> entities);

    #endregion

    #region 更新操作

    /// <summary>
    /// 更新实体（异步）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>影响行数</returns>
    Task<int> UpdateAsync(TEntity entity);

    /// <summary>
    /// 更新实体（异步，指定用户名）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    Task<int> UpdateAsync(TEntity entity, string? userName);

    /// <summary>
    /// 更新实体（同步，用于事务内避免死锁）
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="userName">用户名</param>
    /// <returns>影响行数</returns>
    int Update(TEntity entity, string? userName);

    /// <summary>
    /// 批量更新
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>影响行数</returns>
    Task<int> UpdateRangeAsync(List<TEntity> entities);

    #endregion

    #region 删除操作

    /// <summary>
    /// 根据主键删除
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns>影响行数</returns>
    Task<int> DeleteAsync(object id);

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <returns>影响行数</returns>
    Task<int> DeleteAsync(TEntity entity);

    /// <summary>
    /// 根据条件删除
    /// </summary>
    /// <param name="condition">删除条件</param>
    /// <returns>影响行数</returns>
    Task<int> DeleteAsync(Expression<Func<TEntity, bool>> condition);

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="ids">主键列表</param>
    /// <returns>影响行数</returns>
    Task<int> DeleteRangeAsync(List<object> ids);

    #endregion

    #region 状态操作

    /// <summary>
    /// 修改实体状态
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="status">新状态值</param>
    /// <returns>影响行数</returns>
    Task<int> StatusAsync(object id, int status);

    #endregion
}
