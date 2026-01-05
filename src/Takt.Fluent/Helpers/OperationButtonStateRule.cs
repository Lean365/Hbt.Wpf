// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：OperationButtonStateRule.cs
// 创建时间：2025-12-01
// 创建人：Takt365(Cursor AI)
// 功能描述：操作按钮状态规则配置（定义操作按钮与状态的关联关系）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 操作按钮状态规则配置
/// 用于定义操作按钮在哪些状态下可见/可用
/// </summary>
public class OperationButtonStateRule
{
    /// <summary>
    /// 操作按钮名称（如 "Run", "Stop", "Pause", "Resume", "Restart", "Start"）
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// 允许的状态值列表（按钮在这些状态下可见）
    /// </summary>
    public HashSet<int> AllowedStates { get; set; } = new();

    /// <summary>
    /// 排除的状态值列表（按钮在这些状态下隐藏）
    /// </summary>
    public HashSet<int> ExcludedStates { get; set; } = new();

    /// <summary>
    /// 状态属性名称（默认为 "Status"）
    /// </summary>
    public string StatusPropertyName { get; set; } = "Status";

    /// <summary>
    /// 检查指定状态是否允许显示按钮
    /// </summary>
    public bool IsAllowed(int status)
    {
        // 如果在排除列表中，不允许
        if (ExcludedStates.Count > 0 && ExcludedStates.Contains(status))
        {
            return false;
        }

        // 如果定义了允许列表，必须在允许列表中
        if (AllowedStates.Count > 0)
        {
            return AllowedStates.Contains(status);
        }

        // 如果没有定义允许列表，且不在排除列表中，则允许
        return true;
    }

    /// <summary>
    /// 创建规则构建器
    /// </summary>
    public static OperationButtonStateRuleBuilder For(string operationName)
    {
        return new OperationButtonStateRuleBuilder(operationName);
    }
}

/// <summary>
/// 操作按钮状态规则构建器
/// </summary>
public class OperationButtonStateRuleBuilder
{
    private readonly OperationButtonStateRule _rule;

    public OperationButtonStateRuleBuilder(string operationName)
    {
        _rule = new OperationButtonStateRule
        {
            OperationName = operationName
        };
    }

    /// <summary>
    /// 设置允许的状态
    /// </summary>
    public OperationButtonStateRuleBuilder AllowStates(params int[] states)
    {
        foreach (var state in states)
        {
            _rule.AllowedStates.Add(state);
        }
        return this;
    }

    /// <summary>
    /// 设置排除的状态
    /// </summary>
    public OperationButtonStateRuleBuilder ExcludeStates(params int[] states)
    {
        foreach (var state in states)
        {
            _rule.ExcludedStates.Add(state);
        }
        return this;
    }

    /// <summary>
    /// 设置状态属性名称
    /// </summary>
    public OperationButtonStateRuleBuilder StatusProperty(string propertyName)
    {
        _rule.StatusPropertyName = propertyName;
        return this;
    }

    /// <summary>
    /// 构建规则
    /// </summary>
    public OperationButtonStateRule Build()
    {
        return _rule;
    }
}

/// <summary>
/// 操作按钮状态规则集合
/// 用于统一管理所有操作按钮的状态转换规则
/// </summary>
public class OperationButtonStateRuleCollection : Dictionary<string, OperationButtonStateRule>
{
    /// <summary>
    /// 获取指定操作的规则
    /// </summary>
    public OperationButtonStateRule? GetRule(string operationName)
    {
        return TryGetValue(operationName, out var rule) ? rule : null;
    }

    /// <summary>
    /// 检查指定操作在指定状态下是否允许
    /// </summary>
    public bool IsOperationAllowed(string operationName, int status)
    {
        var rule = GetRule(operationName);
        return rule?.IsAllowed(status) ?? true; // 如果没有规则，默认允许
    }
}

