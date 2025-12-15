//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : DbSeedTranslationDictionary.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 字典翻译种子数据初始化服务
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//===================================================================

using System.Collections.Generic;
using System.Linq;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// 字典翻译种子数据初始化服务
/// 创建字典数据值的翻译种子（dict.* 开头的所有字典翻译）
/// </summary>
public class DbSeedTranslationDictionary
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly IBaseRepository<Translation> _translationRepository;

    public DbSeedTranslationDictionary(
        InitLogManager initLog,
        IBaseRepository<Language> languageRepository,
        IBaseRepository<Translation> translationRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
        _translationRepository = translationRepository ?? throw new ArgumentNullException(nameof(translationRepository));
    }

    /// <summary>
    /// 初始化通用翻译数据
    /// </summary>
    public void Initialize()
    {
        _initLog.Information("开始初始化字典翻译种子数据...");

        // 从仓库获取语言数据
        var languages = _languageRepository.AsQueryable().ToList();
        if (languages.Count == 0)
        {
            _initLog.Warning("语言数据为空，跳过字典翻译数据初始化");
            return;
        }

        InitializeTranslations(languages);

        _initLog.Information("✅ 字典翻译种子数据初始化完成");
    }

    /// <summary>
    /// 初始化翻译数据
    /// </summary>
    private void InitializeTranslations(List<Language> languages)
    {
        var zhCn = languages.FirstOrDefault(l => l.LanguageCode == "zh-CN");
        var enUs = languages.FirstOrDefault(l => l.LanguageCode == "en-US");
        var jaJp = languages.FirstOrDefault(l => l.LanguageCode == "ja-JP");

        if (zhCn == null || enUs == null || jaJp == null)
        {
            _initLog.Warning("语言数据不完整，跳过翻译数据初始化");
            return;
        }

        // 翻译数据定义（基础通用项）
        var translations = new List<dynamic>
        {
           // 字典数据翻译种子（dict. 开头）
            // sys_common_gender
            new { Key = "dict.sys_common_gender.male", Module = "Frontend", ZhCn = "男", EnUs = "Male", JaJp = "男性" },
            new { Key = "dict.sys_common_gender.female", Module = "Frontend", ZhCn = "女", EnUs = "Female", JaJp = "女性" },
            new { Key = "dict.sys_common_gender.unknown", Module = "Frontend", ZhCn = "未知", EnUs = "Unknown", JaJp = "不明" },
            
            // sys_common_yes_no
            new { Key = "dict.sys_common_yes_no.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.sys_common_yes_no.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // sys_common_status
            new { Key = "dict.sys_common_status.normal", Module = "Frontend", ZhCn = "正常", EnUs = "Normal", JaJp = "正常" },
            new { Key = "dict.sys_common_status.disabled", Module = "Frontend", ZhCn = "禁用", EnUs = "Disabled", JaJp = "無効" },
            
            // sys_user_type
            new { Key = "dict.sys_user_type.takt365", Module = "Frontend", ZhCn = "系统用户", EnUs = "System User", JaJp = "システムユーザー" },
            new { Key = "dict.sys_user_type.normal", Module = "Frontend", ZhCn = "普通用户", EnUs = "Normal User", JaJp = "一般ユーザー" },
            
            // sys_data_scope
            new { Key = "dict.sys_data_scope.all", Module = "Frontend", ZhCn = "全部数据", EnUs = "All Data", JaJp = "全データ" },
            new { Key = "dict.sys_data_scope.custom", Module = "Frontend", ZhCn = "自定义数据", EnUs = "Custom Data", JaJp = "カスタムデータ" },
            new { Key = "dict.sys_data_scope.department", Module = "Frontend", ZhCn = "本部门数据", EnUs = "Department Data", JaJp = "部門データ" },
            new { Key = "dict.sys_data_scope.department_below", Module = "Frontend", ZhCn = "本部门及以下数据", EnUs = "Department and Below Data", JaJp = "部門および下位データ" },
            new { Key = "dict.sys_data_scope.self", Module = "Frontend", ZhCn = "仅本人数据", EnUs = "Self Data Only", JaJp = "本人データのみ" },
            
            // sys_dict_data_source
            new { Key = "dict.sys_dict_data_source.system", Module = "Frontend", ZhCn = "系统", EnUs = "System", JaJp = "システム" },
            new { Key = "dict.sys_dict_data_source.sql_script", Module = "Frontend", ZhCn = "SQL脚本", EnUs = "SQL Script", JaJp = "SQLスクリプト" },
            
            // sys_dict_is_builtin
            new { Key = "dict.sys_dict_is_builtin.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.sys_dict_is_builtin.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // menu_link_external
            new { Key = "dict.menu_link_external.external", Module = "Frontend", ZhCn = "外链", EnUs = "External Link", JaJp = "外部リンク" },
            new { Key = "dict.menu_link_external.not_external", Module = "Frontend", ZhCn = "不是外链", EnUs = "Not External Link", JaJp = "外部リンクではない" },
            
            // menu_cache_enable
            new { Key = "dict.menu_cache_enable.cache", Module = "Frontend", ZhCn = "缓存", EnUs = "Cache", JaJp = "キャッシュ" },
            new { Key = "dict.menu_cache_enable.no_cache", Module = "Frontend", ZhCn = "不缓存", EnUs = "No Cache", JaJp = "キャッシュなし" },
            
            // menu_visible_enable
            new { Key = "dict.menu_visible_enable.visible", Module = "Frontend", ZhCn = "可见", EnUs = "Visible", JaJp = "表示" },
            new { Key = "dict.menu_visible_enable.invisible", Module = "Frontend", ZhCn = "不可见", EnUs = "Invisible", JaJp = "非表示" },
            
            // menu_type_category
            new { Key = "dict.menu_type_category.directory", Module = "Frontend", ZhCn = "目录", EnUs = "Directory", JaJp = "ディレクトリ" },
            new { Key = "dict.menu_type_category.menu", Module = "Frontend", ZhCn = "菜单", EnUs = "Menu", JaJp = "メニュー" },
            new { Key = "dict.menu_type_category.button", Module = "Frontend", ZhCn = "按钮", EnUs = "Button", JaJp = "ボタン" },
            new { Key = "dict.menu_type_category.api", Module = "Frontend", ZhCn = "API", EnUs = "API", JaJp = "API" },
            
            // setting_value_type
            new { Key = "dict.setting_value_type.string", Module = "Frontend", ZhCn = "字符串", EnUs = "String", JaJp = "文字列" },
            new { Key = "dict.setting_value_type.number", Module = "Frontend", ZhCn = "数字", EnUs = "Number", JaJp = "数値" },
            new { Key = "dict.setting_value_type.boolean", Module = "Frontend", ZhCn = "布尔值", EnUs = "Boolean", JaJp = "ブール値" },
            new { Key = "dict.setting_value_type.json", Module = "Frontend", ZhCn = "JSON", EnUs = "JSON", JaJp = "JSON" },
            
            // setting_is_default
            new { Key = "dict.setting_is_default.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.setting_is_default.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // setting_is_editable
            new { Key = "dict.setting_is_editable.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.setting_is_editable.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // sys_ui_css_class
            new { Key = "dict.sys_ui_css_class.primary", Module = "Frontend", ZhCn = "主要", EnUs = "Primary", JaJp = "プライマリ" },
            new { Key = "dict.sys_ui_css_class.success", Module = "Frontend", ZhCn = "成功", EnUs = "Success", JaJp = "成功" },
            new { Key = "dict.sys_ui_css_class.info", Module = "Frontend", ZhCn = "信息", EnUs = "Info", JaJp = "情報" },
            new { Key = "dict.sys_ui_css_class.warning", Module = "Frontend", ZhCn = "警告", EnUs = "Warning", JaJp = "警告" },
            new { Key = "dict.sys_ui_css_class.danger", Module = "Frontend", ZhCn = "危险", EnUs = "Danger", JaJp = "危険" },
            new { Key = "dict.sys_ui_css_class.secondary", Module = "Frontend", ZhCn = "次要", EnUs = "Secondary", JaJp = "セカンダリ" },
            new { Key = "dict.sys_ui_css_class.light", Module = "Frontend", ZhCn = "浅色", EnUs = "Light", JaJp = "ライト" },
            new { Key = "dict.sys_ui_css_class.dark", Module = "Frontend", ZhCn = "深色", EnUs = "Dark", JaJp = "ダーク" },
            
            // sys_module_name
            new { Key = "dict.sys_module_name.frontend", Module = "Frontend", ZhCn = "前端", EnUs = "Frontend", JaJp = "フロントエンド" },
            new { Key = "dict.sys_module_name.backend", Module = "Frontend", ZhCn = "后端", EnUs = "Backend", JaJp = "バックエンド" },
            new { Key = "dict.sys_module_name.mobile", Module = "Frontend", ZhCn = "移动端", EnUs = "Mobile", JaJp = "モバイル" },
            
            // setting_category_type
            new { Key = "dict.setting_category_type.system", Module = "Frontend", ZhCn = "系统设置", EnUs = "System Settings", JaJp = "システム設定" },
            new { Key = "dict.setting_category_type.user", Module = "Frontend", ZhCn = "用户设置", EnUs = "User Settings", JaJp = "ユーザー設定" },
            new { Key = "dict.setting_category_type.appearance", Module = "Frontend", ZhCn = "外观设置", EnUs = "Appearance Settings", JaJp = "外観設定" },
            new { Key = "dict.setting_category_type.security", Module = "Frontend", ZhCn = "安全设置", EnUs = "Security Settings", JaJp = "セキュリティ設定" },
            new { Key = "dict.setting_category_type.notification", Module = "Frontend", ZhCn = "通知设置", EnUs = "Notification Settings", JaJp = "通知設定" },
            
            // material_plant_code工厂字典：C100-》DTA,H100-》TAC,1000-》TCJ
            new { Key = "dict.material_plant_code.C100", Module = "Frontend", ZhCn = "DTA", EnUs = "DTA", JaJp = "DTA" },
            new { Key = "dict.material_plant_code.H100", Module = "Frontend", ZhCn = "TAC", EnUs = "TAC", JaJp = "TAC" },
            new { Key = "dict.material_plant_code.1000", Module = "Frontend", ZhCn = "TCJ", EnUs = "TCJ", JaJp = "TCJ" },
            
            // material_industry_field
            new { Key = "dict.material_industry_field.m", Module = "Frontend", ZhCn = "机械制造", EnUs = "Machinery Manufacturing", JaJp = "機械製造" },
            new { Key = "dict.material_industry_field.e", Module = "Frontend", ZhCn = "电子制造", EnUs = "Electronics Manufacturing", JaJp = "電子製造" },
            new { Key = "dict.material_industry_field.c", Module = "Frontend", ZhCn = "化工", EnUs = "Chemical", JaJp = "化学" },
            new { Key = "dict.material_industry_field.f", Module = "Frontend", ZhCn = "食品", EnUs = "Food", JaJp = "食品" },
            
            // material_type_category
            new { Key = "dict.material_type_category.fert", Module = "Frontend", ZhCn = "成品", EnUs = "Finished Product", JaJp = "完成品" },
            new { Key = "dict.material_type_category.halb", Module = "Frontend", ZhCn = "半成品", EnUs = "Semi-Finished Product", JaJp = "半完成品" },
            new { Key = "dict.material_type_category.roh", Module = "Frontend", ZhCn = "原材料", EnUs = "Raw Material", JaJp = "原材料" },
            new { Key = "dict.material_type_category.hibe", Module = "Frontend", ZhCn = "贸易商品", EnUs = "Trading Goods", JaJp = "貿易商品" },
            new { Key = "dict.material_type_category.nlag", Module = "Frontend", ZhCn = "非库存物料", EnUs = "Non-Stock Material", JaJp = "在庫外資材" },
            
            // material_base_unit
            new { Key = "dict.material_base_unit.pc", Module = "Frontend", ZhCn = "件", EnUs = "Piece", JaJp = "個" },
            new { Key = "dict.material_base_unit.g", Module = "Frontend", ZhCn = "克", EnUs = "Gram", JaJp = "グラム" },
            new { Key = "dict.material_base_unit.kg", Module = "Frontend", ZhCn = "千克", EnUs = "Kilogram", JaJp = "キログラム" },
            new { Key = "dict.material_base_unit.m", Module = "Frontend", ZhCn = "米", EnUs = "Meter", JaJp = "メートル" },
            new { Key = "dict.material_base_unit.km", Module = "Frontend", ZhCn = "千米", EnUs = "Kilometer", JaJp = "キロメートル" },
            new { Key = "dict.material_base_unit.m2", Module = "Frontend", ZhCn = "平方米", EnUs = "Square Meter", JaJp = "平方メートル" },
            new { Key = "dict.material_base_unit.m3", Module = "Frontend", ZhCn = "立方米", EnUs = "Cubic Meter", JaJp = "立方メートル" },
            new { Key = "dict.material_base_unit.ml", Module = "Frontend", ZhCn = "毫升", EnUs = "Milliliter", JaJp = "ミリリットル" },
            new { Key = "dict.material_base_unit.l", Module = "Frontend", ZhCn = "升", EnUs = "Liter", JaJp = "リットル" },
            new { Key = "dict.material_base_unit.box", Module = "Frontend", ZhCn = "箱", EnUs = "Box", JaJp = "箱" },
            new { Key = "dict.material_base_unit.set", Module = "Frontend", ZhCn = "套", EnUs = "Set", JaJp = "セット" },
            new { Key = "dict.material_base_unit.ea", Module = "Frontend", ZhCn = "个", EnUs = "Each", JaJp = "個" },
            new { Key = "dict.material_base_unit.can", Module = "Frontend", ZhCn = "罐", EnUs = "Can", JaJp = "缶" },
            
            // material_group_code物料组，应该根据材料的性质还区别
            new { Key = "dict.material_group_code.fdb", Module = "Frontend", ZhCn = "SW RGLTD PS", EnUs = "SW RGLTD PS", JaJp = "SW RGLTD PS" },
            new { Key = "dict.material_group_code.fdc", Module = "Frontend", ZhCn = "SER RGLTD PS", EnUs = "SER RGLTD PS", JaJp = "SER RGLTD PS" },
            new { Key = "dict.material_group_code.fdd", Module = "Frontend", ZhCn = "DC/DC CONV", EnUs = "DC/DC CONV", JaJp = "DC/DC CONV" },
            new { Key = "dict.material_group_code.fde", Module = "Frontend", ZhCn = "AC ADAPTER", EnUs = "AC ADAPTER", JaJp = "AC ADAPTER" },
            new { Key = "dict.material_group_code.fdf", Module = "Frontend", ZhCn = "BAT CHG", EnUs = "BAT CHG", JaJp = "BAT CHG" },
            new { Key = "dict.material_group_code.fdg", Module = "Frontend", ZhCn = "CAR ADAPTER", EnUs = "CAR ADAPTER", JaJp = "CAR ADAPTER" },
            new { Key = "dict.material_group_code.fdh", Module = "Frontend", ZhCn = "NON BREAK PS", EnUs = "NON BREAK PS", JaJp = "NON BREAK PS" },
            new { Key = "dict.material_group_code.fdz", Module = "Frontend", ZhCn = "FDZ", EnUs = "FDZ", JaJp = "FDZ" },
            new { Key = "dict.material_group_code.fec", Module = "Frontend", ZhCn = "CRT DISPLAY", EnUs = "CRT DISPLAY", JaJp = "CRT DISPLAY" },
            new { Key = "dict.material_group_code.fee", Module = "Frontend", ZhCn = "EL DISPLAY", EnUs = "EL DISPLAY", JaJp = "EL DISPLAY" },
            new { Key = "dict.material_group_code.fef", Module = "Frontend", ZhCn = "FIL DISPLAY", EnUs = "FIL DISPLAY", JaJp = "FIL DISPLAY" },
            new { Key = "dict.material_group_code.feg", Module = "Frontend", ZhCn = "ELCTRN RAY DSPL", EnUs = "ELCTRN RAY DSPL", JaJp = "ELCTRN RAY DSPL" },
            new { Key = "dict.material_group_code.feh", Module = "Frontend", ZhCn = "LED DISPLAY", EnUs = "LED DISPLAY", JaJp = "LED DISPLAY" },
            new { Key = "dict.material_group_code.fel", Module = "Frontend", ZhCn = "LCD", EnUs = "LCD", JaJp = "LCD" },
            new { Key = "dict.material_group_code.fen", Module = "Frontend", ZhCn = "NEON SEG DSPL", EnUs = "NEON SEG DSPL", JaJp = "NEON SEG DSPL" },
            new { Key = "dict.material_group_code.fep", Module = "Frontend", ZhCn = "PLASMA DISPLAY", EnUs = "PLASMA DISPLAY", JaJp = "PLASMA DISPLAY" },
            new { Key = "dict.material_group_code.fez", Module = "Frontend", ZhCn = "FEZ", EnUs = "FEZ", JaJp = "FEZ" },
            new { Key = "dict.material_group_code.ffc", Module = "Frontend", ZhCn = "VIDEO CAMERA", EnUs = "VIDEO CAMERA", JaJp = "VIDEO CAMERA" },
            
            // material_purchase_group
            new { Key = "dict.material_purchase_group.001", Module = "Frontend", ZhCn = "采购组001", EnUs = "Purchase Group 001", JaJp = "購買グループ001" },
            new { Key = "dict.material_purchase_group.002", Module = "Frontend", ZhCn = "采购组002", EnUs = "Purchase Group 002", JaJp = "購買グループ002" },
            new { Key = "dict.material_purchase_group.003", Module = "Frontend", ZhCn = "采购组003", EnUs = "Purchase Group 003", JaJp = "購買グループ003" },
            
            // material_purchase_type
            new { Key = "dict.material_purchase_type.e", Module = "Frontend", ZhCn = "外部采购", EnUs = "External Purchase", JaJp = "外部調達" },
            new { Key = "dict.material_purchase_type.f", Module = "Frontend", ZhCn = "自制", EnUs = "In-House Production", JaJp = "自社製造" },
            new { Key = "dict.material_purchase_type.x", Module = "Frontend", ZhCn = "两者皆可", EnUs = "Both", JaJp = "両方" },
            
            // material_special_purchase
            new { Key = "dict.material_special_purchase.10", Module = "Frontend", ZhCn = "标准采购", EnUs = "Standard Purchase", JaJp = "標準調達" },
            new { Key = "dict.material_special_purchase.20", Module = "Frontend", ZhCn = "寄售", EnUs = "Consignment", JaJp = "委託販売" },
            new { Key = "dict.material_special_purchase.30", Module = "Frontend", ZhCn = "分包", EnUs = "Subcontracting", JaJp = "外注" },
            new { Key = "dict.material_special_purchase.40", Module = "Frontend", ZhCn = "第三方", EnUs = "Third Party", JaJp = "第三者" },
            
            // material_bulk_type
            new { Key = "dict.material_bulk_type.x", Module = "Frontend", ZhCn = "散装物料", EnUs = "Bulk Material", JaJp = "バルク資材" },
            new { Key = "dict.material_bulk_type.space", Module = "Frontend", ZhCn = "非散装物料", EnUs = "Non-Bulk Material", JaJp = "非バルク資材" },
            
            // material_inspection_stock
            new { Key = "dict.material_inspection_stock.x", Module = "Frontend", ZhCn = "过账到检验库存", EnUs = "Post to Inspection Stock", JaJp = "検査在庫に転記" },
            new { Key = "dict.material_inspection_stock.space", Module = "Frontend", ZhCn = "不过账到检验库存", EnUs = "Do Not Post to Inspection Stock", JaJp = "検査在庫に転記しない" },
            
            // material_profit_center，HI,PR,MI,PRO,BS这是对的修改顺序
            new { Key = "dict.material_profit_center.2u10", Module = "Frontend", ZhCn = "高端", EnUs = "High-End", JaJp = "ハイエンド" },
            new { Key = "dict.material_profit_center.2u20", Module = "Frontend", ZhCn = "高级", EnUs = "PREMIUM", JaJp = "プレミアム" },
            new { Key = "dict.material_profit_center.3u10", Module = "Frontend", ZhCn = "乐器", EnUs = "Musical Instruments", JaJp = "楽器" },
            new { Key = "dict.material_profit_center.3u20", Module = "Frontend", ZhCn = "专业", EnUs = "Professional", JaJp = "プロフェッショナル" },
            new { Key = "dict.material_profit_center.4u10", Module = "Frontend", ZhCn = "ODM", EnUs = "ODM", JaJp = "ODM" },
            new { Key = "dict.material_profit_center.4u10", Module = "Frontend", ZhCn = "ODM", EnUs = "ODM", JaJp = "ODM" },
            new { Key = "dict.material_profit_center.4u20", Module = "Frontend", ZhCn = "EMS", EnUs = "EMS", JaJp = "EMS" },
            new { Key = "dict.material_profit_center.4u30", Module = "Frontend", ZhCn = "情报机器", EnUs = "Information Equipment", JaJp = "情報機器" },
            
            // material_batch_management
            new { Key = "dict.material_batch_management.x", Module = "Frontend", ZhCn = "批次管理", EnUs = "Batch Management", JaJp = "ロット管理" },
            new { Key = "dict.material_batch_management.space", Module = "Frontend", ZhCn = "非批次管理", EnUs = "Non-Batch Management", JaJp = "非ロット管理" },
            
            // material_evaluation_type正确的是Z300为材料，Z790为成品,Z791为半成品
            new { Key = "dict.material_evaluation_type.z300", Module = "Frontend", ZhCn = "材料", EnUs = "Material", JaJp = "材料" },
            new { Key = "dict.material_evaluation_type.z790", Module = "Frontend", ZhCn = "成品", EnUs = "Finished Product", JaJp = "完成品" },
            new { Key = "dict.material_evaluation_type.z791", Module = "Frontend", ZhCn = "半成品", EnUs = "Semi-Finished Product", JaJp = "半完成品" },
            
            // material_currency_code
            new { Key = "dict.material_currency_code.cny", Module = "Frontend", ZhCn = "人民币", EnUs = "Chinese Yuan", JaJp = "人民元" },
            new { Key = "dict.material_currency_code.usd", Module = "Frontend", ZhCn = "美元", EnUs = "US Dollar", JaJp = "米ドル" },
            new { Key = "dict.material_currency_code.eur", Module = "Frontend", ZhCn = "欧元", EnUs = "Euro", JaJp = "ユーロ" },
            new { Key = "dict.material_currency_code.jpy", Module = "Frontend", ZhCn = "日元", EnUs = "Japanese Yen", JaJp = "日本円" },
            new { Key = "dict.material_currency_code.hkd", Module = "Frontend", ZhCn = "港币", EnUs = "Hong Kong Dollar", JaJp = "香港ドル" },
            
            // material_price_control
            new { Key = "dict.material_price_control.v", Module = "Frontend", ZhCn = "移动平均", EnUs = "Moving Average", JaJp = "移動平均" },
            new { Key = "dict.material_price_control.s", Module = "Frontend", ZhCn = "标准价格", EnUs = "Standard Price", JaJp = "標準価格" },
            
            // material_cross_plant_status，01調達/倉庫に対しブロック中,02 Task List/BOMのブロック中,Z0計画品目,ZM現法在庫に確認必要,ZP製造中止,ZQ生産終了（製品）,ZW PC MRP対象外,ZX PC 仲介専用品,ZY PC DISCON(MRP対象外),ZZ PC 代替品目あり
            new { Key = "dict.material_cross_plant_status.01", Module = "Frontend", ZhCn = "对采购/仓库已锁定", EnUs = "Blocked for Procurement/Warehouse", JaJp = "調達/倉庫に対しブロック中" },
            new { Key = "dict.material_cross_plant_status.02", Module = "Frontend", ZhCn = "Task List/BOM已锁定", EnUs = "Task List/BOM Blocked", JaJp = "Task List/BOMのブロック中" },
            new { Key = "dict.material_cross_plant_status.z0", Module = "Frontend", ZhCn = "计划品目", EnUs = "Planned Item", JaJp = "計画品目" },
            new { Key = "dict.material_cross_plant_status.zm", Module = "Frontend", ZhCn = "需要确认当前法人在库", EnUs = "Current Legal Entity Stock Confirmation Required", JaJp = "現法在庫に確認必要" },
            new { Key = "dict.material_cross_plant_status.zp", Module = "Frontend", ZhCn = "制造中止", EnUs = "Manufacturing Suspended", JaJp = "製造中止" },
            new { Key = "dict.material_cross_plant_status.zq", Module = "Frontend", ZhCn = "生产结束（产品）", EnUs = "Production Ended (Product)", JaJp = "生産終了（製品）" },
            new { Key = "dict.material_cross_plant_status.zw", Module = "Frontend", ZhCn = "PC MRP对象外", EnUs = "PC MRP Excluded", JaJp = "PC MRP対象外" },
            new { Key = "dict.material_cross_plant_status.zx", Module = "Frontend", ZhCn = "PC 中介专用品", EnUs = "PC Intermediary Exclusive", JaJp = "PC 仲介専用品" },
            new { Key = "dict.material_cross_plant_status.zy", Module = "Frontend", ZhCn = "PC DISCON(MRP对象外)", EnUs = "PC DISCON (MRP Excluded)", JaJp = "PC DISCON(MRP対象外)" },
            new { Key = "dict.material_cross_plant_status.zz", Module = "Frontend", ZhCn = "PC 有替代品目", EnUs = "PC Alternative Item Available", JaJp = "PC 代替品目あり" },
            
            // material_variance_code只有一个选项000001和空
            new { Key = "dict.material_variance_code.000001", Module = "Frontend", ZhCn = "000001", EnUs = "000001", JaJp = "000001" },
            new { Key = "dict.material_variance_code.empty", Module = "Frontend", ZhCn = "空", EnUs = "Empty", JaJp = "空" },
            
            // material_manufacturer，电容、机芯、存储、电源等知名制造商
            // 电容制造商
            new { Key = "dict.material_manufacturer.murata", Module = "Frontend", ZhCn = "村田", EnUs = "Murata", JaJp = "村田製作所" },
            new { Key = "dict.material_manufacturer.tdk", Module = "Frontend", ZhCn = "TDK", EnUs = "TDK", JaJp = "TDK" },
            new { Key = "dict.material_manufacturer.taiyo", Module = "Frontend", ZhCn = "太阳诱电", EnUs = "Taiyo Yuden", JaJp = "太陽誘電" },
            new { Key = "dict.material_manufacturer.kyocera", Module = "Frontend", ZhCn = "京瓷", EnUs = "Kyocera", JaJp = "京セラ" },
            new { Key = "dict.material_manufacturer.avx", Module = "Frontend", ZhCn = "AVX", EnUs = "AVX", JaJp = "AVX" },
            new { Key = "dict.material_manufacturer.kemet", Module = "Frontend", ZhCn = "KEMET", EnUs = "KEMET", JaJp = "KEMET" },
            // 机芯制造商
            new { Key = "dict.material_manufacturer.seiko", Module = "Frontend", ZhCn = "精工", EnUs = "Seiko", JaJp = "セイコー" },
            new { Key = "dict.material_manufacturer.citizen", Module = "Frontend", ZhCn = "西铁城", EnUs = "Citizen", JaJp = "シチズン" },
            new { Key = "dict.material_manufacturer.epson", Module = "Frontend", ZhCn = "爱普生", EnUs = "Epson", JaJp = "エプソン" },
            // 存储制造商
            new { Key = "dict.material_manufacturer.samsung", Module = "Frontend", ZhCn = "三星", EnUs = "Samsung", JaJp = "サムスン" },
            new { Key = "dict.material_manufacturer.skhynix", Module = "Frontend", ZhCn = "SK海力士", EnUs = "SK Hynix", JaJp = "SKハイニックス" },
            new { Key = "dict.material_manufacturer.micron", Module = "Frontend", ZhCn = "美光", EnUs = "Micron", JaJp = "マイクロン" },
            new { Key = "dict.material_manufacturer.toshiba", Module = "Frontend", ZhCn = "东芝", EnUs = "Toshiba", JaJp = "東芝" },
            new { Key = "dict.material_manufacturer.wd", Module = "Frontend", ZhCn = "西部数据", EnUs = "Western Digital", JaJp = "ウェスタンデジタル" },
            new { Key = "dict.material_manufacturer.seagate", Module = "Frontend", ZhCn = "希捷", EnUs = "Seagate", JaJp = "シーゲート" },
            // 电源制造商
            new { Key = "dict.material_manufacturer.delta", Module = "Frontend", ZhCn = "台达", EnUs = "Delta", JaJp = "デルタ" },
            new { Key = "dict.material_manufacturer.liteon", Module = "Frontend", ZhCn = "光宝", EnUs = "Lite-On", JaJp = "ライトオン" },
            new { Key = "dict.material_manufacturer.acbel", Module = "Frontend", ZhCn = "康舒", EnUs = "AcBel", JaJp = "康舒" },
            new { Key = "dict.material_manufacturer.meanwell", Module = "Frontend", ZhCn = "明纬", EnUs = "Mean Well", JaJp = "明緯" },
            new { Key = "dict.material_manufacturer.emerson", Module = "Frontend", ZhCn = "艾默生", EnUs = "Emerson", JaJp = "エマーソン" },
            
            // material_storage_location，C001为机构材料，C002为电子材料，C003为辅材
            new { Key = "dict.material_storage_location.c001", Module = "Frontend", ZhCn = "机构材料", EnUs = "Mechanical Materials", JaJp = "機構材料" },
            new { Key = "dict.material_storage_location.c002", Module = "Frontend", ZhCn = "电子材料", EnUs = "Electronic Materials", JaJp = "電子材料" },
            new { Key = "dict.material_storage_location.c003", Module = "Frontend", ZhCn = "辅材", EnUs = "Auxiliary Materials", JaJp = "補助材料" },
            
            // material_storage_position
            new { Key = "dict.material_storage_position.a_01_01", Module = "Frontend", ZhCn = "A区-01排-01位", EnUs = "Zone A-Row 01-Position 01", JaJp = "A区-01列-01位置" },
            new { Key = "dict.material_storage_position.a_01_02", Module = "Frontend", ZhCn = "A区-01排-02位", EnUs = "Zone A-Row 01-Position 02", JaJp = "A区-01列-02位置" },
            new { Key = "dict.material_storage_position.b_01_01", Module = "Frontend", ZhCn = "B区-01排-01位", EnUs = "Zone B-Row 01-Position 01", JaJp = "B区-01列-01位置" },
            new { Key = "dict.material_storage_position.b_01_02", Module = "Frontend", ZhCn = "B区-01排-02位", EnUs = "Zone B-Row 01-Position 02", JaJp = "B区-01列-02位置" },


        };

        foreach (var trans in translations)
        {
            CreateOrUpdateTranslation(zhCn, trans.Key.ToLower(), trans.Module, trans.ZhCn);
            CreateOrUpdateTranslation(enUs, trans.Key.ToLower(), trans.Module, trans.EnUs);
            CreateOrUpdateTranslation(jaJp, trans.Key.ToLower(), trans.Module, trans.JaJp);
        }

        _initLog.Information("✅ 通用翻译数据初始化完成");
    }

    /// <summary>
    /// 创建或更新翻译
    /// </summary>
    private void CreateOrUpdateTranslation(Language language, string key, string module, string value)
    {
        // 统一按语言代码与翻译键判重；WPF 前端固定模块 Frontend
        // 在查询前将 key 转换为小写，避免在 LINQ 表达式中使用 ToLower()
        var normalizedKey = key.ToLower();
        var existing = _translationRepository.GetFirst(t =>
            t.LanguageCode == language.LanguageCode && t.TranslationKey == normalizedKey && t.Module == "Frontend");
        
        if (existing == null)
        {
            var translation = new Translation
            {
                LanguageCode = language.LanguageCode,
                TranslationKey = normalizedKey,
                TranslationValue = value,
                Module = "Frontend",
                OrderNum = 0
            };
            _translationRepository.Create(translation, "Takt365");
        }
        else
        {
            existing.TranslationValue = value;
            existing.Module = "Frontend";
            _translationRepository.Update(existing, "Takt365");
        }
    }
}
