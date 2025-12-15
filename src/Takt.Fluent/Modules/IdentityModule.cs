// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Modules
// 文件名称：IdentityModule.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：身份认证模块（用户、角色、菜单管理）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Prism.Ioc;
using Prism.Modularity;
using Takt.Fluent.ViewModels.Identity;
using Takt.Fluent.Views;
using Takt.Fluent.Views.Identity;
using Takt.Fluent.Views.Identity.MenuComponent;
using Takt.Fluent.Views.Identity.RoleComponent;
using Takt.Fluent.Views.Identity.UserComponent;

namespace Takt.Fluent.Modules;

/// <summary>
/// 身份认证模块
/// </summary>
public class IdentityModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        // 模块初始化后的逻辑（如果需要）
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册 Identity 模块的 Views（Page 使用 NavigationPageViewModel）
        containerRegistry.RegisterForNavigation<IdentityPage>();
        
        // 用户管理
        containerRegistry.RegisterForNavigation<UserView, UserViewModel>();
        containerRegistry.RegisterForNavigation<UserForm, UserFormViewModel>();
        containerRegistry.RegisterForNavigation<UserProfile, UserProfileViewModel>();
        containerRegistry.RegisterForNavigation<UserAssignRole, UserAssignRoleViewModel>();
        
        // 角色管理
        containerRegistry.RegisterForNavigation<RoleView, RoleViewModel>();
        containerRegistry.RegisterForNavigation<RoleForm, RoleFormViewModel>();
        containerRegistry.RegisterForNavigation<RoleAssignMenu, RoleAssignMenuViewModel>();
        
        // 菜单管理
        containerRegistry.RegisterForNavigation<MenuView, MenuViewModel>();
        containerRegistry.RegisterForNavigation<MenuForm, MenuFormViewModel>();
        
        // 注册 ViewModels（Transient 生命周期）
        containerRegistry.Register<UserViewModel>();
        containerRegistry.Register<UserFormViewModel>();
        containerRegistry.Register<UserProfileViewModel>();
        containerRegistry.Register<UserAssignRoleViewModel>();
        containerRegistry.Register<RoleViewModel>();
        containerRegistry.Register<RoleFormViewModel>();
        containerRegistry.Register<RoleAssignMenuViewModel>();
        containerRegistry.Register<MenuViewModel>();
        containerRegistry.Register<MenuFormViewModel>();
    }
}

