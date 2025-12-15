using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Takt.Application.Dtos.Identity;
using Takt.Common.Enums;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Fluent.Models;
using Takt.Fluent.Services;
using Takt.Fluent.ViewModels;

#pragma warning disable WPF0001

namespace Takt.Fluent.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }
    private readonly ThemeService _themeService;

    /// <summary>
    /// 主窗口构造函数
    /// </summary>
    /// <param name="viewModel">主窗口视图模型</param>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        if (App.Services == null)
        {
            throw new InvalidOperationException("App.Services is null. Ensure App host initialized before creating MainWindow.");
        }

        _themeService = App.Services.GetRequiredService<ThemeService>();
        _themeService.ThemeChanged += OnThemeChanged;

        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;

        UpdateMainWindowVisuals();

        StateChanged += (s, e) => UpdateMainWindowVisuals();
        Activated += (s, e) => UpdateMainWindowVisuals();
        Deactivated += (s, e) => UpdateMainWindowVisuals();

        // 订阅菜单加载完成事件，默认打开仪表盘
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    /// <summary>
    /// 主题变化事件处理（确保主窗口 UI 能够响应所有主题变化）
    /// 注意：使用 DynamicResource 的元素会自动更新，但 Window 背景可能需要强制刷新
    /// </summary>
    private void OnThemeChanged(object? sender, System.Windows.ThemeMode appliedTheme)
    {
        try
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Debug("[主窗口] 主题已变化: {ThemeMode}", appliedTheme);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    InvalidateVisual();

                    if (TryFindResource("ApplicationBackgroundBrush") is System.Windows.Media.Brush backgroundBrush)
                    {
                        Background = backgroundBrush;
                    }

                    operLog?.Debug("[主窗口] 背景已强制刷新");
                }
                catch (Exception ex)
                {
                    operLog?.Error(ex, "[主窗口] 强制刷新背景时发生异常");
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[主窗口] 主题变化事件处理时发生异常");
        }
    }

    /// <summary>
    /// 窗口关闭时取消订阅事件，防止内存泄漏，并保存侧边栏宽度
    /// </summary>
    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        try
        {
            _themeService.ThemeChanged -= OnThemeChanged;
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            
            // 保存侧边栏宽度
            SaveSidebarWidth();
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Error(ex, "[主窗口] 取消订阅事件时发生异常");
        }
    }

    /// <summary>
    /// 加载侧边栏宽度
    /// </summary>
    private void LoadSidebarWidth()
    {
        try
        {
            var savedWidth = AppSettingsHelper.GetSetting("mainwindow.sidebar.width", "240");
            if (double.TryParse(savedWidth, out var width))
            {
                // 确保宽度在有效范围内
                width = Math.Max(64, Math.Min(320, width));
                SidebarColumn.Width = new GridLength(width);
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[主窗口] 加载侧边栏宽度失败");
        }
    }

    /// <summary>
    /// 保存侧边栏宽度
    /// </summary>
    private void SaveSidebarWidth()
    {
        try
        {
            var width = SidebarColumn.ActualWidth;
            AppSettingsHelper.SaveSetting("mainwindow.sidebar.width", width.ToString("F0"));
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[主窗口] 保存侧边栏宽度失败");
        }
    }

    /// <summary>
    /// 侧边栏分隔条拖拽事件处理
    /// </summary>
    private void SidebarSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        try
        {
            var newWidth = SidebarColumn.ActualWidth + e.HorizontalChange;
            // 确保宽度在有效范围内
            newWidth = Math.Max(64, Math.Min(320, newWidth));
            SidebarColumn.Width = new GridLength(newWidth);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[主窗口] 调整侧边栏宽度失败");
        }
    }

    /// <summary>
    /// 侧边栏分隔条拖拽完成事件处理（保存宽度）
    /// </summary>
    private void SidebarSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        SaveSidebarWidth();
    }

    /// <summary>
    /// ViewModel 属性变更事件处理
    /// 当菜单加载完成时，自动打开默认仪表盘
    /// </summary>
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

    /// <summary>
    /// 主窗口加载完成事件处理
    /// 初始化主题、语言设置，并打开默认仪表盘
    /// </summary>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var operLog = App.Services?.GetService<OperLogManager>();

            // 加载侧边栏宽度
            LoadSidebarWidth();

            // 重新初始化主题（确保主窗口打开时应用保存的主题设置）
            // 简化逻辑：直接使用登录窗口已经设置好的主题（通过缓存传递）
            try
            {
                var savedTheme = _themeService.GetCurrentTheme();
                operLog?.Debug("[主窗口] 获取当前主题（优先缓存）：{ThemeMode}", savedTheme);

                _themeService.SetTheme(savedTheme);

                // 立即刷新窗口背景，确保首次加载视觉正确
                // 使用 _ = 明确表示我们不等待此操作（触发并忘记）
                _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                    if (TryFindResource("ApplicationBackgroundBrush") is System.Windows.Media.Brush backgroundBrush)
                    {
                        Background = backgroundBrush;
                    }
                        operLog?.Debug("[主窗口] 初始化时背景已刷新");
                    }
                    catch (Exception ex)
                    {
                        operLog?.Error(ex, "[主窗口] 初始化时刷新背景发生异常");
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                operLog?.Warning("[主窗口] 重新初始化主题失败，使用当前主题：{Exception}", ex.Message);
            }

            // 重新初始化语言（确保主窗口打开时应用保存的语言设置）
            try
            {
                var localizationManager = App.Services?.GetService<Takt.Domain.Interfaces.ILocalizationManager>();
                if (localizationManager != null)
                {
                    // 确保本地化管理器已初始化
                    await localizationManager.InitializeAsync();

                    // 从配置文件读取保存的语言设置
                    var savedLang = Takt.Common.Helpers.AppSettingsHelper.GetLanguage();
                    if (!string.IsNullOrWhiteSpace(savedLang) && savedLang != localizationManager.CurrentLanguage)
                    {
                        // 重新应用保存的语言（确保主窗口使用最新的语言设置）
                        localizationManager.ChangeLanguage(savedLang);
                        operLog?.Debug("[主窗口] 语言已重新初始化：{LanguageCode}", savedLang);
                    }
                    else
                    {
                        // 如果没有保存的语言或语言已匹配，使用当前语言
                        var currentLang = localizationManager.CurrentLanguage;
                        operLog?.Debug("[主窗口] 使用当前本地化管理器的语言：{LanguageCode}", currentLang);
                    }
                }
            }
            catch (Exception ex)
            {
                operLog?.Warning("[主窗口] 重新初始化语言失败，使用当前语言：{Exception}", ex.Message);
            }

            // 刷新用户信息显示
            if (ViewModel != null)
            {
                ViewModel.LoadCurrentUserInfo();

                // 窗口加载完成后，如果菜单已经加载，打开仪表盘
                if (ViewModel.Menus != null && ViewModel.Menus.Any())
                {
                    // 延迟执行，确保 TabControl 完全初始化
                    // 使用 _ = 明确表示我们不等待此操作（触发并忘记）
                    _ = Dispatcher.BeginInvoke(new Action(() =>
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

    /// <summary>
    /// 打开默认仪表盘
    /// 查找第一个菜单类型的菜单项并导航到该视图
    /// </summary>
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

                    // 导航到仪表盘（使用 ViewModel 导航方法）
                    ViewModel.NavigateToMenu(dashboardMenu);

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
    /// <returns>如果找到目标菜单，返回 true；否则返回 false</returns>
    private bool SelectMenuItem(MenuDto targetMenu, List<MenuDto> menus, System.Windows.Controls.TreeViewItem? parentItem = null)
    {
        foreach (var menu in menus)
        {
            if (menu.Id == targetMenu.Id)
            {
                // 找到目标菜单，选中它
                var container = MenusList.ItemContainerGenerator.ContainerFromItem(menu) as System.Windows.Controls.TreeViewItem;
                if (container != null)
                {
                    container.IsSelected = true;
                    container.BringIntoView();
                }
                return true; // 找到目标，返回 true
            }
            if (menu.Children != null && menu.Children.Any())
            {
                // 先递归查找，如果找到目标菜单，才展开当前父级
                var container = MenusList.ItemContainerGenerator.ContainerFromItem(menu) as System.Windows.Controls.TreeViewItem;
                bool found = SelectMenuItem(targetMenu, menu.Children, container);
                if (found && container != null)
                {
                    // 只有在找到目标菜单的情况下，才展开当前父级
                    container.IsExpanded = true;
                    return true; // 找到目标，返回 true
                }
            }
        }
        return false; // 未找到目标，返回 false
    }

    /// <summary>
    /// 更新主窗口视觉效果
    /// 根据窗口状态调整主网格的边距和最大化按钮图标
    /// </summary>
    private void UpdateMainWindowVisuals()
    {
        MainGrid.Margin = default;
        if (WindowState == WindowState.Maximized)
        {
            MainGrid.Margin = new Thickness(8);
            // 最大化时显示还原图标
            if (MaximizeIcon != null)
            {
                MaximizeIcon.Icon = FontAwesome.Sharp.IconChar.WindowRestore;
                MaximizeIcon.IconFont = FontAwesome.Sharp.IconFont.Regular;
            }
        }
        else
        {
            // 还原时显示最大化图标
            if (MaximizeIcon != null)
            {
                MaximizeIcon.Icon = FontAwesome.Sharp.IconChar.Square;
                MaximizeIcon.IconFont = FontAwesome.Sharp.IconFont.Regular;
            }
        }
    }

    /// <summary>
    /// 最小化窗口按钮点击事件处理
    /// </summary>
    private void MinimizeWindow(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// 最大化/还原窗口按钮点击事件处理
    /// </summary>
    private void MaximizeWindow(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
            // 还原时显示最大化图标（实心正方形）
            MaximizeIcon.Icon = FontAwesome.Sharp.IconChar.Square;
            MaximizeIcon.IconFont = FontAwesome.Sharp.IconFont.Regular;
        }
        else
        {
            this.WindowState = WindowState.Maximized;
            // 最大化时显示还原图标（两个重叠的正方形）
            MaximizeIcon.Icon = FontAwesome.Sharp.IconChar.WindowRestore;
            MaximizeIcon.IconFont = FontAwesome.Sharp.IconFont.Regular;
        }
    }

    /// <summary>
    /// 关闭窗口按钮点击事件处理
    /// </summary>
    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    /// <summary>
    /// 标题栏鼠标左键按下事件处理
    /// 双击切换最大化/还原，单击拖动窗口
    /// </summary>
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

    /// <summary>
    /// 菜单列表键盘预览事件处理
    /// 按 Enter 键时触发选中菜单项的导航
    /// </summary>
    private void MenusList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var treeView = sender as TreeView;
            if (treeView?.SelectedItem is MenuDto menuItem)
            {
                var tvi = MenusList.ItemContainerGenerator.ContainerFromItem(treeView.SelectedItem) as System.Windows.Controls.TreeViewItem;
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
    private System.Windows.Controls.TreeViewItem? FindTreeViewItemByItem(ItemsControl itemsControl, object item)
    {
        // 先尝试在当前层级查找
        if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is System.Windows.Controls.TreeViewItem tvi)
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
                return itemsControl.ItemContainerGenerator.ContainerFromItem(childItem) as System.Windows.Controls.TreeViewItem;
            }

            var childContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(childItem) as System.Windows.Controls.TreeViewItem;
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

    /// <summary>
    /// 菜单列表鼠标左键释放预览事件处理
    /// 点击菜单项时触发导航（排除展开/折叠按钮）
    /// </summary>
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
    private System.Windows.Controls.TreeViewItem? FindParentTreeViewItem(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is System.Windows.Controls.TreeViewItem treeViewItem)
            {
                return treeViewItem;
            }
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    /// <summary>
    /// 菜单列表加载完成事件处理
    /// 不再自动选中第一项，因为第一项可能是目录
    /// 默认打开仪表盘的逻辑在 MainWindow_Loaded 中处理
    /// </summary>
    private void MenusList_Loaded(object sender, RoutedEventArgs e)
    {
        // 不再自动选中第一项，因为第一项可能是目录
        // 默认打开仪表盘的逻辑在 MainWindow_Loaded 中处理
    }

    /// <summary>
    /// 菜单项选中变更处理
    /// 根据菜单类型执行展开/折叠或导航操作
    /// 实现手风琴效果：展开一个菜单项时，自动收缩其他已展开的菜单项
    /// </summary>
    /// <param name="tvi">TreeViewItem 容器</param>
    /// <param name="menuItem">菜单项数据（如果为 null，则从 tvi.DataContext 获取）</param>
    private void SelectedItemChanged(System.Windows.Controls.TreeViewItem? tvi, MenuDto? menuItem = null)
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
            // 获取当前展开状态
            bool willBeExpanded = !tvi.IsExpanded;
            
            // 如果将要展开，先收缩所有其他已展开的菜单项（手风琴效果）
            if (willBeExpanded)
            {
                CollapseAllTreeViewItemsExcept(MenusList, tvi);
            }
            
            // 切换展开/折叠状态
            tvi.IsExpanded = willBeExpanded;
            
            // 导航到快速导航页面（如果routePath或Component存在）
            if (!string.IsNullOrEmpty(menuItem.RoutePath) || !string.IsNullOrEmpty(menuItem.Component))
            {
                // 使用 ViewModel 的导航方法（符合 MVVM）
                ViewModel.NavigateToMenu(menuItem);
            }
        }
        else if (menuItem.MenuType == MenuTypeEnum.Menu)
        {
            // 菜单类型触发导航
            HandleSelectedMenu();
        }
    }

    /// <summary>
    /// 收缩所有 TreeViewItem，除了指定的项（手风琴效果）
    /// </summary>
    /// <param name="treeView">TreeView 控件</param>
    /// <param name="exceptItem">不收缩的 TreeViewItem</param>
    private void CollapseAllTreeViewItemsExcept(System.Windows.Controls.TreeView treeView, System.Windows.Controls.TreeViewItem exceptItem)
    {
        if (treeView == null || exceptItem == null)
        {
            return;
        }

        // 遍历所有顶层项
        foreach (var item in treeView.Items)
        {
            var container = treeView.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.Controls.TreeViewItem;
            if (container != null)
            {
                CollapseTreeViewItemRecursive(container, exceptItem);
            }
        }
    }

    /// <summary>
    /// 递归收缩 TreeViewItem，除了指定的项
    /// </summary>
    /// <param name="item">要处理的 TreeViewItem</param>
    /// <param name="exceptItem">不收缩的 TreeViewItem</param>
    private void CollapseTreeViewItemRecursive(System.Windows.Controls.TreeViewItem item, System.Windows.Controls.TreeViewItem exceptItem)
    {
        if (item == null)
        {
            return;
        }

        // 如果不是要保留的项，且当前是展开状态，则收缩
        if (item != exceptItem && item.IsExpanded)
        {
            item.IsExpanded = false;
        }

        // 递归处理子项
        if (item.HasItems)
        {
            for (int i = 0; i < item.Items.Count; i++)
            {
                var childContainer = item.ItemContainerGenerator.ContainerFromIndex(i) as System.Windows.Controls.TreeViewItem;
                if (childContainer != null)
                {
                    CollapseTreeViewItemRecursive(childContainer, exceptItem);
                }
            }
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
            else if (menuItem.MenuType == MenuTypeEnum.Directory && (!string.IsNullOrEmpty(menuItem.RoutePath) || !string.IsNullOrEmpty(menuItem.Component)))
            {
                // 目录类型：如果配置了路由路径或组件，也可以导航
                // 使用 ViewModel 的导航方法（符合 MVVM）
                ViewModel.NavigateToMenu(menuItem);
            }
        }
    }

    /// <summary>
    /// 处理选中的菜单项
    /// 更新 ViewModel 选中项并触发导航
    /// </summary>
    private void HandleSelectedMenu()
    {
        if (MenusList.SelectedItem is MenuDto menuItem)
        {
            ViewModel.SelectedMenu = menuItem;

            // 将选中项滚动到可视区域
            var tvi = MenusList.ItemContainerGenerator.ContainerFromItem(menuItem) as System.Windows.Controls.TreeViewItem;
            if (tvi != null)
            {
                tvi.BringIntoView();
            }

            // 只有菜单类型（MenuTypeEnum.Menu）才应该触发导航
            // 目录类型（MenuTypeEnum.Directory）只用于展开/折叠，不应该导航
            if (menuItem.MenuType == MenuTypeEnum.Menu && (!string.IsNullOrEmpty(menuItem.RoutePath) || !string.IsNullOrEmpty(menuItem.Component)))
            {
                var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Debug("[导航] HandleSelectedMenu 触发导航：菜单={MenuCode} ({MenuName})",
                    menuItem.MenuCode, menuItem.MenuName);
                // 使用 ViewModel 的导航方法（符合 MVVM）
                ViewModel.NavigateToMenu(menuItem);
            }
        }
    }

    /// <summary>
    /// 导航到指定的菜单（公共方法，供导航页面调用）
    /// 使用 ViewModel 的导航方法（符合 MVVM）
    /// </summary>
    public void NavigateToMenu(MenuDto menuItem)
    {
        // 使用 ViewModel 的导航方法
        ViewModel.NavigateToMenu(menuItem);

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
            // Component 应该已经是完整的类型名称，如 "Takt.Fluent.Views.Identity.UserView"
            typeName = menuItem.Component;
            operLog?.Debug("[导航] 使用 Component 作为类型名称：{TypeName}", typeName);
        }
        else if (!string.IsNullOrEmpty(menuItem.RoutePath))
        {
            // 约定：RoutePath 如 "Views/Identity/UserView"，转换为 "Takt.Fluent.Views.Identity.UserView"
            typeName = $"Takt.Fluent.{menuItem.RoutePath.Replace('/', '.')}";
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

            // 验证实例类型（必须是 UIElement 才能正确显示在 TabControl 中）
            if (instance is not System.Windows.UIElement uiElement)
            {
                operLog?.Error("[导航] 创建的视图实例不是 UIElement：{TypeName}, InstanceType={InstanceType}",
                    typeName, instance.GetType().FullName ?? "未知类型");
                return;
            }

            operLog?.Debug("[导航] 视图实例创建成功：{TypeName}, InstanceType={InstanceType}, IsUIElement=True",
                typeName, instance.GetType().FullName ?? "未知类型");

            // 检查 ViewModel 是否可用
            if (ViewModel == null)
            {
                operLog?.Error("[导航] ViewModel 为 null，无法添加标签页：{TypeName}", typeName);
                return;
            }

            // 检查 ViewModel.DocumentTabs 是否已初始化
            if (ViewModel.DocumentTabs == null)
            {
                operLog?.Error("[导航] ViewModel.DocumentTabs 为 null，无法添加标签页：{TypeName}", typeName);
                return;
            }

            // 使用 WPF UI TabControl：添加或激活标签页
            Models.DocumentTabItem? tabItem = null;
            try
            {
                tabItem = ViewModel.CreateOrActivateTab(menuItem, instance, typeName);
            }
            catch (ArgumentNullException ex)
            {
                operLog?.Error(ex, "[导航] 添加标签页时参数为空：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
                return;
            }
            catch (InvalidOperationException ex)
            {
                operLog?.Error(ex, "[导航] 添加标签页时操作无效：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
                return;
            }

            if (tabItem != null)
            {
                // 验证标签页已成功添加到集合中
                if (ViewModel.DocumentTabs.Contains(tabItem))
                {
                    // 获取图标信息（如果有）
                    string iconInfo = !string.IsNullOrEmpty(menuItem.Icon) ? $"，图标：{menuItem.Icon}" : "";
                    operLog?.Information("[导航] 成功导航到视图：{TypeName} (菜单: {MenuCode})，标签页：{TabTitle}{IconInfo}",
                        typeName, menuItem.MenuCode, tabItem.Title, iconInfo);
                }
                else
                {
                    operLog?.Warning("[导航] 标签页创建成功但未添加到集合：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
                }
            }
            else
            {
                operLog?.Warning("[导航] 添加标签页返回 null：{TypeName} (菜单: {MenuCode})", typeName, menuItem.MenuCode);
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
    /// TabItem 关闭按钮点击事件处理
    /// </summary>
    private void TabItem_CloseButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var operLog = App.Services?.GetService<OperLogManager>();

            // 获取 TabItem
            var button = sender as System.Windows.Controls.Button;
            if (button == null) return;

            // 通过视觉树向上查找 TabItem
            var tabItem = FindAncestor<TabItem>(button);
            if (tabItem == null)
            {
                operLog?.Warning("[Tab] 无法找到 TabItem，关闭按钮点击失败");
                return;
            }

            // 获取对应的 DocumentTabItem
            var documentTabItem = tabItem.DataContext as DocumentTabItem;
            if (documentTabItem == null)
            {
                operLog?.Warning("[Tab] TabItem.DataContext 不是 DocumentTabItem，关闭失败");
                return;
            }

            // 调用 ViewModel 的关闭方法
            if (ViewModel != null)
            {
                ViewModel.CloseTab(documentTabItem);
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[Tab] 关闭按钮点击事件处理时发生异常");
        }
    }

    /// <summary>
    /// 在视觉树中向上查找指定类型的父元素
    /// </summary>
    private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T t)
            {
                return t;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
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
        if (sender is System.Windows.Controls.Button button && button.ContextMenu != null)
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

    /// <summary>
    /// 递归刷新元素及其子元素的背景（确保 DynamicResource 正确更新）
    /// </summary>
}

