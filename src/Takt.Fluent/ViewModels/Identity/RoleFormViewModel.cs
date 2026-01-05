//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : RoleFormViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 角色表单视图模型（新建/更新）
//===================================================================

using System.Collections;
using System.ComponentModel;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Enums;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 角色表单视图模型
/// </summary>
/// <remarks>
/// 使用 WPF 原生验证系统 INotifyDataErrorInfo
/// </remarks>
public partial class RoleFormViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly IRoleService _roleService;
    private readonly ILocalizationManager _localizationManager;

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private bool _isCreate = true;
    [ObservableProperty] private string _roleName = "";
    [ObservableProperty] private string _roleCode = "";
    [ObservableProperty] private string? _description;
    [ObservableProperty] private int _dataScope = (int)DataScopeEnum.Self;
    [ObservableProperty] private int _orderNum = 0;
    [ObservableProperty] private int _roleStatus = (int)StatusEnum.Normal;
    [ObservableProperty] private string? _remarks;
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _error = string.Empty;

    // Hint 提示属性
    public string RoleNameHint => _localizationManager.GetString("identity.role.validation.rolenamehint");

    public string RoleCodeHint => _localizationManager.GetString("identity.role.validation.rolecodehint");

    public string DescriptionHint => _localizationManager.GetString("identity.role.validation.descriptionhint");

    public string OrderNumHint => _localizationManager.GetString("identity.role.validation.ordernumhint");

    public string RemarksHint => _localizationManager.GetString("identity.role.validation.remarkshint");

    // 错误消息属性（保留用于向后兼容，同时更新 INotifyDataErrorInfo）
    private string _roleNameError = string.Empty;
    public string RoleNameError
    {
        get => _roleNameError;
        private set
        {
            if (SetProperty(ref _roleNameError, value))
            {
                SetError(nameof(RoleName), value);
            }
        }
    }

    private string _roleCodeError = string.Empty;
    public string RoleCodeError
    {
        get => _roleCodeError;
        private set
        {
            if (SetProperty(ref _roleCodeError, value))
            {
                SetError(nameof(RoleCode), value);
            }
        }
    }

    private string _descriptionError = string.Empty;
    public string DescriptionError
    {
        get => _descriptionError;
        private set
        {
            if (SetProperty(ref _descriptionError, value))
            {
                SetError(nameof(Description), value);
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

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public RoleFormViewModel(IRoleService roleService, ILocalizationManager localizationManager)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }

    public void ForCreate()
    {
        ClearAllErrors();

        IsCreate = true;
        // 拼接标题：新增 + 角色 = "新增角色"
        var createText = _localizationManager.GetString("common.button.create");
        var entityText = _localizationManager.GetString("identity.role.entity");
        Title = $"{createText}{entityText}";
        RoleName = string.Empty;
        RoleCode = string.Empty;
        Description = null;
        DataScope = (int)DataScopeEnum.Self;
        OrderNum = 0;
        RoleStatus = (int)StatusEnum.Normal;
        Remarks = null;
    }

    public void ForUpdate(RoleDto dto)
    {
        ClearAllErrors();

        IsCreate = false;
        // 拼接标题：更新 + 角色 = "更新角色"
        var updateText = _localizationManager.GetString("common.button.update");
        var entityText = _localizationManager.GetString("identity.role.entity");
        Title = $"{updateText}{entityText}";
        Id = dto.Id;
        RoleName = dto.RoleName ?? string.Empty;
        RoleCode = dto.RoleCode ?? string.Empty;
        Description = dto.Description;
        DataScope = (int)dto.DataScope;
        OrderNum = dto.OrderNum;
        RoleStatus = (int)dto.RoleStatus;
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        RoleNameError = string.Empty;
        RoleCodeError = string.Empty;
        DescriptionError = string.Empty;
        OrderNumError = string.Empty;
        RemarksError = string.Empty;
        Error = string.Empty;

        // 清除所有 INotifyDataErrorInfo 错误
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    // 属性变更时进行实时验证
    partial void OnRoleNameChanged(string value)
    {
        ValidateRoleName();
    }

    partial void OnRoleCodeChanged(string value)
    {
        ValidateRoleCode();
    }

    /// <summary>
    /// 验证角色名称（实时验证）
    /// </summary>
    private void ValidateRoleName()
    {
        if (string.IsNullOrWhiteSpace(RoleName))
        {
            RoleNameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (RoleName.Length > 128)
        {
            RoleNameError = _localizationManager.GetString("identity.role.validation.rolenamemaxlength");
        }
        else
        {
            RoleNameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证角色编码（实时验证）
    /// </summary>
    private void ValidateRoleCode()
    {
        if (string.IsNullOrWhiteSpace(RoleCode))
        {
            RoleCodeError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (RoleCode.Length > 10)
        {
            RoleCodeError = _localizationManager.GetString("identity.role.validation.rolecodemaxlength");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(RoleCode, @"^[a-z0-9_]+$"))
        {
            RoleCodeError = _localizationManager.GetString("identity.role.validation.rolecodeinvalid");
        }
        else
        {
            RoleCodeError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        // 验证角色名称（必填）
        if (string.IsNullOrWhiteSpace(RoleName))
        {
            RoleNameError = _localizationManager.GetString("identity.role.validation.rolenamerequired");
            isValid = false;
        }
        else if (RoleName.Length > 128)
        {
            RoleNameError = _localizationManager.GetString("identity.role.validation.rolenamemaxlength");
            isValid = false;
        }

        // 验证角色编码（必填）
        if (string.IsNullOrWhiteSpace(RoleCode))
        {
            RoleCodeError = _localizationManager.GetString("identity.role.validation.rolecoderequired");
            isValid = false;
        }
        else if (RoleCode.Length > 10)
        {
            RoleCodeError = _localizationManager.GetString("identity.role.validation.rolecodemaxlength");
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(RoleCode, @"^[a-z0-9_]+$"))
        {
            RoleCodeError = _localizationManager.GetString("identity.role.validation.rolecodeinvalid");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 保存角色（新建或更新）
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
                var dto = new RoleCreateDto
                {
                    RoleName = RoleName,
                    RoleCode = RoleCode,
                    Description = Description!,
                    DataScope = (DataScopeEnum)DataScope,
                    OrderNum = OrderNum,
                    RoleStatus = (StatusEnum)RoleStatus,
                    Remarks = Remarks!
                };

                var result = await _roleService.CreateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("identity.role.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 创建成功，显示成功消息
                var createEntityName = _localizationManager.GetString("identity.role.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.create"), createEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
                SaveSuccessCallback?.Invoke();
            }
            else
            {
                var dto = new RoleUpdateDto
                {
                    Id = Id,
                    RoleName = RoleName,
                    RoleCode = RoleCode,
                    Description = Description!,
                    DataScope = (DataScopeEnum)DataScope,
                    OrderNum = OrderNum,
                    RoleStatus = (StatusEnum)RoleStatus,
                    Remarks = Remarks!
                };

                var result = await _roleService.UpdateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("identity.role.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 更新成功，显示成功消息
                var updateEntityName = _localizationManager.GetString("identity.role.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.update"), updateEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
                SaveSuccessCallback?.Invoke();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // 关闭窗口由窗口本身处理
        if (System.Windows.Application.Current?.MainWindow is System.Windows.Window mainWindow)
        {
            var window = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}

