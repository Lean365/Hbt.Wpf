//===================================================================
// 项目名 : Takt.Fluent
// 文件名 : MySettingsView.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 2.0
// 描述    : 用户自定义设置页面视图
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用

using System.Windows;
using System.Windows.Controls;
using Takt.Fluent.ViewModels.Settings;

namespace Takt.Fluent.Views.Settings;

/// <summary>
/// 用户设置页面视图
/// 用于管理用户的个人设置（语言、主题等）
/// </summary>
public partial class MySettingsView : UserControl
{
    public MySettingsViewModel ViewModel { get; }

    public MySettingsView(MySettingsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;

        Loaded += SettingsView_Loaded;
    }

    private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}
