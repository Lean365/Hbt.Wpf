// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：VisitorDetail.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：访客详情实体
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

using SqlSugar;

namespace Hbt.Domain.Entities.Logistics.Visitors;

/// <summary>
/// 访客详情实体
/// 访客的详细信息
/// </summary>
[SugarTable("hbt_logistics_visitor_detail", "访客详情表")]
[SugarIndex("IX_hbt_logistics_visitor_detail_visitor_id", nameof(VisitorDetail.VisitorId), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_visitor_detail_name", nameof(VisitorDetail.Name), OrderByType.Asc, false)]
[SugarIndex("IX_hbt_logistics_visitor_detail_created_time", nameof(VisitorDetail.CreatedTime), OrderByType.Desc, false)]
public class VisitorDetail : BaseEntity
{
    /// <summary>
    /// 访客ID
    /// 关联的访客主表ID
    /// </summary>
    [SugarColumn(ColumnName = "visitor_id", ColumnDescription = "访客ID", IsNullable = false)]
    public long VisitorId { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    [SugarColumn(ColumnName = "department", ColumnDescription = "部门", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "姓名", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 职务
    /// </summary>
    [SugarColumn(ColumnName = "position", ColumnDescription = "职务", ColumnDataType = "nvarchar", Length = 50, IsNullable = false)]
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// 关联的访客主表（导航属性）
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(VisitorId))]
    public Visitor? Visitor { get; set; }
}
