// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Common.Models
// 文件名称：ColumnInfo.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：数据库列信息模型
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Common.Models;

/// <summary>
/// 数据库列信息模型
/// 用于描述数据库表的列结构信息
/// </summary>
public class ColumnInfo
{
    /// <summary>
    /// 列名（数据库列名）
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// 是否主键
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// 是否自增
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// 是否可空
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// 列长度
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// 小数位数
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 列描述/注释
    /// </summary>
    public string? Description { get; set; }
}

