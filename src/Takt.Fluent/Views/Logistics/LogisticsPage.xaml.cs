//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : LogisticsPage.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 后勤管理快速导航页面
//===================================================================

using System.Windows.Controls;
using Takt.Application.Services.Identity;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels;
using System.Linq;

namespace Takt.Fluent.Views.Logistics;

/// <summary>
/// 后勤管理快速导航页面
/// </summary>
public partial class LogisticsPage : UserControl
{
    public NavigationPageViewModel ViewModel { get; }

    public LogisticsPage()
    {
        InitializeComponent();
        
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        ViewModel = new NavigationPageViewModel(localizationManager);
        DataContext = this;
        
        Loaded += LogisticsPage_Loaded;
    }

    private async void LogisticsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Loaded -= LogisticsPage_Loaded;
        
        var menuService = App.Services?.GetService<IMenuService>();
        if (menuService != null)
        {
            var result = await menuService.GetAllMenuTreeAsync();
            if (result.Success && result.Data != null)
            {
                var logisticsMenu = FindMenuByCode(result.Data, "logistics");
                if (logisticsMenu != null)
                {
                    ViewModel.InitializeFromMenuWithLocalization(logisticsMenu, NavigateToMenu);
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
