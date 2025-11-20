//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : MenuFormViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 菜单表单视图模型（新建/更新）
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Enums;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 菜单表单视图模型
/// </summary>
public partial class MenuFormViewModel : ObservableObject
{
    private readonly IMenuService _menuService;
    private readonly ILocalizationManager _localizationManager;

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private bool _isCreate = true;
    [ObservableProperty] private string _menuName = "";
    [ObservableProperty] private string _menuCode = "";
    [ObservableProperty] private string? _i18nKey;
    [ObservableProperty] private string? _permCode;
    [ObservableProperty] private int _menuType = (int)MenuTypeEnum.Directory;
    [ObservableProperty] private string _parentId = "0";
    [ObservableProperty] private string? _routePath;
    [ObservableProperty] private string? _icon;
    [ObservableProperty] private string? _component;
    [ObservableProperty] private int _isExternal = (int)ExternalEnum.NotExternal;
    [ObservableProperty] private int _isCache = (int)CacheEnum.NoCache;
    [ObservableProperty] private int _isVisible = (int)VisibilityEnum.Visible;
    [ObservableProperty] private int _orderNum = 0;
    [ObservableProperty] private int _menuStatus = (int)StatusEnum.Normal;
    [ObservableProperty] private string? _remarks;
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _error = string.Empty;
    
    // Hint 提示属性
    public string MenuNameHint => GetTranslation("Identity.Menu.Validation.MenuNameInvalid", "菜单名称不能为空，长度不能超过20个字符");
    
    public string MenuCodeHint => GetTranslation("Identity.Menu.Validation.MenuCodeInvalid", "菜单编码不能为空，长度不能超过10个字符，只能包含小写字母、数字和下划线");
    
    public string I18nKeyHint => GetTranslation("Identity.Menu.Validation.I18nKeyInvalid", "国际化键长度不能超过64个字符");
    
    public string PermCodeHint => GetTranslation("Identity.Menu.Validation.PermCodeInvalid", "权限码长度不能超过100个字符");
    
    // 错误消息属性
    [ObservableProperty] private string _menuNameError = string.Empty;
    [ObservableProperty] private string _menuCodeError = string.Empty;
    [ObservableProperty] private string _i18nKeyError = string.Empty;
    [ObservableProperty] private string _permCodeError = string.Empty;
    [ObservableProperty] private string _menuTypeError = string.Empty;
    [ObservableProperty] private string _parentIdError = string.Empty;
    [ObservableProperty] private string _routePathError = string.Empty;
    [ObservableProperty] private string _iconError = string.Empty;
    [ObservableProperty] private string _componentError = string.Empty;
    [ObservableProperty] private string _isExternalError = string.Empty;
    [ObservableProperty] private string _isCacheError = string.Empty;
    [ObservableProperty] private string _isVisibleError = string.Empty;
    [ObservableProperty] private string _orderNumError = string.Empty;
    [ObservableProperty] private string _menuStatusError = string.Empty;
    [ObservableProperty] private string _remarksError = string.Empty;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public MenuFormViewModel(IMenuService menuService, ILocalizationManager localizationManager)
    {
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }

    /// <summary>
    /// 获取翻译文本（如果找不到翻译，返回默认值）
    /// </summary>
    private string GetTranslation(string key, string defaultValue)
    {
        var translation = _localizationManager.GetString(key);
        return (translation == key) ? defaultValue : translation;
    }

    public void ForCreate()
    {
        ClearAllErrors();
        
        IsCreate = true;
        Title = GetTranslation("Identity.Menu.Create", "新建菜单");
        MenuName = string.Empty;
        MenuCode = string.Empty;
        I18nKey = null;
        PermCode = null;
        MenuType = (int)MenuTypeEnum.Directory;
        ParentId = "0";
        RoutePath = null;
        Icon = null;
        Component = null;
        IsExternal = (int)ExternalEnum.NotExternal;
        IsCache = (int)CacheEnum.NoCache;
        IsVisible = (int)VisibilityEnum.Visible;
        OrderNum = 0;
        MenuStatus = (int)StatusEnum.Normal;
        Remarks = null;
    }

    public void ForUpdate(MenuDto dto)
    {
        ClearAllErrors();
        
        IsCreate = false;
        Title = GetTranslation("Identity.Menu.Update", "编辑菜单");
        Id = dto.Id;
        MenuName = dto.MenuName ?? string.Empty;
        MenuCode = dto.MenuCode ?? string.Empty;
        I18nKey = dto.I18nKey;
        PermCode = dto.PermCode;
        MenuType = (int)dto.MenuType;
        ParentId = dto.ParentId?.ToString() ?? "0";
        RoutePath = dto.RoutePath;
        Icon = dto.Icon;
        Component = dto.Component;
        IsExternal = (int)dto.IsExternal;
        IsCache = (int)dto.IsCache;
        IsVisible = (int)dto.IsVisible;
        OrderNum = dto.OrderNum;
        MenuStatus = (int)dto.MenuStatus;
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        MenuNameError = string.Empty;
        MenuCodeError = string.Empty;
        I18nKeyError = string.Empty;
        PermCodeError = string.Empty;
        MenuTypeError = string.Empty;
        ParentIdError = string.Empty;
        RoutePathError = string.Empty;
        IconError = string.Empty;
        ComponentError = string.Empty;
        IsExternalError = string.Empty;
        IsCacheError = string.Empty;
        IsVisibleError = string.Empty;
        OrderNumError = string.Empty;
        MenuStatusError = string.Empty;
        RemarksError = string.Empty;
        Error = string.Empty;
    }

    // 属性变更时进行实时验证
    partial void OnMenuNameChanged(string value)
    {
        ValidateMenuName();
    }

    partial void OnMenuCodeChanged(string value)
    {
        ValidateMenuCode();
    }

    partial void OnI18nKeyChanged(string? value)
    {
        ValidateI18nKey();
    }

    partial void OnPermCodeChanged(string? value)
    {
        ValidatePermCode();
    }

    /// <summary>
    /// 验证菜单名称（实时验证）
    /// </summary>
    private void ValidateMenuName()
    {
        if (string.IsNullOrWhiteSpace(MenuName))
        {
            MenuNameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (MenuName.Length > 20)
        {
            MenuNameError = GetTranslation("Identity.Menu.Validation.MenuNameMaxLength", "菜单名称长度不能超过20个字符");
        }
        else
        {
            MenuNameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证菜单编码（实时验证）
    /// </summary>
    private void ValidateMenuCode()
    {
        if (string.IsNullOrWhiteSpace(MenuCode))
        {
            MenuCodeError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (MenuCode.Length > 10)
        {
            MenuCodeError = GetTranslation("Identity.Menu.Validation.MenuCodeMaxLength", "菜单编码长度不能超过10个字符");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(MenuCode, @"^[a-z0-9_]+$"))
        {
            MenuCodeError = GetTranslation("Identity.Menu.Validation.MenuCodeInvalid", "菜单编码只能包含小写字母、数字和下划线");
        }
        else
        {
            MenuCodeError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证国际化键（实时验证）
    /// </summary>
    private void ValidateI18nKey()
    {
        if (string.IsNullOrWhiteSpace(I18nKey))
        {
            I18nKeyError = string.Empty;
            return;
        }

        if (I18nKey.Length > 64)
        {
            I18nKeyError = GetTranslation("Identity.Menu.Validation.I18nKeyMaxLength", "国际化键长度不能超过64个字符");
        }
        else
        {
            I18nKeyError = string.Empty;
        }
    }

    /// <summary>
    /// 验证权限码（实时验证）
    /// </summary>
    private void ValidatePermCode()
    {
        if (string.IsNullOrWhiteSpace(PermCode))
        {
            PermCodeError = string.Empty;
            return;
        }

        if (PermCode.Length > 100)
        {
            PermCodeError = GetTranslation("Identity.Menu.Validation.PermCodeMaxLength", "权限码长度不能超过100个字符");
        }
        else
        {
            PermCodeError = string.Empty;
        }
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields()
    {
        ClearAllErrors();
        bool isValid = true;

        // 验证菜单名称（必填）
        if (string.IsNullOrWhiteSpace(MenuName))
        {
            MenuNameError = GetTranslation("Identity.Menu.Validation.MenuNameRequired", "菜单名称不能为空");
            isValid = false;
        }
        else if (MenuName.Length > 20)
        {
            MenuNameError = GetTranslation("Identity.Menu.Validation.MenuNameMaxLength", "菜单名称长度不能超过20个字符");
            isValid = false;
        }

        // 验证菜单编码（必填）
        if (string.IsNullOrWhiteSpace(MenuCode))
        {
            MenuCodeError = GetTranslation("Identity.Menu.Validation.MenuCodeRequired", "菜单编码不能为空");
            isValid = false;
        }
        else if (MenuCode.Length > 10)
        {
            MenuCodeError = GetTranslation("Identity.Menu.Validation.MenuCodeMaxLength", "菜单编码长度不能超过10个字符");
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(MenuCode, @"^[a-z0-9_]+$"))
        {
            MenuCodeError = GetTranslation("Identity.Menu.Validation.MenuCodeInvalid", "菜单编码只能包含小写字母、数字和下划线");
            isValid = false;
        }

        // 验证国际化键（可选，但如果填写则不能超过64个字符）
        if (!string.IsNullOrWhiteSpace(I18nKey) && I18nKey.Length > 64)
        {
            I18nKeyError = GetTranslation("Identity.Menu.Validation.I18nKeyMaxLength", "国际化键长度不能超过64个字符");
            isValid = false;
        }

        // 验证权限码（可选，但如果填写则不能超过100个字符）
        if (!string.IsNullOrWhiteSpace(PermCode) && PermCode.Length > 100)
        {
            PermCodeError = GetTranslation("Identity.Menu.Validation.PermCodeMaxLength", "权限码长度不能超过100个字符");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 保存菜单（新建或更新）
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // 验证字段
            if (!ValidateFields())
            {
                return;
            }

            if (IsCreate)
            {
                var dto = new MenuCreateDto
                {
                    MenuName = MenuName,
                    MenuCode = MenuCode,
                    I18nKey = I18nKey,
                    PermCode = PermCode,
                    MenuType = (MenuTypeEnum)MenuType,
                    ParentId = long.TryParse(ParentId, out var parentIdValue) ? parentIdValue : 0,
                    RoutePath = RoutePath,
                    Icon = Icon,
                    Component = Component,
                    IsExternal = (ExternalEnum)IsExternal,
                    IsCache = (CacheEnum)IsCache,
                    IsVisible = (VisibilityEnum)IsVisible,
                    OrderNum = OrderNum,
                    MenuStatus = (StatusEnum)MenuStatus,
                    Remarks = Remarks
                };

                var result = await _menuService.CreateMenuAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? GetTranslation("Identity.Menu.CreateFailed", "创建菜单失败");
                    return;
                }

                SaveSuccessCallback?.Invoke();
            }
            else
            {
                var dto = new MenuUpdateDto
                {
                    Id = Id,
                    MenuName = MenuName,
                    MenuCode = MenuCode,
                    I18nKey = I18nKey,
                    PermCode = PermCode,
                    MenuType = (MenuTypeEnum)MenuType,
                    ParentId = long.TryParse(ParentId, out var parentIdValue) ? parentIdValue : 0,
                    RoutePath = RoutePath,
                    Icon = Icon,
                    Component = Component,
                    IsExternal = (ExternalEnum)IsExternal,
                    IsCache = (CacheEnum)IsCache,
                    IsVisible = (VisibilityEnum)IsVisible,
                    OrderNum = OrderNum,
                    MenuStatus = (StatusEnum)MenuStatus,
                    Remarks = Remarks
                };

                var result = await _menuService.UpdateMenuAsync(Id, dto);
                if (!result.Success)
                {
                    Error = result.Message ?? GetTranslation("Identity.Menu.UpdateFailed", "更新菜单失败");
                    return;
                }

                SaveSuccessCallback?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // 关闭窗口由窗口本身处理
        if (System.Windows.Application.Current?.MainWindow is System.Windows.Window mainWindow)
        {
            var window = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}

