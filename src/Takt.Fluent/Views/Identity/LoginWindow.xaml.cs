//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : LoginWindow.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 登录窗口代码后台（参照WPFGallery实现）
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FontAwesome.Sharp;
using Takt.Fluent.Services;
using Takt.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Takt.Domain.Interfaces;
using Takt.Application.Services.Routine;
using Takt.Application.Dtos.Routine;
using Takt.Fluent.Controls;

namespace Takt.Fluent.Views.Identity;

/// <summary>
/// 登录窗口
/// </summary>
public partial class LoginWindow : Window
{
    private readonly ThemeService _themeService;
    private readonly ILocalizationManager _localizationManager;
    private readonly ILanguageService _languageService;
    private List<LanguageOptionDto> _availableLanguages = new();

    public LoginWindow()
    {
        InitializeComponent();
        
        // 检查 Services 是否可用
        if (App.Services == null)
        {
            throw new InvalidOperationException("App.Services 为 null，无法获取服务。请确保 Host 已正确启动。");
        }
        
        try
        {
            // 获取主题服务和本地化管理器
            _themeService = App.Services.GetRequiredService<ThemeService>();
            _localizationManager = App.Services.GetRequiredService<ILocalizationManager>();
            _languageService = App.Services.GetRequiredService<ILanguageService>();
            
            // 订阅主题变化事件
            _themeService.ThemeChanged += OnThemeChanged;
            
            // 订阅语言切换事件
            _localizationManager.LanguageChanged += OnLanguageChanged;
            
            // 初始化主题选择
            InitializeThemeComboBox();
            
            // 通过依赖注入获取ViewModel
            var viewModel = App.Services.GetRequiredService<ViewModels.Identity.LoginViewModel>();
            DataContext = viewModel;
            
            // 初始化语言选择（使用默认值，不等待数据库）
            UpdateLanguageIcon();
            
            // 初始化语言菜单（不等待数据，数据加载完成后会自动更新）
            InitializeLanguageMenu();
            
            // 绑定完成后，同步密码框的值，并更新主题图标提示
            Loaded += (s, e) =>
            {
                var passwordBox = FindName("PasswordBox") as PasswordBox;
                if (passwordBox != null && viewModel.Password != null)
                {
                    passwordBox.Password = viewModel.Password;
                }
            
                // 确保主题图标和提示在窗口加载后更新
                UpdateThemeIcon(_themeService.GetAppliedThemeMode());
                
                // 主题：优先配置文件，否则默认深色（同步操作，很快）
                var savedTheme = AppSettingsHelper.GetTheme();
                var themeMode = ParseTheme(savedTheme);
                _themeService.SetTheme(themeMode);
                var appliedTheme = _themeService.GetAppliedThemeMode();
                UpdateThemeIcon(appliedTheme);
                UpdateBrandAreaBackground(appliedTheme);

                // 语言：优先配置文件，否则按系统语言映射（同步操作，很快）
                // 注意：翻译已在 App.xaml.cs 中预加载，这里只需要应用语言设置
                var savedLang = AppSettingsHelper.GetLanguage();
                var systemLang = SystemInfoHelper.GetSystemLanguageCode();
                var initLang = string.IsNullOrWhiteSpace(savedLang) ? MapSystemLanguage(systemLang) : savedLang;
                if (!string.IsNullOrWhiteSpace(initLang) && _localizationManager.CurrentLanguage != initLang)
                {
                    // 如果当前语言与配置不一致，切换语言（翻译已预加载，切换很快）
                    _localizationManager.ChangeLanguage(initLang);
                }

                UpdateLanguageIcon();
                UpdateTranslations();
                
                // 检查数据库连接状态
                _ = CheckDatabaseConnectionAsync();
                
                // 在后台异步加载语言列表（用于语言选择菜单），不阻塞 UI
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // 只加载语言列表（翻译已在启动时预加载）
                        await LoadAvailableLanguagesAsync().ConfigureAwait(false);
                        
                        // 回到 UI 线程更新界面
                        Dispatcher.Invoke(() =>
                        {
                            UpdateLanguageMenuItems();
                            UpdateLanguageIcon();
                        });
                    }
                    catch (Exception ex)
                    {
                        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
                        operLog?.Error(ex, "[登录] 后台加载语言列表失败");
                    }
                });
            };

        Closed += LoginWindow_Closed;
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[登录] LoginWindow 初始化失败");
            throw;
        }
    }

    private void LoginWindow_Closed(object? sender, EventArgs e)
    {
        _themeService.ThemeChanged -= OnThemeChanged;
        _localizationManager.LanguageChanged -= OnLanguageChanged;
    }
    
    /// <summary>
    /// 加载可用语言列表
    /// </summary>
    private async Task LoadAvailableLanguagesAsync()
    {
        try
        {
            var result = await _languageService.OptionAsync(false);
            if (result.Success && result.Data != null)
            {
                _availableLanguages = result.Data;
                Dispatcher.Invoke(() =>
                {
                    UpdateLanguageMenuItems();
                });
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[登录] 加载可用语言列表失败");
        }
    }

    /// <summary>
    /// 初始化主题图标
    /// </summary>
    private void InitializeThemeComboBox()
    {
        UpdateThemeIcon(_themeService.GetAppliedThemeMode());
    }

    /// <summary>
    /// 更新主题图标显示
    /// </summary>
    private void UpdateThemeIcon(System.Windows.ThemeMode currentTheme)
    {
        // 使用 FontAwesome 图标：Sun - 浅色主题, Moon - 深色主题, Palette - 跟随系统
        if (currentTheme == System.Windows.ThemeMode.Light)
        {
            ThemeIcon.Icon = IconChar.Sun; // 太阳图标 - 浅色主题
            ThemeButton.ToolTip = "浅色主题（点击切换到深色）";
        }
        else if (currentTheme == System.Windows.ThemeMode.Dark)
        {
            ThemeIcon.Icon = IconChar.Moon; // 月亮图标 - 深色主题
            ThemeButton.ToolTip = "深色主题（点击切换到系统）";
        }
        else
        {
            ThemeIcon.Icon = IconChar.Palette; // 调色板图标 - 跟随系统
            ThemeButton.ToolTip = "跟随系统（点击切换到浅色）";
        }
    }

    /// <summary>
    /// 主题变化事件处理
    /// </summary>
    private void OnThemeChanged(object? sender, System.Windows.ThemeMode e)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateBrandAreaBackground(e);
            UpdateThemeIcon(e);
        });
    }

    /// <summary>
    /// 主题按钮点击事件 - 循环切换主题
    /// </summary>
    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentTheme = _themeService.GetCurrentTheme();
            
            // 如果当前主题是 None，默认使用 System
            if (currentTheme == System.Windows.ThemeMode.None)
            {
                currentTheme = System.Windows.ThemeMode.System;
            }
            
            System.Windows.ThemeMode nextTheme;
            
            if (currentTheme == System.Windows.ThemeMode.Light)
            {
                nextTheme = System.Windows.ThemeMode.Dark;
            }
            else if (currentTheme == System.Windows.ThemeMode.Dark)
            {
                nextTheme = System.Windows.ThemeMode.System;
            }
            else
            {
                nextTheme = System.Windows.ThemeMode.Light;
            }

            // 设置主题（ThemeService.SetTheme 已经包含了保存逻辑，不需要再调用 LocalConfigHelper）
            _themeService.SetTheme(nextTheme);
            
            // 立即更新 UI（不等待，确保立即响应）
            var appliedTheme = _themeService.GetAppliedThemeMode();
            UpdateThemeIcon(appliedTheme);
            UpdateBrandAreaBackground(appliedTheme);
            
            // 更新主题提示文本
            if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
            {
                viewModel.ThemeToolTip = _localizationManager.GetString("theme.switch") ?? "切换主题";
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[登录] 主题切换失败");
        }
    }

    /// <summary>
    /// 更新品牌展示区和登录表单背景色（根据主题）
    /// </summary>
    private void UpdateBrandAreaBackground(System.Windows.ThemeMode appliedTheme)
    {
        var layerBrush = TryFindResource("LayerFillColorDefaultBrush") as SolidColorBrush;
        if (layerBrush == null)
        {
            layerBrush = new SolidColorBrush(Colors.Transparent);
        }

        if (BrandAreaBorder != null)
        {
            BrandAreaBorder.Background = layerBrush;
        }

        if (LoginFormBorder != null)
        {
            LoginFormBorder.Background = layerBrush;
        }
    }

    private static string MapSystemLanguage(string systemCode)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return "zh-CN";
        var s = systemCode.ToLowerInvariant();
        if (s.StartsWith("zh")) return "zh-CN";
        if (s.StartsWith("ja")) return "ja-JP";
        return "en-US";
    }

    private static System.Windows.ThemeMode ParseTheme(string? stored)
    {
        return stored switch
        {
            "Light" => System.Windows.ThemeMode.Light,
            "System" => System.Windows.ThemeMode.System,
            _ => System.Windows.ThemeMode.Dark
        };
    }


    /// <summary>
    /// 初始化语言菜单（不等待数据加载，数据加载完成后会自动更新）
    /// </summary>
    private void InitializeLanguageMenu()
    {
        // 如果数据已加载，立即更新菜单
        if (_availableLanguages.Count > 0)
        {
            UpdateLanguageMenuItems();
        }
        // 否则等待后台加载完成后自动更新（在 Loaded 事件中处理）
    }
    
    private void LanguageButton_Click(object sender, RoutedEventArgs e)
    {
        if (LanguageContextMenu == null) return;
        
        if (sender is Button button)
        {
            LanguageContextMenu.PlacementTarget = button;
        }
        LanguageContextMenu.Placement = PlacementMode.Bottom;
        LanguageContextMenu.IsOpen = true;
    }
    
    /// <summary>
    /// 更新语言菜单项
    /// </summary>
    private void UpdateLanguageMenuItems()
    {
        if (LanguageContextMenu == null) return;
        
        LanguageContextMenu.ItemsSource = null;
        LanguageContextMenu.ItemsSource = _availableLanguages;
        
        UpdateMenuItemsCheckedState();
    }
    
    /// <summary>
    /// 更新菜单项的选中状态
    /// </summary>
    private void UpdateMenuItemsCheckedState()
    {
        if (LanguageContextMenu == null) return;
        
        var currentLang = _localizationManager.CurrentLanguage;
        
        foreach (var language in _availableLanguages)
        {
            var menuItem = LanguageContextMenu.ItemContainerGenerator.ContainerFromItem(language) as System.Windows.Controls.MenuItem;
            if (menuItem != null)
            {
                menuItem.IsChecked = language.Code == currentLang;
            }
        }
    }
    
    /// <summary>
    /// 语言菜单子菜单打开事件
    /// </summary>
    private async void LanguageContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (_availableLanguages.Count == 0)
        {
            await LoadAvailableLanguagesAsync();
        }
        
        // 确保在 UI 线程上更新菜单项
        Dispatcher.Invoke(() =>
        {
            // 强制刷新菜单项，确保子菜单打开时能看到项
            UpdateLanguageMenuItems();
            
            // 延迟一点确保容器已生成，然后更新选中状态
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateMenuItemsCheckedState();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        });
    }
    
    /// <summary>
    /// 语言菜单项点击事件
    /// </summary>
    private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem menuItem && menuItem.DataContext is Takt.Application.Dtos.Routine.LanguageOptionDto language)
        {
            _localizationManager.ChangeLanguage(language.Code);
            AppSettingsHelper.SaveLanguage(language.Code);
            UpdateLanguageIcon();
            UpdateTranslations();
            UpdateLanguageMenuItems(); // 更新选中状态
            if (LanguageContextMenu != null)
            {
                LanguageContextMenu.IsOpen = false;
            }
        }
    }

    /// <summary>
    /// 更新语言图标显示
    /// </summary>
    private void UpdateLanguageIcon()
    {
        if (LanguageIcon == null || LanguageButton == null) return;
        
        var currentLang = _localizationManager.CurrentLanguage;
        // 语言图标使用 FontAwesome 的 Globe 图标（地球图标，表示国际化/语言切换）
        LanguageIcon.Icon = IconChar.Globe;
        LanguageIcon.Visibility = Visibility.Visible;
        
        // 更新工具提示
        var toolTip = _localizationManager.GetString("language.switch") ?? "切换语言";
        var currentLanguage = _availableLanguages.FirstOrDefault(l => l.Code == currentLang);
        string toolTipText;
        if (currentLanguage != null)
        {
            toolTipText = $"{toolTip} ({currentLanguage.Name})";
        }
        else
        {
            toolTipText = $"{toolTip} ({currentLang})";
        }
        LanguageButton.ToolTip = toolTipText;
        
        // 更新菜单项的选中状态
        UpdateLanguageMenuItems();
        
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.LanguageToolTip = toolTip;
        }
    }

    /// <summary>
    /// 语言切换事件处理
    /// </summary>
    private void OnLanguageChanged(object? sender, string languageCode)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateLanguageIcon();
            UpdateTranslations();
        });
    }

    /// <summary>
    /// 更新所有翻译文本
    /// </summary>
    private void UpdateTranslations()
    {
        // 窗口标题（其余文本已用 XAML 标记扩展绑定）
        this.Title = _localizationManager.GetString("Login.Title") ?? string.Empty;

        // 密码占位
        // 工具提示（语言/主题）
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.LanguageToolTip = _localizationManager.GetString("language.switch") ?? string.Empty;
            viewModel.ThemeToolTip = _localizationManager.GetString("theme.switch") ?? string.Empty;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
        
        // Material Design 密码框自动处理占位提示，无需手动更新
    }

    /// <summary>
    /// 检查数据库连接状态
    /// </summary>
    private async Task CheckDatabaseConnectionAsync()
    {
        try
        {
            // 获取数据库上下文
            var dbContext = App.Services?.GetService<Takt.Infrastructure.Data.DbContext>();
            if (dbContext == null)
            {
                Dispatcher.Invoke(() =>
                {
                    TaktMessageBox.Error(
                        _localizationManager.GetString("Database.ConnectionError.ServiceNotFound") ?? "无法获取数据库服务",
                        _localizationManager.GetString("Common.Error") ?? "错误",
                        this);
                });
                return;
            }

            // 异步检查数据库连接（设置超时时间 5 秒）
            var checkTask = dbContext.CheckConnectionAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            var completedTask = await Task.WhenAny(checkTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // 超时
                Dispatcher.Invoke(() =>
                {
                    TaktMessageBox.Warning(
                        _localizationManager.GetString("Database.ConnectionError.Timeout") ?? "数据库连接超时，请检查网络连接和数据库服务器状态",
                        _localizationManager.GetString("Common.Error") ?? "错误",
                        this);
                });
                return;
            }

            var isConnected = await checkTask;
            if (!isConnected)
            {
                // 连接失败
                Dispatcher.Invoke(() =>
                {
                    TaktMessageBox.Error(
                        _localizationManager.GetString("Database.ConnectionError.Failed") ?? "无法连接到数据库，请检查数据库连接配置和服务器状态",
                        _localizationManager.GetString("Common.Error") ?? "错误",
                        this);
                });
            }
        }
        catch (Exception ex)
        {
            // 异常处理
            Dispatcher.Invoke(() =>
            {
                var errorMessage = _localizationManager.GetString("Database.ConnectionError.Exception") 
                    ?? "数据库连接检查时发生异常";
                TaktMessageBox.Error(
                    $"{errorMessage}\n\n{ex.Message}",
                    _localizationManager.GetString("Common.Error") ?? "错误",
                    this);
            });
        }
    }
}
#pragma warning restore WPF0001