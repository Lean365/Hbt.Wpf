// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktInlineEditDataGrid.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：支持行内编辑的数据表格控件（完全参照 CodeGenForm.xaml 实现）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using MaterialDesignThemes.Wpf;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Takt.Fluent;
using Takt.Fluent.ViewModels;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.Controls;

/// <summary>
/// 支持行内编辑的数据表格控件（完全参照 CodeGenForm.xaml 实现）
/// </summary>
public partial class TaktInlineEditDataGrid : UserControl
{
    private DataGrid? _dataGrid;
    private DataGridTemplateColumn? _operationColumn;
    private bool _isApplyingColumns;
    private INotifyCollectionChanged? _itemsSourceNotifier;

    public TaktInlineEditDataGrid()
    {
        var resourceLocator = new Uri("/Takt.Fluent;component/Controls/TaktInlineEditDataGrid.xaml", UriKind.Relative);
        System.Windows.Application.LoadComponent(this, resourceLocator);

        if (GetValue(ColumnsProperty) is not ObservableCollection<TaktDataGridColumnDefinition> columns || columns == null)
        {
            columns = new ObservableCollection<TaktDataGridColumnDefinition>();
            SetCurrentValue(ColumnsProperty, columns);
        }

        AttachColumnHandlers(columns);

        // 初始化新增行命令（如果没有外部设置）
        if (GetValue(AddRowCommandProperty) == null)
        {
            SetCurrentValue(AddRowCommandProperty, new ViewModels.RelayCommand<object?>(_ => OnAddRow(), _ => CanAddRow()));
        }

        // 初始化编辑命令（如果没有外部设置）
        if (GetValue(RowUpdateCommandProperty) == null)
        {
            SetCurrentValue(RowUpdateCommandProperty, new ViewModels.RelayCommand<object?>(param => OnRowUpdate(param), _ => true));
        }

        // 初始化取消命令（如果没有外部设置）
        if (GetValue(RowCancelCommandProperty) == null)
        {
            SetCurrentValue(RowCancelCommandProperty, new ViewModels.RelayCommand<object?>(_ => OnRowCancel(), _ => true));
        }

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        // 初始化分页
        UpdatePagination();
    }

    #region 依赖属性

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<TaktDataGridColumnDefinition>), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null, OnColumnsChanged));

    public ObservableCollection<TaktDataGridColumnDefinition> Columns
    {
        get
        {
            if (GetValue(ColumnsProperty) is not ObservableCollection<TaktDataGridColumnDefinition> value || value == null)
            {
                value = new ObservableCollection<TaktDataGridColumnDefinition>();
                SetCurrentValue(ColumnsProperty, value);
            }
            return value;
        }
        set => SetValue(ColumnsProperty, value);
    }

    // 编辑状态控制（参照 CodeGenForm）
    public static readonly DependencyProperty EditingItemProperty =
        DependencyProperty.Register(nameof(EditingItem), typeof(object), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null, OnEditingItemChanged));

    public object? EditingItem
    {
        get => GetValue(EditingItemProperty);
        set => SetValue(EditingItemProperty, value);
    }

    private static void OnEditingItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktInlineEditDataGrid grid && grid._dataGrid != null)
        {
            // 通知命令重新评估 CanExecute
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    // 操作命令（参照 CodeGenForm）
    public static readonly DependencyProperty RowUpdateCommandProperty =
        DependencyProperty.Register(nameof(RowUpdateCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null, OnOperationPropertyChanged));

    public ICommand? RowUpdateCommand
    {
        get => (ICommand?)GetValue(RowUpdateCommandProperty);
        set => SetValue(RowUpdateCommandProperty, value);
    }

    public static readonly DependencyProperty RowSaveCommandProperty =
        DependencyProperty.Register(nameof(RowSaveCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null, OnOperationPropertyChanged));

    public ICommand? RowSaveCommand
    {
        get => (ICommand?)GetValue(RowSaveCommandProperty);
        set => SetValue(RowSaveCommandProperty, value);
    }

    public static readonly DependencyProperty RowCancelCommandProperty =
        DependencyProperty.Register(nameof(RowCancelCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null, OnOperationPropertyChanged));

    public ICommand? RowCancelCommand
    {
        get => (ICommand?)GetValue(RowCancelCommandProperty);
        set => SetValue(RowCancelCommandProperty, value);
    }

    public static readonly DependencyProperty RowDeleteCommandProperty =
        DependencyProperty.Register(nameof(RowDeleteCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null, OnOperationPropertyChanged));

    public ICommand? RowDeleteCommand
    {
        get => (ICommand?)GetValue(RowDeleteCommandProperty);
        set => SetValue(RowDeleteCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowUpdateProperty =
        DependencyProperty.Register(nameof(ShowRowUpdate), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true, OnOperationPropertyChanged));

    public bool ShowRowUpdate
    {
        get => (bool)GetValue(ShowRowUpdateProperty);
        set => SetValue(ShowRowUpdateProperty, value);
    }

    public static readonly DependencyProperty ShowRowSaveProperty =
        DependencyProperty.Register(nameof(ShowRowSave), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true, OnOperationPropertyChanged));

    public bool ShowRowSave
    {
        get => (bool)GetValue(ShowRowSaveProperty);
        set => SetValue(ShowRowSaveProperty, value);
    }

    public static readonly DependencyProperty ShowRowCancelProperty =
        DependencyProperty.Register(nameof(ShowRowCancel), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true, OnOperationPropertyChanged));

    public bool ShowRowCancel
    {
        get => (bool)GetValue(ShowRowCancelProperty);
        set => SetValue(ShowRowCancelProperty, value);
    }

    public static readonly DependencyProperty ShowRowDeleteProperty =
        DependencyProperty.Register(nameof(ShowRowDelete), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true, OnOperationPropertyChanged));

    public bool ShowRowDelete
    {
        get => (bool)GetValue(ShowRowDeleteProperty);
        set => SetValue(ShowRowDeleteProperty, value);
    }

    public static readonly DependencyProperty OperationColumnHeaderProperty =
        DependencyProperty.Register(nameof(OperationColumnHeader), typeof(object), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Translate("common.operation", "操作"), OnOperationPropertyChanged));

    public object OperationColumnHeader
    {
        get => GetValue(OperationColumnHeaderProperty);
        set => SetValue(OperationColumnHeaderProperty, value);
    }

    public static readonly DependencyProperty OperationColumnWidthProperty =
        DependencyProperty.Register(nameof(OperationColumnWidth), typeof(double), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(120.0, OnOperationPropertyChanged));

    public double OperationColumnWidth
    {
        get => (double)GetValue(OperationColumnWidthProperty);
        set => SetValue(OperationColumnWidthProperty, value);
    }

    private static readonly DependencyPropertyKey IsOperationColumnVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsOperationColumnVisible), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsOperationColumnVisibleProperty = IsOperationColumnVisiblePropertyKey.DependencyProperty;

    public bool IsOperationColumnVisible
    {
        get => (bool)GetValue(IsOperationColumnVisibleProperty);
        private set => SetValue(IsOperationColumnVisiblePropertyKey, value);
    }

    // 查询相关
    public static readonly DependencyProperty ShowQueryAreaProperty =
        DependencyProperty.Register(nameof(ShowQueryArea), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true));

    public bool ShowQueryArea
    {
        get => (bool)GetValue(ShowQueryAreaProperty);
        set => SetValue(ShowQueryAreaProperty, value);
    }

    public static readonly DependencyProperty QueryKeywordProperty =
        DependencyProperty.Register(nameof(QueryKeyword), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(string.Empty));

    public string QueryKeyword
    {
        get => (string)GetValue(QueryKeywordProperty);
        set => SetValue(QueryKeywordProperty, value);
    }

    public static readonly DependencyProperty QueryPlaceholderProperty =
        DependencyProperty.Register(nameof(QueryPlaceholder), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Translate("common.placeholder.keywordHint", "请输入关键字进行搜索")));

    public string QueryPlaceholder
    {
        get => (string)GetValue(QueryPlaceholderProperty);
        set => SetValue(QueryPlaceholderProperty, value);
    }

    public static readonly DependencyProperty QueryButtonTextProperty =
        DependencyProperty.Register(nameof(QueryButtonText), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Translate("common.button.query", "查询")));

    public string QueryButtonText
    {
        get => (string)GetValue(QueryButtonTextProperty);
        set => SetValue(QueryButtonTextProperty, value);
    }

    public static readonly DependencyProperty ResetButtonTextProperty =
        DependencyProperty.Register(nameof(ResetButtonText), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Translate("common.button.reset", "重置")));

    public string ResetButtonText
    {
        get => (string)GetValue(ResetButtonTextProperty);
        set => SetValue(ResetButtonTextProperty, value);
    }

    public static readonly DependencyProperty QueryCommandProperty =
        DependencyProperty.Register(nameof(QueryCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? QueryCommand
    {
        get => (ICommand?)GetValue(QueryCommandProperty);
        set => SetValue(QueryCommandProperty, value);
    }

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? ResetCommand
    {
        get => (ICommand?)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    // 工具栏
    public static readonly DependencyProperty ShowToolbarProperty =
        DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true));

    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    public static readonly DependencyProperty IsColumnPanelOpenProperty =
        DependencyProperty.Register(nameof(IsColumnPanelOpen), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(false));

    public bool IsColumnPanelOpen
    {
        get => (bool)GetValue(IsColumnPanelOpenProperty);
        set => SetValue(IsColumnPanelOpenProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarAddRowProperty =
        DependencyProperty.Register(nameof(ShowToolbarAddRow), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true));

    public bool ShowToolbarAddRow
    {
        get => (bool)GetValue(ShowToolbarAddRowProperty);
        set => SetValue(ShowToolbarAddRowProperty, value);
    }

    public static readonly DependencyProperty AddRowCommandProperty =
        DependencyProperty.Register(nameof(AddRowCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? AddRowCommand
    {
        get => (ICommand?)GetValue(AddRowCommandProperty);
        set => SetValue(AddRowCommandProperty, value);
    }

    // 样式
    public static readonly DependencyProperty RowStyleProperty =
        DependencyProperty.Register(nameof(RowStyle), typeof(Style), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public Style? RowStyle
    {
        get => (Style?)GetValue(RowStyleProperty);
        set => SetValue(RowStyleProperty, value);
    }

    public static readonly DependencyProperty CellStyleProperty =
        DependencyProperty.Register(nameof(CellStyle), typeof(Style), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public Style? CellStyle
    {
        get => (Style?)GetValue(CellStyleProperty);
        set => SetValue(CellStyleProperty, value);
    }

    public static readonly DependencyProperty ColumnHeaderStyleProperty =
        DependencyProperty.Register(nameof(ColumnHeaderStyle), typeof(Style), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public Style? ColumnHeaderStyle
    {
        get => (Style?)GetValue(ColumnHeaderStyleProperty);
        set => SetValue(ColumnHeaderStyleProperty, value);
    }

    // 空状态
    private static readonly DependencyPropertyKey IsEmptyPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        private set => SetValue(IsEmptyPropertyKey, value);
    }

    public static readonly DependencyProperty EmptyTextProperty =
        DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Translate("common.noData", "暂无数据")));

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    #endregion

    #region 依赖属性 - 分页

    public static readonly DependencyProperty ShowPaginationProperty =
        DependencyProperty.Register(nameof(ShowPagination), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(false));

    public bool ShowPagination
    {
        get => (bool)GetValue(ShowPaginationProperty);
        set => SetValue(ShowPaginationProperty, value);
    }

    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(1, OnPaginationPropertyChanged));

    public int PageIndex
    {
        get => (int)GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(20, OnPaginationPropertyChanged));

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(0, OnPaginationPropertyChanged));

    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public static readonly DependencyProperty TotalTextProperty =
        DependencyProperty.Register(nameof(TotalText), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Format("common.total", "共 {0} 条记录", 0)));

    public string TotalText
    {
        get => (string)GetValue(TotalTextProperty);
        set => SetValue(TotalTextProperty, value);
    }

    public static readonly DependencyProperty PageDisplayProperty =
        DependencyProperty.Register(nameof(PageDisplay), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Format("common.pageDisplay", "第 {0} / {1} 页", 1, 1)));

    public string PageDisplay
    {
        get => (string)GetValue(PageDisplayProperty);
        set => SetValue(PageDisplayProperty, value);
    }

    public static readonly DependencyProperty GoToPageTextProperty =
        DependencyProperty.Register(nameof(GoToPageText), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(string.Empty));

    public string GoToPageText
    {
        get => (string)GetValue(GoToPageTextProperty);
        set => SetValue(GoToPageTextProperty, value);
    }

    public static readonly DependencyProperty GoToPageHintProperty =
        DependencyProperty.Register(nameof(GoToPageHint), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Translate("common.goToPageHint", "页码")));

    public string GoToPageHint
    {
        get => (string)GetValue(GoToPageHintProperty);
        set => SetValue(GoToPageHintProperty, value);
    }

    public static readonly DependencyProperty GoToPageButtonTextProperty =
        DependencyProperty.Register(nameof(GoToPageButtonText), typeof(string), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(Translate("common.goTo", "跳转")));

    public string GoToPageButtonText
    {
        get => (string)GetValue(GoToPageButtonTextProperty);
        set => SetValue(GoToPageButtonTextProperty, value);
    }

    private static readonly DependencyPropertyKey PageSizeOptionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PageSizeOptions), typeof(ObservableCollection<int>), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(new ObservableCollection<int> { 10, 20, 50, 100, 200 }));

    public static readonly DependencyProperty PageSizeOptionsProperty = PageSizeOptionsPropertyKey.DependencyProperty;

    public ObservableCollection<int> PageSizeOptions
    {
        get => (ObservableCollection<int>)GetValue(PageSizeOptionsProperty);
        private set => SetValue(PageSizeOptionsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey PageButtonsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(PageButtons), typeof(ObservableCollection<PageButtonInfo>), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(new ObservableCollection<PageButtonInfo>()));

    public static readonly DependencyProperty PageButtonsProperty = PageButtonsPropertyKey.DependencyProperty;

    public ObservableCollection<PageButtonInfo> PageButtons
    {
        get => (ObservableCollection<PageButtonInfo>)GetValue(PageButtonsProperty);
        private set => SetValue(PageButtonsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasPreviousPagePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasPreviousPage), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasPreviousPageProperty = HasPreviousPagePropertyKey.DependencyProperty;

    public bool HasPreviousPage
    {
        get => (bool)GetValue(HasPreviousPageProperty);
        private set => SetValue(HasPreviousPagePropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasNextPagePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasNextPage), typeof(bool), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasNextPageProperty = HasNextPagePropertyKey.DependencyProperty;

    public bool HasNextPage
    {
        get => (bool)GetValue(HasNextPageProperty);
        private set => SetValue(HasNextPagePropertyKey, value);
    }

    public static readonly DependencyProperty PageChangedCommandProperty =
        DependencyProperty.Register(nameof(PageChangedCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? PageChangedCommand
    {
        get => (ICommand?)GetValue(PageChangedCommandProperty);
        set => SetValue(PageChangedCommandProperty, value);
    }

    private static void OnPaginationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktInlineEditDataGrid grid)
        {
            grid.UpdatePagination();
        }
    }

    private void UpdatePagination()
    {
        if (!ShowPagination)
        {
            return;
        }

        var totalPages = TotalCount > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
        PageIndex = Math.Max(1, Math.Min(PageIndex, totalPages));

        TotalText = Format("common.total", "共 {0} 条记录", TotalCount);
        PageDisplay = Format("common.pageDisplay", "第 {0} / {1} 页", PageIndex, totalPages);

        HasPreviousPage = PageIndex > 1;
        HasNextPage = PageIndex < totalPages;

        UpdatePageButtons(totalPages);
    }

    private void UpdatePageButtons(int totalPages)
    {
        PageButtons.Clear();

        if (totalPages <= 0)
        {
            return;
        }

        const int maxButtons = 7;
        int startPage = 1;
        int endPage = totalPages;

        if (totalPages > maxButtons)
        {
            int halfButtons = maxButtons / 2;
            startPage = Math.Max(1, PageIndex - halfButtons);
            endPage = Math.Min(totalPages, startPage + maxButtons - 1);

            if (endPage - startPage < maxButtons - 1)
            {
                startPage = Math.Max(1, endPage - maxButtons + 1);
            }
        }

        for (int i = startPage; i <= endPage; i++)
        {
            PageButtons.Add(new PageButtonInfo
            {
                Number = i,
                IsCurrent = i == PageIndex
            });
        }
    }

    public sealed record PageButtonInfo
    {
        public int Number { get; init; }
        public bool IsCurrent { get; init; }
    }

    #endregion

    #region 分页命令

    public static readonly DependencyProperty FirstPageCommandProperty =
        DependencyProperty.Register(nameof(FirstPageCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? FirstPageCommand
    {
        get => (ICommand?)GetValue(FirstPageCommandProperty);
        set => SetValue(FirstPageCommandProperty, value);
    }

    public static readonly DependencyProperty PrevPageCommandProperty =
        DependencyProperty.Register(nameof(PrevPageCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? PrevPageCommand
    {
        get => (ICommand?)GetValue(PrevPageCommandProperty);
        set => SetValue(PrevPageCommandProperty, value);
    }

    public static readonly DependencyProperty NextPageCommandProperty =
        DependencyProperty.Register(nameof(NextPageCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? NextPageCommand
    {
        get => (ICommand?)GetValue(NextPageCommandProperty);
        set => SetValue(NextPageCommandProperty, value);
    }

    public static readonly DependencyProperty LastPageCommandProperty =
        DependencyProperty.Register(nameof(LastPageCommand), typeof(ICommand), typeof(TaktInlineEditDataGrid),
            new PropertyMetadata(null));

    public ICommand? LastPageCommand
    {
        get => (ICommand?)GetValue(LastPageCommandProperty);
        set => SetValue(LastPageCommandProperty, value);
    }

    #endregion

    #region 事件处理

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _dataGrid = this.FindName("PART_DataGrid") as DataGrid;
        if (_dataGrid != null)
        {
            EnsureOperationColumn();
            ApplyColumns();
            UpdateEmptyState();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DetachItemsSourceHandlers();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktInlineEditDataGrid grid)
        {
            grid.DetachItemsSourceHandlers();
            grid.AttachItemsSourceHandlers(e.NewValue);
            grid.UpdateEmptyState();
        }
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktInlineEditDataGrid grid)
        {
            if (e.OldValue is ObservableCollection<TaktDataGridColumnDefinition> oldCollection)
            {
                grid.DetachColumnHandlers(oldCollection);
            }

            if (e.NewValue is ObservableCollection<TaktDataGridColumnDefinition> newCollection)
            {
                grid.AttachColumnHandlers(newCollection);
            }
            else
            {
                var collection = new ObservableCollection<TaktDataGridColumnDefinition>();
                grid.SetCurrentValue(ColumnsProperty, collection);
                grid.AttachColumnHandlers(collection);
            }

            grid.ApplyColumns();
        }
    }

    private static void OnOperationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktInlineEditDataGrid grid && grid.IsLoaded)
        {
            grid.EnsureOperationColumn();
        }
    }

    #endregion

    #region 列处理

    private void AttachColumnHandlers(ObservableCollection<TaktDataGridColumnDefinition> collection)
    {
        collection.CollectionChanged += Columns_CollectionChanged;
        foreach (var column in collection)
        {
            column.PropertyChanged += Column_PropertyChanged;
        }
    }

    private void DetachColumnHandlers(ObservableCollection<TaktDataGridColumnDefinition> collection)
    {
        collection.CollectionChanged -= Columns_CollectionChanged;
        foreach (var column in collection)
        {
            column.PropertyChanged -= Column_PropertyChanged;
        }
    }

    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (TaktDataGridColumnDefinition column in e.OldItems)
            {
                column.PropertyChanged -= Column_PropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (TaktDataGridColumnDefinition column in e.NewItems)
            {
                column.PropertyChanged += Column_PropertyChanged;
            }
        }

        ApplyColumns();
    }

    private void Column_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ApplyColumns();
    }

    private void ApplyColumns()
    {
        if (_dataGrid == null || _isApplyingColumns)
        {
            return;
        }

        _isApplyingColumns = true;
        try
        {
            // 清除现有列（保留操作列）
            var columnsToRemove = _dataGrid.Columns.Where(c => c != _operationColumn).ToList();
            foreach (var column in columnsToRemove)
            {
                _dataGrid.Columns.Remove(column);
            }

            // 添加新列
            foreach (var definition in Columns.Where(c => c.IsVisible))
            {
                var textColumn = CreateTextColumn(definition);
                _dataGrid.Columns.Add(textColumn);
            }

            // 确保操作列在最后
            EnsureOperationColumn();
        }
        finally
        {
            _isApplyingColumns = false;
        }
    }

    private System.Windows.Controls.DataGridTextColumn CreateTextColumn(TaktDataGridColumnDefinition definition)
    {
        var binding = new Binding(definition.BindingPath)
        {
            Mode = BindingMode.TwoWay,
            StringFormat = definition.StringFormat,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        var column = new System.Windows.Controls.DataGridTextColumn
        {
            Header = definition.Header,
            Binding = binding,
            CanUserSort = definition.CanSort
        };

        if (definition.Width.IsAuto)
        {
            column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
        }
        else
        {
            column.Width = definition.Width;
        }

        var elementStyle = new Style(typeof(TextBlock));
        var textAlignment = definition.IsNumeric ? TextAlignment.Right : definition.TextAlignment;
        elementStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, textAlignment));
        elementStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
        elementStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.NoWrap));
        elementStyle.Setters.Add(new Setter(TextBlock.ToolTipProperty, new Binding(definition.BindingPath)
        {
            TargetNullValue = string.Empty,
            FallbackValue = string.Empty
        }));
        column.ElementStyle = elementStyle;

        var editingStyle = new Style(typeof(TextBox));
        editingStyle.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(8, 0, 8, 0)));
        editingStyle.Setters.Add(new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        column.EditingElementStyle = editingStyle;

        return column;
    }

    #endregion

    #region 操作列处理

    private void EnsureOperationColumn()
    {
        if (_dataGrid == null)
        {
            return;
        }

        // 检查是否需要显示操作列
        bool shouldShow = ShowRowUpdate || ShowRowSave || ShowRowCancel || ShowRowDelete;
        SetValue(IsOperationColumnVisiblePropertyKey, shouldShow);

        if (!shouldShow)
        {
            if (_operationColumn != null && _dataGrid.Columns.Contains(_operationColumn))
            {
                _dataGrid.Columns.Remove(_operationColumn);
            }
            _operationColumn = null;
            return;
        }

        // 使用 XAML 中定义的模板
        var template = Resources["OperationColumnTemplate"] as DataTemplate;
        if (template == null)
        {
            return;
        }

        if (_operationColumn == null)
        {
            _operationColumn = new DataGridTemplateColumn
            {
                Header = OperationColumnHeader,
                CellTemplate = template,
                CanUserResize = false,
                CanUserSort = false,
                IsReadOnly = true
            };

            if (!double.IsNaN(OperationColumnWidth) && OperationColumnWidth > 0)
            {
                _operationColumn.Width = new DataGridLength(OperationColumnWidth);
            }

            _dataGrid.Columns.Add(_operationColumn);
        }
        else
        {
            _operationColumn.Header = OperationColumnHeader;
            _operationColumn.CellTemplate = template;
            if (!double.IsNaN(OperationColumnWidth) && OperationColumnWidth > 0)
            {
                _operationColumn.Width = new DataGridLength(OperationColumnWidth);
            }
            else
            {
                _operationColumn.Width = DataGridLength.Auto;
            }
        }

        if (!_dataGrid.Columns.Contains(_operationColumn))
        {
            _dataGrid.Columns.Add(_operationColumn);
        }
    }

    #endregion

    #region 行内编辑事件处理（参照 CodeGenForm.xaml.cs）

    private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        // 参照 CodeGenForm：只有当前行是正在编辑的行时，才允许编辑
        // 必须点击编辑按钮（设置 EditingItem）才能进入编辑状态
        if (e.Row.DataContext == null || EditingItem == null || EditingItem != e.Row.DataContext)
        {
            e.Cancel = true;
            return;
        }
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        // 参照 CodeGenForm：如果编辑被取消，不执行任何操作
        if (e.EditAction == DataGridEditAction.Cancel)
        {
            return;
        }

        // 如果需要根据列索引做联动更新，可以在这里添加
    }

    // 默认的编辑按钮处理：设置 EditingItem 并开始编辑
    private void OnRowUpdate(object? parameter)
    {
        if (parameter == null || _dataGrid == null)
        {
            return;
        }

        // 设置正在编辑的项
        EditingItem = parameter;
        SelectedItem = parameter;

        // 滚动到该项
        _dataGrid.ScrollIntoView(parameter);
        _dataGrid.SelectedItem = parameter;

        // 延迟执行，确保 UI 已更新
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_dataGrid != null && _dataGrid.SelectedItem == parameter)
            {
                _dataGrid.CurrentItem = parameter;

                // 找到第一个可编辑的列（排除操作列）
                DataGridColumn? editableColumn = _dataGrid.Columns
                    .OfType<DataGridBoundColumn>()
                    .Cast<DataGridColumn>()
                    .FirstOrDefault(c => !c.IsReadOnly && c != _operationColumn);

                if (editableColumn == null)
                {
                    editableColumn = _dataGrid.Columns
                        .OfType<DataGridTemplateColumn>()
                        .Cast<DataGridColumn>()
                        .FirstOrDefault(c => !c.IsReadOnly && c != _operationColumn);
                }

                if (editableColumn != null)
                {
                    _dataGrid.CurrentColumn = editableColumn;

                    // 再次延迟，确保列已设置
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_dataGrid != null && _dataGrid.CurrentItem == parameter)
                        {
                            _dataGrid.BeginEdit();
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    // 默认的取消按钮处理：清除 EditingItem
    private void OnRowCancel()
    {
        if (_dataGrid != null)
        {
            // 如果正在编辑，取消编辑
            try
            {
                _dataGrid.CancelEdit();
            }
            catch
            {
                // 忽略错误（可能没有正在编辑的单元格）
            }
        }
        EditingItem = null;
    }

    #endregion

    #region 新增行功能（参照 CodeGenForm）

    private bool CanAddRow()
    {
        return _dataGrid != null && ItemsSource != null && !_dataGrid.CanUserAddRows;
    }

    private void OnAddRow()
    {
        if (_dataGrid == null || ItemsSource == null || _dataGrid.CanUserAddRows)
        {
            return;
        }

        // 如果 ItemsSource 是 ObservableCollection，尝试添加新项
        if (ItemsSource is System.Collections.IList list)
        {
            try
            {
                // 尝试创建新项（使用反射创建默认实例）
                var itemType = GetItemType(ItemsSource);
                if (itemType != null)
                {
                    var newItem = Activator.CreateInstance(itemType);
                    if (newItem != null)
                    {
                        list.Add(newItem);

                        // 滚动到新添加的行
                        _dataGrid.ScrollIntoView(newItem);
                        _dataGrid.SelectedItem = newItem;

                        // 延迟执行，确保 UI 已更新
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (_dataGrid != null && newItem != null)
                            {
                                // 设置当前项和当前列
                                _dataGrid.CurrentItem = newItem;

                                // 找到第一个可编辑的列（排除操作列）
                                DataGridColumn? editableColumn = _dataGrid.Columns
                                    .OfType<DataGridBoundColumn>()
                                    .Cast<DataGridColumn>()
                                    .FirstOrDefault(c => !c.IsReadOnly && c != _operationColumn);

                                if (editableColumn != null)
                                {
                                    _dataGrid.CurrentColumn = editableColumn;

                                    // 再次延迟，确保列已设置
                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        if (_dataGrid != null && _dataGrid.CurrentItem == newItem)
                                        {
                                            _dataGrid.BeginEdit();
                                        }
                                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                                }
                            }
                        }), System.Windows.Threading.DispatcherPriority.Loaded);
                    }
                }
            }
            catch
            {
                // 如果无法创建新项，忽略错误
            }
        }
    }

    private Type? GetItemType(IEnumerable source)
    {
        var type = source.GetType();
        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                return genericArgs[0];
            }
        }

        // 尝试从第一个元素获取类型
        foreach (var item in source)
        {
            return item?.GetType();
        }

        return null;
    }

    #endregion

    #region 空状态处理

    private void AttachItemsSourceHandlers(object? source)
    {
        if (source is INotifyCollectionChanged notifier)
        {
            _itemsSourceNotifier = notifier;
            notifier.CollectionChanged += ItemsSource_CollectionChanged;
        }
        else if (source is IEnumerable enumerable)
        {
            // 如果不是 INotifyCollectionChanged，尝试从第一个元素判断
            UpdateEmptyState();
        }
    }

    private void DetachItemsSourceHandlers()
    {
        if (_itemsSourceNotifier != null)
        {
            _itemsSourceNotifier.CollectionChanged -= ItemsSource_CollectionChanged;
            _itemsSourceNotifier = null;
        }
    }

    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        if (ItemsSource == null)
        {
            IsEmpty = true;
            return;
        }

        var count = 0;
        foreach (var _ in ItemsSource)
        {
            count++;
            if (count > 0)
            {
                break;
            }
        }

        IsEmpty = count == 0;
    }

    #endregion

    #region 查询事件处理

    private void OnQueryButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var context = new QueryContext(QueryKeyword ?? string.Empty, 1, 20, this);
            QueryRequested?.Invoke(this, context);
            if (QueryCommand is { } command && command.CanExecute(context))
            {
                command.Execute(context);
            }
        }
        catch
        {
            // 忽略错误
        }
    }

    private void OnResetButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            SetCurrentValue(QueryKeywordProperty, string.Empty);
            var context = new QueryContext(string.Empty, 1, 20, this);
            if (ResetCommand is { } command && command.CanExecute(context))
            {
                command.Execute(context);
            }
            else
            {
                QueryRequested?.Invoke(this, context);
                if (QueryCommand is { } queryCommand && queryCommand.CanExecute(context))
                {
                    queryCommand.Execute(context);
                }
            }
        }
        catch
        {
            // 忽略错误
        }
    }

    // 查询事件
    public sealed record QueryContext(string Keyword, int PageIndex, int PageSize, TaktInlineEditDataGrid Sender);
    public event EventHandler<QueryContext>? QueryRequested;

    #endregion

    #region 分页事件处理

    private bool _internalPageChange;

    private void NavigateToPage(int targetPage, bool clearGotoText = true)
    {
        if (!ShowPagination)
        {
            return;
        }

        var totalPages = CalculateTotalPages();
        targetPage = Math.Clamp(targetPage, 1, totalPages);

        if (targetPage == PageIndex && !_internalPageChange)
        {
            if (clearGotoText)
            {
                SetCurrentValue(GoToPageTextProperty, string.Empty);
            }
            return;
        }

        try
        {
            _internalPageChange = true;
            SetCurrentValue(PageIndexProperty, targetPage);
        }
        finally
        {
            _internalPageChange = false;
        }

        UpdatePagination();
        RaisePageChanged(targetPage);

        if (clearGotoText)
        {
            SetCurrentValue(GoToPageTextProperty, string.Empty);
        }
    }

    private int CalculateTotalPages()
    {
        if (PageSize <= 0)
        {
            return 1;
        }

        var totalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
        return totalPages <= 0 ? 1 : totalPages;
    }

    private void RaisePageChanged(int pageIndex)
    {
        var context = new PageRequest(pageIndex, PageSize, this);
        PageChanged?.Invoke(this, context);
        if (PageChangedCommand is { } command && command.CanExecute(context))
        {
            command.Execute(context);
        }
    }

    public sealed record PageRequest(int PageIndex, int PageSize, TaktInlineEditDataGrid Sender);
    public event EventHandler<PageRequest>? PageChanged;

    private void OnFirstPageButtonClick(object sender, RoutedEventArgs e)
    {
        NavigateToPage(1);
        ExecuteOptionalCommand(FirstPageCommand);
    }

    private void OnPrevPageButtonClick(object sender, RoutedEventArgs e)
    {
        NavigateToPage(PageIndex - 1);
        ExecuteOptionalCommand(PrevPageCommand);
    }

    private void OnNextPageButtonClick(object sender, RoutedEventArgs e)
    {
        NavigateToPage(PageIndex + 1);
        ExecuteOptionalCommand(NextPageCommand);
    }

    private void OnLastPageButtonClick(object sender, RoutedEventArgs e)
    {
        var totalPages = CalculateTotalPages();
        NavigateToPage(totalPages);
        ExecuteOptionalCommand(LastPageCommand);
    }

    private void OnPageNumberButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton || toggleButton.Tag is not int pageNumber)
        {
            return;
        }

        NavigateToPage(pageNumber);
        e.Handled = true;
    }

    private void OnGoToPageButtonClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GoToPageText))
        {
            return;
        }

        if (int.TryParse(GoToPageText, out var page))
        {
            NavigateToPage(page);
            ExecuteOptionalCommand(null);
        }
    }

    private void OnPageSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is int newPageSize && newPageSize > 0)
        {
            if (PageSize != newPageSize)
            {
                SetCurrentValue(PageSizeProperty, newPageSize);
            }
        }
    }

    private void ExecuteOptionalCommand(ICommand? command)
    {
        if (command != null)
        {
            var context = new PageRequest(PageIndex, PageSize, this);
            if (command.CanExecute(context))
            {
                command.Execute(context);
            }
        }
    }

    #endregion

    #region 列设置事件处理

    private void OnToggleColumnPanelClick(object sender, RoutedEventArgs e)
    {
        IsColumnPanelOpen = !IsColumnPanelOpen;
    }

    private void OnColumnChooserClosed(object? sender, EventArgs e)
    {
        IsColumnPanelOpen = false;
    }

    #endregion

    #region 辅助方法

    private static string Translate(string key, string fallback)
    {
        try
        {
            var localizationManager = App.Services?.GetService<ILocalizationManager>();
            if (localizationManager != null)
            {
                var translated = localizationManager.GetString(key);
                if (!string.IsNullOrWhiteSpace(translated))
                {
                    return translated;
                }
            }
        }
        catch
        {
            // 忽略错误
        }

        return fallback;
    }

    private static string Format(string key, string fallback, params object[] args)
    {
        var template = Translate(key, fallback);
        try
        {
            return string.Format(System.Globalization.CultureInfo.CurrentUICulture, template, args);
        }
        catch
        {
            return fallback;
        }
    }

    #endregion
}

