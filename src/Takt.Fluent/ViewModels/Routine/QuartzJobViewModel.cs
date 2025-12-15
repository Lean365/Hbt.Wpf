// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：QuartzJobViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务视图模型
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Common.Context;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 任务视图模型
/// </summary>
public partial class QuartzJobViewModel : ObservableObject
{
    private readonly IQuartzJobService _quartzJobService;
    private readonly IQuartzSchedulerManager _quartzSchedulerManager;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<QuartzJobDto> QuartzJobs { get; } = [];

    [ObservableProperty]
    private QuartzJobDto? _selectedQuartzJob;

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

    public QuartzJobViewModel(
        IQuartzJobService quartzJobService,
        IQuartzSchedulerManager quartzSchedulerManager,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _quartzJobService = quartzJobService ?? throw new ArgumentNullException(nameof(quartzJobService));
        _quartzSchedulerManager = quartzSchedulerManager ?? throw new ArgumentNullException(nameof(quartzSchedulerManager));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = _localizationManager.GetString("common.nodata");

        _ = LoadAsync();
    }

    /// <summary>
    /// 加载任务列表
    /// </summary>
    private async Task LoadAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[QuartzJobView] Load jobs: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            var query = new QuartzJobQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _quartzJobService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                QuartzJobs.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("routine.quartzjob.loadfailed");
                return;
            }

            QuartzJobs.Clear();
            foreach (var job in result.Data.Items)
            {
                QuartzJobs.Add(job);
            }

            TotalCount = result.Data.TotalNum;
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(HasPreviousPage));

            // 数据加载完成后，更新命令状态
            RunCommand.NotifyCanExecuteChanged();
            ResumeCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();

            UpdateEmptyMessage();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[QuartzJobView] 加载任务列表失败");
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
    /// 查询命令
    /// </summary>
    [RelayCommand]
    private async Task QueryAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[QuartzJobView] 执行查询操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}",
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
        _operLog?.Information("[QuartzJobView] 执行重置操作，操作人={Operator}", operatorName);

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
        _operLog?.Information("[QuartzJobView] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}",
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
    /// 新建任务
    /// </summary>
    [RelayCommand]
    private void Create()
    {
        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.QuartzJobComponent.QuartzJobForm>();
            if (window.DataContext is not QuartzJobFormViewModel formViewModel)
            {
                throw new InvalidOperationException("QuartzJobForm DataContext 不是 QuartzJobFormViewModel");
            }

            formViewModel.ForCreate();

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadAsync();
            };

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[QuartzJobView] 打开新建任务窗口，操作人={Operator}", operatorName);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[QuartzJobView] 打开新建任务窗口失败");
        }
    }

    /// <summary>
    /// 更新任务
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateAsync(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;

        if (job == null)
        {
            return;
        }

        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.QuartzJobComponent.QuartzJobForm>();
            if (window.DataContext is not QuartzJobFormViewModel formViewModel)
            {
                throw new InvalidOperationException("QuartzJobForm DataContext 不是 QuartzJobFormViewModel");
            }

            SelectedQuartzJob = job;
            await formViewModel.ForUpdateAsync(job.Id);

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadAsync();
            };

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[QuartzJobView] 打开更新任务窗口，操作人={Operator}, 任务Id={JobId}, 任务名称={JobName}",
                operatorName, job.Id, job.JobName ?? string.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[QuartzJobView] 打开更新任务窗口失败");
        }
    }

    private bool CanUpdate(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;
        return job != null;
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;

        if (job == null)
        {
            return;
        }

        SelectedQuartzJob = job;

        var confirmText = string.Format(_localizationManager.GetString("common.confirm.delete"), _localizationManager.GetString("routine.quartzjob.entity"));
        var owner = System.Windows.Application.Current?.MainWindow;
        if (!TaktMessageManager.ShowDeleteConfirm(confirmText, owner))
        {
            return;
        }

        try
        {
            var result = await _quartzJobService.DeleteAsync(job.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("routine.quartzjob.entity");
                var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                TaktMessageManager.ShowError(errorMessage);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("routine.quartzjob.entity");
            var successMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            TaktMessageManager.ShowSuccess(successMessage);
            _operLog?.Information("[QuartzJobView] 删除任务成功，操作人={Operator}, 任务Id={JobId}, 任务名称={JobName}",
                operatorName, job.Id, job.JobName ?? string.Empty);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            TaktMessageManager.ShowError(errorMessage);
            _operLog?.Error(ex, "[QuartzJobView] 删除任务失败，Id={JobId}", job.Id);
        }
    }

    private bool CanDelete(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;
        return job != null;
    }

    partial void OnSelectedQuartzJobChanged(QuartzJobDto? value)
    {
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        RunCommand.NotifyCanExecuteChanged();
        ResumeCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// 立即运行任务
    /// 立即触发任务执行，不变更任务状态
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;

        if (job == null)
        {
            return;
        }

        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[QuartzJobView] 立即运行任务，操作人={Operator}, 任务Id={JobId}, 任务名称={JobName}", 
            operatorName, job.Id, job.JobName ?? string.Empty);

        try
        {
            // 检查任务状态：只能运行启用(0)状态的任务
            if (job.Status != 0)
            {
                var statusText = job.Status switch
                {
                    1 => "禁用",
                    _ => "未知"
                };
                var errorMsg = _localizationManager.GetString("routine.quartzjob.runfailed.invalidstatus") + 
                              $"任务当前状态为{statusText}，无法运行。请先启用任务";
                TaktMessageManager.ShowError(errorMsg);
                return;
            }

            // 触发任务执行（不变更状态）
            if (string.IsNullOrWhiteSpace(job.JobName) || string.IsNullOrWhiteSpace(job.JobGroup))
            {
                var errorMsg = _localizationManager.GetString("routine.quartzjob.runfailed");
                TaktMessageManager.ShowError(errorMsg);
                return;
            }
            var success = await _quartzSchedulerManager.TriggerJobAsync(job.JobName, job.JobGroup);
            if (!success)
            {
                var errorMsg = _localizationManager.GetString("routine.quartzjob.runfailed");
                TaktMessageManager.ShowError(errorMsg);
                return;
            }

            var successMsg = _localizationManager.GetString("routine.quartzjob.runsuccess");
            TaktMessageManager.ShowSuccess(successMsg);
            _operLog?.Information("[QuartzJobView] 立即运行任务成功，操作人={Operator}, 任务Id={JobId}", operatorName, job.Id);
            
            // 刷新列表以更新任务的执行状态（最后执行时间、执行次数等）
            await LoadAsync();
        }
        catch (Exception ex)
        {
            var errorMsg = _localizationManager.GetString("routine.quartzjob.runfailed");
            TaktMessageManager.ShowError($"{errorMsg}: {ex.Message}");
            _operLog?.Error(ex, "[QuartzJobView] 立即运行任务失败，任务Id={JobId}", job.Id);
        }
    }

    private bool CanRun(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;
        if (job == null)
        {
            return false;
        }
        // 只能运行启用(0)状态的任务
        return job.Status == 0;
    }

    /// <summary>
    /// 停止任务（禁用任务）
    /// 禁用任务，任务将不会执行
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopAsync(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;

        if (job == null)
        {
            return;
        }

        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[QuartzJobView] 停止任务，操作人={Operator}, 任务Id={JobId}, 任务名称={JobName}", 
            operatorName, job.Id, job.JobName ?? string.Empty);

        try
        {
            // 检查任务状态：只能停止启用(0)状态的任务
            if (job.Status != 0)
            {
                var statusText = job.Status switch
                {
                    1 => "禁用",
                    _ => "未知"
                };
                var errorMsg = _localizationManager.GetString("routine.quartzjob.stopfailed.invalidstatus") + 
                              $"任务当前状态为{statusText}，无法停止";
                TaktMessageManager.ShowError(errorMsg);
                return;
            }

            // 从调度器中删除任务（禁用）
            if (string.IsNullOrWhiteSpace(job.JobName) || string.IsNullOrWhiteSpace(job.JobGroup))
            {
                var errorMsg = _localizationManager.GetString("routine.quartzjob.stopfailed");
                TaktMessageManager.ShowError(errorMsg);
                return;
            }
            var success = await _quartzSchedulerManager.DeleteJobAsync(job.JobName, job.JobGroup);
            if (!success)
            {
                // 如果任务不存在于调度器中，也继续更新数据库状态
                _operLog?.Warning("[QuartzJobView] 任务可能不存在于调度器中，但继续更新数据库状态");
            }

            // 更新数据库状态为禁用（1）
            var statusDto = new QuartzJobStatusDto { Id = job.Id, Status = 1 };
            var result = await _quartzJobService.StatusAsync(statusDto);
            if (!result.Success)
            {
                TaktMessageManager.ShowError(result.Message ?? _localizationManager.GetString("routine.quartzjob.stopfailed.updatestatus"));
                return;
            }

            var successMsg = _localizationManager.GetString("routine.quartzjob.stopsuccess");
            TaktMessageManager.ShowSuccess(successMsg);
            _operLog?.Information("[QuartzJobView] 停止任务成功，操作人={Operator}, 任务Id={JobId}", operatorName, job.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            var errorMsg = _localizationManager.GetString("routine.quartzjob.stopfailed");
            TaktMessageManager.ShowError($"{errorMsg}: {ex.Message}");
            _operLog?.Error(ex, "[QuartzJobView] 停止任务失败，任务Id={JobId}", job.Id);
        }
    }

    private bool CanStop(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;
        if (job == null)
        {
            return false;
        }
        // 只能停止启用(0)状态的任务
        return job.Status == 0;
    }

    /// <summary>
    /// 恢复任务（启用任务）
    /// 启用任务，任务将根据日程自动执行
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanResume))]
    private async Task ResumeAsync(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;

        if (job == null)
        {
            return;
        }

        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[QuartzJobView] 恢复任务，操作人={Operator}, 任务Id={JobId}, 任务名称={JobName}", 
            operatorName, job.Id, job.JobName ?? string.Empty);

        try
        {
            // 检查任务状态：只能恢复禁用(1)状态的任务
            if (job.Status != 1)
            {
                var statusText = job.Status switch
                {
                    0 => "启用",
                    _ => "未知"
                };
                var errorMsg = _localizationManager.GetString("routine.quartzjob.resumefailed.invalidstatus") + 
                              $"任务当前状态为{statusText}，无法恢复。请先禁用任务";
                TaktMessageManager.ShowError(errorMsg);
                return;
            }

            // 重新加载任务到调度器
            if (string.IsNullOrWhiteSpace(job.JobName) || string.IsNullOrWhiteSpace(job.JobGroup) ||
                string.IsNullOrWhiteSpace(job.TriggerName) || string.IsNullOrWhiteSpace(job.TriggerGroup) ||
                string.IsNullOrWhiteSpace(job.CronExpression) || string.IsNullOrWhiteSpace(job.JobClassName))
            {
                var errorMsg = _localizationManager.GetString("routine.quartzjob.resumefailed");
                TaktMessageManager.ShowError(errorMsg);
                return;
            }
            var success = await _quartzSchedulerManager.AddJobAsync(
                job.JobName,
                job.JobGroup,
                job.TriggerName,
                job.TriggerGroup,
                job.CronExpression,
                job.JobClassName,
                job.JobParams);
            
            if (!success)
            {
                var errorMsg = _localizationManager.GetString("routine.quartzjob.resumefailed");
                TaktMessageManager.ShowError(errorMsg);
                return;
            }

            // 更新数据库状态为启用(0)
            var statusDto = new QuartzJobStatusDto { Id = job.Id, Status = 0 };
            var result = await _quartzJobService.StatusAsync(statusDto);
            if (!result.Success)
            {
                TaktMessageManager.ShowError(result.Message ?? _localizationManager.GetString("routine.quartzjob.resumefailed.updatestatus"));
                return;
            }

            var successMsg = _localizationManager.GetString("routine.quartzjob.resumesuccess");
            TaktMessageManager.ShowSuccess(successMsg);
            _operLog?.Information("[QuartzJobView] 恢复任务成功，操作人={Operator}, 任务Id={JobId}", operatorName, job.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            var errorMsg = _localizationManager.GetString("routine.quartzjob.resumefailed");
            TaktMessageManager.ShowError($"{errorMsg}: {ex.Message}");
            _operLog?.Error(ex, "[QuartzJobView] 恢复任务失败，任务Id={JobId}", job.Id);
        }
    }

    private bool CanResume(QuartzJobDto? job)
    {
        job ??= SelectedQuartzJob;
        if (job == null)
        {
            return false;
        }
        // 只能恢复禁用(1)状态的任务
        return job.Status == 1;
    }
}

