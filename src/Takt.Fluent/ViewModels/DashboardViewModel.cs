//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : DashboardViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 仪表盘视图模型
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Microsoft.Extensions.DependencyInjection;
using Takt.Common.Context;
using Takt.Domain.Interfaces;
using System.Globalization;
using System.Windows.Threading;

namespace Takt.Fluent.ViewModels;

/// <summary>
/// 仪表盘视图模型
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _greetingText = string.Empty;

    [ObservableProperty]
    private int _onlineUsers = 0;

    [ObservableProperty]
    private int _todayInbound = 0;

    [ObservableProperty]
    private int _todayOutbound = 0;

    [ObservableProperty]
    private int _todayVisitors = 0;

    private DispatcherTimer? _animationTimer;
    private int _targetOutbound = 0;
    private int _currentOutbound = 0;
    private const int AnimationStep = 5;

    private readonly ILocalizationManager? _localizationManager;

    public DashboardViewModel(ILocalizationManager? localizationManager = null)
    {
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();

        // 初始化数据
        LoadData();

        // 初始化欢迎语
        UpdateGreeting();
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    public void LoadData()
    {
        RefreshDashboardStats();

        // TODO: 从服务获取真实数据
        var targetOutbound = 1000; // 示例值

        // 启动动画
        StartOutboundAnimation(targetOutbound);
    }

    /// <summary>
    /// 启动今日出库数值动画
    /// </summary>
    private void StartOutboundAnimation(int targetValue)
    {
        _targetOutbound = targetValue;
        _currentOutbound = 1; // 从1开始
        TodayOutbound = 1;

        // 停止之前的定时器
        if (_animationTimer != null)
        {
            _animationTimer.Stop();
            _animationTimer = null;
        }

        // 创建定时器，每50毫秒更新一次（步长为5，所以200次更新）
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 50ms 更新一次，步长5，约10ms/步
        };

        _animationTimer.Tick += (s, e) =>
        {
            if (_currentOutbound < _targetOutbound)
            {
                _currentOutbound = Math.Min(_currentOutbound + AnimationStep, _targetOutbound);
                TodayOutbound = _currentOutbound;
            }
            else
            {
                // 动画完成，停止定时器
                _animationTimer?.Stop();
                _animationTimer = null;
            }
        };

        _animationTimer.Start();
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        if (_animationTimer != null)
        {
            _animationTimer.Stop();
            _animationTimer = null;
        }
    }

    /// <summary>
    /// 刷新仪表盘统计数据
    /// </summary>
    public void RefreshDashboardStats()
    {
        UpdateOnlineUsers();
    }

    private void UpdateOnlineUsers()
    {
        try
        {
            var onlineUsers = UserContext.GetAllUsers();
            OnlineUsers = onlineUsers?.Count ?? 0;
        }
        catch
        {
            OnlineUsers = 0;
        }
    }

    /// <summary>
    /// 获取翻译文本（如果找不到翻译，返回默认值）
    /// </summary>
    private string GetTranslation(string key, string defaultValue)
    {
        if (_localizationManager == null) return defaultValue;
        var translation = _localizationManager.GetString(key);
        return (translation == key) ? defaultValue : translation;
    }

    /// <summary>
    /// 更新欢迎语
    /// </summary>
    public void UpdateGreeting()
    {
        var greetingInfo = GetGreetingResource(DateTime.Now);
        var greetingText = GetTranslation(greetingInfo.Key, greetingInfo.DefaultValue);

        var userContext = UserContext.Current;
        var displayName = !string.IsNullOrWhiteSpace(userContext.RealName)
            ? userContext.RealName
            : userContext.Username;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = GetTranslation("dashboard.greeting.anonymousName", "访客");
        }

        var now = DateTime.Now;
        var culture = CultureInfo.CurrentCulture;
        var calendar = culture.Calendar;
        var weekdayName = culture.DateTimeFormat.GetDayName(now.DayOfWeek);
        var dayOfYearText = now.DayOfYear.ToString("D3", culture);
        var quarter = ((now.Month - 1) / 3) + 1;
        var quarterText = quarter.ToString("D2", culture);
        var weekRule = culture.DateTimeFormat.CalendarWeekRule;
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        var weekOfYear = calendar.GetWeekOfYear(now, weekRule, firstDayOfWeek);
        var weekOfYearText = weekOfYear.ToString("D2", culture);

        var fullFormat = GetTranslation(
            "dashboard.greeting.fullFormat",
            "{0}，欢迎 {1}，今天是{2}年{3}月{4}日，{5}，（第{6}天，第{7}季，第{8}周）");

        var weekdayValue = weekdayName;
        if (fullFormat.Contains("星期{5}", StringComparison.Ordinal) &&
            weekdayValue.StartsWith("星期", StringComparison.Ordinal))
        {
            weekdayValue = weekdayValue.Substring(2);
        }

        GreetingText = string.Format(
            culture,
            fullFormat,
            greetingText,
            displayName,
            now.Year.ToString("D4", culture),
            now.Month.ToString("D2", culture),
            now.Day.ToString("D2", culture),
            weekdayValue,
            dayOfYearText,
            quarterText,
            weekOfYearText);
    }

    private static (string Key, string DefaultValue) GetGreetingResource(DateTime timestamp)
    {
        var hour = timestamp.Hour;

        if (hour >= 5 && hour < 12)
        {
            return ("dashboard.greeting.morning", "早上好");
        }

        if (hour >= 12 && hour < 14)
        {
            return ("dashboard.greeting.noon", "中午好");
        }

        if (hour >= 14 && hour < 18)
        {
            return ("dashboard.greeting.afternoon", "下午好");
        }

        if (hour >= 18 && hour < 22)
        {
            return ("dashboard.greeting.evening", "晚上好");
        }

        return ("dashboard.greeting.night", "夜深了，请注意休息");
    }
}

