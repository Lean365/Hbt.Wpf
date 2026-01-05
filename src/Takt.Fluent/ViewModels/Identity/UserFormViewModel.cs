//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : UserFormViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-30
// 版本号 : 0.0.1
// 描述    : 用户表单视图模型（新建/更新）
//===================================================================

using System.Collections;
using System.ComponentModel;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Application.Services.Routine;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 用户表单视图模型（新建/更新）
/// 使用 WPF 原生验证系统 INotifyDataErrorInfo
/// </summary>
public partial class UserFormViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly IUserService _userService;
    private readonly IDictionaryTypeService _dictionaryTypeService;
    private readonly ILocalizationManager _localizationManager;
    private static readonly OperLogManager? _operLog = App.Services?.GetService<OperLogManager>();

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private bool _isCreate = true;
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string? _realName;
    [ObservableProperty] private string _nickname = "";
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private int _userType;
    [ObservableProperty] private int _userGender;
    [ObservableProperty] private int _userStatus;
    [ObservableProperty] private string? _avatar;
    [ObservableProperty] private string? _remarks;
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _error = string.Empty;

    /// <summary>
    /// 用户类型选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> UserTypeOptions { get; } = new();

    /// <summary>
    /// 性别选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> UserGenderOptions { get; } = new();

    /// <summary>
    /// 状态选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> UserStatusOptions { get; } = new();

    // Hint 提示属性
    public string UsernameHint => _localizationManager.GetString("identity.user.validation.usernameinvalid");

    public string EmailHint => _localizationManager.GetString("identity.user.validation.emailinvalid");

    public string RealNameHint => _localizationManager.GetString("identity.user.validation.realnamehint");

    public string NicknameHint => _localizationManager.GetString("identity.user.validation.nicknameinvalid");

    public string PhoneHint => _localizationManager.GetString("identity.user.validation.phoneinvalid");

    public string PasswordHint => _localizationManager.GetString("identity.user.validation.passwordminlength");

    public string PasswordConfirmHint => _localizationManager.GetString("identity.user.validation.passwordconfirminthint");

    public string RemarksHint => _localizationManager.GetString("identity.user.validation.remarkshint");

    // 错误消息属性（保留用于向后兼容，同时更新 INotifyDataErrorInfo）
    private string _usernameError = string.Empty;
    public string UsernameError
    {
        get => _usernameError;
        private set
        {
            if (SetProperty(ref _usernameError, value))
            {
                SetError(nameof(Username), value);
            }
        }
    }

    private string _realNameError = string.Empty;
    public string RealNameError
    {
        get => _realNameError;
        private set
        {
            if (SetProperty(ref _realNameError, value))
            {
                SetError(nameof(RealName), value);
            }
        }
    }

    private string _nicknameError = string.Empty;
    public string NicknameError
    {
        get => _nicknameError;
        private set
        {
            if (SetProperty(ref _nicknameError, value))
            {
                SetError(nameof(Nickname), value);
            }
        }
    }

    private string _emailError = string.Empty;
    public string EmailError
    {
        get => _emailError;
        private set
        {
            if (SetProperty(ref _emailError, value))
            {
                SetError(nameof(Email), value);
            }
        }
    }

    private string _phoneError = string.Empty;
    public string PhoneError
    {
        get => _phoneError;
        private set
        {
            if (SetProperty(ref _phoneError, value))
            {
                SetError(nameof(Phone), value);
            }
        }
    }

    private string _avatarError = string.Empty;
    public string AvatarError
    {
        get => _avatarError;
        private set
        {
            if (SetProperty(ref _avatarError, value))
            {
                SetError(nameof(Avatar), value);
            }
        }
    }

    private string _passwordError = string.Empty;
    public string PasswordError
    {
        get => _passwordError;
        private set
        {
            if (SetProperty(ref _passwordError, value))
            {
                SetError(nameof(PasswordError), value); // 注意：密码字段没有对应的属性
            }
        }
    }

    private string _passwordConfirmError = string.Empty;
    public string PasswordConfirmError
    {
        get => _passwordConfirmError;
        private set
        {
            if (SetProperty(ref _passwordConfirmError, value))
            {
                SetError(nameof(PasswordConfirmError), value); // 注意：确认密码字段没有对应的属性
            }
        }
    }

    private string _userTypeError = string.Empty;
    public string UserTypeError
    {
        get => _userTypeError;
        private set
        {
            if (SetProperty(ref _userTypeError, value))
            {
                SetError(nameof(UserType), value);
            }
        }
    }

    private string _userGenderError = string.Empty;
    public string UserGenderError
    {
        get => _userGenderError;
        private set
        {
            if (SetProperty(ref _userGenderError, value))
            {
                SetError(nameof(UserGender), value);
            }
        }
    }

    private string _userStatusError = string.Empty;
    public string UserStatusError
    {
        get => _userStatusError;
        private set
        {
            if (SetProperty(ref _userStatusError, value))
            {
                SetError(nameof(UserStatus), value);
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

    private Func<(string pwd, string confirm)>? _passwordAccessor;
    public void AttachPasswordAccess(Func<(string pwd, string confirm)> accessor) => _passwordAccessor = accessor;

    // 文本字段值访问器（类似密码的方式，直接从控件读取值）
    private Func<(string username, string realName, string nickname, string email, string phone, string avatar, string remarks)>? _textFieldsAccessor;
    public void AttachTextFieldsAccess(Func<(string username, string realName, string nickname, string email, string phone, string avatar, string remarks)> accessor) => _textFieldsAccessor = accessor;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public UserFormViewModel(IUserService userService, IDictionaryTypeService dictionaryTypeService, ILocalizationManager localizationManager)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _dictionaryTypeService = dictionaryTypeService ?? throw new ArgumentNullException(nameof(dictionaryTypeService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        // 初始化字典选项
        _ = InitializeDictionaryOptionsAsync();
    }

    /// <summary>
    /// 初始化字典选项列表
    /// </summary>
    private async Task InitializeDictionaryOptionsAsync()
    {
        await InitializeUserTypeOptionsAsync();
        await InitializeUserGenderOptionsAsync();
        await InitializeUserStatusOptionsAsync();
    }

    /// <summary>
    /// 初始化用户类型选项列表
    /// </summary>
    private async Task InitializeUserTypeOptionsAsync()
    {
        try
        {
            var result = await _dictionaryTypeService.GetOptionsAsync("sys_user_type");
            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    UserTypeOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        // 将字典值转换为枚举值存储在 ExtValue 中
                        var enumValue = ConvertUserTypeToEnum(option.DataValue);
                        UserTypeOptions.Add(new SelectOptionModel
                        {
                            DataValue = enumValue.ToString(),
                            DataLabel = option.DataLabel,
                            ExtValue = option.DataValue, // 保存原始字典值
                            OrderNum = option.OrderNum
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserForm] 加载用户类型选项失败");
        }
    }

    /// <summary>
    /// 初始化性别选项列表
    /// </summary>
    private async Task InitializeUserGenderOptionsAsync()
    {
        try
        {
            var result = await _dictionaryTypeService.GetOptionsAsync("sys_common_gender");
            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    UserGenderOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        // 将字典值转换为枚举值存储在 ExtValue 中
                        var enumValue = ConvertUserGenderToEnum(option.DataValue);
                        UserGenderOptions.Add(new SelectOptionModel
                        {
                            DataValue = enumValue.ToString(),
                            DataLabel = option.DataLabel,
                            ExtValue = option.DataValue, // 保存原始字典值
                            OrderNum = option.OrderNum
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserForm] 加载性别选项失败");
        }
    }

    /// <summary>
    /// 初始化状态选项列表
    /// </summary>
    private async Task InitializeUserStatusOptionsAsync()
    {
        try
        {
            var result = await _dictionaryTypeService.GetOptionsAsync("sys_common_status");
            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    UserStatusOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        // 将字典值转换为枚举值存储在 ExtValue 中
                        var enumValue = ConvertUserStatusToEnum(option.DataValue);
                        UserStatusOptions.Add(new SelectOptionModel
                        {
                            DataValue = enumValue.ToString(),
                            DataLabel = option.DataLabel,
                            ExtValue = option.DataValue, // 保存原始字典值
                            OrderNum = option.OrderNum
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserForm] 加载状态选项失败");
        }
    }

    /// <summary>
    /// 将字典值转换为用户类型枚举值
    /// </summary>
    private static int ConvertUserTypeToEnum(string dictValue)
    {
        return dictValue switch
        {
            "Takt365" => 0, // 系统用户
            "normal" => 1,  // 普通用户
            _ => 1 // 默认普通用户
        };
    }

    /// <summary>
    /// 将字典值转换为性别枚举值
    /// </summary>
    private static int ConvertUserGenderToEnum(string dictValue)
    {
        return dictValue switch
        {
            "unknown" => 0, // 未知
            "male" => 1,    // 男
            "female" => 2,  // 女
            _ => 0 // 默认未知
        };
    }

    /// <summary>
    /// 将字典值转换为状态枚举值
    /// </summary>
    private static int ConvertUserStatusToEnum(string dictValue)
    {
        return dictValue switch
        {
            "normal" => 0,   // 正常
            "disabled" => 1, // 禁用
            _ => 0 // 默认正常
        };
    }

    /// <summary>
    /// 设置用户类型命令
    /// </summary>
    [RelayCommand]
    private void SetUserType(string? value)
    {
        if (int.TryParse(value, out var intValue))
        {
            UserType = intValue;
        }
    }

    /// <summary>
    /// 设置性别命令
    /// </summary>
    [RelayCommand]
    private void SetUserGender(string? value)
    {
        if (int.TryParse(value, out var intValue))
        {
            UserGender = intValue;
        }
    }

    /// <summary>
    /// 设置状态命令
    /// </summary>
    [RelayCommand]
    private void SetUserStatus(string? value)
    {
        if (int.TryParse(value, out var intValue))
        {
            UserStatus = intValue;
        }
    }

    public void ForCreate()
    {
        // 清除所有错误消息
        ClearAllErrors();

        IsCreate = true;
        // 拼接标题：新增 + 用户 = "新增用户"
        var createText = _localizationManager.GetString("common.button.create");
        var entityText = _localizationManager.GetString("identity.user.entity");
        Title = $"{createText}{entityText}";
        Username = string.Empty;
        RealName = null;
        Nickname = string.Empty;
        Email = null;
        Phone = null;
        UserType = 1; // 默认普通用户（0=系统用户，1=普通用户）
        UserGender = 0;
        UserStatus = 0; // 启用
        Avatar = "assets/avatar.png"; // 默认头像路径
        Remarks = null;
    }

    public void ForUpdate(UserDto dto)
    {
        // 清除所有错误消息
        ClearAllErrors();

        IsCreate = false;
        // 拼接标题：更新 + 用户 = "更新用户"
        var updateText = _localizationManager.GetString("common.button.update");
        var entityText = _localizationManager.GetString("identity.user.entity");
        Title = $"{updateText}{entityText}";
        Id = dto.Id;
        Username = dto.Username ?? string.Empty;
        RealName = dto.RealName;
        Nickname = dto.Nickname ?? string.Empty;
        Email = dto.Email;
        Phone = dto.Phone;
        UserType = (int)dto.UserType;
        UserGender = (int)dto.UserGender;
        UserStatus = (int)dto.UserStatus;
        Avatar = string.IsNullOrWhiteSpace(dto.Avatar) ? "assets/avatar.png" : dto.Avatar; // 如果为空则使用默认头像
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        UsernameError = string.Empty;
        RealNameError = string.Empty;
        NicknameError = string.Empty;
        EmailError = string.Empty;
        PhoneError = string.Empty;
        AvatarError = string.Empty;
        PasswordError = string.Empty;
        PasswordConfirmError = string.Empty;
        UserTypeError = string.Empty;
        UserGenderError = string.Empty;
        UserStatusError = string.Empty;
        RemarksError = string.Empty;
        Error = string.Empty;
        // 同时清除 WPF 原生验证错误
        ClearAllValidationErrors();
    }

    // 属性变更时进行实时验证
    partial void OnUsernameChanged(string value)
    {
        _operLog?.Debug("[UserFormViewModel] OnUsernameChanged: Username='{Username}'", value);
        ValidateUsername();
    }

    partial void OnRealNameChanged(string? value)
    {
        ValidateRealName();
    }

    partial void OnNicknameChanged(string value)
    {
        ValidateNickname();
    }

    partial void OnEmailChanged(string? value)
    {
        ValidateEmail();
    }

    partial void OnPhoneChanged(string? value)
    {
        ValidatePhone();
    }

    partial void OnAvatarChanged(string? value)
    {
        ValidateAvatar();
    }

    /// <summary>
    /// 验证用户名（实时验证）
    /// </summary>
    public void ValidateUsername(string? username = null)
    {
        var usernameToValidate = username ?? Username;
        _operLog?.Debug("[UserFormViewModel] ValidateUsername: 开始验证，Username='{Username}'", usernameToValidate);

        // 失去焦点时立即验证，不等待提交
        // 统一逻辑：空值时清除错误，等待提交时验证（与其他字段保持一致）
        if (string.IsNullOrWhiteSpace(usernameToValidate))
        {
            UsernameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (usernameToValidate.Length < 4)
        {
            UsernameError = _localizationManager.GetString("identity.user.validation.usernameminlength");
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证失败-长度不足，UsernameError='{Error}'", UsernameError);
        }
        else if (usernameToValidate.Length > 10)
        {
            UsernameError = _localizationManager.GetString("identity.user.validation.usernamemaxlength");
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证失败-长度超限，UsernameError='{Error}'", UsernameError);
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(usernameToValidate, @"^[a-z][a-z0-9]{3,9}$"))
        {
            UsernameError = _localizationManager.GetString("identity.user.validation.usernameinvalid");
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证失败-格式不正确，UsernameError='{Error}'", UsernameError);
        }
        else
        {
            UsernameError = string.Empty; // 验证通过，清除错误
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证通过");
        }
    }

    /// <summary>
    /// 验证真实姓名（实时验证）
    /// </summary>
    public void ValidateRealName(string? realName = null)
    {
        var realNameToValidate = realName ?? RealName;
        if (string.IsNullOrWhiteSpace(realNameToValidate))
        {
            RealNameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        // 不允许数字、点号、空格开头
        // 如果首字符是英文字母，必须是大写
        // 如果首字符是其他语言的字母（中文、日文等），直接允许
        // 后续字符可以是：任何语言的字母、数字、点号、空格
        if (realNameToValidate.Length == 0)
        {
            RealNameError = string.Empty;
            return;
        }

        var firstChar = realNameToValidate[0];
        bool isValidFirstChar = false;

        // 检查首字符：不允许数字、点号、空格开头
        if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
        {
            // 首字符不能是数字、点号、空格
            isValidFirstChar = false;
        }
        else if (char.IsLetter(firstChar))
        {
            // 首字符是字母
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                // 英文字母，必须是大写
                isValidFirstChar = true;
            }
            else if (firstChar >= 'a' && firstChar <= 'z')
            {
                // 英文字母，但是小写，不符合要求
                isValidFirstChar = false;
            }
            else
            {
                // 其他语言的字母（中文、日文、韩文等），直接允许
                isValidFirstChar = true;
            }
        }
        else
        {
            // 其他字符（如标点符号等），允许
            isValidFirstChar = true;
        }

        if (!isValidFirstChar)
        {
            RealNameError = _localizationManager.GetString("identity.user.validation.realnameinvalid");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(realNameToValidate, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
        {
            RealNameError = _localizationManager.GetString("identity.user.validation.realnameinvalid");
        }
        else
        {
            RealNameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证昵称（实时验证）
    /// </summary>
    public void ValidateNickname(string? nickname = null)
    {
        var nicknameToValidate = nickname ?? Nickname;
        if (string.IsNullOrWhiteSpace(nicknameToValidate))
        {
            NicknameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (nicknameToValidate.Length > 40)
        {
            NicknameError = _localizationManager.GetString("identity.user.validation.nicknamemaxlength");
            return;
        }

        // 不允许数字、点号、空格开头
        // 如果首字符是英文字母，必须是大写
        // 如果首字符是其他语言的字母（中文、日文等），直接允许
        // 后续字符可以是：任何语言的字母、数字、点号、空格
        if (nicknameToValidate.Length == 0)
        {
            NicknameError = string.Empty;
            return;
        }

        var firstChar = nicknameToValidate[0];
        bool isValidFirstChar = false;

        // 检查首字符：不允许数字、点号、空格开头
        if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
        {
            // 首字符不能是数字、点号、空格
            isValidFirstChar = false;
        }
        else if (char.IsLetter(firstChar))
        {
            // 首字符是字母
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                // 英文字母，必须是大写
                isValidFirstChar = true;
            }
            else if (firstChar >= 'a' && firstChar <= 'z')
            {
                // 英文字母，但是小写，不符合要求
                isValidFirstChar = false;
            }
            else
            {
                // 其他语言的字母（中文、日文、韩文等），直接允许
                isValidFirstChar = true;
            }
        }
        else
        {
            // 其他字符（如标点符号等），允许
            isValidFirstChar = true;
        }

        if (!isValidFirstChar)
        {
            NicknameError = _localizationManager.GetString("identity.user.validation.nicknameinvalid");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(nicknameToValidate, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
        {
            NicknameError = _localizationManager.GetString("identity.user.validation.nicknameinvalid");
        }
        else
        {
            NicknameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证邮箱（实时验证）
    /// </summary>
    public void ValidateEmail(string? email = null)
    {
        var emailToValidate = email ?? Email;
        if (string.IsNullOrWhiteSpace(emailToValidate))
        {
            EmailError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(emailToValidate, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$"))
        {
            EmailError = _localizationManager.GetString("identity.user.validation.emailinvalid");
        }
        else
        {
            EmailError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证手机号（实时验证）
    /// </summary>
    public void ValidatePhone(string? phone = null)
    {
        var phoneToValidate = phone ?? Phone;
        if (string.IsNullOrWhiteSpace(phoneToValidate))
        {
            PhoneError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(phoneToValidate, @"^1[3-9]\d{9}$"))
        {
            var formatError = string.Format(_localizationManager.GetString("common.validation.format"), _localizationManager.GetString("identity.user.phone"));
            var specificRule = _localizationManager.GetString("identity.user.validation.phoneinvalid");
            PhoneError = $"{formatError}，{specificRule}";
        }
        else
        {
            PhoneError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证头像（实时验证）
    /// </summary>
    public void ValidateAvatar(string? avatar = null)
    {
        var avatarToValidate = avatar ?? Avatar;
        if (string.IsNullOrWhiteSpace(avatarToValidate))
        {
            AvatarError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        var avatarTrimmed = avatarToValidate.Trim();

        // 检查是否为绝对路径（Windows盘符或Unix根路径）
        if (System.IO.Path.IsPathRooted(avatarTrimmed) ||
            avatarTrimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            avatarTrimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            avatarTrimmed.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            AvatarError = string.Format(_localizationManager.GetString("common.validation.mustberelativepath"), _localizationManager.GetString("identity.user.avatar"));
        }
        // 检查路径长度（数据库字段最大256字符）
        else if (avatarTrimmed.Length > 256)
        {
            AvatarError = _localizationManager.GetString("identity.user.validation.avatarpathtoolong");
        }
        else
        {
            AvatarError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证密码（实时验证）
    /// </summary>
    public void ValidatePassword(string password, string passwordConfirm)
    {
        if (!IsCreate) return; // 更新模式不验证密码

        if (string.IsNullOrWhiteSpace(password))
        {
            PasswordError = string.Empty; // 空值时不清除错误，等待提交时验证
        }
        else if (password.Length < 6)
        {
            PasswordError = _localizationManager.GetString("identity.user.validation.passwordminlength");
        }
        else
        {
            PasswordError = string.Empty; // 验证通过，清除错误
        }

        // 同时验证确认密码
        ValidatePasswordConfirm(password, passwordConfirm);
    }

    /// <summary>
    /// 验证确认密码（实时验证）
    /// </summary>
    public void ValidatePasswordConfirm(string password, string passwordConfirm)
    {
        if (!IsCreate) return; // 更新模式不验证密码

        if (string.IsNullOrWhiteSpace(passwordConfirm))
        {
            PasswordConfirmError = string.Empty; // 空值时不清除错误，等待提交时验证
        }
        else if (!string.IsNullOrWhiteSpace(password) && password != passwordConfirm)
        {
            PasswordConfirmError = _localizationManager.GetString("identity.user.validation.passwordmismatch");
        }
        else
        {
            PasswordConfirmError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields(string? password = null!, string? passwordConfirm = null)
    {
        // 清除所有错误，重新验证
        ClearAllErrors();
        bool isValid = true;

        // 验证用户名（必填）
        if (string.IsNullOrWhiteSpace(Username))
        {
            UsernameError = _localizationManager.GetString("identity.user.validation.usernamerequired");
            isValid = false;
        }
        else if (Username.Length < 4)
        {
            UsernameError = _localizationManager.GetString("identity.user.validation.usernameminlength");
            isValid = false;
        }
        else if (Username.Length > 10)
        {
            UsernameError = _localizationManager.GetString("identity.user.validation.usernamemaxlength");
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Username, @"^[a-z][a-z0-9]{3,9}$"))
        {
            UsernameError = _localizationManager.GetString("identity.user.validation.usernameinvalid");
            isValid = false;
        }

        // 验证真实姓名（必填）
        if (string.IsNullOrWhiteSpace(RealName))
        {
            RealNameError = _localizationManager.GetString("identity.user.validation.realnamerequired");
            isValid = false;
        }
        else
        {
            // 不允许数字、点号、空格开头
            // 如果首字符是英文字母，必须是大写
            var firstChar = RealName[0];
            bool isValidFirstChar = false;

            if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
            {
                // 首字符不能是数字、点号、空格
                isValidFirstChar = false;
            }
            else if (char.IsLetter(firstChar))
            {
                // 首字符是字母
                if (firstChar >= 'A' && firstChar <= 'Z')
                {
                    isValidFirstChar = true;
                }
                else if (firstChar >= 'a' && firstChar <= 'z')
                {
                    isValidFirstChar = false;
                }
                else
                {
                    // 其他语言的字母（中文、日文、韩文等），直接允许
                    isValidFirstChar = true;
                }
            }
            else
            {
                // 其他字符（如标点符号等），允许
                isValidFirstChar = true;
            }

            if (!isValidFirstChar)
            {
                RealNameError = _localizationManager.GetString("identity.user.validation.realnameinvalid");
                isValid = false;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(RealName, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
            {
                RealNameError = _localizationManager.GetString("identity.user.validation.realnameinvalid");
                isValid = false;
            }
        }

        // 验证昵称（必填）
        if (string.IsNullOrWhiteSpace(Nickname))
        {
            NicknameError = _localizationManager.GetString("identity.user.validation.nicknamerequired");
            isValid = false;
        }
        else if (Nickname.Length > 40)
        {
            NicknameError = _localizationManager.GetString("identity.user.validation.nicknamemaxlength");
            isValid = false;
        }
        else
        {
            // 不允许数字、点号、空格开头
            // 如果首字符是英文字母，必须是大写
            var firstChar = Nickname[0];
            bool isValidFirstChar = false;

            if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
            {
                // 首字符不能是数字、点号、空格
                isValidFirstChar = false;
            }
            else if (char.IsLetter(firstChar))
            {
                // 首字符是字母
                if (firstChar >= 'A' && firstChar <= 'Z')
                {
                    isValidFirstChar = true;
                }
                else if (firstChar >= 'a' && firstChar <= 'z')
                {
                    isValidFirstChar = false;
                }
                else
                {
                    // 其他语言的字母（中文、日文、韩文等），直接允许
                    isValidFirstChar = true;
                }
            }
            else
            {
                // 其他字符（如标点符号等），允许
                isValidFirstChar = true;
            }

            if (!isValidFirstChar)
            {
                NicknameError = _localizationManager.GetString("identity.user.validation.nicknameinvalid");
                isValid = false;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(Nickname, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
            {
                NicknameError = _localizationManager.GetString("identity.user.validation.nicknameinvalid");
                isValid = false;
            }
        }

        // 验证邮箱（必填）
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = _localizationManager.GetString("identity.user.validation.emailrequired");
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$"))
        {
            EmailError = _localizationManager.GetString("identity.user.validation.emailinvalid");
            isValid = false;
        }

        // 验证手机号（必填）
        if (string.IsNullOrWhiteSpace(Phone))
        {
            PhoneError = _localizationManager.GetString("identity.user.validation.phonerequired");
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^1[3-9]\d{9}$"))
        {
            var formatError = string.Format(_localizationManager.GetString("common.validation.format"), _localizationManager.GetString("identity.user.phone"));
            var specificRule = _localizationManager.GetString("identity.user.validation.phoneinvalid");
            PhoneError = $"{formatError}，{specificRule}";
            isValid = false;
        }

        // 验证头像（必填，且必须是相对路径）
        if (string.IsNullOrWhiteSpace(Avatar))
        {
            AvatarError = _localizationManager.GetString("identity.user.validation.avatarrequired");
            isValid = false;
        }
        else
        {
            // 验证是否为相对路径（不能是绝对路径或URL）
            var avatar = Avatar.Trim();

            // 检查是否为绝对路径（Windows盘符或Unix根路径）
            if (System.IO.Path.IsPathRooted(avatar) ||
                avatar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                avatar.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                avatar.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                AvatarError = string.Format(_localizationManager.GetString("common.validation.mustberelativepath"), _localizationManager.GetString("identity.user.avatar"));
                isValid = false;
            }
            // 检查路径长度（数据库字段最大256字符）
            else if (avatar.Length > 256)
            {
                AvatarError = _localizationManager.GetString("identity.user.validation.avatarpathtoolong");
                isValid = false;
            }
        }

        // 验证密码（创建时必填）
        if (IsCreate)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordError = _localizationManager.GetString("identity.user.validation.passwordrequired");
                isValid = false;
            }
            else if (password.Length < 6)
            {
                PasswordError = _localizationManager.GetString("identity.user.validation.passwordminlength");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(passwordConfirm))
            {
                PasswordConfirmError = _localizationManager.GetString("identity.user.validation.passwordconfirmrequired");
                isValid = false;
            }
            else if (!string.IsNullOrWhiteSpace(password) && password != passwordConfirm)
            {
                PasswordConfirmError = _localizationManager.GetString("identity.user.validation.passwordmismatch");
                isValid = false;
            }
        }

        return isValid;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // 先从控件读取所有值（类似密码的方式）
            string username = string.Empty;
            string realName = string.Empty;
            string nickname = string.Empty;
            string email = string.Empty;
            string phone = string.Empty;
            string avatar = string.Empty;
            string? password = null;
            string? passwordConfirm = null;

            if (_textFieldsAccessor != null)
            {
                var (u, rn, n, e, p, a, r) = _textFieldsAccessor.Invoke();
                username = u ?? string.Empty;
                realName = rn ?? string.Empty;
                nickname = n ?? string.Empty;
                email = e ?? string.Empty;
                phone = p ?? string.Empty;
                avatar = a ?? string.Empty;
                var remarksValue = r ?? string.Empty;

                // 更新 ViewModel 属性
                Username = username;
                RealName = realName;
                Nickname = nickname;
                Email = email;
                Phone = phone;
                Avatar = avatar;
                Remarks = remarksValue;
            }

            if (IsCreate)
            {
                var (pwd, confirm) = _passwordAccessor?.Invoke() ?? (string.Empty, string.Empty);
                password = pwd;
                passwordConfirm = confirm;
            }

            // 验证所有字段（清除所有错误并重新验证）
            if (!ValidateFields(password, passwordConfirm))
            {
                return;
            }

            if (IsCreate)
            {
                var dto = new UserCreateDto
                {
                    Username = Username,
                    Password = password!,
                    RealName = RealName!,
                    Nickname = Nickname,
                    Email = Email!,
                    Phone = Phone!,
                    Avatar = Avatar!,
                    UserType = (Takt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Takt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Takt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks!
                };
                var result = await _userService.CreateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("identity.user.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 创建成功，显示成功消息
                var entityNameSuccess = _localizationManager.GetString("identity.user.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.create"), entityNameSuccess);
                TaktMessageManager.ShowSuccess(successMessage);
            }
            else
            {
                var dto = new UserUpdateDto
                {
                    Id = Id,
                    Username = Username,
                    RealName = RealName!,
                    Nickname = Nickname!,
                    Email = Email!,
                    Phone = Phone!,
                    Avatar = Avatar!,
                    UserType = (Takt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Takt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Takt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks!,
                    Password = "" // 更新不改密码
                };
                var result = await _userService.UpdateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("identity.user.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 更新成功，显示成功消息
                var entityNameSuccess = _localizationManager.GetString("identity.user.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.update"), entityNameSuccess);
                TaktMessageManager.ShowSuccess(successMessage);
            }

            // 保存成功，触发回调关闭窗口
            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
        }
    }
}


