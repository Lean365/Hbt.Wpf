//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : DbSeedRoutine.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : Routine 模块种子数据初始化服务
//===================================================================

using Hbt.Common.Helpers;
using Hbt.Common.Logging;
using Hbt.Domain.Entities.Routine;
using Hbt.Domain.Repositories;

namespace Hbt.Infrastructure.Data;

/// <summary>
/// Routine 模块种子数据初始化服务
/// 创建语言、翻译、字典和系统设置的种子数据
/// </summary>
public class DbSeedRoutine
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<DictionaryType> _dictionaryTypeRepository;
    private readonly IBaseRepository<DictionaryData> _dictionaryDataRepository;
    private readonly IBaseRepository<Setting> _settingRepository;

    public DbSeedRoutine(
        InitLogManager initLog,
        IBaseRepository<Language> languageRepository,
        IBaseRepository<Translation> translationRepository,
        IBaseRepository<DictionaryType> dictionaryTypeRepository,
        IBaseRepository<DictionaryData> dictionaryDataRepository,
        IBaseRepository<Setting> settingRepository)
    {
        _initLog = initLog;
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
        _dictionaryTypeRepository = dictionaryTypeRepository;
        _dictionaryDataRepository = dictionaryDataRepository;
        _settingRepository = settingRepository;
    }

    /// <summary>
    /// 初始化 Routine 模块种子数据
    /// </summary>
    public void Initialize()
    {
        _initLog.Information("开始初始化 Routine 模块种子数据...");

        // 1. 初始化语言数据
        var languages = InitializeLanguages();

        // 2. 初始化翻译数据
        InitializeTranslations(languages);

        // 3. 初始化字典类型
        var dictionaryTypes = InitializeDictionaryTypes();

        // 4. 初始化字典数据
        InitializeDictionaryData(dictionaryTypes);

        // 5. 初始化系统设置
        InitializeSettings();

        _initLog.Information("Routine 模块种子数据初始化完成");
    }

    /// <summary>
    /// 初始化语言数据（中英日三语）
    /// </summary>
    private List<Language> InitializeLanguages()
    {
        var languages = new List<Language>();

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
            _languageRepository.Create(zhCn, "Hbt365");
            _initLog.Information("✅ 创建语言：简体中文");
        }
        languages.Add(zhCn);

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
            _languageRepository.Create(enUs, "Hbt365");
            _initLog.Information("✅ 创建语言：English");
        }
        languages.Add(enUs);

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
            _languageRepository.Create(jaJp, "Hbt365");
            _initLog.Information("✅ 创建语言：日本語");
        }
        languages.Add(jaJp);

        return languages;
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

        // 翻译数据定义
        var translations = new[]
        {
            // 登录相关
            new { Key = "Login.Welcome", Module = "Frontend", ZhCn = "欢迎登录", EnUs = "Welcome", JaJp = "ようこそ" },
            new { Key = "Login.Title", Module = "Frontend", ZhCn = "黑冰台中后台管理 - 登录", EnUs = "HBT Management Platform - Login", JaJp = "HBT管理プラットフォーム - ログイン" },
            new { Key = "Login.Username", Module = "Frontend", ZhCn = "用户名", EnUs = "Username", JaJp = "ユーザー名" },
            new { Key = "Login.Password", Module = "Frontend", ZhCn = "密码", EnUs = "Password", JaJp = "パスワード" },
            new { Key = "Login.RememberMe", Module = "Frontend", ZhCn = "记住密码", EnUs = "Remember Me", JaJp = "パスワードを記憶する" },
            new { Key = "Login.Forgot", Module = "Frontend", ZhCn = "忘记密码？", EnUs = "Forgot password?", JaJp = "パスワードをお忘れですか？" },
            new { Key = "Login.Button", Module = "Frontend", ZhCn = "登录", EnUs = "Login", JaJp = "ログイン" },
            new { Key = "Login.Error", Module = "Frontend", ZhCn = "登录失败：{0}", EnUs = "Login failed: {0}", JaJp = "ログイン失敗：{0}" },
            
            // 审计字段相关
            new { Key = "common.audit.id", Module = "Frontend", ZhCn = "主键", EnUs = "ID", JaJp = "ID" },
            new { Key = "common.audit.remarks", Module = "Frontend", ZhCn = "备注", EnUs = "Remarks", JaJp = "備考" },
            new { Key = "common.audit.created_by", Module = "Frontend", ZhCn = "创建人", EnUs = "Created By", JaJp = "作成者" },
            new { Key = "common.audit.created_time", Module = "Frontend", ZhCn = "创建时间", EnUs = "Created Time", JaJp = "作成時間" },
            new { Key = "common.audit.updated_by", Module = "Frontend", ZhCn = "更新人", EnUs = "Updated By", JaJp = "更新者" },
            new { Key = "common.audit.updated_time", Module = "Frontend", ZhCn = "更新时间", EnUs = "Updated Time", JaJp = "更新時間" },
            new { Key = "common.audit.deleted_by", Module = "Frontend", ZhCn = "删除人", EnUs = "Deleted By", JaJp = "削除者" },
            new { Key = "common.audit.deleted_time", Module = "Frontend", ZhCn = "删除时间", EnUs = "Deleted Time", JaJp = "削除時間" },
            new { Key = "common.audit.is_deleted", Module = "Frontend", ZhCn = "是否删除", EnUs = "Is Deleted", JaJp = "削除済み" },
            
            
            // 通用
            new { Key = "Common.Welcome", Module = "Frontend", ZhCn = "欢迎", EnUs = "Welcome", JaJp = "ようこそ" },
            new { Key = "Common.CompanyName", Module = "Frontend", ZhCn = "黑冰台", EnUs = "HBT", JaJp = "HBT" },
            new { Key = "Common.CompanySlogan", Module = "Frontend", ZhCn = "中后台管理平台", EnUs = "Management Platform", JaJp = "管理プラットフォーム" },
            new { Key = "Common.CompanyTagline", Module = "Frontend", ZhCn = "数据驱动决策 • 智能赋能业务", EnUs = "Data Driven Decision • Intelligent Business Empowerment", JaJp = "データ駆動型意思決定 • インテリジェントなビジネス強化" },
            new { Key = "Common.Copyright", Module = "Frontend", ZhCn = "© 2025 黑冰台科技. All Rights Reserved.", EnUs = "© 2025 HBT Technology. All Rights Reserved.", JaJp = "© 2025 HBT技術. 全著作権所有." },
            
            // 按钮文本
            new { Key = "Button.Close", Module = "Frontend", ZhCn = "关闭", EnUs = "Close", JaJp = "閉じる" },
            new { Key = "Button.ChangeTheme", Module = "Frontend", ZhCn = "切换主题", EnUs = "Toggle Theme", JaJp = "テーマを切り替え" },
            new { Key = "Button.ChangeLanguage", Module = "Frontend", ZhCn = "切换语言", EnUs = "Toggle Language", JaJp = "言語を切り替え" },
            
            // 左侧公司信息
            new { Key = "Company.Name", Module = "Frontend", ZhCn = "黑冰台", EnUs = "HBT", JaJp = "HBT" },
            new { Key = "Company.Slogan", Module = "Frontend", ZhCn = "中后台管理平台", EnUs = "Management Platform", JaJp = "管理プラットフォーム" },
            new { Key = "Company.Tagline", Module = "Frontend", ZhCn = "数据驱动决策 • 智能赋能业务", EnUs = "Data Driven Decision • Intelligent Business Empowerment", JaJp = "データ駆動型意思決定 • インテリジェントなビジネス強化" },
            new { Key = "Company.Copyright", Module = "Frontend", ZhCn = "© 2025 黑冰台科技. All Rights Reserved.", EnUs = "© 2025 HBT Technology. All Rights Reserved.", JaJp = "© 2025 HBT技術. 全著作権所有." },

            // 通用操作结果提示（成功类）
            new { Key = "common.success.create.by_entity_id", Module = "Frontend", ZhCn = "{0}表ID为{1}的记录已创建成功", EnUs = "Record in {0} with ID {1} has been created successfully", JaJp = "{0}テーブルのIDが{1}のレコードは作成されました" },
            new { Key = "common.success.delete.by_entity_ids", Module = "Frontend", ZhCn = "{0}表ID为[{1}]的记录已删除成功", EnUs = "Records in {0} with IDs [{1}] have been deleted successfully", JaJp = "{0}テーブルのIDが[{1}]のレコードは削除されました" },
            new { Key = "common.success.update.by_entity_id", Module = "Frontend", ZhCn = "{0}表ID为{1}的记录已更新成功", EnUs = "Record in {0} with ID {1} has been updated successfully", JaJp = "{0}テーブルのIDが{1}のレコードは更新されました" },
            new { Key = "common.success.import.summary", Module = "Frontend", ZhCn = "{0}表导入成功：新建{1}条，更新{2}条，共{3}条记录", EnUs = "{0} import succeeded: created {1}, updated {2}, total {3}", JaJp = "{0}のインポート成功：新規{1}件、更新{2}件、合計{3}件" },
            new { Key = "common.success.export.summary", Module = "Frontend", ZhCn = "{0}表导出成功：共{1}条记录", EnUs = "{0} export succeeded: total {1} records", JaJp = "{0}のエクスポート成功：合計{1}件" },
            new { Key = "common.success.authorize.user", Module = "Frontend", ZhCn = "用户名{0}已成功授权", EnUs = "User {0} has been authorized successfully", JaJp = "ユーザー{0}の権限付与に成功しました" },
            new { Key = "common.success.enable.name", Module = "Frontend", ZhCn = "{0}启用成功", EnUs = "{0} enabled successfully", JaJp = "{0} が有効化されました" },
            new { Key = "common.success.disable.name", Module = "Frontend", ZhCn = "{0}禁用成功", EnUs = "{0} disabled successfully", JaJp = "{0} が無効化されました" },
            new { Key = "common.success.submit", Module = "Frontend", ZhCn = "提交成功", EnUs = "Submitted successfully", JaJp = "提出に成功しました" },
            new { Key = "common.success.approve.name", Module = "Frontend", ZhCn = "{0}审核成功", EnUs = "{0} approved successfully", JaJp = "{0} の承認に成功しました" },
            new { Key = "common.success.reject.name", Module = "Frontend", ZhCn = "{0}撤销成功", EnUs = "{0} revoked successfully", JaJp = "{0} の取り消しに成功しました" },

            // 通用删除确认
            new { Key = "common.confirm.delete", Module = "Frontend", ZhCn = "确定要删除{0}吗？", EnUs = "Are you sure you want to delete {0}?", JaJp = "{0} を削除してもよろしいですか？" },
            new { Key = "common.confirm.delete.by_entity_ids", Module = "Frontend", ZhCn = "确定要删除{0}表ID为[{1}]的{2}条记录吗？", EnUs = "Are you sure to delete {2} records in {0} with IDs [{1}]?", JaJp = "{0}テーブルのIDが[{1}]の{2}件のレコードを削除してもよろしいですか？" },

            // 通用校验
            new { Key = "validation.required", Module = "Frontend", ZhCn = "{0}为必填项", EnUs = "{0} is required", JaJp = "{0} は必須です" },
            new { Key = "validation.format", Module = "Frontend", ZhCn = "{0}格式不正确", EnUs = "Invalid {0} format", JaJp = "{0} の形式が正しくありません" },
            
            // 通用操作结果（失败类）
            new { Key = "common.saveFailed", Module = "Frontend", ZhCn = "保存失败", EnUs = "Save failed", JaJp = "保存に失敗しました" },

            // 用户实体字段（严格对齐实体）
            new { Key = "Identity.User", Module = "Frontend", ZhCn = "用户", EnUs = "User", JaJp = "ユーザー" },
            new { Key = "Identity.User.Username", Module = "Frontend", ZhCn = "用户名", EnUs = "Username", JaJp = "ユーザー名" },
            new { Key = "Identity.User.Password", Module = "Frontend", ZhCn = "密码", EnUs = "Password", JaJp = "パスワード" },
            new { Key = "Identity.User.PasswordConfirm", Module = "Frontend", ZhCn = "确认密码", EnUs = "Confirm Password", JaJp = "パスワード確認" },
            new { Key = "Identity.User.Email", Module = "Frontend", ZhCn = "邮箱", EnUs = "Email", JaJp = "メール" },
            new { Key = "Identity.User.Phone", Module = "Frontend", ZhCn = "手机号", EnUs = "Phone", JaJp = "携帯番号" },
            new { Key = "Identity.User.RealName", Module = "Frontend", ZhCn = "真实姓名", EnUs = "Real Name", JaJp = "氏名" },
            new { Key = "Identity.User.UserType", Module = "Frontend", ZhCn = "用户类型", EnUs = "User Type", JaJp = "ユーザー種別" },
            new { Key = "Identity.User.UserType.System", Module = "Frontend", ZhCn = "系统用户", EnUs = "System User", JaJp = "システムユーザー" },
            new { Key = "Identity.User.UserType.Normal", Module = "Frontend", ZhCn = "普通用户", EnUs = "Normal User", JaJp = "一般ユーザー" },
            new { Key = "Identity.User.UserGender", Module = "Frontend", ZhCn = "性别", EnUs = "Gender", JaJp = "性別" },
            new { Key = "Identity.User.UserGender.Unknown", Module = "Frontend", ZhCn = "未知", EnUs = "Unknown", JaJp = "不明" },
            new { Key = "Identity.User.UserGender.Male", Module = "Frontend", ZhCn = "男", EnUs = "Male", JaJp = "男性" },
            new { Key = "Identity.User.UserGender.Female", Module = "Frontend", ZhCn = "女", EnUs = "Female", JaJp = "女性" },
            new { Key = "Identity.User.UserStatus", Module = "Frontend", ZhCn = "状态", EnUs = "Status", JaJp = "状態" },
            new { Key = "Identity.User.UserStatus.Normal", Module = "Frontend", ZhCn = "正常", EnUs = "Normal", JaJp = "正常" },
            new { Key = "Identity.User.UserStatus.Disabled", Module = "Frontend", ZhCn = "禁用", EnUs = "Disabled", JaJp = "無効" },
            new { Key = "Identity.User.Create", Module = "Frontend", ZhCn = "新建用户", EnUs = "Create User", JaJp = "ユーザー作成" },
            new { Key = "Identity.User.Edit", Module = "Frontend", ZhCn = "编辑用户", EnUs = "Edit User", JaJp = "ユーザー編集" },
            new { Key = "Identity.User.Validation.UsernamePasswordRequired", Module = "Frontend", ZhCn = "用户名和密码不能为空", EnUs = "Username and password cannot be empty", JaJp = "ユーザー名とパスワードは空にできません" },
            new { Key = "Identity.User.Validation.PasswordMismatch", Module = "Frontend", ZhCn = "两次输入的密码不一致", EnUs = "The two passwords do not match", JaJp = "2つのパスワードが一致しません" },
            new { Key = "Identity.User.Roles", Module = "Frontend", ZhCn = "角色用户", EnUs = "Role Users", JaJp = "ロールユーザー" },

            // 角色实体字段（严格对齐实体）
            new { Key = "Identity.Role", Module = "Frontend", ZhCn = "角色", EnUs = "Role", JaJp = "ロール" },
            new { Key = "Identity.Role.RoleName", Module = "Frontend", ZhCn = "角色名称", EnUs = "Role Name", JaJp = "ロール名" },
            new { Key = "Identity.Role.RoleCode", Module = "Frontend", ZhCn = "角色编码", EnUs = "Role Code", JaJp = "ロールコード" },
            new { Key = "Identity.Role.Description", Module = "Frontend", ZhCn = "描述", EnUs = "Description", JaJp = "説明" },
            new { Key = "Identity.Role.DataScope", Module = "Frontend", ZhCn = "数据范围", EnUs = "Data Scope", JaJp = "データ範囲" },
            new { Key = "Identity.Role.UserCount", Module = "Frontend", ZhCn = "用户数", EnUs = "User Count", JaJp = "ユーザー数" },
            new { Key = "Identity.Role.OrderNum", Module = "Frontend", ZhCn = "排序号", EnUs = "Order Num", JaJp = "並び順" },
            new { Key = "Identity.Role.RoleStatus", Module = "Frontend", ZhCn = "状态", EnUs = "Status", JaJp = "状態" },
            new { Key = "Identity.Role.Users", Module = "Frontend", ZhCn = "角色用户", EnUs = "Role Users", JaJp = "ロールユーザー" },
            new { Key = "Identity.Role.Menus", Module = "Frontend", ZhCn = "角色菜单", EnUs = "Role Menus", JaJp = "ロールメニュー" },

            // 菜单实体字段（严格对齐实体）
            new { Key = "Identity.Menu", Module = "Frontend", ZhCn = "菜单", EnUs = "Menu", JaJp = "メニュー" },
            new { Key = "Identity.Menu.MenuName", Module = "Frontend", ZhCn = "菜单名称", EnUs = "Menu Name", JaJp = "メニュー名" },
            new { Key = "Identity.Menu.MenuCode", Module = "Frontend", ZhCn = "菜单编码", EnUs = "Menu Code", JaJp = "メニューコード" },
            new { Key = "Identity.Menu.I18nKey", Module = "Frontend", ZhCn = "国际化键", EnUs = "I18n Key", JaJp = "多言語キー" },
            new { Key = "Identity.Menu.PermCode", Module = "Frontend", ZhCn = "权限码", EnUs = "Permission Code", JaJp = "権限コード" },
            new { Key = "Identity.Menu.MenuType", Module = "Frontend", ZhCn = "菜单类型", EnUs = "Menu Type", JaJp = "メニュー種別" },
            new { Key = "Identity.Menu.ParentId", Module = "Frontend", ZhCn = "父级ID", EnUs = "Parent ID", JaJp = "上位ID" },
            new { Key = "Identity.Menu.RoutePath", Module = "Frontend", ZhCn = "路由路径", EnUs = "Route Path", JaJp = "ルートパス" },
            new { Key = "Identity.Menu.Icon", Module = "Frontend", ZhCn = "图标", EnUs = "Icon", JaJp = "アイコン" },
            new { Key = "Identity.Menu.Component", Module = "Frontend", ZhCn = "组件路径", EnUs = "Component Path", JaJp = "コンポーネントパス" },
            new { Key = "Identity.Menu.IsExternal", Module = "Frontend", ZhCn = "是否外链", EnUs = "Is External", JaJp = "外部リンクか" },
            new { Key = "Identity.Menu.IsCache", Module = "Frontend", ZhCn = "是否缓存", EnUs = "Is Cache", JaJp = "キャッシュか" },
            new { Key = "Identity.Menu.IsVisible", Module = "Frontend", ZhCn = "是否可见", EnUs = "Is Visible", JaJp = "表示か" },
            new { Key = "Identity.Menu.OrderNum", Module = "Frontend", ZhCn = "排序号", EnUs = "Order Num", JaJp = "並び順" },
            new { Key = "Identity.Menu.MenuStatus", Module = "Frontend", ZhCn = "状态", EnUs = "Status", JaJp = "状態" },
            new { Key = "Identity.Menu.Roles", Module = "Frontend", ZhCn = "角色菜单", EnUs = "Role Menus", JaJp = "ロールメニュー" },

            // 通用占位（参数化占位符）
            new { Key = "common.placeholder.input", Module = "Frontend", ZhCn = "请输入{0}", EnUs = "Please enter {0}", JaJp = "{0} を入力してください" },
            new { Key = "common.placeholder.select", Module = "Frontend", ZhCn = "请选择{0}", EnUs = "Please select {0}", JaJp = "{0} を選択してください" },
            new { Key = "common.placeholder.search", Module = "Frontend", ZhCn = "请输入{0}进行搜索", EnUs = "Please enter {0} to search", JaJp = "検索するには {0} を入力してください" },
            new { Key = "common.placeholder.range", Module = "Frontend", ZhCn = "请选择{0}范围", EnUs = "Please select {0} range", JaJp = "{0} の範囲を選択してください" },

            // 通用操作
            new { Key = "common.search", Module = "Frontend", ZhCn = "搜索", EnUs = "Search", JaJp = "検索" },
            new { Key = "common.reset", Module = "Frontend", ZhCn = "重置", EnUs = "Reset", JaJp = "リセット" },
            new { Key = "common.advancedQuery", Module = "Frontend", ZhCn = "高级查询", EnUs = "Advanced Query", JaJp = "高度検索" },
            new { Key = "common.toggleColumns", Module = "Frontend", ZhCn = "显隐列", EnUs = "Toggle Columns", JaJp = "列の表示/非表示" },
            new { Key = "common.toggleQueryBar", Module = "Frontend", ZhCn = "显隐查询栏", EnUs = "Toggle Query Bar", JaJp = "検索バーの表示/非表示" },
            new { Key = "common.prevPage", Module = "Frontend", ZhCn = "上一页", EnUs = "Previous Page", JaJp = "前のページ" },
            new { Key = "common.nextPage", Module = "Frontend", ZhCn = "下一页", EnUs = "Next Page", JaJp = "次のページ" },
            
            // 通用按钮（44项，统一键）
            new { Key = "common.button.query", Module = "Frontend", ZhCn = "查询", EnUs = "Query", JaJp = "検索" },
            new { Key = "common.button.read", Module = "Frontend", ZhCn = "查看", EnUs = "Read", JaJp = "閲覧" },
            new { Key = "common.button.create", Module = "Frontend", ZhCn = "新增", EnUs = "Create", JaJp = "新規" },
            new { Key = "common.button.update", Module = "Frontend", ZhCn = "更新", EnUs = "Update", JaJp = "更新" },
            new { Key = "common.button.delete", Module = "Frontend", ZhCn = "删除", EnUs = "Delete", JaJp = "削除" },
            new { Key = "common.button.detail", Module = "Frontend", ZhCn = "详情", EnUs = "Detail", JaJp = "詳細" },
            new { Key = "common.button.export", Module = "Frontend", ZhCn = "导出", EnUs = "Export", JaJp = "エクスポート" },
            new { Key = "common.button.import", Module = "Frontend", ZhCn = "导入", EnUs = "Import", JaJp = "インポート" },
            new { Key = "common.button.print", Module = "Frontend", ZhCn = "打印", EnUs = "Print", JaJp = "印刷" },
            new { Key = "common.button.preview", Module = "Frontend", ZhCn = "预览", EnUs = "Preview", JaJp = "プレビュー" },
            new { Key = "common.button.enable", Module = "Frontend", ZhCn = "启用", EnUs = "Enable", JaJp = "有効化" },
            new { Key = "common.button.disable", Module = "Frontend", ZhCn = "禁用", EnUs = "Disable", JaJp = "無効化" },
            new { Key = "common.button.lock", Module = "Frontend", ZhCn = "锁定", EnUs = "Lock", JaJp = "ロック" },
            new { Key = "common.button.unlock", Module = "Frontend", ZhCn = "解锁", EnUs = "Unlock", JaJp = "アンロック" },
            new { Key = "common.button.authorize", Module = "Frontend", ZhCn = "授权", EnUs = "Authorize", JaJp = "権限付与" },
            new { Key = "common.button.grant", Module = "Frontend", ZhCn = "授予", EnUs = "Grant", JaJp = "付与" },
            new { Key = "common.button.revoke", Module = "Frontend", ZhCn = "收回", EnUs = "Revoke", JaJp = "剥奪" },
            new { Key = "common.button.run", Module = "Frontend", ZhCn = "运行", EnUs = "Run", JaJp = "実行" },
            new { Key = "common.button.start", Module = "Frontend", ZhCn = "启动", EnUs = "Start", JaJp = "開始" },
            new { Key = "common.button.stop", Module = "Frontend", ZhCn = "停止", EnUs = "Stop", JaJp = "停止" },
            new { Key = "common.button.pause", Module = "Frontend", ZhCn = "暂停", EnUs = "Pause", JaJp = "一時停止" },
            new { Key = "common.button.resume", Module = "Frontend", ZhCn = "恢复", EnUs = "Resume", JaJp = "再開" },
            new { Key = "common.button.restart", Module = "Frontend", ZhCn = "重启", EnUs = "Restart", JaJp = "再起動" },
            new { Key = "common.button.submit", Module = "Frontend", ZhCn = "提交", EnUs = "Submit", JaJp = "提出" },
            new { Key = "common.button.approve", Module = "Frontend", ZhCn = "通过", EnUs = "Approve", JaJp = "承認" },
            new { Key = "common.button.reject", Module = "Frontend", ZhCn = "驳回", EnUs = "Reject", JaJp = "却下" },
            new { Key = "common.button.recall", Module = "Frontend", ZhCn = "撤回", EnUs = "Recall", JaJp = "取り消し" },
            new { Key = "common.button.send", Module = "Frontend", ZhCn = "发送", EnUs = "Send", JaJp = "送信" },
            new { Key = "common.button.publish", Module = "Frontend", ZhCn = "发布", EnUs = "Publish", JaJp = "公開" },
            new { Key = "common.button.notify", Module = "Frontend", ZhCn = "通知", EnUs = "Notify", JaJp = "通知" },
            new { Key = "common.button.download", Module = "Frontend", ZhCn = "下载", EnUs = "Download", JaJp = "ダウンロード" },
            new { Key = "common.button.upload", Module = "Frontend", ZhCn = "上传", EnUs = "Upload", JaJp = "アップロード" },
            new { Key = "common.button.attach", Module = "Frontend", ZhCn = "附件", EnUs = "Attach", JaJp = "添付" },
            new { Key = "common.button.favorite", Module = "Frontend", ZhCn = "收藏", EnUs = "Favorite", JaJp = "お気に入り" },
            new { Key = "common.button.like", Module = "Frontend", ZhCn = "点赞", EnUs = "Like", JaJp = "いいね" },
            new { Key = "common.button.comment", Module = "Frontend", ZhCn = "评论", EnUs = "Comment", JaJp = "コメント" },
            new { Key = "common.button.share", Module = "Frontend", ZhCn = "分享", EnUs = "Share", JaJp = "共有" },
            new { Key = "common.button.subscribe", Module = "Frontend", ZhCn = "订阅", EnUs = "Subscribe", JaJp = "購読" },
            new { Key = "common.button.reset", Module = "Frontend", ZhCn = "重置", EnUs = "Reset", JaJp = "リセット" },
            new { Key = "common.button.copy", Module = "Frontend", ZhCn = "复制", EnUs = "Copy", JaJp = "コピー" },
            new { Key = "common.button.clone", Module = "Frontend", ZhCn = "克隆", EnUs = "Clone", JaJp = "クローン" },
            new { Key = "common.button.refresh", Module = "Frontend", ZhCn = "刷新", EnUs = "Refresh", JaJp = "リフレッシュ" },
            new { Key = "common.button.archive", Module = "Frontend", ZhCn = "归档", EnUs = "Archive", JaJp = "アーカイブ" },
            new { Key = "common.button.restore", Module = "Frontend", ZhCn = "还原", EnUs = "Restore", JaJp = "復元" },

            // 应用标题
            new { Key = "application.title", Module = "Frontend", ZhCn = "黑冰台管理系统", EnUs = "HBT Management System", JaJp = "HBT管理システム" },
            
            // 菜单翻译（从菜单种子数据中的 I18nKey）
            new { Key = "menu.dashboard", Module = "Frontend", ZhCn = "仪表盘", EnUs = "Dashboard", JaJp = "ダッシュボード" },
            new { Key = "menu.logistics", Module = "Frontend", ZhCn = "后勤管理", EnUs = "Logistics", JaJp = "ロジスティクス" },
            new { Key = "menu.identity", Module = "Frontend", ZhCn = "身份认证", EnUs = "Identity", JaJp = "アイデンティティ" },
            new { Key = "menu.logging", Module = "Frontend", ZhCn = "日志管理", EnUs = "Logging", JaJp = "ログ管理" },
            new { Key = "menu.routine", Module = "Frontend", ZhCn = "日常事务", EnUs = "Routine", JaJp = "日常業務" },
            new { Key = "menu.about", Module = "Frontend", ZhCn = "关于", EnUs = "About", JaJp = "について" },
            new { Key = "menu.logistics.materials", Module = "Frontend", ZhCn = "物料管理", EnUs = "Materials", JaJp = "資材管理" },
            new { Key = "menu.logistics.materials.material", Module = "Frontend", ZhCn = "生产物料", EnUs = "Production Material", JaJp = "生産資材" },
            new { Key = "menu.logistics.materials.model", Module = "Frontend", ZhCn = "机种仕向", EnUs = "Model", JaJp = "機種仕向" },
            new { Key = "menu.logistics.serials", Module = "Frontend", ZhCn = "序列号管理", EnUs = "Serial Management", JaJp = "シリアル管理" },
            new { Key = "menu.logistics.serials.inbound", Module = "Frontend", ZhCn = "序列号入库", EnUs = "Serial Inbound", JaJp = "シリアル入庫" },
            new { Key = "menu.logistics.serials.outbound", Module = "Frontend", ZhCn = "序列号出库", EnUs = "Serial Outbound", JaJp = "シリアル出庫" },
            new { Key = "menu.logistics.visitors", Module = "Frontend", ZhCn = "访客服务", EnUs = "Visitor Service", JaJp = "訪問者サービス" },
            new { Key = "menu.logistics.visitors.management", Module = "Frontend", ZhCn = "访客管理", EnUs = "Visitor Management", JaJp = "訪問者管理" },
            new { Key = "menu.logistics.visitors.signage", Module = "Frontend", ZhCn = "数字标牌", EnUs = "Digital Signage", JaJp = "デジタルサイネージ" },
            new { Key = "menu.logistics.reports", Module = "Frontend", ZhCn = "报表管理", EnUs = "Report Management", JaJp = "レポート管理" },
            new { Key = "menu.logistics.reports.export", Module = "Frontend", ZhCn = "报表导出", EnUs = "Report Export", JaJp = "レポートエクスポート" },
            new { Key = "menu.logistics.reports.import", Module = "Frontend", ZhCn = "报表导入", EnUs = "Report Import", JaJp = "レポートインポート" },
            new { Key = "menu.routine.localization", Module = "Frontend", ZhCn = "本地化", EnUs = "Localization", JaJp = "ローカライゼーション" },
            new { Key = "menu.routine.dictionary", Module = "Frontend", ZhCn = "字典", EnUs = "Dictionary", JaJp = "辞書" },
            new { Key = "menu.routine.setting", Module = "Frontend", ZhCn = "系统设置", EnUs = "System Settings", JaJp = "システム設定" },
            new { Key = "menu.identity.user", Module = "Frontend", ZhCn = "用户管理", EnUs = "User Management", JaJp = "ユーザー管理" },
            new { Key = "menu.identity.role", Module = "Frontend", ZhCn = "角色管理", EnUs = "Role Management", JaJp = "ロール管理" },
            new { Key = "menu.identity.menu", Module = "Frontend", ZhCn = "菜单管理", EnUs = "Menu Management", JaJp = "メニュー管理" },
            new { Key = "menu.logging.login", Module = "Frontend", ZhCn = "登录日志", EnUs = "Login Log", JaJp = "ログインログ" },
            new { Key = "menu.logging.exception", Module = "Frontend", ZhCn = "异常日志", EnUs = "Exception Log", JaJp = "例外ログ" },
            new { Key = "menu.logging.operation", Module = "Frontend", ZhCn = "操作日志", EnUs = "Operation Log", JaJp = "操作ログ" },
            new { Key = "menu.logging.diff", Module = "Frontend", ZhCn = "差异日志", EnUs = "Diff Log", JaJp = "差分ログ" },
            new { Key = "menu.settings", Module = "Frontend", ZhCn = "设置", EnUs = "Settings", JaJp = "設定" },
            
            // 菜单按钮翻译改由 DbSeedMenu 按按钮 I18nKey 精准下发，这里不再写入通用动作按钮
        };

        foreach (var trans in translations)
        {
            CreateOrUpdateTranslation(zhCn, trans.Key, trans.Module, trans.ZhCn);
            CreateOrUpdateTranslation(enUs, trans.Key, trans.Module, trans.EnUs);
            CreateOrUpdateTranslation(jaJp, trans.Key, trans.Module, trans.JaJp);
        }

        _initLog.Information("✅ 翻译数据初始化完成");
    }

    /// <summary>
    /// 创建或更新翻译
    /// </summary>
    private void CreateOrUpdateTranslation(Language language, string key, string module, string value)
    {
        // 统一按语言代码与翻译键判重；WPF 前端固定模块 Frontend
        var existing = _translationRepository.GetFirst(t =>
            t.LanguageCode == language.LanguageCode && t.TranslationKey == key && t.Module == "Frontend");
        
        if (existing == null)
        {
            var translation = new Translation
            {
                LanguageCode = language.LanguageCode,
                TranslationKey = key,
                TranslationValue = value,
                Module = "Frontend",
                OrderNum = 0
            };
            _translationRepository.Create(translation, "Hbt365");
        }
        else
        {
            existing.TranslationValue = value;
            existing.Module = "Frontend";
            _translationRepository.Update(existing, "Hbt365");
        }
    }

    /// <summary>
    /// 初始化字典类型
    /// </summary>
    private List<DictionaryType> InitializeDictionaryTypes()
    {
        var dictionaryTypes = new List<DictionaryType>();

        // 性别
        var gender = CreateOrUpdateDictionaryType("gender", "性别", 1);
        dictionaryTypes.Add(gender);

        // 系统是否
        var yesNo = CreateOrUpdateDictionaryType("yes_no", "系统是否", 2);
        dictionaryTypes.Add(yesNo);

        // 状态
        var status = CreateOrUpdateDictionaryType("status", "状态", 3);
        dictionaryTypes.Add(status);

        // 用户类型
        var userType = CreateOrUpdateDictionaryType("user_type", "用户类型", 4);
        dictionaryTypes.Add(userType);

        return dictionaryTypes;
    }

    /// <summary>
    /// 创建或更新字典类型
    /// </summary>
    private DictionaryType CreateOrUpdateDictionaryType(string code, string name, int order)
    {
        var existing = _dictionaryTypeRepository.GetFirst(d => d.TypeCode == code);
        
        if (existing == null)
        {
            existing = new DictionaryType
            {
                TypeCode = code,
                TypeName = name,
                IsBuiltin = 0,  // 布尔字段：0=是（内置）
                OrderNum = order,
                TypeStatus = 0
            };
            _dictionaryTypeRepository.Create(existing, "Hbt365");
            _initLog.Information($"✅ 创建字典类型：{name}");
        }
        else
        {
            existing.TypeName = name;
            existing.OrderNum = order;
            _dictionaryTypeRepository.Update(existing, "Hbt365");
        }
        
        return existing;
    }

    /// <summary>
    /// 初始化字典数据
    /// </summary>
    private void InitializeDictionaryData(List<DictionaryType> dictionaryTypes)
    {
        var gender = dictionaryTypes.FirstOrDefault(d => d.TypeCode == "gender");
        var yesNo = dictionaryTypes.FirstOrDefault(d => d.TypeCode == "yes_no");
        var status = dictionaryTypes.FirstOrDefault(d => d.TypeCode == "status");
        var userType = dictionaryTypes.FirstOrDefault(d => d.TypeCode == "user_type");

        if (gender == null || yesNo == null || status == null || userType == null)
        {
            _initLog.Warning("字典类型数据不完整，跳过字典数据初始化");
            return;
        }

        // 性别数据
        CreateOrUpdateDictionaryData(gender, "male", "男", 1);
        CreateOrUpdateDictionaryData(gender, "female", "女", 2);
        CreateOrUpdateDictionaryData(gender, "unknown", "未知", 3);

        // 系统是否
        CreateOrUpdateDictionaryData(yesNo, "yes", "是", 1);
        CreateOrUpdateDictionaryData(yesNo, "no", "否", 2);

        // 状态
        CreateOrUpdateDictionaryData(status, "normal", "正常", 0);
        CreateOrUpdateDictionaryData(status, "disabled", "禁用", 1);

        // 用户类型
        CreateOrUpdateDictionaryData(userType, "Hbt365", "系统用户", 0);
        CreateOrUpdateDictionaryData(userType, "normal", "普通用户", 1);

        _initLog.Information("✅ 字典数据初始化完成");
    }

    /// <summary>
    /// 创建或更新字典数据
    /// </summary>
    private void CreateOrUpdateDictionaryData(DictionaryType type, string code, string name, int order)
    {
        // 改为按类型代码 + 数据代码唯一
        var existing = _dictionaryDataRepository.GetFirst(d => d.TypeCode == type.TypeCode && d.DataCode == code);
        
        if (existing == null)
        {
            var data = new DictionaryData
            {
                TypeCode = type.TypeCode,
                DataCode = code,
                DataName = name,
                DataValue = code,
                OrderNum = order
            };
            _dictionaryDataRepository.Create(data, "Hbt365");
        }
        else
        {
            existing.DataName = name;
            existing.DataValue = code;
            existing.OrderNum = order;
            _dictionaryDataRepository.Update(existing, "Hbt365");
        }
    }

    /// <summary>
    /// 初始化系统设置
    /// </summary>
    private void InitializeSettings()
    {
        _initLog.Information("开始初始化系统设置...");

        // ==================== 应用设置分类 (AppSettings) ====================
        
        // 1. 系统默认语言
        CreateOrUpdateSetting(
            "AppSettings:DefaultLanguage", 
            "zh-CN", 
            "系统默认语言（zh-CN:简体中文, en-US:英文, ja-JP:日文）", 
            category: "AppSettings",
            type: 0, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 1);
        
        // 2. 系统默认主题
        CreateOrUpdateSetting(
            "AppSettings:DefaultTheme", 
            "Hbt365", 
            "系统默认主题（Light:浅色, Dark:深色, System:跟随系统）", 
            category: "AppSettings",
            type: 0, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 2);
        
        // 3. 系统字体设置
        CreateOrUpdateSetting(
            "AppSettings:FontFamily", 
            "Microsoft YaHei UI", 
            "系统默认字体（如：Microsoft YaHei UI, SimSun, Arial）", 
            category: "AppSettings",
            type: 0, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 3);
        
        CreateOrUpdateSetting(
            "AppSettings:FontSize", 
            "14", 
            "系统默认字体大小（单位：pt）", 
            category: "AppSettings",
            type: 1, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 4);

        // ==================== 系统信息分类 (SystemInfo) ====================
        
        // 4. 系统名称
        CreateOrUpdateSetting(
            "SystemInfo:SystemName", 
            "黑冰台中后台管理平台", 
            "系统名称，显示在标题栏和应用标题位置", 
            category: "SystemInfo",
            type: 0, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 1);
        
        // 5. 系统版本
        CreateOrUpdateSetting(
            "SystemInfo:SystemVersion", 
            "1.0.0", 
            "系统版本号", 
            category: "SystemInfo",
            type: 0, 
            isBuiltin: true, 
            isEditable: false,
            orderNum: 2);
        
        // 6. 水印
        CreateOrUpdateSetting(
            "SystemInfo:Watermark", 
            "", 
            "系统水印文本（留空则不显示水印）", 
            category: "SystemInfo",
            type: 0, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 3);
        
        // 8. Logo设置
        CreateOrUpdateSetting(
            "SystemInfo:LogoPath", 
            "pack://application:,,,/Assets/hbt-loto.ico", 
            "系统Logo路径（支持资源路径或文件路径）", 
            category: "SystemInfo",
            type: 0, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 4);
        
        // 9. 版权信息
        CreateOrUpdateSetting(
            "SystemInfo:Copyright", 
            "© 2025 黑冰台. All rights reserved.", 
            "系统版权信息", 
            category: "SystemInfo",
            type: 0, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 5);

        // ==================== 安全设置分类 (Security) ====================
        
        // 7. 用户锁定时间
        CreateOrUpdateSetting(
            "Security:UserLockoutDuration", 
            "30", 
            "用户锁定时间（单位：分钟，登录失败达到指定次数后锁定）", 
            category: "Security",
            type: 1, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 1);
        
        CreateOrUpdateSetting(
            "Security:MaxLoginAttempts", 
            "5", 
            "最大登录尝试次数（超过此次数将锁定账户）", 
            category: "Security",
            type: 1, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 2);
        
        CreateOrUpdateSetting(
            "Security:PasswordMinLength", 
            "8", 
            "密码最小长度", 
            category: "Security",
            type: 1, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 3);
        
        CreateOrUpdateSetting(
            "Security:PasswordRequireDigit", 
            "true", 
            "密码是否必须包含数字", 
            category: "Security",
            type: 2, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 4);
        
        CreateOrUpdateSetting(
            "Security:PasswordRequireUppercase", 
            "false", 
            "密码是否必须包含大写字母", 
            category: "Security",
            type: 2, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 5);
        
        CreateOrUpdateSetting(
            "Security:PasswordRequireLowercase", 
            "false", 
            "密码是否必须包含小写字母", 
            category: "Security",
            type: 2, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 6);
        
        CreateOrUpdateSetting(
            "Security:PasswordRequireSpecialChar", 
            "false", 
            "密码是否必须包含特殊字符", 
            category: "Security",
            type: 2, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 7);

        // ==================== UI设置分类 (UI) ====================
        
        CreateOrUpdateSetting(
            "UI:ShowWatermark", 
            "false", 
            "是否显示水印", 
            category: "UI",
            type: 2, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 1);
        
        CreateOrUpdateSetting(
            "UI:WatermarkOpacity", 
            "0.3", 
            "水印透明度（0.0-1.0）", 
            category: "UI",
            type: 1, 
            isBuiltin: true, 
            isEditable: true,
            orderNum: 2);

        _initLog.Information("✅ 系统设置初始化完成");
    }

    /// <summary>
    /// 创建或更新系统设置
    /// </summary>
    /// <param name="key">设置键</param>
    /// <param name="value">设置值</param>
    /// <param name="description">设置描述</param>
    /// <param name="category">分类（如：AppSettings, SystemInfo, Security, UI）</param>
    /// <param name="type">设置类型（0=字符串, 1=数字, 2=布尔值, 3=JSON）</param>
    /// <param name="isBuiltin">是否内置（true=是，内置数据不可删除）</param>
    /// <param name="isEditable">是否可修改（true=是，false=只读）</param>
    /// <param name="orderNum">排序号</param>
    private void CreateOrUpdateSetting(
        string key, 
        string value, 
        string description, 
        string category = "AppSettings",
        int type = 0, 
        bool isBuiltin = true, 
        bool isEditable = true,
        int orderNum = 0)
    {
        var existing = _settingRepository.GetFirst(s => s.SettingKey == key);
        
        if (existing == null)
        {
            var setting = new Setting
            {
                SettingKey = key,
                SettingValue = value,
                SettingDescription = description,
                SettingType = type,
                IsBuiltin = isBuiltin ? 0 : 1,  // 布尔字段：0=是，1=否
                IsEditable = isEditable ? 0 : 1,  // 布尔字段：0=是，1=否
                OrderNum = orderNum,
                Category = category
            };
            _settingRepository.Create(setting, "Hbt365");
        }
        else
        {
            existing.SettingValue = value;
            existing.SettingDescription = description;
            existing.Category = category;
            existing.SettingType = type;
            existing.OrderNum = orderNum;
            existing.IsEditable = isEditable ? 0 : 1;
            _settingRepository.Update(existing, "Hbt365");
        }
    }
}

