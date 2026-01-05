//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : MaterialsPage.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 物料管理快速导航页面
//===================================================================

using System.Windows.Controls;
using Takt.Application.Services.Identity;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels;
using System.Linq;

namespace Takt.Fluent.Views.Logistics.Materials;

/// <summary>
/// 物料管理快速导航页面
/// </summary>
public partial class MaterialsPage : UserControl
{
    public NavigationPageViewModel ViewModel { get; }

    public MaterialsPage()
    {
        InitializeComponent();
        
        var localizationManager = App.Services?.GetService<ILocalizationManager>();
        ViewModel = new NavigationPageViewModel(localizationManager);
        DataContext = this;
        
        Loaded += MaterialsPage_Loaded;
    }

    private async void MaterialsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Loaded -= MaterialsPage_Loaded;
        
        var menuService = App.Services?.GetService<IMenuService>();
        if (menuService != null)
        {
            var result = await menuService.GetAllMenuTreeAsync();
            if (result.Success && result.Data != null)
            {
                var materialsMenu = FindMenuByCode(result.Data, "materials");
                if (materialsMenu != null)
                {
                    ViewModel.InitializeFromMenuWithLocalization(materialsMenu, NavigateToMenu);
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
