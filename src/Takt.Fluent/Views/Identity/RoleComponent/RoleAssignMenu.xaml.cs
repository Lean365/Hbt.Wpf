// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity.RoleComponent
// 文件名称：RoleAssignMenu.xaml.cs
// 创建时间：2025-11-13
// 创建人：Takt365(Cursor AI)
// 功能描述：角色分配菜单窗口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Takt.Fluent.ViewModels.Identity;

namespace Takt.Fluent.Views.Identity.RoleComponent;

/// <summary>
/// 角色分配菜单窗口
/// </summary>
public partial class RoleAssignMenu : Window
{
    public RoleAssignMenuViewModel ViewModel { get; }

    public RoleAssignMenu()
    {
        InitializeComponent();
        ViewModel = App.Services!.GetRequiredService<RoleAssignMenuViewModel>();
        DataContext = ViewModel;
    }
}

