// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logging
// 文件名称：QuartzJobLogViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务日志视图模型
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logging;
using Takt.Application.Services.Logging;
using LogCleanupRange = Takt.Application.Services.Logging.LogCleanupRange;
using LogType = Takt.Application.Services.Logging.LogType;
using Takt.Common.Context;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;
using TaktMessageManager = Takt.Fluent.Controls.TaktMessageManager;

namespace Takt.Fluent.ViewModels.Logging;

/// <summary>
/// 任务日志视图模型
/// </summary>
public partial class QuartzJobLogViewModel : ObservableObject
{
    private readonly IQuartzJobLogService _quartzJobLogService;
    private readonly ILogCleanupService _logCleanupService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<QuartzJobLogDto> QuartzJobLogs { get; } = new();

    [ObservableProperty]
    private QuartzJobLogDto? _selectedQuartzJobLog;

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
    private string _emptyMessage = string.Empty;

    public QuartzJobLogViewModel(
        IQuartzJobLogService quartzJobLogService,
        ILogCleanupService logCleanupService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _quartzJobLogService = quartzJobLogService ?? throw new ArgumentNullException(nameof(quartzJobLogService));
        _logCleanupService = logCleanupService ?? throw new ArgumentNullException(nameof(logCleanupService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = _localizationManager.GetString("common.nodata");

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[QuartzJobLogView] Load logs: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            var query = new QuartzJobLogQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _quartzJobLogService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                QuartzJobLogs.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logging.quartzjoblog.loadfailed");
                return;
            }

            QuartzJobLogs.Clear();
            foreach (var log in result.Data.Items)
            {
                QuartzJobLogs.Add(log);
            }

            TotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[QuartzJobLogView] Load logs failed");
            ErrorMessage = _localizationManager.GetString("logging.quartzjoblog.loadfailed");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task QueryAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[QuartzJobLogView] 执行查询操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}",
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

    [RelayCommand]
    private async Task ResetAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[QuartzJobLogView] 执行重置操作，操作人={Operator}", operatorName);

        Keyword = string.Empty;
        PageIndex = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task PageChangedAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[QuartzJobLogView] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}",
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

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            _operLog?.Information("[QuartzJobLogView] 开始导出任务日志，操作人={Operator}", operatorName);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"任务日志导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                Title = "保存Excel文件"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            IsLoading = true;
            var loadingMessage = _localizationManager.GetString("common.exporting");
            TaktMessageManager.ShowInformation(loadingMessage);

            var query = new QuartzJobLogQueryDto
            {
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _quartzJobLogService.ExportAsync(query);

            if (result.Success && result.Data.content != null)
            {
                await System.IO.File.WriteAllBytesAsync(dialog.FileName, result.Data.content);
                TaktMessageManager.ShowSuccess(result.Message ?? _localizationManager.GetString("common.success.export"));
                _operLog?.Information("[QuartzJobLogView] 导出任务日志成功，操作人={Operator}, 文件路径={FilePath}, 记录数={Count}", 
                    operatorName, dialog.FileName, result.Data.content.Length);
            }
            else
            {
                TaktMessageManager.ShowError(result.Message ?? _localizationManager.GetString("common.error.export"));
                _operLog?.Warning("[QuartzJobLogView] 导出任务日志失败，操作人={Operator}, 错误={Message}", 
                    operatorName, result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[QuartzJobLogView] 导出任务日志异常");
            TaktMessageManager.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearAsync(object? parameter)
    {
        // 参数可以是 LogCleanupRange 枚举值（int）或字符串
        LogCleanupRange range = LogCleanupRange.SevenDays;
        
        if (parameter != null)
        {
            if (parameter is LogCleanupRange rangeEnum)
            {
                range = rangeEnum;
            }
            else if (parameter is int rangeInt)
            {
                range = (LogCleanupRange)rangeInt;
            }
            else if (parameter is string rangeStr && int.TryParse(rangeStr, out var parsedInt))
            {
                range = (LogCleanupRange)parsedInt;
            }
        }

        var rangeText = GetRangeText(range);
        var confirmMessage = string.Format(
            _localizationManager.GetString("logging.cleanup.confirm"),
            rangeText);
        var owner = System.Windows.Application.Current?.MainWindow;
        
        if (owner == null || !TaktMessageManager.ShowDeleteConfirm(confirmMessage, owner))
        {
            return;
        }

        try
        {
            IsLoading = true;
            _operLog?.Information("[QuartzJobLogView] Start cleaning up logs, range={Range}", range);

            var loadingMessage = _localizationManager.GetString("logging.cleanup.inprogress");
            TaktMessageManager.ShowInformation(loadingMessage);

            var result = await _logCleanupService.CleanupLogsAsync(LogType.QuartzJobLog, range);

            if (result.Success && result.Data != null)
            {
                var successMessage = string.Format(
                    _localizationManager.GetString("logging.cleanup.success"),
                    result.Data.CleanedDatabaseLogCount);
                
                TaktMessageManager.ShowSuccess(successMessage);
                _operLog?.Information("[QuartzJobLogView] Log cleanup successful: {RecordCount} records", 
                    result.Data.CleanedDatabaseLogCount);

                // 清理成功后重新加载数据
                await LoadAsync();
            }
            else
            {
                var errorMessage = string.Format(
                    _localizationManager.GetString("logging.cleanup.failed"),
                    result.Message ?? "未知错误");
                TaktMessageManager.ShowError(errorMessage);
                _operLog?.Warning("[QuartzJobLogView] Log cleanup failed: {Message}", result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[QuartzJobLogView] Log cleanup exception");
            var errorMessage = string.Format(
                _localizationManager.GetString("logging.cleanup.failed"),
                ex.Message);
            TaktMessageManager.ShowError(errorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetRangeText(LogCleanupRange range)
    {
        return range switch
        {
            LogCleanupRange.Today => _localizationManager.GetString("logging.cleanup.range.today"),
            LogCleanupRange.SevenDays => _localizationManager.GetString("logging.cleanup.range.sevendays"),
            LogCleanupRange.ThirtyDays => _localizationManager.GetString("logging.cleanup.range.thirtydays"),
            LogCleanupRange.All => _localizationManager.GetString("logging.cleanup.range.all"),
            _ => _localizationManager.GetString("logging.cleanup.range.sevendays")
        };
    }
}

