// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：StateBasedVisibilityConverter.cs
// 创建时间：2025-12-01
// 创建人：Takt365(Cursor AI)
// 功能描述：基于状态的可见性转换器（支持多个允许状态和排除状态）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 基于状态的可见性转换器
/// 支持定义允许的状态值列表和排除的状态值列表
/// 用于实现通用的状态转换逻辑
/// </summary>
public class StateBasedVisibilityConverter : IMultiValueConverter
{
    private readonly HashSet<int> _allowedStates;
    private readonly HashSet<int> _excludedStates;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="allowedStates">允许的状态值（可以是单个值、数组或集合）</param>
    /// <param name="excludedStates">排除的状态值（可选）</param>
    public StateBasedVisibilityConverter(object? allowedStates = null!, int[]? excludedStates = null)
    {
        _allowedStates = new HashSet<int>();
        _excludedStates = excludedStates != null ? new HashSet<int>(excludedStates) : new HashSet<int>();

        // 解析允许的状态值
        if (allowedStates != null)
        {
            if (allowedStates is int singleValue)
            {
                _allowedStates.Add(singleValue);
            }
            else if (allowedStates is int[] array)
            {
                foreach (var value in array)
                {
                    _allowedStates.Add(value);
                }
            }
            else if (allowedStates is IEnumerable<int> enumerable)
            {
                foreach (var value in enumerable)
                {
                    _allowedStates.Add(value);
                }
            }
            else if (int.TryParse(allowedStates.ToString(), out var parsedValue))
            {
                _allowedStates.Add(parsedValue);
            }
        }
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length == 0)
        {
            return Visibility.Collapsed;
        }

        // 获取当前状态值（第一个绑定值）
        var currentState = GetStateValue(values[0]);
        if (currentState == null)
        {
            return Visibility.Collapsed;
        }

        var state = currentState.Value;

        // 如果定义了排除状态，检查是否在排除列表中（排除优先）
        if (_excludedStates.Count > 0)
        {
            if (_excludedStates.Contains(state))
            {
                return Visibility.Collapsed;
            }
        }

        // 如果定义了允许状态，检查是否在允许列表中
        if (_allowedStates.Count > 0)
        {
            if (!_allowedStates.Contains(state))
            {
                return Visibility.Collapsed;
            }
        }

        // 如果既没有定义允许状态，也没有定义排除状态，默认显示
        // 如果定义了允许状态且匹配，或者定义了排除状态且不在排除列表中，显示
        return Visibility.Visible;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private int? GetStateValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        if (int.TryParse(value.ToString(), out var parsedValue))
        {
            return parsedValue;
        }

        return null;
    }
}

