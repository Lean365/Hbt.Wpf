//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : LoginWindow.xaml.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 登录窗口代码后台（参照WPFGallery实现）
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FontAwesome.Sharp;
using Hbt.Fluent.Services;
using Hbt.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Hbt.Fluent.Views.Identity;

/// <summary>
/// 登录窗口
/// </summary>
public partial class LoginWindow : Window
{
    private readonly ThemeService _themeService;
    private readonly LanguageService _languageService;

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
            // 获取主题服务和语言服务
            _themeService = App.Services.GetRequiredService<ThemeService>();
            _languageService = App.Services.GetRequiredService<LanguageService>();
            
            // 初始化语言服务
            _ = _languageService.InitializeAsync();
            
            // 订阅语言切换事件
            _languageService.LanguageChanged += OnLanguageChanged;
            
            // 初始化主题选择
            InitializeThemeComboBox();
            
            // 通过依赖注入获取ViewModel
            var viewModel = App.Services.GetRequiredService<ViewModels.Identity.LoginViewModel>();
            DataContext = viewModel;
            
            // 初始化语言菜单
            InitializeLanguageMenu();
            
            // 初始化语言选择（在菜单初始化后）
            UpdateLanguageIcon();
            
            // 绑定完成后，同步密码框的值，并更新主题图标提示
            Loaded += async (s, e) =>
            {
                if (viewModel.Password != null)
                {
                    PasswordBox.Password = viewModel.Password;
                }
            
            // 注册模板中的占位符以便访问
            if (PasswordBox.Template != null)
            {
                var placeholder = PasswordBox.Template.FindName("PasswordPlaceholder", PasswordBox) as System.Windows.Controls.TextBlock;
                if (placeholder != null)
                {
                    RegisterName("PasswordPlaceholder", placeholder);
                }
            }
            
            // 初始化密码框占位符状态
            UpdatePasswordPlaceholder();
            
            // 确保主题图标和提示在窗口加载后更新
            UpdateThemeIcon();
            
            // 初始化语言服务（如果还没有初始化）
            if (_languageService.AvailableLanguages.Count == 0)
            {
                await _languageService.InitializeAsync();
            }
            
            // 语言：优先本地配置，否则按系统语言映射
            var savedLang = LocalConfigHelper.GetLanguage();
            var systemLang = SystemInfoHelper.GetSystemLanguageCode();
            var initLang = string.IsNullOrWhiteSpace(savedLang) ? MapSystemLanguage(systemLang) : savedLang;
            if (!string.IsNullOrWhiteSpace(initLang))
            {
                await _languageService.SetLanguageAsync(initLang);
            }

            // 主题：优先本地配置，否则默认深色
            var savedTheme = LocalConfigHelper.GetTheme();
            var themeMode = ParseTheme(savedTheme);
            _themeService.SetTheme(themeMode);
            UpdateThemeIcon();

            UpdateLanguageIcon();
            UpdateTranslations();
        };
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[登录] LoginWindow 初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 初始化主题图标
    /// </summary>
    private void InitializeThemeComboBox()
    {
        UpdateThemeIcon();
    }

    /// <summary>
    /// 更新主题图标显示
    /// </summary>
    private void UpdateThemeIcon()
    {
        var currentTheme = _themeService.GetCurrentTheme();
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
    /// 主题按钮点击事件 - 循环切换主题
    /// </summary>
    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        var currentTheme = _themeService.GetCurrentTheme();
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

        _themeService.SetTheme(nextTheme);
        LocalConfigHelper.SaveTheme(ThemeToString(nextTheme));
        UpdateThemeIcon();
        
        // 更新主题提示文本
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.ThemeToolTip = _languageService.GetTranslation("theme.switch", "切换主题");
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

    private static string ThemeToString(System.Windows.ThemeMode mode)
    {
        if (mode == System.Windows.ThemeMode.Light) return "Light";
        if (mode == System.Windows.ThemeMode.System) return "System";
        return "Dark";
    }

    /// <summary>
    /// 初始化语言菜单
    /// </summary>
    private async void InitializeLanguageMenu()
    {
        if (_languageService.AvailableLanguages.Count == 0)
        {
            await _languageService.InitializeAsync();
        }
        
        // 确保在 UI 线程上更新菜单项
        Dispatcher.Invoke(() =>
        {
            UpdateLanguageMenuItems();
        });
    }
    
    /// <summary>
    /// 更新语言菜单项
    /// </summary>
    private void UpdateLanguageMenuItems()
    {
        if (LanguageMenuItem == null) return;
        
        var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
        operLog?.Debug("[登录] 更新语言菜单项，可用语言数量: {Count}", _languageService.AvailableLanguages.Count);
        
        // 设置 ItemsSource 进行数据绑定
        LanguageMenuItem.ItemsSource = null; // 先清空
        LanguageMenuItem.ItemsSource = _languageService.AvailableLanguages;
        
        operLog?.Debug("[登录] ItemsSource 设置完成，Items.Count: {Count}", LanguageMenuItem.Items.Count);
        foreach (var item in LanguageMenuItem.Items)
        {
            if (item is Hbt.Application.Dtos.Routine.LanguageOptionDto lang)
            {
                operLog?.Debug("[登录] 菜单项: {Name} (Code: {Code})", lang.Name, lang.Code);
            }
        }
    }
    
    /// <summary>
    /// 更新菜单项的选中状态
    /// </summary>
    private void UpdateMenuItemsCheckedState()
    {
        if (LanguageMenuItem == null) return;
        
        var currentLang = _languageService.CurrentLanguageCode;
        var operLog = App.Services?.GetService<Hbt.Common.Logging.OperLogManager>();
        operLog?.Debug("[登录] 更新选中状态，当前语言: {CurrentLang}", currentLang);
        
        foreach (var language in _languageService.AvailableLanguages)
        {
            var menuItem = LanguageMenuItem.ItemContainerGenerator.ContainerFromItem(language) as MenuItem;
            if (menuItem != null)
            {
                menuItem.IsChecked = language.Code == currentLang;
                operLog?.Debug("[登录] 设置 {Name} 选中状态: {IsChecked}", language.Name, menuItem.IsChecked);
            }
            else
            {
                operLog?.Debug("[登录] 未找到 {Name} 的容器", language.Name);
            }
        }
    }
    
    /// <summary>
    /// 语言菜单子菜单打开事件
    /// </summary>
    private async void LanguageMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        if (_languageService.AvailableLanguages.Count == 0)
        {
            await _languageService.InitializeAsync();
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
    private async void LanguageMenuItem_ItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is Hbt.Application.Dtos.Routine.LanguageOptionDto language)
        {
            await _languageService.SetLanguageAsync(language.Code);
            LocalConfigHelper.SaveLanguage(language.Code);
            UpdateLanguageIcon();
            UpdateTranslations();
            UpdateLanguageMenuItems(); // 更新选中状态
        }
    }

    /// <summary>
    /// 更新语言图标显示
    /// </summary>
    private void UpdateLanguageIcon()
    {
        if (LanguageMenuItem?.Header is not IconBlock languageIcon) return;
        
        var currentLang = _languageService.CurrentLanguageCode;
        // 语言图标使用 FontAwesome 的 Globe 图标（地球图标，表示国际化/语言切换）
        languageIcon.Icon = IconChar.Globe;
        languageIcon.Visibility = Visibility.Visible;
        
        // 更新工具提示
        var toolTip = _languageService.GetTranslation("language.switch", "切换语言");
        var currentLanguage = _languageService.AvailableLanguages.FirstOrDefault(l => l.Code == currentLang);
        if (currentLanguage != null)
        {
            if (LanguageMenuItem != null) LanguageMenuItem.ToolTip = $"{toolTip} ({currentLanguage.Name})";
        }
        else
        {
            if (LanguageMenuItem != null) LanguageMenuItem.ToolTip = $"{toolTip} ({currentLang})";
        }
        
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
        this.Title = _languageService.GetTranslation("Login.Title", string.Empty);

        // 密码占位
        {
            var passwordPlaceholder = FindName("PasswordPlaceholder") as System.Windows.Controls.TextBlock
                ?? PasswordBox.Template?.FindName("PasswordPlaceholder", PasswordBox) as System.Windows.Controls.TextBlock;
            if (passwordPlaceholder != null)
            {
                passwordPlaceholder.Text = _languageService.GetTranslation("Login.PasswordPlaceholder", string.Empty);
            }
        }

        // 工具提示（语言/主题）
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.LanguageToolTip = _languageService.GetTranslation("language.switch", string.Empty);
            viewModel.ThemeToolTip = _languageService.GetTranslation("theme.switch", string.Empty);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
        
        // 更新占位符显示状态
        UpdatePasswordPlaceholder();
    }
    
    private void UpdatePasswordPlaceholder()
    {
        if (PasswordBox == null) return;
        
        // 通过注册的名称查找占位符，如果找不到则从模板查找
        var placeholder = FindName("PasswordPlaceholder") as System.Windows.Controls.TextBlock
            ?? PasswordBox.Template?.FindName("PasswordPlaceholder", PasswordBox) as System.Windows.Controls.TextBlock;
            
        if (placeholder != null)
        {
            // 逻辑：如果有密码内容，始终隐藏占位符；如果没有密码，有焦点时隐藏，无焦点时显示
            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                // 有文本时，永远隐藏占位符
                placeholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                // 无文本时，根据焦点状态：有焦点隐藏，无焦点显示
                placeholder.Visibility = PasswordBox.IsFocused 
                    ? Visibility.Collapsed 
                    : Visibility.Visible;
            }
        }
    }
    
    private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // 获取焦点时，如果有密码则隐藏占位符，如果没有密码也隐藏占位符（允许输入）
        UpdatePasswordPlaceholder();
    }
    
    private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // 失去焦点时，根据密码内容决定是否显示占位符
        UpdatePasswordPlaceholder();
    }
}
#pragma warning restore WPF0001