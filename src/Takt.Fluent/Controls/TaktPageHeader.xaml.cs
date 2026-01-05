//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : TaktPageHeader.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 页面标题控件
//===================================================================

namespace Takt.Fluent.Controls;

/// <summary>
/// 页面标题控件
/// </summary>
public class TaktPageHeader : Control
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(TaktPageHeader),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(TaktPageHeader),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty ShowDescriptionProperty = DependencyProperty.Register(
        nameof(ShowDescription),
        typeof(bool),
        typeof(TaktPageHeader),
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

