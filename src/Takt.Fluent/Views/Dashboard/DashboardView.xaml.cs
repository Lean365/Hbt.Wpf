//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : DashboardView.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 仪表盘视图代码后台
//===================================================================

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Takt.Fluent.ViewModels;

namespace Takt.Fluent.Views.Dashboard;

/// <summary>
/// 仪表盘视图
/// </summary>
public partial class DashboardView : UserControl
{
    private DispatcherTimer? _timer;
    private DashboardViewModel? _viewModel;

    public DashboardViewModel ViewModel
    {
        get => _viewModel ??= new DashboardViewModel();
        set => _viewModel = value;
    }

    public DashboardView()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += DashboardView_Loaded;
        Unloaded += DashboardView_Unloaded;
    }

    private void DashboardView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // 更新欢迎语
        UpdateGreeting();
        
        // 启动定时器，每秒更新一次欢迎语
        _timer = new DispatcherTimer
        {
            Interval = System.TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, args) =>
        {
            UpdateGreeting();
            ViewModel?.RefreshDashboardStats();
        };
        _timer.Start();
    }

    private void DashboardView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // 停止定时器
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
        
        // 清理 ViewModel
        _viewModel?.Dispose();
    }

    private void UpdateGreeting()
    {
        ViewModel?.UpdateGreeting();
        ViewModel?.RefreshDashboardStats();
    }


}

