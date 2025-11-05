//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : UserFormViewModel.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-30
// 版本号 : 1.0
// 描述    : 用户表单视图模型（新建/更新）
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hbt.Application.Dtos.Identity;
using Hbt.Application.Services.Identity;
using Hbt.Fluent.Services;

namespace Hbt.Fluent.ViewModels.Identity;

public partial class UserFormViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly LanguageService _languageService;

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private bool _isCreate = true;
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string? _realName;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private int _userType;
    [ObservableProperty] private int _userGender;
    [ObservableProperty] private int _userStatus;
    [ObservableProperty] private string? _remarks;
    [ObservableProperty] private long _id;
    [ObservableProperty] private DateTime _createdTime;
    [ObservableProperty] private DateTime _updatedTime;
    [ObservableProperty] private string _error = string.Empty;

    private Func<(string pwd, string confirm)>? _passwordAccessor;
    public void AttachPasswordAccess(Func<(string pwd, string confirm)> accessor) => _passwordAccessor = accessor;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public UserFormViewModel(IUserService userService, LanguageService languageService)
    {
        _userService = userService;
        _languageService = languageService;
    }

    public void ForCreate()
    {
        IsCreate = true;
        Title = _languageService.GetTranslation("Identity.User.Create", "新建用户");
        UserStatus = 0; // 启用
    }

    public void ForUpdate(UserDto dto)
    {
        IsCreate = false;
        Title = _languageService.GetTranslation("Identity.User.Edit", "编辑用户");
        Id = dto.Id;
        Username = dto.Username;
        RealName = dto.RealName;
        Email = dto.Email;
        Phone = dto.Phone;
        UserType = (int)dto.UserType;
        UserGender = (int)dto.UserGender;
        UserStatus = (int)dto.UserStatus;
        Remarks = dto.Remarks;
        CreatedTime = dto.CreatedTime;
        UpdatedTime = dto.UpdatedTime;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        Error = string.Empty;
        try
        {
            if (IsCreate)
            {
                var (pwd, confirm) = _passwordAccessor?.Invoke() ?? (string.Empty, string.Empty);
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(pwd))
                {
                    Error = _languageService.GetTranslation("Identity.User.Validation.UsernamePasswordRequired", "用户名和密码不能为空");
                    return;
                }
                if (pwd != confirm)
                {
                    Error = _languageService.GetTranslation("Identity.User.Validation.PasswordMismatch", "两次输入的密码不一致");
                    return;
                }
                var dto = new UserCreateDto
                {
                    Username = Username,
                    Password = pwd,
                    RealName = RealName,
                    Email = Email,
                    Phone = Phone,
                    UserType = (Hbt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Hbt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Hbt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks
                };
                var result = await _userService.CreateAsync(dto);
                if (!result.Success) { Error = result.Message ?? _languageService.GetTranslation("common.saveFailed", "保存失败"); return; }
            }
            else
            {
                var dto = new UserUpdateDto
                {
                    Id = Id,
                    Username = Username,
                    RealName = RealName,
                    Email = Email,
                    Phone = Phone,
                    UserType = (Hbt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Hbt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Hbt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks,
                    Password = "" // 更新不改密码
                };
                var result = await _userService.UpdateAsync(dto);
                if (!result.Success) { Error = result.Message ?? _languageService.GetTranslation("common.saveFailed", "保存失败"); return; }
            }

            // 保存成功，触发回调关闭窗口
            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}


