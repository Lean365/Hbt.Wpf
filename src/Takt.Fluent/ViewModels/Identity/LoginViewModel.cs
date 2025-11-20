//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : LoginViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 登录窗体视图模型（参照WPFGallery实现，使用CommunityToolkit.Mvvm）
//===================================================================

using System.Windows;
using System.Windows.Media.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Domain.Interfaces;
using Takt.Application.Services.Identity;
using Takt.Common.Results;
using Takt.Fluent.Views;
using Takt.Fluent.Controls;
using Takt.Common.Helpers;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 登录窗体视图模型
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly ILoginService _loginService;
    private readonly ILocalizationManager _localizationManager;

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
    private string _usernameError = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    [ObservableProperty]
    private string _languageToolTip = string.Empty;

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        ErrorMessage = string.Empty;
        UsernameError = string.Empty;
        PasswordError = string.Empty;
    }

    /// <summary>
    /// 清除字段错误消息
    /// </summary>
    private void ClearFieldErrors()
    {
        UsernameError = string.Empty;
        PasswordError = string.Empty;
    }

    [ObservableProperty]
    private string _themeToolTip = string.Empty;

    public LoginViewModel(ILoginService loginService, ILocalizationManager localizationManager)
    {
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        // 初始化本地化提示文本
        LanguageToolTip = _localizationManager.GetString("Button.ChangeLanguage") ?? "切换语言";
        ThemeToolTip = _localizationManager.GetString("Button.ChangeTheme") ?? "切换主题";

        // 读取本地默认账号配置
        try
        {
            var remember = AppSettingsHelper.GetSetting("login.remember", "0");
            RememberMe = remember == "1";

            var savedUser = AppSettingsHelper.GetSetting("login.username", string.Empty);
            var savedPwd = AppSettingsHelper.GetSetting("login.password", string.Empty);

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
                var operLog = App.Services.GetService<Takt.Common.Logging.OperLogManager>();
                operLog?.Information("[Login] LoginCommand 调用，User='{Username}', HasPassword={HasPwd}", Username, !string.IsNullOrWhiteSpace(Password));
            }
            catch { /* 忽略日志获取异常，避免影响登录流程 */ }

            IsLoading = true;
            ClearAllErrors();

            // 先检查数据库连接
            if (!await CheckDatabaseConnectionAsync())
            {
                IsLoading = false;
                return;
            }

            // 验证输入
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameRequired") ?? "用户名不能为空";
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordRequired") ?? "密码不能为空";
                isValid = false;
            }

            if (!isValid)
            {
                IsLoading = false;
                return;
            }

            var loginDto = new LoginDto
            {
                Username = Username,
                Password = Password,
                RememberMe = RememberMe
            };

            var result = await _loginService.LoginAsync(loginDto);

            if (result.Success && result.Data != null)
            {
                // 显示登录成功消息（类型A：弹出自动消失提示框，10秒，顶端对齐）
                var successMessage = result.Message ?? "登录成功";
                TaktMessageManager.ShowToastWindow(successMessage, "成功", MessageBoxImage.Information, 10000);
                
                // 登录成功，保存用户会话
                await SaveUserSessionAsync(result.Data);

                // 记住账号（可选）
                try
                {
                    AppSettingsHelper.SaveSetting("login.remember", RememberMe ? "1" : "0");
                    if (RememberMe)
                    {
                        AppSettingsHelper.SaveSetting("login.username", Username);
                        AppSettingsHelper.SaveSetting("login.password", Password);
                    }
                    else
                    {
                        AppSettingsHelper.SaveSetting("login.username", string.Empty);
                        AppSettingsHelper.SaveSetting("login.password", string.Empty);
                    }
                }
                catch { }

                // 打开主窗口（使用 InvokeAsync 避免阻塞 UI 线程）
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
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
                        
                        // 先显示主窗口（使用淡入动画）
                        mainWindow.Opacity = 0;
                        mainWindow.Show();
                        
                        // 重要：设置 Application.Current.MainWindow，确保后续导航可以找到主窗口
                        System.Windows.Application.Current.MainWindow = mainWindow;

                        // 主窗口淡入动画
                        var fadeInAnimation = new DoubleAnimation
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromMilliseconds(300)
                        };
                        
                        // 主窗口淡入动画完成后的处理
                        fadeInAnimation.Completed += (s, e) =>
                        {
                            // 主窗口淡入完成后，开始登录窗口淡出
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                            {
                                // 查找并关闭登录窗口
                                foreach (Window window in System.Windows.Application.Current.Windows)
                                {
                                    if (window is Views.Identity.LoginWindow loginWindow)
                                    {
                                        // 登录窗口淡出动画
                                        var fadeOutAnimation = new DoubleAnimation
                                        {
                                            From = 1,
                                            To = 0,
                                            Duration = TimeSpan.FromMilliseconds(200)
                                        };
                                        fadeOutAnimation.Completed += (s2, e2) =>
                                        {
                                            try
                                            {
                                                loginWindow.Close();
                                            }
                                            finally
                                            {
                                                // 确保 IsLoading 状态被正确重置
                                                IsLoading = false;
                                            }
                                        };
                                        loginWindow.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
                                        break;
                                    }
                                }
                            }), System.Windows.Threading.DispatcherPriority.Normal);
                        };
                        
                        mainWindow.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                    }
                    catch (Exception ex)
                    {
                        // 如果主窗口显示失败，重置 IsLoading 状态并显示错误
                        IsLoading = false;
                        ClearFieldErrors();
                        ErrorMessage = $"打开主窗口失败：{ex.Message}";
                    }
                });
            }
            else
            {
                // 服务端错误处理
                var errorMessage = result.Message ?? "登录失败，请检查用户名和密码";
                
                // 显示登录失败消息（类型A：弹出自动消失提示框，10秒，顶端对齐）
                TaktMessageManager.ShowToastWindow(errorMessage, "错误", MessageBoxImage.Error, 10000);
                
                // 根据错误消息类型，显示在相应的字段级别
                if (errorMessage.Contains("用户名不存在") || errorMessage.Contains("用户名错误"))
                {
                    // 用户名错误：只显示在用户名字段
                    UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameNotFound") ?? "该用户名不存在";
                    PasswordError = string.Empty;
                    ErrorMessage = string.Empty;
                }
                else if (errorMessage.Contains("密码不正确") || errorMessage.Contains("密码错误"))
                {
                    // 密码错误：只显示在密码字段
                    UsernameError = string.Empty;
                    PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordIncorrect") ?? "密码不正确";
                    ErrorMessage = string.Empty;
                }
                else
                {
                    // 其他服务端错误（如"用户已被禁用"、"用户未分配角色"等）：显示在统一错误消息区域
                    ClearFieldErrors();
                    ErrorMessage = errorMessage;
                }
                IsLoading = false;
            }
        }
        catch (Exception ex)
        {
            // 系统错误：显示类型A提示框（弹出自动消失提示框，10秒，顶端对齐）
            TaktMessageManager.ShowToastWindow($"登录失败：{ex.Message}", "错误", MessageBoxImage.Error, 10000);
            
            // 清除字段错误
            ClearFieldErrors();
            ErrorMessage = $"登录失败：{ex.Message}";
            IsLoading = false;
        }
    }

    /// <summary>
    /// 检查数据库连接状态
    /// </summary>
    /// <returns>如果连接可用返回 true，否则返回 false</returns>
    private async Task<bool> CheckDatabaseConnectionAsync()
    {
        try
        {
            // 获取数据库上下文
            var dbContext = App.Services?.GetService<Takt.Infrastructure.Data.DbContext>();
            if (dbContext == null)
            {
                TaktMessageBox.Error(
                    _localizationManager.GetString("Database.ConnectionError.ServiceNotFound") ?? "无法获取数据库服务",
                    _localizationManager.GetString("Common.Error") ?? "错误");
                return false;
            }

            // 异步检查数据库连接（设置超时时间 5 秒）
            var checkTask = dbContext.CheckConnectionAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            var completedTask = await Task.WhenAny(checkTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // 超时
                TaktMessageBox.Warning(
                    _localizationManager.GetString("Database.ConnectionError.Timeout") ?? "数据库连接超时，请检查网络连接和数据库服务器状态",
                    _localizationManager.GetString("Common.Error") ?? "错误");
                return false;
            }

            var isConnected = await checkTask;
            if (!isConnected)
            {
                // 连接失败
                TaktMessageBox.Error(
                    _localizationManager.GetString("Database.ConnectionError.Failed") ?? "无法连接到数据库，请检查数据库连接配置和服务器状态",
                    _localizationManager.GetString("Common.Error") ?? "错误");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // 异常处理
            var errorMessage = _localizationManager.GetString("Database.ConnectionError.Exception") 
                ?? "数据库连接检查时发生异常";
            TaktMessageBox.Error(
                $"{errorMessage}\n\n{ex.Message}",
                _localizationManager.GetString("Common.Error") ?? "错误");
            return false;
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
            var userContext = Takt.Common.Context.UserContext.AddUser(loginResult.UserId);
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
            Takt.Common.Context.UserContext.SetCurrent(loginResult.UserId);
        }
        
        await Task.CompletedTask;
    }

    partial void OnUsernameChanged(string value)
    {
        // 清除用户名错误（当用户开始输入时）
        if (!string.IsNullOrWhiteSpace(value))
        {
            UsernameError = string.Empty;
        }
        // LoginCommand 会自动更新 CanExecute，无需手动调用
        // LoginCommand?.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        // 清除密码错误（当用户开始输入时）
        if (!string.IsNullOrWhiteSpace(value))
        {
            PasswordError = string.Empty;
        }
        // LoginCommand 会自动更新 CanExecute，无需手动调用
        // LoginCommand?.NotifyCanExecuteChanged();
    }

}