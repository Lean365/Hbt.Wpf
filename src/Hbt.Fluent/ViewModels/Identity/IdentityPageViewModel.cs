using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hbt.Application.Dtos.Identity;
using Hbt.Application.Services.Identity;
using Hbt.Common.Logging;
using Hbt.Fluent.Models;
using Hbt.Fluent.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hbt.Fluent.ViewModels.Identity;

public partial class IdentityPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = string.Empty;

    [ObservableProperty]
    private string? _pageDescription;

    [ObservableProperty]
    private ObservableCollection<NavigationCard> _navigationCards = new();

    private readonly LanguageService? _languageService;
    private readonly IMenuService? _menuService;

    public IdentityPageViewModel(LanguageService? languageService = null, IMenuService? menuService = null)
    {
        _languageService = languageService;
        _menuService = menuService;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var menuService = _menuService ?? App.Services?.GetService<IMenuService>();
        if (menuService != null)
        {
            var result = await menuService.GetAllMenuTreeAsync();
            if (result.Success && result.Data != null)
            {
                var menu = FindMenuByCode(result.Data, "identity");
                if (menu != null)
                {
                    var titleKey = menu.I18nKey ?? menu.MenuCode;
                    PageTitle = _languageService?.GetTranslation(titleKey, menu.MenuName) ?? menu.MenuName;

                    if (menu.Children != null)
                    {
                        NavigationCards.Clear();
                        foreach (var childMenu in menu.Children.OrderBy(m => m.OrderNum))
                        {
                            var childTitleKey = childMenu.I18nKey ?? childMenu.MenuCode;
                            NavigationCards.Add(new NavigationCard(
                                title: _languageService?.GetTranslation(childTitleKey, childMenu.MenuName) ?? childMenu.MenuName,
                                description: null,
                                icon: childMenu.Icon,
                                menuItem: childMenu
                            ));
                        }
                    }
                }
            }
        }
    }

    private MenuDto? FindMenuByCode(List<MenuDto> menus, string menuCode)
    {
        foreach (var menu in menus)
        {
            if (menu.MenuCode == menuCode) return menu;
            if (menu.Children != null)
            {
                var found = FindMenuByCode(menu.Children, menuCode);
                if (found != null) return found;
            }
        }
        return null;
    }

    [RelayCommand]
    public void Navigate(object? menuItem)
    {
        try
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Information("[导航] Navigate 命令被调用，参数: {MenuItem}, 类型: {Type}",
                menuItem?.ToString() ?? "null", menuItem?.GetType().FullName ?? "null");
            
            if (menuItem is MenuDto menu)
            {
                operLog?.Information("[导航] 参数解析为 MenuDto：MenuCode={MenuCode}, MenuName={MenuName}, RoutePath={RoutePath}, Component={Component}",
                    menu.MenuCode, menu.MenuName, menu.RoutePath ?? "", menu.Component ?? "");
                
                if (!string.IsNullOrEmpty(menu.RoutePath) || !string.IsNullOrEmpty(menu.Component))
                {
                    // 优先通过依赖注入获取 MainWindow（Singleton）
                    Views.MainWindow? mainWindow = App.Services?.GetService<Views.MainWindow>();
                    
                    // 如果 DI 获取失败，尝试通过 Application.Current.MainWindow
                    if (mainWindow == null)
                    {
                        mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
                    }
                    
                    // 如果还是失败，遍历所有窗口查找
                    if (mainWindow == null)
                    {
                        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window is Views.MainWindow mw)
                            {
                                mainWindow = mw;
                                break;
                            }
                        }
                    }
                    
                    if (mainWindow != null)
                    {
                        operLog?.Information("[导航] 导航到菜单：{MenuCode} ({MenuName}), RoutePath: {RoutePath}, Component: {Component}",
                            menu.MenuCode, menu.MenuName, menu.RoutePath ?? "", menu.Component ?? "");
                        mainWindow.NavigateToMenu(menu);
                    }
                    else
                    {
                        operLog?.Error("[导航] 无法找到 MainWindow 实例。Application.Current.MainWindow: {MainWindowType}, Windows 数量: {WindowCount}, DI 服务: {HasService}",
                            System.Windows.Application.Current.MainWindow?.GetType().FullName ?? "null",
                            System.Windows.Application.Current.Windows.Count,
                            App.Services?.GetService<Views.MainWindow>() != null);
                    }
                }
                else
                {
                    operLog?.Warning("[导航] 菜单 RoutePath 和 Component 都为空，无法导航：{MenuCode} ({MenuName})",
                        menu.MenuCode, menu.MenuName);
                }
            }
            else
            {
                operLog?.Error("[导航] 导航参数类型错误，期望 MenuDto，实际类型: {Type}",
                    menuItem?.GetType().FullName ?? "null");
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[导航] 导航失败：参数类型 {Type}", menuItem?.GetType().FullName ?? "null");
        }
    }
}

