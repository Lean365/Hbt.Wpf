//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DocumentTabItem.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : MDI 文档标签页数据模型
//===================================================================

using Hbt.Application.Dtos.Identity;

namespace Hbt.Fluent.Models;

/// <summary>
/// MDI 文档标签页项
/// </summary>
public class DocumentTabItem
{
    /// <summary>
    /// 菜单信息
    /// </summary>
    public MenuDto MenuItem { get; set; } = null!;

    /// <summary>
    /// 标签页标题（本地化后的菜单名称）
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 视图实例
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// 视图类型名称（用于判断是否已打开）
    /// </summary>
    public string ViewTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 是否已修改（用于显示保存提示）
    /// </summary>
    public bool IsModified { get; set; }

    /// <summary>
    /// 是否可以关闭（默认仪表盘标签页不允许关闭）
    /// </summary>
    public bool CanClose { get; set; } = true;

    public DocumentTabItem(MenuDto menuItem, string title, object content, string viewTypeName)
    {
        // 参数验证
        MenuItem = menuItem ?? throw new ArgumentNullException(nameof(menuItem));
        Title = title ?? string.Empty;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        ViewTypeName = viewTypeName ?? throw new ArgumentNullException(nameof(viewTypeName));
        Icon = menuItem.Icon;
        
        // 默认仪表盘标签页不允许关闭
        // 判断条件：MenuCode 为 "dashboard" 或 ViewTypeName 包含 "Dashboard.DashboardView"
        CanClose = menuItem.MenuCode?.ToLowerInvariant() != "dashboard" 
                   && !viewTypeName.Contains("Dashboard.DashboardView", StringComparison.OrdinalIgnoreCase);
    }
}

