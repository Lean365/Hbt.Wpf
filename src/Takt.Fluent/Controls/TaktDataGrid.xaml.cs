// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktDataGrid.xaml.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：完整的 MaterialDesign 风格通用数据表格控件，集成查询、工具栏、分页与行内操作。
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Takt.Common.Logging;
using Takt.Fluent;
using Takt.Fluent.ViewModels;
using Takt.Fluent.Helpers;
using Takt.Domain.Interfaces;
using System.Windows.Threading;

namespace Takt.Fluent.Controls;

public sealed class TaktDataGridColumnDefinition : DependencyObject, INotifyPropertyChanged
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(null, OnAnyPropertyChanged));

    public static readonly DependencyProperty BindingPathProperty =
        DependencyProperty.Register(nameof(BindingPath), typeof(string), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(string.Empty, OnAnyPropertyChanged));

    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(true, OnAnyPropertyChanged));

    public static readonly DependencyProperty CanSortProperty =
        DependencyProperty.Register(nameof(CanSort), typeof(bool), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(true, OnAnyPropertyChanged));

    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register(nameof(Width), typeof(DataGridLength), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(DataGridLength.Auto, OnAnyPropertyChanged));

    public static readonly DependencyProperty StringFormatProperty =
        DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(null, OnAnyPropertyChanged));

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(TextAlignment.Left, OnAnyPropertyChanged));

    public static readonly DependencyProperty IsNumericProperty =
        DependencyProperty.Register(nameof(IsNumeric), typeof(bool), typeof(TaktDataGridColumnDefinition), new PropertyMetadata(false, OnAnyPropertyChanged));

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string BindingPath
    {
        get => (string)GetValue(BindingPathProperty);
        set => SetValue(BindingPathProperty, value);
    }

    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public bool CanSort
    {
        get => (bool)GetValue(CanSortProperty);
        set => SetValue(CanSortProperty, value);
    }

    public DataGridLength Width
    {
        get => (DataGridLength)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public string? StringFormat
    {
        get => (string?)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public bool IsNumeric
    {
        get => (bool)GetValue(IsNumericProperty);
        set => SetValue(IsNumericProperty, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void OnAnyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGridColumnDefinition column)
        {
            column.PropertyChanged?.Invoke(column, new PropertyChangedEventArgs(e.Property.Name));
        }
    }
}

/// <summary>
/// 查询上下文。
/// </summary>
public sealed record QueryContext(string Keyword, int PageIndex, int PageSize, TaktDataGrid Sender);

/// <summary>
/// 分页请求。
/// </summary>
public sealed record PageRequest(int PageIndex, int PageSize, TaktDataGrid Sender);

public partial class TaktDataGrid : UserControl
{
    #region 嵌套常量

    #endregion

    private readonly ObservableCollection<int> _pageSizeOptions = new() { 10, 20, 30, 50, 100 };
    private readonly ObservableCollection<PageButtonInfo> _pageButtons = new();
    private DataGrid? _dataGrid;
    private System.Windows.Controls.DataGridTemplateColumn? _selectionColumn;
    private System.Windows.Controls.DataGridTemplateColumn? _operationColumn;
    private CheckBox? _selectionHeaderCheckBox;
    private bool _isApplyingColumns;
    private bool _internalPageChange;
    private INotifyCollectionChanged? _itemsSourceNotifier;

    // 内部命令包装器，用于自动启用/禁用
    private ICommand? _internalUpdateCommand;
    private ICommand? _internalDeleteCommand;

    // 操作日志管理器
    private OperLogManager? _operLog;

    // 操作按钮状态转换规则集合（统一管理所有操作按钮的状态关联关系）
    private readonly OperationButtonStateRuleCollection _operationStateRules = new();

    public static readonly DependencyProperty InternalUpdateCommandProperty =
        DependencyProperty.Register(nameof(InternalUpdateCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InternalDeleteCommandProperty =
        DependencyProperty.Register(nameof(InternalDeleteCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public TaktDataGrid()
    {
        var resourceLocator = new Uri("/Takt.Fluent;component/Controls/TaktDataGrid.xaml", UriKind.Relative);
        System.Windows.Application.LoadComponent(this, resourceLocator);

        if (GetValue(ColumnsProperty) is not ObservableCollection<TaktDataGridColumnDefinition> columns || columns == null)
        {
            columns = new ObservableCollection<TaktDataGridColumnDefinition>();
            SetCurrentValue(ColumnsProperty, columns);
        }

        AttachColumnHandlers(columns);
        UpdatePaginationTexts();

        // 获取操作日志管理器
        _operLog = App.Services?.GetService<OperLogManager>();

        // 创建内部命令包装器
        CreateInternalCommands();

        // 初始化操作按钮状态转换规则（统一管理启动、停止、暂停、重启、恢复之间的关联关系）
        InitializeOperationStateRules();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region 公开事件

    public event EventHandler<QueryContext>? QueryRequested;

    public event EventHandler<PageRequest>? PageChanged;

    public event EventHandler<PageRequest>? PageSizeChanged;

    #endregion

    #region CLR 属性

    public ObservableCollection<int> PageSizeOptions => _pageSizeOptions;
    public ObservableCollection<PageButtonInfo> PageButtons => _pageButtons;

    #endregion

    #region 依赖属性 - 数据绑定

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TaktDataGrid),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TaktDataGrid),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static readonly DependencyPropertyKey SelectedItemsCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedItemsCount), typeof(int), typeof(TaktDataGrid),
            new PropertyMetadata(0, OnSelectedItemsCountChanged));

    public static readonly DependencyProperty SelectedItemsCountProperty = SelectedItemsCountPropertyKey.DependencyProperty;

    public int SelectedItemsCount
    {
        get => (int)GetValue(SelectedItemsCountProperty);
        private set => SetValue(SelectedItemsCountPropertyKey, value);
    }

    public event EventHandler<int>? SelectedItemsCountChanged;

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<TaktDataGridColumnDefinition>), typeof(TaktDataGrid),
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

    public static readonly DependencyProperty ShowQueryAreaProperty =
        DependencyProperty.Register(nameof(ShowQueryArea), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool ShowQueryArea
    {
        get => (bool)GetValue(ShowQueryAreaProperty);
        set => SetValue(ShowQueryAreaProperty, value);
    }

    public static readonly DependencyProperty QueryKeywordProperty =
        DependencyProperty.Register(nameof(QueryKeyword), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(string.Empty));

    public string QueryKeyword
    {
        get => (string)GetValue(QueryKeywordProperty);
        set => SetValue(QueryKeywordProperty, value);
    }

    public static readonly DependencyProperty QueryPlaceholderProperty =
        DependencyProperty.Register(nameof(QueryPlaceholder), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Format("common.placeholder.keywordHint", "请输入{0}进行搜索", Translate("common.keyword", "关键字"))));

    public string QueryPlaceholder
    {
        get => (string)GetValue(QueryPlaceholderProperty);
        set => SetValue(QueryPlaceholderProperty, value);
    }

    public static readonly DependencyProperty QueryButtonTextProperty =
        DependencyProperty.Register(nameof(QueryButtonText), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.button.query", "查询")));

    public string QueryButtonText
    {
        get => (string)GetValue(QueryButtonTextProperty);
        set => SetValue(QueryButtonTextProperty, value);
    }

    public static readonly DependencyProperty ResetButtonTextProperty =
        DependencyProperty.Register(nameof(ResetButtonText), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.button.reset", "重置")));

    public static readonly DependencyProperty PageSizeHintProperty =
        DependencyProperty.Register(nameof(PageSizeHint), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.pageSizeHint", "每页")));

    public static readonly DependencyProperty GoToPageHintProperty =
        DependencyProperty.Register(nameof(GoToPageHint), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.pageInputHint", "页码")));

    public static readonly DependencyProperty GoToPageButtonTextProperty =
        DependencyProperty.Register(nameof(GoToPageButtonText), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.goTo", "前往")));

    private static readonly DependencyPropertyKey IsEmptyPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    public static readonly DependencyProperty EmptyTextProperty =
        DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.noData", "暂无数据")));

    public string ResetButtonText
    {
        get => (string)GetValue(ResetButtonTextProperty);
        set => SetValue(ResetButtonTextProperty, value);
    }

    public string PageSizeHint
    {
        get => (string)GetValue(PageSizeHintProperty);
        set => SetValue(PageSizeHintProperty, value);
    }

    public string GoToPageHint
    {
        get => (string)GetValue(GoToPageHintProperty);
        set => SetValue(GoToPageHintProperty, value);
    }

    public string GoToPageButtonText
    {
        get => (string)GetValue(GoToPageButtonTextProperty);
        set => SetValue(GoToPageButtonTextProperty, value);
    }

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        private set => SetValue(IsEmptyPropertyKey, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarProperty =
        DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarCreateProperty =
        DependencyProperty.Register(nameof(ShowToolbarCreate), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool ShowToolbarCreate
    {
        get => (bool)GetValue(ShowToolbarCreateProperty);
        set => SetValue(ShowToolbarCreateProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarUpdateProperty =
        DependencyProperty.Register(nameof(ShowToolbarUpdate), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool ShowToolbarUpdate
    {
        get => (bool)GetValue(ShowToolbarUpdateProperty);
        set => SetValue(ShowToolbarUpdateProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarDeleteProperty =
        DependencyProperty.Register(nameof(ShowToolbarDelete), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool ShowToolbarDelete
    {
        get => (bool)GetValue(ShowToolbarDeleteProperty);
        set => SetValue(ShowToolbarDeleteProperty, value);
    }

    public static readonly DependencyProperty SelectionColumnHeaderProperty =
        DependencyProperty.Register(nameof(SelectionColumnHeader), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.placeholder.select", "请选择{0}"), OnSelectionColumnPropertyChanged));

    public string SelectionColumnHeader
    {
        get => (string)GetValue(SelectionColumnHeaderProperty);
        set => SetValue(SelectionColumnHeaderProperty, value);
    }

    private static readonly DependencyPropertyKey IsSelectionColumnVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSelectionColumnVisible), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsSelectionColumnVisibleProperty = IsSelectionColumnVisiblePropertyKey.DependencyProperty;

    public bool IsSelectionColumnVisible => (bool)GetValue(IsSelectionColumnVisibleProperty);

    public static readonly DependencyProperty ShowToolbarImportProperty =
        DependencyProperty.Register(nameof(ShowToolbarImport), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool ShowToolbarImport
    {
        get => (bool)GetValue(ShowToolbarImportProperty);
        set => SetValue(ShowToolbarImportProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarExportProperty =
        DependencyProperty.Register(nameof(ShowToolbarExport), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool ShowToolbarExport
    {
        get => (bool)GetValue(ShowToolbarExportProperty);
        set => SetValue(ShowToolbarExportProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarClearProperty =
        DependencyProperty.Register(nameof(ShowToolbarClear), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool ShowToolbarClear
    {
        get => (bool)GetValue(ShowToolbarClearProperty);
        set => SetValue(ShowToolbarClearProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarScanInboundProperty =
        DependencyProperty.Register(nameof(ShowToolbarScanInbound), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool ShowToolbarScanInbound
    {
        get => (bool)GetValue(ShowToolbarScanInboundProperty);
        set => SetValue(ShowToolbarScanInboundProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarScanOutboundProperty =
        DependencyProperty.Register(nameof(ShowToolbarScanOutbound), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool ShowToolbarScanOutbound
    {
        get => (bool)GetValue(ShowToolbarScanOutboundProperty);
        set => SetValue(ShowToolbarScanOutboundProperty, value);
    }

    public static readonly DependencyProperty CustomToolbarContentProperty =
        DependencyProperty.Register(nameof(CustomToolbarContent), typeof(UIElement), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public UIElement? CustomToolbarContent
    {
        get => (UIElement?)GetValue(CustomToolbarContentProperty);
        set => SetValue(CustomToolbarContentProperty, value);
    }

    public static readonly DependencyProperty ShowAdvancedQueryButtonProperty =
        DependencyProperty.Register(nameof(ShowAdvancedQueryButton), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool ShowAdvancedQueryButton
    {
        get => (bool)GetValue(ShowAdvancedQueryButtonProperty);
        set => SetValue(ShowAdvancedQueryButtonProperty, value);
    }

    public static readonly DependencyProperty ShowPaginationProperty =
        DependencyProperty.Register(nameof(ShowPagination), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool ShowPagination
    {
        get => (bool)GetValue(ShowPaginationProperty);
        set => SetValue(ShowPaginationProperty, value);
    }

    public static readonly DependencyProperty IsColumnPanelOpenProperty =
        DependencyProperty.Register(nameof(IsColumnPanelOpen), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool IsColumnPanelOpen
    {
        get => (bool)GetValue(IsColumnPanelOpenProperty);
        set => SetValue(IsColumnPanelOpenProperty, value);
    }

    private static readonly DependencyPropertyKey HasPreviousPagePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasPreviousPage), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasPreviousPageProperty = HasPreviousPagePropertyKey.DependencyProperty;

    public bool HasPreviousPage
    {
        get => (bool)GetValue(HasPreviousPageProperty);
        private set => SetValue(HasPreviousPagePropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasNextPagePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasNextPage), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasNextPageProperty = HasNextPagePropertyKey.DependencyProperty;

    public bool HasNextPage
    {
        get => (bool)GetValue(HasNextPageProperty);
        private set => SetValue(HasNextPagePropertyKey, value);
    }

    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(TaktDataGrid),
            new PropertyMetadata(1, OnPaginationPropertyChanged));

    public int PageIndex
    {
        get => (int)GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(TaktDataGrid),
            new PropertyMetadata(20, OnPaginationPropertyChanged));

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(TaktDataGrid),
            new PropertyMetadata(0, OnPaginationPropertyChanged));

    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public static readonly DependencyProperty TotalTextProperty =
        DependencyProperty.Register(nameof(TotalText), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Format("common.total", "共 {0} 条记录", 0)));

    public string TotalText
    {
        get => (string)GetValue(TotalTextProperty);
        set => SetValue(TotalTextProperty, value);
    }

    public static readonly DependencyProperty PageDisplayProperty =
        DependencyProperty.Register(nameof(PageDisplay), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Format("common.pageDisplay", "第 {0} / {1} 页", 1, 1)));

    public string PageDisplay
    {
        get => (string)GetValue(PageDisplayProperty);
        set => SetValue(PageDisplayProperty, value);
    }

    public static readonly DependencyProperty GoToPageTextProperty =
        DependencyProperty.Register(nameof(GoToPageText), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(string.Empty));

    public string GoToPageText
    {
        get => (string)GetValue(GoToPageTextProperty);
        set => SetValue(GoToPageTextProperty, value);
    }

    public static readonly DependencyProperty OperationColumnHeaderProperty =
        DependencyProperty.Register(nameof(OperationColumnHeader), typeof(string), typeof(TaktDataGrid),
            new PropertyMetadata(Translate("common.operation", "操作"), OnOperationPropertyChanged));

    public string OperationColumnHeader
    {
        get => (string)GetValue(OperationColumnHeaderProperty);
        set => SetValue(OperationColumnHeaderProperty, value);
    }

    public static readonly DependencyProperty OperationColumnWidthProperty =
        DependencyProperty.Register(nameof(OperationColumnWidth), typeof(double), typeof(TaktDataGrid),
            new PropertyMetadata(double.NaN, OnOperationPropertyChanged));

    public double OperationColumnWidth
    {
        get => (double)GetValue(OperationColumnWidthProperty);
        set => SetValue(OperationColumnWidthProperty, value);
    }

    private static readonly DependencyPropertyKey IsOperationColumnVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsOperationColumnVisible), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsOperationColumnVisibleProperty = IsOperationColumnVisiblePropertyKey.DependencyProperty;

    public bool IsOperationColumnVisible => (bool)GetValue(IsOperationColumnVisibleProperty);

    #endregion

    #region 依赖属性 - Toolbar 命令

    public static readonly DependencyProperty QueryCommandProperty =
        DependencyProperty.Register(nameof(QueryCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? QueryCommand
    {
        get => (ICommand?)GetValue(QueryCommandProperty);
        set => SetValue(QueryCommandProperty, value);
    }

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? ResetCommand
    {
        get => (ICommand?)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public static readonly DependencyProperty CreateCommandProperty =
        DependencyProperty.Register(nameof(CreateCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? CreateCommand
    {
        get => (ICommand?)GetValue(CreateCommandProperty);
        set => SetValue(CreateCommandProperty, value);
    }

    public static readonly DependencyProperty UpdateCommandProperty =
        DependencyProperty.Register(nameof(UpdateCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null, OnUpdateCommandChanged));

    public ICommand? UpdateCommand
    {
        get => (ICommand?)GetValue(UpdateCommandProperty);
        set => SetValue(UpdateCommandProperty, value);
    }

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null, OnDeleteCommandChanged));

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public static readonly DependencyProperty ImportCommandProperty =
        DependencyProperty.Register(nameof(ImportCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? ImportCommand
    {
        get => (ICommand?)GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }

    public static readonly DependencyProperty DownloadTemplateCommandProperty =
        DependencyProperty.Register(nameof(DownloadTemplateCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? DownloadTemplateCommand
    {
        get => (ICommand?)GetValue(DownloadTemplateCommandProperty);
        set => SetValue(DownloadTemplateCommandProperty, value);
    }

    public static readonly DependencyProperty ExportCommandProperty =
        DependencyProperty.Register(nameof(ExportCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? ExportCommand
    {
        get => (ICommand?)GetValue(ExportCommandProperty);
        set => SetValue(ExportCommandProperty, value);
    }

    public static readonly DependencyProperty ClearCommandProperty =
        DependencyProperty.Register(nameof(ClearCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? ClearCommand
    {
        get => (ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public static readonly DependencyProperty ScanInboundCommandProperty =
        DependencyProperty.Register(nameof(ScanInboundCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? ScanInboundCommand
    {
        get => (ICommand?)GetValue(ScanInboundCommandProperty);
        set => SetValue(ScanInboundCommandProperty, value);
    }

    public static readonly DependencyProperty ScanOutboundCommandProperty =
        DependencyProperty.Register(nameof(ScanOutboundCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? ScanOutboundCommand
    {
        get => (ICommand?)GetValue(ScanOutboundCommandProperty);
        set => SetValue(ScanOutboundCommandProperty, value);
    }

    public static readonly DependencyProperty AdvancedQueryCommandProperty =
        DependencyProperty.Register(nameof(AdvancedQueryCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? AdvancedQueryCommand
    {
        get => (ICommand?)GetValue(AdvancedQueryCommandProperty);
        set => SetValue(AdvancedQueryCommandProperty, value);
    }

    public static readonly DependencyProperty FirstPageCommandProperty =
        DependencyProperty.Register(nameof(FirstPageCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? FirstPageCommand
    {
        get => (ICommand?)GetValue(FirstPageCommandProperty);
        set => SetValue(FirstPageCommandProperty, value);
    }

    public static readonly DependencyProperty PrevPageCommandProperty =
        DependencyProperty.Register(nameof(PrevPageCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? PrevPageCommand
    {
        get => (ICommand?)GetValue(PrevPageCommandProperty);
        set => SetValue(PrevPageCommandProperty, value);
    }

    public static readonly DependencyProperty NextPageCommandProperty =
        DependencyProperty.Register(nameof(NextPageCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? NextPageCommand
    {
        get => (ICommand?)GetValue(NextPageCommandProperty);
        set => SetValue(NextPageCommandProperty, value);
    }

    public static readonly DependencyProperty LastPageCommandProperty =
        DependencyProperty.Register(nameof(LastPageCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? LastPageCommand
    {
        get => (ICommand?)GetValue(LastPageCommandProperty);
        set => SetValue(LastPageCommandProperty, value);
    }

    public static readonly DependencyProperty PageChangedCommandProperty =
        DependencyProperty.Register(nameof(PageChangedCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? PageChangedCommand
    {
        get => (ICommand?)GetValue(PageChangedCommandProperty);
        set => SetValue(PageChangedCommandProperty, value);
    }

    public static readonly DependencyProperty PageSizeChangedCommandProperty =
        DependencyProperty.Register(nameof(PageSizeChangedCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? PageSizeChangedCommand
    {
        get => (ICommand?)GetValue(PageSizeChangedCommandProperty);
        set => SetValue(PageSizeChangedCommandProperty, value);
    }

    public static readonly DependencyProperty GoToPageCommandProperty =
        DependencyProperty.Register(nameof(GoToPageCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? GoToPageCommand
    {
        get => (ICommand?)GetValue(GoToPageCommandProperty);
        set => SetValue(GoToPageCommandProperty, value);
    }

    #endregion

    #region 依赖属性 - 行内操作按钮

    private const string RowOperationStyleKey = "DefaultIconPlainPrimarySmall";

    public static readonly DependencyProperty ShowRowCreateProperty =
        DependencyProperty.Register(nameof(ShowRowCreate), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowCreate
    {
        get => (bool)GetValue(ShowRowCreateProperty);
        set => SetValue(ShowRowCreateProperty, value);
    }

    public static readonly DependencyProperty RowCreateCommandProperty =
        DependencyProperty.Register(nameof(RowCreateCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowCreateCommand
    {
        get => (ICommand?)GetValue(RowCreateCommandProperty);
        set => SetValue(RowCreateCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowUpdateProperty =
        DependencyProperty.Register(nameof(ShowRowUpdate), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true, OnOperationPropertyChanged));

    public bool ShowRowUpdate
    {
        get => (bool)GetValue(ShowRowUpdateProperty);
        set => SetValue(ShowRowUpdateProperty, value);
    }

    public static readonly DependencyProperty RowUpdateCommandProperty =
        DependencyProperty.Register(nameof(RowUpdateCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowUpdateCommand
    {
        get => (ICommand?)GetValue(RowUpdateCommandProperty);
        set => SetValue(RowUpdateCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowDeleteProperty =
        DependencyProperty.Register(nameof(ShowRowDelete), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true, OnOperationPropertyChanged));

    public bool ShowRowDelete
    {
        get => (bool)GetValue(ShowRowDeleteProperty);
        set => SetValue(ShowRowDeleteProperty, value);
    }

    public static readonly DependencyProperty RowDeleteCommandProperty =
        DependencyProperty.Register(nameof(RowDeleteCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowDeleteCommand
    {
        get => (ICommand?)GetValue(RowDeleteCommandProperty);
        set => SetValue(RowDeleteCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowDetailProperty =
        DependencyProperty.Register(nameof(ShowRowDetail), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowDetail
    {
        get => (bool)GetValue(ShowRowDetailProperty);
        set => SetValue(ShowRowDetailProperty, value);
    }

    public static readonly DependencyProperty RowDetailCommandProperty =
        DependencyProperty.Register(nameof(RowDetailCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowDetailCommand
    {
        get => (ICommand?)GetValue(RowDetailCommandProperty);
        set => SetValue(RowDetailCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowAuthorizeProperty =
        DependencyProperty.Register(nameof(ShowRowAuthorize), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowAuthorize
    {
        get => (bool)GetValue(ShowRowAuthorizeProperty);
        set => SetValue(ShowRowAuthorizeProperty, value);
    }

    public static readonly DependencyProperty RowAuthorizeCommandProperty =
        DependencyProperty.Register(nameof(RowAuthorizeCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowAuthorizeCommand
    {
        get => (ICommand?)GetValue(RowAuthorizeCommandProperty);
        set => SetValue(RowAuthorizeCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowAssignRoleProperty =
        DependencyProperty.Register(nameof(ShowRowAssignRole), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowAssignRole
    {
        get => (bool)GetValue(ShowRowAssignRoleProperty);
        set => SetValue(ShowRowAssignRoleProperty, value);
    }

    public static readonly DependencyProperty RowAssignRoleCommandProperty =
        DependencyProperty.Register(nameof(RowAssignRoleCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowAssignRoleCommand
    {
        get => (ICommand?)GetValue(RowAssignRoleCommandProperty);
        set => SetValue(RowAssignRoleCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowRunProperty =
        DependencyProperty.Register(nameof(ShowRowRun), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowRun
    {
        get => (bool)GetValue(ShowRowRunProperty);
        set => SetValue(ShowRowRunProperty, value);
    }

    public static readonly DependencyProperty RowRunCommandProperty =
        DependencyProperty.Register(nameof(RowRunCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowRunCommand
    {
        get => (ICommand?)GetValue(RowRunCommandProperty);
        set => SetValue(RowRunCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowGenerateProperty =
        DependencyProperty.Register(nameof(ShowRowGenerate), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowGenerate
    {
        get => (bool)GetValue(ShowRowGenerateProperty);
        set => SetValue(ShowRowGenerateProperty, value);
    }

    public static readonly DependencyProperty RowGenerateCommandProperty =
        DependencyProperty.Register(nameof(RowGenerateCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowGenerateCommand
    {
        get => (ICommand?)GetValue(RowGenerateCommandProperty);
        set => SetValue(RowGenerateCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowStartProperty =
        DependencyProperty.Register(nameof(ShowRowStart), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowStart
    {
        get => (bool)GetValue(ShowRowStartProperty);
        set => SetValue(ShowRowStartProperty, value);
    }

    public static readonly DependencyProperty RowStartCommandProperty =
        DependencyProperty.Register(nameof(RowStartCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowStartCommand
    {
        get => (ICommand?)GetValue(RowStartCommandProperty);
        set => SetValue(RowStartCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowStopProperty =
        DependencyProperty.Register(nameof(ShowRowStop), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowStop
    {
        get => (bool)GetValue(ShowRowStopProperty);
        set => SetValue(ShowRowStopProperty, value);
    }

    public static readonly DependencyProperty RowStopCommandProperty =
        DependencyProperty.Register(nameof(RowStopCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowStopCommand
    {
        get => (ICommand?)GetValue(RowStopCommandProperty);
        set => SetValue(RowStopCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowPauseProperty =
        DependencyProperty.Register(nameof(ShowRowPause), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowPause
    {
        get => (bool)GetValue(ShowRowPauseProperty);
        set => SetValue(ShowRowPauseProperty, value);
    }

    public static readonly DependencyProperty RowPauseCommandProperty =
        DependencyProperty.Register(nameof(RowPauseCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowPauseCommand
    {
        get => (ICommand?)GetValue(RowPauseCommandProperty);
        set => SetValue(RowPauseCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowRestartProperty =
        DependencyProperty.Register(nameof(ShowRowRestart), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowRestart
    {
        get => (bool)GetValue(ShowRowRestartProperty);
        set => SetValue(ShowRowRestartProperty, value);
    }

    public static readonly DependencyProperty RowRestartCommandProperty =
        DependencyProperty.Register(nameof(RowRestartCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowRestartCommand
    {
        get => (ICommand?)GetValue(RowRestartCommandProperty);
        set => SetValue(RowRestartCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowResumeProperty =
        DependencyProperty.Register(nameof(ShowRowResume), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowResume
    {
        get => (bool)GetValue(ShowRowResumeProperty);
        set => SetValue(ShowRowResumeProperty, value);
    }

    public static readonly DependencyProperty RowResumeCommandProperty =
        DependencyProperty.Register(nameof(RowResumeCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowResumeCommand
    {
        get => (ICommand?)GetValue(RowResumeCommandProperty);
        set => SetValue(RowResumeCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowCloneProperty =
        DependencyProperty.Register(nameof(ShowRowClone), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowClone
    {
        get => (bool)GetValue(ShowRowCloneProperty);
        set => SetValue(ShowRowCloneProperty, value);
    }

    public static readonly DependencyProperty RowCloneCommandProperty =
        DependencyProperty.Register(nameof(RowCloneCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowCloneCommand
    {
        get => (ICommand?)GetValue(RowCloneCommandProperty);
        set => SetValue(RowCloneCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowResetProperty =
        DependencyProperty.Register(nameof(ShowRowReset), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowReset
    {
        get => (bool)GetValue(ShowRowResetProperty);
        set => SetValue(ShowRowResetProperty, value);
    }

    public static readonly DependencyProperty RowResetCommandProperty =
        DependencyProperty.Register(nameof(RowResetCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowResetCommand
    {
        get => (ICommand?)GetValue(RowResetCommandProperty);
        set => SetValue(RowResetCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowSyncProperty =
        DependencyProperty.Register(nameof(ShowRowSync), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowSync
    {
        get => (bool)GetValue(ShowRowSyncProperty);
        set => SetValue(ShowRowSyncProperty, value);
    }

    public static readonly DependencyProperty RowSyncCommandProperty =
        DependencyProperty.Register(nameof(RowSyncCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowSyncCommand
    {
        get => (ICommand?)GetValue(RowSyncCommandProperty);
        set => SetValue(RowSyncCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowPreviewProperty =
        DependencyProperty.Register(nameof(ShowRowPreview), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowPreview
    {
        get => (bool)GetValue(ShowRowPreviewProperty);
        set => SetValue(ShowRowPreviewProperty, value);
    }

    public static readonly DependencyProperty RowPreviewCommandProperty =
        DependencyProperty.Register(nameof(RowPreviewCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowPreviewCommand
    {
        get => (ICommand?)GetValue(RowPreviewCommandProperty);
        set => SetValue(RowPreviewCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowPrintProperty =
        DependencyProperty.Register(nameof(ShowRowPrint), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowPrint
    {
        get => (bool)GetValue(ShowRowPrintProperty);
        set => SetValue(ShowRowPrintProperty, value);
    }

    public static readonly DependencyProperty RowPrintCommandProperty =
        DependencyProperty.Register(nameof(RowPrintCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowPrintCommand
    {
        get => (ICommand?)GetValue(RowPrintCommandProperty);
        set => SetValue(RowPrintCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowAssignDeptProperty =
        DependencyProperty.Register(nameof(ShowRowAssignDept), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowAssignDept
    {
        get => (bool)GetValue(ShowRowAssignDeptProperty);
        set => SetValue(ShowRowAssignDeptProperty, value);
    }

    public static readonly DependencyProperty RowAssignDeptCommandProperty =
        DependencyProperty.Register(nameof(RowAssignDeptCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowAssignDeptCommand
    {
        get => (ICommand?)GetValue(RowAssignDeptCommandProperty);
        set => SetValue(RowAssignDeptCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowAssignMenuProperty =
        DependencyProperty.Register(nameof(ShowRowAssignMenu), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowAssignMenu
    {
        get => (bool)GetValue(ShowRowAssignMenuProperty);
        set => SetValue(ShowRowAssignMenuProperty, value);
    }

    public static readonly DependencyProperty RowAssignMenuCommandProperty =
        DependencyProperty.Register(nameof(RowAssignMenuCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowAssignMenuCommand
    {
        get => (ICommand?)GetValue(RowAssignMenuCommandProperty);
        set => SetValue(RowAssignMenuCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowAssignPostProperty =
        DependencyProperty.Register(nameof(ShowRowAssignPost), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false, OnOperationPropertyChanged));

    public bool ShowRowAssignPost
    {
        get => (bool)GetValue(ShowRowAssignPostProperty);
        set => SetValue(ShowRowAssignPostProperty, value);
    }

    public static readonly DependencyProperty RowAssignPostCommandProperty =
        DependencyProperty.Register(nameof(RowAssignPostCommand), typeof(ICommand), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public ICommand? RowAssignPostCommand
    {
        get => (ICommand?)GetValue(RowAssignPostCommandProperty);
        set => SetValue(RowAssignPostCommandProperty, value);
    }

    #endregion

    #region 依赖属性 - DataGrid 外观同步

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(DataGridSelectionMode), typeof(TaktDataGrid),
            new PropertyMetadata(DataGridSelectionMode.Extended));

    public DataGridSelectionMode SelectionMode
    {
        get => (DataGridSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public static readonly DependencyProperty SelectionUnitProperty =
        DependencyProperty.Register(nameof(SelectionUnit), typeof(DataGridSelectionUnit), typeof(TaktDataGrid),
            new PropertyMetadata(DataGridSelectionUnit.FullRow));

    public DataGridSelectionUnit SelectionUnit
    {
        get => (DataGridSelectionUnit)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty HeadersVisibilityProperty =
        DependencyProperty.Register(nameof(HeadersVisibility), typeof(DataGridHeadersVisibility), typeof(TaktDataGrid),
            new PropertyMetadata(DataGridHeadersVisibility.Column));

    public DataGridHeadersVisibility HeadersVisibility
    {
        get => (DataGridHeadersVisibility)GetValue(HeadersVisibilityProperty);
        set => SetValue(HeadersVisibilityProperty, value);
    }

    public static readonly DependencyProperty GridLinesVisibilityProperty =
        DependencyProperty.Register(nameof(GridLinesVisibility), typeof(DataGridGridLinesVisibility), typeof(TaktDataGrid),
            new PropertyMetadata(DataGridGridLinesVisibility.None));

    public DataGridGridLinesVisibility GridLinesVisibility
    {
        get => (DataGridGridLinesVisibility)GetValue(GridLinesVisibilityProperty);
        set => SetValue(GridLinesVisibilityProperty, value);
    }

    public static readonly DependencyProperty RowStyleProperty =
        DependencyProperty.Register(nameof(RowStyle), typeof(Style), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public Style? RowStyle
    {
        get => (Style?)GetValue(RowStyleProperty);
        set => SetValue(RowStyleProperty, value);
    }

    public static readonly DependencyProperty CellStyleProperty =
        DependencyProperty.Register(nameof(CellStyle), typeof(Style), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public Style? CellStyle
    {
        get => (Style?)GetValue(CellStyleProperty);
        set => SetValue(CellStyleProperty, value);
    }

    public static readonly DependencyProperty ColumnHeaderStyleProperty =
        DependencyProperty.Register(nameof(ColumnHeaderStyle), typeof(Style), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public Style? ColumnHeaderStyle
    {
        get => (Style?)GetValue(ColumnHeaderStyleProperty);
        set => SetValue(ColumnHeaderStyleProperty, value);
    }

    public static readonly DependencyProperty HorizontalGridLinesBrushProperty =
        DependencyProperty.Register(nameof(HorizontalGridLinesBrush), typeof(System.Windows.Media.Brush), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public System.Windows.Media.Brush? HorizontalGridLinesBrush
    {
        get => (System.Windows.Media.Brush?)GetValue(HorizontalGridLinesBrushProperty);
        set => SetValue(HorizontalGridLinesBrushProperty, value);
    }

    public static readonly DependencyProperty VerticalGridLinesBrushProperty =
        DependencyProperty.Register(nameof(VerticalGridLinesBrush), typeof(System.Windows.Media.Brush), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public System.Windows.Media.Brush? VerticalGridLinesBrush
    {
        get => (System.Windows.Media.Brush?)GetValue(VerticalGridLinesBrushProperty);
        set => SetValue(VerticalGridLinesBrushProperty, value);
    }

    public static readonly DependencyProperty RowBackgroundProperty =
        DependencyProperty.Register(nameof(RowBackground), typeof(System.Windows.Media.Brush), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public System.Windows.Media.Brush? RowBackground
    {
        get => (System.Windows.Media.Brush?)GetValue(RowBackgroundProperty);
        set => SetValue(RowBackgroundProperty, value);
    }

    public static readonly DependencyProperty AlternatingRowBackgroundProperty =
        DependencyProperty.Register(nameof(AlternatingRowBackground), typeof(System.Windows.Media.Brush), typeof(TaktDataGrid),
            new PropertyMetadata(null));

    public System.Windows.Media.Brush? AlternatingRowBackground
    {
        get => (System.Windows.Media.Brush?)GetValue(AlternatingRowBackgroundProperty);
        set => SetValue(AlternatingRowBackgroundProperty, value);
    }

    public static readonly DependencyProperty ClipboardCopyModeProperty =
        DependencyProperty.Register(nameof(ClipboardCopyMode), typeof(DataGridClipboardCopyMode), typeof(TaktDataGrid),
            new PropertyMetadata(DataGridClipboardCopyMode.IncludeHeader));

    public DataGridClipboardCopyMode ClipboardCopyMode
    {
        get => (DataGridClipboardCopyMode)GetValue(ClipboardCopyModeProperty);
        set => SetValue(ClipboardCopyModeProperty, value);
    }

    public static readonly DependencyProperty EnableRowVirtualizationProperty =
        DependencyProperty.Register(nameof(EnableRowVirtualization), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool EnableRowVirtualization
    {
        get => (bool)GetValue(EnableRowVirtualizationProperty);
        set => SetValue(EnableRowVirtualizationProperty, value);
    }

    public static readonly DependencyProperty EnableColumnVirtualizationProperty =
        DependencyProperty.Register(nameof(EnableColumnVirtualization), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(true));

    public bool EnableColumnVirtualization
    {
        get => (bool)GetValue(EnableColumnVirtualizationProperty);
        set => SetValue(EnableColumnVirtualizationProperty, value);
    }

    public static readonly DependencyProperty CanUserAddRowsProperty =
        DependencyProperty.Register(nameof(CanUserAddRows), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool CanUserAddRows
    {
        get => (bool)GetValue(CanUserAddRowsProperty);
        set => SetValue(CanUserAddRowsProperty, value);
    }

    public static readonly DependencyProperty CanUserDeleteRowsProperty =
        DependencyProperty.Register(nameof(CanUserDeleteRows), typeof(bool), typeof(TaktDataGrid),
            new PropertyMetadata(false));

    public bool CanUserDeleteRows
    {
        get => (bool)GetValue(CanUserDeleteRowsProperty);
        set => SetValue(CanUserDeleteRowsProperty, value);
    }

    #endregion

    #region 内部命令包装器

    /// <summary>
    /// 创建内部命令包装器，用于自动启用/禁用更新和删除按钮
    /// </summary>
    private void CreateInternalCommands()
    {
        // 更新命令：单选时激活（SelectedItem != null && SelectedItemsCount == 1）
        // 默认禁用，只有选中单行时才激活
        _internalUpdateCommand = new ViewModels.RelayCommand<object?>(
            parameter =>
            {
                try
                {
                    // 执行外部命令，传递 SelectedItem 或参数
                    var targetCommand = UpdateCommand;
                    if (targetCommand != null && SelectedItem != null && SelectedItemsCount == 1)
                    {
                        var commandParameter = SelectedItem ?? parameter;
                        if (targetCommand.CanExecute(commandParameter))
                        {
                            _operLog?.Information("[TaktDataGrid] 工具栏编辑按钮点击: SelectedItem={SelectedItem}", SelectedItem?.ToString() ?? "null");
                            targetCommand.Execute(commandParameter);
                            _operLog?.Information("[TaktDataGrid] 编辑命令执行成功");
                        }
                        else
                        {
                            _operLog?.Information("[TaktDataGrid] 编辑命令不可执行");
                        }
                    }
                    else
                    {
                        _operLog?.Information("[TaktDataGrid] 编辑按钮点击: 未选中项或选中项数量不正确 (SelectedItemsCount={SelectedItemsCount})", SelectedItemsCount);
                    }
                }
                catch (Exception ex)
                {
                    _operLog?.Error(ex, "[TaktDataGrid] 编辑操作失败");
                }
            },
            parameter =>
            {
                // 单选更新：必须有选中项且只能选中一行，并且外部命令允许执行
                // 默认禁用，只有选中单行时才激活
                if (SelectedItem == null || SelectedItemsCount != 1)
                {
                    return false;
                }
                
                // 检查外部命令的 CanExecute
                var targetCommand = UpdateCommand;
                if (targetCommand == null)
                {
                    return false;
                }
                
                var commandParameter = SelectedItem ?? parameter;
                var canExecute = targetCommand.CanExecute(commandParameter);
                
                // 确保只有在外部命令允许执行时才返回 true
                return canExecute;
            });

        // 删除命令：单选或多选时激活（SelectedItemsCount > 0）
        // 默认禁用，只有选中行时才激活
        _internalDeleteCommand = new ViewModels.RelayCommand<object?>(
            parameter =>
            {
                try
                {
                    // 执行外部命令，传递 SelectedItem 或参数
                    var targetCommand = DeleteCommand;
                    if (targetCommand != null && SelectedItemsCount > 0)
                    {
                        // 始终传递 SelectedItem 或参数，不传递 SelectedItemsCount
                        // 如果有多选，ViewModel 应该通过 SelectedItemsCount 属性来处理
                        var commandParameter = SelectedItem ?? parameter;
                        if (commandParameter != null && targetCommand.CanExecute(commandParameter))
                        {
                            _operLog?.Information("[TaktDataGrid] 工具栏删除按钮点击: SelectedItemsCount={SelectedItemsCount}, SelectedItem={SelectedItem}", SelectedItemsCount, SelectedItem?.ToString() ?? "null");
                            targetCommand.Execute(commandParameter);
                            _operLog?.Information("[TaktDataGrid] 删除命令执行成功");
                        }
                        else
                        {
                            _operLog?.Information("[TaktDataGrid] 删除命令不可执行: commandParameter={CommandParameter}", commandParameter?.ToString() ?? "null");
                        }
                    }
                    else
                    {
                        _operLog?.Information("[TaktDataGrid] 删除按钮点击: 未选中项 (SelectedItemsCount={SelectedItemsCount})", SelectedItemsCount);
                    }
                }
                catch (Exception ex)
                {
                    _operLog?.Error(ex, "[TaktDataGrid] 删除操作失败");
                }
            },
            parameter =>
            {
                // 单选/多选删除：必须有选中项（SelectedItemsCount > 0），并且外部命令允许执行
                if (SelectedItemsCount == 0)
                {
                    return false;
                }
                
                // 检查外部命令的 CanExecute
                var targetCommand = DeleteCommand;
                if (targetCommand == null)
                {
                    return false;
                }
                
                // 始终传递 SelectedItem 或参数，不传递 SelectedItemsCount
                // 如果有多选，ViewModel 应该通过 SelectedItemsCount 属性来处理
                var commandParameter = SelectedItem ?? parameter;
                if (commandParameter == null)
                {
                    return false;
                }
                return targetCommand.CanExecute(commandParameter);
            });

        // 将内部命令设置为依赖属性，以便 XAML 绑定能正确工作
        InternalUpdateCommand = _internalUpdateCommand;
        InternalDeleteCommand = _internalDeleteCommand;
    }

    /// <summary>
    /// 当外部命令变化时，重新创建内部命令
    /// </summary>
    private static void OnUpdateCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid)
        {
            grid.CreateInternalCommands();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 当外部命令变化时，重新创建内部命令
    /// </summary>
    private static void OnDeleteCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid)
        {
            grid.CreateInternalCommands();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 当选中项变化时，触发命令重新查询
    /// </summary>
    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 当选中项数量变化时，触发命令重新查询
    /// </summary>
    private static void OnSelectedItemsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 内部更新命令（自动根据选中项启用/禁用）
    /// </summary>
    public ICommand? InternalUpdateCommand
    {
        get => (ICommand?)GetValue(InternalUpdateCommandProperty);
        private set => SetValue(InternalUpdateCommandProperty, value);
    }

    /// <summary>
    /// 内部删除命令（自动根据选中项启用/禁用）
    /// </summary>
    public ICommand? InternalDeleteCommand
    {
        get => (ICommand?)GetValue(InternalDeleteCommandProperty);
        private set => SetValue(InternalDeleteCommandProperty, value);
    }

    #endregion

    #region 生命周期

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _dataGrid ??= FindName("PART_DataGrid") as DataGrid;
        
        // 确保内部命令已创建，并触发初始状态查询
        if (_internalUpdateCommand == null || _internalDeleteCommand == null)
        {
            CreateInternalCommands();
        }
        
        // 确保 SelectionMode 是 Extended（支持多选）
        if (_dataGrid != null)
        {
            _dataGrid.SelectionMode = DataGridSelectionMode.Extended;
            _dataGrid.UnselectAll();
            SelectedItem = null;
            SelectedItemsCount = 0;

        }
        
        // 如果 ItemsSource 已经设置，应用列；否则等待 OnItemsSourceChanged
        if (_dataGrid != null && ItemsSource != null)
        {
            ApplyColumns();
        }
        
        // 监听窗口大小变化，更新弹窗位置
        var window = Window.GetWindow(this);
        if (window != null)
        {
            window.SizeChanged += OnWindowSizeChanged;
        }
        
        // 监听控件大小变化，更新弹窗位置
        SizeChanged += OnControlSizeChanged;
        
        UpdatePaginationTexts();
        AttachItemsSourceHandlers(ItemsSource);
        
        CommandManager.InvalidateRequerySuggested();
    }
    
    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateColumnChooserPopupPosition();
    }
    
    private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateColumnChooserPopupPosition();
    }
    
    private void UpdateColumnChooserPopupPosition()
    {
        var popup = FindName("ColumnChooserPopup") as Popup;
        if (popup != null && popup.IsOpen)
        {
            // 强制更新弹窗位置
            popup.HorizontalOffset = popup.HorizontalOffset == 0 ? 1 : 0;
            popup.HorizontalOffset = 0;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DetachItemsSourceHandlers();
        
        // 移除窗口大小变化监听
        var window = Window.GetWindow(this);
        if (window != null)
        {
            window.SizeChanged -= OnWindowSizeChanged;
        }
        
        // 移除控件大小变化监听
        SizeChanged -= OnControlSizeChanged;
    }

    #endregion

    #region 列定义处理

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid)
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
            _dataGrid.Columns.Clear();

            EnsureSelectionColumn();

            foreach (var definition in Columns.Where(c => c.IsVisible))
            {
                var textColumn = CreateTextColumn(definition);
                _dataGrid.Columns.Add(textColumn);
            }

            EnsureOperationColumn();
            UpdateSelectionHeaderIndicator();
        }
        finally
        {
            _isApplyingColumns = false;
        }
    }

    private System.Windows.Controls.DataGridColumn CreateTextColumn(TaktDataGridColumnDefinition definition)
    {
        // 如果 BindingPath 为空或 null，使用空字符串作为默认值
        var bindingPath = string.IsNullOrWhiteSpace(definition.BindingPath) ? string.Empty : definition.BindingPath;
        var binding = new Binding(bindingPath)
        {
            Mode = IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            StringFormat = definition.StringFormat,
            TargetNullValue = string.Empty,
            FallbackValue = string.Empty
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
        // 如果 IsNumeric 为 true，则右对齐；否则使用定义的对齐方式（默认为左对齐）
        var textAlignment = definition.IsNumeric ? TextAlignment.Right : definition.TextAlignment;
        elementStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, textAlignment));
        elementStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
        elementStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.NoWrap));
        // 只有当 BindingPath 不为空时才设置 ToolTip 绑定
        if (!string.IsNullOrWhiteSpace(bindingPath))
        {
            elementStyle.Setters.Add(new Setter(TextBlock.ToolTipProperty, new Binding(bindingPath)
            {
                TargetNullValue = string.Empty,
                FallbackValue = string.Empty
            }));
        }
        column.ElementStyle = elementStyle;

        var editingStyle = new Style(typeof(TextBox));
        editingStyle.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
        editingStyle.Setters.Add(new Setter(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto));
        editingStyle.Setters.Add(new Setter(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto));
        column.EditingElementStyle = editingStyle;

        return column;
    }

    #endregion


    #region 选择列

    private static void OnSelectionColumnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid && grid.IsLoaded)
        {
            grid.EnsureSelectionColumn();
        }
    }

    private void EnsureSelectionColumn()
    {
        SetValue(IsSelectionColumnVisiblePropertyKey, true);

        if (_dataGrid == null)
        {
            return;
        }

        if (_selectionColumn != null && !_dataGrid.Columns.Contains(_selectionColumn))
        {
            _selectionColumn = null;
        }

        _selectionHeaderCheckBox = null;

        var cellTemplate = BuildSelectionCellTemplate();
        var headerTemplate = BuildSelectionHeaderTemplate();

        if (_selectionColumn == null)
        {
            _selectionColumn = new System.Windows.Controls.DataGridTemplateColumn
            {
                Header = SelectionColumnHeader,
                CellTemplate = cellTemplate,
                HeaderTemplate = headerTemplate,
                CanUserSort = false,
                CanUserResize = false,
                IsReadOnly = false,
                Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                MinWidth = 40
            };

            _dataGrid.Columns.Insert(0, _selectionColumn);
        }
        else
        {
            _selectionColumn.Header = SelectionColumnHeader;
            _selectionColumn.CellTemplate = cellTemplate;
            _selectionColumn.HeaderTemplate = headerTemplate;
            _selectionColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            _selectionColumn.MinWidth = 40;
        }

        if (!_dataGrid.Columns.Contains(_selectionColumn))
        {
            _dataGrid.Columns.Insert(0, _selectionColumn);
        }

        UpdateSelectionHeaderIndicator();
    }

    private DataTemplate BuildSelectionCellTemplate()
    {
        var checkBox = new FrameworkElementFactory(typeof(CheckBox));
        checkBox.SetResourceReference(FrameworkElement.StyleProperty, "SelectionCheckBoxStyle");
        checkBox.SetValue(FocusableProperty, false);
        checkBox.SetValue(ToggleButton.IsThreeStateProperty, false);
        checkBox.SetValue(FrameworkElement.ToolTipProperty, Translate("common.selectionRowHint", "选择/取消选择该行"));
        checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsSelected")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1),
            Mode = BindingMode.TwoWay
        });
        checkBox.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnSelectionCellCheckBoxClick), true);

        return new DataTemplate { VisualTree = checkBox };
    }

    private DataTemplate BuildSelectionHeaderTemplate()
    {
        var stack = new FrameworkElementFactory(typeof(StackPanel));
        stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stack.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        stack.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

        var checkBox = new FrameworkElementFactory(typeof(CheckBox));
        checkBox.SetResourceReference(FrameworkElement.StyleProperty, "SelectionCheckBoxStyle");
        checkBox.SetValue(ToggleButton.IsThreeStateProperty, true);
        checkBox.SetValue(FocusableProperty, false);
        checkBox.SetValue(FrameworkElement.ToolTipProperty, Translate("common.selectionHeaderHint", "全选/取消全选"));
        checkBox.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnSelectionHeaderCheckBoxClick));
        checkBox.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnSelectionHeaderCheckBoxLoaded));

        stack.AppendChild(checkBox);

        return new DataTemplate { VisualTree = stack };
    }

    private void OnSelectionHeaderCheckBoxLoaded(object sender, RoutedEventArgs e)
    {
        _selectionHeaderCheckBox = sender as CheckBox;
        UpdateSelectionHeaderIndicator();
    }

    private void OnSelectionHeaderCheckBoxClick(object? sender, RoutedEventArgs e)
    {
        if (_dataGrid == null || sender is not CheckBox checkBox)
        {
            return;
        }

        if (_dataGrid.SelectionMode != DataGridSelectionMode.Extended)
        {
            _dataGrid.SelectionMode = DataGridSelectionMode.Extended;
        }

        var selectableCount = GetSelectableItemsCount();
        var selectedCount = _dataGrid.SelectedItems.Cast<object>()
            .Count(item => item != CollectionView.NewItemPlaceholder);

        var shouldSelectAll = selectedCount < selectableCount;

        if (shouldSelectAll)
        {
            _dataGrid.SelectAll();
            checkBox.IsChecked = true;
        }
        else
        {
            _dataGrid.UnselectAll();
            checkBox.IsChecked = false;
        }

        e.Handled = true;
        UpdateSelectionHeaderIndicator();
    }

    private void UpdateSelectionHeaderIndicator()
    {
        if (_selectionHeaderCheckBox == null || _dataGrid == null)
        {
            return;
        }

        var selectableCount = GetSelectableItemsCount();
        var selectedCount = _dataGrid.SelectedItems.Cast<object>()
            .Count(item => item != CollectionView.NewItemPlaceholder);

        if (selectableCount <= 0 || selectedCount <= 0)
        {
            _selectionHeaderCheckBox.IsChecked = false;
            return;
        }

        if (selectedCount >= selectableCount)
        {
            _selectionHeaderCheckBox.IsChecked = true;
        }
        else
        {
            _selectionHeaderCheckBox.IsChecked = null;
        }
    }

    private int GetSelectableItemsCount()
    {
        if (_dataGrid == null)
        {
            return 0;
        }

        return _dataGrid.Items.Cast<object>()
            .Count(item => item != CollectionView.NewItemPlaceholder);
    }

    private void RefreshPageButtons(int totalPages, int currentPage)
    {
        _pageButtons.Clear();

        if (totalPages <= 0)
        {
            return;
        }

        const int maxButtons = 5;
        var start = Math.Max(1, currentPage - 2);
        var end = Math.Min(totalPages, start + maxButtons - 1);

        if (end - start + 1 < maxButtons)
        {
            start = Math.Max(1, end - maxButtons + 1);
        }

        for (var page = start; page <= end; page++)
        {
            _pageButtons.Add(new PageButtonInfo(page, page == currentPage));
        }
    }

    private void OnSelectionCellCheckBoxClick(object? sender, RoutedEventArgs e)
    {
        if (_dataGrid == null || sender is not CheckBox checkBox)
        {
            return;
        }

        if (_dataGrid.SelectionMode != DataGridSelectionMode.Extended)
        {
            _dataGrid.SelectionMode = DataGridSelectionMode.Extended;
        }

        var row = FindAncestor<DataGridRow>(checkBox);
        if (row == null)
        {
            return;
        }

        var newState = !row.IsSelected;
        row.IsSelected = newState;
        checkBox.IsChecked = newState;

        e.Handled = true;
        UpdateSelectionHeaderIndicator();
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

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T target)
            {
                return target;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    #endregion

    public sealed class PageButtonInfo
    {
        public PageButtonInfo(int number, bool isCurrent)
        {
            Number = number;
            IsCurrent = isCurrent;
        }

        public int Number { get; }
        public bool IsCurrent { get; }
    }

    #region 操作按钮状态转换规则管理

    /// <summary>
    /// 初始化操作按钮状态转换规则
    /// 统一管理启动、停止、暂停、重启、恢复之间的关联关系
    /// </summary>
    private void InitializeOperationStateRules()
    {
        // 运行（Run）：只能在启用状态(0)时执行，不变更状态
        _operationStateRules["Run"] = OperationButtonStateRule
            .For("Run")
            .AllowStates(0)
            .Build();

        // 启动（Start）：只能在禁用状态(1)时执行，将状态从1改为0
        _operationStateRules["Start"] = OperationButtonStateRule
            .For("Start")
            .AllowStates(1)
            .Build();

        // 停止（Stop）：只能在启用状态(0)时执行，将状态从0改为1
        _operationStateRules["Stop"] = OperationButtonStateRule
            .For("Stop")
            .AllowStates(0)
            .Build();

        // 暂停（Pause）：只能在启用状态(0)时执行，将状态从0改为3（暂停）
        // 注意：如果未来支持暂停状态，需要扩展状态模型
        _operationStateRules["Pause"] = OperationButtonStateRule
            .For("Pause")
            .AllowStates(0)
            .Build();

        // 恢复（Resume）：只能在禁用状态(1)时执行，将状态从1改为0
        // 注意：如果未来支持暂停状态，也需要支持从暂停状态(3)恢复
        _operationStateRules["Resume"] = OperationButtonStateRule
            .For("Resume")
            .AllowStates(1)  // 当前只支持从禁用恢复，未来可扩展为 AllowStates(1, 3)
            .Build();

        // 重启（Restart）：可以在暂停状态(3)或启用状态(0)时执行
        // 注意：如果未来支持暂停状态，可以重启暂停的任务
        _operationStateRules["Restart"] = OperationButtonStateRule
            .For("Restart")
            .AllowStates(0)  // 当前只支持在启用状态重启，未来可扩展为 AllowStates(0, 3)
            .ExcludeStates(1)  // 禁用状态不能重启
            .Build();
    }

    /// <summary>
    /// 根据状态转换规则判断操作按钮是否应该显示
    /// </summary>
    private bool ShouldShowOperationButton(string operationName, int status)
    {
        var rule = _operationStateRules.GetRule(operationName);
        if (rule == null)
        {
            // 如果没有定义规则，默认显示
            return true;
        }
        return rule.IsAllowed(status);
    }

    /// <summary>
    /// 使用状态转换规则添加操作按钮（通用方法）
    /// </summary>
    private void AppendOperationButtonWithRule(
        FrameworkElementFactory parent,
        bool isVisible,
        string operationName,
        string commandPropertyName,
        string translationKey,
        string fallback,
        PackIconKind iconKind,
        string? styleKey = null!, 
        string? statusPropertyName = "Status")
    {
        if (!isVisible)
        {
            return;
        }

        var button = new FrameworkElementFactory(typeof(Button));
        button.SetResourceReference(FrameworkElement.StyleProperty, styleKey ?? RowOperationStyleKey);
        button.SetValue(Button.ToolTipProperty, Translate(translationKey, fallback));

        var commandBinding = new Binding(commandPropertyName)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TaktDataGrid), 1)
        };
        button.SetBinding(Button.CommandProperty, commandBinding);

        var commandParameterBinding = new Binding("DataContext")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
        };
        button.SetBinding(Button.CommandParameterProperty, commandParameterBinding);

        // 获取状态转换规则
        var rule = _operationStateRules.GetRule(operationName);
        if (rule != null)
        {
            // 使用状态转换规则控制按钮可见性
            var visibilityBinding = new MultiBinding
            {
                Converter = new StateBasedVisibilityConverter(
                    rule.AllowedStates.Count > 0 ? rule.AllowedStates.ToArray() : null,
                    rule.ExcludedStates.Count > 0 ? rule.ExcludedStates.ToArray() : null),
                Mode = BindingMode.OneWay
            };
            
            // 绑定状态属性：从 DataGridRow 的 DataContext（行数据对象）获取状态值
            // 使用 PropertyPath 明确指定绑定路径
            var statusBinding = new Binding
            {
                Path = new PropertyPath("DataContext." + statusPropertyName),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1),
                Mode = BindingMode.OneWay
            };
            visibilityBinding.Bindings.Add(statusBinding);
            
            button.SetBinding(Button.VisibilityProperty, visibilityBinding);
        }

        var icon = new FrameworkElementFactory(typeof(PackIcon));
        icon.SetValue(PackIcon.KindProperty, iconKind);
        icon.SetValue(FrameworkElement.WidthProperty, 16.0);
        icon.SetValue(FrameworkElement.HeightProperty, 16.0);
        icon.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        icon.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        button.AppendChild(icon);
        parent.AppendChild(button);
    }

    #endregion

    #region 行内操作列

    private static void OnOperationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid && grid.IsLoaded)
        {
            grid.EnsureOperationColumn();
        }
    }

    private void EnsureOperationColumn()
    {
        if (_dataGrid == null)
        {
            return;
        }

        // 检查是否有任何操作按钮需要显示
        bool hasAnyOperation = ShowRowDetail || ShowRowPreview || ShowRowPrint || ShowRowCreate || 
                               ShowRowUpdate || ShowRowDelete || ShowRowAssignRole || ShowRowAssignDept || 
                               ShowRowAssignMenu || ShowRowAssignPost || ShowRowAuthorize || ShowRowRun || 
                               ShowRowGenerate || ShowRowStart || ShowRowStop || ShowRowPause || 
                               ShowRowRestart || ShowRowResume || ShowRowClone || ShowRowReset || ShowRowSync;

        if (!hasAnyOperation)
        {
            // 没有任何操作按钮，移除操作列
            if (_operationColumn != null && _dataGrid.Columns.Contains(_operationColumn))
            {
                _dataGrid.Columns.Remove(_operationColumn);
            }
            SetValue(IsOperationColumnVisiblePropertyKey, false);
            return;
        }

        SetValue(IsOperationColumnVisiblePropertyKey, true);

        if (_operationColumn != null && !_dataGrid.Columns.Contains(_operationColumn))
        {
            _operationColumn = null;
        }

        var template = BuildOperationTemplate();
        if (_operationColumn == null)
        {
            _operationColumn = new System.Windows.Controls.DataGridTemplateColumn
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

    private DataTemplate BuildOperationTemplate()
    {
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

        AppendOperationButton(stackPanel, ShowRowDetail, nameof(RowDetailCommand), "common.button.detail", "详情", PackIconKind.Eye, "DefaultIconPlainInfoSmall");
        AppendOperationButton(stackPanel, ShowRowPreview, nameof(RowPreviewCommand), "common.button.preview", "预览", PackIconKind.EyeOutline, "DefaultIconPlainInfoSmall");
        AppendOperationButton(stackPanel, ShowRowPrint, nameof(RowPrintCommand), "common.button.print", "打印", PackIconKind.Printer, "DefaultIconPlainInfoSmall");
        AppendOperationButton(stackPanel, ShowRowCreate, nameof(RowCreateCommand), "common.button.create", "新增", PackIconKind.PlaylistPlus, "DefaultIconPlainPrimarySmall");
        AppendOperationButton(stackPanel, ShowRowUpdate, nameof(RowUpdateCommand), "common.button.update", "编辑", PackIconKind.Pencil, "DefaultIconPlainSuccessSmall");
        AppendOperationButton(stackPanel, ShowRowDelete, nameof(RowDeleteCommand), "common.button.delete", "删除", PackIconKind.Delete, "DefaultIconPlainWarningSmall");
        AppendOperationButton(stackPanel, ShowRowAssignRole, nameof(RowAssignRoleCommand), "common.button.assignrole", "分配角色", PackIconKind.AccountMultiple, "DefaultIconPlainFeatureSmall");
        AppendOperationButton(stackPanel, ShowRowAssignDept, nameof(RowAssignDeptCommand), "common.button.assigndept", "分配部门", PackIconKind.OfficeBuilding, "DefaultIconPlainFeatureSmall");
        AppendOperationButton(stackPanel, ShowRowAssignMenu, nameof(RowAssignMenuCommand), "common.button.assignmenu", "分配菜单", PackIconKind.Menu, "DefaultIconPlainFeatureSmall");
        AppendOperationButton(stackPanel, ShowRowAssignPost, nameof(RowAssignPostCommand), "common.button.assignpost", "分配岗位", PackIconKind.Briefcase, "DefaultIconPlainFeatureSmall");
        AppendOperationButton(stackPanel, ShowRowAuthorize, nameof(RowAuthorizeCommand), "common.button.authorize", "授权", PackIconKind.ShieldCheck, "DefaultIconPlainPromoSmall");
        // 运行按钮：使用状态转换规则（只能在启用状态(0)时执行）
        // 启动、停止、运行按钮：使用状态转换规则
        AppendOperationButtonWithRule(stackPanel, ShowRowStart, "Start", nameof(RowStartCommand), "common.button.start", "启动", PackIconKind.PlayCircle, "DefaultIconPlainBrandSmall");
        AppendOperationButtonWithRule(stackPanel, ShowRowStop, "Stop", nameof(RowStopCommand), "common.button.stop", "停止", PackIconKind.StopCircle, "DefaultIconPlainWarningSmall");
        AppendOperationButtonWithRule(stackPanel, ShowRowRun, "Run", nameof(RowRunCommand), "common.button.run", "运行", PackIconKind.Play, "DefaultIconPlainAccentSmall");
        // 暂停、重启、恢复按钮：不使用状态转换规则，默认不可见
        AppendOperationButton(stackPanel, ShowRowPause, nameof(RowPauseCommand), "common.button.pause", "暂停", PackIconKind.PauseCircle, "DefaultIconPlainWarningSmall");
        AppendOperationButton(stackPanel, ShowRowRestart, nameof(RowRestartCommand), "common.button.restart", "重启", PackIconKind.Refresh, "DefaultIconPlainDangerSmall");
        AppendOperationButton(stackPanel, ShowRowResume, nameof(RowResumeCommand), "common.button.resume", "恢复", PackIconKind.PlayCircleOutline, "DefaultIconPlainSuccessSmall");
        AppendOperationButton(stackPanel, ShowRowGenerate, nameof(RowGenerateCommand), "common.button.generate", "生成", PackIconKind.CodeTags, "DefaultIconPlainAccentSmall");
        AppendOperationButton(stackPanel, ShowRowSync, nameof(RowSyncCommand), "common.button.sync", "同步", PackIconKind.Sync, "DefaultIconPlainInfoSmall");
        AppendOperationButton(stackPanel, ShowRowClone, nameof(RowCloneCommand), "common.button.clone", "克隆", PackIconKind.ContentCopy, "DefaultIconPlainFeatureSmall");
        AppendOperationButton(stackPanel, ShowRowReset, nameof(RowResetCommand), "common.button.reset", "重置", PackIconKind.Restore, "DefaultIconPlainWarningSmall");

        return new DataTemplate { VisualTree = stackPanel };
    }

    private void AppendOperationButton(FrameworkElementFactory parent, bool isVisible, string commandPropertyName, string translationKey, string fallback, PackIconKind iconKind, string? styleKey = null)
    {
        if (!isVisible)
        {
            return;
        }

        var button = new FrameworkElementFactory(typeof(Button));
        button.SetResourceReference(FrameworkElement.StyleProperty, styleKey ?? RowOperationStyleKey);
        button.SetValue(Button.ToolTipProperty, Translate(translationKey, fallback));

        var commandBinding = new Binding(commandPropertyName)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TaktDataGrid), 1)
        };
        button.SetBinding(Button.CommandProperty, commandBinding);

        var commandParameterBinding = new Binding("DataContext")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
        };
        button.SetBinding(Button.CommandParameterProperty, commandParameterBinding);

        var icon = new FrameworkElementFactory(typeof(PackIcon));
        icon.SetValue(PackIcon.KindProperty, iconKind);
        icon.SetValue(FrameworkElement.WidthProperty, 16.0);
        icon.SetValue(FrameworkElement.HeightProperty, 16.0);
        icon.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        icon.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        button.AppendChild(icon);
        parent.AppendChild(button);
    }

    /// <summary>
    /// 添加操作按钮，支持基于行数据的条件显示和状态转换逻辑
    /// </summary>
    /// <param name="parent">父容器</param>
    /// <param name="isVisible">全局是否可见</param>
    /// <param name="commandPropertyName">命令属性名</param>
    /// <param name="translationKey">翻译键</param>
    /// <param name="fallback">后备文本</param>
    /// <param name="iconKind">图标类型</param>
    /// <param name="styleKey">样式键</param>
    /// <param name="conditionProperty">条件属性名（行数据中的属性，如 "Status"）</param>
    /// <param name="conditionValue">条件值（当属性值等于此值时显示，支持单个值或数组）</param>
    /// <param name="excludeValues">排除的状态值（当属性值等于这些值时隐藏）</param>
    private void AppendOperationButtonWithCondition(FrameworkElementFactory parent, bool isVisible, string commandPropertyName, string translationKey, string fallback, PackIconKind iconKind, string? styleKey = null!, string? conditionProperty = null!, object? conditionValue = null!, int[]? excludeValues = null)
    {
        if (!isVisible)
        {
            return;
        }

        var button = new FrameworkElementFactory(typeof(Button));
        button.SetResourceReference(FrameworkElement.StyleProperty, styleKey ?? RowOperationStyleKey);
        button.SetValue(Button.ToolTipProperty, Translate(translationKey, fallback));

        var commandBinding = new Binding(commandPropertyName)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TaktDataGrid), 1)
        };
        button.SetBinding(Button.CommandProperty, commandBinding);

        var commandParameterBinding = new Binding("DataContext")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
        };
        button.SetBinding(Button.CommandParameterProperty, commandParameterBinding);

        // 如果指定了条件属性，添加条件绑定来控制可见性
        if (!string.IsNullOrEmpty(conditionProperty) && conditionValue != null)
        {
            // 创建多值转换器来处理复杂的状态转换逻辑
            var visibilityBinding = new MultiBinding
            {
                Converter = new Takt.Fluent.Helpers.StateBasedVisibilityConverter(conditionValue, excludeValues),
                Mode = BindingMode.OneWay
            };
            
            // 绑定状态属性：从 DataGridRow 的 DataContext（行数据对象）获取状态值
            visibilityBinding.Bindings.Add(new Binding("DataContext." + conditionProperty)
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
            });
            
            button.SetBinding(Button.VisibilityProperty, visibilityBinding);
        }

        var icon = new FrameworkElementFactory(typeof(PackIcon));
        icon.SetValue(PackIcon.KindProperty, iconKind);
        icon.SetValue(FrameworkElement.WidthProperty, 16.0);
        icon.SetValue(FrameworkElement.HeightProperty, 16.0);
        icon.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        icon.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        button.AppendChild(icon);
        parent.AppendChild(button);
    }

    #endregion

    #region 分页处理

    private static void OnPaginationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid)
        {
            grid.HandlePaginationPropertyChanged(e);
        }
    }

    private void HandlePaginationPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.Property == PageSizeProperty)
        {
            if (PageSize <= 0)
            {
                SetCurrentValue(PageSizeProperty, 1);
                return;
            }

            NavigateToPage(1, false);
            RaisePageSizeChanged();
        }
        else if (e.Property == PageIndexProperty)
        {
            if (PageIndex <= 0)
            {
                SetCurrentValue(PageIndexProperty, 1);
                return;
            }

            UpdatePaginationTexts();
            if (!_internalPageChange)
            {
                RaisePageChanged(PageIndex);
            }
        }
        else if (e.Property == TotalCountProperty)
        {
            UpdatePaginationTexts();
        }
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktDataGrid grid)
        {
            grid.AttachItemsSourceHandlers(e.NewValue);
            grid.UpdatePaginationTexts();
            
            
            // ItemsSource 变化时，清除选中项，确保更新和删除按钮默认禁用
            if (grid._dataGrid != null)
            {
                grid._dataGrid.UnselectAll();
                grid.SelectedItem = null;
                grid.SelectedItemsCount = 0;
                // 延迟执行，确保 UI 完全更新后再清除
                grid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (grid._dataGrid != null)
                    {
                        grid._dataGrid.UnselectAll();
                        grid.SelectedItem = null;
                        grid.SelectedItemsCount = 0;
                        grid.UpdateSelectedItemsCount();
                        CommandManager.InvalidateRequerySuggested();
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
    }

    private void UpdatePaginationTexts()
    {
        var totalPages = CalculateTotalPages();
        var currentPage = Math.Clamp(PageIndex, 1, totalPages);
        SetCurrentValue(PageDisplayProperty, Format("common.pageDisplay", "第 {0} / {1} 页", currentPage, totalPages));
        SetCurrentValue(TotalTextProperty, Format("common.total", "共 {0} 条记录", Math.Max(GetEffectiveTotalCount(), 0)));

        HasPreviousPage = currentPage > 1;
        HasNextPage = currentPage < totalPages;
        RefreshPageButtons(totalPages, currentPage);
    }


    private void AttachItemsSourceHandlers(object? source)
    {
        DetachItemsSourceHandlers();

        if (source is ICollectionView collectionView)
        {
            if (collectionView is INotifyCollectionChanged viewNotifier)
            {
                _itemsSourceNotifier = viewNotifier;
            }
            else if (collectionView.SourceCollection is INotifyCollectionChanged sourceCollection)
            {
                _itemsSourceNotifier = sourceCollection;
            }
        }
        else if (source is INotifyCollectionChanged notifier)
        {
            _itemsSourceNotifier = notifier;
        }

        if (_itemsSourceNotifier != null)
        {
            _itemsSourceNotifier.CollectionChanged += ItemsSource_CollectionChanged;
        }

        UpdateEmptyState();
        UpdateSelectionHeaderIndicator();
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
        UpdateSelectionHeaderIndicator();
        
        // 数据集合变化时，清除选中项，确保更新和删除按钮默认禁用
        if (_dataGrid != null && e.Action == NotifyCollectionChangedAction.Reset)
        {
            _dataGrid.UnselectAll();
            SelectedItem = null;
            SelectedItemsCount = 0;
            // 延迟执行，确保 UI 完全更新后再清除
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_dataGrid != null)
                {
                    _dataGrid.UnselectAll();
                    SelectedItem = null;
                    SelectedItemsCount = 0;
                    UpdateSelectedItemsCount();
                    CommandManager.InvalidateRequerySuggested();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void UpdateEmptyState()
    {
        IsEmpty = GetItemsSourceCount() <= 0;
    }

    private int CalculateTotalPages()
    {
        if (PageSize <= 0)
        {
            return 1;
        }

        var effectiveTotal = Math.Max(GetEffectiveTotalCount(), 0);
        var totalPages = (int)Math.Ceiling(effectiveTotal / (double)PageSize);
        return totalPages <= 0 ? 1 : totalPages;
    }

    private int GetEffectiveTotalCount()
    {
        if (TotalCount > 0)
        {
            return TotalCount;
        }

        return GetItemsSourceCount();
    }

    private int GetItemsSourceCount()
    {
        if (ItemsSource is null)
        {
            return 0;
        }

        if (ItemsSource is ICollection collection)
        {
            return collection.Count;
        }

        if (ItemsSource is ICollectionView collectionView)
        {
            return collectionView.Cast<object>().Count();
        }

        if (ItemsSource is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Count();
        }

        return 0;
    }

    private void NavigateToPage(int targetPage, bool clearGotoText = true)
    {
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

        UpdatePaginationTexts();
        RaisePageChanged(targetPage);

        if (clearGotoText)
        {
            SetCurrentValue(GoToPageTextProperty, string.Empty);
        }
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

    private void RaisePageSizeChanged()
    {
        var context = new PageRequest(PageIndex, PageSize, this);
        PageSizeChanged?.Invoke(this, context);
        if (PageSizeChangedCommand is { } command && command.CanExecute(context))
        {
            command.Execute(context);
        }
    }

    #endregion

    #region 事件处理 - 查询与分页

    private void OnQueryButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _operLog?.Information("[TaktDataGrid] 查询按钮点击: Keyword={Keyword}, PageIndex={PageIndex}, PageSize={PageSize}", QueryKeyword, PageIndex, PageSize);
            NavigateToPage(1, false);

            var context = new QueryContext(QueryKeyword ?? string.Empty, PageIndex, PageSize, this);
            QueryRequested?.Invoke(this, context);
            if (QueryCommand is { } command && command.CanExecute(context))
            {
                command.Execute(context);
                _operLog?.Information("[TaktDataGrid] 查询命令执行成功");
            }
            else
            {
                _operLog?.Information("[TaktDataGrid] 查询命令不可执行或未设置");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 查询操作失败");
        }
    }

    private void OnResetButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _operLog?.Information("[TaktDataGrid] 重置按钮点击: 开始重置操作, PageIndex={PageIndex}, PageSize={PageSize}", PageIndex, PageSize);
            SetCurrentValue(QueryKeywordProperty, string.Empty);

            var context = new QueryContext(string.Empty, PageIndex, PageSize, this);
            if (ResetCommand is { } command && command.CanExecute(context))
            {
                command.Execute(context);
                _operLog?.Information("[TaktDataGrid] 重置命令执行成功");
            }
            else
            {
                _operLog?.Information("[TaktDataGrid] 重置命令不可执行或未设置，尝试执行查询命令");
                QueryRequested?.Invoke(this, context);
                if (QueryCommand is { } queryCommand && queryCommand.CanExecute(context))
                {
                    queryCommand.Execute(context);
                    _operLog?.Information("[TaktDataGrid] 查询命令执行成功（作为重置的替代）");
                }
                else
                {
                    _operLog?.Information("[TaktDataGrid] 查询命令也不可执行或未设置");
                }
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 重置操作失败");
        }
    }

    private void OnFirstPageButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _operLog?.Information("[TaktDataGrid] 第一页按钮点击");
            NavigateToPage(1);
            ExecuteOptionalCommand(FirstPageCommand);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 第一页操作失败");
        }
    }

    private void OnPrevPageButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _operLog?.Information("[TaktDataGrid] 上一页按钮点击: 当前页={PageIndex}", PageIndex);
            NavigateToPage(PageIndex - 1);
            ExecuteOptionalCommand(PrevPageCommand);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 上一页操作失败");
        }
    }

    private void OnNextPageButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _operLog?.Information("[TaktDataGrid] 下一页按钮点击: 当前页={PageIndex}", PageIndex);
            NavigateToPage(PageIndex + 1);
            ExecuteOptionalCommand(NextPageCommand);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 下一页操作失败");
        }
    }

    private void OnLastPageButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var totalPages = CalculateTotalPages();
            _operLog?.Information("[TaktDataGrid] 最后一页按钮点击: 总页数={TotalPages}", totalPages);
            NavigateToPage(totalPages);
            ExecuteOptionalCommand(LastPageCommand);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 最后一页操作失败");
        }
    }

    private void OnGoToPageButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(GoToPageText))
            {
                return;
            }

            if (int.TryParse(GoToPageText, out var page))
            {
                _operLog?.Information("[TaktDataGrid] 跳转页面按钮点击: 目标页={Page}", page);
                NavigateToPage(page);
                ExecuteOptionalCommand(GoToPageCommand);
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 跳转页面操作失败");
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

    private void OnPageSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // SelectionChanged 事件在绑定更新之后触发
        // 如果绑定已经更新了 PageSize，HandlePaginationPropertyChanged 应该已经被触发
        // 这里作为备用机制，确保 PageSize 被正确设置
        if (sender is ComboBox comboBox && comboBox.SelectedItem is int newPageSize && newPageSize > 0)
        {
            // 只有当值真正改变时才更新，避免重复触发
            if (PageSize != newPageSize)
            {
                _operLog?.Information("[TaktDataGrid] 每页数量变更: {OldPageSize} -> {NewPageSize}", PageSize, newPageSize);
                // 设置 PageSize 会触发 OnPaginationPropertyChanged -> HandlePaginationPropertyChanged
                // HandlePaginationPropertyChanged 会自动处理：
                // 1. NavigateToPage(1, false) - 重置到第一页
                // 2. RaisePageSizeChanged() - 触发分页大小变更事件和命令
                SetCurrentValue(PageSizeProperty, newPageSize);
            }
            else
            {
                // 如果值已经相同（可能通过绑定已更新），但需要确保事件被触发
                // 这种情况通常不会发生，因为绑定更新应该已经触发了 PropertyChanged
                // 但为了保险起见，我们仍然触发一次（使用 _internalPageChange 避免重复）
                _internalPageChange = true;
                try
                {
                    NavigateToPage(1, false);
                    RaisePageSizeChanged();
                }
                finally
                {
                    _internalPageChange = false;
                }
            }
        }
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectionHeaderIndicator();
        UpdateSelectedItemsCount();
        
        // 触发命令重新查询，更新按钮启用状态
        CommandManager.InvalidateRequerySuggested();
    }

    private void UpdateSelectedItemsCount()
    {
        if (_dataGrid?.SelectedItems == null)
        {
            SelectedItemsCount = 0;
            SelectedItemsCountChanged?.Invoke(this, 0);
            // 确保 SelectedItem 也为 null
            if (SelectedItem != null)
            {
                SelectedItem = null;
            }
            return;
        }

        var count = _dataGrid.SelectedItems.Cast<object>()
            .Count(item => item != CollectionView.NewItemPlaceholder);
        SelectedItemsCount = count;
        SelectedItemsCountChanged?.Invoke(this, count);
        
        // 如果选中项数量为 0，确保 SelectedItem 也为 null
        if (count == 0 && SelectedItem != null)
        {
            SelectedItem = null;
        }
        // 如果选中项数量为 1，确保 SelectedItem 与 DataGrid 的 SelectedItem 同步
        else if (count == 1 && _dataGrid.SelectedItem != null && SelectedItem != _dataGrid.SelectedItem)
        {
            SelectedItem = _dataGrid.SelectedItem;
        }
    }

    private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_dataGrid == null)
        {
            return;
        }

        // 确保 SelectionMode 是 Extended（支持多选）
        if (_dataGrid.SelectionMode != DataGridSelectionMode.Extended)
        {
            _dataGrid.SelectionMode = DataGridSelectionMode.Extended;
        }

        // 查找点击的元素
        var hitTestResult = VisualTreeHelper.HitTest(_dataGrid, e.GetPosition(_dataGrid));
        if (hitTestResult == null)
        {
            return;
        }

        // 检查是否点击在复选框、按钮或其他交互控件上，如果是则让默认行为处理
        var element = hitTestResult.VisualHit;
        while (element != null && element != _dataGrid)
        {
            // 如果点击的是复选框、按钮或其他交互控件，不处理（让默认行为处理）
            if (element is CheckBox || element is Button || element is ToggleButton)
            {
                return;
            }

            // 如果点击的是 DataGridRow，处理多选逻辑
            if (element is DataGridRow row)
            {
                // 如果按住 Ctrl 或 Shift，使用默认行为（扩展选择）
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
                    Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    return;
                }

                // 阻止默认的单选行为
                e.Handled = true;

                // 切换行的选中状态
                if (row.IsSelected)
                {
                    // 如果行已选中，取消选中
                    row.IsSelected = false;
                }
                else
                {
                    // 如果行未选中，选中它（不取消其他行的选中）
                    row.IsSelected = true;
                }

                // 更新选中项计数和表头指示器
                UpdateSelectionHeaderIndicator();
                UpdateSelectedItemsCount();
                CommandManager.InvalidateRequerySuggested();

                return;
            }

            element = VisualTreeHelper.GetParent(element) as DependencyObject;
        }
    }

    private void OnToggleQueryAreaClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var newValue = !ShowQueryArea;
            _operLog?.Information("[TaktDataGrid] 切换查询区域: {OldValue} -> {NewValue}", ShowQueryArea, newValue);
            SetCurrentValue(ShowQueryAreaProperty, newValue);
            _operLog?.Information("[TaktDataGrid] 切换查询区域成功");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 切换查询区域失败");
        }
    }

    private void OnToggleColumnPanelClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var next = !IsColumnPanelOpen;
            _operLog?.Information("[TaktDataGrid] 切换列选择面板: {OldValue} -> {NewValue}", IsColumnPanelOpen, next);
            SetCurrentValue(IsColumnPanelOpenProperty, next);
            
            // 如果打开弹窗，延迟更新位置以确保布局完成
            if (next)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var popup = FindName("ColumnChooserPopup") as Popup;
                    if (popup != null)
                    {
                        // 强制更新弹窗位置
                        popup.HorizontalOffset = popup.HorizontalOffset == 0 ? 1 : 0;
                        popup.HorizontalOffset = 0;
                    }
                }), DispatcherPriority.Loaded);
            }
            
            _operLog?.Information("[TaktDataGrid] 切换列选择面板成功");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktDataGrid] 切换列选择面板失败");
        }
    }

    private void OnColumnChooserClosed(object? sender, EventArgs e)
    {
        SetCurrentValue(IsColumnPanelOpenProperty, false);
    }

    private CustomPopupPlacement[] OnColumnChooserPlacement(Size popupSize, Size targetSize, Point offset)
    {
        // 获取主视图（UserControl）的宽度
        var mainViewWidth = ActualWidth;
        if (mainViewWidth <= 0)
        {
            mainViewWidth = this.RenderSize.Width;
        }
        
        // 计算弹窗位置：右边框对齐主视图右边框，保持8px间距
        // popupSize.Width 是弹窗宽度，mainViewWidth 是主视图宽度
        // 弹窗右边框 = 主视图右边框 - 8px
        // 所以弹窗左边框 = 主视图右边框 - 8px - 弹窗宽度
        // 转换为相对于 PlacementTarget 的偏移
        var button = FindName("ColumnToggleButton") as FrameworkElement;
        if (button != null)
        {
            // 获取按钮相对于主视图的位置
            var buttonPosition = button.TransformToAncestor(this).Transform(new Point(0, 0));
            
            // 计算弹窗应该的左边位置：主视图右边 - 8px - 弹窗宽度
            var popupLeft = mainViewWidth - 8 - popupSize.Width;
            
            // 计算相对于按钮的偏移
            var horizontalOffset = popupLeft - buttonPosition.X;
            var verticalOffset = 4; // 按钮下方4px
            
            return new[]
            {
                new CustomPopupPlacement(new Point(horizontalOffset, verticalOffset), PopupPrimaryAxis.Horizontal)
            };
        }
        
        // 如果找不到按钮，使用默认位置
        return new[]
        {
            new CustomPopupPlacement(new Point(-popupSize.Width - 4, targetSize.Height + 4), PopupPrimaryAxis.Horizontal)
        };
    }

    #endregion

    private static string Translate(string key, string fallback)
    {
        var localizationManager = App.Services?.GetService<Takt.Domain.Interfaces.ILocalizationManager>();
        if (localizationManager == null) return fallback;
        var translation = localizationManager.GetString(key);
        return (translation == key) ? fallback : translation;
    }

    private static string Format(string key, string fallback, params object[] args)
    {
        var template = Translate(key, fallback);
        try
        {
            return string.Format(CultureInfo.CurrentUICulture, template, args);
        }
        catch (FormatException)
        {
            return fallback;
        }
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            // 设置 DataContext 以便 ContextMenu 中的绑定可以访问命令
            if (button.ContextMenu.DataContext == null)
            {
                button.ContextMenu.DataContext = DataContext;
            }
            button.ContextMenu.IsOpen = true;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            // 设置 DataContext 以便 ContextMenu 中的绑定可以访问命令
            if (button.ContextMenu.DataContext == null)
            {
                button.ContextMenu.DataContext = DataContext;
            }
            button.ContextMenu.IsOpen = true;
        }
    }
}


