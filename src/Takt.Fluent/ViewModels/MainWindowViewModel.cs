//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : MainWindowViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-29
// 版本号 : 0.0.1
// 描述    : 主窗口视图模型
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Context;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Models;
using Takt.Fluent.Controls;
using Takt.Fluent.Services;
using Microsoft.Extensions.DependencyInjection;
using Prism.Navigation.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using WpfApp = System.Windows.Application;

namespace Takt.Fluent.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IRegionManager _regionManager;
    private readonly ILocalizationManager _localizationManager;
    private readonly INavigationService _navigationService;
    private readonly OperLogManager? _operLog;

    public MainWindowViewModel(
        IRegionManager regionManager,
        ILocalizationManager localizationManager,
        INavigationService navigationService,
        OperLogManager? operLog = null)
    {
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _operLog = operLog;
    }

    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private List<MenuDto> _menus = new();

    [ObservableProperty]
    private MenuDto? _selectedMenu;

    [ObservableProperty]
    private bool _canNavigateback;

    /// <summary>
    /// 文档标签页集合（使用 WPF UI TabControl）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DocumentTabItem> _documentTabs = new();

    /// <summary>
    /// 当前选中的标签页
    /// </summary>
    [ObservableProperty]
    private DocumentTabItem? _selectedTab;

    /// <summary>
    /// 当前登录用户名
    /// </summary>
    [ObservableProperty]
    private string _currentUsername = string.Empty;

    /// <summary>
    /// 当前登录用户真实姓名
    /// </summary>
    [ObservableProperty]
    private string _currentRealName = string.Empty;

    /// <summary>
    /// 当前登录用户角色名称
    /// </summary>
    [ObservableProperty]
    private string _currentRoleName = string.Empty;

    /// <summary>
    /// 当前登录用户头像路径
    /// </summary>
    [ObservableProperty]
    private string _currentAvatar = string.Empty;

    /// <summary>
    /// 状态栏消息（用于显示 Toast 通知）
    /// </summary>
    [ObservableProperty]
    private string? _statusBarMessage;

    /// <summary>
    /// 状态栏消息图标
    /// </summary>
    [ObservableProperty]
    private MaterialDesignThemes.Wpf.PackIconKind _statusBarMessageIcon = MaterialDesignThemes.Wpf.PackIconKind.Information;

    /// <summary>
    /// 状态栏消息颜色
    /// </summary>
    [ObservableProperty]
    private System.Windows.Media.Brush _statusBarMessageBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243));

    /// <summary>
    /// 状态栏消息定时器
    /// </summary>
    private DispatcherTimer? _statusBarMessageTimer;

    /// <summary>
    /// 显示状态栏消息（Toast 通知）
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="icon">图标类型</param>
    /// <param name="brush">图标颜色</param>
    /// <param name="duration">显示时长（毫秒），默认 10000（10秒）</param>
    public void ShowStatusBarMessage(string message, PackIconKind icon, Brush brush, int duration = 10000)
    {
        // 停止之前的定时器
        _statusBarMessageTimer?.Stop();
        
        // 设置消息
        StatusBarMessage = message;
        StatusBarMessageIcon = icon;
        StatusBarMessageBrush = brush;
        
        // 启动定时器，自动清除消息
        _statusBarMessageTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(duration)
        };
        _statusBarMessageTimer.Tick += (s, e) =>
        {
            _statusBarMessageTimer.Stop();
            StatusBarMessage = null;
        };
        _statusBarMessageTimer.Start();
    }

    /// <summary>
    /// 当前活动文档标题（用于状态栏显示）
    /// </summary>
    [ObservableProperty]
    private string _activeDocumentTitle = string.Empty;

    private readonly IMenuService _menuService = null!;
    private readonly IUserService? _userService;
    private long _currentUserId;

    [RelayCommand]
    public void Settings()
    {
        // 直接打开用户自定义设置（MySettingsView）
        if (App.Services == null || DocumentTabs == null)
        {
            return;
        }

        try
        {
            const string viewTypeName = "Takt.Fluent.Views.Settings.MySettingsView";
            
            // 检查是否已经打开
            var existingTab = DocumentTabs.FirstOrDefault(t => t.ViewTypeName == viewTypeName);
            if (existingTab != null)
            {
                SelectedTab = existingTab;
                return;
            }

            // 从 DI 容器获取视图实例
            var view = App.Services.GetService<Views.Settings.MySettingsView>();
            if (view == null)
            {
                return;
            }

            // 创建 DocumentTabItem
            var menuItem = new MenuDto
            {
                MenuCode = "customize_settings",
                MenuName = "用户设置",
                I18nKey = "menu.settings",
                Icon = "Gear"
            };
            
            var title = _localizationManager.GetString("menu.settings");
            var tabItem = new Models.DocumentTabItem(menuItem, title, view, viewTypeName);

            // 添加到标签页集合并激活
            DocumentTabs.Add(tabItem);
            SelectedTab = tabItem;
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[设置] 打开用户自定义设置失败");
        }
    }

    /// <summary>
    /// 扁平化菜单树（递归）
    /// </summary>
    private IEnumerable<MenuDto> FlattenMenu(MenuDto menu)
    {
        yield return menu;
        if (menu.Children != null)
        {
            foreach (var child in menu.Children)
            {
                foreach (var flattened in FlattenMenu(child))
                {
                    yield return flattened;
                }
            }
        }
    }

    public MainWindowViewModel(
        IRegionManager regionManager,
        IMenuService menuService, 
        ILocalizationManager localizationManager,
        INavigationService navigationService,
        IUserService? userService = null!, 
        OperLogManager? operLog = null)
        : this(regionManager, localizationManager, navigationService, operLog)
    {
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _userService = userService;
        ApplicationTitle = GetLocalizedApplicationTitle();
        _localizationManager.LanguageChanged += OnLanguageChanged;

        _ = LoadMenusAsync(0); // 默认加载，实际应传入当前登录用户ID
        
        // 加载当前用户信息
        LoadCurrentUserInfo();
        
        // 监听 SelectedTab 变化，更新活动文档标题
        PropertyChanged += MainWindowViewModel_PropertyChanged;
    }
    
    private void MainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedTab))
        {
            UpdateActiveDocumentTitle();
        }
    }
    
    /// <summary>
    /// 更新当前活动文档标题
    /// </summary>
    private void UpdateActiveDocumentTitle()
    {
        if (SelectedTab != null)
        {
            ActiveDocumentTitle = SelectedTab.Title ?? string.Empty;
        }
        else
        {
            ActiveDocumentTitle = string.Empty;
        }
    }
    
    /// <summary>
    /// 获取本地化的标题（与菜单树 StringToTranslationConverter 完全相同的逻辑）
    /// </summary>
    private string GetLocalizedTitle(MenuDto menuItem)
    {
        // 完全复制 StringToTranslationConverter.Convert() 的逻辑
        var key = menuItem.I18nKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            // 如果没有 I18nKey，使用 MenuName
            return menuItem.MenuName ?? menuItem.MenuCode ?? "未命名";
        }
        
        // 使用本地化管理器获取翻译
        var translation = _localizationManager.GetString(key);
        
        // 如果找不到翻译，返回 key 本身（与转换器逻辑一致）
        if (translation == key)
        {
            return menuItem.MenuName ?? menuItem.MenuCode ?? key;
        }
        
        return translation;
    }

    /// <summary>
    /// 加载当前用户信息
    /// </summary>
    public void LoadCurrentUserInfo()
    {
        var userContext = UserContext.Current;
        if (userContext.IsAuthenticated)
        {
            CurrentUsername = userContext.Username;
            CurrentRealName = userContext.RealName;
            CurrentRoleName = userContext.RoleName;
            _currentUserId = userContext.UserId;
            
            // 异步加载头像
            _ = LoadCurrentUserAvatarAsync();
        }
        else
        {
            CurrentAvatar = string.Empty;
        }
    }

    /// <summary>
    /// 异步加载当前用户头像
    /// </summary>
    private async Task LoadCurrentUserAvatarAsync()
    {
        if (_userService == null || _currentUserId == 0)
        {
            CurrentAvatar = "assets/avatar.png"; // 默认头像
            return;
        }

        try
        {
            var result = await _userService.GetByIdAsync(_currentUserId);
            if (result.Success && result.Data != null)
            {
                // 如果头像为空，使用默认头像
                CurrentAvatar = string.IsNullOrWhiteSpace(result.Data.Avatar) 
                    ? "assets/avatar.png" 
                    : result.Data.Avatar;
            }
            else
            {
                CurrentAvatar = "assets/avatar.png"; // 默认头像
            }
        }
        catch
        {
            CurrentAvatar = "assets/avatar.png"; // 默认头像
        }
    }

    private string GetLocalizedApplicationTitle()
    {
        return _localizationManager.GetString("application.title");
    }

    private void OnLanguageChanged(object? sender, string languageCode)
    {
        // 确保在 UI 线程上执行
        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                UpdateLanguageDependentProperties();
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateLanguageDependentProperties();
                });
            }
        }
        else
        {
            // 如果没有 Dispatcher，直接执行（可能不在 UI 线程）
            UpdateLanguageDependentProperties();
        }
    }
    
    private void UpdateLanguageDependentProperties()
    {
        ApplicationTitle = GetLocalizedApplicationTitle();
        
        // 更新所有已打开的标签页标题
        if (DocumentTabs != null)
        {
            foreach (var tab in DocumentTabs)
            {
                if (tab.MenuItem != null)
                {
                    var titleKey = tab.MenuItem.I18nKey ?? tab.MenuItem.MenuCode ?? string.Empty;
                    var newTitle = !string.IsNullOrWhiteSpace(titleKey)
                        ? _localizationManager.GetString(titleKey)
                        : tab.MenuItem.MenuName ?? "未命名";
                    tab.Title = newTitle;
                }
            }
        }
        
        // 触发菜单集合的 PropertyChanged，强制刷新菜单显示
        // 注意：由于 Menus 是 List，不是 ObservableCollection，需要重新设置才能触发更新
        // 但更好的方法是让 XAML 绑定自动响应 CurrentLanguageCode 的变化（已通过 MultiBinding 实现）
        // 这里只需要确保菜单项能够响应绑定更新
        OnPropertyChanged(nameof(Menus));
        
        // 更新活动文档标题
        UpdateActiveDocumentTitle();
    }

    ~MainWindowViewModel()
    {
        _localizationManager.LanguageChanged -= OnLanguageChanged;
    }

    /// <summary>
    /// 创建或激活文档（WPF UI TabControl 版本，兼容旧代码）
    /// </summary>
    [Obsolete("请使用 CreateOrActivateTab 方法")]
    public DocumentTabItem AddOrActivateDocument(MenuDto menuItem, object content, string viewTypeName)
    {
        // 直接调用 CreateOrActivateTab（已支持 UI 线程检查）
        return CreateOrActivateTab(menuItem, content, viewTypeName);
    }


    /// <summary>
    /// 创建或激活标签页（WPF UI TabControl 版本）
    /// </summary>
    public DocumentTabItem CreateOrActivateTab(MenuDto menuItem, object content, string viewTypeName)
        {
            // 参数验证
            if (menuItem == null) 
                throw new ArgumentNullException(nameof(menuItem));
            if (content == null) 
                throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrEmpty(viewTypeName)) 
                throw new ArgumentException("视图类型名称不能为空", nameof(viewTypeName));

        // 在 UI 线程中执行
        if (System.Windows.Application.Current?.Dispatcher == null)
            throw new InvalidOperationException("Application.Current 或 Dispatcher 为 null，无法执行 UI 操作");

        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
            return CreateOrActivateTabCore(menuItem, content, viewTypeName);
            }
            else
            {
            DocumentTabItem? result = null;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                result = CreateOrActivateTabCore(menuItem, content, viewTypeName);
            });
            return result ?? throw new InvalidOperationException("创建标签页失败，返回 null");
        }
    }

    /// <summary>
    /// 创建或激活标签页的核心实现（必须在 UI 线程中调用）
    /// 标准 MDI 实现：检查是否已打开，存在则激活，不存在则创建并激活
    /// </summary>
    private DocumentTabItem CreateOrActivateTabCore(MenuDto menuItem, object content, string viewTypeName)
    {
        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
        try
        {
            // 参数验证
            if (menuItem == null) 
                throw new ArgumentNullException(nameof(menuItem));
            if (content == null) 
                throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrEmpty(viewTypeName)) 
                throw new ArgumentException("视图类型名称不能为空", nameof(viewTypeName));
            if (DocumentTabs == null)
                throw new InvalidOperationException("DocumentTabs 集合未初始化");

            // 检查标签页是否已存在（通过 ViewTypeName 判断）
            var existingTab = DocumentTabs.FirstOrDefault(t => t != null && t.ViewTypeName == viewTypeName);
            if (existingTab != null && DocumentTabs.Contains(existingTab))
            {
                // 已存在，直接激活
                operLog?.Debug("[Tab] 标签页已存在，激活：Title={Title}, ViewTypeName={ViewTypeName}", 
                    existingTab.Title, viewTypeName);
                SelectedTab = existingTab;
                return existingTab;
            }

            // 获取本地化的标题
            var titleKey = menuItem.I18nKey ?? menuItem.MenuCode ?? string.Empty;
            var title = !string.IsNullOrWhiteSpace(titleKey) 
                ? _localizationManager.GetString(titleKey)
                : menuItem.MenuName ?? "未命名";

            // 创建新标签页
            var newTab = new DocumentTabItem(menuItem, title, content, viewTypeName);
            
            // 验证创建成功
            if (newTab == null)
                throw new InvalidOperationException("创建标签页失败，返回 null");
            
            // 添加到集合
            DocumentTabs.Add(newTab);
            operLog?.Debug("[Tab] 新标签页已创建：Title={Title}, ViewTypeName={ViewTypeName}, Count={Count}", 
                title, viewTypeName, DocumentTabs.Count);
            
            // 激活新标签页（确保集合包含该项）
            if (DocumentTabs.Contains(newTab))
            {
                SelectedTab = newTab;
                operLog?.Debug("[Tab] 新标签页已激活：Title={Title}", title);
            }
            else
            {
                operLog?.Warning("[Tab] 新标签页未在集合中找到，无法激活：Title={Title}", title);
            }
            
            return newTab;
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[Tab] 添加或激活标签页时发生异常：ViewTypeName={ViewTypeName}", viewTypeName);
            throw;
        }
    }


    /// <summary>
    /// 关闭指定的标签页（标准 MDI 实现）
    /// </summary>
    [RelayCommand]
    public void CloseTab(DocumentTabItem? tabItem)
    {
        if (tabItem == null) return;
        if (DocumentTabs == null) return;
            
            // 检查是否可以关闭
            if (!tabItem.CanClose)
            {
                System.Windows.MessageBox.Show(
                    "默认仪表盘标签页不允许关闭。",
                    "提示",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
        try
        {
            operLog?.Debug("[Tab] 开始关闭标签页：Title={Title}, ViewTypeName={ViewTypeName}, Count={Count}", 
                tabItem.Title, tabItem.ViewTypeName, DocumentTabs.Count);
            
            // 检查是否在集合中
            if (!DocumentTabs.Contains(tabItem))
            {
                operLog?.Warning("[Tab] 标签页不在集合中，无法关闭：Title={Title}", tabItem.Title);
                return;
            }
            
            // 获取当前索引和选中状态
            int currentIndex = DocumentTabs.IndexOf(tabItem);
            bool wasSelected = SelectedTab == tabItem;
            
            operLog?.Debug("[Tab] 标签页信息：Index={Index}, WasSelected={WasSelected}, Count={Count}", 
                currentIndex, wasSelected, DocumentTabs.Count);
            
            // 如果关闭的是当前选中的标签页，需要激活相邻的标签页
            if (wasSelected)
            {
                // 先清除选中状态，避免移除时触发异常
                    SelectedTab = null;
                
                // 计算应该激活的标签页索引
                // 标准 MDI 行为：优先选择右侧的标签，如果没有右侧则选择左侧
                int targetIndex = -1;
                if (currentIndex < DocumentTabs.Count - 1)
                {
                    // 有右侧标签，选择右侧
                    targetIndex = currentIndex;
                }
                else if (currentIndex > 0)
                {
                    // 没有右侧，选择左侧
                    targetIndex = currentIndex - 1;
                }
                // 如果 currentIndex == 0 且 Count == 1，targetIndex 保持 -1，表示没有可激活的标签
                
                // 从集合中移除
                DocumentTabs.RemoveAt(currentIndex);
                operLog?.Debug("[Tab] 标签页已移除：Title={Title}, Removed={Removed}, RemainingCount={Count}", 
                    tabItem.Title, true, DocumentTabs.Count);
                
                // 激活目标标签页
                if (targetIndex >= 0 && targetIndex < DocumentTabs.Count)
        {
                    var targetTab = DocumentTabs[targetIndex];
                    if (targetTab != null)
                    {
                        SelectedTab = targetTab;
                        operLog?.Debug("[Tab] 已激活相邻标签页：Title={Title}, Index={Index}", 
                            targetTab.Title, targetIndex);
                    }
                }
                else if (DocumentTabs.Count > 0)
            {
                    // 如果计算出的索引无效，选择第一个可用标签
                    var firstTab = DocumentTabs.FirstOrDefault(t => t != null);
                    if (firstTab != null)
                            {
                        SelectedTab = firstTab;
                        operLog?.Debug("[Tab] 已激活第一个可用标签页：Title={Title}", firstTab.Title);
                    }
                                }
                                else
                                {
                    // 没有剩余标签页
                                        SelectedTab = null;
                    operLog?.Debug("[Tab] 没有剩余标签页，已清空选中状态");
                                }
                            }
                            else
                            {
                // 关闭的不是当前选中的标签页，直接移除即可
                DocumentTabs.Remove(tabItem);
                operLog?.Debug("[Tab] 标签页已移除（非选中）：Title={Title}, RemainingCount={Count}", 
                    tabItem.Title, DocumentTabs.Count);
            }
            
            // 释放资源
            if (tabItem.Content is IDisposable disposable)
            {
                            try
                            {
                    disposable.Dispose();
                    operLog?.Debug("[Tab] 已释放 Content 资源：ViewTypeName={ViewTypeName}", tabItem.ViewTypeName);
                }
                catch (Exception ex)
                {
                    operLog?.Error(ex, "[Tab] 释放 Content 资源时发生异常：{ViewTypeName}", tabItem.ViewTypeName);
                }
            }
            
            // 清空 Content 引用
            tabItem.Content = null;
            
            operLog?.Debug("[Tab] 标签页关闭完成：Title={Title}, ViewTypeName={ViewTypeName}, RemainingCount={Count}", 
                tabItem.Title, tabItem.ViewTypeName, DocumentTabs.Count);
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[Tab] 关闭标签页时发生异常：Title={Title}, ViewTypeName={ViewTypeName}", 
                tabItem.Title, tabItem.ViewTypeName);
        }
    }


    /// <summary>
    /// 关闭除当前标签外的所有标签（标准 MDI 实现）
    /// </summary>
    [RelayCommand]
    public void CloseAllTabsExceptCurrent()
    {
        if (DocumentTabs == null || SelectedTab == null || DocumentTabs.Count <= 1) return;
        
        var currentTab = SelectedTab;
        if (currentTab == null || !DocumentTabs.Contains(currentTab)) return;

        // 移除其他可关闭的标签页
        var tabsToClose = DocumentTabs
            .Where(t => t != currentTab && t.CanClose)
            .ToList();
        
        foreach (var tab in tabsToClose)
        {
            DocumentTabs.Remove(tab);
        }
        
        // 确保当前标签被选中
        if (DocumentTabs.Contains(currentTab))
        {
            SelectedTab = currentTab;
        }
    }

    /// <summary>
    /// 关闭所有标签（标准 MDI 实现）
    /// </summary>
    [RelayCommand]
    public void CloseAllTabs()
    {
        if (DocumentTabs == null) return;
        
        // 先取消选中
        SelectedTab = null;
        
        // 移除所有可关闭的标签页
        var tabsToRemove = DocumentTabs.Where(t => t.CanClose).ToList();
        foreach (var tab in tabsToRemove)
        {
            DocumentTabs.Remove(tab);
        }
        
        // 选择要保留的标签页（如果有不可关闭的）
        var nonCloseableTabs = DocumentTabs.Where(t => !t.CanClose).ToList();
        if (nonCloseableTabs.Count > 0)
        {
            SelectedTab = nonCloseableTabs[0];
        }
        else if (DocumentTabs.Count == 0)
        {
            SelectedTab = null;
        }
    }

    public async Task LoadMenusAsync(long userId)
    {
        try
        {
            _currentUserId = userId;
            
            if (userId > 0)
            {
                var result = await _menuService.GetUserMenuTreeAsync(userId);
                if (result.Success && result.Data != null)
                {
                    Menus = result.Data.Menus;
                }
                else if (!result.Success && !string.IsNullOrWhiteSpace(result.Message))
                {
                    // 如果操作失败，检查是否是数据库连接错误
                    var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
                    operLog?.Error("[菜单] 加载用户菜单失败: {Message}", result.Message);
                    
                    // 如果是数据库连接错误，显示消息框提示用户
                    if (result.Message.Contains("数据库") || result.Message.Contains("连接") || 
                        result.Message.Contains("SqlSugar") || result.Message.Contains("Connection"))
                    {
                        WpfApp.Current.Dispatcher.Invoke(() =>
                        {
                            TaktMessageBox.Error(
                                $"加载菜单失败：{result.Message}\n\n请检查数据库连接是否正常。",
                                _localizationManager.GetString("common.messagebox.error"));
                        });
                    }
                    Menus = new List<MenuDto>();
                }
            }
            else
            {
                // 如果用户ID为0，加载所有菜单（管理员模式或初始化）
                var result = await _menuService.GetAllMenuTreeAsync();
                if (result.Success && result.Data != null)
                {
                    Menus = result.Data;
                }
                else if (!result.Success && !string.IsNullOrWhiteSpace(result.Message))
                {
                    // 如果操作失败，检查是否是数据库连接错误
                    var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
                    operLog?.Error("[菜单] 加载所有菜单失败: {Message}", result.Message);
                    
                    // 如果是数据库连接错误，显示消息框提示用户
                    if (result.Message.Contains("数据库") || result.Message.Contains("连接") || 
                        result.Message.Contains("SqlSugar") || result.Message.Contains("Connection"))
                    {
                        WpfApp.Current.Dispatcher.Invoke(() =>
                        {
                            TaktMessageBox.Error(
                                $"加载菜单失败：{result.Message}\n\n请检查数据库连接是否正常。",
                                _localizationManager.GetString("common.messagebox.error"));
                        });
                    }
                    Menus = new List<MenuDto>();
                }
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[菜单] 加载菜单失败");
            
            // 检查是否是数据库连接错误
            var errorMessage = ex.Message;
            if (errorMessage.Contains("数据库") || errorMessage.Contains("连接") || 
                errorMessage.Contains("SqlSugar") || errorMessage.Contains("Connection"))
            {
                WpfApp.Current.Dispatcher.Invoke(() =>
                {
                    TaktMessageBox.Error(
                        $"加载菜单失败：{errorMessage}\n\n请检查数据库连接是否正常。",
                        _localizationManager.GetString("common.messagebox.error"));
                });
            }
            
            Menus = new List<MenuDto>();
        }
    }

    partial void OnSelectedMenuChanged(MenuDto? value)
    {
        // 菜单选择变更时，可以在这里触发导航
        // 导航逻辑将在 MainWindow.xaml.cs 中处理
    }

    /// <summary>
    /// 导航到指定菜单（供 MainWindow.xaml.cs 调用）
    /// </summary>
    public void NavigateToMenu(MenuDto? menuItem)
    {
        if (menuItem == null)
        {
            return;
        }

        _navigationService?.NavigateToMenu(menuItem);
    }

    /// <summary>
    /// 选中标签页变更时的处理
    /// 按照推荐实现：不需要额外处理，WPF 的绑定会自动处理
    /// </summary>
    partial void OnSelectedTabChanged(DocumentTabItem? value)
    {
        // 这个部分方法会在设置 SelectedTab 后立即调用
        // 但由于我们在 CreateOrActivateTabCore 中已经使用了延迟设置 SelectedTab
        // 所以这里通常 value 应该已经是完全初始化的 TabItem
        // 如果仍然出现问题，可以在这里添加额外的延迟处理或异常捕获
    }

    /// <summary>
    /// 显示用户信息命令
    /// </summary>
    [RelayCommand]
    public void ShowUserInfo()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
            if (mainWindow != null)
            {
                var userInfoWindow = App.Services?.GetService<Views.Identity.UserComponent.UserProfile>();
                if (userInfoWindow != null)
                {
                    userInfoWindow.Owner = mainWindow;
                    userInfoWindow.ShowDialog();
                }
            }
        });
    }

    /// <summary>
    /// 登出命令（带确认对话框）
    /// </summary>
    [RelayCommand]
    public void Logout()
    {
        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
        operLog?.Information("[登出] 开始登出流程：当前用户={Username}, 用户ID={UserId}, 真实姓名={RealName}", 
            CurrentUsername, _currentUserId, CurrentRealName);
        
        try
        {
            // 显示确认对话框（使用拼接方式）
            operLog?.Debug("[登出] 显示确认对话框");
            var localizationManager = App.Services?.GetService<Takt.Domain.Interfaces.ILocalizationManager>();
            if (localizationManager != null)
            {
                // "退出登录" = "退出" + "登录"
                var logoutText = localizationManager.GetString("common.button.exit") + localizationManager.GetString("common.button.login");
                // "确定要退出登录吗？" = 使用通用确认消息格式
                var confirmMessage = string.Format(localizationManager.GetString("common.confirm.message"), logoutText);
                // "确认登出" = "确认" + "退出登录"
                var confirmTitle = localizationManager.GetString("common.button.confirm") + logoutText;
                
                var result = TaktMessageManager.ShowMessageBox(
                    confirmMessage,
                    confirmTitle,
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo);
                
                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    operLog?.Information("[登出] 用户取消登出");
                    return;
                }
            }
            else
            {
                // 如果 localizationManager 为空，使用默认值
                var result = TaktMessageManager.ShowMessageBox(
                    "确定要退出登录吗？",
                    "确认登出",
                    MessageBoxImage.Question,
                    MessageBoxButton.YesNo);
                
                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    operLog?.Information("[登出] 用户取消登出");
                    return;
                }
            }

            operLog?.Information("[登出] 用户确认登出，开始执行登出操作");

            // 清除用户上下文
            var userContext = UserContext.Current;
            if (userContext.IsAuthenticated)
            {
                var userId = userContext.UserId;
                operLog?.Debug("[登出] 清除用户上下文：UserId={UserId}, IsAuthenticated={IsAuthenticated}", 
                    userId, userContext.IsAuthenticated);
                UserContext.RemoveUser(userId);
                userContext.Clear();
                operLog?.Debug("[登出] 用户上下文已清除：UserId={UserId}", userId);
            }
            else
            {
                operLog?.Debug("[登出] 用户上下文未认证，跳过清除");
            }

            // 清除当前用户信息
            var oldUsername = CurrentUsername;
            var oldRealName = CurrentRealName;
            var oldRoleName = CurrentRoleName;
            var oldUserId = _currentUserId;
            
            CurrentUsername = string.Empty;
            CurrentRealName = string.Empty;
            CurrentRoleName = string.Empty;
            CurrentAvatar = string.Empty;
            _currentUserId = 0;
            
            operLog?.Debug("[登出] 已清除当前用户信息：Username={Username}, RealName={RealName}, RoleName={RoleName}, UserId={UserId}", 
                oldUsername, oldRealName, oldRoleName, oldUserId);

            // 关闭主窗口并清除标签页（在 UI 线程中执行，避免清除时 IconBlock 属性错误）
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    operLog?.Debug("[登出] 开始关闭主窗口和清除标签页");
                    
                    // 关闭所有标签页（先清除选中项，再逐个移除，避免直接 Clear 导致的属性重置问题）
                    var selectedTabTitle = SelectedTab?.Title ?? "无";
                    SelectedTab = null;
                    operLog?.Debug("[登出] 已清除选中标签：Title={Title}", selectedTabTitle);
                    
                    // 逐个移除标签页，避免直接 Clear 导致的 IconBlock IconFont 属性错误
                    var tabsToRemove = DocumentTabs.ToList();
                    operLog?.Debug("[登出] 开始移除标签页：数量={Count}", tabsToRemove.Count);
                    foreach (var tab in tabsToRemove)
                    {
                        operLog?.Debug("[登出] 移除标签页：Title={Title}, ViewTypeName={ViewTypeName}", 
                            tab.Title, tab.ViewTypeName);
                        DocumentTabs.Remove(tab);
                    }
                    operLog?.Debug("[登出] 标签页移除完成：剩余数量={Count}", DocumentTabs.Count);


                    var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
                    if (mainWindow != null)
                    {
                        operLog?.Debug("[登出] 开始关闭主窗口");
                        mainWindow.Close();
                        operLog?.Information("[登出] 主窗口已关闭");
                    }
                    else
                    {
                        operLog?.Warning("[登出] 主窗口为 null，无法关闭");
                    }

                    // 重新打开登录窗口
                    // 重要：WPF 窗口关闭后无法再次调用 Show()，必须创建新实例
                    // 从依赖注入获取的 LoginView 可能是已关闭的实例，不能直接重用
                    operLog?.Debug("[登出] 开始查找或创建登录窗口");
                    Views.Identity.LoginView? loginWindow = null;
                    
                    // 检查是否有已存在的未关闭的登录窗口（隐藏但未关闭）
                    bool foundExisting = false;
                    int existingWindowsCount = 0;
                    foreach (Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window is Views.Identity.LoginView existingWindow)
                        {
                            existingWindowsCount++;
                            try
                            {
                                // 检查窗口是否仍可用（通过检查 Visibility 属性）
                                // 如果窗口已关闭，访问 Visibility 会抛出异常
                                var visibility = existingWindow.Visibility;
                                if (existingWindow.IsLoaded && visibility != Visibility.Collapsed)
                                {
                                    loginWindow = existingWindow;
                                    foundExisting = true;
                                    operLog?.Debug("[登出] 找到可重用的登录窗口：IsLoaded={IsLoaded}, Visibility={Visibility}", 
                                        existingWindow.IsLoaded, visibility);
                                    break;
                                }
                                else
                                {
                                    operLog?.Debug("[登出] 登录窗口不可用：IsLoaded={IsLoaded}, Visibility={Visibility}", 
                                        existingWindow.IsLoaded, visibility);
                                }
                            }
                            catch (Exception ex)
                            {
                                // 窗口已关闭，继续查找或创建新实例
                                operLog?.Error(ex, "[登出] 检查登录窗口时发生异常（窗口已关闭）：Exception={Exception}", ex.Message ?? string.Empty);
                                continue;
                            }
                        }
                    }
                    
                    operLog?.Debug("[登出] 登录窗口检查完成：找到窗口数={Count}, 可重用={FoundExisting}", 
                        existingWindowsCount, foundExisting);
                    
                    // 如果没有找到可用的登录窗口，通过容器解析实例
                    if (!foundExisting)
                    {
                        operLog?.Debug("[登出] 通过容器解析登录窗口实例");
                        if (App.Services != null)
                        {
                            loginWindow = App.Services.GetRequiredService<Views.Identity.LoginView>();
                        }
                        else
                        {
                            operLog?.Warning("[登出] App.Services 为 null，无法解析 LoginView");
                            throw new InvalidOperationException("App.Services 为 null，无法解析 LoginView");
                        }
                    }
                    
                    // 确保 loginWindow 已初始化
                    if (loginWindow == null)
                    {
                        operLog?.Warning("[登出] loginWindow 为 null，尝试通过容器重新解析");
                        if (App.Services != null)
                        {
                            loginWindow = App.Services.GetRequiredService<Views.Identity.LoginView>();
                        }
                        else
                        {
                            throw new InvalidOperationException("App.Services 为 null，无法解析 LoginView");
                        }
                    }
                    
                    // 显示登录窗口
                    // 重要：WPF 窗口关闭后无法再次调用 Show()，会抛出 InvalidOperationException
                    // 如果窗口已关闭，需要创建新实例
                    operLog?.Debug("[登出] 显示登录窗口");
                    try
                    {
                        loginWindow.Show();
                        System.Windows.Application.Current.MainWindow = loginWindow;
                        operLog?.Information("[登出] 登录窗口已显示，登出流程完成：Username={Username}, UserId={UserId}", 
                            oldUsername, oldUserId);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // 窗口已关闭，无法显示，需要创建新实例
                        operLog?.Error(ex, "[登出] 无法显示登录窗口（窗口已关闭），创建新实例");
                        
                        // 由于 LoginView 是 Singleton，容器会返回同一个已关闭的实例
                        // 我们需要手动创建新实例
                        if (App.Services != null)
                        {
                            loginWindow = new Views.Identity.LoginView(
                                App.Services.GetRequiredService<Services.ThemeService>(),
                                App.Services.GetRequiredService<Domain.Interfaces.ILocalizationManager>(),
                                App.Services.GetRequiredService<Application.Services.Routine.ILanguageService>(),
                                App.Services.GetRequiredService<ViewModels.Identity.LoginViewModel>()
                            );
                            loginWindow.Show();
                            System.Windows.Application.Current.MainWindow = loginWindow;
                            operLog?.Information("[登出] 已创建并显示新的登录窗口实例");
                        }
                        else
                        {
                            throw new InvalidOperationException("App.Services 为 null，无法创建新的 LoginView 实例", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    operLog?.Error(ex, "[登出] 关闭主窗口或显示登录窗口时发生异常");
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[登出] 登出流程发生异常：Username={Username}, UserId={UserId}", 
                CurrentUsername, _currentUserId);
            throw;
        }
    }
}

