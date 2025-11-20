// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Serials
// 文件名称：SerialOutboundViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号出库视图模型（主子表视图）
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
/// 序列号出库视图模型（主子表视图）
/// 主表：ProdSerial（产品序列号）
/// 子表：ProdSerialOutbound（序列号出库记录）
/// </summary>
public partial class SerialOutboundViewModel : ObservableObject
{
    private readonly IProdSerialService _prodSerialService;
    private readonly IProdSerialOutboundService _prodSerialOutboundService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<ProdSerialDto> ProdSerials { get; } = new();

    [ObservableProperty]
    private ProdSerialDto? _selectedProdSerial;

    // 子表数据
    public ObservableCollection<ProdSerialOutboundDto> ProdSerialOutbounds { get; } = new();

    [ObservableProperty]
    private ProdSerialOutboundDto? _selectedProdSerialOutbound;

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
    private string _outboundKeyword = string.Empty;

    [ObservableProperty]
    private int _outboundPageIndex = 1;

    [ObservableProperty]
    private int _outboundPageSize = 20;

    [ObservableProperty]
    private int _outboundTotalCount;

    [ObservableProperty]
    private bool _isLoadingSerials;

    [ObservableProperty]
    private bool _isLoadingOutbounds;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public SerialOutboundViewModel(
        IProdSerialService prodSerialService,
        IProdSerialOutboundService prodSerialOutboundService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _prodSerialService = prodSerialService ?? throw new ArgumentNullException(nameof(prodSerialService));
        _prodSerialOutboundService = prodSerialOutboundService ?? throw new ArgumentNullException(nameof(prodSerialOutboundService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        _ = LoadSerialsAsync();
    }

    partial void OnSelectedProdSerialChanged(ProdSerialDto? value)
    {
        _ = LoadOutboundsAsync();
    }

    private async Task LoadSerialsAsync()
    {
        if (IsLoadingSerials) return;

        IsLoadingSerials = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[SerialOutboundView] Load serials: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
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
            _operLog?.Error(ex, "[SerialOutboundView] 加载产品序列号列表失败");
        }
        finally
        {
            IsLoadingSerials = false;
        }
    }

    private async Task LoadOutboundsAsync()
    {
        if (SelectedProdSerial == null)
        {
            ProdSerialOutbounds.Clear();
            OutboundTotalCount = 0;
            return;
        }

        if (IsLoadingOutbounds) return;

        IsLoadingOutbounds = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[SerialOutboundView] Load outbounds: materialCode={MaterialCode}, pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SelectedProdSerial.MaterialCode, OutboundPageIndex, OutboundPageSize, OutboundKeyword);

            var query = new ProdSerialOutboundQueryDto
            {
                MaterialCode = SelectedProdSerial.MaterialCode,
                OutboundNo = string.IsNullOrWhiteSpace(OutboundKeyword) ? null : OutboundKeyword.Trim(),
                SerialNumber = string.IsNullOrWhiteSpace(OutboundKeyword) ? null : OutboundKeyword.Trim(),
                Destination = string.IsNullOrWhiteSpace(OutboundKeyword) ? null : OutboundKeyword.Trim(),
                PageIndex = OutboundPageIndex,
                PageSize = OutboundPageSize
            };

            var result = await _prodSerialOutboundService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                ProdSerialOutbounds.Clear();
                foreach (var outbound in result.Data.Items)
                {
                    ProdSerialOutbounds.Add(outbound);
                }

                OutboundTotalCount = result.Data.TotalNum;
            }
            else
            {
                ProdSerialOutbounds.Clear();
                OutboundTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Logistics.SerialOutbound.LoadFailed") ?? "加载序列号出库记录失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 加载序列号出库记录失败");
        }
        finally
        {
            IsLoadingOutbounds = false;
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
    private async Task QueryOutboundsAsync(QueryContext context)
    {
        OutboundKeyword = context.Keyword;
        if (OutboundPageIndex != context.PageIndex)
        {
            OutboundPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (OutboundPageSize != context.PageSize)
        {
            OutboundPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadOutboundsAsync();
    }

    [RelayCommand]
    private async Task ResetOutboundsAsync()
    {
        OutboundKeyword = string.Empty;
        OutboundPageIndex = 1;
        OutboundPageSize = 20;
        await LoadOutboundsAsync();
    }

    [RelayCommand]
    private async Task PageChangedOutboundsAsync(PageRequest request)
    {
        OutboundPageIndex = request.PageIndex;
        OutboundPageSize = request.PageSize;
        await LoadOutboundsAsync();
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
            _operLog?.Error(ex, "[SerialOutboundView] 删除产品序列号失败");
        }
    }

    [RelayCommand]
    private void CreateOutbound()
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
    private void UpdateOutbound(ProdSerialOutboundDto? outbound)
    {
        if (outbound == null)
        {
            outbound = SelectedProdSerialOutbound;
        }

        if (outbound == null)
        {
            return;
        }

        // TODO: 实现更新功能
        ErrorMessage = "更新功能待实现";
    }

    [RelayCommand]
    private async Task DeleteOutboundAsync(ProdSerialOutboundDto? outbound)
    {
        if (outbound == null)
        {
            outbound = SelectedProdSerialOutbound;
        }

        if (outbound == null)
        {
            return;
        }

        try
        {
            var result = await _prodSerialOutboundService.DeleteAsync(outbound.Id);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            await LoadOutboundsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 删除序列号出库记录失败");
        }
    }
}

