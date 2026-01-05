// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Modules
// 文件名称：LoggingModule.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：日志模块（操作日志、登录日志、差异日志）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Prism.Ioc;
using Prism.Modularity;
using Takt.Fluent.ViewModels.Logging;
using Takt.Fluent.Views.Logging;

namespace Takt.Fluent.Modules;

/// <summary>
/// 日志模块
/// </summary>
public class LoggingModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        // 模块初始化后的逻辑（如果需要）
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册 Logging 模块的 Views 和 ViewModels
        containerRegistry.RegisterForNavigation<LoggingPage>();
        containerRegistry.RegisterForNavigation<OperLogView, OperLogViewModel>();
        containerRegistry.RegisterForNavigation<LoginLogView, LoginLogViewModel>();
        containerRegistry.RegisterForNavigation<DiffLogView, DiffLogViewModel>();
        containerRegistry.RegisterForNavigation<QuartzJobLogView, QuartzJobLogViewModel>();
        
        // 注册 ViewModels（Transient 生命周期）
        containerRegistry.Register<OperLogViewModel>();
        containerRegistry.Register<LoginLogViewModel>();
        containerRegistry.Register<DiffLogViewModel>();
        containerRegistry.Register<QuartzJobLogViewModel>();
    }
}

