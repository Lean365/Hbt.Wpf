// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedCoordinator.cs
// 创建时间：2025-11-12
// 创建人：Takt365(Cursor AI)
// 功能描述：统一种子数据协调执行器
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Takt.Common.Logging;

namespace Takt.Infrastructure.Data;

/// <summary>
/// 种子数据协调执行器，统一调度各模块的种子初始化
/// </summary>
public class DbSeedCoordinator
{
    private readonly IConfiguration _configuration;
    private readonly InitLogManager _initLog;
    private readonly DbSeedLanguage _languageSeeder;
    private readonly DbSeedTranslationCommon _translationCommonSeeder;
    private readonly DbSeedTranslationDictionary _translationDictionarySeeder;
    private readonly DbSeedTranslationEntity _translationEntitySeeder;
    private readonly DbSeedTranslationValidation _translationValidationSeeder;
    private readonly DbSeedRoutineDictionary _dictionarySeeder;
    private readonly DbSeedSetting _settingSeeder;
    private readonly DbSeedRbac _rbacSeeder;
    private readonly DbSeedProdModel _prodModelSeeder;
    private readonly DbSeedVisit _visitSeeder;
    private readonly DbSeedQuartz _quartzSeeder;

    public DbSeedCoordinator(
        IConfiguration configuration,
        InitLogManager initLog,
        DbSeedLanguage languageSeeder,
        DbSeedTranslationCommon translationCommonSeeder,
        DbSeedTranslationDictionary translationDictionarySeeder,
        DbSeedTranslationEntity translationEntitySeeder,
        DbSeedTranslationValidation translationValidationSeeder,
        DbSeedRoutineDictionary dictionarySeeder,
        DbSeedSetting settingSeeder,
        DbSeedRbac rbacSeeder,
        DbSeedProdModel prodModelSeeder,
        DbSeedVisit visitSeeder,
        DbSeedQuartz quartzSeeder)
    {
        _configuration = configuration;
        _initLog = initLog;
        _languageSeeder = languageSeeder;
        _translationCommonSeeder = translationCommonSeeder;
        _translationDictionarySeeder = translationDictionarySeeder;
        _translationEntitySeeder = translationEntitySeeder;
        _translationValidationSeeder = translationValidationSeeder;
        _dictionarySeeder = dictionarySeeder;
        _settingSeeder = settingSeeder;
        _rbacSeeder = rbacSeeder;
        _prodModelSeeder = prodModelSeeder;
        _visitSeeder = visitSeeder;
        _quartzSeeder = quartzSeeder;
    }

    /// <summary>
    /// 执行全部种子初始化
    /// </summary>
    public async Task InitializeAsync()
    {
        var enableSeedData = bool.Parse(_configuration["DatabaseSettings:EnableSeedData"] ?? "false");
        if (!enableSeedData)
        {
            _initLog.Information("种子数据功能已禁用，跳过协调器执行");
            return;
        }

        _initLog.Information("================== 种子数据协调器 ==================");

        // 注意：系统菜单在 RBAC 初始化中统一创建，避免重复初始化
        _initLog.Information("[1/11] 初始化 RBAC（用户/角色/菜单）...");
        await _rbacSeeder.InitializeAsync();

        _initLog.Information("[2/11] 初始化基础语言...");
        _languageSeeder.Initialize();

        _initLog.Information("[3/11] 初始化产品机种种子数据...");
        _prodModelSeeder.Run();

        _initLog.Information("[4/11] 初始化随行人员种子数据...");
        _visitSeeder.Run();

        _initLog.Information("[5/11] 初始化Quartz任务种子数据...");
        _quartzSeeder.Run();

        _initLog.Information("[6/11] 初始化通用翻译...");
        _translationCommonSeeder.Initialize();

        _initLog.Information("[7/11] 初始化字典翻译...");
        _translationDictionarySeeder.Initialize();

        _initLog.Information("[8/11] 初始化实体字段翻译...");
        _translationEntitySeeder.Run();

        _initLog.Information("[9/11] 初始化验证消息翻译...");
        _translationValidationSeeder.Initialize();

        _initLog.Information("[10/11] 初始化字典类型与数据...");
        _dictionarySeeder.Run();

        _initLog.Information("[11/11] 初始化系统设置...");
        _settingSeeder.Run();

        _initLog.Information("✅ 种子数据协调器执行完成");
        _initLog.Information("====================================================");
    }
}
