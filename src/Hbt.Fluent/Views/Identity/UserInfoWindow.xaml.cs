//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : UserInfoWindow.xaml.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 用户信息窗体
//===================================================================

using Hbt.Fluent.ViewModels.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Hbt.Fluent.Views.Identity;

/// <summary>
/// UserInfoWindow.xaml 的交互逻辑
/// </summary>
public partial class UserInfoWindow : Window
{
    public UserInfoViewModel ViewModel { get; }

    public UserInfoWindow(UserInfoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;
    }
}
