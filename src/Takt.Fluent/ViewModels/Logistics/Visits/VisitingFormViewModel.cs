// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visits
// 文件名称：VisitingFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员表单视图模型（新建/编辑随行人员）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logistics.Visits;
using Takt.Application.Services.Logistics.Visits;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;

namespace Takt.Fluent.ViewModels.Logistics.Visits;

/// <summary>
/// 来访公司表单视图模型（新建/编辑来访公司）
/// 使用 WPF 原生验证系统 INotifyDataErrorInfo
/// </summary>
public partial class VisitingFormViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly IVisitingCompanyService _visitingCompanyService;
    private readonly IVisitingEntourageService _visitingEntourageService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private string _visitingCompany = string.Empty;

    [ObservableProperty]
    private DateTime _visitStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);

    [ObservableProperty]
    private DateTime _visitEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);

    /// <summary>
    /// 开始日期（用于绑定到 DatePicker）
    /// </summary>
    public DateTime? VisitStartDate
    {
        get => VisitStartTime.Date;
        set
        {
            if (value.HasValue)
            {
                VisitStartTime = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 
                    VisitStartTime.Hour, VisitStartTime.Minute, VisitStartTime.Second);
            }
        }
    }

    /// <summary>
    /// 开始时间部分（用于绑定到 TimePicker）
    /// </summary>
    public TimeSpan? VisitStartTimeOfDay
    {
        get => VisitStartTime.TimeOfDay;
        set
        {
            if (value.HasValue)
            {
                VisitStartTime = new DateTime(VisitStartTime.Year, VisitStartTime.Month, VisitStartTime.Day,
                    value.Value.Hours, value.Value.Minutes, value.Value.Seconds);
            }
        }
    }

    /// <summary>
    /// 结束日期（用于绑定到 DatePicker）
    /// </summary>
    public DateTime? VisitEndDate
    {
        get => VisitEndTime.Date;
        set
        {
            if (value.HasValue)
            {
                VisitEndTime = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day,
                    VisitEndTime.Hour, VisitEndTime.Minute, VisitEndTime.Second);
            }
        }
    }

    /// <summary>
    /// 结束时间部分（用于绑定到 TimePicker）
    /// </summary>
    public TimeSpan? VisitEndTimeOfDay
    {
        get => VisitEndTime.TimeOfDay;
        set
        {
            if (value.HasValue)
            {
                VisitEndTime = new DateTime(VisitEndTime.Year, VisitEndTime.Month, VisitEndTime.Day,
                    value.Value.Hours, value.Value.Minutes, value.Value.Seconds);
            }
        }
    }

    /// <summary>
    /// 开始时间文本（保留用于向后兼容，但不再在 XAML 中使用）
    /// </summary>
    [ObservableProperty]
    private string _startTimeText = string.Empty;

    /// <summary>
    /// 结束时间文本（保留用于向后兼容，但不再在 XAML 中使用）
    /// </summary>
    [ObservableProperty]
    private string _endTimeText = string.Empty;

    [ObservableProperty]
    private string? _reservationsDept;

    [ObservableProperty]
    private string? _contact;

    [ObservableProperty]
    private string? _purpose;

    [ObservableProperty]
    private int? _duration;

    [ObservableProperty]
    private string? _industry;

    [ObservableProperty]
    private string? _vehiclePlate;

    [ObservableProperty]
    private int _isWelcomeSign = 0;

    [ObservableProperty]
    private int _isVehicleNeeded = 1;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    // 错误消息属性（保留用于向后兼容，同时更新 INotifyDataErrorInfo）
    private string _visitingCompanyError = string.Empty;
    public string VisitingCompanyError
    {
        get => _visitingCompanyError;
        private set
        {
            if (SetProperty(ref _visitingCompanyError, value))
            {
                SetError(nameof(VisitingCompany), value);
            }
        }
    }

    private string _visitStartTimeError = string.Empty;
    public string VisitStartTimeError
    {
        get => _visitStartTimeError;
        private set
        {
            if (SetProperty(ref _visitStartTimeError, value))
            {
                SetError(nameof(StartTimeText), value);
            }
        }
    }

    private string _visitEndTimeError = string.Empty;
    public string VisitEndTimeError
    {
        get => _visitEndTimeError;
        private set
        {
            if (SetProperty(ref _visitEndTimeError, value))
            {
                SetError(nameof(EndTimeText), value);
            }
        }
    }

    private string _remarksError = string.Empty;
    public string RemarksError
    {
        get => _remarksError;
        private set
        {
            if (SetProperty(ref _remarksError, value))
            {
                SetError(nameof(Remarks), value);
            }
        }
    }

    // 子表数据
    public ObservableCollection<VisitingEntourageDto> EntourageDetails { get; } = new();

    [ObservableProperty]
    private VisitingEntourageDto? _selectedEntourageDetail;

    [ObservableProperty]
    private VisitingEntourageDto? _editingEntourageDetail;

    // Hint 提示属性
    public string VisitingCompanyHint => _localizationManager.GetString("logistics.visitors.validation.companynamehint");
    
    public string VisitStartTimeHint => _localizationManager.GetString("logistics.visitors.validation.starttimehint");
    
    public string VisitEndTimeHint => _localizationManager.GetString("logistics.visitors.validation.endtimehint");

    public string RemarksHint => _localizationManager.GetString("identity.user.validation.remarkshint");

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    // WPF 原生验证系统：INotifyDataErrorInfo 实现
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(e => e);
        }

        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    /// <summary>
    /// 设置字段错误（WPF 原生验证，替换现有错误）
    /// </summary>
    private void SetError(string propertyName, string? error)
    {
        if (string.IsNullOrEmpty(error))
        {
            if (_errors.Remove(propertyName))
            {
                OnErrorsChanged(propertyName);
            }
        }
        else
        {
            _errors[propertyName] = new List<string> { error };
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// 清除所有错误（WPF 原生验证）
    /// </summary>
    private void ClearAllValidationErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    public VisitingFormViewModel(
        IVisitingCompanyService visitingCompanyService,
        IVisitingEntourageService visitingEntourageService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _visitingCompanyService = visitingCompanyService ?? throw new ArgumentNullException(nameof(visitingCompanyService));
        _visitingEntourageService = visitingEntourageService ?? throw new ArgumentNullException(nameof(visitingEntourageService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;
        
        // 初始化时间文本
        UpdateTimeTextProperties();
    }

    public void ForCreate()
    {
        ClearAllErrors();

        IsCreate = true;
        // 拼接标题：新增 + 随行人员
        var createText = _localizationManager.GetString("common.button.create");
        var entityText = _localizationManager.GetString("logistics.visitors.entity");
        Title = $"{createText}{entityText}";
        VisitingCompany = string.Empty;
        var now = DateTime.Now;
        VisitStartTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
        VisitEndTime = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0);
        UpdateTimeTextProperties();
        ReservationsDept = null;
        Contact = null;
        Purpose = null;
        Duration = null;
        Industry = null;
        VehiclePlate = null;
        IsWelcomeSign = 0;
        IsVehicleNeeded = 1;
        Remarks = null;

        // 清空子表数据
        EntourageDetails.Clear();
        SelectedEntourageDetail = null;
        EditingEntourageDetail = null;
    }

    public void ForUpdate(VisitingCompanyDto dto)
    {
        ClearAllErrors();

        IsCreate = false;
        // 拼接标题：更新 + 随行人员
        var updateText = _localizationManager.GetString("common.button.update");
        var entityText = _localizationManager.GetString("logistics.visitors.entity");
        Title = $"{updateText}{entityText}";
        Id = dto.Id;
        VisitingCompany = dto.VisitingCompanyName ?? string.Empty;
        VisitStartTime = dto.VisitStartTime;
        VisitEndTime = dto.VisitEndTime;
        UpdateTimeTextProperties();
        ReservationsDept = dto.ReservationsDept;
        Contact = dto.Contact;
        Purpose = dto.Purpose;
        Duration = dto.Duration;
        Industry = dto.Industry;
        VehiclePlate = dto.VehiclePlate;
        IsWelcomeSign = dto.IsWelcomeSign;
        IsVehicleNeeded = dto.IsVehicleNeeded;
        Remarks = dto.Remarks;

        // 清空子表数据
        EntourageDetails.Clear();
        SelectedEntourageDetail = null;
        EditingEntourageDetail = null;

        // 异步加载子表数据
        _ = LoadEntourageDetailsAsync();
    }

    /// <summary>
    /// 加载随行人员详情
    /// </summary>
    private async Task LoadEntourageDetailsAsync()
    {
        if (Id <= 0)
        {
            return;
        }

        try
        {
            var query = new VisitingEntourageQueryDto
            {
                VisitingCompanyId = Id,
                PageIndex = 1,
                PageSize = 1000 // 加载所有详情
            };

            var result = await _visitingEntourageService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    EntourageDetails.Clear();
                    foreach (var detail in result.Data.Items)
                    {
                        EntourageDetails.Add(detail);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[EntourageForm] 加载随行人员详情失败");
        }
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        VisitingCompanyError = string.Empty;
        VisitStartTimeError = string.Empty;
        VisitEndTimeError = string.Empty;
        RemarksError = string.Empty;
        Error = string.Empty;
        ClearAllValidationErrors();
    }

    /// <summary>
    /// 更新时间文本属性（基于 DateTime 属性）
    /// </summary>
    private void UpdateTimeTextProperties()
    {
        StartTimeText = VisitStartTime.ToString("yyyy-MM-dd HH:mm");
        EndTimeText = VisitEndTime.ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>
    /// 从文本解析 DateTime
    /// </summary>
    private bool TryParseDateTime(string text, out DateTime dateTime)
    {
        dateTime = DateTime.Now;
        
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        // 尝试多种日期时间格式
        string[] formats = new[]
        {
            "yyyy-MM-dd HH:mm",
            "yyyy/MM/dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy-MM-dd",
            "yyyy/MM/dd"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(text.Trim(), format, null, System.Globalization.DateTimeStyles.None, out dateTime))
            {
                // 如果只有日期没有时间，使用默认时间
                if (format.Contains("yyyy") && !format.Contains("HH"))
                {
                    dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 8, 0, 0);
                }
                return true;
            }
        }

        // 如果所有格式都不匹配，尝试通用解析
        if (DateTime.TryParse(text.Trim(), out dateTime))
        {
            return true;
        }

        return false;
    }

    // 属性变更时进行实时验证
    partial void OnVisitingCompanyChanged(string value)
    {
        ValidateVisitingCompany();
    }

    partial void OnVisitStartTimeChanged(DateTime value)
    {
        UpdateTimeTextProperties();
        OnPropertyChanged(nameof(VisitStartDate));
        OnPropertyChanged(nameof(VisitStartTimeOfDay));
        ValidateTimeRange();
    }

    partial void OnVisitEndTimeChanged(DateTime value)
    {
        UpdateTimeTextProperties();
        OnPropertyChanged(nameof(VisitEndDate));
        OnPropertyChanged(nameof(VisitEndTimeOfDay));
        ValidateTimeRange();
    }

    partial void OnStartTimeTextChanged(string value)
    {
        if (TryParseDateTime(value, out var dateTime))
        {
            VisitStartTime = dateTime;
        }
        else if (!string.IsNullOrWhiteSpace(value))
        {
            VisitStartTimeError = _localizationManager.GetString("logistics.visitors.validation.starttimeinvalidformat");
        }
        else
        {
            VisitStartTimeError = string.Empty;
        }
    }

    partial void OnEndTimeTextChanged(string value)
    {
        if (TryParseDateTime(value, out var dateTime))
        {
            VisitEndTime = dateTime;
        }
        else if (!string.IsNullOrWhiteSpace(value))
        {
            VisitEndTimeError = _localizationManager.GetString("logistics.visitors.validation.endtimeinvalidformat");
        }
        else
        {
            VisitEndTimeError = string.Empty;
        }
    }

    /// <summary>
    /// 验证公司名称（实时验证）
    /// </summary>
    private void ValidateVisitingCompany()
    {
        if (string.IsNullOrWhiteSpace(VisitingCompany))
        {
            VisitingCompanyError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (VisitingCompany.Length > 200)
        {
            VisitingCompanyError = _localizationManager.GetString("logistics.visitors.validation.companynamemaxlength");
        }
        else
        {
            VisitingCompanyError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证时间范围（实时验证）
    /// </summary>
    private void ValidateTimeRange()
    {
        VisitStartTimeError = string.Empty;
        VisitEndTimeError = string.Empty;

        if (VisitStartTime >= VisitEndTime)
        {
            VisitStartTimeError = _localizationManager.GetString("logistics.visitors.validation.starttimemustbeforeendtime");
            VisitEndTimeError = _localizationManager.GetString("logistics.visitors.validation.endtimemustafterstarttime");
            return;
        }

        // 验证时间段间隔：结束时间必须比开始时间至少大1小时
        var timeDiff = VisitEndTime - VisitStartTime;
        if (timeDiff.TotalHours < 1.0)
        {
            var errorMessage = $"结束时间必须比开始时间至少大1小时（当前间隔：{timeDiff.TotalMinutes:F0} 分钟）";
            VisitStartTimeError = errorMessage;
            VisitEndTimeError = errorMessage;
        }
    }

    /// <summary>
    /// 验证所有字段（主表和子表）
    /// </summary>
    private bool ValidateFields()
    {
        bool isValid = true;

        // 验证主表字段：公司名称（VisitingCompany）
        if (string.IsNullOrWhiteSpace(VisitingCompany))
        {
            VisitingCompanyError = _localizationManager.GetString("logistics.visitors.validation.companynamerequired");
            isValid = false;
        }
        else if (VisitingCompany.Length > 200)
        {
            VisitingCompanyError = _localizationManager.GetString("logistics.visitors.validation.companynamemaxlength");
            isValid = false;
        }
        else
        {
            VisitingCompanyError = string.Empty;
        }

        // 验证主表字段：开始时间（VisitStartTime）
        if (VisitStartTime == default || string.IsNullOrWhiteSpace(StartTimeText) || !TryParseDateTime(StartTimeText, out var startTime))
        {
            VisitStartTimeError = _localizationManager.GetString("logistics.visitors.validation.starttimerequired");
            isValid = false;
        }
        else
        {
            VisitStartTimeError = string.Empty;
        }

        // 验证主表字段：结束时间（VisitEndTime）
        if (VisitEndTime == default || string.IsNullOrWhiteSpace(EndTimeText) || !TryParseDateTime(EndTimeText, out var endTime))
        {
            VisitEndTimeError = _localizationManager.GetString("logistics.visitors.validation.endtimerequired");
            isValid = false;
        }
        else
        {
            VisitEndTimeError = string.Empty;
        }

        // 验证时间范围
        if (VisitStartTime != default && VisitEndTime != default)
        {
            if (VisitStartTime >= VisitEndTime)
            {
                VisitStartTimeError = _localizationManager.GetString("logistics.visitors.validation.starttimemustbeforeendtime");
                VisitEndTimeError = _localizationManager.GetString("logistics.visitors.validation.endtimemustafterstarttime");
                isValid = false;
            }
            else
            {
                // 验证时间段间隔：结束时间必须比开始时间至少大1小时
                var timeDiff = VisitEndTime - VisitStartTime;
                if (timeDiff.TotalHours < 1.0)
                {
                    var errorMessage = $"结束时间必须比开始时间至少大1小时（当前间隔：{timeDiff.TotalMinutes:F0} 分钟）";
                    VisitStartTimeError = errorMessage;
                    VisitEndTimeError = errorMessage;
                    isValid = false;
                }
            }
        }

        // 验证子表字段：遍历所有随行人员详情
        _operLog?.Information("[EntourageForm] 验证随行人员详情 - EntourageDetails.Count={Count}", EntourageDetails.Count);
        
        if (EntourageDetails.Count == 0)
        {
            Error = _localizationManager.GetString("logistics.visitors.validation.atleastonedetailrequired");
            isValid = false;
            _operLog?.Warning("[EntourageForm] 验证失败：至少需要一条随行人员详情，EntourageDetails.Count=0");
        }
        else
        {
            int validDetailCount = 0; // 有效的详情数量（字段都填写的）
            int emptyDetailCount = 0; // 无效的详情数量（字段有空的）
            
            foreach (var detail in EntourageDetails)
            {
                // 检查是否有任何字段为空
                bool hasEmptyField = string.IsNullOrWhiteSpace(detail.VisitDept) ||
                                    string.IsNullOrWhiteSpace(detail.VisitingMembers) ||
                                    string.IsNullOrWhiteSpace(detail.VisitPost);
                
                if (hasEmptyField)
                {
                    emptyDetailCount++;
                    _operLog?.Information("[EntourageForm] 发现无效详情 - Id={Id}, Dept='{Dept}', VisitingMembers='{VisitingMembers}', Post='{Post}'", 
                        detail.Id, detail.VisitDept ?? "(空)", detail.VisitingMembers ?? "(空)", detail.VisitPost ?? "(空)");
                    
                    // 验证部门（VisitDept）
                    if (string.IsNullOrWhiteSpace(detail.VisitDept))
                    {
                        Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.departmentrequired");
                        isValid = false;
                        _operLog?.Warning("[EntourageForm] 验证失败：随行人员详情部门为空，详情Id={Id}", detail.Id);
                        break;
                    }

                    // 验证姓名（VisitingMembers）
                    if (string.IsNullOrWhiteSpace(detail.VisitingMembers))
                    {
                        Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.namerequired");
                        isValid = false;
                        _operLog?.Warning("[EntourageForm] 验证失败：随行人员详情姓名为空，详情Id={Id}", detail.Id);
                        break;
                    }

                    // 验证职务（VisitPost）
                    if (string.IsNullOrWhiteSpace(detail.VisitPost))
                    {
                        Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.positionrequired");
                        isValid = false;
                        _operLog?.Warning("[EntourageForm] 验证失败：随行人员详情职务为空，详情Id={Id}", detail.Id);
                        break;
                    }
                }
                else
                {
                    validDetailCount++;
                    _operLog?.Information("[EntourageForm] 发现有效详情 - Id={Id}, Dept='{Dept}', VisitingMembers='{VisitingMembers}', Post='{Post}'", 
                        detail.Id, detail.VisitDept, detail.VisitingMembers, detail.VisitPost);
                }
            }
            
            _operLog?.Information("[EntourageForm] 验证统计 - 总数={Total}, 有效={Valid}, 无效={Empty}", 
                EntourageDetails.Count, validDetailCount, emptyDetailCount);
            
            // **关键修复：如果所有详情都无效（字段为空），应该提示需要至少一条有效详情**
            if (validDetailCount == 0 && emptyDetailCount > 0)
            {
                Error = _localizationManager.GetString("logistics.visitors.validation.atleastonedetailrequired");
                isValid = false;
                _operLog?.Warning("[EntourageForm] 验证失败：虽然有{Count}条详情，但所有详情字段都为空，至少需要一条有效的随行人员详情", EntourageDetails.Count);
            }
        }

        if (isValid)
        {
            _operLog?.Information("[EntourageForm] 字段验证通过");
        }

        return isValid;
    }

    /// <summary>
    /// 保存随行人员信息
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _operLog?.Information("[EntourageForm] 开始保存随行人员，IsCreate={IsCreate}, Id={Id}, EditingEntourageDetail={Editing}, EntourageDetails.Count={Count}", 
                IsCreate, Id, EditingEntourageDetail != null ? "存在" : "不存在", EntourageDetails.Count);
            
            // **详细记录EntourageDetails的内容**
            if (EntourageDetails.Count > 0)
            {
                for (int i = 0; i < EntourageDetails.Count; i++)
                {
                    var detail = EntourageDetails[i];
                    _operLog?.Information("[EntourageForm] EntourageDetails[{Index}]: Id={Id}, Dept='{Dept}', VisitingMembers='{VisitingMembers}', Post='{Post}'", 
                        i, detail.Id, detail.VisitDept ?? "(空)", detail.VisitingMembers ?? "(空)", detail.VisitPost ?? "(空)");
                }
            }
            else
            {
                _operLog?.Warning("[EntourageForm] EntourageDetails列表为空！");
            }
            
            // 验证字段
            if (!ValidateFields())
            {
                _operLog?.Warning("[EntourageForm] 字段验证失败，Error={Error}", Error);
                return;
            }

            // 如果有正在编辑的子表项，提示先保存或取消
            if (EditingEntourageDetail != null)
            {
                _operLog?.Warning("[EntourageForm] 存在正在编辑的子表项，无法保存主表");
                Error = _localizationManager.GetString("logistics.visitors.pleasesaveorcanceldetail");
                return;
            }

            _operLog?.Information("[EntourageForm] ✅ 验证通过，开始保存主表数据");
            
            // **关键修复**：在保存前强制更新所有绑定源，确保获取 UI 中的最新值
            // 特别是当 TextBox 还在焦点上时，最新输入可能还没有更新到 ViewModel
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // 查找当前 ViewModel 所属的窗口
                Window? ownerWindow = null;
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        ownerWindow = window;
                        break;
                    }
                }

                if (ownerWindow != null)
                {
                    // 强制失去焦点，确保所有 TextBox 的绑定都已更新
                    var focusedElement = System.Windows.Input.Keyboard.FocusedElement;
                    if (focusedElement != null)
                    {
                        System.Windows.Input.Keyboard.ClearFocus();
                    }

                    // 强制更新布局和绑定
                    ownerWindow.UpdateLayout();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }, System.Windows.Threading.DispatcherPriority.Send);

            // **再次读取属性值**：确保获取更新后的最新值
            // 在强制更新绑定后重新读取，确保获取 UI 中最新的值
            var currentCompanyName = VisitingCompany?.Trim() ?? string.Empty;
            var currentStartTime = VisitStartTime;
            var currentEndTime = VisitEndTime;
            var currentRemarks = Remarks?.Trim();

            long visitorId;

            if (IsCreate)
            {
                _operLog?.Information("[EntourageForm] 准备创建新随行人员 - 公司名称='{Company}', 开始时间={StartTime}, 结束时间={EndTime}", 
                    currentCompanyName, currentStartTime, currentEndTime);
                
                var dto = new VisitingCompanyCreateDto
                {
                    VisitingCompanyName = currentCompanyName,
                    VisitStartTime = currentStartTime,
                    VisitEndTime = currentEndTime,
                    ReservationsDept = ReservationsDept,
                    Contact = Contact,
                    Purpose = Purpose,
                    Duration = Duration,
                    Industry = Industry,
                    VehiclePlate = VehiclePlate,
                    IsWelcomeSign = IsWelcomeSign,
                    IsVehicleNeeded = IsVehicleNeeded,
                    Remarks = string.IsNullOrWhiteSpace(currentRemarks) ? null! : currentRemarks
                };

                _operLog?.Information("[EntourageForm] 调用 _visitingCompanyService.CreateAsync...");
                var result = await _visitingCompanyService.CreateAsync(dto);
                
                if (!result.Success)
                {
                    _operLog?.Error("[EntourageForm] ❌ 创建随行人员失败 - Message={Message}", result.Message);
                    var entityName = _localizationManager.GetString("logistics.visitors.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                visitorId = result.Data;
                Id = visitorId; // 更新 Id，用于后续保存子表

                _operLog?.Information("[EntourageForm] ✅ 创建随行人员成功，Id={Id}, 公司名称={Company}", visitorId, currentCompanyName);
                
                // 创建成功，显示成功消息
                var createEntityName = _localizationManager.GetString("logistics.visitors.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.create"), createEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
            }
            else
            {
                // **使用已获取的最新值**（已在上面获取）
                _operLog?.Information("[EntourageForm] 准备更新随行人员 - Id={Id}, 公司名称='{Company}', 开始时间={StartTime}, 结束时间={EndTime}", 
                    Id, currentCompanyName, currentStartTime, currentEndTime);
                
                var dto = new VisitingCompanyUpdateDto
                {
                    Id = Id,
                    VisitingCompanyName = currentCompanyName,
                    VisitStartTime = currentStartTime,
                    VisitEndTime = currentEndTime,
                    ReservationsDept = ReservationsDept,
                    Contact = Contact,
                    Purpose = Purpose,
                    Duration = Duration,
                    Industry = Industry,
                    VehiclePlate = VehiclePlate,
                    IsWelcomeSign = IsWelcomeSign,
                    IsVehicleNeeded = IsVehicleNeeded,
                    Remarks = string.IsNullOrWhiteSpace(currentRemarks) ? null! : currentRemarks
                };

                _operLog?.Information("[EntourageForm] 调用 _visitingCompanyService.UpdateAsync...");
                var result = await _visitingCompanyService.UpdateAsync(dto);
                
                if (!result.Success)
                {
                    _operLog?.Error("[EntourageForm] ❌ 更新随行人员失败 - Message={Message}", result.Message);
                    var entityName = _localizationManager.GetString("logistics.visitors.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                visitorId = Id;

                _operLog?.Information("[EntourageForm] ✅ 更新随行人员成功，Id={Id}, 公司名称={Company}", Id, currentCompanyName);
                
                // 更新成功，显示成功消息
                var updateEntityName = _localizationManager.GetString("logistics.visitors.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.update"), updateEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
            }

            // 保存子表数据
            _operLog?.Information("[EntourageForm] 开始保存子表数据，visitorId={EntourageId}, 详情数量={Count}", visitorId, EntourageDetails.Count);
            await SaveEntourageDetailsAsync(visitorId);
            
            // 检查子表保存是否失败
            if (!string.IsNullOrWhiteSpace(Error))
            {
                _operLog?.Warning("[EntourageForm] ⚠️ 子表数据保存失败 - Error={Error}", Error);
                TaktMessageManager.ShowError(Error);
                return;
            }
            
            _operLog?.Information("[EntourageForm] ✅ 子表数据保存完成");

            _operLog?.Information("[EntourageForm] 调用 SaveSuccessCallback...");
            
            // **注意：成功消息框已在创建/更新主表成功后显示，这里不再重复显示**
            // **但在关闭窗口前，确保成功消息框已显示（如果主表保存成功）**
            
            SaveSuccessCallback?.Invoke();
            _operLog?.Information("[EntourageForm] ✅ 保存流程全部完成");
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            _operLog?.Error(ex, "[EntourageForm] 保存随行人员失败");
            TaktMessageManager.ShowError(errorMessage);
        }
    }

    /// <summary>
    /// 保存随行人员详情
    /// </summary>
    private async Task SaveEntourageDetailsAsync(long visitorId)
    {
        // 获取需要新增和更新的详情项
        var newDetails = EntourageDetails.Where(d => d.Id == 0).ToList();
        var updatedDetails = EntourageDetails.Where(d => d.Id > 0).ToList();

        // 获取数据库中已存在的详情ID
        var existingDetails = await _visitingEntourageService.GetListAsync(new VisitingEntourageQueryDto
        {
            VisitingCompanyId = visitorId,
            PageIndex = 1,
            PageSize = 1000
        });

        var existingIds = existingDetails.Success && existingDetails.Data != null
            ? existingDetails.Data.Items.Select(d => d.Id).ToList()
            : new List<long>();

        // 删除已从列表中移除的详情（如果存在）
        var deletedIds = existingIds.Except(updatedDetails.Select(d => d.Id)).ToList();
        foreach (var deletedId in deletedIds)
        {
            await _visitingEntourageService.DeleteAsync(deletedId);
        }

        // 新增详情（验证必填字段：VisitDept, Entourage, VisitPost）
        foreach (var detail in newDetails)
        {
            // 验证部门（VisitDept）
            if (string.IsNullOrWhiteSpace(detail.VisitDept))
            {
                Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.departmentrequired");
                continue;
            }

            // 验证姓名（VisitingMembers）
            if (string.IsNullOrWhiteSpace(detail.VisitingMembers))
            {
                Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.namerequired");
                continue;
            }

            // 验证职务（VisitPost）
            if (string.IsNullOrWhiteSpace(detail.VisitPost))
            {
                Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.positionrequired");
                continue;
            }

            var createDto = new VisitingEntourageCreateDto
            {
                VisitingCompanyId = visitorId,
                VisitDept = detail.VisitDept.Trim(),
                VisitingMembers = detail.VisitingMembers.Trim(),
                VisitPost = detail.VisitPost.Trim(),
                Remarks = string.IsNullOrWhiteSpace(detail.Remarks) ? null! : detail.Remarks.Trim()
            };

            var result = await _visitingEntourageService.CreateAsync(createDto);
            if (result.Success && result.Data > 0)
            {
                detail.Id = result.Data;
            }
            else
            {
                Error = result.Message ?? _localizationManager.GetString("logistics.visitors.visitordetail.validation.detailsavefailed");
            }
        }

        // 更新详情（验证必填字段：VisitDept, Entourage, VisitPost）
        foreach (var detail in updatedDetails)
        {
            // 验证部门（VisitDept）
            if (string.IsNullOrWhiteSpace(detail.VisitDept))
            {
                Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.departmentrequired");
                continue;
            }

            // 验证姓名（VisitingMembers）
            if (string.IsNullOrWhiteSpace(detail.VisitingMembers))
            {
                Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.namerequired");
                continue;
            }

            // 验证职务（VisitPost）
            if (string.IsNullOrWhiteSpace(detail.VisitPost))
            {
                Error = _localizationManager.GetString("logistics.visitors.visitordetail.validation.positionrequired");
                continue;
            }

            var updateDto = new VisitingEntourageUpdateDto
            {
                Id = detail.Id,
                VisitingCompanyId = visitorId,
                VisitDept = detail.VisitDept.Trim(),
                VisitingMembers = detail.VisitingMembers.Trim(),
                VisitPost = detail.VisitPost.Trim(),
                Remarks = string.IsNullOrWhiteSpace(detail.Remarks) ? null! : detail.Remarks.Trim()
            };

            var updateResult = await _visitingEntourageService.UpdateAsync(updateDto);
            if (!updateResult.Success)
            {
                Error = updateResult.Message ?? _localizationManager.GetString("logistics.visitors.visitordetail.validation.detailupdatefailed");
            }
        }
    }

    /// <summary>
    /// 取消操作（由窗口关闭事件调用）
    /// </summary>
    public void OnCancel()
    {
        // 清除所有错误信息
        ClearAllErrors();
        
        // 取消正在编辑的子表项
        if (EditingEntourageDetail != null)
        {
            var detail = EditingEntourageDetail;
            
            // 如果是新添加的项（Id=0），从列表中移除
            if (detail.Id == 0)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    EntourageDetails.Remove(detail);
                });
            }
            
            EditingEntourageDetail = null;
            SelectedEntourageDetail = null;
        }
    }

    #region 子表命令

    partial void OnEditingEntourageDetailChanged(VisitingEntourageDto? value)
    {
        // 通知所有相关命令重新评估 CanExecute
        CreateDetailInlineCommand.NotifyCanExecuteChanged();
        UpdateDetailInlineCommand.NotifyCanExecuteChanged();
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();
        DeleteDetailCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    [RelayCommand(CanExecute = nameof(CanCreateDetailInline))]
    private void CreateDetailInline()
    {
        // 验证主表必填字段：公司名称、开始时间、结束时间
        if (string.IsNullOrWhiteSpace(VisitingCompany))
        {
            VisitingCompanyError = _localizationManager.GetString("logistics.visitors.validation.companynamerequired");
            Error = _localizationManager.GetString("logistics.visitors.pleasefillmaintablefirst");
            return;
        }

        if (VisitStartTime == default || string.IsNullOrWhiteSpace(StartTimeText) || !TryParseDateTime(StartTimeText, out _))
        {
            VisitStartTimeError = _localizationManager.GetString("logistics.visitors.validation.starttimerequired");
            Error = _localizationManager.GetString("logistics.visitors.pleasefillmaintablefirst");
            return;
        }

        if (VisitEndTime == default || string.IsNullOrWhiteSpace(EndTimeText) || !TryParseDateTime(EndTimeText, out _))
        {
            VisitEndTimeError = _localizationManager.GetString("logistics.visitors.validation.endtimerequired");
            Error = _localizationManager.GetString("logistics.visitors.pleasefillmaintablefirst");
            return;
        }

        // 清除错误信息
        VisitingCompanyError = string.Empty;
        VisitStartTimeError = string.Empty;
        VisitEndTimeError = string.Empty;
        Error = string.Empty;

        // 创建新的随行人员详情对象
        var newDetail = new VisitingEntourageDto
        {
            VisitingCompanyId = Id > 0 ? Id : 0, // 如果是新建，Id 为 0，保存主表后会更新
            VisitDept = string.Empty,
            VisitingMembers = string.Empty,
            VisitPost = string.Empty,
            CreatedTime = DateTime.Now,
            UpdatedTime = DateTime.Now
        };

        // 添加到列表（在 UI 线程上）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            EntourageDetails.Add(newDetail);
        });

        // 设置正在编辑的项，让 TaktInlineEditDataGrid 自动进入编辑状态
        EditingEntourageDetail = newDetail;
        SelectedEntourageDetail = newDetail;

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

        _operLog?.Information("[EntourageForm] 新增随行人员详情行");
    }

    private bool CanCreateDetailInline()
    {
        return EditingEntourageDetail == null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdateDetailInline))]
    private void UpdateDetailInline(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedEntourageDetail;
        }

        if (detail == null)
        {
            return;
        }

        EditingEntourageDetail = detail;

        // 通知命令重新评估 CanExecute
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[EntourageForm] 进入编辑随行人员详情状态，详情Id={Id}", detail.Id);
    }

    private bool CanUpdateDetailInline(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            return SelectedEntourageDetail is not null && EditingEntourageDetail == null;
        }
        return detail is not null && EditingEntourageDetail == null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveDetailInline))]
    private void SaveDetailInline(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            detail = EditingEntourageDetail;
        }

        if (detail == null || EditingEntourageDetail != detail)
        {
            _operLog?.Warning("[EntourageForm] SaveDetailInline: detail或EditingEntourageDetail为空或不匹配");
            return;
        }

        _operLog?.Information("[EntourageForm] SaveDetailInline开始 - detail.Id={Id}, Dept='{Dept}', VisitingMembers='{VisitingMembers}', Post='{Post}', EntourageDetails.Count={Count}", 
            detail.Id, detail.VisitDept ?? "(空)", detail.VisitingMembers ?? "(空)", detail.VisitPost ?? "(空)", EntourageDetails.Count);

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(detail.VisitDept) || 
            string.IsNullOrWhiteSpace(detail.VisitingMembers) || 
            string.IsNullOrWhiteSpace(detail.VisitPost))
        {
            _operLog?.Warning("[EntourageForm] SaveDetailInline验证失败：字段为空 - Dept='{Dept}', VisitingMembers='{VisitingMembers}', Post='{Post}'", 
                detail.VisitDept ?? "(空)", detail.VisitingMembers ?? "(空)", detail.VisitPost ?? "(空)");
            Error = _localizationManager.GetString("logistics.visitors.detailfieldsrequired");
            return;
        }

        // **关键：确保detail对象在EntourageDetails集合中**
        if (!EntourageDetails.Contains(detail))
        {
            _operLog?.Warning("[EntourageForm] SaveDetailInline: detail不在EntourageDetails集合中，尝试添加");
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                if (!EntourageDetails.Contains(detail))
                {
                    EntourageDetails.Add(detail);
                    _operLog?.Information("[EntourageForm] SaveDetailInline: detail已添加到EntourageDetails集合");
                }
            });
        }

        // 清除编辑状态（实际保存会在保存主表时进行）
        EditingEntourageDetail = null;

        // 通知命令重新评估 CanExecute
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();
        
        // 通知所有命令重新评估（包括 SaveCommand）
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();

        _operLog?.Information("[EntourageForm] 随行人员详情保存成功（待主表保存时提交），EditingEntourageDetail已清除，当前EntourageDetails.Count={Count}", EntourageDetails.Count);
    }

    private bool CanSaveDetailInline(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            return EditingEntourageDetail is not null;
        }
        return EditingEntourageDetail != null && EditingEntourageDetail == detail;
    }

    [RelayCommand(CanExecute = nameof(CanCancelDetailInline))]
    private void CancelDetailInline()
    {
        if (EditingEntourageDetail != null)
        {
            var detail = EditingEntourageDetail;

            // 如果是新添加的项（Id=0），从列表中移除
            if (detail.Id == 0)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    EntourageDetails.Remove(detail);
                });
            }

            EditingEntourageDetail = null;
            SelectedEntourageDetail = null;

            // 通知命令重新评估 CanExecute
            SaveDetailInlineCommand.NotifyCanExecuteChanged();
            CancelDetailInlineCommand.NotifyCanExecuteChanged();

            _operLog?.Information("[EntourageForm] 取消编辑随行人员详情");
        }
    }

    private bool CanCancelDetailInline()
    {
        return EditingEntourageDetail != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteDetail))]
    private void DeleteDetail(VisitingEntourageDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedEntourageDetail;
        }

        if (detail == null)
        {
            return;
        }

        // 从列表中移除（如果是新添加的项，直接移除；如果是已存在的项，标记为删除，在保存时处理）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            EntourageDetails.Remove(detail);
        });

        SelectedEntourageDetail = null;

        _operLog?.Information("[EntourageForm] 删除随行人员详情，详情Id={Id}", detail.Id);
    }

    private bool CanDeleteDetail(VisitingEntourageDto? detail)
    {
        return EditingEntourageDetail == null; // 编辑状态下不能删除
    }

    #endregion
}

