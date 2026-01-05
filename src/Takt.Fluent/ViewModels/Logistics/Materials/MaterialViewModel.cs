// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Materials
// 文件名称：MaterialViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：生产物料视图模型
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logistics.Materials;
using Takt.Application.Services.Logistics.Materials;
using Takt.Common.Context;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Logistics.Materials;

/// <summary>
/// 生产物料视图模型
/// </summary>
public partial class MaterialViewModel : ObservableObject
{
    private readonly IProdMaterialService _prodMaterialService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<ProdMaterialDto> ProdMaterials { get; } = new();

    [ObservableProperty]
    private ProdMaterialDto? _selectedProdMaterial;

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

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public MaterialViewModel(
        IProdMaterialService prodMaterialService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _prodMaterialService = prodMaterialService ?? throw new ArgumentNullException(nameof(prodMaterialService));
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
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[MaterialView] Load materials: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}", PageIndex, PageSize, Keyword);

            // 构建查询DTO
            var query = new ProdMaterialQueryDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                Keywords = string.IsNullOrWhiteSpace(Keyword) ? null! : Keyword.Trim()
            };

            var result = await _prodMaterialService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                ProdMaterials.Clear();
                TotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.material.loadfailed");
                return;
            }

            ProdMaterials.Clear();
            foreach (var material in result.Data.Items)
            {
                ProdMaterials.Add(material);
            }

            TotalCount = result.Data.TotalNum;
            OnPropertyChanged(nameof(TotalPages));
            UpdateEmptyMessage();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[MaterialView] 加载生产物料列表失败");
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

    [RelayCommand]
    private async Task QueryAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[MaterialView] 执行查询操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}", 
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
    private async Task ResetAsync()
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[MaterialView] 执行重置操作，操作人={Operator}", operatorName);
        
        Keyword = string.Empty;
        PageIndex = 1;
        PageSize = 20;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task PageChangedAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[MaterialView] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}", 
            operatorName, request.PageIndex, request.PageSize);
        
        PageIndex = request.PageIndex;
        PageSize = request.PageSize;
        await LoadAsync();
    }

    [RelayCommand]
    private void Create()
    {
        // TODO: 实现创建功能
        ErrorMessage = "创建功能待实现";
    }

    [RelayCommand]
    private void Update(ProdMaterialDto? material)
    {
        if (material == null)
        {
            material = SelectedProdMaterial;
        }

        if (material == null)
        {
            return;
        }

        // TODO: 实现更新功能
        ErrorMessage = "更新功能待实现";
    }

    [RelayCommand]
    private async Task DeleteAsync(ProdMaterialDto? material)
    {
        if (material == null)
        {
            material = SelectedProdMaterial;
        }

        if (material == null)
        {
            return;
        }

        try
        {
            var result = await _prodMaterialService.DeleteAsync(material.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.material.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("logistics.material.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[MaterialView] 删除生产物料成功，操作人={Operator}, 物料Id={Id}, 物料代码={MaterialCode}", 
                operatorName, material.Id, material.MaterialCode ?? string.Empty);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[MaterialView] 删除生产物料失败");
        }
    }
}

