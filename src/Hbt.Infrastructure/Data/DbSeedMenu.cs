// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：DbSeedMenu.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：菜单种子数据初始化服务
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

using Hbt.Common.Enums;
using Hbt.Common.Logging;
using Hbt.Domain.Entities.Identity;
using Hbt.Domain.Entities.Routine;
using Hbt.Domain.Repositories;

namespace Hbt.Infrastructure.Data;

/// <summary>
/// 菜单种子数据初始化服务
/// </summary>
/// <remarks>
/// 使用 BaseRepository 自动填充审计字段和处理雪花ID
/// </remarks>
public class DbSeedMenu
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Menu> _menuRepository;
    private readonly IBaseRepository<RoleMenu> _roleMenuRepository;

    public DbSeedMenu(InitLogManager initLog, IBaseRepository<Menu> menuRepository, IBaseRepository<RoleMenu> roleMenuRepository)
    {
        _initLog = initLog;
        _menuRepository = menuRepository;
        _roleMenuRepository = roleMenuRepository;
    }

    /// <summary>
    /// 创建或更新菜单的辅助方法
    /// </summary>
    private Menu CreateOrUpdateMenu(string code, Action<Menu> configureMenu)
    {
        var existingMenu = _menuRepository.GetFirst(m => m.MenuCode == code);
        var isNew = existingMenu == null;
        
        var menu = existingMenu ?? new Menu();
        
        configureMenu(menu);
        
        // 确保 MenuCode 被设置（在 configureMenu 之后）
        menu.MenuCode = code;
        
        if (isNew)
        {
            _menuRepository.Create(menu, "Hbt365");
            _initLog.Information($"✅ 创建菜单：{menu.MenuName}");
        }
        else
        {
            _menuRepository.Update(menu, "Hbt365");
            _initLog.Information($"✅ 更新菜单：{menu.MenuName}");
        }
        
        return menu;
    }


    /// <summary>
    /// 创建完整的菜单对象
    /// </summary>
    private Menu CreateMenu(
        string menuName,
        string menuCode,
        string? i18nKey = null,
        string? permCode = null,
        MenuTypeEnum menuType = MenuTypeEnum.Menu,
        long? parentId = null,
        string? routePath = null,
        string? icon = null,
        string? component = null,
        ExternalEnum isExternal = ExternalEnum.NotExternal,
        CacheEnum isCache = CacheEnum.NoCache,
        VisibilityEnum isVisible = VisibilityEnum.Visible,
        int orderNum = 0,
        StatusEnum menuStatus = StatusEnum.Normal)
    {
        // 统一处理：null 或 0 都视为顶级菜单（0）
        long normalizedParentId = parentId ?? 0;
        
        return new Menu
        {
            MenuName = menuName,
            MenuCode = menuCode,
            I18nKey = i18nKey,
            PermCode = permCode,
            MenuType = menuType,
            ParentId = normalizedParentId,
            RoutePath = routePath,
            Icon = icon,
            Component = component,
            IsExternal = isExternal,
            IsCache = isCache,
            IsVisible = isVisible,
            OrderNum = orderNum,
            MenuStatus = menuStatus
        };
    }

    /// <summary>
    /// 创建或更新菜单的完整方法（推荐使用）
    /// </summary>
    private Menu CreateOrUpdateMenuComplete(
        string menuCode,
        string menuName,
        string? i18nKey = null,
        string? permCode = null,
        MenuTypeEnum menuType = MenuTypeEnum.Menu,
        long? parentId = null,
        string? routePath = null,
        string? icon = null,
        string? component = null,
        ExternalEnum isExternal = ExternalEnum.NotExternal,
        CacheEnum isCache = CacheEnum.NoCache,
        VisibilityEnum isVisible = VisibilityEnum.Visible,
        int orderNum = 0,
        StatusEnum menuStatus = StatusEnum.Normal)
    {
        var existingMenu = _menuRepository.GetFirst(m => m.MenuCode == menuCode);
        var isNew = existingMenu == null;
        
        var menu = existingMenu ?? CreateMenu(
            menuName, menuCode, i18nKey, permCode, menuType, parentId,
            routePath, icon, component, isExternal, isCache, isVisible, orderNum, menuStatus);
        
        if (!isNew)
        {
            // 更新现有菜单的所有字段
            // 统一处理：null 或 0 都视为顶级菜单（0）
            long normalizedParentId = parentId ?? 0;
            
            menu.MenuName = menuName;
            menu.I18nKey = i18nKey;
            menu.PermCode = permCode;
            menu.MenuType = menuType;
            menu.ParentId = normalizedParentId;
            menu.RoutePath = routePath;
            menu.Icon = icon;
            menu.Component = component;
            menu.IsExternal = isExternal;
            menu.IsCache = isCache;
            menu.IsVisible = isVisible;
            menu.OrderNum = orderNum;
            menu.MenuStatus = menuStatus;
        }
        
        if (isNew)
        {
            _menuRepository.Create(menu, "Hbt365");
            _initLog.Information($"✅ 创建菜单：{menuName}");
        }
        else
        {
            _menuRepository.Update(menu, "Hbt365");
            _initLog.Information($"✅ 更新菜单：{menuName}");
        }
        
        return menu;
    }


    /// <summary>
    /// 创建系统菜单（使用 BaseRepository 同步方法，避免死锁）
    /// </summary>
    /// <remarks>
    /// ⚠️ 重要：在桌面应用（WPF）的事务中使用同步方法，避免死锁
    /// 使用 BaseRepository 自动填充审计字段（CreatedBy、CreatedTime、UpdatedBy、UpdatedTime）
    /// 并根据配置自动处理雪花ID（自增ID或雪花ID）
    /// </remarks>
    public List<Menu> CreateSystemMenus()
    {
        var menus = new List<Menu>();

        _initLog.Information("开始初始化系统菜单（存在则跳过，不存在则创建）...");

        // ==================== 顶级菜单 ====================
        
        // 1. 仪表盘
        var dashboard = CreateOrUpdateMenu("dashboard", m =>
        {
            m.MenuName = "仪表盘";
            m.MenuCode = "dashboard";
            m.I18nKey = "menu.dashboard";
            m.PermCode = "hbt:dashboard:view";
            m.MenuType = MenuTypeEnum.Menu;
            m.ParentId = 0;
            m.RoutePath = "Views/Dashboard/DashboardView";
            m.Component = "Hbt.Fluent.Views.Dashboard.DashboardView";
            m.Icon = "House"; // FontAwesome.Sharp IconChar
            m.OrderNum = 1;
            m.IsExternal = ExternalEnum.NotExternal;
            m.IsCache = CacheEnum.NoCache;
            m.IsVisible = VisibilityEnum.Visible;
            m.MenuStatus = StatusEnum.Normal;
        });
        menus.Add(dashboard);

        // 2. 后勤管理 - 使用完整方法创建
        var logistics = CreateOrUpdateMenuComplete(
            menuCode: "logistics",
            menuName: "后勤管理",
            i18nKey: "menu.logistics",
            menuType: MenuTypeEnum.Directory,
            parentId: 0,
            routePath: "Views/Logistics/LogisticsPage",
            component: "Hbt.Fluent.Views.Logistics.LogisticsPage",
            icon: "BoxesStacked",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(logistics);

        // 3. 身份认证 - 使用完整方法创建
        var identity = CreateOrUpdateMenuComplete(
            menuCode: "identity",
            menuName: "身份认证",
            i18nKey: "menu.identity",
            menuType: MenuTypeEnum.Directory,
            parentId: 0,
            routePath: "Views/Identity/IdentityPage",
            component: "Hbt.Fluent.Views.Identity.IdentityPage",
            icon: "Lock",
            orderNum: 3,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(identity);

        // 4. 日志管理 - 使用完整方法创建
        var logging = CreateOrUpdateMenuComplete(
            menuCode: "logging",
            menuName: "日志管理",
            i18nKey: "menu.logging",
            menuType: MenuTypeEnum.Directory,
            parentId: 0,
            routePath: "Views/Logging/LoggingPage",
            component: "Hbt.Fluent.Views.Logging.LoggingPage",
            icon: "FileLines",
            orderNum: 4,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(logging);

        // 5. 日常事务 - 使用完整方法创建
        var routine = CreateOrUpdateMenuComplete(
            menuCode: "routine",
            menuName: "日常事务",
            i18nKey: "menu.routine",
            menuType: MenuTypeEnum.Directory,
            parentId: 0,
            routePath: "Views/Routine/RoutinePage",
            component: "Hbt.Fluent.Views.Routine.RoutinePage",
            icon: "CalendarDays",
            orderNum: 5,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(routine);

        // 6. 关于 - 使用完整方法创建
        var about = CreateOrUpdateMenuComplete(
            menuCode: "about",
            menuName: "关于",
            i18nKey: "menu.about",
            permCode: "hbt:about:view",
            menuType: MenuTypeEnum.Menu,
            parentId: 0,
            routePath: "Views/About/AboutView",
            component: "Hbt.Fluent.Views.About.AboutView",
            icon: "CircleInfo",
            orderNum: 999,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(about);

        // ==================== 后勤管理子菜单 ====================
        
        // 5. 物料管理（后勤下的二级目录）- 使用完整方法创建
        var materialsMenu = CreateOrUpdateMenuComplete(
            menuCode: "materials",
            menuName: "物料管理",
            i18nKey: "menu.logistics.materials",
            menuType: MenuTypeEnum.Directory,
            parentId: logistics.Id,
            routePath: "Views/Logistics/Materials/MaterialsPage",
            component: "Hbt.Fluent.Views.Logistics.Materials.MaterialsPage",
            icon: "Database",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(materialsMenu);

        // 5.1 生产物料 - 使用完整方法创建
        var materialMenu = CreateOrUpdateMenuComplete(
            menuCode: "prod_material",
            menuName: "生产物料",
            i18nKey: "menu.logistics.materials.material",
            permCode: "logistics:materials:list",
            menuType: MenuTypeEnum.Menu,
            parentId: materialsMenu.Id,
            routePath: "Views/Logistics/Materials/MaterialView",
            component: "Hbt.Fluent.Views.Logistics.Materials.MaterialView",
            icon: "Cube",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(materialMenu);

        // 5.2 机种仕向 - 使用完整方法创建
        var modelMenu = CreateOrUpdateMenuComplete(
            menuCode: "prod_model",
            menuName: "机种仕向",
            i18nKey: "menu.logistics.materials.model",
            permCode: "logistics:materials:model:list",
            menuType: MenuTypeEnum.Menu,
            parentId: materialsMenu.Id,
            routePath: "Views/Logistics/Materials/ModelView",
            component: "Hbt.Fluent.Views.Logistics.Materials.ModelView",
            icon: "TableList",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(modelMenu);

        // 6. 序列号管理（后勤下的二级目录）- 使用完整方法创建
        var serialsMenu = CreateOrUpdateMenuComplete(
            menuCode: "serials",
            menuName: "序列号管理",
            i18nKey: "menu.logistics.serials",
            menuType: MenuTypeEnum.Directory,
            parentId: logistics.Id,
            routePath: "Views/Logistics/Serials/SerialsPage",
            component: "Hbt.Fluent.Views.Logistics.Serials.SerialsPage",
            icon: "Barcode",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(serialsMenu);

        // 6.1 序列号入库 - 使用完整方法创建
        var serialInboundMenu = CreateOrUpdateMenuComplete(
            menuCode: "serial_inbound",
            menuName: "序列号入库",
            i18nKey: "menu.logistics.serials.inbound",
            permCode: "logistics:serials:inbound:list",
            menuType: MenuTypeEnum.Menu,
            parentId: serialsMenu.Id,
            routePath: "Views/Logistics/Serials/SerialInboundView",
            component: "Hbt.Fluent.Views.Logistics.Serials.SerialInboundView",
            icon: "ArrowDownToBracket",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(serialInboundMenu);

        // 6.2 序列号出库 - 使用完整方法创建
        var serialOutboundMenu = CreateOrUpdateMenuComplete(
            menuCode: "serial_outbound",
            menuName: "序列号出库",
            i18nKey: "menu.logistics.serials.outbound",
            permCode: "logistics:serials:outbound:list",
            menuType: MenuTypeEnum.Menu,
            parentId: serialsMenu.Id,
            routePath: "Views/Logistics/Serials/SerialOutboundView",
            component: "Hbt.Fluent.Views.Logistics.Serials.SerialOutboundView",
            icon: "ArrowUpFromBracket",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(serialOutboundMenu);

        // 7. 访客服务（后勤下的二级目录）- 使用完整方法创建
        var visitorsMenu = CreateOrUpdateMenuComplete(
            menuCode: "visitors",
            menuName: "访客服务",
            i18nKey: "menu.logistics.visitors",
            menuType: MenuTypeEnum.Directory,
            parentId: logistics.Id,
            routePath: "Views/Logistics/Visitors/VisitorsPage",
            component: "Hbt.Fluent.Views.Logistics.Visitors.VisitorsPage",
            icon: "IdCard",
            orderNum: 3,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(visitorsMenu);

        // 7.1 访客管理 - 使用完整方法创建
        var visitorMenu = CreateOrUpdateMenuComplete(
            menuCode: "visitor_management",
            menuName: "访客管理",
            i18nKey: "menu.logistics.visitors.management",
            permCode: "logistics:visitors:list",
            menuType: MenuTypeEnum.Menu,
            parentId: visitorsMenu.Id,
            routePath: "Views/Logistics/Visitors/VisitorView",
            component: "Hbt.Fluent.Views.Logistics.Visitors.VisitorView",
            icon: "UserPlus",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(visitorMenu);

        // 7.2 数字标牌 - 使用完整方法创建
        var digitalSignageMenu = CreateOrUpdateMenuComplete(
            menuCode: "digital_signage",
            menuName: "数字标牌",
            i18nKey: "menu.logistics.visitors.signage",
            permCode: "logistics:visitors:signage:list",
            menuType: MenuTypeEnum.Menu,
            parentId: visitorsMenu.Id,
            routePath: "Views/Logistics/Visitors/DigitalSignageView",
            component: "Hbt.Fluent.Views.Logistics.Visitors.DigitalSignageView",
            icon: "Tv",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(digitalSignageMenu);

        // 8. 报表管理（后勤下的二级目录）- 使用完整方法创建
        var reportsMenu = CreateOrUpdateMenuComplete(
            menuCode: "reports",
            menuName: "报表管理",
            i18nKey: "menu.logistics.reports",
            menuType: MenuTypeEnum.Directory,
            parentId: logistics.Id,
            routePath: "Views/Logistics/Reports/ReportsPage",
            component: "Hbt.Fluent.Views.Logistics.Reports.ReportsPage",
            icon: "ChartBar",
            orderNum: 4,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(reportsMenu);

        // 8.1 报表导出 - 使用完整方法创建
        var reportExportMenu = CreateOrUpdateMenuComplete(
            menuCode: "report_export",
            menuName: "报表导出",
            i18nKey: "menu.logistics.reports.export",
            permCode: "logistics:reports:export",
            menuType: MenuTypeEnum.Menu,
            parentId: reportsMenu.Id,
            routePath: "Views/Logistics/Reports/ReportExportView",
            component: "Hbt.Fluent.Views.Logistics.Reports.ReportExportView",
            icon: "FileArrowDown",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(reportExportMenu);

        // 8.2 报表导入 - 使用完整方法创建
        var reportImportMenu = CreateOrUpdateMenuComplete(
            menuCode: "report_import",
            menuName: "报表导入",
            i18nKey: "menu.logistics.reports.import",
            permCode: "logistics:reports:import",
            menuType: MenuTypeEnum.Menu,
            parentId: reportsMenu.Id,
            routePath: "Views/Logistics/Reports/ReportImportView",
            component: "Hbt.Fluent.Views.Logistics.Reports.ReportImportView",
            icon: "FileArrowUp",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(reportImportMenu);

        // ==================== 日常事务子菜单 ====================

        // 8. 本地化 - 使用完整方法创建
        var localizationMenu = CreateOrUpdateMenuComplete(
            menuCode: "localization_management",
            menuName: "本地化",
            i18nKey: "menu.routine.localization",
            permCode: "routine:localization:list",
            menuType: MenuTypeEnum.Menu,
            parentId: routine.Id,
            routePath: "Views/Routine/LocalizationView",
            component: "Hbt.Fluent.Views.Routine.LocalizationView",
            icon: "Language",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(localizationMenu);

        // 9. 字典 - 使用完整方法创建
        var dictionaryMenu = CreateOrUpdateMenuComplete(
            menuCode: "dictionary_management",
            menuName: "字典",
            i18nKey: "menu.routine.dictionary",
            permCode: "routine:dictionary:list",
            menuType: MenuTypeEnum.Menu,
            parentId: routine.Id,
            routePath: "Views/Routine/DictionaryView",
            component: "Hbt.Fluent.Views.Routine.DictionaryView",
            icon: "Database",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(dictionaryMenu);

        // 10. 系统设置 - 使用完整方法创建
        var settingMenu = CreateOrUpdateMenuComplete(
            menuCode: "setting_management",
            menuName: "系统设置",
            i18nKey: "menu.routine.setting",
            permCode: "routine:setting:list",
            menuType: MenuTypeEnum.Menu,
            parentId: routine.Id,
            routePath: "Views/Settings/SettingsView",
            component: "Hbt.Fluent.Views.Settings.SettingsView",
            icon: "Gear",
            orderNum: 3,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(settingMenu);

        // ==================== 身份认证子菜单 ====================

        // 13. 用户管理 - 使用完整方法创建
        var userMenu = CreateOrUpdateMenuComplete(
            menuCode: "user_management",
            menuName: "用户管理",
            i18nKey: "menu.identity.user",
            permCode: "identity:user:list",
            menuType: MenuTypeEnum.Menu,
            parentId: identity.Id,
            routePath: "Views/Identity/UserView",
            component: "Hbt.Fluent.Views.Identity.UserView",
            icon: "User",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(userMenu);

        // 14. 角色管理 - 使用完整方法创建
        var roleMenu = CreateOrUpdateMenuComplete(
            menuCode: "role_management",
            menuName: "角色管理",
            i18nKey: "menu.identity.role",
            permCode: "identity:role:list",
            menuType: MenuTypeEnum.Menu,
            parentId: identity.Id,
            routePath: "Views/Identity/RoleView",
            component: "Hbt.Fluent.Views.Identity.RoleView",
            icon: "Users",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(roleMenu);

        // 15. 菜单管理 - 使用完整方法创建
        var menuMenu = CreateOrUpdateMenuComplete(
            menuCode: "menu_management",
            menuName: "菜单管理",
            i18nKey: "menu.identity.menu",
            permCode: "identity:menu:list",
            menuType: MenuTypeEnum.Menu,
            parentId: identity.Id,
            routePath: "Views/Identity/MenuView",
            component: "Hbt.Fluent.Views.Identity.MenuView",
            icon: "List",
            orderNum: 3,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(menuMenu);

        // ==================== 日志管理子菜单 ====================

        // 16. 登录日志 - 使用完整方法创建
        var loginLogMenu = CreateOrUpdateMenuComplete(
            menuCode: "login_log",
            menuName: "登录日志",
            i18nKey: "menu.logging.login",
            permCode: "logging:login:list",
            menuType: MenuTypeEnum.Menu,
            parentId: logging.Id,
            routePath: "Views/Logging/LoginLogView",
            component: "Hbt.Fluent.Views.Logging.LoginLogView",
            icon: "Key",
            orderNum: 1,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(loginLogMenu);

        // 17. 异常日志 - 使用完整方法创建
        var exceptionLogMenu = CreateOrUpdateMenuComplete(
            menuCode: "exception_log",
            menuName: "异常日志",
            i18nKey: "menu.logging.exception",
            permCode: "logging:exception:list",
            menuType: MenuTypeEnum.Menu,
            parentId: logging.Id,
            routePath: "Views/Logging/ExceptionLogView",
            component: "Hbt.Fluent.Views.Logging.ExceptionLogView",
            icon: "TriangleExclamation",
            orderNum: 2,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(exceptionLogMenu);

        // 18. 操作日志 - 使用完整方法创建
        var operationLogMenu = CreateOrUpdateMenuComplete(
            menuCode: "operation_log",
            menuName: "操作日志",
            i18nKey: "menu.logging.operation",
            permCode: "logging:operation:list",
            menuType: MenuTypeEnum.Menu,
            parentId: logging.Id,
            routePath: "Views/Logging/OperationLogView",
            component: "Hbt.Fluent.Views.Logging.OperationLogView",
            icon: "PenToSquare",
            orderNum: 3,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(operationLogMenu);

        // 19. 差异日志 - 使用完整方法创建
        var diffLogMenu = CreateOrUpdateMenuComplete(
            menuCode: "diff_log",
            menuName: "差异日志",
            i18nKey: "menu.logging.diff",
            permCode: "logging:diff:list",
            menuType: MenuTypeEnum.Menu,
            parentId: logging.Id,
            routePath: "Views/Logging/DiffLogView",
            component: "Hbt.Fluent.Views.Logging.DiffLogView",
            icon: "CodeBranch",
            orderNum: 4,
            isExternal: ExternalEnum.NotExternal,
            isCache: CacheEnum.NoCache,
            isVisible: VisibilityEnum.Visible,
            menuStatus: StatusEnum.Normal
        );
        menus.Add(diffLogMenu);

        // ==================== 为所有管理菜单自动添加通用按钮权限 ====================

        // 收集所有类型为Menu(1)的菜单
        var managementMenus = menus.Where(m => m.MenuType == MenuTypeEnum.Menu && m.ParentId.HasValue).ToList();

        foreach (var menu in managementMenus)
        {
            // 从菜单的 PermCode 提取权限前缀（更可靠的方式）
            // 例如：logistics:material:list -> logistics:material
            //      logistics:report:export -> logistics:report:export (保留完整路径以避免重复)
            if (!string.IsNullOrEmpty(menu.PermCode))
            {
                var permParts = menu.PermCode.Split(':');
                if (permParts.Length >= 2)
                {
                    // 如果是三部分（如：logistics:report:export），使用完整路径避免重复
                    // 如果是两部分（如：logistics:material），使用前两部分
                    string permPrefix;
                    if (permParts.Length >= 3 && permParts[2] != "list")
                    {
                        // 三部分且最后不是 list，说明是特定操作菜单（如 export/import）
                        permPrefix = menu.PermCode.Replace(":list", "").TrimEnd(':');
                    }
                    else
                    {
                        // 标准的列表菜单，取前两部分
                        permPrefix = $"{permParts[0]}:{permParts[1]}";
                    }
                    
                    // 为每个管理菜单创建通用按钮权限
                    var buttons = CreateCommonButtons(menu, permPrefix);
                    menus.AddRange(buttons);
                }
            }
        }

        _initLog.Information("系统菜单创建成功，共创建 {Count} 个菜单", menus.Count);
        return menus;
    }

    /// <summary>
    /// 创建通用按钮权限（使用 BaseRepository 同步方法，存在则更新，不存在则创建）
    /// </summary>
    /// <param name="parentMenu">父级菜单</param>
    /// <param name="permPrefix">权限前缀（如：identity:user）</param>
    /// <returns>创建的按钮菜单列表</returns>
    private List<Menu> CreateCommonButtons(Menu parentMenu, string permPrefix)
    {
        var buttons = new List<Menu>();

        // 从父级菜单的I18nKey提取路径（如：menu.identity.user）
        var parentI18nKey = parentMenu.I18nKey ?? "menu.unknown";
        
        // 通用按钮权限定义（全场景操作）
        var commonButtons = new[]
        {
            // 基础CRUD
            new { Name = "Query", Code = "query", I18nKey = $"{parentI18nKey}.query", PermCode = $"{permPrefix}:query", Order = 1 },
            new { Name = "Read", Code = "read", I18nKey = $"{parentI18nKey}.read", PermCode = $"{permPrefix}:read", Order = 2 },
            new { Name = "Create", Code = "create", I18nKey = $"{parentI18nKey}.create", PermCode = $"{permPrefix}:create", Order = 3 },
            new { Name = "Update", Code = "update", I18nKey = $"{parentI18nKey}.update", PermCode = $"{permPrefix}:update", Order = 4 },
            new { Name = "Delete", Code = "delete", I18nKey = $"{parentI18nKey}.delete", PermCode = $"{permPrefix}:delete", Order = 5 },
            new { Name = "Detail", Code = "detail", I18nKey = $"{parentI18nKey}.detail", PermCode = $"{permPrefix}:detail", Order = 6 },
            
            // 导入导出打印
            new { Name = "Export", Code = "export", I18nKey = $"{parentI18nKey}.export", PermCode = $"{permPrefix}:export", Order = 7 },
            new { Name = "Import", Code = "import", I18nKey = $"{parentI18nKey}.import", PermCode = $"{permPrefix}:import", Order = 8 },
            new { Name = "Print", Code = "print", I18nKey = $"{parentI18nKey}.print", PermCode = $"{permPrefix}:print", Order = 9 },
            new { Name = "Preview", Code = "preview", I18nKey = $"{parentI18nKey}.preview", PermCode = $"{permPrefix}:preview", Order = 10 },
            
            // 状态操作
            new { Name = "Enable", Code = "enable", I18nKey = $"{parentI18nKey}.enable", PermCode = $"{permPrefix}:enable", Order = 11 },
            new { Name = "Disable", Code = "disable", I18nKey = $"{parentI18nKey}.disable", PermCode = $"{permPrefix}:disable", Order = 12 },
            new { Name = "Lock", Code = "lock", I18nKey = $"{parentI18nKey}.lock", PermCode = $"{permPrefix}:lock", Order = 13 },
            new { Name = "Unlock", Code = "unlock", I18nKey = $"{parentI18nKey}.unlock", PermCode = $"{permPrefix}:unlock", Order = 14 },
            
            // 授权操作
            new { Name = "Authorize", Code = "authorize", I18nKey = $"{parentI18nKey}.authorize", PermCode = $"{permPrefix}:authorize", Order = 15 },
            new { Name = "Grant", Code = "grant", I18nKey = $"{parentI18nKey}.grant", PermCode = $"{permPrefix}:grant", Order = 16 },
            new { Name = "Revoke", Code = "revoke", I18nKey = $"{parentI18nKey}.revoke", PermCode = $"{permPrefix}:revoke", Order = 17 },
            
            // 流程操作
            new { Name = "Run", Code = "run", I18nKey = $"{parentI18nKey}.run", PermCode = $"{permPrefix}:run", Order = 18 },
            new { Name = "Start", Code = "start", I18nKey = $"{parentI18nKey}.start", PermCode = $"{permPrefix}:start", Order = 19 },
            new { Name = "Stop", Code = "stop", I18nKey = $"{parentI18nKey}.stop", PermCode = $"{permPrefix}:stop", Order = 20 },
            new { Name = "Pause", Code = "pause", I18nKey = $"{parentI18nKey}.pause", PermCode = $"{permPrefix}:pause", Order = 21 },
            new { Name = "Resume", Code = "resume", I18nKey = $"{parentI18nKey}.resume", PermCode = $"{permPrefix}:resume", Order = 22 },
            new { Name = "Restart", Code = "restart", I18nKey = $"{parentI18nKey}.restart", PermCode = $"{permPrefix}:restart", Order = 23 },
            
            // 审批操作
            new { Name = "Submit", Code = "submit", I18nKey = $"{parentI18nKey}.submit", PermCode = $"{permPrefix}:submit", Order = 24 },
            new { Name = "Approve", Code = "approve", I18nKey = $"{parentI18nKey}.approve", PermCode = $"{permPrefix}:approve", Order = 25 },
            new { Name = "Reject", Code = "reject", I18nKey = $"{parentI18nKey}.reject", PermCode = $"{permPrefix}:reject", Order = 26 },
            new { Name = "Recall", Code = "recall", I18nKey = $"{parentI18nKey}.recall", PermCode = $"{permPrefix}:recall", Order = 27 },
            
            // 发送操作
            new { Name = "Send", Code = "send", I18nKey = $"{parentI18nKey}.send", PermCode = $"{permPrefix}:send", Order = 28 },
            new { Name = "Publish", Code = "publish", I18nKey = $"{parentI18nKey}.publish", PermCode = $"{permPrefix}:publish", Order = 29 },
            new { Name = "Notify", Code = "notify", I18nKey = $"{parentI18nKey}.notify", PermCode = $"{permPrefix}:notify", Order = 30 },
            
            // 文件操作
            new { Name = "Download", Code = "download", I18nKey = $"{parentI18nKey}.download", PermCode = $"{permPrefix}:download", Order = 31 },
            new { Name = "Upload", Code = "upload", I18nKey = $"{parentI18nKey}.upload", PermCode = $"{permPrefix}:upload", Order = 32 },
            new { Name = "Attach", Code = "attach", I18nKey = $"{parentI18nKey}.attach", PermCode = $"{permPrefix}:attach", Order = 33 },
            
            // 互动操作
            new { Name = "Favorite", Code = "favorite", I18nKey = $"{parentI18nKey}.favorite", PermCode = $"{permPrefix}:favorite", Order = 34 },
            new { Name = "Like", Code = "like", I18nKey = $"{parentI18nKey}.like", PermCode = $"{permPrefix}:like", Order = 35 },
            new { Name = "Comment", Code = "comment", I18nKey = $"{parentI18nKey}.comment", PermCode = $"{permPrefix}:comment", Order = 36 },
            new { Name = "Share", Code = "share", I18nKey = $"{parentI18nKey}.share", PermCode = $"{permPrefix}:share", Order = 37 },
            new { Name = "Subscribe", Code = "subscribe", I18nKey = $"{parentI18nKey}.subscribe", PermCode = $"{permPrefix}:subscribe", Order = 38 },
            
            // 其他操作
            new { Name = "Reset", Code = "reset", I18nKey = $"{parentI18nKey}.reset", PermCode = $"{permPrefix}:reset", Order = 39 },
            new { Name = "Copy", Code = "copy", I18nKey = $"{parentI18nKey}.copy", PermCode = $"{permPrefix}:copy", Order = 40 },
            new { Name = "Clone", Code = "clone", I18nKey = $"{parentI18nKey}.clone", PermCode = $"{permPrefix}:clone", Order = 41 },
            new { Name = "Refresh", Code = "refresh", I18nKey = $"{parentI18nKey}.refresh", PermCode = $"{permPrefix}:refresh", Order = 42 },
            new { Name = "Archive", Code = "archive", I18nKey = $"{parentI18nKey}.archive", PermCode = $"{permPrefix}:archive", Order = 43 },
            new { Name = "Restore", Code = "restore", I18nKey = $"{parentI18nKey}.restore", PermCode = $"{permPrefix}:restore", Order = 44 }
        };

        var createdCount = 0;
        var existingCount = 0;
        
        foreach (var btn in commonButtons)
        {
            // ✅ 检查按钮是否存在（通过 PermCode 唯一标识）
            var existingButton = _menuRepository.GetFirst(m => m.PermCode == btn.PermCode && m.ParentId == parentMenu.Id);
            
            if (existingButton == null)
            {
                // 创建新按钮 - 使用完整方法
                var button = CreateOrUpdateMenuComplete(
                    menuCode: $"{permPrefix.Replace(":", "_")}_{btn.Code}",
                    menuName: btn.Name,
                    i18nKey: btn.I18nKey,
                    permCode: btn.PermCode,
                    menuType: MenuTypeEnum.Button,
                    parentId: parentMenu.Id,
                    orderNum: btn.Order,
                    isExternal: ExternalEnum.NotExternal,
                    isCache: CacheEnum.NoCache,
                    isVisible: VisibilityEnum.Invisible, // 按钮默认不可见
                    menuStatus: StatusEnum.Normal
                );
                buttons.Add(button);
                createdCount++;
            }
            else
            {
                // 更新现有按钮 - 使用完整字段设置
                existingButton.MenuName = btn.Name;
                existingButton.MenuCode = $"{permPrefix.Replace(":", "_")}_{btn.Code}";
                existingButton.I18nKey = btn.I18nKey;
                existingButton.PermCode = btn.PermCode;
                existingButton.MenuType = MenuTypeEnum.Button;
                existingButton.ParentId = parentMenu.Id;
                existingButton.OrderNum = btn.Order;
                existingButton.IsExternal = ExternalEnum.NotExternal;
                existingButton.IsCache = CacheEnum.NoCache;
                existingButton.IsVisible = VisibilityEnum.Invisible; // 按钮默认不可见
                existingButton.MenuStatus = StatusEnum.Normal;
                _menuRepository.Update(existingButton, "Hbt365");
                buttons.Add(existingButton);
                existingCount++;
            }
        }

        _initLog.Information("为 {ParentMenu} 处理按钮权限：新增 {Created} 个，更新 {Existing} 个", parentMenu.MenuName, createdCount, existingCount);
        return buttons;
    }

    // 按钮翻译统一在 DbSeedRoutine 中以通用键下发
}

