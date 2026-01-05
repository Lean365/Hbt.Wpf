// ========================================
// 项目名称：Takt.Wpf
// 文件名称：PagedQuery.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：分页查询基类
// 
// 版权信息：
// Copyright (c) 2025 Takt All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

namespace Takt.Common.Results;

/// <summary>
/// 分页查询基类
/// 用于所有查询DTO的基类
/// 包含分页和排序的基础字段，其他查询字段应在具体的 QueryDto 中定义
/// </summary>
public abstract class PagedQuery
{
    /// <summary>
    /// 页码
    /// </summary>
    public int PageIndex { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 排序字段
    /// </summary>
    public string? OrderBy { get; set; }
    
    /// <summary>
    /// 排序方向（asc/desc）
    /// </summary>
    public string? OrderDirection { get; set; } = "desc";
}
