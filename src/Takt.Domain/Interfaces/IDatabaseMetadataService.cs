// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Domain.Interfaces
// 文件名称：IDatabaseMetadataService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：数据库元数据服务接口（领域层）
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Threading.Tasks;
using Takt.Common.Models;
using Takt.Common.Results;

namespace Takt.Domain.Interfaces;

/// <summary>
/// 数据库元数据服务接口
/// 提供获取数据库表名和列信息的通用功能
/// </summary>
/// <remarks>
/// 参考：https://www.donet5.com/home/Doc?typeId=1203
/// </remarks>
public interface IDatabaseMetadataService
{
    /// <summary>
    /// 获取所有表名
    /// </summary>
    /// <param name="isCache">是否使用缓存，默认 true</param>
    /// <returns>表名列表</returns>
    List<string> GetAllTableNames(bool isCache = true);

    /// <summary>
    /// 根据表名获取列信息
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="isCache">是否使用缓存，默认 true</param>
    /// <returns>列信息列表</returns>
    List<ColumnInfo> GetColumnsByTableName(string tableName, bool isCache = true);

    /// <summary>
    /// 获取表信息（包含表名和描述）
    /// </summary>
    /// <param name="isCache">是否使用缓存，默认 true</param>
    /// <returns>表信息列表，包含表名和描述</returns>
    List<TableInfo> GetTableInfoList(bool isCache = true);

    /// <summary>
    /// 获取当前数据库名称
    /// </summary>
    /// <returns>数据库名称</returns>
    string GetDatabaseName();

    /// <summary>
    /// 从代码生成配置创建或更新数据库表
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="tableDescription">表描述</param>
    /// <param name="columns">列配置列表</param>
    /// <returns>操作结果</returns>
    Task<Result> CreateOrUpdateTableFromConfigAsync(string tableName, string? tableDescription, List<ColumnInfo> columns);
}

/// <summary>
/// 表信息模型
/// </summary>
public class TableInfo
{
    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 表描述/注释
    /// </summary>
    public string? Description { get; set; }
}

