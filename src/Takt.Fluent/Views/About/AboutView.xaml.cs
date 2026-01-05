//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : AboutView.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-31
// 版本号 : 0.0.1
// 描述    : 关于视图代码后台
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Takt.Fluent.Views.About;

/// <summary>
/// 关于视图
/// </summary>
public partial class AboutView : UserControl
{
    private readonly ILocalizationManager? _localizationManager;
    private readonly ThemeService? _themeService;
    private List<string> _installedComponents = new();

    public AboutView()
    {
        InitializeComponent();
        _localizationManager = App.Services?.GetService<ILocalizationManager>();
        
        // 订阅主题变化事件
        if (App.Services != null)
        {
            _themeService = App.Services.GetService<ThemeService>();
            if (_themeService != null)
            {
                _themeService.ThemeChanged += OnThemeChanged;
            }
        }

        Loaded += AboutView_Loaded;
        Unloaded += AboutView_Unloaded;
    }

    /// <summary>
    /// 主题变化事件处理
    /// 强制刷新 UI 以确保主题正确应用
    /// </summary>
    private void OnThemeChanged(object? sender, System.Windows.ThemeMode appliedTheme)
    {
        try
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 强制刷新视觉树
                    InvalidateVisual();
                    InvalidateArrange();
                    UpdateLayout();

                    // 刷新根 Grid 和 ScrollViewer 背景
                    if (Content is Grid rootGrid)
                    {
                        if (TryFindResource("ApplicationBackgroundBrush") is Brush backgroundBrush)
                        {
                            rootGrid.Background = backgroundBrush;
                        }

                        // 查找 ScrollViewer 并刷新其背景
                        var scrollViewer = FindVisualChild<ScrollViewer>(rootGrid);
                        if (scrollViewer != null)
                        {
                            if (TryFindResource("ApplicationBackgroundBrush") is Brush scrollBackgroundBrush)
                            {
                                scrollViewer.Background = scrollBackgroundBrush;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var operLog = App.Services?.GetService<OperLogManager>();
                    operLog?.Error(ex, "[AboutView] 主题变化时刷新 UI 失败");
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[AboutView] 主题变化事件处理时发生异常");
        }
    }

    /// <summary>
    /// 视图卸载时取消订阅事件，防止内存泄漏
    /// </summary>
    private void AboutView_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_themeService != null)
            {
                _themeService.ThemeChanged -= OnThemeChanged;
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[AboutView] 取消订阅主题变化事件时发生异常");
        }
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

    private void AboutView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var fileInfo = new System.IO.FileInfo(assembly.Location);
        var productName = GetTranslation("Common.CompanyName", "节拍Takt");
        var edition = GetTranslation("about.edition", "社区版");
        var versionFormat = GetTranslation("about.version.format", "版本 {0}");
        var dotnetFormat = GetTranslation("about.dotnetVersion.format", ".NET {0}");

        const string DefaultVersion = "0.0.1";
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var versionValue = !string.IsNullOrWhiteSpace(informationalVersion) ? informationalVersion : DefaultVersion;
        var versionString = string.Format(versionFormat, versionValue);
        var dotnetVersion = RuntimeInformation.FrameworkDescription;
        var dotnetString = string.Format(dotnetFormat, dotnetVersion);
        var buildTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

        EditionTextBlock.Text = edition;
        VersionTextBlock.Text = versionString;
        DotNetVersionTextBlock.Text = dotnetString;

        ProductNameValueTextBlock.Text = productName;
        EditionValueTextBlock.Text = edition;
        VersionValueTextBlock.Text = versionValue;
        BuildDateTextBlock.Text = buildTime;
        CompanyValueTextBlock.Text = productName;
        InstallPathValueTextBlock.Text = AppContext.BaseDirectory;

        DotNetVersionValueTextBlock.Text = dotnetVersion;
        OsVersionValueTextBlock.Text = RuntimeInformation.OSDescription;
        ArchitectureValueTextBlock.Text = RuntimeInformation.ProcessArchitecture.ToString();
        ProcessorValueTextBlock.Text = Environment.ProcessorCount.ToString();
        RuntimeIdentifierValueTextBlock.Text = RuntimeInformation.RuntimeIdentifier;

        DotNetVersionValueTextBlock.ToolTip = DotNetVersionValueTextBlock.Text;
        OsVersionValueTextBlock.ToolTip = OsVersionValueTextBlock.Text;
        InstallPathValueTextBlock.ToolTip = InstallPathValueTextBlock.Text;
        RuntimeIdentifierValueTextBlock.ToolTip = RuntimeIdentifierValueTextBlock.Text;

        _installedComponents = BuildInstalledComponentsList();
        InstalledProductsList.ItemsSource = _installedComponents;
        DisclaimerTextBlock.Text = GetTranslation(
            "about.disclaimer",
            "警告: 本计算机程序受著作权法以及国际版权公约保护。未经授权而擅自复制或传播本程序（或其中任何部分），将受到严厉的民事及刑事处罚，并将在法律许可的最大限度内受到追诉。");
    }

    private List<string> BuildInstalledComponentsList()
    {
        var keys = new[]
        {
            "about.installed.item1",   // .NET 9.0 SDK
            "about.installed.item2",   // WPF
            "about.installed.item3",   // Prism
            "about.installed.item4",   // CommunityToolkit.Mvvm
            "about.installed.item5",   // MaterialDesignThemes
            "about.installed.item6",   // FontAwesome.Sharp
            "about.installed.item7",   // Autofac
            "about.installed.item8",   // SqlSugar
            "about.installed.item9",   // Serilog
            "about.installed.item10",  // Scriban
            "about.installed.item11",  // Newtonsoft.Json
            "about.installed.item12"   // LibVLCSharp.WPF
        };

        return keys
            .Select(key => GetTranslation(key, key))
            .ToList();
    }

    private string GetTranslation(string key, string fallback)
    {
        if (_localizationManager == null) return fallback;
        var translation = _localizationManager.GetString(key);
        return (translation == key) ? fallback : translation;
    }

    private void LicenseStatusHyperlink_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(GetTranslation("about.links.licenseStatus.message", "许可状态功能尚未实现。"), GetTranslation("about.dialog.title", "关于"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LicenseTermsHyperlink_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(GetTranslation("about.links.licenseTerms.message", "许可证条款查看功能尚未实现。"), GetTranslation("about.dialog.title", "关于"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SystemInfoButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new MySystemView
        {
            Owner = Window.GetWindow(this)
        };
        window.ShowDialog();
    }

}

