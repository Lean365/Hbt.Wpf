//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : UserViewModel.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-30
// 版本号 : 1.0
// 描述    : 用户管理视图模型
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hbt.Application.Dtos.Identity;
using Hbt.Application.Services.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Hbt.Fluent.ViewModels.Identity;

public partial class UserViewModel : ObservableObject
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = new();

    [ObservableProperty]
    private UserDto? _selectedUser;

    // 分页
    [ObservableProperty]
    private int _pageIndex = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    [ObservableProperty]
    private int _total = 0;

    public string TotalText => $"共 {Total} 条";
    public string PageDisplay => $"{PageIndex} / {Math.Max(1, (int)Math.Ceiling((double)Total / Math.Max(1, PageSize)))}";

    public UserViewModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task LoadAsync()
    {
        await LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync(object? _ = null)
    {
        var result = await _userService.GetListAsync(PageIndex, PageSize, Keyword);
        if (result.Success && result.Data != null)
        {
            Users = new ObservableCollection<UserDto>(result.Data.Items);
            Total = result.Data.TotalNum;
            OnPropertyChanged(nameof(TotalText));
            OnPropertyChanged(nameof(PageDisplay));
        }
    }

    [RelayCommand]
    private async Task QueryAsync()
    {
        PageIndex = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        Keyword = string.Empty;
        PageIndex = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private Task ToggleAdvancedAsync() => Task.CompletedTask; // 预留：打开高级查询面板

    [RelayCommand]
    private void CreateAsync()
    {
        try
        {
            var services = App.Services;
            if (services == null)
            {
                return;
            }

            // 通过依赖注入创建 ViewModel 和 LanguageService
            var viewModel = services.GetService<UserFormViewModel>();
            var languageService = services.GetService<Hbt.Fluent.Services.LanguageService>();
            if (viewModel == null || languageService == null)
            {
                return;
            }

            // 设置为新建模式
            viewModel.ForCreate();

            // 通过依赖注入创建 UserFormWindow（需要 ViewModel 和 LanguageService）
            var userFormWindow = ActivatorUtilities.CreateInstance<Views.Identity.UserFormWindow>(services, viewModel, languageService);

            // 设置窗口位置（相对于主窗口）
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                userFormWindow.Left = mainWindow.Left + 50;
                userFormWindow.Top = mainWindow.Top + 50;
            }

            // 订阅窗口关闭事件，关闭后刷新列表
            userFormWindow.Closed += (sender, e) =>
            {
                _ = LoadAsync();
            };

            // 将关闭窗口的回调传递给 ViewModel，保存成功后自动关闭窗口
            viewModel.SaveSuccessCallback = () =>
            {
                userFormWindow.Close();
            };

            // 显示非模态窗口（与主窗体平行）
            userFormWindow.Show();
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[用户] 打开新建用户窗体失败");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private void EditAsync()
    {
        if (SelectedUser == null) return;

        try
        {
            var services = App.Services;
            if (services == null)
            {
                return;
            }

            // 通过依赖注入创建 ViewModel 和 LanguageService
            var viewModel = services.GetService<UserFormViewModel>();
            var languageService = services.GetService<Hbt.Fluent.Services.LanguageService>();
            if (viewModel == null || languageService == null)
            {
                return;
            }

            // 设置为编辑模式，传入选中的用户数据
            viewModel.ForUpdate(SelectedUser);

            // 通过依赖注入创建 UserFormWindow（需要 ViewModel 和 LanguageService）
            var userFormWindow = ActivatorUtilities.CreateInstance<Views.Identity.UserFormWindow>(services, viewModel, languageService);

            // 设置窗口位置（相对于主窗口）
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                userFormWindow.Left = mainWindow.Left + 50;
                userFormWindow.Top = mainWindow.Top + 50;
            }

            // 订阅窗口关闭事件，关闭后刷新列表
            userFormWindow.Closed += (sender, e) =>
            {
                _ = LoadAsync();
            };

            // 将关闭窗口的回调传递给 ViewModel，保存成功后自动关闭窗口
            viewModel.SaveSuccessCallback = () =>
            {
                userFormWindow.Close();
            };

            // 显示非模态窗口（与主窗体平行）
            userFormWindow.Show();
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[用户] 打开编辑用户窗体失败");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedUser == null) return;
        var result = await _userService.DeleteAsync(SelectedUser.Id);
        if (result.Success)
        {
            await LoadAsync();
        }
    }

    private bool CanEditOrDelete() => SelectedUser != null;

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (PageIndex > 1)
        {
            PageIndex--;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)Total / Math.Max(1, PageSize)));
        if (PageIndex < totalPages)
        {
            PageIndex++;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private Task ImportAsync() => Task.CompletedTask; // 预留

    [RelayCommand]
    private Task ExportAsync() => Task.CompletedTask; // 预留

    [RelayCommand]
    private Task ToggleColumnsAsync() => Task.CompletedTask; // 预留

    [RelayCommand]
    private Task ToggleQueryBarAsync() => Task.CompletedTask; // 预留

    // 列显隐配置
    [ObservableProperty] private bool _showId = true;
    [ObservableProperty] private bool _showUsername = true;
    [ObservableProperty] private bool _showRealName = true;
    [ObservableProperty] private bool _showEmail = true;
    [ObservableProperty] private bool _showPhone = true;
    [ObservableProperty] private bool _showUserType = true;
    [ObservableProperty] private bool _showUserGender = true;
    [ObservableProperty] private bool _showStatus = true;
}


