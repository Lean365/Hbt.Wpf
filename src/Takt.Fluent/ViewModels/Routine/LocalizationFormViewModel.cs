// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：LocalizationFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：翻译表单视图模型（新建/编辑）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 翻译表单视图模型
/// </summary>
public partial class LocalizationFormViewModel : ObservableObject
{
    private readonly ITranslationService _translationService;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationManager _localizationManager;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private string _languageCode = string.Empty;

    [ObservableProperty]
    private string _translationKey = string.Empty;

    [ObservableProperty]
    private string _translationValue = string.Empty;

    [ObservableProperty]
    private string? _module;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private int _orderNum;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    // 语言选项列表
    public ObservableCollection<LanguageOptionDto> Languages { get; } = new();

    // 错误消息属性
    [ObservableProperty]
    private string _languageCodeError = string.Empty;

    [ObservableProperty]
    private string _translationKeyError = string.Empty;

    [ObservableProperty]
    private string _translationValueError = string.Empty;

    [ObservableProperty]
    private string _moduleError = string.Empty;

    [ObservableProperty]
    private string _orderNumError = string.Empty;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public LocalizationFormViewModel(
        ITranslationService translationService,
        ILanguageService languageService,
        ILocalizationManager localizationManager)
    {
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        _ = LoadLanguagesAsync();
    }

    /// <summary>
    /// 加载语言列表
    /// </summary>
    private async Task LoadLanguagesAsync()
    {
        try
        {
            var result = await _languageService.OptionAsync(false);
            if (result.Success && result.Data != null)
            {
                Languages.Clear();
                foreach (var lang in result.Data)
                {
                    Languages.Add(lang);
                }
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    /// <summary>
    /// 初始化创建模式
    /// </summary>
    public void ForCreate()
    {
        IsCreate = true;
        Title = _localizationManager.GetString("Routine.Localization.Create") ?? "新建翻译";
        LanguageCode = string.Empty;
        TranslationKey = string.Empty;
        TranslationValue = string.Empty;
        Module = null;
        Description = null;
        OrderNum = 0;
        Remarks = null;
    }

    /// <summary>
    /// 初始化编辑模式
    /// </summary>
    public void ForUpdate(TranslationDto dto)
    {
        IsCreate = false;
        Title = _localizationManager.GetString("Routine.Localization.Update") ?? "编辑翻译";
        Id = dto.Id;
        LanguageCode = dto.LanguageCode;
        TranslationKey = dto.TranslationKey;
        TranslationValue = dto.TranslationValue;
        Module = dto.Module;
        Description = dto.Description;
        OrderNum = dto.OrderNum;
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        LanguageCodeError = string.Empty;
        TranslationKeyError = string.Empty;
        TranslationValueError = string.Empty;
        ModuleError = string.Empty;
        OrderNumError = string.Empty;
        Error = string.Empty;
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        // 验证语言代码（必填）
        if (string.IsNullOrWhiteSpace(LanguageCode))
        {
            LanguageCodeError = _localizationManager.GetString("Routine.Localization.Validation.LanguageCodeRequired") ?? "语言代码不能为空";
            isValid = false;
        }
        else if (!Languages.Any(l => l.Code == LanguageCode))
        {
            LanguageCodeError = _localizationManager.GetString("Routine.Localization.Validation.LanguageCodeInvalid") ?? "语言代码无效";
            isValid = false;
        }

        // 验证翻译键（必填）
        if (string.IsNullOrWhiteSpace(TranslationKey))
        {
            TranslationKeyError = _localizationManager.GetString("Routine.Localization.Validation.TranslationKeyRequired") ?? "翻译键不能为空";
            isValid = false;
        }
        else if (TranslationKey.Length > 200)
        {
            TranslationKeyError = _localizationManager.GetString("Routine.Localization.Validation.TranslationKeyMaxLength") ?? "翻译键长度不能超过200个字符";
            isValid = false;
        }

        // 验证翻译值（必填）
        if (string.IsNullOrWhiteSpace(TranslationValue))
        {
            TranslationValueError = _localizationManager.GetString("Routine.Localization.Validation.TranslationValueRequired") ?? "翻译值不能为空";
            isValid = false;
        }

        // 验证模块（可选，但如果填写则不能超过50个字符）
        if (!string.IsNullOrWhiteSpace(Module) && Module.Length > 50)
        {
            ModuleError = _localizationManager.GetString("Routine.Localization.Validation.ModuleMaxLength") ?? "模块长度不能超过50个字符";
            isValid = false;
        }

        return isValid;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ClearAllErrors();

        try
        {
            // 验证所有字段
            if (!ValidateFields())
            {
                return;
            }

            if (IsCreate)
            {
                var dto = new TranslationCreateDto
                {
                    LanguageCode = LanguageCode.Trim(),
                    TranslationKey = TranslationKey.Trim(),
                    TranslationValue = TranslationValue.Trim(),
                    Module = string.IsNullOrWhiteSpace(Module) ? null : Module.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    OrderNum = OrderNum
                };

                var result = await _translationService.CreateAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? _localizationManager.GetString("common.saveFailed") ?? "保存失败";
                    return;
                }
            }
            else
            {
                var dto = new TranslationUpdateDto
                {
                    Id = Id,
                    LanguageCode = LanguageCode.Trim(),
                    TranslationKey = TranslationKey.Trim(),
                    TranslationValue = TranslationValue.Trim(),
                    Module = string.IsNullOrWhiteSpace(Module) ? null : Module.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    OrderNum = OrderNum
                };

                var result = await _translationService.UpdateAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? _localizationManager.GetString("common.saveFailed") ?? "保存失败";
                    return;
                }
            }

            // 保存成功，触发回调关闭窗口
            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}

