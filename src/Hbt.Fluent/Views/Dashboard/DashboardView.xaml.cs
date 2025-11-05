//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DashboardView.xaml.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 仪表盘视图代码后台
//===================================================================

using System.Windows.Controls;
using System.Windows.Threading;

namespace Hbt.Fluent.Views.Dashboard;

/// <summary>
/// 仪表盘视图
/// </summary>
public partial class DashboardView : UserControl
{
    private DispatcherTimer? _timer;

    public DashboardView()
    {
        InitializeComponent();
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
    }

    private void UpdateGreeting()
    {
        if (GreetingTextBlock != null)
        {
            var hour = System.DateTime.Now.Hour;
            string greeting;
            
            if (hour >= 5 && hour < 12)
            {
                greeting = "早上好！";
            }
            else if (hour >= 12 && hour < 14)
            {
                greeting = "中午好！";
            }
            else if (hour >= 14 && hour < 18)
            {
                greeting = "下午好！";
            }
            else if (hour >= 18 && hour < 22)
            {
                greeting = "晚上好！";
            }
            else
            {
                greeting = "夜深了，请注意休息！";
            }
            
            GreetingTextBlock.Text = greeting;
        }
    }
}

