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
using System.IO;
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

        EmptyMessage = _localizationManager.GetString("common.nodata");

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
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _userService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                Users.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("identity.user.loadfailed");
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

        EmptyMessage = _localizationManager.GetString("common.nodata");
    }

    /// <summary>
    /// 查询命令（来自自定义表格）
    /// </summary>
    [RelayCommand]
    private async Task QueryAsync(QueryContext context)
    {
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[UserView] 执行查询操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}", 
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
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[UserView] 执行重置操作，操作人={Operator}", operatorName);
        
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
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[UserView] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}", 
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
    /// 新建用户
    /// </summary>
    [RelayCommand]
    private void Create()
    {
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[UserView] 打开新建用户窗口，操作人={Operator}", operatorName);
        
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

        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[UserView] 打开更新用户窗口，操作人={Operator}, 用户Id={UserId}, 用户名={Username}", 
            operatorName, user.Id, user.Username ?? string.Empty);

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

        var confirmText = string.Format(_localizationManager.GetString("common.confirm.delete"), _localizationManager.GetString("identity.user.entity"));
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
                var entityName = _localizationManager.GetString("identity.user.entity");
                var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                TaktMessageManager.ShowError(errorMessage);
                return;
            }

            var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("identity.user.entity");
            var successMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            TaktMessageManager.ShowSuccess(successMessage);
            _operLog?.Information("[UserView] 删除用户成功，操作人={Operator}, 用户Id={UserId}, 用户名={Username}", 
                operatorName, user.Id, user.Username ?? string.Empty);
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
        ErrorMessage = _localizationManager.GetString("identity.user.authorizenotimplemented");
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

    /// <summary>
    /// 导出用户
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            _operLog?.Information("[UserView] 开始导出用户，操作人={Operator}", operatorName);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"用户导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                Title = "保存Excel文件"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            IsLoading = true;
            var loadingMessage = _localizationManager.GetString("common.exporting");
            TaktMessageManager.ShowInformation(loadingMessage);

            var query = new UserQueryDto
            {
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _userService.ExportAsync(query);

            if (result.Success && result.Data.content != null)
            {
                await File.WriteAllBytesAsync(dialog.FileName, result.Data.content);
                TaktMessageManager.ShowSuccess(result.Message ?? _localizationManager.GetString("common.success.export"));
                _operLog?.Information("[UserView] 导出用户成功，操作人={Operator}, 文件路径={FilePath}, 记录数={Count}", 
                    operatorName, dialog.FileName, result.Data.content.Length);
            }
            else
            {
                TaktMessageManager.ShowError(result.Message ?? _localizationManager.GetString("common.error.export"));
                _operLog?.Warning("[UserView] 导出用户失败，操作人={Operator}, 错误={Message}", 
                    operatorName, result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserView] 导出用户异常");
            TaktMessageManager.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        try
        {
            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            _operLog?.Information("[UserView] 开始下载用户导入模板，操作人={Operator}", operatorName);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"用户导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                Title = "保存Excel模板文件"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            IsLoading = true;
            var result = await _userService.GetTemplateAsync();

            if (result.Success && result.Data.content != null)
            {
                await File.WriteAllBytesAsync(dialog.FileName, result.Data.content);
                TaktMessageManager.ShowSuccess(result.Message ?? _localizationManager.GetString("common.success.download"));
                _operLog?.Information("[UserView] 下载用户导入模板成功，操作人={Operator}, 文件路径={FilePath}", 
                    operatorName, dialog.FileName);
            }
            else
            {
                TaktMessageManager.ShowError(result.Message ?? _localizationManager.GetString("common.error.download"));
                _operLog?.Warning("[UserView] 下载用户导入模板失败，操作人={Operator}, 错误={Message}", 
                    operatorName, result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserView] 下载用户导入模板异常");
            TaktMessageManager.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 导入用户
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            _operLog?.Information("[UserView] 开始导入用户，操作人={Operator}", operatorName);

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "选择要导入的Excel文件"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            IsLoading = true;
            var loadingMessage = _localizationManager.GetString("common.importing");
            TaktMessageManager.ShowInformation(loadingMessage);

            using var fileStream = File.OpenRead(dialog.FileName);
            var result = await _userService.ImportAsync(fileStream);

            if (result.Success && result.Data.success > 0)
            {
                var successMessage = string.Format(
                    _localizationManager.GetString("common.success.import"),
                    result.Data.success, result.Data.fail);
                TaktMessageManager.ShowSuccess(successMessage);
                _operLog?.Information("[UserView] 导入用户成功，操作人={Operator}, 文件路径={FilePath}, 成功={Success}, 失败={Fail}", 
                    operatorName, dialog.FileName, result.Data.success, result.Data.fail);
                await LoadAsync();
            }
            else
            {
                var errorMessage = result.Message ?? string.Format(
                    _localizationManager.GetString("common.error.import"),
                    result.Data.success, result.Data.fail);
                TaktMessageManager.ShowError(errorMessage);
                _operLog?.Warning("[UserView] 导入用户失败，操作人={Operator}, 文件路径={FilePath}, 错误={Message}", 
                    operatorName, dialog.FileName, result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserView] 导入用户异常");
            TaktMessageManager.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
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

