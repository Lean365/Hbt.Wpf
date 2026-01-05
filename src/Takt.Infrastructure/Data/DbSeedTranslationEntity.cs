// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedTranslationEntity.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：实体字段翻译种子数据
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.Generic;
using System.Linq;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Routine 模块实体字段翻译种子初始化器
/// </summary>
public class DbSeedTranslationEntity
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<Language> _languageRepository;

    public DbSeedTranslationEntity(
        InitLogManager initLog,
        IBaseRepository<Translation> translationRepository,
        IBaseRepository<Language> languageRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _translationRepository = translationRepository ?? throw new ArgumentNullException(nameof(translationRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
    }

    /// <summary>
    /// 初始化实体字段翻译
    /// </summary>
    public void Run()
    {
        var languages = _languageRepository.AsQueryable().ToList();
        if (languages.Count == 0)
        {
            _initLog.Warning("语言数据为空，跳过实体字段翻译初始化");
            return;
        }

        var zhCn = languages.FirstOrDefault(l => l.LanguageCode == "zh-CN");
        var enUs = languages.FirstOrDefault(l => l.LanguageCode == "en-US");
        var jaJp = languages.FirstOrDefault(l => l.LanguageCode == "ja-JP");

        if (zhCn == null || enUs == null || jaJp == null)
        {
            _initLog.Warning("语言数据不完整（zh-CN/en-US/ja-JP 缺失），跳过实体字段翻译初始化");
            return;
        }

        foreach (var seed in BuildTranslationSeeds())
        {
            CreateOrUpdateTranslation(zhCn, seed.Key.ToLower(), seed.Module, seed.ZhCn);
            CreateOrUpdateTranslation(enUs, seed.Key.ToLower(), seed.Module, seed.EnUs);
            CreateOrUpdateTranslation(jaJp, seed.Key.ToLower(), seed.Module, seed.JaJp);
        }

        _initLog.Information("✅ 实体字段翻译初始化完成");
    }

    private void CreateOrUpdateTranslation(Language language, string key, string module, string value)
    {
        // 在查询前将 key 转换为小写，避免在 LINQ 表达式中使用 ToLower()
        var normalizedKey = key.ToLower();
        var existing = _translationRepository.GetFirst(t =>
            t.LanguageCode == language.LanguageCode &&
            t.TranslationKey == normalizedKey &&
            t.Module == module);

        if (existing == null)
        {
            var translation = new Translation
            {
                LanguageCode = language.LanguageCode,
                TranslationKey = normalizedKey,
                TranslationValue = value,
                Module = module,
                OrderNum = 0
            };
            _translationRepository.Create(translation, "Takt365");
        }
        else
        {
            existing.TranslationValue = value;
            existing.Module = module;
            _translationRepository.Update(existing, "Takt365");
        }
    }

    private static List<TranslationSeed> BuildTranslationSeeds()
    {
        return new List<TranslationSeed>
        {
            // 用户实体字段（严格对齐实体）
            new("identity.user.entity", "Frontend", "用户", "User", "ユーザー"),
            new("identity.user.query.keyword", "Frontend", "用户名、手机等", "username, phone,etc", "ユーザー名、電話など"),
            new("identity.user.username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("identity.user.avatar", "Frontend", "头像", "Avatar", "アバター"),
            new("identity.user.password", "Frontend", "密码", "Password", "パスワード"),
            new("identity.user.passwordconfirm", "Frontend", "确认密码", "Confirm Password", "パスワード確認"),
            new("identity.user.email", "Frontend", "邮箱", "Email", "メール"),
            new("identity.user.phone", "Frontend", "手机号", "Phone", "携帯番号"),
            new("identity.user.realname", "Frontend", "真实姓名", "Real Name", "氏名"),
            new("identity.user.nickname", "Frontend", "昵称", "Nickname", "ニックネーム"),
            new("identity.user.usertype", "Frontend", "用户类型", "User Type", "ユーザー種別"),
            new("identity.user.usergender", "Frontend", "性别", "Gender", "性別"),
            new("identity.user.userstatus", "Frontend", "状态", "Status", "状態"),
            new("identity.user.roles", "Frontend", "角色用户", "Role Users", "ロールユーザー"),
            
            // 角色实体字段（严格对齐实体）
            new("identity.role.entity", "Frontend", "角色", "Role", "ロール"),
            new("identity.role.query.keyword", "Frontend", "角色名称、编码等", "role name, code,etc", "ロール名、コードなど"),
            new("identity.role.rolename", "Frontend", "角色名称", "Role Name", "ロール名"),
            new("identity.role.rolecode", "Frontend", "角色编码", "Role Code", "ロールコード"),
            new("identity.role.description", "Frontend", "描述", "Description", "説明"),
            new("identity.role.datascope", "Frontend", "数据范围", "Data Scope", "データ範囲"),
            new("identity.role.usercount", "Frontend", "用户数", "User Count", "ユーザー数"),
            new("identity.role.ordernum", "Frontend", "排序号", "Order Num", "並び順"),
            new("identity.role.rolestatus", "Frontend", "状态", "Status", "状態"),
            new("identity.role.users", "Frontend", "角色用户", "Role Users", "ロールユーザー"),
            new("identity.role.menus", "Frontend", "角色菜单", "Role Menus", "ロールメニュー"),

            // 菜单实体字段（严格对齐实体）
            new("identity.menu.entity", "Frontend", "菜单", "Menu", "メニュー"),
            new("identity.menu.query.keyword", "Frontend", "菜单名称、编码等", "menu name, code,etc", "メニュー名、コードなど"),
            new("identity.menu.menuname", "Frontend", "菜单名称", "Menu Name", "メニュー名"),
            new("identity.menu.menucode", "Frontend", "菜单编码", "Menu Code", "メニューコード"),
            new("identity.menu.i18nkey", "Frontend", "国际化键", "I18n Key", "多言語キー"),
            new("identity.menu.permcode", "Frontend", "权限码", "Permission Code", "権限コード"),
            new("identity.menu.menutype", "Frontend", "菜单类型", "Menu Type", "メニュー種別"),
            new("identity.menu.parentid", "Frontend", "父级ID", "Parent ID", "上位ID"),
            new("identity.menu.routepath", "Frontend", "路由路径", "Route Path", "ルートパス"),
            new("identity.menu.icon", "Frontend", "图标", "Icon", "アイコン"),
            new("identity.menu.component", "Frontend", "组件路径", "Component Path", "コンポーネントパス"),
            new("identity.menu.isexternal", "Frontend", "是否外链", "Is External", "外部リンクか"),
            new("identity.menu.iscache", "Frontend", "是否缓存", "Is Cache", "キャッシュか"),
            new("identity.menu.isvisible", "Frontend", "是否可见", "Is Visible", "表示か"),
            new("identity.menu.ordernum", "Frontend", "排序号", "Order Num", "並び順"),
            new("identity.menu.menustatus", "Frontend", "状态", "Status", "状態"),
            new("identity.menu.roles", "Frontend", "角色菜单", "Role Menus", "ロールメニュー"),

            // 用户会话实体字段
            new("identity.usersession.entity", "Frontend", "用户会话", "User Session", "ユーザーセッション"),
            new("identity.usersession.query.keyword", "Frontend", "用户名、手机等", "username, phone,etc", "ユーザー名、電話など"),
            new("identity.usersession.sessionid", "Frontend", "会话ID", "Session ID", "セッションID"),
            new("identity.usersession.userid", "Frontend", "用户ID", "User ID", "ユーザーID"),
            new("identity.usersession.username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("identity.usersession.realname", "Frontend", "真实姓名", "Real Name", "氏名"),
            new("identity.usersession.roleid", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("identity.usersession.rolename", "Frontend", "角色名称", "Role Name", "ロール名"),
            new("identity.usersession.logintime", "Frontend", "登录时间", "Login Time", "ログイン時間"),
            new("identity.usersession.expiresat", "Frontend", "过期时间", "Expires At", "有効期限"),
            new("identity.usersession.lastactivitytime", "Frontend", "最后活动时间", "Last Activity Time", "最終活動時刻"),
            new("identity.usersession.loginip", "Frontend", "登录IP", "Login IP", "ログインIP"),
            new("identity.usersession.clientinfo", "Frontend", "客户端信息", "Client Info", "クライアント情報"),
            new("identity.usersession.clientsnapshot", "Frontend", "客户端快照", "Client Snapshot", "クライアントスナップショット"),
            new("identity.usersession.osdescription", "Frontend", "操作系统描述", "OS Description", "OS説明"),
            new("identity.usersession.osversion", "Frontend", "操作系统版本", "OS Version", "OSバージョン"),
            new("identity.usersession.ostype", "Frontend", "操作系统类型", "OS Type", "OS種別"),
            new("identity.usersession.osarchitecture", "Frontend", "操作系统架构", "OS Architecture", "OSアーキテクチャ"),
            new("identity.usersession.machinename", "Frontend", "机器名称", "Machine Name", "マシン名"),
            new("identity.usersession.macaddress", "Frontend", "MAC地址", "MAC Address", "MACアドレス"),
            new("identity.usersession.frameworkversion", "Frontend", ".NET运行时版本", ".NET Runtime Version", ".NETランタイム"),
            new("identity.usersession.processarchitecture", "Frontend", "进程架构", "Process Architecture", "プロセスアーキテクチャ"),
            new("identity.usersession.isactive", "Frontend", "是否活跃", "Is Active", "有効か"),
            new("identity.usersession.logouttime", "Frontend", "登出时间", "Logout Time", "ログアウト時間"),
            new("identity.usersession.logoutreason", "Frontend", "登出原因", "Logout Reason", "ログアウト理由"),

            // 角色菜单/用户角色关联实体字段
            new("identity.rolemenu.entity", "Frontend", "角色菜单", "Role Menu", "ロールメニュー"),
            new("identity.rolemenu.query.keyword", "Frontend", "角色ID、菜单ID等", "role id, menu id,etc", "ロールID、メニューIDなど"),
            new("identity.rolemenu.roleid", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("identity.rolemenu.menuid", "Frontend", "菜单ID", "Menu ID", "メニューID"),
            new("identity.userrole.entity", "Frontend", "用户角色", "User Role", "ユーザーロール"),
            new("identity.userrole.query.keyword", "Frontend", "用户名、角色名称等", "username, role name,etc", "ユーザー名、ロール名など"),
            new("identity.userrole.userid", "Frontend", "用户ID", "User ID", "ユーザーID"),
            new("identity.userrole.roleid", "Frontend", "角色ID", "Role ID", "ロールID"),

            // 字典实体字段（严格对齐实体）
            new("routine.dictionarytype.entity", "Frontend", "字典类型", "Dictionary Type", "辞書タイプ"),
            new("routine.dictionarytype.query.keyword", "Frontend", "类型代码、类型名称等", "type code, type name,etc", "タイプコード、タイプ名など"),
            new("routine.dictionarytype.typecode", "Frontend", "类型代码", "Type Code", "タイプコード"),
            new("routine.dictionarytype.typename", "Frontend", "类型名称", "Type Name", "タイプ名"),
            new("routine.dictionarytype.datasource", "Frontend", "数据源", "Data Source", "データソース"),
            new("routine.dictionarytype.sqlscript", "Frontend", "SQL脚本", "SQL Script", "SQLスクリプト"),
            new("routine.dictionarytype.ordernum", "Frontend", "排序号", "Order Number", "順序"),
            new("routine.dictionarytype.isbuiltin", "Frontend", "内置", "Is Built-in", "内蔵か"),
            new("routine.dictionarytype.typestatus", "Frontend", "状态", "Status", "状態"),
            new("routine.dictionarydata.entity", "Frontend", "字典数据", "Dictionary Data", "辞書データ"),
            new("routine.dictionarydata.query.keyword", "Frontend", "数据标签、数据值、国际化键", "data label, data value, i18n key", "データラベル、データ値、多言語キー"),
            new("routine.dictionarydata.typecode", "Frontend", "类型代码", "Type Code", "タイプコード"),
            new("routine.dictionarydata.datalabel", "Frontend", "数据标签", "Data Label", "データラベル"),
            new("routine.dictionarydata.i18nkey", "Frontend", "国际化键", "I18n Key", "多言語キー"),
            new("routine.dictionarydata.datavalue", "Frontend", "数据值", "Data Value", "データ値"),
            new("routine.dictionarydata.extlabel", "Frontend", "扩展标签", "Ext Label", "拡張ラベル"),
            new("routine.dictionarydata.extvalue", "Frontend", "扩展值", "Ext Value", "拡張値"),
            new("routine.dictionarydata.cssclass", "Frontend", "CSS类名", "CSS Class", "CSSクラス"),
            new("routine.dictionarydata.listclass", "Frontend", "列表CSS类名", "List CSS Class", "リストCSSクラス"),
            new("routine.dictionarydata.ordernum", "Frontend", "排序号", "Order Number", "順序"),
            
            new("routine.language.entity", "Frontend", "语言", "Language", "言語"),
            new("routine.language.query.keyword", "Frontend", "语言代码、语言名称、本地化名称", "language code, language name, native name", "言語コード、言語名、現地名"),
            new("routine.language.languagecode", "Frontend", "语言代码", "Language Code", "言語コード"),
            new("routine.language.languagename", "Frontend", "语言名称", "Language Name", "言語名"),
            new("routine.language.nativename", "Frontend", "本地化名称", "Native Name", "現地名"),
            new("routine.language.languageicon", "Frontend", "语言图标", "Language Icon", "言語アイコン"),
            new("routine.language.isdefault", "Frontend", "是否默认", "Is Default", "既定か"),
            new("routine.language.ordernum", "Frontend", "排序号", "Order Number", "順序"),
            new("routine.language.isbuiltin", "Frontend", "是否内置", "Is Built-in", "内蔵か"),
            new("routine.language.languagestatus", "Frontend", "语言状态", "Language Status", "言語状態"),
            new("routine.translation.entity", "Frontend", "翻译", "Translation", "翻訳"),
            new("routine.translation.query.keyword", "Frontend", "语言代码、翻译键等", "language code, translation key,etc", "言語コード、翻訳キーなど"),
            new("routine.translation.languagecode", "Frontend", "语言代码", "Language Code", "言語コード"),
            new("routine.translation.translationkey", "Frontend", "翻译键", "Translation Key", "翻訳キー"),
            new("routine.translation.translationvalue", "Frontend", "翻译值", "Translation Value", "翻訳値"),
            new("routine.translation.module", "Frontend", "模块", "Module", "モジュール"),
            new("routine.translation.description", "Frontend", "描述", "Description", "説明"),
            new("routine.translation.ordernum", "Frontend", "排序号", "Order Number", "順序"),
            // 设置实体字段（严格对齐实体）
            new("routine.setting.entity", "Frontend", "系统设置", "System Setting", "システム設定"),
            new("routine.setting.query.keyword", "Frontend", "配置键、分类等", "setting key, category,etc", "設定キー、カテゴリなど"),
            new("routine.setting.key", "Frontend", "配置键", "Setting Key", "設定キー"),
            new("routine.setting.value", "Frontend", "配置值", "Setting Value", "設定値"),
            new("routine.setting.category", "Frontend", "分类", "Category", "カテゴリ"),
            new("routine.setting.ordernum", "Frontend", "排序号", "Order Num", "並び順"),
            new("routine.setting.type", "Frontend", "配置类型", "Setting Type", "設定タイプ"),
            new("routine.setting.isbuiltin", "Frontend", "是否内置", "Is Built-in", "内蔵か"),
            new("routine.setting.isdefault", "Frontend", "是否默认", "Is Default", "デフォルトか"),
            new("routine.setting.iseditable", "Frontend", "是否可修改", "Is Editable", "編集可か"),
            new("routine.setting.description", "Frontend", "设置描述", "Description", "説明"),

            // 任务管理实体字段（严格对齐实体）
            new("routine.quartzjob.entity", "Frontend", "任务管理", "Job Management", "タスク管理"),
            new("routine.quartzjob.query.keyword", "Frontend", "任务名称、任务组、任务描述", "job name, job group, job description", "タスク名、タスクグループ、タスク説明"),
            new("routine.quartzjob.jobname", "Frontend", "任务名称", "Job Name", "タスク名"),
            new("routine.quartzjob.jobgroup", "Frontend", "任务组", "Job Group", "タスクグループ"),
            new("routine.quartzjob.triggername", "Frontend", "触发器名称", "Trigger Name", "トリガー名"),
            new("routine.quartzjob.triggergroup", "Frontend", "触发器组", "Trigger Group", "トリガーグループ"),
            new("routine.quartzjob.cronexpression", "Frontend", "Cron表达式", "Cron Expression", "Cron式"),
            new("routine.quartzjob.jobclassname", "Frontend", "任务类名", "Job Class Name", "タスククラス名"),
            new("routine.quartzjob.jobdescription", "Frontend", "任务描述", "Job Description", "タスク説明"),
            new("routine.quartzjob.status", "Frontend", "任务状态", "Status", "状態"),
            new("routine.quartzjob.jobparams", "Frontend", "任务参数", "Job Parameters", "タスクパラメータ"),
            new("routine.quartzjob.lastruntime", "Frontend", "最后执行时间", "Last Run Time", "最終実行時刻"),
            new("routine.quartzjob.nextruntime", "Frontend", "下次执行时间", "Next Run Time", "次回実行時刻"),
            new("routine.quartzjob.runcount", "Frontend", "执行次数", "Run Count", "実行回数"),

            // 任务日志实体字段
            new("logging.quartzjoblog.entity", "Frontend", "任务日志", "Job Log", "タスクログ"),
            new("logging.quartzjoblog.query.keyword", "Frontend", "任务名称、任务组等", "job name, job group,etc", "タスク名、タスクグループなど"),
            new("logging.quartzjoblog.quartzid", "Frontend", "任务ID", "Job ID", "タスクID"),
            new("logging.quartzjoblog.jobname", "Frontend", "任务名称", "Job Name", "タスク名"),
            new("logging.quartzjoblog.jobgroup", "Frontend", "任务组", "Job Group", "タスクグループ"),
            new("logging.quartzjoblog.triggername", "Frontend", "触发器名称", "Trigger Name", "トリガー名"),
            new("logging.quartzjoblog.triggergroup", "Frontend", "触发器组", "Trigger Group", "トリガーグループ"),
            new("logging.quartzjoblog.starttime", "Frontend", "开始时间", "Start Time", "開始時間"),
            new("logging.quartzjoblog.endtime", "Frontend", "结束时间", "End Time", "終了時間"),
            new("logging.quartzjoblog.elapsedtime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("logging.quartzjoblog.executeresult", "Frontend", "执行结果", "Execute Result", "実行結果"),
            new("logging.quartzjoblog.errormessage", "Frontend", "错误信息", "Error Message", "エラーメッセージ"),
            new("logging.quartzjoblog.jobparams", "Frontend", "执行参数", "Job Parameters", "実行パラメーター"),

            // 登录日志实体字段
            new("logging.loginlog.entity", "Frontend", "登录日志", "Login Log", "ログインログ"),
            new("logging.loginlog.query.keyword", "Frontend", "用户名、登录时间等", "username, login time,etc", "ユーザー名、ログイン時間など"),
            new("logging.loginlog.username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("logging.loginlog.logintime", "Frontend", "登录时间", "Login Time", "ログイン時間"),
            new("logging.loginlog.logouttime", "Frontend", "登出时间", "Logout Time", "ログアウト時間"),
            new("logging.loginlog.loginip", "Frontend", "登录IP", "Login IP", "ログインIP"),
            new("logging.loginlog.macaddress", "Frontend", "MAC地址", "MAC Address", "MACアドレス"),
            new("logging.loginlog.machinename", "Frontend", "机器名称", "Machine Name", "マシン名"),
            new("logging.loginlog.loginlocation", "Frontend", "登录地点", "Login Location", "ログイン場所"),
            new("logging.loginlog.client", "Frontend", "客户端", "Client", "クライアント"),
            new("logging.loginlog.os", "Frontend", "操作系统", "Operating System", "OS"),
            new("logging.loginlog.osversion", "Frontend", "操作系统版本", "OS Version", "OSバージョン"),
            new("logging.loginlog.osarchitecture", "Frontend", "系统架构", "OS Architecture", "OSアーキテクチャ"),
            new("logging.loginlog.cpuinfo", "Frontend", "CPU信息", "CPU Info", "CPU情報"),
            new("logging.loginlog.totalmemorygb", "Frontend", "物理内存(GB)", "Total Memory (GB)", "物理メモリ(GB)"),
            new("logging.loginlog.frameworkversion", "Frontend", ".NET运行时", ".NET Runtime", ".NETランタイム"),
            new("logging.loginlog.isadmin", "Frontend", "是否管理员", "Is Admin", "管理者か"),
            new("logging.loginlog.clienttype", "Frontend", "客户端类型", "Client Type", "クライアント種別"),
            new("logging.loginlog.clientversion", "Frontend", "客户端版本", "Client Version", "クライアントバージョン"),
            new("logging.loginlog.loginstatus", "Frontend", "登录状态", "Login Status", "ログイン状態"),
            new("logging.loginlog.failreason", "Frontend", "失败原因", "Fail Reason", "失敗理由"),

            // 操作日志实体字段
            new("logging.operlog.entity", "Frontend", "操作日志", "Operation Log", "操作ログ"),
            new("logging.operlog.query.keyword", "Frontend", "用户名、操作类型等", "username, operation type,etc", "ユーザー名、操作種別など"),
            new("logging.operlog.username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("logging.operlog.operationtype", "Frontend", "操作类型", "Operation Type", "操作種別"),
            new("logging.operlog.operationmodule", "Frontend", "操作模块", "Operation Module", "操作モジュール"),
            new("logging.operlog.operationdesc", "Frontend", "操作描述", "Operation Description", "操作説明"),
            new("logging.operlog.operationtime", "Frontend", "操作时间", "Operation Time", "操作時間"),
            new("logging.operlog.requestpath", "Frontend", "请求路径", "Request Path", "リクエストパス"),
            new("logging.operlog.requestmethod", "Frontend", "请求方法", "Request Method", "リクエスト方法"),
            new("logging.operlog.requestparams", "Frontend", "请求参数", "Request Parameters", "リクエストパラメーター"),
            new("logging.operlog.responseresult", "Frontend", "响应结果", "Response Result", "応答結果"),
            new("logging.operlog.elapsedtime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("logging.operlog.ipaddress", "Frontend", "IP地址", "IP Address", "IPアドレス"),
            new("logging.operlog.useragent", "Frontend", "用户代理", "User Agent", "ユーザーエージェント"),
            new("logging.operlog.os", "Frontend", "操作系统", "Operating System", "OS"),
            new("logging.operlog.browser", "Frontend", "浏览器", "Browser", "ブラウザー"),
            new("logging.operlog.operationresult", "Frontend", "操作结果", "Operation Result", "操作結果"),

            // 差异日志实体字段
            new("logging.difflog.entity", "Frontend", "差异日志", "Diff Log", "差分ログ"),
            new("logging.difflog.query.keyword", "Frontend", "表名、差异类型等", "table name, diff type,etc", "テーブル名、差分種別など"),
            new("logging.difflog.tablename", "Frontend", "表名", "Table Name", "テーブル名"),
            new("logging.difflog.difftype", "Frontend", "差异类型", "Diff Type", "差分種別"),
            new("logging.difflog.businessdata", "Frontend", "业务数据", "Business Data", "業務データ"),
            new("logging.difflog.beforedata", "Frontend", "变更前数据", "Before Data", "変更前データ"),
            new("logging.difflog.afterdata", "Frontend", "变更后数据", "After Data", "変更後データ"),
            new("logging.difflog.sql", "Frontend", "执行SQL", "Executed SQL", "実行SQL"),
            new("logging.difflog.parameters", "Frontend", "SQL参数", "SQL Parameters", "SQLパラメーター"),
            new("logging.difflog.difftime", "Frontend", "差异时间", "Diff Time", "差分時間"),
            new("logging.difflog.elapsedtime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("logging.difflog.username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("logging.difflog.ipaddress", "Frontend", "IP地址", "IP Address", "IPアドレス"),

            // 生产物料实体字段
            new("logistics.materials.prodmaterial.entity", "Frontend", "生产物料", "Production Material", "生産資材"),
            new("logistics.materials.prodmaterial.query.keyword", "Frontend", "物料编码、物料名称等", "material code, material name,etc", "資材コード、資材名など"),
            new("logistics.materials.prodmaterial.plant", "Frontend", "工厂", "Plant", "工場"),
            new("logistics.materials.prodmaterial.materialcode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("logistics.materials.prodmaterial.industryfield", "Frontend", "行业领域", "Industry Field", "業界分野"),
            new("logistics.materials.prodmaterial.materialtype", "Frontend", "物料类型", "Material Type", "資材種別"),
            new("logistics.materials.prodmaterial.materialdescription", "Frontend", "物料描述", "Material Description", "資材説明"),
            new("logistics.materials.prodmaterial.baseunit", "Frontend", "基本计量单位", "Base Unit", "基準単位"),
            new("logistics.materials.prodmaterial.producthierarchy", "Frontend", "产品层次", "Product Hierarchy", "製品階層"),
            new("logistics.materials.prodmaterial.materialgroup", "Frontend", "物料组", "Material Group", "資材グループ"),
            new("logistics.materials.prodmaterial.purchasegroup", "Frontend", "采购组", "Purchase Group", "購買グループ"),
            new("logistics.materials.prodmaterial.purchasetype", "Frontend", "采购类型", "Purchase Type", "購買種別"),
            new("logistics.materials.prodmaterial.specialpurchasetype", "Frontend", "特殊采购类", "Special Purchase Type", "特別購買種"),
            new("logistics.materials.prodmaterial.bulkmaterial", "Frontend", "散装物料", "Bulk Material", "バルク資材"),
            new("logistics.materials.prodmaterial.minimumorderquantity", "Frontend", "最小起订量", "Minimum Order Quantity", "最小発注量"),
            new("logistics.materials.prodmaterial.roundingvalue", "Frontend", "舍入值", "Rounding Value", "丸め値"),
            new("logistics.materials.prodmaterial.planneddeliverytime", "Frontend", "计划交货时间", "Planned Delivery Time", "計画納期"),
            new("logistics.materials.prodmaterial.selfproductiondays", "Frontend", "自制生产天数", "Self Production Days", "自社生産日数"),
            new("logistics.materials.prodmaterial.posttoinspectionstock", "Frontend", "过账到检验库存", "Post To Inspection Stock", "検査在庫へ転記"),
            new("logistics.materials.prodmaterial.profitcenter", "Frontend", "利润中心", "Profit Center", "損益センター"),
            new("logistics.materials.prodmaterial.variancecode", "Frontend", "差异码", "Variance Code", "差異コード"),
            new("logistics.materials.prodmaterial.batchmanagement", "Frontend", "批次管理", "Batch Management", "ロット管理"),
            new("logistics.materials.prodmaterial.manufacturerpartnumber", "Frontend", "制造商零件编号", "Manufacturer Part Number", "メーカー部品番号"),
            new("logistics.materials.prodmaterial.manufacturer", "Frontend", "制造商", "Manufacturer", "メーカー"),
            new("logistics.materials.prodmaterial.evaluationtype", "Frontend", "评估类", "Evaluation Type", "評価種類"),
            new("logistics.materials.prodmaterial.movingaverageprice", "Frontend", "移动平均价", "Moving Average Price", "移動平均価格"),
            new("logistics.materials.prodmaterial.currency", "Frontend", "货币", "Currency", "通貨"),
            new("logistics.materials.prodmaterial.pricecontrol", "Frontend", "价格控制", "Price Control", "価格管理"),
            new("logistics.materials.prodmaterial.priceunit", "Frontend", "价格单位", "Price Unit", "価格単位"),
            new("logistics.materials.prodmaterial.productionstoragelocation", "Frontend", "生产仓储地点", "Production Storage Location", "生産保管場所"),
            new("logistics.materials.prodmaterial.externalpurchasestoragelocation", "Frontend", "外部采购仓储地点", "External Purchase Storage Location", "外部調達保管場所"),
            new("logistics.materials.prodmaterial.storageposition", "Frontend", "仓位", "Storage Position", "保管位置"),
            new("logistics.materials.prodmaterial.crossplantmaterialstatus", "Frontend", "跨工厂物料状态", "Cross-Plant Material Status", "プラント間資材状態"),
            new("logistics.materials.prodmaterial.stockquantity", "Frontend", "在库数量", "Stock Quantity", "在庫数量"),
            new("logistics.materials.prodmaterial.hscode", "Frontend", "HS编码", "HS Code", "HSコード"),
            new("logistics.materials.prodmaterial.hsname", "Frontend", "HS名称", "HS Name", "HS名称"),
            new("logistics.materials.prodmaterial.materialweight", "Frontend", "重量", "Material Weight", "重量"),
            new("logistics.materials.prodmaterial.materialvolume", "Frontend", "容积", "Material Volume", "容積"),

            // 产品机种实体字段
            new("logistics.materials.prodmodel.entity", "Frontend", "产品机种", "Product Model", "製品機種"),
            new("logistics.materials.prodmodel.query.keyword", "Frontend", "物料编码、机种编码、仕向编码", "material code, model code, destination code", "資材コード、機種コード、仕向コード"),
            new("logistics.materials.prodmodel.materialcode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("logistics.materials.prodmodel.modelcode", "Frontend", "机种编码", "Model Code", "機種コード"),
            new("logistics.materials.prodmodel.destcode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),

            // 包装信息实体字段
            new("logistics.packing.entity", "Frontend", "包装信息", "Packing Information", "包装情報"),
            new("logistics.packing.query.keyword", "Frontend", "物料编码等", "material code,etc", "資材コードなど"),
            new("logistics.packing.materialcode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("logistics.packing.packingtype", "Frontend", "包装类型", "Packing Type", "包装種別"),
            new("logistics.packing.packingunit", "Frontend", "包装单位", "Packing Unit", "包装単位"),
            new("logistics.packing.grossweight", "Frontend", "毛重", "Gross Weight", "総重量"),
            new("logistics.packing.netweight", "Frontend", "净重", "Net Weight", "正味重量"),
            new("logistics.packing.weightunit", "Frontend", "重量单位", "Weight Unit", "重量単位"),
            new("logistics.packing.businessvolume", "Frontend", "业务量", "Business Volume", "容积"),
            new("logistics.packing.volumeunit", "Frontend", "体积单位", "Volume Unit", "体積単位"),
            new("logistics.packing.sizedimension", "Frontend", "大小/量纲", "Size Dimension", "サイズ/次元"),
            new("logistics.packing.quantityperpacking", "Frontend", "每包装数量", "Quantity Per Packing", "包装あたり数量"),

            // 产品序列号主表字段
            new("logistics.serials.prodserial.entity", "Frontend", "产品序列号", "Product Serial", "製品シリアル"),
            new("logistics.serials.prodserial.query.keyword", "Frontend", "物料编码、机种编码、仕向编码", "material code, model code, destination code", "資材コード、機種コード、仕向コード"),
            new("logistics.serials.prodserial.materialcode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("logistics.serials.prodserial.modelcode", "Frontend", "机种编码", "Model Code", "機種コード"),
            new("logistics.serials.prodserial.destcode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),

            // 产品序列号入库字段
            new("logistics.serials.prodserialinbound.entity", "Frontend", "序列号入库", "Serial Inbound", "シリアル入庫"),
            new("logistics.serials.prodserialinbound.query.keyword", "Frontend", "完整序列号、物料编码等", "full serial number, material code,etc", "完全シリアル番号、資材コードなど"),
            new("logistics.serials.prodserialinbound.fullserialnumber", "Frontend", "完整序列号", "Full Serial Number", "完全シリアル番号"),
            new("logistics.serials.prodserialinbound.materialcode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("logistics.serials.prodserialinbound.serialnumber", "Frontend", "真正序列号", "Serial Number", "シリアル番号"),
            new("logistics.serials.prodserialinbound.quantity", "Frontend", "数量", "Quantity", "数量"),
            new("logistics.serials.prodserialinbound.inboundno", "Frontend", "入库单号", "Inbound No.", "入庫番号"),
            new("logistics.serials.prodserialinbound.inbounddate", "Frontend", "入库日期", "Inbound Date", "入庫日"),
            new("logistics.serials.prodserialinbound.warehouse", "Frontend", "仓库", "Warehouse", "倉庫"),
            new("logistics.serials.prodserialinbound.location", "Frontend", "库位", "Location", "ロケーション"),

            // 产品序列号出库字段
            new("logistics.serials.prodserialoutbound.entity", "Frontend", "序列号出库", "Serial Outbound", "シリアル出庫"),
            new("logistics.serials.prodserialoutbound.query.keyword", "Frontend", "完整序列号、物料编码等", "full serial number, material code,etc", "完全シリアル番号、資材コードなど"),
            new("logistics.serials.prodserialoutbound.fullserialnumber", "Frontend", "完整序列号", "Full Serial Number", "完全シリアル番号"),
            new("logistics.serials.prodserialoutbound.materialcode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("logistics.serials.prodserialoutbound.serialnumber", "Frontend", "真正序列号", "Serial Number", "シリアル番号"),
            new("logistics.serials.prodserialoutbound.quantity", "Frontend", "数量", "Quantity", "数量"),
            new("logistics.serials.prodserialoutbound.outboundno", "Frontend", "出库单号", "Outbound No.", "出庫番号"),
            new("logistics.serials.prodserialoutbound.outbounddate", "Frontend", "出库日期", "Outbound Date", "出庫日"),
            new("logistics.serials.prodserialoutbound.destcode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("logistics.serials.prodserialoutbound.destport", "Frontend", "目的地港口", "Destination Port", "目的地港"),

            // 产品序列号扫描记录字段
            new("logistics.serials.prodserialscanning.entity", "Frontend", "序列号扫描记录", "Serial Scanning Record", "シリアルスキャン記録"),
            new("logistics.serials.prodserialscanning.query.keyword", "Frontend", "入库完整序列号等", "inbound full serial number,etc", "入庫完全シリアル番号など"),
            new("logistics.serials.prodserialscanning.inboundfullserialnumber", "Frontend", "入库完整序列号", "Inbound Full Serial Number", "入庫完全シリアル番号"),
            new("logistics.serials.prodserialscanning.inbounddate", "Frontend", "入库日期", "Inbound Date", "入庫日"),
            new("logistics.serials.prodserialscanning.inboundclient", "Frontend", "入库用户", "Inbound User", "入庫ユーザー"),
            new("logistics.serials.prodserialscanning.inboundip", "Frontend", "入库IP", "Inbound IP", "入庫IP"),
            new("logistics.serials.prodserialscanning.inboundmachinename", "Frontend", "入库机器名称", "Inbound Machine Name", "入庫マシン名"),
            new("logistics.serials.prodserialscanning.inboundlocation", "Frontend", "入库地点", "Inbound Location", "入庫場所"),
            new("logistics.serials.prodserialscanning.inboundos", "Frontend", "入库OS", "Inbound OS", "入庫OS"),
            new("logistics.serials.prodserialscanning.outboundfullserialnumber", "Frontend", "出库完整序列号", "Outbound Full Serial Number", "出庫完全シリアル番号"),
            new("logistics.serials.prodserialscanning.outbounddate", "Frontend", "出库日期", "Outbound Date", "出庫日"),
            new("logistics.serials.prodserialscanning.outboundclient", "Frontend", "出库用户", "Outbound User", "出庫ユーザー"),
            new("logistics.serials.prodserialscanning.outboundip", "Frontend", "出库IP", "Outbound IP", "出庫IP"),
            new("logistics.serials.prodserialscanning.outboundmachinename", "Frontend", "出库机器名称", "Outbound Machine Name", "出庫マシン名"),
            new("logistics.serials.prodserialscanning.outboundlocation", "Frontend", "出库地点", "Outbound Location", "出庫場所"),
            new("logistics.serials.prodserialscanning.outboundos", "Frontend", "出库OS", "Outbound OS", "出庫OS"),

            // 产品序列号扫描异常记录字段
            new("logistics.serials.prodserialscanningex.entity", "Frontend", "序列号扫描异常记录", "Serial Scanning Exception Record", "シリアルスキャン異常記録"),
            new("logistics.serials.prodserialscanningex.query.keyword", "Frontend", "入库完整序列号等", "inbound full serial number,etc", "入庫完全シリアル番号など"),
            new("logistics.serials.prodserialscanningex.inboundfullserialnumber", "Frontend", "入库完整序列号", "Inbound Full Serial Number", "入庫完全シリアル番号"),
            new("logistics.serials.prodserialscanningex.inbounddate", "Frontend", "入库日期", "Inbound Date", "入庫日"),
            new("logistics.serials.prodserialscanningex.inboundclient", "Frontend", "入库用户", "Inbound User", "入庫ユーザー"),
            new("logistics.serials.prodserialscanningex.inboundip", "Frontend", "入库IP", "Inbound IP", "入庫IP"),
            new("logistics.serials.prodserialscanningex.inboundmachinename", "Frontend", "入库机器名称", "Inbound Machine Name", "入庫マシン名"),
            new("logistics.serials.prodserialscanningex.inboundlocation", "Frontend", "入库地点", "Inbound Location", "入庫場所"),
            new("logistics.serials.prodserialscanningex.inboundos", "Frontend", "入库OS", "Inbound OS", "入庫OS"),
            new("logistics.serials.prodserialscanningex.inbounddesc", "Frontend", "入库异常描述", "Inbound Exception Description", "入庫異常説明"),
            new("logistics.serials.prodserialscanningex.outboundno", "Frontend", "出库单号", "Outbound No.", "出庫番号"),
            new("logistics.serials.prodserialscanningex.destcode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("logistics.serials.prodserialscanningex.destport", "Frontend", "目的地港口", "Destination Port", "目的地港"),
            new("logistics.serials.prodserialscanningex.outbounddate", "Frontend", "出库日期", "Outbound Date", "出庫日"),
            new("logistics.serials.prodserialscanningex.outboundfullserialnumber", "Frontend", "出库完整序列号", "Outbound Full Serial Number", "出庫完全シリアル番号"),
            new("logistics.serials.prodserialscanningex.outboundclient", "Frontend", "出库用户", "Outbound User", "出庫ユーザー"),
            new("logistics.serials.prodserialscanningex.outboundip", "Frontend", "出库IP", "Outbound IP", "出庫IP"),
            new("logistics.serials.prodserialscanningex.outboundmachinename", "Frontend", "出库机器名称", "Outbound Machine Name", "出庫マシン名"),
            new("logistics.serials.prodserialscanningex.outboundlocation", "Frontend", "出库地点", "Outbound Location", "出庫場所"),
            new("logistics.serials.prodserialscanningex.outboundos", "Frontend", "出库OS", "Outbound OS", "出庫OS"),
            new("logistics.serials.prodserialscanningex.outbounddesc", "Frontend", "出库异常描述", "Outbound Exception Description", "出庫異常説明"),

            // 来访公司实体字段
            new("logistics.visits.visitingcompany.entity", "Frontend", "来访公司信息", "Visiting Company", "訪問会社情報"),
            new("logistics.visits.visitingcompany.query.keyword", "Frontend", "公司名称、起始时间、结束时间等", "visiting company name, visit start time, visit end time,etc", "訪問会社名、訪問開始時間、訪問終了時間など"),
            new("logistics.visits.visitingcompany.visitingcompanyname", "Frontend", "公司名称", "Visiting Company Name", "訪問会社名"),
            new("logistics.visits.visitingcompany.visitstarttime", "Frontend", "起始时间", "Visit Start Time", "訪問開始時間"),
            new("logistics.visits.visitingcompany.visitendtime", "Frontend", "结束时间", "Visit End Time", "訪問終了時間"),
            new("logistics.visits.visitingcompany.reservationsdept", "Frontend", "预约部门", "Reservations Department", "予約部門"),
            new("logistics.visits.visitingcompany.contact", "Frontend", "联系人", "Contact", "連絡先"),
            new("logistics.visits.visitingcompany.purpose", "Frontend", "访问目的", "Purpose", "訪問目的"),
            new("logistics.visits.visitingcompany.duration", "Frontend", "预计时长", "Duration", "予定期間"),
            new("logistics.visits.visitingcompany.industry", "Frontend", "所属行业", "Industry", "所属業界"),
            new("logistics.visits.visitingcompany.vehicleplate", "Frontend", "车牌号", "Vehicle Plate", "ナンバープレート"),
            new("logistics.visits.visitingcompany.iswelcomesign", "Frontend", "欢迎牌显示", "Welcome Sign", "歓迎看板表示"),
            new("logistics.visits.visitingcompany.isvehicleneeded", "Frontend", "是否用车", "Vehicle Needed", "車両必要か"),

            // 来访成员详情实体字段
            new("logistics.visits.visitingentourage.entity", "Frontend", "随行人员详情", "Visiting Entourage", "訪問者詳細"),
            new("logistics.visits.visitingentourage.query.keyword", "Frontend", "来访公司ID、来访部门等", "visiting company id, visit dept,etc", "訪問会社ID、訪問部門など"),
            new("logistics.visits.visitingentourage.visitingcompanyid", "Frontend", "来访公司ID", "Visiting Company ID", "訪問会社ID"),
            new("logistics.visits.visitingentourage.visitdept", "Frontend", "来访部门", "Visit Department", "訪問部門"),
            new("logistics.visits.visitingentourage.visitpost", "Frontend", "来访职务", "Visit Post", "訪問職務"),
            new("logistics.visits.visitingentourage.visitingmembers", "Frontend", "来访成员", "Visiting Members", "訪問メンバー"),

            // 代码生成表配置实体字段
            new("generator.gentable.entity", "Frontend", "代码生成表配置", "Code Generation Table Config", "コード生成テーブル設定"),
            new("generator.gentable.query.keyword", "Frontend", "库表名称、库表描述等", "table name, table description,etc", "テーブル名、テーブル説明など"),
            new("generator.gentable.tablename", "Frontend", "库表名称", "Table Name", "テーブル名"),
            new("generator.gentable.tablenamehint", "Frontend", "提示：用于无数据表的手动配置，表名可以不存在于数据库中", "Hint: For manual configuration without database table, table name may not exist in database", "ヒント：データベーステーブルなしの手動設定用。テーブル名はデータベースに存在しない場合があります"),
            new("generator.gentable.tabledescription", "Frontend", "库表描述", "Table Description", "テーブル説明"),
            new("generator.gentable.classname", "Frontend", "实体类名称", "Class Name", "クラス名"),
            new("generator.gentable.namespace", "Frontend", "命名空间", "Namespace", "名前空間"),
            new("generator.gentable.modulecode", "Frontend", "模块标识", "Module Code", "モジュールコード"),
            new("generator.gentable.modulename", "Frontend", "模块名称", "Module Name", "モジュール名"),
            new("generator.gentable.parenttableid", "Frontend", "主表ID", "Parent Table ID", "親テーブルID"),
            new("generator.gentable.detailtablename", "Frontend", "子表名称", "Detail Table Name", "詳細テーブル名"),
            new("generator.gentable.detailcomment", "Frontend", "子表描述", "Detail Comment", "詳細コメント"),
            new("generator.gentable.detailrelationfield", "Frontend", "子表关联字段", "Detail Relation Field", "詳細関連フィールド"),
            new("generator.gentable.treecodefield", "Frontend", "树编码字段", "Tree Code Field", "ツリーコードフィールド"),
            new("generator.gentable.treeparentcodefield", "Frontend", "树父编码字段", "Tree Parent Code Field", "ツリー親コードフィールド"),
            new("generator.gentable.treenamefield", "Frontend", "树名称字段", "Tree Name Field", "ツリー名フィールド"),
            new("generator.gentable.author", "Frontend", "作者", "Author", "作成者"),
            new("generator.gentable.templatetype", "Frontend", "生成模板类型", "Template Type", "テンプレートタイプ"),
            new("generator.gentable.gennamespaceprefix", "Frontend", "命名空间前缀", "Namespace Prefix", "名前空間プレフィックス"),
            new("generator.gentable.genbusinessname", "Frontend", "生成业务名称", "Business Name", "ビジネス名"),
            new("generator.gentable.genmodulename", "Frontend", "生成模块名称", "Gen Module Name", "生成モジュール名"),
            new("generator.gentable.genfunctionname", "Frontend", "生成功能名", "Function Name", "機能名"),
            new("generator.gentable.gentype", "Frontend", "生成方式", "Gen Type", "生成タイプ"),
            new("generator.gentable.genfunctions", "Frontend", "生成功能", "Gen Functions", "生成機能"),
            new("generator.gentable.genpath", "Frontend", "代码生成路径", "Gen Path", "生成パス"),
            new("generator.gentable.options", "Frontend", "其它生成选项", "Options", "その他のオプション"),
            new("generator.gentable.parentmenuname", "Frontend", "上级菜单名称", "Parent Menu Name", "親メニュー名"),
            new("generator.gentable.permissionprefix", "Frontend", "权限前缀", "Permission Prefix", "権限プレフィックス"),
            new("generator.gentable.isdatabasetable", "Frontend", "是否有表", "Is Database Table", "テーブルがあるか"),
            new("generator.gentable.isgenmenu", "Frontend", "是否生成菜单", "Is Gen Menu", "メニュー生成か"),
            new("generator.gentable.isgentranslation", "Frontend", "是否生成翻译", "Is Gen Translation", "翻訳生成か"),
            new("generator.gentable.isgencode", "Frontend", "是否生成代码", "Is Gen Code", "コード生成か"),
            new("generator.gentable.defaultsortfield", "Frontend", "默认排序字段", "Default Sort Field", "デフォルトソートフィールド"),
            new("generator.gentable.defaultsortorder", "Frontend", "默认排序", "Default Sort Order", "デフォルトソート順"),

            // 代码生成列配置实体字段
            new("generator.gencolumn.entity", "Frontend", "代码生成列配置", "Code Generation Column Config", "コード生成カラム設定"),
            new("generator.gencolumn.query.keyword", "Frontend", "表名、列名等", "table name, column name,etc", "テーブル名、カラム名など"),
            new("generator.gencolumn.tablename", "Frontend", "表名", "Table Name", "テーブル名"),
            new("generator.gencolumn.columnname", "Frontend", "列名", "Column Name", "カラム名"),
            new("generator.gencolumn.columndescription", "Frontend", "列描述", "Column Description", "カラム説明"),
            new("generator.gencolumn.columndatatype", "Frontend", "库列类型", "Column Data Type", "カラムデータタイプ"),
            new("generator.gencolumn.propertyname", "Frontend", "属性名称", "Property Name", "プロパティ名"),
            new("generator.gencolumn.datatype", "Frontend", "C#类型", "C# Type", "C#タイプ"),
            new("generator.gencolumn.isnullable", "Frontend", "可空", "Nullable", "NULL許可"),
            new("generator.gencolumn.isprimarykey", "Frontend", "主键", "Primary Key", "主キー"),
            new("generator.gencolumn.isidentity", "Frontend", "自增", "Identity", "自動増分"),
            new("generator.gencolumn.length", "Frontend", "长度", "Length", "長さ"),
            new("generator.gencolumn.decimalplaces", "Frontend", "精度", "Decimal Places", "小数点以下桁数"),
            new("generator.gencolumn.defaultvalue", "Frontend", "默认值", "Default Value", "デフォルト値"),
            new("generator.gencolumn.ordernum", "Frontend", "库列排序", "Order Number", "順序"),
            new("generator.gencolumn.isquery", "Frontend", "查询", "Query", "クエリ"),
            new("generator.gencolumn.querytype", "Frontend", "查询方式", "Query Type", "クエリタイプ"),
            new("generator.gencolumn.iscreate", "Frontend", "创建", "Create", "作成"),
            new("generator.gencolumn.isupdate", "Frontend", "更新", "Update", "更新"),
            new("generator.gencolumn.isdelete", "Frontend", "删除", "Delete", "削除"),
            new("generator.gencolumn.islist", "Frontend", "列表", "List", "リスト"),
            new("generator.gencolumn.isexport", "Frontend", "导出", "Export", "エクスポート"),
            new("generator.gencolumn.issort", "Frontend", "排序", "Sort", "ソート"),
            new("generator.gencolumn.isrequired", "Frontend", "必填", "Required", "必須"),
            new("generator.gencolumn.isform", "Frontend", "表单显示", "Form Display", "フォーム表示"),
            new("generator.gencolumn.formcontroltype", "Frontend", "表单类型", "Form Type", "フォームタイプ"),
            new("generator.gencolumn.dicttype", "Frontend", "字典类型", "Dict Type", "辞書タイプ"),

            // 代码生成导入表相关
            new("generator.importtable.entity", "Frontend", "导入表", "Import Table", "テーブルをインポート"),
            new("generator.importtable.tablename", "Frontend", "表名", "Table Name", "テーブル名"),
            new("generator.importtable.description", "Frontend", "表描述", "Table Description", "テーブル説明"),
            new("generator.importtable.columnname", "Frontend", "列名", "Column Name", "列名"),
            new("generator.importtable.columndescription", "Frontend", "列描述", "Column Description", "列説明"),
            new("generator.importtable.datatype", "Frontend", "数据类型", "Data Type", "データ型"),
            new("generator.importtable.length", "Frontend", "长度", "Length", "長さ"),
            new("generator.importtable.decimalplaces", "Frontend", "精度", "Decimal Places", "小数点以下桁数"),
            new("generator.importtable.defaultvalue", "Frontend", "默认值", "Default Value", "デフォルト値"),
            new("generator.importtable.isprimarykey", "Frontend", "主键", "Primary Key", "主キー"),
            new("generator.importtable.isidentity", "Frontend", "自增", "Identity", "自動増分"),
            new("generator.importtable.isnullable", "Frontend", "可空", "Nullable", "NULL許可"),
        };
    }

    private sealed record TranslationSeed(string Key, string Module, string ZhCn, string EnUs, string JaJp);
}
