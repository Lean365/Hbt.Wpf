// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Bootstrapper
// 文件名称：PrismBootstrapper.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：Prism Bootstrapper，负责应用程序的启动配置和依赖注入
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.DryIoc;
using Prism.Ioc;
using DryIoc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using Prism.Navigation.Regions.Behaviors;
using Serilog;
using System.Reflection;
using System.IO;
using System.Windows;
using Takt.Common.Config;
using Takt.Common.Logging;
using Takt.Fluent.Helpers;
using Takt.Fluent.Modules;
using Takt.Fluent.Services;
using Takt.Fluent.Extensions;
using Takt.Fluent.Controls;
using Takt.Fluent.ViewModels;
using Takt.Fluent.ViewModels.Identity;
using Takt.Fluent.ViewModels.Settings;
using Takt.Fluent.Views;
using Takt.Fluent.Views.About;
using Takt.Fluent.Views.Dashboard;
using Takt.Fluent.Views.Identity;
using Takt.Fluent.Views.Settings;
using Takt.Infrastructure.DependencyInjection;
using Takt.Domain.Interfaces;
using Takt.Domain.Repositories;
using Takt.Infrastructure.Repositories;
using Takt.Infrastructure.Data;
using System.Threading.Tasks;
using System.Linq;

namespace Takt.Fluent.Bootstrapper;

/// <summary>
/// Prism Bootstrapper，负责应用程序的启动配置
/// 使用 DryIoc 作为依赖注入容器
/// </summary>
public class PrismBootstrapper : PrismBootstrapperBase
{
    /// <summary>
    /// 创建容器扩展
    /// </summary>
    protected override IContainerExtension CreateContainerExtension()
    {
        return new DryIocContainerExtension();
    }

    /// <summary>
    /// 初始化（在 RegisterTypes 之前调用）
    /// Prism 执行顺序：Run() -> Initialize() -> RegisterTypes() -> CreateShell()
    /// 在这里确保资源已加载，并清除旧日志
    /// </summary>
    protected override void Initialize()
    {
        try
        {
            TimestampedDebug.Reset(); // 重置时间戳起始点
            TimestampedDebug.WriteLine("========== PrismBootstrapper.Initialize() 开始 ==========");
            App.StartupLogManager?.Information("PrismBootstrapper.Initialize() 开始执行");
            
            // Prism 已经在 Run() 中创建了 Application 实例，现在确保资源已加载
            var app = System.Windows.Application.Current as App;
            TimestampedDebug.WriteLine($"Application.Current 类型: {app?.GetType().Name ?? "null"}");
            
            // 注意：清除日志文件已在 App.InitializeStartupLogger() 中执行（在日志文件打开之前）
            
            if (app != null && (app.Resources == null || app.Resources.MergedDictionaries.Count == 0))
            {
                TimestampedDebug.WriteLineWarning("警告: 资源字典未加载，尝试强制加载");
                // 如果资源未加载，强制加载
                app.LoadAppXamlResources();
                
                // 验证资源
                var testResource = app.TryFindResource("BaseDefaultButtonStyleSmall");
                if (testResource == null)
                {
                    throw new InvalidOperationException(
                        "关键资源 'BaseDefaultButtonStyleSmall' 在 Initialize() 中未找到！\n" +
                        $"资源字典数量: {app.Resources?.MergedDictionaries?.Count ?? 0}");
                }
                TimestampedDebug.WriteLineSuccess("资源验证通过");
            }
            else
            {
                TimestampedDebug.WriteLineSuccess($"资源已加载，数量: {app?.Resources?.MergedDictionaries?.Count ?? 0}");
            }

            // 调用基类 Initialize() -> RegisterTypes() -> ConfigureModuleCatalog -> OnInitialized
            TimestampedDebug.WriteLine("准备调用 base.Initialize()...");
            App.StartupLogManager?.Information("准备调用 base.Initialize()（将依次调用 RegisterTypes、ConfigureModuleCatalog、OnInitialized）");
            
            try
            {
                base.Initialize();
                TimestampedDebug.WriteLineSuccess("base.Initialize() 完成");
                App.StartupLogManager?.Information("base.Initialize() 执行成功");
            }
            catch (Exception baseInitEx)
            {
                TimestampedDebug.WriteLineError($"base.Initialize() 失败: {baseInitEx.GetType().FullName ?? "Unknown"}: {baseInitEx.Message ?? string.Empty}");
                App.StartupLogManager?.Error(baseInitEx, "base.Initialize() 执行失败: {ExceptionType}: {Message}", 
                    baseInitEx.GetType().FullName ?? "Unknown", baseInitEx.Message ?? string.Empty);
                throw;
            }
        }
        catch (Exception ex)
        {
            TimestampedDebug.WriteLineError($"PrismBootstrapper.Initialize() 异常: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
            TimestampedDebug.WriteLine($"异常堆栈: {ex.StackTrace ?? string.Empty}");
            App.StartupLogManager?.Error(ex, "PrismBootstrapper.Initialize() 执行失败: {ExceptionType}: {Message}", 
                ex.GetType().Name, ex.Message ?? string.Empty);
            throw;
        }
        
        TimestampedDebug.WriteLine("========== PrismBootstrapper.Initialize() 完成 ==========");
        App.StartupLogManager?.Information("PrismBootstrapper.Initialize() 执行完成");
    }
    

    /// <summary>
    /// 创建 Shell（主窗口）
    /// 根据 Prism 官方示例：直接返回窗口，Prism 会自动显示它
    /// 注意：登录窗口在初始化完成前保持隐藏，初始化完成后再显示
    /// </summary>
    protected override DependencyObject CreateShell()
    {
        TimestampedDebug.WriteLine("========== CreateShell() 开始 ==========");
        TimestampedDebug.WriteLine("准备解析 LoginView...");
        var loginWindow = Container.Resolve<LoginView>();
        
        // 初始化完成前隐藏登录窗口
        loginWindow.Visibility = Visibility.Hidden;
        loginWindow.ShowInTaskbar = false;
        
        TimestampedDebug.WriteLine("LoginView 解析成功（已隐藏，等待初始化完成）");
        TimestampedDebug.WriteLine("========== CreateShell() 完成 ==========");
        
        return loginWindow;
    }

    /// <summary>
    /// 登录窗口引用（用于初始化完成后显示）
    /// </summary>
    private LoginView? _loginWindow;
    
    /// <summary>
    /// 初始化日志窗口引用
    /// </summary>
    private InitializationLogWindow? _initLogWindow;
    
    /// <summary>
    /// 初始化日志事件处理器引用（用于取消订阅）
    /// </summary>
    private EventHandler<string>? _initLogEventHandler;

    /// <summary>
    /// 初始化 Shell
    /// 根据 Prism 官方示例：InitializeShell 在 CreateShell 之后调用
    /// 这里设置登录窗口关闭后的处理逻辑
    /// </summary>
    protected override void InitializeShell(DependencyObject shell)
    {
        var loginWindow = shell as LoginView;
        if (loginWindow == null) return;
        
        // 保存登录窗口引用，用于初始化完成后显示
        _loginWindow = loginWindow;

        // 设置 Application.ShutdownMode，确保关闭登录窗口时不会退出应用
        System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // 创建 MainWindow（隐藏），等登录成功后再显示
        var mainWindow = Container.Resolve<MainWindow>();
        mainWindow.Visibility = Visibility.Hidden;
        mainWindow.WindowState = WindowState.Minimized;
        mainWindow.ShowInTaskbar = false;

        // 订阅登录窗口关闭事件
        loginWindow.Closed += (s, e) =>
        {
            var userContext = Takt.Common.Context.UserContext.Current;
            var isAuthenticated = userContext?.IsAuthenticated == true;

            if (isAuthenticated && mainWindow != null)
            {
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
                
                mainWindow.Visibility = Visibility.Visible;
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.ShowInTaskbar = true;
                mainWindow.Show();
                mainWindow.Activate();

                System.Windows.Application.Current.MainWindow = mainWindow;
                System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }
        };
    }



    /// <summary>
    /// 注册类型到容器（由 Prism 的 base.Initialize() 调用）
    /// 
    /// 注册顺序和原则：
    /// 1. 配置和基础设施（DryIoc 容器）
    /// 2. 应用层服务（直接在 DryIoc 中注册）
    /// 3. Fluent 层服务（ThemeService、NavigationService 等）
    /// 4. ViewModels
    /// 5. Views
    /// 
    /// 注意：
    /// - MaterialDesign 资源在 App.xaml 中定义，不需要在容器中注册
    /// - Prism 管理服务、ViewModel、View 的实例
    /// - MaterialDesign 提供 UI 样式、模板、资源
    /// - 两者互不相干，通过 WPF 资源系统协同工作
    /// </summary>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        try
        {
            TimestampedDebug.WriteLine("========== RegisterTypes() 开始 ==========");
            App.StartupLogManager?.Information("RegisterTypes() 开始执行");
            
        // ========== 第一阶段：配置和基础设施（DryIoc） ==========
            TimestampedDebug.WriteLine("第一阶段：配置和基础设施...");
        
        // 1.1 加载配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("未找到数据库连接字符串 'DefaultConnection'");

        var databaseSettings = configuration.GetSection("DatabaseSettings").Get<HbtDatabaseSettings>()
            ?? new HbtDatabaseSettings();

        // 1.2 将配置注册到 DryIoc 容器
        containerRegistry.RegisterInstance<IConfiguration>(configuration);

        // 1.3 注册所有基础设施层服务到 DryIoc 容器
        TimestampedDebug.WriteLine("1.3 注册基础设施层服务到 DryIoc...");
        RegisterInfrastructureServices(containerRegistry, connectionString, databaseSettings, configuration);
        TimestampedDebug.WriteLine("✓ 基础设施层服务注册完成");

        // ========== 第三阶段：Fluent 层服务（直接在 DryIoc 中注册） ==========
        
        // 3.1 主题服务（UI 层服务，不依赖基础设施）
        containerRegistry.RegisterSingleton<ThemeService>();

        // 3.2 本地化属性通知类（包装领域层的 ILocalizationManager）
        containerRegistry.RegisterSingleton<LocalizationNotifyProperty>(c =>
        {
            var localizationManager = c.Resolve<ILocalizationManager>();
            return new LocalizationNotifyProperty(localizationManager);
        });

        // 3.3 拖拽辅助线服务
        containerRegistry.RegisterSingleton<IDragDropService, DragDropService>();

        // ========== 第四阶段：创建 ServiceProvider（DryIoc） ==========
        // 注意：必须在注册 ViewModel/View 之前创建，因为它们可能需要通过 App.Services 获取服务
        // DryIoc 容器本身实现了 IServiceProvider
        var dryIocContainer = containerRegistry.GetContainer() as DryIoc.IContainer
            ?? throw new InvalidOperationException("无法获取 DryIoc 容器");
        var serviceProvider = dryIocContainer as IServiceProvider 
            ?? throw new InvalidOperationException("DryIoc 容器未实现 IServiceProvider");
        containerRegistry.RegisterInstance<IServiceProvider>(serviceProvider);
        
        // 设置 App.Services（使用反射，因为 set 是 private）
        var appType = typeof(App);
        var servicesProperty = appType.GetProperty("Services", BindingFlags.Public | BindingFlags.Static);
        if (servicesProperty != null && servicesProperty.SetMethod != null)
        {
            servicesProperty.SetValue(null, serviceProvider);
        }

        // ========== 第五阶段：注册需要 ServiceProvider 的服务 ==========
        
        try
        {
            TimestampedDebug.WriteLine("第五阶段：注册需要 ServiceProvider 的服务...");
            // 5.1 导航服务（需要 RegionManager 和 ServiceProvider）
            TimestampedDebug.WriteLine("5.1 解析 IRegionManager...");
            var regionManager = Container.Resolve<IRegionManager>();
            TimestampedDebug.WriteLine("✓ IRegionManager 解析成功");
            
            TimestampedDebug.WriteLine("5.2 从 ServiceProvider 获取 OperLogManager...");
            var operLog = serviceProvider.GetService<Takt.Common.Logging.OperLogManager>();
            TimestampedDebug.WriteLine($"✓ OperLogManager 获取成功: {(operLog != null ? "非null" : "null")}");
            
            TimestampedDebug.WriteLine("5.3 注册 INavigationService...");
            containerRegistry.RegisterSingleton<Takt.Fluent.Services.INavigationService>(c => 
                new Takt.Fluent.Services.NavigationService(regionManager, serviceProvider, Container, operLog));
            TimestampedDebug.WriteLine("✓ INavigationService 注册成功");
        }
        catch (Exception ex)
        {
            TimestampedDebug.WriteLine($"❌ 第五阶段失败: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
            throw new InvalidOperationException("第五阶段：注册需要 ServiceProvider 的服务失败", ex);
        }

        // ========== 第六阶段：注册 ViewModels ==========
        
        try
        {
            TimestampedDebug.WriteLine("第六阶段：注册 ViewModels...");
            // 6.1 主窗口 ViewModel
            TimestampedDebug.WriteLine("6.1 注册 MainWindowViewModel...");
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            TimestampedDebug.WriteLine("✓ MainWindowViewModel 注册成功");
            
            // 6.2 登录 ViewModel
            TimestampedDebug.WriteLine("6.2 注册 LoginViewModel...");
            containerRegistry.Register<LoginViewModel>();
            TimestampedDebug.WriteLine("✓ LoginViewModel 注册成功");
            
            // 6.3 导航页面 ViewModel
            TimestampedDebug.WriteLine("6.3 注册 NavigationPageViewModel...");
            containerRegistry.Register<NavigationPageViewModel>();
            TimestampedDebug.WriteLine("✓ NavigationPageViewModel 注册成功");
            
            // 6.4 Dashboard ViewModel
            TimestampedDebug.WriteLine("6.4 注册 DashboardViewModel...");
            containerRegistry.Register<DashboardViewModel>();
            TimestampedDebug.WriteLine("✓ DashboardViewModel 注册成功");
            
            // 6.5 Settings ViewModel
            TimestampedDebug.WriteLine("6.5 注册 MySettingsViewModel...");
            containerRegistry.Register<MySettingsViewModel>();
            TimestampedDebug.WriteLine("✓ MySettingsViewModel 注册成功");
        }
        catch (Exception ex)
        {
            TimestampedDebug.WriteLine($"❌ 第六阶段失败: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
            throw new InvalidOperationException("第六阶段：注册 ViewModels 失败", ex);
        }

        // ========== 第七阶段：注册 Views ==========
        
        try
        {
            TimestampedDebug.WriteLine("第七阶段：注册 Views...");
            // 7.1 主窗口（需要 MainWindowViewModel）
            TimestampedDebug.WriteLine("7.1 注册 MainWindow...");
            containerRegistry.RegisterSingleton<MainWindow>();
            TimestampedDebug.WriteLine("✓ MainWindow 注册成功");
            
            // 7.2 登录窗口（需要 ThemeService、ILocalizationManager、ILanguageService、LoginViewModel）
            TimestampedDebug.WriteLine("7.2 注册 LoginView...");
            containerRegistry.RegisterSingleton<LoginView>();
            TimestampedDebug.WriteLine("✓ LoginView 注册成功");
            
            // 7.3 Dashboard View
            TimestampedDebug.WriteLine("7.3 注册 DashboardView...");
            containerRegistry.Register<DashboardView>();
            TimestampedDebug.WriteLine("✓ DashboardView 注册成功");
            
            // 7.4 Settings View
            TimestampedDebug.WriteLine("7.4 注册 MySettingsView...");
            containerRegistry.Register<MySettingsView>();
            TimestampedDebug.WriteLine("✓ MySettingsView 注册成功");
            
            // 7.5 About View
            TimestampedDebug.WriteLine("7.5 注册 AboutView...");
            containerRegistry.Register<AboutView>();
            TimestampedDebug.WriteLine("✓ AboutView 注册成功");
            
            // 7.6 System View
            TimestampedDebug.WriteLine("7.6 注册 MySystemView...");
            containerRegistry.Register<MySystemView>();
            TimestampedDebug.WriteLine("✓ MySystemView 注册成功");
        }
        catch (Exception ex)
        {
            TimestampedDebug.WriteLine($"❌ 第七阶段失败: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
            throw new InvalidOperationException("第七阶段：注册 Views 失败", ex);
        }

        // ========== 第八阶段：初始化日志 ==========
        
        try
        {
            TimestampedDebug.WriteLine("第八阶段：初始化日志...");
            // 8.1 初始化 Serilog
            TimestampedDebug.WriteLine("8.1 初始化 Serilog...");
            InitializeSerilog(configuration);
            TimestampedDebug.WriteLine("✓ Serilog 初始化成功");

            // 8.2 数据库初始化改为后台异步执行，不阻塞 UI 启动
            // 数据库初始化将在 OnInitialized 之后异步执行
            TimestampedDebug.WriteLine("8.2 数据库初始化将在后台异步执行...");
        }
        catch (Exception ex)
        {
            TimestampedDebug.WriteLine($"❌ 第八阶段失败: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
            throw new InvalidOperationException("第八阶段：初始化日志失败", ex);
        }

        // ========== 第九阶段：立即启动 LocalizationManager 初始化（关键：必须在 CreateShell 之前） ==========
        // 这是因为 LoginViewModel 构造函数会调用 GetString()，如果 InitializeAsync() 还没开始，会导致长时间等待
        // 使用 Task.Factory.StartNew 并指定 TaskCreationOptions 确保立即执行，避免线程池调度延迟
        try
        {
            TimestampedDebug.WriteLine("第九阶段：立即启动 LocalizationManager 初始化...");
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    var startTime = DateTime.Now;
                    TimestampedDebug.WriteLine($"[LocalizationManager] 异步任务实际开始执行，时间: {startTime:HH:mm:ss.fff}");
                    
                    var localizationManager = Container.Resolve<ILocalizationManager>();
                    await localizationManager.InitializeAsync().ConfigureAwait(false);
                    
                    var endTime = DateTime.Now;
                    var elapsed = (endTime - startTime).TotalMilliseconds;
                    TimestampedDebug.WriteLine($"[LocalizationManager] InitializeAsync 完成，耗时: {elapsed:F0}ms");
                    App.StartupLogManager?.Information($"[LocalizationManager] InitializeAsync 完成，耗时: {elapsed:F0}ms");
                    
                    // 初始化完成后，如果当前语言与保存的语言不同，切换到保存的语言
                    var savedLang = Takt.Common.Helpers.AppSettingsHelper.GetLanguage();
                    if (!string.IsNullOrWhiteSpace(savedLang) && savedLang != localizationManager.CurrentLanguage)
                    {
                        TimestampedDebug.WriteLine($"[LocalizationManager] 切换到保存的语言: {savedLang}");
                        localizationManager.ChangeLanguage(savedLang);
                    }
                }
                catch (Exception ex)
                {
                    TimestampedDebug.WriteLineError($"[LocalizationManager] InitializeAsync 失败: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
                    App.StartupLogManager?.Error(ex, "[LocalizationManager] InitializeAsync 失败");
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.RunContinuationsAsynchronously, TaskScheduler.Default).Unwrap();
            TimestampedDebug.WriteLine("✓ LocalizationManager 初始化已启动（异步，不阻塞，立即执行）");
        }
        catch (Exception ex)
        {
            TimestampedDebug.WriteLineError($"❌ 第九阶段失败: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
            // 不抛出异常，允许应用继续启动
        }
        
            TimestampedDebug.WriteLine("========== RegisterTypes() 完成 ==========");
            App.StartupLogManager?.Information("RegisterTypes() 执行完成");
        }
        catch (Exception ex)
        {
            // 记录完整异常信息到 Debug 输出
            TimestampedDebug.WriteLine($"❌ RegisterTypes() 异常: {ex.GetType().FullName ?? "Unknown"}");
            TimestampedDebug.WriteLine($"异常消息: {ex.Message ?? string.Empty}");
            TimestampedDebug.WriteLine($"异常堆栈:\n{ex.StackTrace ?? string.Empty}");
            
            // 如果是 ContainerResolutionException，记录更详细的信息
            if (ex is Prism.Ioc.ContainerResolutionException containerEx)
            {
                TimestampedDebug.WriteLine($"========== 容器解析异常详情 ==========");
                TimestampedDebug.WriteLine($"异常类型: {containerEx.GetType().FullName ?? "Unknown"}");
                TimestampedDebug.WriteLine($"异常消息: {containerEx.Message ?? string.Empty}");
                
                // 递归记录所有内部异常
                Exception? innerEx = containerEx.InnerException;
                int level = 1;
                while (innerEx != null)
                {
                    TimestampedDebug.WriteLine($"内部异常 #{level}: {innerEx.GetType().FullName ?? "Unknown"}");
                    TimestampedDebug.WriteLine($"  消息: {innerEx.Message ?? string.Empty}");
                    TimestampedDebug.WriteLine($"  堆栈:\n{innerEx.StackTrace ?? string.Empty}");
                    innerEx = innerEx.InnerException;
                    level++;
                }
                TimestampedDebug.WriteLine($"=====================================");
            }
            else
            {
                // 非 ContainerResolutionException，也递归记录内部异常
                Exception? innerEx = ex.InnerException;
                int level = 1;
                while (innerEx != null)
                {
                    TimestampedDebug.WriteLine($"内部异常 #{level}: {innerEx.GetType().FullName ?? "Unknown"}: {innerEx.Message ?? string.Empty}");
                    TimestampedDebug.WriteLine($"  堆栈:\n{innerEx.StackTrace ?? string.Empty}");
                    innerEx = innerEx.InnerException;
                    level++;
                }
            }
            
            // 记录到日志文件
            App.StartupLogManager?.Error(ex, "========== RegisterTypes() 执行失败 ==========");
            App.StartupLogManager?.Error(ex, "异常类型: {ExceptionType}", ex.GetType().FullName ?? "Unknown");
            App.StartupLogManager?.Error(ex, "异常消息: {Message}", ex.Message ?? string.Empty);
            App.StartupLogManager?.Error(ex, "异常堆栈:\n{StackTrace}", ex.StackTrace ?? string.Empty);
            
            // 递归记录所有内部异常到日志
            Exception? innerExForLog = ex.InnerException;
            int logLevel = 1;
            while (innerExForLog != null)
            {
                App.StartupLogManager?.Error(innerExForLog, "内部异常 #{Level}: {InnerExceptionType}: {InnerMessage}", 
                    logLevel, innerExForLog.GetType().FullName ?? "Unknown", innerExForLog.Message ?? string.Empty);
                App.StartupLogManager?.Error(innerExForLog, "内部异常 #{Level} 堆栈:\n{InnerStackTrace}", 
                    logLevel, innerExForLog.StackTrace ?? string.Empty);
                innerExForLog = innerExForLog.InnerException;
                logLevel++;
            }
            
            App.StartupLogManager?.Error(ex, "==========================================");
            
            // 显示异常对话框，确保用户能看到异常信息
            try
            {
                var exceptionMessage = $"RegisterTypes() 执行失败\n\n" +
                    $"异常类型: {ex.GetType().FullName ?? "Unknown"}\n" +
                    $"异常消息: {ex.Message ?? string.Empty}\n\n";
                
                if (ex.InnerException != null)
                {
                    exceptionMessage += $"内部异常:\n";
                    Exception? inner = ex.InnerException;
                    int level = 1;
                    while (inner != null)
                    {
                        exceptionMessage += $"  [{level}] {inner.GetType().FullName}: {inner.Message}\n";
                        inner = inner.InnerException;
                        level++;
                    }
                }
                
                exceptionMessage += $"\n详细堆栈:\n{ex.StackTrace ?? string.Empty}";
                
                MessageBox.Show(
                    exceptionMessage,
                    "容器注册失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                // 如果 MessageBox 也失败，至少输出到控制台
                Console.WriteLine("==========================================");
                Console.WriteLine("RegisterTypes() 执行失败");
                Console.WriteLine($"异常类型: {ex.GetType().FullName ?? "Unknown"}");
                Console.WriteLine($"异常消息: {ex.Message ?? string.Empty}");
                Console.WriteLine($"异常堆栈:\n{ex.StackTrace ?? string.Empty}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}");
                }
                Console.WriteLine("==========================================");
            }
            
            throw;
        }
    }


    /// <summary>
    /// 初始化 Serilog
    /// </summary>
    private static void InitializeSerilog(IConfiguration configuration)
    {
        var logDirectory = Takt.Common.Helpers.PathHelper.GetLogDirectory();
        var logFilePath = Path.Combine(logDirectory, "app-.txt");

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 8 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8)
            .CreateLogger();
    }

    /// <summary>
    /// 配置 Region 适配器映射
    /// </summary>
    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);
        
        // 注册 TabControl Region 适配器
        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
        var regionBehaviorFactory = Container.Resolve<IRegionBehaviorFactory>();
        regionAdapterMappings.RegisterMapping(typeof(System.Windows.Controls.TabControl), 
            new TabControlRegionAdapter(regionBehaviorFactory, operLog));
    }

    /// <summary>
    /// 配置模块目录
    /// </summary>
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<IdentityModule>();
        moduleCatalog.AddModule<LogisticsModule>();
        moduleCatalog.AddModule<RoutineModule>();
        moduleCatalog.AddModule<LoggingModule>();
        moduleCatalog.AddModule<GeneratorModule>();
    }
    
    /// <summary>
    /// 注册所有基础设施层服务到 DryIoc 容器
    /// </summary>
    private void RegisterInfrastructureServices(IContainerRegistry containerRegistry, string connectionString, HbtDatabaseSettings databaseSettings, IConfiguration configuration)
    {
        // 注册日志
        containerRegistry.RegisterInstance<ILogger>(Log.Logger);

        // 注册日志管理器
        containerRegistry.RegisterSingleton<InitLogManager>();

        // 注册 AppLogManager（必须在 DbContext 之前注册，以便传递给 SqlSugarAop）
        containerRegistry.RegisterSingleton<AppLogManager>(c =>
        {
            var logger = c.Resolve<ILogger>();
            var instance = new AppLogManager(logger);
            // 在创建后立即设置到 SqlSugarAop 的静态引用
            SqlSugarAop.SetAppLogManager(instance);
            System.Diagnostics.Debug.WriteLine("🟢 [PrismBootstrapper] AppLogManager 已设置到 SqlSugarAop");
            return instance;
        });

        // 注册数据库上下文（必须在 BaseRepository 之前注册，因为 BaseRepository 依赖 DbContext）
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbContext>(c =>
        {
            var logger = c.Resolve<ILogger>();
            var appLog = c.Resolve<AppLogManager>();
            // 不在这里解析 LogDatabaseWriter，避免循环依赖
            var dbContext = new Takt.Infrastructure.Data.DbContext(connectionString, logger, databaseSettings, null, appLog);
            System.Diagnostics.Debug.WriteLine("🟢 [PrismBootstrapper] DbContext 已创建");
            return dbContext;
        });

        // 注册基础仓储（泛型注册）- 必须在 DbContext 之后注册，因为 BaseRepository 依赖 DbContext
        // 使用 Singleton 而不是 Scoped，因为 DbContext 已经是 Singleton，且注册阶段还没有作用域
        var dryIocContainer = containerRegistry.GetContainer() as DryIoc.IContainer;
        if (dryIocContainer != null)
        {
            dryIocContainer.Register(typeof(IBaseRepository<>), typeof(BaseRepository<>), Reuse.Singleton);
            System.Diagnostics.Debug.WriteLine("🟢 [PrismBootstrapper] IBaseRepository<> 已注册为 Singleton");
        }

        // 注册日志数据库写入器（依赖 Repository，必须在 BaseRepository 之后注册）
        containerRegistry.Register<Takt.Common.Logging.ILogDatabaseWriter>(c =>
        {
            var operLogRepo = c.Resolve<Takt.Domain.Repositories.IBaseRepository<Takt.Domain.Entities.Logging.OperLog>>();
            var diffLogRepo = c.Resolve<Takt.Domain.Repositories.IBaseRepository<Takt.Domain.Entities.Logging.DiffLog>>();
            var quartzJobLogRepo = c.Resolve<Takt.Domain.Repositories.IBaseRepository<Takt.Domain.Entities.Logging.QuartzJobLog>>();
            var appLog = c.Resolve<AppLogManager>();
            var instance = new Takt.Infrastructure.Logging.LogDatabaseWriter(operLogRepo, diffLogRepo, quartzJobLogRepo, appLog);
            // 在创建后立即设置到 SqlSugarAop 和 OperLogManager 的静态引用
            SqlSugarAop.SetLogDatabaseWriter(instance);
            Takt.Common.Logging.OperLogManager.SetLogDatabaseWriter(instance);
            System.Diagnostics.Debug.WriteLine("🟢 [PrismBootstrapper] ILogDatabaseWriter 已设置到 SqlSugarAop 和 OperLogManager");
            return instance;
        });

        // 注册数据表初始化服务
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbTableInitializer>();

        // 注册 RBAC 种子数据初始化服务
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedRbac>();

        // 注册翻译种子服务
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedLanguage>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedTranslationCommon>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedTranslationDictionary>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedTranslationEntity>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedTranslationValidation>();

        // 注册 Routine 模块种子服务
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedRoutineDictionary>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedSetting>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedMenu>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedProdModel>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedVisit>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedQuartz>();
        containerRegistry.RegisterSingleton<Takt.Infrastructure.Data.DbSeedCoordinator>();

        // 注册操作日志管理器
        containerRegistry.RegisterSingleton<OperLogManager>(c =>
        {
            var logger = c.Resolve<ILogger>();
            // 尝试解析 ILogDatabaseWriter，如果失败返回 null
            Takt.Common.Logging.ILogDatabaseWriter? logDatabaseWriter = null;
            try
            {
                logDatabaseWriter = c.Resolve<Takt.Common.Logging.ILogDatabaseWriter>();
            }
            catch
            {
                // 忽略，OperLogManager 会通过静态引用机制在运行时获取
            }
            var instance = new OperLogManager(logger, logDatabaseWriter);
            System.Diagnostics.Debug.WriteLine("🟢 [PrismBootstrapper] OperLogManager 已创建");
            return instance;
        });

        // 通过批量注册自动注册所有 *Service 结尾的应用层服务
        var applicationAssembly = typeof(Takt.Application.Services.Identity.IUserService).Assembly;
        var serviceTypes = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"))
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            var interfaces = serviceType.GetInterfaces()
                .Where(i => i.Name.EndsWith("Service") || i.Name == "I" + serviceType.Name)
                .ToList();

            if (interfaces.Any())
            {
                foreach (var interfaceType in interfaces)
                {
                    containerRegistry.Register(interfaceType, serviceType);
                }
            }
            else
            {
                // 如果没有找到接口，注册为自身
                containerRegistry.Register(serviceType);
            }
        }

        // 注册本地化管理器（基础设施层实现 -> 领域层接口）
        containerRegistry.RegisterSingleton<ILocalizationManager, Takt.Infrastructure.Services.LocalizationManager>();

        // 注册数据库元数据服务（基础设施层实现 -> 领域层接口）
        containerRegistry.RegisterSingleton<IDatabaseMetadataService, Takt.Infrastructure.Services.DatabaseMetadataService>();

        // 注册序列号管理器（基础设施层实现 -> 领域层接口）
        containerRegistry.Register<ISerialsManager, Takt.Infrastructure.Services.SerialsManager>();

        // 注册 Quartz 调度器管理器（基础设施层实现 -> 领域层接口）
        containerRegistry.RegisterSingleton<IQuartzSchedulerManager, Takt.Infrastructure.Services.QuartzSchedulerManager>();

        // 注册 Quartz Job 类（每次执行任务时创建新实例）
        containerRegistry.Register<Takt.Infrastructure.Jobs.GenericServiceJob>();

        // 注册日志清理后台服务（每月1号0点执行，只保留最近7天的日志）
        containerRegistry.RegisterSingleton<Microsoft.Extensions.Hosting.IHostedService, Takt.Infrastructure.Services.LogCleanupBackgroundService>();
    }

    /// <summary>
    /// 初始化完成后的处理（在所有模块加载完成后调用）
    /// Prism 会在 OnInitialized 之后启动消息循环，所以这里需要确保窗口已准备好显示
    /// </summary>
    protected override void OnInitialized()
    {
        try
        {
            TimestampedDebug.WriteLine("========== OnInitialized 开始 ==========");
            App.StartupLogManager?.Information("OnInitialized() 开始执行");
            
            // 根据 Prism 官方示例：OnInitialized 在模块初始化后调用
            // 此时 CreateShell 和 InitializeShell 都已完成，窗口已由 Prism 自动显示
            // 这里不需要再做窗口相关的操作
            base.OnInitialized();
            
            // 从容器中解析已注册的 IServiceProvider
            var serviceProvider = Container.Resolve<IServiceProvider>();
            
            // 检查是否需要初始化
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseSettings = configuration.GetSection("DatabaseSettings").Get<HbtDatabaseSettings>() ?? new HbtDatabaseSettings();
            var needsInitialization = databaseSettings.EnableCodeFirst || databaseSettings.EnableSeedData;
            
            if (needsInitialization)
            {
                // 需要初始化：显示初始化日志窗口 → 执行初始化 → 完成后显示登录窗口
                TimestampedDebug.WriteLine("需要初始化数据库，显示初始化日志窗口");
                
                // 创建并显示初始化日志窗口
                var initLogViewModel = new InitializationLogViewModel();
                _initLogWindow = new InitializationLogWindow
                {
                    DataContext = initLogViewModel
                };
                _initLogWindow.Show();
                
                // 订阅 InitLogManager 的日志输出事件
                _initLogEventHandler = (sender, message) =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        initLogViewModel.AppendLog(message);
                    });
                };
                InitLogManager.LogOutput += _initLogEventHandler;
                
                // 确保初始化状态已设置为 Initializing
                InitializationStatusManager.UpdateStatus(
                    InitializationStatus.Initializing,
                    ResourceFileLocalizationHelper.GetString("login.initialization.inprogress", "数据初始化中..."));
                
                // 添加初始日志
                initLogViewModel.AppendLog("开始初始化数据库...");
                
                // 在后台异步执行数据库初始化，不阻塞 UI 启动
                _ = Task.Run(async () =>
                {
                    try
                    {
                        TimestampedDebug.WriteLine("开始后台数据库初始化...");
                        await InitializeApplicationDataAsync(serviceProvider);
                        TimestampedDebug.WriteLine("✓ 后台数据库初始化完成");
                        
                        // 更新初始化状态：初始化完成（在 UI 线程上更新）
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            // 更新状态
                            InitializationStatusManager.UpdateStatus(
                                InitializationStatus.Completed,
                                ResourceFileLocalizationHelper.GetString("login.initialization.completed", "数据初始化完成，可以登录"));
                            
                            // 关闭初始化日志窗口
                            if (_initLogWindow?.DataContext is InitializationLogViewModel viewModel)
                            {
                                viewModel.AppendLog("✓ 数据初始化完成");
                                InitLogManager.LogOutput -= _initLogEventHandler!;
                                _initLogEventHandler = null;
                                
                                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                                timer.Tick += (s, e) =>
                                {
                                    timer.Stop();
                                    _initLogWindow.Close();
                                    _initLogWindow = null;
                                    ShowLoginWindow();
                                };
                                timer.Start();
                            }
                            else
                            {
                                ShowLoginWindow();
                            }
                        });
                    }
                    catch (InvalidOperationException ex)
                    {
                        // 数据库连接/初始化失败，在日志窗口中显示错误
                        TimestampedDebug.WriteLineError($"❌ 数据库初始化失败: {ex.Message}");
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var message = ResourceFileLocalizationHelper.GetString("database.connectionerror.failed_detail");
                            if (_initLogWindow?.DataContext is InitializationLogViewModel viewModel)
                            {
                                viewModel.AppendLog($"❌ 错误: {message}");
                                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                                timer.Tick += (s, e) => { timer.Stop(); System.Windows.Application.Current.Shutdown(); };
                                timer.Start();
                            }
                            else
                            {
                                // 如果没有日志窗口，使用消息框
                                var title = ResourceFileLocalizationHelper.GetString("database.initialization.title");
                                TaktMessageBox.Error(message, title);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // 数据库初始化异常，在日志窗口中显示错误
                        TimestampedDebug.WriteLineError($"❌ 数据库初始化异常: {ex.GetType().Name}: {ex.Message ?? string.Empty}");
                        TimestampedDebug.WriteLineError($"异常堆栈: {ex.StackTrace ?? string.Empty}");
                        
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var errorMessage = $"❌ 初始化失败: {ex.GetType().Name}";
                            var detailMessage = ex.Message ?? "未知错误";
                            
                            if (_initLogWindow?.DataContext is InitializationLogViewModel viewModel)
                            {
                                viewModel.AppendLog(errorMessage);
                                viewModel.AppendLog($"   错误详情: {detailMessage}");
                                if (ex.InnerException != null)
                                {
                                    viewModel.AppendLog($"   内部异常: {ex.InnerException.Message}");
                                }
                                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                                timer.Tick += (s, e) => { timer.Stop(); System.Windows.Application.Current.Shutdown(); };
                                timer.Start();
                            }
                            else
                            {
                                // 如果没有日志窗口，使用消息框
                                var message = ResourceFileLocalizationHelper.GetString("database.initialization.error", detailMessage);
                                var title = ResourceFileLocalizationHelper.GetString("database.initialization.title");
                                TaktMessageBox.Error(message, title);
                            }
                        });
                    }
                });
            }
            else
            {
                // 不需要初始化：直接显示登录窗口
                TimestampedDebug.WriteLine("不需要初始化，直接显示登录窗口");
                
                // 设置初始化状态为已完成
                InitializationStatusManager.UpdateStatus(
                    InitializationStatus.Completed,
                    ResourceFileLocalizationHelper.GetString("login.initialization.completed", "数据初始化完成，可以登录"));
                
                // 直接显示登录窗口
                ShowLoginWindow();
            }
            
            TimestampedDebug.WriteLine("========== OnInitialized 完成 ==========");
            App.StartupLogManager?.Information("OnInitialized() 执行完成，准备启动消息循环");
        }
        catch (Exception ex)
        {
            TimestampedDebug.WriteLineError($"OnInitialized 异常: {ex.GetType().FullName ?? "Unknown"}: {ex.Message ?? string.Empty}");
            TimestampedDebug.WriteLine($"异常堆栈:\n{ex.StackTrace ?? string.Empty}");
            App.StartupLogManager?.Error(ex, "OnInitialized() 执行失败: {ExceptionType}: {Message}", 
                ex.GetType().FullName ?? "Unknown", ex.Message ?? string.Empty);
            throw;
        }
    }

    /// <summary>
    /// 显示登录窗口
    /// </summary>
    private void ShowLoginWindow()
    {
        if (_loginWindow != null)
        {
            _loginWindow.Visibility = Visibility.Visible;
            _loginWindow.ShowInTaskbar = true;
            _loginWindow.Show();
            _loginWindow.Activate();
            _loginWindow.WindowState = WindowState.Normal;
            
            TimestampedDebug.WriteLine("✓ 登录窗口已显示");
            App.StartupLogManager?.Information("登录窗口已显示");
        }
    }

    /// <summary>
    /// 初始化应用程序数据（数据库初始化、种子数据）
    /// 统一的数据库连接检查和初始化入口，避免重复验证
    /// </summary>
    private async Task InitializeApplicationDataAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var operLog = serviceProvider.GetService<OperLogManager>();
            operLog?.Information("开始初始化应用程序...");

            // 获取数据库配置
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseSettings = configuration.GetSection("DatabaseSettings").Get<HbtDatabaseSettings>() ?? new HbtDatabaseSettings();

            // 如果 CodeFirst 和 SeedData 都禁用，检查数据库是否已初始化
            if (!databaseSettings.EnableCodeFirst && !databaseSettings.EnableSeedData)
            {
                TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] CodeFirst 和 SeedData 都已禁用，检查数据库是否已初始化");
                
                var dbContext = serviceProvider.GetRequiredService<Takt.Infrastructure.Data.DbContext>();
                
                // 检查数据库连接
                var isConnected = await dbContext.CheckConnectionAsync();
                if (!isConnected)
                {
                    // 数据库连接失败，立即停止所有初始化进程并在日志窗口中显示错误信息
                    TimestampedDebug.WriteLine("❌ [PrismBootstrapper] 数据库连接失败，停止所有初始化进程");
                    
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var message = ResourceFileLocalizationHelper.GetString("database.connectionerror.failed_detail");
                        if (_initLogWindow?.DataContext is InitializationLogViewModel viewModel)
                        {
                            viewModel.AppendLog($"❌ 错误: {message}");
                            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                            timer.Tick += (s, e) => { timer.Stop(); System.Windows.Application.Current.Shutdown(); };
                            timer.Start();
                        }
                        else
                        {
                            TaktMessageBox.Error(message, ResourceFileLocalizationHelper.GetString("database.initialization.title"));
                            System.Windows.Application.Current.Shutdown();
                        }
                    }, System.Windows.Threading.DispatcherPriority.Send);
                    
                    // 强制退出应用（确保所有线程都停止）
                    Environment.Exit(1);
                    
                    // 抛出异常以确保后续初始化不会执行（虽然不会执行到这里）
                    throw new InvalidOperationException("数据库连接失败，应用程序已停止");
                }
                
                // 检查关键表是否存在
                var db = dbContext.Db;
                var userTableExists = db.DbMaintenance.IsAnyTable("takt_oidc_user");
                var menuTableExists = db.DbMaintenance.IsAnyTable("takt_oidc_menu");
                
                if (!userTableExists || !menuTableExists)
                {
                    // 数据库表不存在，立即停止所有初始化进程并在日志窗口中显示错误信息
                    TimestampedDebug.WriteLine("❌ [PrismBootstrapper] 数据库表不存在，停止所有初始化进程");
                    
                    // 在日志窗口中显示错误信息
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var message = ResourceFileLocalizationHelper.GetString("database.tables_not_initialized.message");
                        if (_initLogWindow?.DataContext is InitializationLogViewModel viewModel)
                        {
                            viewModel.AppendLog($"❌ 错误: {message}");
                            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                            timer.Tick += (s, e) => { timer.Stop(); System.Windows.Application.Current.Shutdown(); };
                            timer.Start();
                        }
                        else
                        {
                            // 如果没有日志窗口，使用消息框
                            var title = ResourceFileLocalizationHelper.GetString("database.tables_not_initialized.title");
                            TaktMessageBox.Error(message, title);
                            System.Windows.Application.Current.Shutdown();
                        }
                    }, System.Windows.Threading.DispatcherPriority.Send);
                    
                    // 强制退出应用（确保所有线程都停止）
                    Environment.Exit(1);
                    
                    // 抛出异常以确保后续初始化不会执行（虽然不会执行到这里）
                    throw new InvalidOperationException("数据库未初始化，应用程序已停止");
                }
                
                TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] 数据库检查通过，表已存在");
            }

            // 初始化数据库表
            TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] 准备解析 DbTableInitializer");
            var dbTableInitializer = serviceProvider.GetRequiredService<Takt.Infrastructure.Data.DbTableInitializer>();
            TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] DbTableInitializer 解析成功");
            
            await dbTableInitializer.InitializeAsync();

            // 初始化种子数据（如果启用）
            if (databaseSettings.EnableSeedData)
            {
                TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] 准备禁用差异日志（种子数据初始化前）");
                // 临时禁用差异日志，避免启动时连接冲突
                // 种子数据初始化不应该记录差异日志
                Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(false);
                
                try
                {
                    // 使用协调器统一执行所有种子数据初始化
                    TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] 开始执行种子数据初始化");
                    var dbSeedCoordinator = serviceProvider.GetRequiredService<Takt.Infrastructure.Data.DbSeedCoordinator>();
                    await dbSeedCoordinator.InitializeAsync();
                    TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] 种子数据初始化完成");
                }
                finally
                {
                    // 种子数据初始化完成后，重新启用差异日志
                    TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] 准备启用差异日志（种子数据初始化后）");
                    Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(true);
                    TimestampedDebug.WriteLine("🟣 [PrismBootstrapper] 差异日志已启用");
                }
            }
            else
            {
                // 如果种子数据未启用，确保差异日志是启用的
                Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(true);
            }

            operLog?.Information("应用程序数据初始化完成");
        }
        catch (InvalidOperationException)
        {
            // 数据库连接/初始化失败，已显示消息框并关闭应用，直接重新抛出
            throw;
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "应用程序数据初始化失败");
            throw;
        }
    }

}

