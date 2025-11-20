// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：SettingViewModel.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：系统设置管理视图模型
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 系统设置管理视图模型
/// </summary>
public partial class SettingViewModel : ObservableObject
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<SettingDto> Settings { get; } = new();

    [ObservableProperty]
    private SettingDto? _selectedSetting;

    [ObservableProperty]
    private int _selectedItemsCount;

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

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public SettingViewModel(ISettingService settingService,
                            ILocalizationManager localizationManager,
                            OperLogManager? operLog = null)
    {
        _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        _ = LoadAsync();
    }

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
            _operLog?.Information("[SettingView] Load settings: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            // 构建查询DTO
            var query = new SettingQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim()
            };

            var result = await _settingService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                Settings.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Routine.Setting.LoadFailed") ?? "加载系统设置失败";
                return;
            }

            Settings.Clear();
            foreach (var item in result.Data.Items)
            {
                Settings.Add(item);
            }

            TotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SettingView] 加载系统设置失败");
        }
        finally
        {
            IsLoading = false;
        }
    }

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

    [RelayCommand]
    private async Task ResetAsync(QueryContext context)
    {
        Keyword = string.Empty;
        PageIndex = 1;
        await LoadAsync();
    }

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

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private void Update(SettingDto? setting)
    {
        // 如果没有传递参数，使用 SelectedSetting
        if (setting == null)
        {
            setting = SelectedSetting;
        }

        if (setting == null)
        {
            return;
        }

        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.SettingComponent.SettingForm>();
            if (window.DataContext is not SettingFormViewModel formViewModel)
            {
                throw new InvalidOperationException("SettingForm DataContext 不是 SettingFormViewModel");
            }

            formViewModel.ForUpdate(setting);

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
            _operLog?.Error(ex, "[SettingView] 打开编辑窗口失败");
        }
    }

    private bool CanUpdate(SettingDto? setting)
    {
        // 如果没有传递参数，检查 SelectedSetting
        if (setting == null)
        {
            return SelectedSetting is not null;
        }
        return setting is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(SettingDto? setting)
    {
        if (setting == null)
        {
            return;
        }

        try
        {
            var confirmMessage = _localizationManager.GetString("Routine.Setting.DeleteConfirm") ?? "确定要删除该系统设置吗？";
            var confirmTitle = _localizationManager.GetString("common.confirm") ?? "确认";
            var result = System.Windows.MessageBox.Show(
                confirmMessage,
                confirmTitle,
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            var deleteResult = await _settingService.DeleteAsync(setting.Id);
            if (!deleteResult.Success)
            {
                ErrorMessage = deleteResult.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SettingView] 删除系统设置失败");
        }
    }

    private bool CanDelete(SettingDto? setting) => setting is not null;

    [RelayCommand]
    private void Create()
    {
        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.SettingComponent.SettingForm>();
            if (window.DataContext is not SettingFormViewModel formViewModel)
            {
                throw new InvalidOperationException("SettingForm DataContext 不是 SettingFormViewModel");
            }

            formViewModel.ForCreate();

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
            _operLog?.Error(ex, "[SettingView] 打开新建窗口失败");
        }
    }

    partial void OnSelectedSettingChanged(SettingDto? value)
    {
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedItemsCountChanged(int value)
    {
        // 移除 SelectedItemsCount 对 UpdateCommand 的影响，因为 Update 命令只依赖 SelectedSetting
        DeleteCommand.NotifyCanExecuteChanged();
    }
}
