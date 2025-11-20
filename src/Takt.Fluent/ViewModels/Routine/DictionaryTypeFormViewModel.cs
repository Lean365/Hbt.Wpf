// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：DictionaryTypeFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典类型表单视图模型（新建/编辑）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 字典类型表单视图模型
/// </summary>
public partial class DictionaryTypeFormViewModel : ObservableObject
{
    private readonly IDictionaryTypeService _dictionaryTypeService;
    private readonly ILocalizationManager _localizationManager;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private string _typeCode = string.Empty;

    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    private int _orderNum;

    [ObservableProperty]
    private int _typeStatus;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    // 错误消息属性
    [ObservableProperty]
    private string _typeCodeError = string.Empty;

    [ObservableProperty]
    private string _typeNameError = string.Empty;

    [ObservableProperty]
    private string _orderNumError = string.Empty;

    [ObservableProperty]
    private string _typeStatusError = string.Empty;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public DictionaryTypeFormViewModel(IDictionaryTypeService dictionaryTypeService, ILocalizationManager localizationManager)
    {
        _dictionaryTypeService = dictionaryTypeService ?? throw new ArgumentNullException(nameof(dictionaryTypeService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }

    /// <summary>
    /// 初始化创建模式
    /// </summary>
    public void ForCreate()
    {
        IsCreate = true;
        Title = _localizationManager.GetString("Routine.Dictionary.CreateType") ?? "新建字典类型";
        TypeCode = string.Empty;
        TypeName = string.Empty;
        OrderNum = 0;
        TypeStatus = 0; // 默认启用
        Remarks = null;
    }

    /// <summary>
    /// 初始化编辑模式
    /// </summary>
    public void ForUpdate(DictionaryTypeDto dto)
    {
        IsCreate = false;
        Title = _localizationManager.GetString("Routine.Dictionary.UpdateType") ?? "编辑字典类型";
        Id = dto.Id;
        TypeCode = dto.TypeCode;
        TypeName = dto.TypeName;
        OrderNum = dto.OrderNum;
        TypeStatus = dto.TypeStatus;
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        TypeCodeError = string.Empty;
        TypeNameError = string.Empty;
        OrderNumError = string.Empty;
        TypeStatusError = string.Empty;
        Error = string.Empty;
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        // 验证类型代码（必填）
        if (string.IsNullOrWhiteSpace(TypeCode))
        {
            TypeCodeError = _localizationManager.GetString("Routine.Dictionary.Validation.TypeCodeRequired") ?? "类型代码不能为空";
            isValid = false;
        }
        else if (TypeCode.Length > 50)
        {
            TypeCodeError = _localizationManager.GetString("Routine.Dictionary.Validation.TypeCodeMaxLength") ?? "类型代码长度不能超过50个字符";
            isValid = false;
        }

        // 验证类型名称（必填）
        if (string.IsNullOrWhiteSpace(TypeName))
        {
            TypeNameError = _localizationManager.GetString("Routine.Dictionary.Validation.TypeNameRequired") ?? "类型名称不能为空";
            isValid = false;
        }
        else if (TypeName.Length > 100)
        {
            TypeNameError = _localizationManager.GetString("Routine.Dictionary.Validation.TypeNameMaxLength") ?? "类型名称长度不能超过100个字符";
            isValid = false;
        }

        // 验证状态（0=启用，1=禁用）
        if (TypeStatus < 0 || TypeStatus > 1)
        {
            TypeStatusError = _localizationManager.GetString("Routine.Dictionary.Validation.TypeStatusInvalid") ?? "类型状态无效，必须是0或1";
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
                var dto = new DictionaryTypeCreateDto
                {
                    TypeCode = TypeCode.Trim(),
                    TypeName = TypeName.Trim(),
                    OrderNum = OrderNum
                };

                var result = await _dictionaryTypeService.CreateAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? _localizationManager.GetString("common.saveFailed") ?? "保存失败";
                    return;
                }
            }
            else
            {
                var dto = new DictionaryTypeUpdateDto
                {
                    Id = Id,
                    TypeCode = TypeCode.Trim(),
                    TypeName = TypeName.Trim(),
                    OrderNum = OrderNum,
                    TypeStatus = TypeStatus
                };

                var result = await _dictionaryTypeService.UpdateAsync(dto);
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

