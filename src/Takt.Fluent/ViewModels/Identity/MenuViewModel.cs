//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : MenuViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 菜单管理视图模型（列表、筛选、增删改导出）
//===================================================================

using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using PageRequest = Takt.Fluent.Controls.PageRequest;
using QueryContext = Takt.Fluent.Controls.QueryContext;
// TaktTreeViewQueryContext 已删除，使用 QueryContext 替代

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 菜单管理视图模型
/// </summary>
public partial class MenuViewModel : ObservableObject
{
    private readonly IMenuService _menuService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<MenuDto> Menus { get; } = new();

    [ObservableProperty]
    private MenuDto? _selectedMenu;

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private int _pageIndex = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private string _emptyMessage = string.Empty;

    /// <summary>
    /// 是否显示分页（树形视图不显示分页）
    /// </summary>
    public static bool ShowPagination => false;

    /// <summary>
    /// 总页数（树形视图不使用分页）
    /// </summary>
    public static int TotalPages => 1;

    /// <summary>
    /// 是否存在上一页（树形视图不使用分页）
    /// </summary>
    public static bool HasPreviousPage => false;

    /// <summary>
    /// 是否存在下一页（树形视图不使用分页）
    /// </summary>
    public static bool HasNextPage => false;

    public MenuViewModel(
        IMenuService menuService,
        IServiceProvider serviceProvider,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = _localizationManager.GetString("common.nodata");

        _ = LoadAsync();
    }

    /// <summary>
    /// 加载菜单树
    /// </summary>
    private async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[MenuView] Load menu tree: keyword={Keyword}", Keyword);

            // 获取所有菜单树（树形视图不使用分页）
            var result = await _menuService.GetAllMenuTreeAsync();

            if (!result.Success || result.Data == null)
            {
                Menus.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("identity.menu.loadfailed");
                return;
            }

            List<MenuDto> menuTree = result.Data;

            // 如果有关键词，进行过滤
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                menuTree = FilterMenuTree(result.Data, Keyword.Trim());
            }

            Menus.Clear();
            foreach (var menu in menuTree)
            {
                Menus.Add(menu);
            }

            // 计算总数量（包括所有子节点）
            TotalCount = CountMenuNodes(menuTree);

            UpdateEmptyMessage();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[MenuView] 加载菜单树失败");
        }
        finally
        {
            IsLoading = false;
            if (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                UpdateEmptyMessage();
            }
        }
    }

    /// <summary>
    /// 过滤菜单树（保留匹配的节点及其父节点和子节点）
    /// </summary>
    private static List<MenuDto> FilterMenuTree(List<MenuDto> menuTree, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return menuTree;
        }

        var result = new List<MenuDto>();

        foreach (var menu in menuTree)
        {
            var filteredMenu = FilterMenuNode(menu, keyword);
            if (filteredMenu != null)
            {
                result.Add(filteredMenu);
            }
        }

        return result;
    }

    /// <summary>
    /// 过滤菜单节点（递归过滤子节点）
    /// </summary>
    private static MenuDto? FilterMenuNode(MenuDto menu, string keyword)
    {
        bool matches = (menu.MenuName != null && menu.MenuName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                       (menu.MenuCode != null && menu.MenuCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                       (menu.I18nKey != null && menu.I18nKey.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                       (menu.PermCode != null && menu.PermCode.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        // 递归过滤子节点
        List<MenuDto>? filteredChildren = null;
        if (menu.Children != null && menu.Children.Count > 0)
        {
            filteredChildren = new List<MenuDto>();
            foreach (var child in menu.Children)
            {
                var filteredChild = FilterMenuNode(child, keyword);
                if (filteredChild != null)
                {
                    filteredChildren.Add(filteredChild);
                    matches = true; // 如果有子节点匹配，父节点也应该显示
                }
            }
        }

        // 如果有匹配，返回过滤后的菜单
        if (matches)
        {
            return new MenuDto
            {
                Id = menu.Id,
                MenuName = menu.MenuName ?? string.Empty,
                MenuCode = menu.MenuCode ?? string.Empty,
                I18nKey = menu.I18nKey!,
                PermCode = menu.PermCode!,
                MenuType = menu.MenuType,
                ParentId = menu.ParentId,
                RoutePath = menu.RoutePath,
                Icon = menu.Icon,
                Component = menu.Component,
                IsExternal = menu.IsExternal,
                IsCache = menu.IsCache,
                IsVisible = menu.IsVisible,
                OrderNum = menu.OrderNum,
                MenuStatus = menu.MenuStatus,
                Remarks = menu.Remarks,
                Children = filteredChildren
            };
        }

        return null;
    }

    /// <summary>
    /// 递归统计菜单树中的总节点数
    /// </summary>
    private static int CountMenuNodes(List<MenuDto> menuTree)
    {
        int count = 0;
        foreach (var menu in menuTree)
        {
            count++;
            if (menu.Children != null && menu.Children.Count > 0)
            {
                count += CountMenuNodes(menu.Children);
            }
        }
        return count;
    }

    partial void OnErrorMessageChanged(string? value)
    {
        UpdateEmptyMessage();
    }

    private void UpdateEmptyMessage()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            EmptyMessage = ErrorMessage!;
            return;
        }

        EmptyMessage = _localizationManager.GetString("common.nodata");
    }

    /// <summary>
    /// 查询命令（树形视图只支持关键词搜索）
    /// </summary>
    [RelayCommand]
    private async Task QueryAsync(object? context = null)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        string keyword = string.Empty;

        if (context is QueryContext queryContext)
        {
            keyword = queryContext.Keyword ?? string.Empty;
            Keyword = keyword;
        }
        else if (context is QueryContext treeViewContext)
        {
            keyword = treeViewContext.Keyword ?? string.Empty;
            Keyword = keyword;
        }

        _operLog?.Information("[MenuView] 执行查询操作，操作人={Operator}, 关键词={Keyword}", operatorName, keyword);
        await LoadAsync();
    }

    /// <summary>
    /// 重置查询
    /// </summary>
    [RelayCommand]
    private async Task ResetAsync(object? context = null)
    {
        _operLog?.Information("[MenuView] Reset query: clearing keyword");
        Keyword = string.Empty;
        await LoadAsync();
    }

    /// <summary>
    /// 分页变化（树形视图不使用分页，此方法不做任何操作）
    /// </summary>
    [RelayCommand]
    private async Task PageChangedAsync(PageRequest? request = null)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[MenuView] 分页变化（树形视图不使用分页），操作人={Operator}", operatorName);
        // 树形视图不使用分页，不做任何操作
        await Task.CompletedTask;
    }

    /// <summary>
    /// 新建菜单
    /// </summary>
    [RelayCommand]
    private void Create()
    {
        if (App.Services == null) return;

        try
        {
            var formViewModel = App.Services.GetRequiredService<MenuFormViewModel>();
            var form = App.Services.GetRequiredService<Views.Identity.MenuComponent.MenuForm>();

            formViewModel.ForCreate();
            formViewModel.SaveSuccessCallback = () =>
            {
                form.Close();
                _ = LoadAsync();
            };

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            form.Owner = System.Windows.Application.Current?.MainWindow;
            form.ShowDialog();
            _operLog?.Information("[MenuView] 打开新建菜单窗口，操作人={Operator}", operatorName);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[MenuView] 打开新建菜单窗口失败");
        }
    }

    /// <summary>
    /// 更新菜单
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private void Update(MenuDto? menu)
    {
        // 如果没有传递参数，使用 SelectedMenu
        if (menu == null)
        {
            menu = SelectedMenu;
        }

        if (menu == null)
        {
            return;
        }

        SelectedMenu = menu;

        if (App.Services == null) return;

        try
        {
            var formViewModel = App.Services.GetRequiredService<MenuFormViewModel>();
            var form = App.Services.GetRequiredService<Views.Identity.MenuComponent.MenuForm>();

            formViewModel.ForUpdate(menu);
            formViewModel.SaveSuccessCallback = () =>
            {
                form.Close();
                _ = LoadAsync();
            };

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            form.Owner = System.Windows.Application.Current?.MainWindow;
            form.ShowDialog();
            _operLog?.Information("[MenuView] 打开更新菜单窗口，操作人={Operator}, 菜单Id={MenuId}, 菜单代码={MenuCode}",
                operatorName, menu.Id, menu.MenuCode ?? string.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[MenuView] 打开更新菜单窗口失败，菜单Id={MenuId}", menu.Id);
        }
    }

    private bool CanUpdate(MenuDto? menu)
    {
        // 如果没有传递参数，检查 SelectedMenu
        if (menu == null)
        {
            return SelectedMenu is not null;
        }
        return menu is not null;
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(MenuDto? menu)
    {
        // 如果没有传递参数，使用 SelectedMenu
        if (menu == null)
        {
            menu = SelectedMenu;
        }

        if (menu == null)
        {
            return;
        }

        SelectedMenu = menu;

        var confirmText = string.Format(_localizationManager.GetString("common.confirm.delete"), _localizationManager.GetString("identity.menu.entity"));
        var confirmTitle = _localizationManager.GetString("common.messagebox.question");
        if (System.Windows.MessageBox.Show(confirmText, confirmTitle, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var result = await _menuService.DeleteMenuAsync(menu.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("identity.menu.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("identity.menu.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[MenuView] 删除菜单成功，操作人={Operator}, 菜单Id={MenuId}, 菜单代码={MenuCode}",
                operatorName, menu.Id, menu.MenuCode ?? string.Empty);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[MenuView] 删除菜单失败，Id={MenuId}", menu.Id);
        }
    }

    private bool CanDelete(MenuDto? menu)
    {
        // 如果没有传递参数，检查 SelectedMenu
        if (menu == null)
        {
            return SelectedMenu is not null;
        }
        return menu is not null;
    }

    partial void OnSelectedMenuChanged(MenuDto? value)
    {
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }
}

