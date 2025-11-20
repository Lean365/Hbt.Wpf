// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：SettingFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：系统设置表单视图模型（新建/编辑）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 系统设置表单视图模型
/// </summary>
public partial class SettingFormViewModel : ObservableObject
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationManager _localizationManager;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private string _settingKey = string.Empty;

    [ObservableProperty]
    private string _settingValue = string.Empty;

    [ObservableProperty]
    private string? _category;

    [ObservableProperty]
    private int _orderNum;

    [ObservableProperty]
    private string? _settingDescription;

    [ObservableProperty]
    private int _settingType;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    // 错误消息属性
    [ObservableProperty]
    private string _settingKeyError = string.Empty;

    [ObservableProperty]
    private string _settingValueError = string.Empty;

    [ObservableProperty]
    private string _categoryError = string.Empty;

    [ObservableProperty]
    private string _orderNumError = string.Empty;

    [ObservableProperty]
    private string _settingTypeError = string.Empty;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public SettingFormViewModel(ISettingService settingService, ILocalizationManager localizationManager)
    {
        _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }

    /// <summary>
    /// 初始化创建模式
    /// </summary>
    public void ForCreate()
    {
        IsCreate = true;
        Title = _localizationManager.GetString("Routine.Setting.Create") ?? "新建系统设置";
        SettingType = 0; // 默认字符串类型
        OrderNum = 0;
    }

    /// <summary>
    /// 初始化编辑模式
    /// </summary>
    public void ForUpdate(SettingDto dto)
    {
        IsCreate = false;
        Title = _localizationManager.GetString("Routine.Setting.Update") ?? "编辑系统设置";
        Id = dto.Id;
        SettingKey = dto.SettingKey;
        SettingValue = dto.SettingValue;
        Category = dto.Category;
        OrderNum = dto.OrderNum;
        SettingDescription = dto.SettingDescription;
        SettingType = dto.SettingType;
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        SettingKeyError = string.Empty;
        SettingValueError = string.Empty;
        CategoryError = string.Empty;
        OrderNumError = string.Empty;
        SettingTypeError = string.Empty;
        Error = string.Empty;
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        // 验证设置键（必填）
        if (string.IsNullOrWhiteSpace(SettingKey))
        {
            SettingKeyError = _localizationManager.GetString("Routine.Setting.Validation.KeyRequired") ?? "设置键不能为空";
            isValid = false;
        }
        else if (SettingKey.Length > 100)
        {
            SettingKeyError = _localizationManager.GetString("Routine.Setting.Validation.KeyMaxLength") ?? "设置键长度不能超过100个字符";
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(SettingKey, @"^[a-zA-Z0-9._-]+$"))
        {
            SettingKeyError = _localizationManager.GetString("Routine.Setting.Validation.KeyInvalid") ?? "设置键只能包含字母、数字、点号、下划线和连字符";
            isValid = false;
        }

        // 验证设置值（必填）
        if (string.IsNullOrWhiteSpace(SettingValue))
        {
            SettingValueError = _localizationManager.GetString("Routine.Setting.Validation.ValueRequired") ?? "设置值不能为空";
            isValid = false;
        }
        else if (SettingValue.Length > 2000)
        {
            SettingValueError = _localizationManager.GetString("Routine.Setting.Validation.ValueMaxLength") ?? "设置值长度不能超过2000个字符";
            isValid = false;
        }

        // 验证分类（可选，但如果填写则不能超过50个字符）
        if (!string.IsNullOrWhiteSpace(Category) && Category.Length > 50)
        {
            CategoryError = _localizationManager.GetString("Routine.Setting.Validation.CategoryMaxLength") ?? "分类长度不能超过50个字符";
            isValid = false;
        }

        // 验证设置描述（可选，但如果填写则不能超过500个字符）
        if (!string.IsNullOrWhiteSpace(SettingDescription) && SettingDescription.Length > 500)
        {
            // 使用 SettingValueError 来显示描述错误（因为描述字段没有单独的错误属性）
            SettingValueError = _localizationManager.GetString("Routine.Setting.Validation.DescriptionMaxLength") ?? "设置描述长度不能超过500个字符";
            isValid = false;
        }

        // 验证设置类型（0=字符串, 1=数字, 2=布尔值, 3=JSON）
        if (SettingType < 0 || SettingType > 3)
        {
            SettingTypeError = _localizationManager.GetString("Routine.Setting.Validation.TypeInvalid") ?? "设置类型无效，必须是0-3之间的值";
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
                var dto = new SettingCreateDto
                {
                    SettingKey = SettingKey.Trim(),
                    SettingValue = SettingValue.Trim(),
                    Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim(),
                    OrderNum = OrderNum,
                    SettingDescription = string.IsNullOrWhiteSpace(SettingDescription) ? null : SettingDescription.Trim(),
                    SettingType = SettingType
                };

                var result = await _settingService.CreateAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? _localizationManager.GetString("common.saveFailed") ?? "保存失败";
                    return;
                }
            }
            else
            {
                var dto = new SettingUpdateDto
                {
                    Id = Id,
                    SettingKey = SettingKey.Trim(),
                    SettingValue = SettingValue.Trim(),
                    Category = string.IsNullOrWhiteSpace(Category) ? null : Category.Trim(),
                    OrderNum = OrderNum,
                    SettingDescription = string.IsNullOrWhiteSpace(SettingDescription) ? null : SettingDescription.Trim(),
                    SettingType = SettingType
                };

                var result = await _settingService.UpdateAsync(dto);
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

