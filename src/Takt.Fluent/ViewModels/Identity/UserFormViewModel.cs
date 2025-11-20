//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : UserFormViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-30
// 版本号 : 0.0.1
// 描述    : 用户表单视图模型（新建/更新）
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;

namespace Takt.Fluent.ViewModels.Identity;

public partial class UserFormViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly ILocalizationManager _localizationManager;

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
    
    // Hint 提示属性
    public string UsernameHint => _localizationManager.GetString("Identity.User.Validation.UsernameInvalid") ?? "用户名必须以小写字母开头，只能包含小写字母和数字，长度4-10位";
    
    public string EmailHint => _localizationManager.GetString("Identity.User.Validation.EmailInvalid") ?? "邮箱格式不正确";
    
    public string RealNameHint => _localizationManager.GetString("Identity.User.Validation.RealNameHint") ?? "不允许数字、点号、空格开头，英文字母首字母大写，30字以内";
    
    public string NicknameHint => _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等，如：Cheng.Jianhong、Joseph Robinette Biden Jr. 或 张三";
    
    public string PhoneHint => _localizationManager.GetString("Identity.User.Validation.PhoneInvalid") ?? "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9";
    
    public string PasswordHint => _localizationManager.GetString("Identity.User.Validation.PasswordMinLength") ?? "密码长度不能少于6位";
    
    // 错误消息属性
    [ObservableProperty] private string _usernameError = string.Empty;
    [ObservableProperty] private string _realNameError = string.Empty;
    [ObservableProperty] private string _nicknameError = string.Empty;
    [ObservableProperty] private string _emailError = string.Empty;
    [ObservableProperty] private string _phoneError = string.Empty;
    [ObservableProperty] private string _avatarError = string.Empty;
    [ObservableProperty] private string _passwordError = string.Empty;
    [ObservableProperty] private string _passwordConfirmError = string.Empty;
    [ObservableProperty] private string _userTypeError = string.Empty;
    [ObservableProperty] private string _userGenderError = string.Empty;
    [ObservableProperty] private string _userStatusError = string.Empty;
    [ObservableProperty] private string _remarksError = string.Empty;

    private Func<(string pwd, string confirm)>? _passwordAccessor;
    public void AttachPasswordAccess(Func<(string pwd, string confirm)> accessor) => _passwordAccessor = accessor;
    
    // 文本字段值访问器（类似密码的方式，直接从控件读取值）
    private Func<(string username, string realName, string nickname, string email, string phone, string avatar)>? _textFieldsAccessor;
    public void AttachTextFieldsAccess(Func<(string username, string realName, string nickname, string email, string phone, string avatar)> accessor) => _textFieldsAccessor = accessor;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public UserFormViewModel(IUserService userService, ILocalizationManager localizationManager)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }

    public void ForCreate()
    {
        // 清除所有错误消息
        ClearAllErrors();
        
        IsCreate = true;
        Title = _localizationManager.GetString("Identity.User.Create") ?? "新建用户";
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
        Title = _localizationManager.GetString("Identity.User.Update") ?? "编辑用户";
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
    }

    // 属性变更时进行实时验证
    partial void OnUsernameChanged(string value)
    {
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
    private void ValidateUsername()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            UsernameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (Username.Length < 4)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMinLength") ?? "用户名长度不能少于4位";
        }
        else if (Username.Length > 10)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMaxLength") ?? "用户名长度不能超过10位";
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Username, @"^[a-z][a-z0-9]{3,9}$"))
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameInvalid") ?? "用户名必须以小写字母开头，只能包含小写字母和数字，长度4-10位";
        }
        else
        {
            UsernameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证真实姓名（实时验证）
    /// </summary>
    private void ValidateRealName()
    {
        if (string.IsNullOrWhiteSpace(RealName))
        {
            RealNameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        // 不允许数字、点号、空格开头
        // 如果首字符是英文字母，必须是大写
        // 如果首字符是其他语言的字母（中文、日文等），直接允许
        // 后续字符可以是：任何语言的字母、数字、点号、空格
        if (RealName.Length == 0)
        {
            RealNameError = string.Empty;
            return;
        }

        var firstChar = RealName[0];
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
            RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid") ?? "不允许数字、点号、空格开头，英文字母首字母必须大写";
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(RealName, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
        {
            RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid") ?? "只能包含字母、数字、点和空格";
        }
        else
        {
            RealNameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证昵称（实时验证）
    /// </summary>
    private void ValidateNickname()
    {
        if (string.IsNullOrWhiteSpace(Nickname))
        {
            NicknameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (Nickname.Length > 40)
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameMaxLength") ?? "昵称长度不能超过40个字符";
            return;
        }

        // 不允许数字、点号、空格开头
        // 如果首字符是英文字母，必须是大写
        // 如果首字符是其他语言的字母（中文、日文等），直接允许
        // 后续字符可以是：任何语言的字母、数字、点号、空格
        if (Nickname.Length == 0)
        {
            NicknameError = string.Empty;
            return;
        }

        var firstChar = Nickname[0];
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
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等";
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Nickname, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称只能包含字母、数字、点和空格";
        }
        else
        {
            NicknameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证邮箱（实时验证）
    /// </summary>
    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$"))
        {
            EmailError = _localizationManager.GetString("Identity.User.Validation.EmailInvalid") ?? "邮箱格式不正确";
        }
        else
        {
            EmailError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证手机号（实时验证）
    /// </summary>
    private void ValidatePhone()
    {
        if (string.IsNullOrWhiteSpace(Phone))
        {
            PhoneError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^1[3-9]\d{9}$"))
        {
            PhoneError = _localizationManager.GetString("Identity.User.Validation.PhoneInvalid") ?? "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9";
        }
        else
        {
            PhoneError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证头像（实时验证）
    /// </summary>
    private void ValidateAvatar()
    {
        if (string.IsNullOrWhiteSpace(Avatar))
        {
            AvatarError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        var avatar = Avatar.Trim();
        
        // 检查是否为绝对路径（Windows盘符或Unix根路径）
        if (System.IO.Path.IsPathRooted(avatar) || 
            avatar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            avatar.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            avatar.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarMustBeRelativePath") ?? "头像必须是相对路径，不能使用绝对路径或URL";
        }
        // 检查路径长度（数据库字段最大256字符）
        else if (avatar.Length > 256)
        {
            AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarPathTooLong") ?? "头像路径长度不能超过256个字符";
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
            PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordMinLength") ?? "密码长度不能少于6位";
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
            PasswordConfirmError = _localizationManager.GetString("Identity.User.Validation.PasswordMismatch") ?? "两次输入的密码不一致";
        }
        else
        {
            PasswordConfirmError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields(string? password = null, string? passwordConfirm = null)
    {
        // 清除所有错误，重新验证
        ClearAllErrors();
        bool isValid = true;

        // 验证用户名（必填）
        if (string.IsNullOrWhiteSpace(Username))
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameRequired") ?? "用户名不能为空";
            isValid = false;
        }
        else if (Username.Length < 4)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMinLength") ?? "用户名长度不能少于4位";
            isValid = false;
        }
        else if (Username.Length > 10)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMaxLength") ?? "用户名长度不能超过10位";
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Username, @"^[a-z][a-z0-9]{3,9}$"))
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameInvalid") ?? "用户名必须以小写字母开头，只能包含小写字母和数字，长度4-10位";
            isValid = false;
        }

        // 验证真实姓名（必填）
        if (string.IsNullOrWhiteSpace(RealName))
        {
            RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameRequired") ?? "真实姓名不能为空";
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
                RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid") ?? "不允许数字、点号、空格开头，英文字母首字母必须大写";
                isValid = false;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(RealName, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
            {
                RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid") ?? "只能包含字母、数字、点和空格";
                isValid = false;
            }
        }

        // 验证昵称（必填）
        if (string.IsNullOrWhiteSpace(Nickname))
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameRequired") ?? "昵称不能为空";
            isValid = false;
        }
        else if (Nickname.Length > 40)
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameMaxLength") ?? "昵称长度不能超过40个字符";
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
                NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等";
                isValid = false;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(Nickname, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
            {
                NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称只能包含字母、数字、点和空格";
                isValid = false;
            }
        }

        // 验证邮箱（必填）
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = _localizationManager.GetString("Identity.User.Validation.EmailRequired") ?? "邮箱不能为空";
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$"))
        {
            EmailError = _localizationManager.GetString("Identity.User.Validation.EmailInvalid") ?? "邮箱格式不正确";
            isValid = false;
        }

        // 验证手机号（必填）
        if (string.IsNullOrWhiteSpace(Phone))
        {
            PhoneError = _localizationManager.GetString("Identity.User.Validation.PhoneRequired") ?? "手机号不能为空";
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^1[3-9]\d{9}$"))
        {
            PhoneError = _localizationManager.GetString("Identity.User.Validation.PhoneInvalid") ?? "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9";
            isValid = false;
        }

        // 验证头像（必填，且必须是相对路径）
        if (string.IsNullOrWhiteSpace(Avatar))
        {
            AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarRequired") ?? "头像不能为空";
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
                AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarMustBeRelativePath") ?? "头像必须是相对路径，不能使用绝对路径或URL";
                isValid = false;
            }
            // 检查路径长度（数据库字段最大256字符）
            else if (avatar.Length > 256)
            {
                AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarPathTooLong") ?? "头像路径长度不能超过256个字符";
                isValid = false;
            }
        }

        // 验证密码（创建时必填）
        if (IsCreate)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordRequired") ?? "密码不能为空";
                isValid = false;
            }
            else if (password.Length < 6)
            {
                PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordMinLength") ?? "密码长度不能少于6位";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(passwordConfirm))
            {
                PasswordConfirmError = _localizationManager.GetString("Identity.User.Validation.PasswordConfirmRequired") ?? "确认密码不能为空";
                isValid = false;
            }
            else if (!string.IsNullOrWhiteSpace(password) && password != passwordConfirm)
            {
                PasswordConfirmError = _localizationManager.GetString("Identity.User.Validation.PasswordMismatch") ?? "两次输入的密码不一致";
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
                var (u, rn, n, e, p, a) = _textFieldsAccessor.Invoke();
                username = u ?? string.Empty;
                realName = rn ?? string.Empty;
                nickname = n ?? string.Empty;
                email = e ?? string.Empty;
                phone = p ?? string.Empty;
                avatar = a ?? string.Empty;
                
                // 更新 ViewModel 属性
                Username = username;
                RealName = realName;
                Nickname = nickname;
                Email = email;
                Phone = phone;
                Avatar = avatar;
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
                    RealName = RealName,
                    Nickname = Nickname,
                    Email = Email,
                    Phone = Phone,
                    Avatar = Avatar,
                    UserType = (Takt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Takt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Takt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks
                };
                var result = await _userService.CreateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create") ?? "{0}创建失败", entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }
                
                // 创建成功，显示成功消息
                var entityNameSuccess = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                var successMessage = string.Format(_localizationManager.GetString("common.success.create") ?? "{0}创建成功", entityNameSuccess);
                TaktMessageManager.ShowSuccess(successMessage);
            }
            else
            {
                var dto = new UserUpdateDto
                {
                    Id = Id,
                    Username = Username,
                    RealName = RealName,
                    Nickname = Nickname,
                    Email = Email,
                    Phone = Phone,
                    Avatar = Avatar,
                    UserType = (Takt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Takt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Takt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks,
                    Password = "" // 更新不改密码
                };
                var result = await _userService.UpdateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update") ?? "{0}更新失败", entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }
                
                // 更新成功，显示成功消息
                var entityNameSuccess = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                var successMessage = string.Format(_localizationManager.GetString("common.success.update") ?? "{0}更新成功", entityNameSuccess);
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


