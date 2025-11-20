//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : UserViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 用户管理视图模型（列表、筛选、增删改导出）
//===================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;
using Takt.Fluent.Views.Identity;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 用户管理视图模型
/// </summary>
public partial class UserViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<UserDto> Users { get; } = new();

    [ObservableProperty]
    private UserDto? _selectedUser;

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
    /// 总页数（根据 TotalCount 与 PageSize 计算）
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// 是否存在上一页
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// 是否存在下一页
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    public UserViewModel(
        IUserService userService,
        IServiceProvider serviceProvider,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = _localizationManager.GetString("common.noData") ?? "暂无数据";

        _ = LoadAsync();
    }

    /// <summary>
    /// 加载用户列表
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
            _operLog?.Information("[UserView] Load users: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            // 构建查询DTO
            var query = new UserQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim()
            };

            var result = await _userService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                Users.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Identity.User.LoadFailed") ?? "加载用户数据失败";
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                return;
            }

            Users.Clear();
            foreach (var user in result.Data.Items)
            {
                Users.Add(user);
            }
            
            // 数据加载完成后，重新评估所有命令的 CanExecute
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            TotalCount = result.Data.TotalNum;
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(HasPreviousPage));

            UpdateEmptyMessage();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[UserView] 加载用户列表失败");
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

        EmptyMessage = _localizationManager.GetString("common.noData") ?? "暂无数据";
    }

    /// <summary>
    /// 查询命令（来自自定义表格）
    /// </summary>
    [RelayCommand]
    private async Task QueryAsync(QueryContext context)
    {
        Keyword = context.Keyword;
        if (PageIndex != context.PageIndex)
        {
            PageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (PageSize != context.PageSize)
        {
            PageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadAsync();
    }

    /// <summary>
    /// 重置查询
    /// </summary>
    [RelayCommand]
    private async Task ResetAsync(QueryContext context)
    {
        Keyword = string.Empty;
        PageIndex = 1;
        await LoadAsync();
    }

    /// <summary>
    /// 分页变化
    /// </summary>
    [RelayCommand]
    private async Task PageChangedAsync(PageRequest request)
    {
        if (PageIndex != request.PageIndex)
        {
            PageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        }

        if (PageSize != request.PageSize && request.PageSize > 0)
        {
            PageSize = request.PageSize;
        }

        await LoadAsync();
    }

    /// <summary>
    /// 新建用户
    /// </summary>
    [RelayCommand]
    private void Create()
    {
        ShowUserForm(null);
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private void Update(UserDto? user)
    {
        // 如果没有传递参数，使用 SelectedUser
        if (user == null)
        {
            user = SelectedUser;
        }

        if (user == null)
        {
            return;
        }

        SelectedUser = user;
        ShowUserForm(user);
    }

    private bool CanUpdate(UserDto? user)
    {
        // 如果没有传递参数，检查 SelectedUser
        if (user == null)
        {
            user = SelectedUser;
        }
        
        // 如果用户不存在，不能更新
        if (user == null)
        {
            return false;
        }
        
        // 超级用户（admin）不允许更新
        if (user.Username == "admin")
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(UserDto? user)
    {
        if (user == null)
        {
            return;
        }

        SelectedUser = user;

        var confirmText = _localizationManager.GetString("Identity.User.DeleteConfirm") ?? "确定要删除该用户吗？";
        var owner = System.Windows.Application.Current?.MainWindow;
        if (!TaktMessageManager.ShowDeleteConfirm(confirmText, owner))
        {
            return;
        }

        try
        {
            var result = await _userService.DeleteAsync(user.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete") ?? "{0}删除失败", entityName);
                TaktMessageManager.ShowError(errorMessage);
                return;
            }

            var entityNameSuccess = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
            var successMessage = string.Format(_localizationManager.GetString("common.success.delete") ?? "{0}删除成功", entityNameSuccess);
            TaktMessageManager.ShowSuccess(successMessage);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            TaktMessageManager.ShowError(errorMessage);
            _operLog?.Error(ex, "[UserView] 删除用户失败，Id={UserId}", user.Id);
        }
    }

    private bool CanDelete(UserDto? user)
    {
        // 如果没有传递参数，检查 SelectedUser
        if (user == null)
        {
            user = SelectedUser;
        }
        
        // 如果用户不存在，不能删除
        if (user == null)
        {
            return false;
        }
        
        // 超级用户（admin）不允许删除
        if (user.Username == "admin")
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 分配角色
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAssignRole))]
    private async Task AssignRoleAsync(UserDto? user)
    {
        // 如果没有传递参数，使用 SelectedUser
        if (user == null)
        {
            user = SelectedUser;
        }

        if (user == null)
        {
            return;
        }

        SelectedUser = user;

        try
        {
            var window = _serviceProvider.GetRequiredService<Takt.Fluent.Views.Identity.UserComponent.UserAssignRole>();
            if (window.DataContext is not UserAssignRoleViewModel formViewModel)
            {
                throw new InvalidOperationException("UserAssignRole DataContext 不是 UserAssignRoleViewModel");
            }

            await formViewModel.InitializeAsync(user);

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[UserView] 打开分配角色窗口失败，用户Id={UserId}", user.Id);
        }
    }

    private bool CanAssignRole(UserDto? user)
    {
        // 如果没有传递参数，检查 SelectedUser
        if (user == null)
        {
            user = SelectedUser;
        }
        
        // 如果用户不存在，不能分配角色
        if (user == null)
        {
            return false;
        }
        
        // 超级用户（admin）不允许分配角色
        if (user.Username == "admin")
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 授权
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAuthorize))]
    private void Authorize(UserDto? user)
    {
        // 如果没有传递参数，使用 SelectedUser
        if (user == null)
        {
            user = SelectedUser;
        }

        if (user == null)
        {
            return;
        }

        SelectedUser = user;
        // TODO: 实现授权窗口
        ErrorMessage = _localizationManager.GetString("Identity.User.AuthorizeNotImplemented") ?? "授权功能待实现";
        _operLog?.Information("[UserView] 授权：用户Id={UserId}, 用户名={Username}", user.Id, user.Username);
    }

    private bool CanAuthorize(UserDto? user)
    {
        // 如果没有传递参数，检查 SelectedUser
        if (user == null)
        {
            user = SelectedUser;
        }
        
        // 如果用户不存在，不能授权
        if (user == null)
        {
            return false;
        }
        
        // 超级用户（admin）不允许授权
        if (user.Username == "admin")
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 打开用户表单窗口
    /// </summary>
    /// <param name="user">要编辑的用户，null 表示新建</param>
    private void ShowUserForm(UserDto? user)
    {
        try
        {
            var window = _serviceProvider.GetRequiredService<Takt.Fluent.Views.Identity.UserComponent.UserForm>();
            if (window.DataContext is not UserFormViewModel formViewModel)
            {
                throw new InvalidOperationException("UserForm DataContext 不是 UserFormViewModel");
            }

            if (user == null)
            {
                formViewModel.ForCreate();
            }
            else
            {
                formViewModel.ForUpdate(user);
            }

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[UserView] 打开用户表单窗口失败");
        }
    }

    partial void OnSelectedUserChanged(UserDto? value)
    {
        // 通知命令系统重新评估所有命令的 CanExecute
        // 这对于工具栏按钮和行操作按钮都很重要
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        AssignRoleCommand.NotifyCanExecuteChanged();
        AuthorizeCommand.NotifyCanExecuteChanged();
        CreateCommand.NotifyCanExecuteChanged();
        
        // 同时触发全局命令重新评估，确保行操作按钮也能正确更新
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

}

