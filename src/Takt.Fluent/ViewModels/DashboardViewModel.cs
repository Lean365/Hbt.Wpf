//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : DashboardViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 仪表盘视图模型
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Application.Services.Logistics.Serials;
using Takt.Common.Context;
using Takt.Domain.Interfaces;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Threading;
using Takt.Fluent.Models;

namespace Takt.Fluent.ViewModels;

/// <summary>
/// 仪表盘视图模型
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _greetingLine1 = string.Empty;

    [ObservableProperty]
    private string _greetingLine2 = string.Empty;

    [ObservableProperty]
    private int _onlineUsers = 0;

    [ObservableProperty]
    private int _todayInbound = 0;

    [ObservableProperty]
    private int _todayOutbound = 0;

    [ObservableProperty]
    private int _todayEntourage = 0;

    // 目的地进度数据（USA, EUR, CHINA, JAPAN, OTHER）
    [ObservableProperty]
    private double _destinationProgress1 = 0.0; // USA

    [ObservableProperty]
    private double _destinationProgress2 = 0.0; // EUR

    [ObservableProperty]
    private double _destinationProgress3 = 0.0; // CHINA

    [ObservableProperty]
    private double _destinationProgress4 = 0.0; // JAPAN

    [ObservableProperty]
    private double _destinationProgress5 = 0.0; // OTHER

    // 日历数据
    [ObservableProperty]
    private DateTime _calendarDate = DateTime.Now;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today; // 默认选中今天

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _calendarDays = new();

    [ObservableProperty]
    private string _calendarMonthName = string.Empty;

    private DispatcherTimer? _animationTimer;
    private int _targetOutbound = 0;
    private int _currentOutbound = 0;
    private const int AnimationStep = 5;

    private readonly ILocalizationManager? _localizationManager;
    private readonly IProdSerialOutboundService? _prodSerialOutboundService;

    public DashboardViewModel(ILocalizationManager? localizationManager = null!, 
                              IProdSerialOutboundService? prodSerialOutboundService = null)
    {
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _prodSerialOutboundService = prodSerialOutboundService ?? App.Services?.GetService<IProdSerialOutboundService>();

        // 初始化数据
        LoadData();

        // 初始化欢迎语
        UpdateGreeting();

        // 初始化日历
        UpdateCalendar();
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    public void LoadData()
    {
        RefreshDashboardStats();

        // 加载目的地统计数据
        _ = LoadDestinationStatsAsync();

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
    /// 更新欢迎语
    /// </summary>
    public void UpdateGreeting()
    {
        var greetingKey = GetGreetingResource(DateTime.Now);
        var greetingText = _localizationManager?.GetString(greetingKey);

        var userContext = UserContext.Current;
        var displayName = !string.IsNullOrWhiteSpace(userContext.RealName)
            ? userContext.RealName
            : userContext.Username;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = _localizationManager?.GetString("dashboard.greeting.anonymousname");
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

        var fullFormat = _localizationManager?.GetString("dashboard.greeting.fullformat");

        var weekdayValue = weekdayName;
        if (fullFormat != null && fullFormat.Contains("星期{5}", StringComparison.Ordinal) &&
            weekdayValue.StartsWith("星期", StringComparison.Ordinal))
        {
            weekdayValue = weekdayValue.Substring(2);
        }

        // 第一行：问候语 + 用户名
        var line1Format = _localizationManager?.GetString("dashboard.greeting.line1format");
        GreetingLine1 = string.Format(
            culture,
            line1Format ?? string.Empty,
            greetingText ?? string.Empty,
            displayName);

        // 第二行：日期信息
        var line2Format = _localizationManager?.GetString("dashboard.greeting.line2format");
        GreetingLine2 = string.Format(
            culture,
            line2Format ?? string.Empty,
            now.Year.ToString("D4", culture),
            now.Month.ToString("D2", culture),
            now.Day.ToString("D2", culture),
            weekdayValue,
            dayOfYearText,
            quarterText,
            weekOfYearText);
    }

    private static string GetGreetingResource(DateTime timestamp)
    {
        var hour = timestamp.Hour;

        if (hour >= 5 && hour < 12)
        {
            return "dashboard.greeting.morning";
        }

        if (hour >= 12 && hour < 14)
        {
            return "dashboard.greeting.noon";
        }

        if (hour >= 14 && hour < 18)
        {
            return "dashboard.greeting.afternoon";
        }

        if (hour >= 18 && hour < 22)
        {
            return "dashboard.greeting.evening";
        }

        return "dashboard.greeting.night";
    }

    /// <summary>
    /// 更新日历
    /// </summary>
    public void UpdateCalendar()
    {
        CalendarDays.Clear();

        // 更新月份名称
        var culture = CultureInfo.CurrentCulture;
        CalendarMonthName = culture.DateTimeFormat.GetMonthName(CalendarDate.Month);

        var firstDayOfMonth = new DateTime(CalendarDate.Year, CalendarDate.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        
        // 获取本月第一天是星期几（转换为周一为0的格式）
        var firstDayOfWeek = firstDayOfMonth.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)firstDayOfMonth.DayOfWeek - 1;
        
        // 添加上个月的日期
        for (int i = firstDayOfWeek - 1; i >= 0; i--)
        {
            var date = firstDayOfMonth.AddDays(-i - 1);
            CalendarDays.Add(new CalendarDay
            {
                Day = date.Day,
                IsCurrentMonth = false,
                IsToday = date.Date == DateTime.Today,
                IsSelected = date.Date == SelectedDate.Date,
                ActualDate = date.Date,
                // 非当前月份的文字颜色在 XAML 中通过 Style 处理
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            });
        }

        // 添加本月的日期
        for (int day = 1; day <= lastDayOfMonth.Day; day++)
        {
            var date = new DateTime(CalendarDate.Year, CalendarDate.Month, day);
            var isToday = date.Date == DateTime.Today;
            var isSelected = date.Date == SelectedDate.Date;

            CalendarDays.Add(new CalendarDay
            {
                Day = day,
                IsCurrentMonth = true,
                IsToday = isToday,
                IsSelected = isSelected,
                ActualDate = date.Date,
                // 基础文字颜色，选中和今天状态在 XAML 中通过 Style 处理
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60))
            });
        }

        // 填充剩余的日期（下个月）
        var remainingDays = 42 - CalendarDays.Count; // 6 行 x 7 列 = 42
        for (int day = 1; day <= remainingDays; day++)
        {
            var date = lastDayOfMonth.AddDays(day);
            CalendarDays.Add(new CalendarDay
            {
                Day = date.Day,
                IsCurrentMonth = false,
                IsToday = date.Date == DateTime.Today,
                IsSelected = date.Date == SelectedDate.Date,
                ActualDate = date.Date,
                // 非当前月份的文字颜色在 XAML 中通过 Style 处理
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            });
        }
    }

    /// <summary>
    /// 上一月
    /// </summary>
    public void PreviousMonth()
    {
        CalendarDate = CalendarDate.AddMonths(-1);
        UpdateCalendar();
    }

    /// <summary>
    /// 下一月
    /// </summary>
    public void NextMonth()
    {
        CalendarDate = CalendarDate.AddMonths(1);
        UpdateCalendar();
    }

    partial void OnCalendarDateChanged(DateTime value)
    {
        UpdateCalendar();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateCalendar();
    }

    /// <summary>
    /// 选择日期命令
    /// </summary>
    [RelayCommand]
    private void SelectDay(CalendarDay? day)
    {
        if (day != null && day.ActualDate.HasValue)
        {
            SelectedDate = day.ActualDate.Value;
            // UpdateCalendar 会在 OnSelectedDateChanged 中自动调用
        }
    }

    /// <summary>
    /// 加载目的地统计数据
    /// </summary>
    private async Task LoadDestinationStatsAsync()
    {
        if (_prodSerialOutboundService == null) return;

        try
        {
            // 统计各目的地数量
            var usaTotal = 0m;
            var eurTotal = 0m;
            var chinaTotal = 0m;
            var japanTotal = 0m;
            var otherTotal = 0m;

            // 循环查询所有数据（处理大数据量情况）
            const int pageSize = 1000;
            int pageIndex = 1;
            bool hasMore = true;

            while (hasMore)
            {
                var query = new ProdSerialOutboundQueryDto
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                var result = await _prodSerialOutboundService.GetListAsync(query);

                if (!result.Success || result.Data == null || result.Data.Items == null || !result.Data.Items.Any())
                {
                    hasMore = false;
                    break;
                }

                // 统计当前页数据
                foreach (var item in result.Data.Items)
                {
                    var quantity = item.Quantity;
                    var destination = ClassifyDestination(item.DestCode, item.DestPort);

                    switch (destination)
                    {
                        case DestinationType.USA:
                            usaTotal += quantity;
                            break;
                        case DestinationType.EUR:
                            eurTotal += quantity;
                            break;
                        case DestinationType.CHINA:
                            chinaTotal += quantity;
                            break;
                        case DestinationType.JAPAN:
                            japanTotal += quantity;
                            break;
                        case DestinationType.OTHER:
                            otherTotal += quantity;
                            break;
                    }
                }

                // 检查是否还有更多数据
                var totalCount = result.Data.TotalNum;
                var currentCount = pageIndex * pageSize;
                hasMore = currentCount < totalCount;
                pageIndex++;
            }

            // 计算总数
            var grandTotal = usaTotal + eurTotal + chinaTotal + japanTotal + otherTotal;

            // 计算百分比（如果总数为0，则所有百分比为0）
            // 在 UI 线程上更新属性
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                if (grandTotal > 0)
                {
                    DestinationProgress1 = (double)(usaTotal / grandTotal * 100);
                    DestinationProgress2 = (double)(eurTotal / grandTotal * 100);
                    DestinationProgress3 = (double)(chinaTotal / grandTotal * 100);
                    DestinationProgress4 = (double)(japanTotal / grandTotal * 100);
                    DestinationProgress5 = (double)(otherTotal / grandTotal * 100);
                }
                else
                {
                    DestinationProgress1 = 0;
                    DestinationProgress2 = 0;
                    DestinationProgress3 = 0;
                    DestinationProgress4 = 0;
                    DestinationProgress5 = 0;
                }
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }
        catch (Exception ex)
        {
            // 记录错误，但不抛出异常，避免影响UI
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[Dashboard] 加载目的地统计数据失败");

            // 重置为0（在 UI 线程上）
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                DestinationProgress1 = 0;
                DestinationProgress2 = 0;
                DestinationProgress3 = 0;
                DestinationProgress4 = 0;
                DestinationProgress5 = 0;
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }
    }

    /// <summary>
    /// 目的地类型枚举
    /// </summary>
    private enum DestinationType
    {
        USA,
        EUR,
        CHINA,
        JAPAN,
        OTHER
    }

    /// <summary>
    /// 根据 DestCode 分类目的地
    /// 规则：
    /// - 包含 TCA -> USA
    /// - 包含 EUR -> EUR
    /// - 包含 T/C、EX、DM -> JAPAN
    /// - 包含 CHI -> CHINA
    /// - 其它 -> OTHER
    /// </summary>
    private static DestinationType ClassifyDestination(string? destCode, string? destPort)
    {
        // 只根据 DestCode 进行判断，统一转换为大写
        var code = destCode?.ToUpperInvariant() ?? string.Empty;

        // 如果 DestCode 为空，返回 OTHER
        if (string.IsNullOrWhiteSpace(code))
        {
            return DestinationType.OTHER;
        }

        // USA 判断：包含 TCA
        if (code.Contains("TCA"))
        {
            return DestinationType.USA;
        }

        // EUR 判断：包含 EUR
        if (code.Contains("EUR"))
        {
            return DestinationType.EUR;
        }

        // JAPAN 判断：包含 T/C、EX、DM
        if (code.Contains("T/C") || code.Contains("EX") || code.Contains("DM"))
        {
            return DestinationType.JAPAN;
        }

        // CHINA 判断：包含 CHI
        if (code.Contains("CHI"))
        {
            return DestinationType.CHINA;
        }

        // 其他
        return DestinationType.OTHER;
    }
}

