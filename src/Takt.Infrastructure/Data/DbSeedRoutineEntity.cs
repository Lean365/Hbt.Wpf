// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedRoutineEntity.cs
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
public class DbSeedRoutineEntity
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<Language> _languageRepository;

    public DbSeedRoutineEntity(
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
            CreateOrUpdateTranslation(zhCn, seed.Key, seed.Module, seed.ZhCn);
            CreateOrUpdateTranslation(enUs, seed.Key, seed.Module, seed.EnUs);
            CreateOrUpdateTranslation(jaJp, seed.Key, seed.Module, seed.JaJp);
        }

        _initLog.Information("✅ 实体字段翻译初始化完成");
    }

    private void CreateOrUpdateTranslation(Language language, string key, string module, string value)
    {
        var existing = _translationRepository.GetFirst(t =>
            t.LanguageCode == language.LanguageCode &&
            t.TranslationKey == key &&
            t.Module == module);

        if (existing == null)
        {
            var translation = new Translation
            {
                LanguageCode = language.LanguageCode,
                TranslationKey = key,
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
            new("Identity.User", "Frontend", "用户", "User", "ユーザー"),
            new("Identity.User.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Identity.User.Avatar", "Frontend", "头像", "Avatar", "アバター"),
            new("Identity.User.Password", "Frontend", "密码", "Password", "パスワード"),
            new("Identity.User.PasswordConfirm", "Frontend", "确认密码", "Confirm Password", "パスワード確認"),
            new("Identity.User.Email", "Frontend", "邮箱", "Email", "メール"),
            new("Identity.User.Phone", "Frontend", "手机号", "Phone", "携帯番号"),
            new("Identity.User.RealName", "Frontend", "真实姓名", "Real Name", "氏名"),
            new("Identity.User.Nickname", "Frontend", "昵称", "Nickname", "ニックネーム"),
            new("Identity.User.UserType", "Frontend", "用户类型", "User Type", "ユーザー種別"),
            new("Identity.User.UserType.System", "Frontend", "系统用户", "System User", "システムユーザー"),
            new("Identity.User.UserType.Normal", "Frontend", "普通用户", "Normal User", "一般ユーザー"),
            new("Identity.User.UserGender", "Frontend", "性别", "Gender", "性別"),
            new("Identity.User.UserGender.Unknown", "Frontend", "未知", "Unknown", "不明"),
            new("Identity.User.UserGender.Male", "Frontend", "男", "Male", "男性"),
            new("Identity.User.UserGender.Female", "Frontend", "女", "Female", "女性"),
            new("Identity.User.UserStatus", "Frontend", "状态", "Status", "状態"),
            new("Identity.User.UserStatus.Normal", "Frontend", "正常", "Normal", "正常"),
            new("Identity.User.UserStatus.Disabled", "Frontend", "禁用", "Disabled", "無効"),
            new("Identity.User.Roles", "Frontend", "角色用户", "Role Users", "ロールユーザー"),
            new("Identity.User.Keyword", "Frontend", "用户名、手机", "username, phone", "ユーザー名、電話"),
            new("Identity.User.DetailTitle", "Frontend", "用户信息", "User Information", "ユーザー情報"),
            new("Identity.User.DetailSubtitle", "Frontend", "查看用户详细信息", "View user details", "ユーザー詳細を表示"),
            new("Identity.User.BasicInfo", "Frontend", "基本信息", "Basic Information", "基本情報"),
            new("Identity.User.StatusInfo", "Frontend", "状态信息", "Status Information", "状態情報"),
            new("Identity.User.RemarksInfo", "Frontend", "备注信息", "Remarks Information", "備考情報"),
            
            // 用户验证相关（仅保留特定业务规则，通用验证使用 validation.required/format/maxLength/minLength/invalid）
            new("Identity.User.Validation.UsernameInvalid", "Frontend", "用户名必须以小写字母开头，只能包含小写字母和数字，长度4-10位", "Username must start with a lowercase letter and contain only lowercase letters and numbers, 4-10 characters", "ユーザー名は小文字で始まり、小文字と数字のみを含む必要があります（4-10文字）"),
            new("Identity.User.Validation.UsernamePasswordRequired", "Frontend", "用户名和密码不能为空", "Username and password cannot be empty", "ユーザー名とパスワードは空にできません"),
            new("Identity.User.Validation.RealNameHint", "Frontend", "不允许数字、点号、空格开头，英文字母首字母大写，30字以内", "Cannot start with digits, dots, or spaces. English letters must be uppercase. Max 30 characters", "数字、ドット、スペースで始めることはできません。英語の文字は大文字である必要があります。最大30文字"),
            new("Identity.User.Validation.RealNameInvalid", "Frontend", "不允许数字、点号、空格开头，英文字母首字母必须大写", "Cannot start with digits, dots, or spaces. English letters must be uppercase", "数字、ドット、スペースで始めることはできません。英語の文字は大文字である必要があります"),
            new("Identity.User.Validation.NicknameInvalid", "Frontend", "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等，如：Cheng.Jianhong、Joseph Robinette Biden Jr. 或 张三", "Nickname cannot start with digits, dots, or spaces, if the first character is an English letter it must be uppercase, allow letters, numbers, dots, and spaces, support Chinese, Japanese, Korean, Vietnamese, etc., e.g., Cheng.Jianhong, Joseph Robinette Biden Jr. or 张三", "ニックネームは数字、ドット、スペースで始めることはできません。最初の文字が英語の文字の場合は大文字である必要があります。文字、数字、ドット、スペースを含むことができ、中国語、日本語、韓国語、ベトナム語などをサポートします。例：Cheng.Jianhong、Joseph Robinette Biden Jr. または 张三"),
            new("Identity.User.Validation.PhoneInvalid", "Frontend", "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9", "Invalid phone number format. Must be 11 digits starting with 1, second digit 3-9", "携帯番号の形式が正しくありません。1で始まり、2桁目が3-9の11桁の数字である必要があります"),
            new("Identity.User.Validation.AvatarMustBeRelativePath", "Frontend", "头像必须是相对路径，不能使用绝对路径或URL", "Avatar must be a relative path, absolute paths or URLs are not allowed", "アバターは相対パスである必要があります。絶対パスやURLは使用できません"),
            new("Identity.User.Validation.PasswordMismatch", "Frontend", "两次输入的密码不一致", "The two passwords do not match", "2つのパスワードが一致しません"),

            // 角色实体字段（严格对齐实体）
            new("Identity.Role", "Frontend", "角色", "Role", "ロール"),
            new("Identity.Role.RoleName", "Frontend", "角色名称", "Role Name", "ロール名"),
            new("Identity.Role.RoleCode", "Frontend", "角色编码", "Role Code", "ロールコード"),
            new("Identity.Role.Description", "Frontend", "描述", "Description", "説明"),
            new("Identity.Role.DataScope", "Frontend", "数据范围", "Data Scope", "データ範囲"),
            new("Identity.Role.UserCount", "Frontend", "用户数", "User Count", "ユーザー数"),
            new("Identity.Role.OrderNum", "Frontend", "排序号", "Order Num", "並び順"),
            new("Identity.Role.RoleStatus", "Frontend", "状态", "Status", "状態"),
            new("Identity.Role.Users", "Frontend", "角色用户", "Role Users", "ロールユーザー"),
            new("Identity.Role.Menus", "Frontend", "角色菜单", "Role Menus", "ロールメニュー"),
            new("Identity.Role.Keyword", "Frontend", "角色名称、编码", "role name, code", "ロール名、コード"),

            // 菜单实体字段（严格对齐实体）
            new("Identity.Menu", "Frontend", "菜单", "Menu", "メニュー"),
            new("Identity.Menu.MenuName", "Frontend", "菜单名称", "Menu Name", "メニュー名"),
            new("Identity.Menu.MenuCode", "Frontend", "菜单编码", "Menu Code", "メニューコード"),
            new("Identity.Menu.I18nKey", "Frontend", "国际化键", "I18n Key", "多言語キー"),
            new("Identity.Menu.PermCode", "Frontend", "权限码", "Permission Code", "権限コード"),
            new("Identity.Menu.MenuType", "Frontend", "菜单类型", "Menu Type", "メニュー種別"),
            new("Identity.Menu.ParentId", "Frontend", "父级ID", "Parent ID", "上位ID"),
            new("Identity.Menu.RoutePath", "Frontend", "路由路径", "Route Path", "ルートパス"),
            new("Identity.Menu.Icon", "Frontend", "图标", "Icon", "アイコン"),
            new("Identity.Menu.Component", "Frontend", "组件路径", "Component Path", "コンポーネントパス"),
            new("Identity.Menu.IsExternal", "Frontend", "是否外链", "Is External", "外部リンクか"),
            new("Identity.Menu.IsCache", "Frontend", "是否缓存", "Is Cache", "キャッシュか"),
            new("Identity.Menu.IsVisible", "Frontend", "是否可见", "Is Visible", "表示か"),
            new("Identity.Menu.OrderNum", "Frontend", "排序号", "Order Num", "並び順"),
            new("Identity.Menu.MenuStatus", "Frontend", "状态", "Status", "状態"),
            new("Identity.Menu.Roles", "Frontend", "角色菜单", "Role Menus", "ロールメニュー"),
            new("Identity.Menu.Keyword", "Frontend", "菜单名称、编码", "menu name, code", "メニュー名、コード"),

            // 用户会话实体字段
            new("Identity.UserSession", "Frontend", "用户会话", "User Session", "ユーザーセッション"),
            new("Identity.UserSession.SessionId", "Frontend", "会话ID", "Session ID", "セッションID"),
            new("Identity.UserSession.UserId", "Frontend", "用户ID", "User ID", "ユーザーID"),
            new("Identity.UserSession.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Identity.UserSession.RealName", "Frontend", "真实姓名", "Real Name", "氏名"),
            new("Identity.UserSession.RoleId", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("Identity.UserSession.RoleName", "Frontend", "角色名称", "Role Name", "ロール名"),
            new("Identity.UserSession.LoginTime", "Frontend", "登录时间", "Login Time", "ログイン時間"),
            new("Identity.UserSession.ExpiresAt", "Frontend", "过期时间", "Expires At", "有効期限"),
            new("Identity.UserSession.LastActivityTime", "Frontend", "最后活动时间", "Last Activity Time", "最終活動時刻"),
            new("Identity.UserSession.LoginIp", "Frontend", "登录IP", "Login IP", "ログインIP"),
            new("Identity.UserSession.ClientInfo", "Frontend", "客户端信息", "Client Info", "クライアント情報"),
            new("Identity.UserSession.ClientSnapshot", "Frontend", "客户端快照", "Client Snapshot", "クライアントスナップショット"),
            new("Identity.UserSession.OsDescription", "Frontend", "操作系统描述", "OS Description", "OS説明"),
            new("Identity.UserSession.OsVersion", "Frontend", "操作系统版本", "OS Version", "OSバージョン"),
            new("Identity.UserSession.OsType", "Frontend", "操作系统类型", "OS Type", "OS種別"),
            new("Identity.UserSession.OsArchitecture", "Frontend", "操作系统架构", "OS Architecture", "OSアーキテクチャ"),
            new("Identity.UserSession.MachineName", "Frontend", "机器名称", "Machine Name", "マシン名"),
            new("Identity.UserSession.MacAddress", "Frontend", "MAC地址", "MAC Address", "MACアドレス"),
            new("Identity.UserSession.FrameworkVersion", "Frontend", ".NET运行时版本", ".NET Runtime Version", ".NETランタイム"),
            new("Identity.UserSession.ProcessArchitecture", "Frontend", "进程架构", "Process Architecture", "プロセスアーキテクチャ"),
            new("Identity.UserSession.IsActive", "Frontend", "是否活跃", "Is Active", "有効か"),
            new("Identity.UserSession.LogoutTime", "Frontend", "登出时间", "Logout Time", "ログアウト時間"),
            new("Identity.UserSession.LogoutReason", "Frontend", "登出原因", "Logout Reason", "ログアウト理由"),
            new("Identity.UserSession.Keyword", "Frontend", "用户名、手机", "username, phone", "ユーザー名、電話"),

            // 角色菜单/用户角色关联实体字段
            new("Identity.RoleMenu", "Frontend", "角色菜单", "Role Menu", "ロールメニュー"),
            new("Identity.RoleMenu.RoleId", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("Identity.RoleMenu.MenuId", "Frontend", "菜单ID", "Menu ID", "メニューID"),
            new("Identity.RoleMenu.Keyword", "Frontend", "角色ID、菜单ID", "role id, menu id", "ロールID、メニューID"),
            new("Identity.UserRole", "Frontend", "用户角色", "User Role", "ユーザーロール"),
            new("Identity.UserRole.UserId", "Frontend", "用户ID", "User ID", "ユーザーID"),
            new("Identity.UserRole.RoleId", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("Identity.UserRole.Keyword", "Frontend", "用户名、角色名称", "username, role name", "ユーザー名、ロール名"),

            // 字典实体字段
            new("Routine.DictionaryType", "Frontend", "字典类型", "Dictionary Type", "辞書タイプ"),
            new("Routine.DictionaryType.TypeCode", "Frontend", "类型代码", "Type Code", "タイプコード"),
            new("Routine.DictionaryType.TypeName", "Frontend", "类型名称", "Type Name", "タイプ名"),
            new("Routine.DictionaryType.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.DictionaryType.IsBuiltin", "Frontend", "是否内置", "Is Built-in", "内蔵か"),
            new("Routine.DictionaryType.TypeStatus", "Frontend", "字典类型状态", "Type Status", "タイプ状態"),
            new("Routine.DictionaryType.Keyword", "Frontend", "类型代码、类型名称", "type code, type name", "タイプコード、タイプ名"),
            new("Routine.DictionaryData", "Frontend", "字典数据", "Dictionary Data", "辞書データ"),
            new("Routine.DictionaryData.TypeCode", "Frontend", "字典类型代码", "Dictionary Type Code", "辞書タイプコード"),
            new("Routine.DictionaryData.DataLabel", "Frontend", "数据标签", "Data Label", "データラベル"),
            new("Routine.DictionaryData.I18nKey", "Frontend", "国际化键", "I18n Key", "多言語キー"),
            new("Routine.DictionaryData.DataValue", "Frontend", "数据值", "Data Value", "データ値"),
            new("Routine.DictionaryData.ExtLabel", "Frontend", "扩展标签", "Ext Label", "拡張ラベル"),
            new("Routine.DictionaryData.ExtValue", "Frontend", "扩展值", "Ext Value", "拡張値"),
            new("Routine.DictionaryData.CssClass", "Frontend", "CSS类名", "CSS Class", "CSSクラス"),
            new("Routine.DictionaryData.ListClass", "Frontend", "列表CSS类名", "List CSS Class", "リストCSSクラス"),
            new("Routine.DictionaryData.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.DictionaryData.Keyword", "Frontend", "数据标签、数据值、国际化键", "data label, data value, i18n key", "データラベル、データ値、多言語キー"),
            
            // 字典数据操作相关
            new("Routine.Dictionary.CreateData", "Frontend", "新建字典数据", "Create Dictionary Data", "辞書データを作成"),
            new("Routine.Dictionary.UpdateData", "Frontend", "编辑字典数据", "Edit Dictionary Data", "辞書データを編集"),
            
            // 字典数据验证相关（仅保留特定业务规则，通用验证使用 validation.required/format/maxLength/minLength/invalid）
            new("Routine.Dictionary.Validation.DataLabelRequired", "Frontend", "数据标签不能为空", "Data label cannot be empty", "データラベルは空にできません"),
            new("Routine.Dictionary.Validation.DataLabelMaxLength", "Frontend", "数据标签长度不能超过100个字符", "Data label cannot exceed 100 characters", "データラベルは100文字を超えることはできません"),
            new("Routine.Dictionary.Validation.I18nKeyRequired", "Frontend", "国际化键不能为空", "I18n key cannot be empty", "多言語キーは空にできません"),
            new("Routine.Dictionary.Validation.I18nKeyMaxLength", "Frontend", "国际化键长度不能超过64个字符", "I18n key cannot exceed 64 characters", "多言語キーは64文字を超えることはできません"),
            
            new("Routine.Language", "Frontend", "语言", "Language", "言語"),
            new("Routine.Language.LanguageCode", "Frontend", "语言代码", "Language Code", "言語コード"),
            new("Routine.Language.LanguageName", "Frontend", "语言名称", "Language Name", "言語名"),
            new("Routine.Language.NativeName", "Frontend", "本地化名称", "Native Name", "現地名"),
            new("Routine.Language.LanguageIcon", "Frontend", "语言图标", "Language Icon", "言語アイコン"),
            new("Routine.Language.IsDefault", "Frontend", "是否默认", "Is Default", "既定か"),
            new("Routine.Language.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.Language.IsBuiltin", "Frontend", "是否内置", "Is Built-in", "内蔵か"),
            new("Routine.Language.LanguageStatus", "Frontend", "语言状态", "Language Status", "言語状態"),
            new("Routine.Language.Keyword", "Frontend", "语言代码、语言名称、本地化名称", "language code, language name, native name", "言語コード、言語名、現地名"),
            new("Routine.Translation", "Frontend", "翻译", "Translation", "翻訳"),
            new("Routine.Translation.LanguageCode", "Frontend", "语言代码", "Language Code", "言語コード"),
            new("Routine.Translation.TranslationKey", "Frontend", "翻译键", "Translation Key", "翻訳キー"),
            new("Routine.Translation.TranslationValue", "Frontend", "翻译值", "Translation Value", "翻訳値"),
            new("Routine.Translation.Module", "Frontend", "模块", "Module", "モジュール"),
            new("Routine.Translation.Description", "Frontend", "描述", "Description", "説明"),
            new("Routine.Translation.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.Translation.Keyword", "Frontend", "语言代码、翻译键", "language code, translation key", "言語コード、翻訳キー"),
            // 设置实体字段（严格对齐实体）
            new("Routine.Setting", "Frontend", "系统设置", "System Setting", "システム設定"),
            new("Routine.Setting.Key", "Frontend", "配置键", "Setting Key", "設定キー"),
            new("Routine.Setting.Value", "Frontend", "配置值", "Setting Value", "設定値"),
            new("Routine.Setting.Category", "Frontend", "分类", "Category", "カテゴリ"),
            new("Routine.Setting.OrderNum", "Frontend", "排序号", "Order Num", "並び順"),
            new("Routine.Setting.Type", "Frontend", "配置类型", "Setting Type", "設定タイプ"),
            new("Routine.Setting.IsBuiltin", "Frontend", "是否内置", "Is Built-in", "内蔵か"),
            new("Routine.Setting.IsDefault", "Frontend", "是否默认", "Is Default", "デフォルトか"),
            new("Routine.Setting.IsEditable", "Frontend", "是否可修改", "Is Editable", "編集可か"),
            new("Routine.Setting.Keyword", "Frontend", "配置键、分类", "setting key, category", "設定キー、カテゴリ"),


            // 登录日志实体字段
            new("Logging.LoginLog", "Frontend", "登录日志", "Login Log", "ログインログ"),
            new("Logging.LoginLog.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Logging.LoginLog.LoginTime", "Frontend", "登录时间", "Login Time", "ログイン時間"),
            new("Logging.LoginLog.LogoutTime", "Frontend", "登出时间", "Logout Time", "ログアウト時間"),
            new("Logging.LoginLog.LoginIp", "Frontend", "登录IP", "Login IP", "ログインIP"),
            new("Logging.LoginLog.MacAddress", "Frontend", "MAC地址", "MAC Address", "MACアドレス"),
            new("Logging.LoginLog.MachineName", "Frontend", "机器名称", "Machine Name", "マシン名"),
            new("Logging.LoginLog.LoginLocation", "Frontend", "登录地点", "Login Location", "ログイン場所"),
            new("Logging.LoginLog.Client", "Frontend", "客户端", "Client", "クライアント"),
            new("Logging.LoginLog.Os", "Frontend", "操作系统", "Operating System", "OS"),
            new("Logging.LoginLog.OsVersion", "Frontend", "操作系统版本", "OS Version", "OSバージョン"),
            new("Logging.LoginLog.OsArchitecture", "Frontend", "系统架构", "OS Architecture", "OSアーキテクチャ"),
            new("Logging.LoginLog.CpuInfo", "Frontend", "CPU信息", "CPU Info", "CPU情報"),
            new("Logging.LoginLog.TotalMemoryGb", "Frontend", "物理内存(GB)", "Total Memory (GB)", "物理メモリ(GB)"),
            new("Logging.LoginLog.FrameworkVersion", "Frontend", ".NET运行时", ".NET Runtime", ".NETランタイム"),
            new("Logging.LoginLog.IsAdmin", "Frontend", "是否管理员", "Is Admin", "管理者か"),
            new("Logging.LoginLog.ClientType", "Frontend", "客户端类型", "Client Type", "クライアント種別"),
            new("Logging.LoginLog.ClientVersion", "Frontend", "客户端版本", "Client Version", "クライアントバージョン"),
            new("Logging.LoginLog.LoginStatus", "Frontend", "登录状态", "Login Status", "ログイン状態"),
            new("Logging.LoginLog.FailReason", "Frontend", "失败原因", "Fail Reason", "失敗理由"),
            new("Logging.LoginLog.Keyword", "Frontend", "用户名、登录时间、登录IP、MAC地址、机器名称、登录地点、客户端、操作系统、操作系统版本、系统架构、CPU信息、物理内存(GB)、.NET运行时、是否管理员、客户端类型、客户端版本、登录状态、失败原因", "username, login time, login IP, MAC address, machine name, login location, client, operating system, operating system version, OS architecture, CPU info, total memory (GB), .NET runtime, is admin, client type, client version, login status, fail reason", "ユーザー名、ログイン時間、ログインIP、MACアドレス、マシン名、ログイン場所、クライアント、OS、OSバージョン、OSアーキテクチャ、CPU情報、物理メモリ(GB)、.NETランタイム、管理者か、クライアント種別、クライアントバージョン、ログイン状態、失敗理由"),

            // 异常日志实体字段
            new("Logging.ExceptionLog", "Frontend", "异常日志", "Exception Log", "例外ログ"),
            new("Logging.ExceptionLog.ExceptionType", "Frontend", "异常类型", "Exception Type", "例外種別"),
            new("Logging.ExceptionLog.ExceptionMessage", "Frontend", "异常消息", "Exception Message", "例外メッセージ"),
            new("Logging.ExceptionLog.StackTrace", "Frontend", "堆栈跟踪", "Stack Trace", "スタックトレース"),
            new("Logging.ExceptionLog.InnerException", "Frontend", "内部异常", "Inner Exception", "内部例外"),
            new("Logging.ExceptionLog.Level", "Frontend", "日志级别", "Log Level", "ログレベル"),
            new("Logging.ExceptionLog.ExceptionTime", "Frontend", "异常时间", "Exception Time", "例外時間"),
            new("Logging.ExceptionLog.RequestPath", "Frontend", "请求路径", "Request Path", "リクエストパス"),
            new("Logging.ExceptionLog.RequestMethod", "Frontend", "请求方法", "Request Method", "リクエスト方法"),
            new("Logging.ExceptionLog.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Logging.ExceptionLog.IpAddress", "Frontend", "IP地址", "IP Address", "IPアドレス"),
            new("Logging.ExceptionLog.UserAgent", "Frontend", "用户代理", "User Agent", "ユーザーエージェント"),
            new("Logging.ExceptionLog.Keyword", "Frontend", "用户名、异常类型、异常消息、堆栈跟踪、内部异常、日志级别、异常时间、请求路径、请求方法、IP地址、用户代理、操作系统、浏览器", "username, exception type, exception message, stack trace, inner exception, log level, exception time, request path, request method, IP address, user agent, operating system, browser", "ユーザー名、例外種別、例外メッセージ、スタックトレース、内部例外、ログレベル、例外時間、リクエストパス、リクエスト方法、IPアドレス、ユーザーエージェント、OS、ブラウザー"),

            // 操作日志实体字段
            new("Logging.OperationLog", "Frontend", "操作日志", "Operation Log", "操作ログ"),
            new("Logging.OperationLog.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Logging.OperationLog.OperationType", "Frontend", "操作类型", "Operation Type", "操作種別"),
            new("Logging.OperationLog.OperationModule", "Frontend", "操作模块", "Operation Module", "操作モジュール"),
            new("Logging.OperationLog.OperationDesc", "Frontend", "操作描述", "Operation Description", "操作説明"),
            new("Logging.OperationLog.OperationTime", "Frontend", "操作时间", "Operation Time", "操作時間"),
            new("Logging.OperationLog.RequestPath", "Frontend", "请求路径", "Request Path", "リクエストパス"),
            new("Logging.OperationLog.RequestMethod", "Frontend", "请求方法", "Request Method", "リクエスト方法"),
            new("Logging.OperationLog.RequestParams", "Frontend", "请求参数", "Request Parameters", "リクエストパラメーター"),
            new("Logging.OperationLog.ResponseResult", "Frontend", "响应结果", "Response Result", "応答結果"),
            new("Logging.OperationLog.ElapsedTime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("Logging.OperationLog.IpAddress", "Frontend", "IP地址", "IP Address", "IPアドレス"),
            new("Logging.OperationLog.UserAgent", "Frontend", "用户代理", "User Agent", "ユーザーエージェント"),
            new("Logging.OperationLog.Os", "Frontend", "操作系统", "Operating System", "OS"),
            new("Logging.OperationLog.Browser", "Frontend", "浏览器", "Browser", "ブラウザー"),
            new("Logging.OperationLog.OperationResult", "Frontend", "操作结果", "Operation Result", "操作結果"),
            new("Logging.OperationLog.Keyword", "Frontend", "用户名、操作类型、操作模块、操作描述、操作时间、请求路径、请求方法、请求参数、响应结果、执行耗时、IP地址、用户代理、操作系统、浏览器、操作结果", "username, operation type, operation module, operation description, operation time, request path, request method, request parameters, response result, elapsed time, IP address, user agent, operating system, browser, operation result", "ユーザー名、操作種別、操作モジュール、操作説明、操作時間、リクエストパス、リクエスト方法、リクエストパラメーター、応答結果、処理時間、IPアドレス、ユーザーエージェント、OS、ブラウザー、操作結果"),

            // 差异日志实体字段
            new("Logging.DiffLog", "Frontend", "差异日志", "Diff Log", "差分ログ"),
            new("Logging.DiffLog.TableName", "Frontend", "表名", "Table Name", "テーブル名"),
            new("Logging.DiffLog.DiffType", "Frontend", "差异类型", "Diff Type", "差分種別"),
            new("Logging.DiffLog.BusinessData", "Frontend", "业务数据", "Business Data", "業務データ"),
            new("Logging.DiffLog.BeforeData", "Frontend", "变更前数据", "Before Data", "変更前データ"),
            new("Logging.DiffLog.AfterData", "Frontend", "变更后数据", "After Data", "変更後データ"),
            new("Logging.DiffLog.Sql", "Frontend", "执行SQL", "Executed SQL", "実行SQL"),
            new("Logging.DiffLog.Parameters", "Frontend", "SQL参数", "SQL Parameters", "SQLパラメーター"),
            new("Logging.DiffLog.DiffTime", "Frontend", "差异时间", "Diff Time", "差分時間"),
            new("Logging.DiffLog.ElapsedTime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("Logging.DiffLog.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Logging.DiffLog.IpAddress", "Frontend", "IP地址", "IP Address", "IPアドレス"),
            new("Logging.DiffLog.Keyword", "Frontend", "表名、差异类型、业务数据、变更前数据、变更后数据、执行SQL、SQL参数、差异时间、执行耗时、用户名、IP地址", "table name, diff type, business data, before data, after data, executed SQL, SQL parameters, diff time, elapsed time, username, IP address", "テーブル名、差分種別、業務データ、変更前データ、変更後データ、実行SQL、SQLパラメーター、差分時間、処理時間、ユーザー名、IPアドレス"),

            // 生产物料实体字段
            new("Logistics.ProdMaterial", "Frontend", "生产物料", "Production Material", "生産資材"),
            new("Logistics.ProdMaterial.Plant", "Frontend", "工厂", "Plant", "工場"),
            new("Logistics.ProdMaterial.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.ProdMaterial.IndustryField", "Frontend", "行业领域", "Industry Field", "業界分野"),
            new("Logistics.ProdMaterial.MaterialType", "Frontend", "物料类型", "Material Type", "資材種別"),
            new("Logistics.ProdMaterial.MaterialDescription", "Frontend", "物料描述", "Material Description", "資材説明"),
            new("Logistics.ProdMaterial.BaseUnit", "Frontend", "基本计量单位", "Base Unit", "基準単位"),
            new("Logistics.ProdMaterial.ProductHierarchy", "Frontend", "产品层次", "Product Hierarchy", "製品階層"),
            new("Logistics.ProdMaterial.MaterialGroup", "Frontend", "物料组", "Material Group", "資材グループ"),
            new("Logistics.ProdMaterial.PurchaseGroup", "Frontend", "采购组", "Purchase Group", "購買グループ"),
            new("Logistics.ProdMaterial.PurchaseType", "Frontend", "采购类型", "Purchase Type", "購買種別"),
            new("Logistics.ProdMaterial.SpecialPurchaseType", "Frontend", "特殊采购类", "Special Purchase Type", "特別購買種"),
            new("Logistics.ProdMaterial.BulkMaterial", "Frontend", "散装物料", "Bulk Material", "バルク資材"),
            new("Logistics.ProdMaterial.MinimumOrderQuantity", "Frontend", "最小起订量", "Minimum Order Quantity", "最小発注量"),
            new("Logistics.ProdMaterial.RoundingValue", "Frontend", "舍入值", "Rounding Value", "丸め値"),
            new("Logistics.ProdMaterial.PlannedDeliveryTime", "Frontend", "计划交货时间", "Planned Delivery Time", "計画納期"),
            new("Logistics.ProdMaterial.SelfProductionDays", "Frontend", "自制生产天数", "Self Production Days", "自社生産日数"),
            new("Logistics.ProdMaterial.PostToInspectionStock", "Frontend", "过账到检验库存", "Post To Inspection Stock", "検査在庫へ転記"),
            new("Logistics.ProdMaterial.ProfitCenter", "Frontend", "利润中心", "Profit Center", "損益センター"),
            new("Logistics.ProdMaterial.VarianceCode", "Frontend", "差异码", "Variance Code", "差異コード"),
            new("Logistics.ProdMaterial.BatchManagement", "Frontend", "批次管理", "Batch Management", "ロット管理"),
            new("Logistics.ProdMaterial.ManufacturerPartNumber", "Frontend", "制造商零件编号", "Manufacturer Part Number", "メーカー部品番号"),
            new("Logistics.ProdMaterial.Manufacturer", "Frontend", "制造商", "Manufacturer", "メーカー"),
            new("Logistics.ProdMaterial.EvaluationType", "Frontend", "评估类", "Evaluation Type", "評価種類"),
            new("Logistics.ProdMaterial.MovingAveragePrice", "Frontend", "移动平均价", "Moving Average Price", "移動平均価格"),
            new("Logistics.ProdMaterial.Currency", "Frontend", "货币", "Currency", "通貨"),
            new("Logistics.ProdMaterial.PriceControl", "Frontend", "价格控制", "Price Control", "価格管理"),
            new("Logistics.ProdMaterial.PriceUnit", "Frontend", "价格单位", "Price Unit", "価格単位"),
            new("Logistics.ProdMaterial.ProductionStorageLocation", "Frontend", "生产仓储地点", "Production Storage Location", "生産保管場所"),
            new("Logistics.ProdMaterial.ExternalPurchaseStorageLocation", "Frontend", "外部采购仓储地点", "External Purchase Storage Location", "外部調達保管場所"),
            new("Logistics.ProdMaterial.StoragePosition", "Frontend", "仓位", "Storage Position", "保管位置"),
            new("Logistics.ProdMaterial.CrossPlantMaterialStatus", "Frontend", "跨工厂物料状态", "Cross-Plant Material Status", "プラント間資材状態"),
            new("Logistics.ProdMaterial.StockQuantity", "Frontend", "在库数量", "Stock Quantity", "在庫数量"),
            new("Logistics.ProdMaterial.HsCode", "Frontend", "HS编码", "HS Code", "HSコード"),
            new("Logistics.ProdMaterial.HsName", "Frontend", "HS名称", "HS Name", "HS名称"),
            new("Logistics.ProdMaterial.MaterialWeight", "Frontend", "重量", "Material Weight", "重量"),
            new("Logistics.ProdMaterial.MaterialVolume", "Frontend", "容积", "Material Volume", "容積"),
            new("Logistics.ProdMaterial.Keyword", "Frontend", "物料编码、物料名称、物料描述、基本计量单位、产品层次、物料组、采购组、采购类型、特殊采购类、散装物料、最小起订量、舍入值、计划交货时间、自制生产天数、过账到检验库存、利润中心、差异码、批次管理、制造商零件编号、制造商、评估类、移动平均价、货币、价格控制、价格单位、生产仓储地点、外部采购仓储地点、仓位、跨工厂物料状态、在库数量、HS编码、HS名称、重量、容积", "material code, material name, material description, base unit, product hierarchy, material group, purchase group, purchase type, special purchase type, bulk material, minimum order quantity, rounding value, planned delivery time, self production days, post to inspection stock, profit center, variance code, batch management, manufacturer part number, manufacturer, evaluation type, moving average price, currency, price control, price unit, production storage location, external purchase storage location, storage position, cross-plant material status, stock quantity, HS code, HS name, material weight, material volume", "資材コード、資材名、資材説明、基準単位、製品階層、資材グループ、購買グループ、購買種別、特別購買種、バルク資材、最小発注量、丸め値、計画納期、自社生産日数、検査在庫へ転記、損益センター、差異コード、ロット管理、メーカー部品番号、メーカー、評価種類、移動平均価格、通貨、価格管理、価格単位、生産保管場所、外部調達保管場所、保管位置、プラント間資材状態、在庫数量、HSコード、HS名称、重量、容積"),

            // 产品机种实体字段
            new("Logistics.ProdModel", "Frontend", "产品机种", "Product Model", "製品機種"),
            new("Logistics.ProdModel.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.ProdModel.ModelCode", "Frontend", "机种编码", "Model Code", "機種コード"),
            new("Logistics.ProdModel.DestCode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("Logistics.ProdModel.Keyword", "Frontend", "物料编码、机种编码、仕向编码", "material code, model code, destination code", "資材コード、機種コード、仕向コード"),

            // 产品序列号主表字段
            new("Logistics.ProdSerial", "Frontend", "产品序列号", "Product Serial", "製品シリアル"),
            new("Logistics.ProdSerial.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.ProdSerial.ModelCode", "Frontend", "机种编码", "Model Code", "機種コード"),
            new("Logistics.ProdSerial.DestCode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("Logistics.ProdSerial.Keyword", "Frontend", "物料编码、机种编码、仕向编码", "material code, model code, destination code", "資材コード、機種コード、仕向コード"),

            // 产品序列号入库字段
            new("Logistics.ProdSerialInbound", "Frontend", "序列号入库", "Serial Inbound", "シリアル入庫"),
            new("Logistics.ProdSerialInbound.FullSerialNumber", "Frontend", "完整序列号", "Full Serial Number", "完全シリアル番号"),
            new("Logistics.ProdSerialInbound.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.ProdSerialInbound.SerialNumber", "Frontend", "真正序列号", "Serial Number", "シリアル番号"),
            new("Logistics.ProdSerialInbound.Quantity", "Frontend", "数量", "Quantity", "数量"),
            new("Logistics.ProdSerialInbound.InboundNo", "Frontend", "入库单号", "Inbound No.", "入庫番号"),
            new("Logistics.ProdSerialInbound.InboundDate", "Frontend", "入库日期", "Inbound Date", "入庫日"),
            new("Logistics.ProdSerialInbound.Warehouse", "Frontend", "仓库", "Warehouse", "倉庫"),
            new("Logistics.ProdSerialInbound.Location", "Frontend", "库位", "Location", "ロケーション"),
            new("Logistics.ProdSerialInbound.Operator", "Frontend", "入库员", "Operator", "入庫担当"),
            new("Logistics.ProdSerialInbound.Keyword", "Frontend", "完整序列号、物料编码、真正序列号、数量、入库单号、入库日期、仓库、库位、入库员", "full serial number, material code, serial number, quantity, inbound no., inbound date, warehouse, location, operator", "完全シリアル番号、資材コード、シリアル番号、数量、入庫番号、入庫日、倉庫、ロケーション、入庫担当"),

            // 产品序列号出库字段
            new("Logistics.ProdSerialOutbound", "Frontend", "序列号出库", "Serial Outbound", "シリアル出庫"),
            new("Logistics.ProdSerialOutbound.FullSerialNumber", "Frontend", "完整序列号", "Full Serial Number", "完全シリアル番号"),
            new("Logistics.ProdSerialOutbound.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.ProdSerialOutbound.SerialNumber", "Frontend", "真正序列号", "Serial Number", "シリアル番号"),
            new("Logistics.ProdSerialOutbound.Quantity", "Frontend", "数量", "Quantity", "数量"),
            new("Logistics.ProdSerialOutbound.OutboundNo", "Frontend", "出库单号", "Outbound No.", "出庫番号"),
            new("Logistics.ProdSerialOutbound.OutboundDate", "Frontend", "出库日期", "Outbound Date", "出庫日"),
            new("Logistics.ProdSerialOutbound.Destination", "Frontend", "仕向地", "Destination", "仕向地"),
            new("Logistics.ProdSerialOutbound.DestCode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("Logistics.ProdSerialOutbound.DestinationPort", "Frontend", "目的地港口", "Destination Port", "目的地港"),
            new("Logistics.ProdSerialOutbound.Operator", "Frontend", "出库员", "Operator", "出庫担当"),
            new("Logistics.ProdSerialOutbound.Keyword", "Frontend", "完整序列号、物料编码、真正序列号、数量、出库单号、出库日期、仕向地、仕向编码、目的地港口、出库员", "full serial number, material code, serial number, quantity, outbound no., outbound date, destination, destination code, destination port, operator", "完全シリアル番号、資材コード、シリアル番号、数量、出庫番号、出庫日、仕向地、仕向コード、目的地港、出庫担当"),

            // 访客实体字段
            new("Logistics.Visitor", "Frontend", "访客", "Visitor", "訪問者"),
            new("Logistics.Visitor.CompanyName", "Frontend", "公司名称", "Company Name", "会社名"),
            new("Logistics.Visitor.StartTime", "Frontend", "起始时间", "Start Time", "開始時間"),
            new("Logistics.Visitor.EndTime", "Frontend", "结束时间", "End Time", "終了時間"),
            new("Logistics.Visitor.Keyword", "Frontend", "公司名称、起始时间、结束时间", "company name, start time, end time", "会社名、開始時間、終了時間"),

            // 访客详情实体字段
            new("Logistics.VisitorDetail", "Frontend", "访客详情", "Visitor Detail", "訪問者詳細"),
            new("Logistics.VisitorDetail.VisitorId", "Frontend", "访客ID", "Visitor ID", "訪問者ID"),
            new("Logistics.VisitorDetail.Department", "Frontend", "部门", "Department", "部署"),
            new("Logistics.VisitorDetail.Name", "Frontend", "姓名", "Name", "氏名"),
            new("Logistics.VisitorDetail.Position", "Frontend", "职务", "Position", "職務"),
            new("Logistics.VisitorDetail.Keyword", "Frontend", "访客ID、部门、姓名、职务", "visitor id, department, name, position", "訪問者ID、部署、氏名、職務"),

            // 代码生成表配置实体字段
            new("Generator.GenTable", "Frontend", "代码生成表配置", "Code Generation Table Config", "コード生成テーブル設定"),
            new("Generator.GenTable.TableName", "Frontend", "库表名称", "Table Name", "テーブル名"),
            new("Generator.GenTable.TableNameHint", "Frontend", "提示：用于无数据表的手动配置，表名可以不存在于数据库中", "Hint: For manual configuration without database table, table name may not exist in database", "ヒント：データベーステーブルなしの手動設定用。テーブル名はデータベースに存在しない場合があります"),
            new("Generator.GenTable.TableDescription", "Frontend", "库表描述", "Table Description", "テーブル説明"),
            new("Generator.GenTable.ClassName", "Frontend", "实体类名称", "Class Name", "クラス名"),
            new("Generator.GenTable.Namespace", "Frontend", "命名空间", "Namespace", "名前空間"),
            new("Generator.GenTable.ModuleCode", "Frontend", "模块标识", "Module Code", "モジュールコード"),
            new("Generator.GenTable.ModuleName", "Frontend", "模块名称", "Module Name", "モジュール名"),
            new("Generator.GenTable.ParentTableId", "Frontend", "主表ID", "Parent Table ID", "親テーブルID"),
            new("Generator.GenTable.DetailTableName", "Frontend", "子表名称", "Detail Table Name", "詳細テーブル名"),
            new("Generator.GenTable.DetailComment", "Frontend", "子表描述", "Detail Comment", "詳細コメント"),
            new("Generator.GenTable.DetailRelationField", "Frontend", "子表关联字段", "Detail Relation Field", "詳細関連フィールド"),
            new("Generator.GenTable.TreeCodeField", "Frontend", "树编码字段", "Tree Code Field", "ツリーコードフィールド"),
            new("Generator.GenTable.TreeParentCodeField", "Frontend", "树父编码字段", "Tree Parent Code Field", "ツリー親コードフィールド"),
            new("Generator.GenTable.TreeNameField", "Frontend", "树名称字段", "Tree Name Field", "ツリー名フィールド"),
            new("Generator.GenTable.Author", "Frontend", "作者", "Author", "作成者"),
            new("Generator.GenTable.TemplateType", "Frontend", "生成模板类型", "Template Type", "テンプレートタイプ"),
            new("Generator.GenTable.GenNamespacePrefix", "Frontend", "命名空间前缀", "Namespace Prefix", "名前空間プレフィックス"),
            new("Generator.GenTable.GenBusinessName", "Frontend", "生成业务名称", "Business Name", "ビジネス名"),
            new("Generator.GenTable.GenModuleName", "Frontend", "生成模块名称", "Gen Module Name", "生成モジュール名"),
            new("Generator.GenTable.GenFunctionName", "Frontend", "生成功能名", "Function Name", "機能名"),
            new("Generator.GenTable.GenType", "Frontend", "生成方式", "Gen Type", "生成タイプ"),
            new("Generator.GenTable.GenFunctions", "Frontend", "生成功能", "Gen Functions", "生成機能"),
            new("Generator.GenTable.GenPath", "Frontend", "代码生成路径", "Gen Path", "生成パス"),
            new("Generator.GenTable.Options", "Frontend", "其它生成选项", "Options", "その他のオプション"),
            new("Generator.GenTable.ParentMenuName", "Frontend", "上级菜单名称", "Parent Menu Name", "親メニュー名"),
            new("Generator.GenTable.PermissionPrefix", "Frontend", "权限前缀", "Permission Prefix", "権限プレフィックス"),
            new("Generator.GenTable.IsDatabaseTable", "Frontend", "是否有表", "Is Database Table", "テーブルがあるか"),
            new("Generator.GenTable.IsGenMenu", "Frontend", "是否生成菜单", "Is Gen Menu", "メニュー生成か"),
            new("Generator.GenTable.IsGenTranslation", "Frontend", "是否生成翻译", "Is Gen Translation", "翻訳生成か"),
            new("Generator.GenTable.IsGenCode", "Frontend", "是否生成代码", "Is Gen Code", "コード生成か"),
            new("Generator.GenTable.DefaultSortField", "Frontend", "默认排序字段", "Default Sort Field", "デフォルトソートフィールド"),
            new("Generator.GenTable.DefaultSortOrder", "Frontend", "默认排序", "Default Sort Order", "デフォルトソート順"),
            new("Generator.GenTable.Keyword", "Frontend", "库表名称、库表描述、实体类名称、模块标识、模块名称", "table name, table description, class name, module code, module name", "テーブル名、テーブル説明、クラス名、モジュールコード、モジュール名"),
            new("Generator.GenTable.BasicInfo", "Frontend", "基本信息", "Basic Information", "基本情報"),
            new("Generator.GenTable.GenConfig", "Frontend", "生成配置", "Generation Config", "生成設定"),
            new("Generator.GenTable.ColumnInfo", "Frontend", "列配置", "Column Config", "カラム設定"),
            new("Generator.GenTable.Create", "Frontend", "新建代码生成配置", "Create Code Generation Config", "コード生成設定を作成"),
            new("Generator.GenTable.Update", "Frontend", "编辑代码生成配置", "Edit Code Generation Config", "コード生成設定を編集"),
            new("Generator.GenTable.DeleteConfirm", "Frontend", "确定要删除该代码生成配置吗？", "Are you sure you want to delete this code generation config?", "このコード生成設定を削除してもよろしいですか？"),
            new("Generator.GenTable.SyncTitle", "Frontend", "选择同步方向", "Select Sync Direction", "同期方向を選択"),
            new("Generator.GenTable.SyncFromDatabase", "Frontend", "从数据库同步到配置", "Sync from Database to Config", "データベースから設定へ同期"),
            new("Generator.GenTable.SyncToDatabase", "Frontend", "从配置同步到数据库", "Sync from Config to Database", "設定からデータベースへ同期"),
            new("Generator.GenTable.SyncFromDatabaseConfirm", "Frontend", "确定要从数据库同步表结构到配置吗？", "Are you sure you want to sync table structure from database to config?", "データベースから設定へテーブル構造を同期してもよろしいですか？"),
            new("Generator.GenTable.SyncToDatabaseConfirm", "Frontend", "确定要从配置同步表结构到数据库吗？", "Are you sure you want to sync table structure from config to database?", "設定からデータベースへテーブル構造を同期してもよろしいですか？"),
            new("Generator.GenTable.SyncFromDatabaseSuccess", "Frontend", "从数据库同步成功", "Sync from database successful", "データベースから同期成功"),
            new("Generator.GenTable.SyncToDatabaseSuccess", "Frontend", "同步到数据库成功", "Sync to database successful", "データベースへ同期成功"),
            new("Generator.GenTable.Validation.TableNameRequired", "Frontend", "表名不能为空", "Table name cannot be empty", "テーブル名は空にできません"),

            // 代码生成列配置实体字段
            new("Generator.GenColumn", "Frontend", "代码生成列配置", "Code Generation Column Config", "コード生成カラム設定"),           
            new("Generator.GenColumn.TableName", "Frontend", "表名", "Table Name", "テーブル名"),
            new("Generator.GenColumn.ColumnName", "Frontend", "列名", "Column Name", "カラム名"),
            new("Generator.GenColumn.ColumnDescription", "Frontend", "列描述", "Column Description", "カラム説明"),
            new("Generator.GenColumn.ColumnDataType", "Frontend", "库列类型", "Column Data Type", "カラムデータタイプ"),
            new("Generator.GenColumn.PropertyName", "Frontend", "属性名称", "Property Name", "プロパティ名"),
            new("Generator.GenColumn.DataType", "Frontend", "C#类型", "C# Type", "C#タイプ"),
            new("Generator.GenColumn.IsNullable", "Frontend", "可空", "Nullable", "NULL許可"),
            new("Generator.GenColumn.IsPrimaryKey", "Frontend", "主键", "Primary Key", "主キー"),
            new("Generator.GenColumn.IsIdentity", "Frontend", "自增", "Identity", "自動増分"),
            new("Generator.GenColumn.Length", "Frontend", "长度", "Length", "長さ"),
            new("Generator.GenColumn.DecimalPlaces", "Frontend", "精度", "Decimal Places", "小数点以下桁数"),
            new("Generator.GenColumn.DefaultValue", "Frontend", "默认值", "Default Value", "デフォルト値"),
            new("Generator.GenColumn.OrderNum", "Frontend", "库列排序", "Order Number", "順序"),
            new("Generator.GenColumn.IsQuery", "Frontend", "查询", "Query", "クエリ"),
            new("Generator.GenColumn.QueryType", "Frontend", "查询方式", "Query Type", "クエリタイプ"),
            new("Generator.GenColumn.IsCreate", "Frontend", "创建", "Create", "作成"),
            new("Generator.GenColumn.IsUpdate", "Frontend", "更新", "Update", "更新"),
            new("Generator.GenColumn.IsDelete", "Frontend", "删除", "Delete", "削除"),
            new("Generator.GenColumn.IsList", "Frontend", "列表", "List", "リスト"),
            new("Generator.GenColumn.IsExport", "Frontend", "导出", "Export", "エクスポート"),
            new("Generator.GenColumn.IsSort", "Frontend", "排序", "Sort", "ソート"),
            new("Generator.GenColumn.IsRequired", "Frontend", "必填", "Required", "必須"),
            new("Generator.GenColumn.IsForm", "Frontend", "表单显示", "Form Display", "フォーム表示"),
            new("Generator.GenColumn.FormControlType", "Frontend", "表单类型", "Form Type", "フォームタイプ"),
            new("Generator.GenColumn.DictType", "Frontend", "字典类型", "Dict Type", "辞書タイプ"),
            new("Generator.GenColumn.Keyword", "Frontend", "表名、列名、属性名称、列描述", "table name, column name, property name, column description", "テーブル名、カラム名、プロパティ名、カラム説明"),

            // 代码生成导入表相关
            new("Generator.ImportTable", "Frontend", "导入表", "Import Table", "テーブルをインポート"),
            new("Generator.ImportTable.Title", "Frontend", "导入数据库表", "Import Database Table", "データベーステーブルをインポート"),
            new("Generator.ImportTable.TableName", "Frontend", "表名", "Table Name", "テーブル名"),
            new("Generator.ImportTable.Description", "Frontend", "表描述", "Table Description", "テーブル説明"),
            new("Generator.ImportTable.ColumnName", "Frontend", "列名", "Column Name", "列名"),
            new("Generator.ImportTable.ColumnDescription", "Frontend", "列描述", "Column Description", "列説明"),
            new("Generator.ImportTable.DataType", "Frontend", "数据类型", "Data Type", "データ型"),
            new("Generator.ImportTable.Length", "Frontend", "长度", "Length", "長さ"),
            new("Generator.ImportTable.DecimalPlaces", "Frontend", "精度", "Decimal Places", "小数点以下桁数"),
            new("Generator.ImportTable.DefaultValue", "Frontend", "默认值", "Default Value", "デフォルト値"),
            new("Generator.ImportTable.IsPrimaryKey", "Frontend", "主键", "Primary Key", "主キー"),
            new("Generator.ImportTable.IsIdentity", "Frontend", "自增", "Identity", "自動増分"),
            new("Generator.ImportTable.IsNullable", "Frontend", "可空", "Nullable", "NULL許可"),
            new("Generator.ImportTable.NoSelection", "Frontend", "请至少选择一个表", "Please select at least one table", "少なくとも1つのテーブルを選択してください"),
        };
    }

    private sealed record TranslationSeed(string Key, string Module, string ZhCn, string EnUs, string JaJp);
}

