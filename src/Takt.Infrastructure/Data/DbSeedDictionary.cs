// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedRoutineDictionary.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：字典种子数据初始化
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
/// Routine 模块字典种子初始化器
/// </summary>
public class DbSeedRoutineDictionary
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<DictionaryType> _dictionaryTypeRepository;
    private readonly IBaseRepository<DictionaryData> _dictionaryDataRepository;

    public DbSeedRoutineDictionary(
        InitLogManager initLog,
        IBaseRepository<DictionaryType> dictionaryTypeRepository,
        IBaseRepository<DictionaryData> dictionaryDataRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _dictionaryTypeRepository = dictionaryTypeRepository ?? throw new ArgumentNullException(nameof(dictionaryTypeRepository));
        _dictionaryDataRepository = dictionaryDataRepository ?? throw new ArgumentNullException(nameof(dictionaryDataRepository));
    }

    /// <summary>
    /// 执行字典类型与字典数据的初始化（创建或更新）
    /// </summary>
    public void Run()
    {
        EnsureDictionaryTypes();
        EnsureDictionaryData();
    }

    private void EnsureDictionaryTypes()
    {
        foreach (var seed in BuildDictionaryTypeSeeds())
        {
            var existing = _dictionaryTypeRepository.GetFirst(d => d.TypeCode == seed.TypeCode);

            if (existing == null)
            {
                _dictionaryTypeRepository.Create(seed, "Takt365");
                _initLog.Information($"✅ 创建字典类型：{seed.TypeName}");
            }
            else
            {
                existing.TypeName = seed.TypeName;
                existing.OrderNum = seed.OrderNum;
                existing.IsBuiltin = seed.IsBuiltin;
                existing.TypeStatus = seed.TypeStatus;
                _dictionaryTypeRepository.Update(existing, "Takt365");
            }
        }

        _initLog.Information("✅ 字典类型初始化完成");
    }

    private void EnsureDictionaryData()
    {
        foreach (var seed in BuildDictionaryDataSeeds())
        {
            var existing = _dictionaryDataRepository.GetFirst(d =>
                d.TypeCode == seed.TypeCode && d.DataValue == seed.DataValue);

            if (existing == null)
            {
                _dictionaryDataRepository.Create(seed, "Takt365");
            }
            else
            {
                existing.DataLabel = seed.DataLabel;
                existing.I18nKey = seed.I18nKey;
                existing.DataValue = seed.DataValue;
                existing.CssClass = seed.CssClass;
                existing.ListClass = seed.ListClass;
                existing.OrderNum = seed.OrderNum;
                existing.ExtLabel = seed.ExtLabel;
                existing.ExtValue = seed.ExtValue;
                _dictionaryDataRepository.Update(existing, "Takt365");
            }
        }

        _initLog.Information("✅ 字典数据初始化完成");
    }

    private static List<DictionaryType> BuildDictionaryTypeSeeds()
    {
        return new List<DictionaryType>
        {
            // 系统通用（sys_ 开头，内置不可删除）
            new DictionaryType { TypeCode = "sys_common_gender", TypeName = "性别", DataSource = 0, SqlScript = null!, OrderNum = 1, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_common_yes_no", TypeName = "系统是否", DataSource = 0, SqlScript = null!, OrderNum = 2, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_common_status", TypeName = "状态", DataSource = 0, SqlScript = null!, OrderNum = 3, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_user_type", TypeName = "用户类型", DataSource = 0, SqlScript = null!, OrderNum = 4, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_data_scope", TypeName = "数据权限范围", DataSource = 0, SqlScript = null!, OrderNum = 14, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_ui_css_class", TypeName = "样式类", DataSource = 0, SqlScript = null!, OrderNum = 15, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_dict_data_source", TypeName = "数据源", DataSource = 0, SqlScript = null!, OrderNum = 20, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_dict_is_builtin", TypeName = "是否内置", DataSource = 0, SqlScript = null!, OrderNum = 21, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_module_name", TypeName = "模块", DataSource = 0, SqlScript = null!, OrderNum = 40, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_dest_port", TypeName = "目的地港口", DataSource = 0, SqlScript = null!, OrderNum = 41, IsBuiltin = 0, TypeStatus = 0 },
            
            // 菜单相关（menu_ 开头，业务可删除）
            new DictionaryType { TypeCode = "menu_link_external", TypeName = "是否外链", DataSource = 0, SqlScript = null!, OrderNum = 10, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "menu_cache_enable", TypeName = "是否缓存", DataSource = 0, SqlScript = null!, OrderNum = 11, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "menu_visible_enable", TypeName = "是否可见", DataSource = 0, SqlScript = null!, OrderNum = 12, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "menu_type_category", TypeName = "菜单类型", DataSource = 0, SqlScript = null!, OrderNum = 13, IsBuiltin = 1, TypeStatus = 0 },
            
            // 设置相关（setting_ 开头，业务可删除）
            new DictionaryType { TypeCode = "setting_category_type", TypeName = "设置分类", DataSource = 0, SqlScript = null!, OrderNum = 30, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "setting_value_type", TypeName = "设置类型", DataSource = 0, SqlScript = null!, OrderNum = 31, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "setting_is_default", TypeName = "是否默认", DataSource = 0, SqlScript = null!, OrderNum = 32, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "setting_is_editable", TypeName = "是否可修改", DataSource = 0, SqlScript = null!, OrderNum = 33, IsBuiltin = 1, TypeStatus = 0 },
            
            // 物料相关（material_ 开头，业务可删除）
            new DictionaryType { TypeCode = "material_plant_code", TypeName = "工厂", DataSource = 0, SqlScript = null!, OrderNum = 50, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_industry_field", TypeName = "行业领域", DataSource = 0, SqlScript = null!, OrderNum = 51, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_type_category", TypeName = "物料类型", DataSource = 0, SqlScript = null!, OrderNum = 52, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_base_unit", TypeName = "基本计量单位", DataSource = 0, SqlScript = null!, OrderNum = 53, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_group_code", TypeName = "物料组", DataSource = 0, SqlScript = null!, OrderNum = 54, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_purchase_group", TypeName = "采购组", DataSource = 0, SqlScript = null!, OrderNum = 55, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_purchase_type", TypeName = "采购类型", DataSource = 0, SqlScript = null!, OrderNum = 56, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_special_purchase", TypeName = "特殊采购类", DataSource = 0, SqlScript = null!, OrderNum = 57, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_bulk_type", TypeName = "散装物料", DataSource = 0, SqlScript = null!, OrderNum = 58, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_delivery_time", TypeName = "计划交货时间", DataSource = 0, SqlScript = null!, OrderNum = 59, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_production_days", TypeName = "自制生产天数", DataSource = 0, SqlScript = null!, OrderNum = 60, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_inspection_stock", TypeName = "过账到检验库存", DataSource = 0, SqlScript = null!, OrderNum = 61, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_profit_center", TypeName = "利润中心", DataSource = 0, SqlScript = null!, OrderNum = 62, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_variance_code", TypeName = "差异码", DataSource = 0, SqlScript = null!, OrderNum = 63, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_batch_management", TypeName = "批次管理", DataSource = 0, SqlScript = null!, OrderNum = 64, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_manufacturer", TypeName = "制造商", DataSource = 0, SqlScript = null!, OrderNum = 65, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_evaluation_type", TypeName = "评估类", DataSource = 0, SqlScript = null!, OrderNum = 66, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_currency_code", TypeName = "货币", DataSource = 0, SqlScript = null!, OrderNum = 67, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_price_control", TypeName = "价格控制", DataSource = 0, SqlScript = null!, OrderNum = 68, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_price_unit", TypeName = "价格单位", DataSource = 0, SqlScript = null!, OrderNum = 69, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_storage_location", TypeName = "生产仓储地点", DataSource = 0, SqlScript = null!, OrderNum = 70, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_storage_position", TypeName = "仓位", DataSource = 0, SqlScript = null!, OrderNum = 71, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_cross_plant_status", TypeName = "跨工厂物料状态", DataSource = 0, SqlScript = null!, OrderNum = 72, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_hs_code", TypeName = "HS编码", DataSource = 0, SqlScript = null!, OrderNum = 73, IsBuiltin = 1, TypeStatus = 0 }
        };
    }

    private static List<DictionaryData> BuildDictionaryDataSeeds()
    {
        return new List<DictionaryData>
        {
            // sys_common_gender
            new DictionaryData { TypeCode = "sys_common_gender", DataLabel = "男", DataValue = "male", I18nKey = "dict.sys_common_gender.male", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_common_gender", DataLabel = "女", DataValue = "female", I18nKey = "dict.sys_common_gender.female", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_common_gender", DataLabel = "未知", DataValue = "unknown", I18nKey = "dict.sys_common_gender.unknown", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 3 },

            // sys_common_yes_no
            new DictionaryData { TypeCode = "sys_common_yes_no", DataLabel = "是", DataValue = "yes", I18nKey = "dict.sys_common_yes_no.yes", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_common_yes_no", DataLabel = "否", DataValue = "no", I18nKey = "dict.sys_common_yes_no.no", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },

            // sys_common_status
            new DictionaryData { TypeCode = "sys_common_status", DataLabel = "正常", DataValue = "normal", I18nKey = "dict.sys_common_status.normal", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_common_status", DataLabel = "禁用", DataValue = "disabled", I18nKey = "dict.sys_common_status.disabled", ExtLabel = null!, ExtValue = null!, CssClass = "danger", ListClass = "danger", OrderNum = 1 },

            // sys_user_type
            new DictionaryData { TypeCode = "sys_user_type", DataLabel = "系统用户", DataValue = "Takt365", I18nKey = "dict.sys_user_type.Takt365", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_user_type", DataLabel = "普通用户", DataValue = "normal", I18nKey = "dict.sys_user_type.normal", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            
            // sys_data_scope
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "全部数据", DataValue = "1", I18nKey = "dict.sys_data_scope.all", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "自定义数据", DataValue = "2", I18nKey = "dict.sys_data_scope.custom", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "本部门数据", DataValue = "3", I18nKey = "dict.sys_data_scope.department", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "本部门及以下数据", DataValue = "4", I18nKey = "dict.sys_data_scope.department_below", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "仅本人数据", DataValue = "5", I18nKey = "dict.sys_data_scope.self", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // sys_dict_data_source
            new DictionaryData { TypeCode = "sys_dict_data_source", DataLabel = "系统", DataValue = "0", I18nKey = "dict.sys_dict_data_source.system", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_dict_data_source", DataLabel = "SQL脚本", DataValue = "1", I18nKey = "dict.sys_dict_data_source.sql_script", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 1 },
            
            // sys_dict_is_builtin
            new DictionaryData { TypeCode = "sys_dict_is_builtin", DataLabel = "是", DataValue = "0", I18nKey = "dict.sys_dict_is_builtin.yes", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_dict_is_builtin", DataLabel = "否", DataValue = "1", I18nKey = "dict.sys_dict_is_builtin.no", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // menu_link_external
            new DictionaryData { TypeCode = "menu_link_external", DataLabel = "外链", DataValue = "0", I18nKey = "dict.menu_link_external.external", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_link_external", DataLabel = "不是外链", DataValue = "1", I18nKey = "dict.menu_link_external.not_external", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // menu_cache_enable
            new DictionaryData { TypeCode = "menu_cache_enable", DataLabel = "缓存", DataValue = "0", I18nKey = "dict.menu_cache_enable.cache", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_cache_enable", DataLabel = "不缓存", DataValue = "1", I18nKey = "dict.menu_cache_enable.no_cache", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // menu_visible_enable
            new DictionaryData { TypeCode = "menu_visible_enable", DataLabel = "可见", DataValue = "0", I18nKey = "dict.menu_visible_enable.visible", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_visible_enable", DataLabel = "不可见", DataValue = "1", I18nKey = "dict.menu_visible_enable.invisible", ExtLabel = null!, ExtValue = null!, CssClass = "danger", ListClass = "danger", OrderNum = 1 },
            
            // menu_type_category
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "目录", DataValue = "0", I18nKey = "dict.menu_type_category.directory", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "菜单", DataValue = "1", I18nKey = "dict.menu_type_category.menu", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 1 },
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "按钮", DataValue = "2", I18nKey = "dict.menu_type_category.button", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 2 },
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "API", DataValue = "3", I18nKey = "dict.menu_type_category.api", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 3 },
            
            // setting_value_type
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "字符串", DataValue = "0", I18nKey = "dict.setting_value_type.string", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 0 },
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "数字", DataValue = "1", I18nKey = "dict.setting_value_type.number", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 1 },
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "布尔值", DataValue = "2", I18nKey = "dict.setting_value_type.boolean", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 2 },
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "JSON", DataValue = "3", I18nKey = "dict.setting_value_type.json", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            
            // setting_is_default
            new DictionaryData { TypeCode = "setting_is_default", DataLabel = "是", DataValue = "0", I18nKey = "dict.setting_is_default.yes", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "setting_is_default", DataLabel = "否", DataValue = "1", I18nKey = "dict.setting_is_default.no", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // setting_is_editable
            new DictionaryData { TypeCode = "setting_is_editable", DataLabel = "是", DataValue = "0", I18nKey = "dict.setting_is_editable.yes", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "setting_is_editable", DataLabel = "否", DataValue = "1", I18nKey = "dict.setting_is_editable.no", ExtLabel = null!, ExtValue = null!, CssClass = "danger", ListClass = "danger", OrderNum = 1 },
            
            // sys_ui_css_class
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "主要", DataValue = "primary", I18nKey = "dict.sys_ui_css_class.primary", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "成功", DataValue = "success", I18nKey = "dict.sys_ui_css_class.success", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "信息", DataValue = "info", I18nKey = "dict.sys_ui_css_class.info", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 3 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "警告", DataValue = "warning", I18nKey = "dict.sys_ui_css_class.warning", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "危险", DataValue = "danger", I18nKey = "dict.sys_ui_css_class.danger", ExtLabel = null!, ExtValue = null!, CssClass = "danger", ListClass = "danger", OrderNum = 5 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "次要", DataValue = "secondary", I18nKey = "dict.sys_ui_css_class.secondary", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 6 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "浅色", DataValue = "light", I18nKey = "dict.sys_ui_css_class.light", ExtLabel = null!, ExtValue = null!, CssClass = "light", ListClass = "light", OrderNum = 7 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "深色", DataValue = "dark", I18nKey = "dict.sys_ui_css_class.dark", ExtLabel = null!, ExtValue = null!, CssClass = "dark", ListClass = "dark", OrderNum = 8 },
            
            // sys_module_name
            new DictionaryData { TypeCode = "sys_module_name", DataLabel = "前端", DataValue = "Frontend", I18nKey = "dict.sys_module_name.Frontend", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_module_name", DataLabel = "后端", DataValue = "Backend", I18nKey = "dict.sys_module_name.Backend", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_module_name", DataLabel = "移动端", DataValue = "Mobile", I18nKey = "dict.sys_module_name.Mobile", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // setting_category_type
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "系统设置", DataValue = "system", I18nKey = "dict.setting_category_type.system", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "用户设置", DataValue = "user", I18nKey = "dict.setting_category_type.user", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "外观设置", DataValue = "appearance", I18nKey = "dict.setting_category_type.appearance", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "安全设置", DataValue = "security", I18nKey = "dict.setting_category_type.security", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "通知设置", DataValue = "notification", I18nKey = "dict.setting_category_type.notification", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_plant_code工厂字典：C100-》DTA,H100-》TAC,1000-》TCJ
            new DictionaryData { TypeCode = "material_plant_code", DataLabel = "DTA", DataValue = "C100", I18nKey = "dict.material_plant_code.C100", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_plant_code", DataLabel = "TAC", DataValue = "H100", I18nKey = "dict.material_plant_code.H100", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_plant_code", DataLabel = "TCJ", DataValue = "1000", I18nKey = "dict.material_plant_code.1000", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_industry_field
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "机械制造", DataValue = "M", I18nKey = "dict.material_industry_field.M", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "电子制造", DataValue = "E", I18nKey = "dict.material_industry_field.E", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "化工", DataValue = "C", I18nKey = "dict.material_industry_field.C", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "食品", DataValue = "F", I18nKey = "dict.material_industry_field.F", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 4 },
            
            // material_type_category
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "成品", DataValue = "FERT", I18nKey = "dict.material_type_category.FERT", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "半成品", DataValue = "HALB", I18nKey = "dict.material_type_category.HALB", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "原材料", DataValue = "ROH", I18nKey = "dict.material_type_category.ROH", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "贸易商品", DataValue = "HIBE", I18nKey = "dict.material_type_category.HIBE", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "非库存物料", DataValue = "NLAG", I18nKey = "dict.material_type_category.NLAG", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_base_unit
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "件", DataValue = "PC", I18nKey = "dict.material_base_unit.PC", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "克", DataValue = "G", I18nKey = "dict.material_base_unit.G", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "千克", DataValue = "KG", I18nKey = "dict.material_base_unit.KG", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "米", DataValue = "M", I18nKey = "dict.material_base_unit.M", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "千米", DataValue = "KM", I18nKey = "dict.material_base_unit.KM", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 5 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "平方米", DataValue = "M2", I18nKey = "dict.material_base_unit.M2", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 6 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "立方米", DataValue = "M3", I18nKey = "dict.material_base_unit.M3", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 7 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "毫升", DataValue = "ML", I18nKey = "dict.material_base_unit.ML", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 8 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "升", DataValue = "L", I18nKey = "dict.material_base_unit.L", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 9 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "箱", DataValue = "BOX", I18nKey = "dict.material_base_unit.BOX", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 10 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "套", DataValue = "SET", I18nKey = "dict.material_base_unit.SET", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 11 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "个", DataValue = "EA", I18nKey = "dict.material_base_unit.EA", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 12 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "罐", DataValue = "CAN", I18nKey = "dict.material_base_unit.CAN", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 13 },
            
            // material_group_code物料组，应该根据材料的性质还区别
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "SW RGLTD PS", DataValue = "FDB", I18nKey = "dict.material_group_code.FDB", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "SER RGLTD PS", DataValue = "FDC", I18nKey = "dict.material_group_code.FDC", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "DC/DC CONV", DataValue = "FDD", I18nKey = "dict.material_group_code.FDD", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "AC ADAPTER", DataValue = "FDE", I18nKey = "dict.material_group_code.FDE", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "BAT CHG", DataValue = "FDF", I18nKey = "dict.material_group_code.FDF", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "CAR ADAPTER", DataValue = "FDG", I18nKey = "dict.material_group_code.FDG", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 6 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "NON BREAK PS", DataValue = "FDH", I18nKey = "dict.material_group_code.FDH", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 7 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "FDZ", DataValue = "FDZ", I18nKey = "dict.material_group_code.FDZ", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 8 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "CRT DISPLAY", DataValue = "FEC", I18nKey = "dict.material_group_code.FEC", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 9 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "EL DISPLAY", DataValue = "FEE", I18nKey = "dict.material_group_code.FEE", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 10 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "FIL DISPLAY", DataValue = "FEF", I18nKey = "dict.material_group_code.FEF", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 11 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "ELCTRN RAY DSPL", DataValue = "FEG", I18nKey = "dict.material_group_code.FEG", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 12 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "LED DISPLAY", DataValue = "FEH", I18nKey = "dict.material_group_code.FEH", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 13 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "LCD", DataValue = "FEL", I18nKey = "dict.material_group_code.FEL", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 14 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "NEON SEG DSPL", DataValue = "FEN", I18nKey = "dict.material_group_code.FEN", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 15 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "PLASMA DISPLAY", DataValue = "FEP", I18nKey = "dict.material_group_code.FEP", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 16 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "FEZ", DataValue = "FEZ", I18nKey = "dict.material_group_code.FEZ", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 17 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "VIDEO CAMERA", DataValue = "FFC", I18nKey = "dict.material_group_code.FFC", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 18 },
            
            // material_purchase_group
            new DictionaryData { TypeCode = "material_purchase_group", DataLabel = "采购组001", DataValue = "001", I18nKey = "dict.material_purchase_group.001", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_purchase_group", DataLabel = "采购组002", DataValue = "002", I18nKey = "dict.material_purchase_group.002", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_purchase_group", DataLabel = "采购组003", DataValue = "003", I18nKey = "dict.material_purchase_group.003", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_purchase_type
            new DictionaryData { TypeCode = "material_purchase_type", DataLabel = "外部采购", DataValue = "E", I18nKey = "dict.material_purchase_type.E", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_purchase_type", DataLabel = "自制", DataValue = "F", I18nKey = "dict.material_purchase_type.F", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_purchase_type", DataLabel = "两者皆可", DataValue = "X", I18nKey = "dict.material_purchase_type.X", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 3 },
            
            // material_special_purchase
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "标准采购", DataValue = "10", I18nKey = "dict.material_special_purchase.10", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "寄售", DataValue = "20", I18nKey = "dict.material_special_purchase.20", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "分包", DataValue = "30", I18nKey = "dict.material_special_purchase.30", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "第三方", DataValue = "40", I18nKey = "dict.material_special_purchase.40", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 4 },
            
            // material_bulk_type
            new DictionaryData { TypeCode = "material_bulk_type", DataLabel = "散装物料", DataValue = "X", I18nKey = "dict.material_bulk_type.X", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_bulk_type", DataLabel = "非散装物料", DataValue = " ", I18nKey = "dict.material_bulk_type.space", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },
            
            // material_inspection_stock
            new DictionaryData { TypeCode = "material_inspection_stock", DataLabel = "过账到检验库存", DataValue = "X", I18nKey = "dict.material_inspection_stock.X", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_inspection_stock", DataLabel = "不过账到检验库存", DataValue = " ", I18nKey = "dict.material_inspection_stock.space", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },
            
            // material_profit_center，HI,PR,MI,PRO,BS这是对的修改顺序
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "HI", DataValue = "2U10", I18nKey = "dict.material_profit_center.2U10", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "PRE", DataValue = "2U20", I18nKey = "dict.material_profit_center.2U20", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "MI", DataValue = "3U10", I18nKey = "dict.material_profit_center.3U10", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "PRO", DataValue = "3U20", I18nKey = "dict.material_profit_center.3U20", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "ODM", DataValue = "4U10", I18nKey = "dict.material_profit_center.4U10", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "EMS", DataValue = "4U20", I18nKey = "dict.material_profit_center.4U20", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 6 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "BS", DataValue = "4U30", I18nKey = "dict.material_profit_center.4U30", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 7 },
            
            // material_batch_management
            new DictionaryData { TypeCode = "material_batch_management", DataLabel = "批次管理", DataValue = "X", I18nKey = "dict.material_batch_management.X", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_batch_management", DataLabel = "非批次管理", DataValue = " ", I18nKey = "dict.material_batch_management.space", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },
            
            // material_evaluation_type正确的是Z300为材料，Z790为成品,Z791为半成品
            new DictionaryData { TypeCode = "material_evaluation_type", DataLabel = "材料", DataValue = "Z300", I18nKey = "dict.material_evaluation_type.Z300", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_evaluation_type", DataLabel = "成品", DataValue = "Z790", I18nKey = "dict.material_evaluation_type.Z790", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_evaluation_type", DataLabel = "半成品", DataValue = "Z791", I18nKey = "dict.material_evaluation_type.Z791", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_currency_code
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "人民币", DataValue = "CNY", I18nKey = "dict.material_currency_code.CNY", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "美元", DataValue = "USD", I18nKey = "dict.material_currency_code.USD", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "欧元", DataValue = "EUR", I18nKey = "dict.material_currency_code.EUR", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "日元", DataValue = "JPY", I18nKey = "dict.material_currency_code.JPY", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "港币", DataValue = "HKD", I18nKey = "dict.material_currency_code.HKD", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_price_control
            new DictionaryData { TypeCode = "material_price_control", DataLabel = "移动平均", DataValue = "V", I18nKey = "dict.material_price_control.V", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_price_control", DataLabel = "标准价格", DataValue = "S", I18nKey = "dict.material_price_control.S", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            
            // material_cross_plant_status，01調達/倉庫に対しブロック中,02 Task List/BOMのブロック中,Z0計画品目,ZM現法在庫に確認必要,ZP製造中止,ZQ生産終了（製品）,ZW PC MRP対象外,ZX PC 仲介専用品,ZY PC DISCON(MRP対象外),ZZ PC 代替品目あり
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "对采购/仓库已锁定", DataValue = "01", I18nKey = "dict.material_cross_plant_status.01", ExtLabel = null!, ExtValue = null!, CssClass = "danger", ListClass = "danger", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "Task List/BOM已锁定", DataValue = "02", I18nKey = "dict.material_cross_plant_status.02", ExtLabel = null!, ExtValue = null!, CssClass = "danger", ListClass = "danger", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "计划品目", DataValue = "Z0", I18nKey = "dict.material_cross_plant_status.Z0", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "需要确认当前法人在库", DataValue = "ZM", I18nKey = "dict.material_cross_plant_status.ZM", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "制造中止", DataValue = "ZP", I18nKey = "dict.material_cross_plant_status.ZP", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "生产结束（产品）", DataValue = "ZQ", I18nKey = "dict.material_cross_plant_status.ZQ", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 6 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "PC MRP对象外", DataValue = "ZW", I18nKey = "dict.material_cross_plant_status.ZW", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 7 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "PC 中介专用品", DataValue = "ZX", I18nKey = "dict.material_cross_plant_status.ZX", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 8 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "PC DISCON(MRP对象外)", DataValue = "ZY", I18nKey = "dict.material_cross_plant_status.ZY", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 9 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "PC 有替代品目", DataValue = "ZZ", I18nKey = "dict.material_cross_plant_status.ZZ", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 10 },
            
            // material_variance_code只有一个选项000001和空
            new DictionaryData { TypeCode = "material_variance_code", DataLabel = "000001", DataValue = "000001", I18nKey = "dict.material_variance_code.000001", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_variance_code", DataLabel = "空", DataValue = "", I18nKey = "dict.material_variance_code.empty", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },
            
            // material_manufacturer，电容、机芯、存储、电源等知名制造商
            // 电容制造商
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "村田", DataValue = "MURATA", I18nKey = "dict.material_manufacturer.MURATA", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "TDK", DataValue = "TDK", I18nKey = "dict.material_manufacturer.TDK", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "太阳诱电", DataValue = "TAIYO", I18nKey = "dict.material_manufacturer.TAIYO", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "京瓷", DataValue = "KYOCERA", I18nKey = "dict.material_manufacturer.KYOCERA", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "AVX", DataValue = "AVX", I18nKey = "dict.material_manufacturer.AVX", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 5 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "KEMET", DataValue = "KEMET", I18nKey = "dict.material_manufacturer.KEMET", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 6 },
            // 机芯制造商
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "精工", DataValue = "SEIKO", I18nKey = "dict.material_manufacturer.SEIKO", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 7 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "西铁城", DataValue = "CITIZEN", I18nKey = "dict.material_manufacturer.CITIZEN", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 8 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "爱普生", DataValue = "EPSON", I18nKey = "dict.material_manufacturer.EPSON", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 9 },
            // 存储制造商
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "三星", DataValue = "SAMSUNG", I18nKey = "dict.material_manufacturer.SAMSUNG", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 10 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "SK海力士", DataValue = "SKHYNIX", I18nKey = "dict.material_manufacturer.SKHYNIX", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 11 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "美光", DataValue = "MICRON", I18nKey = "dict.material_manufacturer.MICRON", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 12 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "东芝", DataValue = "TOSHIBA", I18nKey = "dict.material_manufacturer.TOSHIBA", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 13 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "西部数据", DataValue = "WD", I18nKey = "dict.material_manufacturer.WD", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 14 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "希捷", DataValue = "SEAGATE", I18nKey = "dict.material_manufacturer.SEAGATE", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 15 },
            // 电源制造商
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "台达", DataValue = "DELTA", I18nKey = "dict.material_manufacturer.DELTA", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 16 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "光宝", DataValue = "LITEON", I18nKey = "dict.material_manufacturer.LITEON", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 17 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "康舒", DataValue = "ACBEL", I18nKey = "dict.material_manufacturer.ACBEL", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 18 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "明纬", DataValue = "MEANWELL", I18nKey = "dict.material_manufacturer.MEANWELL", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 19 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "艾默生", DataValue = "EMERSON", I18nKey = "dict.material_manufacturer.EMERSON", ExtLabel = null!, ExtValue = null!, CssClass = "secondary", ListClass = "secondary", OrderNum = 20 },
            
            // material_storage_location，C001为机构材料，C002为电子材料，C003为辅材
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "机构材料", DataValue = "C001", I18nKey = "dict.material_storage_location.C001", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "电子材料", DataValue = "C002", I18nKey = "dict.material_storage_location.C002", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "辅材", DataValue = "C003", I18nKey = "dict.material_storage_location.C003", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_storage_position
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "A区-01排-01位", DataValue = "A-01-01", I18nKey = "dict.material_storage_position.A_01_01", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "A区-01排-02位", DataValue = "A-01-02", I18nKey = "dict.material_storage_position.A_01_02", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "B区-01排-01位", DataValue = "B-01-01", I18nKey = "dict.material_storage_position.B_01_01", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "B区-01排-02位", DataValue = "B-01-02", I18nKey = "dict.material_storage_position.B_01_02", ExtLabel = null!, ExtValue = null!, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            
            // sys_dest_port
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "安恒利_空运", DataValue = "ACE_AIR", I18nKey = "dict.sys_dest_port.ACE_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "安恒利_船运", DataValue = "ACE_BOAT", I18nKey = "dict.sys_dest_port.ACE_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "安恒利_卡车", DataValue = "ACE_TRUCK", I18nKey = "dict.sys_dest_port.ACE_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "ALPS_空运", DataValue = "ALPS_AIR", I18nKey = "dict.sys_dest_port.ALPS_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 4 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "ALPS_船运", DataValue = "ALPS_BOAT", I18nKey = "dict.sys_dest_port.ALPS_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 5 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "ALPS_卡车", DataValue = "ALPS_TRUCK", I18nKey = "dict.sys_dest_port.ALPS_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 6 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "雅士_空运", DataValue = "ARTS_AIR", I18nKey = "dict.sys_dest_port.ARTS_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 7 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "雅士_船运", DataValue = "ARTS_BOAT", I18nKey = "dict.sys_dest_port.ARTS_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 8 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "雅士_卡车", DataValue = "ARTS_TRUCK", I18nKey = "dict.sys_dest_port.ARTS_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 9 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "北京_空运", DataValue = "BEIJING_AIR", I18nKey = "dict.sys_dest_port.BEIJING_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 10 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "北京_船运", DataValue = "BEIJING_BOAT", I18nKey = "dict.sys_dest_port.BEIJING_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 11 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "北京_卡车", DataValue = "BEIJING_TRUCK", I18nKey = "dict.sys_dest_port.BEIJING_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 12 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "加拿大_空运", DataValue = "CANADA_AIR", I18nKey = "dict.sys_dest_port.CANADA_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 13 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "加拿大_船运", DataValue = "CANADA_BOAT", I18nKey = "dict.sys_dest_port.CANADA_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 14 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "加拿大_卡车", DataValue = "CANADA_TRUCK", I18nKey = "dict.sys_dest_port.CANADA_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 15 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "悦昌_空运", DataValue = "DCHAV_AIR", I18nKey = "dict.sys_dest_port.DCHAV_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 16 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "悦昌_船运", DataValue = "DCHAV_BOAT", I18nKey = "dict.sys_dest_port.DCHAV_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 17 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "悦昌_卡车", DataValue = "DCHAV_TRUCK", I18nKey = "dict.sys_dest_port.DCHAV_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 18 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "DTA_空运", DataValue = "DTA_AIR", I18nKey = "dict.sys_dest_port.DTA_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 19 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "DTA_船运", DataValue = "DTA_BOAT", I18nKey = "dict.sys_dest_port.DTA_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 20 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "DTA_卡车", DataValue = "DTA_TRUCK", I18nKey = "dict.sys_dest_port.DTA_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 21 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "德国_空运", DataValue = "GERMANY_AIR", I18nKey = "dict.sys_dest_port.GERMANY_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 22 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "德国_船运", DataValue = "GERMANY_BOAT", I18nKey = "dict.sys_dest_port.GERMANY_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 23 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "德国_卡车", DataValue = "GERMANY_TRUCK", I18nKey = "dict.sys_dest_port.GERMANY_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 24 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "广州_空运", DataValue = "GUANGZHOU_AIR", I18nKey = "dict.sys_dest_port.GUANGZHOU_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 25 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "广州_船运", DataValue = "GUANGZHOU_BOAT", I18nKey = "dict.sys_dest_port.GUANGZHOU_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 26 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "广州_卡车", DataValue = "GUANGZHOU_TRUCK", I18nKey = "dict.sys_dest_port.GUANGZHOU_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 27 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "长城_空运", DataValue = "GW_AIR", I18nKey = "dict.sys_dest_port.GW_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 28 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "长城_船运", DataValue = "GW_BOAT", I18nKey = "dict.sys_dest_port.GW_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 29 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "长城_卡车", DataValue = "GW_TRUCK", I18nKey = "dict.sys_dest_port.GW_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 30 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "荷兰_空运", DataValue = "HOLLAND_AIR", I18nKey = "dict.sys_dest_port.HOLLAND_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 31 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "荷兰_船运", DataValue = "HOLLAND_BOAT", I18nKey = "dict.sys_dest_port.HOLLAND_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 32 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "荷兰_卡车", DataValue = "HOLLAND_TRUCK", I18nKey = "dict.sys_dest_port.HOLLAND_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 33 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "香港_空运", DataValue = "HONGKONG_AIR", I18nKey = "dict.sys_dest_port.HONGKONG_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 34 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "香港_船运", DataValue = "HONGKONG_BOAT", I18nKey = "dict.sys_dest_port.HONGKONG_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 35 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "香港_卡车", DataValue = "HONGKONG_TRUCK", I18nKey = "dict.sys_dest_port.HONGKONG_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 36 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "意大利_空运", DataValue = "ITALY_AIR", I18nKey = "dict.sys_dest_port.ITALY_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 37 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "意大利_船运", DataValue = "ITALY_BOAT", I18nKey = "dict.sys_dest_port.ITALY_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 38 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "意大利_卡车", DataValue = "ITALY_TRUCK", I18nKey = "dict.sys_dest_port.ITALY_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 39 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "日本_空运", DataValue = "JAPAN_AIR", I18nKey = "dict.sys_dest_port.JAPAN_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 40 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "日本_船运", DataValue = "JAPAN_BOAT", I18nKey = "dict.sys_dest_port.JAPAN_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 41 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "日本_卡车", DataValue = "JAPAN_TRUCK", I18nKey = "dict.sys_dest_port.JAPAN_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 42 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "马来西亚_空运", DataValue = "MALAYSIA_AIR", I18nKey = "dict.sys_dest_port.MALAYSIA_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 43 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "马来西亚_船运", DataValue = "MALAYSIA_BOAT", I18nKey = "dict.sys_dest_port.MALAYSIA_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 44 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "马来西亚_卡车", DataValue = "MALAYSIA_TRUCK", I18nKey = "dict.sys_dest_port.MALAYSIA_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 45 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "无印良品_空运", DataValue = "MUJI_AIR", I18nKey = "dict.sys_dest_port.MUJI_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 46 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "无印良品_船运", DataValue = "MUJI_BOAT", I18nKey = "dict.sys_dest_port.MUJI_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 47 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "无印良品_卡车", DataValue = "MUJI_TRUCK", I18nKey = "dict.sys_dest_port.MUJI_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 48 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "长秦城_空运", DataValue = "MUSICGW_AIR", I18nKey = "dict.sys_dest_port.MUSICGW_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 49 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "长秦城_船运", DataValue = "MUSICGW_BOAT", I18nKey = "dict.sys_dest_port.MUSICGW_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 50 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "长秦城_卡车", DataValue = "MUSICGW_TRUCK", I18nKey = "dict.sys_dest_port.MUSICGW_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 51 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "南沙_空运", DataValue = "NAISHA_AIR", I18nKey = "dict.sys_dest_port.NAISHA_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 52 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "南沙_船运", DataValue = "NAISHA_BOAT", I18nKey = "dict.sys_dest_port.NAISHA_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 53 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "南沙_卡车", DataValue = "NAISHA_TRUCK", I18nKey = "dict.sys_dest_port.NAISHA_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 54 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "其它_空运", DataValue = "OTHER_AIR", I18nKey = "dict.sys_dest_port.OTHER_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 55 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "其它_船运", DataValue = "OTHER_BOAT", I18nKey = "dict.sys_dest_port.OTHER_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 56 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "其它_卡车", DataValue = "OTHER_TRUCK", I18nKey = "dict.sys_dest_port.OTHER_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 57 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "鹿特丹_空运", DataValue = "ROTTERDAM_AIR", I18nKey = "dict.sys_dest_port.ROTTERDAM_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 58 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "鹿特丹_船运", DataValue = "ROTTERDAM_BOAT", I18nKey = "dict.sys_dest_port.ROTTERDAM_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 59 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "鹿特丹_卡车", DataValue = "ROTTERDAM_TRUCK", I18nKey = "dict.sys_dest_port.ROTTERDAM_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 60 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "上海_空运", DataValue = "SHANGHAI_AIR", I18nKey = "dict.sys_dest_port.SHANGHAI_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 61 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "上海_船运", DataValue = "SHANGHAI_BOAT", I18nKey = "dict.sys_dest_port.SHANGHAI_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 62 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "上海_卡车", DataValue = "SHANGHAI_TRUCK", I18nKey = "dict.sys_dest_port.SHANGHAI_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 63 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "深圳_空运", DataValue = "SHENGZHEN_AIR", I18nKey = "dict.sys_dest_port.SHENGZHEN_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 64 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "深圳_船运", DataValue = "SHENGZHEN_BOAT", I18nKey = "dict.sys_dest_port.SHENGZHEN_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 65 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "深圳_卡车", DataValue = "SHENGZHEN_TRUCK", I18nKey = "dict.sys_dest_port.SHENGZHEN_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 66 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TAC_空运", DataValue = "TAC_AIR", I18nKey = "dict.sys_dest_port.TAC_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 67 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TAC_船运", DataValue = "TAC_BOAT", I18nKey = "dict.sys_dest_port.TAC_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 68 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TAC_卡车", DataValue = "TAC_TRUCK", I18nKey = "dict.sys_dest_port.TAC_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 69 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TCA_空运", DataValue = "TCA_AIR", I18nKey = "dict.sys_dest_port.TCA_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 70 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TCA_船运", DataValue = "TCA_BOAT", I18nKey = "dict.sys_dest_port.TCA_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 71 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TCA_卡车", DataValue = "TCA_TRUCK", I18nKey = "dict.sys_dest_port.TCA_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 72 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TOA_空运", DataValue = "TOA_AIR", I18nKey = "dict.sys_dest_port.TOA_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 73 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TOA_船运", DataValue = "TOA_BOAT", I18nKey = "dict.sys_dest_port.TOA_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 74 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "TOA_卡车", DataValue = "TOA_TRUCK", I18nKey = "dict.sys_dest_port.TOA_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 75 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "美国_空运", DataValue = "USA_AIR", I18nKey = "dict.sys_dest_port.USA_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 76 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "美国_船运", DataValue = "USA_BOAT", I18nKey = "dict.sys_dest_port.USA_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 77 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "美国_卡车", DataValue = "USA_TRUCK", I18nKey = "dict.sys_dest_port.USA_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 78 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "无锡_空运", DataValue = "WUXI_AIR", I18nKey = "dict.sys_dest_port.WUXI_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 79 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "无锡_船运", DataValue = "WUXI_BOAT", I18nKey = "dict.sys_dest_port.WUXI_BOAT", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 80 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "无锡_卡车", DataValue = "WUXI_TRUCK", I18nKey = "dict.sys_dest_port.WUXI_TRUCK", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 81 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "韩国－空运", DataValue = "INCHEON_AIR", I18nKey = "dict.sys_dest_port.INCHEON_AIR", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 82 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "越南-卡车", DataValue = "VIE", I18nKey = "dict.sys_dest_port.VIE", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 83 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "法兰克福", DataValue = "FRANKFURT", I18nKey = "dict.sys_dest_port.FRANKFURT", ExtLabel = null!, ExtValue = null!, CssClass = "success", ListClass = "success", OrderNum = 84 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "斯洛文尼亚-船运", DataValue = "SLOVENIA", I18nKey = "dict.sys_dest_port.SLOVENIA", ExtLabel = null!, ExtValue = null!, CssClass = "primary", ListClass = "primary", OrderNum = 85 },
            new DictionaryData { TypeCode = "sys_dest_port", DataLabel = "安达斯", DataValue = "ANDAS", I18nKey = "dict.sys_dest_port.ANDAS", ExtLabel = null!, ExtValue = null!, CssClass = "info", ListClass = "info", OrderNum = 86 }
        };
    }
}

