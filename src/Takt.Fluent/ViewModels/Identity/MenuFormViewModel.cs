//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : MenuFormViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-11
// 版本号 : 0.0.1
// 描述    : 菜单表单视图模型（新建/更新）
//===================================================================

using System.Collections;
using System.ComponentModel;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Common.Enums;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 菜单表单视图模型
/// </summary>
/// <remarks>
/// 使用 WPF 原生验证系统 INotifyDataErrorInfo
/// </remarks>
public partial class MenuFormViewModel : ObservableObject, INotifyDataErrorInfo
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
    public string MenuNameHint => _localizationManager.GetString("identity.menu.validation.menunamehint");

    public string MenuCodeHint => _localizationManager.GetString("identity.menu.validation.menucodehint");

    public string I18nKeyHint => _localizationManager.GetString("identity.menu.validation.i18nkeyhint");

    public string PermCodeHint => _localizationManager.GetString("identity.menu.validation.permcodehint");

    public string ParentIdHint => _localizationManager.GetString("identity.menu.validation.parentidhint");

    public string RoutePathHint => _localizationManager.GetString("identity.menu.validation.routepathhint");

    public string IconHint => _localizationManager.GetString("identity.menu.validation.iconhint");

    public string ComponentHint => _localizationManager.GetString("identity.menu.validation.componenthint");

    public string OrderNumHint => _localizationManager.GetString("identity.menu.validation.ordernumhint");

    public string RemarksHint => _localizationManager.GetString("identity.menu.validation.remarkshint");

    // 错误消息属性（保留用于向后兼容，同时更新 INotifyDataErrorInfo）
    private string _menuNameError = string.Empty;
    public string MenuNameError
    {
        get => _menuNameError;
        private set
        {
            if (SetProperty(ref _menuNameError, value))
            {
                SetError(nameof(MenuName), value);
            }
        }
    }

    private string _menuCodeError = string.Empty;
    public string MenuCodeError
    {
        get => _menuCodeError;
        private set
        {
            if (SetProperty(ref _menuCodeError, value))
            {
                SetError(nameof(MenuCode), value);
            }
        }
    }

    private string _i18nKeyError = string.Empty;
    public string I18nKeyError
    {
        get => _i18nKeyError;
        private set
        {
            if (SetProperty(ref _i18nKeyError, value))
            {
                SetError(nameof(I18nKey), value);
            }
        }
    }

    private string _permCodeError = string.Empty;
    public string PermCodeError
    {
        get => _permCodeError;
        private set
        {
            if (SetProperty(ref _permCodeError, value))
            {
                SetError(nameof(PermCode), value);
            }
        }
    }

    private string _orderNumError = string.Empty;
    public string OrderNumError
    {
        get => _orderNumError;
        private set
        {
            if (SetProperty(ref _orderNumError, value))
            {
                SetError(nameof(OrderNum), value);
            }
        }
    }

    private string _remarksError = string.Empty;
    public string RemarksError
    {
        get => _remarksError;
        private set
        {
            if (SetProperty(ref _remarksError, value))
            {
                SetError(nameof(Remarks), value);
            }
        }
    }

    // WPF 原生验证系统：INotifyDataErrorInfo 实现
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(e => e);
        }

        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    /// <summary>
    /// 设置字段错误（WPF 原生验证，替换现有错误）
    /// </summary>
    private void SetError(string propertyName, string? error)
    {
        if (string.IsNullOrEmpty(error))
        {
            if (_errors.Remove(propertyName))
            {
                OnErrorsChanged(propertyName);
            }
        }
        else
        {
            _errors[propertyName] = new List<string> { error };
            OnErrorsChanged(propertyName);
        }
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public MenuFormViewModel(IMenuService menuService, ILocalizationManager localizationManager)
    {
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
    }


    public void ForCreate()
    {
        ClearAllErrors();

        IsCreate = true;
        // 拼接标题：新增 + 菜单 = "新增菜单"
        var createText = _localizationManager.GetString("common.button.create");
        var entityText = _localizationManager.GetString("identity.menu.entity");
        Title = $"{createText}{entityText}";
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
        // 拼接标题：更新 + 菜单 = "更新菜单"
        var updateText = _localizationManager.GetString("common.button.update");
        var entityText = _localizationManager.GetString("identity.menu.entity");
        Title = $"{updateText}{entityText}";
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
        OrderNumError = string.Empty;
        RemarksError = string.Empty;
        Error = string.Empty;

        // 清除所有 INotifyDataErrorInfo 错误
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
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
            MenuNameError = _localizationManager.GetString("identity.menu.validation.menunamemaxlength");
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
            MenuCodeError = _localizationManager.GetString("identity.menu.validation.menucodemaxlength");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(MenuCode, @"^[a-z0-9_]+$"))
        {
            MenuCodeError = _localizationManager.GetString("identity.menu.validation.menucodeinvalid");
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
            I18nKeyError = _localizationManager.GetString("identity.menu.validation.i18nkeymaxlength");
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
            PermCodeError = _localizationManager.GetString("identity.menu.validation.permcodemaxlength");
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
            MenuNameError = _localizationManager.GetString("identity.menu.validation.menunamerequired");
            isValid = false;
        }
        else if (MenuName.Length > 20)
        {
            MenuNameError = _localizationManager.GetString("identity.menu.validation.menunamemaxlength");
            isValid = false;
        }

        // 验证菜单编码（必填）
        if (string.IsNullOrWhiteSpace(MenuCode))
        {
            MenuCodeError = _localizationManager.GetString("identity.menu.validation.menucoderequired");
            isValid = false;
        }
        else if (MenuCode.Length > 10)
        {
            MenuCodeError = _localizationManager.GetString("identity.menu.validation.menucodemaxlength");
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(MenuCode, @"^[a-z0-9_]+$"))
        {
            MenuCodeError = _localizationManager.GetString("identity.menu.validation.menucodeinvalid");
            isValid = false;
        }

        // 验证国际化键（可选，但如果填写则不能超过64个字符）
        if (!string.IsNullOrWhiteSpace(I18nKey) && I18nKey.Length > 64)
        {
            I18nKeyError = _localizationManager.GetString("identity.menu.validation.i18nkeymaxlength");
            isValid = false;
        }

        // 验证权限码（可选，但如果填写则不能超过100个字符）
        if (!string.IsNullOrWhiteSpace(PermCode) && PermCode.Length > 100)
        {
            PermCodeError = _localizationManager.GetString("identity.menu.validation.permcodemaxlength");
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
                    I18nKey = I18nKey!,
                    PermCode = PermCode!,
                    MenuType = (MenuTypeEnum)MenuType,
                    ParentId = long.TryParse(ParentId, out var parentIdValue) ? parentIdValue : 0,
                    RoutePath = RoutePath!,
                    Icon = Icon!,
                    Component = Component!,
                    IsExternal = (ExternalEnum)IsExternal,
                    IsCache = (CacheEnum)IsCache,
                    IsVisible = (VisibilityEnum)IsVisible,
                    OrderNum = OrderNum,
                    MenuStatus = (StatusEnum)MenuStatus,
                    Remarks = Remarks!
                };

                var result = await _menuService.CreateMenuAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("identity.menu.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 创建成功，显示成功消息
                var createEntityName = _localizationManager.GetString("identity.menu.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.create"), createEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
                SaveSuccessCallback?.Invoke();
            }
            else
            {
                var dto = new MenuUpdateDto
                {
                    Id = Id,
                    MenuName = MenuName,
                    MenuCode = MenuCode,
                    I18nKey = I18nKey!,
                    PermCode = PermCode!,
                    MenuType = (MenuTypeEnum)MenuType,
                    ParentId = long.TryParse(ParentId, out var parentIdValue) ? parentIdValue : 0,
                    RoutePath = RoutePath!,
                    Icon = Icon!,
                    Component = Component!,
                    IsExternal = (ExternalEnum)IsExternal,
                    IsCache = (CacheEnum)IsCache,
                    IsVisible = (VisibilityEnum)IsVisible,
                    OrderNum = OrderNum,
                    MenuStatus = (StatusEnum)MenuStatus,
                    Remarks = Remarks!
                };

                var result = await _menuService.UpdateMenuAsync(Id, dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("identity.menu.entity");
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update"), entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }

                // 更新成功，显示成功消息
                var updateEntityName = _localizationManager.GetString("identity.menu.entity");
                var successMessage = string.Format(_localizationManager.GetString("common.success.update"), updateEntityName);
                TaktMessageManager.ShowSuccess(successMessage);
                SaveSuccessCallback?.Invoke();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
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

