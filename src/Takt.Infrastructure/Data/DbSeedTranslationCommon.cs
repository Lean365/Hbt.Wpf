//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : DbSeedTranslationCommon.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 通用翻译种子数据初始化服务
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
/// 通用翻译种子数据初始化服务
/// 创建通用翻译种子数据（登录、通用、仪表盘、关于、设置、菜单等）
/// </summary>
public class DbSeedTranslationCommon
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly IBaseRepository<Translation> _translationRepository;

    public DbSeedTranslationCommon(
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
        _initLog.Information("开始初始化通用翻译种子数据...");

        // 从仓库获取语言数据
        var languages = _languageRepository.AsQueryable().ToList();
        if (languages.Count == 0)
        {
            _initLog.Warning("语言数据为空，跳过通用翻译数据初始化");
            return;
        }

        InitializeTranslations(languages);

        _initLog.Information("✅ 通用翻译种子数据初始化完成");
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
            new { Key = "login.welcome", Module = "Frontend", ZhCn = "欢迎登录", EnUs = "Welcome", JaJp = "ようこそ" },
            new { Key = "login.username", Module = "Frontend", ZhCn = "用户名", EnUs = "Username", JaJp = "ユーザー名" },
            new { Key = "login.password", Module = "Frontend", ZhCn = "密码", EnUs = "Password", JaJp = "パスワード" },
            new { Key = "login.rememberme", Module = "Frontend", ZhCn = "记住密码", EnUs = "Remember Me", JaJp = "パスワードを記憶する" },
            new { Key = "login.forgot", Module = "Frontend", ZhCn = "忘记密码？", EnUs = "Forgot password?", JaJp = "パスワードをお忘れの方" },
            new { Key = "login.button", Module = "Frontend", ZhCn = "登录", EnUs = "Login", JaJp = "ログイン" },
            new { Key = "login.loading", Module = "Frontend", ZhCn = "登录中...", EnUs = "Signing in...", JaJp = "ログイン中..." },
            new { Key = "login.error", Module = "Frontend", ZhCn = "登录失败：{0}", EnUs = "Login failed: {0}", JaJp = "ログイン失敗：{0}" },
            new { Key = "login.success", Module = "Frontend", ZhCn = "登录成功", EnUs = "Login successful", JaJp = "ログイン成功" },
            new { Key = "login.failed.default", Module = "Frontend", ZhCn = "登录失败，请检查用户名和密码", EnUs = "Login failed, please check your username and password", JaJp = "ログインに失敗しました。ユーザー名とパスワードを確認してください" },
            new { Key = "login.description", Module = "Frontend", ZhCn = "请输入您的账号信息", EnUs = "Please enter your account information", JaJp = "アカウント情報を入力してください" },
            new { Key = "login.initialization.inprogress", Module = "Frontend", ZhCn = "数据初始化中...", EnUs = "Initializing data...", JaJp = "データ初期化中..." },
            new { Key = "login.initialization.completed", Module = "Frontend", ZhCn = "数据初始化完成，可以登录", EnUs = "Data initialization completed, you can login now", JaJp = "データ初期化が完了しました。ログインできます" },
            new { Key = "login.initialization.database", Module = "Frontend", ZhCn = "正在初始化数据库表...", EnUs = "Initializing database tables...", JaJp = "データベーステーブルを初期化中..." },
            new { Key = "login.initialization.seeddata", Module = "Frontend", ZhCn = "正在初始化种子数据...", EnUs = "Initializing seed data...", JaJp = "シードデータを初期化中..." },
            
            // 审计字段相关
            new { Key = "common.audit.id", Module = "Frontend", ZhCn = "主键", EnUs = "ID", JaJp = "ID" },
            new { Key = "common.audit.remarks", Module = "Frontend", ZhCn = "备注", EnUs = "Remarks", JaJp = "備考" },
            new { Key = "common.audit.createdby", Module = "Frontend", ZhCn = "创建人", EnUs = "Created By", JaJp = "作成者" },
            new { Key = "common.audit.createdtime", Module = "Frontend", ZhCn = "创建时间", EnUs = "Created Time", JaJp = "作成時間" },
            new { Key = "common.audit.updatedby", Module = "Frontend", ZhCn = "更新人", EnUs = "Updated By", JaJp = "更新者" },
            new { Key = "common.audit.updatedtime", Module = "Frontend", ZhCn = "更新时间", EnUs = "Updated Time", JaJp = "更新時間" },
            new { Key = "common.audit.deletedby", Module = "Frontend", ZhCn = "删除人", EnUs = "Deleted By", JaJp = "削除者" },
            new { Key = "common.audit.deletedtime", Module = "Frontend", ZhCn = "删除时间", EnUs = "Deleted Time", JaJp = "削除時間" },
            new { Key = "common.audit.isdeleted", Module = "Frontend", ZhCn = "是否删除", EnUs = "Is Deleted", JaJp = "削除済み" },
            
            // 通用标签页（Tabs）
            new { Key = "common.tabs.basicview", Module = "Frontend", ZhCn = "基本视图", EnUs = "Basic View", JaJp = "基本ビュー" },
            new { Key = "common.tabs.remarkview", Module = "Frontend", ZhCn = "备注视图", EnUs = "Remarks View", JaJp = "備考ビュー" },
            new { Key = "common.tabs.relatedview", Module = "Frontend", ZhCn = "关联视图", EnUs = "Related View", JaJp = "関連ビュー" },
            new { Key = "common.tabs.purchaseview", Module = "Frontend", ZhCn = "采购视图", EnUs = "Purchase View", JaJp = "購買ビュー" },
            new { Key = "common.tabs.inventoryview", Module = "Frontend", ZhCn = "库存视图", EnUs = "Inventory View", JaJp = "在庫ビュー" },
            new { Key = "common.tabs.salesview", Module = "Frontend", ZhCn = "销售视图", EnUs = "Sales View", JaJp = "販売ビュー" },
            new { Key = "common.tabs.financeview", Module = "Frontend", ZhCn = "财务视图", EnUs = "Finance View", JaJp = "財務ビュー" },
            new { Key = "common.tabs.productionview", Module = "Frontend", ZhCn = "生产视图", EnUs = "Production View", JaJp = "生産ビュー" },
            new { Key = "common.tabs.columnview", Module = "Frontend", ZhCn = "列视图", EnUs = "Column View", JaJp = "列ビュー" },
            new { Key = "common.tabs.tableview", Module = "Frontend", ZhCn = "表视图", EnUs = "Table View", JaJp = "表ビュー" },
            new { Key = "common.tabs.generationview", Module = "Frontend", ZhCn = "生成视图", EnUs = "Generation View", JaJp = "生成ビュー" },
            new { Key = "common.tabs.permissionview", Module = "Frontend", ZhCn = "权限视图", EnUs = "Permission View", JaJp = "権限ビュー" },
            // 通用应用信息
            new { Key = "common.company.name", Module = "Frontend", ZhCn = "节拍Takt", EnUs = "Takt", JaJp = "タクトTakt" },
            new { Key = "common.company.slogan", Module = "Frontend", ZhCn = "创新 • 智能 • 未来", EnUs = "Innovation • Intelligence • Future", JaJp = "革新 • 知能 • 未来" },
            new { Key = "common.company.tagline", Module = "Frontend", ZhCn = "启动未来", EnUs = "Ignite the Future", JaJp = "未来ドライブ" },
            new { Key = "common.app.name", Module = "Frontend", ZhCn = "节拍Takt", EnUs = "Takt", JaJp = "タクトTakt" },
            new { Key = "common.app.title", Module = "Frontend", ZhCn = "节拍中小企业平台", EnUs = "Takt SMEs Platform", JaJp = "タクトSMEsプラットフォーム" },
            new { Key = "common.app.slogan", Module = "Frontend", ZhCn = "连接 • 协作 • 共创", EnUs = "Connect • Collaborate• Co-creation", JaJp = "つながる • 協力する • 共に創造する" },
            new { Key = "common.app.tagline", Module = "Frontend", ZhCn = "敏捷 • 灵活 • 实用", EnUs = "Agile • Flexible • Practical", JaJp = "アジャイル • 柔軟 • 実用" },
            new { Key = "common.app.description", Module = "Frontend", ZhCn = "节拍（Takt）中小企业平台是一套面向中小企业的智能化管理系统，提供身份认证、权限控制、日常事务和后勤等一体化能力。", EnUs = "The SMEs Platform is an intelligent management suite for small and medium enterprises, offering integrated identity, authorization, routine and logistics capabilities.", JaJp = "SMEsプラットフォームは、中小企業向けの統合管理システムで、認証、権限管理、日常業務、ロジスティクスなどを一体的に提供します。" },
            new { Key = "common.app.copyright", Module = "Frontend", ZhCn = "Takt All Rights Reserved.节拍信息 保留所有权利.", EnUs = "Takt All Rights Reserved.", JaJp = "タクト情報技術 全著作権所有." },
            new { Key = "common.app.copyrightshort", Module = "Frontend", ZhCn = "节拍", EnUs = "Takt", JaJp = "タクト" },
            
            // 仪表盘欢迎语
            new { Key = "dashboard.greeting.morning", Module = "Frontend", ZhCn = "早上好", EnUs = "Good morning", JaJp = "おはようございます" },
            new { Key = "dashboard.greeting.noon", Module = "Frontend", ZhCn = "中午好", EnUs = "Good noon", JaJp = "こんにちは" },
            new { Key = "dashboard.greeting.afternoon", Module = "Frontend", ZhCn = "下午好", EnUs = "Good afternoon", JaJp = "こんにちは" },
            new { Key = "dashboard.greeting.evening", Module = "Frontend", ZhCn = "晚上好", EnUs = "Good evening", JaJp = "こんばんは" },
            new { Key = "dashboard.greeting.night", Module = "Frontend", ZhCn = "夜深了，请注意休息", EnUs = "It's late, please take a rest", JaJp = "夜も遅いです。ごゆっくりお休みください" },
            new { Key = "dashboard.greeting.welcomeformat", Module = "Frontend", ZhCn = "{0}，欢迎 {1}", EnUs = "{0}, welcome {1}", JaJp = "{0}、ようこそ {1} さん" },
            new { Key = "dashboard.greeting.anonymousname", Module = "Frontend", ZhCn = "随行人员", EnUs = "Guest", JaJp = "ゲスト" },
            new { Key = "dashboard.greeting.fullformat", Module = "Frontend", ZhCn = "{0}，欢迎 {1}，今天是{2}年{3}月{4}日，{5}，（第{6}天，第{7}季，第{8}周）", EnUs = "{0}, welcome {1}. Today is {2}-{3}-{4}, {5}, (Day {6}, Quarter {7}, Week {8})", JaJp = "{0}、{1} さん、ようこそ。本日は{2}年{3}月{4}日、{5}、（{6}日目、第{7}四半期、第{8}週）" },
            new { Key = "dashboard.greeting.line1format", Module = "Frontend", ZhCn = "{0}，欢迎 {1}", EnUs = "{0}, welcome {1}", JaJp = "{0}、ようこそ {1} さん" },
            new { Key = "dashboard.greeting.line2format", Module = "Frontend", ZhCn = "今天是{0}年{1}月{2}日，{3}，（第{4}天，第{5}季，第{6}周）", EnUs = "Today is {0}-{1}-{2}, {3}, (Day {4}, Quarter {5}, Week {6})", JaJp = "本日は{0}年{1}月{2}日、{3}、（{4}日目、第{5}四半期、第{6}週）" },
            
            // 仪表盘卡片统计
            new { Key = "dashboard.card.onlineusers", Module = "Frontend", ZhCn = "在线用户", EnUs = "Online Users", JaJp = "オンラインユーザー" },
            new { Key = "dashboard.card.todayinbound", Module = "Frontend", ZhCn = "今日入库", EnUs = "Today Inbound", JaJp = "本日入庫" },
            new { Key = "dashboard.card.todayoutbound", Module = "Frontend", ZhCn = "今日出库", EnUs = "Today Outbound", JaJp = "本日出庫" },
            new { Key = "dashboard.card.todayvisitors", Module = "Frontend", ZhCn = "今日来访", EnUs = "Today Entourage", JaJp = "本日訪問者" },
            
            // 仪表盘目的地
            new { Key = "dashboard.destination.usa", Module = "Frontend", ZhCn = "美国", EnUs = "USA", JaJp = "アメリカ" },
            new { Key = "dashboard.destination.eur", Module = "Frontend", ZhCn = "欧洲", EnUs = "EUR", JaJp = "ヨーロッパ" },
            new { Key = "dashboard.destination.china", Module = "Frontend", ZhCn = "中国", EnUs = "CHINA", JaJp = "中国" },
            new { Key = "dashboard.destination.japan", Module = "Frontend", ZhCn = "日本", EnUs = "JAPAN", JaJp = "日本" },
            new { Key = "dashboard.destination.other", Module = "Frontend", ZhCn = "其他", EnUs = "OTHER", JaJp = "その他" },
            
            // 关于页面
            new { Key = "about.technology", Module = "Frontend", ZhCn = "技术栈", EnUs = "Technology Stack", JaJp = "技術スタック" },
            new { Key = "about.framework", Module = "Frontend", ZhCn = "开发框架", EnUs = "Development Framework", JaJp = "開発フレームワーク" },
            new { Key = "about.framework.value", Module = "Frontend", ZhCn = ".NET 9.0", EnUs = ".NET 9.0", JaJp = ".NET 9.0" },
            new { Key = "about.uiframework", Module = "Frontend", ZhCn = "界面框架", EnUs = "UI Framework", JaJp = "UIフレームワーク" },
            new { Key = "about.uiframework.value", Module = "Frontend", ZhCn = "WPF（Windows Presentation Foundation）", EnUs = "WPF (Windows Presentation Foundation)", JaJp = "WPF（Windows Presentation Foundation）" },
            new { Key = "about.database", Module = "Frontend", ZhCn = "数据存储", EnUs = "Data Storage", JaJp = "データストレージ" },
            new { Key = "about.database.value", Module = "Frontend", ZhCn = "SQL Server / SqlSugar ORM", EnUs = "SQL Server / SqlSugar ORM", JaJp = "SQL Server / SqlSugar ORM" },
            new { Key = "about.architecture", Module = "Frontend", ZhCn = "架构模式", EnUs = "Architecture Pattern", JaJp = "アーキテクチャパターン" },
            new { Key = "about.architecture.value", Module = "Frontend", ZhCn = "MVVM（Model-View-ViewModel）", EnUs = "MVVM (Model-View-ViewModel)", JaJp = "MVVM（Model-View-ViewModel）" },
            new { Key = "about.builddate", Module = "Frontend", ZhCn = "构建时间", EnUs = "Build Time", JaJp = "ビルド日時" },
            new { Key = "about.edition", Module = "Frontend", ZhCn = "社区版 (64 位) - Current", EnUs = "Community (64-bit) - Current", JaJp = "コミュニティ版 (64 ビット) - Current" },
            new { Key = "about.version.format", Module = "Frontend", ZhCn = "版本 {0}", EnUs = "Version {0}", JaJp = "バージョン {0}" },
            new { Key = "about.dotnetversion.format", Module = "Frontend", ZhCn = ".NET {0}", EnUs = ".NET {0}", JaJp = ".NET {0}" },
            new { Key = "about.links.licensestatus", Module = "Frontend", ZhCn = "许可状态", EnUs = "License Status", JaJp = "ライセンス状態" },
            new { Key = "about.links.licenseterms", Module = "Frontend", ZhCn = "许可证条款", EnUs = "License Terms", JaJp = "ライセンス条項" },
            new { Key = "about.links.licensestatus.message", Module = "Frontend", ZhCn = "许可状态功能尚未实现。", EnUs = "License status is not implemented yet.", JaJp = "ライセンス状態はまだ実装されていません。" },
            new { Key = "about.links.licenseterms.message", Module = "Frontend", ZhCn = "许可证条款查看功能尚未实现。", EnUs = "Viewing license terms is not implemented yet.", JaJp = "ライセンス条項の表示はまだ実装されていません。" },
            new { Key = "about.section.productinfo", Module = "Frontend", ZhCn = "产品信息", EnUs = "Product Information", JaJp = "製品情報" },
            new { Key = "about.section.environmentinfo", Module = "Frontend", ZhCn = "环境信息", EnUs = "Environment Information", JaJp = "環境情報" },
            new { Key = "about.label.productname", Module = "Frontend", ZhCn = "产品名称", EnUs = "Product Name", JaJp = "製品名" },
            new { Key = "about.label.edition", Module = "Frontend", ZhCn = "版本类型", EnUs = "Edition", JaJp = "エディション" },
            new { Key = "about.label.version", Module = "Frontend", ZhCn = "版本号", EnUs = "Version", JaJp = "バージョン" },
            new { Key = "about.label.company", Module = "Frontend", ZhCn = "公司", EnUs = "Company", JaJp = "会社" },
            new { Key = "about.label.installationpath", Module = "Frontend", ZhCn = "安装位置", EnUs = "Installation Path", JaJp = "インストール場所" },
            new { Key = "about.label.dotnetversion", Module = "Frontend", ZhCn = ".NET 版本", EnUs = ".NET Version", JaJp = ".NET バージョン" },
            new { Key = "about.label.osversion", Module = "Frontend", ZhCn = "操作系统", EnUs = "Operating System", JaJp = "オペレーティングシステム" },
            new { Key = "about.label.architecture", Module = "Frontend", ZhCn = "体系结构", EnUs = "Architecture", JaJp = "アーキテクチャ" },
            new { Key = "about.label.processorcount", Module = "Frontend", ZhCn = "处理器数量", EnUs = "Processor Count", JaJp = "プロセッサ数" },
            new { Key = "about.label.runtimeidentifier", Module = "Frontend", ZhCn = "运行时标识", EnUs = "Runtime Identifier", JaJp = "ランタイム識別子" },
            new { Key = "about.installedproducts.title", Module = "Frontend", ZhCn = "已安装的组件", EnUs = "Installed Products", JaJp = "インストール済みコンポーネント" },
            new { Key = "about.installedproducts.subtitle", Module = "Frontend", ZhCn = "以下组件已经安装并可供使用。", EnUs = "The following components are installed and ready to use.", JaJp = "次のコンポーネントがインストールされ、使用できます。" },
            new { Key = "about.installed.item1", Module = "Frontend", ZhCn = ".NET 9.0 SDK 与 Windows 桌面运行时", EnUs = ".NET 9.0 SDK & Windows Desktop Runtime", JaJp = ".NET 9.0 SDK と Windows デスクトップ ランタイム" },
            new { Key = "about.installed.item2", Module = "Frontend", ZhCn = "WPF - Windows Presentation Foundation", EnUs = "WPF - Windows Presentation Foundation", JaJp = "WPF - Windows Presentation Foundation" },
            new { Key = "about.installed.item3", Module = "Frontend", ZhCn = "Prism (9.0.537) - 模块化 MVVM 框架", EnUs = "Prism (9.0.537) - Modular MVVM Framework", JaJp = "Prism (9.0.537) - モジュラー MVVM フレームワーク" },
            new { Key = "about.installed.item4", Module = "Frontend", ZhCn = "CommunityToolkit.Mvvm (8.4.0)", EnUs = "CommunityToolkit.Mvvm (8.4.0)", JaJp = "CommunityToolkit.Mvvm (8.4.0)" },
            new { Key = "about.installed.item5", Module = "Frontend", ZhCn = "MaterialDesignThemes (5.3.0) UI 库", EnUs = "MaterialDesignThemes (5.3.0) UI Library", JaJp = "MaterialDesignThemes (5.3.0) UI ライブラリ" },
            new { Key = "about.installed.item6", Module = "Frontend", ZhCn = "FontAwesome.Sharp (6.6.0) 图标库", EnUs = "FontAwesome.Sharp (6.6.0) Icon Library", JaJp = "FontAwesome.Sharp (6.6.0) アイコンライブラリ" },
            new { Key = "about.installed.item7", Module = "Frontend", ZhCn = "Autofac (8.4.0) 依赖注入容器", EnUs = "Autofac (8.4.0) Dependency Injection Container", JaJp = "Autofac (8.4.0) 依存性注入コンテナ" },
            new { Key = "about.installed.item8", Module = "Frontend", ZhCn = "SqlSugar ORM - 轻量级 ORM 框架", EnUs = "SqlSugar ORM - Lightweight ORM Framework", JaJp = "SqlSugar ORM - 軽量 ORM フレームワーク" },
            new { Key = "about.installed.item9", Module = "Frontend", ZhCn = "Serilog (4.3.0) 结构化日志框架", EnUs = "Serilog (4.3.0) Structured Logging Framework", JaJp = "Serilog (4.3.0) 構造化ロギングフレームワーク" },
            new { Key = "about.installed.item10", Module = "Frontend", ZhCn = "Scriban (6.5.2) 模板引擎", EnUs = "Scriban (6.5.2) Template Engine", JaJp = "Scriban (6.5.2) テンプレートエンジン" },
            new { Key = "about.installed.item11", Module = "Frontend", ZhCn = "Newtonsoft.Json (13.0.4) JSON 序列化库", EnUs = "Newtonsoft.Json (13.0.4) JSON Serialization Library", JaJp = "Newtonsoft.Json (13.0.4) JSON シリアル化ライブラリ" },
            new { Key = "about.installed.item12", Module = "Frontend", ZhCn = "LibVLCSharp.WPF (3.9.4) 媒体播放器", EnUs = "LibVLCSharp.WPF (3.9.4) Media Player", JaJp = "LibVLCSharp.WPF (3.9.4) メディアプレーヤー" },
            new { Key = "about.dialog.title", Module = "Frontend", ZhCn = "关于", EnUs = "About", JaJp = "バージョン情報" },
            new { Key = "about.button.systeminfo", Module = "Frontend", ZhCn = "系统信息", EnUs = "System Information", JaJp = "システム情報" },
            new { Key = "about.systeminfo.title", Module = "Frontend", ZhCn = "系统信息", EnUs = "System Information", JaJp = "システム情報" },
            new { Key = "about.systeminfo.copysuccess", Module = "Frontend", ZhCn = "系统信息已复制到剪贴板", EnUs = "System information copied to clipboard", JaJp = "システム情報がクリップボードにコピーされました" },
            new { Key = "about.systeminfo.copyfailed", Module = "Frontend", ZhCn = "复制失败", EnUs = "Copy failed", JaJp = "コピーに失敗しました" },
            new { Key = "about.systeminfo.tab.summary", Module = "Frontend", ZhCn = "系统摘要", EnUs = "System Summary", JaJp = "システム概要" },
            new { Key = "about.systeminfo.tab.hardware", Module = "Frontend", ZhCn = "硬件信息", EnUs = "Hardware Information", JaJp = "ハードウェア情報" },
            new { Key = "about.systeminfo.tab.software", Module = "Frontend", ZhCn = "软件信息", EnUs = "Software Information", JaJp = "ソフトウェア情報" },
            new { Key = "about.systeminfo.tab.network", Module = "Frontend", ZhCn = "网络信息", EnUs = "Network Information", JaJp = "ネットワーク情報" },
            new { Key = "about.systeminfo.column.item", Module = "Frontend", ZhCn = "项目", EnUs = "Item", JaJp = "項目" },
            new { Key = "about.systeminfo.column.value", Module = "Frontend", ZhCn = "值", EnUs = "Value", JaJp = "値" },
            new { Key = "about.systeminfo.network.adapters", Module = "Frontend", ZhCn = "网络适配器", EnUs = "Network Adapters", JaJp = "ネットワークアダプター" },
            new { Key = "about.systeminfo.network.name", Module = "Frontend", ZhCn = "名称", EnUs = "Name", JaJp = "名前" },
            new { Key = "about.systeminfo.network.mac", Module = "Frontend", ZhCn = "MAC地址", EnUs = "MAC Address", JaJp = "MACアドレス" },
            new { Key = "about.systeminfo.network.ip", Module = "Frontend", ZhCn = "IP地址", EnUs = "IP Address", JaJp = "IPアドレス" },
            new { Key = "about.systeminfo.network.speed", Module = "Frontend", ZhCn = "速度", EnUs = "Speed", JaJp = "速度" },
            new { Key = "about.systeminfo.network.status", Module = "Frontend", ZhCn = "状态", EnUs = "Status", JaJp = "状態" },
            new { Key = "about.systeminfo.network.active", Module = "Frontend", ZhCn = "活动", EnUs = "Active", JaJp = "アクティブ" },
            new { Key = "about.systeminfo.network.inactive", Module = "Frontend", ZhCn = "非活动", EnUs = "Inactive", JaJp = "非アクティブ" },
            new { Key = "about.systeminfo.disk.name", Module = "Frontend", ZhCn = "磁盘", EnUs = "Disk", JaJp = "ディスク" },
            new { Key = "about.systeminfo.software.name", Module = "Frontend", ZhCn = "软件名称", EnUs = "Software Name", JaJp = "ソフトウェア名" },
            new { Key = "about.systeminfo.software.version", Module = "Frontend", ZhCn = "版本", EnUs = "Version", JaJp = "バージョン" },
            new { Key = "about.systeminfo.software.publisher", Module = "Frontend", ZhCn = "发布者", EnUs = "Publisher", JaJp = "発行者" },
            new { Key = "about.systeminfo.user.name", Module = "Frontend", ZhCn = "用户名", EnUs = "User Name", JaJp = "ユーザー名" },
            new { Key = "about.systeminfo.user.fullname", Module = "Frontend", ZhCn = "全名", EnUs = "Full Name", JaJp = "フルネーム" },
            new { Key = "about.systeminfo.user.isadmin", Module = "Frontend", ZhCn = "管理员", EnUs = "Administrator", JaJp = "管理者" },
            new { Key = "about.systeminfo.user.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "about.systeminfo.user.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            new { Key = "about.systeminfo.summary.systemlanguage", Module = "Frontend", ZhCn = "系统语言", EnUs = "System Language", JaJp = "システム言語" },
            new { Key = "about.systeminfo.hardware.diskinfo", Module = "Frontend", ZhCn = "磁盘信息", EnUs = "Disk Information", JaJp = "ディスク情報" },
            new { Key = "about.systeminfo.copy.title", Module = "Frontend", ZhCn = "========== 系统信息 ==========", EnUs = "========== System Information ==========", JaJp = "========== システム情報 ==========" },
            new { Key = "about.systeminfo.copy.section.summary", Module = "Frontend", ZhCn = "【系统摘要】", EnUs = "【System Summary】", JaJp = "【システム概要】" },
            new { Key = "about.systeminfo.copy.section.hardware", Module = "Frontend", ZhCn = "【硬件信息】", EnUs = "【Hardware Information】", JaJp = "【ハードウェア情報】" },
            new { Key = "about.systeminfo.copy.section.software", Module = "Frontend", ZhCn = "【软件信息】", EnUs = "【Software Information】", JaJp = "【ソフトウェア情報】" },
            new { Key = "about.systeminfo.copy.section.network", Module = "Frontend", ZhCn = "【网络信息】", EnUs = "【Network Information】", JaJp = "【ネットワーク情報】" },
            new { Key = "about.systeminfo.key.os", Module = "Frontend", ZhCn = "操作系统", EnUs = "Operating System", JaJp = "オペレーティングシステム" },
            new { Key = "about.systeminfo.key.osversion", Module = "Frontend", ZhCn = "系统版本", EnUs = "OS Version", JaJp = "OSバージョン" },
            new { Key = "about.systeminfo.key.ostype", Module = "Frontend", ZhCn = "系统类型", EnUs = "OS Type", JaJp = "OSタイプ" },
            new { Key = "about.systeminfo.key.osarchitecture", Module = "Frontend", ZhCn = "系统架构", EnUs = "OS Architecture", JaJp = "OSアーキテクチャ" },
            new { Key = "about.systeminfo.key.machinename", Module = "Frontend", ZhCn = "机器名称", EnUs = "Machine Name", JaJp = "マシン名" },
            new { Key = "about.systeminfo.key.username", Module = "Frontend", ZhCn = "用户名", EnUs = "User Name", JaJp = "ユーザー名" },
            new { Key = "about.systeminfo.key.isadmin", Module = "Frontend", ZhCn = "是否管理员", EnUs = "Is Administrator", JaJp = "管理者か" },
            new { Key = "about.systeminfo.key.runtime", Module = "Frontend", ZhCn = "运行时", EnUs = "Runtime", JaJp = "ランタイム" },
            new { Key = "about.systeminfo.key.processarchitecture", Module = "Frontend", ZhCn = "进程架构", EnUs = "Process Architecture", JaJp = "プロセスアーキテクチャ" },
            new { Key = "about.systeminfo.key.systemuptime", Module = "Frontend", ZhCn = "系统运行时间", EnUs = "System Uptime", JaJp = "システム稼働時間" },
            new { Key = "about.systeminfo.key.cpu", Module = "Frontend", ZhCn = "CPU", EnUs = "CPU", JaJp = "CPU" },
            new { Key = "about.systeminfo.key.cpuname", Module = "Frontend", ZhCn = "CPU名称", EnUs = "CPU Name", JaJp = "CPU名" },
            new { Key = "about.systeminfo.key.cpucores", Module = "Frontend", ZhCn = "CPU核心", EnUs = "CPU Cores", JaJp = "CPUコア" },
            new { Key = "about.systeminfo.key.totalmemory", Module = "Frontend", ZhCn = "物理内存", EnUs = "Total Memory", JaJp = "物理メモリ" },
            new { Key = "about.systeminfo.key.availablememory", Module = "Frontend", ZhCn = "可用内存", EnUs = "Available Memory", JaJp = "利用可能メモリ" },
            new { Key = "about.systeminfo.key.memoryusage", Module = "Frontend", ZhCn = "内存使用率", EnUs = "Memory Usage", JaJp = "メモリ使用率" },
            new { Key = "about.systeminfo.key.processmemory", Module = "Frontend", ZhCn = "进程内存", EnUs = "Process Memory", JaJp = "プロセスメモリ" },
            new { Key = "about.systeminfo.key.ipaddress", Module = "Frontend", ZhCn = "IP地址", EnUs = "IP Address", JaJp = "IPアドレス" },
            new { Key = "about.systeminfo.key.macaddress", Module = "Frontend", ZhCn = "MAC地址", EnUs = "MAC Address", JaJp = "MACアドレス" },
            new { Key = "about.disclaimer", Module = "Frontend", ZhCn = "警告: 本计算机程序受著作权法以及国际版权公约保护。未经授权而擅自复制或传播本程序（或其中任何部分），将受到严厉的民事及刑事处罚，并将在法律许可的最大限度内受到追诉。", EnUs = "Warning: This computer program is protected by copyright law and international copyright conventions. Unauthorized copying or distribution of this program (or any part thereof) will be subject to severe civil and criminal penalties, and will be prosecuted to the fullest extent permitted by law.", JaJp = "警告: 本コンピュータプログラムは著作権法および国際著作権条約によって保護されています。許可なく本プログラム（またはその一部）を複製または配布することは、厳しい民事および刑事罰の対象となり、法律で許可される最大限の範囲で起訴されます。" },

            // 用户管理相关（通用功能，非实体字段）
            new { Key = "common.button.assignrole", Module = "Frontend", ZhCn = "分配角色", EnUs = "Assign Role", JaJp = "ロールを割り当て" },
            new { Key = "common.button.assigndept", Module = "Frontend", ZhCn = "分配部门", EnUs = "Assign Department", JaJp = "部門を割り当て" },
            new { Key = "common.button.assignmenu", Module = "Frontend", ZhCn = "分配菜单", EnUs = "Assign Menu", JaJp = "メニューを割り当て" },
            new { Key = "common.button.assignpost", Module = "Frontend", ZhCn = "分配岗位", EnUs = "Assign Position", JaJp = "ポジションを割り当て" },

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

            // 通用校验（格式验证翻译）
            // 注意：所有通用的验证消息都使用参数化格式，便于复用
            new { Key = "common.validation.required", Module = "Frontend", ZhCn = "{0}不能为空", EnUs = "{0} cannot be empty", JaJp = "{0} は空にできません" },
            new { Key = "common.validation.format", Module = "Frontend", ZhCn = "{0}格式不正确", EnUs = "{0} format is incorrect", JaJp = "{0} の形式が正しくありません" },
            new { Key = "common.validation.maxlength", Module = "Frontend", ZhCn = "{0}长度不能超过{1}个字符", EnUs = "{0} cannot exceed {1} characters", JaJp = "{0} は{1}文字を超えることはできません" },
            new { Key = "common.validation.minlength", Module = "Frontend", ZhCn = "{0}长度不能少于{1}位", EnUs = "{0} must be at least {1} characters", JaJp = "{0} は{1}文字以上である必要があります" },
            new { Key = "common.validation.invalid", Module = "Frontend", ZhCn = "{0}无效", EnUs = "{0} is invalid", JaJp = "{0} が無効です" },
            new { Key = "common.validation.range", Module = "Frontend", ZhCn = "{0}必须在{1}和{2}之间", EnUs = "{0} must be between {1} and {2}", JaJp = "{0} は{1}と{2}の間である必要があります" },
            new { Key = "common.validation.mustberelativepath", Module = "Frontend", ZhCn = "{0}必须是相对路径，不能使用绝对路径或URL", EnUs = "{0} must be a relative path, absolute paths or URLs are not allowed", JaJp = "{0} は相対パスである必要があります。絶対パスやURLは使用できません" },
            
            // 通用操作结果（失败类）
            new { Key = "common.failed.create", Module = "Frontend", ZhCn = "{0}创建失败", EnUs = "Failed to create {0}", JaJp = "{0} の作成に失敗しました" },
            new { Key = "common.failed.update", Module = "Frontend", ZhCn = "{0}更新失败", EnUs = "Failed to update {0}", JaJp = "{0} の更新に失敗しました" },
            new { Key = "common.failed.delete", Module = "Frontend", ZhCn = "{0}删除失败", EnUs = "Failed to delete {0}", JaJp = "{0} の削除に失敗しました" },
            new { Key = "common.failed.import", Module = "Frontend", ZhCn = "{0}导入失败", EnUs = "Failed to import {0}", JaJp = "{0} のインポートに失敗しました" },
            new { Key = "common.failed.export", Module = "Frontend", ZhCn = "{0}导出失败", EnUs = "Failed to export {0}", JaJp = "{0} のエクスポートに失敗しました" },
            new { Key = "common.savefailed", Module = "Frontend", ZhCn = "保存失败：{0}", EnUs = "Save failed: {0}", JaJp = "保存に失敗しました：{0}" },
            
            // 消息框相关
            new { Key = "common.messagebox.information", Module = "Frontend", ZhCn = "信息", EnUs = "Information", JaJp = "情報" },
            new { Key = "common.messagebox.warning", Module = "Frontend", ZhCn = "警告", EnUs = "Warning", JaJp = "警告" },
            new { Key = "common.messagebox.error", Module = "Frontend", ZhCn = "错误", EnUs = "Error", JaJp = "エラー" },
            new { Key = "common.messagebox.question", Module = "Frontend", ZhCn = "确认", EnUs = "Question", JaJp = "確認" },
            new { Key = "common.messagebox.confirmdelete", Module = "Frontend", ZhCn = "确认删除", EnUs = "Confirm Delete", JaJp = "削除の確認" },
            new { Key = "common.messagebox.confirmdeletemessage", Module = "Frontend", ZhCn = "确定要删除这条记录吗？", EnUs = "Are you sure you want to delete this record?", JaJp = "このレコードを削除してもよろしいですか？" },
            
            // 数据库相关错误消息
            new { Key = "database.connectionerror.title", Module = "Frontend", ZhCn = "数据库连接失败", EnUs = "Database Connection Failed", JaJp = "データベース接続失敗" },
            new { Key = "database.connectionerror.failed", Module = "Frontend", ZhCn = "数据库连接失败，无法启动系统。\n\n请检查数据库服务器和连接字符串配置。", EnUs = "Database connection failed, unable to start the system.\n\nPlease check the database server and connection string configuration.", JaJp = "データベース接続に失敗しました。システムを起動できません。\n\nデータベースサーバーと接続文字列の設定を確認してください。" },
            new { Key = "database.connectionerror.failed_detail", Module = "Frontend", ZhCn = "数据库连接失败，无法启动系统。\n\n请检查：\n1. 数据库服务器是否正常运行\n2. 连接字符串配置是否正确（appsettings.json）\n3. 数据库是否已创建\n\n如需自动初始化数据库，请在 appsettings.json 中设置：\n- \"EnableCodeFirst\": true（自动创建表结构）\n- \"EnableSeedData\": true（自动初始化种子数据）", EnUs = "Database connection failed, unable to start the system.\n\nPlease check:\n1. Is the database server running normally\n2. Is the connection string configured correctly (appsettings.json)\n3. Has the database been created\n\nTo automatically initialize the database, please set in appsettings.json:\n- \"EnableCodeFirst\": true (automatically create table structure)\n- \"EnableSeedData\": true (automatically initialize seed data)", JaJp = "データベース接続に失敗しました。システムを起動できません。\n\n確認してください：\n1. データベースサーバーは正常に稼働していますか\n2. 接続文字列の設定は正しいですか（appsettings.json）\n3. データベースは作成されていますか\n\nデータベースを自動的に初期化するには、appsettings.json で以下を設定してください：\n- \"EnableCodeFirst\": true（テーブル構造を自動作成）\n- \"EnableSeedData\": true（シードデータを自動初期化）" },
            new { Key = "database.connectionerror.exception_startup", Module = "Frontend", ZhCn = "数据库连接检查失败：{0}\n\n应用程序将关闭。", EnUs = "Database connection check failed: {0}\n\nThe application will close.", JaJp = "データベース接続チェックに失敗しました：{0}\n\nアプリケーションを終了します。" },
            new { Key = "database.tables_not_initialized.title", Module = "Frontend", ZhCn = "数据库未初始化", EnUs = "Database Not Initialized", JaJp = "データベースが初期化されていません" },
            new { Key = "database.tables_not_initialized.message", Module = "Frontend", ZhCn = "数据库未初始化，无法启动系统。\n\n检测到数据库表不存在，系统需要初始化数据库。\n\n请在 appsettings.json 中设置以下配置之一：\n\n方案一：自动创建表结构\n  \"EnableCodeFirst\": true\n\n方案二：自动初始化完整数据\n  \"EnableCodeFirst\": true\n  \"EnableSeedData\": true\n\n配置完成后，请重新启动应用程序。", EnUs = "Database not initialized, unable to start the system.\n\nDetected that database tables do not exist, the system needs to initialize the database.\n\nPlease set one of the following configurations in appsettings.json:\n\nOption 1: Automatically create table structure\n  \"EnableCodeFirst\": true\n\nOption 2: Automatically initialize complete data\n  \"EnableCodeFirst\": true\n  \"EnableSeedData\": true\n\nAfter configuration, please restart the application.", JaJp = "データベースが初期化されていません。システムを起動できません。\n\nデータベーステーブルが存在しないことが検出されました。システムはデータベースを初期化する必要があります。\n\nappsettings.json で以下のいずれかの設定を行ってください：\n\nオプション1：テーブル構造を自動作成\n  \"EnableCodeFirst\": true\n\nオプション2：完全なデータを自動初期化\n  \"EnableCodeFirst\": true\n  \"EnableSeedData\": true\n\n設定後、アプリケーションを再起動してください。" },
            new { Key = "database.initialization.title", Module = "Frontend", ZhCn = "数据库初始化提示", EnUs = "Database Initialization", JaJp = "データベース初期化" },
            new { Key = "database.initialization.error", Module = "Frontend", ZhCn = "数据库初始化失败：{0}\n\n应用程序将关闭。", EnUs = "Database initialization failed: {0}\n\nThe application will close.", JaJp = "データベース初期化に失敗しました：{0}\n\nアプリケーションを終了します。" },
            new { Key = "application.startup.error", Module = "Frontend", ZhCn = "应用程序启动失败：{0}\n\n详细信息：{1}", EnUs = "Application startup failed: {0}\n\nDetails: {1}", JaJp = "アプリケーション起動に失敗しました：{0}\n\n詳細：{1}" },
            new { Key = "application.startup.error.title", Module = "Frontend", ZhCn = "启动错误", EnUs = "Startup Error", JaJp = "起動エラー" },

            // 通用占位（参数化占位符）
            new { Key = "common.nodata", Module = "Frontend", ZhCn = "暂无数据", EnUs = "No Data", JaJp = "データなし" },
            new { Key = "common.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "common.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            new { Key = "common.template", Module = "Frontend", ZhCn = "模板", EnUs = "Template", JaJp = "テンプレート" },
            new { Key = "common.file", Module = "Frontend", ZhCn = "文件", EnUs = "File", JaJp = "ファイル" },
            new { Key = "common.placeholder.input", Module = "Frontend", ZhCn = "请输入{0}", EnUs = "Please enter {0}", JaJp = "{0} を入力してください" },
            new { Key = "common.placeholder.select", Module = "Frontend", ZhCn = "请选择{0}", EnUs = "Please select {0}", JaJp = "{0} を選択してください" },
            new { Key = "common.placeholder.keywordHint", Module = "Frontend", ZhCn = "请输入{0}进行搜索", EnUs = "Please enter {0} to search", JaJp = "{0} を入力して検索してください" },
            // 注意：以下翻译键已删除，应使用通用翻译拼接：
            // - "请输入{0}等关键字查询" -> 使用 common.placeholder.input + "等关键字查询"
            // - "请选择{0}范围" -> 使用 common.placeholder.select + "范围"
            new { Key = "common.selectionheaderhint", Module = "Frontend", ZhCn = "全选/取消全选", EnUs = "Select/Deselect all", JaJp = "全選択/全解除" },
            new { Key = "common.selectionrowhint", Module = "Frontend", ZhCn = "选择/取消选择该行", EnUs = "Select/Deselect this row", JaJp = "この行を選択/解除" },
            new { Key = "common.goto", Module = "Frontend", ZhCn = "前往", EnUs = "Go to", JaJp = "移動" },

            // 通用操作
            new { Key = "common.operation", Module = "Frontend", ZhCn = "操作", EnUs = "Operation", JaJp = "操作" },
            new { Key = "common.search", Module = "Frontend", ZhCn = "搜索", EnUs = "Search", JaJp = "検索" },
            new { Key = "common.reset", Module = "Frontend", ZhCn = "重置", EnUs = "Reset", JaJp = "リセット" },
            new { Key = "common.loading", Module = "Frontend", ZhCn = "加载中...", EnUs = "Loading...", JaJp = "読み込み中..." },
            new { Key = "common.advancedquery", Module = "Frontend", ZhCn = "高级查询", EnUs = "Advanced Query", JaJp = "高度検索" },
            new { Key = "common.togglecolumns", Module = "Frontend", ZhCn = "显隐列", EnUs = "Toggle Columns", JaJp = "列の表示/非表示" },
            new { Key = "common.togglequerybar", Module = "Frontend", ZhCn = "显隐查询栏", EnUs = "Toggle Query Bar", JaJp = "検索バーの表示/非表示" },
            new { Key = "common.expandall", Module = "Frontend", ZhCn = "展开全部", EnUs = "Expand All", JaJp = "すべて展開" },
            new { Key = "common.collapseall", Module = "Frontend", ZhCn = "折叠全部", EnUs = "Collapse All", JaJp = "すべて折りたたむ" },
            new { Key = "common.firstpage", Module = "Frontend", ZhCn = "首页", EnUs = "First Page", JaJp = "最初のページ" },
            new { Key = "common.prevpage", Module = "Frontend", ZhCn = "上一页", EnUs = "Previous Page", JaJp = "前のページ" },
            new { Key = "common.nextpage", Module = "Frontend", ZhCn = "下一页", EnUs = "Next Page", JaJp = "次のページ" },
            new { Key = "common.lastpage", Module = "Frontend", ZhCn = "末页", EnUs = "Last Page", JaJp = "最後のページ" },
            new { Key = "common.total", Module = "Frontend", ZhCn = "共 {0} 条记录", EnUs = "Total {0} records", JaJp = "合計 {0} 件" },
            new { Key = "common.pagedisplay", Module = "Frontend", ZhCn = "第 {0} / {1} 页", EnUs = "Page {0} / {1}", JaJp = "{0} / {1} ページ" },
            new { Key = "common.pagesizehint", Module = "Frontend", ZhCn = "每页", EnUs = "Per Page", JaJp = "1ページあたり" },
            new { Key = "common.pageinputhint", Module = "Frontend", ZhCn = "页码", EnUs = "Page", JaJp = "ページ" },
            
            // 通用按钮（统一键）
            new { Key = "common.button.close", Module = "Frontend", ZhCn = "关闭", EnUs = "Close", JaJp = "閉じる" },
            new { Key = "common.button.fullscreen", Module = "Frontend", ZhCn = "全屏", EnUs = "Full Screen", JaJp = "フルスクリーン" },
            new { Key = "common.button.exit", Module = "Frontend", ZhCn = "退出", EnUs = "Exit", JaJp = "終了" },
            new { Key = "common.button.quit", Module = "Frontend", ZhCn = "退出", EnUs = "Quit", JaJp = "終了" },
             
            new { Key = "common.button.edit", Module = "Frontend", ZhCn = "编辑", EnUs = "Edit", JaJp = "編集" },
            new { Key = "common.button.insert", Module = "Frontend", ZhCn = "插入", EnUs = "Insert", JaJp = "挿入" },
            new { Key = "common.button.change", Module = "Frontend", ZhCn = "切换", EnUs = "Change", JaJp = "切り替え" },
            new { Key = "common.button.login", Module = "Frontend", ZhCn = "登录", EnUs = "Login", JaJp = "ログイン" },
            new { Key = "common.button.confirm", Module = "Frontend", ZhCn = "确认", EnUs = "Confirm", JaJp = "確認" },
            new { Key = "common.button.query", Module = "Frontend", ZhCn = "查询", EnUs = "Query", JaJp = "検索" },
            new { Key = "common.button.read", Module = "Frontend", ZhCn = "查看", EnUs = "Read", JaJp = "閲覧" },
            new { Key = "common.button.create", Module = "Frontend", ZhCn = "新增", EnUs = "Create", JaJp = "新規" },
            new { Key = "common.button.createrow", Module = "Frontend", ZhCn = "新增行", EnUs = "Create Row", JaJp = "新規行" },
            new { Key = "common.button.createcolumn", Module = "Frontend", ZhCn = "新增列", EnUs = "Create Column", JaJp = "新規列" },
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
            new { Key = "common.button.save", Module = "Frontend", ZhCn = "保存", EnUs = "Save", JaJp = "保存" },
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
            new { Key = "common.button.download", Module = "Frontend", ZhCn = "{0}下载", EnUs = "Download {0}", JaJp = "{0}をダウンロード" },
            new { Key = "common.button.upload", Module = "Frontend", ZhCn = "{0}上传", EnUs = "Upload {0}", JaJp = "{0}をアップロード" },
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
            new { Key = "common.button.clear", Module = "Frontend", ZhCn = "清理", EnUs = "Clear", JaJp = "クリア" },
            
            // 日志清理相关
            new { Key = "logging.cleanup.confirm", Module = "Frontend", ZhCn = "确定要清理超过7天的日志吗？此操作将删除文本日志文件和数据表日志记录，且无法恢复。", EnUs = "Are you sure you want to clean up logs older than 7 days? This operation will delete text log files and database log records, and cannot be undone.", JaJp = "7日を超えるログをクリーンアップしてもよろしいですか？この操作はテキストログファイルとデータベースログレコードを削除し、元に戻すことはできません。" },
            new { Key = "logging.cleanup.success", Module = "Frontend", ZhCn = "日志清理成功：清理了 {0} 个文件（{1}）和 {2} 条数据库记录", EnUs = "Log cleanup successful: {0} files ({1}) and {2} database records cleaned", JaJp = "ログクリーンアップ成功：{0} ファイル（{1}）と {2} データベースレコードをクリーンアップしました" },
            new { Key = "logging.cleanup.failed", Module = "Frontend", ZhCn = "日志清理失败：{0}", EnUs = "Log cleanup failed: {0}", JaJp = "ログクリーンアップ失敗：{0}" },
            new { Key = "logging.cleanup.inprogress", Module = "Frontend", ZhCn = "正在清理日志，请稍候...", EnUs = "Cleaning up logs, please wait...", JaJp = "ログをクリーンアップしています。お待ちください..." },
            new { Key = "logging.cleanup.filesize", Module = "Frontend", ZhCn = "{0} KB", EnUs = "{0} KB", JaJp = "{0} KB" },
            
            // 日志清理范围选项
            new { Key = "logging.cleanup.range.today", Module = "Frontend", ZhCn = "按日", EnUs = "By Day", JaJp = "日ごと" },
            new { Key = "logging.cleanup.range.sevendays", Module = "Frontend", ZhCn = "按周", EnUs = "By Week", JaJp = "週ごと" },
            new { Key = "logging.cleanup.range.thirtydays", Module = "Frontend", ZhCn = "按月", EnUs = "By Month", JaJp = "月ごと" },
            new { Key = "logging.cleanup.range.all", Module = "Frontend", ZhCn = "全部", EnUs = "All", JaJp = "すべて" },
            

            // 应用标题
            new { Key = "application.title", Module = "Frontend", ZhCn = "Takt SMEs", EnUs = "Takt SMEs", JaJp = "Takt SMEs" },
            
            // 设置页面翻译
            new { Key = "settings.customize.title", Module = "Frontend", ZhCn = "用户设置", EnUs = "User Settings", JaJp = "ユーザー設定" },
            new { Key = "settings.customize.description", Module = "Frontend", ZhCn = "自定义应用程序的外观和行为", EnUs = "Customize the appearance and behavior of the application", JaJp = "アプリケーションの外観と動作をカスタマイズ" },
            new { Key = "settings.customize.appearanceandbehavior", Module = "Frontend", ZhCn = "外观与行为", EnUs = "Appearance & Behavior", JaJp = "外観と動作" },
            new { Key = "settings.customize.thememode", Module = "Frontend", ZhCn = "主题模式", EnUs = "Theme Mode", JaJp = "テーマモード" },
            new { Key = "settings.customize.thememode.description", Module = "Frontend", ZhCn = "选择应用程序的主题模式", EnUs = "Select the theme mode for the application", JaJp = "アプリケーションのテーマモードを選択" },
            
            // 主题、语言（用于拼接）
            new { Key = "common.theme.title", Module = "Frontend", ZhCn = "主题", EnUs = "Theme", JaJp = "テーマ" },
            new { Key = "common.language.title", Module = "Frontend", ZhCn = "语言", EnUs = "Language", JaJp = "言語" },
            new { Key = "common.confirm.message", Module = "Frontend", ZhCn = "确定要{0}吗？", EnUs = "Are you sure you want to {0}?", JaJp = "{0} してもよろしいですか？" },
            
            // 主题选项（统一使用 common.theme.* 前缀）
            new { Key = "common.theme.system", Module = "Frontend", ZhCn = "跟随系统", EnUs = "Follow System", JaJp = "システムに従う" },
            new { Key = "common.theme.light", Module = "Frontend", ZhCn = "浅色", EnUs = "Light", JaJp = "ライト" },
            new { Key = "common.theme.dark", Module = "Frontend", ZhCn = "深色", EnUs = "Dark", JaJp = "ダーク" },
            
            // 主题切换提示文本
            new { Key = "common.clicktoswitch", Module = "Frontend", ZhCn = "点击切换到", EnUs = "Click to switch to", JaJp = "クリックして切り替え" },
            new { Key = "settings.customize.language", Module = "Frontend", ZhCn = "语言", EnUs = "Language", JaJp = "言語" },
            new { Key = "settings.customize.language.description", Module = "Frontend", ZhCn = "选择应用程序的显示语言", EnUs = "Select the display language for the application", JaJp = "アプリケーションの表示言語を選択" },
            new { Key = "settings.customize.fontsettings", Module = "Frontend", ZhCn = "字体设置", EnUs = "Font Settings", JaJp = "フォント設定" },
            new { Key = "settings.customize.fontfamily", Module = "Frontend", ZhCn = "字体族", EnUs = "Font Family", JaJp = "フォントファミリー" },
            new { Key = "settings.customize.fontfamily.description", Module = "Frontend", ZhCn = "选择应用程序的字体族", EnUs = "Select the font family for the application", JaJp = "アプリケーションのフォントファミリーを選択" },
            new { Key = "settings.customize.fontpreview", Module = "Frontend", ZhCn = "字体预览：", EnUs = "Font Preview: ", JaJp = "フォントプレビュー：" },
            new { Key = "settings.customize.fontsize", Module = "Frontend", ZhCn = "字体大小", EnUs = "Font Size", JaJp = "フォントサイズ" },
            new { Key = "settings.customize.fontsize.description", Module = "Frontend", ZhCn = "选择应用程序的字体大小", EnUs = "Select the font size for the application", JaJp = "アプリケーションのフォントサイズを選択" },
            new { Key = "settings.customize.fontsizepreview", Module = "Frontend", ZhCn = "字体大小预览", EnUs = "Font Size Preview", JaJp = "フォントサイズプレビュー" },
            new { Key = "settings.customize.fontsizepreview.sample", Module = "Frontend", ZhCn = "这是字体大小预览文本", EnUs = "This is a font size preview text", JaJp = "これはフォントサイズのプレビューテキストです" },
            new { Key = "settings.customize.loadfailed", Module = "Frontend", ZhCn = "加载设置失败：{0}", EnUs = "Failed to load settings: {0}", JaJp = "設定の読み込みに失敗しました：{0}" },
            new { Key = "settings.customize.languagenotselected", Module = "Frontend", ZhCn = "本地化管理器未初始化或未选择语言", EnUs = "Localization manager not initialized or language not selected", JaJp = "ローカライゼーションマネージャーが初期化されていないか、言語が選択されていません" },
            new { Key = "settings.customize.languagechanged", Module = "Frontend", ZhCn = "语言已切换为 {0}", EnUs = "Language changed to {0}", JaJp = "言語が {0} に変更されました" },
            new { Key = "settings.customize.languagechangefailed", Module = "Frontend", ZhCn = "切换语言失败：{0}", EnUs = "Failed to change language: {0}", JaJp = "言語の変更に失敗しました：{0}" },
            new { Key = "settings.customize.fontfamilynotselected", Module = "Frontend", ZhCn = "请选择字体", EnUs = "Please select a font family", JaJp = "フォントファミリーを選択してください" },
            new { Key = "settings.customize.fontfamilychanged", Module = "Frontend", ZhCn = "字体已切换为 {0}", EnUs = "Font family changed to {0}", JaJp = "フォントファミリーが {0} に変更されました" },
            new { Key = "settings.customize.fontfamilychangefailed", Module = "Frontend", ZhCn = "切换字体失败：{0}", EnUs = "Failed to change font family: {0}", JaJp = "フォントファミリーの変更に失敗しました：{0}" },
            new { Key = "settings.customize.fontsizenotselected", Module = "Frontend", ZhCn = "请选择字体大小", EnUs = "Please select a font size", JaJp = "フォントサイズを選択してください" },
            new { Key = "settings.customize.fontsizechanged", Module = "Frontend", ZhCn = "字体大小已切换为 {0}", EnUs = "Font size changed to {0}", JaJp = "フォントサイズが {0} に変更されました" },
            new { Key = "settings.customize.fontsizechangefailed", Module = "Frontend", ZhCn = "切换字体大小失败：{0}", EnUs = "Failed to change font size: {0}", JaJp = "フォントサイズの変更に失敗しました：{0}" },
            new { Key = "settings.customize.savesuccess", Module = "Frontend", ZhCn = "设置已保存成功", EnUs = "Settings saved successfully", JaJp = "設定が正常に保存されました" },
            new { Key = "settings.customize.restartrequired", Module = "Frontend", ZhCn = "设置已保存成功。请重启应用程序以使更改生效。", EnUs = "Settings saved successfully. Please restart the application for changes to take effect.", JaJp = "設定が正常に保存されました。変更を反映するには、アプリケーションを再起動してください。" },
            
            // 主窗口菜单项
            new { Key = "mainwindow.userinfocenter", Module = "Frontend", ZhCn = "用户信息", EnUs = "User Info", JaJp = "ユーザー情報" },
            
            // 菜单翻译（从菜单种子数据中的 I18nKey）
            new { Key = "menu.dashboard", Module = "Frontend", ZhCn = "仪表盘", EnUs = "Dashboard", JaJp = "ダッシュボード" },
            new { Key = "menu.logistics", Module = "Frontend", ZhCn = "后勤管理", EnUs = "Logistics", JaJp = "ロジスティクス" },
            new { Key = "menu.identity", Module = "Frontend", ZhCn = "身份认证", EnUs = "Identity", JaJp = "アイデンティティ" },
            new { Key = "menu.logging", Module = "Frontend", ZhCn = "日志管理", EnUs = "Logging", JaJp = "ログ管理" },
            new { Key = "menu.routine", Module = "Frontend", ZhCn = "日常事务", EnUs = "Routine", JaJp = "日常業務" },
            new { Key = "menu.about", Module = "Frontend", ZhCn = "关于", EnUs = "About", JaJp = "について" },
            new { Key = "menu.logistics.materials", Module = "Frontend", ZhCn = "物料管理", EnUs = "Materials", JaJp = "資材管理" },
            new { Key = "menu.logistics.materials.material", Module = "Frontend", ZhCn = "生产物料", EnUs = "Prod Material", JaJp = "生産資材" },
            new { Key = "menu.logistics.materials.model", Module = "Frontend", ZhCn = "机种仕向", EnUs = "Model", JaJp = "機種仕向" },
            new { Key = "menu.logistics.materials.packing", Module = "Frontend", ZhCn = "包装信息", EnUs = "Packing", JaJp = "包装情報" },
            new { Key = "menu.logistics.serials", Module = "Frontend", ZhCn = "序列号管理", EnUs = "Serial", JaJp = "シリアル管理" },
            new { Key = "menu.logistics.serials.inbound", Module = "Frontend", ZhCn = "序列号入库", EnUs = "Inbound", JaJp = "シリアル入庫" },
            new { Key = "menu.logistics.serials.outbound", Module = "Frontend", ZhCn = "序列号出库", EnUs = "Outbound", JaJp = "シリアル出庫" },
            new { Key = "menu.logistics.serials.scanning", Module = "Frontend", ZhCn = "序列号扫描", EnUs = "Scanning", JaJp = "シリアルスキャン" },
            new { Key = "menu.logistics.visits", Module = "Frontend", ZhCn = "客户来访", EnUs = "Customer Visits", JaJp = "顧客訪問" },
            new { Key = "menu.logistics.visits.management", Module = "Frontend", ZhCn = "来访信息", EnUs = "Visiting Information", JaJp = "訪問情報" },
            new { Key = "menu.logistics.visits.welcomesign", Module = "Frontend", ZhCn = "欢迎标识", EnUs = "Welcome Sign", JaJp = "ウェルカムサイン" },
            new { Key = "menu.logistics.reports", Module = "Frontend", ZhCn = "报表管理", EnUs = "Report", JaJp = "レポート" },
            new { Key = "menu.logistics.reports.export", Module = "Frontend", ZhCn = "报表导出", EnUs = "Export", JaJp = "エクスポート" },
            new { Key = "menu.logistics.reports.import", Module = "Frontend", ZhCn = "报表导入", EnUs = "Import", JaJp = "インポート" },
            new { Key = "menu.routine.localization", Module = "Frontend", ZhCn = "本地化", EnUs = "Localization", JaJp = "現地化" },
            new { Key = "menu.routine.dictionary", Module = "Frontend", ZhCn = "字典", EnUs = "Dictionary", JaJp = "辞書" },
            new { Key = "menu.routine.setting", Module = "Frontend", ZhCn = "系统设置", EnUs = "Settings", JaJp = "システム設定" },
            new { Key = "menu.routine.quartz.job", Module = "Frontend", ZhCn = "任务管理", EnUs = "QuartzJob", JaJp = "タスク管理" },
            new { Key = "menu.identity.user", Module = "Frontend", ZhCn = "用户管理", EnUs = "User", JaJp = "ユーザー管理" },
            new { Key = "menu.identity.role", Module = "Frontend", ZhCn = "角色管理", EnUs = "Role", JaJp = "ロール管理" },
            new { Key = "menu.identity.menu", Module = "Frontend", ZhCn = "菜单管理", EnUs = "Menu", JaJp = "メニュー管理" },
            new { Key = "menu.logging.login", Module = "Frontend", ZhCn = "登录日志", EnUs = "Login", JaJp = "ログインログ" },
            new { Key = "menu.logging.oper", Module = "Frontend", ZhCn = "操作日志", EnUs = "Oper", JaJp = "操作ログ" },
            new { Key = "menu.logging.diff", Module = "Frontend", ZhCn = "差异日志", EnUs = "Diff", JaJp = "差分ログ" },
            new { Key = "menu.logging.quartz.log", Module = "Frontend", ZhCn = "任务日志", EnUs = "QuartzJob", JaJp = "タスクログ" },
            new { Key = "menu.generator", Module = "Frontend", ZhCn = "代码管理", EnUs = "Code", JaJp = "コード管理" },
            new { Key = "menu.generator.code", Module = "Frontend", ZhCn = "代码生成", EnUs = "Generator", JaJp = "コード生成" },
            new { Key = "menu.settings", Module = "Frontend", ZhCn = "设置", EnUs = "Settings", JaJp = "設定" },

            // 欢迎标识功能相关翻译
            new { Key = "logistics.welcomesign.header", Module = "Frontend", ZhCn = "热烈欢迎", EnUs = "Warm Welcome", JaJp = "心より歓迎いたします" },
            new { Key = "logistics.welcomesign.footer", Module = "Frontend", ZhCn = "莅临指导", EnUs = "Welcome Your Visit", JaJp = "ご来訪ありがとうございます" },

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
