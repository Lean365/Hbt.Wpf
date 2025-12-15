//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : LoginViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 登录窗体视图模型（参照WPFGallery实现，使用CommunityToolkit.Mvvm）
//===================================================================

using System.Media;
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
using Takt.Fluent.Services;
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
    private bool _showProgressBar = false;

    [ObservableProperty]
    private double _progressValue = 0.0;

    [ObservableProperty]
    private string _elapsedTime = "0.0s";

    private System.Threading.CancellationTokenSource? _progressCancellationTokenSource;
    private DateTime _progressStartTime;
    private System.Windows.Threading.DispatcherTimer? _progressTimer;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _usernameError = string.Empty;

    [ObservableProperty]
    private string _passwordError = string.Empty;

    [ObservableProperty]
    private string _usernameHint = string.Empty;

    [ObservableProperty]
    private string _passwordHint = string.Empty;

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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isInitializing = true;

    [ObservableProperty]
    private string _initializationMessage = string.Empty;

    public LoginViewModel(ILoginService loginService, ILocalizationManager localizationManager)
    {
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        // 初始化本地化提示文本（使用拼接方式）
        LanguageToolTip = _localizationManager.GetString("common.button.change") + _localizationManager.GetString("common.language.title");
        ThemeToolTip = _localizationManager.GetString("common.button.change") + _localizationManager.GetString("common.theme.title");
        UsernameHint = _localizationManager.GetString("login.username");
        PasswordHint = _localizationManager.GetString("login.password");

        // 订阅初始化状态变化事件
        InitializationStatusManager.StatusChanged += OnInitializationStatusChanged;
        
        // 初始化当前状态
        UpdateInitializationStatus();

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

    /// <summary>
    /// 初始化状态变化事件处理
    /// </summary>
    private void OnInitializationStatusChanged(object? sender, InitializationStatusChangedEventArgs e)
    {
        // 在 UI 线程更新状态
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateInitializationStatus();
        });
    }

    /// <summary>
    /// 更新初始化状态
    /// </summary>
    private void UpdateInitializationStatus()
    {
        var status = InitializationStatusManager.Status;
        var message = InitializationStatusManager.StatusMessage;

        // 只有在 Completed 状态时才允许登录，其他状态（NotStarted 或 Initializing）都禁用登录
        IsInitializing = status != InitializationStatus.Completed;
        
        // 根据状态设置消息
        if (string.IsNullOrWhiteSpace(message))
        {
            switch (status)
            {
                case InitializationStatus.NotStarted:
                case InitializationStatus.Initializing:
                    InitializationMessage = _localizationManager.GetString("login.initialization.inprogress");
                    break;
                case InitializationStatus.Completed:
                    InitializationMessage = _localizationManager.GetString("login.initialization.completed");
                    break;
            }
        }
        else
        {
            InitializationMessage = message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        try
        {
            // 命令入口日志
            try
            {
                if (App.Services != null)
                {
                    var operLog = App.Services.GetService<Takt.Common.Logging.OperLogManager>();
                    operLog?.Information("[Login] LoginCommand 调用，User='{Username}', HasPassword={HasPwd}", Username, !string.IsNullOrWhiteSpace(Password));
                }
            }
            catch { /* 忽略日志获取异常，避免影响登录流程 */ }

            IsLoading = true;
            ClearAllErrors();

            // 先检查数据库连接
            if (!await CheckDatabaseConnectionAsync())
            {
                IsLoading = false;
                ShowProgressBar = false;
                ProgressValue = 0.0;
                ElapsedTime = "0.0s";
                return;
            }

            // 验证输入
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = _localizationManager.GetString("identity.user.validation.usernamerequired");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = _localizationManager.GetString("identity.user.validation.passwordrequired");
                isValid = false;
            }

            if (!isValid)
            {
                IsLoading = false;
                ShowProgressBar = false;
                ProgressValue = 0.0;
                ElapsedTime = "0.0s";
                return;
            }

            // 记录开始时间（在任何UI操作之前）
            _progressStartTime = DateTime.Now;

            // 触发进度条显示（在UI线程执行，但不阻塞）
            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Render,
                new Action(() =>
                {
                    ShowProgressBar = true;
                    ProgressValue = 1.0; // 从1%开始
                    ElapsedTime = "0.0s";
                }));

            // 启动进度条动画（从1%到100%）
            _progressCancellationTokenSource = new System.Threading.CancellationTokenSource();
            StartProgressAnimation();

            var loginDto = new LoginDto
            {
                Username = Username,
                Password = Password,
                RememberMe = RememberMe
            };

            var result = await _loginService.LoginAsync(loginDto);

            // 停止进度动画
            StopProgressAnimation();
            
            // 确保进度条平滑过渡到100%
            var finalElapsed = (DateTime.Now - _progressStartTime).TotalSeconds;
            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Render,
                new Action(() =>
                {
                    // 如果还没到100%，使用动画平滑过渡
                    if (ProgressValue < 100.0)
                    {
                        AnimateTo100Percent(finalElapsed);
                    }
                    else
                    {
                        ProgressValue = 100.0;
                        ElapsedTime = $"{finalElapsed:F1}s";
                    }
                }));

            // 等待一小段时间让用户看到100%的完成状态
            await Task.Delay(300);

            if (result.Success && result.Data != null)
            {
                // 播放登录成功系统声音
                SystemSounds.Asterisk.Play();
                
                // 显示登录成功消息（类型A：弹出自动消失提示框，10秒，顶端对齐）
                var successMessage = result.Message ?? _localizationManager.GetString("login.success");
                var successTitle = _localizationManager.GetString("common.messagebox.information");
                TaktMessageManager.ShowToastWindow(successMessage, successTitle, MessageBoxImage.Information, 10000);
                
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
                        
                        // 先显示主窗口（使用淡入动画），然后在 Loaded 事件中居中
                        mainWindow.Opacity = 0;
                        mainWindow.Show();
                        
                        // 手动居中窗口（窗口已创建后，WindowStartupLocation 可能不生效）
                        // 使用 Loaded 事件确保窗口大小已确定
                        void CenterMainWindow(object? sender, RoutedEventArgs e)
                        {
                            mainWindow.Loaded -= CenterMainWindow;
                            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                            var windowWidth = double.IsNaN(mainWindow.Width) || mainWindow.Width == 0 
                                ? mainWindow.ActualWidth 
                                : mainWindow.Width;
                            var windowHeight = double.IsNaN(mainWindow.Height) || mainWindow.Height == 0 
                                ? mainWindow.ActualHeight 
                                : mainWindow.Height;
                            mainWindow.Left = (screenWidth - windowWidth) / 2;
                            mainWindow.Top = (screenHeight - windowHeight) / 2;
                        }
                        mainWindow.Loaded += CenterMainWindow;
                        
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
                                    if (window is Views.Identity.LoginView loginWindow)
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
                                                ShowProgressBar = false;
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
                        ShowProgressBar = false;
                        ClearFieldErrors();
                        ErrorMessage = $"打开主窗口失败：{ex.Message}";
                    }
                });
            }
            else
            {
                // 播放登录失败系统声音
                SystemSounds.Hand.Play();
                
                // 服务端错误处理
                var errorMessage = result.Message ?? _localizationManager.GetString("login.failed.default");
                
                // 显示登录失败消息（类型A：弹出自动消失提示框，10秒，顶端对齐）
                var errorTitle = _localizationManager.GetString("common.messagebox.error");
                TaktMessageManager.ShowToastWindow(errorMessage, errorTitle, MessageBoxImage.Error, 10000);
                
                // 根据错误消息类型，显示在相应的字段级别
                if (errorMessage.Contains("用户名不存在") || errorMessage.Contains("用户名错误"))
                {
                    // 用户名错误：只显示在用户名字段
                    UsernameError = _localizationManager.GetString("identity.user.validation.usernamenotfound");
                    PasswordError = string.Empty;
                    ErrorMessage = string.Empty;
                }
                else if (errorMessage.Contains("密码不正确") || errorMessage.Contains("密码错误"))
                {
                    // 密码错误：只显示在密码字段
                    UsernameError = string.Empty;
                    PasswordError = _localizationManager.GetString("identity.user.validation.passwordincorrect");
                    ErrorMessage = string.Empty;
                }
                else
                {
                    // 其他服务端错误（如"用户已被禁用"、"用户未分配角色"等）：显示在统一错误消息区域
                    ClearFieldErrors();
                    ErrorMessage = errorMessage;
                }
                IsLoading = false;
                ShowProgressBar = false;
                ProgressValue = 0.0;
                ElapsedTime = "0.0s";
            }
        }
        catch (Exception ex)
        {
            // 停止进度动画
            StopProgressAnimation();
            _progressCancellationTokenSource?.Cancel();
            
            // 播放登录失败系统声音
            SystemSounds.Hand.Play();
            
            // 系统错误：显示类型A提示框（弹出自动消失提示框，10秒，顶端对齐）
            var errorMessageFormat = _localizationManager.GetString("login.error");
            var errorMessage = string.Format(errorMessageFormat, ex.Message);
            var errorTitle = _localizationManager.GetString("common.messagebox.error");
            TaktMessageManager.ShowToastWindow(errorMessage, errorTitle, MessageBoxImage.Error, 10000);
            
            // 清除字段错误
            ClearFieldErrors();
            ErrorMessage = errorMessage;
            IsLoading = false;
            ShowProgressBar = false;
            ProgressValue = 0.0;
            ElapsedTime = "0.0s";
        }
        finally
        {
            StopProgressAnimation();
            _progressCancellationTokenSource?.Dispose();
            _progressCancellationTokenSource = null;
        }
    }

    /// <summary>
    /// 启动进度条动画（使用 DispatcherTimer 实现流畅的60fps动画）
    /// </summary>
    private void StartProgressAnimation()
    {
        // 停止之前的动画（如果存在）
        StopProgressAnimation();

        // 创建定时器，每16ms更新一次（约60fps，流畅动画）
        _progressTimer = new System.Windows.Threading.DispatcherTimer(
            TimeSpan.FromMilliseconds(16), // 60fps
            System.Windows.Threading.DispatcherPriority.Render, // 使用 Render 优先级确保流畅
            OnProgressTimerTick,
            System.Windows.Application.Current.Dispatcher);

        _progressTimer.Start();
    }

    /// <summary>
    /// 进度条定时器事件处理（每16ms执行一次，约60fps）
    /// </summary>
    private void OnProgressTimerTick(object? sender, EventArgs e)
    {
        if (_progressCancellationTokenSource?.IsCancellationRequested == true)
        {
            StopProgressAnimation();
            return;
        }

        var elapsed = (DateTime.Now - _progressStartTime).TotalSeconds;
        
        // 进度条平滑增长：前0.5秒快速到30%，然后逐渐变慢
        // 最大到95%，等待实际登录完成后再跳到100%
        double targetProgress;
        if (elapsed < 0.5)
        {
            // 前0.5秒：从1%快速到30%（使用缓动）
            var t = elapsed / 0.5;
            var eased = EaseOutCubic(t);
            targetProgress = 1.0 + (30.0 - 1.0) * eased;
        }
        else if (elapsed < 2.0)
        {
            // 0.5-2秒：从30%到70%（平滑过渡）
            var t = (elapsed - 0.5) / 1.5;
            var eased = EaseInOutCubic(t);
            targetProgress = 30.0 + (70.0 - 30.0) * eased;
        }
        else
        {
            // 2秒后：从70%缓慢增长到95%（逐渐变慢，等待实际登录完成）
            var t = Math.Min((elapsed - 2.0) / 10.0, 1.0); // 最多10秒到达95%
            var eased = EaseInQuad(t);
            targetProgress = 70.0 + (95.0 - 70.0) * eased;
        }
        
        // 确保不超过95%（最后5%等待登录实际完成）
        targetProgress = Math.Max(1.0, Math.Min(95.0, targetProgress));

        // 更新UI（已在UI线程，直接更新）
        ProgressValue = targetProgress;
        ElapsedTime = $"{elapsed:F1}s";
    }

    /// <summary>
    /// 停止进度条动画
    /// </summary>
    private void StopProgressAnimation()
    {
        _progressTimer?.Stop();
        _progressTimer = null;
    }

    /// <summary>
    /// 平滑动画到100%（使用定时器平滑过渡）
    /// </summary>
    private void AnimateTo100Percent(double finalElapsed)
    {
        var currentValue = ProgressValue;
        if (currentValue >= 100.0)
        {
            ProgressValue = 100.0;
            ElapsedTime = $"{finalElapsed:F1}s";
            return;
        }

        // 使用定时器平滑过渡到100%（250ms，16ms间隔）
        var startValue = currentValue;
        var targetValue = 100.0;
        var duration = 250.0; // 250ms 平滑过渡
        
        var startTime = DateTime.Now;
        var finishTimer = new System.Windows.Threading.DispatcherTimer(
            System.Windows.Threading.DispatcherPriority.Render,
            System.Windows.Application.Current.Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        finishTimer.Tick += (s, e) =>
        {
            var elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
            if (elapsedMs >= duration)
            {
                ProgressValue = 100.0;
                ElapsedTime = $"{finalElapsed:F1}s";
                finishTimer.Stop();
                finishTimer = null;
                return;
            }

            // 使用缓动函数平滑过渡
            var t = Math.Min(elapsedMs / duration, 1.0);
            var eased = EaseOutCubic(t);
            var newValue = startValue + (targetValue - startValue) * eased;
            ProgressValue = Math.Min(100.0, newValue);
            ElapsedTime = $"{finalElapsed:F1}s";
        };

        finishTimer.Start();
    }

    /// <summary>
    /// 缓动函数：Ease Out Cubic（快速开始，慢速结束）
    /// </summary>
    private static double EaseOutCubic(double t)
    {
        t = Math.Max(0, Math.Min(1, t));
        return 1 - Math.Pow(1 - t, 3);
    }

    /// <summary>
    /// 缓动函数：Ease In Out Cubic（平滑的缓入缓出，最自然的动画效果）
    /// 开始时慢速，中间加速，结束时慢速，提供最流畅的动画体验
    /// </summary>
    private static double EaseInOutCubic(double t)
    {
        t = Math.Max(0, Math.Min(1, t));
        return t < 0.5
            ? 4 * t * t * t  // 前半段：三次方缓入
            : 1 - Math.Pow(-2 * t + 2, 3) / 2;  // 后半段：三次方缓出
    }

    /// <summary>
    /// 缓动函数：Ease In Quad（慢速开始，快速结束）
    /// </summary>
    private static double EaseInQuad(double t)
    {
        t = Math.Max(0, Math.Min(1, t));
        return t * t;
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
                    _localizationManager.GetString("database.connectionerror.servicenotfound"),
                    _localizationManager.GetString("common.messagebox.error"));
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
                    _localizationManager.GetString("database.connectionerror.timeout"),
                    _localizationManager.GetString("common.messagebox.warning"));
                return false;
            }

            var isConnected = await checkTask;
            if (!isConnected)
            {
                // 连接失败
                TaktMessageBox.Error(
                    _localizationManager.GetString("database.connectionerror.failed"),
                    _localizationManager.GetString("common.messagebox.error"));
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // 异常处理
            var errorMessage = _localizationManager.GetString("database.connectionerror.exception");
            TaktMessageBox.Error(
                $"{errorMessage}\n\n{ex.Message}",
                _localizationManager.GetString("common.messagebox.error"));
            return false;
        }
    }

    private bool CanLogin()
    {
        // 必须满足以下所有条件才能登录：
        // 1. 不在加载中
        // 2. 初始化已完成（IsInitializing = false）
        // 3. 用户名和密码均有值
        return !IsLoading && !IsInitializing && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }

    /// <summary>
    /// 保存用户会话
    /// </summary>
    private static async Task SaveUserSessionAsync(LoginResultDto loginResult)
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