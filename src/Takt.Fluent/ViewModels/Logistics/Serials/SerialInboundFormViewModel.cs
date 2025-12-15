// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Serials
// 文件名称：SerialInboundFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号入库表单视图模型（扫描入库）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Application.Services.Logistics.Serials;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Logistics.Serials;

/// <summary>
/// 序列号入库表单视图模型（扫描入库）
/// </summary>
public partial class SerialInboundFormViewModel : ObservableObject
{
    private readonly IProdSerialInboundService _prodSerialInboundService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _fullSerialNumber = string.Empty;

    [ObservableProperty]
    private string _error = string.Empty;

    [ObservableProperty]
    private string _fullSerialNumberError = string.Empty;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public SerialInboundFormViewModel(
        IProdSerialInboundService prodSerialInboundService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _prodSerialInboundService = prodSerialInboundService ?? throw new ArgumentNullException(nameof(prodSerialInboundService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;
        
        Title = _localizationManager.GetString("logistics.serials.prodserialinbound.scaninbound");
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        FullSerialNumberError = string.Empty;
        Error = string.Empty;
    }

    /// <summary>
    /// FullSerialNumber 属性变化时清除错误提示
    /// </summary>
    partial void OnFullSerialNumberChanged(string value)
    {
        // 当用户重新输入时，清除之前的错误提示
        if (!string.IsNullOrWhiteSpace(value))
        {
            FullSerialNumberError = string.Empty;
        }
    }

    /// <summary>
    /// 检查序列号是否重复（异步）
    /// </summary>
    public async Task<bool> CheckDuplicateAsync(string fullSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(fullSerialNumber))
        {
            return false;
        }

        try
        {
            // 通过查询列表检查是否存在相同的序列号
            var query = new ProdSerialInboundQueryDto
            {
                Keywords = fullSerialNumber.Trim(),
                PageIndex = 1,
                PageSize = 1
            };
            
            var result = await _prodSerialInboundService.GetListAsync(query);
            if (result.Success && result.Data != null && result.Data.Items != null)
            {
                // 检查是否有完全匹配的序列号
                return result.Data.Items.Any(item => 
                    item.FullSerialNumber?.Trim().Equals(fullSerialNumber.Trim(), StringComparison.OrdinalIgnoreCase) == true);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[SerialInboundForm] 检查序列号重复失败，完整序列号={FullSerialNumber}", fullSerialNumber);
            return false;
        }
    }

    /// <summary>
    /// 验证字段
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(FullSerialNumber))
        {
            FullSerialNumberError = _localizationManager.GetString("logistics.serials.prodserialinbound.validation.fullserialnumberrequired");
            isValid = false;
        }
        else if (FullSerialNumber.Length == 0 || FullSerialNumber.Length > 19)
        {
            FullSerialNumberError = "序列号长度必须大于0且小于等于19位";
            isValid = false;
        }
        else if (FullSerialNumber.Length > 200)
        {
            FullSerialNumberError = _localizationManager.GetString("logistics.serials.prodserialinbound.validation.fullserialnumbermaxlength");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 保存入库记录
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // 验证字段
            if (!ValidateFields())
            {
                return;
            }

            // 创建入库记录
            // 注意：MaterialCode、SerialNumber、Quantity 需要从 FullSerialNumber 中解析
            // 服务层或数据库层会处理解析
            var dto = new ProdSerialInboundCreateDto
            {
                FullSerialNumber = FullSerialNumber.Trim()
            };

            var result = await _prodSerialInboundService.CreateAsync(dto);
            if (!result.Success)
            {
                var errorMessage = result.Message ?? _localizationManager.GetString("logistics.serials.prodserialinbound.createfailed");
                FullSerialNumberError = string.Empty;
                Error = string.Empty;
                
                // 判断错误类型并显示相应的消息
                if (errorMessage.Contains("已存在") || errorMessage.Contains("重复"))
                {
                    TaktMessageManager.ShowWarning(errorMessage);
                    _operLog?.Warning("[SerialInboundForm] 扫描入库失败：序列号重复，完整序列号={FullSerialNumber}", FullSerialNumber);
                }
                else
                {
                    TaktMessageManager.ShowError(errorMessage);
                    _operLog?.Error("[SerialInboundForm] 扫描入库失败：{ErrorMessage}, 完整序列号={FullSerialNumber}", errorMessage, FullSerialNumber);
                }
                return;
            }

            var savedSerialNumber = FullSerialNumber;
            _operLog?.Information("[SerialInboundForm] 扫描入库成功，完整序列号={FullSerialNumber}", savedSerialNumber);
            
            // 使用统一消息管理器显示成功提示（Toast，自动消失）
            TaktMessageManager.ShowSuccess($"扫描入库成功：{savedSerialNumber}");
            
            // 清空输入框以便继续扫描
            FullSerialNumber = string.Empty;
            ClearAllErrors();
            
            // 通知回调刷新列表，但不关闭窗口（以便继续扫描）
            // SaveSuccessCallback 通常用于关闭窗口，但扫描场景需要保持窗口打开
            // 如果需要刷新列表，可以通过其他方式实现
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
            _operLog?.Error(ex, "[SerialInboundForm] 保存入库记录失败");
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // 关闭窗口
        var window = System.Windows.Application.Current?.Windows.OfType<Views.Logistics.Serials.SerialComponent.SerialInboundForm>()
            .FirstOrDefault(w => w.DataContext == this);
        window?.Close();
    }
}

