//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : LoginViewModel.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 登录窗体视图模型（参照WPFGallery实现，使用CommunityToolkit.Mvvm）
//===================================================================

using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hbt.Application.Dtos.Identity;
using Hbt.Application.Services.Identity;
using Hbt.Common.Results;
using Hbt.Fluent.Views;
using Hbt.Common.Helpers;

namespace Hbt.Fluent.ViewModels.Identity;

/// <summary>
/// 登录窗体视图模型
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly ILoginService _loginService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _languageToolTip = "切换语言";

    [ObservableProperty]
    private string _themeToolTip = "切换主题";

    public LoginViewModel(ILoginService loginService)
    {
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));

        // 读取本地默认账号配置
        try
        {
            var remember = LocalConfigHelper.GetSetting("login.remember", "0");
            RememberMe = remember == "1";

            var savedUser = LocalConfigHelper.GetSetting("login.username", string.Empty);
            var savedPwd = LocalConfigHelper.GetSetting("login.password", string.Empty);

            if (RememberMe)
            {
                Username = savedUser;
                Password = savedPwd; // 将在窗口 Loaded 时同步到 PasswordBox
            }
            else
            {
                // 未勾选记住时，使用种子默认账号做一次性预填
                Username = string.IsNullOrWhiteSpace(savedUser) ? "admin" : savedUser;
                Password = string.IsNullOrWhiteSpace(savedPwd) ? "Hbt@123" : savedPwd;
            }
        }
        catch { }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        try
        {
            // 命令入口日志
            try
            {
                var operLog = App.Services.GetService<Hbt.Common.Logging.OperLogManager>();
                operLog?.Information("[Login] LoginCommand 调用，User='{Username}', HasPassword={HasPwd}", Username, !string.IsNullOrWhiteSpace(Password));
            }
            catch { /* 忽略日志获取异常，避免影响登录流程 */ }

            IsLoading = true;
            ErrorMessage = string.Empty;

            var loginDto = new LoginDto
            {
                Username = Username,
                Password = Password,
                RememberMe = RememberMe
            };

            var result = await _loginService.LoginAsync(loginDto);

            if (result.Success && result.Data != null)
            {
                // 登录成功，保存用户会话
                await SaveUserSessionAsync(result.Data);

                // 记住账号（可选）
                try
                {
                    LocalConfigHelper.SaveSetting("login.remember", RememberMe ? "1" : "0");
                    if (RememberMe)
                    {
                        LocalConfigHelper.SaveSetting("login.username", Username);
                        LocalConfigHelper.SaveSetting("login.password", Password);
                    }
                    else
                    {
                        LocalConfigHelper.SaveSetting("login.username", string.Empty);
                        LocalConfigHelper.SaveSetting("login.password", string.Empty);
                    }
                }
                catch { }

                // 打开主窗口
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // 检查 Services 是否可用
                    if (App.Services == null)
                    {
                        throw new InvalidOperationException("App.Services 为 null，无法获取服务");
                    }

                    // 通过依赖注入获取主窗口
                    var mainWindow = App.Services.GetRequiredService<Views.MainWindow>();
                    
                    // 加载当前用户的菜单
                    var mainViewModel = App.Services.GetRequiredService<ViewModels.MainWindowViewModel>();
                    if (result.Data?.UserId > 0)
                    {
                        _ = mainViewModel.LoadMenusAsync(result.Data.UserId);
                    }
                    
                    mainWindow.Show();
                    
                    // 重要：设置 Application.Current.MainWindow，确保后续导航可以找到主窗口
                    System.Windows.Application.Current.MainWindow = mainWindow;

                    // 关闭登录窗口
                    foreach (Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window is Views.Identity.LoginWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                });
            }
            else
            {
                ErrorMessage = result.Message ?? "登录失败，请检查用户名和密码";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"登录失败：{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLogin()
    {
        // 仅当不在加载中且用户名与密码均有值时才可点击
        return !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }

    /// <summary>
    /// 保存用户会话
    /// </summary>
    private async Task SaveUserSessionAsync(LoginResultDto loginResult)
    {
        // 保存用户会话到 UserContext
        if (loginResult != null && loginResult.UserId > 0)
        {
            var userContext = Hbt.Common.Context.UserContext.AddUser(loginResult.UserId);
            userContext.SetLoginInfo(
                loginResult.UserId,
                loginResult.Username ?? string.Empty,
                loginResult.RealName ?? string.Empty,
                loginResult.RoleId,
                loginResult.RoleName ?? string.Empty,
                string.Empty, // SessionId - 可以根据需要设置
                loginResult.AccessToken ?? string.Empty,
                loginResult.RefreshToken ?? string.Empty,
                loginResult.ExpiresAt);
            
            // 设置当前用户
            Hbt.Common.Context.UserContext.SetCurrent(loginResult.UserId);
        }
        
        await Task.CompletedTask;
    }

    partial void OnUsernameChanged(string value)
    {
        // LoginCommand 会自动更新 CanExecute，无需手动调用
        // LoginCommand?.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        // LoginCommand 会自动更新 CanExecute，无需手动调用
        // LoginCommand?.NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        // LoginCommand 会自动更新 CanExecute，无需手动调用
        // LoginCommand?.NotifyCanExecuteChanged();
    }
}