//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DbTableInitializer.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-20
// 版本号 : 1.0
// 描述    : 数据表初始化服务
//===================================================================

using Hbt.Common.Logging;
using Hbt.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Linq;

namespace Hbt.Infrastructure.Data;

/// <summary>
/// 数据表初始化服务
/// 负责创建数据库表结构（CodeFirst）
/// </summary>
public class DbTableInitializer
{
    private readonly DbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly InitLogManager _initLog;

    public DbTableInitializer(DbContext dbContext, IConfiguration configuration, InitLogManager initLog)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _initLog = initLog;
    }

    /// <summary>
    /// 初始化数据表（使用 DbContext 的通用方法）
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // 检查是否启用 CodeFirst
            var enableCodeFirst = bool.Parse(_configuration["DatabaseSettings:EnableCodeFirst"] ?? "false");
            
            if (!enableCodeFirst)
            {
                _initLog.Information("CodeFirst 功能已禁用，跳过数据表初始化");
                return;
            }

            _initLog.Information("开始初始化数据表（SqlSugar CodeFirst）...");

            // ✅ 1. 使用 DbContext 确保数据库存在
            _dbContext.EnsureDatabaseCreated();

            var db = _dbContext.Db;

            // 自动查找所有继承自 BaseEntity 的实体类型
            var domainAssembly = Assembly.Load("Hbt.Domain");
            var entityTypes = domainAssembly.GetTypes()
                .Where(t => t.IsClass 
                    && !t.IsAbstract 
                    && t.IsSubclassOf(typeof(BaseEntity)))
                .ToArray();

            _initLog.Information("✅ 自动发现 {Count} 个实体类型", entityTypes.Length);

            // 检查并处理每个表
            // SqlSugar 官方方法：InitTables 对已存在的表会尝试更新列结构（新增字段等）
            // 参考：https://www.donet5.com/home/Doc?typeId=1206
            _initLog.Information("开始批量初始化/更新所有表...");

            // 强制重建：先删表再重建（忽略表内数据）
            
            foreach (var entityType in entityTypes)
            {
                var tableName = db.EntityMaintenance.GetTableName(entityType);
                var exists = db.DbMaintenance.IsAnyTable(tableName);
                if (!exists)
                {
                    _initLog.Information("🆕 表 [{TableName}] 不存在，创建...", tableName);
                    db.CodeFirst.InitTables(entityType);
                    _initLog.Information("✅ 表 [{TableName}] 创建完成", tableName);
                }
                else
                {
                    // 检测是否有字段变化（名称/长度/小数位/可空/类型）
                    var dbColumns = db.DbMaintenance.GetColumnInfosByTableName(tableName);
                    var entityInfo = db.EntityMaintenance.GetEntityInfo(entityType);
                        var entityColumns = entityInfo.Columns;

                    List<string> GetDiffs()
                    {
                        // 忽略审计/软删等通用字段，避免这些字段的元数据差异导致重复重建
                        var ignoredColumns = new HashSet<string>(new[]
                        {
                            "id", // 基类主键，按你的要求排除
                            "remarks",
                            "created_by",
                            "created_time",
                            "updated_by",
                            "updated_time",
                            "deleted_by",
                            "deleted_time",
                            "is_deleted"
                        }, StringComparer.OrdinalIgnoreCase);
                        var diffs = new List<string>();

                        bool IsNavType(System.Type? t)
                        {
                            if (t == null) return false;
                            if (t == typeof(string)) return false;
                            if (t.IsPrimitive) return false;
                            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && t != typeof(string)) return true;
                            return t.Namespace != null && t.Namespace.StartsWith("Hbt.Domain.Entities");
                        }

                        string Norm(string? t)
                        {
                            if (string.IsNullOrWhiteSpace(t)) return string.Empty;
                            t = t.Trim().ToLowerInvariant();
                            // 去掉括号内容，如 nvarchar(50) -> nvarchar
                            var p = t.IndexOf('(');
                            if (p > 0) t = t.Substring(0, p);
                            // 同义映射
                            return t switch
                            {
                                "numeric" => "decimal",
                                "datetime2" => "datetime",
                                "datetimeoffset" => "datetime",
                                "ntext" => "nvarchar",
                                "text" => "varchar",
                                _ => t
                            };
                        }

                        bool IsLengthType(string normType)
                            => normType is "nvarchar" or "varchar" or "nchar" or "char";

                        bool IsDecimalType(string normType)
                            => normType == "decimal";

                        // 1) 实体新增了列（DB 中不存在）
                        var dbColNames = dbColumns.Select(c => c.DbColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        var entityColNames = entityColumns
                            .Where(c => !ignoredColumns.Contains((c.DbColumnName ?? c.PropertyName) ?? string.Empty))
                            .Where(c => !c.IsIgnore)
                            .Where(c => !IsNavType(c.PropertyInfo?.PropertyType))
                            .Where(c => !string.IsNullOrEmpty(c.DbColumnName))
                            .Select(c => c.DbColumnName!)
                            .ToList();
                        foreach (var name in entityColNames)
                        {
                            if (!dbColNames.Contains(name))
                            {
                                diffs.Add($"新增列: {name}");
                            }
                        }

                        // 2) 关键列属性发生变化
                        foreach (var ec in entityColumns)
                        {
                            if (ignoredColumns.Contains(ec.DbColumnName ?? ec.PropertyName)) continue;
                            if (ec.IsIgnore) continue;
                            if (IsNavType(ec.PropertyInfo?.PropertyType)) continue;
                            if (string.IsNullOrEmpty(ec.DbColumnName)) continue; // 导航或非映射属性
                            var dc = dbColumns.FirstOrDefault(c => c.DbColumnName.Equals(ec.DbColumnName, StringComparison.OrdinalIgnoreCase));
                            if (dc == null)
                            {
                                // 已在新增列中记录
                                continue;
                            }
                            // 比较数据类型/长度/小数/可空（带规范化与适配）
                            var dType = Norm(dc.DataType);
                            var eType = Norm(ec.DataType);

                            // 若实体未指定 ColumnDataType，则不比类型，只比可空/长度（按推断）
                            if (!string.IsNullOrEmpty(eType) && dType != eType)
                                diffs.Add($"列 {ec.DbColumnName} 类型变化: DB={dc.DataType}, Entity={ec.DataType}");

                            // 字符类型才比较长度
                            if (IsLengthType(string.IsNullOrEmpty(eType) ? dType : eType))
                            {
                                // -1 == nvarchar(max)，当实体未显式给长度（0）时不触发差异
                                var dbLen = dc.Length;
                                var enLen = ec.Length;
                                if (enLen > 0 && dbLen != enLen)
                                    diffs.Add($"列 {ec.DbColumnName} 长度变化: DB={dc.Length}, Entity={ec.Length}");
                            }

                            // Decimal 才比较小数位
                            if (IsDecimalType(string.IsNullOrEmpty(eType) ? dType : eType))
                            {
                                if (ec.Length > 0 && dc.Length != ec.Length)
                                    diffs.Add($"列 {ec.DbColumnName} 精度变化: DB={dc.Length}, Entity={ec.Length}");
                                if (ec.DecimalDigits > 0 && dc.DecimalDigits != ec.DecimalDigits)
                                    diffs.Add($"列 {ec.DbColumnName} 小数位变化: DB={dc.DecimalDigits}, Entity={ec.DecimalDigits}");
                            }
                            if (dc.IsNullable != ec.IsNullable)
                                diffs.Add($"列 {ec.DbColumnName} 可空变化: DB={(dc.IsNullable ? 1:0)}, Entity={(ec.IsNullable ? 1:0)}");
                        }

                        // 3) 可忽略：DB 多余列不触发重建（保留容错）
                        return diffs;
                    }

                    // 全量字段对比日志（逐列输出 DB 与 Entity 的类型/长度/小数/可空）
                    {
                        string NormForLog(string? t)
                        {
                            if (string.IsNullOrWhiteSpace(t)) return string.Empty;
                            t = t.Trim().ToLowerInvariant();
                            var p = t.IndexOf('(');
                            if (p > 0) t = t.Substring(0, p);
                            return t switch
                            {
                                "numeric" => "decimal",
                                "datetime2" => "datetime",
                                "datetimeoffset" => "datetime",
                                "ntext" => "nvarchar",
                                "text" => "varchar",
                                _ => t
                            };
                        }

                        var ignoredForLog = new HashSet<string>(new[]
                        {
                            "id","remarks","created_by","created_time","updated_by","updated_time","deleted_by","deleted_time","is_deleted"
                        }, StringComparer.OrdinalIgnoreCase);

                        _initLog.Information("[结构对比] 表 [{TableName}] —— 开始", tableName);

                        var dbColMap = dbColumns.ToDictionary(c => c.DbColumnName, StringComparer.OrdinalIgnoreCase);
                        foreach (var ec in entityColumns)
                        {
                            if (ignoredForLog.Contains(ec.DbColumnName)) continue;
                            dbColMap.TryGetValue(ec.DbColumnName, out var dc);
                            var dType = dc != null ? NormForLog(dc.DataType) : "(missing)";
                            var eType = NormForLog(ec.DataType);
                            var lenDb = dc?.Length ?? 0;
                            var lenEn = ec.Length;
                            var decDb = dc?.DecimalDigits ?? 0;
                            var decEn = ec.DecimalDigits;
                            var nulDb = dc?.IsNullable ?? true;
                            var nulEn = ec.IsNullable;
                            bool same = (dc != null) &&
                                        (string.IsNullOrEmpty(eType) || dType == eType) &&
                                        (lenDb == lenEn) && (decDb == decEn) && (nulDb == nulEn);
                            _initLog.Information(
                                "[结构对比] {Table}.{Col} => 类型(DB={DbType},Entity={EnType}) 长度(DB={DbLen},Entity={EnLen}) 小数(DB={DbDec},Entity={EnDec}) 可空(DB={DbNull},Entity={EnNull}) [{Status}]",
                                tableName, ec.DbColumnName, dType, eType, lenDb, lenEn, decDb, decEn, nulDb ? 1 : 0, nulEn ? 1 : 0, same ? "OK" : "CHANGED");
                        }

                        // 数据库多余列
                        var entityColNamesSet = new HashSet<string>(entityColumns.Select(c => c.DbColumnName), StringComparer.OrdinalIgnoreCase);
                        foreach (var dc in dbColumns)
                        {
                            if (ignoredForLog.Contains(dc.DbColumnName)) continue;
                            if (!entityColNamesSet.Contains(dc.DbColumnName))
                            {
                                _initLog.Information("[结构对比] {Table} 额外列(仅DB): {Col} 类型={Type} 长度={Len} 小数={Dec} 可空={Null}",
                                    tableName, dc.DbColumnName, NormForLog(dc.DataType), dc.Length, dc.DecimalDigits, dc.IsNullable ? 1 : 0);
                            }
                        }

                        _initLog.Information("[结构对比] 表 [{TableName}] —— 结束", tableName);
                    }

                    var diffs = GetDiffs();
                    if (diffs.Count > 0)
                    {
                        // 输出详细差异
                        foreach (var d in diffs)
                        {
                            _initLog.Warning("[表结构差异] {TableName} -> {Diff}", tableName, d);
                        }
                        _initLog.Warning("⚠️ 表 [{TableName}] 结构发生变化，删除并重建", tableName);
                        db.DbMaintenance.DropTable(tableName);
                    db.CodeFirst.InitTables(entityType);
                        _initLog.Information("✅ 表 [{TableName}] 已重建", tableName);
                    }
                    else
                    {
                        _initLog.Information("✅ 表 [{TableName}] 已存在且结构一致，保持不变", tableName);
                    }
                }
            }

            _initLog.Information("========================================");
            _initLog.Information("📊 数据表初始化/更新完成！");
            _initLog.Information("  - 总计处理: {Total} 个表", entityTypes.Length);
            _initLog.Information("========================================");
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _initLog.Error(ex, "数据表初始化失败");
            throw;
        }
    }

}

