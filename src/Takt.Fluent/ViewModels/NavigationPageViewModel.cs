//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : NavigationPageViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 导航页面 ViewModel 基类
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Domain.Interfaces;
using Takt.Fluent.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Takt.Fluent.ViewModels;

/// <summary>
/// 导航页面 ViewModel 基类
/// </summary>
public partial class NavigationPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = string.Empty;

    [ObservableProperty]
    private string? _pageDescription;

    [ObservableProperty]
    private ObservableCollection<NavigationCard> _navigationCards = new();

    private readonly ILocalizationManager? _localizationManager;
    private readonly IMenuService? _menuService;
    private Action<MenuDto>? _navigateAction;

    public NavigationPageViewModel(ILocalizationManager? localizationManager = null!, IMenuService? menuService = null)
    {
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _menuService = menuService;
    }


    /// <summary>
    /// 异步加载指定菜单的导航卡片
    /// </summary>
    public async Task LoadAsync(string menuCode)
    {
        var menuService = _menuService ?? App.Services?.GetService<IMenuService>();
        
        if (menuService != null)
        {
            var result = await menuService.GetAllMenuTreeAsync();
            if (result.Success && result.Data != null)
            {
                var menu = FindMenuByCode(result.Data, menuCode);
                if (menu != null)
                {
                    InitializeFromMenuWithLocalization(menu, NavigateToMenu);
                }
            }
        }
    }

    private MenuDto? FindMenuByCode(List<MenuDto> menus, string menuCode)
    {
        foreach (var menu in menus)
        {
            if (menu.MenuCode == menuCode)
            {
                return menu;
            }
            if (menu.Children != null)
            {
                var found = FindMenuByCode(menu.Children, menuCode);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    private void NavigateToMenu(MenuDto menu)
    {
        var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
        if (mainWindow != null && !string.IsNullOrEmpty(menu.RoutePath))
        {
            mainWindow.NavigateToMenu(menu);
        }
    }

    /// <summary>
    /// 从菜单数据初始化导航卡片（使用 I18nKey 进行本地化）
    /// </summary>
    public void InitializeFromMenuWithLocalization(MenuDto menu, Action<MenuDto>? navigateAction = null)
    {
        // 使用 I18nKey 获取翻译
        var titleKey = menu.I18nKey ?? menu.MenuCode;
        PageTitle = _localizationManager?.GetString(titleKey) ?? menu.MenuName ?? string.Empty;
        _navigateAction = navigateAction;

        // 获取子菜单作为导航卡片
        if (menu.Children != null && menu.Children.Any())
        {
            NavigationCards.Clear();
            foreach (var childMenu in menu.Children.OrderBy(m => m.OrderNum))
            {
                var childTitleKey = childMenu.I18nKey ?? childMenu.MenuCode;
                var card = new NavigationCard(
                    title: _localizationManager?.GetString(childTitleKey) ?? childMenu.MenuName ?? string.Empty,
                    description: null,
                    icon: childMenu.Icon,
                    menuItem: childMenu
                );
                NavigationCards.Add(card);
            }
        }
    }

    /// <summary>
    /// 导航命令（与 WPFGallery 相同的实现方式）
    /// </summary>
    [RelayCommand]
    public void Navigate(object menuItem)
    {
        if (menuItem is MenuDto menu)
        {
            _navigateAction?.Invoke(menu);
        }
    }
}

