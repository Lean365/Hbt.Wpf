// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：DictionaryViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典管理视图模型（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using PageRequest = Takt.Fluent.Controls.PageRequest;
using QueryContext = Takt.Fluent.Controls.QueryContext;

namespace Takt.Fluent.ViewModels.Routine;

/// <summary>
/// 字典管理视图模型（主子表视图）
/// 主表：DictionaryType（字典类型）
/// 子表：DictionaryData（字典数据）
/// </summary>
public partial class DictionaryViewModel : ObservableObject
{
    private readonly IDictionaryTypeService _dictionaryTypeService;
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<DictionaryTypeDto> DictionaryTypes { get; } = new();

    [ObservableProperty]
    private DictionaryTypeDto? _selectedDictionaryType;

    // 子表数据
    public ObservableCollection<DictionaryDataDto> DictionaryDataList { get; } = new();

    [ObservableProperty]
    private DictionaryDataDto? _selectedDictionaryData;

    // 行内编辑状态（参照 CodeGenFormViewModel）
    [ObservableProperty]
    private DictionaryDataDto? _editingDictionaryData;

    // 主表查询相关
    [ObservableProperty]
    private string _typeKeyword = string.Empty;

    [ObservableProperty]
    private int _typePageIndex = 1;

    [ObservableProperty]
    private int _typePageSize = 20;

    [ObservableProperty]
    private int _typeTotalCount;

    // 子表查询相关
    [ObservableProperty]
    private string _dataKeyword = string.Empty;

    [ObservableProperty]
    private int _dataPageIndex = 1;

    [ObservableProperty]
    private int _dataPageSize = 20;

    [ObservableProperty]
    private int _dataTotalCount;

    [ObservableProperty]
    private bool _isLoadingTypes;

    [ObservableProperty]
    private bool _isLoadingData;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public int TypeTotalPages => TypePageSize <= 0 ? 0 : (int)Math.Ceiling((double)TypeTotalCount / TypePageSize);
    public int DataTotalPages => DataPageSize <= 0 ? 0 : (int)Math.Ceiling((double)DataTotalCount / DataPageSize);

    public DictionaryViewModel(
        IDictionaryTypeService dictionaryTypeService,
        IDictionaryDataService dictionaryDataService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _dictionaryTypeService = dictionaryTypeService ?? throw new ArgumentNullException(nameof(dictionaryTypeService));
        _dictionaryDataService = dictionaryDataService ?? throw new ArgumentNullException(nameof(dictionaryDataService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        _ = LoadTypesAsync();
    }

    /// <summary>
    /// 加载字典类型列表（主表）
    /// </summary>
    private async Task LoadTypesAsync()
    {
        if (IsLoadingTypes)
        {
            return;
        }

        IsLoadingTypes = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[DictionaryView] Load dictionary types: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                TypePageIndex, TypePageSize, TypeKeyword);

            // 构建查询DTO
            var query = new DictionaryTypeQueryDto
            {
                PageIndex = TypePageIndex,
                PageSize = TypePageSize,
                Keywords = string.IsNullOrWhiteSpace(TypeKeyword) ? null! : TypeKeyword.Trim()
            };

            var result = await _dictionaryTypeService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                DictionaryTypes.Clear();
                TypeTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("routine.dictionary.loadtypesfailed");
                return;
            }

            DictionaryTypes.Clear();
            foreach (var item in result.Data.Items)
            {
                DictionaryTypes.Add(item);
            }

            TypeTotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 加载字典类型失败");
        }
        finally
        {
            IsLoadingTypes = false;
        }
    }

    /// <summary>
    /// 加载字典数据列表（子表）
    /// </summary>
    private async Task LoadDataAsync()
    {
        if (SelectedDictionaryType == null)
        {
            DictionaryDataList.Clear();
            DataTotalCount = 0;
            return;
        }

        if (IsLoadingData)
        {
            return;
        }

        IsLoadingData = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[DictionaryView] Load dictionary data: typeCode={TypeCode}, pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SelectedDictionaryType.TypeCode, DataPageIndex, DataPageSize, DataKeyword);

            // 使用查询DTO进行分页查询
            var query = new DictionaryDataQueryDto
            {
                TypeCode = SelectedDictionaryType.TypeCode,
                Keywords = string.IsNullOrWhiteSpace(DataKeyword) ? null! : DataKeyword.Trim(),
                PageIndex = DataPageIndex,
                PageSize = DataPageSize
            };

            var result = await _dictionaryDataService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                DictionaryDataList.Clear();
                foreach (var item in result.Data.Items)
                {
                    DictionaryDataList.Add(item);
                }

                DataTotalCount = result.Data.TotalNum;
            }
            else
            {
                DictionaryDataList.Clear();
                DataTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("routine.dictionary.loaddatafailed");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 加载字典数据失败");
        }
        finally
        {
            IsLoadingData = false;
        }
    }

    [RelayCommand]
    private async Task QueryTypesAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[DictionaryView] 执行查询字典类型操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}",
            operatorName, context.Keyword ?? string.Empty, context.PageIndex, context.PageSize);

        TypeKeyword = context.Keyword ?? string.Empty;
        if (TypePageIndex != context.PageIndex)
        {
            TypePageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (TypePageSize != context.PageSize)
        {
            TypePageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadTypesAsync();
    }

    [RelayCommand]
    private async Task ResetTypesAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[DictionaryView] 执行重置字典类型操作，操作人={Operator}", operatorName);

        TypeKeyword = string.Empty;
        TypePageIndex = 1;
        await LoadTypesAsync();
    }

    [RelayCommand]
    private async Task PageChangedTypesAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[DictionaryView] 字典类型分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}",
            operatorName, request.PageIndex, request.PageSize);

        if (TypePageIndex != request.PageIndex)
        {
            TypePageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        }

        if (TypePageSize != request.PageSize && request.PageSize > 0)
        {
            TypePageSize = request.PageSize;
        }

        await LoadTypesAsync();
    }

    [RelayCommand]
    private async Task QueryDataAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[DictionaryView] 执行查询字典数据操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}",
            operatorName, context.Keyword ?? string.Empty, context.PageIndex, context.PageSize);

        DataKeyword = context.Keyword ?? string.Empty;
        if (DataPageIndex != context.PageIndex)
        {
            DataPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (DataPageSize != context.PageSize)
        {
            DataPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ResetDataAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[DictionaryView] 执行重置字典数据操作，操作人={Operator}", operatorName);

        DataKeyword = string.Empty;
        DataPageIndex = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task PageChangedDataAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[DictionaryView] 字典数据分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}",
            operatorName, request.PageIndex, request.PageSize);

        if (DataPageIndex != request.PageIndex)
        {
            DataPageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        }

        if (DataPageSize != request.PageSize && request.PageSize > 0)
        {
            DataPageSize = request.PageSize;
        }

        await LoadDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUpdateType))]
    private void UpdateType(DictionaryTypeDto? dictionaryType)
    {
        if (dictionaryType == null)
        {
            dictionaryType = SelectedDictionaryType;
        }

        if (dictionaryType == null)
        {
            return;
        }

        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.DictionaryComponent.DictionaryForm>();
            if (window.DataContext is not DictionaryFormViewModel formViewModel)
            {
                throw new InvalidOperationException("DictionaryForm DataContext 不是 DictionaryFormViewModel");
            }

            formViewModel.ForUpdate(dictionaryType);

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadTypesAsync();
                // 如果当前选中的类型被更新，重新加载子表数据
                if (SelectedDictionaryType?.Id == dictionaryType.Id)
                {
                    await LoadDataAsync();
                }
            };

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[DictionaryView] 打开更新字典类型窗口，操作人={Operator}, 字典类型Id={Id}, 类型代码={TypeCode}",
                operatorName, dictionaryType.Id, dictionaryType.TypeCode ?? string.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 打开编辑字典类型窗口失败");
        }
    }

    private bool CanUpdateType(DictionaryTypeDto? dictionaryType)
    {
        if (dictionaryType == null)
        {
            return SelectedDictionaryType is not null;
        }
        return dictionaryType is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteType))]
    private async Task DeleteTypeAsync(DictionaryTypeDto? dictionaryType)
    {
        if (dictionaryType == null)
        {
            dictionaryType = SelectedDictionaryType;
        }

        if (dictionaryType == null)
        {
            return;
        }

        try
        {
            var confirmMessage = _localizationManager.GetString("routine.dictionary.deletetypeconfirm");
            var confirmTitle = _localizationManager.GetString("common.messagebox.question");
            var result = System.Windows.MessageBox.Show(
                confirmMessage,
                confirmTitle,
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            var deleteResult = await _dictionaryTypeService.DeleteAsync(dictionaryType.Id);
            if (!deleteResult.Success)
            {
                var entityName = _localizationManager.GetString("routine.dictionary.type");
                ErrorMessage = deleteResult.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("routine.dictionary.type");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[DictionaryView] 删除字典类型成功，操作人={Operator}, 字典类型Id={Id}, 类型代码={TypeCode}",
                operatorName, dictionaryType.Id, dictionaryType.TypeCode ?? string.Empty);
            await LoadTypesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 删除字典类型失败");
        }
    }

    private bool CanDeleteType(DictionaryTypeDto? dictionaryType) => dictionaryType is not null || SelectedDictionaryType is not null;

    [RelayCommand]
    private void CreateType()
    {
        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.DictionaryComponent.DictionaryForm>();
            if (window.DataContext is not DictionaryFormViewModel formViewModel)
            {
                throw new InvalidOperationException("DictionaryForm DataContext 不是 DictionaryFormViewModel");
            }

            formViewModel.ForCreate();

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadTypesAsync();
                // 重新加载子表数据（如果有选中的类型）
                if (SelectedDictionaryType != null)
                {
                    await LoadDataAsync();
                }
            };

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[DictionaryView] 打开新建字典类型窗口，操作人={Operator}", operatorName);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 打开新建字典类型窗口失败");
        }
    }

    // UpdateData 和 CreateData 命令已移除，子表使用行内编辑（UpdateDataInlineCommand, CreateDataInlineCommand）

    [RelayCommand(CanExecute = nameof(CanDeleteData))]
    private async Task DeleteDataAsync(DictionaryDataDto? dictionaryData)
    {
        if (dictionaryData == null)
        {
            dictionaryData = SelectedDictionaryData;
        }

        if (dictionaryData == null)
        {
            return;
        }

        try
        {
            var confirmMessage = _localizationManager.GetString("routine.dictionary.deletedataconfirm");
            var confirmTitle = _localizationManager.GetString("common.messagebox.question");
            var result = System.Windows.MessageBox.Show(
                confirmMessage,
                confirmTitle,
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            var deleteResult = await _dictionaryDataService.DeleteAsync(dictionaryData.Id);
            if (!deleteResult.Success)
            {
                var entityName = _localizationManager.GetString("routine.dictionary.data");
                ErrorMessage = deleteResult.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            var entityNameSuccess = _localizationManager.GetString("routine.dictionary.data");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[DictionaryView] 删除字典数据成功，操作人={Operator}, 字典数据Id={Id}, 数据值={DataValue}",
                operatorName, dictionaryData.Id, dictionaryData.DataValue ?? string.Empty);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 删除字典数据失败");
        }
    }

    private bool CanDeleteData(DictionaryDataDto? dictionaryData) => dictionaryData is not null || SelectedDictionaryData is not null;

    // CreateData 命令已移除，子表使用行内编辑（CreateDataInlineCommand）

    partial void OnSelectedDictionaryTypeChanged(DictionaryTypeDto? value)
    {
        UpdateTypeCommand.NotifyCanExecuteChanged();
        DeleteTypeCommand.NotifyCanExecuteChanged();
        CreateDataInlineCommand.NotifyCanExecuteChanged();

        // 当选中字典类型改变时，加载对应的字典数据
        if (value != null)
        {
            DataPageIndex = 1;
            DataKeyword = string.Empty;
            _ = LoadDataAsync();
        }
        else
        {
            DictionaryDataList.Clear();
            DataTotalCount = 0;
        }
    }

    partial void OnSelectedDictionaryDataChanged(DictionaryDataDto? value)
    {
        DeleteDataCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 当 EditingDictionaryData 改变时，通知所有相关命令重新评估 CanExecute
    /// </summary>
    partial void OnEditingDictionaryDataChanged(DictionaryDataDto? value)
    {
        // 通知所有行的按钮命令重新评估 CanExecute
        CreateDataInlineCommand.NotifyCanExecuteChanged();
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();
        UpdateDataInlineCommand.NotifyCanExecuteChanged();
        DeleteDataCommand.NotifyCanExecuteChanged();

        // 触发全局命令重新评估，确保所有行的按钮状态都能正确更新
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// 行内编辑：进入编辑状态（参照 CodeGenFormViewModel.UpdateColumnCommand）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdateDataInline))]
    private void UpdateDataInline(DictionaryDataDto? dictionaryData)
    {
        if (dictionaryData == null)
        {
            dictionaryData = SelectedDictionaryData;
        }

        if (dictionaryData == null)
        {
            return;
        }

        EditingDictionaryData = dictionaryData;

        // 通知命令重新评估 CanExecute
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[DictionaryView] 进入编辑字典数据状态，字典数据Id={Id}, 数据值={DataValue}",
            dictionaryData.Id, dictionaryData.DataValue ?? string.Empty);
    }

    private bool CanUpdateDataInline(DictionaryDataDto? dictionaryData)
    {
        if (dictionaryData == null)
        {
            return SelectedDictionaryData is not null && EditingDictionaryData == null;
        }
        return dictionaryData is not null && EditingDictionaryData == null;
    }

    /// <summary>
    /// 行内编辑：保存当前编辑的字典数据（参照 CodeGenFormViewModel.SaveColumnCommand）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveDataInline))]
    private async Task SaveDataInlineAsync(DictionaryDataDto? dictionaryData)
    {
        if (dictionaryData == null)
        {
            dictionaryData = EditingDictionaryData;
        }

        if (dictionaryData == null || EditingDictionaryData != dictionaryData)
        {
            return;
        }

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(dictionaryData.DataLabel))
        {
            ErrorMessage = _localizationManager.GetString("routine.dictionary.datalabelrequired");
            return;
        }

        if (string.IsNullOrWhiteSpace(dictionaryData.I18nKey))
        {
            ErrorMessage = _localizationManager.GetString("routine.dictionary.i18nkeyrequired");
            return;
        }

        try
        {
            if (dictionaryData.Id == 0)
            {
                // 新增
                var createDto = new DictionaryDataCreateDto
                {
                    TypeCode = dictionaryData.TypeCode,
                    DataLabel = dictionaryData.DataLabel,
                    I18nKey = dictionaryData.I18nKey,
                    DataValue = dictionaryData.DataValue,
                    ExtLabel = dictionaryData.ExtLabel,
                    ExtValue = dictionaryData.ExtValue,
                    CssClass = dictionaryData.CssClass,
                    ListClass = dictionaryData.ListClass,
                    OrderNum = dictionaryData.OrderNum,
                    Remarks = dictionaryData.Remarks
                };

                var result = await _dictionaryDataService.CreateAsync(createDto);
                if (!result.Success || result.Data <= 0)
                {
                    var entityName = _localizationManager.GetString("routine.dictionary.data");
                    ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create"), entityName);
                    _operLog?.Error("[DictionaryView] 字典数据创建失败，字典类型代码={TypeCode}, 数据标签={DataLabel}, 错误信息={ErrorMessage}",
                        dictionaryData.TypeCode ?? string.Empty, dictionaryData.DataLabel, ErrorMessage);
                    return;
                }

                dictionaryData.Id = result.Data;
                EditingDictionaryData = null;

                // 通知命令重新评估 CanExecute
                SaveDataInlineCommand.NotifyCanExecuteChanged();
                CancelDataInlineCommand.NotifyCanExecuteChanged();

                var entityNameSuccess = _localizationManager.GetString("routine.dictionary.data");
                SuccessMessage = string.Format(_localizationManager.GetString("common.success.create"), entityNameSuccess);
                _operLog?.Information("[DictionaryView] 字典数据创建成功，字典数据Id={Id}, 数据标签={DataLabel}, 数据值={DataValue}",
                    dictionaryData.Id, dictionaryData.DataLabel, dictionaryData.DataValue ?? string.Empty);

                await LoadDataAsync();
            }
            else
            {
                // 更新
                var updateDto = new DictionaryDataUpdateDto
                {
                    Id = dictionaryData.Id,
                    TypeCode = dictionaryData.TypeCode,
                    DataLabel = dictionaryData.DataLabel,
                    I18nKey = dictionaryData.I18nKey,
                    DataValue = dictionaryData.DataValue,
                    ExtLabel = dictionaryData.ExtLabel,
                    ExtValue = dictionaryData.ExtValue,
                    CssClass = dictionaryData.CssClass,
                    ListClass = dictionaryData.ListClass,
                    OrderNum = dictionaryData.OrderNum,
                    Remarks = dictionaryData.Remarks
                };

                var result = await _dictionaryDataService.UpdateAsync(updateDto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("routine.dictionary.data");
                    ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update"), entityName);
                    _operLog?.Error("[DictionaryView] 字典数据更新失败，字典数据Id={Id}, 数据标签={DataLabel}, 错误信息={ErrorMessage}",
                        dictionaryData.Id, dictionaryData.DataLabel, ErrorMessage);
                    return;
                }

                EditingDictionaryData = null;

                // 通知命令重新评估 CanExecute
                SaveDataInlineCommand.NotifyCanExecuteChanged();
                CancelDataInlineCommand.NotifyCanExecuteChanged();

                var entityNameSuccess = _localizationManager.GetString("routine.dictionary.data");
                SuccessMessage = string.Format(_localizationManager.GetString("common.success.update"), entityNameSuccess);
                _operLog?.Information("[DictionaryView] 字典数据更新成功，字典数据Id={Id}, 数据标签={DataLabel}, 数据值={DataValue}",
                    dictionaryData.Id, dictionaryData.DataLabel, dictionaryData.DataValue ?? string.Empty);

                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 字典数据保存失败");
        }
    }

    private bool CanSaveDataInline(DictionaryDataDto? dictionaryData)
    {
        if (dictionaryData == null)
        {
            return EditingDictionaryData is not null;
        }
        return EditingDictionaryData != null && EditingDictionaryData == dictionaryData;
    }

    /// <summary>
    /// 行内编辑：取消编辑（参照 CodeGenFormViewModel.CancelUpdateCommand）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelDataInline))]
    private async Task CancelDataInlineAsync()
    {
        if (EditingDictionaryData != null)
        {
            EditingDictionaryData = null;

            // 通知命令重新评估 CanExecute
            SaveDataInlineCommand.NotifyCanExecuteChanged();
            CancelDataInlineCommand.NotifyCanExecuteChanged();

            await LoadDataAsync(); // 重新加载以恢复原始数据
        }
    }

    private bool CanCancelDataInline()
    {
        return EditingDictionaryData != null;
    }

    /// <summary>
    /// 行内新增：添加新行并进入编辑状态（参照 CodeGenFormViewModel.CreateColumnCommand）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCreateDataInline))]
    private void CreateDataInline()
    {
        if (SelectedDictionaryType == null)
        {
            return;
        }

        // 创建新的字典数据对象
        var newData = new DictionaryDataDto
        {
            TypeCode = SelectedDictionaryType.TypeCode,
            DataLabel = string.Empty,
            I18nKey = string.Empty,
            DataValue = null!,
            ExtLabel = null!,
            ExtValue = null!,
            CssClass = null!,
            ListClass = null!,
            OrderNum = DictionaryDataList.Count > 0 ? DictionaryDataList.Max(d => d.OrderNum) + 1 : 1,
            Remarks = null!
        };

        // 添加到列表
        DictionaryDataList.Add(newData);

        // 设置正在编辑的项，让 TaktInlineEditDataGrid 自动进入编辑状态
        EditingDictionaryData = newData;
        SelectedDictionaryData = newData;

        // 通知命令重新评估 CanExecute
        SaveDataInlineCommand.NotifyCanExecuteChanged();
        CancelDataInlineCommand.NotifyCanExecuteChanged();
        UpdateDataInlineCommand.NotifyCanExecuteChanged();

        // 延迟触发编辑状态，确保 UI 已更新
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            // 通过调用 UpdateDataInlineCommand 来触发编辑状态
            if (UpdateDataInlineCommand.CanExecute(newData))
            {
                UpdateDataInlineCommand.Execute(newData);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        _operLog?.Information("[DictionaryView] 新增字典数据行，字典类型代码={TypeCode}, 序号={OrderNum}",
            SelectedDictionaryType.TypeCode ?? string.Empty, newData.OrderNum);
    }

    private bool CanCreateDataInline()
    {
        return SelectedDictionaryType is not null && EditingDictionaryData == null;
    }
}

