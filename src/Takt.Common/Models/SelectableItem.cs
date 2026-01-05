//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : SelectableItem.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 2.0
// 描述    : 通用的可选择项包装类，用于 DataGrid 多选功能，支持行颜色、单选状态、高亮状态
//===================================================================

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Takt.Common.Models;

/// <summary>
/// 通用的可选择项包装类
/// 用于包装任何类型的数据，添加选择状态支持、行颜色、单选状态、高亮状态
/// </summary>
/// <typeparam name="T">被包装的数据类型</typeparam>
public class SelectableItem<T> : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isSingleSelected;
    private bool _isHighlighted;
    private T _item;

    /// <summary>
    /// 被包装的数据项
    /// </summary>
    public T Item
    {
        get => _item;
        set
        {
            if (!Equals(_item, value))
            {
                _item = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 是否被选中（多选状态）
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                OnSelectionChanged();
            }
        }
    }

    /// <summary>
    /// 是否被单选（单选状态）
    /// </summary>
    public bool IsSingleSelected
    {
        get => _isSingleSelected;
        set
        {
            if (_isSingleSelected != value)
            {
                _isSingleSelected = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 是否高亮（用于搜索等）
    /// </summary>
    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            if (_isHighlighted != value)
            {
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }
    }


    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="item">被包装的数据项</param>
    /// <param name="isSelected">初始选中状态</param>
    public SelectableItem(T item, bool isSelected = false)
    {
        _item = item;
        _isSelected = isSelected;
    }

    /// <summary>
    /// 选择状态改变时的回调（子类可以重写）
    /// </summary>
    protected virtual void OnSelectionChanged()
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

