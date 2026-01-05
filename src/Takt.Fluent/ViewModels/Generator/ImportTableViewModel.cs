// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Generator
// 文件名称：ImportTableViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：导入表视图模型
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Generator;
using Takt.Application.Services.Generator;
using Takt.Application.Services.Generator.Engine;
using Takt.Common.Logging;
using Takt.Common.Models;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;

namespace Takt.Fluent.ViewModels.Generator;

/// <summary>
/// 导入表视图模型
/// </summary>
public partial class ImportTableViewModel : ObservableObject
{
    private readonly IGenTableService _genTableService;
    private readonly IDatabaseMetadataService _metadataService;
    private readonly ILocalizationManager _localizationManager;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly OperLogManager? _operLog;

    public ObservableCollection<TableInfo> Tables { get; } = new();

    public ObservableCollection<TableInfo> SelectedTables { get; } = new();

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _databaseName = string.Empty;

    [ObservableProperty]
    private TableInfo? _selectedTable;

    public ObservableCollection<ColumnInfo> Columns { get; } = new();

    /// <summary>
    /// 导入成功后的回调
    /// </summary>
    public Action? ImportSuccessCallback { get; set; }

    /// <summary>
    /// 导入成功的数量
    /// </summary>
    public int ImportedCount { get; private set; }

    public ImportTableViewModel(
        IGenTableService genTableService,
        IDatabaseMetadataService metadataService,
        ILocalizationManager localizationManager,
        ICodeGeneratorService codeGeneratorService,
        OperLogManager? operLog = null)
    {
        _genTableService = genTableService ?? throw new ArgumentNullException(nameof(genTableService));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _codeGeneratorService = codeGeneratorService ?? throw new ArgumentNullException(nameof(codeGeneratorService));
        _operLog = operLog;
        
        // 验证命令是否正确生成
        try
        {
            var queryCmd = QueryCommand;
            var resetCmd = ResetCommand;
            var loadCmd = LoadCommand;
            _operLog?.Information("[ImportTable] ViewModel 初始化完成，QueryCommand={QueryCmd}, ResetCommand={ResetCmd}, LoadCommand={LoadCmd}", 
                queryCmd != null ? "已生成" : "未生成",
                resetCmd != null ? "已生成" : "未生成",
                loadCmd != null ? "已生成" : "未生成");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[ImportTable] ViewModel 初始化时验证命令失败");
        }
    }

    private List<TableInfo> _allTables = new();
    private HashSet<string> _existingTableNames = new();

    /// <summary>
    /// 加载表列表
    /// 自动获取当前连接字符串中数据库的所有表清单
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        _operLog?.Information("[ImportTable] LoadAsync 开始加载表列表");
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // 获取数据库名称（当前连接字符串中的数据库）
            DatabaseName = _metadataService.GetDatabaseName();
            _operLog?.Debug("[ImportTable] 数据库名称：{DatabaseName}", DatabaseName);
            
            // 获取当前数据库的所有表列表（不使用缓存，确保获取最新数据）
            _allTables = _metadataService.GetTableInfoList(isCache: false);
            _operLog?.Information("[ImportTable] 从数据库获取表列表完成，表数量={TableCount}", _allTables.Count);
            
            // 获取已存在的表名（已导入到代码生成配置中的表）
            var existingTablesResult = await _genTableService.GetListAsync(new GenTableQueryDto { PageSize = int.MaxValue });
            _existingTableNames = existingTablesResult.Success && existingTablesResult.Data?.Items != null
                ? existingTablesResult.Data.Items.Select(t => t.TableName).ToHashSet(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>();
            _operLog?.Debug("[ImportTable] 已存在的表数量={ExistingCount}", _existingTableNames.Count);

            // 应用过滤
            ApplyFilter();
            _operLog?.Information("[ImportTable] LoadAsync 完成，显示表数量={DisplayCount}", Tables.Count);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[ImportTable] LoadAsync 加载表列表失败");
            TaktMessageManager.ShowError($"加载表列表失败：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
            _operLog?.Debug("[ImportTable] LoadAsync 结束，IsLoading={IsLoading}", IsLoading);
        }
    }

    /// <summary>
    /// 应用过滤（根据关键词和已存在的表）
    /// </summary>
    private void ApplyFilter()
    {
        _operLog?.Debug("[ImportTable] ApplyFilter 开始，Keyword='{Keyword}', AllTablesCount={AllTablesCount}, ExistingCount={ExistingCount}", 
            Keyword, _allTables.Count, _existingTableNames.Count);
        
        try
        {
            // 先根据关键词过滤（如果有）
            var filteredTables = _allTables;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                filteredTables = _allTables.Where(t => 
                    t.TableName.Contains(Keyword, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(Keyword, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                _operLog?.Debug("[ImportTable] 关键词过滤后数量={FilteredCount}", filteredTables.Count);
            }

            // 过滤已存在的表，只显示未导入的表
            Tables.Clear();
            foreach (var table in filteredTables)
            {
                // 只添加未导入的表
                if (!_existingTableNames.Contains(table.TableName))
                {
                    Tables.Add(table);
                }
            }
            
            _operLog?.Debug("[ImportTable] ApplyFilter 完成，最终显示数量={ResultCount}", Tables.Count);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[ImportTable] ApplyFilter 执行失败，Keyword='{Keyword}'", Keyword);
            throw;
        }
    }

    /// <summary>
    /// 查询
    /// </summary>
    [RelayCommand]
    private void Query()
    {
        _operLog?.Information("[ImportTable] QueryCommand 被调用，Keyword='{Keyword}', IsLoading={IsLoading}, AllTablesCount={AllTablesCount}", 
            Keyword, IsLoading, _allTables.Count);
        
        try
        {
            // 如果正在加载中，不执行查询
            if (IsLoading)
            {
                _operLog?.Warning("[ImportTable] QueryCommand 被调用但正在加载中，忽略此次调用");
                return;
            }

            // 如果数据已加载，应用过滤
            if (_allTables.Any())
            {
                _operLog?.Debug("[ImportTable] QueryCommand 开始应用过滤，Keyword='{Keyword}', AllTablesCount={AllTablesCount}", 
                    Keyword, _allTables.Count);
                ApplyFilter();
                _operLog?.Information("[ImportTable] QueryCommand 过滤完成，结果数量={ResultCount}", Tables.Count);
            }
            else
            {
                // 如果数据未加载，提示用户
                var message = _localizationManager.GetString("generator.importtable.datanotloaded");
                _operLog?.Warning("[ImportTable] QueryCommand 被调用但数据未加载，提示用户");
                TaktMessageManager.ShowWarning(message);
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[ImportTable] QueryCommand 执行失败，Keyword='{Keyword}'", Keyword);
            TaktMessageManager.ShowError($"查询失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    [RelayCommand]
    private void Reset()
    {
        _operLog?.Information("[ImportTable] ResetCommand 被调用，当前 Keyword='{Keyword}', IsLoading={IsLoading}", 
            Keyword, IsLoading);
        
        try
        {
            // 如果正在加载中，不执行重置
            if (IsLoading)
            {
                _operLog?.Warning("[ImportTable] ResetCommand 被调用但正在加载中，忽略此次调用");
                return;
            }

            // 重置关键词（OnKeywordChanged 会自动触发过滤）
            var oldKeyword = Keyword;
            Keyword = string.Empty;
            _operLog?.Debug("[ImportTable] ResetCommand 已重置关键词，旧值='{OldKeyword}', 新值='{NewKeyword}'", 
                oldKeyword, Keyword);
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[ImportTable] ResetCommand 执行失败");
            TaktMessageManager.ShowError($"重置失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 当关键词改变时，自动应用过滤
    /// </summary>
    partial void OnKeywordChanged(string value)
    {
        _operLog?.Information("[ImportTable] OnKeywordChanged 触发，新值='{NewValue}', IsLoading={IsLoading}, AllTablesCount={AllTablesCount}", 
            value, IsLoading, _allTables.Count);
        
        // 只有在数据已加载且不在加载中时才自动过滤
        // 注意：重置按钮会触发此方法，确保重置后能正确显示所有数据
        if (!IsLoading && _allTables.Any())
        {
            _operLog?.Debug("[ImportTable] OnKeywordChanged 开始自动过滤");
            ApplyFilter();
        }
        else
        {
            _operLog?.Debug("[ImportTable] OnKeywordChanged 跳过自动过滤，IsLoading={IsLoading}, HasTables={HasTables}", 
                IsLoading, _allTables.Any());
        }
    }

    /// <summary>
    /// 从数据库表导入并保存到 GenTable 和 GenColumn
    /// 该方法会同时导入 GenTable 和 GenColumn 两个表的相关数据
    /// </summary>
    [RelayCommand]
    private async Task ImportFromTableAsync()
    {
        if (SelectedTables.Count == 0)
        {
            TaktMessageManager.ShowWarning(_localizationManager.GetString("generator.importtable.noselection"));
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            int success = 0;
            int fail = 0;

            foreach (var table in SelectedTables)
            {
                try
                {
                    _operLog?.Information("[ImportTable] 开始导入表：{TableName}", table.TableName);
                    
                    // 使用 CodeGeneratorService.ImportFromTableAsync 方法
                    // 该方法会同时导入 GenTable 和 GenColumn 两个表的相关数据
                    var result = await _codeGeneratorService.ImportFromTableAsync(table.TableName, "Takt365(Cursor AI)");
                    
                    if (result.Success)
                    {
                        _operLog?.Information("[ImportTable] 表导入成功：{TableName}", table.TableName);
                        success++;
                    }
                    else
                    {
                        _operLog?.Warning("[ImportTable] 表导入失败：{TableName}, 错误：{Error}", table.TableName, result.Message);
                        fail++;
                    }
                }
                catch (Exception ex)
                {
                    _operLog?.Error(ex, "[ImportTable] 导入表时发生异常：{TableName}", table.TableName);
                    fail++;
                }
            }

            ImportedCount = success;
            
            var message = string.Format(
                _localizationManager.GetString("common.success.import"),
                success, fail);
            TaktMessageManager.ShowSuccess(message);

            ImportSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            TaktMessageManager.ShowError(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 将表名转换为PascalCase类名
    /// </summary>
    private static string ToPascalCase(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return string.Empty;
        }

        // 移除表前缀（如 takt_）
        var name = tableName;
        if (name.StartsWith("takt_", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Substring(5);
        }

        // 分割下划线并转换为PascalCase
        var parts = name.Split('_');
        var result = string.Join("", parts.Select(p => 
            p.Length > 0 ? char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : string.Empty) : string.Empty));

        return result;
    }

    /// <summary>
    /// 当选中表改变时，加载该表的列信息
    /// </summary>
    partial void OnSelectedTableChanged(TableInfo? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.TableName))
        {
            Columns.Clear();
            return;
        }

        try
        {
            var columns = _metadataService.GetColumnsByTableName(value.TableName);
            Columns.Clear();
            foreach (var column in columns)
            {
                Columns.Add(column);
            }
        }
        catch (Exception ex)
        {
            TaktMessageManager.ShowError($"加载列信息失败：{ex.Message}");
            Columns.Clear();
        }
    }
}

