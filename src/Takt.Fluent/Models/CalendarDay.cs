//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : CalendarDay.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 日历日期模型
//===================================================================

using System;
using System.Windows.Media;

namespace Takt.Fluent.Models;

/// <summary>
/// 日历日期模型
/// </summary>
public class CalendarDay
{
    public int Day { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool IsSelected { get; set; }
    public Brush Foreground { get; set; } = Brushes.Black;
    
    /// <summary>
    /// 对应的实际日期（用于点击选择）
    /// </summary>
    public DateTime? ActualDate { get; set; }
}

