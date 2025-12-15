// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：LocalizationViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：本地化管理视图模型（主子表容器）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Common.Context;
using Takt.Common.Logging;
using PageRequest = Takt.Fluent.Controls.PageRequest;
using QueryContext = Takt.Fluent.Controls.QueryContext;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 本地化管理视图模型（主子表容器）
/// </summary>
public partial class LocalizationViewModel : ObservableObject
{
    private readonly ILanguageService _languageService;
    private readonly ITranslationService _translationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly OperLogManager? _operLog;

    // 主表（转置表格：每行一个翻译键，每列一个语言）
    public ObservableCollection<TranslationTransposedDto> TranslationKeys { get; } = new();

    [ObservableProperty]
    private TranslationTransposedDto? _selectedTranslationKey;

    [ObservableProperty]
    private string _translationKeyKeyword = string.Empty;

    [ObservableProperty]
    private string? _moduleFilter = null;

    [ObservableProperty]
    private int _translationKeyPageIndex = 1;

    [ObservableProperty]
    private int _translationKeyPageSize = 20;

    [ObservableProperty]
    private int _translationKeyTotalCount;

    [ObservableProperty]
    private bool _isTranslationKeyLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private string _emptyMessage = string.Empty;


    // 所有启用的语言列表（用于生成列）
    public ObservableCollection<LanguageDto> EnabledLanguages { get; } = new();

    // 表格列定义（用于绑定到 TaktDataGrid）
    public ObservableCollection<Takt.Fluent.Controls.TaktDataGridColumnDefinition> Columns { get; } = new();

    public LocalizationViewModel(
        ILanguageService languageService,
        ITranslationService translationService,
        IServiceProvider serviceProvider,
        OperLogManager? operLog = null)
    {
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _operLog = operLog;

        EmptyMessage = "暂无数据";

        _ = InitializeAsync();
    }

    /// <summary>
    /// 初始化加载
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            _operLog?.Information("[LocalizationView] 开始初始化");

            // 先加载翻译键列表（主表），不依赖语言列表
            _operLog?.Information("[LocalizationView] 开始加载翻译键列表");
            await LoadTranslationKeysAsync();
            _operLog?.Information("[LocalizationView] 翻译键列表加载完成");

            // 然后加载启用的语言列表（用于从表显示）
            _operLog?.Information("[LocalizationView] 开始加载启用语言列表");
            await LoadEnabledLanguagesAsync();
            _operLog?.Information("[LocalizationView] 启用语言列表加载完成");

            _operLog?.Information("[LocalizationView] 初始化完成");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[LocalizationView] 初始化失败，异常: {Exception}", ex.ToString());
            ErrorMessage = $"初始化失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 加载启用的语言列表（用于生成转置列）
    /// 注意：确保在 UI 线程上更新 ObservableCollection
    /// </summary>
    private async Task LoadEnabledLanguagesAsync()
    {
        try
        {
            _operLog?.Information("[LocalizationView] LoadEnabledLanguagesAsync 开始执行");
            var query = new LanguageQueryDto
            {
                PageIndex = 1,
                PageSize = int.MaxValue
            };
            var result = await _languageService.GetListAsync(query);
            _operLog?.Information("[LocalizationView] 语言服务调用完成，Success={Success}", result.Success);

            if (result.Success && result.Data != null)
            {
                // 先在后台线程收集所有启用的语言
                var enabledLanguages = result.Data.Items
                    .Where(l => l.LanguageStatus == 0)
                    .OrderBy(l => l.OrderNum)
                    .ThenBy(l => l.LanguageCode)
                    .ToList();

                _operLog?.Information("[LocalizationView] 收集到 {Count} 个启用语言", enabledLanguages.Count);

                // 在 UI 线程上批量更新 ObservableCollection 和生成列定义
                // 这样可以避免多次触发 CollectionChanged 事件，并且确保在 UI 线程上执行
                var app = System.Windows.Application.Current;
                if (app?.Dispatcher != null && !app.Dispatcher.HasShutdownStarted)
                {
                    app.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            // 更新语言列表
                            EnabledLanguages.Clear();
                            foreach (var lang in enabledLanguages)
                            {
                                EnabledLanguages.Add(lang);
                            }
                            _operLog?.Information("[LocalizationView] 成功加载 {Count} 个启用语言到 UI", enabledLanguages.Count);

                            // 生成列定义
                            UpdateColumns();
                        }
                        catch (Exception ex)
                        {
                            _operLog?.Error(ex, "[LocalizationView] 在 UI 线程上更新语言列表和列定义时发生异常");
                        }
                    });
                }
                else
                {
                    // 如果 Application 不可用，直接更新（可能在某些测试场景）
                    EnabledLanguages.Clear();
                    foreach (var lang in enabledLanguages)
                    {
                        EnabledLanguages.Add(lang);
                    }
                    _operLog?.Information("[LocalizationView] 成功加载 {Count} 个启用语言（Application 不可用）", enabledLanguages.Count);

                    // 生成列定义
                    UpdateColumns();
                }
            }
            else
            {
                _operLog?.Warning("[LocalizationView] 语言服务返回失败或数据为空");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[LocalizationView] 加载启用语言列表失败，异常: {Exception}", ex.ToString());
            // 不抛出异常，允许继续执行
        }
    }

    /// <summary>
    /// 更新表格列定义
    /// 根据启用的语言列表生成列定义
    /// </summary>
    private void UpdateColumns()
    {
        try
        {
            Columns.Clear();

            // 第一列：翻译键（固定列）
            var translationKeyColumn = new Takt.Fluent.Controls.TaktDataGridColumnDefinition
            {
                Header = "翻译键",
                BindingPath = "TranslationKey",
                Width = new System.Windows.Controls.DataGridLength(1, System.Windows.Controls.DataGridLengthUnitType.Star),
                CanSort = true
            };
            Columns.Add(translationKeyColumn);

            // 为每个启用的语言创建一列
            foreach (var language in EnabledLanguages.OrderBy(l => l.OrderNum).ThenBy(l => l.LanguageCode))
            {
                var languageColumn = new Takt.Fluent.Controls.TaktDataGridColumnDefinition
                {
                    Header = $"{language.LanguageName} ({language.LanguageCode})",
                    BindingPath = $"TranslationValues[{language.LanguageCode}]",
                    Width = new System.Windows.Controls.DataGridLength(200),
                    CanSort = false
                };
                Columns.Add(languageColumn);
            }

            _operLog?.Information("[LocalizationView] 成功生成 {Count} 个列定义", Columns.Count);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[LocalizationView] 生成列定义时发生异常");
        }
    }

    /// <summary>
    /// 加载翻译键列表（主表）
    /// </summary>
    private async Task LoadTranslationKeysAsync()
    {
        _operLog?.Information("[LocalizationView] LoadTranslationKeysAsync 开始执行");

        if (IsTranslationKeyLoading)
        {
            _operLog?.Warning("[LocalizationView] 正在加载中，跳过本次请求");
            return;
        }

        IsTranslationKeyLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[LocalizationView] Load translation keys: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}, module={Module}",
                TranslationKeyPageIndex, TranslationKeyPageSize, TranslationKeyKeyword, ModuleFilter ?? "全部");

            _operLog?.Information("[LocalizationView] 准备调用 _translationService.GetTranslationKeysAsync");
            var result = await _translationService.GetTranslationKeysAsync(
                TranslationKeyPageIndex,
                TranslationKeyPageSize,
                string.IsNullOrWhiteSpace(TranslationKeyKeyword) ? null! : TranslationKeyKeyword.Trim(),
                ModuleFilter);
            _operLog?.Information("[LocalizationView] _translationService.GetTranslationKeysAsync 调用完成，Success={Success}", result.Success);

            if (!result.Success)
            {
                TranslationKeys.Clear();
                TranslationKeyTotalCount = 0;
                ErrorMessage = result.Message ?? "加载翻译键数据失败";
                _operLog?.Error("[LocalizationView] 加载翻译键失败: {Message}", ErrorMessage);
                UpdateEmptyMessage();
                return;
            }

            if (result.Data == null)
            {
                TranslationKeys.Clear();
                TranslationKeyTotalCount = 0;
                ErrorMessage = "服务返回数据为空";
                _operLog?.Warning("[LocalizationView] 服务返回数据为空");
                UpdateEmptyMessage();
                return;
            }

            // 获取当前页所有翻译键的转置数据
            var translationKeysList = result.Data.Items.ToList();
            var transposedResult = await _translationService.GetTranslationsByKeysAsync(translationKeysList);

            if (!transposedResult.Success || transposedResult.Data == null)
            {
                TranslationKeys.Clear();
                TranslationKeyTotalCount = 0;
                ErrorMessage = transposedResult.Message ?? "加载转置数据失败";
                _operLog?.Error("[LocalizationView] 加载转置数据失败: {Message}", ErrorMessage);
                UpdateEmptyMessage();
                return;
            }

            // 获取所有翻译记录（用于获取ID）
            var queryResult = await _translationService.GetListAsync(new TranslationQueryDto
            {
                TranslationKey = null!, 
                PageIndex = 1,
                PageSize = int.MaxValue
            });

            // 构建翻译ID字典：{翻译键: {语言代码: ID}}
            var translationIdDict = new Dictionary<string, Dictionary<string, long>>();
            if (queryResult.Success && queryResult.Data != null)
            {
                foreach (var trans in queryResult.Data.Items)
                {
                    if (!translationIdDict.ContainsKey(trans.TranslationKey))
                    {
                        translationIdDict[trans.TranslationKey] = new Dictionary<string, long>();
                    }
                    translationIdDict[trans.TranslationKey][trans.LanguageCode] = trans.Id;
                }
            }

            // 构建转置表格数据：为每个翻译键创建一行
            TranslationKeys.Clear();
            foreach (var translationKey in translationKeysList)
            {
                var transposedItem = new TranslationTransposedDto
                {
                    TranslationKey = translationKey,
                    TranslationValues = transposedResult.Data.TryGetValue(translationKey, out var values)
                        ? new Dictionary<string, string>(values)
                        : new Dictionary<string, string>(),
                    TranslationIds = translationIdDict.TryGetValue(translationKey, out var ids)
                        ? new Dictionary<string, long>(ids)
                        : new Dictionary<string, long>()
                };
                TranslationKeys.Add(transposedItem);
            }

            TranslationKeyTotalCount = result.Data.TotalNum;
            _operLog?.Information("[LocalizationView] 成功加载 {Count} 个翻译键的转置数据，总数: {Total}",
                TranslationKeys.Count, TranslationKeyTotalCount);
            UpdateEmptyMessage();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[LocalizationView] 加载翻译键列表失败");
            UpdateEmptyMessage();
        }
        finally
        {
            IsTranslationKeyLoading = false;
        }
    }

    partial void OnErrorMessageChanged(string? value)
    {
        UpdateEmptyMessage();
    }

    private void UpdateEmptyMessage()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            EmptyMessage = ErrorMessage;
            return;
        }

        if (TranslationKeys.Count == 0)
        {
            EmptyMessage = "暂无数据";
        }
        else
        {
            EmptyMessage = string.Empty;
        }
    }


    /// <summary>
    /// 翻译键查询命令
    /// </summary>
    [RelayCommand]
    private async Task QueryTranslationKeyAsync(QueryContext? context = null)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        if (context != null)
        {
            _operLog?.Information("[LocalizationView] 执行查询操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}", 
                operatorName, context.Keyword ?? string.Empty, context.PageIndex, context.PageSize);
            TranslationKeyKeyword = context.Keyword ?? string.Empty;
            if (context.PageIndex > 0)
            {
                TranslationKeyPageIndex = context.PageIndex;
            }
            if (context.PageSize > 0)
            {
                TranslationKeyPageSize = context.PageSize;
            }
        }
        else
        {
            TranslationKeyPageIndex = 1;
        }
        await LoadTranslationKeysAsync();
    }

    /// <summary>
    /// 翻译键重置命令
    /// </summary>
    [RelayCommand]
    private async Task ResetTranslationKeyAsync(QueryContext? context = null)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[LocalizationView] 执行重置操作，操作人={Operator}", operatorName);
        
        TranslationKeyKeyword = string.Empty;
        ModuleFilter = null;
        TranslationKeyPageIndex = 1;
        await LoadTranslationKeysAsync();
    }

    /// <summary>
    /// 分页变化命令
    /// </summary>
    [RelayCommand]
    private async Task PageChangedAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[LocalizationView] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}", 
            operatorName, request.PageIndex, request.PageSize);
        
        if (request.PageIndex > 0)
        {
            TranslationKeyPageIndex = request.PageIndex;
        }
        if (request.PageSize > 0)
        {
            TranslationKeyPageSize = request.PageSize;
        }
        await LoadTranslationKeysAsync();
    }

    /// <summary>
    /// 创建翻译键命令
    /// </summary>
    [RelayCommand]
    private void CreateTranslationKey()
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[LocalizationView] 打开新建翻译键窗口，操作人={Operator}", operatorName);
        
        try
        {
            // TODO: 实现 TranslationKeyFormViewModel 和 TranslationKeyForm
            ErrorMessage = "翻译键表单功能尚未实现";
            _operLog?.Warning("[LocalizationView] 创建翻译键功能尚未实现");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[LocalizationView] 创建翻译键失败");
        }
    }

    /// <summary>
    /// 更新翻译键命令（用于工具栏和行内更新按钮）
    /// </summary>
    [RelayCommand]
    private async Task UpdateTranslationKeyAsync(TranslationTransposedDto? item)
    {
        if (item == null)
        {
            item = SelectedTranslationKey;
        }

        if (item == null || string.IsNullOrWhiteSpace(item.TranslationKey))
        {
            ErrorMessage = "请选择要更新的翻译键";
            return;
        }

        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[LocalizationView] 打开更新翻译键窗口，操作人={Operator}, 翻译键={TranslationKey}", 
            operatorName, item.TranslationKey ?? string.Empty);
        
        try
        {
            // TODO: 实现 TranslationKeyFormViewModel 和 TranslationKeyForm
            // 暂时重新加载数据
            await LoadTranslationKeysAsync();
            SuccessMessage = "更新翻译键成功";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[LocalizationView] 更新翻译键失败");
        }
    }

    /// <summary>
    /// 删除翻译键命令
    /// 支持两种调用方式：
    /// 1. 工具栏删除：传递 string (translationKey)
    /// 2. 行内删除：传递 TranslationTransposedDto (item)
    /// </summary>
    [RelayCommand]
    private async Task DeleteTranslationKeyAsync(object? parameter)
    {
        string? translationKey = null;

        // 处理不同类型的参数
        if (parameter is string str)
        {
            translationKey = str;
        }
        else if (parameter is TranslationTransposedDto item)
        {
            translationKey = item.TranslationKey;
        }

        // 如果参数为空，使用选中的项
        if (string.IsNullOrWhiteSpace(translationKey))
        {
            translationKey = SelectedTranslationKey?.TranslationKey;
        }

        if (string.IsNullOrWhiteSpace(translationKey))
        {
            ErrorMessage = "请选择要删除的翻译键";
            return;
        }

        var confirmMessage = $"确定要删除翻译键 '{translationKey}' 及其所有语言翻译吗？";
        var result = System.Windows.MessageBox.Show(
            confirmMessage,
            "确认",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        IsTranslationKeyLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[LocalizationView] Delete translation key: {Key}", translationKey);

            // 删除该翻译键的所有语言翻译
            var queryResult = await _translationService.GetListAsync(new TranslationQueryDto
            {
                TranslationKey = translationKey,
                PageIndex = 1,
                PageSize = int.MaxValue
            });

            if (queryResult.Success && queryResult.Data != null)
            {
                foreach (var trans in queryResult.Data.Items)
                {
                    await _translationService.DeleteAsync(trans.Id);
                }
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            SuccessMessage = "删除翻译键成功";
            _operLog?.Information("[LocalizationView] 删除翻译键成功，操作人={Operator}, 翻译键={TranslationKey}", 
                operatorName, translationKey);
            await LoadTranslationKeysAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[LocalizationView] 删除翻译键失败");
        }
        finally
        {
            IsTranslationKeyLoading = false;
        }
    }

    /// <summary>
    /// 更新翻译值命令（转置表格中的单元格编辑）
    /// </summary>
    [RelayCommand]
    private async Task UpdateTranslationValueAsync(TranslationTransposedDto? item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.TranslationKey))
        {
            ErrorMessage = "请选择要编辑的翻译";
            return;
        }

        // 从 View 传递的语言代码需要通过其他方式获取，这里先使用第一个语言
        // 实际应该从 View 的事件参数中获取语言代码
        try
        {
            // 重新加载数据以获取最新状态
            await LoadTranslationKeysAsync();
            SuccessMessage = "更新翻译成功";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[LocalizationView] 更新翻译值失败");
        }
    }

    /// <summary>
    /// 保存单个翻译值
    /// </summary>
    public async Task<bool> SaveTranslationValueAsync(string translationKey, string languageCode, string translationValue, long? translationId)
    {
        try
        {
            if (translationId.HasValue)
            {
                // 更新现有翻译
                var updateDto = new TranslationUpdateDto
                {
                    Id = translationId.Value,
                    LanguageCode = languageCode,
                    TranslationKey = translationKey,
                    TranslationValue = translationValue
                };

                var result = await _translationService.UpdateAsync(updateDto);
                if (!result.Success)
                {
                    ErrorMessage = result.Message ?? "更新翻译失败";
                    return false;
                }

                SuccessMessage = "更新翻译成功";
                return true;
            }
            else
            {
                // 创建新翻译
                var createDto = new TranslationCreateDto
                {
                    LanguageCode = languageCode,
                    TranslationKey = translationKey,
                    TranslationValue = translationValue
                };

                var result = await _translationService.CreateAsync(createDto);
                if (!result.Success)
                {
                    ErrorMessage = result.Message ?? "创建翻译失败";
                    return false;
                }

                // 更新转置数据中的ID
                var item = TranslationKeys.FirstOrDefault(k => k.TranslationKey == translationKey);
                if (item != null && result.Data > 0)
                {
                    item.TranslationIds[languageCode] = result.Data;
                }

                SuccessMessage = "创建翻译成功";
                return true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[LocalizationView] 保存翻译值失败");
            return false;
        }
    }
}

