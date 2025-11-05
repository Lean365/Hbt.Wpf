//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ButtonStyleExtension.cs
// 创建者 : AI Assistant
// 创建时间: 2025-11-04
// 版本号 : 1.0
// 描述    : 按钮样式标记扩展，用于在 XAML 中根据按钮代码动态获取样式
//===================================================================

using System;
using System.Windows;
using System.Windows.Markup;

namespace Hbt.Fluent.Helpers;

/// <summary>
/// 按钮样式标记扩展
/// 用法：Style="{helpers:ButtonStyle ButtonCode=create}"
/// </summary>
public class ButtonStyleExtension : MarkupExtension
{
    /// <summary>
    /// 按钮代码（如 "create", "delete"）
    /// </summary>
    public string ButtonCode { get; set; } = string.Empty;

    public ButtonStyleExtension() { }

    public ButtonStyleExtension(string buttonCode)
    {
        ButtonCode = buttonCode;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var styleName = ButtonStyleHelper.GetStyleResourceKey(ButtonCode);
        
        // 从应用程序资源中获取样式
        if (Application.Current?.Resources.Contains(styleName) == true)
        {
            return Application.Current.Resources[styleName];
        }

        // 如果找不到样式，返回 null（使用默认样式）
        return null;
    }
}

