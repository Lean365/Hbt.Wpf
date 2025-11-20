// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visitors
// 文件名称：VisitorViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客视图模型（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logistics.Visitors;
using Takt.Application.Services.Logistics.Visitors;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Logistics.Visitors;

/// <summary>
/// 访客视图模型（主子表视图）
/// 主表：Visitor（访客）
/// 子表：VisitorDetail（访客详情）
/// </summary>
public partial class VisitorViewModel : ObservableObject
{
    private readonly IVisitorService _visitorService;
    private readonly IVisitorDetailService _visitorDetailService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<VisitorDto> Visitors { get; } = new();

    [ObservableProperty]
    private VisitorDto? _selectedVisitor;

    // 子表数据
    public ObservableCollection<VisitorDetailDto> VisitorDetails { get; } = new();

    [ObservableProperty]
    private VisitorDetailDto? _selectedVisitorDetail;

    // 主表查询相关
    [ObservableProperty]
    private string _visitorKeyword = string.Empty;

    [ObservableProperty]
    private int _visitorPageIndex = 1;

    [ObservableProperty]
    private int _visitorPageSize = 20;

    [ObservableProperty]
    private int _visitorTotalCount;

    // 子表查询相关
    [ObservableProperty]
    private string _detailKeyword = string.Empty;

    [ObservableProperty]
    private int _detailPageIndex = 1;

    [ObservableProperty]
    private int _detailPageSize = 20;

    [ObservableProperty]
    private int _detailTotalCount;

    [ObservableProperty]
    private bool _isLoadingVisitors;

    [ObservableProperty]
    private bool _isLoadingDetails;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public VisitorViewModel(
        IVisitorService visitorService,
        IVisitorDetailService visitorDetailService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _visitorService = visitorService ?? throw new ArgumentNullException(nameof(visitorService));
        _visitorDetailService = visitorDetailService ?? throw new ArgumentNullException(nameof(visitorDetailService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        _ = LoadVisitorsAsync();
    }

    partial void OnSelectedVisitorChanged(VisitorDto? value)
    {
        _ = LoadDetailsAsync();
    }

    private async Task LoadVisitorsAsync()
    {
        if (IsLoadingVisitors) return;

        IsLoadingVisitors = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[VisitorView] Load visitors: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                VisitorPageIndex, VisitorPageSize, VisitorKeyword);

            // 构建查询DTO
            var query = new VisitorQueryDto
            {
                PageIndex = VisitorPageIndex,
                PageSize = VisitorPageSize,
                Keywords = string.IsNullOrWhiteSpace(VisitorKeyword) ? null : VisitorKeyword.Trim()
            };

            var result = await _visitorService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                Visitors.Clear();
                VisitorTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Logistics.Visitor.LoadFailed") ?? "加载访客数据失败";
                return;
            }

            Visitors.Clear();
            foreach (var visitor in result.Data.Items)
            {
                Visitors.Add(visitor);
            }

            VisitorTotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 加载访客列表失败");
        }
        finally
        {
            IsLoadingVisitors = false;
        }
    }

    private async Task LoadDetailsAsync()
    {
        if (SelectedVisitor == null)
        {
            VisitorDetails.Clear();
            DetailTotalCount = 0;
            return;
        }

        if (IsLoadingDetails) return;

        IsLoadingDetails = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[VisitorView] Load visitor details: visitorId={VisitorId}, pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SelectedVisitor.Id, DetailPageIndex, DetailPageSize, DetailKeyword);

            var query = new VisitorDetailQueryDto
            {
                VisitorId = SelectedVisitor.Id,
                Name = string.IsNullOrWhiteSpace(DetailKeyword) ? null : DetailKeyword.Trim(),
                Department = string.IsNullOrWhiteSpace(DetailKeyword) ? null : DetailKeyword.Trim(),
                PageIndex = DetailPageIndex,
                PageSize = DetailPageSize
            };

            var result = await _visitorDetailService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                VisitorDetails.Clear();
                foreach (var detail in result.Data.Items)
                {
                    VisitorDetails.Add(detail);
                }

                DetailTotalCount = result.Data.TotalNum;
            }
            else
            {
                VisitorDetails.Clear();
                DetailTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Logistics.Visitor.LoadDetailsFailed") ?? "加载访客详情失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 加载访客详情失败");
        }
        finally
        {
            IsLoadingDetails = false;
        }
    }

    [RelayCommand]
    private async Task QueryVisitorsAsync(QueryContext context)
    {
        VisitorKeyword = context.Keyword;
        if (VisitorPageIndex != context.PageIndex)
        {
            VisitorPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (VisitorPageSize != context.PageSize)
        {
            VisitorPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadVisitorsAsync();
    }

    [RelayCommand]
    private async Task ResetVisitorsAsync()
    {
        VisitorKeyword = string.Empty;
        VisitorPageIndex = 1;
        VisitorPageSize = 20;
        await LoadVisitorsAsync();
    }

    [RelayCommand]
    private async Task PageChangedVisitorsAsync(PageRequest request)
    {
        VisitorPageIndex = request.PageIndex;
        VisitorPageSize = request.PageSize;
        await LoadVisitorsAsync();
    }

    [RelayCommand]
    private async Task QueryDetailsAsync(QueryContext context)
    {
        DetailKeyword = context.Keyword;
        if (DetailPageIndex != context.PageIndex)
        {
            DetailPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (DetailPageSize != context.PageSize)
        {
            DetailPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadDetailsAsync();
    }

    [RelayCommand]
    private async Task ResetDetailsAsync()
    {
        DetailKeyword = string.Empty;
        DetailPageIndex = 1;
        DetailPageSize = 20;
        await LoadDetailsAsync();
    }

    [RelayCommand]
    private async Task PageChangedDetailsAsync(PageRequest request)
    {
        DetailPageIndex = request.PageIndex;
        DetailPageSize = request.PageSize;
        await LoadDetailsAsync();
    }

    [RelayCommand]
    private void CreateVisitor()
    {
        // TODO: 实现创建功能
        ErrorMessage = "创建功能待实现";
    }

    [RelayCommand]
    private void UpdateVisitor(VisitorDto? visitor)
    {
        if (visitor == null)
        {
            visitor = SelectedVisitor;
        }

        if (visitor == null)
        {
            return;
        }

        // TODO: 实现更新功能
        ErrorMessage = "更新功能待实现";
    }

    [RelayCommand]
    private async Task DeleteVisitorAsync(VisitorDto? visitor)
    {
        if (visitor == null)
        {
            visitor = SelectedVisitor;
        }

        if (visitor == null)
        {
            return;
        }

        try
        {
            var result = await _visitorService.DeleteAsync(visitor.Id);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            await LoadVisitorsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 删除访客失败");
        }
    }

    [RelayCommand]
    private void CreateDetail()
    {
        if (SelectedVisitor == null)
        {
            ErrorMessage = "请先选择访客";
            return;
        }

        // TODO: 实现创建功能
        ErrorMessage = "创建功能待实现";
    }

    [RelayCommand]
    private void UpdateDetail(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedVisitorDetail;
        }

        if (detail == null)
        {
            return;
        }

        // TODO: 实现更新功能
        ErrorMessage = "更新功能待实现";
    }

    [RelayCommand]
    private async Task DeleteDetailAsync(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedVisitorDetail;
        }

        if (detail == null)
        {
            return;
        }

        try
        {
            var result = await _visitorDetailService.DeleteAsync(detail.Id);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            await LoadDetailsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 删除访客详情失败");
        }
    }
}

