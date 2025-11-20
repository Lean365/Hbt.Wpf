//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : DbSeedRoutine.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : Routine 模块种子数据初始化服务
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//===================================================================

using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Routine 模块种子数据初始化服务
/// 创建语言、翻译、字典和系统设置的种子数据
/// </summary>
public class DbSeedRoutineLanguage
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly IBaseRepository<Translation> _translationRepository;

    public DbSeedRoutineLanguage(
        InitLogManager initLog,
        IBaseRepository<Language> languageRepository,
        IBaseRepository<Translation> translationRepository)
    {
        _initLog = initLog;
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
    }

    /// <summary>
    /// 初始化 Routine 模块种子数据
    /// </summary>
    public void Initialize()
    {
        _initLog.Information("开始初始化 Routine 模块种子数据..");

        // 1. 初始化语言数据
        var languages = InitializeLanguages();

        // 2. 初始化翻译数据
        InitializeTranslations(languages);

        _initLog.Information("Routine 模块通用翻译初始化完成");
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
            _languageRepository.Create(zhCn, "Takt365");
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
            _languageRepository.Create(enUs, "Takt365");
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
            _languageRepository.Create(jaJp, "Takt365");
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

        // 翻译数据定义（基础通用项）
        var translations = new List<dynamic>
        {
            // 登录相关
            new { Key = "Login.Welcome", Module = "Frontend", ZhCn = "欢迎登录", EnUs = "Welcome", JaJp = "ようこそ" },
            new { Key = "Login.Title", Module = "Frontend", ZhCn = "节拍中小企业管理平台 - 登录", EnUs = "Takt SMEs Platform - Login", JaJp = "Takt SMEsプラットフォーム - ログイン" },
            new { Key = "Login.Username", Module = "Frontend", ZhCn = "用户名", EnUs = "Username", JaJp = "ユーザー名" },
            new { Key = "Login.Password", Module = "Frontend", ZhCn = "密码", EnUs = "Password", JaJp = "パスワード" },
            new { Key = "Login.RememberMe", Module = "Frontend", ZhCn = "记住密码", EnUs = "Remember Me", JaJp = "パスワードを記憶する" },
            new { Key = "Login.Forgot", Module = "Frontend", ZhCn = "忘记密码？", EnUs = "Forgot password?", JaJp = "パスワードをお忘れの方" },
            new { Key = "Login.Button", Module = "Frontend", ZhCn = "登录", EnUs = "Login", JaJp = "ログイン" },
            new { Key = "Login.Loading", Module = "Frontend", ZhCn = "登录中...", EnUs = "Signing in...", JaJp = "ログイン中..." },
            new { Key = "Login.Error", Module = "Frontend", ZhCn = "登录失败：{0}", EnUs = "Login failed: {0}", JaJp = "ログイン失敗：{0}" },
            new { Key = "Login.Description", Module = "Frontend", ZhCn = "请输入您的账号信息", EnUs = "Please enter your account information", JaJp = "アカウント情報を入力してください" },
            
            // 审计字段相关
            new { Key = "common.audit.Id", Module = "Frontend", ZhCn = "主键", EnUs = "ID", JaJp = "ID" },
            new { Key = "common.audit.id", Module = "Frontend", ZhCn = "主键", EnUs = "ID", JaJp = "ID" }, // 小写版本（兼容性）
            new { Key = "common.audit.Remarks", Module = "Frontend", ZhCn = "备注", EnUs = "Remarks", JaJp = "備考" },
            new { Key = "common.audit.remarks", Module = "Frontend", ZhCn = "备注", EnUs = "Remarks", JaJp = "備考" }, // 小写版本（兼容性）
            new { Key = "common.audit.CreatedBy", Module = "Frontend", ZhCn = "创建人", EnUs = "Created By", JaJp = "作成者" },
            new { Key = "common.audit.CreatedTime", Module = "Frontend", ZhCn = "创建时间", EnUs = "Created Time", JaJp = "作成時間" },
            new { Key = "common.audit.created_time", Module = "Frontend", ZhCn = "创建时间", EnUs = "Created Time", JaJp = "作成時間" }, // 下划线版本（兼容性）
            new { Key = "common.audit.UpdatedBy", Module = "Frontend", ZhCn = "更新人", EnUs = "Updated By", JaJp = "更新者" },
            new { Key = "common.audit.UpdatedTime", Module = "Frontend", ZhCn = "更新时间", EnUs = "Updated Time", JaJp = "更新時間" },
            new { Key = "common.audit.DeletedBy", Module = "Frontend", ZhCn = "删除人", EnUs = "Deleted By", JaJp = "削除者" },
            new { Key = "common.audit.DeletedTime", Module = "Frontend", ZhCn = "删除时间", EnUs = "Deleted Time", JaJp = "削除時間" },
            new { Key = "common.audit.IsDeleted", Module = "Frontend", ZhCn = "是否删除", EnUs = "Is Deleted", JaJp = "削除済み" },
            
            
            // 通用基础信息
            new { Key = "Common.CompanyName", Module = "Frontend", ZhCn = "节拍Takt", EnUs = "Takt", JaJp = "タクトTakt" },
            new { Key = "Common.CompanySlogan", Module = "Frontend", ZhCn = "中小企业管理平台", EnUs = "SMEs Platform", JaJp = "SMEs Platform" },
            new { Key = "Common.CompanyTagline", Module = "Frontend", ZhCn = "数据驱动・决策精准・赋能业务", EnUs = "Data. Precision. Empowerment.", JaJp = "データ・精密・業務革新" },
            new { Key = "Common.Copyright", Module = "Frontend", ZhCn = "Takt All Rights Reserved.节拍信息 保留所有权利.", EnUs = "Takt All Rights Reserved.", JaJp = "タクト情報技術 全著作権所有." },
            new { Key = "Common.CopyrightShort", Module = "Frontend", ZhCn = "节拍", EnUs = "Takt", JaJp = "タクト" },
            new { Key = "Common.Loading", Module = "Frontend", ZhCn = "加载中...", EnUs = "Loading...", JaJp = "読み込み中..." },
            
            // 仪表盘欢迎语
            new { Key = "dashboard.greeting.morning", Module = "Frontend", ZhCn = "早上好", EnUs = "Good morning", JaJp = "おはようございます" },
            new { Key = "dashboard.greeting.noon", Module = "Frontend", ZhCn = "中午好", EnUs = "Good noon", JaJp = "こんにちは" },
            new { Key = "dashboard.greeting.afternoon", Module = "Frontend", ZhCn = "下午好", EnUs = "Good afternoon", JaJp = "こんにちは" },
            new { Key = "dashboard.greeting.evening", Module = "Frontend", ZhCn = "晚上好", EnUs = "Good evening", JaJp = "こんばんは" },
            new { Key = "dashboard.greeting.night", Module = "Frontend", ZhCn = "夜深了，请注意休息", EnUs = "It's late, please take a rest", JaJp = "夜も遅いです。ごゆっくりお休みください" },
            new { Key = "dashboard.greeting.welcomeFormat", Module = "Frontend", ZhCn = "{0}，欢迎 {1}", EnUs = "{0}, welcome {1}", JaJp = "{0}、ようこそ {1} さん" },
            new { Key = "dashboard.greeting.anonymousName", Module = "Frontend", ZhCn = "访客", EnUs = "Guest", JaJp = "ゲスト" },
            new { Key = "dashboard.greeting.fullFormat", Module = "Frontend", ZhCn = "{0}，欢迎 {1}，今天是{2}年{3}月{4}日，{5}，（第{6}天，第{7}季，第{8}周）", EnUs = "{0}, welcome {1}. Today is {2}-{3}-{4}, {5}, (Day {6}, Quarter {7}, Week {8})", JaJp = "{0}、{1} さん、ようこそ。本日は{2}年{3}月{4}日、{5}、（{6}日目、第{7}四半期、第{8}週）" },
            
            // 关于页面
            new { Key = "about.description", Module = "Frontend", ZhCn = "节拍中后台管理平台是一套面向中小企业的智能化管理系统，提供身份认证、权限控制、日常事务和后勤等一体化能力。", EnUs = "The SMEs Platform is an intelligent management suite for small and medium enterprises, offering integrated identity, authorization, routine and logistics capabilities.", JaJp = "SMEsプラットフォームは、中小企業向けの統合管理システムで、認証、権限管理、日常業務、ロジスティクスなどを一体的に提供します。" },
            new { Key = "about.technology", Module = "Frontend", ZhCn = "技术栈", EnUs = "Technology Stack", JaJp = "技術スタック" },
            new { Key = "about.framework", Module = "Frontend", ZhCn = "开发框架", EnUs = "Development Framework", JaJp = "開発フレームワーク" },
            new { Key = "about.framework.value", Module = "Frontend", ZhCn = ".NET 9.0", EnUs = ".NET 9.0", JaJp = ".NET 9.0" },
            new { Key = "about.uiFramework", Module = "Frontend", ZhCn = "界面框架", EnUs = "UI Framework", JaJp = "UIフレームワーク" },
            new { Key = "about.uiFramework.value", Module = "Frontend", ZhCn = "WPF（Windows Presentation Foundation）", EnUs = "WPF (Windows Presentation Foundation)", JaJp = "WPF（Windows Presentation Foundation）" },
            new { Key = "about.database", Module = "Frontend", ZhCn = "数据存储", EnUs = "Data Storage", JaJp = "データストレージ" },
            new { Key = "about.database.value", Module = "Frontend", ZhCn = "SQL Server / SqlSugar ORM", EnUs = "SQL Server / SqlSugar ORM", JaJp = "SQL Server / SqlSugar ORM" },
            new { Key = "about.architecture", Module = "Frontend", ZhCn = "架构模式", EnUs = "Architecture Pattern", JaJp = "アーキテクチャパターン" },
            new { Key = "about.architecture.value", Module = "Frontend", ZhCn = "MVVM（Model-View-ViewModel）", EnUs = "MVVM (Model-View-ViewModel)", JaJp = "MVVM（Model-View-ViewModel）" },
            new { Key = "about.buildDate", Module = "Frontend", ZhCn = "构建时间", EnUs = "Build Time", JaJp = "ビルド日時" },
            new { Key = "about.edition", Module = "Frontend", ZhCn = "社区版 (64 位) - Current", EnUs = "Community (64-bit) - Current", JaJp = "コミュニティ版 (64 ビット) - Current" },
            new { Key = "about.version.format", Module = "Frontend", ZhCn = "版本 {0}", EnUs = "Version {0}", JaJp = "バージョン {0}" },
            new { Key = "about.dotnetVersion.format", Module = "Frontend", ZhCn = ".NET {0}", EnUs = ".NET {0}", JaJp = ".NET {0}" },
            new { Key = "about.links.licenseStatus", Module = "Frontend", ZhCn = "许可状态", EnUs = "License Status", JaJp = "ライセンス状態" },
            new { Key = "about.links.licenseTerms", Module = "Frontend", ZhCn = "许可证条款", EnUs = "License Terms", JaJp = "ライセンス条項" },
            new { Key = "about.links.licenseStatus.message", Module = "Frontend", ZhCn = "许可状态功能尚未实现。", EnUs = "License status is not implemented yet.", JaJp = "ライセンス状態はまだ実装されていません。" },
            new { Key = "about.links.licenseTerms.message", Module = "Frontend", ZhCn = "许可证条款查看功能尚未实现。", EnUs = "Viewing license terms is not implemented yet.", JaJp = "ライセンス条項の表示はまだ実装されていません。" },
            new { Key = "about.section.productInfo", Module = "Frontend", ZhCn = "产品信息", EnUs = "Product Information", JaJp = "製品情報" },
            new { Key = "about.section.environmentInfo", Module = "Frontend", ZhCn = "环境信息", EnUs = "Environment Information", JaJp = "環境情報" },
            new { Key = "about.label.productName", Module = "Frontend", ZhCn = "产品名称", EnUs = "Product Name", JaJp = "製品名" },
            new { Key = "about.label.edition", Module = "Frontend", ZhCn = "版本类型", EnUs = "Edition", JaJp = "エディション" },
            new { Key = "about.label.version", Module = "Frontend", ZhCn = "版本号", EnUs = "Version", JaJp = "バージョン" },
            new { Key = "about.label.company", Module = "Frontend", ZhCn = "公司", EnUs = "Company", JaJp = "会社" },
            new { Key = "about.label.installationPath", Module = "Frontend", ZhCn = "安装位置", EnUs = "Installation Path", JaJp = "インストール場所" },
            new { Key = "about.label.dotnetVersion", Module = "Frontend", ZhCn = ".NET 版本", EnUs = ".NET Version", JaJp = ".NET バージョン" },
            new { Key = "about.label.osVersion", Module = "Frontend", ZhCn = "操作系统", EnUs = "Operating System", JaJp = "オペレーティングシステム" },
            new { Key = "about.label.architecture", Module = "Frontend", ZhCn = "体系结构", EnUs = "Architecture", JaJp = "アーキテクチャ" },
            new { Key = "about.label.processorCount", Module = "Frontend", ZhCn = "处理器数量", EnUs = "Processor Count", JaJp = "プロセッサ数" },
            new { Key = "about.label.runtimeIdentifier", Module = "Frontend", ZhCn = "运行时标识", EnUs = "Runtime Identifier", JaJp = "ランタイム識別子" },
            new { Key = "about.installedProducts.title", Module = "Frontend", ZhCn = "已安装的组件", EnUs = "Installed Products", JaJp = "インストール済みコンポーネント" },
            new { Key = "about.installedProducts.subtitle", Module = "Frontend", ZhCn = "以下组件已经安装并可供使用。", EnUs = "The following components are installed and ready to use.", JaJp = "次のコンポーネントがインストールされ、使用できます。" },
            new { Key = "about.installed.item1", Module = "Frontend", ZhCn = ".NET 9.0 SDK 与 Windows 桌面运行时", EnUs = ".NET 9.0 SDK & Windows Desktop Runtime", JaJp = ".NET 9.0 SDK と Windows デスクトップ ランタイム" },
            new { Key = "about.installed.item2", Module = "Frontend", ZhCn = "MaterialDesignThemes UI 库", EnUs = "MaterialDesignThemes UI Library", JaJp = "MaterialDesignThemes UI ライブラリ" },
            new { Key = "about.installed.item3", Module = "Frontend", ZhCn = "CommunityToolkit.Mvvm", EnUs = "CommunityToolkit.Mvvm", JaJp = "CommunityToolkit.Mvvm" },
            new { Key = "about.installed.item4", Module = "Frontend", ZhCn = "SqlSugar ORM", EnUs = "SqlSugar ORM", JaJp = "SqlSugar ORM" },
            new { Key = "about.installed.item5", Module = "Frontend", ZhCn = "Autofac 依赖注入容器", EnUs = "Autofac Dependency Injection", JaJp = "Autofac 依存性注入" },
            new { Key = "about.installed.item6", Module = "Frontend", ZhCn = "Serilog 日志框架", EnUs = "Serilog Logging Framework", JaJp = "Serilog ログフレームワーク" },
            new { Key = "about.installed.item7", Module = "Frontend", ZhCn = "FontAwesome.Sharp 图标库", EnUs = "FontAwesome.Sharp Icon Library", JaJp = "FontAwesome.Sharp アイコンライブラリ" },
            new { Key = "about.installed.item8", Module = "Frontend", ZhCn = "Microsoft.Data.SqlClient 数据驱动程序", EnUs = "Microsoft.Data.SqlClient Driver", JaJp = "Microsoft.Data.SqlClient ドライバー" },
            new { Key = "about.dialog.title", Module = "Frontend", ZhCn = "关于", EnUs = "About", JaJp = "バージョン情報" },
            new { Key = "about.button.systemInfo", Module = "Frontend", ZhCn = "系统信息", EnUs = "System Information", JaJp = "システム情報" },
            new { Key = "about.systemInfo.title", Module = "Frontend", ZhCn = "系统信息", EnUs = "System Information", JaJp = "システム情報" },
            new { Key = "about.systemInfo.copySuccess", Module = "Frontend", ZhCn = "系统信息已复制到剪贴板", EnUs = "System information copied to clipboard", JaJp = "システム情報がクリップボードにコピーされました" },
            new { Key = "about.systemInfo.copyFailed", Module = "Frontend", ZhCn = "复制失败", EnUs = "Copy failed", JaJp = "コピーに失敗しました" },
            new { Key = "about.systemInfo.tab.summary", Module = "Frontend", ZhCn = "系统摘要", EnUs = "System Summary", JaJp = "システム概要" },
            new { Key = "about.systemInfo.tab.hardware", Module = "Frontend", ZhCn = "硬件信息", EnUs = "Hardware Information", JaJp = "ハードウェア情報" },
            new { Key = "about.systemInfo.tab.software", Module = "Frontend", ZhCn = "软件信息", EnUs = "Software Information", JaJp = "ソフトウェア情報" },
            new { Key = "about.systemInfo.tab.network", Module = "Frontend", ZhCn = "网络信息", EnUs = "Network Information", JaJp = "ネットワーク情報" },
            new { Key = "about.systemInfo.column.item", Module = "Frontend", ZhCn = "项目", EnUs = "Item", JaJp = "項目" },
            new { Key = "about.systemInfo.column.value", Module = "Frontend", ZhCn = "值", EnUs = "Value", JaJp = "値" },
            new { Key = "about.systemInfo.network.adapters", Module = "Frontend", ZhCn = "网络适配器", EnUs = "Network Adapters", JaJp = "ネットワークアダプター" },
            new { Key = "about.systemInfo.network.name", Module = "Frontend", ZhCn = "名称", EnUs = "Name", JaJp = "名前" },
            new { Key = "about.systemInfo.network.mac", Module = "Frontend", ZhCn = "MAC地址", EnUs = "MAC Address", JaJp = "MACアドレス" },
            new { Key = "about.systemInfo.network.ip", Module = "Frontend", ZhCn = "IP地址", EnUs = "IP Address", JaJp = "IPアドレス" },
            new { Key = "about.systemInfo.network.speed", Module = "Frontend", ZhCn = "速度", EnUs = "Speed", JaJp = "速度" },
            new { Key = "about.systemInfo.network.status", Module = "Frontend", ZhCn = "状态", EnUs = "Status", JaJp = "状態" },
            new { Key = "about.systemInfo.network.active", Module = "Frontend", ZhCn = "活动", EnUs = "Active", JaJp = "アクティブ" },
            new { Key = "about.systemInfo.network.inactive", Module = "Frontend", ZhCn = "非活动", EnUs = "Inactive", JaJp = "非アクティブ" },
            new { Key = "about.systemInfo.disk.name", Module = "Frontend", ZhCn = "磁盘", EnUs = "Disk", JaJp = "ディスク" },
            new { Key = "about.systemInfo.software.name", Module = "Frontend", ZhCn = "软件名称", EnUs = "Software Name", JaJp = "ソフトウェア名" },
            new { Key = "about.systemInfo.software.version", Module = "Frontend", ZhCn = "版本", EnUs = "Version", JaJp = "バージョン" },
            new { Key = "about.systemInfo.software.publisher", Module = "Frontend", ZhCn = "发布者", EnUs = "Publisher", JaJp = "発行者" },
            new { Key = "about.systemInfo.user.name", Module = "Frontend", ZhCn = "用户名", EnUs = "User Name", JaJp = "ユーザー名" },
            new { Key = "about.systemInfo.user.fullName", Module = "Frontend", ZhCn = "全名", EnUs = "Full Name", JaJp = "フルネーム" },
            new { Key = "about.systemInfo.user.isAdmin", Module = "Frontend", ZhCn = "管理员", EnUs = "Administrator", JaJp = "管理者" },
            new { Key = "about.systemInfo.user.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "about.systemInfo.user.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            new { Key = "about.systemInfo.summary.systemLanguage", Module = "Frontend", ZhCn = "系统语言", EnUs = "System Language", JaJp = "システム言語" },
            new { Key = "about.systemInfo.hardware.diskInfo", Module = "Frontend", ZhCn = "磁盘信息", EnUs = "Disk Information", JaJp = "ディスク情報" },
            new { Key = "about.systemInfo.copy.title", Module = "Frontend", ZhCn = "========== 系统信息 ==========", EnUs = "========== System Information ==========", JaJp = "========== システム情報 ==========" },
            new { Key = "about.systemInfo.copy.section.summary", Module = "Frontend", ZhCn = "【系统摘要】", EnUs = "【System Summary】", JaJp = "【システム概要】" },
            new { Key = "about.systemInfo.copy.section.hardware", Module = "Frontend", ZhCn = "【硬件信息】", EnUs = "【Hardware Information】", JaJp = "【ハードウェア情報】" },
            new { Key = "about.systemInfo.copy.section.software", Module = "Frontend", ZhCn = "【软件信息】", EnUs = "【Software Information】", JaJp = "【ソフトウェア情報】" },
            new { Key = "about.systemInfo.copy.section.network", Module = "Frontend", ZhCn = "【网络信息】", EnUs = "【Network Information】", JaJp = "【ネットワーク情報】" },
            new { Key = "about.systemInfo.key.os", Module = "Frontend", ZhCn = "操作系统", EnUs = "Operating System", JaJp = "オペレーティングシステム" },
            new { Key = "about.systemInfo.key.osVersion", Module = "Frontend", ZhCn = "系统版本", EnUs = "OS Version", JaJp = "OSバージョン" },
            new { Key = "about.systemInfo.key.osType", Module = "Frontend", ZhCn = "系统类型", EnUs = "OS Type", JaJp = "OSタイプ" },
            new { Key = "about.systemInfo.key.osArchitecture", Module = "Frontend", ZhCn = "系统架构", EnUs = "OS Architecture", JaJp = "OSアーキテクチャ" },
            new { Key = "about.systemInfo.key.machineName", Module = "Frontend", ZhCn = "机器名称", EnUs = "Machine Name", JaJp = "マシン名" },
            new { Key = "about.systemInfo.key.userName", Module = "Frontend", ZhCn = "用户名", EnUs = "User Name", JaJp = "ユーザー名" },
            new { Key = "about.systemInfo.key.isAdmin", Module = "Frontend", ZhCn = "是否管理员", EnUs = "Is Administrator", JaJp = "管理者か" },
            new { Key = "about.systemInfo.key.runtime", Module = "Frontend", ZhCn = "运行时", EnUs = "Runtime", JaJp = "ランタイム" },
            new { Key = "about.systemInfo.key.processArchitecture", Module = "Frontend", ZhCn = "进程架构", EnUs = "Process Architecture", JaJp = "プロセスアーキテクチャ" },
            new { Key = "about.systemInfo.key.systemUptime", Module = "Frontend", ZhCn = "系统运行时间", EnUs = "System Uptime", JaJp = "システム稼働時間" },
            new { Key = "about.systemInfo.key.cpu", Module = "Frontend", ZhCn = "CPU", EnUs = "CPU", JaJp = "CPU" },
            new { Key = "about.systemInfo.key.cpuName", Module = "Frontend", ZhCn = "CPU名称", EnUs = "CPU Name", JaJp = "CPU名" },
            new { Key = "about.systemInfo.key.cpuCores", Module = "Frontend", ZhCn = "CPU核心", EnUs = "CPU Cores", JaJp = "CPUコア" },
            new { Key = "about.systemInfo.key.totalMemory", Module = "Frontend", ZhCn = "物理内存", EnUs = "Total Memory", JaJp = "物理メモリ" },
            new { Key = "about.systemInfo.key.availableMemory", Module = "Frontend", ZhCn = "可用内存", EnUs = "Available Memory", JaJp = "利用可能メモリ" },
            new { Key = "about.systemInfo.key.memoryUsage", Module = "Frontend", ZhCn = "内存使用率", EnUs = "Memory Usage", JaJp = "メモリ使用率" },
            new { Key = "about.systemInfo.key.processMemory", Module = "Frontend", ZhCn = "进程内存", EnUs = "Process Memory", JaJp = "プロセスメモリ" },
            new { Key = "about.systemInfo.key.ipAddress", Module = "Frontend", ZhCn = "IP地址", EnUs = "IP Address", JaJp = "IPアドレス" },
            new { Key = "about.systemInfo.key.macAddress", Module = "Frontend", ZhCn = "MAC地址", EnUs = "MAC Address", JaJp = "MACアドレス" },
            new { Key = "about.disclaimer", Module = "Frontend", ZhCn = "警告: 本计算机程序受著作权法以及国际版权公约保护。未经授权而擅自复制或传播本程序（或其中任何部分），将受到严厉的民事及刑事处罚，并将在法律许可的最大限度内受到追诉。", EnUs = "Warning: This computer program is protected by copyright law and international copyright conventions. Unauthorized copying or distribution of this program (or any part thereof) will be subject to severe civil and criminal penalties, and will be prosecuted to the fullest extent permitted by law.", JaJp = "警告: 本コンピュータプログラムは著作権法および国際著作権条約によって保護されています。許可なく本プログラム（またはその一部）を複製または配布することは、厳しい民事および刑事罰の対象となり、法律で許可される最大限の範囲で起訴されます。" },

            // 用户管理相关
            new { Key = "Identity.User.Keyword", Module = "Frontend", ZhCn = "用户", EnUs = "User", JaJp = "ユーザー" },
            new { Key = "common.button.assignrole", Module = "Frontend", ZhCn = "分配角色", EnUs = "Assign Role", JaJp = "ロールを割り当て" },
            new { Key = "common.button.assigndept", Module = "Frontend", ZhCn = "分配部门", EnUs = "Assign Department", JaJp = "部門を割り当て" },
            new { Key = "common.button.assignmenu", Module = "Frontend", ZhCn = "分配菜单", EnUs = "Assign Menu", JaJp = "メニューを割り当て" },
            new { Key = "common.button.assignpost", Module = "Frontend", ZhCn = "分配岗位", EnUs = "Assign Position", JaJp = "ポジションを割り当て" },
            new { Key = "Identity.User.LoadRolesFailed", Module = "Frontend", ZhCn = "加载角色列表失败", EnUs = "Failed to load role list", JaJp = "ロールリストの読み込みに失敗しました" },
            new { Key = "Identity.User.LoadUserRolesFailed", Module = "Frontend", ZhCn = "加载用户角色失败", EnUs = "Failed to load user roles", JaJp = "ユーザーロールの読み込みに失敗しました" },
            new { Key = "Identity.User.AssignRoleFailed", Module = "Frontend", ZhCn = "分配角色失败", EnUs = "Failed to assign roles", JaJp = "ロールの割り当てに失敗しました" },
            new { Key = "Identity.User.AssignRoleSuccess", Module = "Frontend", ZhCn = "分配角色成功", EnUs = "Roles assigned successfully", JaJp = "ロールが正常に割り当てられました" },
            
            // 用户验证相关（登录表单验证 - 仅保留特定业务规则）
            new { Key = "Identity.User.Validation.UsernameNotFound", Module = "Frontend", ZhCn = "该用户名不存在", EnUs = "Username does not exist", JaJp = "このユーザー名は存在しません" },
            new { Key = "Identity.User.Validation.PasswordIncorrect", Module = "Frontend", ZhCn = "密码不正确", EnUs = "Password is incorrect", JaJp = "パスワードが正しくありません" },
            new { Key = "Identity.User.Transfer.Unassigned", Module = "Frontend", ZhCn = "未分配角色", EnUs = "Unassigned Roles", JaJp = "未割り当てロール" },
            new { Key = "Identity.User.Transfer.Assigned", Module = "Frontend", ZhCn = "已分配角色", EnUs = "Assigned Roles", JaJp = "割り当て済みロール" },
            new { Key = "Identity.User.Transfer.MoveRight", Module = "Frontend", ZhCn = "添加到已分配", EnUs = "Add to Assigned", JaJp = "割り当て済みに追加" },
            new { Key = "Identity.User.Transfer.MoveAllRight", Module = "Frontend", ZhCn = "全部添加到已分配", EnUs = "Add All to Assigned", JaJp = "すべてを割り当て済みに追加" },
            new { Key = "Identity.User.Transfer.MoveLeft", Module = "Frontend", ZhCn = "从已分配移除", EnUs = "Remove from Assigned", JaJp = "割り当て済みから削除" },
            new { Key = "Identity.User.Transfer.MoveAllLeft", Module = "Frontend", ZhCn = "全部从已分配移除", EnUs = "Remove All from Assigned", JaJp = "すべてを割り当て済みから削除" },

            // 通用操作结果提示（成功类）
            new { Key = "common.success.create", Module = "Frontend", ZhCn = "{0}创建成功", EnUs = "{0} created successfully", JaJp = "{0} が作成されました" },
            new { Key = "common.success.update", Module = "Frontend", ZhCn = "{0}更新成功", EnUs = "{0} updated successfully", JaJp = "{0} が更新されました" },
            new { Key = "common.success.delete", Module = "Frontend", ZhCn = "{0}删除成功", EnUs = "{0} deleted successfully", JaJp = "{0} が削除されました" },
            new { Key = "common.success.import", Module = "Frontend", ZhCn = "{0}导入成功", EnUs = "{0} imported successfully", JaJp = "{0} のインポートに成功しました" },
            new { Key = "common.success.export", Module = "Frontend", ZhCn = "{0}导出成功", EnUs = "{0} exported successfully", JaJp = "{0} のエクスポートに成功しました" },
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
            new { Key = "validation.maxLength", Module = "Frontend", ZhCn = "{0}长度不能超过{1}个字符", EnUs = "{0} cannot exceed {1} characters", JaJp = "{0} は{1}文字を超えることはできません" },
            new { Key = "validation.minLength", Module = "Frontend", ZhCn = "{0}长度不能少于{1}位", EnUs = "{0} must be at least {1} characters", JaJp = "{0} は{1}文字以上である必要があります" },
            new { Key = "validation.invalid", Module = "Frontend", ZhCn = "{0}无效", EnUs = "{0} is invalid", JaJp = "{0} が無効です" },
            
            // 通用操作结果（失败类）
            new { Key = "common.failed.create", Module = "Frontend", ZhCn = "{0}创建失败", EnUs = "Failed to create {0}", JaJp = "{0} の作成に失敗しました" },
            new { Key = "common.failed.update", Module = "Frontend", ZhCn = "{0}更新失败", EnUs = "Failed to update {0}", JaJp = "{0} の更新に失敗しました" },
            new { Key = "common.failed.delete", Module = "Frontend", ZhCn = "{0}删除失败", EnUs = "Failed to delete {0}", JaJp = "{0} の削除に失敗しました" },
            new { Key = "common.failed.import", Module = "Frontend", ZhCn = "{0}导入失败", EnUs = "Failed to import {0}", JaJp = "{0} のインポートに失敗しました" },
            new { Key = "common.failed.export", Module = "Frontend", ZhCn = "{0}导出失败", EnUs = "Failed to export {0}", JaJp = "{0} のエクスポートに失敗しました" },
            new { Key = "common.saveFailed", Module = "Frontend", ZhCn = "保存失败", EnUs = "Save failed", JaJp = "保存に失敗しました" },
            
            // 消息框相关
            new { Key = "common.messageBox.information", Module = "Frontend", ZhCn = "信息", EnUs = "Information", JaJp = "情報" },
            new { Key = "common.messageBox.warning", Module = "Frontend", ZhCn = "警告", EnUs = "Warning", JaJp = "警告" },
            new { Key = "common.messageBox.error", Module = "Frontend", ZhCn = "错误", EnUs = "Error", JaJp = "エラー" },
            new { Key = "common.messageBox.question", Module = "Frontend", ZhCn = "确认", EnUs = "Question", JaJp = "確認" },

            // 通用占位（参数化占位符）
            new { Key = "common.noData", Module = "Frontend", ZhCn = "暂无数据", EnUs = "No Data", JaJp = "データなし" },
            new { Key = "common.placeholder.input", Module = "Frontend", ZhCn = "请输入{0}", EnUs = "Please enter {0}", JaJp = "{0} を入力してください" },
            new { Key = "common.placeholder.select", Module = "Frontend", ZhCn = "请选择{0}", EnUs = "Please select {0}", JaJp = "{0} を選択してください" },
            new { Key = "common.placeholder.search", Module = "Frontend", ZhCn = "请输入{0}进行搜索", EnUs = "Please enter {0} to search", JaJp = "検索するには {0} を入力してください" },
            new { Key = "common.placeholder.range", Module = "Frontend", ZhCn = "请选择{0}范围", EnUs = "Select {0} range", JaJp = "{0} の範囲を選択" },
            new { Key = "common.placeholder.keywordHint", Module = "Frontend", ZhCn = "请输入{0}等关键字查询", EnUs = "Enter {0} keywords", JaJp = "{0}などのキーワードを入力" },
            new { Key = "common.selectionHeaderHint", Module = "Frontend", ZhCn = "全选/取消全选", EnUs = "Select/Deselect all", JaJp = "全選択/全解除" },
            new { Key = "common.selectionRowHint", Module = "Frontend", ZhCn = "选择/取消选择该行", EnUs = "Select/Deselect this row", JaJp = "この行を選択/解除" },
            new { Key = "common.goTo", Module = "Frontend", ZhCn = "前往", EnUs = "Go to", JaJp = "移動" },

            // 通用操作
            new { Key = "common.selection", Module = "Frontend", ZhCn = "选择", EnUs = "Selection", JaJp = "選択" },
            new { Key = "common.operation", Module = "Frontend", ZhCn = "操作", EnUs = "Operation", JaJp = "操作" },
            new { Key = "common.search", Module = "Frontend", ZhCn = "搜索", EnUs = "Search", JaJp = "検索" },
            new { Key = "common.reset", Module = "Frontend", ZhCn = "重置", EnUs = "Reset", JaJp = "リセット" },
            new { Key = "common.advancedQuery", Module = "Frontend", ZhCn = "高级查询", EnUs = "Advanced Query", JaJp = "高度検索" },
            new { Key = "common.toggleColumns", Module = "Frontend", ZhCn = "显隐列", EnUs = "Toggle Columns", JaJp = "列の表示/非表示" },
            new { Key = "common.toggleQueryBar", Module = "Frontend", ZhCn = "显隐查询栏", EnUs = "Toggle Query Bar", JaJp = "検索バーの表示/非表示" },
            new { Key = "common.expandAll", Module = "Frontend", ZhCn = "展开全部", EnUs = "Expand All", JaJp = "すべて展開" },
            new { Key = "common.collapseAll", Module = "Frontend", ZhCn = "折叠全部", EnUs = "Collapse All", JaJp = "すべて折りたたむ" },
            new { Key = "common.firstPage", Module = "Frontend", ZhCn = "首页", EnUs = "First Page", JaJp = "最初のページ" },
            new { Key = "common.prevPage", Module = "Frontend", ZhCn = "上一页", EnUs = "Previous Page", JaJp = "前のページ" },
            new { Key = "common.nextPage", Module = "Frontend", ZhCn = "下一页", EnUs = "Next Page", JaJp = "次のページ" },
            new { Key = "common.lastPage", Module = "Frontend", ZhCn = "末页", EnUs = "Last Page", JaJp = "最後のページ" },
            new { Key = "common.total", Module = "Frontend", ZhCn = "共 {0} 条记录", EnUs = "Total {0} records", JaJp = "合計 {0} 件" },
            new { Key = "common.pageDisplay", Module = "Frontend", ZhCn = "第 {0} / {1} 页", EnUs = "Page {0} / {1}", JaJp = "{0} / {1} ページ" },
            new { Key = "common.pageSizeHint", Module = "Frontend", ZhCn = "每页", EnUs = "Per Page", JaJp = "1ページあたり" },
            new { Key = "common.pageInputHint", Module = "Frontend", ZhCn = "页码", EnUs = "Page", JaJp = "ページ" },
            
            // 通用按钮（统一键）
            new { Key = "common.button.close", Module = "Frontend", ZhCn = "关闭", EnUs = "Close", JaJp = "閉じる" },
            new { Key = "common.button.changeTheme", Module = "Frontend", ZhCn = "切换主题", EnUs = "Toggle Theme", JaJp = "テーマを切り替え" },
            new { Key = "common.button.changeLanguage", Module = "Frontend", ZhCn = "切换语言", EnUs = "Toggle Language", JaJp = "言語を切り替え" },
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
            new { Key = "common.enabled", Module = "Frontend", ZhCn = "启用", EnUs = "Enabled", JaJp = "有効" },
            new { Key = "common.disabled", Module = "Frontend", ZhCn = "禁用", EnUs = "Disabled", JaJp = "無効" },
            new { Key = "common.button.lock", Module = "Frontend", ZhCn = "锁定", EnUs = "Lock", JaJp = "ロック" },
            new { Key = "common.button.unlock", Module = "Frontend", ZhCn = "解锁", EnUs = "Unlock", JaJp = "アンロック" },
            new { Key = "common.button.authorize", Module = "Frontend", ZhCn = "授权", EnUs = "Authorize", JaJp = "権限付与" },
            new { Key = "common.button.grant", Module = "Frontend", ZhCn = "授予", EnUs = "Grant", JaJp = "付与" },
            new { Key = "common.button.revoke", Module = "Frontend", ZhCn = "收回", EnUs = "Revoke", JaJp = "剥奪" },
            new { Key = "common.button.run", Module = "Frontend", ZhCn = "运行", EnUs = "Run", JaJp = "実行" },
            new { Key = "common.button.generate", Module = "Frontend", ZhCn = "生成", EnUs = "Generate", JaJp = "生成" },
            new { Key = "common.button.start", Module = "Frontend", ZhCn = "启动", EnUs = "Start", JaJp = "開始" },
            new { Key = "common.button.stop", Module = "Frontend", ZhCn = "停止", EnUs = "Stop", JaJp = "停止" },
            new { Key = "common.button.pause", Module = "Frontend", ZhCn = "暂停", EnUs = "Pause", JaJp = "一時停止" },
            new { Key = "common.button.resume", Module = "Frontend", ZhCn = "恢复", EnUs = "Resume", JaJp = "再開" },
            new { Key = "common.button.restart", Module = "Frontend", ZhCn = "重启", EnUs = "Restart", JaJp = "再起動" },
            new { Key = "common.button.submit", Module = "Frontend", ZhCn = "提交", EnUs = "Submit", JaJp = "提出" },
            new { Key = "common.button.ok", Module = "Frontend", ZhCn = "确定", EnUs = "OK", JaJp = "OK" },
            new { Key = "common.button.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "common.button.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            new { Key = "common.button.cancel", Module = "Frontend", ZhCn = "取消", EnUs = "Cancel", JaJp = "キャンセル" },
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
            new { Key = "common.button.sync", Module = "Frontend", ZhCn = "同步", EnUs = "Sync", JaJp = "同期" },
            new { Key = "common.button.copy", Module = "Frontend", ZhCn = "复制", EnUs = "Copy", JaJp = "コピー" },
            new { Key = "common.button.clone", Module = "Frontend", ZhCn = "克隆", EnUs = "Clone", JaJp = "クローン" },
            new { Key = "common.button.refresh", Module = "Frontend", ZhCn = "刷新", EnUs = "Refresh", JaJp = "リフレッシュ" },
            new { Key = "common.button.archive", Module = "Frontend", ZhCn = "归档", EnUs = "Archive", JaJp = "アーカイブ" },
            new { Key = "common.button.restore", Module = "Frontend", ZhCn = "还原", EnUs = "Restore", JaJp = "復元" },
            new { Key = "common.button.apply", Module = "Frontend", ZhCn = "应用", EnUs = "Apply", JaJp = "適用" },
            

            // 应用标题
            new { Key = "application.title", Module = "Frontend", ZhCn = "Takt SMEs", EnUs = "Takt SMEs", JaJp = "Takt SMEs" },
            
            // 设置页面翻译
            new { Key = "Settings.Customize.Title", Module = "Frontend", ZhCn = "用户设置", EnUs = "User Settings", JaJp = "ユーザー設定" },
            new { Key = "Settings.Customize.Description", Module = "Frontend", ZhCn = "自定义应用程序的外观和行为", EnUs = "Customize the appearance and behavior of the application", JaJp = "アプリケーションの外観と動作をカスタマイズ" },
            new { Key = "Settings.Customize.AppearanceAndBehavior", Module = "Frontend", ZhCn = "外观与行为", EnUs = "Appearance & Behavior", JaJp = "外観と動作" },
            new { Key = "Settings.Customize.ThemeMode", Module = "Frontend", ZhCn = "主题模式", EnUs = "Theme Mode", JaJp = "テーマモード" },
            new { Key = "Settings.Customize.ThemeMode.Description", Module = "Frontend", ZhCn = "选择应用程序的主题模式", EnUs = "Select the theme mode for the application", JaJp = "アプリケーションのテーマモードを選択" },
            new { Key = "Settings.Customize.ThemeMode.System", Module = "Frontend", ZhCn = "跟随系统", EnUs = "Follow System", JaJp = "システムに従う" },
            new { Key = "Settings.Customize.ThemeMode.Light", Module = "Frontend", ZhCn = "浅色", EnUs = "Light", JaJp = "ライト" },
            new { Key = "Settings.Customize.ThemeMode.Dark", Module = "Frontend", ZhCn = "深色", EnUs = "Dark", JaJp = "ダーク" },
            new { Key = "Settings.Customize.Language", Module = "Frontend", ZhCn = "语言", EnUs = "Language", JaJp = "言語" },
            new { Key = "Settings.Customize.Language.Description", Module = "Frontend", ZhCn = "选择应用程序的显示语言", EnUs = "Select the display language for the application", JaJp = "アプリケーションの表示言語を選択" },
            new { Key = "Settings.Customize.FontSettings", Module = "Frontend", ZhCn = "字体设置", EnUs = "Font Settings", JaJp = "フォント設定" },
            new { Key = "Settings.Customize.FontFamily", Module = "Frontend", ZhCn = "字体族", EnUs = "Font Family", JaJp = "フォントファミリー" },
            new { Key = "Settings.Customize.FontFamily.Description", Module = "Frontend", ZhCn = "选择应用程序的字体族", EnUs = "Select the font family for the application", JaJp = "アプリケーションのフォントファミリーを選択" },
            new { Key = "Settings.Customize.FontPreview", Module = "Frontend", ZhCn = "字体预览：", EnUs = "Font Preview: ", JaJp = "フォントプレビュー：" },
            new { Key = "Settings.Customize.FontSize", Module = "Frontend", ZhCn = "字体大小", EnUs = "Font Size", JaJp = "フォントサイズ" },
            new { Key = "Settings.Customize.FontSize.Description", Module = "Frontend", ZhCn = "选择应用程序的字体大小", EnUs = "Select the font size for the application", JaJp = "アプリケーションのフォントサイズを選択" },
            new { Key = "Settings.Customize.FontSizePreview", Module = "Frontend", ZhCn = "字体大小预览", EnUs = "Font Size Preview", JaJp = "フォントサイズプレビュー" },
            new { Key = "Settings.Customize.FontSizePreview.Sample", Module = "Frontend", ZhCn = "这是字体大小预览文本", EnUs = "This is a font size preview text", JaJp = "これはフォントサイズのプレビューテキストです" },
            new { Key = "Settings.Customize.LoadFailed", Module = "Frontend", ZhCn = "加载设置失败：{0}", EnUs = "Failed to load settings: {0}", JaJp = "設定の読み込みに失敗しました：{0}" },
            new { Key = "Settings.Customize.LanguageNotSelected", Module = "Frontend", ZhCn = "本地化管理器未初始化或未选择语言", EnUs = "Localization manager not initialized or language not selected", JaJp = "ローカライゼーションマネージャーが初期化されていないか、言語が選択されていません" },
            new { Key = "Settings.Customize.LanguageChanged", Module = "Frontend", ZhCn = "语言已切换为 {0}", EnUs = "Language changed to {0}", JaJp = "言語が {0} に変更されました" },
            new { Key = "Settings.Customize.LanguageChangeFailed", Module = "Frontend", ZhCn = "切换语言失败：{0}", EnUs = "Failed to change language: {0}", JaJp = "言語の変更に失敗しました：{0}" },
            
            // 主窗口菜单项
            new { Key = "MainWindow.UserInfoCenter", Module = "Frontend", ZhCn = "用户信息中心", EnUs = "User Info Center", JaJp = "ユーザー情報センター" },
            new { Key = "MainWindow.Logout", Module = "Frontend", ZhCn = "登出", EnUs = "Logout", JaJp = "ログアウト" },
            
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
            new { Key = "menu.generator", Module = "Frontend", ZhCn = "代码管理", EnUs = "Code Management", JaJp = "コード管理" },
            new { Key = "menu.generator.code", Module = "Frontend", ZhCn = "代码生成", EnUs = "Code Generator", JaJp = "コード生成" },
            new { Key = "menu.settings", Module = "Frontend", ZhCn = "设置", EnUs = "Settings", JaJp = "設定" },

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

