//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : RoleViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 角色管理视图模型（列表、筛选、增删改导出）
//===================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Context;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;
using Takt.Fluent.Views.Identity;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 角色管理视图模型
/// </summary>
public partial class RoleViewModel : ObservableObject
{
    private readonly IRoleService _roleService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<RoleDto> Roles { get; } = new();

    [ObservableProperty]
    private RoleDto? _selectedRole;

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

    public RoleViewModel(
        IRoleService roleService,
        IServiceProvider serviceProvider,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = _localizationManager.GetString("common.nodata");

        _ = LoadAsync();
    }

    /// <summary>
    /// 加载角色列表
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
            _operLog?.Information("[RoleView] Load roles: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            // 构建查询DTO
            var query = new RoleQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _roleService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                Roles.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("identity.role.loadfailed");
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                return;
            }

            Roles.Clear();
            foreach (var role in result.Data.Items)
            {
                Roles.Add(role);
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
            _operLog?.Error(ex, "[RoleView] 加载角色列表失败");
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

        EmptyMessage = _localizationManager.GetString("common.nodata");
    }

    /// <summary>
    /// 查询命令（来自自定义表格）
    /// </summary>
    [RelayCommand]
    private async Task QueryAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[RoleView] 执行查询操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}", 
            operatorName, context.Keyword ?? string.Empty, context.PageIndex, context.PageSize);
        
        Keyword = context.Keyword ?? string.Empty;
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
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[RoleView] 执行重置操作，操作人={Operator}", operatorName);
        
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
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[RoleView] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}", 
            operatorName, request.PageIndex, request.PageSize);
        
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
    /// 新建角色
    /// </summary>
    [RelayCommand]
    private void Create()
    {
        if (App.Services == null) return;

        try
        {
            var formViewModel = App.Services.GetRequiredService<RoleFormViewModel>();
            var form = App.Services.GetRequiredService<Views.Identity.RoleComponent.RoleForm>();
            
            formViewModel.ForCreate();
            formViewModel.SaveSuccessCallback = () =>
            {
                form.Close();
                _ = LoadAsync();
            };
            
            form.Owner = System.Windows.Application.Current?.MainWindow;
            form.ShowDialog();
            _operLog?.Information("[RoleView] 新建角色");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[RoleView] 打开新建角色窗口失败");
        }
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private void Update(RoleDto? role)
    {
        // 如果没有传递参数，使用 SelectedRole
        if (role == null)
        {
            role = SelectedRole;
        }

        if (role == null)
        {
            return;
        }

        SelectedRole = role;

        if (App.Services == null) return;

        try
        {
            var formViewModel = App.Services.GetRequiredService<RoleFormViewModel>();
            var form = App.Services.GetRequiredService<Views.Identity.RoleComponent.RoleForm>();
            
            formViewModel.ForUpdate(role);
            formViewModel.SaveSuccessCallback = () =>
            {
                form.Close();
                _ = LoadAsync();
            };
            
            form.Owner = System.Windows.Application.Current?.MainWindow;
            form.ShowDialog();
            _operLog?.Information("[RoleView] 更新角色：Id={RoleId}", role.Id);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[RoleView] 打开更新角色窗口失败，角色Id={RoleId}", role.Id);
        }
    }

    private bool CanUpdate(RoleDto? role)
    {
        // 如果没有传递参数，检查 SelectedRole
        if (role == null)
        {
            role = SelectedRole;
        }
        
        // 如果角色不存在，不能更新
        if (role == null)
        {
            return false;
        }
        
        // 超级角色（super）不允许更新
        if (role.RoleCode == "super")
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(RoleDto? role)
    {
        if (role == null)
        {
            return;
        }

        SelectedRole = role;

        var confirmText = string.Format(_localizationManager.GetString("common.confirm.delete"), _localizationManager.GetString("identity.role.entity"));
        var confirmTitle = _localizationManager.GetString("common.messagebox.question");
        var owner = System.Windows.Application.Current?.MainWindow;
        if (TaktMessageBox.Question(confirmText, confirmTitle, owner) != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var result = await _roleService.DeleteAsync(role.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("identity.role.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("identity.role.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[RoleView] 删除角色成功，操作人={Operator}, 角色Id={RoleId}, 角色代码={RoleCode}", 
                operatorName, role.Id, role.RoleCode ?? string.Empty);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[RoleView] 删除角色失败，Id={RoleId}", role.Id);
        }
    }

    private bool CanDelete(RoleDto? role)
    {
        // 如果没有传递参数，检查 SelectedRole
        if (role == null)
        {
            role = SelectedRole;
        }
        
        // 如果角色不存在，不能删除
        if (role == null)
        {
            return false;
        }
        
        // 超级角色（super）不允许删除
        if (role.RoleCode == "super")
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 分配菜单
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAssignMenu))]
    private async Task AssignMenuAsync(RoleDto? role)
    {
        // 如果没有传递参数，使用 SelectedRole
        if (role == null)
        {
            role = SelectedRole;
        }

        if (role == null)
        {
            return;
        }

        SelectedRole = role;

        try
        {
            var window = _serviceProvider.GetRequiredService<Takt.Fluent.Views.Identity.RoleComponent.RoleAssignMenu>();
            if (window.DataContext is not RoleAssignMenuViewModel formViewModel)
            {
                throw new InvalidOperationException("RoleAssignMenu DataContext 不是 RoleAssignMenuViewModel");
            }

            await formViewModel.InitializeAsync(role);

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
            _operLog?.Error(ex, "[RoleView] 打开分配菜单窗口失败，角色Id={RoleId}", role.Id);
        }
    }

    private bool CanAssignMenu(RoleDto? role)
    {
        // 如果没有传递参数，检查 SelectedRole
        if (role == null)
        {
            role = SelectedRole;
        }
        
        // 如果角色不存在，不能分配菜单
        if (role == null)
        {
            return false;
        }
        
        return true;
    }

    partial void OnSelectedRoleChanged(RoleDto? value)
    {
        // 通知命令系统重新评估所有命令的 CanExecute
        // 这对于工具栏按钮和行操作按钮都很重要
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        AssignMenuCommand.NotifyCanExecuteChanged();
        
        // 同时触发全局命令重新评估，确保行操作按钮也能正确更新
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

}

