// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visitors
// 文件名称：DigitalSignageViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：数字标牌视图模型
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Takt.Application.Dtos.Logistics.Visitors;
using Takt.Application.Services.Logistics.Visitors;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Logistics.Visitors;

/// <summary>
/// 数字标牌视图模型
/// 用于显示欢迎牌或广告视频
/// </summary>
public partial class DigitalSignageViewModel : ObservableObject, IDisposable
{
    private readonly IVisitorService _visitorService;
    private readonly IVisitorDetailService _visitorDetailService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    private Timer? _refreshTimer;
    private bool _disposed = false;

    // 当前显示的访客信息
    [ObservableProperty]
    private VisitorDto? _currentVisitor;

    [ObservableProperty]
    private ObservableCollection<VisitorDetailDto> _currentVisitorDetails = new();

    // 是否显示访客信息（true=显示访客信息，false=显示广告视频）
    [ObservableProperty]
    private bool _showVisitorInfo;

    // 广告视频路径
    [ObservableProperty]
    private string? _adVideoPath;

    // 刷新间隔（秒）
    [ObservableProperty]
    private int _refreshInterval = 30; // 默认30秒刷新一次

    [ObservableProperty]
    private string? _errorMessage;

    public DigitalSignageViewModel(
        IVisitorService visitorService,
        IVisitorDetailService visitorDetailService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _visitorService = visitorService ?? throw new ArgumentNullException(nameof(visitorService));
        _visitorDetailService = visitorDetailService ?? throw new ArgumentNullException(nameof(visitorDetailService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        // 设置默认广告视频路径（可以从配置中读取）
        AdVideoPath = "Videos/Advertisement.mp4"; // 默认路径，可以后续从配置读取

        // 立即加载一次
        _ = LoadCurrentVisitorAsync();

        // 启动定时刷新
        StartRefreshTimer();
    }

    /// <summary>
    /// 启动定时刷新
    /// </summary>
    private void StartRefreshTimer()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = new Timer(async _ => await LoadCurrentVisitorAsync(), null, 
            TimeSpan.Zero, 
            TimeSpan.FromSeconds(RefreshInterval));
    }

    /// <summary>
    /// 加载当前时间范围内的访客信息
    /// </summary>
    private async Task LoadCurrentVisitorAsync()
    {
        try
        {
            var now = DateTime.Now;

            // 查询当前时间范围内的访客
            // 查询所有未删除的访客，然后在内存中筛选当前时间范围内的
            var query = new VisitorQueryDto
            {
                CompanyName = null,
                StartTimeFrom = null, // 不限制开始时间
                StartTimeTo = null,   // 不限制结束时间
                PageIndex = 1,
                PageSize = 1000 // 获取足够多的记录以便筛选
            };

            var result = await _visitorService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                _operLog?.Warning("[DigitalSignageView] 查询访客列表失败: {Message}", result.Message);
                ShowVisitorInfo = false;
                CurrentVisitor = null;
                CurrentVisitorDetails.Clear();
                return;
            }

            // 筛选出当前时间范围内的访客（StartTime <= now <= EndTime）
            var activeVisitor = result.Data.Items
                .FirstOrDefault(v => v.StartTime <= now && now <= v.EndTime && v.IsDeleted == 0);

            if (activeVisitor != null)
            {
                // 找到当前时间范围内的访客，显示访客信息
                CurrentVisitor = activeVisitor;
                ShowVisitorInfo = true;

                // 加载访客详情
                await LoadVisitorDetailsAsync(activeVisitor.Id);
            }
            else
            {
                // 没有当前时间范围内的访客，显示广告视频
                CurrentVisitor = null;
                CurrentVisitorDetails.Clear();
                ShowVisitorInfo = false;
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[DigitalSignageView] 加载当前访客信息失败");
            ErrorMessage = ex.Message;
            ShowVisitorInfo = false;
            CurrentVisitor = null;
            CurrentVisitorDetails.Clear();
        }
    }

    /// <summary>
    /// 加载访客详情
    /// </summary>
    private async Task LoadVisitorDetailsAsync(long visitorId)
    {
        try
        {
            var query = new VisitorDetailQueryDto
            {
                VisitorId = visitorId,
                PageIndex = 1,
                PageSize = 100 // 获取所有详情
            };

            var result = await _visitorDetailService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                CurrentVisitorDetails.Clear();
                foreach (var detail in result.Data.Items)
                {
                    CurrentVisitorDetails.Add(detail);
                }
            }
            else
            {
                CurrentVisitorDetails.Clear();
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[DigitalSignageView] 加载访客详情失败");
            CurrentVisitorDetails.Clear();
        }
    }

    /// <summary>
    /// 手动刷新
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadCurrentVisitorAsync();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _refreshTimer?.Dispose();
        _refreshTimer = null;
        _disposed = true;
    }
}

