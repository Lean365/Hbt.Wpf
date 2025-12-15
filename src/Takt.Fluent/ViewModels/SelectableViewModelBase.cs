//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : SelectableViewModelBase.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 通用的可选择 ViewModel 基类，用于 DataGrid 多选功能
//===================================================================

using System.Collections.ObjectModel;
using System.Linq;
using Takt.Common.Helpers;
using Takt.Common.Models;

namespace Takt.Fluent.ViewModels;

/// <summary>
/// 可选择 ViewModel 接口（非泛型，用于 UI 绑定）
/// </summary>
public interface ISelectableViewModel
{
    string SearchText { get; set; }
    int SelectedCount { get; }
    string SelectionInfo { get; }
    RelayCommand SelectAllCommand { get; }
    RelayCommand InvertSelectionCommand { get; }
    RelayCommand ClearSelectionCommand { get; }
    void UpdateSelectionState();
    
    // 分页属性（标准规范）
    int PageIndex { get; set; }              // 当前页码（从1开始）
    int PageSize { get; set; }               // 每页大小
    int Total { get; }                       // 总记录数
    int TotalPages { get; }                   // 总页数
    bool HasPreviousPage { get; }            // 是否有上一页
    bool HasNextPage { get; }                // 是否有下一页
    int FirstPageIndex { get; }              // 第一页索引（通常是1）
    int LastPageIndex { get; }               // 最后一页索引
    int StartIndex { get; }                  // 当前页起始记录索引（从1开始）
    int EndIndex { get; }                    // 当前页结束记录索引
    string TotalText { get; }                // 总记录数文本（如 "共 100 条"）
    string PageDisplay { get; }              // 分页显示文本（如 "1 / 10"）
    RelayCommand PrevPageCommand { get; }    // 上一页命令
    RelayCommand NextPageCommand { get; }    // 下一页命令
    RelayCommand FirstPageCommand { get; }   // 第一页命令
    RelayCommand LastPageCommand { get; }    // 最后一页命令
}

/// <summary>
/// 通用的可选择 ViewModel 基类
/// 提供多选、单选、全选、反选等通用功能
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public abstract class SelectableViewModelBase<T> : ViewModelBase, ISelectableViewModel
{
    private ObservableCollection<SelectableItem<T>> _selectableItems = new();
    private SelectionService<T>? _selectionService;
    private string _searchText = string.Empty;
    private int _pageIndex = 1;
    private int _pageSize = 20;
    private int _total = 0;

    /// <summary>
    /// 可选择项集合
    /// </summary>
    public ObservableCollection<SelectableItem<T>> SelectableItems
    {
        get => _selectableItems;
        set
        {
            if (SetProperty(ref _selectableItems, value))
            {
                _selectionService = new SelectionService<T>(_selectableItems);
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(SelectionInfo));
            }
        }
    }

    /// <summary>
    /// 选择服务
    /// </summary>
    public SelectionService<T>? SelectionService => _selectionService;

    /// <summary>
    /// 搜索文本
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                OnSearchTextChanged();
            }
        }
    }

    /// <summary>
    /// 选中数量
    /// </summary>
    public int SelectedCount => _selectionService?.SelectedCount ?? 0;

    /// <summary>
    /// 单选选中的项（只读）
    /// </summary>
    public SelectableItem<T>? SingleSelectedItem => _selectionService?.SingleSelectedItem;

    /// <summary>
    /// 单选选中的项（可写，用于双向绑定）
    /// </summary>
    public SelectableItem<T>? SelectedSelectableItem
    {
        get => _selectionService?.SingleSelectedItem;
        set
        {
            if (_selectionService != null && value != null)
            {
                _selectionService.SelectSingle(value, false);
                OnPropertyChanged(nameof(SingleSelectedItem));
                OnPropertyChanged(nameof(SingleSelectedData));
                UpdateSelectionState();
                // 通知子类可能依赖 SingleSelectedData 的属性（如 SelectedSetting）
                OnPropertyChanged("SelectedSetting");
            }
            else if (_selectionService != null && value == null)
            {
                _selectionService.ClearSingleSelection();
                OnPropertyChanged(nameof(SingleSelectedItem));
                OnPropertyChanged(nameof(SingleSelectedData));
                UpdateSelectionState();
                // 通知子类可能依赖 SingleSelectedData 的属性（如 SelectedSetting）
                OnPropertyChanged("SelectedSetting");
            }
        }
    }

    /// <summary>
    /// 单选选中的原始数据
    /// </summary>
    public T? SingleSelectedData => SingleSelectedItem != null ? SingleSelectedItem.Item : default(T);

    /// <summary>
    /// 选中的原始数据集合
    /// </summary>
    public IEnumerable<T> SelectedDataItems => _selectionService?.SelectedDataItems ?? Enumerable.Empty<T>();

    /// <summary>
    /// 选择信息文本
    /// </summary>
    public virtual string SelectionInfo => $"已选中: {SelectedCount} 项";

    /// <summary>
    /// 全选命令
    /// </summary>
    public RelayCommand SelectAllCommand { get; protected set; } = null!;

    /// <summary>
    /// 反选命令
    /// </summary>
    public RelayCommand InvertSelectionCommand { get; protected set; } = null!;

    /// <summary>
    /// 清除选择命令
    /// </summary>
    public RelayCommand ClearSelectionCommand { get; protected set; } = null!;

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (SetProperty(ref _pageIndex, value))
            {
                OnPropertyChanged(nameof(PageDisplay));
                OnPropertyChanged(nameof(TotalText));
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(FirstPageIndex));
                OnPropertyChanged(nameof(LastPageIndex));
                OnPropertyChanged(nameof(StartIndex));
                OnPropertyChanged(nameof(EndIndex));
                PrevPageCommand.RaiseCanExecuteChanged();
                NextPageCommand.RaiseCanExecuteChanged();
                FirstPageCommand.RaiseCanExecuteChanged();
                LastPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                OnPropertyChanged(nameof(PageDisplay));
                OnPropertyChanged(nameof(TotalText));
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(FirstPageIndex));
                OnPropertyChanged(nameof(LastPageIndex));
                OnPropertyChanged(nameof(StartIndex));
                OnPropertyChanged(nameof(EndIndex));
                PrevPageCommand.RaiseCanExecuteChanged();
                NextPageCommand.RaiseCanExecuteChanged();
                FirstPageCommand.RaiseCanExecuteChanged();
                LastPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total
    {
        get => _total;
        protected set
        {
            if (SetProperty(ref _total, value))
            {
                OnPropertyChanged(nameof(PageDisplay));
                OnPropertyChanged(nameof(TotalText));
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(HasPreviousPage));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(FirstPageIndex));
                OnPropertyChanged(nameof(LastPageIndex));
                OnPropertyChanged(nameof(StartIndex));
                OnPropertyChanged(nameof(EndIndex));
                PrevPageCommand.RaiseCanExecuteChanged();
                NextPageCommand.RaiseCanExecuteChanged();
                FirstPageCommand.RaiseCanExecuteChanged();
                LastPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)Total / Math.Max(1, PageSize)));

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// 第一页索引（通常是1）
    /// </summary>
    public int FirstPageIndex => 1;

    /// <summary>
    /// 最后一页索引
    /// </summary>
    public int LastPageIndex => TotalPages;

    /// <summary>
    /// 当前页起始记录索引（从1开始）
    /// 例如：第1页（PageIndex=1, PageSize=20）的 StartIndex = 1
    /// </summary>
    public int StartIndex => Total == 0 ? 0 : ((PageIndex - 1) * PageSize + 1);

    /// <summary>
    /// 当前页结束记录索引
    /// 例如：第1页（PageIndex=1, PageSize=20, Total=100）的 EndIndex = 20
    /// </summary>
    public int EndIndex
    {
        get
        {
            if (Total == 0) return 0;
            var end = PageIndex * PageSize;
            return end > Total ? Total : end;
        }
    }

    /// <summary>
    /// 总记录数文本
    /// </summary>
    public virtual string TotalText => $"共 {Total} 条";

    /// <summary>
    /// 分页显示文本（标准格式：当前页/总页数，如 "1 / 10"）
    /// </summary>
    public virtual string PageDisplay => $"{PageIndex} / {TotalPages}";

    /// <summary>
    /// 上一页命令
    /// </summary>
    public RelayCommand PrevPageCommand { get; protected set; } = null!;

    /// <summary>
    /// 下一页命令
    /// </summary>
    public RelayCommand NextPageCommand { get; protected set; } = null!;

    /// <summary>
    /// 第一页命令
    /// </summary>
    public RelayCommand FirstPageCommand { get; protected set; } = null!;

    /// <summary>
    /// 最后一页命令
    /// </summary>
    public RelayCommand LastPageCommand { get; protected set; } = null!;

    /// <summary>
    /// 构造函数
    /// </summary>
    protected SelectableViewModelBase()
    {
        InitializeCommands();
    }

    /// <summary>
    /// 初始化命令
    /// </summary>
    protected virtual void InitializeCommands()
    {
        SelectAllCommand = new RelayCommand(
            () => _selectionService?.SelectAll(),
            () => SelectableItems?.Count > 0);

        InvertSelectionCommand = new RelayCommand(
            () => _selectionService?.InvertSelection(),
            () => SelectableItems?.Count > 0);

        ClearSelectionCommand = new RelayCommand(
            () => _selectionService?.ClearAllSelection());

        PrevPageCommand = new RelayCommand(
            () =>
            {
                if (PageIndex > 1)
                {
                    PageIndex--;
                    OnPageChanged();
                }
            },
            () => PageIndex > 1);

        NextPageCommand = new RelayCommand(
            () =>
            {
                if (HasNextPage)
                {
                    PageIndex++;
                    OnPageChanged();
                }
            },
            () => HasNextPage);

        FirstPageCommand = new RelayCommand(
            () =>
            {
                if (PageIndex != FirstPageIndex)
                {
                    PageIndex = FirstPageIndex;
                    OnPageChanged();
                }
            },
            () => PageIndex != FirstPageIndex && Total > 0);

        LastPageCommand = new RelayCommand(
            () =>
            {
                if (PageIndex != LastPageIndex)
                {
                    PageIndex = LastPageIndex;
                    OnPageChanged();
                }
            },
            () => PageIndex != LastPageIndex && Total > 0);
    }

    /// <summary>
    /// 页码改变时的处理（子类可以重写实现数据加载）
    /// </summary>
    protected virtual void OnPageChanged()
    {
        // 子类可以重写实现数据加载逻辑
    }

    /// <summary>
    /// 搜索文本改变时的处理
    /// </summary>
    protected virtual void OnSearchTextChanged()
    {
        UpdateHighlighting();
    }

    /// <summary>
    /// 更新高亮状态（根据搜索文本）
    /// </summary>
    protected virtual void UpdateHighlighting()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // 清除所有高亮
            foreach (var item in SelectableItems)
            {
                item.IsHighlighted = false;
            }
        }
        else
        {
            // 根据搜索文本设置高亮
            foreach (var item in SelectableItems)
            {
                item.IsHighlighted = ShouldHighlight(item, SearchText);
            }
        }
    }

    /// <summary>
    /// 判断是否应该高亮（子类可以重写实现自定义搜索逻辑）
    /// </summary>
    /// <param name="item">可选择项</param>
    /// <param name="searchText">搜索文本</param>
    /// <returns>是否应该高亮</returns>
    protected virtual bool ShouldHighlight(SelectableItem<T> item, string searchText)
    {
        if (item == null || string.IsNullOrWhiteSpace(searchText))
        {
            return false;
        }

        // 默认实现：检查 Item 的 ToString() 是否包含搜索文本（不区分大小写）
        var itemText = item.Item?.ToString() ?? string.Empty;
        return itemText.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 从原始数据创建可选择项集合
    /// </summary>
    /// <param name="items">原始数据集合</param>
    /// <returns>可选择项集合</returns>
    protected ObservableCollection<SelectableItem<T>> CreateSelectableItems(IEnumerable<T> items)
    {
        return new ObservableCollection<SelectableItem<T>>(
            items.Select(item => new SelectableItem<T>(item)));
    }

    /// <summary>
    /// 更新选择状态（当通过其他方式改变选择时调用）
    /// </summary>
    public virtual void UpdateSelectionState()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(SelectionInfo));
        SelectAllCommand.RaiseCanExecuteChanged();
        InvertSelectionCommand.RaiseCanExecuteChanged();
    }
}

