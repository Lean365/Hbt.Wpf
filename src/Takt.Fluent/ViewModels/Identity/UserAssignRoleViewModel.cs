// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Identity
// 文件名称：UserAssignRoleViewModel.cs
// 创建时间：2025-11-13
// 创建人：Takt365(Cursor AI)
// 功能描述：用户分配角色视图模型
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
/// 用户分配角色视图模型
/// </summary>
public partial class UserAssignRoleViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILocalizationManager _localizationManager;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private long _userId;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _realName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RoleItemViewModel> _unassignedRoles = new();

    [ObservableProperty]
    private ObservableCollection<RoleItemViewModel> _assignedRoles = new();

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

    public UserAssignRoleViewModel(
        IUserService userService,
        IRoleService roleService,
        ILocalizationManager localizationManager)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }


    /// <summary>
    /// 初始化用户信息
    /// </summary>
    public async Task InitializeAsync(UserDto user)
    {
        UserId = user.Id;
        Username = user.Username;
        RealName = user.RealName ?? string.Empty;
        var assignRoleText = _localizationManager.GetString("identity.user.assignrole");
        Title = $"{assignRoleText} - {Username}";

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            // 1. 加载所有角色
            var rolesQuery = new RoleQueryDto
            {
                PageIndex = 1,
                PageSize = 1000
            };
            var rolesResult = await _roleService.GetListAsync(rolesQuery);
            if (!rolesResult.Success || rolesResult.Data == null)
            {
                ErrorMessage = rolesResult.Message ?? _localizationManager.GetString("identity.user.loadrolesfailed");
                return;
            }

            // 2. 获取用户当前的角色
            var userRolesResult = await _userService.GetUserRolesAsync(user.Id);
            if (!userRolesResult.Success || userRolesResult.Data == null)
            {
                ErrorMessage = userRolesResult.Message ?? _localizationManager.GetString("identity.user.loaduserrolesfailed");
                return;
            }

            var userRoleIds = userRolesResult.Data;

            // 3. 构建角色列表（分为未分配和已分配）
            UnassignedRoles.Clear();
            AssignedRoles.Clear();
            
            foreach (var role in rolesResult.Data.Items)
            {
                var roleItem = new RoleItemViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.RoleName,
                    RoleCode = role.RoleCode,
                    Description = role.Description
                };

                if (userRoleIds.Contains(role.Id))
                {
                    AssignedRoles.Add(roleItem);
                }
                else
                {
                    UnassignedRoles.Add(roleItem);
                }
            }
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
    /// 保存分配的角色
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
            var selectedRoleIds = AssignedRoles.Select(r => r.RoleId).ToList();

            var result = await _userService.AssignRolesAsync(UserId, selectedRoleIds);
            if (!result.Success)
            {
                var errorMessage = result.Message ?? _localizationManager.GetString("identity.user.assignrolefailed");
                ErrorMessage = errorMessage;
                TaktMessageManager.ShowError(errorMessage);
                return;
            }

            var successMessage = _localizationManager.GetString("identity.user.assignrolesuccess");
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
/// 角色项视图模型
/// </summary>
public partial class RoleItemViewModel : ObservableObject
{
    [ObservableProperty]
    private long _roleId;

    [ObservableProperty]
    private string _roleName = string.Empty;

    [ObservableProperty]
    private string _roleCode = string.Empty;

    [ObservableProperty]
    private string? _description;

}

