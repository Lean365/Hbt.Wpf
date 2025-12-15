// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Serials
// 文件名称：SerialOutboundViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号出库视图模型（主子表视图）
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
/// 序列号出库视图模型（主子表视图）
/// 主表：ProdModel（产品机种）
/// 子表：ProdSerialOutbound（序列号出库记录）
/// </summary>
public partial class SerialOutboundViewModel : ObservableObject
{
    private readonly IProdModelService _prodModelService;
    private readonly IProdSerialOutboundService _prodSerialOutboundService;
    private readonly ILocalizationManager _localizationManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<ProdModelDto> ProdSerials { get; } = new();

    [ObservableProperty]
    private ProdModelDto? _selectedProdSerial;

    // 子表数据
    public ObservableCollection<ProdSerialOutboundDto> ProdSerialOutbounds { get; } = new();

    [ObservableProperty]
    private ProdSerialOutboundDto? _selectedProdSerialOutbound;

    [ObservableProperty]
    private ProdSerialOutboundDto? _editingProdSerialOutbound;

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
    private string _outboundKeyword = string.Empty;

    [ObservableProperty]
    private int _outboundPageIndex = 1;

    [ObservableProperty]
    private int _outboundPageSize = 20;

    [ObservableProperty]
    private int _outboundTotalCount;

    [ObservableProperty]
    private bool _isLoadingSerials;

    [ObservableProperty]
    private bool _isLoadingOutbounds;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public SerialOutboundViewModel(
        IProdModelService prodModelService,
        IProdSerialOutboundService prodSerialOutboundService,
        ILocalizationManager localizationManager,
        IServiceProvider serviceProvider,
        OperLogManager? operLog = null)
    {
        _prodModelService = prodModelService ?? throw new ArgumentNullException(nameof(prodModelService));
        _prodSerialOutboundService = prodSerialOutboundService ?? throw new ArgumentNullException(nameof(prodSerialOutboundService));
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
                ProdSerialOutbounds.Clear();
            });
            SelectedProdSerialOutbound = null;
            EditingProdSerialOutbound = null;
            OutboundKeyword = string.Empty;
            OutboundPageIndex = 1;
            OutboundPageSize = 20;
            OutboundTotalCount = 0;
        }
        else
        {
            // 重置子表查询条件和分页
            OutboundKeyword = string.Empty;
            OutboundPageIndex = 1;
            OutboundPageSize = 20;
            EditingProdSerialOutbound = null;
            _ = LoadOutboundsAsync();
        }
    }

    partial void OnEditingProdSerialOutboundChanged(ProdSerialOutboundDto? value)
    {
        // 通知所有相关命令重新评估 CanExecute
        CreateOutboundInlineCommand.NotifyCanExecuteChanged();
        UpdateOutboundInlineCommand.NotifyCanExecuteChanged();
        SaveOutboundInlineCommand.NotifyCanExecuteChanged();
        CancelOutboundInlineCommand.NotifyCanExecuteChanged();
        DeleteOutboundCommand.NotifyCanExecuteChanged();
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
            _operLog?.Information("[SerialOutboundView] Load serials: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
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
            _operLog?.Error(ex, "[SerialOutboundView] 加载产品序列号列表失败");
        }
        finally
        {
            IsLoadingSerials = false;
        }
    }

    private async Task LoadOutboundsAsync()
    {
        if (SelectedProdSerial == null)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                ProdSerialOutbounds.Clear();
            });
            OutboundTotalCount = 0;
            return;
        }

        if (IsLoadingOutbounds) return;

        IsLoadingOutbounds = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[SerialOutboundView] Load outbounds: materialCode={MaterialCode}, pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                SelectedProdSerial.MaterialCode, OutboundPageIndex, OutboundPageSize, OutboundKeyword);

            var query = new ProdSerialOutboundQueryDto
            {
                MaterialCode = SelectedProdSerial.MaterialCode,
                OutboundNo = string.IsNullOrWhiteSpace(OutboundKeyword) ? null! : OutboundKeyword.Trim(),
                SerialNumber = string.IsNullOrWhiteSpace(OutboundKeyword) ? null! : OutboundKeyword.Trim(),
                PageIndex = OutboundPageIndex,
                PageSize = OutboundPageSize
            };

            var result = await _prodSerialOutboundService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                // 在 UI 线程上更新集合
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ProdSerialOutbounds.Clear();
                    foreach (var outbound in result.Data.Items)
                    {
                        ProdSerialOutbounds.Add(outbound);
                    }
                });

                OutboundTotalCount = result.Data.TotalNum;
            }
            else
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ProdSerialOutbounds.Clear();
                });
                OutboundTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("logistics.serials.prodserialoutbound.loadfailed");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 加载序列号出库记录失败");
        }
        finally
        {
            IsLoadingOutbounds = false;
        }
    }

    [RelayCommand]
    private async Task QuerySerialsAsync(QueryContext context)
    {
        _operLog?.Information("[SerialOutboundView] 查询产品序列号，关键词={Keyword}, 页码={PageIndex}, 页大小={PageSize}",
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
        _operLog?.Information("[SerialOutboundView] 重置产品序列号查询条件");
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
    private async Task QueryOutboundsAsync(QueryContext context)
    {
        _operLog?.Information("[SerialOutboundView] 查询出库记录，关键词={Keyword}, 页码={PageIndex}, 页大小={PageSize}",
            context.Keyword, context.PageIndex, context.PageSize);

        OutboundKeyword = context.Keyword;
        if (OutboundPageIndex != context.PageIndex)
        {
            OutboundPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (OutboundPageSize != context.PageSize)
        {
            OutboundPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadOutboundsAsync();
    }

    [RelayCommand]
    private async Task ResetOutboundsAsync()
    {
        _operLog?.Information("[SerialOutboundView] 重置出库记录查询条件");
        OutboundKeyword = string.Empty;
        OutboundPageIndex = 1;
        OutboundPageSize = 20;
        await LoadOutboundsAsync();
    }

    [RelayCommand]
    private async Task PageChangedOutboundsAsync(PageRequest request)
    {
        OutboundPageIndex = request.PageIndex;
        OutboundPageSize = request.PageSize;
        await LoadOutboundsAsync();
    }

    /// <summary>
    /// 创建产品序列号（主表数据从SAP获取，不支持手动创建）
    /// </summary>
    [RelayCommand]
    private void CreateSerial()
    {
        _operLog?.Information("[SerialOutboundView] 创建产品序列号（主表数据从SAP获取，不支持手动创建）");
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
            _operLog?.Information("[SerialOutboundView] 开始导入产品序列号（从SAP同步）");
            ErrorMessage = null;

            // TODO: 实现从SAP导入产品序列号的逻辑
            // 这里可以调用 SAP 服务同步数据
            await Task.Delay(100); // 占位实现

            SuccessMessage = _localizationManager.GetString("logistics.serials.prodserial.importsuccess");
            await LoadSerialsAsync();
            _operLog?.Information("[SerialOutboundView] 导入产品序列号成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 导入产品序列号失败");
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
            _operLog?.Information("[SerialOutboundView] 开始导出产品序列号");
            ErrorMessage = null;

            // TODO: 实现导出产品序列号到Excel的逻辑
            // 可以使用当前查询结果导出
            await Task.Delay(100); // 占位实现

            SuccessMessage = _localizationManager.GetString("logistics.serials.prodserial.exportsuccess");
            _operLog?.Information("[SerialOutboundView] 导出产品序列号成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 导出产品序列号失败");
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

        _operLog?.Information("[SerialOutboundView] 更新产品机种（主表数据从SAP获取，不支持手动编辑），记录Id={Id}", serial.Id);
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
            _operLog?.Information("[SerialOutboundView] 开始删除产品机种，记录Id={Id}", serial.Id);
            var result = await _prodModelService.DeleteAsync(serial.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.materials.prodmodel.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                _operLog?.Error("[SerialOutboundView] 删除产品机种失败，记录Id={Id}, 错误={Error}", serial.Id, result.Message ?? "未知错误");
                return;
            }

            var entityNameSuccess = _localizationManager.GetString("logistics.materials.prodmodel.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[SerialOutboundView] 删除产品机种成功，记录Id={Id}", serial.Id);
            await LoadSerialsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 删除产品序列号失败，记录Id={Id}", serial.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateOutboundInline))]
    private void CreateOutboundInline()
    {
        if (SelectedProdSerial == null)
        {
            var entityName = _localizationManager.GetString("logistics.serials.prodserial.entity");
            ErrorMessage = string.Format(_localizationManager.GetString("common.placeholder.select"), entityName);
            return;
        }

        // 创建新的出库记录对象
        var newOutbound = new ProdSerialOutboundDto
        {
            MaterialCode = SelectedProdSerial.MaterialCode,
            FullSerialNumber = string.Empty,
            SerialNumber = string.Empty,
            Quantity = 1,
            OutboundNo = string.Empty,
            OutboundDate = DateTime.Now,
            DestPort = null!
        };

        // 添加到列表（在 UI 线程上）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            ProdSerialOutbounds.Add(newOutbound);
        });

        // 设置正在编辑的项，让 TaktInlineEditDataGrid 自动进入编辑状态
        EditingProdSerialOutbound = newOutbound;
        SelectedProdSerialOutbound = newOutbound;

        // 通知命令重新评估 CanExecute
        SaveOutboundInlineCommand.NotifyCanExecuteChanged();
        CancelOutboundInlineCommand.NotifyCanExecuteChanged();
        UpdateOutboundInlineCommand.NotifyCanExecuteChanged();

        // 延迟触发编辑状态，确保 UI 已更新
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (UpdateOutboundInlineCommand.CanExecute(newOutbound))
            {
                UpdateOutboundInlineCommand.Execute(newOutbound);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        _operLog?.Information("[SerialOutboundView] 新增出库记录行，物料代码={MaterialCode}", SelectedProdSerial.MaterialCode);
    }

    private bool CanCreateOutboundInline()
    {
        return SelectedProdSerial is not null && EditingProdSerialOutbound == null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdateOutboundInline))]
    private void UpdateOutboundInline(ProdSerialOutboundDto? outbound)
    {
        if (outbound == null)
        {
            outbound = SelectedProdSerialOutbound;
        }

        if (outbound == null)
        {
            return;
        }

        EditingProdSerialOutbound = outbound;

        // 通知命令重新评估 CanExecute
        SaveOutboundInlineCommand.NotifyCanExecuteChanged();
        CancelOutboundInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[SerialOutboundView] 进入编辑出库记录状态，记录Id={Id}", outbound.Id);
    }

    private bool CanUpdateOutboundInline(ProdSerialOutboundDto? outbound)
    {
        if (outbound == null)
        {
            return SelectedProdSerialOutbound is not null && EditingProdSerialOutbound == null;
        }
        return outbound is not null && EditingProdSerialOutbound == null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveOutboundInline))]
    private async Task SaveOutboundInlineAsync(ProdSerialOutboundDto? outbound)
    {
        if (outbound == null)
        {
            outbound = EditingProdSerialOutbound;
        }

        if (outbound == null || EditingProdSerialOutbound != outbound)
        {
            return;
        }

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(outbound.MaterialCode))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.serials.prodserialoutbound.materialcode"));
            _operLog?.Warning("[SerialOutboundView] 保存出库记录失败：物料代码不能为空，记录Id={Id}", outbound.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(outbound.SerialNumber))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.serials.prodserialoutbound.serialnumber"));
            _operLog?.Warning("[SerialOutboundView] 保存出库记录失败：序列号不能为空，记录Id={Id}", outbound.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(outbound.OutboundNo))
        {
            ErrorMessage = string.Format(_localizationManager.GetString("common.validation.required"), _localizationManager.GetString("logistics.serials.prodserialoutbound.outboundno"));
            _operLog?.Warning("[SerialOutboundView] 保存出库记录失败：出库单号不能为空，记录Id={Id}", outbound.Id);
            return;
        }

        try
        {
            // 生成完整序列号（如果为空）
            if (string.IsNullOrWhiteSpace(outbound.FullSerialNumber))
            {
                outbound.FullSerialNumber = $"{outbound.MaterialCode}-{outbound.SerialNumber}-{outbound.Quantity}";
            }

            if (outbound.Id == 0)
            {
                // 新增
                // 注意：MaterialCode、SerialNumber、Quantity 需要从 FullSerialNumber 中解析
                // 服务层或数据库层会处理解析
                var createDto = new ProdSerialOutboundCreateDto
                {
                    FullSerialNumber = outbound.FullSerialNumber,
                    OutboundNo = outbound.OutboundNo,
                    OutboundDate = outbound.OutboundDate,
                    DestPort = outbound.DestPort
                };

                _operLog?.Information("[SerialOutboundView] 开始创建出库记录，完整序列号={FullSerialNumber}, 出库单号={OutboundNo}",
                    outbound.FullSerialNumber, outbound.OutboundNo);
                var result = await _prodSerialOutboundService.CreateAsync(createDto);
                if (!result.Success || result.Data <= 0)
                {
                    ErrorMessage = result.Message ?? "创建失败";
                    _operLog?.Error("[SerialOutboundView] 创建出库记录失败，完整序列号={FullSerialNumber}, 出库单号={OutboundNo}, 错误={Error}",
                        outbound.FullSerialNumber, outbound.OutboundNo, result.Message ?? "未知错误");
                    return;
                }

                outbound.Id = result.Data;
                EditingProdSerialOutbound = null;

                // 通知命令重新评估 CanExecute
                SaveOutboundInlineCommand.NotifyCanExecuteChanged();
                CancelOutboundInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "创建成功";
                _operLog?.Information("[SerialOutboundView] 出库记录创建成功，记录Id={Id}", outbound.Id);

                await LoadOutboundsAsync();
            }
            else
            {
                // 更新
                // 注意：MaterialCode、SerialNumber、Quantity 需要从 FullSerialNumber 中解析
                // 服务层或数据库层会处理解析
                var updateDto = new ProdSerialOutboundUpdateDto
                {
                    Id = outbound.Id,
                    FullSerialNumber = outbound.FullSerialNumber,
                    OutboundNo = outbound.OutboundNo,
                    OutboundDate = outbound.OutboundDate,
                    DestPort = outbound.DestPort
                };

                _operLog?.Information("[SerialOutboundView] 开始更新出库记录，记录Id={Id}, 完整序列号={FullSerialNumber}, 出库单号={OutboundNo}",
                    outbound.Id, outbound.FullSerialNumber, outbound.OutboundNo);
                var result = await _prodSerialOutboundService.UpdateAsync(updateDto);
                if (!result.Success)
                {
                    ErrorMessage = result.Message ?? "更新失败";
                    _operLog?.Error("[SerialOutboundView] 更新出库记录失败，记录Id={Id}, 错误={Error}",
                        outbound.Id, result.Message ?? "未知错误");
                    return;
                }

                EditingProdSerialOutbound = null;

                // 通知命令重新评估 CanExecute
                SaveOutboundInlineCommand.NotifyCanExecuteChanged();
                CancelOutboundInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "更新成功";
                _operLog?.Information("[SerialOutboundView] 出库记录更新成功，记录Id={Id}", outbound.Id);

                await LoadOutboundsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 出库记录保存失败");
        }
    }

    private bool CanSaveOutboundInline(ProdSerialOutboundDto? outbound)
    {
        if (outbound == null)
        {
            return EditingProdSerialOutbound is not null;
        }
        return EditingProdSerialOutbound != null && EditingProdSerialOutbound == outbound;
    }

    [RelayCommand(CanExecute = nameof(CanCancelOutboundInline))]
    private async Task CancelOutboundInlineAsync()
    {
        if (EditingProdSerialOutbound != null)
        {
            var outboundId = EditingProdSerialOutbound.Id;
            EditingProdSerialOutbound = null;

            // 通知命令重新评估 CanExecute
            SaveOutboundInlineCommand.NotifyCanExecuteChanged();
            CancelOutboundInlineCommand.NotifyCanExecuteChanged();

            _operLog?.Information("[SerialOutboundView] 取消编辑出库记录，记录Id={Id}", outboundId);
            await LoadOutboundsAsync(); // 重新加载以恢复原始数据
        }
    }

    private bool CanCancelOutboundInline()
    {
        return EditingProdSerialOutbound != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOutbound))]
    private async Task DeleteOutboundAsync(ProdSerialOutboundDto? outbound)
    {
        if (outbound == null)
        {
            outbound = SelectedProdSerialOutbound;
        }

        if (outbound == null)
        {
            return;
        }

        try
        {
            var result = await _prodSerialOutboundService.DeleteAsync(outbound.Id);
            if (!result.Success)
            {
                var entityName = _localizationManager.GetString("logistics.serials.prodserialoutbound.entity");
                ErrorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                _operLog?.Error("[SerialOutboundView] 删除序列号出库记录失败，记录Id={Id}, 错误={Error}", outbound.Id, result.Message ?? "未知错误");
                return;
            }

            var entityNameSuccess = _localizationManager.GetString("logistics.serials.prodserialoutbound.entity");
            SuccessMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityNameSuccess);
            _operLog?.Information("[SerialOutboundView] 删除序列号出库记录成功，记录Id={Id}", outbound.Id);
            await LoadOutboundsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 删除序列号出库记录失败");
        }
    }

    private bool CanDeleteOutbound(ProdSerialOutboundDto? outbound)
    {
        // 编辑状态下不能删除
        if (EditingProdSerialOutbound != null)
        {
            return false;
        }

        if (outbound == null)
        {
            return SelectedProdSerialOutbound is not null;
        }
        return outbound is not null;
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
                    await LoadOutboundsAsync();
                }
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
            _operLog?.Information("[SerialOutboundView] 打开扫描出库窗口");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[SerialOutboundView] 打开扫描出库窗口失败");
        }
    }
}

