// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Logging
// 文件名称：LoginLogView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：登录日志视图
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Takt.Fluent.ViewModels.Logging;

namespace Takt.Fluent.Views.Logging;

/// <summary>
/// 登录日志视图
/// </summary>
public partial class LoginLogView : UserControl
{
    public LoginLogView()
    {
        InitializeComponent();
        
        if (App.Services != null)
        {
            var viewModel = App.Services.GetRequiredService<LoginLogViewModel>();
            DataContext = viewModel;
        }
    }
}

