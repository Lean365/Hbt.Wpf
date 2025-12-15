//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : SerialsPage.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 序列号管理快速导航页面
//===================================================================

using System.Windows.Controls;
using Takt.Application.Services.Identity;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels;
using System.Linq;

namespace Takt.Fluent.Views.Logistics.Serials;

/// <summary>
/// 序列号管理快速导航页面
/// </summary>
public partial class SerialsPage : UserControl
{
    public NavigationPageViewModel ViewModel { get; }

    public SerialsPage()
    {
        InitializeComponent();
        
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        ViewModel = new NavigationPageViewModel(localizationManager);
        DataContext = this;
        
        Loaded += SerialsPage_Loaded;
    }

    private async void SerialsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Loaded -= SerialsPage_Loaded;
        
        var menuService = App.Services?.GetService<IMenuService>();
        if (menuService != null)
        {
            var result = await menuService.GetAllMenuTreeAsync();
            if (result.Success && result.Data != null)
            {
                var serialsMenu = FindMenuByCode(result.Data, "serials");
                if (serialsMenu != null)
                {
                    ViewModel.InitializeFromMenuWithLocalization(serialsMenu, NavigateToMenu);
                }
            }
        }
    }

    private void NavigateToMenu(Takt.Application.Dtos.Identity.MenuDto menu)
    {
        var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        if (mainWindow != null && !string.IsNullOrEmpty(menu.RoutePath))
        {
            mainWindow.NavigateToMenu(menu);
        }
    }

    private Takt.Application.Dtos.Identity.MenuDto? FindMenuByCode(System.Collections.Generic.List<Takt.Application.Dtos.Identity.MenuDto> menus, string menuCode)
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
