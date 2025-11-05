//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : MainWindowViewModel.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-29
// 版本号 : 1.0
// 描述    : 主窗口视图模型
//===================================================================

using AvalonDock.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hbt.Application.Dtos.Identity;
using Hbt.Application.Services.Identity;
using Hbt.Common.Context;
using Hbt.Fluent.Models;
using Hbt.Fluent.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hbt.Fluent.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "黑冰台管理系统";

    [ObservableProperty]
    private List<MenuDto> _menus = new();

    [ObservableProperty]
    private MenuDto? _selectedMenu;

    [ObservableProperty]
    private bool _canNavigateback;

    /// <summary>
    /// 文档标签页集合（保留以兼容旧代码，逐步迁移）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DocumentTabItem> _documentTabs = new();

    /// <summary>
    /// 当前选中的标签页（保留以兼容旧代码）
    /// </summary>
    [ObservableProperty]
    private DocumentTabItem? _selectedTab;

    /// <summary>
    /// AvalonDock 文档集合
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<AvalonDocument> _documents = new();
    
    /// <summary>
    /// 初始化 Documents 集合，订阅集合变更事件以处理文档关闭
    /// </summary>
    private void InitializeDocuments()
    {
        // 订阅集合变更事件，监听文档移除（用于处理关闭）
        // 注意：使用 Documents 属性而不是 _documents 字段（因为 ObservableProperty 会生成属性）
        Documents.CollectionChanged += (sender, e) =>
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is AvalonDocument doc)
                    {
                        operLog?.Debug("[AvalonDock] 集合变更检测到文档移除：Title={Title}, ViewTypeName={ViewTypeName}, ContentId={ContentId}", 
                            doc.Title, doc.ViewTypeName, doc.ContentId);
                        // 确保活动文档引用被清除
                        if (ActiveDocument == doc)
                        {
                            ActiveDocument = null;
                            operLog?.Debug("[AvalonDock] 已清除活动文档引用（集合变更）：{ViewTypeName}", doc.ViewTypeName);
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// AvalonDock 当前活动文档
    /// </summary>
    [ObservableProperty]
    private LayoutContent? _activeDocument;

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
    /// 当前活动文档标题（用于状态栏显示）
    /// </summary>
    [ObservableProperty]
    private string _activeDocumentTitle = string.Empty;

    private readonly LanguageService? _languageService;

    private readonly IMenuService _menuService;
    private long _currentUserId;

    [RelayCommand]
    public void Settings()
    {
        // 从菜单列表中查找"系统设置"菜单项（菜单代码：setting_management）
        var settingsMenu = Menus
            .SelectMany(m => FlattenMenu(m))
            .FirstOrDefault(m => m.MenuCode == "setting_management");

        if (settingsMenu == null)
        {
            // 如果找不到，尝试从所有菜单中查找（包括子菜单）
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Warning("[菜单] 未找到系统设置菜单项");
            return;
        }

        // 获取 MainWindow 并导航到菜单项
        var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.NavigateToMenu(settingsMenu);
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

    public MainWindowViewModel(IMenuService menuService, LanguageService? languageService = null)
    {
        _menuService = menuService;
        _languageService = languageService ?? App.Services?.GetService<LanguageService>();
        
        // 初始化 Documents 集合监听
        InitializeDocuments();
        
        _ = LoadMenusAsync(0); // 默认加载，实际应传入当前登录用户ID
        
        // 加载当前用户信息
        LoadCurrentUserInfo();
        
        // 监听 ActiveDocument 变化，更新活动文档标题
        PropertyChanged += MainWindowViewModel_PropertyChanged;
    }
    
    private void MainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActiveDocument))
        {
            UpdateActiveDocumentTitle();
        }
    }
    
    /// <summary>
    /// 更新当前活动文档标题
    /// </summary>
    private void UpdateActiveDocumentTitle()
    {
        if (ActiveDocument is AvalonDocument doc)
        {
            ActiveDocumentTitle = doc.Title ?? string.Empty;
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
        
        // 获取语言服务（与转换器相同的获取方式）
        var languageService = _languageService ?? App.Services?.GetService(typeof(LanguageService)) as LanguageService;
        if (languageService == null)
        {
            // 如果语言服务不可用，返回 key 本身（与转换器逻辑一致）
            return key;
        }
        
        // 与转换器完全一致：GetTranslation(key, key)
        // 如果找到翻译，返回翻译；如果找不到，返回 key 本身
        return languageService.GetTranslation(key, key);
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
        }
    }

    /// <summary>
    /// 添加或激活标签页（AvalonDock 版本）
    /// </summary>
    public AvalonDocument AddOrActivateDocument(MenuDto menuItem, object content, string viewTypeName)
    {
        if (menuItem == null)
        {
            throw new ArgumentNullException(nameof(menuItem));
        }
        
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }
        
        if (string.IsNullOrEmpty(viewTypeName))
        {
            throw new ArgumentException("视图类型名称不能为空", nameof(viewTypeName));
        }

        // 在 UI 线程中执行，确保绑定和控件正确初始化
        if (System.Windows.Application.Current?.Dispatcher == null)
        {
            throw new InvalidOperationException("Application.Current 或 Dispatcher 为 null，无法执行 UI 操作");
        }

        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            return AddOrActivateDocumentCore(menuItem, content, viewTypeName);
        }
        else
        {
            AvalonDocument? result = null;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                result = AddOrActivateDocumentCore(menuItem, content, viewTypeName);
            });
            return result ?? throw new InvalidOperationException("添加文档失败，返回 null");
        }
    }

    /// <summary>
    /// 添加或激活文档的核心实现（AvalonDock 版本）
    /// </summary>
    private AvalonDocument AddOrActivateDocumentCore(MenuDto menuItem, object content, string viewTypeName)
    {
        var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
        try
        {
            // 参数验证
            if (menuItem == null) 
                throw new ArgumentNullException(nameof(menuItem));
            if (content == null) 
                throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrEmpty(viewTypeName)) 
                throw new ArgumentException("视图类型名称不能为空", nameof(viewTypeName));
            if (Documents == null)
                throw new InvalidOperationException("Documents 集合未初始化");

            operLog?.Debug("[AvalonDock] AddOrActivateDocumentCore 开始：ViewTypeName={ViewTypeName}, MenuCode={MenuCode}, Documents.Count={Count}", 
                viewTypeName, menuItem.MenuCode, Documents.Count);

            // 确定 ViewTypeName（用于判断是否已打开）
            // 优先使用 Component，如果没有则使用基于 RoutePath 生成的类型名称
            string actualViewTypeName = viewTypeName;
            if (!string.IsNullOrEmpty(menuItem.Component))
            {
                // 使用 Component 作为唯一标识
                actualViewTypeName = menuItem.Component;
            }
            else if (!string.IsNullOrEmpty(menuItem.RoutePath))
            {
                // 使用 RoutePath 生成的类型名称作为唯一标识
                actualViewTypeName = viewTypeName;
            }
            
            operLog?.Debug("[AvalonDock] ViewTypeName: Component={Component}, RoutePath={RoutePath}, ActualViewTypeName={ActualViewTypeName}", 
                menuItem.Component ?? "null", menuItem.RoutePath ?? "null", actualViewTypeName);

            // 检查文档是否已存在（基于 ViewTypeName 和 ContentId）
            // 注意：只检查集合中确实存在的文档，忽略已关闭但对象仍存在的文档
            AvalonDocument? existingDoc = null;
            // 使用 ToList() 创建副本，避免在遍历时集合被修改
            var documentsList = Documents.ToList();
            foreach (var doc in documentsList)
            {
                if (doc != null)
                {
                    // 检查 ViewTypeName 是否匹配
                    bool viewTypeNameMatch = doc.ViewTypeName == actualViewTypeName;
                    // 也检查 ContentId 是否匹配（作为备用标识）
                    bool contentIdMatch = doc.ContentId == actualViewTypeName;
                    // 检查文档是否仍在集合中（确保未被关闭）
                    bool isInCollection = Documents.Contains(doc);
                    
                    if ((viewTypeNameMatch || contentIdMatch) && isInCollection)
                    {
                        existingDoc = doc;
                        operLog?.Debug("[AvalonDock] 找到已存在的文档：ViewTypeName={ViewTypeName}, ContentId={ContentId}, Title={Title}, IsInCollection={IsInCollection}", 
                            doc.ViewTypeName, doc.ContentId, doc.Title, isInCollection);
                        break;
                    }
                }
            }
            
            if (existingDoc != null)
            {
                // 已存在，验证 Content 是否正确
                operLog?.Debug("[AvalonDock] 文档已存在，检查 Content：ViewTypeName={ViewTypeName}, Content={ContentType}, ContentIsNull={ContentIsNull}", 
                    actualViewTypeName, existingDoc.Content?.GetType().FullName ?? "null", existingDoc.Content == null);
                
                // 如果 Content 为空或类型不匹配，需要更新 Content
                if (existingDoc.Content == null || existingDoc.Content.GetType().FullName != content.GetType().FullName)
                {
                    operLog?.Warning("[AvalonDock] 已存在的文档 Content 不正确，更新 Content：OldContent={OldContent}, NewContent={NewContent}", 
                        existingDoc.Content?.GetType().FullName ?? "null", content.GetType().FullName);
                    
                    // 更新 Content（确保传入的 content 是 UIElement）
                    if (content is System.Windows.UIElement newContentElement)
                    {
                        // 直接设置新的 Content（AvalonDock 会自动处理旧内容的移除）
                        existingDoc.Content = newContentElement;
                        operLog?.Debug("[AvalonDock] Content 已更新：ContentType={ContentType}", newContentElement.GetType().FullName);
                    }
                }
                
                // 激活文档
                operLog?.Debug("[AvalonDock] 文档已存在，激活：{ViewTypeName} (Component={Component}, RoutePath={RoutePath}), Documents.Count={Count}", 
                    actualViewTypeName, menuItem.Component ?? "null", menuItem.RoutePath ?? "null", Documents.Count);
                ActiveDocument = existingDoc;
                existingDoc.IsSelected = true;
                return existingDoc;
            }
            
            operLog?.Debug("[AvalonDock] 文档不存在，将创建新文档：{ViewTypeName} (Component={Component}, RoutePath={RoutePath}), Documents.Count={Count}", 
                actualViewTypeName, menuItem.Component ?? "null", menuItem.RoutePath ?? "null", Documents.Count);

            // 获取本地化的标题（使用菜单的实际 I18nKey）
            string title = GetLocalizedTitle(menuItem);
            
            // 验证标题不为空
            if (string.IsNullOrWhiteSpace(title))
            {
                // 如果标题为空，使用备用值
                title = menuItem.MenuName ?? menuItem.MenuCode ?? "未命名";
                operLog?.Warning("[AvalonDock] 本地化标题为空，使用备用标题：{Title}, I18nKey={I18nKey}", 
                    title, menuItem.I18nKey ?? "null");
            }
            
            operLog?.Debug("[AvalonDock] 获取本地化标题: I18nKey={I18nKey}, Title={Title}, MenuName={MenuName}, MenuCode={MenuCode}", 
                menuItem.I18nKey ?? "null", title, menuItem.MenuName ?? "null", menuItem.MenuCode ?? "null");

            // 验证 content 是 UIElement
            if (content is not System.Windows.UIElement uiElement)
            {
                operLog?.Error("[AvalonDock] Content 不是 UIElement：ContentType={ContentType}", 
                    content?.GetType().FullName ?? "null");
                throw new ArgumentException($"Content 必须是 UIElement，实际类型：{content?.GetType().FullName ?? "null"}", nameof(content));
            }
            
            operLog?.Debug("[AvalonDock] Content 验证通过：ContentType={ContentType}, IsUIElement=True", 
                content.GetType().FullName);
            
            // 确保 content (UIElement) 已初始化（如果它需要初始化）
            if (content is System.Windows.Controls.UserControl userControl)
            {
                // 确保 UserControl 已加载
                // UserControl 的 InitializeComponent 通常已在构造函数中调用
                // 但如果视图有特定的初始化逻辑，可能需要调用 Loaded 事件
                operLog?.Debug("[AvalonDock] Content 是 UserControl：{TypeName}", userControl.GetType().FullName);
            }
            
            // 创建 DocumentTabItem（用于数据存储，使用确定的 ViewTypeName）
            var tabItem = new DocumentTabItem(menuItem, title, content, actualViewTypeName);
            // 使用菜单的图标（如果存在）
            if (!string.IsNullOrEmpty(menuItem.Icon))
            {
                tabItem.Icon = menuItem.Icon;
            }
            
            // 创建 AvalonDocument
            var newDoc = new AvalonDocument(tabItem);
            
            // 验证图标是否已设置（在构造函数中设置）
            if (!string.IsNullOrEmpty(tabItem.Icon) && newDoc.IconSource == null)
            {
                operLog?.Warning("[AvalonDock] 警告：文档创建后 IconSource 为 null，Icon={Icon}", tabItem.Icon);
                // 尝试重新设置图标
                try
                {
                    var iconSource = Models.AvalonDocument.ConvertIconToImageSourceStatic(tabItem.Icon);
                    if (iconSource != null)
                    {
                        newDoc.IconSource = iconSource;
                        operLog?.Debug("[AvalonDock] IconSource 已重新设置（文档创建后）: Icon={Icon}, IconSource={IconSourceType}", 
                            tabItem.Icon, iconSource.GetType().Name);
                    }
                }
                catch (Exception ex)
                {
                    operLog?.Error(ex, "[AvalonDock] 重新设置 IconSource 失败: Icon={Icon}", tabItem.Icon);
                }
            }
            
            // 验证标题已正确设置，如果为空则强制设置（官方标准方法）
            if (string.IsNullOrWhiteSpace(newDoc.Title))
            {
                operLog?.Error("[AvalonDock] 创建的文档标题为空！TabItem.Title={TabItemTitle}, MenuName={MenuName}, MenuCode={MenuCode}", 
                    tabItem.Title, menuItem.MenuName, menuItem.MenuCode);
                // 官方标准方法：直接设置 Title 属性
                var fallbackTitle = menuItem.MenuName ?? menuItem.MenuCode ?? "未命名";
                newDoc.Title = fallbackTitle;
                // 同时更新 TabItem 的标题
                tabItem.Title = fallbackTitle;
                operLog?.Warning("[AvalonDock] 已强制设置备用标题：{FallbackTitle}", fallbackTitle);
            }
            
            // 再次验证标题
            if (string.IsNullOrWhiteSpace(newDoc.Title))
            {
                throw new InvalidOperationException($"无法设置文档标题：TabItem.Title={tabItem.Title}, MenuName={menuItem.MenuName}, MenuCode={menuItem.MenuCode}");
            }
            
            operLog?.Debug("[AvalonDock] 文档创建完成：Title={Title}, TabItem.Title={TabItemTitle}, ViewTypeName={ViewTypeName}, ContentId={ContentId}", 
                newDoc.Title, tabItem.Title, actualViewTypeName, newDoc.ContentId);
            
            // 再次验证 Content 设置
            if (newDoc.Content == null)
            {
                throw new InvalidOperationException($"AvalonDocument.Content 为 null，无法显示内容");
            }
            
            if (newDoc.Content is not System.Windows.UIElement)
            {
                throw new InvalidOperationException(
                    $"AvalonDocument.Content 不是 UIElement：{newDoc.Content.GetType().FullName}");
            }
            
            // 验证创建成功
            if (newDoc == null)
                throw new InvalidOperationException("创建文档失败，返回 null");
            
            // 验证 Content 属性已正确设置
            if (newDoc.Content != content)
            {
                operLog?.Warning("[AvalonDock] Content 属性未正确设置：Expected={ExpectedType}, Actual={ActualType}", 
                    content.GetType().FullName, newDoc.Content?.GetType().FullName ?? "null");
            }
            else
            {
                operLog?.Debug("[AvalonDock] Content 属性已正确设置：ContentType={ContentType}", 
                    newDoc.Content?.GetType().FullName ?? "null");
            }
            
            // 验证 Content 在添加到集合前是否正确
            string? contentParentInfo = null;
            if (newDoc.Content is System.Windows.UIElement contentElement)
            {
                try
                {
                    // 使用 VisualTreeHelper 获取父元素
                    var parent = System.Windows.Media.VisualTreeHelper.GetParent(contentElement);
                    contentParentInfo = parent?.GetType().FullName ?? "null";
                }
                catch
                {
                    contentParentInfo = "无法获取";
                }
            }
            
            operLog?.Debug("[AvalonDock] 添加前验证：Content={ContentType}, IsUIElement={IsUIElement}, ContentParent={ContentParent}", 
                newDoc.Content?.GetType().FullName ?? "null",
                newDoc.Content is System.Windows.UIElement,
                contentParentInfo ?? "null");
            
            // 添加到集合（确保在UI线程上执行）
            if (System.Windows.Application.Current?.Dispatcher != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Documents.Add(newDoc);
                    // 确保 Title 在添加到集合后仍然正确（使用 SetValue 强制刷新）
                    if (!string.IsNullOrWhiteSpace(tabItem.Title))
                    {
                        newDoc.SetValue(AvalonDock.Layout.LayoutDocument.TitleProperty, tabItem.Title);
                    }
                    // 确保 IconSource 在添加到集合后仍然正确（在 UI 线程上设置，确保绑定生效）
                    if (!string.IsNullOrEmpty(tabItem.Icon))
                    {
                        try
                        {
                            var iconSource = Models.AvalonDocument.ConvertIconToImageSourceStatic(tabItem.Icon);
                            if (iconSource != null)
                            {
                                // 尝试使用 SetCurrentValue 强制更新绑定（如果 IconSource 是依赖属性）
                                try
                                {
                                    var iconSourceProp = typeof(AvalonDock.Layout.LayoutContent).GetProperty("IconSourceProperty", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                                    if (iconSourceProp != null && iconSourceProp.GetValue(null) is System.Windows.DependencyProperty dp)
                                    {
                                        newDoc.SetCurrentValue(dp, iconSource);
                                        operLog?.Debug("[AvalonDock] IconSource 已更新（使用 SetCurrentValue，添加到集合后）: Icon={Icon}, IconSource={IconSourceType}, IconSourceIsNull={IconSourceIsNull}", 
                                            tabItem.Icon, iconSource.GetType().Name, newDoc.IconSource == null);
                                    }
                                    else
                                    {
                                        newDoc.IconSource = iconSource;
                                        operLog?.Debug("[AvalonDock] IconSource 已更新（直接赋值，添加到集合后）: Icon={Icon}, IconSource={IconSourceType}, IconSourceIsNull={IconSourceIsNull}", 
                                            tabItem.Icon, iconSource.GetType().Name, newDoc.IconSource == null);
                                    }
                                }
                                catch (Exception setEx)
                                {
                                    newDoc.IconSource = iconSource;
                                    operLog?.Debug("[AvalonDock] IconSource 已更新（反射失败，直接赋值，添加到集合后）: Icon={Icon}, IconSource={IconSourceType}, IconSourceIsNull={IconSourceIsNull}, Exception={Exception}", 
                                        tabItem.Icon, iconSource.GetType().Name, newDoc.IconSource == null, setEx.Message);
                                }
                                
                                // 验证 IconSource 是否真的设置成功
                                if (newDoc.IconSource == null)
                                {
                                    operLog?.Warning("[AvalonDock] 警告：IconSource 设置后仍为 null！");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[AvalonDock] IconSource 验证: Icon={tabItem.Icon}, IconSource={newDoc.IconSource.GetType().Name}, Width={newDoc.IconSource.Width}, Height={newDoc.IconSource.Height}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            operLog?.Error(ex, "[AvalonDock] IconSource 设置失败（添加到集合后）: Icon={Icon}", tabItem.Icon);
                        }
                    }
                });
            }
            else
            {
                Documents.Add(newDoc);
                // 确保 Title 在添加到集合后仍然正确（使用 SetValue 强制刷新）
                if (!string.IsNullOrWhiteSpace(tabItem.Title))
                {
                    newDoc.SetValue(AvalonDock.Layout.LayoutDocument.TitleProperty, tabItem.Title);
                }
                // 确保 IconSource 在添加到集合后仍然正确（延迟设置，确保绑定已建立）
                if (!string.IsNullOrEmpty(tabItem.Icon))
                {
                    try
                    {
                        var iconSource = Models.AvalonDocument.ConvertIconToImageSourceStatic(tabItem.Icon);
                        if (iconSource != null)
                        {
                            // 尝试使用 SetCurrentValue 强制更新绑定（如果 IconSource 是依赖属性）
                            // 否则直接赋值
                            try
                            {
                                // 检查 IconSourceProperty 是否存在
                                var iconSourceProp = typeof(AvalonDock.Layout.LayoutContent).GetProperty("IconSourceProperty", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                                if (iconSourceProp != null && iconSourceProp.GetValue(null) is System.Windows.DependencyProperty dp)
                                {
                                    // 使用 SetCurrentValue 强制更新绑定，不覆盖本地值
                                    newDoc.SetCurrentValue(dp, iconSource);
                                    operLog?.Debug("[AvalonDock] IconSource 已更新（使用 SetCurrentValue）: Icon={Icon}, IconSource={IconSourceType}, IconSourceIsNull={IconSourceIsNull}", 
                                        tabItem.Icon, iconSource.GetType().Name, newDoc.IconSource == null);
                                }
                                else
                                {
                                    // 直接设置 IconSource（如果 IconSourceProperty 不存在）
                                    newDoc.IconSource = iconSource;
                                    operLog?.Debug("[AvalonDock] IconSource 已更新（直接赋值）: Icon={Icon}, IconSource={IconSourceType}, IconSourceIsNull={IconSourceIsNull}", 
                                        tabItem.Icon, iconSource.GetType().Name, newDoc.IconSource == null);
                                }
                            }
                            catch (Exception setEx)
                            {
                                // 如果反射失败，直接赋值
                                newDoc.IconSource = iconSource;
                                operLog?.Debug("[AvalonDock] IconSource 已更新（反射失败，直接赋值）: Icon={Icon}, IconSource={IconSourceType}, IconSourceIsNull={IconSourceIsNull}, Exception={Exception}", 
                                    tabItem.Icon, iconSource.GetType().Name, newDoc.IconSource == null, setEx.Message);
                            }
                            
                            // 验证 IconSource 是否真的设置成功
                            if (newDoc.IconSource == null)
                            {
                                operLog?.Warning("[AvalonDock] 警告：IconSource 设置后仍为 null！");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[AvalonDock] IconSource 验证: Icon={tabItem.Icon}, IconSource={newDoc.IconSource.GetType().Name}, Width={newDoc.IconSource.Width}, Height={newDoc.IconSource.Height}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        operLog?.Error(ex, "[AvalonDock] IconSource 设置失败（添加到集合后）: Icon={Icon}", tabItem.Icon);
                    }
                }
            }
            
            operLog?.Debug("[AvalonDock] 文档已添加到集合：{ViewTypeName}, Title={Title}, Documents.Count={Count}, Content={ContentType}", 
                viewTypeName, newDoc.Title ?? "null", Documents.Count, newDoc.Content?.GetType().FullName ?? "null");
            
            // 设置为活动文档（确保在UI线程上执行）
            if (System.Windows.Application.Current?.Dispatcher != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Documents.Contains(newDoc))
                    {
                        // 再次确保 Title 正确（使用 SetValue 强制刷新）
                        if (!string.IsNullOrWhiteSpace(tabItem.Title))
                        {
                            newDoc.SetValue(AvalonDock.Layout.LayoutDocument.TitleProperty, tabItem.Title);
                        }
                        // 再次确保 IconSource 正确（延迟设置，确保绑定已建立）
                        if (!string.IsNullOrEmpty(tabItem.Icon))
                        {
                            try
                            {
                                var iconSource = Models.AvalonDocument.ConvertIconToImageSourceStatic(tabItem.Icon);
                                if (iconSource != null)
                                {
                                    // 尝试使用 SetCurrentValue 强制更新绑定（如果 IconSource 是依赖属性）
                                    try
                                    {
                                        var iconSourceProp = typeof(AvalonDock.Layout.LayoutContent).GetProperty("IconSourceProperty", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                                        if (iconSourceProp != null && iconSourceProp.GetValue(null) is System.Windows.DependencyProperty dp)
                                        {
                                            newDoc.SetCurrentValue(dp, iconSource);
                                            operLog?.Debug("[AvalonDock] IconSource 已更新（使用 SetCurrentValue，设置为活动文档后）: Icon={Icon}, IconSource={IconSourceType}", 
                                                tabItem.Icon, iconSource.GetType().Name);
                                        }
                                        else
                                        {
                                            newDoc.IconSource = iconSource;
                                            operLog?.Debug("[AvalonDock] IconSource 已更新（直接赋值，设置为活动文档后）: Icon={Icon}, IconSource={IconSourceType}", 
                                                tabItem.Icon, iconSource.GetType().Name);
                                        }
                                    }
                                    catch (Exception setEx)
                                    {
                                        newDoc.IconSource = iconSource;
                                        operLog?.Debug("[AvalonDock] IconSource 已更新（反射失败，直接赋值，设置为活动文档后）: Icon={Icon}, IconSource={IconSourceType}, Exception={Exception}", 
                                            tabItem.Icon, iconSource.GetType().Name, setEx.Message);
                                    }
                                    
                                    // 验证 IconSource 是否真的设置成功
                                    if (newDoc.IconSource == null)
                                    {
                                        operLog?.Warning("[AvalonDock] 警告：IconSource 设置后仍为 null！");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[AvalonDock] IconSource 验证: Icon={tabItem.Icon}, IconSource={newDoc.IconSource.GetType().Name}, Width={newDoc.IconSource.Width}, Height={newDoc.IconSource.Height}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                operLog?.Error(ex, "[AvalonDock] IconSource 设置失败（设置为活动文档后）: Icon={Icon}", tabItem.Icon);
                            }
                        }
                        ActiveDocument = newDoc;
                        newDoc.IsSelected = true;
                    }
                });
            }
            else
            {
                if (Documents.Contains(newDoc))
                {
                    // 再次确保 Title 正确（使用 SetValue 强制刷新）
                    if (!string.IsNullOrWhiteSpace(tabItem.Title))
                    {
                        newDoc.SetValue(AvalonDock.Layout.LayoutDocument.TitleProperty, tabItem.Title);
                    }
                    // 再次确保 IconSource 正确（在 UI 线程上设置）
                    if (!string.IsNullOrEmpty(tabItem.Icon))
                    {
                        try
                        {
                            var iconSource = Models.AvalonDocument.ConvertIconToImageSourceStatic(tabItem.Icon);
                            if (iconSource != null)
                            {
                                // 尝试使用 SetCurrentValue 强制更新绑定
                                try
                                {
                                    var iconSourceProp = typeof(AvalonDock.Layout.LayoutContent).GetProperty("IconSourceProperty", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                                    if (iconSourceProp != null && iconSourceProp.GetValue(null) is System.Windows.DependencyProperty dp)
                                    {
                                        // 延迟设置，确保 UI 刷新
                                        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                                            System.Windows.Threading.DispatcherPriority.Render,
                                            new Action(() =>
                                            {
                                                newDoc.SetCurrentValue(dp, iconSource);
                                                operLog?.Debug("[AvalonDock] IconSource 已更新（使用 SetCurrentValue，设置为活动文档后）: Icon={Icon}, IconSource={IconSourceType}", 
                                                    tabItem.Icon, iconSource.GetType().Name);
                                            }));
                                    }
                                    else
                                    {
                                        // 先清除旧的 IconSource，触发绑定更新
                                        newDoc.IconSource = null;
                                        // 延迟设置，确保 UI 刷新
                                        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                                            System.Windows.Threading.DispatcherPriority.Render,
                                            new Action(() =>
                                            {
                                                newDoc.IconSource = iconSource;
                                                operLog?.Debug("[AvalonDock] IconSource 已更新（直接赋值，设置为活动文档后）: Icon={Icon}, IconSource={IconSourceType}", 
                                                    tabItem.Icon, iconSource.GetType().Name);
                                            }));
                                    }
                                }
                                catch (Exception setEx)
                                {
                                    // 先清除旧的 IconSource，触发绑定更新
                                    newDoc.IconSource = null;
                                    // 延迟设置，确保 UI 刷新
                                    System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                                        System.Windows.Threading.DispatcherPriority.Render,
                                        new Action(() =>
                                        {
                                            newDoc.IconSource = iconSource;
                                            operLog?.Debug("[AvalonDock] IconSource 已更新（反射失败，直接赋值，设置为活动文档后）: Icon={Icon}, IconSource={IconSourceType}, Exception={Exception}", 
                                                tabItem.Icon, iconSource.GetType().Name, setEx.Message);
                                        }));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            operLog?.Error(ex, "[AvalonDock] IconSource 设置失败（设置为活动文档后）: Icon={Icon}", tabItem.Icon);
                        }
                    }
                    ActiveDocument = newDoc;
                    newDoc.IsSelected = true;
                }
            }
            
            if (Documents.Contains(newDoc))
            {
                operLog?.Debug("[AvalonDock] 文档已设置为活动：{ViewTypeName}, ActiveDocument={ActiveTitle}, ActiveContent={ActiveContentType}", 
                    viewTypeName, ActiveDocument?.Title ?? "null", ActiveDocument?.Content?.GetType().FullName ?? "null");
            }
            else
            {
                operLog?.Warning("[AvalonDock] 文档添加后不在集合中：{ViewTypeName}", viewTypeName);
            }
            
            return newDoc;
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[AvalonDock] 添加或激活文档时发生异常：ViewTypeName={ViewTypeName}", viewTypeName);
            throw;
        }
    }

    /// <summary>
    /// 添加或激活标签页（保留旧方法以兼容，内部调用 AvalonDock 版本）
    /// </summary>
    public DocumentTabItem AddOrActivateTab(MenuDto menuItem, object content, string viewTypeName)
    {
        // 调用 AvalonDock 版本，然后返回对应的 DocumentTabItem
        var doc = AddOrActivateDocument(menuItem, content, viewTypeName);
        return doc.TabItem;
    }

    /// <summary>
    /// 添加或激活标签页的核心实现（必须在 UI 线程中调用）
    /// 标准 MDI 实现：检查是否已打开，存在则激活，不存在则创建并激活
    /// </summary>
    private DocumentTabItem AddOrActivateTabCore(MenuDto menuItem, object content, string viewTypeName)
    {
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

            // 检查标签页是否已存在
            var existingTab = DocumentTabs.FirstOrDefault(t => t?.ViewTypeName == viewTypeName);
            if (existingTab != null && DocumentTabs.Contains(existingTab))
            {
                // 已存在，直接激活（确保选中项不为 null）
                SelectedTab = existingTab;
                return existingTab;
            }

            // 获取本地化的标题
            var titleKey = menuItem.I18nKey ?? menuItem.MenuCode ?? string.Empty;
            var title = _languageService?.GetTranslation(titleKey, menuItem.MenuName) ?? menuItem.MenuName ?? "未命名";

            // 创建新标签页
            var newTab = new DocumentTabItem(menuItem, title, content, viewTypeName);
            
            // 验证创建成功
            if (newTab == null)
                throw new InvalidOperationException("创建标签页失败，返回 null");
            
            // 添加到集合
            DocumentTabs.Add(newTab);
            
            // 直接设置为选中项（确保集合包含该项）
            if (DocumentTabs.Contains(newTab))
            {
                SelectedTab = newTab;
            }
            
            return newTab;
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[Tab] 添加或激活标签页时发生异常：ViewTypeName={ViewTypeName}", viewTypeName);
            throw;
        }
    }

    /// <summary>
    /// 关闭指定的文档（AvalonDock 版本）
    /// </summary>
    public void CloseDocument(AvalonDocument? document)
    {
        if (document == null) return;
        if (Documents == null) return;
        
        // 检查是否可以关闭（在检查集合之前，因为 AvalonDock 可能已经移除了）
        if (!document.CanClose)
        {
            System.Windows.MessageBox.Show(
                "默认仪表盘标签页不允许关闭。",
                "提示",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
        var viewTypeName = document.ViewTypeName;
        var title = document.Title;
        var contentId = document.ContentId;
        
        try
        {
            operLog?.Debug("[AvalonDock] 开始关闭文档：Title={Title}, ViewTypeName={ViewTypeName}, ContentId={ContentId}, Documents.Count={Count}", 
                title, viewTypeName, contentId, Documents.Count);
            
            // 如果关闭的是当前活动文档，先清除活动文档引用
            if (ActiveDocument == document)
            {
                ActiveDocument = null;
                operLog?.Debug("[AvalonDock] 已清除活动文档引用：{ViewTypeName}", viewTypeName);
            }
            
            // 检查文档是否仍在集合中（AvalonDock 可能已经自动移除）
            bool wasInCollection = Documents.Contains(document);
            operLog?.Debug("[AvalonDock] 文档是否在集合中：{WasInCollection}, ViewTypeName={ViewTypeName}", wasInCollection, viewTypeName);
            
            // 从集合中移除（如果仍在集合中）
            // 根据官方文档，从 Documents 集合移除后，AvalonDock 会自动从 LayoutDocumentPane 中移除
            // 注意：必须在释放资源之前移除，避免资源访问问题
            bool removed = false;
            if (wasInCollection)
            {
                // 确保在UI线程上移除
                if (System.Windows.Application.Current?.Dispatcher != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        removed = Documents.Remove(document);
                    });
                }
                else
                {
                    removed = Documents.Remove(document);
                }
                
                operLog?.Debug("[AvalonDock] 从集合中移除文档：Title={Title}, ViewTypeName={ViewTypeName}, ContentId={ContentId}, Removed={Removed}, RemainingCount={Count}", 
                    title, viewTypeName, contentId, removed, Documents.Count);
                
                // 验证文档已从集合中移除
                bool stillInCollection = false;
                if (System.Windows.Application.Current?.Dispatcher != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        stillInCollection = Documents.Contains(document);
                        if (stillInCollection)
                        {
                            // 再次尝试移除
                            Documents.Remove(document);
                            operLog?.Warning("[AvalonDock] 警告：文档移除后仍在集合中！ViewTypeName={ViewTypeName}，已再次尝试移除", viewTypeName);
                        }
                    });
                }
                else
                {
                    stillInCollection = Documents.Contains(document);
                    if (stillInCollection)
                    {
                        // 再次尝试移除
                        Documents.Remove(document);
                        operLog?.Warning("[AvalonDock] 警告：文档移除后仍在集合中！ViewTypeName={ViewTypeName}，已再次尝试移除", viewTypeName);
                    }
                }
            }
            else
            {
                // AvalonDock 已经自动移除了文档（通过DocumentClosed事件）
                operLog?.Debug("[AvalonDock] 文档已被 AvalonDock 自动移除：Title={Title}, ViewTypeName={ViewTypeName}, ContentId={ContentId}, RemainingCount={Count}", 
                    title, viewTypeName, contentId, Documents.Count);
            }
            
            // 释放资源（在移除后释放，避免资源访问问题）
            if (document.Content is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                    operLog?.Debug("[AvalonDock] 已释放 Content 资源：ViewTypeName={ViewTypeName}", viewTypeName);
                }
                catch (Exception ex)
                {
                    operLog?.Error(ex, "[AvalonDock] 释放 Content 资源时发生异常：{ViewTypeName}", viewTypeName);
                }
            }
            
            // 清空 Content 引用
            document.Content = null;
            
            operLog?.Debug("[AvalonDock] 文档关闭完成：Title={Title}, ViewTypeName={ViewTypeName}, ContentId={ContentId}, RemainingCount={Count}", 
                title, viewTypeName, contentId, Documents.Count);
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[AvalonDock] 关闭文档时发生异常：Title={Title}, ViewTypeName={ViewTypeName}", title, viewTypeName);
        }
    }

    /// <summary>
    /// 关闭指定的标签页（标准 MDI 实现，保留以兼容，内部调用 AvalonDock 版本）
    /// </summary>
    [RelayCommand]
    public void CloseTab(DocumentTabItem? tabItem)
    {
        if (tabItem == null) return;
        
        // 查找对应的 AvalonDocument
        var document = Documents.FirstOrDefault(d => d?.TabItem == tabItem);
        if (document != null)
        {
            CloseDocument(document);
        }
        else
        {
            // 如果找不到，使用旧方法（向后兼容）
            CloseTabLegacy(tabItem);
        }
    }

    /// <summary>
    /// 旧的关闭标签页方法（保留以兼容，但不再使用）
    /// </summary>
    private void CloseTabLegacy(DocumentTabItem? tabItem)
    {
        var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
        
        // 在方法开始时声明变量，确保在 try 块外也能访问
        int index = -1;
        bool wasSelected = false;
        bool wasLast = false;
        
        try
        {
            // 参数验证
            if (tabItem == null)
            {
                operLog?.Warning("[Tab] CloseTab: tabItem 为 null");
                return;
            }
            if (DocumentTabs == null)
            {
                operLog?.Warning("[Tab] CloseTab: DocumentTabs 为 null");
                return;
            }
            if (!DocumentTabs.Contains(tabItem))
            {
                operLog?.Warning("[Tab] CloseTab: 标签页不在集合中，Title={Title}", tabItem.Title);
                return;
            }
            
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

            // 保存状态（在移除之前）
            index = DocumentTabs.IndexOf(tabItem);
            if (index < 0)
            {
                operLog?.Warning("[Tab] CloseTab: 无法找到标签页索引，Title={Title}", tabItem.Title);
                return;
            }
            
            wasSelected = SelectedTab == tabItem;
            wasLast = (index == DocumentTabs.Count - 1);
            
            operLog?.Debug("[Tab] CloseTab: 开始关闭，Title={Title}, Index={Index}, WasSelected={WasSelected}, WasLast={WasLast}, Count={Count}",
                tabItem.Title, index, wasSelected, wasLast, DocumentTabs.Count);
            
            // 先清空选中项，避免 TabControl 在移除过程中访问已移除的项
            if (wasSelected)
            {
                try
                {
                    SelectedTab = null;
                    operLog?.Debug("[Tab] CloseTab: 已清空选中项");
                }
                catch (Exception ex)
                {
                    operLog?.Error(ex, "[Tab] CloseTab: 清空选中项时发生异常");
                    throw;
                }
            }
            
            // 移除标签页
            // 注意：RemoveAt 可能会触发 IconBlock 卸载，导致 ArgumentException（FontFamily 空值）
            // 这是 WPF 模板清理过程中的已知问题，我们需要捕获并忽略这个异常
            try
            {
                DocumentTabs.RemoveAt(index);
                operLog?.Debug("[Tab] CloseTab: 已移除标签页，剩余数量={Count}", DocumentTabs.Count);
            }
            catch (System.ArgumentException argEx) when (argEx.Message.Contains("FontFamily") || argEx.Message.Contains("IconFont"))
            {
                // IconBlock 卸载时的 FontFamily 异常，这是预期的，不需要处理
                // 日志记录但不抛出，继续执行
                operLog?.Debug("[Tab] CloseTab: IconBlock FontFamily 异常（可忽略），Title={Title}, Message={Message}",
                    tabItem.Title, argEx.Message);
                // 异常已发生，但 RemoveAt 可能已经执行，需要验证
                if (DocumentTabs.Contains(tabItem))
                {
                    // 如果还在集合中，尝试用 Remove 方法
                    try
                    {
                        DocumentTabs.Remove(tabItem);
                        operLog?.Debug("[Tab] CloseTab: 使用 Remove 方法成功移除标签页");
                    }
                    catch
                    {
                        // 忽略二次异常
                    }
                }
            }
            catch (Exception ex)
            {
                operLog?.Error(ex, "[Tab] CloseTab: 移除标签页时发生其他异常，Title={Title}, Index={Index}, Count={Count}",
                    tabItem.Title, index, DocumentTabs?.Count ?? -1);
                // 对于其他异常，如果是移除失败，尝试用 Remove 方法
                if (DocumentTabs.Contains(tabItem))
                {
                    try
                    {
                        DocumentTabs.Remove(tabItem);
                    }
                    catch { }
                }
                // 不抛出异常，继续执行后续清理逻辑
            }
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[Tab] CloseTab: 发生未捕获的异常，Title={Title}, StackTrace={StackTrace}",
                tabItem?.Title ?? "未知", ex.StackTrace ?? "");
            return;
        }
        
        // 释放资源
        try
        {
            if (tabItem.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }
            tabItem.Content = null;
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[Tab] CloseTab: 释放资源时发生异常");
        }
        
        // 设置新的选中标签（需要在 try 块外访问这些变量）
        // 但要注意：如果前面的 try 块抛出异常，这些变量可能未初始化
        // 所以我们需要重新检查状态
        try
        {
            if (DocumentTabs == null || DocumentTabs.Count == 0)
            {
                operLog?.Debug("[Tab] CloseTab: 没有剩余标签页，清空选中项");
                SelectedTab = null;
                return;
            }
            
            // 检查是否需要选择新标签页
            // 如果关闭的是当前选中的标签，需要选择一个新的标签
            var currentSelected = SelectedTab;
            var needSelectNew = (currentSelected == null || currentSelected == tabItem);
            
            if (!needSelectNew)
            {
                operLog?.Debug("[Tab] CloseTab: 不需要选择新标签页，当前选中={Title}", currentSelected?.Title ?? "null");
                return;
            }
            
            operLog?.Debug("[Tab] CloseTab: 需要选择新标签页，剩余数量={Count}", DocumentTabs.Count);
            
            // 计算目标索引
            // 注意：tabItem 已经从集合中移除，所以需要用之前保存的 index
            // 但为了安全，我们重新计算（更新之前保存的 wasLast 变量）
            wasLast = (index >= DocumentTabs.Count - 1);
            
            int targetIndex;
            if (wasLast || index < 0)
            {
                // 关闭的是最后一个，或者无法找到索引，选择新的最后一个
                targetIndex = DocumentTabs.Count - 1;
            }
            else
            {
                // 关闭的不是最后一个，选择原来位置的下一个（移除后，后面的标签会前移到该位置）
                targetIndex = index;
            }
            
            operLog?.Debug("[Tab] CloseTab: 目标索引={TargetIndex}, WasLast={WasLast}", targetIndex, wasLast);
            
            // 延迟设置，让 TabControl 完成布局更新
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() =>
                    {
                        var innerOperLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
                        try
                        {
                            // 再次验证集合状态（延迟执行时可能已经被修改）
                            if (DocumentTabs == null || DocumentTabs.Count == 0)
                            {
                                innerOperLog?.Debug("[Tab] CloseTab: 延迟执行时集合为空，清空选中项");
                                SelectedTab = null;
                                return;
                            }
                            
                            // 确保目标索引有效
                            var safeIndex = Math.Max(0, Math.Min(targetIndex, DocumentTabs.Count - 1));
                            innerOperLog?.Debug("[Tab] CloseTab: 延迟执行，SafeIndex={SafeIndex}, Count={Count}", safeIndex, DocumentTabs.Count);
                            
                            if (safeIndex >= 0 && safeIndex < DocumentTabs.Count)
                            {
                                var newTab = DocumentTabs[safeIndex];
                                if (newTab != null)
                                {
                                    innerOperLog?.Debug("[Tab] CloseTab: 延迟执行，设置选中项={Title}", newTab.Title);
                                    SelectedTab = newTab;
                                }
                                else
                                {
                                    innerOperLog?.Warning("[Tab] CloseTab: 延迟执行，目标位置为 null，SafeIndex={SafeIndex}", safeIndex);
                                    // 如果目标位置为 null，选择第一个非 null 的标签页
                                    var firstValidTab = DocumentTabs.FirstOrDefault(t => t != null);
                                    if (firstValidTab != null)
                                    {
                                        innerOperLog?.Debug("[Tab] CloseTab: 延迟执行，回退选择第一个有效标签={Title}", firstValidTab.Title);
                                        SelectedTab = firstValidTab;
                                    }
                                    else
                                    {
                                        innerOperLog?.Warning("[Tab] CloseTab: 延迟执行，找不到任何有效的标签页");
                                        SelectedTab = null;
                                    }
                                }
                            }
                            else
                            {
                                innerOperLog?.Warning("[Tab] CloseTab: 延迟执行，索引无效，SafeIndex={SafeIndex}, Count={Count}", safeIndex, DocumentTabs.Count);
                                SelectedTab = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            innerOperLog?.Error(ex, "[Tab] CloseTab: 延迟设置选中标签页时发生异常，StackTrace={StackTrace}", ex.StackTrace ?? "");
                            // 如果设置失败，尝试清空选中项
                            try
                            {
                                SelectedTab = null;
                            }
                            catch (Exception ex2)
                            {
                                innerOperLog?.Error(ex2, "[Tab] CloseTab: 清空选中项时也发生异常");
                            }
                        }
                    }));
            }
            else
            {
                operLog?.Warning("[Tab] CloseTab: Dispatcher 不可用，直接设置");
                // 如果 Dispatcher 不可用，直接设置
                try
                {
                    var safeIndex = Math.Max(0, Math.Min(targetIndex, DocumentTabs.Count - 1));
                    if (safeIndex >= 0 && safeIndex < DocumentTabs.Count)
                    {
                        var newTab = DocumentTabs[safeIndex];
                        if (newTab != null)
                        {
                            SelectedTab = newTab;
                        }
                    }
                }
                catch (Exception ex)
                {
                    operLog?.Error(ex, "[Tab] CloseTab: 直接设置选中项时发生异常");
                    SelectedTab = null;
                }
            }
        }
        catch (Exception ex)
        {
            operLog?.Error(ex, "[Tab] CloseTab: 设置新选中标签时发生异常，StackTrace={StackTrace}", ex.StackTrace ?? "");
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
            }
            else
            {
                // 如果用户ID为0，加载所有菜单（管理员模式或初始化）
                var result = await _menuService.GetAllMenuTreeAsync();
                if (result.Success && result.Data != null)
                {
                    Menus = result.Data;
                }
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[菜单] 加载菜单失败");
            Menus = new List<MenuDto>();
        }
    }

    partial void OnSelectedMenuChanged(MenuDto? value)
    {
        // 菜单选择变更时，可以在这里触发导航
        // 导航逻辑将在 MainWindow.xaml.cs 中处理
    }

    /// <summary>
    /// 选中标签页变更时的处理
    /// 按照推荐实现：不需要额外处理，WPF 的绑定会自动处理
    /// </summary>
    partial void OnSelectedTabChanged(DocumentTabItem? value)
    {
        // 这个部分方法会在设置 SelectedTab 后立即调用
        // 但由于我们在 AddOrActivateTabCore 中已经使用了延迟设置 SelectedTab
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
                var userInfoWindow = App.Services?.GetService<Views.Identity.UserInfoWindow>();
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
        var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
        operLog?.Information("[登出] 开始登出流程：当前用户={Username}, 用户ID={UserId}, 真实姓名={RealName}", 
            CurrentUsername, _currentUserId, CurrentRealName);
        
        try
        {
            // 显示确认对话框
            operLog?.Debug("[登出] 显示确认对话框");
            var result = System.Windows.MessageBox.Show(
                "确定要退出登录吗？",
                "确认登出",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                operLog?.Information("[登出] 用户取消登出");
                return;
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

                    // 关闭 AvalonDock 文档
                    var documentsCount = Documents.Count;
                    operLog?.Debug("[登出] 开始关闭 AvalonDock 文档：数量={Count}", documentsCount);
                    var documentsToClose = Documents.ToList();
                    foreach (var doc in documentsToClose)
                    {
                        if (doc is Models.AvalonDocument avalonDoc)
                        {
                            operLog?.Debug("[登出] 关闭文档：Title={Title}, ViewTypeName={ViewTypeName}", 
                                avalonDoc.Title, avalonDoc.ViewTypeName);
                        }
                    }
                    // Documents 集合会在关闭主窗口时自动清理，这里只记录日志
                    operLog?.Debug("[登出] AvalonDock 文档处理完成");

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
                    // 从依赖注入获取的 LoginWindow 可能是已关闭的实例，不能直接重用
                    operLog?.Debug("[登出] 开始查找或创建登录窗口");
                    Views.Identity.LoginWindow? loginWindow = null;
                    
                    // 检查是否有已存在的未关闭的登录窗口（隐藏但未关闭）
                    bool foundExisting = false;
                    int existingWindowsCount = 0;
                    foreach (Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window is Views.Identity.LoginWindow existingWindow)
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
                                operLog?.Error(ex, "[登出] 检查登录窗口时发生异常（窗口已关闭）：Exception={Exception}", ex.Message);
                                continue;
                            }
                        }
                    }
                    
                    operLog?.Debug("[登出] 登录窗口检查完成：找到窗口数={Count}, 可重用={FoundExisting}", 
                        existingWindowsCount, foundExisting);
                    
                    // 如果没有找到可用的登录窗口，创建新实例
                    if (!foundExisting)
                    {
                        operLog?.Debug("[登出] 创建新的登录窗口实例");
                        loginWindow = new Views.Identity.LoginWindow();
                    }
                    
                    // 确保 loginWindow 已初始化
                    if (loginWindow == null)
                    {
                        operLog?.Warning("[登出] loginWindow 为 null，创建新实例");
                        loginWindow = new Views.Identity.LoginWindow();
                    }
                    
                    // 显示登录窗口
                    operLog?.Debug("[登出] 显示登录窗口");
                    loginWindow.Show();
                    System.Windows.Application.Current.MainWindow = loginWindow;
                    operLog?.Information("[登出] 登录窗口已显示，登出流程完成：Username={Username}, UserId={UserId}", 
                        oldUsername, oldUserId);
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

