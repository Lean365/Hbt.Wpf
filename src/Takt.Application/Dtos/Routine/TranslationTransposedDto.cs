// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Routine
// 文件名称：TranslationTransposedDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：转置后的翻译数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 转置后的翻译数据传输对象
/// 用于以转置方式显示翻译数据：翻译键作为行，语言代码作为列
/// </summary>
public class TranslationTransposedDto : INotifyPropertyChanged
{
    private string _translationKey = string.Empty;
    private string? _module;
    private string? _description;
    private int _orderNum;
    private Dictionary<string, string> _translationValues = new();
    private Dictionary<string, long> _translationIds = new();

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
    public string? Module
    {
        get => _module;
        set => SetProperty(ref _module, value);
    }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description
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
    public Dictionary<string, string> TranslationValues
    {
        get => _translationValues;
        set => SetProperty(ref _translationValues, value);
    }

    /// <summary>
    /// 各语言的翻译ID（用于更新操作）
    /// Key: 语言代码
    /// Value: Translation实体ID
    /// </summary>
    public Dictionary<string, long> TranslationIds
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

