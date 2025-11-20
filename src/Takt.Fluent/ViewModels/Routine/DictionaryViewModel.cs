// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Routine
// 文件名称：DictionaryViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典管理视图模型（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Routine;
using Takt.Application.Services.Routine;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

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
                Keywords = string.IsNullOrWhiteSpace(TypeKeyword) ? null : TypeKeyword.Trim()
            };

            var result = await _dictionaryTypeService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                DictionaryTypes.Clear();
                TypeTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Routine.Dictionary.LoadTypesFailed") ?? "加载字典类型失败";
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
                Keywords = string.IsNullOrWhiteSpace(DataKeyword) ? null : DataKeyword.Trim(),
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
                ErrorMessage = result.Message ?? _localizationManager.GetString("Routine.Dictionary.LoadDataFailed") ?? "加载字典数据失败";
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
        TypeKeyword = context.Keyword;
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
        TypeKeyword = string.Empty;
        TypePageIndex = 1;
        await LoadTypesAsync();
    }

    [RelayCommand]
    private async Task PageChangedTypesAsync(PageRequest request)
    {
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
        DataKeyword = context.Keyword;
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
        DataKeyword = string.Empty;
        DataPageIndex = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task PageChangedDataAsync(PageRequest request)
    {
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

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.DictionaryComponent.DictionaryTypeForm>();
            if (window.DataContext is not DictionaryTypeFormViewModel formViewModel)
            {
                throw new InvalidOperationException("DictionaryTypeForm DataContext 不是 DictionaryTypeFormViewModel");
            }

            formViewModel.ForUpdate(dictionaryType);

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadTypesAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
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
            var confirmMessage = _localizationManager.GetString("Routine.Dictionary.DeleteTypeConfirm") ?? "确定要删除该字典类型吗？删除后关联的字典数据也会被删除。";
            var confirmTitle = _localizationManager.GetString("common.confirm") ?? "确认";
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
                ErrorMessage = deleteResult.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
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

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.DictionaryComponent.DictionaryTypeForm>();
            if (window.DataContext is not DictionaryTypeFormViewModel formViewModel)
            {
                throw new InvalidOperationException("DictionaryTypeForm DataContext 不是 DictionaryTypeFormViewModel");
            }

            formViewModel.ForCreate();

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadTypesAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 打开新建字典类型窗口失败");
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdateData))]
    private void UpdateData(DictionaryDataDto? dictionaryData)
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
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.DictionaryComponent.DictionaryDataForm>();
            if (window.DataContext is not DictionaryDataFormViewModel formViewModel)
            {
                throw new InvalidOperationException("DictionaryDataForm DataContext 不是 DictionaryDataFormViewModel");
            }

            formViewModel.ForUpdate(dictionaryData);

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadDataAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 打开编辑字典数据窗口失败");
        }
    }

    private bool CanUpdateData(DictionaryDataDto? dictionaryData)
    {
        if (dictionaryData == null)
        {
            return SelectedDictionaryData is not null;
        }
        return dictionaryData is not null;
    }

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
            var confirmMessage = _localizationManager.GetString("Routine.Dictionary.DeleteDataConfirm") ?? "确定要删除该字典数据吗？";
            var confirmTitle = _localizationManager.GetString("common.confirm") ?? "确认";
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
                ErrorMessage = deleteResult.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 删除字典数据失败");
        }
    }

    private bool CanDeleteData(DictionaryDataDto? dictionaryData) => dictionaryData is not null || SelectedDictionaryData is not null;

    [RelayCommand(CanExecute = nameof(CanCreateData))]
    private void CreateData()
    {
        if (SelectedDictionaryType == null)
        {
            return;
        }

        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = App.Services.GetRequiredService<Takt.Fluent.Views.Routine.DictionaryComponent.DictionaryDataForm>();
            if (window.DataContext is not DictionaryDataFormViewModel formViewModel)
            {
                throw new InvalidOperationException("DictionaryDataForm DataContext 不是 DictionaryDataFormViewModel");
            }

            formViewModel.ForCreate(SelectedDictionaryType.TypeCode);

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadDataAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[DictionaryView] 打开新建字典数据窗口失败");
        }
    }

    private bool CanCreateData() => SelectedDictionaryType is not null;

    partial void OnSelectedDictionaryTypeChanged(DictionaryTypeDto? value)
    {
        UpdateTypeCommand.NotifyCanExecuteChanged();
        DeleteTypeCommand.NotifyCanExecuteChanged();
        CreateDataCommand.NotifyCanExecuteChanged();

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
        UpdateDataCommand.NotifyCanExecuteChanged();
        DeleteDataCommand.NotifyCanExecuteChanged();
    }
}

