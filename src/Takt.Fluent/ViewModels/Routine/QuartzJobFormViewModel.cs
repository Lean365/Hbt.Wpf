// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：QuartzJobFormViewModel.cs
// 创建时间：2025-12-01
// 创建人：Takt365(Cursor AI)
// 功能描述：任务表单视图模型（新建/编辑任务）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 任务表单视图模型（新建/编辑任务）
/// 使用 WPF 原生验证系统 INotifyDataErrorInfo
/// </summary>
public partial class QuartzJobFormViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly IQuartzJobService _quartzJobService;
    private readonly IQuartzSchedulerManager _quartzSchedulerManager;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private string _jobName = string.Empty;

    [ObservableProperty]
    private string _jobGroup = "DEFAULT";

    [ObservableProperty]
    private string _triggerName = string.Empty;

    [ObservableProperty]
    private string _triggerGroup = "DEFAULT";

    [ObservableProperty]
    private string _cronExpression = string.Empty;

    [ObservableProperty]
    private string _jobClassName = string.Empty;

    [ObservableProperty]
    private string? _jobDescription;

    [ObservableProperty]
    private int _status = 0; // 0=启用，1=禁用，2=运行中，3=暂停

    [ObservableProperty]
    private string? _jobParams;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

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

    // 错误属性用于向后兼容
    private string _jobNameError = string.Empty;
    public string JobNameError
    {
        get => _jobNameError;
        private set
        {
            if (SetProperty(ref _jobNameError, value))
            {
                SetError(nameof(JobName), value);
            }
        }
    }

    private string _jobGroupError = string.Empty;
    public string JobGroupError
    {
        get => _jobGroupError;
        private set
        {
            if (SetProperty(ref _jobGroupError, value))
            {
                SetError(nameof(JobGroup), value);
            }
        }
    }

    private string _triggerNameError = string.Empty;
    public string TriggerNameError
    {
        get => _triggerNameError;
        private set
        {
            if (SetProperty(ref _triggerNameError, value))
            {
                SetError(nameof(TriggerName), value);
            }
        }
    }

    private string _triggerGroupError = string.Empty;
    public string TriggerGroupError
    {
        get => _triggerGroupError;
        private set
        {
            if (SetProperty(ref _triggerGroupError, value))
            {
                SetError(nameof(TriggerGroup), value);
            }
        }
    }

    private string _cronExpressionError = string.Empty;
    public string CronExpressionError
    {
        get => _cronExpressionError;
        private set
        {
            if (SetProperty(ref _cronExpressionError, value))
            {
                SetError(nameof(CronExpression), value);
            }
        }
    }

    private string _jobClassNameError = string.Empty;
    public string JobClassNameError
    {
        get => _jobClassNameError;
        private set
        {
            if (SetProperty(ref _jobClassNameError, value))
            {
                SetError(nameof(JobClassName), value);
            }
        }
    }

    private string _jobParamsError = string.Empty;
    public string JobParamsError
    {
        get => _jobParamsError;
        private set
        {
            if (SetProperty(ref _jobParamsError, value))
            {
                SetError(nameof(JobParams), value);
            }
        }
    }

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口和刷新列表
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public QuartzJobFormViewModel(
        IQuartzJobService quartzJobService,
        IQuartzSchedulerManager quartzSchedulerManager,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _quartzJobService = quartzJobService ?? throw new ArgumentNullException(nameof(quartzJobService));
        _quartzSchedulerManager = quartzSchedulerManager ?? throw new ArgumentNullException(nameof(quartzSchedulerManager));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;
    }

    /// <summary>
    /// 初始化创建模式
    /// </summary>
    public void ForCreate()
    {
        ClearAllErrors();

        IsCreate = true;
        // 拼接标题：新增 + 任务
        var createText = _localizationManager.GetString("common.button.create");
        var entityText = _localizationManager.GetString("routine.quartzjob.entity");
        Title = $"{createText}{entityText}";
        Id = 0;
        JobName = string.Empty;
        JobGroup = "DEFAULT";
        TriggerName = string.Empty;
        TriggerGroup = "DEFAULT";
        CronExpression = string.Empty;
        JobClassName = string.Empty;
        JobDescription = null;
        Status = 0; // 默认启用
        JobParams = null;
        Remarks = null;
    }

    /// <summary>
    /// 初始化编辑模式
    /// </summary>
    public async Task ForUpdateAsync(long id)
    {
        ClearAllErrors();

        IsCreate = false;
        // 拼接标题：更新 + 任务
        var updateText = _localizationManager.GetString("common.button.update");
        var entityText = _localizationManager.GetString("routine.quartzjob.entity");
        Title = $"{updateText}{entityText}";

        var result = await _quartzJobService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            Error = result.Message ?? "获取任务信息失败";
            return;
        }

        var dto = result.Data;
        Id = dto.Id;
        JobName = dto.JobName;
        JobGroup = dto.JobGroup;
        TriggerName = dto.TriggerName;
        TriggerGroup = dto.TriggerGroup;
        CronExpression = dto.CronExpression;
        JobClassName = dto.JobClassName;
        JobDescription = dto.JobDescription;
        Status = dto.Status;
        JobParams = dto.JobParams;
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息（同时清除 WPF 原生验证错误）
    /// </summary>
    private void ClearAllErrors()
    {
        JobNameError = string.Empty;
        JobGroupError = string.Empty;
        TriggerNameError = string.Empty;
        TriggerGroupError = string.Empty;
        CronExpressionError = string.Empty;
        JobClassNameError = string.Empty;
        JobParamsError = string.Empty;
        Error = string.Empty;
        ClearAllValidationErrors();
    }

    /// <summary>
    /// 验证所有必填字段（提交验证）
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        // 验证任务名称
        var jobNameTrimmed = JobName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jobNameTrimmed))
        {
            JobNameError = _localizationManager.GetString("routine.quartzjob.validation.jobnamerequired");
            isValid = false;
        }
        else if (jobNameTrimmed.Length > 100)
        {
            JobNameError = _localizationManager.GetString("routine.quartzjob.validation.jobnamemaxlength");
            isValid = false;
        }

        // 验证任务组
        var jobGroupTrimmed = JobGroup?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jobGroupTrimmed))
        {
            JobGroupError = _localizationManager.GetString("routine.quartzjob.validation.jobgrouprequired");
            isValid = false;
        }
        else if (jobGroupTrimmed.Length > 50)
        {
            JobGroupError = _localizationManager.GetString("routine.quartzjob.validation.jobgroupmaxlength");
            isValid = false;
        }

        // 验证触发器名称
        var triggerNameTrimmed = TriggerName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(triggerNameTrimmed))
        {
            TriggerNameError = _localizationManager.GetString("routine.quartzjob.validation.triggernamerequired");
            isValid = false;
        }
        else if (triggerNameTrimmed.Length > 100)
        {
            TriggerNameError = _localizationManager.GetString("routine.quartzjob.validation.triggernamemaxlength");
            isValid = false;
        }

        // 验证触发器组
        var triggerGroupTrimmed = TriggerGroup?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(triggerGroupTrimmed))
        {
            TriggerGroupError = _localizationManager.GetString("routine.quartzjob.validation.triggergrouprequired");
            isValid = false;
        }
        else if (triggerGroupTrimmed.Length > 50)
        {
            TriggerGroupError = _localizationManager.GetString("routine.quartzjob.validation.triggergroupmaxlength");
            isValid = false;
        }

        // 验证Cron表达式
        var cronExpressionTrimmed = CronExpression?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cronExpressionTrimmed))
        {
            CronExpressionError = _localizationManager.GetString("routine.quartzjob.validation.cronexpressionrequired");
            isValid = false;
        }
        else if (cronExpressionTrimmed.Length > 100)
        {
            CronExpressionError = _localizationManager.GetString("routine.quartzjob.validation.cronexpressionmaxlength");
            isValid = false;
        }

        // 验证任务类名
        var jobClassNameTrimmed = JobClassName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jobClassNameTrimmed))
        {
            JobClassNameError = _localizationManager.GetString("routine.quartzjob.validation.jobclassnamerequired");
            isValid = false;
        }
        else if (jobClassNameTrimmed.Length > 200)
        {
            JobClassNameError = _localizationManager.GetString("routine.quartzjob.validation.jobclassnamemaxlength");
            isValid = false;
        }

        // 验证任务参数（JSON格式，可选，但如果填写了需要验证格式）
        if (!string.IsNullOrWhiteSpace(JobParams))
        {
            try
            {
                JToken.Parse(JobParams);
            }
            catch
            {
                JobParamsError = _localizationManager.GetString("routine.quartzjob.validation.jobparamsinvalid");
                isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// 保存任务
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

            if (IsCreate)
            {
                var dto = new QuartzJobCreateDto
                {
                    JobName = JobName.Trim(),
                    JobGroup = JobGroup.Trim(),
                    TriggerName = TriggerName.Trim(),
                    TriggerGroup = TriggerGroup.Trim(),
                    CronExpression = CronExpression.Trim(),
                    JobClassName = JobClassName.Trim(),
                    JobDescription = string.IsNullOrWhiteSpace(JobDescription) ? null! : JobDescription.Trim(),
                    JobParams = string.IsNullOrWhiteSpace(JobParams) ? null! : JobParams.Trim(),
                    Remarks = string.IsNullOrWhiteSpace(Remarks) ? null! : Remarks.Trim()
                };

                var result = await _quartzJobService.CreateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("routine.quartzjob.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 创建成功，显示成功消息
                var createEntityName = _localizationManager.GetString("routine.quartzjob.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.create"), createEntityName);
                TaktMessageManager.ShowSuccess(successMessage);

                // 创建成功后，将任务添加到调度器
                var addResult = await _quartzSchedulerManager.AddJobAsync(
                    dto.JobName,
                    dto.JobGroup,
                    dto.TriggerName,
                    dto.TriggerGroup,
                    dto.CronExpression,
                    dto.JobClassName,
                    dto.JobParams);

                if (!addResult)
                {
                    _operLog?.Warning("[QuartzJobForm] 创建任务成功，但添加到调度器失败，任务名称={JobName}", dto.JobName);
                }

                _operLog?.Information("[QuartzJobForm] 创建任务成功，ID={Id}, 任务名称={JobName}", result.Data, dto.JobName);
            }
            else
            {
                var dto = new QuartzJobUpdateDto
                {
                    Id = Id,
                    JobName = JobName.Trim(),
                    JobGroup = JobGroup.Trim(),
                    TriggerName = TriggerName.Trim(),
                    TriggerGroup = TriggerGroup.Trim(),
                    CronExpression = CronExpression.Trim(),
                    JobClassName = JobClassName.Trim(),
                    JobDescription = string.IsNullOrWhiteSpace(JobDescription) ? null! : JobDescription.Trim(),
                    Status = Status,
                    JobParams = string.IsNullOrWhiteSpace(JobParams) ? null! : JobParams.Trim(),
                    Remarks = string.IsNullOrWhiteSpace(Remarks) ? null! : Remarks.Trim()
                };

                var result = await _quartzJobService.UpdateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("routine.quartzjob.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 更新成功，显示成功消息
                var updateEntityName = _localizationManager.GetString("routine.quartzjob.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.update"), updateEntityName);
                TaktMessageManager.ShowSuccess(successMessage);

                // 更新成功后，重新加载任务到调度器
                // 先删除旧任务
                var jobResult = await _quartzJobService.GetByIdAsync(Id);
                if (jobResult.Success && jobResult.Data != null)
                {
                    await _quartzSchedulerManager.DeleteJobAsync(
                        jobResult.Data.JobName,
                        jobResult.Data.JobGroup);
                }

                // 添加新任务
                var addResult = await _quartzSchedulerManager.AddJobAsync(
                    dto.JobName,
                    dto.JobGroup,
                    dto.TriggerName,
                    dto.TriggerGroup,
                    dto.CronExpression,
                    dto.JobClassName,
                    dto.JobParams);

                if (!addResult)
                {
                    _operLog?.Warning("[QuartzJobForm] 更新任务成功，但重新添加到调度器失败，任务名称={JobName}", dto.JobName);
                }

                _operLog?.Information("[QuartzJobForm] 更新任务成功，ID={Id}, 任务名称={JobName}", Id, dto.JobName);
            }

            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
            _operLog?.Error(ex, "[QuartzJobForm] 保存任务失败");
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // 关闭窗口
        var window = System.Windows.Application.Current?.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this);
        window?.Close();
    }
}

