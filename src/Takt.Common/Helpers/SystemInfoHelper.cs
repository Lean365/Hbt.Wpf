// ========================================
// 项目名称：Takt.Wpf
// 文件名称：SystemInfoHelper.cs
// 创建时间：2025-10-21
// 创建人：Hbt365(Cursor AI)
// 功能描述：系统信息获取帮助类（跨平台，基于Hardware.Info）
// ========================================

using Hardware.Info;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Takt.Common.Helpers;

/// <summary>
/// 系统信息获取帮助类
/// </summary>
/// <remarks>
/// 基于 Hardware.Info 库实现跨平台硬件信息获取
/// 支持 Windows、Linux、macOS
/// </remarks>
public static class SystemInfoHelper
{
    private static readonly Lazy<IHardwareInfo> _hardwareInfo = new(() =>
    {
        var info = new HardwareInfo();
        try
        {
            // 刷新硬件信息（可能比较慢，所以使用懒加载）
            info.RefreshAll();
        }
        catch
        {
            // 忽略刷新错误
        }
        return info;
    });

    private static IHardwareInfo HardwareInfo => _hardwareInfo.Value;

    #region 网络信息

    /// <summary>
    /// 网络适配器信息
    /// </summary>
    public class NetworkAdapterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public List<string> IpAddresses { get; set; } = new();
        public bool IsActive { get; set; }
        public string Speed { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 获取本机IP地址
    /// </summary>
    /// <returns>本机IP地址，获取失败返回 "127.0.0.1"</returns>
    public static string GetLocalIpAddress()
    {
        try
        {
            // 优先从Hardware.Info获取
            var networkAdapter = HardwareInfo.NetworkAdapterList
                .FirstOrDefault(na => na.IPAddressList?.Any() == true);

            if (networkAdapter?.IPAddressList != null && networkAdapter.IPAddressList.Count > 0)
            {
                foreach (var ip in networkAdapter.IPAddressList)
                {
                    var ipStr = ip?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(ipStr) && !ipStr.Contains(':') && ipStr != "127.0.0.1")
                        return ipStr;
                }
            }

            // 备用方案：通过NetworkInterface获取
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .OrderByDescending(ni => ni.Speed);

            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties();
                var ipAddress = ipProperties.UnicastAddresses
                    .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                                         !IPAddress.IsLoopback(ip.Address))?.Address;

                if (ipAddress != null)
                    return ipAddress.ToString();
            }

            return "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// 获取本机MAC地址
    /// </summary>
    /// <returns>本机MAC地址，获取失败返回空字符串</returns>
    public static string GetMacAddress()
    {
        try
        {
            // 从Hardware.Info获取
            var networkAdapter = HardwareInfo.NetworkAdapterList
                .FirstOrDefault(na => !string.IsNullOrEmpty(na.MACAddress));

            return networkAdapter?.MACAddress ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取所有网络适配器列表（区分活动和非活动）
    /// </summary>
    /// <returns>网络适配器信息列表</returns>
    public static List<NetworkAdapterInfo> GetNetworkAdapters()
    {
        try
        {
            return HardwareInfo.NetworkAdapterList
                .Select(na => new NetworkAdapterInfo
                {
                    Name = na.Name ?? "Unknown",
                    MacAddress = na.MACAddress ?? "",
                    IpAddresses = na.IPAddressList?.Select(ip => ip.ToString()).ToList() ?? new List<string>(),
                    IsActive = na.IPAddressList?.Any() == true, // 有IP地址表示活动
                    Speed = na.Speed > 0 ? $"{na.Speed / 1_000_000} Mbps" : "N/A",
                    Description = na.Description ?? ""
                })
                .OrderByDescending(na => na.IsActive) // 活动的排在前面
                .ToList();
        }
        catch
        {
            return new List<NetworkAdapterInfo>();
        }
    }

    /// <summary>
    /// 获取活动的网络适配器列表
    /// </summary>
    /// <returns>活动的网络适配器信息列表</returns>
    public static List<NetworkAdapterInfo> GetActiveNetworkAdapters()
    {
        return GetNetworkAdapters().Where(na => na.IsActive).ToList();
    }

    #endregion

    #region 操作系统信息

    /// <summary>
    /// 获取操作系统信息
    /// </summary>
    /// <returns>操作系统描述</returns>
    public static string GetOsDescription()
    {
        try
        {
            return HardwareInfo.OperatingSystem.Name ?? RuntimeInformation.OSDescription;
        }
        catch
        {
            return "Unknown OS";
        }
    }

    /// <summary>
    /// 获取操作系统版本
    /// </summary>
    /// <returns>操作系统版本</returns>
    public static string GetOsVersion()
    {
        try
        {
            return HardwareInfo.OperatingSystem.VersionString ?? Environment.OSVersion.VersionString;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取操作系统类型
    /// </summary>
    /// <returns>操作系统类型（Windows、Linux、macOS、Other）</returns>
    public static string GetOsType()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS";
        return "Other";
    }

    /// <summary>
    /// 获取操作系统架构
    /// </summary>
    /// <returns>操作系统架构（X64、X86、Arm64、Arm）</returns>
    public static string GetOsArchitecture()
    {
        return RuntimeInformation.OSArchitecture.ToString();
    }

    /// <summary>
    /// 获取系统语言代码
    /// </summary>
    /// <returns>系统语言代码（如：zh-CN、en-US、ja-JP）</returns>
    public static string GetSystemLanguageCode()
    {
        try
        {
            // 从当前线程的文化信息获取语言代码
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            return culture.Name; // 返回完整语言代码，如 zh-CN
        }
        catch
        {
            return "en-US"; // 默认返回英文
        }
    }

    /// <summary>
    /// 获取系统语言名称
    /// </summary>
    /// <returns>系统语言名称</returns>
    public static string GetSystemLanguageName()
    {
        try
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            return culture.DisplayName;
        }
        catch
        {
            return "English";
        }
    }

    #endregion

    #region CPU信息

    /// <summary>
    /// 获取CPU信息
    /// </summary>
    /// <returns>CPU信息字符串</returns>
    public static string GetCpuInfo()
    {
        try
        {
            var cpu = HardwareInfo.CpuList.FirstOrDefault();
            return cpu != null
                ? $"{cpu.Name} - {cpu.NumberOfCores} Cores / {cpu.NumberOfLogicalProcessors} Threads"
                : $"{Environment.ProcessorCount} Processors";
        }
        catch
        {
            return $"{Environment.ProcessorCount} Processors";
        }
    }

    /// <summary>
    /// 获取CPU名称
    /// </summary>
    /// <returns>CPU名称</returns>
    public static string GetCpuName()
    {
        try
        {
            return HardwareInfo.CpuList.FirstOrDefault()?.Name ?? "Unknown CPU";
        }
        catch
        {
            return "Unknown CPU";
        }
    }

    /// <summary>
    /// 获取处理器核心数
    /// </summary>
    /// <returns>处理器核心数</returns>
    public static int GetCpuCores()
    {
        try
        {
            return (int)(HardwareInfo.CpuList.FirstOrDefault()?.NumberOfCores ?? (uint)Environment.ProcessorCount);
        }
        catch
        {
            return Environment.ProcessorCount;
        }
    }

    #endregion

    #region 内存信息

    /// <summary>
    /// 获取物理内存总量（GB）
    /// </summary>
    /// <returns>物理内存总量（GB）</returns>
    public static double GetTotalMemoryGb()
    {
        try
        {
            var totalMemory = HardwareInfo.MemoryStatus.TotalPhysical;
            return totalMemory / 1024.0 / 1024.0 / 1024.0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 获取可用内存（GB）
    /// </summary>
    /// <returns>可用内存（GB）</returns>
    public static double GetAvailableMemoryGb()
    {
        try
        {
            var availableMemory = HardwareInfo.MemoryStatus.AvailablePhysical;
            return availableMemory / 1024.0 / 1024.0 / 1024.0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 获取内存使用率（%）
    /// </summary>
    /// <returns>内存使用率（0-100）</returns>
    public static double GetMemoryUsagePercent()
    {
        try
        {
            var total = HardwareInfo.MemoryStatus.TotalPhysical;
            var available = HardwareInfo.MemoryStatus.AvailablePhysical;
            if (total == 0) return 0;
            return (total - available) * 100.0 / total;
        }
        catch
        {
            return 0;
        }
    }

    #endregion

    #region 磁盘信息

    /// <summary>
    /// 获取磁盘信息列表
    /// </summary>
    /// <returns>磁盘信息列表</returns>
    public static List<string> GetDriveInfo()
    {
        try
        {
            return HardwareInfo.DriveList
                .Select(d => $"{d.Name} - {d.Size / 1024.0 / 1024.0 / 1024.0:F2} GB")
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    #endregion

    #region 系统基本信息

    /// <summary>
    /// 获取机器名称
    /// </summary>
    /// <returns>机器名称</returns>
    public static string GetMachineName()
    {
        try
        {
            return Environment.MachineName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取用户名
    /// </summary>
    /// <returns>当前系统用户名</returns>
    public static string GetUserName()
    {
        try
        {
            return Environment.UserName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取.NET运行时版本
    /// </summary>
    /// <returns>.NET运行时版本</returns>
    public static string GetFrameworkVersion()
    {
        try
        {
            return RuntimeInformation.FrameworkDescription;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取进程架构
    /// </summary>
    /// <returns>进程架构（X64、X86、Arm64、Arm）</returns>
    public static string GetProcessArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture.ToString();
    }

    /// <summary>
    /// 获取系统启动时间
    /// </summary>
    /// <returns>系统启动以来的时间</returns>
    public static TimeSpan GetSystemUptime()
    {
        return TimeSpan.FromMilliseconds(Environment.TickCount64);
    }

    /// <summary>
    /// 获取工作集内存（MB）
    /// </summary>
    /// <returns>工作集内存大小（MB）</returns>
    public static double GetWorkingSetMemoryMb()
    {
        return Environment.WorkingSet / 1024.0 / 1024.0;
    }

    /// <summary>
    /// 获取客户端类型
    /// </summary>
    /// <returns>客户端类型（Desktop、Web、Mobile、API等）</returns>
    public static string GetClientType()
    {
        try
        {
            // 根据运行时环境判断客户端类型
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // 检查是否为 WPF 应用
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    var assemblyName = assembly.GetName().Name ?? "";
                    // 如果程序集名称包含 Fluent，通常是 WPF 桌面应用
                    if (assemblyName.Contains("Fluent", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Desktop";
                    }
                }
                return "Desktop"; // 默认桌面应用
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取客户端名称
    /// </summary>
    /// <returns>客户端名称（如：WPF Desktop App）</returns>
    public static string GetClientName()
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var productAttribute = assembly.GetCustomAttribute<AssemblyProductAttribute>();
                if (productAttribute != null && !string.IsNullOrWhiteSpace(productAttribute.Product))
                {
                    var type = GetClientType();
                    return $"{productAttribute.Product} ({type})";
                }
            }
            
            // 备用方案：根据运行时环境判断
            var clientType = GetClientType();
            if (clientType == "Desktop")
            {
                return "WPF Desktop App";
            }
            
            return "Unknown Client";
        }
        catch
        {
            return "Unknown Client";
        }
    }

    /// <summary>
    /// 获取客户端版本
    /// </summary>
    /// <returns>客户端版本</returns>
    public static string GetClientVersion()
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var version = assembly.GetName().Version;
                if (version != null)
                {
                    return version.ToString();
                }
                
                // 尝试获取信息版本
                var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (informationalVersion != null && !string.IsNullOrWhiteSpace(informationalVersion.InformationalVersion))
                {
                    return informationalVersion.InformationalVersion;
                }
            }
            
            return "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }

    /// <summary>
    /// 获取操作系统制造商
    /// </summary>
    /// <returns>操作系统制造商</returns>
    public static string GetOsManufacturer()
    {
        try
        {
            // 备用方案（Hardware.Info 的 OperatingSystem 可能不包含 Manufacturer）
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var manufacturer = key?.GetValue("RegisteredOrganization")?.ToString();
                if (!string.IsNullOrEmpty(manufacturer))
                    return manufacturer;
                return "Microsoft Corporation";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "Linux Foundation";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "Apple Inc.";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取系统制造商
    /// </summary>
    /// <returns>系统制造商</returns>
    public static string GetSystemManufacturer()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var computerSystem = HardwareInfo.ComputerSystemList.FirstOrDefault();
            if (computerSystem != null)
            {
                var manufacturerProp = computerSystem.GetType().GetProperty("Manufacturer");
                if (manufacturerProp != null)
                {
                    var value = manufacturerProp.GetValue(computerSystem)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            // 备用方案
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                return key?.GetValue("SystemManufacturer")?.ToString() ?? "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var manufacturer = ReadLinuxDmiFile("sys_vendor");
                return !string.IsNullOrEmpty(manufacturer) ? manufacturer : "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (ExecuteCommand("system_profiler", "SPHardwareDataType", out var output))
                {
                    var match = Regex.Match(output, @"Model Name:\s*(.+)");
                    if (match.Success)
                        return "Apple Inc.";
                }
                return "Apple Inc.";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取系统型号
    /// </summary>
    /// <returns>系统型号</returns>
    public static string GetSystemModel()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var computerSystem = HardwareInfo.ComputerSystemList.FirstOrDefault();
            if (computerSystem != null)
            {
                var modelProp = computerSystem.GetType().GetProperty("Model");
                if (modelProp != null)
                {
                    var value = modelProp.GetValue(computerSystem)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            // 备用方案
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                return key?.GetValue("SystemProductName")?.ToString() ?? "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var model = ReadLinuxDmiFile("product_name");
                return !string.IsNullOrEmpty(model) ? model : "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (ExecuteCommand("system_profiler", "SPHardwareDataType", out var output))
                {
                    var match = Regex.Match(output, @"Model Name:\s*(.+)");
                    if (match.Success)
                        return match.Groups[1].Value.Trim();
                }
                return "Unknown";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取系统类型（详细）
    /// </summary>
    /// <returns>系统类型（如：x64-based PC）</returns>
    public static string GetSystemType()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var computerSystem = HardwareInfo.ComputerSystemList.FirstOrDefault();
            if (computerSystem != null)
            {
                var systemTypeProp = computerSystem.GetType().GetProperty("SystemType");
                if (systemTypeProp != null)
                {
                    var value = systemTypeProp.GetValue(computerSystem)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            // 备用方案
            var arch = RuntimeInformation.OSArchitecture.ToString();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"{arch}-based PC";
            }
            return $"{arch}-based System";
        }
        catch
        {
            return RuntimeInformation.OSArchitecture.ToString();
        }
    }

    /// <summary>
    /// 获取处理器名称（完整）
    /// </summary>
    /// <returns>处理器名称</returns>
    public static string GetProcessorName()
    {
        return GetCpuName();
    }

    /// <summary>
    /// 获取BIOS版本
    /// </summary>
    /// <returns>BIOS版本</returns>
    public static string GetBiosVersion()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var bios = HardwareInfo.BiosList.FirstOrDefault();
            if (bios != null)
            {
                var versionProp = bios.GetType().GetProperty("Version");
                if (versionProp != null)
                {
                    var value = versionProp.GetValue(bios)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            // 备用方案
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                return key?.GetValue("BIOSVersion")?.ToString() ?? "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var version = ReadLinuxDmiFile("bios_version");
                return !string.IsNullOrEmpty(version) ? version : "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (ExecuteCommand("system_profiler", "SPHardwareDataType", out var output))
                {
                    var match = Regex.Match(output, @"Boot ROM Version:\s*(.+)");
                    if (match.Success)
                        return match.Groups[1].Value.Trim();
                }
                return "Unknown";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取BIOS发布日期
    /// </summary>
    /// <returns>BIOS发布日期</returns>
    public static string GetBiosReleaseDate()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var bios = HardwareInfo.BiosList.FirstOrDefault();
            if (bios != null)
            {
                var releaseDateProp = bios.GetType().GetProperty("ReleaseDate");
                if (releaseDateProp != null)
                {
                    var releaseDate = releaseDateProp.GetValue(bios)?.ToString();
                    if (!string.IsNullOrWhiteSpace(releaseDate) && releaseDate.Length >= 8)
                    {
                        // 格式：yyyyMMdd 或 yyyyMMddHHmmss
                        var year = releaseDate.Substring(0, 4);
                        var month = releaseDate.Substring(4, 2);
                        var day = releaseDate.Substring(6, 2);
                        return $"{year}-{month}-{day}";
                    }
                    if (!string.IsNullOrWhiteSpace(releaseDate))
                        return releaseDate;
                }
            }

            // 备用方案
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                var releaseDate = key?.GetValue("BIOSReleaseDate")?.ToString();
                if (!string.IsNullOrEmpty(releaseDate) && releaseDate.Length >= 8)
                {
                    var year = releaseDate.Substring(0, 4);
                    var month = releaseDate.Substring(4, 2);
                    var day = releaseDate.Substring(6, 2);
                    return $"{year}-{month}-{day}";
                }
                return "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var date = ReadLinuxDmiFile("bios_date");
                return !string.IsNullOrEmpty(date) ? date : "Unknown";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取BIOS模式
    /// </summary>
    /// <returns>BIOS模式（UEFI 或 Legacy）</returns>
    public static string GetBiosMode()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: 检查 EFI 分区是否存在
                var efiPartition = DriveInfo.GetDrives()
                    .FirstOrDefault(d => d.Name.StartsWith(@"C:\") && 
                        Directory.Exists(Path.Combine(d.RootDirectory.FullName, "EFI")));
                return efiPartition != null ? "UEFI" : "Legacy";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: 检查 /sys/firmware/efi 是否存在
                return Directory.Exists("/sys/firmware/efi") ? "UEFI" : "Legacy";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS 通常使用 UEFI
                return "UEFI";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取主板制造商
    /// </summary>
    /// <returns>主板制造商</returns>
    public static string GetMotherboardManufacturer()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var motherboard = HardwareInfo.MotherboardList.FirstOrDefault();
            if (motherboard != null)
            {
                var manufacturerProp = motherboard.GetType().GetProperty("Manufacturer");
                if (manufacturerProp != null)
                {
                    var value = manufacturerProp.GetValue(motherboard)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            // 备用方案
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                return key?.GetValue("BaseBoardManufacturer")?.ToString() ?? "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var manufacturer = ReadLinuxDmiFile("board_vendor");
                return !string.IsNullOrEmpty(manufacturer) ? manufacturer : "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (ExecuteCommand("system_profiler", "SPHardwareDataType", out var output))
                {
                    var match = Regex.Match(output, @"Model Identifier:\s*(.+)");
                    if (match.Success)
                        return "Apple Inc.";
                }
                return "Apple Inc.";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取主板产品型号
    /// </summary>
    /// <returns>主板产品型号</returns>
    public static string GetMotherboardProduct()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var motherboard = HardwareInfo.MotherboardList.FirstOrDefault();
            if (motherboard != null)
            {
                var productProp = motherboard.GetType().GetProperty("Product");
                if (productProp != null)
                {
                    var value = productProp.GetValue(motherboard)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            // 备用方案
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                return key?.GetValue("BaseBoardProduct")?.ToString() ?? "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var product = ReadLinuxDmiFile("board_name");
                return !string.IsNullOrEmpty(product) ? product : "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (ExecuteCommand("system_profiler", "SPHardwareDataType", out var output))
                {
                    var match = Regex.Match(output, @"Model Identifier:\s*(.+)");
                    if (match.Success)
                        return match.Groups[1].Value.Trim();
                }
                return "Unknown";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取主板版本
    /// </summary>
    /// <returns>主板版本</returns>
    public static string GetMotherboardVersion()
    {
        try
        {
            // 优先使用 Hardware.Info（通过反射检查属性）
            var motherboard = HardwareInfo.MotherboardList.FirstOrDefault();
            if (motherboard != null)
            {
                var versionProp = motherboard.GetType().GetProperty("Version");
                if (versionProp != null)
                {
                    var value = versionProp.GetValue(motherboard)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            // 备用方案
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                return key?.GetValue("BaseBoardVersion")?.ToString() ?? "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var version = ReadLinuxDmiFile("board_version");
                return !string.IsNullOrEmpty(version) ? version : "Unknown";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取平台角色
    /// </summary>
    /// <returns>平台角色（台式机、笔记本、工作站、服务器等）</returns>
    public static string GetPlatformRole()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: 从注册表获取
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SystemInformation");
                var systemType = key?.GetValue("SystemType")?.ToString();
                if (!string.IsNullOrEmpty(systemType))
                {
                    if (systemType.Contains("Server", StringComparison.OrdinalIgnoreCase))
                        return "Server";
                    if (systemType.Contains("Workstation", StringComparison.OrdinalIgnoreCase))
                        return "Workstation";
                }
                // 尝试从系统型号判断
                var model = GetSystemModel();
                if (model.Contains("Laptop", StringComparison.OrdinalIgnoreCase) || 
                    model.Contains("Notebook", StringComparison.OrdinalIgnoreCase))
                    return "Mobile";
                return "Desktop";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: 从 /sys/class/dmi/id/ 读取
                var chassis = ReadLinuxDmiFile("chassis_type");
                if (!string.IsNullOrEmpty(chassis))
                {
                    // DMI chassis type codes
                    return chassis switch
                    {
                        "1" => "Other",
                        "2" => "Unknown",
                        "3" => "Desktop",
                        "4" => "Low Profile Desktop",
                        "5" => "Pizza Box",
                        "6" => "Mini Tower",
                        "7" => "Tower",
                        "8" => "Portable",
                        "9" => "Laptop",
                        "10" => "Notebook",
                        "11" => "Hand Held",
                        "12" => "Docking Station",
                        "13" => "All in One",
                        "14" => "Sub Notebook",
                        "15" => "Space-Saving",
                        "16" => "Lunch Box",
                        "17" => "Main System Chassis",
                        "18" => "Expansion Chassis",
                        "19" => "SubChassis",
                        "20" => "Bus Expansion Chassis",
                        "21" => "Peripheral Chassis",
                        "22" => "RAID Chassis",
                        "23" => "Rack Mount Chassis",
                        "24" => "Sealed-Case PC",
                        _ => "Unknown"
                    };
                }
                return "Unknown";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: 使用 system_profiler
                if (ExecuteCommand("system_profiler", "SPHardwareDataType", out var output))
                {
                    var match = Regex.Match(output, @"Model Name:\s*(.+)");
                    if (match.Success)
                    {
                        var modelName = match.Groups[1].Value.Trim();
                        if (modelName.Contains("MacBook", StringComparison.OrdinalIgnoreCase))
                            return "Mobile";
                        if (modelName.Contains("Mac Pro", StringComparison.OrdinalIgnoreCase) ||
                            modelName.Contains("Mac Studio", StringComparison.OrdinalIgnoreCase))
                            return "Workstation";
                        if (modelName.Contains("Mac mini", StringComparison.OrdinalIgnoreCase))
                            return "Desktop";
                    }
                }
                return "Desktop";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 读取 Linux DMI 文件
    /// </summary>
    private static string ReadLinuxDmiFile(string fileName)
    {
        try
        {
            var path = $"/sys/class/dmi/id/{fileName}";
            if (File.Exists(path))
            {
                return File.ReadAllText(path).Trim();
            }
        }
        catch
        {
            // 忽略错误
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取区域信息
    /// </summary>
    /// <returns>区域信息</returns>
    public static string GetRegion()
    {
        try
        {
            var region = System.Globalization.RegionInfo.CurrentRegion;
            return region.DisplayName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取时区
    /// </summary>
    /// <returns>时区信息</returns>
    public static string GetTimeZone()
    {
        try
        {
            return TimeZoneInfo.Local.DisplayName;
        }
        catch
        {
            return "Unknown";
        }
    }

    #endregion

    #region 已安装软件信息（跨平台）

    /// <summary>
    /// 软件信息
    /// </summary>
    public class SoftwareInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string InstallDate { get; set; } = string.Empty;
        public string InstallLocation { get; set; } = string.Empty;
    }

    /// <summary>
    /// 获取已安装的软件列表（跨平台）
    /// </summary>
    /// <returns>软件信息列表</returns>
    public static List<SoftwareInfo> GetInstalledSoftware()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetInstalledSoftwareWindows();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetInstalledSoftwareLinux();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetInstalledSoftwareMacOS();
        return new List<SoftwareInfo>();
    }

    /// <summary>
    /// 获取已安装的软件列表（Windows）
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static List<SoftwareInfo> GetInstalledSoftwareWindows()
    {
        var softwareList = new List<SoftwareInfo>();
        
        try
        {
#pragma warning disable CA1416 // 此方法已通过 [SupportedOSPlatform("windows")] 特性和运行时检查保护
            // 读取64位软件注册表
            ReadWindowsRegistry(softwareList, 
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"));
            
            // 读取32位软件注册表（在64位系统上）
            if (Environment.Is64BitOperatingSystem)
            {
                ReadWindowsRegistry(softwareList, 
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"));
            }
            
            // 读取当前用户安装的软件
            ReadWindowsRegistry(softwareList, 
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"));
#pragma warning restore CA1416
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取Windows软件列表失败: {ex.Message}");
        }
        
        return softwareList.DistinctBy(s => s.Name).OrderBy(s => s.Name).ToList();
    }

    /// <summary>
    /// 读取Windows注册表获取软件信息
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ReadWindowsRegistry(List<SoftwareInfo> softwareList, RegistryKey? parentKey)
    {
        if (parentKey == null) return;
        
        try
        {
#pragma warning disable CA1416 // 此方法已通过 [SupportedOSPlatform("windows")] 特性保护
            foreach (var subKeyName in parentKey.GetSubKeyNames())
            {
                using var subKey = parentKey.OpenSubKey(subKeyName);
                if (subKey == null) continue;
                
                var displayName = subKey.GetValue("DisplayName")?.ToString();
                if (string.IsNullOrWhiteSpace(displayName)) continue;
                
                // 过滤系统更新和补丁
                if (displayName.Contains("Update for") || displayName.StartsWith("KB"))
                    continue;
                
                softwareList.Add(new SoftwareInfo
                {
                    Name = displayName,
                    Version = subKey.GetValue("DisplayVersion")?.ToString() ?? "",
                    Publisher = subKey.GetValue("Publisher")?.ToString() ?? "",
                    InstallDate = subKey.GetValue("InstallDate")?.ToString() ?? "",
                    InstallLocation = subKey.GetValue("InstallLocation")?.ToString() ?? ""
                });
            }
#pragma warning restore CA1416
        }
        catch
        {
            // 忽略读取失败的注册表项
        }
        finally
        {
#pragma warning disable CA1416
            parentKey.Close();
#pragma warning restore CA1416
        }
    }

    /// <summary>
    /// 获取已安装的软件列表（Linux）
    /// </summary>
    private static List<SoftwareInfo> GetInstalledSoftwareLinux()
    {
        var softwareList = new List<SoftwareInfo>();
        
        try
        {
            // 尝试不同的包管理器
            if (ExecuteCommand("which", "dpkg", out _))
            {
                // Debian/Ubuntu (dpkg)
                if (ExecuteCommand("dpkg", "-l", out var output))
                {
                    ParseDpkgOutput(output, softwareList);
                }
            }
            else if (ExecuteCommand("which", "rpm", out _))
            {
                // Red Hat/CentOS (rpm)
                if (ExecuteCommand("rpm", "-qa", out var output))
                {
                    ParseRpmOutput(output, softwareList);
                }
            }
            else if (ExecuteCommand("which", "pacman", out _))
            {
                // Arch Linux (pacman)
                if (ExecuteCommand("pacman", "-Q", out var output))
                {
                    ParsePacmanOutput(output, softwareList);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取Linux软件列表失败: {ex.Message}");
        }
        
        return softwareList.OrderBy(s => s.Name).ToList();
    }

    /// <summary>
    /// 解析 dpkg 输出
    /// </summary>
    private static void ParseDpkgOutput(string output, List<SoftwareInfo> softwareList)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (!line.StartsWith("ii")) continue;
            
            var parts = Regex.Split(line, @"\s+");
            if (parts.Length >= 4)
            {
                softwareList.Add(new SoftwareInfo
                {
                    Name = parts[1],
                    Version = parts[2],
                    Publisher = "Debian Package"
                });
            }
        }
    }

    /// <summary>
    /// 解析 rpm 输出
    /// </summary>
    private static void ParseRpmOutput(string output, List<SoftwareInfo> softwareList)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"^(.+?)-(\d+.*)$");
            if (match.Success)
            {
                softwareList.Add(new SoftwareInfo
                {
                    Name = match.Groups[1].Value,
                    Version = match.Groups[2].Value,
                    Publisher = "RPM Package"
                });
            }
        }
    }

    /// <summary>
    /// 解析 pacman 输出
    /// </summary>
    private static void ParsePacmanOutput(string output, List<SoftwareInfo> softwareList)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                softwareList.Add(new SoftwareInfo
                {
                    Name = parts[0],
                    Version = parts[1],
                    Publisher = "Arch Package"
                });
            }
        }
    }

    /// <summary>
    /// 获取已安装的软件列表（macOS）
    /// </summary>
    private static List<SoftwareInfo> GetInstalledSoftwareMacOS()
    {
        var softwareList = new List<SoftwareInfo>();
        
        try
        {
            // 获取 /Applications 目录下的应用
            var applicationsDir = "/Applications";
            if (Directory.Exists(applicationsDir))
            {
                var appDirs = Directory.GetDirectories(applicationsDir, "*.app");
                foreach (var appDir in appDirs)
                {
                    var appName = Path.GetFileNameWithoutExtension(appDir);
                    softwareList.Add(new SoftwareInfo
                    {
                        Name = appName,
                        InstallLocation = appDir,
                        Publisher = "macOS Application"
                    });
                }
            }
            
            // 获取 Homebrew 安装的软件
            if (ExecuteCommand("brew", "list --formula", out var brewOutput))
            {
                var lines = brewOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    softwareList.Add(new SoftwareInfo
                    {
                        Name = line.Trim(),
                        Publisher = "Homebrew"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取macOS软件列表失败: {ex.Message}");
        }
        
        return softwareList.OrderBy(s => s.Name).ToList();
    }

    #endregion

    #region 用户账户信息（跨平台）

    /// <summary>
    /// 用户账户信息
    /// </summary>
    public class UserAccountInfo
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string HomeDirectory { get; set; } = string.Empty;
        public string Shell { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 获取系统用户账户列表（跨平台）
    /// </summary>
    /// <returns>用户账户信息列表</returns>
    public static List<UserAccountInfo> GetUserAccounts()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetUserAccountsWindows();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetUserAccountsLinux();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetUserAccountsMacOS();
        return new List<UserAccountInfo>();
    }

    /// <summary>
    /// 获取当前用户是否为管理员
    /// </summary>
    /// <returns>是否为管理员</returns>
    public static bool IsCurrentUserAdmin()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            else
            {
                // Linux/macOS: 检查是否为 root 用户或在 sudo 组
                var userId = Environment.GetEnvironmentVariable("UID") ?? "1000";
                return userId == "0";
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取系统用户账户列表（Windows）
    /// </summary>
    private static List<UserAccountInfo> GetUserAccountsWindows()
    {
        var userList = new List<UserAccountInfo>();
        
        try
        {
            if (ExecuteCommand("net", "user", out var output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var startParsing = false;
                
                foreach (var line in lines)
                {
                    if (line.Contains("----"))
                    {
                        startParsing = true;
                        continue;
                    }
                    
                    if (startParsing && !line.StartsWith("The command"))
                    {
                        var users = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var user in users)
                        {
                            if (!string.IsNullOrWhiteSpace(user))
                            {
                                userList.Add(new UserAccountInfo
                                {
                                    UserName = user.Trim(),
                                    IsAdmin = IsWindowsUserAdmin(user.Trim())
                                });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取Windows用户列表失败: {ex.Message}");
        }
        
        return userList;
    }

    /// <summary>
    /// 检查Windows用户是否为管理员
    /// </summary>
    private static bool IsWindowsUserAdmin(string userName)
    {
        try
        {
            if (ExecuteCommand("net", $"localgroup administrators", out var output))
            {
                return output.Contains(userName, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // 忽略错误
        }
        return false;
    }

    /// <summary>
    /// 获取系统用户账户列表（Linux）
    /// </summary>
    private static List<UserAccountInfo> GetUserAccountsLinux()
    {
        var userList = new List<UserAccountInfo>();
        
        try
        {
            var passwdFile = "/etc/passwd";
            if (File.Exists(passwdFile))
            {
                var lines = File.ReadAllLines(passwdFile);
                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length >= 7)
                    {
                        // 过滤系统账户（UID < 1000）
                        if (int.TryParse(parts[2], out var uid) && uid >= 1000)
                        {
                            userList.Add(new UserAccountInfo
                            {
                                UserName = parts[0],
                                FullName = parts[4],
                                HomeDirectory = parts[5],
                                Shell = parts[6],
                                UserId = parts[2],
                                GroupId = parts[3],
                                IsAdmin = IsLinuxUserAdmin(parts[0])
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取Linux用户列表失败: {ex.Message}");
        }
        
        return userList;
    }

    /// <summary>
    /// 检查Linux用户是否为管理员
    /// </summary>
    private static bool IsLinuxUserAdmin(string userName)
    {
        try
        {
            if (ExecuteCommand("groups", userName, out var output))
            {
                return output.Contains("sudo") || output.Contains("wheel") || output.Contains("admin");
            }
        }
        catch
        {
            // 忽略错误
        }
        return false;
    }

    /// <summary>
    /// 获取系统用户账户列表（macOS）
    /// </summary>
    private static List<UserAccountInfo> GetUserAccountsMacOS()
    {
        var userList = new List<UserAccountInfo>();
        
        try
        {
            if (ExecuteCommand("dscl", ". -list /Users", out var output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var userName = line.Trim();
                    // 过滤系统账户
                    if (!userName.StartsWith("_") && userName != "daemon" && userName != "nobody")
                    {
                        userList.Add(new UserAccountInfo
                        {
                            UserName = userName,
                            IsAdmin = IsMacOSUserAdmin(userName)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取macOS用户列表失败: {ex.Message}");
        }
        
        return userList;
    }

    /// <summary>
    /// 检查macOS用户是否为管理员
    /// </summary>
    private static bool IsMacOSUserAdmin(string userName)
    {
        try
        {
            if (ExecuteCommand("dscl", $". -read /Groups/admin GroupMembership", out var output))
            {
                return output.Contains(userName);
            }
        }
        catch
        {
            // 忽略错误
        }
        return false;
    }

    #endregion

    #region 命令执行辅助方法

    /// <summary>
    /// 执行系统命令
    /// </summary>
    private static bool ExecuteCommand(string command, string arguments, out string output)
    {
        output = string.Empty;
        
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(processStartInfo);
            if (process == null) return false;
            
            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000); // 5秒超时
            
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region 综合信息

    /// <summary>
    /// 获取完整的系统信息摘要
    /// </summary>
    /// <returns>系统信息字典</returns>
    public static Dictionary<string, string> GetSystemInfo()
    {
        return new Dictionary<string, string>
        {
            { "IP地址", GetLocalIpAddress() },
            { "MAC地址", GetMacAddress() },
            { "操作系统", GetOsDescription() },
            { "系统版本", GetOsVersion() },
            { "系统类型", GetOsType() },
            { "系统架构", GetOsArchitecture() },
            { "机器名称", GetMachineName() },
            { "用户名", GetUserName() },
            { "是否管理员", IsCurrentUserAdmin() ? "是" : "否" },
            { "CPU", GetCpuInfo() },
            { "CPU名称", GetCpuName() },
            { "CPU核心", GetCpuCores().ToString() },
            { "物理内存", $"{GetTotalMemoryGb():F2} GB" },
            { "可用内存", $"{GetAvailableMemoryGb():F2} GB" },
            { "内存使用率", $"{GetMemoryUsagePercent():F1}%" },
            { "运行时", GetFrameworkVersion() },
            { "进程架构", GetProcessArchitecture() },
            { "系统运行时间", GetSystemUptime().ToString(@"dd\.hh\:mm\:ss") },
            { "进程内存", $"{GetWorkingSetMemoryMb():F2} MB" },
            { "已安装软件数", GetInstalledSoftware().Count.ToString() },
            { "系统用户数", GetUserAccounts().Count.ToString() }
        };
    }

    #endregion
}
