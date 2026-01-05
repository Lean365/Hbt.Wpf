// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Modules
// 文件名称：RoutineModule.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：基础模块（多语言、字典、设置、Quartz任务管理）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Prism.Ioc;
using Prism.Modularity;
using Takt.Fluent.ViewModels.Routine;
using Takt.Fluent.Views.Routine;
using Takt.Fluent.Views.Routine.DictionaryComponent;
using Takt.Fluent.Views.Routine.LocalizationComponent;
using Takt.Fluent.Views.Routine.QuartzJobComponent;
using Takt.Fluent.Views.Routine.SettingComponent;

namespace Takt.Fluent.Modules;

/// <summary>
/// 基础模块
/// </summary>
public class RoutineModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        // 模块初始化后的逻辑（如果需要）
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册 Routine 模块的 Views 和 ViewModels
        containerRegistry.RegisterForNavigation<RoutinePage>();
        
        // 多语言管理
        containerRegistry.RegisterForNavigation<LocalizationView, LocalizationViewModel>();
        containerRegistry.RegisterForNavigation<LocalizationForm, LocalizationFormViewModel>();
        
        // 字典管理
        containerRegistry.RegisterForNavigation<DictionaryView, DictionaryViewModel>();
        containerRegistry.RegisterForNavigation<DictionaryForm, DictionaryFormViewModel>();
        
        // 系统设置
        containerRegistry.RegisterForNavigation<SettingView, SettingViewModel>();
        containerRegistry.RegisterForNavigation<SettingForm, SettingFormViewModel>();
        
        // Quartz 任务管理
        containerRegistry.RegisterForNavigation<QuartzJobView, QuartzJobViewModel>();
        containerRegistry.RegisterForNavigation<QuartzJobForm, QuartzJobFormViewModel>();
        
        // 注册 ViewModels（Transient 生命周期）
        containerRegistry.Register<LocalizationViewModel>();
        containerRegistry.Register<LocalizationFormViewModel>();
        containerRegistry.Register<DictionaryViewModel>();
        containerRegistry.Register<DictionaryFormViewModel>();
        containerRegistry.Register<SettingViewModel>();
        containerRegistry.Register<SettingFormViewModel>();
        containerRegistry.Register<QuartzJobViewModel>();
        containerRegistry.Register<QuartzJobFormViewModel>();
    }
}

