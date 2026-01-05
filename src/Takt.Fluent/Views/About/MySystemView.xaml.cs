// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.About
// 文件名称：MySystemView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：系统信息窗口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用

using System.Windows;
using System.Windows.Media;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Services;

namespace Takt.Fluent.Views.About;

/// <summary>
/// 系统信息窗口
/// </summary>
public partial class MySystemView : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private readonly ThemeService? _themeService;

    public MySystemView()
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

        Loaded += MySystemView_Loaded;
        Closed += MySystemView_Closed;
        LoadSystemInfo();
    }

    /// <summary>
    /// 窗口加载完成事件处理
    /// </summary>
    private void MySystemView_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化时刷新背景
        if (TryFindResource("ApplicationBackgroundBrush") is Brush backgroundBrush)
        {
            Background = backgroundBrush;
        }
    }

    /// <summary>
    /// 窗口关闭时取消订阅事件，防止内存泄漏
    /// </summary>
    private void MySystemView_Closed(object? sender, EventArgs e)
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
            operLog?.Error(ex, "[MySystemView] 取消订阅主题变化事件时发生异常");
        }
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

                    // 刷新窗口背景
                    if (TryFindResource("ApplicationBackgroundBrush") is Brush backgroundBrush)
                    {
                        Background = backgroundBrush;
                    }
                }
                catch (Exception ex)
                {
                    var operLog = App.Services?.GetService<OperLogManager>();
                    operLog?.Error(ex, "[MySystemView] 主题变化时刷新 UI 失败");
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[MySystemView] 主题变化事件处理时发生异常");
        }
    }

    private void LoadSystemInfo()
    {
        LoadSummaryInfo();
        LoadHardwareInfo();
        LoadSoftwareInfo();
        LoadNetworkInfo();
    }

    /// <summary>
    /// 加载系统摘要信息
    /// </summary>
    private void LoadSummaryInfo()
    {
        var items = new List<SystemInfoItem>();

        var systemInfo = SystemInfoHelper.GetSystemInfo();
        // 系统摘要：操作系统、操作系统制造商、系统版本、系统类型、系统架构、机器名称、用户名、是否管理员、运行时、进程架构、系统运行时间
        var summaryKeys = new[] { "操作系统", "系统版本", "系统类型", "系统架构", "机器名称", "用户名", "是否管理员",
            "运行时", "进程架构", "系统运行时间" };

        foreach (var kvp in systemInfo.Where(k => summaryKeys.Contains(k.Key)))
        {
            items.Add(new SystemInfoItem
            {
                Key = GetLocalizedKey(kvp.Key),
                Value = kvp.Value
            });
        }

        // 添加操作系统制造商
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.summary.osManufacturer", "操作系统制造商"),
            Value = SystemInfoHelper.GetOsManufacturer()
        });

        // 添加系统制造商
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.summary.systemManufacturer", "系统制造商"),
            Value = SystemInfoHelper.GetSystemManufacturer()
        });

        // 添加系统型号
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.summary.systemModel", "系统型号"),
            Value = SystemInfoHelper.GetSystemModel()
        });

        // 添加系统类型（详细）
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.summary.systemTypeDetail", "系统类型（详细）"),
            Value = SystemInfoHelper.GetSystemType()
        });

        // 添加系统语言
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.summary.systemLanguage", "系统语言"),
            Value = $"{SystemInfoHelper.GetSystemLanguageName()} ({SystemInfoHelper.GetSystemLanguageCode()})"
        });

        // 添加区域信息
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.summary.region", "区域"),
            Value = SystemInfoHelper.GetRegion()
        });

        // 添加时区信息
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.summary.timeZone", "时区"),
            Value = SystemInfoHelper.GetTimeZone()
        });

        SummaryInfoDataGrid.ItemsSource = items;
    }

    /// <summary>
    /// 加载硬件信息
    /// </summary>
    private void LoadHardwareInfo()
    {
        var items = new List<SystemInfoItem>();

        var systemInfo = SystemInfoHelper.GetSystemInfo();
        // 硬件信息：CPU、CPU名称、CPU核心、物理内存、可用内存、内存使用率、进程内存
        var hardwareKeys = new[] { "CPU", "CPU名称", "CPU核心", "物理内存", "可用内存", "内存使用率", "进程内存" };

        foreach (var kvp in systemInfo.Where(k => hardwareKeys.Contains(k.Key)))
        {
            items.Add(new SystemInfoItem
            {
                Key = GetLocalizedKey(kvp.Key),
                Value = kvp.Value
            });
        }

        // 添加处理器名称（完整）
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.processorName", "处理器名称"),
            Value = SystemInfoHelper.GetProcessorName()
        });

        // 添加平台角色
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.platformRole", "平台角色"),
            Value = SystemInfoHelper.GetPlatformRole()
        });

        // 添加BIOS版本
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.biosVersion", "BIOS版本"),
            Value = SystemInfoHelper.GetBiosVersion()
        });

        // 添加BIOS发布日期
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.biosReleaseDate", "BIOS发布日期"),
            Value = SystemInfoHelper.GetBiosReleaseDate()
        });

        // 添加BIOS模式
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.biosMode", "BIOS模式"),
            Value = SystemInfoHelper.GetBiosMode()
        });

        // 添加主板制造商
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.motherboardManufacturer", "主板制造商"),
            Value = SystemInfoHelper.GetMotherboardManufacturer()
        });

        // 添加主板产品型号
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.motherboardProduct", "主板产品型号"),
            Value = SystemInfoHelper.GetMotherboardProduct()
        });

        // 添加主板版本
        items.Add(new SystemInfoItem
        {
            Key = GetTranslation("about.systemInfo.hardware.motherboardVersion", "主板版本"),
            Value = SystemInfoHelper.GetMotherboardVersion()
        });

        // 添加磁盘信息
        var drives = SystemInfoHelper.GetDriveInfo();
        if (drives.Any())
        {
            items.Add(new SystemInfoItem
            {
                Key = GetTranslation("about.systemInfo.hardware.diskInfo", "磁盘信息"),
                Value = string.Join("; ", drives)
            });
        }

        HardwareInfoDataGrid.ItemsSource = items;
    }

    /// <summary>
    /// 加载软件信息
    /// </summary>
    private void LoadSoftwareInfo()
    {
        var softwareList = SystemInfoHelper.GetInstalledSoftware();
        var items = softwareList.Select(software => new SoftwareInfoItem
        {
            Name = software.Name,
            Version = software.Version ?? "-",
            Publisher = software.Publisher ?? "-"
        }).ToList();

        SoftwareInfoDataGrid.ItemsSource = items;
    }

    /// <summary>
    /// 加载网络信息
    /// </summary>
    private void LoadNetworkInfo()
    {
        var networkAdapters = SystemInfoHelper.GetNetworkAdapters();
        var items = networkAdapters.Select(adapter => new NetworkAdapterItem
        {
            Name = adapter.Name,
            MacAddress = adapter.MacAddress ?? "-",
            IpAddresses = adapter.IpAddresses.Any() ? string.Join(", ", adapter.IpAddresses) : "-",
            Speed = adapter.Speed,
            Status = adapter.IsActive ? GetTranslation("about.systemInfo.network.active", "活动") : GetTranslation("about.systemInfo.network.inactive", "非活动")
        }).ToList();

        NetworkAdaptersDataGrid.ItemsSource = items;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var text = new StringBuilder();
        var title = GetTranslation("about.systemInfo.copy.title", "========== 系统信息 ==========");
        text.AppendLine(title);
        text.AppendLine();

        // 系统摘要
        var summaryTitle = GetTranslation("about.systemInfo.copy.section.summary", "【系统摘要】");
        text.AppendLine(summaryTitle);
        var systemInfo = SystemInfoHelper.GetSystemInfo();
        var summaryKeys = new[] { "操作系统", "系统版本", "系统类型", "系统架构", "机器名称", "用户名", "是否管理员",
            "运行时", "进程架构", "系统运行时间" };
        foreach (var kvp in systemInfo.Where(k => summaryKeys.Contains(k.Key)))
        {
            text.AppendLine($"{GetLocalizedKey(kvp.Key)}: {kvp.Value}");
        }
        var osManufacturerLabel = GetTranslation("about.systemInfo.summary.osManufacturer", "操作系统制造商");
        text.AppendLine($"{osManufacturerLabel}: {SystemInfoHelper.GetOsManufacturer()}");
        var systemManufacturerLabel = GetTranslation("about.systemInfo.summary.systemManufacturer", "系统制造商");
        text.AppendLine($"{systemManufacturerLabel}: {SystemInfoHelper.GetSystemManufacturer()}");
        var systemModelLabel = GetTranslation("about.systemInfo.summary.systemModel", "系统型号");
        text.AppendLine($"{systemModelLabel}: {SystemInfoHelper.GetSystemModel()}");
        var systemTypeDetailLabel = GetTranslation("about.systemInfo.summary.systemTypeDetail", "系统类型（详细）");
        text.AppendLine($"{systemTypeDetailLabel}: {SystemInfoHelper.GetSystemType()}");
        var systemLanguageLabel = GetTranslation("about.systemInfo.summary.systemLanguage", "系统语言");
        text.AppendLine($"{systemLanguageLabel}: {SystemInfoHelper.GetSystemLanguageName()} ({SystemInfoHelper.GetSystemLanguageCode()})");
        var regionLabel = GetTranslation("about.systemInfo.summary.region", "区域");
        text.AppendLine($"{regionLabel}: {SystemInfoHelper.GetRegion()}");
        var timeZoneLabel = GetTranslation("about.systemInfo.summary.timeZone", "时区");
        text.AppendLine($"{timeZoneLabel}: {SystemInfoHelper.GetTimeZone()}");
        text.AppendLine();

        // 硬件信息
        var hardwareTitle = GetTranslation("about.systemInfo.copy.section.hardware", "【硬件信息】");
        text.AppendLine(hardwareTitle);
        var hardwareKeys = new[] { "CPU", "CPU名称", "CPU核心", "物理内存", "可用内存", "内存使用率", "进程内存" };
        foreach (var kvp in systemInfo.Where(k => hardwareKeys.Contains(k.Key)))
        {
            text.AppendLine($"{GetLocalizedKey(kvp.Key)}: {kvp.Value}");
        }
        var processorNameLabel = GetTranslation("about.systemInfo.hardware.processorName", "处理器名称");
        text.AppendLine($"{processorNameLabel}: {SystemInfoHelper.GetProcessorName()}");
        var platformRoleLabel = GetTranslation("about.systemInfo.hardware.platformRole", "平台角色");
        text.AppendLine($"{platformRoleLabel}: {SystemInfoHelper.GetPlatformRole()}");
        var biosVersionLabel = GetTranslation("about.systemInfo.hardware.biosVersion", "BIOS版本");
        text.AppendLine($"{biosVersionLabel}: {SystemInfoHelper.GetBiosVersion()}");
        var biosReleaseDateLabel = GetTranslation("about.systemInfo.hardware.biosReleaseDate", "BIOS发布日期");
        text.AppendLine($"{biosReleaseDateLabel}: {SystemInfoHelper.GetBiosReleaseDate()}");
        var biosModeLabel = GetTranslation("about.systemInfo.hardware.biosMode", "BIOS模式");
        text.AppendLine($"{biosModeLabel}: {SystemInfoHelper.GetBiosMode()}");
        var motherboardManufacturerLabel = GetTranslation("about.systemInfo.hardware.motherboardManufacturer", "主板制造商");
        text.AppendLine($"{motherboardManufacturerLabel}: {SystemInfoHelper.GetMotherboardManufacturer()}");
        var motherboardProductLabel = GetTranslation("about.systemInfo.hardware.motherboardProduct", "主板产品型号");
        text.AppendLine($"{motherboardProductLabel}: {SystemInfoHelper.GetMotherboardProduct()}");
        var motherboardVersionLabel = GetTranslation("about.systemInfo.hardware.motherboardVersion", "主板版本");
        text.AppendLine($"{motherboardVersionLabel}: {SystemInfoHelper.GetMotherboardVersion()}");
        var drives = SystemInfoHelper.GetDriveInfo();
        if (drives.Any())
        {
            var diskInfoLabel = GetTranslation("about.systemInfo.hardware.diskInfo", "磁盘信息");
            text.AppendLine($"{diskInfoLabel}: {string.Join("; ", drives)}");
        }
        text.AppendLine();

        // 软件信息
        var softwareTitle = GetTranslation("about.systemInfo.copy.section.software", "【软件信息】");
        text.AppendLine(softwareTitle);
        var softwareList = SystemInfoHelper.GetInstalledSoftware();
        var versionLabel = GetTranslation("about.systemInfo.software.version", "版本");
        var publisherLabel = GetTranslation("about.systemInfo.software.publisher", "发布者");
        foreach (var software in softwareList)
        {
            text.AppendLine($"  - {software.Name}");
            if (!string.IsNullOrEmpty(software.Version))
            {
                text.AppendLine($"    {versionLabel}: {software.Version}");
            }
            if (!string.IsNullOrEmpty(software.Publisher))
            {
                text.AppendLine($"    {publisherLabel}: {software.Publisher}");
            }
        }
        text.AppendLine();

        // 网络信息
        var networkTitle = GetTranslation("about.systemInfo.copy.section.network", "【网络信息】");
        text.AppendLine(networkTitle);
        var ipLabel = GetTranslation("about.systemInfo.network.ip", "IP地址");
        var macLabel = GetTranslation("about.systemInfo.network.mac", "MAC地址");
        var adaptersLabel = GetTranslation("about.systemInfo.network.adapters", "网络适配器");
        text.AppendLine($"{ipLabel}: {SystemInfoHelper.GetLocalIpAddress()}");
        text.AppendLine($"{macLabel}: {SystemInfoHelper.GetMacAddress()}");
        text.AppendLine($"{adaptersLabel}:");
        var networkAdapters = SystemInfoHelper.GetNetworkAdapters();
        foreach (var adapter in networkAdapters)
        {
            text.AppendLine($"  - {adapter.Name} ({adapter.Speed})");
            if (!string.IsNullOrEmpty(adapter.MacAddress))
            {
                text.AppendLine($"    MAC: {adapter.MacAddress}");
            }
            if (adapter.IpAddresses.Any())
            {
                text.AppendLine($"    IP: {string.Join(", ", adapter.IpAddresses)}");
            }
        }

        try
        {
            Clipboard.SetText(text.ToString());
            var message = GetTranslation("about.systemInfo.copySuccess", "系统信息已复制到剪贴板");
            MessageBox.Show(message, GetTranslation("about.dialog.title", "关于"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch
        {
            var message = GetTranslation("about.systemInfo.copyFailed", "复制失败");
            MessageBox.Show(message, GetTranslation("about.dialog.title", "关于"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 获取本地化的键名
    /// </summary>
    private string GetLocalizedKey(string key)
    {
        var keyMap = new Dictionary<string, string>
        {
            { "操作系统", "about.systemInfo.key.os" },
            { "系统版本", "about.systemInfo.key.osVersion" },
            { "系统类型", "about.systemInfo.key.osType" },
            { "系统架构", "about.systemInfo.key.osArchitecture" },
            { "机器名称", "about.systemInfo.key.machineName" },
            { "用户名", "about.systemInfo.key.userName" },
            { "是否管理员", "about.systemInfo.key.isAdmin" },
            { "运行时", "about.systemInfo.key.runtime" },
            { "进程架构", "about.systemInfo.key.processArchitecture" },
            { "系统运行时间", "about.systemInfo.key.systemUptime" },
            { "CPU", "about.systemInfo.key.cpu" },
            { "CPU名称", "about.systemInfo.key.cpuName" },
            { "CPU核心", "about.systemInfo.key.cpuCores" },
            { "物理内存", "about.systemInfo.key.totalMemory" },
            { "可用内存", "about.systemInfo.key.availableMemory" },
            { "内存使用率", "about.systemInfo.key.memoryUsage" },
            { "进程内存", "about.systemInfo.key.processMemory" },
            { "IP地址", "about.systemInfo.key.ipAddress" },
            { "MAC地址", "about.systemInfo.key.macAddress" }
        };

        if (keyMap.TryGetValue(key, out var translationKey))
        {
            return GetTranslation(translationKey, key);
        }

        return key;
    }

    private string GetTranslation(string key, string fallback)
    {
        if (_localizationManager == null) return fallback;
        var translation = _localizationManager.GetString(key);
        return (translation == key) ? fallback : translation;
    }

    private class SystemInfoItem
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private class NetworkAdapterItem
    {
        public string Name { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string IpAddresses { get; set; } = string.Empty;
        public string Speed { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    private class SoftwareInfoItem
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
    }
}
