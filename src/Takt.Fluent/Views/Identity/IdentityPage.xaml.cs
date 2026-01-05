// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity
// 文件名称：IdentityPage.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：身份管理快速导航页面
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq;
using System.Windows.Controls;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels;

namespace Takt.Fluent.Views.Identity;

/// <summary>
/// 身份管理快速导航页面
/// </summary>
public partial class IdentityPage : UserControl
{
    public NavigationPageViewModel ViewModel { get; }

    public IdentityPage()
    {
        InitializeComponent();
        
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        ViewModel = new NavigationPageViewModel(localizationManager);
        DataContext = this;
        
        Loaded += IdentityPage_Loaded;
    }

    private async void IdentityPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Loaded -= IdentityPage_Loaded;
        
        var menuService = App.Services?.GetService<IMenuService>();
        if (menuService != null)
        {
            var result = await menuService.GetAllMenuTreeAsync();
            if (result.Success && result.Data != null)
            {
                var identityMenu = FindMenuByCode(result.Data, "identity");
                if (identityMenu != null)
                {
                    ViewModel.InitializeFromMenuWithLocalization(identityMenu, NavigateToMenu);
                }
            }
        }
    }

    private void NavigateToMenu(MenuDto menu)
    {
        var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        if (mainWindow != null && (!string.IsNullOrEmpty(menu.RoutePath) || !string.IsNullOrEmpty(menu.Component)))
        {
            mainWindow.NavigateToMenu(menu);
        }
    }

    private MenuDto? FindMenuByCode(System.Collections.Generic.List<MenuDto> menus, string menuCode)
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
}

