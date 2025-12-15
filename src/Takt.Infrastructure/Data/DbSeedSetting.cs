// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedRoutineSetting.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：系统设置种子数据初始化
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.Generic;
using System;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Routine 模块系统设置种子初始化器
/// </summary>
public class DbSeedSetting
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Setting> _settingRepository;

    public DbSeedSetting(InitLogManager initLog, IBaseRepository<Setting> settingRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
    }

    /// <summary>
    /// 执行系统设置初始化（创建或更新）
    /// </summary>
    public void Run()
    {
        _initLog.Information("开始初始化系统设置..");

        foreach (var seed in BuildSettingSeeds())
        {
            var existing = _settingRepository.GetFirst(s => s.SettingKey == seed.SettingKey);

            if (existing == null)
            {
                _settingRepository.Create(seed, "Takt365");
            }
            else
            {
                existing.SettingValue = seed.SettingValue;
                existing.SettingDescription = seed.SettingDescription;
                existing.Category = seed.Category;
                existing.SettingType = seed.SettingType;
                existing.OrderNum = seed.OrderNum;
                existing.IsBuiltin = seed.IsBuiltin;
                existing.IsEditable = seed.IsEditable;
                existing.IsDefault = seed.IsDefault;
                _settingRepository.Update(existing, "Takt365");
            }
        }

        _initLog.Information("✅ 系统设置初始化完成");
    }

    private static List<Setting> BuildSettingSeeds()
    {
        return new List<Setting>
        {
            // AppSettings
            new Setting { SettingKey = "AppSettings:DefaultLanguage", SettingValue = "zh-CN", SettingDescription = "系统默认语言（zh-CN:简体中文, en-US:英文, ja-JP:日文）", Category = "AppSettings", SettingType = 0, OrderNum = 1, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "AppSettings:DefaultTheme", SettingValue = "Takt365", SettingDescription = "系统默认主题（Light:浅色, Dark:深色, System:跟随系统）", Category = "AppSettings", SettingType = 0, OrderNum = 2, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "AppSettings:FontFamily", SettingValue = "Microsoft YaHei UI", SettingDescription = "系统默认字体（如：Microsoft YaHei UI, SimSun, Arial）", Category = "AppSettings", SettingType = 0, OrderNum = 3, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "AppSettings:FontSize", SettingValue = "14", SettingDescription = "系统默认字体大小（单位：pt）", Category = "AppSettings", SettingType = 1, OrderNum = 4, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
 
            // SystemInfo
            new Setting { SettingKey = "SystemInfo:SystemName", SettingValue = "节拍（Takt）中小企业平台", SettingDescription = "系统名称，显示在标题栏和应用标题位置", Category = "SystemInfo", SettingType = 0, OrderNum = 1, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "SystemInfo:SystemVersion", SettingValue = "0.0.1", SettingDescription = "系统版本号", Category = "SystemInfo", SettingType = 0, OrderNum = 2, IsBuiltin = 0, IsEditable = 1, IsDefault = 1 },
            new Setting { SettingKey = "SystemInfo:Watermark", SettingValue = string.Empty, SettingDescription = "系统水印文本（留空则不显示水印）", Category = "SystemInfo", SettingType = 0, OrderNum = 3, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "SystemInfo:LogoPath", SettingValue = "pack://application:,,,/Assets/hbt-loto.ico", SettingDescription = "系统Logo路径（支持资源路径或文件路径）", Category = "SystemInfo", SettingType = 0, OrderNum = 4, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "SystemInfo:Copyright", SettingValue = "© 2025 Takt All rights reserved.", SettingDescription = "系统版权信息", Category = "SystemInfo", SettingType = 0, OrderNum = 5, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
 
            // Security
            new Setting { SettingKey = "Security:UserLockoutDuration", SettingValue = "30", SettingDescription = "用户锁定时间（单位：分钟，登录失败达到指定次数后锁定）", Category = "Security", SettingType = 1, OrderNum = 1, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "Security:MaxLoginAttempts", SettingValue = "5", SettingDescription = "最大登录尝试次数（超过此次数将锁定账户）", Category = "Security", SettingType = 1, OrderNum = 2, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "Security:PasswordMinLength", SettingValue = "8", SettingDescription = "密码最小长度", Category = "Security", SettingType = 1, OrderNum = 3, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "Security:PasswordRequireDigit", SettingValue = "true", SettingDescription = "密码是否必须包含数字", Category = "Security", SettingType = 2, OrderNum = 4, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "Security:PasswordRequireUppercase", SettingValue = "false", SettingDescription = "密码是否必须包含大写字母", Category = "Security", SettingType = 2, OrderNum = 5, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "Security:PasswordRequireLowercase", SettingValue = "false", SettingDescription = "密码是否必须包含小写字母", Category = "Security", SettingType = 2, OrderNum = 6, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "Security:PasswordRequireSpecialChar", SettingValue = "false", SettingDescription = "密码是否必须包含特殊字符", Category = "Security", SettingType = 2, OrderNum = 7, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
 
            // UI
            new Setting { SettingKey = "UI:ShowWatermark", SettingValue = "false", SettingDescription = "是否显示水印", Category = "UI", SettingType = 2, OrderNum = 1, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 },
            new Setting { SettingKey = "UI:WatermarkOpacity", SettingValue = "0.3", SettingDescription = "水印透明度（0.0-1.0）", Category = "UI", SettingType = 1, OrderNum = 2, IsBuiltin = 0, IsEditable = 0, IsDefault = 1 }
        };
    }
}

