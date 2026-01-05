// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Identity
// 文件名称：RoleAssignMenuViewModel.cs
// 创建时间：2025-11-13
// 创建人：Takt365(Cursor AI)
// 功能描述：角色分配菜单视图模型
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Results;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 角色分配菜单视图模型
/// </summary>
public partial class RoleAssignMenuViewModel : ObservableObject
{
    private readonly IRoleService _roleService;
    private readonly IMenuService _menuService;
    private readonly ILocalizationManager _localizationManager;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private long _roleId;

    [ObservableProperty]
    private string _roleName = string.Empty;

    [ObservableProperty]
    private string _roleCode = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _unassignedMenus = new();

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _assignedMenus = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    /// <summary>
    /// 保存成功后的回调
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public RoleAssignMenuViewModel(
        IRoleService roleService,
        IMenuService menuService,
        ILocalizationManager localizationManager)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }


    /// <summary>
    /// 初始化角色信息
    /// </summary>
    public async Task InitializeAsync(RoleDto role)
    {
        RoleId = role.Id;
        RoleName = role.RoleName;
        RoleCode = role.RoleCode;
        var assignMenuText = _localizationManager.GetString("identity.role.assignmenu");
        Title = $"{assignMenuText} - {RoleName}";

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            // 1. 加载所有菜单（树形结构）
            var menusResult = await _menuService.GetAllMenuTreeAsync();
            if (!menusResult.Success || menusResult.Data == null)
            {
                ErrorMessage = menusResult.Message ?? _localizationManager.GetString("identity.role.loadmenusfailed");
                return;
            }

            // 2. 获取角色当前的菜单
            var roleMenusResult = await _roleService.GetRoleMenusAsync(role.Id);
            if (!roleMenusResult.Success || roleMenusResult.Data == null)
            {
                ErrorMessage = roleMenusResult.Message ?? _localizationManager.GetString("identity.role.loadrolemenusfailed");
                return;
            }

            var roleMenuIds = roleMenusResult.Data;

            // 3. 构建菜单列表（扁平化树形结构，分为未分配和已分配）
            UnassignedMenus.Clear();
            AssignedMenus.Clear();
            
            FlattenMenus(menusResult.Data, roleMenuIds, 0);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 扁平化菜单树
    /// </summary>
    private void FlattenMenus(List<MenuDto> menus, List<long> roleMenuIds, int level)
    {
        foreach (var menu in menus)
        {
            var menuItem = new MenuItemViewModel
            {
                MenuId = menu.Id,
                MenuName = menu.MenuName,
                MenuCode = menu.MenuCode,
                MenuType = menu.MenuType,
                Level = level,
                Indent = new string(' ', level * 2)
            };

            if (roleMenuIds.Contains(menu.Id))
            {
                AssignedMenus.Add(menuItem);
            }
            else
            {
                UnassignedMenus.Add(menuItem);
            }

            // 递归处理子菜单
            if (menu.Children != null && menu.Children.Any())
            {
                FlattenMenus(menu.Children, roleMenuIds, level + 1);
            }
        }
    }

    /// <summary>
    /// 保存分配的菜单
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
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
            var selectedMenuIds = AssignedMenus.Select(m => m.MenuId).ToList();

            var result = await _roleService.AssignMenusAsync(RoleId, selectedMenuIds);
            if (!result.Success)
            {
                var errorMessage = result.Message ?? _localizationManager.GetString("identity.role.assignmenufailed");
                ErrorMessage = errorMessage;
                TaktMessageManager.ShowError(errorMessage);
                return;
            }

            var successMessage = _localizationManager.GetString("identity.role.assignmenusuccess");
            SuccessMessage = successMessage;
            TaktMessageManager.ShowSuccess(successMessage);
            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            ErrorMessage = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// 菜单项视图模型
/// </summary>
public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private long _menuId;

    [ObservableProperty]
    private string _menuName = string.Empty;

    [ObservableProperty]
    private string _menuCode = string.Empty;

    [ObservableProperty]
    private Takt.Common.Enums.MenuTypeEnum _menuType;

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private string _indent = string.Empty;
}

