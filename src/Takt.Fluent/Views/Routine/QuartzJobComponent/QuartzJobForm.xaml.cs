// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine.QuartzJobComponent
// 文件名称：QuartzJobForm.xaml.cs
// 创建时间：2025-12-01
// 创建人：Takt365(Cursor AI)
// 功能描述：任务表单窗口（新建/编辑任务）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows;
using Takt.Domain.Interfaces;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine.QuartzJobComponent;

/// <summary>
/// 任务表单窗口（新建/编辑任务）
/// </summary>
public partial class QuartzJobForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private QuartzJobFormViewModel? _viewModel;

    /// <summary>
    /// 初始化任务表单窗口
    /// </summary>
    /// <param name="vm">任务表单视图模型</param>
    /// <param name="localizationManager">本地化管理器</param>
    public QuartzJobForm(QuartzJobFormViewModel vm, ILocalizationManager? localizationManager = null)
    {
        InitializeComponent();
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _viewModel = vm;
        DataContext = vm;

        // 设置 Owner（如果还没有设置）
        if (Owner == null)
        {
            Owner = System.Windows.Application.Current?.MainWindow;
        }

        Loaded += (s, e) =>
        {
            // 延迟计算高度，确保 UI 已完全渲染
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                // 居中窗口
                CenterWindow();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        };
    }

    /// <summary>
    /// 居中窗口到父窗口或屏幕
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            // 相对于父窗口居中，默认大小为父窗口的80%
            Width = Owner.ActualWidth * 0.8;
            Height = Owner.ActualHeight * 0.8;

            // 计算居中位置
            Left = Owner.Left + (Owner.ActualWidth - Width) / 2;
            Top = Owner.Top + (Owner.ActualHeight - Height) / 2;
        }
        else
        {
            // 相对于屏幕居中
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            if (Width == 0 || double.IsNaN(Width))
            {
                Width = Math.Min(900, screenWidth * 0.6);
            }
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Math.Min(700, screenHeight * 0.7);
            }

            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }
}

