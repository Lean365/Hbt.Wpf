using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock;
using FontAwesome.Sharp;
using Hbt.Application.Dtos.Identity;
using Hbt.Common.Enums;
using Hbt.Common.Logging;
using Hbt.Fluent.Models;
using Hbt.Fluent.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Hbt.Fluent.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
        Loaded += MainWindow_Loaded;

        UpdateMainWindowVisuals();
        
        StateChanged += (s, e) => UpdateMainWindowVisuals();
        Activated += (s, e) => UpdateMainWindowVisuals();
        Deactivated += (s, e) => UpdateMainWindowVisuals();
        
        // 订阅菜单加载完成事件，默认打开仪表盘
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(ViewModel.Menus) && ViewModel?.Menus != null && ViewModel.Menus.Any() && IsLoaded)
            {
                // 菜单加载完成且窗口已加载，打开仪表盘
                // 延迟执行，确保所有控件都完全初始化
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        OpenDefaultDashboard();
                    }
                    catch (Exception ex)
                    {
                        var operLog = App.Services?.GetService<OperLogManager>();
                        operLog?.Error(ex, "[导航] PropertyChanged 中打开默认仪表盘时发生异常");
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[导航] PropertyChanged 事件处理时发生异常");
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // 刷新用户信息显示
            if (ViewModel != null)
            {
                ViewModel.LoadCurrentUserInfo();
                
                // 确保 DocumentClosed 事件已正确订阅（双重保险）
                if (DockingManager != null)
                {
                    // 先移除可能已存在的事件处理程序（避免重复订阅）
                    DockingManager.DocumentClosed -= DockingManager_DocumentClosed;
                    // 重新订阅事件
                    DockingManager.DocumentClosed += DockingManager_DocumentClosed;
                    var operLog = App.Services?.GetService<OperLogManager>();
                    operLog?.Debug("[MainWindow] DocumentClosed 事件已订阅");
                    
                    // 验证图标绑定：延迟检查，确保 UI 已完全渲染
                    // 使用更低优先级，确保在文档完全渲染后检查
                    Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            // 延迟更长时间，确保 LayoutDocumentItem 已创建
                            System.Threading.Thread.Sleep(100);
                            
                            // 查找所有 LayoutDocumentItem，验证 IconSource 绑定
                            var docItems = FindVisualChildren<AvalonDock.Controls.LayoutDocumentItem>(DockingManager);
                            operLog?.Debug("[MainWindow] 找到 {Count} 个 LayoutDocumentItem", docItems.Count());
                            
                            foreach (var docItem in docItems)
                            {
                                var docItemIconSource = docItem.IconSource;
                                var docItemTitle = docItem.Title;
                                
                                // Model 是 object 类型，需要转换为 LayoutDocument 才能访问 IconSource
                                System.Windows.Media.ImageSource? modelIconSource = null;
                                if (docItem.Model is AvalonDock.Layout.LayoutDocument layoutDoc)
                                {
                                    modelIconSource = layoutDoc.IconSource;
                                }
                                
                                operLog?.Debug("[MainWindow] LayoutDocumentItem 验证: DocItem.Title={Title}, DocItem.IconSource={DocItemIconSource}, Model.IconSource={ModelIconSource}", 
                                    docItemTitle ?? "null",
                                    docItemIconSource != null ? $"{docItemIconSource.GetType().Name}({docItemIconSource.Width}x{docItemIconSource.Height})" : "null",
                                    modelIconSource != null ? $"{modelIconSource.GetType().Name}({modelIconSource.Width}x{modelIconSource.Height})" : "null");
                            }
                        }
                        catch (Exception ex)
                        {
                            operLog?.Error(ex, "[MainWindow] 验证图标绑定时发生异常");
                        }
                    }, System.Windows.Threading.DispatcherPriority.ContextIdle);
                }
                
                // 窗口加载完成后，如果菜单已经加载，打开仪表盘
                if (ViewModel.Menus != null && ViewModel.Menus.Any())
                {
                    // 延迟执行，确保 TabControl 完全初始化
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            OpenDefaultDashboard();
                        }
                        catch (Exception ex)
                        {
                            var operLog = App.Services?.GetService<OperLogManager>();
                            operLog?.Error(ex, "[导航] MainWindow_Loaded 中打开默认仪表盘时发生异常");
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[导航] MainWindow_Loaded 事件处理时发生异常");
        }
    }

    private void OpenDefaultDashboard()
    {
        try
        {
            // 确保窗口已完全加载
            if (!IsLoaded)
            {
                // 如果窗口未加载，延迟执行
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    OpenDefaultDashboard();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
                return;
            }

            // 检查 ViewModel 和 Menus 是否可用
            if (ViewModel == null || ViewModel.Menus == null || !ViewModel.Menus.Any())
            {
                return;
            }

            // 查找仪表盘菜单（第一个 MenuTypeEnum.Menu 类型的菜单）
            var dashboardMenu = FindFirstMenu(ViewModel.Menus);
            if (dashboardMenu == null)
            {
                return;
            }

            // 延迟导航，确保 TabControl 完全初始化后再添加标签页
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 再次检查 ViewModel 是否可用
                    if (ViewModel == null)
                    {
                        return;
                    }

                    // 导航到仪表盘
                    NavigateToView(dashboardMenu);
                    
                    // 延迟选中仪表盘菜单项，等待 TreeView 渲染完成
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (MenusList != null && MenusList.Items != null && MenusList.Items.Count > 0)
                            {
                                SelectMenuItem(dashboardMenu, MenusList.Items.Cast<MenuDto>().ToList());
                            }
                        }
                        catch (Exception ex)
                        {
                            var operLog = App.Services?.GetService<OperLogManager>();
                            operLog?.Error(ex, "[导航] 选中菜单项时发生异常");
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                catch (Exception ex)
                {
                    var operLog = App.Services?.GetService<OperLogManager>();
                    operLog?.Error(ex, "[导航] 打开默认仪表盘时发生异常");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[导航] OpenDefaultDashboard 方法执行时发生异常");
        }
    }

    /// <summary>
    /// 递归查找第一个菜单类型的菜单（通常是仪表盘）
    /// </summary>
    private MenuDto? FindFirstMenu(List<MenuDto> menus)
    {
        foreach (var menu in menus)
        {
            if (menu.MenuType == MenuTypeEnum.Menu)
            {
                return menu;
            }
            if (menu.Children != null && menu.Children.Any())
            {
                var found = FindFirstMenu(menu.Children);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 递归选中指定的菜单项并展开父级
    /// </summary>
    private void SelectMenuItem(MenuDto targetMenu, List<MenuDto> menus, TreeViewItem? parentItem = null)
    {
        foreach (var menu in menus)
        {
            if (menu.Id == targetMenu.Id)
            {
                // 找到目标菜单，选中它
                var container = MenusList.ItemContainerGenerator.ContainerFromItem(menu) as TreeViewItem;
                if (container != null)
                {
                    container.IsSelected = true;
                    container.BringIntoView();
                }
                return;
            }
            if (menu.Children != null && menu.Children.Any())
            {
                // 展开父级目录
                var container = MenusList.ItemContainerGenerator.ContainerFromItem(menu) as TreeViewItem;
                if (container != null)
                {
                    container.IsExpanded = true;
                }
                SelectMenuItem(targetMenu, menu.Children, container);
            }
        }
    }

    private void UpdateMainWindowVisuals()
    {
        MainGrid.Margin = default;
        if (WindowState == WindowState.Maximized)
        {
            MainGrid.Margin = new Thickness(8);
        }
    }

    private void MinimizeWindow(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
            MaximizeIcon.Text = "\uE922";
        }
        else
        {
            this.WindowState = WindowState.Maximized;
            MaximizeIcon.Text = "\uE923";
        }
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // 双击标题栏切换最大化/还原
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeIcon.Text = "\uE922";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeIcon.Text = "\uE923";
            }
        }
        else
        {
            // 拖动窗口
            this.DragMove();
        }
    }

    private void MenusList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var treeView = sender as TreeView;
            if (treeView?.SelectedItem is MenuDto menuItem)
            {
                var tvi = MenusList.ItemContainerGenerator.ContainerFromItem(treeView.SelectedItem) as TreeViewItem;
                if (tvi == null)
                {
                    // 如果无法直接获取，尝试递归查找
                    tvi = FindTreeViewItemByItem(MenusList, treeView.SelectedItem);
                }
                SelectedItemChanged(tvi, menuItem);
            }
        }
    }
    
    /// <summary>
    /// 递归查找包含指定项的 TreeViewItem
    /// </summary>
    private TreeViewItem? FindTreeViewItemByItem(ItemsControl itemsControl, object item)
    {
        // 先尝试在当前层级查找
        if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
        {
            return tvi;
        }
        
        // 如果当前层级找不到，递归查找子项
        foreach (object childItem in itemsControl.Items)
        {
            if (childItem == item)
            {
                // 找到了，但可能容器还未生成，尝试强制生成
                itemsControl.UpdateLayout();
                return itemsControl.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
            }
            
            var childContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
            if (childContainer != null)
            {
                // 递归查找子容器
                var found = FindTreeViewItemByItem(childContainer, item);
                if (found != null)
                {
                    return found;
                }
            }
        }
        
        return null;
    }

    private void MenusList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // 如果点击的是 ToggleButton（展开/折叠按钮），让 TreeView 的默认行为处理
        if (e.OriginalSource is ToggleButton)
        {
            return;
        }
        
        // 获取点击的 TreeViewItem
        var treeView = sender as TreeView;
        if (treeView == null)
        {
            return;
        }
        
        // 查找点击位置对应的 TreeViewItem
        var treeViewItem = FindParentTreeViewItem(e.OriginalSource as DependencyObject);
        if (treeViewItem != null && treeViewItem.DataContext is MenuDto menuItem)
        {
            SelectedItemChanged(treeViewItem, menuItem);
        }
    }
    
    /// <summary>
    /// 向上查找 TreeViewItem 父元素
    /// </summary>
    private TreeViewItem? FindParentTreeViewItem(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is TreeViewItem treeViewItem)
            {
                return treeViewItem;
            }
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    private void MenusList_Loaded(object sender, RoutedEventArgs e)
    {
        // 不再自动选中第一项，因为第一项可能是目录
        // 默认打开仪表盘的逻辑在 MainWindow_Loaded 中处理
    }

    private void SelectedItemChanged(TreeViewItem? tvi, MenuDto? menuItem = null)
    {
        if (tvi == null)
        {
            return;
        }
        
        // 如果未传入 menuItem，从 DataContext 获取
        if (menuItem == null)
        {
            menuItem = tvi.DataContext as MenuDto;
        }
        
        if (menuItem == null)
        {
            return;
        }
        
        // 如果是目录类型，展开/折叠，并导航到快速导航页面
        if (menuItem.MenuType == MenuTypeEnum.Directory)
        {
            // 切换展开/折叠状态
            tvi.IsExpanded = !tvi.IsExpanded;
            // 导航到快速导航页面（如果routePath存在）
            if (!string.IsNullOrEmpty(menuItem.RoutePath))
            {
                NavigateToView(menuItem);
            }
        }
        else if (menuItem.MenuType == MenuTypeEnum.Menu)
        {
            // 菜单类型触发导航
            HandleSelectedMenu();
        }
    }

    /// <summary>
    /// 菜单树选中项变更事件处理
    /// 按照推荐的 MDI 实现：菜单树选择直接触发 TabItem 的添加或激活
    /// </summary>
    private void MenusList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // 按照推荐的 MDI 实现：如果选中的是菜单项，直接打开或激活对应的标签页
        if (e.NewValue is MenuDto menuItem && !string.IsNullOrEmpty(menuItem.MenuCode))
        {
            if (menuItem.MenuType == MenuTypeEnum.Menu)
            {
                // 菜单类型：打开或激活标签页（按照推荐的 MDI 实现）
                HandleSelectedMenu();
            }
            else if (menuItem.MenuType == MenuTypeEnum.Directory && !string.IsNullOrEmpty(menuItem.RoutePath))
            {
                // 目录类型：如果配置了路由路径，也可以导航
                NavigateToView(menuItem);
            }
        }
    }

    private void HandleSelectedMenu()
    {
        if (MenusList.SelectedItem is MenuDto menuItem)
        {
            ViewModel.SelectedMenu = menuItem;
            
            // 将选中项滚动到可视区域
            var tvi = MenusList.ItemContainerGenerator.ContainerFromItem(menuItem) as TreeViewItem;
            if (tvi != null)
            {
                tvi.BringIntoView();
            }

            // 只有菜单类型（MenuTypeEnum.Menu）才应该触发导航
            // 目录类型（MenuTypeEnum.Directory）只用于展开/折叠，不应该导航
            if (menuItem.MenuType == MenuTypeEnum.Menu && !string.IsNullOrEmpty(menuItem.RoutePath))
            {
                var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Debug("[导航] HandleSelectedMenu 触发导航：菜单={MenuCode} ({MenuName})", 
                    menuItem.MenuCode, menuItem.MenuName);
                NavigateToView(menuItem);
            }
        }
    }

    /// <summary>
    /// 导航到指定的菜单（公共方法，供导航页面调用）
    /// </summary>
    public void NavigateToMenu(MenuDto menuItem)
    {
        NavigateToView(menuItem);
        
        // 更新选中项
        if (menuItem.MenuType == MenuTypeEnum.Menu)
        {
            ViewModel.SelectedMenu = menuItem;
            // 尝试选中菜单树中的对应项
            SelectMenuItem(menuItem, ViewModel.Menus);
        }
    }

    /// <summary>
    /// 导航到指定的视图（按照推荐的 MDI 实现）
    /// 类似于推荐方案中的 OpenOrActivateTab 方法
    /// </summary>
    private void NavigateToView(MenuDto menuItem)
    {
        if (menuItem == null)
        {
            return;
        }

        var operLog = App.Services?.GetService<OperLogManager>();
        operLog?.Debug("[导航] NavigateToView 开始：菜单={MenuCode} ({MenuName}), MenuType={MenuType}, Component={Component}, RoutePath={RoutePath}, I18nKey={I18nKey}, Icon={Icon}", 
            menuItem.MenuCode, menuItem.MenuName, menuItem.MenuType, 
            menuItem.Component ?? "null", menuItem.RoutePath ?? "null", 
            menuItem.I18nKey ?? "null", menuItem.Icon ?? "null");
        
        // 优先使用 Component，如果没有则使用 RoutePath（类似于推荐方案中的 ViewType）
        string? typeName = null;
        
        if (!string.IsNullOrEmpty(menuItem.Component))
        {
            // Component 应该已经是完整的类型名称，如 "Hbt.Fluent.Views.Identity.UserView"
            typeName = menuItem.Component;
            operLog?.Debug("[导航] 使用 Component 作为类型名称：{TypeName}", typeName);
        }
        else if (!string.IsNullOrEmpty(menuItem.RoutePath))
        {
            // 约定：RoutePath 如 "Views/Identity/UserView"，转换为 "Hbt.Fluent.Views.Identity.UserView"
            typeName = $"Hbt.Fluent.{menuItem.RoutePath.Replace('/', '.')}";
            operLog?.Debug("[导航] 使用 RoutePath 生成类型名称：RoutePath={RoutePath}, TypeName={TypeName}", 
                menuItem.RoutePath, typeName);
        }
        else
        {
            operLog?.Warning("[导航] 菜单 RoutePath 和 Component 都为空，无法导航：{MenuCode} ({MenuName})",
                menuItem.MenuCode, menuItem.MenuName);
            return;
        }

        if (string.IsNullOrEmpty(typeName))
        {
            operLog?.Warning("[导航] 类型名称为空，无法导航：{MenuCode}", menuItem.MenuCode);
            return;
        }

        // 动态创建视图类型（类似于推荐方案中的 CreateViewContent）
        Type? viewType = null;
        try
        {
            viewType = Type.GetType(typeName);
            if (viewType == null)
            {
                // 尝试在当前程序集内查找
                viewType = typeof(MainWindow).Assembly.GetType(typeName);
            }
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[导航] 查找视图类型时发生异常：{TypeName}", typeName);
            return;
        }

        if (viewType == null)
        {
            operLog?.Error("[导航] 找不到视图类型：{TypeName} (菜单: {MenuCode}, RoutePath: {RoutePath}, Component: {Component})",
                typeName, menuItem.MenuCode, menuItem.RoutePath ?? "", menuItem.Component ?? "");
            return;
        }

        try
        {
            // 检查 App.Services 是否可用
            if (App.Services == null)
            {
                operLog?.Error("[导航] App.Services 为 null，无法创建视图实例：{TypeName}", typeName);
                return;
            }
            
            // 创建视图实例（类似于推荐方案：CreateViewContent(menuItem.ViewType)）
            var instance = ActivatorUtilities.CreateInstance(App.Services, viewType);
            if (instance == null)
            {
                operLog?.Error("[导航] 创建视图实例失败，返回 null：{TypeName}", typeName);
                return;
            }
            
            // 验证实例类型（必须是 UIElement 才能正确显示在 AvalonDock 中）
            if (instance is not System.Windows.UIElement uiElement)
            {
                operLog?.Error("[导航] 创建的视图实例不是 UIElement：{TypeName}, InstanceType={InstanceType}", 
                    typeName, instance.GetType().FullName);
                return;
            }
            
            operLog?.Debug("[导航] 视图实例创建成功：{TypeName}, InstanceType={InstanceType}, IsUIElement=True", 
                typeName, instance.GetType().FullName);
            
            // 检查 ViewModel 是否可用
            if (ViewModel == null)
            {
                operLog?.Error("[导航] ViewModel 为 null，无法添加文档：{TypeName}", typeName);
                return;
            }
            
            // 检查 ViewModel.Documents 是否已初始化（AvalonDock 使用 Documents 集合）
            if (ViewModel.Documents == null)
            {
                operLog?.Error("[导航] ViewModel.Documents 为 null，无法添加文档：{TypeName}", typeName);
                return;
            }
            
            // 使用 AvalonDock MDI 模式：添加或激活文档
            Models.AvalonDocument? document = null;
            try
            {
                document = ViewModel.AddOrActivateDocument(menuItem, instance, typeName);
            }
            catch (ArgumentNullException ex)
            {
                operLog?.Error(ex, "[导航] 添加文档时参数为空：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
                return;
            }
            catch (InvalidOperationException ex)
            {
                operLog?.Error(ex, "[导航] 添加文档时操作无效：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
                return;
            }
            
            if (document != null)
            {
                // 验证文档已成功添加到集合中
                if (ViewModel.Documents.Contains(document))
                {
                    // 获取图标信息（如果有）
                    string iconInfo = !string.IsNullOrEmpty(menuItem.Icon) ? $"，图标：{menuItem.Icon}" : "";
                    operLog?.Information("[导航] 成功导航到视图：{TypeName} (菜单: {MenuCode})，文档：{DocTitle}{IconInfo}", 
                        typeName, menuItem.MenuCode, document.Title, iconInfo);
                }
                else
                {
                    operLog?.Warning("[导航] 文档创建成功但未添加到集合：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
                }
            }
            else
            {
                operLog?.Warning("[导航] 添加文档返回 null：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
            }
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[导航] 导航到视图失败：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
        }
    }

    /// <summary>
    /// IconBlock 卸载事件，保护 IconFont 属性
    /// </summary>
    private void IconBlock_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FontAwesome.Sharp.IconBlock iconBlock)
        {
            try
            {
                // 检查控件是否还在可视化树中且有效
                if (iconBlock.IsLoaded && iconBlock.Parent != null)
                {
                    // 尝试读取当前的 IconFont 值，如果已经是有效的，就不需要设置
                    try
                    {
                        var currentFont = iconBlock.IconFont;
                        // 如果是有效的枚举值，就不需要重新设置
                        if (currentFont == FontAwesome.Sharp.IconFont.Solid || 
                            currentFont == FontAwesome.Sharp.IconFont.Regular || 
                            currentFont == FontAwesome.Sharp.IconFont.Brands)
                        {
                            return; // 已经是有效值，不需要设置
                        }
                    }
                    catch
                    {
                        // 如果读取失败，说明 IconFont 可能已经是无效值，需要设置
                    }
                    
                    // 在卸载时强制设置 IconFont，防止变成空字符串
                    iconBlock.IconFont = FontAwesome.Sharp.IconFont.Solid;
                }
            }
            catch (ArgumentException)
            {
                // 如果 IconFont 已经无效（可能是空字符串），静默忽略
                // 这是因为控件可能已经在被清理的过程中，无法设置属性
                // 这种情况是可以接受的，不需要记录错误日志
            }
            catch (Exception ex)
            {
                // 其他类型的异常才记录日志，ArgumentException 是预期的，不需要记录
                var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Error(ex, "[Tab] IconBlock 卸载时保护 IconFont 失败");
            }
        }
    }

    /// <summary>
    /// AvalonDock 文档即将关闭事件处理（在关闭前，属性还未清空）
    /// 根据官方文档，应该在此事件中处理文档关闭逻辑
    /// </summary>
    private void DockingManager_DocumentClosing(object? sender, DocumentClosingEventArgs e)
    {
        var operLog = App.Services?.GetService<OperLogManager>();
        try
        {
            operLog?.Debug("[AvalonDock] DocumentClosing 事件触发：DocumentType={DocumentType}, Title={Title}, ContentId={ContentId}, ContentType={ContentType}", 
                e.Document?.GetType().FullName ?? "null", 
                e.Document?.Title ?? "null",
                e.Document?.ContentId ?? "null",
                e.Document?.Content?.GetType().FullName ?? "null");
            
            // 根据官方文档，e.Document 应该是 LayoutDocument，我们的 AvalonDocument 继承自它
            // 但日志显示 Title 和 ContentId 可能为 null，需要通过 Content 查找
            Models.AvalonDocument? avalonDoc = null;
            
            // 方法1：直接类型转换
            if (e.Document is Models.AvalonDocument directDoc)
            {
                avalonDoc = directDoc;
            }
            // 方法2：通过 Content 查找（根据日志，Content 可能是我们的 AvalonDocument）
            else if (e.Document?.Content is Models.AvalonDocument contentDoc)
            {
                avalonDoc = contentDoc;
            }
            
            if (avalonDoc != null)
            {
                operLog?.Debug("[AvalonDock] DocumentClosing: 文档类型匹配：AvalonDocument, Title={Title}, ViewTypeName={ViewTypeName}, CanClose={CanClose}", 
                    avalonDoc.Title, avalonDoc.ViewTypeName, avalonDoc.CanClose);
                
                // 检查是否可以关闭
                if (!avalonDoc.CanClose)
                {
                    operLog?.Debug("[AvalonDock] DocumentClosing: 文档不允许关闭，取消关闭操作：Title={Title}, ViewTypeName={ViewTypeName}", 
                        avalonDoc.Title, avalonDoc.ViewTypeName);
                    
                    // 取消关闭操作
                    e.Cancel = true;
                    
                    // 显示提示消息
                    System.Windows.MessageBox.Show(
                        "默认仪表盘标签页不允许关闭。",
                        "提示",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                    
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[AvalonDock] DocumentClosing 事件处理时发生异常");
        }
    }

    /// <summary>
    /// AvalonDock 文档关闭事件处理（在关闭后，属性可能已清空）
    /// </summary>
    private void DockingManager_DocumentClosed(object? sender, DocumentClosedEventArgs e)
    {
        var operLog = App.Services?.GetService<OperLogManager>();
        try
        {
            operLog?.Debug("[AvalonDock] DocumentClosed 事件触发：DocumentType={DocumentType}, Document={Document}, Title={Title}, ContentId={ContentId}, ContentType={ContentType}", 
                e.Document?.GetType().FullName ?? "null", 
                e.Document?.ToString() ?? "null",
                e.Document?.Title ?? "null",
                e.Document?.ContentId ?? "null",
                e.Document?.Content?.GetType().FullName ?? "null");
            
            // 根据官方文档和日志，通过多种方式查找 AvalonDocument
            Models.AvalonDocument? avalonDoc = null;
            
            // 方法1：直接类型转换
            if (e.Document is Models.AvalonDocument directDoc)
            {
                avalonDoc = directDoc;
            }
            // 方法2：通过 Content 查找（根据日志，Content 可能是我们的 AvalonDocument）
            else if (e.Document?.Content is Models.AvalonDocument contentDoc)
            {
                avalonDoc = contentDoc;
            }
            
            if (avalonDoc != null)
            {
                operLog?.Debug("[AvalonDock] 文档类型匹配：AvalonDocument, Title={Title}, ViewTypeName={ViewTypeName}", 
                    avalonDoc.Title, avalonDoc.ViewTypeName);
                
                if (ViewModel != null)
                {
                    ViewModel.CloseDocument(avalonDoc);
                }
                else
                {
                    operLog?.Warning("[AvalonDock] ViewModel 为 null，无法关闭文档");
                }
            }
            else if (e.Document != null && ViewModel != null)
            {
                // 如果类型转换失败，通过 ContentId 或 Title 查找对应的 AvalonDocument
                var contentId = e.Document.ContentId;
                var title = e.Document.Title;
                
                operLog?.Debug("[AvalonDock] 尝试通过 ContentId 或 Title 查找文档：ContentId={ContentId}, Title={Title}", 
                    contentId ?? "null", title ?? "null");
                
                // 如果ContentId和Title都是null，说明文档已经被AvalonDock移除或清空
                // 尝试从Documents集合中查找所有文档（通过ToString或其他方式）
                Models.AvalonDocument? foundDoc = null;
                
                if (!string.IsNullOrEmpty(contentId))
                {
                    // 优先通过ContentId查找
                    foundDoc = ViewModel.Documents.FirstOrDefault(d => 
                        d != null && (d.ContentId == contentId || d.ViewTypeName == contentId));
                }
                
                if (foundDoc == null && !string.IsNullOrEmpty(title))
                {
                    // 其次通过Title查找
                    foundDoc = ViewModel.Documents.FirstOrDefault(d => 
                        d != null && d.Title == title);
                }
                
                if (foundDoc == null)
                {
                    // 如果ContentId和Title都是null，尝试通过Content来识别文档
                    var docContent = e.Document?.Content;
                    if (docContent != null)
                    {
                        // 根据日志，e.Document.Content 可能是 AvalonDocument 实例本身
                        // 首先尝试直接匹配 AvalonDocument 实例
                        if (docContent is Models.AvalonDocument contentAsAvalonDoc)
                        {
                            foundDoc = ViewModel.Documents.FirstOrDefault(d => ReferenceEquals(d, contentAsAvalonDoc));
                            if (foundDoc != null)
                            {
                                operLog?.Debug("[AvalonDock] 通过Content（AvalonDocument实例）找到文档：Title={Title}, ViewTypeName={ViewTypeName}", 
                                    foundDoc.Title, foundDoc.ViewTypeName);
                            }
                        }
                        
                        // 如果直接匹配失败，尝试通过 UIElement Content 匹配
                        if (foundDoc == null && docContent is System.Windows.UIElement)
                        {
                            foundDoc = ViewModel.Documents.FirstOrDefault(d => 
                                d != null && ReferenceEquals(d.Content, docContent));
                            if (foundDoc != null)
                            {
                                operLog?.Debug("[AvalonDock] 通过Content（UIElement）找到文档：Title={Title}, ViewTypeName={ViewTypeName}, ContentType={ContentType}", 
                                    foundDoc.Title, foundDoc.ViewTypeName, docContent.GetType().FullName);
                            }
                        }
                    }
                    
                    if (foundDoc == null)
                    {
                        // 如果Content查找也失败，尝试通过文档对象引用查找
                        operLog?.Debug("[AvalonDock] 开始备用查找：Documents.Count={Count}, e.Document类型={DocumentType}, e.Document.Content={ContentType}", 
                            ViewModel.Documents.Count, e.Document?.GetType().FullName ?? "null", e.Document?.Content?.GetType().FullName ?? "null");
                        
                        // 先尝试通过对象引用直接查找（最可靠的方法）
                        foundDoc = ViewModel.Documents.FirstOrDefault(d => ReferenceEquals(d, e.Document));
                        if (foundDoc != null)
                        {
                            operLog?.Debug("[AvalonDock] 通过对象引用找到文档：Title={Title}, ViewTypeName={ViewTypeName}", 
                                foundDoc.Title, foundDoc.ViewTypeName);
                        }
                        else
                        {
                            // 如果对象引用查找失败，尝试获取唯一文档（如果只有一个）
                            if (ViewModel.Documents.Count == 1)
                            {
                                foundDoc = ViewModel.Documents.FirstOrDefault();
                                operLog?.Debug("[AvalonDock] ContentId和Title都是null，使用唯一文档：Title={Title}, ViewTypeName={ViewTypeName}, Found={Found}", 
                                    foundDoc?.Title ?? "null", foundDoc?.ViewTypeName ?? "null", foundDoc != null);
                            }
                            else
                            {
                                // 遍历所有文档，记录详细信息
                                operLog?.Debug("[AvalonDock] 遍历所有文档查找：Count={Count}, e.Document.Content类型={ContentType}", 
                                    ViewModel.Documents.Count, docContent?.GetType().FullName ?? "null");
                                foreach (var doc in ViewModel.Documents)
                                {
                                    if (doc is Models.AvalonDocument avalDoc)
                                    {
                                        // 检查多种匹配方式
                                        bool isSameRef = ReferenceEquals(avalDoc, e.Document);
                                        bool isContentSame = ReferenceEquals(avalDoc, docContent);
                                        bool isUIContentSame = docContent is System.Windows.UIElement && ReferenceEquals(avalDoc.Content, docContent);
                                        operLog?.Debug("[AvalonDock] 文档项：Title={Title}, ViewTypeName={ViewTypeName}, ContentId={ContentId}, Type={Type}, IsSameRef={IsSameRef}, IsContentSame={IsContentSame}, IsUIContentSame={IsUIContentSame}", 
                                            avalDoc.Title, avalDoc.ViewTypeName, avalDoc.ContentId, avalDoc.GetType().FullName, 
                                            isSameRef, isContentSame, isUIContentSame);
                                        
                                        // 如果找到匹配，直接使用
                                        if (isContentSame && foundDoc == null)
                                        {
                                            foundDoc = avalDoc;
                                            operLog?.Debug("[AvalonDock] 在遍历过程中找到匹配文档：Title={Title}", avalDoc.Title);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (foundDoc != null)
                {
                    operLog?.Debug("[AvalonDock] 通过备用方法找到文档：Title={Title}, ViewTypeName={ViewTypeName}, ContentId={ContentId}", 
                        foundDoc.Title, foundDoc.ViewTypeName, foundDoc.ContentId);
                    ViewModel.CloseDocument(foundDoc);
                }
                else
                {
                    operLog?.Warning("[AvalonDock] 无法找到对应的 AvalonDocument：ContentId={ContentId}, Title={Title}, Documents.Count={Count}，可能已被移除", 
                        contentId ?? "null", title ?? "null", ViewModel.Documents.Count);
                    // 即使找不到，也尝试从集合中移除（如果AvalonDock已经移除了，这里不会有影响）
                    // 但需要确保ActiveDocument被清除
                    if (ViewModel.ActiveDocument == e.Document)
                    {
                        ViewModel.ActiveDocument = null;
                        operLog?.Debug("[AvalonDock] 已清除活动文档引用（无法找到文档）");
                    }
                }
            }
            else
            {
                operLog?.Warning("[AvalonDock] 文档类型不匹配且无法查找：DocumentType={DocumentType}, ViewModel={ViewModel}", 
                    e.Document?.GetType().FullName ?? "null", ViewModel != null ? "NotNull" : "Null");
            }
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[AvalonDock] 文档关闭事件处理时发生异常");
        }
    }

    /// <summary>
    /// 关闭标签页按钮点击事件（保留以兼容，但 AvalonDock 会自动处理）
    /// </summary>
    private void CloseTabButton_Click(object sender, RoutedEventArgs e)
    {
        // AvalonDock 会自动处理关闭按钮，此方法保留以兼容旧代码
        // 如果需要自定义关闭逻辑，可以在这里处理
    }

    /// <summary>
    /// TabControl 选中项变更事件处理（保留以兼容）
    /// </summary>
    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // AvalonDock 使用 ActiveContent 绑定，此方法保留以兼容
    }

    /// <summary>
    /// 标签页控件右键菜单显示事件（保留以兼容）
    /// </summary>
    private void TabControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // AvalonDock 内置右键菜单，此方法保留以兼容
    }

    /// <summary>
    /// 用户信息按钮点击事件（打开下拉菜单）
    /// </summary>
    private void UserInfoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.IsOpen = true;
        }
    }

    /// <summary>
    /// 用户信息中心菜单项点击事件
    /// </summary>
    private void UserInfoMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowUserInfo();
    }

    /// <summary>
    /// 登出菜单项点击事件
    /// </summary>
    private void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Logout();
    }

    /// <summary>
    /// 查找视觉树中的所有指定类型的子元素
    /// </summary>
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) yield break;
        
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                yield return t;
            }
            
            foreach (var childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }
}

