// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine
// 文件名称：QuartzJobView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务视图
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine;

/// <summary>
/// 任务视图
/// </summary>
public partial class QuartzJobView : UserControl
{
    public QuartzJobView()
    {
        InitializeComponent();

        if (App.Services != null)
        {
            var viewModel = App.Services.GetRequiredService<QuartzJobViewModel>();
            DataContext = viewModel;
        }
    }
}

