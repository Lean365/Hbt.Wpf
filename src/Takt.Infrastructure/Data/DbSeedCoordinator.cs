// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
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
    private readonly DbSeedRoutineLanguage _languageSeeder;
    private readonly DbSeedRoutineDictionary _dictionarySeeder;
    private readonly DbSeedRoutineSetting _settingSeeder;
    private readonly DbSeedRoutineEntity _entitySeeder;
    private readonly DbSeedMenu _menuSeeder;
    private readonly DbSeedRbac _rbacSeeder;

    public DbSeedCoordinator(
        IConfiguration configuration,
        InitLogManager initLog,
        DbSeedRoutineLanguage languageSeeder,
        DbSeedRoutineDictionary dictionarySeeder,
        DbSeedRoutineSetting settingSeeder,
        DbSeedRoutineEntity entitySeeder,
        DbSeedMenu menuSeeder,
        DbSeedRbac rbacSeeder)
    {
        _configuration = configuration;
        _initLog = initLog;
        _languageSeeder = languageSeeder;
        _dictionarySeeder = dictionarySeeder;
        _settingSeeder = settingSeeder;
        _entitySeeder = entitySeeder;
        _menuSeeder = menuSeeder;
        _rbacSeeder = rbacSeeder;
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

        _initLog.Information("[1/6] 初始化基础语言与通用翻译...");
        _languageSeeder.Initialize();

        _initLog.Information("[2/6] 初始化字典类型与数据...");
        _dictionarySeeder.Run();

        _initLog.Information("[3/6] 初始化系统设置...");
        _settingSeeder.Run();

        _initLog.Information("[4/6] 初始化实体字段翻译...");
        _entitySeeder.Run();

        _initLog.Information("[5/6] 初始化系统菜单...");
        _menuSeeder.CreateSystemMenus();

        _initLog.Information("[6/6] 初始化 RBAC（用户/角色/菜单）...");
        await _rbacSeeder.InitializeAsync();

        _initLog.Information("✅ 种子数据协调器执行完成");
        _initLog.Information("====================================================");
    }
}
