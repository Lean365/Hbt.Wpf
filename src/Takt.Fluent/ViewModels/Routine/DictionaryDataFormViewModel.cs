// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：DictionaryDataFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典数据表单视图模型（新建/编辑）
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
/// 字典数据表单视图模型
/// </summary>
public partial class DictionaryDataFormViewModel : ObservableObject
{
    private readonly IDictionaryDataService _dictionaryDataService;
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
    private string _dataLabel = string.Empty;

    [ObservableProperty]
    private string _i18nKey = string.Empty;

    [ObservableProperty]
    private string? _dataValue;

    [ObservableProperty]
    private string? _extLabel;

    [ObservableProperty]
    private string? _extValue;

    [ObservableProperty]
    private string? _cssClass;

    [ObservableProperty]
    private string? _listClass;

    [ObservableProperty]
    private int _orderNum;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    // 错误消息属性
    [ObservableProperty]
    private string _typeCodeError = string.Empty;

    [ObservableProperty]
    private string _dataLabelError = string.Empty;

    [ObservableProperty]
    private string _i18nKeyError = string.Empty;

    [ObservableProperty]
    private string _dataValueError = string.Empty;

    [ObservableProperty]
    private string _orderNumError = string.Empty;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public DictionaryDataFormViewModel(
        IDictionaryDataService dictionaryDataService,
        IDictionaryTypeService dictionaryTypeService,
        ILocalizationManager localizationManager)
    {
        _dictionaryDataService = dictionaryDataService ?? throw new ArgumentNullException(nameof(dictionaryDataService));
        _dictionaryTypeService = dictionaryTypeService ?? throw new ArgumentNullException(nameof(dictionaryTypeService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }

    /// <summary>
    /// 初始化创建模式
    /// </summary>
    public void ForCreate(string typeCode)
    {
        IsCreate = true;
        Title = _localizationManager.GetString("Routine.Dictionary.CreateData") ?? "新建字典数据";
        TypeCode = typeCode;
        DataLabel = string.Empty;
        I18nKey = string.Empty;
        DataValue = null;
        ExtLabel = null;
        ExtValue = null;
        CssClass = null;
        ListClass = null;
        OrderNum = 0;
        Remarks = null;
    }

    /// <summary>
    /// 初始化编辑模式
    /// </summary>
    public void ForUpdate(DictionaryDataDto dto)
    {
        IsCreate = false;
        Title = _localizationManager.GetString("Routine.Dictionary.UpdateData") ?? "编辑字典数据";
        Id = dto.Id;
        TypeCode = dto.TypeCode;
        DataLabel = dto.DataLabel;
        I18nKey = dto.I18nKey;
        DataValue = dto.DataValue;
        ExtLabel = dto.ExtLabel;
        ExtValue = dto.ExtValue;
        CssClass = dto.CssClass;
        ListClass = dto.ListClass;
        OrderNum = dto.OrderNum;
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        TypeCodeError = string.Empty;
        DataLabelError = string.Empty;
        I18nKeyError = string.Empty;
        DataValueError = string.Empty;
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

        // 验证类型代码（必填）
        if (string.IsNullOrWhiteSpace(TypeCode))
        {
            TypeCodeError = _localizationManager.GetString("Routine.Dictionary.Validation.TypeCodeRequired") ?? "类型代码不能为空";
            isValid = false;
        }

        // 验证数据标签（必填）
        if (string.IsNullOrWhiteSpace(DataLabel))
        {
            DataLabelError = _localizationManager.GetString("Routine.Dictionary.Validation.DataLabelRequired") ?? "数据标签不能为空";
            isValid = false;
        }
        else if (DataLabel.Length > 100)
        {
            DataLabelError = _localizationManager.GetString("Routine.Dictionary.Validation.DataLabelMaxLength") ?? "数据标签长度不能超过100个字符";
            isValid = false;
        }

        // 验证国际化键（必填）
        if (string.IsNullOrWhiteSpace(I18nKey))
        {
            I18nKeyError = _localizationManager.GetString("Routine.Dictionary.Validation.I18nKeyRequired") ?? "国际化键不能为空";
            isValid = false;
        }
        else if (I18nKey.Length > 64)
        {
            I18nKeyError = _localizationManager.GetString("Routine.Dictionary.Validation.I18nKeyMaxLength") ?? "国际化键长度不能超过64个字符";
            isValid = false;
        }

        // 验证数据值（可选，但如果填写则不能超过500个字符）
        if (!string.IsNullOrWhiteSpace(DataValue) && DataValue.Length > 500)
        {
            DataValueError = _localizationManager.GetString("Routine.Dictionary.Validation.DataValueMaxLength") ?? "数据值长度不能超过500个字符";
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
                var dto = new DictionaryDataCreateDto
                {
                    TypeCode = TypeCode.Trim(),
                    DataLabel = DataLabel.Trim(),
                    I18nKey = I18nKey.Trim(),
                    DataValue = string.IsNullOrWhiteSpace(DataValue) ? null : DataValue.Trim(),
                    ExtLabel = string.IsNullOrWhiteSpace(ExtLabel) ? null : ExtLabel.Trim(),
                    ExtValue = string.IsNullOrWhiteSpace(ExtValue) ? null : ExtValue.Trim(),
                    CssClass = string.IsNullOrWhiteSpace(CssClass) ? null : CssClass.Trim(),
                    ListClass = string.IsNullOrWhiteSpace(ListClass) ? null : ListClass.Trim(),
                    OrderNum = OrderNum
                };

                var result = await _dictionaryDataService.CreateAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? _localizationManager.GetString("common.saveFailed") ?? "保存失败";
                    return;
                }
            }
            else
            {
                var dto = new DictionaryDataUpdateDto
                {
                    Id = Id,
                    TypeCode = TypeCode.Trim(),
                    DataLabel = DataLabel.Trim(),
                    I18nKey = I18nKey.Trim(),
                    DataValue = string.IsNullOrWhiteSpace(DataValue) ? null : DataValue.Trim(),
                    ExtLabel = string.IsNullOrWhiteSpace(ExtLabel) ? null : ExtLabel.Trim(),
                    ExtValue = string.IsNullOrWhiteSpace(ExtValue) ? null : ExtValue.Trim(),
                    CssClass = string.IsNullOrWhiteSpace(CssClass) ? null : CssClass.Trim(),
                    ListClass = string.IsNullOrWhiteSpace(ListClass) ? null : ListClass.Trim(),
                    OrderNum = OrderNum
                };

                var result = await _dictionaryDataService.UpdateAsync(dto);
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

