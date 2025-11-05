//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : App.xaml.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : WPF应用程序入口点（参照WPFGallery实现）
//===================================================================

using System.Windows;
using Hbt.Application.Services.Identity;
using Hbt.Common.Config;
using Hbt.Common.Logging;
using Hbt.Domain.Entities.Identity;
using Hbt.Domain.Entities.Logging;
using Hbt.Domain.Repositories;
using MenuEntity = Hbt.Domain.Entities.Identity.Menu; // 别名避免与 System.Windows.Controls.Menu 冲突
using Hbt.Fluent.Services;
using Hbt.Fluent.ViewModels.Identity;
using Hbt.Fluent.Views;
using Hbt.Fluent.Views.Identity;
using Hbt.Infrastructure.Data;
using Hbt.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hbt.Infrastructure.DependencyInjection;

namespace Hbt.Fluent;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .UseSerilog((context, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            // 应用程序通用日志：logs/app-*.txt
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/app-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8))
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureServices((context, services) =>
        {
            // 注册主题服务
            services.AddSingleton<ThemeService>();

            // 注册语言服务（WPF）
            services.AddSingleton<Hbt.Fluent.Services.LanguageService>();

            // 注册窗口和ViewModel
            services.AddSingleton<LoginWindow>();
            services.AddTransient<LoginViewModel>(); // 改为 Transient，因为每次登录可能需要新实例
            
            services.AddSingleton<ViewModels.MainWindowViewModel>();
            services.AddSingleton<MainWindow>();

            // 用户管理 ViewModel
            services.AddTransient<Hbt.Fluent.ViewModels.Identity.UserViewModel>();
            services.AddTransient<Hbt.Fluent.ViewModels.Identity.UserFormViewModel>();
            services.AddTransient<Hbt.Fluent.Views.Identity.UserFormWindow>();
            
            // 用户信息 ViewModel
            services.AddTransient<Hbt.Fluent.ViewModels.Identity.UserInfoViewModel>();
            services.AddTransient<Hbt.Fluent.Views.Identity.UserInfoWindow>();

            // 导航页面
            services.AddTransient<Hbt.Fluent.ViewModels.Identity.IdentityPageViewModel>();
            services.AddTransient<Hbt.Fluent.Views.Identity.IdentityPage>();
            
            // 用户管理视图（用于导航）
            services.AddTransient<Hbt.Fluent.Views.Identity.UserView>();
            
            // 设置页面
            services.AddTransient<Hbt.Fluent.ViewModels.Settings.SettingsViewModel>();
            services.AddTransient<Hbt.Fluent.Views.Settings.SettingsView>();
        })
        .ConfigureContainer<ContainerBuilder>((context, builder) =>
        {
            // 统一由 Autofac 注册应用与基础设施服务
            var connSection = context.Configuration.GetSection("ConnectionStrings");
            var allConns = connSection.GetChildren().ToList();
            if (allConns.Count != 1)
            {
                throw new InvalidOperationException("配置错误：必须且只能配置一个连接字符串（ConnectionStrings 下仅允许一个条目）。");
            }
            var connectionString = allConns.First().Value;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("配置错误：连接字符串不能为空，请在 appsettings.json 的 ConnectionStrings 中正确配置。");
            }

            var databaseSettings = context.Configuration.GetSection("DatabaseSettings").Get<HbtDatabaseSettings>() 
                ?? new HbtDatabaseSettings();

            builder.RegisterModule(new AutofacModule(connectionString!, databaseSettings));
        })
        .Build();

    public static IServiceProvider? Services => _host?.Services;

    [STAThread]
    public static void Main()
    {
        try
        {
            // 检查 Host 是否已构建
            if (_host == null)
            {
                throw new InvalidOperationException("Host 构建失败，无法启动应用");
            }

            // 启动 Host
            try
            {
                _host.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"启动 Host 失败: {ex.Message}", ex);
            }

            // 检查 Services 是否可用（必须在 Start 之后检查）
            if (_host.Services == null)
            {
                throw new InvalidOperationException("Host Services 为 null，Host 可能未正确启动");
            }

            // 创建 App 实例
            App app = new();
            if (app == null)
            {
                throw new InvalidOperationException("创建 App 实例失败");
            }

            // 初始化 App 组件，确保 Application.Current 可用
            app.InitializeComponent();

            try
            {
                // 先进行建库建表（CodeFirst）
                var tableInitializer = _host.Services.GetRequiredService<Hbt.Infrastructure.Data.DbTableInitializer>();
                if (tableInitializer != null)
                {
                    tableInitializer.InitializeAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Error(ex, "[启动] 初始化数据库表失败");
                // 继续执行，允许应用启动
            }

            try
            {
                // 执行 RBAC 种子（用户/角色/菜单/按钮等）
                var seedRbac = _host.Services.GetRequiredService<Hbt.Infrastructure.Data.DbSeedRbac>();
                if (seedRbac != null)
                {
                    seedRbac.InitializeAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Error(ex, "[启动] 初始化 RBAC 种子失败");
                // 继续执行，允许应用启动
            }

            try
            {
                // 执行 Routine 模块种子（语言/翻译/字典/设置）
                var seedRoutine = _host.Services.GetRequiredService<Hbt.Infrastructure.Data.DbSeedRoutine>();
                if (seedRoutine != null)
                {
                    seedRoutine.Initialize();
                }
            }
            catch (Exception ex)
            {
                var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Error(ex, "[启动] 初始化 Routine 种子失败");
                // 继续执行，允许应用启动
            }

            try
            {
                // 初始化主题（在 Application 初始化之后）
                var themeService = _host.Services.GetRequiredService<ThemeService>();
                if (themeService != null)
                {
                    themeService.InitializeTheme();
                }
            }
            catch (Exception ex)
            {
                var operLog = App.Services?.GetService<OperLogManager>();
                operLog?.Error(ex, "[启动] 初始化主题失败");
                // 继续执行，允许应用启动
            }

            // 获取并显示登录窗口
            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            if (loginWindow == null)
            {
                throw new InvalidOperationException("无法创建 LoginWindow 实例");
            }

            app.MainWindow = loginWindow;
            if (app.MainWindow != null)
            {
                app.MainWindow.Visibility = Visibility.Visible;
            }
            app.Run();
        }
        catch (Exception ex)
        {
            // 记录详细的异常信息
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[启动] 应用启动失败");
            
            // 显示错误消息
            MessageBox.Show(
                $"应用程序启动失败：{ex.Message}\n\n详细错误信息请查看日志。",
                "启动错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // 确保应用退出
            Environment.Exit(1);
        }
    }
}


