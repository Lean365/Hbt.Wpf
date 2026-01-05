// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Logistics.Entourage
// 文件名称：WelcomeSignView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：欢迎牌视图代码后台
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using Takt.Common.Logging;
using Takt.Fluent.Adorners;
using Takt.Fluent.ViewModels.Logistics.Visits;

namespace Takt.Fluent.Views.Logistics.Visits;

public partial class WelcomeSignView : UserControl
{
    public WelcomeSignViewModel ViewModel { get; }
    private readonly OperLogManager? _operLog;
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private Media? _currentMedia;
    
    // 全屏状态相关：保存父窗体信息
    private Window? _parentWindow;
    private WindowState _parentWindowState = WindowState.Normal;
    private WindowStyle _parentWindowStyle = WindowStyle.None;
    private LibVLCSharp.WPF.VideoView? _adVideoPlayer; // 保存 AdVideoPlayer 引用，避免在全屏模式下丢失

    public WelcomeSignView(WelcomeSignViewModel viewModel, OperLogManager? operLog = null)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _operLog = operLog;
        DataContext = ViewModel;

        Loaded += WelcomeSignView_Loaded;
        Unloaded += WelcomeSignView_Unloaded;
        SizeChanged += WelcomeSignView_SizeChanged;

        // 监听属性变化：ShowVisitingInfo 和 CurrentVisitingCompany 变化时更新视频播放状态
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.ShowVisitingInfo) || 
                e.PropertyName == nameof(ViewModel.CurrentVisitingCompany) ||
                e.PropertyName == nameof(ViewModel.AdVideoPath))
            {
                _operLog?.Information("[WelcomeSignView] 📢 PropertyChanged: {Property}, ShowVisitingInfo: {Show}, CurrentVisitingCompany: {Company}",
                    e.PropertyName, ViewModel.ShowVisitingInfo, ViewModel.CurrentVisitingCompany != null ? ViewModel.CurrentVisitingCompany.VisitingCompanyName : "null");
                
                // 如果 ShowVisitingInfo 为 true 但 CurrentVisitingCompany 还是 null，说明正在批量设置状态
                // 此时等待 CurrentVisitingCompany 设置完成，避免中间状态导致错误判断
                if (ViewModel.ShowVisitingInfo && ViewModel.CurrentVisitingCompany == null && e.PropertyName == nameof(ViewModel.ShowVisitingInfo))
                {
                    _operLog?.Information("[WelcomeSignView] ⏳ 等待 CurrentVisitingCompany 设置完成，暂不更新视频状态");
                    return;
                }
                
                UpdateVideoPlayback();
            }
            else if (e.PropertyName == nameof(ViewModel.IsFullScreen))
            {
                HandleFullScreenChanged();
            }
            else if (e.PropertyName == nameof(ViewModel.IsEditMode))
            {
                _operLog?.Information("[WelcomeSignView] 📢 PropertyChanged 事件收到 IsEditMode 变化: {IsEdit}", ViewModel.IsEditMode);
                HandleEditModeChanged();
            }
            else if (e.PropertyName == nameof(ViewModel.CurrentVisitingEntourages) || 
                     e.PropertyName == nameof(ViewModel.CurrentVisitingDisplayItems))
            {
                // 当随行人员详情列表或显示项变化时，重新计算并应用字体大小
                _operLog?.Information("[WelcomeSignView] 随行人员详情或显示项变化，重新计算字体大小 - 详情数量: {Count}, 显示项数量: {DisplayCount}", 
                    ViewModel?.CurrentVisitingEntourages?.Count ?? 0,
                    ViewModel?.CurrentVisitingDisplayItems?.Count ?? 0);
                
                // 延迟执行，等待 UI 更新完成
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // 等待ItemsControl的容器生成完成
                    var visitorDetailsItemsControl = FindName("EntourageDetailsItemsControl") as ItemsControl;
                    if (visitorDetailsItemsControl != null && visitorDetailsItemsControl.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        visitorDetailsItemsControl.ItemContainerGenerator.StatusChanged += (s, args) =>
                        {
                            if (visitorDetailsItemsControl.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    UpdateFontSizes();
                                }), DispatcherPriority.Loaded);
                            }
                        };
                    }
                    else
                    {
                        // 直接调用更新字体大小的方法
                        UpdateFontSizes();
                    }
                    
                    // 如果处于编辑模式，延迟重新设置编辑功能（等待ItemsControl容器生成）
                    if (ViewModel.IsEditMode)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            EnableTextEditing();
                        }), DispatcherPriority.Loaded);
                    }
                }), DispatcherPriority.Loaded);
            }
        };
    }

    private void WelcomeSignView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            // **关键修复：在Loaded事件中也更新字体大小，确保覆盖样式中的默认值**
            _operLog?.Information("[WelcomeSignView] Loaded事件触发，更新字体大小");
            UpdateFontSizes();
            // 初始化 LibVLC（VideoLAN.LibVLC.Windows 包会自动处理本地库路径）
            // Core.Initialize() 可以安全地多次调用，如果已初始化则不会重复初始化
            Core.Initialize();
            
            // 创建 LibVLC 实例
            _libVLC = new LibVLC(enableDebugLogs: false);
            
            // 创建 MediaPlayer
            _mediaPlayer = new MediaPlayer(_libVLC);
            
            // 保存 AdVideoPlayer 引用（在 MainGrid 移动前保存）
            _adVideoPlayer = AdVideoPlayer;
            
            // 绑定 MediaPlayer 到 VideoView
            if (_adVideoPlayer != null)
            {
                _adVideoPlayer.MediaPlayer = _mediaPlayer;
                _operLog?.Information("[WelcomeSignView] AdVideoPlayer 引用已保存并绑定 MediaPlayer");
            }
            else
            {
                _operLog?.Warning("[WelcomeSignView] AdVideoPlayer 未找到");
            }
            
            // 订阅事件
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
            _mediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            
            _operLog?.Information("[WelcomeSignView] LibVLC 初始化成功 - 版本: {Version}", _libVLC.Version);
            
            // 视图加载时，确保视频播放器状态正确
            UpdateVideoPlayback();
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] LibVLC 初始化失败: {Message}", ex.Message);
            
            // 如果初始化失败，禁用视频播放功能，但不影响其他功能
            _libVLC?.Dispose();
            _libVLC = null;
            _mediaPlayer?.Dispose();
            _mediaPlayer = null;
        }
    }

    private void WelcomeSignView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            // 如果处于全屏模式，先退出全屏以恢复 MainGrid
            if (ViewModel.IsFullScreen)
            {
                ViewModel.IsFullScreen = false;
            }
            
            // 确保 MainGrid 回到原位置
            RestoreMainGridToOriginalPosition();
            
            // 停止播放
            _mediaPlayer?.Stop();
            
            // 释放 Media
            _currentMedia?.Dispose();
            _currentMedia = null;
            
            // 取消事件订阅
            if (_mediaPlayer != null)
            {
                _mediaPlayer.EndReached -= MediaPlayer_EndReached;
                _mediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
            }
            
            // 释放 MediaPlayer
            _mediaPlayer?.Dispose();
            _mediaPlayer = null;
            
            // 释放 LibVLC
            _libVLC?.Dispose();
            _libVLC = null;
            
            _operLog?.Information("[WelcomeSignView] 资源清理完成");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 释放 LibVLC 资源失败");
        }

        // 释放 ViewModel 资源
        ViewModel?.Dispose();
    }

    /// <summary>
    /// 更新视频播放状态：根据 ShowVisitingInfo 和 CurrentEntourage 决定显示广告还是随行人员信息
    /// </summary>
    private void UpdateVideoPlayback()
    {
        if (_mediaPlayer == null || _libVLC == null)
        {
            _operLog?.Warning("[WelcomeSignView] ⚠️ UpdateVideoPlayback 跳过 - MediaPlayer 未初始化");
            return;
        }

        bool shouldShowEntourage = ViewModel.ShowVisitingInfo && ViewModel.CurrentVisitingCompany != null;
        
        _operLog?.Information("[WelcomeSignView] 🔄 UpdateVideoPlayback - shouldShowEntourage: {ShouldShow}, ShowVisitingInfo: {Show}, CurrentVisitingCompany: {Company}",
            shouldShowEntourage, ViewModel.ShowVisitingInfo, ViewModel.CurrentVisitingCompany != null ? ViewModel.CurrentVisitingCompany.VisitingCompanyName : "null");

        if (shouldShowEntourage)
        {
            // 显示随行人员信息，停止视频
            if (_mediaPlayer.State == VLCState.Playing || _mediaPlayer.State == VLCState.Paused)
            {
                // 临时取消 EndReached 事件，避免 Stop() 触发循环播放
                _mediaPlayer.EndReached -= MediaPlayer_EndReached;
                _mediaPlayer.Stop();
                _mediaPlayer.EndReached += MediaPlayer_EndReached;
                _operLog?.Information("[WelcomeSignView] ✅ 停止视频，显示随行人员信息 - 公司: {Company}, 随行人员ID: {Id}",
                    ViewModel.CurrentVisitingCompany?.VisitingCompanyName ?? "未知", ViewModel.CurrentVisitingCompany?.Id ?? 0);
            }
            else
            {
                _operLog?.Information("[WelcomeSignView] ℹ️ 视频已停止，随行人员信息已显示 - 公司: {Company}, 随行人员ID: {Id}",
                    ViewModel.CurrentVisitingCompany?.VisitingCompanyName ?? "未知", ViewModel.CurrentVisitingCompany?.Id ?? 0);
            }
            
            // 明确输出当前显示状态
            _operLog?.Information("[WelcomeSignView] 📺 当前显示：随行人员信息 - 公司: {Company}, 随行人员ID: {Id}",
                ViewModel.CurrentVisitingCompany?.VisitingCompanyName ?? "未知", ViewModel.CurrentVisitingCompany?.Id ?? 0);
        }
        else
        {
            // 显示广告视频
            if (string.IsNullOrEmpty(ViewModel.AdVideoPath))
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ AdVideoPath 为空，无法播放广告");
                return;
            }

            string? videoPath = GetVideoPath(ViewModel.AdVideoPath);
            if (string.IsNullOrEmpty(videoPath))
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ 无法获取视频路径");
                return;
            }

            try
            {
                // 如果路径改变，重新加载 Media
                if (_currentMedia == null || _currentMedia.Mrl != videoPath)
                {
                    _currentMedia?.Dispose();
                    
                    if (videoPath.StartsWith("http://") || videoPath.StartsWith("https://"))
                        _currentMedia = new Media(_libVLC, videoPath, FromType.FromLocation);
                    else
                        _currentMedia = new Media(_libVLC, videoPath, FromType.FromPath);
                    
                    _mediaPlayer.Media = _currentMedia;
                    _operLog?.Information("[WelcomeSignView] ✅ 加载广告视频: {Path}", videoPath);
                }
                
                // 开始播放
                if (_mediaPlayer.State != VLCState.Playing)
                {
                    _mediaPlayer.Play();
                    _operLog?.Information("[WelcomeSignView] ▶️ 开始播放广告视频");
                }
                
                // 明确输出当前显示状态
                _operLog?.Information("[WelcomeSignView] 📺 当前显示：广告视频 - 路径: {Path}", videoPath);
            }
            catch (Exception ex)
            {
                _operLog?.Error(ex, "[WelcomeSignView] ❌ 播放视频失败");
            }
        }
    }

    /// <summary>
    /// 获取视频路径（LibVLC 支持文件路径和 URI）
    /// </summary>
    private string? GetVideoPath(string? videoPath)
    {
        if (string.IsNullOrEmpty(videoPath))
        {
            _operLog?.Warning("[WelcomeSignView] 视频路径为空");
            return null;
        }

        try
        {
            // 如果是绝对路径，直接使用
            if (System.IO.Path.IsPathRooted(videoPath))
            {
                if (File.Exists(videoPath))
                {
                    _operLog?.Information("[WelcomeSignView] 使用绝对路径: {Path}", videoPath);
                    return videoPath;
                }
                else
                {
                    _operLog?.Warning("[WelcomeSignView] 绝对路径文件不存在: {Path}", videoPath);
                }
            }
            else
            {
                // 相对路径：先尝试从应用程序目录查找
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = System.IO.Path.Combine(appDirectory, videoPath);
                
                if (File.Exists(fullPath))
                {
                    _operLog?.Information("[WelcomeSignView] 使用应用程序目录路径: {Path}", fullPath);
                    return fullPath;
                }
                else
                {
                    _operLog?.Warning("[WelcomeSignView] 应用程序目录文件不存在: {Path}，尝试从资源流提取", fullPath);
                    
                    // 如果文件不存在，尝试从资源流中提取到临时文件
                    // LibVLC 不支持 pack:// URI，需要文件路径
                    try
                    {
                        var normalizedPath = videoPath.Replace('\\', '/');
                        if (!normalizedPath.StartsWith("/"))
                        {
                            normalizedPath = "/" + normalizedPath;
                        }
                        
                        // 确保路径首字母大写（Assets 而不是 assets）
                        var parts = normalizedPath.Split('/');
                        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                        {
                            parts[1] = char.ToUpperInvariant(parts[1][0]) + (parts[1].Length > 1 ? parts[1].Substring(1) : string.Empty);
                            normalizedPath = string.Join("/", parts);
                        }
                        
                        var packUri = new Uri($"pack://application:,,,{normalizedPath}", UriKind.Absolute);
                        var resourceStream = System.Windows.Application.GetResourceStream(packUri);
                        
                        if (resourceStream != null)
                        {
                            // 创建临时文件
                            string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TaktDigitalSignage");
                            Directory.CreateDirectory(tempDir);
                            
                            string fileName = System.IO.Path.GetFileName(videoPath);
                            string tempFilePath = System.IO.Path.Combine(tempDir, fileName);
                            
                            // 如果文件已存在且较新，直接使用
                            if (!File.Exists(tempFilePath) || File.GetLastWriteTime(tempFilePath) < DateTime.Now.AddHours(-1))
                            {
                                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                                {
                                    resourceStream.Stream.CopyTo(fileStream);
                                }
                                _operLog?.Information("[WelcomeSignView] 从资源流提取文件到临时目录: {Path}", tempFilePath);
                            }
                            
                            return tempFilePath;
                        }
                    }
                    catch (Exception ex)
                    {
                        _operLog?.Warning("[WelcomeSignView] 从资源流提取文件失败: {Path}, 错误: {Error}", videoPath, ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 获取视频路径失败: {Path}", videoPath);
        }

        return null;
    }

    /// <summary>
    /// 视频播放结束事件：循环播放广告视频（如果没有随行人员信息需要显示）
    /// </summary>
    private void MediaPlayer_EndReached(object? sender, EventArgs e)
    {
        if (_mediaPlayer == null || _currentMedia == null)
            return;

        // 如果没有随行人员信息需要显示，循环播放视频
        if (!ViewModel.ShowVisitingInfo || ViewModel.CurrentVisitingCompany == null)
        {
            if (_mediaPlayer.State != VLCState.Playing)
            {
                _mediaPlayer.Play();
            }
        }
    }

    /// <summary>
    /// 视频播放错误事件处理
    /// </summary>
    private void MediaPlayer_EncounteredError(object? sender, EventArgs e)
    {
        _operLog?.Error("[WelcomeSignView] 视频播放错误 - 路径: {VideoPath}", 
            ViewModel.AdVideoPath ?? "未知");
    }

    /// <summary>
    /// 视口大小变化时，调整字体大小以保持响应式
    /// 基准视口：1920x1080，字体大小按比例缩放
    /// 根据随行人员详情数量自动调整字体大小：
    /// - 1个部门：公司80，部门60，职务+人员50
    /// - 2个部门：公司70，部门50，职务+人员40
    /// - 3个部门：公司60，部门40，职务+人员30
    /// - 以此类推
    /// </summary>
    private void WelcomeSignView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateFontSizes();
    }

    /// <summary>
    /// 更新字体大小（根据视口大小和随行人员详情数量）
    /// </summary>
    private void UpdateFontSizes()
    {
        try
        {
            // 基准视口尺寸
            const double baseWidth = 1920.0;
            const double baseHeight = 1080.0;
            
            // **关键修复：获取实际视口尺寸**
            // 在全屏模式下，UserControl 的尺寸可能为0，需要使用全屏窗口或 MainGrid 的父容器尺寸
            double currentWidth, currentHeight;
            
            if (ViewModel?.IsFullScreen == true && _fullScreenWindow != null)
            {
                // 全屏模式：使用全屏窗口的尺寸
                currentWidth = _fullScreenWindow.ActualWidth > 0 ? _fullScreenWindow.ActualWidth : _fullScreenWindow.Width;
                currentHeight = _fullScreenWindow.ActualHeight > 0 ? _fullScreenWindow.ActualHeight : _fullScreenWindow.Height;
                
                // 如果全屏窗口尺寸还未确定，尝试使用 MainGrid 的父容器尺寸
                if (currentWidth <= 0 || currentHeight <= 0)
                {
                    var mainGridForSize = FindName("MainGrid") as Grid;
                    if (mainGridForSize?.Parent is Grid parentGrid)
                    {
                        currentWidth = parentGrid.ActualWidth > 0 ? parentGrid.ActualWidth : parentGrid.RenderSize.Width;
                        currentHeight = parentGrid.ActualHeight > 0 ? parentGrid.ActualHeight : parentGrid.RenderSize.Height;
                    }
                }
                
                _operLog?.Information("[WelcomeSignView] 🔍 全屏模式：使用全屏窗口尺寸 {Width:F1}x{Height:F1}", currentWidth, currentHeight);
            }
            else
            {
                // 普通模式：使用 UserControl 的尺寸
                currentWidth = ActualWidth;
                currentHeight = ActualHeight;
                
                // 如果 UserControl 尺寸为0，尝试使用 MainGrid 的父容器尺寸
                if (currentWidth <= 0 || currentHeight <= 0)
                {
                    var mainContainerGrid = FindName("MainContainerGrid") as Grid;
                    if (mainContainerGrid != null)
                    {
                        currentWidth = mainContainerGrid.ActualWidth > 0 ? mainContainerGrid.ActualWidth : mainContainerGrid.RenderSize.Width;
                        currentHeight = mainContainerGrid.ActualHeight > 0 ? mainContainerGrid.ActualHeight : mainContainerGrid.RenderSize.Height;
                    }
                }
            }
            
            if (currentWidth <= 0 || currentHeight <= 0)
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ 无法获取有效的视口尺寸，跳过字体大小更新 - Width: {Width}, Height: {Height}", currentWidth, currentHeight);
                return;
            }
            
            // 计算视口缩放比例（取宽度和高度的较小比例，确保内容不会被裁剪）
            var scaleX = currentWidth / baseWidth;
            var scaleY = currentHeight / baseHeight;
            var viewportScale = Math.Min(scaleX, scaleY);
            
            // 如果视口很小，使用最小缩放比例
            viewportScale = Math.Max(viewportScale, 0.5); // 最小缩放到50%
            
            _operLog?.Information("[WelcomeSignView] 📐 视口尺寸: {Width:F1}x{Height:F1}, 视口缩放比例: {ViewportScale:F3}", 
                currentWidth, currentHeight, viewportScale);
            
            _operLog?.Information("[WelcomeSignView] 📐 视口尺寸: {Width:F1}x{Height:F1}, 视口缩放比例: {ViewportScale:F3}", 
                currentWidth, currentHeight, viewportScale);
            
            // **根据公司数量计算字体大小递减逻辑**
            // 字体大小 = 基准值 - (公司数量-1) * 10pt
            // - 1个公司：80pt（80-0*10=80）
            // - 2个公司：70pt（80-1*10=70）
            // - 3个公司：60pt（80-2*10=60）
            // 当前只有一个公司，所以应该使用 companyCount = 1 的情况（公司80pt，部门70pt，人员60pt）
            // TODO: 如果未来支持多个公司，需要基于实际公司数量计算
            var displayItemsCount = ViewModel?.CurrentVisitingDisplayItems?.Count ?? 0;
            
            // **自动缩进计算规则：单位是 px（像素）**
            // - 公司左边距：20px（始终距离左边距20px）
            // - 公司右边距：20px（固定右边距20px）
            // - 部门对齐公司并缩进20px：部门左边距 = 20px + 20px = 40px
            // - 人员对齐部门并缩进20px：人员左边距 = 40px + 20px = 60px
            // **上下间隔规则（单位：px）**：
            // - 公司距离Header：固定20px
            // - 公司与部门间隔：40px
            // - 部门与人员间隔：40px
            // - 人员与下一个部门间隔：40px（每个部门-人员组合之间间隔40px）
            const double baseSpacingFromHeader = 20.0;  // px（像素）- 公司距离Header固定20px
            const double baseCompanyLeftMargin = 20.0;  // px（像素）- 公司始终距离左边距20px
            const double baseCompanyRightMargin = 20.0; // px（像素）- 公司固定右边距20px
            const double baseSpacingAfterCompany = 40.0; // px（像素）- 公司与部门的间隔（上下间隔统一40px）
            const double baseDeptLeftMargin = 40.0;     // px（像素）- 对齐公司并缩进20px = 20px（公司）+ 20px（缩进）
            const double baseSpacingAfterDept = 40.0;   // px（像素）- 部门与人员的间隔（上下间隔统一40px）
            const double basePersonLeftMargin = 60.0;   // px（像素）- 对齐部门并缩进20px = 40px（部门）+ 20px（缩进）
            const double baseSpacingAfterPerson = 40.0; // px（像素）- 人员与下一个部门的间隔（上下间隔统一40px）
            
            // **根据显示项统计公司数量**
            // 统计ShowCompany=True的显示项数量，即公司名称行的数量
            var companyCount = ViewModel?.CurrentVisitingDisplayItems?
                .Count(item => item.ShowCompany) ?? 0;
            
            // 如果没有找到公司，默认使用1（避免除以0）
            if (companyCount <= 0)
            {
                companyCount = 1;
            }
            
            _operLog?.Information("[WelcomeSignView] 📊 统计到 {Count} 个公司", companyCount);
            
            // **根据公司数量计算companyScale（缩放因子）**
            // scale = 基于公司数量的缩放因子
            // 1个公司：companyScale = 1.0（80pt = 80*1.0 = 80-0*10）
            // 2个公司：companyScale = 0.875（70pt = 80*0.875 = 80-1*10）
            // 3个公司：companyScale = 0.75（60pt = 80*0.75 = 80-2*10）
            // companyScale = 1.0 - (companyCount - 1) * 0.125（每个公司减少12.5%，即10pt/80pt）
            var companyScale = 1.0 - (companyCount - 1) * 0.125; // **scale = 基于公司数量的缩放因子**
            
            // **字体大小计算规则：单位是 pt（点），每个递减10pt**
            // 基准值：公司80pt，部门70pt，人员60pt
            // **字体大小 = 基准值 * companyScale（基于公司数量）* viewportScale（基于视口大小）**
            // 先计算基于公司数量的字体大小（应用companyScale）
            var baseCompanyFontSize = 80.0 * companyScale;  // 1个公司：80*1.0=80pt, 2个公司：80*0.875=70pt, 3个公司：80*0.75=60pt
            var baseDeptFontSize = 70.0 * companyScale;     // 1个公司：70*1.0=70pt, 2个公司：70*0.875=61.25pt, 3个公司：70*0.75=52.5pt
            var basePersonFontSize = 60.0 * companyScale;   // 1个公司：60*1.0=60pt, 2个公司：60*0.875=52.5pt, 3个公司：60*0.75=45pt
            
            // **但是部门和个人应该保持每个递减10pt的规则（在应用视口缩放之前）**
            // 修正为：部门 = 公司 - 10pt，人员 = 部门 - 10pt
            baseDeptFontSize = baseCompanyFontSize - 10.0;    // 部门 = 公司 - 10pt
            basePersonFontSize = baseDeptFontSize - 10.0;     // 人员 = 部门 - 10pt
            
            // **最终字体大小 = 基于公司数量的字体大小 * 视口缩放**
            var companyFontSize = baseCompanyFontSize * viewportScale;
            var deptFontSize = baseDeptFontSize * viewportScale;
            var personFontSize = basePersonFontSize * viewportScale;
            
            _operLog?.Information("[WelcomeSignView] 📏 字体大小计算 - 公司数量: {CompanyCount}, scale={CompanyScale:F3}, viewportScale={ViewportScale:F3}", 
                companyCount, companyScale, viewportScale);
            _operLog?.Information("[WelcomeSignView] 📏 字体大小计算完成 - 基准字体(pt): 公司={Company:F1}, 部门={Dept:F1}, 人员={Person:F1}, 最终字体: 公司={CompanyFinal:F1}pt, 部门={DeptFinal:F1}pt, 人员={PersonFinal:F1}pt", 
                baseCompanyFontSize, baseDeptFontSize, basePersonFontSize, companyFontSize, deptFontSize, personFontSize);
            
            // **关键修复：计算公司上边距 = Header底部 + 20px（固定）**
            var mainGrid = FindName("MainGrid") as Grid;
            double companyTopMargin = 0;
            if (mainGrid != null)
            {
                // 查找Header元素
                var headerStyle = FindResource("WelcomeHeaderStyle") as Style;
                FrameworkElement? headerElement = null;
                
                if (headerStyle != null)
                {
                    foreach (TextBlock? tb in FindVisualChildren<TextBlock>(mainGrid))
                    {
                        if (tb != null && tb.Style != null && ReferenceEquals(tb.Style, headerStyle) && 
                            tb.Visibility == Visibility.Visible && tb.IsLoaded)
                        {
                            headerElement = tb;
                            break;
                        }
                    }
                }
                
                if (headerElement != null)
                {
                    // 强制更新Header布局
                    headerElement.UpdateLayout();
                    headerElement.InvalidateMeasure();
                    headerElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    
                    // 获取Header底部位置
                    var headerPosition = headerElement.TransformToAncestor(mainGrid).Transform(new Point(0, 0));
                    double headerHeight = headerElement.ActualHeight > 0 ? headerElement.ActualHeight :
                                        (headerElement.DesiredSize.Height > 0 ? headerElement.DesiredSize.Height :
                                        (headerElement.RenderSize.Height > 0 ? headerElement.RenderSize.Height :
                                        (headerElement is TextBlock headerTb ? headerTb.FontSize * 1.2 + 10 : 70)));
                    var headerMargin = headerElement.Margin;
                    double headerBottom = headerPosition.Y + headerHeight + headerMargin.Bottom;
                    
                    // 公司上边距 = Header底部 + 20px（应用视口缩放）
                    companyTopMargin = headerBottom + baseSpacingFromHeader * viewportScale;
                    
                    _operLog?.Information("[WelcomeSignView] 📐 公司位置计算 - Header底部: {HeaderBottom:F2}px, 公司上边距: {CompanyTop:F2}px (Header底部 + {Spacing}px固定间隔)", 
                        headerBottom, companyTopMargin, baseSpacingFromHeader);
                }
                else
                {
                    // 如果找不到Header，使用默认值（应用视口缩放）
                    companyTopMargin = 80.0 * viewportScale;
                    _operLog?.Warning("[WelcomeSignView] ⚠️ 未找到Header元素，使用默认公司上边距: {Top:F2}px", companyTopMargin);
                }
            }
                else
                {
                    // 如果找不到MainGrid，使用默认值（不缩放）
                    companyTopMargin = 80.0; // 直接使用80px
                    _operLog?.Warning("[WelcomeSignView] ⚠️ 未找到MainGrid，使用默认公司上边距: {Top:F2}px", companyTopMargin);
                }
            
            // **边距和间距也应用视口缩放（px单位）**
            // Margin使用设备无关像素，1px = 1/96英寸
            var companyLeftMargin = baseCompanyLeftMargin * viewportScale;  // 应用视口缩放
            var companyRightMargin = baseCompanyRightMargin * viewportScale; // 应用视口缩放
            var companyHeight = companyFontSize; // 使用实际的公司字体大小
            var spacingAfterCompany = baseSpacingAfterCompany * viewportScale; // 应用视口缩放
            var deptLeftMargin = baseDeptLeftMargin * viewportScale; // 应用视口缩放
            var deptHeight = deptFontSize; // 使用实际的部门字体大小
            var spacingAfterDept = baseSpacingAfterDept * viewportScale; // 应用视口缩放
            var personLeftMargin = basePersonLeftMargin * viewportScale; // 应用视口缩放
            var spacingAfterPerson = baseSpacingAfterPerson * viewportScale; // 应用视口缩放
            
            // **修复：公司名称现在在ItemsControl中显示，不再有单独的CompanyTextBlock**
            // ItemsControl的上边距 = Header底部 + 20px（第一个公司的位置）
            var itemsControlTopMargin = companyTopMargin;
            
            // 更新ItemsControl的位置和内部字体大小
            var visitorDetailsItemsControl = FindName("EntourageDetailsItemsControl") as ItemsControl;
            if (visitorDetailsItemsControl != null)
            {
                // 更新ItemsControl的上边距
                var itemsMargin = visitorDetailsItemsControl.Margin;
                visitorDetailsItemsControl.Margin = new Thickness(itemsMargin.Left, itemsControlTopMargin, itemsMargin.Right, itemsMargin.Bottom);
                
                // **关键修复**：更新ItemsControl内部元素的字体大小和边距
                // 需要确保容器已生成，如果没有生成则延迟更新
                var itemContainerGenerator = visitorDetailsItemsControl.ItemContainerGenerator;
                
                // 辅助方法：更新所有容器中的字体大小
                Action updateContainerFontSizes = () =>
                {
                    try
                    {
                        // **关键修复：强制更新ItemsControl布局，确保所有容器和元素都已渲染**
                        visitorDetailsItemsControl.UpdateLayout();
                        visitorDetailsItemsControl.InvalidateMeasure();
                        visitorDetailsItemsControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        visitorDetailsItemsControl.InvalidateArrange();
                        visitorDetailsItemsControl.Arrange(new Rect(visitorDetailsItemsControl.DesiredSize));
                        
                        // **关键修复：重新获取ItemContainerGenerator，确保状态是最新的**
                        var currentStatus = visitorDetailsItemsControl.ItemContainerGenerator.Status;
                        
                        if (currentStatus == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                        {
                            int updatedCount = 0;
                            for (int i = 0; i < visitorDetailsItemsControl.Items.Count; i++)
                            {
                                var container = visitorDetailsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                                if (container != null)
                                {
                                    // **强制更新容器布局**
                                    container.UpdateLayout();
                                    container.InvalidateMeasure();
                                    container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                                    
                                    // 查找所有TextBlock并更新字体大小和边距
                                    var textBlocks = FindVisualChildren<TextBlock>(container).ToList();
                                    
                                    // **关键规则**：检查当前容器是否有可见的部门TextBlock，用于判断人员是否有部门
                                    var deptTbInContainer = textBlocks
                                        .FirstOrDefault(tb => tb.Name == "DeptTextBlock" && 
                                                             tb.Visibility == Visibility.Visible);
                                    
                                    // **关键规则**：检查当前容器是否有公司名称行
                                    var companyTbInContainer = textBlocks
                                        .FirstOrDefault(tb => tb.Name == "CompanyTextBlockItem" && 
                                                             tb.Visibility == Visibility.Visible);
                                    
                                    foreach (var tb in textBlocks)
                                {
                                    // **关键修复：使用Name判断类型，因为XAML中使用了BasedOn样式，Style引用不相等**
                                    // 根据Name判断类型并更新字体大小和边距
                                    if (tb.Name == "CompanyTextBlockItem")
                                    {
                                        // **公司名称行：先清除本地值，再直接设置字体大小**
                                        if (tb.ReadLocalValue(TextBlock.FontSizeProperty) != DependencyProperty.UnsetValue)
                                        {
                                            tb.ClearValue(TextBlock.FontSizeProperty);
                                        }
                                        tb.FontSize = companyFontSize;
                                        
                                        // 验证设置是否成功
                                        if (Math.Abs(tb.FontSize - companyFontSize) > 0.1)
                                        {
                                            _operLog?.Error("[WelcomeSignView] ❌ 公司名称行字体大小设置失败！期望: {Expected:F1}pt, 实际: {Actual:F1}pt", 
                                                companyFontSize, tb.FontSize);
                                            tb.FontSize = companyFontSize;
                                        }
                                        
                                        // 公司名称：左边距20px，右边距20px
                                        tb.Margin = new Thickness(companyLeftMargin, 0, companyRightMargin, 0);
                                        updatedCount++;
                                        
                                        _operLog?.Information("[WelcomeSignView] ✅ 公司名称行字体大小已设置: 期望={FontSize:F1}pt, 基准={BaseFontSize:F1}pt, scale={Scale:F3}, viewportScale={ViewportScale:F3}, 实际={Actual:F1}pt", 
                                            companyFontSize, baseCompanyFontSize, companyScale, viewportScale, tb.FontSize);
                                    }
                                    else if (tb.Name == "DeptTextBlock")
                                    {
                                        // **部门：先清除本地值，再直接设置字体大小**
                                        if (tb.ReadLocalValue(TextBlock.FontSizeProperty) != DependencyProperty.UnsetValue)
                                        {
                                            tb.ClearValue(TextBlock.FontSizeProperty);
                                        }
                                        tb.FontSize = deptFontSize;
                                        
                                        // 验证设置是否成功
                                        if (Math.Abs(tb.FontSize - deptFontSize) > 0.1)
                                        {
                                            _operLog?.Error("[WelcomeSignView] ❌ 部门字体大小设置失败！期望: {Expected:F1}pt, 实际: {Actual:F1}pt", 
                                                deptFontSize, tb.FontSize);
                                            tb.FontSize = deptFontSize;
                                        }
                                        
                                        // 部门：对齐公司并缩进20px（左边距40px），下边距40px
                                        tb.Margin = new Thickness(deptLeftMargin, 0, 0, spacingAfterDept);
                                        updatedCount++;
                                        
                                        _operLog?.Information("[WelcomeSignView] ✅ 部门字体大小已设置: 期望={FontSize:F1}pt, 基准={BaseFontSize:F1}pt, scale={Scale:F3}, viewportScale={ViewportScale:F3}, 实际={Actual:F1}pt, 左边距: {Left:F2}px", 
                                            deptFontSize, baseDeptFontSize, companyScale, viewportScale, tb.FontSize, deptLeftMargin);
                                    }
                                    else if (tb.Name == "PersonPostTextBlock")
                                    {
                                        // **人员：先清除本地值，再直接设置字体大小**
                                        if (tb.ReadLocalValue(TextBlock.FontSizeProperty) != DependencyProperty.UnsetValue)
                                        {
                                            tb.ClearValue(TextBlock.FontSizeProperty);
                                        }
                                        tb.FontSize = personFontSize;
                                        
                                        // 验证设置是否成功
                                        if (Math.Abs(tb.FontSize - personFontSize) > 0.1)
                                        {
                                            _operLog?.Error("[WelcomeSignView] ❌ 人员字体大小设置失败！期望: {Expected:F1}pt, 实际: {Actual:F1}pt", 
                                                personFontSize, tb.FontSize);
                                            tb.FontSize = personFontSize;
                                        }
                                        
                                        // **关键规则**：人员左边距根据是否有部门决定
                                        var personMarginLeft = (deptTbInContainer != null) ? personLeftMargin : deptLeftMargin;
                                        tb.Margin = new Thickness(personMarginLeft, 0, 0, spacingAfterPerson);
                                        updatedCount++;
                                        
                                        _operLog?.Information("[WelcomeSignView] ✅ 人员字体大小已设置: 期望={FontSize:F1}pt, 基准={BaseFontSize:F1}pt, scale={Scale:F3}, viewportScale={ViewportScale:F3}, 实际={Actual:F1}pt, 左边距: {Left:F2}px (有部门: {HasDept})", 
                                            personFontSize, basePersonFontSize, companyScale, viewportScale, tb.FontSize, personMarginLeft, deptTbInContainer != null);
                                    }
                                }
                                }
                            }
                            
                            _operLog?.Information("[WelcomeSignView] ✅ 已更新 {Count} 个TextBlock的字体大小和边距", updatedCount);
                        }
                        else
                        {
                            // 容器未生成，使用FindVisualChildren从ItemsControl直接查找
                            _operLog?.Information("[WelcomeSignView] ⚠️ ItemsControl容器未完全生成 (Status: {Status})，将在外部通过视觉树方法更新", currentStatus);
                        }
                    }
                    catch (Exception ex)
                    {
                        _operLog?.Error(ex, "[WelcomeSignView] ❌ 更新容器字体大小异常");
                    }
                };
                
                // **关键修复：无论容器状态如何，都先尝试从视觉树直接更新（最可靠的方法）**
                // 然后再通过容器方法更新（作为补充，确保所有元素都被更新）
                if (visitorDetailsItemsControl.Items.Count > 0)
                {
                    // **第一步：立即尝试从视觉树直接查找并更新（如果元素已经渲染）**
                    var visualTreeCount = UpdateFontSizesFromVisualTree(visitorDetailsItemsControl, companyFontSize, deptFontSize, personFontSize, 
                        companyLeftMargin, companyRightMargin, deptLeftMargin, personLeftMargin, spacingAfterDept, spacingAfterPerson, companyScale, viewportScale);
                    
                    if (visualTreeCount > 0)
                    {
                        _operLog?.Information("[WelcomeSignView] ✅ 视觉树方法已更新 {Count} 个TextBlock", visualTreeCount);
                    }
                    else
                    {
                        _operLog?.Warning("[WelcomeSignView] ⚠️ 视觉树方法未找到任何TextBlock，可能元素尚未渲染");
                    }
                    
                    // **第二步：如果容器已生成，也通过容器方法更新（作为补充，确保所有元素都被更新）**
                    if (itemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        updateContainerFontSizes();
                    }
                    else
                    {
                        // 容器未生成，等待生成后更新（作为补充）
                        _operLog?.Information("[WelcomeSignView] ItemsControl容器未完全生成 (Status: {Status})，等待生成后再次更新...", itemContainerGenerator.Status);
                        itemContainerGenerator.StatusChanged += (s, e) =>
                        {
                            if (itemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    updateContainerFontSizes();
                                }), DispatcherPriority.Loaded);
                            }
                        };
                    }
                }
                else
                {
                    _operLog?.Information("[WelcomeSignView] ⚠️ ItemsControl没有项目，跳过字体大小更新");
                }
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 响应式字体大小调整失败");
        }
    }

    /// <summary>
    /// 从视觉树直接更新字体大小（当ItemsControl容器未完全生成时使用）
    /// </summary>
    /// <returns>返回更新的TextBlock数量</returns>
    private int UpdateFontSizesFromVisualTree(ItemsControl itemsControl, double companyFontSize, double deptFontSize, double personFontSize,
        double companyLeftMargin, double companyRightMargin, double deptLeftMargin, double personLeftMargin, double spacingAfterDept, double spacingAfterPerson, double companyScale, double viewportScale)
    {
        try
        {
            // **关键修复：强制更新ItemsControl布局，确保所有元素都已渲染**
            itemsControl.UpdateLayout();
            itemsControl.InvalidateMeasure();
            itemsControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            itemsControl.InvalidateArrange();
            itemsControl.Arrange(new Rect(itemsControl.DesiredSize));
            
            // 从ItemsControl的视觉树中查找所有TextBlock
            var allTextBlocks = FindVisualChildren<TextBlock>(itemsControl).ToList();
            
            _operLog?.Information("[WelcomeSignView] 🔍 [视觉树] 从ItemsControl中找到 {Count} 个TextBlock", allTextBlocks.Count);
            
            int updatedCount = 0;
            foreach (var tb in allTextBlocks)
            {
                if (tb.Name == "CompanyTextBlockItem")
                {
                    // **公司名称行：先清除本地值，再设置字体大小**
                    if (tb.ReadLocalValue(TextBlock.FontSizeProperty) != DependencyProperty.UnsetValue)
                    {
                        tb.ClearValue(TextBlock.FontSizeProperty);
                    }
                    tb.FontSize = companyFontSize;
                    tb.Margin = new Thickness(companyLeftMargin, 0, companyRightMargin, 0);
                    updatedCount++;
                    
                    _operLog?.Information("[WelcomeSignView] ✅ [视觉树] 公司名称行字体大小已设置: {FontSize:F1}pt, 左边距: {Left:F2}px", 
                        companyFontSize, companyLeftMargin);
                }
                else if (tb.Name == "DeptTextBlock")
                {
                // **部门：先清除本地值，再设置字体大小**
                if (tb.ReadLocalValue(TextBlock.FontSizeProperty) != DependencyProperty.UnsetValue)
                {
                    tb.ClearValue(TextBlock.FontSizeProperty);
                }
                tb.FontSize = deptFontSize;
                tb.Margin = new Thickness(deptLeftMargin, 0, 0, spacingAfterDept);
                updatedCount++;
                
                _operLog?.Information("[WelcomeSignView] ✅ [视觉树] 部门字体大小已设置: {FontSize:F1}pt, 左边距: {Left:F2}px", 
                    deptFontSize, deptLeftMargin);
            }
            else if (tb.Name == "PersonPostTextBlock")
            {
                // **人员：先清除本地值，再设置字体大小**
                if (tb.ReadLocalValue(TextBlock.FontSizeProperty) != DependencyProperty.UnsetValue)
                {
                    tb.ClearValue(TextBlock.FontSizeProperty);
                }
                tb.FontSize = personFontSize;
                
                // 判断是否有部门：查找同一容器中的DeptTextBlock
                var parent = tb.Parent;
                var hasDept = false;
                if (parent != null)
                {
                    var siblingDeptTb = FindVisualChildren<TextBlock>(parent)
                        .FirstOrDefault(t => t.Name == "DeptTextBlock" && t.Visibility == Visibility.Visible);
                    hasDept = siblingDeptTb != null;
                }
                
                var personMarginLeft = hasDept ? personLeftMargin : deptLeftMargin;
                tb.Margin = new Thickness(personMarginLeft, 0, 0, spacingAfterPerson);
                updatedCount++;
                
                _operLog?.Information("[WelcomeSignView] ✅ [视觉树] 人员字体大小已设置: {FontSize:F1}pt, 左边距: {Left:F2}px (有部门: {HasDept})", 
                    personFontSize, personMarginLeft, hasDept);
            }
            }
            
            if (updatedCount > 0)
            {
                _operLog?.Information("[WelcomeSignView] ✅ [视觉树] 已更新 {Count} 个TextBlock的字体大小和边距", updatedCount);
            }
            else
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ [视觉树] 未找到需要更新字体大小的TextBlock (共找到 {Total} 个TextBlock)", allTextBlocks.Count);
                // 输出所有找到的TextBlock信息，用于调试
                foreach (var tb in allTextBlocks)
                {
                    _operLog?.Debug("[WelcomeSignView] 🔍 [视觉树] TextBlock: Name={Name}, Text={Text}, FontSize={FontSize}pt", 
                        tb.Name ?? "未命名", tb.Text != null && tb.Text.Length > 0 ? tb.Text.Substring(0, Math.Min(20, tb.Text.Length)) : "空", tb.FontSize);
                }
            }
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 从视觉树更新字体大小失败");
            return 0;
        }
    }

    /// <summary>
    /// 查找可视化子元素（带条件）
    /// </summary>
    private static T? FindVisualChild<T>(DependencyObject? depObj, Func<T, bool>? predicate = null) where T : DependencyObject
    {
        if (depObj == null) return null;
        
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject? child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
            if (child != null && child is T t)
            {
                if (predicate == null || predicate(t))
                {
                    return t;
                }
            }

            if (child != null)
            {
                var childResult = FindVisualChild(child, predicate);
                if (childResult != null)
                {
                    return childResult;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// 查找可视化子元素
    /// </summary>
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;
        
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject? child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
            if (child != null && child is T t)
            {
                yield return t;
            }

            if (child != null)
            {
                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }

    /// <summary>
    /// 处理全屏状态变化 - 视口内全屏，不包含父窗体
    /// 通过创建新的全屏窗口实现视口全屏效果，不修改父窗体的 WindowState/WindowStyle
    /// </summary>
    private Window? _fullScreenWindow;

    private void HandleFullScreenChanged()
    {
        try
        {
            if (ViewModel.IsFullScreen)
            {
                // 进入全屏：保存父窗体状态（用于退出时恢复，虽然我们不修改它）
                _parentWindow = Window.GetWindow(this);
                if (_parentWindow != null)
                {
                    _parentWindowState = _parentWindow.WindowState;
                    _parentWindowStyle = _parentWindow.WindowStyle;
                }

                // 创建全屏窗口，不修改父窗体
                _fullScreenWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    WindowState = WindowState.Maximized,
                    Background = Brushes.Black,
                    Topmost = false,
                    ShowInTaskbar = false,
                    ResizeMode = ResizeMode.NoResize,
                    Owner = _parentWindow, // 设置 Owner 以确保退出全屏时能正确返回
                    DataContext = ViewModel // 设置窗口级别的 DataContext
                };

                // 复制 UserControl 的资源到全屏窗口（确保转换器等静态资源可用）
                foreach (System.Windows.ResourceDictionary dict in this.Resources.MergedDictionaries)
                {
                    _fullScreenWindow.Resources.MergedDictionaries.Add(dict);
                }
                // 复制直接定义的资源
                foreach (var key in this.Resources.Keys)
                {
                    if (!_fullScreenWindow.Resources.Contains(key))
                    {
                        _fullScreenWindow.Resources[key] = this.Resources[key];
                    }
                }
                
                // 创建全屏窗口的内容（移动 MainGrid 到全屏窗口）
                var fullScreenGrid = CreateFullScreenContent();
                
                // 确保 DataContext 绑定正确
                fullScreenGrid.DataContext = ViewModel;
                
                _fullScreenWindow.Content = fullScreenGrid;

                // 全屏窗口的右键菜单由 MainGrid.ContextMenu 提供（已在 CreateFullScreenContent 中移动 MainGrid）
                // MainGrid 的 ContextMenuOpening 事件仍然有效

                // 添加 ESC 键处理
                _fullScreenWindow.PreviewKeyDown += FullScreenWindow_PreviewKeyDown;
                _fullScreenWindow.KeyDown += FullScreenWindow_KeyDown;

                _fullScreenWindow.Closed += (s, e) =>
                {
                    // 窗口关闭时，如果 ViewModel 仍处于全屏状态，退出全屏模式
                    if (ViewModel != null && ViewModel.IsFullScreen)
                    {
                        _operLog?.Information("[WelcomeSignView] 全屏窗口被关闭，退出全屏模式");
                        ViewModel.IsFullScreen = false;
                    }
                    
                    // 恢复 MainGrid 到原位置（如果窗口被直接关闭）
                    RestoreMainGridToOriginalPosition();
                };

                _fullScreenWindow.Show();
                
                // 等待窗口加载完成后再更新
                _fullScreenWindow.Loaded += (s, e) =>
                {
                    // 强制更新所有绑定
                    MainGrid.UpdateLayout();
                    
                    // 验证绑定状态
                    _operLog?.Information("[WelcomeSignView] 全屏窗口已加载 - ShowVisitingInfo: {Show}, CurrentVisitingCompany: {Company}",
                        ViewModel.ShowVisitingInfo,
                        ViewModel.CurrentVisitingCompany != null ? ViewModel.CurrentVisitingCompany.VisitingCompanyName : "null");
                    
                    // **关键修复：全屏窗口加载后，必须重新应用字体大小和布局**
                    // MainGrid 移动到新窗口后，字体大小可能被重置为默认值
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        UpdateFontSizes();
                        EnableTextEditing(); // 重新启用拖拽功能（如果需要）
                    }), DispatcherPriority.Loaded);
                    
                    // 确保视频播放状态在全屏窗口中正确（MainGrid 移动后可能需要重新检查）
                    UpdateVideoPlayback();
                };
                
                // **关键修复：监听全屏窗口的尺寸变化，更新字体大小**
                _fullScreenWindow.SizeChanged += (s, e) =>
                {
                    _operLog?.Information("[WelcomeSignView] 全屏窗口尺寸变化: {Width}x{Height}", e.NewSize.Width, e.NewSize.Height);
                    UpdateFontSizes();
                };
                
                _operLog?.Information("[WelcomeSignView] 进入视口全屏模式（新窗口，不包含父窗体）");
            }
            else
            {
                // 退出全屏：关闭全屏窗口，恢复 MainGrid 到原位置
                if (_fullScreenWindow != null)
                {
                    // 移除 ESC 键处理
                    _fullScreenWindow.PreviewKeyDown -= FullScreenWindow_PreviewKeyDown;
                    _fullScreenWindow.KeyDown -= FullScreenWindow_KeyDown;
                    
                    // 恢复 MainGrid 到原位置（在关闭窗口之前）
                    RestoreMainGridToOriginalPosition();
                    
                    // 关闭全屏窗口
                    _fullScreenWindow.Close();
                    _fullScreenWindow = null;
                }
                
                // 父窗体状态保持不变（因为我们没有修改它）
                _parentWindow = null;
                
                // 确保视频播放状态恢复正常
                UpdateVideoPlayback();
                
                _operLog?.Information("[WelcomeSignView] 退出视口全屏模式，关闭全屏窗口，恢复 MainGrid");
            }

            // 更新右键菜单中的全屏图标
            var fullScreenMenuItemIcon = FindName("FullScreenMenuItemIcon") as FontAwesome.Sharp.IconBlock;
            if (fullScreenMenuItemIcon != null)
            {
                fullScreenMenuItemIcon.Icon = ViewModel.IsFullScreen ? FontAwesome.Sharp.IconChar.Compress : FontAwesome.Sharp.IconChar.Expand;
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 处理全屏状态变化失败");
        }
    }

    /// <summary>
    /// 创建全屏窗口的内容（移动 MainGrid 到全屏窗口）
    /// </summary>
    private Grid CreateFullScreenContent()
    {
        var fullScreenGrid = new Grid
        {
            Background = Brushes.Black,
            DataContext = ViewModel // 确保数据绑定正确
        };

        // 从原位置移除 MainGrid，添加到全屏窗口
        var mainContainerGrid = FindName("MainContainerGrid") as Grid;
        if (mainContainerGrid != null && mainContainerGrid.Children.Contains(MainGrid))
        {
            mainContainerGrid.Children.Remove(MainGrid);
            _operLog?.Information("[WelcomeSignView] MainGrid 已从原位置移除");
        }

        // 确保 MainGrid 的 DataContext 正确（强制设置以确保绑定有效）
        MainGrid.DataContext = ViewModel;
        
        // 强制刷新 MainGrid 及其所有子元素的绑定
        MainGrid.UpdateLayout();
        
        _operLog?.Information("[WelcomeSignView] 设置 MainGrid DataContext: {HasContext}, ShowVisitingInfo: {Show}, CurrentVisitingCompany: {Company}",
            MainGrid.DataContext != null!, 
            ViewModel.ShowVisitingInfo, 
            ViewModel.CurrentVisitingCompany != null ? ViewModel.CurrentVisitingCompany.VisitingCompanyName : "null");

        // 将 MainGrid 添加到全屏窗口
        fullScreenGrid.Children.Add(MainGrid);
        
        // **关键修复：MainGrid 移动到新窗口后，立即更新字体大小和布局**
        // 使用 Dispatcher.BeginInvoke 确保布局完成后再应用字体大小
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _operLog?.Information("[WelcomeSignView] 🔧 全屏模式：MainGrid 已添加到全屏窗口，开始更新字体大小");
            UpdateFontSizes();
            EnableTextEditing(); // 如果处于编辑模式，重新启用拖拽功能
        }), DispatcherPriority.Loaded);
        
        // 确保 AdVideoPlayer 在全屏窗口中的 MediaPlayer 绑定仍然有效
        // 使用保存的引用，因为 FindName 在全屏模式下可能失效
        if (_adVideoPlayer != null && _mediaPlayer != null)
        {
            _adVideoPlayer.MediaPlayer = _mediaPlayer;
            _operLog?.Information("[WelcomeSignView] 全屏窗口中 AdVideoPlayer MediaPlayer 已重新绑定");
        }
        
        // 验证绑定状态
        _operLog?.Information("[WelcomeSignView] MainGrid 已添加到全屏窗口");
        _operLog?.Information("[WelcomeSignView] 全屏窗口状态 - ShowVisitingInfo: {Show}, CurrentVisitingCompany: {Company}, AdVideoPath: {Path}",
            ViewModel.ShowVisitingInfo,
            ViewModel.CurrentVisitingCompany != null ? ViewModel.CurrentVisitingCompany.VisitingCompanyName : "null",
            ViewModel.AdVideoPath ?? "null");
        
        // 验证 MainGrid 的子元素绑定状态
        // **修复：CompanyTextBlock已移除，改为查找ItemsControl中的CompanyTextBlockItem**
        var companyTextBlockItem = FindVisualChild<TextBlock>(MainGrid, tb => tb.Name == "CompanyTextBlockItem");
        var visitorDetailsControl = FindVisualChild<ItemsControl>(MainGrid, ic => ic.Name == "EntourageDetailsItemsControl");
        var adVideoGrid = FindVisualChild<Grid>(MainGrid, g => g.Children.Count > 0 && g.Children.OfType<LibVLCSharp.WPF.VideoView>().Any());
        
        _operLog?.Information("[WelcomeSignView] 全屏窗口元素检查 - CompanyTextBlockItem: {HasCompany}, EntourageDetails: {HasDetails}, AdVideoGrid: {HasAd}",
            companyTextBlockItem != null!, visitorDetailsControl != null!, adVideoGrid != null);
        
        if (companyTextBlockItem != null)
        {
            // 强制刷新绑定
            var bindingExpression = companyTextBlockItem.GetBindingExpression(TextBlock.VisibilityProperty);
            bindingExpression?.UpdateTarget();
            
            _operLog?.Information("[WelcomeSignView] CompanyTextBlockItem - DataContext: {HasContext}, Visibility: {Visibility}, Text: {Text}, ShowVisitingInfo: {Show}",
                companyTextBlockItem.DataContext != null!, 
                companyTextBlockItem.Visibility,
                companyTextBlockItem.Text ?? "null",
                ViewModel.ShowVisitingInfo);
        }
        
        // 强制刷新所有 Visibility 绑定
        foreach (var element in FindVisualChildren<FrameworkElement>(MainGrid))
        {
            var visibilityBinding = System.Windows.Data.BindingOperations.GetBindingExpression(element, UIElement.VisibilityProperty);
            visibilityBinding?.UpdateTarget();
        }
        
        return fullScreenGrid;
    }

    /// <summary>
    /// 恢复 MainGrid 到原位置
    /// </summary>
    private void RestoreMainGridToOriginalPosition()
    {
        try
        {
            // 从全屏窗口中移除 MainGrid
            if (_fullScreenWindow != null && _fullScreenWindow.Content is Grid fullScreenGrid)
            {
                if (fullScreenGrid.Children.Contains(MainGrid))
                {
                    fullScreenGrid.Children.Remove(MainGrid);
                    _operLog?.Information("[WelcomeSignView] MainGrid 已从全屏窗口移除");
                }
            }

                // 将 MainGrid 恢复到原位置
                var mainContainerGrid = FindName("MainContainerGrid") as Grid;
                if (mainContainerGrid != null && !mainContainerGrid.Children.Contains(MainGrid))
                {
                    mainContainerGrid.Children.Add(MainGrid);
                    
                    // 确保 DataContext 正确（强制设置）
                    MainGrid.DataContext = ViewModel;
                    
                    // 强制刷新布局和绑定
                    MainGrid.UpdateLayout();
                    
                    // **关键修复：MainGrid 恢复到原位置后，必须重新应用字体大小和布局**
                    // 退出全屏后，字体大小可能被重置为默认值
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _operLog?.Information("[WelcomeSignView] 🔧 退出全屏模式：MainGrid 已恢复到原位置，开始更新字体大小");
                        UpdateFontSizes();
                        EnableTextEditing(); // 如果处于编辑模式，重新启用拖拽功能
                    }), DispatcherPriority.Loaded);
                    
                    _operLog?.Information("[WelcomeSignView] MainGrid 已恢复到原位置，DataContext: {HasContext}, ShowVisitingInfo: {Show}",
                        MainGrid.DataContext != null!, ViewModel.ShowVisitingInfo);
                }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 恢复 MainGrid 到原位置失败");
        }
    }

    /// <summary>
    /// 全屏窗口 ESC 键处理（PreviewKeyDown）
    /// </summary>
    private void FullScreenWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ViewModel.IsFullScreen)
        {
            ViewModel.IsFullScreen = false;
            e.Handled = true;
            _operLog?.Information("[WelcomeSignView] ESC 键按下（PreviewKeyDown），退出全屏模式");
        }
    }

    /// <summary>
    /// 全屏窗口 ESC 键处理（KeyDown，备用）
    /// </summary>
    private void FullScreenWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ViewModel.IsFullScreen)
        {
            ViewModel.IsFullScreen = false;
            e.Handled = true;
            _operLog?.Information("[WelcomeSignView] ESC 键按下（KeyDown），退出全屏模式");
        }
    }


    /// <summary>
    /// 右键菜单打开时更新菜单项图标（文本已通过绑定自动更新）
    /// </summary>
    private void MainGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        try
        {
            // 确保右键菜单的 DataContext 正确（菜单项的 Command 绑定需要）
            var mainGrid = sender as Grid;
            if (mainGrid?.ContextMenu != null)
            {
                // 如果 DataContext 为空，设置它
                if (mainGrid.ContextMenu.DataContext == null)
                {
                    mainGrid.ContextMenu.DataContext = ViewModel;
                }
                
                // 同时确保所有 MenuItem 的 DataContext 也正确
                foreach (MenuItem item in mainGrid.ContextMenu.Items.OfType<MenuItem>())
                {
                    if (item.DataContext == null)
                    {
                        item.DataContext = ViewModel;
                    }
                    
                    // 验证命令绑定
                    if (item.Command != null)
                    {
                        var commandName = item.Name == "EditMenuItem" ? "ToggleEditModeCommand" : 
                                         item.Name == "FullScreenMenuItem" ? "ToggleFullScreenCommand" : "未知";
                        _operLog?.Information("[WelcomeSignView] MenuItem {Name} 命令: {Command}, CanExecute: {CanExecute}", 
                            item.Name ?? "未命名", 
                            commandName,
                            item.Command.CanExecute(null));
                    }
                    else
                    {
                        _operLog?.Warning("[WelcomeSignView] MenuItem {Name} 命令为 null", item.Name ?? "未命名");
                    }
                }
                
                _operLog?.Information("[WelcomeSignView] 右键菜单 DataContext 已设置 - ViewModel: {ViewModelType}", ViewModel?.GetType().Name ?? "null");
            }
            
            // 更新全屏菜单项图标（文本已通过 XAML 绑定自动更新）
            var fullScreenMenuItemIcon = FindName("FullScreenMenuItemIcon") as FontAwesome.Sharp.IconBlock;
            if (fullScreenMenuItemIcon != null && ViewModel != null)
            {
                fullScreenMenuItemIcon.Icon = ViewModel.IsFullScreen ? FontAwesome.Sharp.IconChar.Compress : FontAwesome.Sharp.IconChar.Expand;
            }
            
            if (ViewModel != null)
            {
                _operLog?.Information("[WelcomeSignView] 右键菜单打开 - IsEditMode: {IsEdit}, IsFullScreen: {IsFull}, Menu DataContext: {HasContext}",
                    ViewModel.IsEditMode,
                    ViewModel.IsFullScreen,
                    mainGrid?.ContextMenu?.DataContext != null);
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 右键菜单打开时更新失败: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// 处理编辑模式变化
    /// </summary>
    private void HandleEditModeChanged()
    {
        _operLog?.Information("[WelcomeSignView] 🔧 HandleEditModeChanged 调用 - IsEditMode: {IsEdit}", ViewModel.IsEditMode);
        
        if (ViewModel.IsEditMode)
        {
            // 进入编辑模式：启用文本拖拽和大小调整
            EnableTextEditing();
        }
        else
        {
            // 退出编辑模式：禁用文本拖拽和大小调整
            DisableTextEditing();
        }
    }

    /// <summary>
    /// 启用文本编辑功能（拖拽和调整大小）
    /// </summary>
    private void EnableTextEditing()
    {
        try
        {
            _operLog?.Information("[WelcomeSignView] 🔧 EnableTextEditing 开始执行");
            
            // **修复：CompanyTextBlock已移除，公司名称现在在ItemsControl中的CompanyTextBlockItem显示**
            // 不再需要单独查找和设置CompanyTextBlock，所有公司元素都在ItemsControl中
            
            // 为所有文本元素添加编辑功能（Header、Footer、公司、部门、人员、职务等）
            // **关键修复：使用Dispatcher.BeginInvoke延迟执行，确保ItemsControl的容器已经生成**
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var mainGrid = FindName("MainGrid") as Grid;
                if (mainGrid != null)
                {
                    _operLog?.Information("[WelcomeSignView] 找到 MainGrid，查找所有可编辑文本元素");
                    
                    // 获取样式资源
                    var headerStyle = FindResource("WelcomeHeaderStyle") as Style;
                    var footerStyle = FindResource("WelcomeFooterStyle") as Style;
                    var companyStyle = FindResource("WelcomeCompanyStyle") as Style;
                    var deptStyle = FindResource("WelcomeDeptStyle") as Style;
                    var personStyle = FindResource("WelcomePersonStyle") as Style;
                    
                    // **关键修复：确保ItemsControl的容器已经生成**
                    var visitorDetailsItemsControl = FindName("EntourageDetailsItemsControl") as ItemsControl;
                    if (visitorDetailsItemsControl != null)
                    {
                        // 强制生成ItemsControl的容器
                        visitorDetailsItemsControl.UpdateLayout();
                        var itemContainerGenerator = visitorDetailsItemsControl.ItemContainerGenerator;
                        if (itemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                        {
                            _operLog?.Information("[WelcomeSignView] ItemsControl容器未完全生成，等待生成...");
                            itemContainerGenerator.StatusChanged += (s, e) =>
                            {
                                if (itemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                                {
                                    Dispatcher.BeginInvoke(new Action(() => SetupDraggableForAllElements(mainGrid, headerStyle, footerStyle, companyStyle, deptStyle, personStyle)), DispatcherPriority.Loaded);
                                }
                            };
                        }
                    }
                    
                    // 查找所有 TextBlock 元素（包括 ItemsControl 内部生成的）
                    SetupDraggableForAllElements(mainGrid, headerStyle, footerStyle, companyStyle, deptStyle, personStyle);
                }
                else
                {
                    _operLog?.Warning("[WelcomeSignView] 未找到 MainGrid");
                }
            }), DispatcherPriority.Loaded);
            
            _operLog?.Information("[WelcomeSignView] ✅ 编辑模式已启用 - 可以拖拽文本、使用滚轮调整字体大小");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 启用文本编辑功能失败: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// 为所有元素设置拖拽功能（辅助方法）
    /// **关键修复：使用Name属性判断元素类型，因为XAML使用了BasedOn样式，样式引用不相等**
    /// </summary>
    private void SetupDraggableForAllElements(Grid mainGrid, Style? headerStyle, Style? footerStyle, Style? companyStyle, Style? deptStyle, Style? personStyle)
    {
        int editableCount = 0;
        
        // 查找所有 TextBlock 元素（包括 ItemsControl 内部生成的）
        foreach (TextBlock? tb in FindVisualChildren<TextBlock>(mainGrid))
        {
            if (tb == null) continue;
            
            bool shouldEdit = false;
            string elementType = "未知";
            
            // **关键修复：使用Name属性判断，因为XAML使用了BasedOn样式，样式引用不相等**
            // 检查是否是 Header 或 Footer（这些不可拖拽，跳过）
            // Header和Footer没有Name，但可以通过样式判断（如果样式引用相等）
            if (tb.Style != null)
            {
                if ((headerStyle != null && ReferenceEquals(tb.Style, headerStyle)) ||
                    (footerStyle != null && ReferenceEquals(tb.Style, footerStyle)))
                {
                    continue;
                }
            }
            
            // 使用Name属性判断元素类型（最可靠的方式）
            if (tb.Name == "CompanyTextBlockItem")
            {
                // 公司名称元素（在ItemsControl中）
                shouldEdit = true;
                elementType = "Company";
            }
            else if (tb.Name == "DeptTextBlock")
            {
                // 部门元素
                shouldEdit = true;
                elementType = "Dept";
            }
            else if (tb.Name == "PersonPostTextBlock")
            {
                // 人员元素
                shouldEdit = true;
                elementType = "Person";
            }
            else if (tb.Style != null)
            {
                // 如果没有Name，尝试通过样式判断（备选方案）
                // 检查是否是部门样式（使用样式的基础样式判断）
                if (deptStyle != null && tb.Style.BasedOn == deptStyle)
                {
                    shouldEdit = true;
                    elementType = "Dept";
                }
                // 检查是否是人员样式
                else if (personStyle != null && tb.Style.BasedOn == personStyle)
                {
                    shouldEdit = true;
                    elementType = "Person";
                }
            }
            
            if (shouldEdit)
            {
                // 为元素设置唯一名称（如果还没有）
                if (string.IsNullOrEmpty(tb.Name))
                {
                    tb.Name = $"{elementType}_{editableCount}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                }
                
                // 检查是否已经设置过拖拽（避免重复设置）
                if (!_dragContexts.ContainsKey(tb))
                {
                    SetupDraggableText(tb, tb.Name);
                    editableCount++;
                    _operLog?.Information("[WelcomeSignView] ✅ 为 {Type} ({Name}) 设置了编辑功能", elementType, tb.Name);
                }
            }
        }
        
        _operLog?.Information("[WelcomeSignView] ✅ 共设置了 {Count} 个可编辑文本元素", editableCount);
    }

    /// <summary>
    /// 禁用文本编辑功能
    /// </summary>
    private void DisableTextEditing()
    {
        try
        {
            // 清除所有辅助线（使用 AdornerLayer）
            ClearGuideLines();
            
            // 清除字体大小变化提示
            if (_fontSizeChangeToolTip != null)
            {
                _fontSizeChangeToolTip.IsOpen = false;
                _fontSizeChangeToolTip = null;
            }
            if (_fontSizeHintTimer != null)
            {
                _fontSizeHintTimer.Stop();
                _fontSizeHintTimer = null;
            }
            
            // 移除所有文本元素的编辑功能（清除鼠标样式等）
            var mainGrid = FindName("MainGrid") as Grid;
            if (mainGrid != null)
            {
                foreach (TextBlock? tb in FindVisualChildren<TextBlock>(mainGrid))
                {
                    if (tb != null && tb.Tag?.ToString() == "Editable")
                    {
                        tb.Cursor = Cursors.Arrow;
                        tb.Tag = null;
                        // 保留 ToolTip，但可以重置为显示当前字体大小
                        if (tb.ToolTip is ToolTip toolTip)
                        {
                            toolTip.Content = $"{tb.FontSize}pt";
                        }
                    }
                }
            }
            
            _operLog?.Information("[WelcomeSignView] 编辑模式已禁用");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 禁用文本编辑功能失败");
        }
    }

    // 拖拽上下文类，为每个元素存储独立的拖拽状态（解决多元素拖动相互干扰问题）
    // 每个元素拥有独立的虚拟图层，完全隔离拖拽状态
    private class DragContext
    {
        public FrameworkElement Element { get; set; } = null!;
        public Point DragStartPoint { get; set; }
        public Point InitialElementPosition { get; set; } // 元素开始拖拽时的初始位置（布局位置，排除Transform）
        public Point? InitialParentPosition { get; set; } // 父容器开始拖拽时的初始位置（仅嵌套元素，布局位置，排除Transform）
        public bool IsDragging { get; set; }
        public System.Windows.Media.TranslateTransform? DragTransform { get; set; } // 拖拽时的临时变换
        public GuideLineAdorner? GuideLineAdorner { get; set; }
        public DragPreviewAdorner? DragPreviewAdorner { get; set; }
    }

    // 为每个元素存储独立的拖拽上下文（解决多元素拖动相互干扰问题）
    private readonly Dictionary<FrameworkElement, DragContext> _dragContexts = new();
    
    // **关键改进：每个元素都有独立的AdornerLayer，完全隔离**
    // 每个元素的装饰器（GuideLineAdorner、DragPreviewAdorner）都存储在DragContext中
    // 共享的装饰器引用仅用于向后兼容和清理残留
    private GuideLineAdorner? _guideLineAdorner; // 保留用于向后兼容，实际已不使用
    private DragPreviewAdorner? _dragPreviewAdorner; // 保留用于向后兼容，实际已不使用
    // 网格功能已移除，如需要可用 AdornerLayer 实现
    private ToolTip? _fontSizeChangeToolTip; // 用于显示字体大小变化的提示
    private DispatcherTimer? _fontSizeHintTimer; // 用于自动隐藏提示

    /// <summary>
    /// 设置文本元素为可拖拽和可调整大小
    /// </summary>
    private void SetupDraggableText(FrameworkElement? element, string elementName)
    {
        if (element == null) return;

        var mainGrid = FindName("MainGrid") as Grid;
        if (mainGrid == null) return;

        // **Header 和 Footer 始终不可拖拽**
        if (element is TextBlock tb && tb.Style != null)
        {
            var headerStyle = FindResource("WelcomeHeaderStyle") as Style;
            var footerStyle = FindResource("WelcomeFooterStyle") as Style;
            
            if ((headerStyle != null && ReferenceEquals(tb.Style, headerStyle)) ||
                (footerStyle != null && ReferenceEquals(tb.Style, footerStyle)))
            {
                // Header 和 Footer 不可拖拽，但可以调整字体大小（如果需要）
                // 保持默认鼠标样式，不设置拖拽事件
                _operLog?.Information("[WelcomeSignView] ℹ️ {ElementName} 是 {Type}，跳过拖拽设置", 
                    elementName, ReferenceEquals(tb.Style, headerStyle) ? "Header" : "Footer");
                return;
            }
        }

        // 设置鼠标样式为可移动
        element.Cursor = Cursors.SizeAll;
        
        // 鼠标滚轮调整字体大小（仅对TextBlock有效）
        if (element is TextBlock textBlock)
        {
            // **关键修复**：等待UpdateFontSizes完成后再读取字体大小，避免读取到错误的初始值
            // 延迟初始化ToolTip，确保字体大小已经由UpdateFontSizes设置
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // 初始化 ToolTip 显示当前字体大小（此时应该是UpdateFontSizes设置的值）
                if (textBlock.ToolTip == null)
                {
                    textBlock.ToolTip = new ToolTip
                    {
                        Content = $"{textBlock.FontSize}pt",
                        Placement = PlacementMode.RelativePoint,
                        PlacementTarget = textBlock
                    };
                }
                else if (textBlock.ToolTip is ToolTip existingToolTip)
                {
                    // 更新已存在的ToolTip内容为当前字体大小
                    existingToolTip.Content = $"{textBlock.FontSize}pt";
                }
            }), DispatcherPriority.Loaded);
            
            // 添加鼠标滚轮事件处理器
            textBlock.MouseWheel += (s, e) =>
            {
                if (!ViewModel.IsEditMode) return;
                
                // 记录调整前的大小
                var oldSize = textBlock.FontSize;
                
                // 一次放大或缩小 5pt，范围限制：20pt 至 120pt
                var delta = e.Delta > 0 ? 5 : -5;
                var newSize = Math.Max(20, Math.Min(120, textBlock.FontSize + delta));
                
                // 如果大小没有变化（已达到边界），不更新
                if (newSize == oldSize)
                {
                    // 显示提示：已达到边界
                    ShowFontSizeChangeHint(textBlock, oldSize, newSize, isAtBoundary: true);
                    e.Handled = true;
                    return;
                }
                
                // 更新字体大小
                textBlock.FontSize = newSize;
                
                // 更新 ToolTip
                if (textBlock.ToolTip is ToolTip toolTip)
                {
                    toolTip.Content = $"{newSize}pt";
                }
                
                // 显示字体大小变化提示
                ShowFontSizeChangeHint(textBlock, oldSize, newSize, isAtBoundary: false);
                
                _operLog?.Information("[WelcomeSignView] 📏 字体大小调整: {Element} {OldSize}pt -> {NewSize}pt", 
                    elementName, oldSize, newSize);
                e.Handled = true;
            };
            
            // 添加视觉提示（可选：在编辑模式下显示边框）
            textBlock.Tag = "Editable";
        }
        
        element.MouseLeftButtonDown += (s, e) =>
        {
            if (!ViewModel.IsEditMode) return;
            
            // 获取或创建该元素的独立拖拽上下文（解决多元素拖动相互干扰问题）
            if (!_dragContexts.TryGetValue(element, out var context))
            {
                context = new DragContext { Element = element };
                _dragContexts[element] = context;
            }
            
            context.DragStartPoint = e.GetPosition(mainGrid);
            
            // **图层模型：清晰的图层分离**
            // 图层0（布局层）：元素的真实布局位置（Margin/Canvas.SetLeft），这是持久化的位置
            // 图层1（拖拽层）：拖拽时的临时位置（通过Transform实现），拖拽结束后清除
            
            // **核心修复：InitialElementPosition必须是布局位置（Margin），不包含Transform**
            // 这样确保每次拖拽都基于真实布局位置，避免Transform累积导致的位置错误
            Point layoutPosition;
            if (element.Parent == mainGrid)
            {
                layoutPosition = new Point(element.Margin.Left, element.Margin.Top);
            }
            else
            {
                double absX = 0;
                double absY = 0;
                FrameworkElement? current = element;
                while (current != null && current != mainGrid)
                {
                    var margin = current.Margin;
                    absX += margin.Left;
                    absY += margin.Top;
                    current = current.Parent as FrameworkElement;
                }
                layoutPosition = new Point(absX, absY);
                
                // 保存父容器位置
                if (element.Parent is FrameworkElement parentElement)
                {
                    double parentAbsX = 0;
                    double parentAbsY = 0;
                    current = parentElement;
                    while (current != null && current != mainGrid)
                    {
                        var margin = current.Margin;
                        parentAbsX += margin.Left;
                        parentAbsY += margin.Top;
                        current = current.Parent as FrameworkElement;
                    }
                    context.InitialParentPosition = new Point(parentAbsX, parentAbsY);
                }
                else
                {
                    context.InitialParentPosition = null;
                }
            }
            
            // **关键：InitialElementPosition = 布局位置（不包含任何Transform）**
            context.InitialElementPosition = layoutPosition;
            
            // **关键修复：每个元素必须使用完全独立的Transform对象，避免相互干扰**
            // 如果元素已有Transform，先清除它，然后创建新的独立Transform
            if (element.RenderTransform is TranslateTransform existingTransform)
            {
                // 如果已有Transform，先清除它（但保留引用用于后续清除）
                existingTransform.X = 0;
                existingTransform.Y = 0;
                context.DragTransform = existingTransform;
            }
            else if (element.RenderTransform is TransformGroup transformGroup)
            {
                // 如果已有TransformGroup，查找或创建TranslateTransform
                var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
                if (translateTransform != null)
                {
                    translateTransform.X = 0;
                    translateTransform.Y = 0;
                    context.DragTransform = translateTransform;
                }
                else
                {
                    // 创建新的独立Transform
                    context.DragTransform = new TranslateTransform();
                    transformGroup.Children.Add(context.DragTransform);
                }
            }
            else
            {
                // 创建新的独立Transform
                context.DragTransform = new TranslateTransform();
                element.RenderTransform = context.DragTransform;
            }
            
            // **关键：确保Transform初始值为0，从布局位置开始**
            context.DragTransform.X = 0;
            context.DragTransform.Y = 0;
            
            // **日志记录：拖拽开始时的图层状态**
            var currentMargin = element.Margin;
            var currentElementPosition = element.TransformToAncestor(mainGrid).Transform(new Point(0, 0));
            
            context.IsDragging = false;
            element.CaptureMouse();
            
            var elementText = GetElementDisplayText(element, elementName);
            
            _operLog?.Information("[WelcomeSignView] ========== 拖拽开始：元素 {ElementText} ==========", elementText);
            _operLog?.Information("[WelcomeSignView] 🖱️ 鼠标起始位置: ({StartX:F2}, {StartY:F2})", 
                context.DragStartPoint.X, context.DragStartPoint.Y);
            _operLog?.Information("[WelcomeSignView] 📍 图层0（布局层）状态:");
            _operLog?.Information("[WelcomeSignView]   - Margin: Left={MarginLeft:F2}, Top={MarginTop:F2}, Right={MarginRight:F2}, Bottom={MarginBottom:F2}",
                currentMargin.Left, currentMargin.Top, currentMargin.Right, currentMargin.Bottom);
            _operLog?.Information("[WelcomeSignView]   - 图层0初始位置: ({InitX:F2}, {InitY:F2})",
                context.InitialElementPosition.X, context.InitialElementPosition.Y);
            _operLog?.Information("[WelcomeSignView]   - 当前实际位置（验证）: ({CurrentX:F2}, {CurrentY:F2})",
                currentElementPosition.X, currentElementPosition.Y);
            _operLog?.Information("[WelcomeSignView] =========================================");
            e.Handled = true;
        };

        element.MouseMove += (s, e) =>
        {
            if (!ViewModel.IsEditMode || !element.IsMouseCaptured) return;
            
            // 获取该元素的独立拖拽上下文
            if (!_dragContexts.TryGetValue(element, out var context))
            {
                return;
            }

            var currentPoint = e.GetPosition(mainGrid);
            
            // 判断是否开始拖拽
            if (!context.IsDragging)
            {
                var deltaX = currentPoint.X - context.DragStartPoint.X;
                var deltaY = currentPoint.Y - context.DragStartPoint.Y;
                
                if (Math.Abs(deltaX) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(deltaY) > SystemParameters.MinimumVerticalDragDistance)
                {
                    context.IsDragging = true;
                }
            }

            // 如果已经开始拖拽，直接跟随鼠标移动
            if (context.IsDragging && context.DragTransform != null)
            {
                // 计算鼠标偏移量
                var deltaX = currentPoint.X - context.DragStartPoint.X;
                var deltaY = currentPoint.Y - context.DragStartPoint.Y;
                
                // 计算新位置 = 初始位置 + 鼠标偏移
                var newLeft = context.InitialElementPosition.X + deltaX;
                var newTop = context.InitialElementPosition.Y + deltaY;
                
                // 更新Transform，使元素移动到新位置
                context.DragTransform.X = deltaX;
                context.DragTransform.Y = deltaY;
                
                // 显示对齐辅助线（当靠近其他元素时）
                ShowGuideLines(element, newLeft, newTop, mainGrid, context);
            }
        };

        element.MouseLeftButtonUp += (s, e) =>
        {
            if (!element.IsMouseCaptured) return;
            
            // **关键修复：必须通过element参数获取上下文，确保是当前元素的上下文**
            if (!_dragContexts.TryGetValue(element, out var context))
            {
                element.ReleaseMouseCapture();
                return;
            }
            
            // **关键修复：验证context.Element是否匹配，防止上下文混乱**
            if (context.Element != element)
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ 上下文元素不匹配！期望: {Expected}, 实际: {Actual}", 
                    element.Name, context.Element?.Name ?? "null");
                element.ReleaseMouseCapture();
                return;
            }
            
            if (context.IsDragging && context.DragTransform != null)
            {
                var elementText = GetElementDisplayText(element, elementName);
                
                _operLog?.Information("[WelcomeSignView] ========== 拖拽结束：元素 {ElementText} ==========", elementText);
                _operLog?.Information("[WelcomeSignView] 📍 当前状态 - 初始位置: ({InitX:F2}, {InitY:F2}), Transform: ({TX:F2}, {TY:F2})",
                    context.InitialElementPosition.X, context.InitialElementPosition.Y, 
                    context.DragTransform.X, context.DragTransform.Y);
                
                // **最终位置 = 初始视觉位置 + 鼠标偏移量（已在MouseMove中计算好）**
                // 直接使用MouseMove中计算的最终位置，而不是重新计算
                // 但需要在MouseMove中保存最终位置，或者在Up时重新计算一次
                
                // 重新计算最终位置：初始视觉位置 + 当前Transform偏移
                var finalLeft = context.InitialElementPosition.X + context.DragTransform.X;
                var finalTop = context.InitialElementPosition.Y + context.DragTransform.Y;
                
                _operLog?.Information("[WelcomeSignView] 🎯 拖拽结束 - 图层0初始位置: ({InitX:F2}, {InitY:F2}), 图层1 Transform: ({TX:F2}, {TY:F2}), 最终位置: ({FinalX:F2}, {FinalY:F2})",
                    context.InitialElementPosition.X, context.InitialElementPosition.Y,
                    context.DragTransform.X, context.DragTransform.Y, finalLeft, finalTop);
                
                // **关键修复：先清除Transform，再更新Margin，确保位置正确**
                // 这样可以避免Transform和Margin同时存在导致的位置混乱
                _operLog?.Information("[WelcomeSignView] 🔄 清除图层1（拖拽层）Transform: ({TX:F2}, {TY:F2}) -> (0, 0)",
                    context.DragTransform.X, context.DragTransform.Y);
                
                // **步骤1：先清除Transform，让元素回到布局位置**
                context.DragTransform.X = 0;
                context.DragTransform.Y = 0;
                
                // **步骤2：强制更新布局，确保清除Transform生效**
                element.UpdateLayout();
                mainGrid.UpdateLayout();
                
                // **步骤3：更新Margin为最终位置**
                var oldMargin = element.Margin;
                
                if (element.Parent == mainGrid)
                {
                    element.Margin = new Thickness(finalLeft, finalTop, oldMargin.Right, oldMargin.Bottom);
                    _operLog?.Information("[WelcomeSignView] ✅ 图层0更新 - 直接子元素，新Margin: Left={NewLeft:F2}, Top={NewTop:F2}",
                        finalLeft, finalTop);
                }
                else
                {
                    Point parentPosition;
                    if (context.InitialParentPosition.HasValue)
                    {
                        parentPosition = context.InitialParentPosition.Value;
                    }
                    else if (element.Parent is FrameworkElement parent)
                    {
                        double parentAbsX = 0;
                        double parentAbsY = 0;
                        FrameworkElement? current = parent;
                        
                        while (current != null && current != mainGrid)
                        {
                            var margin = current.Margin;
                            parentAbsX += margin.Left;
                            parentAbsY += margin.Top;
                            current = current.Parent as FrameworkElement;
                        }
                        
                        parentPosition = new Point(parentAbsX, parentAbsY);
                    }
                    else
                    {
                        parentPosition = new Point(0, 0);
                    }
                    
                    var relativeLeft = finalLeft - parentPosition.X;
                    var relativeTop = finalTop - parentPosition.Y;
                    element.Margin = new Thickness(relativeLeft, relativeTop, oldMargin.Right, oldMargin.Bottom);
                    
                    _operLog?.Information("[WelcomeSignView] ✅ 图层0更新 - 嵌套元素，父容器位置: ({ParentX:F2}, {ParentY:F2}), 相对位置: ({RelX:F2}, {RelY:F2})",
                        parentPosition.X, parentPosition.Y, relativeLeft, relativeTop);
                }
                
                // **步骤4：再次更新布局，确保Margin生效**
                element.UpdateLayout();
                mainGrid.UpdateLayout();
                
                // 验证最终位置是否正确
                var verifyPosition = element.TransformToAncestor(mainGrid).Transform(new Point(0, 0));
                var errorX = verifyPosition.X - finalLeft;
                var errorY = verifyPosition.Y - finalTop;
                var errorDistance = Math.Sqrt(errorX * errorX + errorY * errorY);
                
                _operLog?.Information("[WelcomeSignView] ✅ 位置验证结果:");
                _operLog?.Information("[WelcomeSignView]   - 期望位置（目标停靠位置）: ({ExpectedX:F2}, {ExpectedY:F2})", finalLeft, finalTop);
                _operLog?.Information("[WelcomeSignView]   - 实际位置（TransformToAncestor验证）: ({ActualX:F2}, {ActualY:F2})", verifyPosition.X, verifyPosition.Y);
                _operLog?.Information("[WelcomeSignView]   - 位置误差: X={ErrorX:F2}, Y={ErrorY:F2}, 距离={Distance:F2}px", errorX, errorY, errorDistance);
                
                if (errorDistance > 1.0)
                {
                    _operLog?.Warning("[WelcomeSignView] ⚠️ ⚠️ ⚠️ 位置不一致！误差超过 1px，可能存在问题！");
                }
                else
                {
                    _operLog?.Information("[WelcomeSignView] ✅ 位置验证通过，误差在可接受范围内");
                }
                
                // 清除拖拽状态
                context.IsDragging = false;
            }
            else
            {
                _operLog?.Information("[WelcomeSignView] ℹ️ 拖拽取消 - IsDragging: {IsDragging}, Transform: {HasTransform}",
                    context.IsDragging, context.DragTransform != null);
                
                // 即使没有拖拽，也要清除 Transform
                if (context.DragTransform != null)
                {
                    context.DragTransform.X = 0;
                    context.DragTransform.Y = 0;
                }
            }
            
            element.ReleaseMouseCapture();
            
            // 清除辅助线和拖拽预览
            ClearGuideLines(context);
            ClearDragPreview(context);
            
            // 清除拖拽状态
            context.IsDragging = false;
            
            e.Handled = true;
        };
    }

    /// <summary>
    /// 显示辅助线（使用 AdornerLayer 实现，参考 AutoCAD 方式）
    /// 优点：不占用布局空间，不影响鼠标事件，性能更好
    /// **关键改进：每个元素使用独立的AdornerLayer，完全隔离**
    /// </summary>
    private void ShowGuideLines(FrameworkElement element, double newLeft, double newTop, Grid mainGrid, DragContext context)
    {
        try
        {
            if (!ViewModel.IsEditMode)
            {
                ClearGuideLines(context);
                return;
            }

            // **关键修复：强制每个元素的Adorner装饰元素本身，绝不装饰MainGrid**
            // 即使共享AdornerLayer，只要AdornedElement不同，就不会相互干扰
            // 获取AdornerLayer（向上查找，通常是MainGrid的，但这不影响）
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer == null)
            {
                // 如果找不到，尝试MainGrid的
                adornerLayer = AdornerLayer.GetAdornerLayer(mainGrid);
                if (adornerLayer == null)
                {
                    _operLog?.Warning("[WelcomeSignView] AdornerLayer 未找到，跳过辅助线显示");
                    return;
                }
            }
            
            // **核心原则：无论使用哪个AdornerLayer，Adorner必须装饰元素本身，而不是MainGrid**
            // 这样即使多个元素共享同一个AdornerLayer，它们也不会相互干扰
            var adornedElement = element; // 强制装饰元素本身
            
            _operLog?.Debug("[WelcomeSignView] 为元素创建独立Adorner - Element: {ElementName}, AdornerLayer: {LayerSource}", 
                element.Name, adornerLayer.GetType().Name);

            // 获取画布尺寸
            var canvasWidth = mainGrid.ActualWidth > 0 ? mainGrid.ActualWidth : mainGrid.RenderSize.Width;
            var canvasHeight = mainGrid.ActualHeight > 0 ? mainGrid.ActualHeight : mainGrid.RenderSize.Height;

            // **第一步：获取元素的实际尺寸（使用 Measure 确保尺寸准确）**
            double elementWidth = 0;
            double elementHeight = 0;
            
            if (element is TextBlock textBlock)
            {
                // 对于 TextBlock，使用 Measure 获取准确尺寸（不受当前 Margin 影响）
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                elementWidth = textBlock.DesiredSize.Width > 0 ? textBlock.DesiredSize.Width : 
                              (element.ActualWidth > 0 ? element.ActualWidth : 
                               (element.RenderSize.Width > 0 ? element.RenderSize.Width : 100));
                elementHeight = textBlock.DesiredSize.Height > 0 ? textBlock.DesiredSize.Height : 
                               (element.ActualHeight > 0 ? element.ActualHeight : 
                                (element.RenderSize.Height > 0 ? element.RenderSize.Height : 30));
            }
            else
            {
                elementWidth = element.ActualWidth > 0 ? element.ActualWidth : 
                              (element.RenderSize.Width > 0 ? element.RenderSize.Width : 100);
                elementHeight = element.ActualHeight > 0 ? element.ActualHeight : 
                               (element.RenderSize.Height > 0 ? element.RenderSize.Height : 30);
            }

            // **第二步：直接使用传入的新位置（newLeft/newTop）计算元素在新位置的边界**
            // 辅助线应该永远紧贴元素的左上角
            var elementLeft = newLeft;  // 左边缘 = 左上角的 X 坐标
            var elementTop = newTop;    // 上边缘 = 左上角的 Y 坐标
            var elementRight = elementLeft + elementWidth;  // 右边缘（用于对齐检测）
            var elementBottom = elementTop + elementHeight; // 下边缘（用于对齐检测）
            var elementCenterX = elementLeft + elementWidth / 2;
            var elementCenterY = elementTop + elementHeight / 2;
            
            // 获取当前 Margin 仅用于日志记录
            var currentMargin = element.Margin;
            _operLog?.Information("[WelcomeSignView] 📍 辅助线计算 - 当前Margin: ({CurrentLeft}, {CurrentTop}), 新位置: ({NewLeft}, {NewTop}), 尺寸: {Width}x{Height}, 左上角: ({Left}, {Top}), 画布: {CW}x{CH}", 
                currentMargin.Left, currentMargin.Top, elementLeft, elementTop, elementWidth, elementHeight, elementLeft, elementTop, canvasWidth, canvasHeight);

            // 获取所有随行人员信息相关的元素（排除 Header 和 Footer）用于对齐检测
            var otherElements = GetEntourageInfoElements(mainGrid, element);
            
            _operLog?.Information("[WelcomeSignView] 📋 获取到 {Count} 个随行人员信息元素用于对齐检测", otherElements.Count);

            const double snapDistance = 50.0; // 对齐线显示距离（像素）- 增加到50px，更容易看到对齐线
            const double snapThreshold = 5.0; // 自动吸附阈值（小于此距离时自动吸附，5px内自动对齐）

            // 收集对齐的目标位置和距离（用于排序，只显示最近的辅助线）
            var horizontalGuideCandidates = new List<(double position, double distance)>(); // 水平辅助线候选（位置，距离）
            var verticalGuideCandidates = new List<(double position, double distance)>(); // 垂直辅助线候选（位置，距离）
            
            // 自动吸附的位置
            double? snapToX = null;
            double? snapToY = null;
            double minDistanceX = snapThreshold;
            double minDistanceY = snapThreshold;

            // 检测所有其他元素的参考点，找出对齐位置
            _operLog?.Information("[WelcomeSignView] 🔍 开始检测对齐 - 当前元素位置: ({ElementLeft:F2}, {ElementTop:F2}), 其他元素数量: {OtherCount}", 
                elementLeft, elementTop, otherElements.Count);
            
            foreach (var other in otherElements)
            {
                // **关键修复：使用当前视觉位置（如果其他元素正在拖拽，使用其当前拖拽位置）**
                // 这样可以确保辅助线显示在其他元素的当前位置，而不是初始位置
                var otherPosition = GetElementCurrentVisualPosition(other, mainGrid);
                var otherLeft = otherPosition.X;
                var otherTop = otherPosition.Y;
                
                // 获取元素的实际尺寸
                double otherWidth = 0;
                double otherHeight = 0;
                if (other is TextBlock otherTextBlock)
                {
                    otherTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    otherWidth = otherTextBlock.DesiredSize.Width > 0 ? otherTextBlock.DesiredSize.Width : 
                                (other.ActualWidth > 0 ? other.ActualWidth : other.RenderSize.Width);
                    otherHeight = otherTextBlock.DesiredSize.Height > 0 ? otherTextBlock.DesiredSize.Height : 
                                 (other.ActualHeight > 0 ? other.ActualHeight : other.RenderSize.Height);
                }
                else
                {
                    otherWidth = other.ActualWidth > 0 ? other.ActualWidth : other.RenderSize.Width;
                    otherHeight = other.ActualHeight > 0 ? other.ActualHeight : other.RenderSize.Height;
                }
                
                var otherCenterX = otherLeft + otherWidth / 2;
                var otherRight = otherLeft + otherWidth;
                var otherCenterY = otherTop + otherHeight / 2;
                var otherBottom = otherTop + otherHeight;
                
                // 计算距离，用于调试
                var distX = Math.Abs(elementLeft - otherLeft);
                var distY = Math.Abs(elementTop - otherTop);
                _operLog?.Debug("[WelcomeSignView] 📐 检测其他元素 - 名称: {Name}, 位置: ({Left:F2}, {Top:F2}), 尺寸: {Width:F2}x{Height:F2}, 距离: X={DistX:F2}, Y={DistY:F2}", 
                    other.Name ?? "未命名", otherLeft, otherTop, otherWidth, otherHeight, distX, distY);

                // 垂直辅助线检测（优先检测左边缘对齐，这是最常用的）
                CheckAlignmentForGuideWithDistance(elementLeft, otherLeft, snapDistance, snapThreshold, ref snapToX, ref minDistanceX, verticalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementLeft, otherRight, snapDistance, snapThreshold, ref snapToX, ref minDistanceX, verticalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementCenterX, otherCenterX, snapDistance, snapThreshold, ref snapToX, ref minDistanceX, verticalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementCenterX, otherLeft, snapDistance, snapThreshold, ref snapToX, ref minDistanceX, verticalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementCenterX, otherRight, snapDistance, snapThreshold, ref snapToX, ref minDistanceX, verticalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementRight, otherLeft, snapDistance, snapThreshold, ref snapToX, ref minDistanceX, verticalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementRight, otherRight, snapDistance, snapThreshold, ref snapToX, ref minDistanceX, verticalGuideCandidates);

                // 水平辅助线检测（优先检测上边缘对齐，这是最常用的）
                CheckAlignmentForGuideWithDistance(elementTop, otherTop, snapDistance, snapThreshold, ref snapToY, ref minDistanceY, horizontalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementTop, otherBottom, snapDistance, snapThreshold, ref snapToY, ref minDistanceY, horizontalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementCenterY, otherCenterY, snapDistance, snapThreshold, ref snapToY, ref minDistanceY, horizontalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementCenterY, otherTop, snapDistance, snapThreshold, ref snapToY, ref minDistanceY, horizontalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementCenterY, otherBottom, snapDistance, snapThreshold, ref snapToY, ref minDistanceY, horizontalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementBottom, otherTop, snapDistance, snapThreshold, ref snapToY, ref minDistanceY, horizontalGuideCandidates);
                CheckAlignmentForGuideWithDistance(elementBottom, otherBottom, snapDistance, snapThreshold, ref snapToY, ref minDistanceY, horizontalGuideCandidates);
            }
            
            // **关键优化：只显示每个方向最近的 2-3 条辅助线，避免显示过多**
            const int maxGuideLinesPerDirection = 3; // 每个方向最多显示 3 条辅助线
            var horizontalGuideY = new HashSet<double>();
            var verticalGuideX = new HashSet<double>();
            
            // 按距离排序，只取最近的几条
            var sortedHorizontal = horizontalGuideCandidates
                .GroupBy(g => g.position) // 按位置去重
                .Select(g => g.OrderBy(x => x.distance).First()) // 每个位置保留距离最近的
                .OrderBy(g => g.distance) // 按距离排序
                .Take(maxGuideLinesPerDirection)
                .Select(g => g.position);
            
            foreach (var pos in sortedHorizontal)
            {
                horizontalGuideY.Add(pos);
            }
            
            var sortedVertical = verticalGuideCandidates
                .GroupBy(g => g.position) // 按位置去重
                .Select(g => g.OrderBy(x => x.distance).First()) // 每个位置保留距离最近的
                .OrderBy(g => g.distance) // 按距离排序
                .Take(maxGuideLinesPerDirection)
                .Select(g => g.position);
            
            foreach (var pos in sortedVertical)
            {
                verticalGuideX.Add(pos);
            }

            // **第三步：显示十字辅助线（参考 AutoCAD 实现）**
            // AutoCAD 十字辅助线特点：
            // 1. 只有一条水平线和一条垂直线，在元素位置交叉形成十字
            // 2. 辅助线紧贴元素的左上角（左边缘和上边缘）
            // 3. 只在检测到对齐时才显示对齐辅助线（在 snapThreshold 范围内）
            
            // elementLeft 和 elementTop 已经是相对于 MainGrid 的绝对坐标，直接使用
            // 确保坐标在画布范围内
            var clampedLeft = Math.Max(0, Math.Min(elementLeft, canvasWidth));
            var clampedTop = Math.Max(0, Math.Min(elementTop, canvasHeight));
            
            _operLog?.Information("[WelcomeSignView] 辅助线位置 - 元素左上角: ({Left}, {Top}), 对齐线: H={HCount}, V={VCount}", 
                elementLeft, elementTop, horizontalGuideY.Count, verticalGuideX.Count);
            
            // 移除旧的 Adorner（如果存在，使用 context 中存储的引用）
            if (context.GuideLineAdorner != null)
            {
                adornerLayer.Remove(context.GuideLineAdorner);
            }

            // **核心原则：每个元素的Adorner装饰元素本身，即使共享AdornerLayer也不会干扰**
            // 传递 snapDistance 参数，让 Adorner 知道对齐虚线的显示距离
            context.GuideLineAdorner = new GuideLineAdorner(
                adornedElement, // 装饰元素本身，不是MainGrid！
                elementLeft, 
                elementTop, 
                elementWidth, 
                elementHeight,
                canvasWidth, 
                canvasHeight,
                horizontalGuideY, 
                verticalGuideX,
                snapDistance); // 传递对齐虚线显示距离

            // 添加到AdornerLayer（可能共享，但AdornedElement不同，所以互不干扰）
            adornerLayer.Add(context.GuideLineAdorner);
            
            _operLog?.Debug("[WelcomeSignView] ✅ 已为元素添加独立Adorner - Element: {ElementName}, AdornedElement: {AdornedName}", 
                element.Name, context.GuideLineAdorner.AdornedElement.GetType().Name);

        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 显示辅助线失败: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// 检查对齐并收集辅助线位置（使用 HashSet 去重）
    /// </summary>
    private void CheckAlignmentForGuide(double elementPos, double otherPos, double snapDistance, double snapThreshold,
        ref double? snapTo, ref double minDistance, ref HashSet<double> guidePositions)
    {
        var distance = Math.Abs(elementPos - otherPos);
        
        if (distance < snapDistance)
        {
            // 添加到辅助线位置集合（自动去重）
            guidePositions.Add(otherPos);
            
            // 如果距离小于吸附阈值，设置自动吸附位置
            if (distance < snapThreshold && distance < minDistance)
            {
                snapTo = otherPos;
                minDistance = distance;
            }
        }
    }
    
    /// <summary>
    /// 检查对齐并收集辅助线位置和距离（用于排序，只显示最近的辅助线）
    /// </summary>
    private void CheckAlignmentForGuideWithDistance(double elementPos, double otherPos, double snapDistance, double snapThreshold,
        ref double? snapTo, ref double minDistance, List<(double position, double distance)> guideCandidates)
    {
        var distance = Math.Abs(elementPos - otherPos);
        
        if (distance < snapDistance)
        {
            // 添加到候选列表（包含位置和距离信息）
            guideCandidates.Add((otherPos, distance));
            
            // 如果距离小于吸附阈值，设置自动吸附位置
            if (distance < snapThreshold && distance < minDistance)
            {
                snapTo = otherPos;
                minDistance = distance;
            }
        }
    }



    /// <summary>
    /// 显示字体大小变化提示
    /// </summary>
    private void ShowFontSizeChangeHint(TextBlock textBlock, double oldSize, double newSize, bool isAtBoundary)
    {
        try
        {
            // 如果提示已存在，先移除
            if (_fontSizeChangeToolTip != null)
            {
                _fontSizeChangeToolTip.IsOpen = false;
                _fontSizeChangeToolTip = null;
            }

            // 停止之前的定时器
            if (_fontSizeHintTimer != null)
            {
                _fontSizeHintTimer.Stop();
                _fontSizeHintTimer = null;
            }

            // 创建提示内容
            string hintText;
            if (isAtBoundary)
            {
                hintText = $"已达到限制：{oldSize}pt\n范围：20pt - 120pt";
            }
            else
            {
                hintText = $"{oldSize}pt → {newSize}pt";
            }

            // 创建 ToolTip
            _fontSizeChangeToolTip = new ToolTip
            {
                Content = hintText,
                Placement = PlacementMode.MousePoint,
                PlacementRectangle = new Rect(0, -50, 0, 0), // 在鼠标上方显示
                Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0)), // 半透明黑色背景
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(8, 4, 8, 4),
                HasDropShadow = true,
                StaysOpen = false
            };

            // 显示提示
            _fontSizeChangeToolTip.IsOpen = true;

            // 创建定时器，1.5秒后自动隐藏
            _fontSizeHintTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.5)
            };
            _fontSizeHintTimer.Tick += (s, e) =>
            {
                if (_fontSizeChangeToolTip != null)
                {
                    _fontSizeChangeToolTip.IsOpen = false;
                    _fontSizeChangeToolTip = null;
                }
                _fontSizeHintTimer?.Stop();
                _fontSizeHintTimer = null;
            };
            _fontSizeHintTimer.Start();
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 显示字体大小变化提示失败");
        }
    }

    /// <summary>
    /// 应用自动吸附功能（参考 Photoshop/CorelDraw 的智能吸附）
    /// 注意：此方法不进行边界检查，边界检查由调用者完成
    /// </summary>
    private Point ApplySnapping(FrameworkElement element, double newLeft, double newTop, Grid mainGrid)
    {
        const double snapThreshold = 5.0; // 自动吸附阈值（像素）- 5px内自动对齐

        // 获取元素尺寸（与 ShowGuideLines 保持一致）
        double elementWidth = 0;
        double elementHeight = 0;
        if (element is TextBlock textBlock)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            elementWidth = textBlock.DesiredSize.Width > 0 ? textBlock.DesiredSize.Width : 
                          (element.ActualWidth > 0 ? element.ActualWidth : element.RenderSize.Width);
            elementHeight = textBlock.DesiredSize.Height > 0 ? textBlock.DesiredSize.Height : 
                           (element.ActualHeight > 0 ? element.ActualHeight : element.RenderSize.Height);
        }
        else
        {
            elementWidth = element.ActualWidth > 0 ? element.ActualWidth : element.RenderSize.Width;
            elementHeight = element.ActualHeight > 0 ? element.ActualHeight : element.RenderSize.Height;
        }

        var elementLeft = newLeft;
        var elementCenterX = newLeft + elementWidth / 2;
        var elementRight = newLeft + elementWidth;
        var elementTop = newTop;
        var elementCenterY = newTop + elementHeight / 2;
        var elementBottom = newTop + elementHeight;

        double? snapX = null;
        double? snapY = null;
        double minDistanceX = snapThreshold;
        double minDistanceY = snapThreshold;

        // 获取所有随行人员信息相关的元素（排除 Header 和 Footer）用于对齐检测
        var otherElements = GetEntourageInfoElements(mainGrid, element);

        // 检测对齐并应用吸附
        foreach (var other in otherElements)
        {
            // **关键修复：使用布局位置（排除 Transform），吸附应该基于稳定的布局位置**
            // 注意：吸附使用布局位置，因为我们需要吸附到元素的最终位置，而不是临时拖拽位置
            var otherPosition = GetElementLayoutPosition(other, mainGrid);
            var otherLeft = otherPosition.X;
            var otherTop = otherPosition.Y;
            
            // 获取元素的实际尺寸
            double otherWidth = 0;
            double otherHeight = 0;
            if (other is TextBlock otherTextBlock)
            {
                otherTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                otherWidth = otherTextBlock.DesiredSize.Width > 0 ? otherTextBlock.DesiredSize.Width : 
                            (other.ActualWidth > 0 ? other.ActualWidth : other.RenderSize.Width);
                otherHeight = otherTextBlock.DesiredSize.Height > 0 ? otherTextBlock.DesiredSize.Height : 
                             (other.ActualHeight > 0 ? other.ActualHeight : other.RenderSize.Height);
            }
            else
            {
                otherWidth = other.ActualWidth > 0 ? other.ActualWidth : other.RenderSize.Width;
                otherHeight = other.ActualHeight > 0 ? other.ActualHeight : other.RenderSize.Height;
            }
            
            var otherCenterX = otherLeft + otherWidth / 2;
            var otherRight = otherLeft + otherWidth;
            var otherCenterY = otherTop + otherHeight / 2;
            var otherBottom = otherTop + otherHeight;

            // 水平对齐检测和吸附（优先使用左上角对齐）
            // 优先检测左上角与左上角对齐（最优先）
            CheckSnap(elementLeft, otherLeft, ref snapX, ref minDistanceX);
            // 然后检测左上角与其他边缘对齐
            CheckSnap(elementLeft, otherRight, ref snapX, ref minDistanceX);
            CheckSnap(elementLeft, otherCenterX, ref snapX, ref minDistanceX);
            // 其他对齐方式作为备选
            CheckSnap(elementCenterX, otherCenterX, ref snapX, ref minDistanceX);
            CheckSnap(elementCenterX, otherLeft, ref snapX, ref minDistanceX);
            CheckSnap(elementCenterX, otherRight, ref snapX, ref minDistanceX);
            CheckSnap(elementRight, otherLeft, ref snapX, ref minDistanceX);
            CheckSnap(elementRight, otherRight, ref snapX, ref minDistanceX);

            // 垂直对齐检测和吸附（优先使用左上角对齐）
            // 优先检测左上角与左上角对齐（最优先）
            CheckSnap(elementTop, otherTop, ref snapY, ref minDistanceY);
            // 然后检测左上角与其他边缘对齐
            CheckSnap(elementTop, otherBottom, ref snapY, ref minDistanceY);
            CheckSnap(elementTop, otherCenterY, ref snapY, ref minDistanceY);
            // 其他对齐方式作为备选
            CheckSnap(elementCenterY, otherCenterY, ref snapY, ref minDistanceY);
            CheckSnap(elementCenterY, otherTop, ref snapY, ref minDistanceY);
            CheckSnap(elementCenterY, otherBottom, ref snapY, ref minDistanceY);
            CheckSnap(elementBottom, otherTop, ref snapY, ref minDistanceY);
            CheckSnap(elementBottom, otherBottom, ref snapY, ref minDistanceY);
        }

        // 应用吸附（计算相对于元素左上角的偏移）
        var finalLeft = snapX.HasValue ? (newLeft - (elementLeft - snapX.Value)) : newLeft;
        var finalTop = snapY.HasValue ? (newTop - (elementTop - snapY.Value)) : newTop;

        return new Point(finalLeft, finalTop);
    }

    /// <summary>
    /// 检查是否应该吸附到目标位置
    /// </summary>
    private void CheckSnap(double elementPos, double targetPos, ref double? snapTo, ref double minDistance)
    {
        var distance = Math.Abs(elementPos - targetPos);
        if (distance < minDistance)
        {
            snapTo = targetPos;
            minDistance = distance;
        }
    }

    /// <summary>
    /// 应用间距约束，确保元素与其他元素保持最小间距（避免重叠）
    /// </summary>
    private Point ApplySpacingConstraint(
        FrameworkElement element, 
        double newLeft, 
        double newTop, 
        double elementWidth, 
        double elementHeight, 
        Grid mainGrid, 
        double minSpacing)
    {
        // 计算当前元素的边界
        var elementLeft = newLeft;
        var elementTop = newTop;
        var elementRight = elementLeft + elementWidth;
        var elementBottom = elementTop + elementHeight;

        // 获取所有其他元素
        var otherElements = GetEntourageInfoElements(mainGrid, element);
        
        // 用于记录需要调整的位置
        double? adjustX = null;
        double? adjustY = null;

        // 检测与其他元素的间距
        foreach (var other in otherElements)
        {
            // **关键修复：使用其他元素的当前视觉位置（如果正在拖拽，使用当前位置）**
            // 这样可以正确计算与其他元素（包括正在拖拽的元素）的间距，避免相互干扰
            var otherPosition = GetElementCurrentVisualPosition(other, mainGrid);
            var otherLeft = otherPosition.X;
            var otherTop = otherPosition.Y;
            
            // 获取其他元素的尺寸
            double otherWidth = 0;
            double otherHeight = 0;
            if (other is TextBlock otherTextBlock)
            {
                otherTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                otherWidth = otherTextBlock.DesiredSize.Width > 0 ? otherTextBlock.DesiredSize.Width : 
                            (other.ActualWidth > 0 ? other.ActualWidth : other.RenderSize.Width);
                otherHeight = otherTextBlock.DesiredSize.Height > 0 ? otherTextBlock.DesiredSize.Height : 
                             (other.ActualHeight > 0 ? other.ActualHeight : other.RenderSize.Height);
            }
            else
            {
                otherWidth = other.ActualWidth > 0 ? other.ActualWidth : other.RenderSize.Width;
                otherHeight = other.ActualHeight > 0 ? other.ActualHeight : other.RenderSize.Height;
            }
            
            var otherRight = otherLeft + otherWidth;
            var otherBottom = otherTop + otherHeight;

            // **水平间距检测（左右）**
            // 情况1：当前元素在其他元素右侧（当前元素左边缘与其他元素右边缘的距离）
            if (elementLeft >= otherRight)
            {
                var horizontalDistance = elementLeft - otherRight;
                if (horizontalDistance < minSpacing)
                {
                    // 需要向右移动，保持20px间距
                    var requiredX = otherRight + minSpacing;
                    if (!adjustX.HasValue || requiredX > adjustX.Value)
                    {
                        adjustX = requiredX;
                    }
                }
            }
            // 情况2：当前元素在其他元素左侧（其他元素左边缘与当前元素右边缘的距离）
            else if (elementRight <= otherLeft)
            {
                var horizontalDistance = otherLeft - elementRight;
                if (horizontalDistance < minSpacing)
                {
                    // 需要向左移动，保持20px间距
                    var requiredX = otherLeft - minSpacing - elementWidth;
                    if (!adjustX.HasValue || requiredX < adjustX.Value)
                    {
                        adjustX = requiredX;
                    }
                }
            }
            // 情况3：水平方向有重叠（需要分离）
            else
            {
                // 计算重叠量，决定向左还是向右移动
                var overlapLeft = Math.Max(0, elementLeft - otherLeft);
                var overlapRight = Math.Max(0, otherRight - elementRight);
                
                if (overlapLeft > 0 || overlapRight > 0)
                {
                    // 选择移动距离较小的方向
                    if (overlapLeft <= overlapRight)
                    {
                        // 向右移动
                        var requiredX = otherRight + minSpacing;
                        if (!adjustX.HasValue || requiredX > adjustX.Value)
                        {
                            adjustX = requiredX;
                        }
                    }
                    else
                    {
                        // 向左移动
                        var requiredX = otherLeft - minSpacing - elementWidth;
                        if (!adjustX.HasValue || requiredX < adjustX.Value)
                        {
                            adjustX = requiredX;
                        }
                    }
                }
            }

            // **垂直间距检测（上下）**
            // 情况1：当前元素在其他元素下方（当前元素上边缘与其他元素下边缘的距离）
            if (elementTop >= otherBottom)
            {
                var verticalDistance = elementTop - otherBottom;
                if (verticalDistance < minSpacing)
                {
                    // 需要向下移动，保持20px间距
                    var requiredY = otherBottom + minSpacing;
                    if (!adjustY.HasValue || requiredY > adjustY.Value)
                    {
                        adjustY = requiredY;
                    }
                }
            }
            // 情况2：当前元素在其他元素上方（其他元素上边缘与当前元素下边缘的距离）
            else if (elementBottom <= otherTop)
            {
                var verticalDistance = otherTop - elementBottom;
                if (verticalDistance < minSpacing)
                {
                    // 需要向上移动，保持20px间距
                    var requiredY = otherTop - minSpacing - elementHeight;
                    if (!adjustY.HasValue || requiredY < adjustY.Value)
                    {
                        adjustY = requiredY;
                    }
                }
            }
            // 情况3：垂直方向有重叠（需要分离）
            else
            {
                // 计算重叠量，决定向上还是向下移动
                var overlapTop = Math.Max(0, elementTop - otherTop);
                var overlapBottom = Math.Max(0, otherBottom - elementBottom);
                
                if (overlapTop > 0 || overlapBottom > 0)
                {
                    // 选择移动距离较小的方向
                    if (overlapTop <= overlapBottom)
                    {
                        // 向下移动
                        var requiredY = otherBottom + minSpacing;
                        if (!adjustY.HasValue || requiredY > adjustY.Value)
                        {
                            adjustY = requiredY;
                        }
                    }
                    else
                    {
                        // 向上移动
                        var requiredY = otherTop - minSpacing - elementHeight;
                        if (!adjustY.HasValue || requiredY < adjustY.Value)
                        {
                            adjustY = requiredY;
                        }
                    }
                }
            }
        }

        // 应用调整
        var finalLeft = adjustX.HasValue ? adjustX.Value : newLeft;
        var finalTop = adjustY.HasValue ? adjustY.Value : newTop;

        return new Point(finalLeft, finalTop);
    }

    /// <summary>
    /// 获取元素的显示文本（用于日志记录）
    /// </summary>
    private string GetElementDisplayText(FrameworkElement element, string elementName)
    {
        if (element is TextBlock textBlock && !string.IsNullOrWhiteSpace(textBlock.Text))
        {
            // 优先使用 TextBlock 的文本内容
            return $"\"{textBlock.Text}\"";
        }
        else if (!string.IsNullOrWhiteSpace(elementName))
        {
            // 其次使用元素名称
            return elementName;
        }
        else
        {
            // 最后使用元素的类型名称
            return element.GetType().Name;
        }
    }

    /// <summary>
    /// 计算排除 Header 和 Footer 后的拖拽边界区域
    /// 注意：由于视口是动态的，每次调用都会重新计算，确保获取最新的位置和尺寸
    /// </summary>
    /// <returns>返回 (minLeft, maxLeft, minTop, maxTop) 元组</returns>
    private (double minLeft, double maxLeft, double minTop, double maxTop) GetDragBounds(
        Grid mainGrid, 
        double elementWidth, 
        double elementHeight)
    {
        // **关键：强制更新布局，确保获取最新的视口尺寸（处理动态视口）**
        mainGrid.UpdateLayout();
        
        // 获取容器的最新尺寸（考虑动态视口变化）
        var containerWidth = mainGrid.ActualWidth > 0 ? mainGrid.ActualWidth : 
                             (mainGrid.RenderSize.Width > 0 ? mainGrid.RenderSize.Width : 1920);
        var containerHeight = mainGrid.ActualHeight > 0 ? mainGrid.ActualHeight : 
                              (mainGrid.RenderSize.Height > 0 ? mainGrid.RenderSize.Height : 1080);

        // 查找 Header 和 Footer 元素（每次重新查找，因为布局可能变化）
        FrameworkElement? headerElement = null;
        FrameworkElement? footerElement = null;
        var headerStyle = FindResource("WelcomeHeaderStyle") as Style;
        var footerStyle = FindResource("WelcomeFooterStyle") as Style;

        if (headerStyle != null || footerStyle != null)
        {
            foreach (TextBlock? tb in FindVisualChildren<TextBlock>(mainGrid))
            {
                if (tb == null || tb.Style == null || tb.Visibility != Visibility.Visible) continue;
                
                if (headerStyle != null && ReferenceEquals(tb.Style, headerStyle))
                {
                    headerElement = tb;
                }
                else if (footerStyle != null && ReferenceEquals(tb.Style, footerStyle))
                {
                    footerElement = tb;
                }
            }
        }

        // 计算 Header 底部位置（排除区域）
        double headerBottom = 0.0;
        if (headerElement != null && headerElement.Visibility == Visibility.Visible && headerElement.IsLoaded)
        {
            // **关键：强制更新 Header 的布局，确保获取最新的位置和尺寸**
            headerElement.UpdateLayout();
            headerElement.InvalidateMeasure();
            headerElement.Measure(new Size(containerWidth, double.PositiveInfinity));
            
            // 获取 Header 相对于 MainGrid 的实际位置（考虑动态定位）
            var headerPosition = headerElement.TransformToAncestor(mainGrid).Transform(new Point(0, 0));
            
            // 优先使用实际渲染的高度，其次使用 DesiredSize，最后使用估算值
            double headerHeight = 0.0;
            if (headerElement.ActualHeight > 0)
            {
                headerHeight = headerElement.ActualHeight;
            }
            else if (headerElement.DesiredSize.Height > 0)
            {
                headerHeight = headerElement.DesiredSize.Height;
            }
            else if (headerElement.RenderSize.Height > 0)
            {
                headerHeight = headerElement.RenderSize.Height;
            }
            else if (headerElement is TextBlock headerTb)
            {
                // 估算：字体大小 + 行高额外空间
                headerHeight = headerTb.FontSize * 1.2 + 10;
            }
            else
            {
                headerHeight = 70; // 默认高度
            }
            
            // 考虑 Header 的 Margin（底部边距）
            var headerMargin = headerElement.Margin;
            headerBottom = headerPosition.Y + headerHeight + headerMargin.Bottom;
            
            _operLog?.Information("[WelcomeSignView] 📏 Header 区域（动态计算） - 位置: ({X}, {Y}), 高度: {Height}, Margin: ({Left}, {Top}, {Right}, {Bottom}), 底部边界: {Bottom}",
                headerPosition.X, headerPosition.Y, headerHeight, headerMargin.Left, headerMargin.Top, headerMargin.Right, headerMargin.Bottom, headerBottom);
        }

        // 计算 Footer 顶部位置（排除区域）
        double footerTop = containerHeight;
        if (footerElement != null && footerElement.Visibility == Visibility.Visible && footerElement.IsLoaded)
        {
            // **关键：强制更新 Footer 的布局，确保获取最新的位置和尺寸**
            footerElement.UpdateLayout();
            footerElement.InvalidateMeasure();
            footerElement.Measure(new Size(containerWidth, double.PositiveInfinity));
            
            // 获取 Footer 相对于 MainGrid 的实际位置（考虑动态定位：VerticalAlignment="Bottom"）
            var footerPosition = footerElement.TransformToAncestor(mainGrid).Transform(new Point(0, 0));
            
            // Footer 的顶部位置就是拖拽区域的底部边界
            // 考虑 Footer 的 Margin（顶部边距，Footer 在底部时通常只有顶部边距）
            var footerMargin = footerElement.Margin;
            
            // Footer 使用 VerticalAlignment="Bottom"，所以位置是从底部计算的
            // 实际顶部位置 = 容器的底部 - Footer 高度 - Footer 的底部边距
            // 但 TransformToAncestor 已经给出了实际位置，所以直接使用即可
            footerTop = footerPosition.Y - footerMargin.Top;
            
            // 确保 Footer 顶部不会小于 Header 底部（防止负的拖拽区域）
            if (footerTop < headerBottom)
            {
                footerTop = headerBottom;
                _operLog?.Warning("[WelcomeSignView] ⚠️ Footer 顶部位置小于 Header 底部，已调整为 Header 底部位置");
            }
            
            _operLog?.Information("[WelcomeSignView] 📏 Footer 区域（动态计算） - 位置: ({X}, {Y}), Margin: ({Left}, {Top}, {Right}, {Bottom}), 顶部边界: {Top}, 容器高度: {ContainerHeight}",
                footerPosition.X, footerPosition.Y, footerMargin.Left, footerMargin.Top, footerMargin.Right, footerMargin.Bottom, footerTop, containerHeight);
        }

        // 计算拖拽边界（排除 Header 和 Footer）
        var minLeft = 0.0;
        var maxLeft = Math.Max(minLeft, containerWidth - elementWidth);
        var minTop = headerBottom; // 从 Header 底部开始
        var maxTop = Math.Max(minTop, footerTop - elementHeight); // 到 Footer 顶部结束
        
        // **保护逻辑：确保边界值有效（处理动态视口变化）**
        if (maxTop < minTop)
        {
            // 如果 Footer 顶部小于 Header 底部（窗口太小），至少保留最小空间
            maxTop = minTop + elementHeight;
            _operLog?.Warning("[WelcomeSignView] ⚠️ 拖拽区域无效（Footer 顶部 < Header 底部），已调整 maxTop = minTop + elementHeight");
        }
        
        if (maxTop < minTop + elementHeight)
        {
            // 如果拖拽区域不足以容纳元素，允许元素稍微超出容器边界（但仍不能进入 Header/Footer 区域）
            maxTop = Math.Max(minTop, containerHeight - elementHeight);
            _operLog?.Warning("[WelcomeSignView] ⚠️ 拖拽区域过小（Header/Footer 区域重叠或太近），已调整为允许元素在容器范围内移动");
        }

        _operLog?.Information("[WelcomeSignView] 📐 拖拽边界计算（动态视口） - 容器: {CW}x{CH}, 元素: {EW}x{EH}, Header底部: {HBottom}, Footer顶部: {FTop}, 边界: X[{MinX}, {MaxX}], Y[{MinY}, {MaxY}]",
            containerWidth, containerHeight, elementWidth, elementHeight, headerBottom, footerTop, minLeft, maxLeft, minTop, maxTop);

        return (minLeft, maxLeft, minTop, maxTop);
    }

    /// <summary>
    /// 获取所有随行人员信息相关的元素（排除 Header 和 Footer，以及当前正在拖拽的元素本身）
    /// 包括：公司（Company）、部门（Dept）、人员（Person）、职务（Post）
    /// 
    /// **draw.io 风格设计**：
    /// - 只排除当前正在拖拽的元素本身（excludeElement）
    /// - 不排除其他正在拖拽的元素，因为需要使用它们的当前位置计算间距
    /// - 使用 GetElementCurrentVisualPosition 获取其他元素的当前位置（包括正在拖拽的）
    /// </summary>
    private List<FrameworkElement> GetEntourageInfoElements(Grid mainGrid, FrameworkElement? excludeElement = null)
    {
        var visitorInfoElements = new List<FrameworkElement>();
        
        // 获取样式资源用于判断元素类型
        var headerStyle = FindResource("WelcomeHeaderStyle") as Style;
        var footerStyle = FindResource("WelcomeFooterStyle") as Style;
        var companyStyle = FindResource("WelcomeCompanyStyle") as Style;
        var deptStyle = FindResource("WelcomeDeptStyle") as Style;
        var personStyle = FindResource("WelcomePersonStyle") as Style;
        
        // **关键修复：确保能找到ItemsControl中的元素**
        // 查找所有 TextBlock 元素（包括ItemsControl中动态生成的）
        var allTextBlocks = FindVisualChildren<TextBlock>(mainGrid).ToList();
        
        _operLog?.Information("[WelcomeSignView] 🔍 GetEntourageInfoElements: 从MainGrid中找到 {Count} 个TextBlock", allTextBlocks.Count);
        
        foreach (TextBlock? tb in allTextBlocks)
        {
            if (tb == null || tb.Visibility != Visibility.Visible || tb == excludeElement) continue;
            
            // 排除 Header 和 Footer
            if (headerStyle != null && ReferenceEquals(tb.Style, headerStyle))
            {
                _operLog?.Debug("[WelcomeSignView] 🔍 跳过Header元素: {Name}", tb.Name ?? "未命名");
                continue;
            }
            if (footerStyle != null && ReferenceEquals(tb.Style, footerStyle))
            {
                _operLog?.Debug("[WelcomeSignView] 🔍 跳过Footer元素: {Name}", tb.Name ?? "未命名");
                continue;
            }
            
            // 只包含随行人员信息相关的元素：Company、Dept、Person、Post
            // **关键修复：使用Name属性识别，因为XAML使用了BasedOn样式，样式引用不相等**
            bool isEntourageInfo = false;
            string elementType = "";
            
            // 优先使用Name属性判断（最可靠）
            if (tb.Name == "CompanyTextBlockItem")
            {
                isEntourageInfo = true; // 公司（在ItemsControl中）
                elementType = "Company";
            }
            else if (tb.Name == "DeptTextBlock")
            {
                isEntourageInfo = true; // 部门
                elementType = "Dept";
            }
            else if (tb.Name == "PersonPostTextBlock")
            {
                isEntourageInfo = true; // 人员或职务
                elementType = "Person/Post";
            }
            // 备选方案：通过样式判断（如果Name为空）
            else if (companyStyle != null && tb.Style != null && (ReferenceEquals(tb.Style, companyStyle) || tb.Style.BasedOn == companyStyle))
            {
                isEntourageInfo = true; // 公司
                elementType = "Company";
            }
            else if (deptStyle != null && tb.Style != null && (ReferenceEquals(tb.Style, deptStyle) || tb.Style.BasedOn == deptStyle))
            {
                isEntourageInfo = true; // 部门
                elementType = "Dept";
            }
            else if (personStyle != null && tb.Style != null && (ReferenceEquals(tb.Style, personStyle) || tb.Style.BasedOn == personStyle))
            {
                isEntourageInfo = true; // 人员或职务
                elementType = "Person/Post";
            }
            
            if (isEntourageInfo)
            {
                visitorInfoElements.Add(tb);
                _operLog?.Debug("[WelcomeSignView] ✅ 添加随行人员信息元素: 类型={Type}, 名称={Name}, 样式={StyleName}", 
                    elementType, tb.Name ?? "未命名", tb.Style?.GetType().Name ?? "无样式");
            }
        }
        
        _operLog?.Information("[WelcomeSignView] 🔍 GetEntourageInfoElements: 找到 {Count} 个随行人员信息元素（已排除 Header/Footer/当前拖拽元素，包含其他正在拖拽的元素以获取实时位置）", visitorInfoElements.Count);
        
        return visitorInfoElements;
    }
    
    /// <summary>
    /// 获取元素的布局位置（排除 Transform 的影响）
    /// 用于在拖拽过程中获取其他元素的稳定位置，避免基于临时 Transform 位置进行吸附
    /// </summary>
    private Point GetElementLayoutPosition(FrameworkElement element, Grid mainGrid)
    {
        // **关键修复：只有正在拖拽的元素才返回InitialElementPosition**
        // 已结束拖拽的元素必须返回实际Margin位置，否则会相互干扰
        if (_dragContexts.TryGetValue(element, out var dragContext) && dragContext.IsDragging)
        {
            return dragContext.InitialElementPosition;
        }
        else
        {
            // 元素不在拖拽中，获取实际布局位置
            // 对于直接子元素，直接从 Margin 获取
            if (element.Parent == mainGrid)
            {
                var margin = element.Margin;
                return new Point(margin.Left, margin.Top);
            }
            else
            {
                // 嵌套元素：递归计算绝对位置（基于Margin）
                double absX = 0;
                double absY = 0;
                FrameworkElement? current = element;
                
                while (current != null && current != mainGrid)
                {
                    var margin = current.Margin;
                    absX += margin.Left;
                    absY += margin.Top;
                    current = current.Parent as FrameworkElement;
                }
                
                return new Point(absX, absY);
            }
        }
    }
    
    /// <summary>
    /// 获取元素的当前视觉位置（包括拖拽时的Transform）
    /// **关键修复：计算与其他元素间距时，应该使用其他元素的当前视觉位置，而不是布局位置**
    /// 如果元素正在被拖拽，返回 InitialPosition + Transform（当前视觉位置）
    /// 如果元素不在拖拽中，返回布局位置
    /// </summary>
    private Point GetElementCurrentVisualPosition(FrameworkElement element, Grid mainGrid)
    {
        // **关键修复：只有正在拖拽的元素才返回视觉位置，已结束拖拽的返回实际Margin位置**
        if (_dragContexts.TryGetValue(element, out var dragContext) && dragContext.IsDragging && dragContext.DragTransform != null)
        {
            // 返回当前视觉位置 = 初始布局位置 + Transform偏移
            var visualPos = new Point(
                dragContext.InitialElementPosition.X + dragContext.DragTransform.X,
                dragContext.InitialElementPosition.Y + dragContext.DragTransform.Y);
            
            _operLog?.Debug("[WelcomeSignView] 📍 GetElementCurrentVisualPosition - 元素: {Name}, 正在拖拽, 视觉位置: ({X:F2}, {Y:F2})", 
                element.Name ?? "未命名", visualPos.X, visualPos.Y);
            
            return visualPos;
        }
        else
        {
            // 元素不在拖拽中或已结束拖拽，返回实际布局位置（Margin）
            var layoutPos = GetElementLayoutPosition(element, mainGrid);
            
            // **关键修复：如果元素不在拖拽中，使用TransformToAncestor获取真实视觉位置**
            // 因为元素可能有Transform但没有在拖拽状态
            try
            {
                var actualVisualPos = element.TransformToAncestor(mainGrid).Transform(new Point(0, 0));
                _operLog?.Debug("[WelcomeSignView] 📍 GetElementCurrentVisualPosition - 元素: {Name}, 未拖拽, 布局位置: ({LayoutX:F2}, {LayoutY:F2}), 视觉位置: ({VisualX:F2}, {VisualY:F2})", 
                    element.Name ?? "未命名", layoutPos.X, layoutPos.Y, actualVisualPos.X, actualVisualPos.Y);
                return actualVisualPos;
            }
            catch
            {
                _operLog?.Debug("[WelcomeSignView] 📍 GetElementCurrentVisualPosition - 元素: {Name}, 未拖拽, 使用布局位置: ({X:F2}, {Y:F2})", 
                    element.Name ?? "未命名", layoutPos.X, layoutPos.Y);
                return layoutPos;
            }
        }
    }

    /// <summary>
    /// 清除辅助线和坐标标签
    /// **关键改进：支持元素独立的AdornerLayer**
    /// </summary>
    private void ClearGuideLines(DragContext? context = null)
    {
        if (context != null && context.GuideLineAdorner != null)
        {
            // **关键改进：从元素自己的AdornerLayer或容器AdornerLayer中移除**
            var adornedElement = context.GuideLineAdorner.AdornedElement;
            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            
            if (adornerLayer != null)
            {
                adornerLayer.Remove(context.GuideLineAdorner);
                _operLog?.Debug("[WelcomeSignView] 已清除元素独立AdornerLayer中的辅助线: {ElementName}", context.Element.Name);
            }
            
            context.GuideLineAdorner = null;
        }
        else if (_guideLineAdorner != null)
        {
            // 兼容旧代码：清除共享的 Adorner
            var mainGrid = FindName("MainGrid") as Grid;
            if (mainGrid != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(mainGrid);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(_guideLineAdorner);
                }
            }
            _guideLineAdorner = null;
        }
    }

    /// <summary>
    /// 显示拖拽预览（虚线边框）
    /// **关键修复：强制装饰元素本身，绝不装饰MainGrid**
    /// </summary>
    private void ShowDragPreview(FrameworkElement element, double left, double top, double width, double height, Grid mainGrid, DragContext context)
    {
        try
        {
            // **核心原则：无论是否有AdornerLayer，都必须装饰元素本身**
            // 获取AdornerLayer（向上查找，可能共享，但AdornedElement不同就不干扰）
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer == null)
            {
                adornerLayer = AdornerLayer.GetAdornerLayer(mainGrid);
                if (adornerLayer == null)
                {
                    _operLog?.Warning("[WelcomeSignView] AdornerLayer 未找到，跳过预览显示");
                    return;
                }
            }

            // 移除旧的预览Adorner（使用 context 中存储的引用）
            if (context.DragPreviewAdorner != null)
            {
                // 通过AdornedElement获取对应的AdornerLayer来移除
                var oldAdornedElement = context.DragPreviewAdorner.AdornedElement;
                var oldAdornerLayer = AdornerLayer.GetAdornerLayer(oldAdornedElement);
                oldAdornerLayer?.Remove(context.DragPreviewAdorner);
            }

            // **强制在元素本身上创建Adorner（attachToElement = true），自动跟随Transform移动**
            // 这样即使多个元素共享AdornerLayer，它们也不会相互干扰
            context.DragPreviewAdorner = new DragPreviewAdorner(element, 0, 0, 0, 0, attachToElement: true);
            adornerLayer.Add(context.DragPreviewAdorner);
            
            _operLog?.Debug("[WelcomeSignView] ✅ 已为元素添加独立预览Adorner - Element: {ElementName}", element.Name);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] 显示拖拽预览失败: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// 清除拖拽预览
    /// **关键修复：通过AdornedElement精确移除，只清除当前元素的Adorner**
    /// </summary>
    private void ClearDragPreview(DragContext? context = null)
    {
        if (context != null && context.DragPreviewAdorner != null)
        {
            // **核心原则：通过AdornedElement获取对应的AdornerLayer，精确移除**
            var adornedElement = context.DragPreviewAdorner.AdornedElement;
            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            
            if (adornerLayer != null)
            {
                adornerLayer.Remove(context.DragPreviewAdorner);
                _operLog?.Debug("[WelcomeSignView] ✅ 已清除元素独立预览Adorner - Element: {ElementName}, AdornedElement: {AdornedName}", 
                    context.Element.Name, adornedElement.GetType().Name);
            }
            else
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ 无法找到AdornerLayer，可能已释放");
            }
            
            context.DragPreviewAdorner = null;
        }
        else if (_dragPreviewAdorner != null)
        {
            // 兼容旧代码：清除共享的 Adorner（已废弃，保留用于向后兼容）
            var mainGrid = FindName("MainGrid") as Grid;
            if (mainGrid != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(mainGrid);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(_dragPreviewAdorner);
                }
            }
            _dragPreviewAdorner = null;
        }
    }

    [Obsolete("旧方法，已改用 AdornerLayer 实现")]
    private void ClearGuideLinesOld()
    {
        // 此方法已废弃，使用 AdornerLayer 实现后不再需要
        // 保留此方法仅用于向后兼容
    }

    // 网格功能已移除，如需可改用 AdornerLayer 实现
}


