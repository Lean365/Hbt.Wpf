//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Domain.Entities.Routine
// 文件名 : Setting.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-17
// 功能描述：系统设置实体
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using SqlSugar;

namespace Takt.Domain.Entities.Routine;

/// <summary>
/// 系统设置实体
/// 用于存储系统级别的配置参数
/// </summary>
[SugarTable("takt_routine_setting", "系统设置表")]
[SugarIndex("IX_takt_routine_setting_key", nameof(SettingKey), OrderByType.Asc, true)]
[SugarIndex("IX_takt_routine_setting_category", nameof(Category), OrderByType.Asc, false)]
public class Setting : BaseEntity
{
    /// <summary>
    /// 设置键
    /// 系统设置的唯一标识
    /// </summary>
    [SugarColumn(ColumnName = "setting_key", ColumnDescription = "设置键", ColumnDataType = "nvarchar", Length = 100, IsNullable = false)]
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// 设置值
    /// 系统设置的值
    /// </summary>
    [SugarColumn(ColumnName = "setting_value", ColumnDescription = "设置值", ColumnDataType = "nvarchar", Length = 2000, IsNullable = false)]
    public string SettingValue { get; set; } = string.Empty;

    /// <summary>
    /// 分类
    /// 设置所属的分类，如：System, Security, Email
    /// </summary>
    [SugarColumn(ColumnName = "category", ColumnDescription = "分类", ColumnDataType = "nvarchar", Length = 50, IsNullable = true)]
    public string? Category { get; set; }

    /// <summary>
    /// 排序号
    /// 用于控制显示顺序
    /// </summary>
    [SugarColumn(ColumnName = "order_num", ColumnDescription = "排序号", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 设置描述
    /// 系统设置的说明信息
    /// </summary>
    [SugarColumn(ColumnName = "setting_description", ColumnDescription = "设置描述", ColumnDataType = "nvarchar", Length = 500, IsNullable = true)]
    public string? SettingDescription { get; set; }

    /// <summary>
    /// 设置类型
    /// 0=字符串, 1=数字, 2=布尔值, 3=JSON
    /// </summary>
    [SugarColumn(ColumnName = "setting_type", ColumnDescription = "设置类型", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int SettingType { get; set; } = 0;

    /// <summary>
    /// 是否内置
    /// 0=是，1=否（内置数据不可删除）
    /// </summary>
    [SugarColumn(ColumnName = "is_builtin", ColumnDescription = "是否内置", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsBuiltin { get; set; } = 1;

    /// <summary>
    /// 是否默认
    /// 0=是，1=否
    /// </summary>
    [SugarColumn(ColumnName = "is_default", ColumnDescription = "是否默认", ColumnDataType = "int", IsNullable = false, DefaultValue = "1")]
    public int IsDefault { get; set; } = 1;

    /// <summary>
    /// 是否可修改
    /// 0=是，1=否
    /// </summary>
    [SugarColumn(ColumnName = "is_editable", ColumnDescription = "是否可修改", ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int IsEditable { get; set; } = 0;
}

