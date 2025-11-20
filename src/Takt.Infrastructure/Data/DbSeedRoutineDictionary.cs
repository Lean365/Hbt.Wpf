// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
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
            new DictionaryType { TypeCode = "sys_common_gender", TypeName = "性别", DataSource = 0, SqlScript = null, OrderNum = 1, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_common_yes_no", TypeName = "系统是否", DataSource = 0, SqlScript = null, OrderNum = 2, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_common_status", TypeName = "状态", DataSource = 0, SqlScript = null, OrderNum = 3, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_user_type", TypeName = "用户类型", DataSource = 0, SqlScript = null, OrderNum = 4, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_data_scope", TypeName = "数据权限范围", DataSource = 0, SqlScript = null, OrderNum = 14, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_ui_css_class", TypeName = "样式类", DataSource = 0, SqlScript = null, OrderNum = 15, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_dict_data_source", TypeName = "数据源", DataSource = 0, SqlScript = null, OrderNum = 20, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_dict_is_builtin", TypeName = "是否内置", DataSource = 0, SqlScript = null, OrderNum = 21, IsBuiltin = 0, TypeStatus = 0 },
            new DictionaryType { TypeCode = "sys_module_name", TypeName = "模块", DataSource = 0, SqlScript = null, OrderNum = 40, IsBuiltin = 0, TypeStatus = 0 },
            
            // 菜单相关（menu_ 开头，业务可删除）
            new DictionaryType { TypeCode = "menu_link_external", TypeName = "是否外链", DataSource = 0, SqlScript = null, OrderNum = 10, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "menu_cache_enable", TypeName = "是否缓存", DataSource = 0, SqlScript = null, OrderNum = 11, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "menu_visible_enable", TypeName = "是否可见", DataSource = 0, SqlScript = null, OrderNum = 12, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "menu_type_category", TypeName = "菜单类型", DataSource = 0, SqlScript = null, OrderNum = 13, IsBuiltin = 1, TypeStatus = 0 },
            
            // 设置相关（setting_ 开头，业务可删除）
            new DictionaryType { TypeCode = "setting_category_type", TypeName = "设置分类", DataSource = 0, SqlScript = null, OrderNum = 30, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "setting_value_type", TypeName = "设置类型", DataSource = 0, SqlScript = null, OrderNum = 31, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "setting_is_default", TypeName = "是否默认", DataSource = 0, SqlScript = null, OrderNum = 32, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "setting_is_editable", TypeName = "是否可修改", DataSource = 0, SqlScript = null, OrderNum = 33, IsBuiltin = 1, TypeStatus = 0 },
            
            // 物料相关（material_ 开头，业务可删除）
            new DictionaryType { TypeCode = "material_plant_code", TypeName = "工厂", DataSource = 0, SqlScript = null, OrderNum = 50, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_industry_field", TypeName = "行业领域", DataSource = 0, SqlScript = null, OrderNum = 51, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_type_category", TypeName = "物料类型", DataSource = 0, SqlScript = null, OrderNum = 52, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_base_unit", TypeName = "基本计量单位", DataSource = 0, SqlScript = null, OrderNum = 53, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_group_code", TypeName = "物料组", DataSource = 0, SqlScript = null, OrderNum = 54, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_purchase_group", TypeName = "采购组", DataSource = 0, SqlScript = null, OrderNum = 55, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_purchase_type", TypeName = "采购类型", DataSource = 0, SqlScript = null, OrderNum = 56, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_special_purchase", TypeName = "特殊采购类", DataSource = 0, SqlScript = null, OrderNum = 57, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_bulk_type", TypeName = "散装物料", DataSource = 0, SqlScript = null, OrderNum = 58, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_delivery_time", TypeName = "计划交货时间", DataSource = 0, SqlScript = null, OrderNum = 59, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_production_days", TypeName = "自制生产天数", DataSource = 0, SqlScript = null, OrderNum = 60, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_inspection_stock", TypeName = "过账到检验库存", DataSource = 0, SqlScript = null, OrderNum = 61, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_profit_center", TypeName = "利润中心", DataSource = 0, SqlScript = null, OrderNum = 62, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_variance_code", TypeName = "差异码", DataSource = 0, SqlScript = null, OrderNum = 63, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_batch_management", TypeName = "批次管理", DataSource = 0, SqlScript = null, OrderNum = 64, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_manufacturer", TypeName = "制造商", DataSource = 0, SqlScript = null, OrderNum = 65, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_evaluation_type", TypeName = "评估类", DataSource = 0, SqlScript = null, OrderNum = 66, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_currency_code", TypeName = "货币", DataSource = 0, SqlScript = null, OrderNum = 67, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_price_control", TypeName = "价格控制", DataSource = 0, SqlScript = null, OrderNum = 68, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_price_unit", TypeName = "价格单位", DataSource = 0, SqlScript = null, OrderNum = 69, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_storage_location", TypeName = "生产仓储地点", DataSource = 0, SqlScript = null, OrderNum = 70, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_storage_position", TypeName = "仓位", DataSource = 0, SqlScript = null, OrderNum = 71, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_cross_plant_status", TypeName = "跨工厂物料状态", DataSource = 0, SqlScript = null, OrderNum = 72, IsBuiltin = 1, TypeStatus = 0 },
            new DictionaryType { TypeCode = "material_hs_code", TypeName = "HS编码", DataSource = 0, SqlScript = null, OrderNum = 73, IsBuiltin = 1, TypeStatus = 0 }
        };
    }

    private static List<DictionaryData> BuildDictionaryDataSeeds()
    {
        return new List<DictionaryData>
        {
            // sys_common_gender
            new DictionaryData { TypeCode = "sys_common_gender", DataLabel = "男", DataValue = "male", I18nKey = "dict.sys_common_gender.male", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_common_gender", DataLabel = "女", DataValue = "female", I18nKey = "dict.sys_common_gender.female", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_common_gender", DataLabel = "未知", DataValue = "unknown", I18nKey = "dict.sys_common_gender.unknown", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 3 },

            // sys_common_yes_no
            new DictionaryData { TypeCode = "sys_common_yes_no", DataLabel = "是", DataValue = "yes", I18nKey = "dict.sys_common_yes_no.yes", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_common_yes_no", DataLabel = "否", DataValue = "no", I18nKey = "dict.sys_common_yes_no.no", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },

            // sys_common_status
            new DictionaryData { TypeCode = "sys_common_status", DataLabel = "正常", DataValue = "normal", I18nKey = "dict.sys_common_status.normal", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_common_status", DataLabel = "禁用", DataValue = "disabled", I18nKey = "dict.sys_common_status.disabled", ExtLabel = null, ExtValue = null, CssClass = "danger", ListClass = "danger", OrderNum = 1 },

            // sys_user_type
            new DictionaryData { TypeCode = "sys_user_type", DataLabel = "系统用户", DataValue = "Takt365", I18nKey = "dict.sys_user_type.Takt365", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_user_type", DataLabel = "普通用户", DataValue = "normal", I18nKey = "dict.sys_user_type.normal", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            
            // sys_data_scope
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "全部数据", DataValue = "1", I18nKey = "dict.sys_data_scope.all", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "自定义数据", DataValue = "2", I18nKey = "dict.sys_data_scope.custom", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "本部门数据", DataValue = "3", I18nKey = "dict.sys_data_scope.department", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "本部门及以下数据", DataValue = "4", I18nKey = "dict.sys_data_scope.department_below", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "sys_data_scope", DataLabel = "仅本人数据", DataValue = "5", I18nKey = "dict.sys_data_scope.self", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // sys_dict_data_source
            new DictionaryData { TypeCode = "sys_dict_data_source", DataLabel = "系统", DataValue = "0", I18nKey = "dict.sys_dict_data_source.system", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_dict_data_source", DataLabel = "SQL脚本", DataValue = "1", I18nKey = "dict.sys_dict_data_source.sql_script", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 1 },
            
            // sys_dict_is_builtin
            new DictionaryData { TypeCode = "sys_dict_is_builtin", DataLabel = "是", DataValue = "0", I18nKey = "dict.sys_dict_is_builtin.yes", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "sys_dict_is_builtin", DataLabel = "否", DataValue = "1", I18nKey = "dict.sys_dict_is_builtin.no", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // menu_link_external
            new DictionaryData { TypeCode = "menu_link_external", DataLabel = "外链", DataValue = "0", I18nKey = "dict.menu_link_external.external", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_link_external", DataLabel = "不是外链", DataValue = "1", I18nKey = "dict.menu_link_external.not_external", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // menu_cache_enable
            new DictionaryData { TypeCode = "menu_cache_enable", DataLabel = "缓存", DataValue = "0", I18nKey = "dict.menu_cache_enable.cache", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_cache_enable", DataLabel = "不缓存", DataValue = "1", I18nKey = "dict.menu_cache_enable.no_cache", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // menu_visible_enable
            new DictionaryData { TypeCode = "menu_visible_enable", DataLabel = "可见", DataValue = "0", I18nKey = "dict.menu_visible_enable.visible", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_visible_enable", DataLabel = "不可见", DataValue = "1", I18nKey = "dict.menu_visible_enable.invisible", ExtLabel = null, ExtValue = null, CssClass = "danger", ListClass = "danger", OrderNum = 1 },
            
            // menu_type_category
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "目录", DataValue = "0", I18nKey = "dict.menu_type_category.directory", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 0 },
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "菜单", DataValue = "1", I18nKey = "dict.menu_type_category.menu", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 1 },
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "按钮", DataValue = "2", I18nKey = "dict.menu_type_category.button", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 2 },
            new DictionaryData { TypeCode = "menu_type_category", DataLabel = "API", DataValue = "3", I18nKey = "dict.menu_type_category.api", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 3 },
            
            // setting_value_type
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "字符串", DataValue = "0", I18nKey = "dict.setting_value_type.string", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 0 },
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "数字", DataValue = "1", I18nKey = "dict.setting_value_type.number", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 1 },
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "布尔值", DataValue = "2", I18nKey = "dict.setting_value_type.boolean", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 2 },
            new DictionaryData { TypeCode = "setting_value_type", DataLabel = "JSON", DataValue = "3", I18nKey = "dict.setting_value_type.json", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            
            // setting_is_default
            new DictionaryData { TypeCode = "setting_is_default", DataLabel = "是", DataValue = "0", I18nKey = "dict.setting_is_default.yes", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "setting_is_default", DataLabel = "否", DataValue = "1", I18nKey = "dict.setting_is_default.no", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 1 },
            
            // setting_is_editable
            new DictionaryData { TypeCode = "setting_is_editable", DataLabel = "是", DataValue = "0", I18nKey = "dict.setting_is_editable.yes", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 0 },
            new DictionaryData { TypeCode = "setting_is_editable", DataLabel = "否", DataValue = "1", I18nKey = "dict.setting_is_editable.no", ExtLabel = null, ExtValue = null, CssClass = "danger", ListClass = "danger", OrderNum = 1 },
            
            // sys_ui_css_class
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "主要", DataValue = "primary", I18nKey = "dict.sys_ui_css_class.primary", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "成功", DataValue = "success", I18nKey = "dict.sys_ui_css_class.success", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "信息", DataValue = "info", I18nKey = "dict.sys_ui_css_class.info", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 3 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "警告", DataValue = "warning", I18nKey = "dict.sys_ui_css_class.warning", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "危险", DataValue = "danger", I18nKey = "dict.sys_ui_css_class.danger", ExtLabel = null, ExtValue = null, CssClass = "danger", ListClass = "danger", OrderNum = 5 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "次要", DataValue = "secondary", I18nKey = "dict.sys_ui_css_class.secondary", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 6 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "浅色", DataValue = "light", I18nKey = "dict.sys_ui_css_class.light", ExtLabel = null, ExtValue = null, CssClass = "light", ListClass = "light", OrderNum = 7 },
            new DictionaryData { TypeCode = "sys_ui_css_class", DataLabel = "深色", DataValue = "dark", I18nKey = "dict.sys_ui_css_class.dark", ExtLabel = null, ExtValue = null, CssClass = "dark", ListClass = "dark", OrderNum = 8 },
            
            // sys_module_name
            new DictionaryData { TypeCode = "sys_module_name", DataLabel = "前端", DataValue = "Frontend", I18nKey = "dict.sys_module_name.Frontend", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "sys_module_name", DataLabel = "后端", DataValue = "Backend", I18nKey = "dict.sys_module_name.Backend", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "sys_module_name", DataLabel = "移动端", DataValue = "Mobile", I18nKey = "dict.sys_module_name.Mobile", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // setting_category_type
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "系统设置", DataValue = "system", I18nKey = "dict.setting_category_type.system", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "用户设置", DataValue = "user", I18nKey = "dict.setting_category_type.user", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "外观设置", DataValue = "appearance", I18nKey = "dict.setting_category_type.appearance", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "安全设置", DataValue = "security", I18nKey = "dict.setting_category_type.security", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "setting_category_type", DataLabel = "通知设置", DataValue = "notification", I18nKey = "dict.setting_category_type.notification", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_plant_code
            new DictionaryData { TypeCode = "material_plant_code", DataLabel = "工厂1000", DataValue = "1000", I18nKey = "dict.material_plant_code.1000", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_plant_code", DataLabel = "工厂2000", DataValue = "2000", I18nKey = "dict.material_plant_code.2000", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_plant_code", DataLabel = "工厂3000", DataValue = "3000", I18nKey = "dict.material_plant_code.3000", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_industry_field
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "机械制造", DataValue = "M", I18nKey = "dict.material_industry_field.M", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "电子制造", DataValue = "E", I18nKey = "dict.material_industry_field.E", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "化工", DataValue = "C", I18nKey = "dict.material_industry_field.C", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_industry_field", DataLabel = "食品", DataValue = "F", I18nKey = "dict.material_industry_field.F", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 4 },
            
            // material_type_category
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "成品", DataValue = "FERT", I18nKey = "dict.material_type_category.FERT", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "半成品", DataValue = "HALB", I18nKey = "dict.material_type_category.HALB", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "原材料", DataValue = "ROH", I18nKey = "dict.material_type_category.ROH", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "贸易商品", DataValue = "HIBE", I18nKey = "dict.material_type_category.HIBE", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_type_category", DataLabel = "非库存物料", DataValue = "NLAG", I18nKey = "dict.material_type_category.NLAG", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_base_unit
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "件", DataValue = "PC", I18nKey = "dict.material_base_unit.PC", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "千克", DataValue = "KG", I18nKey = "dict.material_base_unit.KG", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "米", DataValue = "M", I18nKey = "dict.material_base_unit.M", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "平方米", DataValue = "M2", I18nKey = "dict.material_base_unit.M2", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "立方米", DataValue = "M3", I18nKey = "dict.material_base_unit.M3", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "升", DataValue = "L", I18nKey = "dict.material_base_unit.L", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 6 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "箱", DataValue = "BOX", I18nKey = "dict.material_base_unit.BOX", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 7 },
            new DictionaryData { TypeCode = "material_base_unit", DataLabel = "个", DataValue = "PCS", I18nKey = "dict.material_base_unit.PCS", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 8 },
            
            // material_group_code
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "原材料组", DataValue = "001", I18nKey = "dict.material_group_code.001", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "半成品组", DataValue = "002", I18nKey = "dict.material_group_code.002", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "成品组", DataValue = "003", I18nKey = "dict.material_group_code.003", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "包装材料组", DataValue = "004", I18nKey = "dict.material_group_code.004", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_group_code", DataLabel = "辅助材料组", DataValue = "005", I18nKey = "dict.material_group_code.005", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_purchase_group
            new DictionaryData { TypeCode = "material_purchase_group", DataLabel = "采购组001", DataValue = "001", I18nKey = "dict.material_purchase_group.001", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_purchase_group", DataLabel = "采购组002", DataValue = "002", I18nKey = "dict.material_purchase_group.002", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_purchase_group", DataLabel = "采购组003", DataValue = "003", I18nKey = "dict.material_purchase_group.003", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_purchase_type
            new DictionaryData { TypeCode = "material_purchase_type", DataLabel = "外部采购", DataValue = "E", I18nKey = "dict.material_purchase_type.E", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_purchase_type", DataLabel = "自制", DataValue = "F", I18nKey = "dict.material_purchase_type.F", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_purchase_type", DataLabel = "两者皆可", DataValue = "X", I18nKey = "dict.material_purchase_type.X", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 3 },
            
            // material_special_purchase
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "标准采购", DataValue = "10", I18nKey = "dict.material_special_purchase.10", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "寄售", DataValue = "20", I18nKey = "dict.material_special_purchase.20", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "分包", DataValue = "30", I18nKey = "dict.material_special_purchase.30", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_special_purchase", DataLabel = "第三方", DataValue = "40", I18nKey = "dict.material_special_purchase.40", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 4 },
            
            // material_bulk_type
            new DictionaryData { TypeCode = "material_bulk_type", DataLabel = "散装物料", DataValue = "X", I18nKey = "dict.material_bulk_type.X", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_bulk_type", DataLabel = "非散装物料", DataValue = " ", I18nKey = "dict.material_bulk_type.space", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },
            
            // material_inspection_stock
            new DictionaryData { TypeCode = "material_inspection_stock", DataLabel = "过账到检验库存", DataValue = "X", I18nKey = "dict.material_inspection_stock.X", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_inspection_stock", DataLabel = "不过账到检验库存", DataValue = " ", I18nKey = "dict.material_inspection_stock.space", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },
            
            // material_profit_center
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "利润中心1000", DataValue = "1000", I18nKey = "dict.material_profit_center.1000", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "利润中心2000", DataValue = "2000", I18nKey = "dict.material_profit_center.2000", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_profit_center", DataLabel = "利润中心3000", DataValue = "3000", I18nKey = "dict.material_profit_center.3000", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_batch_management
            new DictionaryData { TypeCode = "material_batch_management", DataLabel = "批次管理", DataValue = "X", I18nKey = "dict.material_batch_management.X", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_batch_management", DataLabel = "非批次管理", DataValue = " ", I18nKey = "dict.material_batch_management.space", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 2 },
            
            // material_evaluation_type
            new DictionaryData { TypeCode = "material_evaluation_type", DataLabel = "移动平均", DataValue = "V", I18nKey = "dict.material_evaluation_type.V", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_evaluation_type", DataLabel = "标准价格", DataValue = "S", I18nKey = "dict.material_evaluation_type.S", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_evaluation_type", DataLabel = "期间单位价格", DataValue = "L", I18nKey = "dict.material_evaluation_type.L", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_currency_code
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "人民币", DataValue = "CNY", I18nKey = "dict.material_currency_code.CNY", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "美元", DataValue = "USD", I18nKey = "dict.material_currency_code.USD", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "欧元", DataValue = "EUR", I18nKey = "dict.material_currency_code.EUR", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "日元", DataValue = "JPY", I18nKey = "dict.material_currency_code.JPY", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_currency_code", DataLabel = "港币", DataValue = "HKD", I18nKey = "dict.material_currency_code.HKD", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_price_control
            new DictionaryData { TypeCode = "material_price_control", DataLabel = "移动平均", DataValue = "V", I18nKey = "dict.material_price_control.V", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_price_control", DataLabel = "标准价格", DataValue = "S", I18nKey = "dict.material_price_control.S", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            
            // material_cross_plant_status
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "已释放", DataValue = "01", I18nKey = "dict.material_cross_plant_status.01", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "已锁定", DataValue = "02", I18nKey = "dict.material_cross_plant_status.02", ExtLabel = null, ExtValue = null, CssClass = "danger", ListClass = "danger", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_cross_plant_status", DataLabel = "已归档", DataValue = "03", I18nKey = "dict.material_cross_plant_status.03", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 3 },
            
            // material_variance_code
            new DictionaryData { TypeCode = "material_variance_code", DataLabel = "采购价格差异", DataValue = "PPV", I18nKey = "dict.material_variance_code.PPV", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_variance_code", DataLabel = "移动价格差异", DataValue = "MPV", I18nKey = "dict.material_variance_code.MPV", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_variance_code", DataLabel = "物料使用差异", DataValue = "MUV", I18nKey = "dict.material_variance_code.MUV", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_variance_code", DataLabel = "人工效率差异", DataValue = "LEV", I18nKey = "dict.material_variance_code.LEV", ExtLabel = null, ExtValue = null, CssClass = "danger", ListClass = "danger", OrderNum = 4 },
            
            // material_manufacturer
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "制造商001", DataValue = "M001", I18nKey = "dict.material_manufacturer.M001", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "制造商002", DataValue = "M002", I18nKey = "dict.material_manufacturer.M002", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_manufacturer", DataLabel = "制造商003", DataValue = "M003", I18nKey = "dict.material_manufacturer.M003", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            
            // material_storage_location
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "仓储地点0001", DataValue = "0001", I18nKey = "dict.material_storage_location.0001", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "仓储地点0002", DataValue = "0002", I18nKey = "dict.material_storage_location.0002", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "仓储地点0003", DataValue = "0003", I18nKey = "dict.material_storage_location.0003", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "生产仓储地点1001", DataValue = "1001", I18nKey = "dict.material_storage_location.1001", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 },
            new DictionaryData { TypeCode = "material_storage_location", DataLabel = "外部采购仓储地点2001", DataValue = "2001", I18nKey = "dict.material_storage_location.2001", ExtLabel = null, ExtValue = null, CssClass = "secondary", ListClass = "secondary", OrderNum = 5 },
            
            // material_storage_position
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "A区-01排-01位", DataValue = "A-01-01", I18nKey = "dict.material_storage_position.A_01_01", ExtLabel = null, ExtValue = null, CssClass = "primary", ListClass = "primary", OrderNum = 1 },
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "A区-01排-02位", DataValue = "A-01-02", I18nKey = "dict.material_storage_position.A_01_02", ExtLabel = null, ExtValue = null, CssClass = "info", ListClass = "info", OrderNum = 2 },
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "B区-01排-01位", DataValue = "B-01-01", I18nKey = "dict.material_storage_position.B_01_01", ExtLabel = null, ExtValue = null, CssClass = "success", ListClass = "success", OrderNum = 3 },
            new DictionaryData { TypeCode = "material_storage_position", DataLabel = "B区-01排-02位", DataValue = "B-01-02", I18nKey = "dict.material_storage_position.B_01_02", ExtLabel = null, ExtValue = null, CssClass = "warning", ListClass = "warning", OrderNum = 4 }
        };
    }
}

