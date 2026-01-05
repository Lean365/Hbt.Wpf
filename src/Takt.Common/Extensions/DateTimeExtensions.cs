// ========================================
// 项目名称：Takt.Wpf
// 文件名称：DateTimeExtensions.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：日期时间扩展方法
// 
// 版权信息：
// Copyright (c) 2025 Takt All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

using Takt.Common.Constants;

namespace Takt.Common.Extensions;

/// <summary>
/// 日期时间扩展方法
/// 提供日期时间处理的便捷扩展方法
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 转换为标准格式字符串
    /// </summary>
    /// <param name="dateTime">日期时间</param>
    /// <returns>格式化后的字符串</returns>
    public static string ToStandardString(this DateTime dateTime)
    {
        return dateTime.ToString(AppConstants.DateFormat.Standard);
    }

    /// <summary>
    /// 转换为短日期格式字符串
    /// </summary>
    /// <param name="dateTime">日期时间</param>
    /// <returns>格式化后的字符串</returns>
    public static string ToShortDateString(this DateTime dateTime)
    {
        return dateTime.ToString(AppConstants.DateFormat.ShortDate);
    }

    /// <summary>
    /// 转换为短时间格式字符串
    /// </summary>
    /// <param name="dateTime">日期时间</param>
    /// <returns>格式化后的字符串</returns>
    public static string ToShortTimeString(this DateTime dateTime)
    {
        return dateTime.ToString(AppConstants.DateFormat.ShortTime);
    }

    /// <summary>
    /// 获取当天开始时间
    /// </summary>
    /// <param name="dateTime">日期时间</param>
    /// <returns>当天开始时间（00:00:00）</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// 获取当天结束时间
    /// </summary>
    /// <param name="dateTime">日期时间</param>
    /// <returns>当天结束时间（23:59:59）</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }
}
