// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：AutofacModule.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：Autofac依赖注入模块
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

using Autofac;
using Hbt.Application.Services.Identity;
using Hbt.Common.Config;
using Hbt.Common.Logging;
using Hbt.Domain.Repositories;
using Hbt.Infrastructure.Data;
using Hbt.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Hbt.Infrastructure.DependencyInjection;

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
        // 注册数据库上下文
        builder.Register(c => 
        {
            var logger = c.Resolve<ILogger>();
            return new DbContext(_connectionString, logger, _databaseSettings);
        })
            .AsSelf()
            .SingleInstance();

        // 注册数据表初始化服务
        builder.RegisterType<Hbt.Infrastructure.Data.DbTableInitializer>()
            .AsSelf()
            .SingleInstance();

        // 注册 RBAC 种子数据初始化服务
        builder.RegisterType<Hbt.Infrastructure.Data.DbSeedRbac>()
            .AsSelf()
            .SingleInstance();

        // 注册 Routine 模块种子服务（被 App.xaml.cs 显式解析调用）
        builder.RegisterType<Hbt.Infrastructure.Data.DbSeedRoutine>()
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

        builder.RegisterType<OperLogManager>()
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
    }
}
