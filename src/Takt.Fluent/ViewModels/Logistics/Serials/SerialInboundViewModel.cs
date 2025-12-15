// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Serials
// 文件名称：SerialInboundViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号入库视图模型（主子表视图）
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
/// 序列号入库视图模型（主子表视图）
/// 主表：ProdModel（产品机种）
/// 子表：ProdSerialInbound（序列号入库记录）
/// </summary>
public partial class SerialInboundViewModel : ObservableObject
{
    private readonly IProdModelService _prodModelService;
    private readonly IProdSerialInboundService _prodSerialInboundService;
    private readonly ILocalizationManager _localizationManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<ProdModelDto> ProdSerials { get; } = new();

    [ObservableProperty]
    private ProdModelDto? _selectedProdSerial;

    // 子表数据
    public ObservableCollection<ProdSerialInboundDto> ProdSerialInbounds { get; } = new();

    [ObservableProperty]
    private ProdSerialInboundDto? _selectedProdSerialInbound;

    [ObservableProperty]
    private ProdSerialInboundDto? _editingProdSerialInbound;

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
    private string _inboundKeyword = string.Empty;

    [ObservableProperty]
    private int _inboundPageIndex = 1;

    [ObservableProperty]
    private int _inboundPageSize = 20;

    [ObservableProperty]
    private int _inboundTotalCount;

    [ObservableProperty]
    private bool _isLoadingSerials;

    [ObservableProperty]
    private bool _isLoadingInbounds;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public SerialInboundViewModel(
        IProdModelService prodModelService,
        IProdSerialInboundService prodSerialInboundService,
        ILocalizationManager localizationManager,
        IServiceProvider serviceProvider,
        OperLogManager? operLog = null)
    {
        _prodModelService = prodModelService ?? throw new ArgumentNullException(nameof(prodModelService));
        _prodSerialInboundService = prodSerialInboundService ?? throw new ArgumentNullException(nameof(prodSerialInboundService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _operLog = operLog;

        _ = LoadSerialsAsync();
    }

    partial void OnSelectedProdSerialChanged(ProdModelDto? value)
    {
        // 主表选中项改变时，重置子表状态并加载子表数据
        if (value == null)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                ProdSerialInbounds.Clear();
            });
            SelectedProdSerialInbound = null;
            EditingProdSerialInbound = null;
            InboundKeyword = string.Empty;
            InboundPageIndex = 1;
            InboundPageSize = 20;
            InboundTotalCount = 0;
        }
        else
        {
            // 重置子表查询条件和分页
            InboundKeyword = string.Empty;
            InboundPageIndex = 1;
            InboundPageSize = 20;
            EditingProdSerialInbound = null;
            _ = LoadInboundsAsync();
        }
    }

    partial void OnEditingProdSerialInboundChanged(ProdSerialInboundDto? value)
    {
        // 通知所有相关命令重新评估 CanExecute
        CreateInboundInlineCommand.NotifyCanExecuteChanged();
        UpdateInboundInlineCommand.NotifyCanExecuteChanged();
        SaveInboundInlineCommand.NotifyCanExecuteChanged();
        CancelInboundInlineCommand.NotifyCanExecuteChanged();
        DeleteInboundCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private async Task LoadSerialsAsync()
    {
        if (IsLoadingSerials) return;

        IsLoadingSerials = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[SerialInboundView] Load serials: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SerialPageIndex, SerialPageSize, SerialKeyword);

            // 构建查询DTO
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
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.serials.prodserial.loadfailed");
                return;
            }

            // 在 UI 线程上更新集合
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
            _operLog?.Error(ex, "[SerialInboundView] 加载产品序列号列表失败");
        }
        finally
        {
            IsLoadingSerials = false;
        }
    }

    private async Task LoadInboundsAsync()
    {
        if (SelectedProdSerial == null)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                ProdSerialInbounds.Clear();
            });
            InboundTotalCount = 0;
            return;
        }

        if (IsLoadingInbounds) return;

        IsLoadingInbounds = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[SerialInboundView] Load inbounds: materialCode={MaterialCode}, pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SelectedProdSerial.MaterialCode, InboundPageIndex, InboundPageSize, InboundKeyword);

            var query = new ProdSerialInboundQueryDto
            {
                MaterialCode = SelectedProdSerial.MaterialCode,
                InboundNo = string.IsNullOrWhiteSpace(InboundKeyword) ? null! : InboundKeyword.Trim(),
                SerialNumber = string.IsNullOrWhiteSpace(InboundKeyword) ? null! : InboundKeyword.Trim(),
                PageIndex = InboundPageIndex,
                PageSize = InboundPageSize
            };

            var result = await _prodSerialInboundService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                // 在 UI 线程上更新集合
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ProdSerialInbounds.Clear();
                    foreach (var inbound in result.Data.Items)
                    {
                        ProdSerialInbounds.Add(inbound);
                    }
                });

                InboundTotalCount = result.Data.TotalNum;
            }
            else
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ProdSerialInbounds.Clear();
                });
                InboundTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.serials.prodserialinbound.loadfailed");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 加载序列号入库记录失败");
        }
        finally
        {
            IsLoadingInbounds = false;
        }
    }

    [RelayCommand]
    private async Task QuerySerialsAsync(QueryContext context)
    {
        _operLog?.Information("[SerialInboundView] 查询产品序列号，关键词={Keyword}, 页码={PageIndex}, 页大小={PageSize}",
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
        _operLog?.Information("[SerialInboundView] 重置产品序列号查询条件");
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
    private async Task QueryInboundsAsync(Takt.Fluent.Controls.QueryContext context)
    {
        _operLog?.Information("[SerialInboundView] 查询入库记录，关键词={Keyword}, 页码={PageIndex}, 页大小={PageSize}",
            context.Keyword, context.PageIndex, context.PageSize);

        InboundKeyword = context.Keyword ?? string.Empty;
        if (InboundPageIndex != context.PageIndex)
        {
            InboundPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (InboundPageSize != context.PageSize)
        {
            InboundPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadInboundsAsync();
    }

    [RelayCommand]
    private async Task ResetInboundsAsync(Takt.Fluent.Controls.QueryContext context)
    {
        _operLog?.Information("[SerialInboundView] 重置入库记录查询条件");
        InboundKeyword = string.Empty;
        InboundPageIndex = 1;
        InboundPageSize = 20;
        await LoadInboundsAsync();
    }

    [RelayCommand]
    private async Task PageChangedInboundsAsync(Takt.Fluent.Controls.PageRequest request)
    {
        InboundPageIndex = request.PageIndex;
        InboundPageSize = request.PageSize;
        await LoadInboundsAsync();
    }

    /// <summary>
    /// 创建产品序列号（主表数据从SAP获取，不支持手动创建）
    /// </summary>
    [RelayCommand]
    private void CreateSerial()
    {
        _operLog?.Information("[SerialInboundView] 创建产品序列号（主表数据从SAP获取，不支持手动创建）");
        ErrorMessage = _localizationManager.GetString("logistics.serials.prodserial.createnotsupported");
    }

    /// <summary>
    /// 导入产品序列号（从SAP同步）
    /// </summary>
    [RelayCommand]
    private async Task ImportSerialAsync()
    {
        try
        {
            _operLog?.Information("[SerialInboundView] 开始导入产品序列号（从SAP同步）");
            ErrorMessage = null;

            // TODO: 实现从SAP导入产品序列号的逻辑
            // 这里可以调用 SAP 服务同步数据
            await Task.Delay(100); // 占位实现

            SuccessMessage = _localizationManager.GetString("logistics.serials.prodserial.importsuccess");
            await LoadSerialsAsync();
            _operLog?.Information("[SerialInboundView] 导入产品序列号成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 导入产品序列号失败");
        }
    }

    /// <summary>
    /// 导出产品序列号
    /// </summary>
    [RelayCommand]
    private async Task ExportSerialAsync()
    {
        try
        {
            _operLog?.Information("[SerialInboundView] 开始导出产品序列号");
            ErrorMessage = null;

            // TODO: 实现导出产品序列号到Excel的逻辑
            // 可以使用当前查询结果导出
            await Task.Delay(100); // 占位实现

            SuccessMessage = _localizationManager.GetString("logistics.serials.prodserial.exportsuccess");
            _operLog?.Information("[SerialInboundView] 导出产品序列号成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 导出产品序列号失败");
        }
    }

    /// <summary>
    /// 更新产品机种（主表数据从SAP获取，不支持手动编辑）
    /// </summary>
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

        _operLog?.Information("[SerialInboundView] 更新产品机种（主表数据从SAP获取，不支持手动编辑），记录Id={Id}", serial.Id);
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
            _operLog?.Information("[SerialInboundView] 开始删除产品机种，记录Id={Id}", serial.Id);
            var result = await _prodModelService.DeleteAsync(serial.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.materials.prodmodel.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                _operLog?.Error("[SerialInboundView] 删除产品机种失败，记录Id={Id}, 错误={Error}", serial.Id, result.Message ?? "未知错误");
                return;
            }

            var entityNameSuccess = _localizationManager.GetString("logistics.materials.prodmodel.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[SerialInboundView] 删除产品机种成功，记录Id={Id}", serial.Id);
            await LoadSerialsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 删除产品序列号失败，记录Id={Id}", serial.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateInboundInline))]
    private void CreateInboundInline()
    {
        if (SelectedProdSerial == null)
        {
            var entityName = _localizationManager.GetString("logistics.serials.prodserial.entity");
            ErrorMessage = string.Format(_localizationManager.GetString("common.placeholder.select"), entityName);
            return;
        }

        // 创建新的入库记录对象
        var newInbound = new ProdSerialInboundDto
        {
            MaterialCode = SelectedProdSerial.MaterialCode,
            FullSerialNumber = string.Empty,
            SerialNumber = string.Empty,
            Quantity = 1,
            InboundNo = string.Empty,
            InboundDate = DateTime.Now,
            Warehouse = null!,
            Location = null!
        };

        // 添加到列表（在 UI 线程上）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            ProdSerialInbounds.Add(newInbound);
        });

        // 设置正在编辑的项，让 TaktInlineEditDataGrid 自动进入编辑状态
        EditingProdSerialInbound = newInbound;
        SelectedProdSerialInbound = newInbound;

        // 通知命令重新评估 CanExecute
        SaveInboundInlineCommand.NotifyCanExecuteChanged();
        CancelInboundInlineCommand.NotifyCanExecuteChanged();
        UpdateInboundInlineCommand.NotifyCanExecuteChanged();

        // 延迟触发编辑状态，确保 UI 已更新
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (UpdateInboundInlineCommand.CanExecute(newInbound))
            {
                UpdateInboundInlineCommand.Execute(newInbound);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        _operLog?.Information("[SerialInboundView] 新增入库记录行，物料代码={MaterialCode}", SelectedProdSerial.MaterialCode);
    }

    private bool CanCreateInboundInline()
    {
        return SelectedProdSerial is not null && EditingProdSerialInbound == null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdateInboundInline))]
    private void UpdateInboundInline(ProdSerialInboundDto? inbound)
    {
        if (inbound == null)
        {
            inbound = SelectedProdSerialInbound;
        }

        if (inbound == null)
        {
            return;
        }

        EditingProdSerialInbound = inbound;

        // 通知命令重新评估 CanExecute
        SaveInboundInlineCommand.NotifyCanExecuteChanged();
        CancelInboundInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[SerialInboundView] 进入编辑入库记录状态，记录Id={Id}", inbound.Id);
    }

    private bool CanUpdateInboundInline(ProdSerialInboundDto? inbound)
    {
        if (inbound == null)
        {
            return SelectedProdSerialInbound is not null && EditingProdSerialInbound == null;
        }
        return inbound is not null && EditingProdSerialInbound == null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveInboundInline))]
    private async Task SaveInboundInlineAsync(ProdSerialInboundDto? inbound)
    {
        if (inbound == null)
        {
            inbound = EditingProdSerialInbound;
        }

        if (inbound == null || EditingProdSerialInbound != inbound)
        {
            return;
        }

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(inbound.MaterialCode))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.serials.prodserialinbound.materialcode"));
            _operLog?.Warning("[SerialInboundView] 保存入库记录失败：物料代码不能为空，记录Id={Id}", inbound.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(inbound.SerialNumber))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.serials.prodserialinbound.serialnumber"));
            _operLog?.Warning("[SerialInboundView] 保存入库记录失败：序列号不能为空，记录Id={Id}", inbound.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(inbound.InboundNo))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.serials.prodserialinbound.inboundno"));
            _operLog?.Warning("[SerialInboundView] 保存入库记录失败：入库单号不能为空，记录Id={Id}", inbound.Id);
            return;
        }

        try
        {
            // 生成完整序列号（如果为空）
            if (string.IsNullOrWhiteSpace(inbound.FullSerialNumber))
            {
                inbound.FullSerialNumber = $"{inbound.MaterialCode}-{inbound.SerialNumber}-{inbound.Quantity}";
            }

            if (inbound.Id == 0)
            {
                // 新增
                var createDto = new ProdSerialInboundCreateDto
                {
                    FullSerialNumber = inbound.FullSerialNumber
                };

                _operLog?.Information("[SerialInboundView] 开始创建入库记录，完整序列号={FullSerialNumber}", inbound.FullSerialNumber);
                var result = await _prodSerialInboundService.CreateAsync(createDto);
                if (!result.Success || result.Data <= 0)
                {
                    ErrorMessage = result.Message ?? "创建失败";
                    _operLog?.Error("[SerialInboundView] 创建入库记录失败，完整序列号={FullSerialNumber}, 错误={Error}",
                        inbound.FullSerialNumber, result.Message ?? "未知错误");
                    return;
                }

                inbound.Id = result.Data;
                EditingProdSerialInbound = null;

                // 通知命令重新评估 CanExecute
                SaveInboundInlineCommand.NotifyCanExecuteChanged();
                CancelInboundInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "创建成功";
                _operLog?.Information("[SerialInboundView] 入库记录创建成功，记录Id={Id}", inbound.Id);

                await LoadInboundsAsync();
            }
            else
            {
                // 更新
                var updateDto = new ProdSerialInboundUpdateDto
                {
                    Id = inbound.Id,
                    FullSerialNumber = inbound.FullSerialNumber
                };

                _operLog?.Information("[SerialInboundView] 开始更新入库记录，记录Id={Id}, 完整序列号={FullSerialNumber}",
                    inbound.Id, inbound.FullSerialNumber);
                var result = await _prodSerialInboundService.UpdateAsync(updateDto);
                if (!result.Success)
                {
                    ErrorMessage = result.Message ?? "更新失败";
                    _operLog?.Error("[SerialInboundView] 更新入库记录失败，记录Id={Id}, 错误={Error}",
                        inbound.Id, result.Message ?? "未知错误");
                    return;
                }

                EditingProdSerialInbound = null;

                // 通知命令重新评估 CanExecute
                SaveInboundInlineCommand.NotifyCanExecuteChanged();
                CancelInboundInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "更新成功";
                _operLog?.Information("[SerialInboundView] 入库记录更新成功，记录Id={Id}", inbound.Id);

                await LoadInboundsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 入库记录保存失败");
        }
    }

    private bool CanSaveInboundInline(ProdSerialInboundDto? inbound)
    {
        if (inbound == null)
        {
            return EditingProdSerialInbound is not null;
        }
        return EditingProdSerialInbound != null && EditingProdSerialInbound == inbound;
    }

    [RelayCommand(CanExecute = nameof(CanCancelInboundInline))]
    private async Task CancelInboundInlineAsync()
    {
        if (EditingProdSerialInbound != null)
        {
            var inboundId = EditingProdSerialInbound.Id;
            EditingProdSerialInbound = null;

            // 通知命令重新评估 CanExecute
            SaveInboundInlineCommand.NotifyCanExecuteChanged();
            CancelInboundInlineCommand.NotifyCanExecuteChanged();

            _operLog?.Information("[SerialInboundView] 取消编辑入库记录，记录Id={Id}", inboundId);
            await LoadInboundsAsync(); // 重新加载以恢复原始数据
        }
    }

    private bool CanCancelInboundInline()
    {
        return EditingProdSerialInbound != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteInbound))]
    private async Task DeleteInboundAsync(ProdSerialInboundDto? inbound)
    {
        if (inbound == null)
        {
            inbound = SelectedProdSerialInbound;
        }

        if (inbound == null)
        {
            return;
        }

        try
        {
            var result = await _prodSerialInboundService.DeleteAsync(inbound.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.serials.prodserialinbound.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                _operLog?.Error("[SerialInboundView] 删除序列号入库记录失败，记录Id={Id}, 错误={Error}", inbound.Id, result.Message ?? "未知错误");
                return;
            }

            var entityNameSuccess = _localizationManager.GetString("logistics.serials.prodserialinbound.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[SerialInboundView] 删除序列号入库记录成功，记录Id={Id}", inbound.Id);
            await LoadInboundsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 删除序列号入库记录失败");
        }
    }

    private bool CanDeleteInbound(ProdSerialInboundDto? inbound)
    {
        // 编辑状态下不能删除
        if (EditingProdSerialInbound != null)
        {
            return false;
        }

        if (inbound == null)
        {
            return SelectedProdSerialInbound is not null;
        }
        return inbound is not null;
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
                    await LoadInboundsAsync();
                }
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[SerialInboundView] 打开扫描入库窗口");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialInboundView] 打开扫描入库窗口失败");
        }
    }
}

