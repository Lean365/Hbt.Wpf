//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : PageHeader.xaml.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 页面标题控件
//===================================================================

using System.Windows;
using System.Windows.Controls;

namespace Hbt.Fluent.Controls;

/// <summary>
/// 页面标题控件
/// </summary>
public class PageHeader : Control
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(PageHeader),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(PageHeader),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty ShowDescriptionProperty = DependencyProperty.Register(
        nameof(ShowDescription),
        typeof(bool),
        typeof(PageHeader),
        new PropertyMetadata(true)
    );

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool ShowDescription
    {
        get => (bool)GetValue(ShowDescriptionProperty);
        set => SetValue(ShowDescriptionProperty, value);
    }
}

