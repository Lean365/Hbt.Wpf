// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logging
// 文件名称：LoginLogViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：登录日志视图模型
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
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

namespace Takt.Fluent.ViewModels.Logging;

/// <summary>
/// 登录日志视图模型
/// </summary>
public partial class LoginLogViewModel : ObservableObject
{
    private readonly ILoginLogService _loginLogService;
    private readonly ILogCleanupService _logCleanupService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<LoginLogDto> LoginLogs { get; } = new();

    [ObservableProperty]
    private LoginLogDto? _selectedLoginLog;

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

    public LoginLogViewModel(
        ILoginLogService loginLogService,
        ILogCleanupService logCleanupService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _loginLogService = loginLogService ?? throw new ArgumentNullException(nameof(loginLogService));
        _logCleanupService = logCleanupService ?? throw new ArgumentNullException(nameof(logCleanupService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = _localizationManager.GetString("common.nodata");

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // ILocalizationManager 初始化在应用启动时完成，无需在此初始化

        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[LoginLogView] Load login logs: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            // 构建查询DTO
            var query = new LoginLogQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _loginLogService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                LoginLogs.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logging.loginlog.loadfailed");
                return;
            }

            LoginLogs.Clear();
            foreach (var log in result.Data.Items)
            {
                LoginLogs.Add(log);
            }

            TotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[LoginLogView] Load login logs failed");
            ErrorMessage = _localizationManager.GetString("logging.loginlog.loadfailed");
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
        _operLog?.Information("[LoginLogView] 执行查询操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}", 
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
        _operLog?.Information("[LoginLogView] 执行重置操作，操作人={Operator}", operatorName);
        
        Keyword = string.Empty;
        PageIndex = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task PageChangedAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[LoginLogView] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}", 
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
            _operLog?.Information("[LoginLogView] 开始导出登录日志，操作人={Operator}", operatorName);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"登录日志导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                Title = "保存Excel文件"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            IsLoading = true;
            var loadingMessage = _localizationManager.GetString("common.exporting");
            TaktMessageManager.ShowInformation(loadingMessage);

            var query = new LoginLogQueryDto
            {
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _loginLogService.ExportAsync(query);

            if (result.Success && result.Data.content != null)
            {
                await System.IO.File.WriteAllBytesAsync(dialog.FileName, result.Data.content);
                TaktMessageManager.ShowSuccess(result.Message ?? _localizationManager.GetString("common.success.export"));
                _operLog?.Information("[LoginLogView] 导出登录日志成功，操作人={Operator}, 文件路径={FilePath}, 记录数={Count}", 
                    operatorName, dialog.FileName, result.Data.content.Length);
            }
            else
            {
                TaktMessageManager.ShowError(result.Message ?? _localizationManager.GetString("common.error.export"));
                _operLog?.Warning("[LoginLogView] 导出登录日志失败，操作人={Operator}, 错误={Message}", 
                    operatorName, result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[LoginLogView] 导出登录日志异常");
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
            _operLog?.Information("[LoginLogView] Start cleaning up logs, range={Range}", range);

            var loadingMessage = _localizationManager.GetString("logging.cleanup.inprogress");
            TaktMessageManager.ShowInformation(loadingMessage);

            var result = await _logCleanupService.CleanupLogsAsync(LogType.LoginLog, range);

            if (result.Success && result.Data != null)
            {
                var successMessage = string.Format(
                    _localizationManager.GetString("logging.cleanup.success"),
                    result.Data.CleanedDatabaseLogCount);
                
                TaktMessageManager.ShowSuccess(successMessage);
                _operLog?.Information("[LoginLogView] Log cleanup successful: {RecordCount} records", 
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
                _operLog?.Warning("[LoginLogView] Log cleanup failed: {Message}", result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[LoginLogView] Log cleanup exception");
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

