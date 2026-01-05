//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : UserProfileViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 用户信息视图模型
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Context;
using Takt.Common.Results;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 用户信息视图模型
/// </summary>
public partial class UserProfileViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly UserContext _userContext;

    [ObservableProperty]
    private UserDto? _userInfo;

    [ObservableProperty]
    private bool _isLoading;

    public UserProfileViewModel(IUserService userService)
    {
        _userService = userService;
        _userContext = UserContext.Current;
        _ = LoadUserInfoAsync();
    }

    /// <summary>
    /// 加载用户信息
    /// </summary>
    private async Task LoadUserInfoAsync()
    {
        if (!_userContext.IsAuthenticated || _userContext.UserId == 0)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _userService.GetByIdAsync(_userContext.UserId);
            if (result.Success && result.Data != null)
            {
                // 如果头像为空，使用默认头像
                if (string.IsNullOrWhiteSpace(result.Data.Avatar))
                {
                    result.Data.Avatar = "assets/avatar.png";
                }
                UserInfo = result.Data;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 关闭命令
    /// </summary>
    [RelayCommand]
    private void Close(Window? window)
    {
        window?.Close();
    }
}
