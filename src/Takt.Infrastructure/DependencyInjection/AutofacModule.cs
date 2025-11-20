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
        // 注册数据库上下文（必须先注册，因为 Repository 依赖它）
        builder.Register(c => 
        {
            var logger = c.Resolve<ILogger>();
            return new DbContext(_connectionString, logger, _databaseSettings);
        })
            .AsSelf()
            .SingleInstance();

        // 注册基础仓储（依赖 DbContext）
        builder.RegisterGeneric(typeof(BaseRepository<>))
            .As(typeof(IBaseRepository<>))
            .InstancePerLifetimeScope();

        // 注册日志数据库写入器（依赖 Repository）
        builder.RegisterType<Takt.Infrastructure.Logging.LogDatabaseWriter>()
            .As<Takt.Common.Logging.ILogDatabaseWriter>()
            .InstancePerLifetimeScope()
            .OnActivated(e =>
            {
                // 在 ILogDatabaseWriter 激活后，设置到 SqlSugarAop 的静态引用
                // 这样 OnDiffLogEvent 就可以使用它来保存差异日志到数据库
                SqlSugarAop.SetLogDatabaseWriter(e.Instance);
            });

        // 注册数据表初始化服务
        builder.RegisterType<Takt.Infrastructure.Data.DbTableInitializer>()
            .AsSelf()
            .SingleInstance();

        // 注册 RBAC 种子数据初始化服务
        builder.RegisterType<Takt.Infrastructure.Data.DbSeedRbac>()
            .AsSelf()
            .SingleInstance();

        // 注册 Routine 模块种子服务（被 App.xaml.cs 显式解析调用）
        builder.RegisterType<Takt.Infrastructure.Data.DbSeedRoutineDictionary>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedRoutineSetting>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedRoutineEntity>()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<Takt.Infrastructure.Data.DbSeedMenu>()
            .AsSelf()
            .SingleInstance();
 
        builder.RegisterType<Takt.Infrastructure.Data.DbSeedRoutineLanguage>()
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

        builder.RegisterType<AppLogManager>()
            .AsSelf()
            .SingleInstance();

        // 注册日志数据库写入器
        builder.RegisterType<Takt.Infrastructure.Logging.LogDatabaseWriter>()
            .As<Takt.Common.Logging.ILogDatabaseWriter>()
            .InstancePerLifetimeScope();

        // 注册操作日志管理器
        builder.Register(c =>
        {
            var logger = c.Resolve<ILogger>();
            var logDatabaseWriter = c.ResolveOptional<Takt.Common.Logging.ILogDatabaseWriter>();
            return new OperLogManager(logger, logDatabaseWriter);
        })
            .AsSelf()
            .SingleInstance();

        // 注册基础仓储
        builder.RegisterGeneric(typeof(BaseRepository<>))
            .As(typeof(IBaseRepository<>))
            .InstancePerLifetimeScope();

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

        // 注册日志清理服务（应用层服务，已通过批量注册自动注册接口，这里注册实现）
        // LogCleanupService 已通过批量注册自动注册为 ILogCleanupService

        // 注册日志清理后台服务（每月1号0点执行，只保留最近7天的日志）
        builder.RegisterType<Takt.Infrastructure.Services.LogCleanupBackgroundService>()
            .As<Microsoft.Extensions.Hosting.IHostedService>()
            .SingleInstance();

        // 注意：CodeGeneratorService 在 Application 层，已通过批量注册自动注册
        // GenTableService 和 GenColumnService 也在 Application 层，已通过批量注册自动注册
    }
}
