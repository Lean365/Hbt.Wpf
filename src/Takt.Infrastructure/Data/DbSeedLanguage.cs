//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : DbSeedLanguage.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 语言种子数据初始化服务
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//===================================================================

using System;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// 语言种子数据初始化服务
/// 创建中英日三语的基础语言数据
/// </summary>
public class DbSeedLanguage
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Language> _languageRepository;

    public DbSeedLanguage(
        InitLogManager initLog,
        IBaseRepository<Language> languageRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
    }

    /// <summary>
    /// 初始化语言数据
    /// </summary>
    public void Initialize()
    {
        _initLog.Information("开始初始化语言种子数据...");

        InitializeLanguages();

        _initLog.Information("✅ 语言种子数据初始化完成");
    }

    /// <summary>
    /// 初始化语言数据（中英日三语）
    /// </summary>
    private void InitializeLanguages()
    {
        // 中文（简体）
        var zhCn = _languageRepository.GetFirst(l => l.LanguageCode == "zh-CN");
        if (zhCn == null)
        {
            zhCn = new Language
            {
                LanguageCode = "zh-CN",
                LanguageName = "简体中文",
                NativeName = "简体中文",
                LanguageIcon = "🇨🇳",
                IsDefault = 0,  // 布尔字段：0=是（默认）
                IsBuiltin = 0,  // 布尔字段：0=是（内置）
                OrderNum = 1,
                LanguageStatus = 0
            };
            _languageRepository.Create(zhCn, "Takt365");
            _initLog.Information("✅ 创建语言：简体中文");
        }

        // 英文（美国）
        var enUs = _languageRepository.GetFirst(l => l.LanguageCode == "en-US");
        if (enUs == null)
        {
            enUs = new Language
            {
                LanguageCode = "en-US",
                LanguageName = "English",
                NativeName = "English",
                LanguageIcon = "🇺🇸",
                IsDefault = 1,  // 布尔字段：1=否（非默认）
                IsBuiltin = 0,  // 布尔字段：0=是（内置）
                OrderNum = 2,
                LanguageStatus = 0
            };
            _languageRepository.Create(enUs, "Takt365");
            _initLog.Information("✅ 创建语言：English");
        }

        // 日文
        var jaJp = _languageRepository.GetFirst(l => l.LanguageCode == "ja-JP");
        if (jaJp == null)
        {
            jaJp = new Language
            {
                LanguageCode = "ja-JP",
                LanguageName = "日本語",
                NativeName = "日本語",
                LanguageIcon = "🇯🇵",
                IsDefault = 1,  // 布尔字段：1=否（非默认）
                IsBuiltin = 0,  // 布尔字段：0=是（内置）
                OrderNum = 3,
                LanguageStatus = 0
            };
            _languageRepository.Create(jaJp, "Takt365");
            _initLog.Information("✅ 创建语言：日本語");
        }
    }
}
