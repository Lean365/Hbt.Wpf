// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：TaktTreeView.xaml.cs
// 创建时间：2025-11-14
// 创建人：Takt365(Cursor AI)
// 功能描述：完整的 MaterialDesign 风格通用树形视图控件，集成查询、工具栏、展开/收缩功能，与 CustomizeDataGrid 功能一致。
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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Takt.Common.Logging;
using Takt.Fluent;
using Takt.Fluent.ViewModels;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.Controls;

/// <summary>
/// 树形视图查询上下文。
/// </summary>
public sealed record TaktTreeViewQueryContext(string Keyword, TaktTreeView Sender);

public partial class TaktTreeView : UserControl
{
    private TreeView? _treeView;
    private INotifyCollectionChanged? _itemsSourceNotifier;

    // 内部命令包装器，用于自动启用/禁用
    private ICommand? _internalUpdateCommand;
    private ICommand? _internalDeleteCommand;

    // 操作日志管理器
    private OperLogManager? _operLog;

    private static readonly DependencyPropertyKey IsAllExpandedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsAllExpanded), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAllExpandedProperty = IsAllExpandedPropertyKey.DependencyProperty;

    /// <summary>
    /// 是否全部展开（只读属性，用于控制展开/收缩按钮的显示）
    /// </summary>
    public bool IsAllExpanded
    {
        get => (bool)GetValue(IsAllExpandedProperty);
        private set => SetValue(IsAllExpandedPropertyKey, value);
    }

    public static readonly DependencyProperty InternalUpdateCommandProperty =
        DependencyProperty.Register(nameof(InternalUpdateCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InternalDeleteCommandProperty =
        DependencyProperty.Register(nameof(InternalDeleteCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public TaktTreeView()
    {
        var resourceLocator = new Uri("/Takt.Fluent;component/Controls/TaktTreeView.xaml", UriKind.Relative);
        System.Windows.Application.LoadComponent(this, resourceLocator);

        // 获取操作日志管理器
        _operLog = App.Services?.GetService<OperLogManager>();

        // 创建内部命令包装器
        CreateInternalCommands();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region 公开事件

    public event EventHandler<TaktTreeViewQueryContext>? QueryRequested;

    #endregion

    #region 依赖属性 - 数据绑定

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TaktTreeView),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TaktTreeView),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set
        {
            SetValue(SelectedItemProperty, value);
            // 同步到 TreeView（如果 TreeView 已加载）
            if (_treeView != null && value != null)
            {
                var container = _treeView.ItemContainerGenerator.ContainerFromItem(value) as TreeViewItem;
                if (container != null)
                {
                    container.IsSelected = true;
                }
            }
        }
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(TaktTreeView),
            new PropertyMetadata(null, OnItemTemplateChanged));

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ChildrenPathProperty =
        DependencyProperty.Register(nameof(ChildrenPath), typeof(string), typeof(TaktTreeView),
            new PropertyMetadata("Children", OnChildrenPathChanged));

    /// <summary>
    /// 子项集合的绑定路径（用于 HierarchicalDataTemplate）
    /// </summary>
    public string ChildrenPath
    {
        get => (string)GetValue(ChildrenPathProperty);
        set => SetValue(ChildrenPathProperty, value);
    }

    public static readonly DependencyProperty ItemContainerStyleProperty =
        DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public Style? ItemContainerStyle
    {
        get => (Style?)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    private static readonly DependencyPropertyKey SelectedItemsCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedItemsCount), typeof(int), typeof(TaktTreeView),
            new PropertyMetadata(0, OnSelectedItemsCountChanged));

    public static readonly DependencyProperty SelectedItemsCountProperty = SelectedItemsCountPropertyKey.DependencyProperty;

    public int SelectedItemsCount
    {
        get => (int)GetValue(SelectedItemsCountProperty);
        private set => SetValue(SelectedItemsCountPropertyKey, value);
    }

    #endregion

    #region 依赖属性 - 查询区域

    public static readonly DependencyProperty ShowQueryAreaProperty =
        DependencyProperty.Register(nameof(ShowQueryArea), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public bool ShowQueryArea
    {
        get => (bool)GetValue(ShowQueryAreaProperty);
        set => SetValue(ShowQueryAreaProperty, value);
    }

    public static readonly DependencyProperty QueryKeywordProperty =
        DependencyProperty.Register(nameof(QueryKeyword), typeof(string), typeof(TaktTreeView),
            new PropertyMetadata(string.Empty));

    public string QueryKeyword
    {
        get => (string)GetValue(QueryKeywordProperty);
        set => SetValue(QueryKeywordProperty, value);
    }

    public static readonly DependencyProperty QueryPlaceholderProperty =
        DependencyProperty.Register(nameof(QueryPlaceholder), typeof(string), typeof(TaktTreeView),
            new PropertyMetadata(Format("common.placeholder.keywordHint", "请输入{0}进行搜索", Translate("common.keyword", "关键字"))));

    public string QueryPlaceholder
    {
        get => (string)GetValue(QueryPlaceholderProperty);
        set => SetValue(QueryPlaceholderProperty, value);
    }

    public static readonly DependencyProperty QueryButtonTextProperty =
        DependencyProperty.Register(nameof(QueryButtonText), typeof(string), typeof(TaktTreeView),
            new PropertyMetadata(Translate("common.button.query", "查询")));

    public string QueryButtonText
    {
        get => (string)GetValue(QueryButtonTextProperty);
        set => SetValue(QueryButtonTextProperty, value);
    }

    public static readonly DependencyProperty ResetButtonTextProperty =
        DependencyProperty.Register(nameof(ResetButtonText), typeof(string), typeof(TaktTreeView),
            new PropertyMetadata(Translate("common.button.reset", "重置")));

    public string ResetButtonText
    {
        get => (string)GetValue(ResetButtonTextProperty);
        set => SetValue(ResetButtonTextProperty, value);
    }

    private static readonly DependencyPropertyKey IsEmptyPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        private set => SetValue(IsEmptyPropertyKey, value);
    }

    public static readonly DependencyProperty EmptyTextProperty =
        DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(TaktTreeView),
            new PropertyMetadata(Translate("common.noData", "暂无数据")));

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public static readonly DependencyProperty HeaderContentProperty =
        DependencyProperty.Register(nameof(HeaderContent), typeof(object), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    #endregion

    #region 依赖属性 - 工具栏

    public static readonly DependencyProperty ShowToolbarProperty =
        DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarCreateProperty =
        DependencyProperty.Register(nameof(ShowToolbarCreate), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public bool ShowToolbarCreate
    {
        get => (bool)GetValue(ShowToolbarCreateProperty);
        set => SetValue(ShowToolbarCreateProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarUpdateProperty =
        DependencyProperty.Register(nameof(ShowToolbarUpdate), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public bool ShowToolbarUpdate
    {
        get => (bool)GetValue(ShowToolbarUpdateProperty);
        set => SetValue(ShowToolbarUpdateProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarDeleteProperty =
        DependencyProperty.Register(nameof(ShowToolbarDelete), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public bool ShowToolbarDelete
    {
        get => (bool)GetValue(ShowToolbarDeleteProperty);
        set => SetValue(ShowToolbarDeleteProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarImportProperty =
        DependencyProperty.Register(nameof(ShowToolbarImport), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(false));

    public bool ShowToolbarImport
    {
        get => (bool)GetValue(ShowToolbarImportProperty);
        set => SetValue(ShowToolbarImportProperty, value);
    }

    public static readonly DependencyProperty ShowToolbarExportProperty =
        DependencyProperty.Register(nameof(ShowToolbarExport), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(false));

    public bool ShowToolbarExport
    {
        get => (bool)GetValue(ShowToolbarExportProperty);
        set => SetValue(ShowToolbarExportProperty, value);
    }

    public static readonly DependencyProperty ShowAdvancedQueryButtonProperty =
        DependencyProperty.Register(nameof(ShowAdvancedQueryButton), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(false));

    public bool ShowAdvancedQueryButton
    {
        get => (bool)GetValue(ShowAdvancedQueryButtonProperty);
        set => SetValue(ShowAdvancedQueryButtonProperty, value);
    }

    #endregion

    #region 依赖属性 - Toolbar 命令

    public static readonly DependencyProperty QueryCommandProperty =
        DependencyProperty.Register(nameof(QueryCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? QueryCommand
    {
        get => (ICommand?)GetValue(QueryCommandProperty);
        set => SetValue(QueryCommandProperty, value);
    }

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? ResetCommand
    {
        get => (ICommand?)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public static readonly DependencyProperty CreateCommandProperty =
        DependencyProperty.Register(nameof(CreateCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? CreateCommand
    {
        get => (ICommand?)GetValue(CreateCommandProperty);
        set => SetValue(CreateCommandProperty, value);
    }

    public static readonly DependencyProperty UpdateCommandProperty =
        DependencyProperty.Register(nameof(UpdateCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null, OnUpdateCommandChanged));

    public ICommand? UpdateCommand
    {
        get => (ICommand?)GetValue(UpdateCommandProperty);
        set => SetValue(UpdateCommandProperty, value);
    }

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null, OnDeleteCommandChanged));

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public static readonly DependencyProperty ImportCommandProperty =
        DependencyProperty.Register(nameof(ImportCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? ImportCommand
    {
        get => (ICommand?)GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }

    public static readonly DependencyProperty ExportCommandProperty =
        DependencyProperty.Register(nameof(ExportCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? ExportCommand
    {
        get => (ICommand?)GetValue(ExportCommandProperty);
        set => SetValue(ExportCommandProperty, value);
    }

    public static readonly DependencyProperty AdvancedQueryCommandProperty =
        DependencyProperty.Register(nameof(AdvancedQueryCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? AdvancedQueryCommand
    {
        get => (ICommand?)GetValue(AdvancedQueryCommandProperty);
        set => SetValue(AdvancedQueryCommandProperty, value);
    }

    #endregion

    #region 内部命令属性

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

    #region 依赖属性 - 行内操作按钮

    public static readonly DependencyProperty ShowRowCreateProperty =
        DependencyProperty.Register(nameof(ShowRowCreate), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(false));

    public bool ShowRowCreate
    {
        get => (bool)GetValue(ShowRowCreateProperty);
        set => SetValue(ShowRowCreateProperty, value);
    }

    public static readonly DependencyProperty RowCreateCommandProperty =
        DependencyProperty.Register(nameof(RowCreateCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? RowCreateCommand
    {
        get => (ICommand?)GetValue(RowCreateCommandProperty);
        set => SetValue(RowCreateCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowUpdateProperty =
        DependencyProperty.Register(nameof(ShowRowUpdate), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public bool ShowRowUpdate
    {
        get => (bool)GetValue(ShowRowUpdateProperty);
        set => SetValue(ShowRowUpdateProperty, value);
    }

    public static readonly DependencyProperty RowUpdateCommandProperty =
        DependencyProperty.Register(nameof(RowUpdateCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? RowUpdateCommand
    {
        get => (ICommand?)GetValue(RowUpdateCommandProperty);
        set => SetValue(RowUpdateCommandProperty, value);
    }

    public static readonly DependencyProperty ShowRowDeleteProperty =
        DependencyProperty.Register(nameof(ShowRowDelete), typeof(bool), typeof(TaktTreeView),
            new PropertyMetadata(true));

    public bool ShowRowDelete
    {
        get => (bool)GetValue(ShowRowDeleteProperty);
        set => SetValue(ShowRowDeleteProperty, value);
    }

    public static readonly DependencyProperty RowDeleteCommandProperty =
        DependencyProperty.Register(nameof(RowDeleteCommand), typeof(ICommand), typeof(TaktTreeView),
            new PropertyMetadata(null));

    public ICommand? RowDeleteCommand
    {
        get => (ICommand?)GetValue(RowDeleteCommandProperty);
        set => SetValue(RowDeleteCommandProperty, value);
    }

    #endregion

    #region 事件处理

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTreeView treeView)
        {
            treeView.AttachItemsSourceHandlers(e.NewValue);
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTreeView treeView)
        {
            treeView.UpdateSelectedItemsCount();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTreeView treeView)
        {
            treeView.UpdateItemTemplate();
        }
    }

    private static void OnChildrenPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTreeView treeView)
        {
            treeView.UpdateItemTemplate();
        }
    }

    private void UpdateItemTemplate()
    {
        if (ItemTemplate == null && _treeView != null)
        {
            // 创建默认的 ItemTemplate，包含操作列
            var defaultTemplate = CreateDefaultItemTemplate();
            if (defaultTemplate != null)
            {
                _treeView.ItemTemplate = defaultTemplate;
            }
        }
        else if (ItemTemplate != null && _treeView != null)
        {
            _treeView.ItemTemplate = ItemTemplate;
        }
    }

    private HierarchicalDataTemplate? CreateDefaultItemTemplate()
    {
        // 创建 Grid 作为容器
        var gridFactory = new FrameworkElementFactory(typeof(Grid));
        gridFactory.SetValue(Grid.MinHeightProperty, 32.0);
        gridFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(16, 2, 16, 2));

        // 注意：ColumnDefinitions 不能通过 FrameworkElementFactory 直接设置
        // 需要在 XAML 中定义，或者使用其他方式
        // 这里我们使用简单的布局，不设置列定义，而是使用 StackPanel

        // 创建水平 StackPanel 容器
        var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        stackPanelFactory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

        // 创建内容显示区域（左侧，自动扩展）
        var contentFactory = new FrameworkElementFactory(typeof(TextBlock));
        contentFactory.SetBinding(TextBlock.TextProperty, new Binding(".") { StringFormat = "{0}" });
        contentFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        contentFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 8, 0));
        contentFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        stackPanelFactory.AppendChild(contentFactory);

        // 创建操作列（右侧）
        var operationFactory = CreateOperationColumnFactory();
        stackPanelFactory.AppendChild(operationFactory);

        gridFactory.AppendChild(stackPanelFactory);

        // 创建 HierarchicalDataTemplate
        var template = new HierarchicalDataTemplate
        {
            ItemsSource = new Binding(ChildrenPath),
            VisualTree = gridFactory
        };

        return template;
    }

    private FrameworkElementFactory CreateOperationColumnFactory()
    {
        var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        stackPanelFactory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        stackPanelFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 0, 0));

        // 新增按钮
        if (ShowRowCreate)
        {
            var createButton = CreateOperationButton(
                "RowCreateCommand",
                PackIconKind.PlaylistPlus,
                "DefaultIconPlainPrimarySmall",
                "common.button.create");
            stackPanelFactory.AppendChild(createButton);
        }

        // 更新按钮
        if (ShowRowUpdate)
        {
            var updateButton = CreateOperationButton(
                "RowUpdateCommand",
                PackIconKind.Pencil,
                "DefaultIconPlainSuccessSmall",
                "common.button.update");
            stackPanelFactory.AppendChild(updateButton);
        }

        // 删除按钮
        if (ShowRowDelete)
        {
            var deleteButton = CreateOperationButton(
                "RowDeleteCommand",
                PackIconKind.Delete,
                "DefaultIconPlainWarningSmall",
                "common.button.delete");
            stackPanelFactory.AppendChild(deleteButton);
        }

        return stackPanelFactory;
    }

    private FrameworkElementFactory CreateOperationButton(string commandPropertyName, PackIconKind iconKind, string styleKey, string tooltipKey)
    {
        var buttonFactory = new FrameworkElementFactory(typeof(Button));
        buttonFactory.SetResourceReference(FrameworkElement.StyleProperty, styleKey);
        buttonFactory.SetValue(FrameworkElement.WidthProperty, 24.0);
        buttonFactory.SetValue(FrameworkElement.HeightProperty, 24.0);
        buttonFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(2, 0, 2, 0));

        // 绑定命令
        var commandBinding = new Binding(commandPropertyName)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TaktTreeView), 1)
        };
        buttonFactory.SetBinding(Button.CommandProperty, commandBinding);

        // 绑定命令参数（当前项）
        var parameterBinding = new Binding(".");
        buttonFactory.SetBinding(Button.CommandParameterProperty, parameterBinding);

        // 设置 ToolTip
        var tooltipText = Translate(tooltipKey, tooltipKey);
        buttonFactory.SetValue(Button.ToolTipProperty, tooltipText);

        // 创建图标
        var iconFactory = new FrameworkElementFactory(typeof(PackIcon));
        iconFactory.SetValue(PackIcon.KindProperty, iconKind);
        iconFactory.SetValue(FrameworkElement.WidthProperty, 16.0);
        iconFactory.SetValue(FrameworkElement.HeightProperty, 16.0);
        buttonFactory.AppendChild(iconFactory);

        return buttonFactory;
    }

    private static void OnSelectedItemsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTreeView treeView)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private static void OnUpdateCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTreeView treeView)
        {
            treeView.CreateInternalCommands();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private static void OnDeleteCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaktTreeView treeView)
        {
            treeView.CreateInternalCommands();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    #endregion

    #region 内部命令创建

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
                            _operLog?.Information("[TaktTreeView] 工具栏编辑按钮点击: SelectedItem={SelectedItem}", SelectedItem?.ToString() ?? "null");
                            targetCommand.Execute(commandParameter);
                            _operLog?.Information("[TaktTreeView] 编辑命令执行成功");
                        }
                        else
                        {
                            _operLog?.Information("[TaktTreeView] 编辑命令不可执行");
                        }
                    }
                    else
                    {
                        _operLog?.Information("[TaktTreeView] 编辑按钮点击: 未选中项或选中项数量不正确 (SelectedItemsCount={SelectedItemsCount})", SelectedItemsCount);
                    }
                }
                catch (Exception ex)
                {
                    _operLog?.Error(ex, "[TaktTreeView] 编辑操作失败");
                }
            },
            parameter =>
            {
                // 单选更新：必须有选中项且只能选中一行
                // 默认禁用，只有 SelectedItem != null && SelectedItemsCount == 1 时才激活
                return SelectedItem != null && SelectedItemsCount == 1;
            });

        // 删除命令：单选时激活（SelectedItemsCount > 0）
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
                        var commandParameter = SelectedItem ?? parameter;
                        if (targetCommand.CanExecute(commandParameter))
                        {
                            _operLog?.Information("[TaktTreeView] 工具栏删除按钮点击: SelectedItem={SelectedItem}", SelectedItem?.ToString() ?? "null");
                            targetCommand.Execute(commandParameter);
                            _operLog?.Information("[TaktTreeView] 删除命令执行成功");
                        }
                        else
                        {
                            _operLog?.Information("[TaktTreeView] 删除命令不可执行");
                        }
                    }
                    else
                    {
                        _operLog?.Information("[TaktTreeView] 删除按钮点击: 未选中项 (SelectedItemsCount={SelectedItemsCount})", SelectedItemsCount);
                    }
                }
                catch (Exception ex)
                {
                    _operLog?.Error(ex, "[TaktTreeView] 删除操作失败");
                }
            },
            parameter =>
            {
                // 单选删除：必须有选中项（SelectedItemsCount > 0）
                // 默认禁用，只有 SelectedItemsCount > 0 时才激活
                return SelectedItemsCount > 0;
            });

        // 将内部命令设置为依赖属性，以便 XAML 绑定能正确工作
        InternalUpdateCommand = _internalUpdateCommand;
        InternalDeleteCommand = _internalDeleteCommand;
    }

    #endregion

    #region 数据源处理

    private void AttachItemsSourceHandlers(object? source)
    {
        DetachItemsSourceHandlers();

        if (source is INotifyCollectionChanged notifier)
        {
            _itemsSourceNotifier = notifier;
            _itemsSourceNotifier.CollectionChanged += ItemsSource_CollectionChanged;
        }

        UpdateEmptyState();
        UpdateSelectedItemsCount();
        
        // 延迟更新展开状态，等待 UI 渲染完成
        if (IsLoaded)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateExpandState()), System.Windows.Threading.DispatcherPriority.Loaded);
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
        UpdateSelectedItemsCount();
        
        // 延迟更新展开状态，等待 UI 渲染完成
        if (IsLoaded)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateExpandState()), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void UpdateEmptyState()
    {
        IsEmpty = GetItemsSourceCount() <= 0;
    }

    private void UpdateSelectedItemsCount()
    {
        SelectedItemsCount = SelectedItem != null ? 1 : 0;
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

        if (ItemsSource is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Count();
        }

        return 0;
    }

    #endregion

    #region UI 事件处理

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _treeView = this.FindName("PART_TreeView") as TreeView;
        UpdateEmptyState();
        UpdateSelectedItemsCount();
        UpdateExpandState();
        UpdateItemTemplate();

        if (_internalUpdateCommand == null || _internalDeleteCommand == null)
        {
            CreateInternalCommands();
        }
        CommandManager.InvalidateRequerySuggested();

        // 监听 TreeView 的展开/收缩事件
        if (_treeView != null)
        {
            _treeView.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnTreeViewItemExpanded));
            _treeView.AddHandler(TreeViewItem.CollapsedEvent, new RoutedEventHandler(OnTreeViewItemCollapsed));
            
            // 同步 SelectedItem 到 TreeView（如果外部设置了 SelectedItem）
            if (SelectedItem != null)
            {
                var container = _treeView.ItemContainerGenerator.ContainerFromItem(SelectedItem) as TreeViewItem;
                if (container != null)
                {
                    container.IsSelected = true;
                }
            }
        }
    }

    private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
    {
        UpdateExpandState();
    }

    private void OnTreeViewItemCollapsed(object sender, RoutedEventArgs e)
    {
        UpdateExpandState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DetachItemsSourceHandlers();

        // 移除事件监听
        if (_treeView != null)
        {
            _treeView.RemoveHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnTreeViewItemExpanded));
            _treeView.RemoveHandler(TreeViewItem.CollapsedEvent, new RoutedEventHandler(OnTreeViewItemCollapsed));
        }
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // 更新 SelectedItem 依赖属性
        SelectedItem = e.NewValue;
        UpdateSelectedItemsCount();
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region 查询区域事件处理

    private void OnQueryButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _operLog?.Information("[TaktTreeView] 查询按钮点击: Keyword={Keyword}", QueryKeyword);
            var context = new TaktTreeViewQueryContext(QueryKeyword, this);
            QueryRequested?.Invoke(this, context);

            if (QueryCommand != null && QueryCommand.CanExecute(context))
            {
                QueryCommand.Execute(context);
                _operLog?.Information("[TaktTreeView] 查询命令执行成功");
            }
            else
            {
                _operLog?.Information("[TaktTreeView] 查询命令不可执行或未设置");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktTreeView] 查询操作失败");
        }
    }

    private void OnResetButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _operLog?.Information("[TaktTreeView] 重置按钮点击: 开始重置操作");
            QueryKeyword = string.Empty;
            var context = new TaktTreeViewQueryContext(string.Empty, this);
            QueryRequested?.Invoke(this, context);

            if (ResetCommand != null && ResetCommand.CanExecute(context))
            {
                ResetCommand.Execute(context);
                _operLog?.Information("[TaktTreeView] 重置命令执行成功");
            }
            else
            {
                _operLog?.Information("[TaktTreeView] 重置命令不可执行或未设置");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktTreeView] 重置操作失败");
        }
    }

    private void OnToggleQueryAreaClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var newValue = !ShowQueryArea;
            _operLog?.Information("[TaktTreeView] 切换查询区域: {OldValue} -> {NewValue}", ShowQueryArea, newValue);
            ShowQueryArea = newValue;
            _operLog?.Information("[TaktTreeView] 切换查询区域成功");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktTreeView] 切换查询区域失败");
        }
    }

    #endregion

    #region 展开/收缩功能

    private void OnToggleExpandCollapseClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_treeView == null)
            {
                _operLog?.Information("[TaktTreeView] 展开/收缩按钮点击: TreeView 未初始化");
                return;
            }

            if (IsAllExpanded)
            {
                _operLog?.Information("[TaktTreeView] 展开/收缩按钮点击: 开始收缩所有节点");
                CollapseAllItems(_treeView.Items);
                _operLog?.Information("[TaktTreeView] 收缩所有节点成功");
            }
            else
            {
                _operLog?.Information("[TaktTreeView] 展开/收缩按钮点击: 开始展开所有节点");
                ExpandAllItems(_treeView.Items);
                _operLog?.Information("[TaktTreeView] 展开所有节点成功");
            }

            UpdateExpandState();
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[TaktTreeView] 展开/收缩操作失败");
        }
    }

    private void OnExpandAllClick(object sender, RoutedEventArgs e)
    {
        if (_treeView == null)
        {
            return;
        }

        ExpandAllItems(_treeView.Items);
        UpdateExpandState();
    }

    private void OnCollapseAllClick(object sender, RoutedEventArgs e)
    {
        if (_treeView == null)
        {
            return;
        }

        CollapseAllItems(_treeView.Items);
        UpdateExpandState();
    }

    /// <summary>
    /// 更新展开状态
    /// </summary>
    private void UpdateExpandState()
    {
        if (_treeView == null)
        {
            IsAllExpanded = false;
            return;
        }

        IsAllExpanded = CheckAllExpanded(_treeView.Items);
    }

    /// <summary>
    /// 检查所有项是否已展开
    /// </summary>
    private bool CheckAllExpanded(ItemCollection items)
    {
        if (items.Count == 0)
        {
            return true; // 空集合视为全部展开
        }

        foreach (var item in items)
        {
            TreeViewItem? container = null;

            if (item is TreeViewItem treeViewItem)
            {
                container = treeViewItem;
            }
            else
            {
                container = _treeView?.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            }

            if (container != null)
            {
                // 如果当前项有子项但未展开，返回 false
                if (container.Items.Count > 0 && !container.IsExpanded)
                {
                    return false;
                }

                // 递归检查子项
                if (container.Items.Count > 0 && !CheckAllExpanded(container.Items))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void ExpandAllItems(ItemCollection items)
    {
        foreach (var item in items)
        {
            if (item is TreeViewItem treeViewItem)
            {
                treeViewItem.IsExpanded = true;
                if (treeViewItem.Items.Count > 0)
                {
                    ExpandAllItems(treeViewItem.Items);
                }
            }
            else
            {
                // 如果 ItemsSource 中的项不是 TreeViewItem，需要找到对应的容器
                var container = _treeView?.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container != null)
                {
                    container.IsExpanded = true;
                    if (container.Items.Count > 0)
                    {
                        ExpandAllItems(container.Items);
                    }
                }
            }
        }
    }

    private void CollapseAllItems(ItemCollection items)
    {
        foreach (var item in items)
        {
            if (item is TreeViewItem treeViewItem)
            {
                treeViewItem.IsExpanded = false;
                if (treeViewItem.Items.Count > 0)
                {
                    CollapseAllItems(treeViewItem.Items);
                }
            }
            else
            {
                // 如果 ItemsSource 中的项不是 TreeViewItem，需要找到对应的容器
                var container = _treeView?.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container != null)
                {
                    container.IsExpanded = false;
                    if (container.Items.Count > 0)
                    {
                        CollapseAllItems(container.Items);
                    }
                }
            }
        }
    }

    #endregion

    #region 辅助方法

    private static string Translate(string key, string fallback)
    {
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
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

    #endregion
}

