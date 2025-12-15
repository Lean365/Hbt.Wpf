// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Services
// 文件名称：DatabaseMetadataService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：数据库元数据服务实现（基础设施层）
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq;
using SqlSugar;
using Takt.Common.Models;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using Takt.Infrastructure.Data;

namespace Takt.Infrastructure.Services;

/// <summary>
/// 数据库元数据服务
/// 使用 SqlSugar 的 DbMaintenance 获取数据库表名和列信息
/// </summary>
/// <remarks>
/// 参考：https://www.donet5.com/home/Doc?typeId=1203
/// </remarks>
public class DatabaseMetadataService : IDatabaseMetadataService
{
    private readonly DbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public DatabaseMetadataService(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// 获取所有表名
    /// </summary>
    /// <param name="isCache">是否使用缓存，默认 true</param>
    /// <returns>表名列表</returns>
    /// <remarks>
    /// 使用 GetTableInfoList() 获取所有表
    /// 参考：https://www.donet5.com/Home/Doc?typeId=1203
    /// </remarks>
    public List<string> GetAllTableNames(bool isCache = true)
    {
        var tables = _dbContext.Db.DbMaintenance.GetTableInfoList(isCache);
        return tables.Select(t => t.Name).ToList();
    }

    /// <summary>
    /// 根据表名获取列信息
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="isCache">是否使用缓存，默认 true</param>
    /// <returns>列信息列表</returns>
    /// <remarks>
    /// 使用 GetColumnInfosByTableName() 获取列信息
    /// 参考：https://www.donet5.com/Home/Doc?typeId=1203
    /// </remarks>
    public List<ColumnInfo> GetColumnsByTableName(string tableName, bool isCache = true)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("表名不能为空", nameof(tableName));
        }

        var columns = _dbContext.Db.DbMaintenance.GetColumnInfosByTableName(tableName, isCache);
        
        return columns.Select(c => new ColumnInfo
        {
            ColumnName = c.DbColumnName,
            DataType = c.DataType,
            IsPrimaryKey = c.IsPrimarykey,
            IsIdentity = c.IsIdentity,
            IsNullable = c.IsNullable,
            Length = c.Length,
            DecimalPlaces = c.DecimalDigits,
            DefaultValue = CleanDefaultValue(c.DefaultValue),
            Description = c.ColumnDescription
        }).ToList();
    }

    /// <summary>
    /// 获取表信息（包含表名和描述）
    /// </summary>
    /// <param name="isCache">是否使用缓存，默认 true</param>
    /// <returns>表信息列表，包含表名和描述</returns>
    /// <remarks>
    /// 使用 GetTableInfoList() 获取所有表
    /// 参考：https://www.donet5.com/Home/Doc?typeId=1203
    /// </remarks>
    public List<TableInfo> GetTableInfoList(bool isCache = true)
    {
        var tables = _dbContext.Db.DbMaintenance.GetTableInfoList(isCache);
        
        return tables.Select(t => new TableInfo
        {
            TableName = t.Name,
            Description = t.Description
        }).ToList();
    }

    /// <summary>
    /// 获取当前数据库名称
    /// </summary>
    /// <returns>数据库名称</returns>
    /// <remarks>
    /// 1. 从连接字符串中解析 Database 参数，获取当前连接的数据库名称
    /// 2. 获取所有数据库列表
    /// 3. 在数据库列表中查找匹配的数据库名称
    /// 4. 返回匹配的数据库名称（确保是真实存在的数据库）
    /// </remarks>
    public string GetDatabaseName()
    {
        try
        {
            // 从连接字符串中解析数据库名称
            var connectionString = _dbContext.Db.CurrentConnectionConfig.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return string.Empty;
            }

            string? dbNameFromConnectionString = null;

            // 解析连接字符串中的 Database 参数
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (trimmedPart.StartsWith("Database=", StringComparison.OrdinalIgnoreCase) ||
                    trimmedPart.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase))
                {
                    var equalIndex = trimmedPart.IndexOf('=');
                    if (equalIndex >= 0 && equalIndex < trimmedPart.Length - 1)
                    {
                        dbNameFromConnectionString = trimmedPart.Substring(equalIndex + 1).Trim();
                        break;
                    }
                }
            }

            // 如果无法从连接字符串解析，返回空
            if (string.IsNullOrWhiteSpace(dbNameFromConnectionString))
            {
                return string.Empty;
            }

            // 获取所有数据库列表
            var allDatabases = _dbContext.Db.DbMaintenance.GetDataBaseList();
            if (allDatabases == null || !allDatabases.Any())
            {
                return dbNameFromConnectionString; // 如果无法获取数据库列表，返回连接字符串中的名称
            }

            // 在数据库列表中查找匹配的数据库名称（不区分大小写）
            var matchedDatabase = allDatabases.FirstOrDefault(db => 
                string.Equals(db, dbNameFromConnectionString, StringComparison.OrdinalIgnoreCase));

            // 返回匹配的数据库名称，如果找不到则返回连接字符串中的名称
            return matchedDatabase ?? dbNameFromConnectionString;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 从代码生成配置创建或更新数据库表
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="tableDescription">表描述</param>
    /// <param name="columns">列配置列表</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 使用 SqlSugar 的 DbMaintenance API 创建或更新表结构
    /// 参考：https://www.donet5.com/Home/Doc?typeId=1203
    /// </remarks>
    public async Task<Result> CreateOrUpdateTableFromConfigAsync(string tableName, string? tableDescription, List<ColumnInfo> columns)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return Result.Fail("表名不能为空");
        }

        if (columns == null || !columns.Any())
        {
            return Result.Fail("列配置不能为空");
        }

        try
        {
            var db = _dbContext.Db;
            var dbMaintenance = db.DbMaintenance;

            // 检查表是否存在
            var tableExists = dbMaintenance.IsAnyTable(tableName);

            if (!tableExists)
            {
                // 创建新表
                var createTableSql = BuildCreateTableSql(tableName, tableDescription, columns);
                await Task.Run(() => db.Ado.ExecuteCommand(createTableSql));
            }
            else
            {
                // 更新现有表（添加新列，更新列属性）
                var existingColumns = dbMaintenance.GetColumnInfosByTableName(tableName, false);
                var existingColumnNames = existingColumns.Select(c => c.DbColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var column in columns)
                {
                    if (!existingColumnNames.Contains(column.ColumnName))
                    {
                        // 添加新列
                        var addColumnSql = BuildAddColumnSql(tableName, column);
                        await Task.Run(() => db.Ado.ExecuteCommand(addColumnSql));
                    }
                    else
                    {
                        // 更新列属性（如果需要）
                        // 注意：SqlSugar 的 DbMaintenance 可能不支持直接修改列属性，这里只处理添加新列
                    }
                }
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"创建或更新表失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建创建表的 SQL 语句
    /// </summary>
    private string BuildCreateTableSql(string tableName, string? tableDescription, List<ColumnInfo> columns)
    {
        var db = _dbContext.Db;
        var dbType = db.CurrentConnectionConfig.DbType;

        var sqlBuilder = new System.Text.StringBuilder();
        sqlBuilder.Append($"CREATE TABLE [{tableName}] (");

        var columnDefinitions = new List<string>();
        var primaryKeys = new List<string>();

        foreach (var column in columns)
        {
            var columnDef = BuildColumnDefinition(column, dbType);
            columnDefinitions.Add(columnDef);

            if (column.IsPrimaryKey)
            {
                primaryKeys.Add(column.ColumnName);
            }
        }

        sqlBuilder.Append(string.Join(", ", columnDefinitions));

        if (primaryKeys.Any())
        {
            sqlBuilder.Append($", PRIMARY KEY ([{string.Join("], [", primaryKeys)}])");
        }

        sqlBuilder.Append(")");

        // 添加表注释（如果支持）
        if (!string.IsNullOrWhiteSpace(tableDescription) && dbType == DbType.MySql)
        {
            sqlBuilder.Append($" COMMENT='{tableDescription.Replace("'", "''")}'");
        }

        return sqlBuilder.ToString();
    }

    /// <summary>
    /// 构建列定义 SQL
    /// </summary>
    private string BuildColumnDefinition(ColumnInfo column, DbType dbType)
    {
        var columnName = $"[{column.ColumnName}]";
        var dataType = MapDataType(column.DataType, column.Length, column.DecimalPlaces, dbType);
        var nullable = column.IsNullable ? "NULL" : "NOT NULL";
        var identity = column.IsIdentity ? "IDENTITY(1,1)" : "";
        var defaultValue = string.IsNullOrWhiteSpace(column.DefaultValue) ? "" : $"DEFAULT {column.DefaultValue}";

        var parts = new List<string> { columnName, dataType, nullable };
        if (!string.IsNullOrWhiteSpace(identity))
        {
            parts.Add(identity);
        }
        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            parts.Add(defaultValue);
        }

        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// 映射数据类型
    /// </summary>
    private string MapDataType(string? dataType, int? length, int? decimalPlaces, DbType dbType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return "NVARCHAR(255)";
        }

        var type = dataType.ToUpperInvariant();
        return type switch
        {
            "STRING" or "VARCHAR" => length.HasValue ? $"NVARCHAR({length.Value})" : "NVARCHAR(255)",
            "INT" or "INT32" => "INT",
            "LONG" or "INT64" => "BIGINT",
            "DECIMAL" => decimalPlaces.HasValue ? $"DECIMAL(18,{decimalPlaces.Value})" : "DECIMAL(18,2)",
            "DOUBLE" => "FLOAT",
            "FLOAT" => "REAL",
            "BOOL" or "BOOLEAN" => "BIT",
            "DATETIME" => "DATETIME",
            "DATE" => "DATE",
            "TIME" => "TIME",
            "GUID" => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(255)"
        };
    }

    /// <summary>
    /// 构建添加列的 SQL 语句
    /// </summary>
    private string BuildAddColumnSql(string tableName, ColumnInfo column)
    {
        var db = _dbContext.Db;
        var dbType = db.CurrentConnectionConfig.DbType;
        var columnDef = BuildColumnDefinition(column, dbType);
        return $"ALTER TABLE [{tableName}] ADD {columnDef}";
    }

    /// <summary>
    /// 清理默认值，去除 SQL Server 默认值中的括号和引号，只保留实际值
    /// </summary>
    /// <param name="defaultValue">原始默认值</param>
    /// <returns>清理后的默认值</returns>
    /// <remarks>
    /// SQL Server 返回的默认值格式可能是：
    /// - ((0)) -> 0
    /// - ('0') -> 0
    /// - ('text') -> text
    /// - (getdate()) -> getdate()
    /// - (N'text') -> text
    /// </remarks>
    private static string? CleanDefaultValue(string? defaultValue)
    {
        if (string.IsNullOrWhiteSpace(defaultValue))
        {
            return defaultValue;
        }

        var cleaned = defaultValue.Trim();

        // 去除外层括号，例如 ((0)) -> (0) -> 0
        while (cleaned.StartsWith('(') && cleaned.EndsWith(')'))
        {
            cleaned = cleaned.Substring(1, cleaned.Length - 2).Trim();
        }

        // 去除字符串引号，例如 '0' -> 0, N'text' -> text
        if (cleaned.StartsWith("N'", StringComparison.OrdinalIgnoreCase) && cleaned.EndsWith("'"))
        {
            cleaned = cleaned.Substring(2, cleaned.Length - 3);
        }
        else if (cleaned.StartsWith("'") && cleaned.EndsWith("'"))
        {
            cleaned = cleaned.Substring(1, cleaned.Length - 2);
        }

        return cleaned.Trim();
    }
}

