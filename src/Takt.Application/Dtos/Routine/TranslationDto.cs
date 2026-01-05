//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Dtos.Routine
// 文件名 : TranslationDto.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 功能描述：翻译数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 翻译数据传输对象
/// 用于传输翻译信息
/// </summary>
public class TranslationDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public TranslationDto()
    {
        LanguageCode = string.Empty;
        TranslationKey = string.Empty;
        TranslationValue = string.Empty;
        Module = string.Empty;
        Description = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 语言代码
    /// </summary>
    public string LanguageCode { get; set; }
    
    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; }
    
    /// <summary>
    /// 翻译值
    /// </summary>
    public string TranslationValue { get; set; }
    
    /// <summary>
    /// 模块
    /// </summary>
    public string Module { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
    
    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedBy { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdatedBy { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }
    
    /// <summary>
    /// 是否删除（0=否，1=是）
    /// </summary>
    public int IsDeleted { get; set; }
    
    /// <summary>
    /// 删除人
    /// </summary>
    public string DeletedBy { get; set; }
    
    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime DeletedTime { get; set; }
}

/// <summary>
/// 翻译查询数据传输对象
/// </summary>
public class TranslationQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public TranslationQueryDto()
    {
        Keywords = string.Empty;
        LanguageCode = string.Empty;
        TranslationKey = string.Empty;
        Module = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在翻译键、翻译值、模块中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 语言ID
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; }

    /// <summary>
    /// 模块
    /// </summary>
    public string Module { get; set; }
}

/// <summary>
/// 创建翻译数据传输对象
/// </summary>
public class TranslationCreateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public TranslationCreateDto()
    {
        LanguageCode = string.Empty;
        TranslationKey = string.Empty;
        TranslationValue = string.Empty;
        Module = string.Empty;
        Description = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 语言ID
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; }

    /// <summary>
    /// 翻译值
    /// </summary>
    public string TranslationValue { get; set; }

    /// <summary>
    /// 模块
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 更新翻译数据传输对象
/// </summary>
public class TranslationUpdateDto : TranslationCreateDto
{
    /// <summary>
    /// 翻译ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 翻译键信息数据传输对象
/// 用于主表显示翻译键列表
/// </summary>
public class TranslationKeyInfoDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public TranslationKeyInfoDto()
    {
        TranslationKey = string.Empty;
        Module = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; }

    /// <summary>
    /// 模块
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
}

/// <summary>
/// 翻译值项数据传输对象
/// 用于从表显示某个翻译键在所有语言下的翻译值
/// </summary>
public class TranslationValueItemDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public TranslationValueItemDto()
    {
        LanguageCode = string.Empty;
        LanguageName = string.Empty;
        TranslationValue = string.Empty;
    }

    /// <summary>
    /// 语言代码
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// 语言名称
    /// </summary>
    public string LanguageName { get; set; }

    /// <summary>
    /// 翻译值
    /// </summary>
    public string TranslationValue { get; set; }

    /// <summary>
    /// 翻译ID（如果存在）
    /// </summary>
    public long? TranslationId { get; set; }
}

/// <summary>
/// 转置后的翻译数据传输对象
/// 用于以转置方式显示翻译数据：翻译键作为行，语言代码作为列
/// </summary>
public class TranslationTransposedDto : System.ComponentModel.INotifyPropertyChanged
{
    private string _translationKey = string.Empty;
    private string _module = string.Empty;
    private string _description = string.Empty;
    private int _orderNum;
    private System.Collections.Generic.Dictionary<string, string> _translationValues = new();
    private System.Collections.Generic.Dictionary<string, long> _translationIds = new();

    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public TranslationTransposedDto()
    {
        _translationKey = string.Empty;
        _module = string.Empty;
        _description = string.Empty;
        _translationValues = new System.Collections.Generic.Dictionary<string, string>();
        _translationIds = new System.Collections.Generic.Dictionary<string, long>();
    }

    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey
    {
        get => _translationKey;
        set => SetProperty(ref _translationKey, value);
    }

    /// <summary>
    /// 模块
    /// </summary>
    public string Module
    {
        get => _module;
        set => SetProperty(ref _module, value);
    }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum
    {
        get => _orderNum;
        set => SetProperty(ref _orderNum, value);
    }

    /// <summary>
    /// 各语言的翻译值
    /// Key: 语言代码 (如: zh-CN, en-US)
    /// Value: 该语言下的翻译值
    /// </summary>
    public System.Collections.Generic.Dictionary<string, string> TranslationValues
    {
        get => _translationValues;
        set => SetProperty(ref _translationValues, value);
    }

    /// <summary>
    /// 各语言的翻译ID（用于更新操作）
    /// Key: 语言代码
    /// Value: Translation实体ID
    /// </summary>
    public System.Collections.Generic.Dictionary<string, long> TranslationIds
    {
        get => _translationIds;
        set => SetProperty(ref _translationIds, value);
    }

    /// <summary>
    /// 获取指定语言的翻译值
    /// </summary>
    public string GetTranslationValue(string languageCode)
    {
        return TranslationValues.TryGetValue(languageCode, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// 设置指定语言的翻译值
    /// </summary>
    public void SetTranslationValue(string languageCode, string value)
    {
        TranslationValues[languageCode] = value;
        OnPropertyChanged(nameof(TranslationValues));
        // 触发属性变更通知，以便UI更新
        OnPropertyChanged($"TranslationValues[{languageCode}]");
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
