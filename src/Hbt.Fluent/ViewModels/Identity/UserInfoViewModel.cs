//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : UserInfoViewModel.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 用户信息视图模型
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hbt.Application.Dtos.Identity;
using Hbt.Application.Services.Identity;
using Hbt.Common.Context;
using Hbt.Common.Results;

namespace Hbt.Fluent.ViewModels.Identity;

/// <summary>
/// 用户信息视图模型
/// </summary>
public partial class UserInfoViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly UserContext _userContext;

    [ObservableProperty]
    private UserDto? _userInfo;

    [ObservableProperty]
    private bool _isLoading;

    public UserInfoViewModel(IUserService userService)
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
