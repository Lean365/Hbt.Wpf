// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Logistics.Visitors
// 文件名称：DigitalSignageView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：数字标牌视图代码后台
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Windows.Controls;
using Takt.Fluent.ViewModels.Logistics.Visitors;

namespace Takt.Fluent.Views.Logistics.Visitors;

public partial class DigitalSignageView : UserControl
{
    public DigitalSignageViewModel ViewModel { get; }

    public DigitalSignageView(DigitalSignageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        Loaded += DigitalSignageView_Loaded;
        Unloaded += DigitalSignageView_Unloaded;

        // 监听 ShowVisitorInfo 属性变化，控制视频播放
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.ShowVisitorInfo))
            {
                UpdateVideoPlayback();
            }
        };
    }

    private void DigitalSignageView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // 视图加载时，确保视频播放器状态正确
        UpdateVideoPlayback();
    }

    private void DigitalSignageView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // 视图卸载时，停止视频播放并释放资源
        if (AdVideoPlayer != null)
        {
            AdVideoPlayer.Stop();
        }

        // 释放 ViewModel 资源
        ViewModel?.Dispose();
    }

    /// <summary>
    /// 更新视频播放状态
    /// </summary>
    private void UpdateVideoPlayback()
    {
        if (AdVideoPlayer == null)
            return;

        if (!ViewModel.ShowVisitorInfo)
        {
            // 显示广告视频，开始播放
            if (AdVideoPlayer.Source != null)
            {
                AdVideoPlayer.Play();
            }
        }
        else
        {
            // 显示访客信息，停止视频
            AdVideoPlayer.Stop();
        }
    }

    /// <summary>
    /// 视频加载完成事件处理
    /// </summary>
    private void AdVideoPlayer_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
    {
        // 视频加载完成后，如果应该显示视频，则开始播放
        if (!ViewModel.ShowVisitorInfo && AdVideoPlayer != null)
        {
            AdVideoPlayer.Play();
        }
    }

    /// <summary>
    /// 视频播放结束事件处理（循环播放）
    /// </summary>
    private void AdVideoPlayer_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (AdVideoPlayer != null && !ViewModel.ShowVisitorInfo)
        {
            AdVideoPlayer.Position = TimeSpan.Zero;
            AdVideoPlayer.Play();
        }
    }
}

