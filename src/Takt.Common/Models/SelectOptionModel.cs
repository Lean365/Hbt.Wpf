//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : SelectOptionModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 通用选项模型（用于下拉列表等UI组件）
//===================================================================

namespace Takt.Common.Models;

/// <summary>
/// 通用选项模型
/// 用于下拉列表、单选按钮组、多选列表等UI组件
/// </summary>
/// <typeparam name="TValue">选项值类型</typeparam>
public class SelectOptionModel<TValue>
{
    /// <summary>
    /// 选项值（Value）
    /// </summary>
    public TValue DataValue { get; set; } = default!;

    /// <summary>
    /// 选项标签（Label - 显示文本）
    /// </summary>
    public string DataLabel { get; set; } = string.Empty;

    /// <summary>
    /// 扩展标签（ExtLabel - 补充信息）
    /// </summary>
    public string? ExtLabel { get; set; }

    /// <summary>
    /// 扩展值（ExtValue - 额外数据）
    /// </summary>
    public string? ExtValue { get; set; }

    /// <summary>
    /// CSS类名（用于自定义样式）
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// 列表类名（用于列表容器样式）
    /// </summary>
    public string? ListClass { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 附加数据（可选）
    /// </summary>
    public object? Extra { get; set; }
}

/// <summary>
/// 字符串类型选项模型
/// </summary>
public class SelectOptionModel : SelectOptionModel<string>
{
    /// <summary>
    /// 创建选项模型
    /// </summary>
    /// <param name="dataValue">选项值</param>
    /// <param name="dataLabel">选项标签</param>
    /// <param name="orderNum">排序号</param>
    /// <param name="extLabel">扩展标签</param>
    /// <param name="extValue">扩展值</param>
    /// <param name="cssClass">CSS类名</param>
    /// <param name="listClass">列表类名</param>
    public static SelectOptionModel Create(
        string dataValue, 
        string dataLabel, 
        int orderNum = 0,
        string? extLabel = null!, 
        string? extValue = null!, 
        string? cssClass = null!, 
        string? listClass = null)
    {
        return new SelectOptionModel
        {
            DataValue = dataValue,
            DataLabel = dataLabel,
            OrderNum = orderNum,
            ExtLabel = extLabel,
            ExtValue = extValue,
            CssClass = cssClass,
            ListClass = listClass
        };
    }
}

