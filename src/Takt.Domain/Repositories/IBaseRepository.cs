// ========================================
// 项目名称：Takt.Wpf
// 文件名称：IBaseRepository.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：通用仓储接口
// 
// 版权信息：
// Copyright (c) 2025 Takt All rights reserved.
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
using Takt.Common.Results;

namespace Takt.Domain.Repositories;

/// <summary>
/// 通用仓储接口
/// 定义所有实体的通用数据访问操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IBaseRepository<TEntity> where TEntity : class, new()
{
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
        Expression<Func<TEntity, bool>>? condition = null!, 
        int pageIndex = 1,
        int pageSize = 20,
        Expression<Func<TEntity, object>>? orderByExpression = null!, 
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
    Task<TResult?> GetMaxAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct;

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
    Task<TResult?> GetMinAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct;

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
    Task<TResult?> GetSumAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct;

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
    Task<TResult?> GetAverageAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct;

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
    Task<TResult?> GetMedianAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? condition = null) where TResult : struct;

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
    Task<int> CreateBatchAsync(List<TEntity> entities);

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
    Task<int> UpdateBatchAsync(List<TEntity> entities);

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
    Task<int> DeleteBatchAsync(List<object> ids);

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
    Task<List<TTreeEntity>> GetTreeListAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, IEnumerable<TTreeEntity>>> childSelector,
        Expression<Func<TTreeEntity, object>> parentIdSelector,
        object rootParentId,
        Expression<Func<TTreeEntity, bool>>? condition = null) where TTreeEntity : class, new();

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
    Task<List<TTreeEntity>> GetTreeListByCodeAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, IEnumerable<TTreeEntity>>> childSelector,
        Expression<Func<TTreeEntity, object>> codeSelector,
        Expression<Func<TTreeEntity, object>> parentCodeSelector,
        string? rootParentCode = null!, 
        Expression<Func<TTreeEntity, bool>>? condition = null) where TTreeEntity : class, new();

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
    Task<List<TTreeEntity>> GetParentListAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> parentIdSelector,
        object nodeId) where TTreeEntity : class, new();

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
    Task<List<TTreeEntity>> GetParentListByCodeAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> codeSelector,
        Expression<Func<TTreeEntity, object>> parentCodeSelector,
        string nodeCode) where TTreeEntity : class, new();

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
    Task<List<TTreeEntity>> GetChildListAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> parentIdSelector,
        object nodeId) where TTreeEntity : class, new();

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
    Task<List<TTreeEntity>> GetChildListByCodeAsync<TTreeEntity>(
        Expression<Func<TTreeEntity, object>> codeSelector,
        Expression<Func<TTreeEntity, object>> parentCodeSelector,
        string nodeCode) where TTreeEntity : class, new();

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
    Task<List<dynamic>> ExecuteSqlAsync(string sql, object? parameters = null);

    #endregion
}
