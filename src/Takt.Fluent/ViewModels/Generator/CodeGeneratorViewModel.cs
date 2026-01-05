// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Generator
// 文件名称：CodeGeneratorViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成视图模型
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;

namespace Takt.Fluent.ViewModels.Generator;

/// <summary>
/// 代码生成视图模型
/// </summary>
public partial class CodeGeneratorViewModel : ObservableObject
{
    private readonly IGenTableService _genTableService;
    private readonly IGenColumnService _genColumnService;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly IDatabaseMetadataService _metadataService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<GenTableDto> GenTables { get; } = [];

    [ObservableProperty]
    private GenTableDto? _selectedGenTable;

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private int _pageIndex = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private string _emptyMessage = string.Empty;

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public CodeGeneratorViewModel(
        IGenTableService genTableService,
        IGenColumnService genColumnService,
        ICodeGeneratorService codeGeneratorService,
        IDatabaseMetadataService metadataService,
        IServiceProvider serviceProvider,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _genTableService = genTableService ?? throw new ArgumentNullException(nameof(genTableService));
        _genColumnService = genColumnService ?? throw new ArgumentNullException(nameof(genColumnService));
        _codeGeneratorService = codeGeneratorService ?? throw new ArgumentNullException(nameof(codeGeneratorService));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        EmptyMessage = _localizationManager.GetString("common.nodata");

        // 显式创建导入命令
        ImportCommand = new RelayCommand(Import);

        _ = LoadAsync();
    }

    /// <summary>
    /// 加载列表
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var query = new GenTableQueryDto
            {
                Keywords = Keyword,
                PageIndex = PageIndex,
                PageSize = PageSize
            };

            var result = await _genTableService.GetListAsync(query);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? "加载失败";
                TaktMessageManager.ShowError(ErrorMessage);
                return;
            }

            GenTables.Clear();
            if (result.Data?.Items != null)
            {
                foreach (var item in result.Data.Items)
                {
                    GenTables.Add(item);
                }
            }

            TotalCount = result.Data?.TotalNum ?? 0;

            var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
            _operLog?.Information("[CodeGenerator] 加载列表成功，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}, 总数={TotalCount}",
                operatorName ?? string.Empty, Keyword ?? string.Empty, PageIndex, PageSize, TotalCount);

            // 数据加载完成后，清除选中项，确保更新和删除按钮默认禁用
            SelectedGenTable = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            TaktMessageManager.ShowError(ErrorMessage);
            _operLog?.Error(ex, "[CodeGenerator] 加载列表失败");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 查询
    /// </summary>
    [RelayCommand]
    private async Task QueryAsync()
    {
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[CodeGenerator] 执行查询操作，操作人={Operator}, 关键词={Keyword}", operatorName, Keyword ?? string.Empty);

        PageIndex = 1;
        await LoadAsync();
    }

    /// <summary>
    /// 重置
    /// </summary>
    [RelayCommand]
    private async Task ResetAsync()
    {
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[CodeGenerator] 执行重置操作，操作人={Operator}", operatorName);

        Keyword = string.Empty;
        PageIndex = 1;
        await LoadAsync();
    }

    /// <summary>
    /// 分页变化
    /// </summary>
    [RelayCommand]
    private async Task PageChangedAsync(PageRequest? request)
    {
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";

        if (request != null)
        {
            _operLog?.Information("[CodeGenerator] 分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}",
                operatorName, request.PageIndex, request.PageSize);
            PageIndex = request.PageIndex;
            PageSize = request.PageSize;
        }
        await LoadAsync();
    }

    /// <summary>
    /// 新建（针对无数据表的情景，手动配置代码生成）
    /// </summary>
    [RelayCommand]
    private void Create()
    {
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[CodeGenerator] 打开新建代码生成配置窗口，操作人={Operator}", operatorName);

        ShowGenTableForm(null);
    }

    /// <summary>
    /// 更新
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private void Update(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        if (genTable == null)
        {
            return;
        }

        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[CodeGenerator] 打开更新代码生成配置窗口，操作人={Operator}, Id={Id}, TableName={TableName}",
            operatorName, genTable.Id, genTable.TableName);

        SelectedGenTable = genTable;
        ShowGenTableForm(genTable);
    }

    private bool CanUpdate(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        return genTable != null;
    }

    /// <summary>
    /// 删除
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(GenTableDto? genTable)
    {
        // 如果没有传递参数，使用 SelectedGenTable
        genTable ??= SelectedGenTable;

        if (genTable == null)
        {
            return;
        }

        SelectedGenTable = genTable;

        var confirmText = string.Format(_localizationManager.GetString("common.confirm.delete"), _localizationManager.GetString("generator.gentable.entity"));
        var owner = System.Windows.Application.Current?.MainWindow;
        if (owner == null || !TaktMessageManager.ShowDeleteConfirm(confirmText, owner))
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var entityName = _localizationManager.GetString("generator.gentable.entity");

        try
        {
            var result = await _genTableService.DeleteAsync(genTable.Id);
            stopwatch.Stop();

            if (!result.Success)
            {
                var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.delete"), entityName);
                TaktMessageManager.ShowError(errorMessage);
                _operLog?.Error("[CodeGenerator] 删除失败，Id={Id}, Message={Message}", genTable.Id, result.Message ?? string.Empty);
                return;
            }

            var requestParams = JsonConvert.SerializeObject(new { genTable.Id, genTable.TableName });
            // 确保 ElapsedTime 正确计算（毫秒），处理溢出和精度问题
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var elapsedTime = elapsedMs > int.MaxValue ? int.MaxValue : (int)elapsedMs;
            _operLog?.Delete(entityName, genTable.Id.ToString(), operatorName, "Generator.CodeGeneratorView", requestParams, null, elapsedTime);

            var successMessage = string.Format(_localizationManager.GetString("common.success.delete"), entityName);
            TaktMessageManager.ShowSuccess(successMessage);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMessage = ex.Message;
            TaktMessageManager.ShowError(errorMessage);
            _operLog?.Error(ex, "[CodeGenerator] 删除失败，Id={Id}", genTable.Id);
        }
    }

    private bool CanDelete(GenTableDto? genTable)
    {
        // 如果没有传递参数，检查 SelectedGenTable
        genTable ??= SelectedGenTable;

        // 如果配置不存在，不能删除
        return genTable != null;
    }

    /// <summary>
    /// 导入命令
    /// </summary>
    public ICommand ImportCommand { get; }

    /// <summary>
    /// 导入
    /// </summary>
    private void Import()
    {
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var entityName = _localizationManager.GetString("generator.gentable.entity");

        try
        {
            _operLog?.Information("[CodeGenerator] 开始导入操作，操作人={Operator}", operatorName);
            var importViewModel = _serviceProvider.GetRequiredService<ImportTableViewModel>();
            var window = new Views.Generator.CodeGenComponent.ImportTableView(importViewModel);

            var importCount = 0;
            importViewModel.ImportSuccessCallback = async () =>
            {
                importCount = importViewModel.ImportedCount;
                window.Close();
                await LoadAsync();

                // 记录导入成功日志
                if (importCount > 0)
                {
                    _operLog?.Import(entityName, importCount, operatorName);
                }
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();

            _operLog?.Information("[CodeGenerator] 导入窗口已关闭，导入数量={Count}", importCount);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            TaktMessageManager.ShowError(ErrorMessage);
            _operLog?.Error(ex, "[CodeGenerator] 打开导入窗口失败，操作人={Operator}", operatorName);
        }
    }

    /// <summary>
    /// 导出
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var entityName = _localizationManager.GetString("generator.gentable.entity");

        var dialog = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            FileName = $"代码生成配置_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            Title = "保存Excel文件"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _operLog?.Information("[CodeGenerator] 开始导出操作，操作人={Operator}, 文件路径={FilePath}", operatorName, dialog.FileName);

            var query = new GenTableQueryDto
            {
                Keywords = Keyword
            };

            var result = await _genTableService.ExportAsync(query);
            stopwatch.Stop();

            if (!result.Success)
            {
                TaktMessageManager.ShowError(result.Message ?? "导出失败");
                _operLog?.Error("[CodeGenerator] 导出失败，Message={Message}", result.Message ?? string.Empty);
                return;
            }

            await File.WriteAllBytesAsync(dialog.FileName, result.Data.content);

            var requestParams = JsonConvert.SerializeObject(new { FilePath = dialog.FileName, Keywords = Keyword, FileSize = result.Data.content.Length });
            _operLog?.Information("[CodeGenerator] 导出成功，操作人={Operator}, 文件路径={FilePath}, 文件大小={FileSize}字节, 耗时={ElapsedMs}ms",
                operatorName, dialog.FileName, result.Data.content.Length, stopwatch.ElapsedMilliseconds);

            TaktMessageManager.ShowSuccess(_localizationManager.GetString("common.success.export"));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            TaktMessageManager.ShowError(ex.Message);
            _operLog?.Error(ex, "[CodeGenerator] 导出失败，操作人={Operator}, 文件路径={FilePath}", operatorName, dialog.FileName);
        }
    }

    /// <summary>
    /// 同步（显示选择对话框，让用户选择同步方向）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSync))]
    private async Task SyncAsync(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        if (genTable == null || string.IsNullOrWhiteSpace(genTable.TableName))
        {
            return;
        }

        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        _operLog?.Information("[CodeGenerator] 开始同步操作，操作人={Operator}, TableName={TableName}", operatorName, genTable.TableName);

        // 显示同步方向选择对话框
        var owner = System.Windows.Application.Current?.MainWindow;
        var syncFromDbText = _localizationManager.GetString("generator.gentable.syncfromdatabase");
        var syncToDbText = _localizationManager.GetString("generator.gentable.synctodatabase");
        var title = _localizationManager.GetString("generator.gentable.synctitle");

        var result = TaktMessageManager.ShowQuestion(
            $"{syncFromDbText}\n{syncToDbText}",
            title,
            owner);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            // 从数据库同步到配置
            await SyncFromDatabaseAsync(genTable);
        }
        else if (result == System.Windows.MessageBoxResult.No)
        {
            // 从配置同步到数据库
            await SyncToDatabaseAsync(genTable);
        }
        else
        {
            _operLog?.Information("[CodeGenerator] 用户取消同步操作，操作人={Operator}, TableName={TableName}", operatorName, genTable.TableName);
        }
    }

    /// <summary>
    /// 同步（从数据库同步表结构到配置）
    /// </summary>
    private async Task SyncFromDatabaseAsync(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        if (genTable == null || string.IsNullOrWhiteSpace(genTable.TableName))
        {
            return;
        }

        var confirmText = _localizationManager.GetString("generator.gentable.syncfromdatabaseconfirm");
        var owner = System.Windows.Application.Current?.MainWindow;
        if (owner == null || !TaktMessageManager.ShowDeleteConfirm(confirmText, owner))
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var entityName = _localizationManager.GetString("generator.gentable.entity");

        try
        {
            IsLoading = true;
            _operLog?.Information("[CodeGenerator] 开始从数据库同步到配置，操作人={Operator}, TableName={TableName}", operatorName, genTable.TableName);

            // 获取数据库表信息
            var tableInfo = _metadataService.GetTableInfoList().FirstOrDefault(t => t.TableName == genTable.TableName);
            if (tableInfo == null)
            {
                var errorMessage = $"表 {genTable.TableName} 不存在于数据库中";
                TaktMessageManager.ShowError(errorMessage);
                _operLog?.Error("[CodeGenerator] 从数据库同步到配置失败，操作人={Operator}, TableName={TableName}, Message={Message}",
                    operatorName, genTable.TableName, errorMessage);
                return;
            }

            // 获取列信息
            var columns = _metadataService.GetColumnsByTableName(genTable.TableName);

            // 更新表配置
            if (string.IsNullOrWhiteSpace(genTable.TableDescription))
            {
                genTable.TableDescription = tableInfo.Description!;
            }

            // 更新列配置
            var existingColumnsResult = await _genColumnService.GetByTableNameAsync(genTable.TableName);
            if (!existingColumnsResult.Success || existingColumnsResult.Data == null)
            {
                var errorMessage = existingColumnsResult.Message ?? "获取列配置失败";
                TaktMessageManager.ShowError(errorMessage);
                _operLog?.Error("[CodeGenerator] 从数据库同步到配置失败，操作人={Operator}, TableName={TableName}, Message={Message}",
                    operatorName, genTable.TableName, errorMessage);
                return;
            }

            var existingColumnDict = existingColumnsResult.Data.ToDictionary(c => c.ColumnName, StringComparer.OrdinalIgnoreCase);

            foreach (var column in columns)
            {
                if (existingColumnDict.TryGetValue(column.ColumnName, out var existingColumn))
                {
                    // 更新现有列
                    var updateDto = new GenColumnUpdateDto
                    {
                        Id = existingColumn.Id,
                        TableName = genTable.TableName,
                        ColumnName = column.ColumnName,
                        DataType = column.DataType,
                        IsPrimaryKey = column.IsPrimaryKey ? 1 : 0,
                        IsIdentity = column.IsIdentity ? 1 : 0,
                        IsNullable = column.IsNullable ? 1 : 0,
                        Length = column.Length,
                        DecimalPlaces = column.DecimalPlaces,
                        DefaultValue = column.DefaultValue!,
                        ColumnDescription = column.Description!
                    };
                    await _genColumnService.UpdateAsync(updateDto);
                }
                else
                {
                    // 创建新列
                    var createDto = new GenColumnCreateDto
                    {
                        TableName = genTable.TableName,
                        ColumnName = column.ColumnName,
                        DataType = column.DataType,
                        IsPrimaryKey = column.IsPrimaryKey ? 1 : 0,
                        IsIdentity = column.IsIdentity ? 1 : 0,
                        IsNullable = column.IsNullable ? 1 : 0,
                        Length = column.Length,
                        DecimalPlaces = column.DecimalPlaces,
                        DefaultValue = column.DefaultValue!,
                        ColumnDescription = column.Description!,
                        OrderNum = columns.IndexOf(column) + 1
                    };
                    await _genColumnService.CreateAsync(createDto);
                }
            }

            stopwatch.Stop();
            var successMessage = _localizationManager.GetString("generator.gentable.syncfromdatabasesuccess");
            TaktMessageManager.ShowSuccess(successMessage);

            var requestParams = JsonConvert.SerializeObject(new { genTable.TableName, Direction = "FromDatabase" });
            _operLog?.Information("[CodeGenerator] 从数据库同步到配置成功，操作人={Operator}, TableName={TableName}, 耗时={ElapsedMs}ms",
                operatorName, genTable.TableName, stopwatch.ElapsedMilliseconds);

            await LoadAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            TaktMessageManager.ShowError(ex.Message);
            _operLog?.Error(ex, "[CodeGenerator] 从数据库同步失败，操作人={Operator}, TableName={TableName}", operatorName, genTable.TableName);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 同步（从配置同步表结构到数据库）
    /// </summary>
    private async Task SyncToDatabaseAsync(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        if (genTable == null || string.IsNullOrWhiteSpace(genTable.TableName))
        {
            return;
        }

        var confirmText = _localizationManager.GetString("generator.gentable.synctodatabaseconfirm");
        var owner = System.Windows.Application.Current?.MainWindow;
        if (owner == null || !TaktMessageManager.ShowDeleteConfirm(confirmText, owner))
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var entityName = _localizationManager.GetString("generator.gentable.entity");

        try
        {
            IsLoading = true;
            _operLog?.Information("[CodeGenerator] 开始从配置同步到数据库，操作人={Operator}, TableName={TableName}", operatorName, genTable.TableName);

            // 获取列配置
            var columnsResult = await _genColumnService.GetByTableNameAsync(genTable.TableName);
            if (!columnsResult.Success || columnsResult.Data == null || columnsResult.Data.Count == 0)
            {
                var errorMessage = columnsResult.Message ?? "请先配置列信息";
                TaktMessageManager.ShowError(errorMessage);
                _operLog?.Error("[CodeGenerator] 从配置同步到数据库失败，操作人={Operator}, TableName={TableName}, Message={Message}",
                    operatorName, genTable.TableName, errorMessage);
                return;
            }

            // 转换为 ColumnInfo 列表
            var columns = columnsResult.Data.Select(c => new ColumnInfo
            {
                ColumnName = c.ColumnName,
                DataType = c.ColumnDataType ?? "nvarchar", // 使用 SQL 类型（ColumnDataType），不是 C# 类型（DataType）
                IsPrimaryKey = c.IsPrimaryKey == 0, // 0=是，1=否，转换为 bool
                IsIdentity = c.IsIdentity == 0, // 0=是，1=否，转换为 bool
                IsNullable = c.IsNullable == 0, // 0=是，1=否，转换为 bool
                Length = c.Length,
                DecimalPlaces = c.DecimalPlaces,
                DefaultValue = c.DefaultValue,
                Description = c.ColumnDescription
            }).ToList();

            // 调用服务创建或更新表
            var result = await _metadataService.CreateOrUpdateTableFromConfigAsync(
                genTable.TableName,
                genTable.TableDescription,
                columns);

            if (!result.Success)
            {
                TaktMessageManager.ShowError(result.Message ?? "同步到数据库失败");
                _operLog?.Error("[CodeGenerator] 从配置同步到数据库失败，操作人={Operator}, TableName={TableName}, Message={Message}",
                    operatorName, genTable.TableName, result.Message ?? "未知错误");
                return;
            }

            stopwatch.Stop();
            var successMessage = _localizationManager.GetString("generator.gentable.synctodatabasesuccess");
            TaktMessageManager.ShowSuccess(successMessage);

            var requestParams = JsonConvert.SerializeObject(new { genTable.TableName, Direction = "ToDatabase" });
            _operLog?.Information("[CodeGenerator] 从配置同步到数据库成功，操作人={Operator}, TableName={TableName}, 耗时={ElapsedMs}ms",
                operatorName, genTable.TableName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            TaktMessageManager.ShowError(ex.Message);
            _operLog?.Error(ex, "[CodeGenerator] 同步到数据库失败，操作人={Operator}, TableName={TableName}", operatorName, genTable.TableName);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSync(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        return genTable != null && !string.IsNullOrWhiteSpace(genTable.TableName);
    }

    /// <summary>
    /// 生成代码
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateAsync(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        if (genTable == null)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var entityName = _localizationManager.GetString("generator.gentable.entity");

        try
        {
            IsLoading = true;
            _operLog?.Information("[CodeGenerator] 开始生成代码，操作人={Operator}, TableName={TableName}, ClassName={ClassName}",
                operatorName, genTable.TableName, genTable.ClassName ?? string.Empty);

            // 获取列配置
            var columnsResult = await _genColumnService.GetByTableNameAsync(genTable.TableName);
            if (!columnsResult.Success || columnsResult.Data == null || columnsResult.Data.Count == 0)
            {
                var errorMessage = columnsResult.Message ?? "请先配置列信息";
                TaktMessageManager.ShowError(errorMessage);
                _operLog?.Error("[CodeGenerator] 生成代码失败，操作人={Operator}, TableName={TableName}, Message={Message}",
                    operatorName, genTable.TableName, errorMessage);
                return;
            }

            var columnDtos = columnsResult.Data.ToList();

            // 如果 IsDatabaseTable = 1（无数据表），需要先初始化数据表
            if (genTable.IsDatabaseTable == 1)
            {
                _operLog?.Information("[CodeGenerator] 无数据表，开始初始化数据表，操作人={Operator}, TableName={TableName}",
                    operatorName, genTable.TableName);

                // 转换为 ColumnInfo 列表
                var columns = columnDtos.Select(c => new ColumnInfo
                {
                    ColumnName = c.ColumnName,
                    DataType = c.ColumnDataType ?? "nvarchar", // 使用 SQL 类型（ColumnDataType），不是 C# 类型（DataType）
                    IsPrimaryKey = c.IsPrimaryKey == 0, // 0=是，1=否，转换为 bool
                    IsIdentity = c.IsIdentity == 0, // 0=是，1=否，转换为 bool
                    IsNullable = c.IsNullable == 0, // 0=是，1=否，转换为 bool
                    Length = c.Length,
                    DecimalPlaces = c.DecimalPlaces,
                    DefaultValue = c.DefaultValue,
                    Description = c.ColumnDescription
                }).ToList();

                // 调用服务创建或更新表
                var initResult = await _metadataService.CreateOrUpdateTableFromConfigAsync(
                    genTable.TableName,
                    genTable.TableDescription,
                    columns);

                if (!initResult.Success)
                {
                    TaktMessageManager.ShowError(initResult.Message ?? "初始化数据表失败");
                    _operLog?.Error("[CodeGenerator] 初始化数据表失败，操作人={Operator}, TableName={TableName}, Message={Message}",
                        operatorName, genTable.TableName, initResult.Message ?? "未知错误");
                    return;
                }

                _operLog?.Information("[CodeGenerator] 数据表初始化成功，操作人={Operator}, TableName={TableName}",
                    operatorName, genTable.TableName);
            }

            // 生成选项
            var options = new CodeGenerationOptions
            {
                Author = genTable.Author ?? "Takt365(Cursor AI)",
                GenerateEntity = true,
                GenerateDto = true,
                GenerateIService = true,
                GenerateService = true,
                GenerateViewModel = true,
                GenerateFormViewModel = true,
                GenerateView = true,
                GenerateFormView = true,
                GenerateMenuSql = true,
                GenerateTranslationSql = true
            };

            // 生成代码
            var generatedCode = await _codeGeneratorService.GenerateFromConfigAsync(genTable, columnDtos, options);

            // 保存代码文件
            var dialog = new SaveFileDialog
            {
                Filter = "ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*",
                FileName = $"{genTable.ClassName ?? genTable.TableName}_Code_{DateTime.Now:yyyyMMddHHmmss}.zip",
                Title = "保存生成的代码"
            };

            if (dialog.ShowDialog() == true)
            {
                stopwatch.Stop();
                // TODO: 实现 ZIP 压缩功能
                TaktMessageManager.ShowSuccess($"代码生成成功，共 {generatedCode.Count} 个文件");

                // 更新 IsGenCode 字段为 0（是），表示已生成代码
                if (genTable.IsGenCode != 0)
                {
                    try
                    {
                        var updateDto = new GenTableUpdateDto
                        {
                            Id = genTable.Id,
                            TableName = genTable.TableName,
                            TableDescription = genTable.TableDescription,
                            ClassName = genTable.ClassName!,
                            DetailTableName = genTable.DetailTableName,
                            DetailRelationField = genTable.DetailRelationField,
                            TreeCodeField = genTable.TreeCodeField,
                            TreeParentCodeField = genTable.TreeParentCodeField,
                            TreeNameField = genTable.TreeNameField,
                            Author = genTable.Author!,
                            TemplateType = genTable.TemplateType,
                            GenNamespacePrefix = genTable.GenNamespacePrefix,
                            GenBusinessName = genTable.GenBusinessName,
                            GenModuleName = genTable.GenModuleName,
                            GenFunctionName = genTable.GenFunctionName,
                            GenType = genTable.GenType,
                            GenFunctions = genTable.GenFunctions,
                            GenPath = genTable.GenPath,
                            Options = genTable.Options,
                            ParentMenuName = genTable.ParentMenuName,
                            PermissionPrefix = genTable.PermissionPrefix,
                            IsDatabaseTable = genTable.IsDatabaseTable,
                            IsGenMenu = genTable.IsGenMenu,
                            IsGenTranslation = genTable.IsGenTranslation,
                            IsGenCode = 0, // 更新为已生成代码
                            DefaultSortField = genTable.DefaultSortField,
                            DefaultSortOrder = genTable.DefaultSortOrder,
                            Remarks = genTable.Remarks
                        };
                        var updateResult = await _genTableService.UpdateAsync(updateDto);
                        if (updateResult.Success)
                        {
                            genTable.IsGenCode = 0;
                            _operLog?.Information("[CodeGenerator] 更新 IsGenCode 字段成功，操作人={Operator}, TableName={TableName}",
                                operatorName, genTable.TableName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _operLog?.Warning("[CodeGenerator] 更新 IsGenCode 字段失败，操作人={Operator}, TableName={TableName}, Error={Error}",
                            operatorName, genTable.TableName, ex.Message);
                    }
                }

                var requestParams = JsonConvert.SerializeObject(new
                {
                    genTable.Id,
                    genTable.TableName,
                    genTable.ClassName,
                    FileCount = generatedCode.Count,
                    FilePath = dialog.FileName
                });
                _operLog?.Information("[CodeGenerator] 代码生成成功，操作人={Operator}, TableName={TableName}, 文件数量={FileCount}, 文件路径={FilePath}, 耗时={ElapsedMs}ms",
                    operatorName, genTable.TableName, generatedCode.Count, dialog.FileName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                stopwatch.Stop();
                _operLog?.Information("[CodeGenerator] 用户取消保存代码文件，操作人={Operator}, TableName={TableName}", operatorName, genTable.TableName);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            TaktMessageManager.ShowError(ex.Message);
            _operLog?.Error(ex, "[CodeGenerator] 生成代码失败，操作人={Operator}, Id={Id}, TableName={TableName}", operatorName, genTable.Id, genTable.TableName);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanGenerate(GenTableDto? genTable)
    {
        genTable ??= SelectedGenTable;

        return genTable != null;
    }

    /// <summary>
    /// 显示表单
    /// </summary>
    private void ShowGenTableForm(GenTableDto? genTable)
    {
        try
        {
            var formViewModel = _serviceProvider.GetRequiredService<CodeGenFormViewModel>();

            // 先初始化 ViewModel 数据，再创建窗口，确保默认值正确绑定
            if (genTable == null)
            {
                formViewModel.ForCreate();
            }
            else
            {
                formViewModel.ForUpdate(genTable);
            }

            var window = new Views.Generator.CodeGenComponent.CodeGenForm(formViewModel);

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            TaktMessageManager.ShowError(ErrorMessage);
            _operLog?.Error(ex, "[CodeGenerator] 打开表单窗口失败");
        }
    }

    partial void OnSelectedGenTableChanged(GenTableDto? value)
    {
        // 通知命令系统重新评估所有命令的 CanExecute
        // 这对于工具栏按钮和行操作按钮都很重要
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();

        // 同时触发全局命令重新评估，确保行操作按钮也能正确更新
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }
}

