// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Controls
// 文件名称：ValidationRules.cs
// 创建时间：2025-01-27
// 创建人：Takt365(Cursor AI)
// 功能描述：验证规则类，用于 MaterialDesignThemes 原生验证样式
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Takt.Fluent.Controls;

/// <summary>
/// 基于 HasError 属性的验证规则，用于触发 MaterialDesignThemes 原生验证样式
/// 参照 MaterialDesign 官方示例，使用 Binding.ValidationRules 自动触发 Validation.HasError
/// 通过绑定表达式查找父控件，获取 HasError 和 HelperText 属性
/// </summary>
public class HasErrorValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    {
        // 这个方法不会被调用，因为我们重写了带 BindingExpressionBase 参数的版本
        return ValidationResult.ValidResult;
    }
    
    /// <summary>
    /// 验证规则验证方法（带绑定表达式参数）
    /// 这些自定义控件已被删除，此验证规则不再需要
    /// </summary>
    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo, BindingExpressionBase owner)
    {
        // 自定义控件已删除，此验证规则不再需要
        // 返回验证通过，因为原生控件使用 MaterialDesign 的内置验证机制
        return ValidationResult.ValidResult;
    }
}

/// <summary>
/// 非空验证规则
/// 用于验证值不能为空
/// 参照 MaterialDesign 官方示例实现
/// </summary>
public class NotEmptyValidationRule : ValidationRule
{
    public new bool ValidatesOnTargetUpdated { get; set; }

    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    {
        if (value == null)
        {
            return new ValidationResult(false, "值不能为空");
        }

        // 处理字符串类型
        if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
        {
            return new ValidationResult(false, "值不能为空");
        }

        // 处理 SelectOptionModel 类型（ComboBox 绑定对象时）
        if (value is Takt.Common.Models.SelectOptionModel optionModel)
        {
            if (string.IsNullOrWhiteSpace(optionModel.DataValue))
            {
                return new ValidationResult(false, "值不能为空");
            }
        }

        return ValidationResult.ValidResult;
    }
}

/// <summary>
/// FrameworkElement 扩展方法，用于查找父控件
/// </summary>
internal static class FrameworkElementExtensions
{
    public static T? FindAncestor<T>(this DependencyObject element) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null)
        {
            if (parent is T ancestor)
            {
                return ancestor;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}

