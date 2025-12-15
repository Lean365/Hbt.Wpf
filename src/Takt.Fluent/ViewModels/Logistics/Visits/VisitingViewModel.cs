// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visits
// 文件名称：VisitingViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员视图模型（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logistics.Visits;
using Takt.Application.Services.Logistics.Visits;
using PageRequest = Takt.Fluent.Controls.PageRequest;
using QueryContext = Takt.Fluent.Controls.QueryContext;

namespace Takt.Fluent.ViewModels.Logistics.Visits;

/// <summary>
/// 来访公司视图模型（主子表视图）
/// 主表：VisitingCompany（来访公司）
/// 子表：VisitingEntourage（来访成员详情）
/// </summary>
public partial class VisitingViewModel : ObservableObject
{
    private readonly IVisitingCompanyService _visitingCompanyService;
    private readonly IVisitingEntourageService _visitingEntourageService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<VisitingCompanyDto> VisitingCompanies { get; } = new();

    [ObservableProperty]
    private VisitingCompanyDto? _selectedVisitingCompany;

    // 子表数据
    public ObservableCollection<VisitingEntourageDto> VisitingEntourages { get; } = new();

    [ObservableProperty]
    private VisitingEntourageDto? _selectedVisitingEntourage;

    [ObservableProperty]
    private VisitingEntourageDto? _editingVisitingEntourage;

    // 主表查询相关
    [ObservableProperty]
    private string _visitorKeyword = string.Empty;

    [ObservableProperty]
    private int _visitorPageIndex = 1;

    [ObservableProperty]
    private int _visitorPageSize = 20;

    [ObservableProperty]
    private int _visitorTotalCount;

    // 子表查询相关（TaktInlineEditDataGrid 不支持分页，改为一次性加载所有数据）
    [ObservableProperty]
    private string _detailKeyword = string.Empty;

    [ObservableProperty]
    private bool _isLoadingVisitingCompanies;

    [ObservableProperty]
    private bool _isLoadingDetails;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public VisitingViewModel(
        IVisitingCompanyService visitingCompanyService,
        IVisitingEntourageService visitingEntourageService,
        IServiceProvider serviceProvider,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _visitingCompanyService = visitingCompanyService ?? throw new ArgumentNullException(nameof(visitingCompanyService));
        _visitingEntourageService = visitingEntourageService ?? throw new ArgumentNullException(nameof(visitingEntourageService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        _ = LoadVisitingCompaniesAsync();
    }

    partial void OnSelectedVisitingCompanyChanged(VisitingCompanyDto? value)
    {
        // 主表选中项改变时，重置子表状态并加载子表数据
        if (value == null)
        {
            VisitingEntourages.Clear();
            SelectedVisitingEntourage = null;
            EditingVisitingEntourage = null;
            DetailKeyword = string.Empty;
        }
        else
        {
            // 重置子表查询条件
            DetailKeyword = string.Empty;
            EditingVisitingEntourage = null;
            _ = LoadDetailsAsync();
        }
    }

    partial void OnEditingVisitingEntourageChanged(VisitingEntourageDto? value)
    {
        // 通知所有相关命令重新评估 CanExecute
        CreateDetailInlineCommand.NotifyCanExecuteChanged();
        UpdateDetailInlineCommand.NotifyCanExecuteChanged();
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();
        DeleteDetailCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private async Task LoadVisitingCompaniesAsync()
    {
        if (IsLoadingVisitingCompanies) return;

        IsLoadingVisitingCompanies = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[VisitingCompaniesView] Load visitors: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                VisitorPageIndex, VisitorPageSize, VisitorKeyword);

            // 构建查询DTO
            var query = new VisitingCompanyQueryDto
            {
                PageIndex = VisitorPageIndex,
                PageSize = VisitorPageSize,
                Keywords = string.IsNullOrWhiteSpace(VisitorKeyword) ? null! : VisitorKeyword.Trim()
            };

            var result = await _visitingCompanyService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                VisitingCompanies.Clear();
                VisitorTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.visitors.loadfailed");
                return;
            }

            VisitingCompanies.Clear();
            foreach (var visitor in result.Data.Items)
            {
                VisitingCompanies.Add(visitor);
            }

            VisitorTotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitingCompaniesView] 加载随行人员列表失败");
        }
        finally
        {
            IsLoadingVisitingCompanies = false;
        }
    }

    private async Task LoadDetailsAsync()
    {
        if (SelectedVisitingCompany == null)
        {
            VisitingEntourages.Clear();
            return;
        }

        if (IsLoadingDetails) return;

        IsLoadingDetails = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[VisitingCompaniesView] Load visitor details: visitorId={VisitingCompanyId}, keyword={Keyword}",
                SelectedVisitingCompany.Id, DetailKeyword);

            // TaktInlineEditDataGrid 不支持分页，一次性加载所有数据
            var query = new VisitingEntourageQueryDto
            {
                VisitingCompanyId = SelectedVisitingCompany.Id,
                VisitingMembers = string.IsNullOrWhiteSpace(DetailKeyword) ? null! : DetailKeyword.Trim(),
                VisitDept = string.IsNullOrWhiteSpace(DetailKeyword) ? null! : DetailKeyword.Trim(),
                PageIndex = 1,
                PageSize = 1000 // 加载足够多的数据
            };

            var result = await _visitingEntourageService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                VisitingEntourages.Clear();
                foreach (var detail in result.Data.Items)
                {
                    VisitingEntourages.Add(detail);
                }
            }
            else
            {
                VisitingEntourages.Clear();
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.visitors.loaddetailsfailed");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitingCompaniesView] 加载随行人员详情失败");
        }
        finally
        {
            IsLoadingDetails = false;
        }
    }

    [RelayCommand]
    private async Task QueryVisitingCompaniesAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitingCompaniesView] 执行查询随行人员操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}",
            operatorName, context.Keyword ?? string.Empty, context.PageIndex, context.PageSize);

        VisitorKeyword = context.Keyword ?? string.Empty;
        if (VisitorPageIndex != context.PageIndex)
        {
            VisitorPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (VisitorPageSize != context.PageSize)
        {
            VisitorPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadVisitingCompaniesAsync();
    }

    [RelayCommand]
    private async Task ResetVisitingCompaniesAsync()
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitingCompaniesView] 执行重置随行人员操作，操作人={Operator}", operatorName);

        VisitorKeyword = string.Empty;
        VisitorPageIndex = 1;
        VisitorPageSize = 20;
        await LoadVisitingCompaniesAsync();
    }

    [RelayCommand]
    private async Task PageChangedVisitingCompaniesAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitingCompaniesView] 随行人员分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}",
            operatorName, request.PageIndex, request.PageSize);

        VisitorPageIndex = request.PageIndex;
        VisitorPageSize = request.PageSize;
        await LoadVisitingCompaniesAsync();
    }

    [RelayCommand]
    private async Task QueryDetailsAsync(Takt.Fluent.Controls.QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitingCompaniesView] 执行查询随行人员详情操作，操作人={Operator}, 关键词={Keyword}",
            operatorName, context.Keyword ?? string.Empty);

        DetailKeyword = context.Keyword ?? string.Empty;
        await LoadDetailsAsync();
    }

    [RelayCommand]
    private async Task ResetDetailsAsync(Takt.Fluent.Controls.QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitingCompaniesView] 执行重置随行人员详情操作，操作人={Operator}", operatorName);

        DetailKeyword = string.Empty;
        await LoadDetailsAsync();
    }

    [RelayCommand]
    private void CreateVisitingCompanies()
    {
        ShowVisitingCompaniesForm(null);
    }

    [RelayCommand]
    private void UpdateVisitingCompanies(VisitingCompanyDto? visitor)
    {
        if (visitor == null)
        {
            visitor = SelectedVisitingCompany;
        }

        if (visitor == null)
        {
            return;
        }

        ShowVisitingCompaniesForm(visitor);
    }

    /// <summary>
    /// 打开随行人员表单窗口
    /// </summary>
    /// <param name="visitor">要编辑的随行人员，null 表示新建</param>
    private void ShowVisitingCompaniesForm(VisitingCompanyDto? visitor)
    {
        try
        {
            var window = _serviceProvider.GetRequiredService<Takt.Fluent.Views.Logistics.Visits.VisitsComponent.VisitingForm>();
            if (window.DataContext is not VisitingFormViewModel formViewModel)
            {
                throw new InvalidOperationException("VisitingForm DataContext 不是 VisitingFormViewModel");
            }

            if (visitor == null)
            {
                formViewModel.ForCreate();
            }
            else
            {
                formViewModel.ForUpdate(visitor);
            }

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadVisitingCompaniesAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitingCompaniesView] 打开随行人员表单窗口失败");
        }
    }

    [RelayCommand]
    private async Task DeleteVisitingCompaniesAsync(VisitingCompanyDto? visitor)
    {
        if (visitor == null)
        {
            visitor = SelectedVisitingCompany;
        }

        if (visitor == null)
        {
            return;
        }

        try
        {
            var result = await _visitingCompanyService.DeleteAsync(visitor.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.visitors.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("logistics.visitors.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[VisitingCompaniesView] 删除随行人员成功，操作人={Operator}, 随行人员Id={Id}", operatorName, visitor.Id);
            await LoadVisitingCompaniesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitingCompaniesView] 删除随行人员失败");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateDetailInline))]
    private void CreateDetailInline()
    {
        if (SelectedVisitingCompany == null)
        {
            var entityName = _localizationManager.GetString("logistics.visitors.visitor.entity");
            ErrorMessage = string.Format(_localizationManager.GetString("common.placeholder.select"), entityName);
            return;
        }

        // 创建新的随行人员详情对象
        var newDetail = new VisitingEntourageDto
        {
            VisitingCompanyId = SelectedVisitingCompany.Id,
            VisitDept = string.Empty,
            VisitingMembers = string.Empty,
            VisitPost = string.Empty
        };

        // 添加到列表
        VisitingEntourages.Add(newDetail);

        // 设置正在编辑的项，让 TaktInlineEditDataGrid 自动进入编辑状态
        EditingVisitingEntourage = newDetail;
        SelectedVisitingEntourage = newDetail;

        // 通知命令重新评估 CanExecute
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();
        UpdateDetailInlineCommand.NotifyCanExecuteChanged();

        // 延迟触发编辑状态，确保 UI 已更新
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (UpdateDetailInlineCommand.CanExecute(newDetail))
            {
                UpdateDetailInlineCommand.Execute(newDetail);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        _operLog?.Information("[VisitingCompaniesView] 新增随行人员详情行，随行人员Id={VisitingCompanyId}", SelectedVisitingCompany.Id);
    }

    private bool CanCreateDetailInline()
    {
        return SelectedVisitingCompany is not null && EditingVisitingEntourage == null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdateDetailInline))]
    private void UpdateDetailInline(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedVisitingEntourage;
        }

        if (detail == null)
        {
            return;
        }

        EditingVisitingEntourage = detail;

        // 通知命令重新评估 CanExecute
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[VisitingCompaniesView] 进入编辑随行人员详情状态，详情Id={Id}", detail.Id);
    }

    private bool CanUpdateDetailInline(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            return SelectedVisitingEntourage is not null && EditingVisitingEntourage == null;
        }
        return detail is not null && EditingVisitingEntourage == null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveDetailInline))]
    private async Task SaveDetailInlineAsync(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            detail = EditingVisitingEntourage;
        }

        if (detail == null || EditingVisitingEntourage != detail)
        {
            return;
        }

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(detail.VisitDept))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.visitors.visitordetail.visitdept"));
            return;
        }

        if (string.IsNullOrWhiteSpace(detail.VisitingMembers))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.visitors.visitordetail.visitors"));
            return;
        }

        if (string.IsNullOrWhiteSpace(detail.VisitPost))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.visitors.visitordetail.visitpost"));
            return;
        }

        try
        {
            if (detail.Id == 0)
            {
                // 新增
                var createDto = new VisitingEntourageCreateDto
                {
                    VisitingCompanyId = detail.VisitingCompanyId,
                    VisitDept = detail.VisitDept,
                    VisitingMembers = detail.VisitingMembers,
                    VisitPost = detail.VisitPost,
                    Remarks = detail.Remarks
                };

                var result = await _visitingEntourageService.CreateAsync(createDto);
                if (!result.Success || result.Data <= 0)
                {
                    ErrorMessage = result.Message ?? "创建失败";
                    return;
                }

                detail.Id = result.Data;
                EditingVisitingEntourage = null;

                // 通知命令重新评估 CanExecute
                SaveDetailInlineCommand.NotifyCanExecuteChanged();
                CancelDetailInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "创建成功";
                _operLog?.Information("[VisitingCompaniesView] 随行人员详情创建成功，详情Id={Id}", detail.Id);

                await LoadDetailsAsync();
            }
            else
            {
                // 更新
                var updateDto = new VisitingEntourageUpdateDto
                {
                    Id = detail.Id,
                    VisitingCompanyId = detail.VisitingCompanyId,
                    VisitDept = detail.VisitDept,
                    VisitingMembers = detail.VisitingMembers,
                    VisitPost = detail.VisitPost,
                    Remarks = detail.Remarks
                };

                var result = await _visitingEntourageService.UpdateAsync(updateDto);
                if (!result.Success)
                {
                    ErrorMessage = result.Message ?? "更新失败";
                    return;
                }

                EditingVisitingEntourage = null;

                // 通知命令重新评估 CanExecute
                SaveDetailInlineCommand.NotifyCanExecuteChanged();
                CancelDetailInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "更新成功";
                _operLog?.Information("[VisitingCompaniesView] 随行人员详情更新成功，详情Id={Id}", detail.Id);

                await LoadDetailsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitingCompaniesView] 随行人员详情保存失败");
        }
    }

    private bool CanSaveDetailInline(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            return EditingVisitingEntourage is not null;
        }
        return EditingVisitingEntourage != null && EditingVisitingEntourage == detail;
    }

    [RelayCommand(CanExecute = nameof(CanCancelDetailInline))]
    private async Task CancelDetailInlineAsync()
    {
        if (EditingVisitingEntourage != null)
        {
            EditingVisitingEntourage = null;

            // 通知命令重新评估 CanExecute
            SaveDetailInlineCommand.NotifyCanExecuteChanged();
            CancelDetailInlineCommand.NotifyCanExecuteChanged();

            await LoadDetailsAsync(); // 重新加载以恢复原始数据
        }
    }

    private bool CanCancelDetailInline()
    {
        return EditingVisitingEntourage != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteDetail))]
    private async Task DeleteDetailAsync(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedVisitingEntourage;
        }

        if (detail == null)
        {
            return;
        }

        try
        {
            var result = await _visitingEntourageService.DeleteAsync(detail.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.visitors.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("logistics.visitors.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[VisitingCompaniesView] 删除随行人员详情成功，操作人={Operator}, 详情Id={Id}", operatorName, detail.Id);
            await LoadDetailsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitingCompaniesView] 删除随行人员详情失败");
        }
    }

    private bool CanDeleteDetail(VisitingEntourageDto? detail)
    {
        // 编辑状态下不能删除
        if (EditingVisitingEntourage != null)
        {
            return false;
        }

        if (detail == null)
        {
            return SelectedVisitingEntourage is not null;
        }
        return detail is not null;
    }
}

