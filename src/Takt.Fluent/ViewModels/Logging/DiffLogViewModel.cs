// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logging
// 文件名称：DiffLogViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：差异日志视图模型
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
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Logging;

/// <summary>
/// 差异日志视图模型
/// </summary>
public partial class DiffLogViewModel : ObservableObject
{
    private readonly IDiffLogService _diffLogService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<DiffLogDto> DiffLogs { get; } = new();

    [ObservableProperty]
    private DiffLogDto? _selectedDiffLog;

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

    public DiffLogViewModel(
        IDiffLogService diffLogService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _diffLogService = diffLogService ?? throw new ArgumentNullException(nameof(diffLogService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = GetTranslation("common.noData", "暂无数据");

        _ = InitializeAsync();
    }

    /// <summary>
    /// 获取翻译文本（如果找不到翻译，返回默认值）
    /// </summary>
    private string GetTranslation(string key, string defaultValue)
    {
        var translation = _localizationManager.GetString(key);
        return (translation == key) ? defaultValue : translation;
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
            _operLog?.Information("[DiffLogView] Load diff logs: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            // 构建查询DTO
            var query = new DiffLogQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim()
            };

            var result = await _diffLogService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                DiffLogs.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? GetTranslation("Logging.DiffLog.LoadFailed", "加载差异日志数据失败");
                return;
            }

            DiffLogs.Clear();
            foreach (var log in result.Data.Items)
            {
                DiffLogs.Add(log);
            }

            TotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[DiffLogView] Load diff logs failed");
            ErrorMessage = GetTranslation("Logging.DiffLog.LoadFailed", "加载差异日志数据失败");
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

    [RelayCommand]
    private void ExportAsync()
    {
        // TODO: 实现导出功能
        _operLog?.Information("[DiffLogView] Export diff logs");
    }
}

