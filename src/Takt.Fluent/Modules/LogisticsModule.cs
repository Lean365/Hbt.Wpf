// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Modules
// 文件名称：LogisticsModule.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：后勤模块（物料、序列号、随行人员管理）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Prism.Ioc;
using Prism.Modularity;
using Takt.Fluent.ViewModels.Logistics.Materials;
using Takt.Fluent.ViewModels.Logistics.Serials;
using Takt.Fluent.ViewModels.Logistics.Visits;
using Takt.Fluent.Views.Logistics;
using Takt.Fluent.Views.Logistics.Materials;
using Takt.Fluent.Views.Logistics.Serials;
using Takt.Fluent.Views.Logistics.Serials.SerialComponent;
using Takt.Fluent.Views.Logistics.Visits;
using Takt.Fluent.Views.Logistics.Visits.VisitsComponent;

namespace Takt.Fluent.Modules;

/// <summary>
/// 后勤模块
/// </summary>
public class LogisticsModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        // 模块初始化后的逻辑（如果需要）
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册 Logistics 模块的 Views 和 ViewModels
        containerRegistry.RegisterForNavigation<LogisticsPage>();
        
        // 物料管理
        containerRegistry.RegisterForNavigation<MaterialsPage>();
        containerRegistry.RegisterForNavigation<MaterialView, MaterialViewModel>();
        containerRegistry.RegisterForNavigation<PackingView, PackingViewModel>();
        containerRegistry.RegisterForNavigation<ModelView, ModelViewModel>();
        
        // 序列号管理
        containerRegistry.RegisterForNavigation<SerialsPage>();
        containerRegistry.RegisterForNavigation<SerialInboundView, SerialInboundViewModel>();
        containerRegistry.RegisterForNavigation<SerialInboundForm, SerialInboundFormViewModel>();
        containerRegistry.RegisterForNavigation<SerialOutboundView, SerialOutboundViewModel>();
        containerRegistry.RegisterForNavigation<SerialOutboundForm, SerialOutboundFormViewModel>();
        containerRegistry.RegisterForNavigation<SerialScanningView, SerialScanningViewModel>();
        
        // 随行人员管理
        containerRegistry.RegisterForNavigation<VisitsPage>();
        containerRegistry.RegisterForNavigation<VisitingView, VisitingViewModel>();
        containerRegistry.RegisterForNavigation<VisitingForm, VisitingFormViewModel>();
        containerRegistry.RegisterForNavigation<WelcomeSignView, WelcomeSignViewModel>();
        
        // 注册 ViewModels（Transient 生命周期）
        containerRegistry.Register<MaterialViewModel>();
        containerRegistry.Register<PackingViewModel>();
        containerRegistry.Register<ModelViewModel>();
        containerRegistry.Register<SerialInboundViewModel>();
        containerRegistry.Register<SerialInboundFormViewModel>();
        containerRegistry.Register<SerialOutboundViewModel>();
        containerRegistry.Register<SerialOutboundFormViewModel>();
        containerRegistry.Register<SerialScanningViewModel>();
        containerRegistry.Register<VisitingViewModel>();
        containerRegistry.Register<VisitingFormViewModel>();
        containerRegistry.Register<WelcomeSignViewModel>();
    }
}

