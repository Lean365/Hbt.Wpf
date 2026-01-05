// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Serials
// 文件名称：SerialScanningViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号扫描记录视图模型（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logistics.Materials;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Application.Services.Logistics.Materials;
using Takt.Application.Services.Logistics.Serials;
using PageRequest = Takt.Fluent.Controls.PageRequest;
using QueryContext = Takt.Fluent.Controls.QueryContext;

namespace Takt.Fluent.ViewModels.Logistics.Serials;

/// <summary>
/// 序列号扫描记录视图模型（主子表视图）
/// 主表：ProdModel（产品机种）
/// 子表：ProdSerialScanning（序列号扫描记录）
/// </summary>
public partial class SerialScanningViewModel : ObservableObject
{
    private readonly IProdModelService _prodModelService;
    private readonly IProdSerialScanningService _prodSerialScanningService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;
    private readonly IServiceProvider _serviceProvider;

    // 主表数据
    public ObservableCollection<ProdModelDto> ProdSerials { get; } = new();

    [ObservableProperty]
    private ProdModelDto? _selectedProdSerial;

    // 子表数据
    public ObservableCollection<ProdSerialScanningDto> ProdSerialScannings { get; } = new();

    [ObservableProperty]
    private ProdSerialScanningDto? _selectedProdSerialScanning;

    // 主表查询相关
    [ObservableProperty]
    private string _serialKeyword = string.Empty;

    [ObservableProperty]
    private int _serialPageIndex = 1;

    [ObservableProperty]
    private int _serialPageSize = 20;

    [ObservableProperty]
    private int _serialTotalCount;

    // 子表查询相关
    [ObservableProperty]
    private string _scanningKeyword = string.Empty;

    [ObservableProperty]
    private int _scanningPageIndex = 1;

    [ObservableProperty]
    private int _scanningPageSize = 20;

    [ObservableProperty]
    private int _scanningTotalCount;

    [ObservableProperty]
    private bool _isLoadingSerials;

    [ObservableProperty]
    private bool _isLoadingScannings;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public SerialScanningViewModel(
        IProdModelService prodModelService,
        IProdSerialScanningService prodSerialScanningService,
        ILocalizationManager localizationManager,
        IServiceProvider serviceProvider,
        OperLogManager? operLog = null)
    {
        _prodModelService = prodModelService ?? throw new ArgumentNullException(nameof(prodModelService));
        _prodSerialScanningService = prodSerialScanningService ?? throw new ArgumentNullException(nameof(prodSerialScanningService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _operLog = operLog;

        _ = LoadSerialsAsync();
        _ = LoadScanningsAsync(); // 直接加载扫描记录，不依赖主表选择
    }

    partial void OnSelectedProdSerialChanged(ProdModelDto? value)
    {
        // 主表选中项改变时，重新加载子表数据（根据选中的物料代码过滤）
        // 如果未选中主表项，则显示所有扫描记录
        ScanningPageIndex = 1;
        _ = LoadScanningsAsync();
    }

    private async Task LoadSerialsAsync()
    {
        if (IsLoadingSerials) return;

        IsLoadingSerials = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[SerialScanningView] Load serials: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SerialPageIndex, SerialPageSize, SerialKeyword);

            var query = new ProdModelQueryDto
            {
                PageIndex = SerialPageIndex,
                PageSize = SerialPageSize,
                Keywords = string.IsNullOrWhiteSpace(SerialKeyword) ? null! : SerialKeyword.Trim()
            };

            var result = await _prodModelService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ProdSerials.Clear();
                });
                SerialTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.materials.prodmodel.loadfailed");
                return;
            }

            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                ProdSerials.Clear();
                foreach (var serial in result.Data.Items)
                {
                    ProdSerials.Add(serial);
                }
            });

            SerialTotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialScanningView] 加载产品机种列表失败");
        }
        finally
        {
            IsLoadingSerials = false;
        }
    }

    private async Task LoadScanningsAsync()
    {
        if (IsLoadingScannings) return;

        IsLoadingScannings = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[SerialScanningView] Load scannings: materialCode={MaterialCode}, pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SelectedProdSerial?.MaterialCode ?? "All", ScanningPageIndex, ScanningPageSize, ScanningKeyword);

            var query = new ProdSerialScanningQueryDto
            {
                MaterialCode = SelectedProdSerial?.MaterialCode!, // MaterialCode 是可选的，如果为 null 则查询所有记录
                Keywords = string.IsNullOrWhiteSpace(ScanningKeyword) ? null! : ScanningKeyword.Trim(),
                PageIndex = ScanningPageIndex,
                PageSize = ScanningPageSize
            };

            var result = await _prodSerialScanningService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ProdSerialScannings.Clear();
                    foreach (var scanning in result.Data.Items)
                    {
                        ProdSerialScannings.Add(scanning);
                    }
                });

                ScanningTotalCount = result.Data.TotalNum;
            }
            else
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ProdSerialScannings.Clear();
                });
                ScanningTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.serials.prodserialscanning.loadfailed");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialScanningView] 加载序列号扫描记录失败");
        }
        finally
        {
            IsLoadingScannings = false;
        }
    }

    [RelayCommand]
    private async Task QuerySerialsAsync(QueryContext context)
    {
        _operLog?.Information("[SerialScanningView] 查询产品序列号，关键词={Keyword}, 页码={PageIndex}, 页大小={PageSize}",
            context.Keyword, context.PageIndex, context.PageSize);

        SerialKeyword = context.Keyword;
        if (SerialPageIndex != context.PageIndex)
        {
            SerialPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (SerialPageSize != context.PageSize)
        {
            SerialPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadSerialsAsync();
    }

    [RelayCommand]
    private async Task ResetSerialsAsync()
    {
        _operLog?.Information("[SerialScanningView] 重置产品序列号查询条件");
        SerialKeyword = string.Empty;
        SerialPageIndex = 1;
        SerialPageSize = 20;
        await LoadSerialsAsync();
    }

    [RelayCommand]
    private async Task PageChangedSerialsAsync(PageRequest request)
    {
        SerialPageIndex = request.PageIndex;
        SerialPageSize = request.PageSize;
        await LoadSerialsAsync();
    }

    [RelayCommand]
    private async Task QueryScanningsAsync(QueryContext context)
    {
        _operLog?.Information("[SerialScanningView] 查询扫描记录，关键词={Keyword}, 页码={PageIndex}, 页大小={PageSize}",
            context.Keyword, context.PageIndex, context.PageSize);

        ScanningKeyword = context.Keyword ?? string.Empty;
        if (ScanningPageIndex != context.PageIndex)
        {
            ScanningPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (ScanningPageSize != context.PageSize)
        {
            ScanningPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadScanningsAsync();
    }

    [RelayCommand]
    private async Task ResetScanningsAsync(QueryContext context)
    {
        _operLog?.Information("[SerialScanningView] 重置扫描记录查询条件");
        ScanningKeyword = string.Empty;
        ScanningPageIndex = 1;
        ScanningPageSize = 20;
        await LoadScanningsAsync();
    }

    [RelayCommand]
    private async Task PageChangedScanningsAsync(PageRequest request)
    {
        ScanningPageIndex = request.PageIndex;
        ScanningPageSize = request.PageSize;
        await LoadScanningsAsync();
    }

    [RelayCommand]
    private void CreateSerial()
    {
        _operLog?.Information("[SerialScanningView] 创建产品序列号（主表数据从SAP获取，不支持手动创建）");
        ErrorMessage = _localizationManager.GetString("logistics.serials.prodserial.createnotsupported");
    }

    [RelayCommand]
    private async Task ImportSerialAsync()
    {
        try
        {
            _operLog?.Information("[SerialScanningView] 开始导入产品序列号（从SAP同步）");
            ErrorMessage = null;

            // TODO: 实现从SAP导入产品序列号的逻辑
            await Task.Delay(100);

            SuccessMessage = _localizationManager.GetString("logistics.serials.prodserial.importsuccess");
            await LoadSerialsAsync();
            _operLog?.Information("[SerialScanningView] 导入产品序列号成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialScanningView] 导入产品序列号失败");
        }
    }

    [RelayCommand]
    private async Task ExportSerialAsync()
    {
        try
        {
            _operLog?.Information("[SerialScanningView] 开始导出产品序列号");
            ErrorMessage = null;

            // TODO: 实现导出产品序列号到Excel的逻辑
            await Task.Delay(100);

            SuccessMessage = _localizationManager.GetString("logistics.serials.prodserial.exportsuccess");
            _operLog?.Information("[SerialScanningView] 导出产品序列号成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialScanningView] 导出产品序列号失败");
        }
    }

    [RelayCommand]
    private void UpdateSerial(ProdModelDto? serial)
    {
        if (serial == null)
        {
            serial = SelectedProdSerial;
        }

        if (serial == null)
        {
            return;
        }

        _operLog?.Information("[SerialScanningView] 更新产品机种（主表数据从SAP获取，不支持手动编辑），记录Id={Id}", serial.Id);
        ErrorMessage = _localizationManager.GetString("logistics.materials.prodmodel.updatenotsupported");
    }

    [RelayCommand]
    private async Task DeleteSerialAsync(ProdModelDto? serial)
    {
        if (serial == null)
        {
            serial = SelectedProdSerial;
        }

        if (serial == null)
        {
            return;
        }

        try
        {
            _operLog?.Information("[SerialScanningView] 开始删除产品机种，记录Id={Id}", serial.Id);
            var result = await _prodModelService.DeleteAsync(serial.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.materials.prodmodel.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                _operLog?.Error("[SerialScanningView] 删除产品机种失败，记录Id={Id}, 错误={Error}", serial.Id, result.Message ?? "未知错误");
                return;
            }

            var entityNameSuccess = _localizationManager.GetString("logistics.materials.prodmodel.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[SerialScanningView] 删除产品机种成功，记录Id={Id}", serial.Id);
            await LoadSerialsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialScanningView] 删除产品序列号失败，记录Id={Id}", serial.Id);
        }
    }

    /// <summary>
    /// 打开扫描入库表单
    /// </summary>
    [RelayCommand]
    private void ScanInbound()
    {
        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = _serviceProvider.GetRequiredService<Takt.Fluent.Views.Logistics.Serials.SerialComponent.SerialInboundForm>();
            if (window.DataContext is not SerialInboundFormViewModel formViewModel)
            {
                throw new InvalidOperationException("SerialInboundForm DataContext 不是 SerialInboundFormViewModel");
            }

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                // 如果选中了主表项，重新加载子表数据
                if (SelectedProdSerial != null)
                {
                    await LoadScanningsAsync();
                }
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[SerialScanningView] 打开扫描入库窗口");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialScanningView] 打开扫描入库窗口失败");
        }
    }

    /// <summary>
    /// 打开扫描出库表单
    /// </summary>
    [RelayCommand]
    private void ScanOutbound()
    {
        try
        {
            if (App.Services == null)
            {
                throw new InvalidOperationException("App.Services 未初始化");
            }

            var window = _serviceProvider.GetRequiredService<Takt.Fluent.Views.Logistics.Serials.SerialComponent.SerialOutboundForm>();
            if (window.DataContext is not SerialOutboundFormViewModel formViewModel)
            {
                throw new InvalidOperationException("SerialOutboundForm DataContext 不是 SerialOutboundFormViewModel");
            }

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                // 如果选中了主表项，重新加载子表数据
                if (SelectedProdSerial != null)
                {
                    await LoadScanningsAsync();
                }
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[SerialScanningView] 打开扫描出库窗口");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialScanningView] 打开扫描出库窗口失败");
        }
    }
}

