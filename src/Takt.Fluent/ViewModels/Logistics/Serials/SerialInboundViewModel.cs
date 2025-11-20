// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Serials
// 文件名称：SerialInboundViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号入库视图模型（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Application.Services.Logistics.Serials;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Logistics.Serials;

/// <summary>
/// 序列号入库视图模型（主子表视图）
/// 主表：ProdSerial（产品序列号）
/// 子表：ProdSerialInbound（序列号入库记录）
/// </summary>
public partial class SerialInboundViewModel : ObservableObject
{
    private readonly IProdSerialService _prodSerialService;
    private readonly IProdSerialInboundService _prodSerialInboundService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<ProdSerialDto> ProdSerials { get; } = new();

    [ObservableProperty]
    private ProdSerialDto? _selectedProdSerial;

    // 子表数据
    public ObservableCollection<ProdSerialInboundDto> ProdSerialInbounds { get; } = new();

    [ObservableProperty]
    private ProdSerialInboundDto? _selectedProdSerialInbound;

    // 主表查询相关
    [ObservableProperty]
    private string _serialKeyword = string.Empty;

    [ObservableProperty]
    private int _serialPageIndex = 1;

    [ObservableProperty]
    private int _serialPageSize = 20;

    [ObservableProperty]
    private int _serialTotalCount;

    // 子表查询相关
    [ObservableProperty]
    private string _inboundKeyword = string.Empty;

    [ObservableProperty]
    private int _inboundPageIndex = 1;

    [ObservableProperty]
    private int _inboundPageSize = 20;

    [ObservableProperty]
    private int _inboundTotalCount;

    [ObservableProperty]
    private bool _isLoadingSerials;

    [ObservableProperty]
    private bool _isLoadingInbounds;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public SerialInboundViewModel(
        IProdSerialService prodSerialService,
        IProdSerialInboundService prodSerialInboundService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _prodSerialService = prodSerialService ?? throw new ArgumentNullException(nameof(prodSerialService));
        _prodSerialInboundService = prodSerialInboundService ?? throw new ArgumentNullException(nameof(prodSerialInboundService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        _ = LoadSerialsAsync();
    }

    partial void OnSelectedProdSerialChanged(ProdSerialDto? value)
    {
        _ = LoadInboundsAsync();
    }

    private async Task LoadSerialsAsync()
    {
        if (IsLoadingSerials) return;

        IsLoadingSerials = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[SerialInboundView] Load serials: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SerialPageIndex, SerialPageSize, SerialKeyword);

            // 构建查询DTO
            var query = new ProdSerialQueryDto
            {
                PageIndex = SerialPageIndex,
                PageSize = SerialPageSize,
                Keywords = string.IsNullOrWhiteSpace(SerialKeyword) ? null : SerialKeyword.Trim()
            };

            var result = await _prodSerialService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                ProdSerials.Clear();
                SerialTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Logistics.Serial.LoadFailed") ?? "加载产品序列号数据失败";
                return;
            }

            ProdSerials.Clear();
            foreach (var serial in result.Data.Items)
            {
                ProdSerials.Add(serial);
            }

            SerialTotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 加载产品序列号列表失败");
        }
        finally
        {
            IsLoadingSerials = false;
        }
    }

    private async Task LoadInboundsAsync()
    {
        if (SelectedProdSerial == null)
        {
            ProdSerialInbounds.Clear();
            InboundTotalCount = 0;
            return;
        }

        if (IsLoadingInbounds) return;

        IsLoadingInbounds = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[SerialInboundView] Load inbounds: materialCode={MaterialCode}, pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SelectedProdSerial.MaterialCode, InboundPageIndex, InboundPageSize, InboundKeyword);

            var query = new ProdSerialInboundQueryDto
            {
                MaterialCode = SelectedProdSerial.MaterialCode,
                InboundNo = string.IsNullOrWhiteSpace(InboundKeyword) ? null : InboundKeyword.Trim(),
                SerialNumber = string.IsNullOrWhiteSpace(InboundKeyword) ? null : InboundKeyword.Trim(),
                PageIndex = InboundPageIndex,
                PageSize = InboundPageSize
            };

            var result = await _prodSerialInboundService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                ProdSerialInbounds.Clear();
                foreach (var inbound in result.Data.Items)
                {
                    ProdSerialInbounds.Add(inbound);
                }

                InboundTotalCount = result.Data.TotalNum;
            }
            else
            {
                ProdSerialInbounds.Clear();
                InboundTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Logistics.SerialInbound.LoadFailed") ?? "加载序列号入库记录失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 加载序列号入库记录失败");
        }
        finally
        {
            IsLoadingInbounds = false;
        }
    }

    [RelayCommand]
    private async Task QuerySerialsAsync(QueryContext context)
    {
        SerialKeyword = context.Keyword;
        if (SerialPageIndex != context.PageIndex)
        {
            SerialPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (SerialPageSize != context.PageSize)
        {
            SerialPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadSerialsAsync();
    }

    [RelayCommand]
    private async Task ResetSerialsAsync()
    {
        SerialKeyword = string.Empty;
        SerialPageIndex = 1;
        SerialPageSize = 20;
        await LoadSerialsAsync();
    }

    [RelayCommand]
    private async Task PageChangedSerialsAsync(PageRequest request)
    {
        SerialPageIndex = request.PageIndex;
        SerialPageSize = request.PageSize;
        await LoadSerialsAsync();
    }

    [RelayCommand]
    private async Task QueryInboundsAsync(QueryContext context)
    {
        InboundKeyword = context.Keyword;
        if (InboundPageIndex != context.PageIndex)
        {
            InboundPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (InboundPageSize != context.PageSize)
        {
            InboundPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadInboundsAsync();
    }

    [RelayCommand]
    private async Task ResetInboundsAsync()
    {
        InboundKeyword = string.Empty;
        InboundPageIndex = 1;
        InboundPageSize = 20;
        await LoadInboundsAsync();
    }

    [RelayCommand]
    private async Task PageChangedInboundsAsync(PageRequest request)
    {
        InboundPageIndex = request.PageIndex;
        InboundPageSize = request.PageSize;
        await LoadInboundsAsync();
    }

    [RelayCommand]
    private void CreateSerial()
    {
        // TODO: 实现创建功能
        ErrorMessage = "创建功能待实现";
    }

    [RelayCommand]
    private void UpdateSerial(ProdSerialDto? serial)
    {
        if (serial == null)
        {
            serial = SelectedProdSerial;
        }

        if (serial == null)
        {
            return;
        }

        // TODO: 实现更新功能
        ErrorMessage = "更新功能待实现";
    }

    [RelayCommand]
    private async Task DeleteSerialAsync(ProdSerialDto? serial)
    {
        if (serial == null)
        {
            serial = SelectedProdSerial;
        }

        if (serial == null)
        {
            return;
        }

        try
        {
            var result = await _prodSerialService.DeleteAsync(serial.Id);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            await LoadSerialsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 删除产品序列号失败");
        }
    }

    [RelayCommand]
    private void CreateInbound()
    {
        if (SelectedProdSerial == null)
        {
            ErrorMessage = "请先选择产品序列号";
            return;
        }

        // TODO: 实现创建功能
        ErrorMessage = "创建功能待实现";
    }

    [RelayCommand]
    private void UpdateInbound(ProdSerialInboundDto? inbound)
    {
        if (inbound == null)
        {
            inbound = SelectedProdSerialInbound;
        }

        if (inbound == null)
        {
            return;
        }

        // TODO: 实现更新功能
        ErrorMessage = "更新功能待实现";
    }

    [RelayCommand]
    private async Task DeleteInboundAsync(ProdSerialInboundDto? inbound)
    {
        if (inbound == null)
        {
            inbound = SelectedProdSerialInbound;
        }

        if (inbound == null)
        {
            return;
        }

        try
        {
            var result = await _prodSerialInboundService.DeleteAsync(inbound.Id);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            await LoadInboundsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 删除序列号入库记录失败");
        }
    }
}

