// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Modules
// 文件名称：GeneratorModule.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成模块
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Prism.Ioc;
using Prism.Modularity;
using Takt.Fluent.ViewModels.Generator;
using Takt.Fluent.Views.Generator;
using Takt.Fluent.Views.Generator.CodeGenComponent;

namespace Takt.Fluent.Modules;

/// <summary>
/// 代码生成模块
/// </summary>
public class GeneratorModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        // 模块初始化后的逻辑（如果需要）
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册 Generator 模块的 Views 和 ViewModels
        containerRegistry.RegisterForNavigation<GeneratorPage>();
        containerRegistry.RegisterForNavigation<CodeGeneratorView, CodeGeneratorViewModel>();
        containerRegistry.RegisterForNavigation<ImportTableView, ImportTableViewModel>();
        containerRegistry.RegisterForNavigation<CodeGenForm, CodeGenFormViewModel>();
        
        // 注册 ViewModels（Transient 生命周期）
        containerRegistry.Register<CodeGeneratorViewModel>();
        containerRegistry.Register<ImportTableViewModel>();
        containerRegistry.Register<CodeGenFormViewModel>();
    }
}

