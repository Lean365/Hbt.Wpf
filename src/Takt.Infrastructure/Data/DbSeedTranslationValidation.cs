//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : DbSeedTranslationValidation.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 验证消息翻译种子数据（仅保留特定业务规则，通用验证使用通用翻译拼接）
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
/// 验证消息翻译种子初始化器
/// 仅保留特定业务规则的验证消息，通用验证消息（如"xxx不能为空"、"请输入"、"请选择"）应使用通用翻译进行拼接
/// </summary>
public class DbSeedTranslationValidation
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<Language> _languageRepository;

    public DbSeedTranslationValidation(
        InitLogManager initLog,
        IBaseRepository<Translation> translationRepository,
        IBaseRepository<Language> languageRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _translationRepository = translationRepository ?? throw new ArgumentNullException(nameof(translationRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
    }

    /// <summary>
    /// 初始化验证消息翻译
    /// </summary>
    public void Initialize()
    {
        var languages = _languageRepository.AsQueryable().ToList();
        if (languages.Count == 0)
        {
            _initLog.Warning("语言数据为空，跳过验证消息翻译初始化");
            return;
        }

        var zhCn = languages.FirstOrDefault(l => l.LanguageCode == "zh-CN");
        var enUs = languages.FirstOrDefault(l => l.LanguageCode == "en-US");
        var jaJp = languages.FirstOrDefault(l => l.LanguageCode == "ja-JP");

        if (zhCn == null || enUs == null || jaJp == null)
        {
            _initLog.Warning("语言数据不完整（zh-CN/en-US/ja-JP 缺失），跳过验证消息翻译初始化");
            return;
        }

        foreach (var seed in BuildValidationSeeds())
        {
            CreateOrUpdateTranslation(zhCn, seed.Key.ToLower(), seed.Module, seed.ZhCn);
            CreateOrUpdateTranslation(enUs, seed.Key.ToLower(), seed.Module, seed.EnUs);
            CreateOrUpdateTranslation(jaJp, seed.Key.ToLower(), seed.Module, seed.JaJp);
        }

        _initLog.Information("✅ 验证消息翻译初始化完成");
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

    private static List<ValidationSeed> BuildValidationSeeds()
    {
        return new List<ValidationSeed>
        {
            // 用户验证相关（仅保留特定业务规则，通用验证使用 common.validation.required/format/maxlength/minlength/invalid）
            // 注意：以下验证消息已删除，应使用通用翻译拼接：
            // - "xxx不能为空" -> 使用 common.validation.required + "xxx"
            // - "请输入xxx" -> 使用 common.placeholder.input + "xxx"
            // - "请选择xxx" -> 使用 common.placeholder.select + "xxx"
            // - "xxx长度不能超过x个字符" -> 使用 common.validation.maxlength + "xxx" + 数字
            // - "xxx长度不能少于x位" -> 使用 common.validation.minlength + "xxx" + 数字
            new("identity.user.validation.usernameinvalid", "Frontend", "用户名必须以小写字母开头，只能包含小写字母和数字，长度4-10位", "Username must start with a lowercase letter and contain only lowercase letters and numbers, 4-10 characters", "ユーザー名は小文字で始まり、小文字と数字のみを含む必要があります（4-10文字）"),
            new("identity.user.validation.usernamepasswordrequired", "Frontend", "用户名和密码不能为空", "Username and password cannot be empty", "ユーザー名とパスワードは空にできません"),
            new("identity.user.validation.realnamehint", "Frontend", "不允许数字、点号、空格开头，英文字母首字母大写，30字以内", "Cannot start with digits, dots, or spaces. English letters must be uppercase. Max 30 characters", "数字、ドット、スペースで始めることはできません。英語の文字は大文字である必要があります。最大30文字"),
            new("identity.user.validation.realnameinvalid", "Frontend", "不允许数字、点号、空格开头，英文字母首字母必须大写", "Cannot start with digits, dots, or spaces. English letters must be uppercase", "数字、ドット、スペースで始めることはできません。英語の文字は大文字である必要があります"),
            new("identity.user.validation.nicknameinvalid", "Frontend", "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等，如：Cheng.Jianhong、Joseph Robinette Biden Jr. 或 张三", "Nickname cannot start with digits, dots, or spaces, if the first character is an English letter it must be uppercase, allow letters, numbers, dots, and spaces, support Chinese, Japanese, Korean, Vietnamese, etc., e.g., Cheng.Jianhong, Joseph Robinette Biden Jr. or 张三", "ニックネームは数字、ドット、スペースで始めることはできません。最初の文字が英語の文字の場合は大文字である必要があります。文字、数字、ドット、スペースを含むことができ、中国語、日本語、韓国語、ベトナム語などをサポートします。例：Cheng.Jianhong、Joseph Robinette Biden Jr. または 张三"),
            new("identity.user.validation.phoneinvalid", "Frontend", "必须是11位数字，以1开头，第二位为3-9", "Must be 11 digits starting with 1, second digit 3-9", "1で始まり、2桁目が3-9の11桁の数字である必要があります"),
            // 注意：identity.user.validation.avatarmustberelativepath 已删除
            // 应使用 common.validation.mustberelativepath + "头像" 拼接
            new("identity.user.validation.passwordmismatch", "Frontend", "两次输入的密码不一致", "The two passwords do not match", "2つのパスワードが一致しません"),
            new("identity.user.validation.passwordconfirminthint", "Frontend", "请再次输入密码以确认", "Please enter password again to confirm", "確認のため、パスワードを再度入力してください"),
            // 注意：identity.user.validation.avatarhint 已删除
            // "请选择{0}" 部分应使用 common.placeholder.select + "头像"
            // "必须是相对路径" 的规则已在 identity.user.validation.avatarmustberelativepath 中说明

            // 角色验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接
            new("identity.role.validation.rolecodeinvalid", "Frontend", "只能包含小写字母、数字和下划线", "Can only contain lowercase letters, numbers, and underscores", "小文字、数字、アンダースコアのみを含むことができます"),
            // 注意：identity.role.validation.ordernumhint 已删除，使用 common.placeholder.input + "排序号" 拼接

            // 菜单验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接
            new("identity.menu.validation.menucodehint", "Frontend", "建议使用小写字母和下划线", "Lowercase letters and underscores recommended", "小文字とアンダースコアを推奨します"),
            new("identity.menu.validation.parentidhint", "Frontend", "父级菜单ID（0表示顶级菜单，留空表示0）", "Parent menu ID (0 for top-level menu, empty means 0)", "親メニューID（0はトップレベルメニュー、空欄は0を意味します）"),
            // 注意：identity.menu.validation.ordernumhint 已删除，使用 common.placeholder.input + "排序号" 拼接

            // 字典验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接
            new("routine.dictionary.validation.typecodehint", "Frontend", "格式：xxx_xxx_xxx，只包含小写字母和数字，且不能以数字开头", "Format: xxx_xxx_xxx, only lowercase letters and numbers, cannot start with a number", "形式：xxx_xxx_xxx、小文字と数字のみ、数字で始めることはできません"),
            new("routine.dictionary.validation.sqlscripthint", "Frontend", "当数据源为SQL时", "When data source is SQL", "データソースがSQLの場合"),
            // 注意：routine.dictionary.validation.ordernumhint 已删除，使用 common.placeholder.input + "排序号" 拼接
            new("routine.dictionary.validation.typecodeformat", "Frontend", "必须是 xxx_xxx_xxx 格式，只包含小写字母和数字，且不能以数字开头", "Must be xxx_xxx_xxx format, only lowercase letters and numbers, and cannot start with a number", "xxx_xxx_xxx形式である必要があり、小文字と数字のみを含み、数字で始めることはできません"),
            // 注意：routine.dictionary.validation.typestatusinvalid 已删除，状态字段默认只有0或1两个值，不需要额外提示

            // 设置验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接
            new("routine.setting.validation.keyhint", "Frontend", "建议使用小写字母、数字和下划线", "Lowercase letters, numbers and underscores recommended", "小文字、数字、アンダースコアを推奨します"),

            // 翻译验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接

            // 序列号出库验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接

            // 序列号入库验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接

            // 来访公司验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接
            new("logistics.visits.visitingcompany.validation.starttimemustbeforeendtime", "Frontend", "开始时间必须早于结束时间", "Start time must be earlier than end time", "開始時間は終了時間より早い必要があります"),
            new("logistics.visits.visitingcompany.validation.endtimemustafterstarttime", "Frontend", "结束时间必须晚于开始时间", "End time must be later than start time", "終了時間は開始時間より遅い必要があります"),
            new("logistics.visits.visitingcompany.validation.starttimehint", "Frontend", "必须早于结束时间", "Must be earlier than end time", "終了時間より早い必要があります"),
            new("logistics.visits.visitingcompany.validation.endtimehint", "Frontend", "必须晚于开始时间", "Must be later than start time", "開始時間より遅い必要があります"),
            new("logistics.visits.visitingcompany.validation.timelengthrequired", "Frontend", "结束时间必须比开始时间大1小时以上", "End time must be at least 1 hour later than start time", "終了時間は開始時間より1時間以上遅い必要があります"),

            // 来访成员验证相关
            // 注意：简单的"请输入"已删除，使用通用翻译拼接

            // 代码生成表验证相关
            // 注意：简单的"不能为空"、"请输入"、"请选择"已删除，使用通用翻译拼接
            new("generator.gentable.validation.tablenamehint", "Frontend", "用于无数据表的手动配置时表名可以不存在于数据库中", "For manual configuration without database table, table name may not exist in database", "データベーステーブルなしの手動設定用の場合、テーブル名はデータベースに存在しない場合があります"),

            // 代码生成列验证相关
            // 注意：简单的"请输入"、"请选择"已删除，使用通用翻译拼接
            // generator.gencolumn.validation.ordernumhint 已删除，直接使用 common.placeholder.input + "{0}"
        };
    }

    private sealed record ValidationSeed(string Key, string Module, string ZhCn, string EnUs, string JaJp);
}

