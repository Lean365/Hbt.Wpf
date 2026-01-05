// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：DictionaryFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典表单视图模型（新建/编辑字典类型和字典数据）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections;
using System.ComponentModel;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Fluent.Controls;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 字典表单视图模型（新建/编辑字典类型和字典数据）
/// 使用 WPF 原生验证系统 INotifyDataErrorInfo
/// </summary>
public partial class DictionaryFormViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly IDictionaryTypeService _dictionaryTypeService;
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    // 主表字段
    [ObservableProperty]
    private string _typeCode = string.Empty;

    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    private int _dataSource;

    [ObservableProperty]
    private string? _sqlScript;

    [ObservableProperty]
    private int _orderNum;

    [ObservableProperty]
    private int _isBuiltin;

    [ObservableProperty]
    private int _typeStatus;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    // WPF 原生验证系统：INotifyDataErrorInfo 实现
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

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

    // 保留原有的 Error 属性用于向后兼容（同时更新 INotifyDataErrorInfo）
    private string _typeCodeError = string.Empty;
    public string TypeCodeError
    {
        get => _typeCodeError;
        private set
        {
            if (SetProperty(ref _typeCodeError, value))
            {
                SetError(nameof(TypeCode), value);
            }
        }
    }

    private string _typeNameError = string.Empty;
    public string TypeNameError
    {
        get => _typeNameError;
        private set
        {
            if (SetProperty(ref _typeNameError, value))
            {
                SetError(nameof(TypeName), value);
            }
        }
    }

    private string _orderNumError = string.Empty;
    public string OrderNumError
    {
        get => _orderNumError;
        private set
        {
            if (SetProperty(ref _orderNumError, value))
            {
                SetError(nameof(OrderNum), value);
            }
        }
    }

    private string _typeStatusError = string.Empty;
    public string TypeStatusError
    {
        get => _typeStatusError;
        private set
        {
            if (SetProperty(ref _typeStatusError, value))
            {
                SetError(nameof(TypeStatus), value);
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

    private string _sqlScriptError = string.Empty;
    public string SqlScriptError
    {
        get => _sqlScriptError;
        private set
        {
            if (SetProperty(ref _sqlScriptError, value))
            {
                SetError(nameof(SqlScript), value);
            }
        }
    }

    // 子表启用状态（DataSource=0时启用，DataSource=1时禁用）
    [ObservableProperty]
    private bool _isSubTableEnabled = true;

    // 子表数据
    public ObservableCollection<DictionaryDataDto> DictionaryDataList { get; } = new();

    [ObservableProperty]
    private DictionaryDataDto? _selectedDictionaryData;

    [ObservableProperty]
    private DictionaryDataDto? _editingDictionaryData;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public DictionaryFormViewModel(
        IDictionaryTypeService dictionaryTypeService,
        IDictionaryDataService dictionaryDataService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _dictionaryTypeService = dictionaryTypeService ?? throw new ArgumentNullException(nameof(dictionaryTypeService));
        _dictionaryDataService = dictionaryDataService ?? throw new ArgumentNullException(nameof(dictionaryDataService));
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
        // 拼接标题：新增 + 字典类型
        var createText = _localizationManager.GetString("common.button.create");
        var entityText = _localizationManager.GetString("routine.dictionary.type");
        Title = $"{createText}{entityText}";
        Id = 0;
        TypeCode = string.Empty;
        TypeName = string.Empty;
        DataSource = 0; // 默认系统
        SqlScript = null;
        OrderNum = 0;
        IsBuiltin = 1; // 默认否（1=否）
        TypeStatus = 0; // 默认启用
        Remarks = null;

        // 根据 DataSource 设置子表启用状态
        IsSubTableEnabled = DataSource == 0;

        // 清空子表数据
        DictionaryDataList.Clear();
        SelectedDictionaryData = null;
        EditingDictionaryData = null;
    }

    /// <summary>
    /// 初始化编辑模式
    /// </summary>
    public void ForUpdate(DictionaryTypeDto dto)
    {
        ClearAllErrors();

        IsCreate = false;
        // 拼接标题：更新 + 字典类型
        var updateText = _localizationManager.GetString("common.button.update");
        var entityText = _localizationManager.GetString("routine.dictionary.type");
        Title = $"{updateText}{entityText}";
        Id = dto.Id;
        TypeCode = dto.TypeCode;
        TypeName = dto.TypeName;
        DataSource = dto.DataSource;
        SqlScript = dto.SqlScript;
        OrderNum = dto.OrderNum;
        IsBuiltin = dto.IsBuiltin;
        TypeStatus = dto.TypeStatus;
        Remarks = dto.Remarks;

        // 清空子表数据
        DictionaryDataList.Clear();
        SelectedDictionaryData = null;
        EditingDictionaryData = null;

        // 根据 DataSource 设置子表启用状态
        IsSubTableEnabled = DataSource == 0;

        // 验证类型代码格式（编辑模式下也需要验证，但 TypeCode 是只读的，所以这里主要是为了显示错误）
        ValidateTypeCode();

        // 异步加载子表数据（仅当子表启用时）
        if (IsSubTableEnabled)
        {
            _ = LoadDictionaryDataAsync();
        }
    }

    /// <summary>
    /// 加载字典数据
    /// </summary>
    private async Task LoadDictionaryDataAsync()
    {
        if (string.IsNullOrWhiteSpace(TypeCode))
        {
            return;
        }

        try
        {
            var query = new DictionaryDataQueryDto
            {
                TypeCode = TypeCode,
                PageIndex = 1,
                PageSize = 1000 // 加载所有数据
            };

            var result = await _dictionaryDataService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    DictionaryDataList.Clear();
                    foreach (var data in result.Data.Items)
                    {
                        DictionaryDataList.Add(data);
                    }
                });
                _operLog?.Information("[DictionaryForm] 加载字典数据成功，类型代码={TypeCode}, 数量={Count}", TypeCode, result.Data.Items.Count);
            }
            else
            {
                _operLog?.Warning("[DictionaryForm] 加载字典数据失败，类型代码={TypeCode}, 错误={Error}", TypeCode, result.Message ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[DictionaryForm] 加载字典数据失败，类型代码={TypeCode}", TypeCode);
        }
    }

    /// <summary>
    /// 清除所有错误消息（同时清除 WPF 原生验证错误）
    /// </summary>
    private void ClearAllErrors()
    {
        TypeCodeError = string.Empty;
        TypeNameError = string.Empty;
        OrderNumError = string.Empty;
        TypeStatusError = string.Empty;
        RemarksError = string.Empty;
        SqlScriptError = string.Empty;
        Error = string.Empty;
        ClearAllValidationErrors();
    }

    /// <summary>
    /// 验证所有必填字段（提交验证 - onSubmit，参照 antd/vue/element 验证方式）
    /// 提交验证规则：先验证必填，再验证长度，最后验证格式
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        // 验证类型代码（提交验证：先验证必填，再验证长度，最后验证格式）
        var typeCodeTrimmed = TypeCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(typeCodeTrimmed))
        {
            // 第一步：验证必填（先验证空值）
            TypeCodeError = _localizationManager.GetString("routine.dictionary.validation.typecoderequired");
            isValid = false;
        }
        else if (typeCodeTrimmed.Length > 50)
        {
            // 第二步：验证长度（必填通过后）
            TypeCodeError = _localizationManager.GetString("routine.dictionary.validation.typecodemaxlength");
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(typeCodeTrimmed, @"^[a-z][a-z0-9]*(_[a-z0-9]+)*$"))
        {
            // 第三步：验证格式（长度通过后，最后验证正则）
            TypeCodeError = _localizationManager.GetString("routine.dictionary.validation.typecodeformat");
            isValid = false;
        }

        // 验证类型名称（提交验证：先验证必填，再验证长度）
        var typeNameTrimmed = TypeName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(typeNameTrimmed))
        {
            // 第一步：验证必填（先验证空值）
            TypeNameError = _localizationManager.GetString("routine.dictionary.validation.typenamerequired");
            isValid = false;
        }
        else if (typeNameTrimmed.Length > 100)
        {
            // 第二步：验证长度（必填通过后）
            TypeNameError = _localizationManager.GetString("routine.dictionary.validation.typenamemaxlength");
            isValid = false;
        }

        // 验证状态（0=启用，1=禁用）
        if (TypeStatus < 0 || TypeStatus > 1)
        {
            TypeStatusError = _localizationManager.GetString("routine.dictionary.validation.typestatusinvalid");
            isValid = false;
        }

        // 验证 SqlScript（当 DataSource=1 时必填）
        if (DataSource == 1)
        {
            if (string.IsNullOrWhiteSpace(SqlScript))
            {
                SqlScriptError = _localizationManager.GetString("routine.dictionary.validation.sqlscriptrequired");
                isValid = false;
            }
            else
            {
                SqlScriptError = string.Empty;
            }
        }
        else
        {
            SqlScriptError = string.Empty;
        }

        // 如果有正在编辑的子表项，提示先保存或取消（仅当子表启用时检查）
        if (IsSubTableEnabled && EditingDictionaryData != null)
        {
            Error = _localizationManager.GetString("routine.dictionary.pleasesaveorcanceldata");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 保存字典类型和字典数据
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

            string typeCode;

            if (IsCreate)
            {
                var dto = new DictionaryTypeCreateDto
                {
                    TypeCode = TypeCode.Trim(),
                    TypeName = TypeName.Trim(),
                    DataSource = DataSource,
                    SqlScript = string.IsNullOrWhiteSpace(SqlScript) ? null! : SqlScript.Trim(),
                    OrderNum = OrderNum,
                    Remarks = string.IsNullOrWhiteSpace(Remarks) ? null! : Remarks.Trim()
                };

                var result = await _dictionaryTypeService.CreateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("routine.dictionary.type");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                typeCode = TypeCode.Trim();
                _operLog?.Information("[DictionaryForm] 创建字典类型成功，类型代码={TypeCode}", typeCode);
                
                // 创建成功，显示成功消息
                var createEntityName = _localizationManager.GetString("routine.dictionary.type");
                var successMessage = string.Format(_localizationManager.GetString("common.success.create"), createEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
            }
            else
            {
                var dto = new DictionaryTypeUpdateDto
                {
                    Id = Id,
                    TypeCode = TypeCode.Trim(),
                    TypeName = TypeName.Trim(),
                    DataSource = DataSource,
                    SqlScript = string.IsNullOrWhiteSpace(SqlScript) ? null! : SqlScript.Trim(),
                    OrderNum = OrderNum,
                    TypeStatus = TypeStatus,
                    Remarks = string.IsNullOrWhiteSpace(Remarks) ? null! : Remarks.Trim()
                };

                var result = await _dictionaryTypeService.UpdateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("routine.dictionary.type");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update"), entityName);
                    Error = errorMessage;
                    
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                typeCode = TypeCode.Trim();
                _operLog?.Information("[DictionaryForm] 更新字典类型成功，Id={Id}, 类型代码={TypeCode}", Id, typeCode);
                
                // 更新成功，显示成功消息
                var updateEntityName = _localizationManager.GetString("routine.dictionary.type");
                var successMessage = string.Format(_localizationManager.GetString("common.success.update"), updateEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
            }

            // 保存子表数据（仅当子表启用时）
            if (IsSubTableEnabled)
            {
                await SaveDictionaryDataAsync(typeCode);
                
                // 检查子表保存是否失败（如果有错误，已在 SaveDictionaryDataAsync 中显示）
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    return;
                }
            }

            // **注意：成功消息框已在创建/更新主表成功后显示，这里不再重复显示**
            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            _operLog?.Error(ex, "[DictionaryForm] 保存字典失败");
            TaktMessageManager.ShowError(errorMessage);
        }
    }

    /// <summary>
    /// 保存字典数据
    /// </summary>
    private async Task SaveDictionaryDataAsync(string typeCode)
    {
        // 获取需要新增和更新的数据项
        var newDataList = DictionaryDataList.Where(d => d.Id == 0).ToList();
        var updatedDataList = DictionaryDataList.Where(d => d.Id > 0).ToList();

        // 获取数据库中已存在的数据ID
        var existingData = await _dictionaryDataService.GetListAsync(new DictionaryDataQueryDto
        {
            TypeCode = typeCode,
            PageIndex = 1,
            PageSize = 1000
        });

        var existingIds = existingData.Success && existingData.Data != null
            ? existingData.Data.Items.Select(d => d.Id).ToList()
            : new System.Collections.Generic.List<long>();

        // 删除已从列表中移除的数据（如果存在）
        var deletedIds = existingIds.Except(updatedDataList.Select(d => d.Id)).ToList();
        foreach (var deletedId in deletedIds)
        {
            await _dictionaryDataService.DeleteAsync(deletedId);
        }

        // 新增数据
        foreach (var data in newDataList)
        {
            if (string.IsNullOrWhiteSpace(data.DataLabel) ||
                string.IsNullOrWhiteSpace(data.I18nKey))
            {
                continue; // 跳过未填写的项
            }

            var createDto = new DictionaryDataCreateDto
            {
                TypeCode = typeCode,
                DataLabel = data.DataLabel.Trim(),
                I18nKey = data.I18nKey.Trim(),
                DataValue = string.IsNullOrWhiteSpace(data.DataValue) ? null! : data.DataValue.Trim(),
                ExtLabel = string.IsNullOrWhiteSpace(data.ExtLabel) ? null! : data.ExtLabel.Trim(),
                ExtValue = string.IsNullOrWhiteSpace(data.ExtValue) ? null! : data.ExtValue.Trim(),
                CssClass = string.IsNullOrWhiteSpace(data.CssClass) ? null! : data.CssClass.Trim(),
                ListClass = string.IsNullOrWhiteSpace(data.ListClass) ? null! : data.ListClass.Trim(),
                OrderNum = data.OrderNum,
                Remarks = string.IsNullOrWhiteSpace(data.Remarks) ? null! : data.Remarks.Trim()
            };

            var result = await _dictionaryDataService.CreateAsync(createDto);
            if (result.Success && result.Data > 0)
            {
                data.Id = result.Data;
            }
        }

        // 更新数据
        foreach (var data in updatedDataList)
        {
            if (string.IsNullOrWhiteSpace(data.DataLabel) ||
                string.IsNullOrWhiteSpace(data.I18nKey))
            {
                continue; // 跳过未填写的项
            }

            var updateDto = new DictionaryDataUpdateDto
            {
                Id = data.Id,
                TypeCode = typeCode,
                DataLabel = data.DataLabel.Trim(),
                I18nKey = data.I18nKey.Trim(),
                DataValue = string.IsNullOrWhiteSpace(data.DataValue) ? null! : data.DataValue.Trim(),
                ExtLabel = string.IsNullOrWhiteSpace(data.ExtLabel) ? null! : data.ExtLabel.Trim(),
                ExtValue = string.IsNullOrWhiteSpace(data.ExtValue) ? null! : data.ExtValue.Trim(),
                CssClass = string.IsNullOrWhiteSpace(data.CssClass) ? null! : data.CssClass.Trim(),
                ListClass = string.IsNullOrWhiteSpace(data.ListClass) ? null! : data.ListClass.Trim(),
                OrderNum = data.OrderNum,
                Remarks = string.IsNullOrWhiteSpace(data.Remarks) ? null! : data.Remarks.Trim()
            };

            await _dictionaryDataService.UpdateAsync(updateDto);
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // 关闭窗口
        var window = System.Windows.Application.Current?.Windows.OfType<System.Windows.Window>()
            .FirstOrDefault(w => w.DataContext == this);
        window?.Close();
    }

    #region 子表命令

    partial void OnEditingDictionaryDataChanged(DictionaryDataDto? value)
    {
        // 通知所有相关命令重新评估 CanExecute
        CreateDataInlineCommand.NotifyCanExecuteChanged();
        UpdateDataInlineCommand.NotifyCanExecuteChanged();
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();
        DeleteDataCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    [RelayCommand(CanExecute = nameof(CanCreateDataInline))]
    private void CreateDataInline()
    {
        // 创建新的字典数据对象
        var newData = new DictionaryDataDto
        {
            TypeCode = TypeCode,
            DataLabel = string.Empty,
            I18nKey = string.Empty,
            DataValue = null!,
            ExtLabel = null!,
            ExtValue = null!,
            CssClass = null!,
            ListClass = null!,
            OrderNum = DictionaryDataList.Count > 0 ? DictionaryDataList.Max(d => d.OrderNum) + 1 : 1,
            Remarks = null!,
            CreatedTime = DateTime.Now,
            UpdatedTime = DateTime.Now
        };

        // 添加到列表（在 UI 线程上）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            DictionaryDataList.Add(newData);
        });

        // 设置正在编辑的项，让 TaktInlineEditDataGrid 自动进入编辑状态
        EditingDictionaryData = newData;
        SelectedDictionaryData = newData;

        // 通知命令重新评估 CanExecute
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();
        UpdateDataInlineCommand.NotifyCanExecuteChanged();

        // 延迟触发编辑状态，确保 UI 已更新
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (UpdateDataInlineCommand.CanExecute(newData))
            {
                UpdateDataInlineCommand.Execute(newData);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        _operLog?.Information("[DictionaryForm] 新增字典数据行");
    }

    private bool CanCreateDataInline()
    {
        // 子表禁用时，不能新增
        if (!IsSubTableEnabled)
        {
            return false;
        }
        // TypeCode 为空时禁用新增
        if (string.IsNullOrWhiteSpace(TypeCode))
        {
            return false;
        }
        // TypeCode 必须符合正则格式才能新增
        var typeCodePattern = @"^[a-z][a-z0-9]*(_[a-z0-9]+)*$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(TypeCode, typeCodePattern))
        {
            return false;
        }
        // TypeCode 长度不能超过50
        if (TypeCode.Length > 50)
        {
            return false;
        }
        return EditingDictionaryData == null;
    }

    /// <summary>
    /// TypeCode 改变时的处理（实时验证）
    /// </summary>
    partial void OnTypeCodeChanged(string value)
    {
        // 实时验证：输入时立即验证格式，不验证空值（空值验证在提交时进行）
        ValidateTypeCode();

        // 当 TypeCode 改变时，通知子表新增命令重新评估 CanExecute（在验证后通知，确保按钮状态正确）
        CreateDataInlineCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();

        // 自动更新备注
        UpdateRemarksAuto();
    }

    /// <summary>
    /// 验证类型代码（实时验证）
    /// 验证规则：
    /// 1. 输入时立即验证格式（长度和正则），不验证空值
    /// 2. 如果字段为空，清除错误，等待提交时验证必填
    /// </summary>
    private void ValidateTypeCode()
    {
        // 如果字段为空，清除错误，等待提交时验证必填
        if (string.IsNullOrWhiteSpace(TypeCode))
        {
            TypeCodeError = string.Empty;
            return;
        }

        // 输入时立即验证长度和正则格式（不验证空值）
        // 先验证长度
        if (TypeCode.Length > 50)
        {
            TypeCodeError = _localizationManager.GetString("routine.dictionary.validation.typecodemaxlength");
        }
        // 长度验证通过后，验证类型代码格式：必须是 xxx_xxx_xxx 格式，只包含小写字母和数字，不能以数字开头
        // 正则表达式说明：
        // ^[a-z] - 必须以小写字母开头（不能以数字开头）
        // [a-z0-9]* - 可以包含小写字母和数字
        // (_[a-z0-9]+)* - 可以有多个下划线分隔的部分，每个部分至少包含一个字符
        else if (!System.Text.RegularExpressions.Regex.IsMatch(TypeCode, @"^[a-z][a-z0-9]*(_[a-z0-9]+)*$"))
        {
            // 输入时立即显示格式错误
            TypeCodeError = _localizationManager.GetString("routine.dictionary.validation.typecodeformat");
        }
        else
        {
            // 格式验证通过，清除所有错误
            TypeCodeError = string.Empty;
        }
    }

    /// <summary>
    /// TypeName 改变时的处理（实时验证）
    /// </summary>
    partial void OnTypeNameChanged(string value)
    {
        // 实时验证：输入时立即验证，不验证空值（空值验证在提交时进行）
        ValidateTypeName();

        // 自动更新备注
        UpdateRemarksAuto();
    }

    /// <summary>
    /// 验证类型名称（实时验证）
    /// 验证规则：
    /// 1. 输入时立即验证长度，不验证空值
    /// 2. 如果字段为空，清除错误，等待提交时验证必填
    /// </summary>
    private void ValidateTypeName()
    {
        // 如果字段为空，清除错误，等待提交时验证必填
        if (string.IsNullOrWhiteSpace(TypeName))
        {
            TypeNameError = string.Empty;
            return;
        }

        // 输入时立即验证长度（不验证空值）
        if (TypeName.Length > 100)
        {
            TypeNameError = _localizationManager.GetString("routine.dictionary.validation.typenamemaxlength");
        }
        else
        {
            // 长度验证通过，清除所有错误
            TypeNameError = string.Empty;
        }
    }

    /// <summary>
    /// 自动更新备注字段
    /// </summary>
    private void UpdateRemarksAuto()
    {
        // 仅在创建模式时自动填充备注
        if (!IsCreate)
        {
            return;
        }

        // 当类型代码和类型名称都不为空时，自动生成备注
        var typeCode = TypeCode?.Trim() ?? string.Empty;
        var typeName = TypeName?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(typeCode) && !string.IsNullOrWhiteSpace(typeName))
        {
            var newRemarks = $"{typeName}字典（{typeCode}）";
            // 自动更新备注（创建模式下总是自动填充）
            Remarks = newRemarks;
        }
    }

    /// <summary>
    /// DataSource 改变时的处理
    /// </summary>
    partial void OnDataSourceChanged(int value)
    {
        // 当 DataSource=0（系统）时，子表启用；DataSource=1（SQL脚本）时，子表禁用
        IsSubTableEnabled = value == 0;

        // 清除 SqlScript 错误
        SqlScriptError = string.Empty;

        // 通知子表命令重新评估 CanExecute
        CreateDataInlineCommand.NotifyCanExecuteChanged();
        UpdateDataInlineCommand.NotifyCanExecuteChanged();
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();
        DeleteDataCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    [RelayCommand(CanExecute = nameof(CanUpdateDataInline))]
    private void UpdateDataInline(DictionaryDataDto? data)
    {
        if (data == null)
        {
            data = SelectedDictionaryData;
        }

        if (data == null)
        {
            return;
        }

        EditingDictionaryData = data;

        // 通知命令重新评估 CanExecute
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[DictionaryForm] 进入编辑字典数据状态，数据Id={Id}", data.Id);
    }

    private bool CanUpdateDataInline(DictionaryDataDto? data)
    {
        // 子表禁用时，不能编辑
        if (!IsSubTableEnabled)
        {
            return false;
        }
        if (data == null)
        {
            return SelectedDictionaryData is not null && EditingDictionaryData == null;
        }
        return data is not null && EditingDictionaryData == null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveDataInline))]
    private void SaveDataInline(DictionaryDataDto? data)
    {
        if (data == null)
        {
            data = EditingDictionaryData;
        }

        if (data == null || EditingDictionaryData != data)
        {
            return;
        }

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(data.DataLabel) ||
            string.IsNullOrWhiteSpace(data.I18nKey))
        {
            Error = _localizationManager.GetString("routine.dictionary.datafieldsrequired");
            return;
        }

        // 清除编辑状态（实际保存会在保存主表时进行）
        EditingDictionaryData = null;

        // 通知命令重新评估 CanExecute
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[DictionaryForm] 字典数据保存成功（待主表保存时提交）");
    }

    private bool CanSaveDataInline(DictionaryDataDto? data)
    {
        if (data == null)
        {
            return EditingDictionaryData is not null;
        }
        return EditingDictionaryData != null && EditingDictionaryData == data;
    }

    [RelayCommand(CanExecute = nameof(CanCancelDataInline))]
    private void CancelDataInline()
    {
        if (EditingDictionaryData != null)
        {
            var data = EditingDictionaryData;

            // 如果是新添加的项（Id=0），从列表中移除
            if (data.Id == 0)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    DictionaryDataList.Remove(data);
                });
            }

            EditingDictionaryData = null;
            SelectedDictionaryData = null;

            // 通知命令重新评估 CanExecute
            SaveDataInlineCommand.NotifyCanExecuteChanged();
            CancelDataInlineCommand.NotifyCanExecuteChanged();

            _operLog?.Information("[DictionaryForm] 取消编辑字典数据");
        }
    }

    private bool CanCancelDataInline()
    {
        return EditingDictionaryData != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteData))]
    private void DeleteData(DictionaryDataDto? data)
    {
        if (data == null)
        {
            data = SelectedDictionaryData;
        }

        if (data == null)
        {
            return;
        }

        // 从列表中移除（如果是新添加的项，直接移除；如果是已存在的项，标记为删除，在保存时处理）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            DictionaryDataList.Remove(data);
        });

        SelectedDictionaryData = null;

        _operLog?.Information("[DictionaryForm] 删除字典数据，数据Id={Id}", data.Id);
    }

    private bool CanDeleteData(DictionaryDataDto? data)
    {
        // 子表禁用时，不能删除
        if (!IsSubTableEnabled)
        {
            return false;
        }
        return EditingDictionaryData == null; // 编辑状态下不能删除
    }

    #endregion
}

