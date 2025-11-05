//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : SettingsView.xaml.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 设置页面视图
//===================================================================

using System.Windows.Controls;
using Hbt.Application.Dtos.Routine;
using Hbt.Fluent.ViewModels.Settings;

namespace Hbt.Fluent.Views.Settings;

public partial class SettingsView : UserControl
{
    public SettingsViewModel ViewModel { get; }

    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;
        
        Loaded += SettingsView_Loaded;
    }

    private async void SettingsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }

    private async void SettingValueTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.Tag is SettingDto setting)
        {
            // 更新设置值
            setting.SettingValue = textBox.Text;
            await ViewModel.SaveSettingAsync(setting);
        }
    }
}

