//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : SelectionService.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 通用的选择管理服务，用于 DataGrid 多选功能
//===================================================================

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Takt.Common.Models;

namespace Takt.Common.Helpers;

/// <summary>
/// 通用的选择管理器
/// 用于管理 DataGrid 的多选、单选、全选、反选等功能
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class SelectionService<T>
{
    private readonly ObservableCollection<SelectableItem<T>> _items;
    private SelectableItem<T>? _singleSelectedItem;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="items">可选择项集合</param>
    public SelectionService(ObservableCollection<SelectableItem<T>> items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>
    /// 单选选中的项
    /// </summary>
    public SelectableItem<T>? SingleSelectedItem
    {
        get => _singleSelectedItem;
        set
        {
            if (_singleSelectedItem != null)
            {
                _singleSelectedItem.IsSingleSelected = false;
            }

            _singleSelectedItem = value;

            if (_singleSelectedItem != null)
            {
                _singleSelectedItem.IsSingleSelected = true;
            }
        }
    }

    /// <summary>
    /// 选中的数量
    /// </summary>
    public int SelectedCount => _items?.Count(p => p.IsSelected) ?? 0;

    /// <summary>
    /// 选中的项集合
    /// </summary>
    public IEnumerable<SelectableItem<T>> SelectedItems => _items?.Where(p => p.IsSelected) ?? Enumerable.Empty<SelectableItem<T>>();

    /// <summary>
    /// 选中的原始数据项集合
    /// </summary>
    public IEnumerable<T> SelectedDataItems => SelectedItems.Select(s => s.Item);

    /// <summary>
    /// 全选
    /// </summary>
    public void SelectAll()
    {
        foreach (var item in _items)
        {
            item.IsSelected = true;
        }
    }

    /// <summary>
    /// 反选
    /// </summary>
    public void InvertSelection()
    {
        foreach (var item in _items)
        {
            item.IsSelected = !item.IsSelected;
        }
    }

    /// <summary>
    /// 清除所有选择
    /// </summary>
    public void ClearAllSelection()
    {
        foreach (var item in _items)
        {
            item.IsSelected = false;
        }
        SingleSelectedItem = null;
    }

    /// <summary>
    /// 清除多选
    /// </summary>
    public void ClearMultiSelection()
    {
        foreach (var item in _items)
        {
            item.IsSelected = false;
        }
    }

    /// <summary>
    /// 清除单选
    /// </summary>
    public void ClearSingleSelection()
    {
        SingleSelectedItem = null;
    }

    /// <summary>
    /// 根据条件选择
    /// </summary>
    /// <param name="condition">选择条件</param>
    public void SelectByCondition(Func<SelectableItem<T>, bool> condition)
    {
        foreach (var item in _items)
        {
            item.IsSelected = condition(item);
        }
    }

    /// <summary>
    /// 单选项目
    /// </summary>
    /// <param name="item">要选中的项</param>
    /// <param name="clearMultiSelection">是否清除多选</param>
    public void SelectSingle(SelectableItem<T> item, bool clearMultiSelection = true)
    {
        if (clearMultiSelection)
        {
            ClearMultiSelection();
        }
        SingleSelectedItem = item;
        // 单选时也设置多选状态，以便在 UI 中显示选中
        if (item != null)
        {
            item.IsSelected = true;
        }
    }

    /// <summary>
    /// 切换选择状态
    /// </summary>
    /// <param name="item">要切换的项</param>
    public void ToggleSelection(SelectableItem<T> item)
    {
        if (item != null)
        {
            item.IsSelected = !item.IsSelected;
        }
    }
}

