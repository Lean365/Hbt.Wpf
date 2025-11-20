// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Domain.Entities
// 文件名称：BaseEntity.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：实体基类
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities;

/// <summary>
/// 实体基类
/// 定义所有实体的公共字段
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    /// <remarks>
    /// 雪花ID（SnowflakeId），全局唯一
    /// </remarks>
    [SugarColumn(ColumnName = "id", ColumnDescription = "主键ID", IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>
    /// 备注
    /// 通用备注信息
    /// </summary>
    [SugarColumn(ColumnName = "remarks", ColumnDescription = "备注", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? Remarks { get; set; }

    /// <summary>
    /// 创建人
    /// 记录的创建者
    /// </summary>
    [SugarColumn(ColumnName = "created_by", ColumnDescription = "创建人", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// 记录的创建时间
    /// </summary>
    [SugarColumn(ColumnName = "created_time", ColumnDescription = "创建时间", IsNullable = false)]
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新人
    /// 记录的最后更新者
    /// </summary>
    [SugarColumn(ColumnName = "updated_by", ColumnDescription = "更新人", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// 更新时间
    /// 记录的最后更新时间
    /// </summary>
    [SugarColumn(ColumnName = "updated_time", ColumnDescription = "更新时间", IsNullable = false)]
    public DateTime UpdatedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否删除（逻辑删除标记）
    /// 0=否（未删除），1=是（已删除）
    /// </summary>
    [SugarColumn(ColumnName = "is_deleted", ColumnDescription = "是否删除", ColumnDataType = "int", DefaultValue = "0", IsNullable = false)]
    public int IsDeleted { get; set; } = 0;

    /// <summary>
    /// 删除人
    /// 执行删除操作的用户
    /// </summary>
    [SugarColumn(ColumnName = "deleted_by", ColumnDescription = "删除人", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? DeletedBy { get; set; }

    /// <summary>
    /// 删除时间
    /// 执行删除操作的时间
    /// </summary>
    [SugarColumn(ColumnName = "deleted_time", ColumnDescription = "删除时间", IsNullable = true)]
    public DateTime? DeletedTime { get; set; }
}
