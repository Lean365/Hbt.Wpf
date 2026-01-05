// ========================================
// 项目名称：Takt.Wpf
// 文件名称：PagedResult.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：分页结果封装类
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
/// 分页结果封装类
/// 用于封装分页查询结果
/// </summary>
/// <typeparam name="T">数据项类型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// 数据项列表
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalNum { get; set; }
    
    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalNum / PageSize);
    
    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious => PageIndex > 1;
    
    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext => PageIndex < TotalPages;
}
