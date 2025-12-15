// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：StatusToVisibilityConverter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：状态值到可见性转换器（根据状态值显示/隐藏控件）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 状态值到可见性转换器
/// 当状态值等于指定值时显示，否则隐藏
/// </summary>
public class StatusToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 目标状态值（当状态值等于此值时显示控件）
    /// </summary>
    public int StatusValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int targetStatus = StatusValue;
        
        // 如果通过 ConverterParameter 传递了值，优先使用它
        if (parameter != null)
        {
            if (int.TryParse(parameter.ToString(), out var paramStatus))
            {
                targetStatus = paramStatus;
            }
            else if (parameter is int intParam)
            {
                targetStatus = intParam;
            }
        }

        if (value is int status && status == targetStatus)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

