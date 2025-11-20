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
using System.Windows.Media;
using Takt.Fluent.Services;
using Takt.Fluent.ViewModels.Settings;

namespace Takt.Fluent.Views.Settings;

/// <summary>
/// 用户设置页面视图
/// 用于管理用户的个人设置（语言、主题等）
/// </summary>
public partial class MySettingsView : UserControl
{
    public MySettingsViewModel ViewModel { get; }
    private readonly ThemeService? _themeService;

    public MySettingsView(MySettingsViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel; // 直接绑定到 ViewModel，而不是 this

        // 订阅主题变化事件
        if (App.Services != null)
        {
            _themeService = App.Services.GetService<ThemeService>();
            if (_themeService != null)
            {
                _themeService.ThemeChanged += OnThemeChanged;
            }
        }

        Loaded += SettingsView_Loaded;
        Unloaded += MySettingsView_Unloaded;
    }

    /// <summary>
    /// 主题变化事件处理
    /// 强制刷新 UI 以确保主题正确应用
    /// </summary>
    private void OnThemeChanged(object? sender, System.Windows.ThemeMode appliedTheme)
    {
        try
        {
            // 使用 Invoke 而不是 BeginInvoke，确保立即执行
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
                    operLog?.Debug("[MySettingsView] 开始刷新 UI 以适配主题: {ThemeMode}", appliedTheme);

                    // 强制刷新所有资源
                    Resources.MergedDictionaries.Clear();
                    Resources.MergedDictionaries.Add(System.Windows.Application.Current.Resources.MergedDictionaries.FirstOrDefault() ?? new ResourceDictionary());

                    // 强制刷新视觉树
                    InvalidateVisual();
                    InvalidateArrange();
                    UpdateLayout();

                    // 刷新根 Grid 和 ScrollViewer 背景
                    if (Content is System.Windows.Controls.Grid rootGrid)
                    {
                        // 清除并重新设置背景
                        rootGrid.Background = null;
                        if (TryFindResource("ApplicationBackgroundBrush") is System.Windows.Media.Brush backgroundBrush)
                        {
                            rootGrid.Background = backgroundBrush;
                        }

                        // 查找 ScrollViewer 并刷新其背景
                        var scrollViewer = FindVisualChild<System.Windows.Controls.ScrollViewer>(rootGrid);
                        if (scrollViewer != null)
                        {
                            scrollViewer.Background = null;
                            if (TryFindResource("ApplicationBackgroundBrush") is System.Windows.Media.Brush scrollBackgroundBrush)
                            {
                                scrollViewer.Background = scrollBackgroundBrush;
                            }
                        }

                        // 递归刷新所有子元素的资源
                        RefreshChildResources(rootGrid);
                    }

                    // 再次强制刷新
                    InvalidateVisual();
                    InvalidateArrange();
                    UpdateLayout();

                    operLog?.Debug("[MySettingsView] UI 刷新完成");
                }
                catch (Exception ex)
                {
                    var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
                    operLog?.Error(ex, "[MySettingsView] 主题变化时刷新 UI 失败");
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[MySettingsView] 主题变化事件处理时发生异常");
        }
    }

    /// <summary>
    /// 递归刷新所有子元素的资源
    /// </summary>
    private void RefreshChildResources(DependencyObject parent)
    {
        if (parent == null) return;

        try
        {
            // 如果是 FrameworkElement，尝试刷新其背景和前景
            if (parent is FrameworkElement element)
            {
                // 刷新背景（Panel、Border 等有 Background 属性）
                if (element is Panel panel)
                {
                    var backgroundBinding = panel.GetBindingExpression(Panel.BackgroundProperty);
                    if (backgroundBinding != null)
                    {
                        backgroundBinding.UpdateTarget();
                    }
                }
                else if (element is Border border)
                {
                    var backgroundBinding = border.GetBindingExpression(Border.BackgroundProperty);
                    if (backgroundBinding != null)
                    {
                        backgroundBinding.UpdateTarget();
                    }
                }
                else if (element is Control control)
                {
                    var backgroundBinding = control.GetBindingExpression(Control.BackgroundProperty);
                    if (backgroundBinding != null)
                    {
                        backgroundBinding.UpdateTarget();
                    }
                }

                // 刷新前景
                if (element is Control control2)
                {
                    var foregroundBinding = control2.GetBindingExpression(Control.ForegroundProperty);
                    if (foregroundBinding != null)
                    {
                        foregroundBinding.UpdateTarget();
                    }
                }
            }

            // 递归处理子元素
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                RefreshChildResources(child);
            }
        }
        catch
        {
            // 忽略单个元素的刷新错误，继续处理其他元素
        }
    }

    /// <summary>
    /// 视图卸载时取消订阅事件并释放资源，防止内存泄漏
    /// </summary>
    private void MySettingsView_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // 取消订阅主题变化事件
            if (_themeService != null)
            {
                _themeService.ThemeChanged -= OnThemeChanged;
            }

            // 释放 ViewModel 资源（在 UI 线程中安全释放）
            if (ViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[MySettingsView] 卸载时释放资源失败");
        }
    }

    private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }

    /// <summary>
    /// 在视觉树中查找指定类型的子元素
    /// </summary>
    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                return t;
            }

            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }

        return null;
    }
}
