//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Logging
// 文件名称：DiffLog.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 功能描述：差异日志实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Logging;

/// <summary>
/// 差异日志实体
/// </summary>
/// <remarks>
/// 记录数据变更前后的差异（SqlSugar AOP自动生成）
/// </remarks>
[SugarTable("takt_logging_diff_log", "差异日志表")]
[SugarIndex("IX_takt_logging_diff_log_table_name", nameof(DiffLog.TableName), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_diff_log_diff_type", nameof(DiffLog.DiffType), OrderByType.Asc, false)]
[SugarIndex("IX_takt_logging_diff_log_diff_time", nameof(DiffLog.DiffTime), OrderByType.Desc, false)]
[SugarIndex("IX_takt_logging_diff_log_created_time", nameof(DiffLog.CreatedTime), OrderByType.Desc, false)]
public class DiffLog : BaseEntity
{
    /// <summary>
    /// 表名
    /// </summary>
    [SugarColumn(ColumnName = "table_name", ColumnDescription = "表名", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 差异类型
    /// </summary>
    /// <remarks>
    /// insert, update, delete
    /// </remarks>
    [SugarColumn(ColumnName = "diff_type", ColumnDescription = "差异类型", ColumnDataType = "nvarchar", Length = 20, IsNullable = false)]
    public string DiffType { get; set; } = string.Empty;

    /// <summary>
    /// 变更前数据
    /// </summary>
    [SugarColumn(ColumnName = "before_data", ColumnDescription = "变更前数据", ColumnDataType = "nvarchar", IsNullable = true)]
    public string? BeforeData { get; set; }

    /// <summary>
    /// 变更后数据
    /// </summary>
    [SugarColumn(ColumnName = "after_data", ColumnDescription = "变更后数据", ColumnDataType = "nvarchar", IsNullable = true)]
    public string? AfterData { get; set; }

    /// <summary>
    /// 执行SQL
    /// </summary>
    [SugarColumn(ColumnName = "sql", ColumnDescription = "执行SQL", ColumnDataType = "nvarchar", IsNullable = true)]
    public string? Sql { get; set; }

    /// <summary>
    /// SQL参数
    /// </summary>
    [SugarColumn(ColumnName = "parameters", ColumnDescription = "SQL参数", ColumnDataType = "nvarchar", IsNullable = true)]
    public string? Parameters { get; set; }

    /// <summary>
    /// 差异时间
    /// </summary>
    [SugarColumn(ColumnName = "diff_time", ColumnDescription = "差异时间", IsNullable = false)]
    public DateTime DiffTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 执行耗时
    /// </summary>
    /// <remarks>
    /// 单位：毫秒
    /// </remarks>
    [SugarColumn(ColumnName = "elapsed_time", ColumnDescription = "执行耗时", ColumnDataType = "int", IsNullable = false)]
    public int ElapsedTime { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "username", ColumnDescription = "用户名", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? Username { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [SugarColumn(ColumnName = "ip_address", ColumnDescription = "IP地址", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? IpAddress { get; set; }
}

