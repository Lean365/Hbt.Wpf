// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent
// 文件名称：App.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：WPF 应用程序入口，配置依赖注入和启动流程
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险.
// ========================================

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using System.Diagnostics;
using Takt.Common.Config;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Services;
using Takt.Fluent.ViewModels;
using Takt.Fluent.ViewModels.Identity;
using Takt.Fluent.ViewModels.Logging;
using Takt.Fluent.ViewModels.Routine;
using Takt.Fluent.ViewModels.Settings;
using Takt.Fluent.ViewModels.Generator;
using Takt.Fluent.Views;
using Takt.Fluent.Views.About;
using Takt.Fluent.Views.Dashboard;
using Takt.Fluent.Views.Identity;
using Takt.Fluent.Views.Identity.MenuComponent;
using Takt.Fluent.Views.Identity.RoleComponent;
using Takt.Fluent.Views.Identity.UserComponent;
using Takt.Fluent.Views.Logging;
using Takt.Fluent.Views.Routine;
using Takt.Fluent.Views.Routine.SettingComponent;
using Takt.Fluent.Views.Settings;
using Takt.Fluent.ViewModels.Logistics.Materials;
using Takt.Fluent.Helpers;
using Takt.Fluent.ViewModels.Logistics.Serials;
using Takt.Fluent.ViewModels.Logistics.Visits;
using Takt.Fluent.Views.Logistics.Materials;
using Takt.Fluent.Views.Logistics.Serials;
using Takt.Fluent.Views.Logistics.Visits;
using Takt.Fluent.Views.Generator;
using Takt.Fluent.Views.Generator.CodeGenComponent;
using Takt.Infrastructure.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Data;
using Takt.Fluent.Controls;

namespace Takt.Fluent;

/// <summary>
/// WPF 应用程序入口
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    /// <summary>
    /// 服务提供者（用于全局访问依赖注入容器）
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    /// <summary>
    /// 启动日志管理器（在 Services 初始化之前使用，使用 InitLogManager 记录初始化过程）
    /// </summary>
    public static InitLogManager? StartupLogManager { get; private set; }

    /// <summary>
    /// 初始化启动日志管理器（在 Services 初始化之前使用）
    /// </summary>
    public static void InitializeStartupLogger()
    {
        if (StartupLogManager != null) return;

        // 在创建日志管理器之前，清除所有旧日志文件
        ClearAllLogFilesBeforeStartup();

        // 创建一个临时的 Serilog Logger 用于 InitLogManager
        var tempLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .CreateLogger();

        // 使用 InitLogManager 记录启动日志（专门用于初始化过程）
        StartupLogManager = new InitLogManager(tempLogger);
    }

    /// <summary>
    /// 在启动时清除所有日志文件（在日志文件打开之前执行）
    /// </summary>
    private static void ClearAllLogFilesBeforeStartup()
    {
        try
        {
            // 获取日志目录路径
            var logDirectory = Takt.Common.Helpers.PathHelper.GetLogDirectory();
            
            if (!Directory.Exists(logDirectory))
            {
                return;
            }

            // 清除所有日志文件（app-*.txt, oper-*.txt, init-*.txt 等）
            var logFilePatterns = new[] { "*.*" };
            int totalDeletedCount = 0;
            long totalDeletedSize = 0;

            foreach (var pattern in logFilePatterns)
            {
                var files = Directory.GetFiles(logDirectory, pattern);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var fileSize = fileInfo.Length;
                        
                        // 尝试删除文件（此时日志文件还未打开，应该可以删除）
                        File.Delete(file);
                        totalDeletedCount++;
                        totalDeletedSize += fileSize;
                    }
                    catch
                    {
                        // 忽略删除失败的文件（可能被其他进程使用）
                    }
                }
            }

            if (totalDeletedCount > 0)
            {
                Console.WriteLine($"清除日志文件完成，共删除 {totalDeletedCount} 个文件，总大小 {totalDeletedSize / (1024.0 * 1024.0):F2} MB");
            }
        }
        catch
        {
            // 清除日志失败不影响启动流程
        }
    }

    /// <summary>
    /// 应用程序构造函数
    /// </summary>
    public App()
    {
        InitializeStartupLogger();
        StartupLogManager?.Information("App 构造函数被调用");
    }

    /// <summary>
    /// 手动加载 App.xaml 中的资源字典
    /// </summary>
    /// <remarks>
    /// 由于 EnableDefaultApplicationDefinition=false，此方法会被 App 构造函数调用
    /// 也可能被 PrismBootstrapper 通过反射调用
    /// </remarks>
    internal void LoadAppXamlResources()
    {
        // 检查资源是否已经加载（避免重复加载）
        var mainDict = this.Resources as ResourceDictionary;
        if (mainDict != null && mainDict.MergedDictionaries.Count > 0)
        {
            StartupLogManager?.Information("资源字典已加载，跳过重复加载（数量: {Count}）", mainDict.MergedDictionaries.Count);
            return;
        }

        // 初始化 Resources（如果为 null）
        if (this.Resources == null)
        {
            this.Resources = new ResourceDictionary();
        }

        mainDict = this.Resources as ResourceDictionary;
        if (mainDict == null)
        {
            this.Resources = new ResourceDictionary();
            mainDict = this.Resources as ResourceDictionary;
        }

        // 重要：由于 EnableDefaultApplicationDefinition=false，Application.LoadComponent 无法正确加载 MergedDictionaries
        // 必须手动加载所有资源字典，严格按照 App.xaml 中的顺序
        StartupLogManager?.Information("使用手动加载方式（因为 EnableDefaultApplicationDefinition=false）");
        LoadAppXamlResourcesManually(mainDict);

        // 验证资源字典是否已加载
        var mergedCount = mainDict?.MergedDictionaries?.Count ?? 0;
        StartupLogManager?.Information("资源字典加载完成，合并的资源字典数量: {Count}", mergedCount);

        if (mergedCount == 0)
        {
            throw new InvalidOperationException("资源字典加载失败：合并的资源字典数量为 0！");
        }

        // 实际验证：检查关键资源是否存在
        var testResource = this.TryFindResource("BaseDefaultButtonStyleSmall");
        if (testResource == null)
        {
            // 输出详细的调试信息
            var resourceList = new System.Collections.Generic.List<string>();
            if (mainDict?.MergedDictionaries != null)
            {
                foreach (var dict in mainDict.MergedDictionaries)
                {
                    if (dict is ResourceDictionary rd && rd.Source != null)
                    {
                        resourceList.Add(rd.Source.ToString());
                    }
                    else
                    {
                        resourceList.Add(dict?.GetType().Name ?? "未知类型");
                    }
                }
            }
            
            var errorMsg = $"关键资源 'BaseDefaultButtonStyleSmall' 未找到！\n" +
                $"资源字典数量: {mergedCount}\n" +
                $"资源字典列表: {string.Join(", ", resourceList)}\n" +
                "请检查 App.xaml 中的资源字典定义是否正确。";
            
            StartupLogManager?.Error(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        StartupLogManager?.Information("✓ 验证通过: BaseDefaultButtonStyleSmall 资源已找到");
    }

    /// <summary>
    /// 手动加载 App.xaml 中定义的资源字典
    /// 严格按照 App.xaml 中的顺序加载（顺序很重要！）
    /// </summary>
    private void LoadAppXamlResourcesManually(ResourceDictionary mainDict)
    {
        // 根据 App.xaml 中的定义，严格按照顺序手动添加资源字典
        // 顺序非常重要：后面的资源可能依赖前面的资源
        StartupLogManager?.Information("开始手动加载资源字典（严格按照 App.xaml 中的顺序）");

        // 1. MaterialDesign BundledTheme（必须在最前面）
        try
        {
            var bundledTheme = new MaterialDesignThemes.Wpf.BundledTheme
            {
                BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Light,
                PrimaryColor = MaterialDesignColors.PrimaryColor.Teal,
                SecondaryColor = MaterialDesignColors.SecondaryColor.Cyan
            };
            mainDict.MergedDictionaries.Add(bundledTheme);
            StartupLogManager?.Information("BundledTheme 已添加");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "添加 BundledTheme 失败");
        }

        // 2. MaterialDesign3.Defaults.xaml
        try
        {
            var materialDesign3Defaults = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign3.Defaults.xaml")
            };
            mainDict.MergedDictionaries.Add(materialDesign3Defaults);
            StartupLogManager?.Information("MaterialDesign3.Defaults.xaml 已添加");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "添加 MaterialDesign3.Defaults.xaml 失败: {Message}", ex.Message);
        }

        // 3. MaterialDesignTheme.ValidationErrorTemplate.xaml
        try
        {
            var validationErrorTemplate = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.ValidationErrorTemplate.xaml")
            };
            mainDict.MergedDictionaries.Add(validationErrorTemplate);
            StartupLogManager?.Information("MaterialDesignTheme.ValidationErrorTemplate.xaml 已添加");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "添加 MaterialDesignTheme.ValidationErrorTemplate.xaml 失败: {Message}", ex.Message);
        }

        // 4. 项目自定义资源字典（严格按照 App.xaml 中的顺序加载）
        // 顺序非常重要：ButtonDefaultNoStyles.xaml 依赖于 ButtonDefaultStyles.xaml 中的资源
        var customResources = new[]
        {
            "pack://application:,,,/Takt.Fluent;component/Controls/TaktPageHeader.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/TaktDefaultColors.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonColors.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultStyles.xaml", // BaseDefaultButtonStyleSmall 在这里
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultNoStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultPlainStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultIconStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonCircleStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/FormStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/NavTemplates.xaml"
        };

        foreach (var resourceUri in customResources)
        {
            try
            {
                var resourceDict = new ResourceDictionary
                {
                    Source = new Uri(resourceUri)
                };
                mainDict.MergedDictionaries.Add(resourceDict);
                StartupLogManager?.Information("✓ 已添加资源字典 [{Index}]: {Uri}", 
                    mainDict.MergedDictionaries.Count, resourceUri);
            }
            catch (Exception ex)
            {
                var errorMsg = $"添加资源字典失败: {resourceUri}, 错误: {ex.Message}";
                StartupLogManager?.Error(ex, errorMsg);
                throw new InvalidOperationException(errorMsg, ex);
            }
        }

        StartupLogManager?.Information("手动加载完成，合并的资源字典数量: {Count}", mainDict.MergedDictionaries.Count);
    }

    /// <summary>
    /// 手动触发启动事件（用于 EnableDefaultApplicationDefinition=false 的情况）
    /// </summary>
    public void StartApplication()
    {
        StartupLogManager?.Information("StartApplication 方法被调用");

        // StartupEventArgs 的构造函数是内部的，需要通过反射获取正确的构造函数
        // 尝试不同的构造函数签名
        StartupEventArgs? args = null;
        var argsArray = Environment.GetCommandLineArgs();

        try
        {
            // 尝试使用 string[] 参数的构造函数
            var constructor = typeof(StartupEventArgs).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(string[]) },
                null);

            if (constructor != null)
            {
                args = (StartupEventArgs)constructor.Invoke(new object[] { argsArray });
            }
        }
        catch (Exception ex)
        {
            StartupLogManager?.Warning("无法通过反射创建 StartupEventArgs: {0}", ex.Message);
        }

        // 如果反射失败，尝试使用无参构造函数或直接调用 OnStartup
        if (args == null)
        {
            try
            {
                // 尝试无参构造函数
                args = (StartupEventArgs)Activator.CreateInstance(
                    typeof(StartupEventArgs),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    Array.Empty<object>(),
                    null)!;
            }
            catch
            {
                // 如果都失败，创建一个包装类
                StartupLogManager?.Warning("无法创建 StartupEventArgs，将使用默认方式");
                // 直接调用 OnStartup，传入 null 或创建一个简单的包装
                // 实际上，app.Run() 可能会自动触发 OnStartup，所以这里可能不需要手动调用
                return;
            }
        }

        if (args != null)
        {
            OnStartup(args);
        }
    }

    /// <summary>
    /// 应用程序启动
    /// 根据 Prism 官方示例 01-BootstrapperShell：
    /// - App 继承 Application
    /// - 在 OnStartup 中调用 base.OnStartup(e)，然后调用 bootstrapper.Run()
    /// - Prism 的 Run() 会启动消息循环并显示窗口
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // 立即设置初始化状态为 Initializing，确保登录窗口显示时按钮被禁用
        InitializationStatusManager.UpdateStatus(
            InitializationStatus.Initializing,
            ResourceFileLocalizationHelper.GetString("login.initialization.inprogress", "数据初始化中..."));

        var bootstrapper = new Bootstrapper.PrismBootstrapper();
        bootstrapper.Run();
    }


    /// <summary>
    /// 清除所有本地日志文件
    /// 在应用程序启动时调用，清除日志目录中的所有日志文件
    /// </summary>
    private void ClearAllLogFiles()
    {
        try
        {
            // 获取日志目录路径
            var logDirectory = Takt.Common.Helpers.PathHelper.GetLogDirectory();
            
            if (!Directory.Exists(logDirectory))
            {
                StartupLogManager?.Information("日志目录不存在，无需清除: {LogDirectory}", logDirectory);
                return;
            }

            // 清除所有日志文件（app-*.txt, oper-*.txt, init-*.txt 等）
            var logFilePatterns = new[] { "app-*.txt", "oper-*.txt", "init-*.txt" };
            int totalDeletedCount = 0;
            long totalDeletedSize = 0;

            foreach (var pattern in logFilePatterns)
            {
                var files = Directory.GetFiles(logDirectory, pattern);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var fileSize = fileInfo.Length;
                        
                        // 尝试删除文件（如果文件正在被使用，可能会失败）
                        File.Delete(file);
                        totalDeletedCount++;
                        totalDeletedSize += fileSize;
                        
                        StartupLogManager?.Debug("删除日志文件: {FileName}, 大小={FileSize} 字节", 
                            fileInfo.Name, fileSize);
                    }
                    catch (Exception ex)
                    {
                        // 如果文件正在被使用（如 init-.txt 正在写入），跳过删除
                        StartupLogManager?.Warning("删除日志文件失败（文件可能正在使用）: {FileName}, 错误: {Error}", 
                            Path.GetFileName(file), ex.Message);
                    }
                }
            }

            StartupLogManager?.Information("清除日志文件完成，共删除 {Count} 个文件，总大小 {Size} 字节 ({SizeMB:F2} MB)", 
                totalDeletedCount, 
                totalDeletedSize,
                totalDeletedSize / (1024.0 * 1024.0));
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "清除日志文件失败");
        }
    }

    private async Task InitializeApplicationAsync()
    {
        try
        {
            // 在启动时清除所有本地日志文件
            ClearAllLogFiles();
            
            StartupLogManager?.Information("开始构建 Host");
            // 构建 Host
            _host = CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

            // 设置全局服务提供者
            Services = _host.Services;
            StartupLogManager?.Information("Host 构建成功，Services 已初始化");

            // 更新初始化状态：开始初始化
            InitializationStatusManager.UpdateStatus(
                InitializationStatus.Initializing,
                ResourceFileLocalizationHelper.GetString("login.initialization.inprogress", "数据初始化中..."));

            // 初始化数据库和种子数据
            await InitializeApplicationDataAsync();

            // 更新初始化状态：初始化完成
            InitializationStatusManager.UpdateStatus(
                InitializationStatus.Completed,
                ResourceFileLocalizationHelper.GetString("login.initialization.completed", "数据初始化完成，可以登录"));

            // 显示登录窗口
            StartupLogManager?.Information("准备显示登录窗口");
            var loginWindow = Services.GetRequiredService<LoginView>();
            loginWindow.Show();

            // 设置主窗口（登录窗口）
            this.Dispatcher.Invoke(() =>
            {
                this.MainWindow = loginWindow;
            });

            StartupLogManager?.Information("应用程序启动完成，登录窗口已显示");

            StartupLogManager?.Information("应用程序启动完成，登录窗口已显示");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "应用程序启动失败");
            // 使用资源文件进行本地化（不依赖数据库）
            this.Dispatcher.Invoke(() =>
            {
                var message = ResourceFileLocalizationHelper.GetString("application.startup.error", ex.Message ?? "", ex.ToString());
                var title = ResourceFileLocalizationHelper.GetString("application.startup.error.title");
                
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Shutdown();
            });
        }
    }

    /// <summary>
    /// 初始化应用程序数据（数据库初始化、种子数据）
    /// 注意：此方法当前未被调用。实际的数据库初始化在 PrismBootstrapper.cs 的 InitializeApplicationDataAsync 中完成
    /// 保留此方法以备将来使用（如果 InitializeApplicationAsync 被调用）
    /// </summary>
    private async Task InitializeApplicationDataAsync()
    {
        if (Services == null)
        {
            throw new InvalidOperationException("Services 未初始化");
        }

        try
        {
            var operLog = Services.GetService<OperLogManager>();
            operLog?.Information("开始初始化应用程序...");

            // 获取数据库配置
            var databaseSettings = Services.GetRequiredService<IConfiguration>()
                .GetSection("DatabaseSettings").Get<HbtDatabaseSettings>() ?? new HbtDatabaseSettings();

            // 如果 CodeFirst 和 SeedData 都禁用，检查数据库是否已初始化
            if (!databaseSettings.EnableCodeFirst && !databaseSettings.EnableSeedData)
            {
                WriteDiagnosticLog("🟣 [App.xaml.cs] CodeFirst 和 SeedData 都已禁用，检查数据库是否已初始化");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] CodeFirst 和 SeedData 都已禁用，检查数据库是否已初始化");

                var dbContext = Services.GetRequiredService<Takt.Infrastructure.Data.DbContext>();
                
                // 检查数据库连接
                var isConnected = await dbContext.CheckConnectionAsync();
                if (!isConnected)
                {
                    // 数据库连接失败，立即停止所有初始化进程并显示错误信息
                    WriteDiagnosticLog("❌ [App.xaml.cs] 数据库连接失败，停止所有初始化进程");
                    System.Diagnostics.Debug.WriteLine("❌ [App.xaml.cs] 数据库连接失败，停止所有初始化进程");
                    
                    // 停止 Host（这会停止所有后台服务和异步任务）
                    try
                    {
                        if (_host != null)
                        {
                            await _host.StopAsync(TimeSpan.FromSeconds(5));
                            _host.Dispose();
                            _host = null;
                        }
                    }
                    catch (Exception hostEx)
                    {
                        WriteDiagnosticLog($"⚠️ [App.xaml.cs] 停止 Host 时发生异常: {hostEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"⚠️ [App.xaml.cs] 停止 Host 时发生异常: {hostEx.Message}");
                    }
                    
                    // 显示错误消息框并等待用户处理
                    // 使用标准 MessageBox，因为在启动早期 TaktMessageBox 可能无法正常工作
                    // 使用 InvokeAsync 确保在 UI 线程上同步执行，阻塞等待用户操作
                    // 使用资源文件进行本地化（不依赖数据库）
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        var message = ResourceFileLocalizationHelper.GetString("database.connectionerror.failed_detail");
                        var title = ResourceFileLocalizationHelper.GetString("database.initialization.title");
                        
                        MessageBox.Show(
                            message,
                            title,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        
                        // 用户点击确定后，关闭应用程序
                        this.Shutdown();
                    }, System.Windows.Threading.DispatcherPriority.Send);
                    
                    // 抛出异常以确保后续初始化不会执行
                    throw new InvalidOperationException("数据库连接失败，应用程序已停止");
                }

                // 检查关键表是否存在（使用用户表作为检查标准）
                var db = dbContext.Db;
                var userTableExists = db.DbMaintenance.IsAnyTable("takt_oidc_user");
                var menuTableExists = db.DbMaintenance.IsAnyTable("takt_oidc_menu");

                if (!userTableExists || !menuTableExists)
                {
                    // 数据库表不存在，立即停止所有初始化进程并显示错误信息
                    WriteDiagnosticLog("❌ [App.xaml.cs] 数据库表不存在，停止所有初始化进程");
                    System.Diagnostics.Debug.WriteLine("❌ [App.xaml.cs] 数据库表不存在，停止所有初始化进程");
                    
                    // 停止 Host（这会停止所有后台服务和异步任务）
                    try
                    {
                        if (_host != null)
                        {
                            await _host.StopAsync(TimeSpan.FromSeconds(5));
                            _host.Dispose();
                            _host = null;
                        }
                    }
                    catch (Exception hostEx)
                    {
                        WriteDiagnosticLog($"⚠️ [App.xaml.cs] 停止 Host 时发生异常: {hostEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"⚠️ [App.xaml.cs] 停止 Host 时发生异常: {hostEx.Message}");
                    }
                    
                    // 显示错误消息框并等待用户处理
                    // 使用标准 MessageBox，因为在启动早期 TaktMessageBox 可能无法正常工作
                    // 使用 InvokeAsync 确保在 UI 线程上同步执行，阻塞等待用户操作
                    // 使用资源文件进行本地化（不依赖数据库）
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        var message = ResourceFileLocalizationHelper.GetString("database.tables_not_initialized.message");
                        var title = ResourceFileLocalizationHelper.GetString("database.tables_not_initialized.title");
                        
                        MessageBox.Show(
                            message,
                            title,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        
                        // 用户点击确定后，关闭应用程序
                        this.Shutdown();
                    }, System.Windows.Threading.DispatcherPriority.Send);
                    
                    // 抛出异常以确保后续初始化不会执行
                    throw new InvalidOperationException("数据库未初始化，应用程序已停止");
                }

                WriteDiagnosticLog("🟣 [App.xaml.cs] 数据库检查通过，表已存在");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 数据库检查通过，表已存在");
            }

            // 初始化数据库表
            System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 准备解析 DbTableInitializer");
            WriteDiagnosticLog("🟣 [App.xaml.cs] 准备解析 DbTableInitializer");
            
            // 更新初始化状态：正在初始化数据库表
            InitializationStatusManager.UpdateStatus(
                InitializationStatus.Initializing,
                ResourceFileLocalizationHelper.GetString("login.initialization.database", "正在初始化数据库表..."));
            
            var dbTableInitializer = Services.GetRequiredService<Takt.Infrastructure.Data.DbTableInitializer>();
            
            System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] DbTableInitializer 解析成功");
            WriteDiagnosticLog("🟣 [App.xaml.cs] DbTableInitializer 解析成功");
            
            await dbTableInitializer.InitializeAsync();

            // 初始化种子数据（如果启用）
            if (databaseSettings.EnableSeedData)
            {
                // 更新初始化状态：正在初始化种子数据
                InitializationStatusManager.UpdateStatus(
                    InitializationStatus.Initializing,
                    ResourceFileLocalizationHelper.GetString("login.initialization.seeddata", "正在初始化种子数据..."));
                
                // 临时禁用差异日志，避免启动时连接冲突
                // 种子数据初始化不应该记录差异日志
                WriteDiagnosticLog("🟣 [App.xaml.cs] 准备禁用差异日志（种子数据初始化前）");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 准备禁用差异日志（种子数据初始化前）");
                Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(false);
                
                try
                {
                    // 使用协调器统一执行所有种子数据初始化
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 开始执行种子数据初始化");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 开始执行种子数据初始化");
                    var dbSeedCoordinator = Services.GetRequiredService<Takt.Infrastructure.Data.DbSeedCoordinator>();
                    await dbSeedCoordinator.InitializeAsync();
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 种子数据初始化完成");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 种子数据初始化完成");
                }
                finally
                {
                    // 种子数据初始化完成后，重新启用差异日志
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 准备启用差异日志（种子数据初始化后）");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 准备启用差异日志（种子数据初始化后）");
                    Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(true);
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 差异日志已启用");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 差异日志已启用");
                }
            }
            else
            {
                // 如果种子数据未启用，确保差异日志是启用的
                WriteDiagnosticLog("🟣 [App.xaml.cs] 种子数据未启用，确保差异日志已启用");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 种子数据未启用，确保差异日志已启用");
                Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(true);
            }

            // 初始化主题服务
            var themeService = Services.GetRequiredService<ThemeService>();
            themeService.InitializeTheme();

            // 初始化本地化（预加载翻译）
            var localizationManager = Services.GetRequiredService<ILocalizationManager>();
            // 先初始化 LocalizationManager（加载语言列表和默认语言翻译）
            await localizationManager.InitializeAsync();
            
            // 然后切换到保存的语言（如果与默认语言不同）
            var savedLang = Takt.Common.Helpers.AppSettingsHelper.GetLanguage();
            if (!string.IsNullOrWhiteSpace(savedLang) && savedLang != localizationManager.CurrentLanguage)
            {
                localizationManager.ChangeLanguage(savedLang);
            }

            // 初始化并启动 Quartz 调度器，从数据库加载任务
            try
            {
                StartupLogManager?.Information("开始初始化 Quartz 调度器...");
                var quartzSchedulerManager = Services.GetRequiredService<Takt.Domain.Interfaces.IQuartzSchedulerManager>();
                await quartzSchedulerManager.InitializeAsync();
                await quartzSchedulerManager.StartAsync();
                StartupLogManager?.Information("Quartz 调度器初始化完成");
                
                StartupLogManager?.Information("开始从数据库加载 Quartz 任务...");
                await quartzSchedulerManager.LoadJobsFromDatabaseAsync();
                StartupLogManager?.Information("从数据库加载 Quartz 任务完成");
            }
            catch (Exception ex)
            {
                StartupLogManager?.Error(ex, "初始化或加载 Quartz 调度器失败，但应用将继续启动");
                operLog?.Error(ex, "初始化或加载 Quartz 调度器失败");
            }

            operLog?.Information("应用程序初始化完成");
        }
        catch (Exception ex)
        {
            var operLog = Services.GetService<OperLogManager>();
            operLog?.Error(ex, "应用程序初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 创建 Host 构建器
    /// </summary>
    private IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context, services);
            })
            .ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                ConfigureAutofacContainer(context, builder);
            })
            .UseSerilog((context, configuration) =>
            {
                // 使用符合 Windows 规范的日志目录（AppData\Local）
                var logDirectory = Takt.Common.Helpers.PathHelper.GetLogDirectory();
                var logFilePath = Path.Combine(logDirectory, "app-.txt");

                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: logFilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        fileSizeLimitBytes: 8 * 1024 * 1024,  // 单个文件最大 8MB
                        rollOnFileSizeLimit: true,  // 达到文件大小限制时自动滚动
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        encoding: System.Text.Encoding.UTF8);
            });
    }

    /// <summary>
    /// 配置服务（表现层服务、View、ViewModel）
    /// </summary>
    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // ========== 1. 全局服务（Singleton）==========
        services.AddSingleton<ThemeService>();

        services.AddSingleton<LocalizationNotifyProperty>(sp =>
        {
            var localizationManager = sp.GetRequiredService<ILocalizationManager>();
            return new LocalizationNotifyProperty(localizationManager);
        });

        // ========== 2. 主窗口和主窗口 ViewModel（Singleton）==========
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        // ========== 3. 登录窗口和登录 ViewModel ==========
        services.AddSingleton<LoginView>();
        services.AddTransient<LoginViewModel>(); // 每次登录可能需要新实例

        // ========== 4. 业务 ViewModel 和 View（Transient）==========
        // Dashboard
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardView>();

        // Identity 模块
        services.AddTransient<Views.Identity.IdentityPage>();
        services.AddTransient<UserViewModel>();
        services.AddTransient<UserView>();
        services.AddTransient<UserFormViewModel>();
        services.AddTransient<UserForm>();
        services.AddTransient<UserProfileViewModel>();
        services.AddTransient<UserProfile>();
        services.AddTransient<UserAssignRoleViewModel>();
        services.AddTransient<UserAssignRole>();
        services.AddTransient<RoleViewModel>();
        services.AddTransient<RoleView>();
        services.AddTransient<RoleFormViewModel>();
        services.AddTransient<RoleForm>();
        services.AddTransient<RoleAssignMenuViewModel>();
        services.AddTransient<RoleAssignMenu>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<MenuView>();
        services.AddTransient<MenuFormViewModel>();
        services.AddTransient<MenuForm>();

        // Routine 模块
        services.AddTransient<Views.Routine.RoutinePage>();
        services.AddTransient<LocalizationViewModel>();
        services.AddTransient<LocalizationView>();
        services.AddTransient<DictionaryViewModel>();
        services.AddTransient<DictionaryView>();
        services.AddTransient<DictionaryFormViewModel>();
        services.AddTransient<Views.Routine.DictionaryComponent.DictionaryForm>();
        services.AddTransient<SettingViewModel>();
        services.AddTransient<SettingView>();
        services.AddTransient<SettingFormViewModel>();
        services.AddTransient<SettingForm>();
           services.AddTransient<QuartzJobViewModel>();
           services.AddTransient<QuartzJobView>();
           services.AddTransient<QuartzJobFormViewModel>();
           services.AddTransient<Views.Routine.QuartzJobComponent.QuartzJobForm>();

        // Logging 模块
        services.AddTransient<Views.Logging.LoggingPage>();
        services.AddTransient<OperLogViewModel>();
        services.AddTransient<OperLogView>();
        services.AddTransient<LoginLogViewModel>();
        services.AddTransient<LoginLogView>();
        services.AddTransient<DiffLogViewModel>();
        services.AddTransient<DiffLogView>();
        services.AddTransient<QuartzJobLogViewModel>();
        services.AddTransient<QuartzJobLogView>();

        // Logistics 模块
        services.AddTransient<Views.Logistics.LogisticsPage>();
        services.AddTransient<Views.Logistics.Materials.MaterialsPage>();
        services.AddTransient<MaterialViewModel>();
        services.AddTransient<MaterialView>();
        services.AddTransient<PackingViewModel>();
        services.AddTransient<Views.Logistics.Materials.PackingView>();
        services.AddTransient<ModelViewModel>();
        services.AddTransient<ModelView>();
        services.AddTransient<Views.Logistics.Serials.SerialsPage>();
        services.AddTransient<SerialInboundViewModel>();
        services.AddTransient<SerialInboundView>();
        services.AddTransient<SerialInboundFormViewModel>();
        services.AddTransient<Views.Logistics.Serials.SerialComponent.SerialInboundForm>();
        services.AddTransient<SerialOutboundViewModel>();
        services.AddTransient<SerialOutboundView>();
        services.AddTransient<SerialOutboundFormViewModel>();
        services.AddTransient<Views.Logistics.Serials.SerialComponent.SerialOutboundForm>();
        services.AddTransient<SerialScanningViewModel>();
        services.AddTransient<SerialScanningView>();
        services.AddTransient<Views.Logistics.Visits.VisitsPage>();
        services.AddTransient<ViewModels.Logistics.Visits.VisitingViewModel>();
        services.AddTransient<Views.Logistics.Visits.VisitingView>();
        services.AddTransient<ViewModels.Logistics.Visits.VisitingFormViewModel>();
        services.AddTransient<Views.Logistics.Visits.VisitsComponent.VisitingForm>();
        services.AddTransient<WelcomeSignViewModel>();
        services.AddTransient<WelcomeSignView>();

        // Settings 模块
        services.AddTransient<MySettingsViewModel>();
        services.AddTransient<MySettingsView>();

        // About 模块
        services.AddTransient<AboutView>();
        services.AddTransient<MySystemView>();

        // Generator 模块
        services.AddTransient<Views.Generator.GeneratorPage>();
        services.AddTransient<CodeGeneratorViewModel>();
        services.AddTransient<CodeGeneratorView>();
        services.AddTransient<CodeGenFormViewModel>();
        services.AddTransient<CodeGenForm>();
        services.AddTransient<ImportTableViewModel>();
        services.AddTransient<ImportTableView>();
    }

    /// <summary>
    /// 配置 Autofac 容器（应用层服务、基础设施层服务）
    /// </summary>
    private void ConfigureAutofacContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        // 获取配置
        var configuration = context.Configuration;
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("未找到数据库连接字符串 'DefaultConnection'");

        var databaseSettings = configuration.GetSection("DatabaseSettings").Get<HbtDatabaseSettings>()
            ?? new HbtDatabaseSettings();

        // 注册 Autofac 模块
        builder.RegisterModule(new AutofacModule(connectionString, databaseSettings));
    }


    /// <summary>
    /// 应用程序关闭
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
    
    /// <summary>
    /// 写入诊断日志到文件
    /// </summary>
    private static void WriteDiagnosticLog(string message)
    {
        try
        {
            var logDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();
            var logFile = Path.Combine(logDir, "diagnostic.log");
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\r\n";
            File.AppendAllText(logFile, logMessage);
        }
        catch
        {
            // 忽略文件写入错误
        }
    }
}


