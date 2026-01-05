// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Serials
// 文件名称：SerialOutboundFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号出库表单视图模型（扫描出库）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Application.Services.Logistics.Materials;
using Takt.Application.Services.Logistics.Serials;
using Takt.Application.Services.Routine;
using Takt.Common.Logging;
using Takt.Common.Models;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Logistics.Serials;

/// <summary>
/// 序列号出库表单视图模型（扫描出库）
/// </summary>
public partial class SerialOutboundFormViewModel : ObservableObject
{
    private readonly IProdSerialOutboundService _prodSerialOutboundService;
    private readonly IProdModelService _prodModelService;
    private readonly IDictionaryTypeService _dictionaryTypeService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _destCode;

    [ObservableProperty]
    private string _outboundNo = string.Empty;

    [ObservableProperty]
    private DateTime _outboundDate = DateTime.Now;

    [ObservableProperty]
    private string? _destPort;

    /// <summary>
    /// 仕向地选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> DestCodeOptions { get; set; } = new();

    /// <summary>
    /// 目的地港口选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> DestPortOptions { get; set; } = new();

    /// <summary>
    /// 选中的仕向地选项
    /// </summary>
    [ObservableProperty]
    private SelectOptionModel? _selectedDestCodeOption;

    partial void OnSelectedDestCodeOptionChanged(SelectOptionModel? value)
    {
        DestCode = value?.DataValue;
    }

    /// <summary>
    /// 选中的目的地港口选项
    /// </summary>
    [ObservableProperty]
    private SelectOptionModel? _selectedDestPortOption;

    partial void OnSelectedDestPortOptionChanged(SelectOptionModel? value)
    {
        DestPort = value?.DataValue;
    }

    [ObservableProperty]
    private string _fullSerialNumber = string.Empty;

    [ObservableProperty]
    private string _error = string.Empty;

    // 错误消息属性
    [ObservableProperty]
    private string _outboundNoError = string.Empty;

    [ObservableProperty]
    private string _outboundDateError = string.Empty;

    [ObservableProperty]
    private string _destCodeError = string.Empty;

    [ObservableProperty]
    private string _destPortError = string.Empty;

    [ObservableProperty]
    private string _fullSerialNumberError = string.Empty;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public SerialOutboundFormViewModel(
        IProdSerialOutboundService prodSerialOutboundService,
        IProdModelService prodModelService,
        IDictionaryTypeService dictionaryTypeService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _prodSerialOutboundService = prodSerialOutboundService ?? throw new ArgumentNullException(nameof(prodSerialOutboundService));
        _prodModelService = prodModelService ?? throw new ArgumentNullException(nameof(prodModelService));
        _dictionaryTypeService = dictionaryTypeService ?? throw new ArgumentNullException(nameof(dictionaryTypeService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;
        
        Title = _localizationManager.GetString("logistics.serials.prodserialoutbound.scanoutbound");
        
        // 初始化选项列表
        // 仕向地从产品机种服务获取，目的地港口从字典服务获取
        _ = InitializeDestCodeOptionsAsync();
        _ = InitializeDestPortOptionsAsync();
    }

    /// <summary>
    /// 初始化仕向地选项列表（从产品机种服务获取）
    /// </summary>
    public async Task InitializeDestCodeOptionsAsync()
    {
        try
        {
            _operLog?.Information("[SerialOutboundForm] 开始加载仕向地选项，从产品机种服务获取");
            
            if (_prodModelService == null)
            {
                _operLog?.Error("[SerialOutboundForm] 产品机种服务未注入，无法加载仕向地选项");
                return;
            }

            var result = await _prodModelService.GetDestOptionAsync();
            
            if (result.Success && result.Data != null && result.Data.Count > 0)
            {
                // 使用 UI 线程同步更新集合
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    DestCodeOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        DestCodeOptions.Add(option);
                    }
                    // 显式触发属性通知，确保 UI 更新
                    OnPropertyChanged(nameof(DestCodeOptions));
                    _operLog?.Information("[SerialOutboundForm] UI线程更新：仕向地选项已添加到集合，共 {Count} 条，第一个选项：{FirstOption}", 
                        DestCodeOptions.Count, 
                        DestCodeOptions.FirstOrDefault()?.DataLabel ?? "无");
                }, System.Windows.Threading.DispatcherPriority.Normal);
                _operLog?.Information("[SerialOutboundForm] 成功加载仕向地选项，共 {Count} 条", result.Data.Count);
            }
            else
            {
                var errorMsg = result.Message ?? "未知错误";
                _operLog?.Warning("[SerialOutboundForm] 加载仕向地选项失败或数据为空：{Message}，数据数量：{Count}", 
                    errorMsg, result.Data?.Count ?? 0);
                
                // 如果加载失败，使用空列表（ComboBox 是可编辑的，用户仍可手动输入）
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    DestCodeOptions.Clear();
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[SerialOutboundForm] 加载仕向地选项异常：{Message}", ex.Message);
            // 如果加载异常，使用空列表（ComboBox 是可编辑的，用户仍可手动输入）
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                DestCodeOptions.Clear();
            });
        }
    }

    /// <summary>
    /// 初始化目的地港口选项列表（从字典服务获取）
    /// </summary>
    public async Task InitializeDestPortOptionsAsync()
    {
        try
        {
            _operLog?.Information("[SerialOutboundForm] 开始加载目的地港口选项，字典类型代码：sys_dest_port");
            
            if (_dictionaryTypeService == null)
            {
                _operLog?.Error("[SerialOutboundForm] 字典服务未注入，无法加载目的地港口选项");
                return;
            }

            var result = await _dictionaryTypeService.GetOptionsAsync("sys_dest_port");
            
            if (result.Success && result.Data != null)
            {
                // 使用 UI 线程同步更新集合
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    DestPortOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        DestPortOptions.Add(option);
                    }
                    // 显式触发属性通知，确保 UI 更新
                    OnPropertyChanged(nameof(DestPortOptions));
                    _operLog?.Information("[SerialOutboundForm] UI线程更新：目的地港口选项已添加到集合，共 {Count} 条", DestPortOptions.Count);
                }, System.Windows.Threading.DispatcherPriority.Normal);
                _operLog?.Information("[SerialOutboundForm] 成功加载目的地港口选项，共 {Count} 条", result.Data.Count);
            }
            else
            {
                var errorMsg = result.Message ?? "未知错误";
                _operLog?.Warning("[SerialOutboundForm] 加载目的地港口选项失败：{Message}，数据数量：{Count}", 
                    errorMsg, result.Data?.Count ?? 0);
                
                // 如果加载失败，使用空列表（ComboBox 是可编辑的，用户仍可手动输入）
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    DestPortOptions.Clear();
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[SerialOutboundForm] 加载目的地港口选项异常：{Message}", ex.Message);
            // 如果加载异常，使用空列表（ComboBox 是可编辑的，用户仍可手动输入）
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                DestPortOptions.Clear();
            });
        }
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        DestCodeError = string.Empty;
        OutboundNoError = string.Empty;
        OutboundDateError = string.Empty;
        DestPortError = string.Empty;
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
            var query = new ProdSerialOutboundQueryDto
            {
                Keywords = fullSerialNumber.Trim(),
                PageIndex = 1,
                PageSize = 1
            };
            
            var result = await _prodSerialOutboundService.GetListAsync(query);
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
            _operLog?.Error(ex, "[SerialOutboundForm] 检查序列号重复失败，完整序列号={FullSerialNumber}", fullSerialNumber);
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

        // 验证仕向地（必填）
        if (string.IsNullOrWhiteSpace(DestCode))
        {
            DestCodeError = _localizationManager.GetString("logistics.serials.prodserial.validation.destcoderequired");
            isValid = false;
        }
        else if (DestCode.Length > 20)
        {
            DestCodeError = _localizationManager.GetString("logistics.serials.prodserial.validation.destcodemaxlength");
            isValid = false;
        }

        // 验证出库单号（必填）
        if (string.IsNullOrWhiteSpace(OutboundNo))
        {
            OutboundNoError = _localizationManager.GetString("logistics.serials.prodserialoutbound.validation.outboundnorequired");
            isValid = false;
        }
        else if (OutboundNo.Length > 50)
        {
            OutboundNoError = _localizationManager.GetString("logistics.serials.prodserialoutbound.validation.outboundnomaxlength");
            isValid = false;
        }

        // 验证出库日期（必填）
        if (OutboundDate == default(DateTime))
        {
            OutboundDateError = _localizationManager.GetString("logistics.serials.prodserialoutbound.validation.outbounddaterequired");
            isValid = false;
        }

        // 验证目的地港口（必填）
        if (string.IsNullOrWhiteSpace(DestPort))
        {
            DestPortError = _localizationManager.GetString("logistics.serials.prodserialoutbound.validation.destportrequired");
            isValid = false;
        }
        else if (DestPort.Length > 100)
        {
            DestPortError = _localizationManager.GetString("logistics.serials.prodserialoutbound.validation.destportmaxlength");
            isValid = false;
        }

        // 验证完整序列号（必填）
        if (string.IsNullOrWhiteSpace(FullSerialNumber))
        {
            FullSerialNumberError = _localizationManager.GetString("logistics.serials.prodserialoutbound.validation.fullserialnumberrequired");
            isValid = false;
        }
        else if (FullSerialNumber.Length == 0 || FullSerialNumber.Length > 19)
        {
            FullSerialNumberError = "序列号长度必须大于0且小于等于19位";
            isValid = false;
        }
        else if (FullSerialNumber.Length > 200)
        {
            FullSerialNumberError = _localizationManager.GetString("logistics.serials.prodserialoutbound.validation.fullserialnumbermaxlength");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 保存出库记录
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

            // 创建出库记录
            // 注意：MaterialCode、SerialNumber、Quantity 需要从 FullSerialNumber 中解析
            // 服务层或数据库层会处理解析
            var dto = new ProdSerialOutboundCreateDto
            {
                FullSerialNumber = FullSerialNumber.Trim(),
                DestCode = string.IsNullOrWhiteSpace(DestCode) ? null! : DestCode.Trim(),
                OutboundNo = OutboundNo.Trim(),
                OutboundDate = OutboundDate,
                DestPort = string.IsNullOrWhiteSpace(DestPort) ? null! : DestPort.Trim()
            };

            var result = await _prodSerialOutboundService.CreateAsync(dto);
            if (!result.Success)
            {
                var errorMessage = result.Message ?? _localizationManager.GetString("logistics.serials.prodserialoutbound.createfailed");
                FullSerialNumberError = string.Empty;
                Error = string.Empty;
                
                // 判断错误类型并显示相应的消息
                if (errorMessage.Contains("尚未入库"))
                {
                    TaktMessageManager.ShowWarning(errorMessage);
                    _operLog?.Warning("[SerialOutboundForm] 扫描出库失败：序列号尚未入库，出库单号={OutboundNo}, 完整序列号={FullSerialNumber}", OutboundNo, FullSerialNumber);
                }
                else if (errorMessage.Contains("已存在") || errorMessage.Contains("重复"))
                {
                    TaktMessageManager.ShowWarning(errorMessage);
                    _operLog?.Warning("[SerialOutboundForm] 扫描出库失败：序列号重复，出库单号={OutboundNo}, 完整序列号={FullSerialNumber}", OutboundNo, FullSerialNumber);
                }
                else
                {
                    TaktMessageManager.ShowError(errorMessage);
                    _operLog?.Error("[SerialOutboundForm] 扫描出库失败：{ErrorMessage}, 出库单号={OutboundNo}, 完整序列号={FullSerialNumber}", errorMessage, OutboundNo, FullSerialNumber);
                }
                return;
            }

            var savedSerialNumber = FullSerialNumber;
            _operLog?.Information("[SerialOutboundForm] 扫描出库成功，出库单号={OutboundNo}, 完整序列号={FullSerialNumber}", OutboundNo, savedSerialNumber);
            
            // 使用统一消息管理器显示成功提示（Toast，自动消失）
            TaktMessageManager.ShowSuccess($"扫描出库成功：{savedSerialNumber}");
            
            // 清空输入框以便继续扫描
            FullSerialNumber = string.Empty;
            FullSerialNumberError = string.Empty;
            
            // 通知回调刷新列表，但不关闭窗口（以便继续扫描）
            // SaveSuccessCallback 通常用于关闭窗口，但扫描场景需要保持窗口打开
            // 如果需要刷新列表，可以通过其他方式实现
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
            _operLog?.Error(ex, "[SerialOutboundForm] 保存出库记录失败");
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // 关闭窗口
        var window = System.Windows.Application.Current?.Windows.OfType<Views.Logistics.Serials.SerialComponent.SerialOutboundForm>()
            .FirstOrDefault(w => w.DataContext == this);
        window?.Close();
    }
}

