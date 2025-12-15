// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.DependencyInjection
// 文件名称：AutofacModule.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：Autofac依赖注入模块
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// 
// ========================================

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Takt.Application.Services.Identity;
using Takt.Application.Services.Routine;
using Takt.Common.Config;
using Takt.Common.Logging;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Interfaces;
using Takt.Domain.Repositories;
using Takt.Infrastructure.Data;
using Takt.Infrastructure.Repositories;
using Takt.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Takt.Infrastructure.DependencyInjection;

/// <summary>
/// Autofac依赖注入模块
/// 注册应用程序所需的所有服务
/// </summary>
public class AutofacModule : Module
{
    private readonly string _connectionString;
    private readonly HbtDatabaseSettings _databaseSettings;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="databaseSettings">数据库配置</param>
    public AutofacModule(string connectionString, HbtDatabaseSettings databaseSettings)
    {
        _connectionString = connectionString;
        _databaseSettings = databaseSettings;
    }

    /// <summary>
    /// 加载模块，注册服务
    /// </summary>
    /// <param name="builder">容器构建器</param>
    protected override void Load(ContainerBuilder builder)
    {
        // 注册基础仓储（必须先注册，因为 LogDatabaseWriter 依赖它）
        builder.RegisterGeneric(typeof(BaseRepository<>))
            .As(typeof(IBaseRepository<>))
            .InstancePerLifetimeScope();

        // 注册日志数据库写入器（依赖 Repository，必须在 DbContext 之前注册）
        builder.RegisterType<Takt.Infrastructure.Logging.LogDatabaseWriter>()
            .As<Takt.Common.Logging.ILogDatabaseWriter>()
            .InstancePerLifetimeScope()
            .OnActivated(e =>
            {
                // 在 ILogDatabaseWriter 激活后，设置到 SqlSugarAop 的静态引用
                // 这样 OnDiffLogEvent 就可以使用它来保存差异日志到数据库
                SqlSugarAop.SetLogDatabaseWriter(e.Instance);
                
                // 同时设置到 OperLogManager 的静态引用
                // 这样 OperLogManager 即使在 SingleInstance 创建时无法解析，也能通过静态引用使用
                Takt.Common.Logging.OperLogManager.SetLogDatabaseWriter(e.Instance);
                
                System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] ILogDatabaseWriter 已设置到 SqlSugarAop 和 OperLogManager");
            });

        // 注册 AppLogManager（必须在 DbContext 之前注册，以便传递给 SqlSugarAop）
        builder.RegisterType<AppLogManager>()
            .AsSelf()
            .SingleInstance()
            .OnActivated(e =>
            {
                // 在 AppLogManager 激活后，设置到 SqlSugarAop 的静态引用
                // 这样 SqlSugarAop 就可以使用统一日志处理
                SqlSugarAop.SetAppLogManager(e.Instance);
            });

        // 注册数据库上下文（不依赖 LogDatabaseWriter，避免循环依赖）
        // LogDatabaseWriter 会通过 OnActivated 回调设置到 SqlSugarAop 的静态引用
        builder.Register(c => 
        {
            // 添加诊断日志，确认注册代码被执行
            System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] 开始解析 DbContext");
            WriteDiagnosticLog("🟢 [AutofacModule] 开始解析 DbContext");
            
            var logger = c.Resolve<ILogger>();
            System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] ILogger 解析成功");
            WriteDiagnosticLog("🟢 [AutofacModule] ILogger 解析成功");
            
            var appLog = c.Resolve<AppLogManager>();
            System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] AppLogManager 解析成功");
            WriteDiagnosticLog("🟢 [AutofacModule] AppLogManager 解析成功");
            
            // 不在这里解析 LogDatabaseWriter，避免循环依赖
            // LogDatabaseWriter 会在后续通过 OnActivated 回调设置到 SqlSugarAop
            System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] 准备创建 DbContext 实例");
            WriteDiagnosticLog("🟢 [AutofacModule] 准备创建 DbContext 实例");
            
            var dbContext = new DbContext(_connectionString, logger, _databaseSettings, null, appLog);
            
            System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] DbContext 实例创建完成");
            WriteDiagnosticLog("🟢 [AutofacModule] DbContext 实例创建完成");
            
            return dbContext;
        })
            .AsSelf()
            .SingleInstance()
            .OnActivated(e =>
            {
                // 在 DbContext 激活后，尝试解析 ILogDatabaseWriter 并设置到 SqlSugarAop
                // 此时 ILogDatabaseWriter 应该已经被创建（如果已解析过）
                try
                {
                    var logDatabaseWriter = e.Context.ResolveOptional<Takt.Common.Logging.ILogDatabaseWriter>();
                    if (logDatabaseWriter != null)
                    {
                        SqlSugarAop.SetLogDatabaseWriter(logDatabaseWriter);
                        System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] DbContext OnActivated: ILogDatabaseWriter 已设置到 SqlSugarAop");
                        WriteDiagnosticLog("🟢 [AutofacModule] DbContext OnActivated: ILogDatabaseWriter 已设置到 SqlSugarAop");
                    }
                }
                catch
                {
                    // 忽略，ILogDatabaseWriter 可能还未创建
                }
            });

        // 注册数据表初始化服务
        builder.RegisterType<Takt.Infrastructure.Data.DbTableInitializer>()
            .AsSelf()
            .SingleInstance();

        // 注册 RBAC 种子数据初始化服务
        builder.RegisterType<Takt.Infrastructure.Data.DbSeedRbac>()
            .AsSelf()
            .SingleInstance();

        // 注册翻译种子服务
        builder.RegisterType<Takt.Infrastructure.Data.DbSeedLanguage>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedTranslationCommon>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedTranslationDictionary>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedTranslationEntity>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedTranslationValidation>()
            .AsSelf()
            .SingleInstance();

        // 注册 Routine 模块种子服务
        builder.RegisterType<Takt.Infrastructure.Data.DbSeedRoutineDictionary>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedSetting>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedMenu>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedProdModel>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedVisit>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedQuartz>()
            .AsSelf()
            .SingleInstance();
 
        builder.RegisterType<Takt.Infrastructure.Data.DbSeedCoordinator>()
            .AsSelf()
            .SingleInstance();
 
        // 统一协调器已移除：改为各自独立执行

        // 注册日志
        builder.Register<ILogger>(c => Log.Logger)
            .SingleInstance();

        // 注册日志管理器
        builder.RegisterType<InitLogManager>()
            .AsSelf()
            .SingleInstance();

        // 注册操作日志管理器（使用 ResolveOptional 避免解析失败，后续通过静态引用机制获取）
        builder.Register(c =>
        {
            var logger = c.Resolve<ILogger>();
            // 使用 ResolveOptional，如果解析失败返回 null
            // OperLogManager 会通过静态引用机制在运行时获取 ILogDatabaseWriter
            var logDatabaseWriter = c.ResolveOptional<Takt.Common.Logging.ILogDatabaseWriter>();
            return new OperLogManager(logger, logDatabaseWriter);
        })
            .AsSelf()
            .SingleInstance()
            .OnActivated(e =>
            {
                // 在 OperLogManager 激活后，验证 ILogDatabaseWriter 是否可用
                var operLogManager = e.Instance;
                System.Diagnostics.Debug.WriteLine("🟢 [AutofacModule] OperLogManager 已激活");
                WriteDiagnosticLog("🟢 [AutofacModule] OperLogManager 已激活");
            });

        // 通过批量注册自动注册所有 *Service 结尾的应用层服务

        // 自动注册所有以Service结尾的类
        builder.RegisterAssemblyTypes(typeof(IUserService).Assembly)
            .Where(t => t.Name.EndsWith("Service"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // 注册本地化管理器（基础设施层实现 -> 领域层接口）
        builder.RegisterType<LocalizationManager>()
            .As<ILocalizationManager>()
            .SingleInstance();

        // 注册数据库元数据服务（基础设施层实现 -> 领域层接口）
        builder.RegisterType<DatabaseMetadataService>()
            .As<IDatabaseMetadataService>()
            .SingleInstance();

        // 注册序列号管理器（基础设施层实现 -> 领域层接口）
        builder.RegisterType<SerialsManager>()
            .As<ISerialsManager>()
            .InstancePerLifetimeScope();

        // 注意：IServiceProvider 不在 AutofacModule 中注册
        // 而是在 PrismBootstrapper 中容器构建完成后注册
        // 这样可以避免在容器构建过程中解析 IContainer 的问题

        // 注册 Quartz 调度器管理器（基础设施层实现 -> 领域层接口）
        builder.RegisterType<QuartzSchedulerManager>()
            .As<IQuartzSchedulerManager>()
            .SingleInstance();

        // 注册 Quartz Job 类（每次执行任务时创建新实例）
        builder.RegisterType<Takt.Infrastructure.Jobs.GenericServiceJob>()
            .AsSelf()
            .InstancePerDependency();

        // 注册日志清理服务（应用层服务，已通过批量注册自动注册接口，这里注册实现）
        // LogCleanupService 已通过批量注册自动注册为 ILogCleanupService

        // 注册日志清理后台服务（每月1号0点执行，只保留最近7天的日志）
        builder.RegisterType<Takt.Infrastructure.Services.LogCleanupBackgroundService>()
            .As<Microsoft.Extensions.Hosting.IHostedService>()
            .SingleInstance();

        // 注意：CodeGeneratorService 在 Application 层，已通过批量注册自动注册
        // GenTableService 和 GenColumnService 也在 Application 层，已通过批量注册自动注册
    }
    
    /// <summary>
    /// 写入诊断日志到文件
    /// </summary>
    private static void WriteDiagnosticLog(string message)
    {
        try
        {
            var logDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();
            var logFile = System.IO.Path.Combine(logDir, "diagnostic.log");
            var now = DateTime.Now;
            var logMessage = $"{now:yyyy-MM-dd HH:mm:ss.fff zzz} [DBG] {message}\r\n";
            System.IO.File.AppendAllText(logFile, logMessage);
            // 同时输出到 Debug，确保与 TimestampedDebug 一致
            System.Diagnostics.Debug.WriteLine(message);
        }
        catch
        {
            // 忽略文件写入错误
        }
    }
}
