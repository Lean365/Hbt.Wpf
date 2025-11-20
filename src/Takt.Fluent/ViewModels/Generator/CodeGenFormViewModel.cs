// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Generator
// 文件名称：CodeGenFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表单视图模型（新建/编辑）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using Takt.Application.Dtos.Generator;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Generator;
using Takt.Application.Services.Generator.Engine;
using Takt.Application.Services.Identity;
using Takt.Common.Context;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;

namespace Takt.Fluent.ViewModels.Generator;

/// <summary>
/// 代码生成表单视图模型
/// </summary>
public partial class CodeGenFormViewModel : ObservableObject
{
    private readonly IGenTableService _genTableService;
    private readonly IGenColumnService _genColumnService;
    private readonly IMenuService _menuService;
    private readonly IDatabaseMetadataService _metadataService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private string _tableName = string.Empty;

    [ObservableProperty]
    private string? _tableDescription;

    [ObservableProperty]
    private string? _className;


    [ObservableProperty]
    private string? _detailTableName;

    [ObservableProperty]
    private string? _detailRelationField;

    [ObservableProperty]
    private string? _treeCodeField;

    [ObservableProperty]
    private string? _treeParentCodeField;

    [ObservableProperty]
    private string? _treeNameField;

    [ObservableProperty]
    private string? _author;

    [ObservableProperty]
    private string? _templateType;

    partial void OnTemplateTypeChanged(string? value)
    {
        // 当模板类型改变时，通知相关属性更新
        OnPropertyChanged(nameof(ShowMasterDetailFields));
        OnPropertyChanged(nameof(ShowTreeFields));
    }

    /// <summary>
    /// 命名空间前缀（用于生成命名空间，如：Takt）
    /// </summary>
    [ObservableProperty]
    private string? _genNamespacePrefix;

    [ObservableProperty]
    private string? _genBusinessName;

    [ObservableProperty]
    private string? _genModuleName;

    [ObservableProperty]
    private string? _genFunctionName;

    [ObservableProperty]
    private string? _genType;

    partial void OnGenTypeChanged(string? value)
    {
        // 当生成方式改变时，通知Folder字段显示状态更新
        OnPropertyChanged(nameof(ShowFolderField));
    }

    [ObservableProperty]
    private string? _genFunctions;

    [ObservableProperty]
    private string? _genPath;

    [ObservableProperty]
    private string? _options;

    [ObservableProperty]
    private string? _parentMenuName;

    [ObservableProperty]
    private string? _permissionPrefix;

    [ObservableProperty]
    private int _isDatabaseTable;

    [ObservableProperty]
    private int _isGenMenu;

    [ObservableProperty]
    private int _isGenTranslation;

    [ObservableProperty]
    private int _isGenCode;

    [ObservableProperty]
    private string? _defaultSortField;

    [ObservableProperty]
    private string? _defaultSortOrder;

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    [ObservableProperty]
    private string _tableNameError = string.Empty;

    [ObservableProperty]
    private ObservableCollection<GenColumnDto> _columns = new();

    [ObservableProperty]
    private GenColumnDto? _selectedColumn;

    [ObservableProperty]
    private bool _isLoadingColumns;

    [ObservableProperty]
    private bool _isUpdatingColumn;

    /// <summary>
    /// 列名列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _columnNames = new();

    /// <summary>
    /// 菜单列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MenuDto> _menus = new();

    /// <summary>
    /// 表名列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _tableNames = new();

    /// <summary>
    /// 列数据类型列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _columnDataTypes = new();

    /// <summary>
    /// 属性名称列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _propertyNames = new();

    /// <summary>
    /// 数据类型列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _dataTypes = new();

    /// <summary>
    /// 查询类型列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _queryTypes = new();

    /// <summary>
    /// 表单控件类型列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _formControlTypes = new();

    /// <summary>
    /// 字典类型列表（用于 ComboBox）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _dictTypes = new();

    /// <summary>
    /// 是否显示主子表相关字段（TemplateType 为 MasterDetail 时显示）
    /// </summary>
    public bool ShowMasterDetailFields => TemplateType == "MasterDetail";

    /// <summary>
    /// 是否显示树表相关字段（TemplateType 为 Tree 时显示）
    /// </summary>
    public bool ShowTreeFields => TemplateType == "Tree";

    /// <summary>
    /// 是否显示代码生成路径（GenType 为 path 时显示）
    /// </summary>
    public bool ShowFolderField => GenType == "path";

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    private readonly ICodeGeneratorService? _codeGeneratorService;

    public CodeGenFormViewModel(
        IGenTableService genTableService,
        IGenColumnService genColumnService,
        IMenuService menuService,
        IDatabaseMetadataService metadataService,
        ILocalizationManager localizationManager,
        ICodeGeneratorService? codeGeneratorService = null,
        OperLogManager? operLog = null)
    {
        _genTableService = genTableService ?? throw new ArgumentNullException(nameof(genTableService));
        _genColumnService = genColumnService ?? throw new ArgumentNullException(nameof(genColumnService));
        _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _codeGeneratorService = codeGeneratorService;
        _operLog = operLog;
        
        // 初始化数据源
        InitializeDataSources();
        
        // 加载菜单列表和表名列表
        _ = LoadMenusAsync();
        _ = LoadTableNamesAsync();
    }

    /// <summary>
    /// 初始化数据源
    /// </summary>
    private void InitializeDataSources()
    {
        // 初始化列数据类型列表
        ColumnDataTypes.Clear();
        ColumnDataTypes.Add("nvarchar");
        ColumnDataTypes.Add("varchar");
        ColumnDataTypes.Add("nchar");
        ColumnDataTypes.Add("char");
        ColumnDataTypes.Add("int");
        ColumnDataTypes.Add("bigint");
        ColumnDataTypes.Add("smallint");
        ColumnDataTypes.Add("tinyint");
        ColumnDataTypes.Add("decimal");
        ColumnDataTypes.Add("numeric");
        ColumnDataTypes.Add("float");
        ColumnDataTypes.Add("real");
        ColumnDataTypes.Add("datetime");
        ColumnDataTypes.Add("datetime2");
        ColumnDataTypes.Add("date");
        ColumnDataTypes.Add("time");
        ColumnDataTypes.Add("bit");
        ColumnDataTypes.Add("uniqueidentifier");
        ColumnDataTypes.Add("text");
        ColumnDataTypes.Add("ntext");
        ColumnDataTypes.Add("image");
        ColumnDataTypes.Add("varbinary");
        ColumnDataTypes.Add("binary");

        // 初始化属性名称列表（常用属性名）
        PropertyNames.Clear();
        PropertyNames.Add("字符串");
        PropertyNames.Add("整数");
        PropertyNames.Add("长整数");
        PropertyNames.Add("小数");
        PropertyNames.Add("布尔值");
        PropertyNames.Add("日期时间");
        PropertyNames.Add("日期");
        PropertyNames.Add("时间");
        PropertyNames.Add("唯一标识符");
        PropertyNames.Add("文本");
        PropertyNames.Add("字节数组");

        // 初始化数据类型列表（C#类型）
        DataTypes.Clear();
        DataTypes.Add("string");
        DataTypes.Add("int");
        DataTypes.Add("long");
        DataTypes.Add("decimal");
        DataTypes.Add("double");
        DataTypes.Add("float");
        DataTypes.Add("bool");
        DataTypes.Add("DateTime");
        DataTypes.Add("DateOnly");
        DataTypes.Add("TimeOnly");
        DataTypes.Add("Guid");
        DataTypes.Add("byte[]");

        // 初始化查询类型列表
        QueryTypes.Clear();
        QueryTypes.Add("Like");      // 模糊查询
        QueryTypes.Add("Equal");     // 精确查询
        QueryTypes.Add("Between");   // 范围查询
        QueryTypes.Add("In");        // 包含查询
        QueryTypes.Add("NotEqual");  // 不等于查询
        QueryTypes.Add("GreaterThan"); // 大于查询
        QueryTypes.Add("LessThan");  // 小于查询
        QueryTypes.Add("GreaterThanOrEqual"); // 大于等于查询
        QueryTypes.Add("LessThanOrEqual"); // 小于等于查询

        // 初始化表单控件类型列表
        FormControlTypes.Clear();
        FormControlTypes.Add("TextBox");
        FormControlTypes.Add("ComboBox");
        FormControlTypes.Add("DatePicker");
        FormControlTypes.Add("DateTimePicker");
        FormControlTypes.Add("CheckBox");
        FormControlTypes.Add("RadioButton");
        FormControlTypes.Add("NumericUpDown");
        FormControlTypes.Add("PasswordBox");
        FormControlTypes.Add("RichTextBox");
        FormControlTypes.Add("TextArea");

        // 初始化字典类型列表（这里可以根据实际需求添加）
        DictTypes.Clear();
        // 字典类型通常是从系统字典表中动态加载的，这里先留空，后续可以从字典服务加载
        // 如果需要预设一些常用字典类型，可以在这里添加
    }

    /// <summary>
    /// 根据列数据类型自动同步其他字段
    /// </summary>
    public void SyncColumnFieldsByDataType(GenColumnDto column)
    {
        if (column == null || string.IsNullOrWhiteSpace(column.ColumnDataType))
        {
            return;
        }

        var columnDataType = column.ColumnDataType.ToLower();

        // 定义数值类型列表
        var numericTypes = new HashSet<string> { "int", "bigint", "smallint", "tinyint", "decimal", "numeric", "float", "real", "bit" };

        // 根据 ColumnDataType 设置其他字段
        switch (columnDataType)
        {
            case "nvarchar":
            case "varchar":
            case "nchar":
            case "char":
            case "text":
            case "ntext":
                column.PropertyName = "字符串";
                column.DataType = "string";
                column.QueryType = "Like";
                column.FormControlType = "TextBox";
                column.Length = 64;
                column.DecimalPlaces = 0;
                column.DefaultValue = string.Empty;
                break;

            case "decimal":
            case "numeric":
                column.PropertyName = "小数";
                column.DataType = "decimal";
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 18;
                column.DecimalPlaces = 5;
                column.DefaultValue = "0";
                break;

            case "int":
            case "smallint":
            case "tinyint":
                column.PropertyName = "整数";
                column.DataType = "int";
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = columnDataType == "smallint" ? 6 : 11;
                column.DecimalPlaces = 0;
                column.DefaultValue = "0";
                break;

            case "bigint":
                column.PropertyName = "长整数";
                column.DataType = "long";
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 20;
                column.DecimalPlaces = 0;
                column.DefaultValue = "0";
                break;

            case "float":
            case "real":
                column.PropertyName = "小数";
                column.DataType = columnDataType == "float" ? "double" : "float";
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 18;
                column.DecimalPlaces = 2;
                column.DefaultValue = "0";
                break;

            case "datetime":
            case "datetime2":
                column.PropertyName = "日期时间";
                column.DataType = "DateTime";
                column.QueryType = "Between";
                column.FormControlType = "DateTimePicker";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                break;

            case "date":
                column.PropertyName = "日期";
                column.DataType = "DateOnly";
                column.QueryType = "Between";
                column.FormControlType = "DatePicker";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                break;

            case "time":
                column.PropertyName = "时间";
                column.DataType = "TimeOnly";
                column.QueryType = "Equal";
                column.FormControlType = "DatePicker";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                break;

            case "bit":
                column.PropertyName = "布尔值";
                column.DataType = "bool";
                column.QueryType = "Equal";
                column.FormControlType = "CheckBox";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = "false";
                break;

            case "uniqueidentifier":
                column.PropertyName = "唯一标识符";
                column.DataType = "Guid";
                column.QueryType = "Equal";
                column.FormControlType = "TextBox";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                break;

            case "varbinary":
            case "binary":
            case "image":
                column.PropertyName = "字节数组";
                column.DataType = "byte[]";
                column.QueryType = "Equal";
                column.FormControlType = "TextBox";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                break;

            default:
                // 对于未知类型，尝试判断是否为数值类型
                if (numericTypes.Contains(columnDataType))
                {
                    column.PropertyName = "整数";
                    column.DataType = "int";
                    column.QueryType = "Equal";
                    column.FormControlType = "NumericUpDown";
                    column.Length = 11;
                    column.DecimalPlaces = 0;
                    column.DefaultValue = "0";
                }
                else
                {
                    // 默认字符串类型
                    column.PropertyName = "字符串";
                    column.DataType = "string";
                    column.QueryType = "Like";
                    column.FormControlType = "TextBox";
                    column.Length = 64;
                    column.DecimalPlaces = 0;
                    column.DefaultValue = string.Empty;
                }
                break;
        }
    }

    /// <summary>
    /// 初始化创建模式（针对无数据表的情景，手动配置代码生成）
    /// </summary>
    public void ForCreate()
    {
        IsCreate = true;
        Title = _localizationManager.GetString("Generator.GenTable.Create") ?? "新建代码生成配置（无数据表）";
        Id = 0;
        TableName = string.Empty;
        TableDescription = null;
        ClassName = null;
        DetailTableName = null;
        DetailRelationField = null;
        TreeCodeField = null;
        TreeParentCodeField = null;
        TreeNameField = null;
        Author = "Takt365(Cursor AI)";
        TemplateType = "CRUD";
        GenNamespacePrefix = "Takt"; // 默认值为 Takt（命名空间前缀）
        GenBusinessName = null;
        GenModuleName = null;
        GenFunctionName = null;
        GenType = "path"; // 默认为自定义路径
        GenFunctions = "List,Query,Create,Update,Export"; // 默认功能
        GenPath = GetProjectRootDirectory(); // 默认为项目根目录
        Options = null;
        ParentMenuName = null;
        PermissionPrefix = string.Empty; // 默认值为空字符串，将在输入 GenBusinessName 后自动生成
        IsDatabaseTable = 1; // 默认无数据表（手动创建）
        IsGenMenu = 1;
        IsGenTranslation = 1;
        IsGenCode = 1;
        DefaultSortField = null;
        DefaultSortOrder = "ASC"; // 默认为ASC
        Remarks = null;
        Columns.Clear();
        ClearErrors();
    }

    /// <summary>
    /// 初始化编辑模式
    /// </summary>
    public void ForUpdate(GenTableDto dto)
    {
        IsCreate = false;
        Title = _localizationManager.GetString("Generator.GenTable.Update") ?? "编辑代码生成配置";
        Id = dto.Id;
        TableName = dto.TableName;
        TableDescription = dto.TableDescription;
        ClassName = dto.ClassName;
        DetailTableName = dto.DetailTableName;
        DetailRelationField = dto.DetailRelationField;
        TreeCodeField = dto.TreeCodeField;
        TreeParentCodeField = dto.TreeParentCodeField;
        TreeNameField = dto.TreeNameField;
        Author = dto.Author;
        TemplateType = dto.TemplateType;
        GenNamespacePrefix = dto.GenNamespacePrefix;
        GenBusinessName = dto.GenBusinessName;
        GenModuleName = dto.GenModuleName;
        GenFunctionName = dto.GenFunctionName;
        GenType = dto.GenType;
        GenFunctions = dto.GenFunctions;
        GenPath = dto.GenPath;
        Options = dto.Options;
        ParentMenuName = dto.ParentMenuName;
        PermissionPrefix = dto.PermissionPrefix;
        IsDatabaseTable = dto.IsDatabaseTable;
        IsGenMenu = dto.IsGenMenu;
        IsGenTranslation = dto.IsGenTranslation;
        IsGenCode = dto.IsGenCode;
        DefaultSortField = dto.DefaultSortField;
        DefaultSortOrder = dto.DefaultSortOrder;
        Remarks = dto.Remarks;
        Columns.Clear();
        ClearErrors();
        
        // 注意：不需要在这里调用 LoadColumnsAsync()，因为设置 TableName 会触发 OnTableNameChanged
        // 而 OnTableNameChanged 会自动调用 LoadColumnsAsync()
        // 这样可以避免重复加载
    }

    /// <summary>
    /// 加载菜单列表
    /// </summary>
    private async Task LoadMenusAsync()
    {
        try
        {
            var result = await _menuService.GetAllMenuTreeAsync();
            if (result.Success && result.Data != null)
            {
                Menus.Clear();
                // 扁平化菜单树，只保留菜单名称用于显示
                FlattenMenuTree(result.Data).ForEach(m => Menus.Add(m));
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[CodeGenForm] 加载菜单列表失败");
        }
    }

    /// <summary>
    /// 加载表名列表
    /// </summary>
    private async Task LoadTableNamesAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                var tableNames = _metadataService.GetAllTableNames(isCache: false);
                TableNames.Clear();
                foreach (var tableName in tableNames.OrderBy(t => t))
                {
                    TableNames.Add(tableName);
                }
            });
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[CodeGenForm] 加载表名列表失败");
        }
    }

    /// <summary>
    /// 扁平化菜单树
    /// </summary>
    private List<MenuDto> FlattenMenuTree(List<MenuDto> menuTree)
    {
        var result = new List<MenuDto>();
        foreach (var menu in menuTree)
        {
            result.Add(menu);
            if (menu.Children != null && menu.Children.Count > 0)
            {
                result.AddRange(FlattenMenuTree(menu.Children));
            }
        }
        return result;
    }

    /// <summary>
    /// 加载字段列表
    /// </summary>
    [RelayCommand]
    private async Task LoadColumnsAsync()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            Columns.Clear();
            ColumnNames.Clear();
            return;
        }

        try
        {
            IsLoadingColumns = true;
            _operLog?.Information("[CodeGenForm] 开始加载字段列表，表名={TableName}", TableName);
            
            var result = await _genColumnService.GetByTableNameAsync(TableName);
            if (result.Success && result.Data != null)
            {
                _operLog?.Information("[CodeGenForm] 成功获取字段列表，数量={Count}", result.Data.Count);
                
                // 验证数据完整性：检查每个字段是否都有值
                foreach (var dto in result.Data)
                {
                    _operLog?.Debug("[CodeGenForm] 字段信息: Id={Id}, TableName={TableName}, ColumnName={ColumnName}, PropertyName={PropertyName}, DataType={DataType}, ColumnDataType={ColumnDataType}, OrderNum={OrderNum}, IsQuery={IsQuery}, QueryType={QueryType}, FormControlType={FormControlType}, DictType={DictType}",
                        dto.Id, dto.TableName, dto.ColumnName, dto.PropertyName ?? string.Empty, dto.DataType ?? string.Empty, dto.ColumnDataType ?? string.Empty, dto.OrderNum, dto.IsQuery, dto.QueryType ?? string.Empty, dto.FormControlType ?? string.Empty, dto.DictType ?? string.Empty);
                }
                
                // 使用 Clear 和 Add 方式更新集合，确保 UI 能正确响应
                Columns.Clear();
                foreach (var item in result.Data.OrderBy(c => c.OrderNum))
                {
                    Columns.Add(item);
                }
                
                _operLog?.Information("[CodeGenForm] 字段列表已加载到 UI，数量={Count}", Columns.Count);
                
                // 验证 UI 绑定
                if (Columns.Count == 0)
                {
                    _operLog?.Warning("[CodeGenForm] 警告：字段列表为空，表名={TableName}，可能数据库中没有该表的列配置数据", TableName);
                    
                    // 提示用户需要导入表结构
                    if (!IsCreate && !string.IsNullOrWhiteSpace(TableName))
                    {
                        var message = $"表 {TableName} 的列配置数据为空。\n\n是否要从数据库导入表结构？";
                        // 注意：这里不自动弹出对话框，而是在 UI 中显示提示信息
                    }
                }
                
                // 更新列名列表
                ColumnNames.Clear();
                foreach (var column in Columns)
                {
                    if (!string.IsNullOrWhiteSpace(column.ColumnName))
                    {
                        ColumnNames.Add(column.ColumnName);
                    }
                }
            }
            else
            {
                _operLog?.Warning("[CodeGenForm] 获取字段列表失败，表名={TableName}, 错误信息={Message}", TableName, result.Message ?? "未知错误");
                Columns.Clear();
                ColumnNames.Clear();
                
                // 如果查询失败，提示用户
                if (!string.IsNullOrWhiteSpace(result.Message))
                {
                    TaktMessageManager.ShowWarning($"加载字段列表失败：{result.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[CodeGenForm] 加载字段列表异常，表名={TableName}", TableName);
            TaktMessageManager.ShowError(ex.Message);
            Columns.Clear();
            ColumnNames.Clear();
        }
        finally
        {
            IsLoadingColumns = false;
        }
    }

    /// <summary>
    /// 保存字段配置
    /// </summary>
    [RelayCommand]
    private async Task SaveColumnsAsync()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            TaktMessageManager.ShowWarning("表名不能为空");
            return;
        }

        if (Columns.Count == 0)
        {
            TaktMessageManager.ShowWarning("没有字段需要保存");
            return;
        }

        try
        {
            IsLoadingColumns = true;
            var successCount = 0;
            var failCount = 0;

            foreach (var column in Columns)
            {
                try
                {
                    var updateDto = new GenColumnUpdateDto
                    {
                        Id = column.Id,
                        TableName = column.TableName,
                        ColumnName = column.ColumnName,
                        PropertyName = column.PropertyName,
                        ColumnDescription = column.ColumnDescription,
                        DataType = column.DataType,
                        ColumnDataType = column.ColumnDataType,
                        IsNullable = column.IsNullable,
                        IsPrimaryKey = column.IsPrimaryKey,
                        IsIdentity = column.IsIdentity,
                        Length = column.Length,
                        DecimalPlaces = column.DecimalPlaces,
                        DefaultValue = column.DefaultValue,
                        OrderNum = column.OrderNum,
                        IsQuery = column.IsQuery,
                        QueryType = column.QueryType,
                        IsCreate = column.IsCreate,
                        IsUpdate = column.IsUpdate,
                        IsList = column.IsList,
                        IsSort = column.IsSort,
                        IsExport = column.IsExport,
                        IsForm = column.IsForm,
                        IsRequired = column.IsRequired,
                        FormControlType = column.FormControlType,
                        DictType = column.DictType,
                        Remarks = column.Remarks
                    };

                    var result = await _genColumnService.UpdateAsync(updateDto);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    _ = ex; // 记录错误但继续处理其他字段
                }
            }

            if (failCount == 0)
            {
                TaktMessageManager.ShowSuccess($"成功保存 {successCount} 个字段配置");
                // 重新加载以确保数据同步
                await LoadColumnsAsync();
            }
            else
            {
                TaktMessageManager.ShowWarning($"成功保存 {successCount} 个字段，失败 {failCount} 个字段");
            }
        }
        catch (Exception ex)
        {
            TaktMessageManager.ShowError($"保存字段配置失败: {ex.Message}");
        }
        finally
        {
            IsLoadingColumns = false;
        }
    }

    partial void OnTableNameChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            TableNameError = string.Empty;
            // 表名为空时清空列数据
            Columns.Clear();
            ColumnNames.Clear();
            return;
        }
        
        // 表名变化时重新加载字段（仅在编辑模式下）
        // 直接调用异步方法，MVVM 框架会自动处理线程切换
        if (!IsCreate)
        {
            _ = LoadColumnsAsync();
        }
    }
    
    /// <summary>
    /// 更新命名空间前缀（默认使用 Takt，可根据需要修改）
    /// </summary>
    private void UpdatePrefix()
    {
        // 如果命名空间前缀为空，设置默认值为 Takt
        if (string.IsNullOrWhiteSpace(GenNamespacePrefix))
        {
            GenNamespacePrefix = "Takt";
        }
    }
    
    /// <summary>
    /// 转换为驼峰命名（首字母小写）
    /// </summary>
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }
        
        // 分割下划线并转换为驼峰命名
        var parts = name.Split('_');
        if (parts.Length == 0)
        {
            return string.Empty;
        }
        
        // 第一个单词首字母小写
        var firstPart = parts[0];
        if (firstPart.Length > 0)
        {
            firstPart = char.ToLowerInvariant(firstPart[0]) + (firstPart.Length > 1 ? firstPart.Substring(1) : string.Empty);
        }
        
        // 后续单词首字母大写
        var result = firstPart;
        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length > 0)
            {
                result += char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part.Substring(1) : string.Empty);
            }
        }
        
        return result;
    }

    partial void OnGenNamespacePrefixChanged(string? value)
    {
        // 命名空间前缀变化时，无需特殊处理
    }

    partial void OnGenBusinessNameChanged(string? value)
    {
        // 自动生成权限前缀
        UpdatePermissionPrefix();
    }

    /// <summary>
    /// 更新权限前缀（格式：:GenBusinessName）
    /// </summary>
    private void UpdatePermissionPrefix()
    {
        if (!string.IsNullOrWhiteSpace(GenBusinessName))
        {
            PermissionPrefix = $":{GenBusinessName}";
        }
        else
        {
            // 如果 GenBusinessName 为空，且权限前缀是自动生成的值（以 ":" 开头），则重置为空字符串
            if (PermissionPrefix != null && PermissionPrefix.StartsWith(":"))
            {
                PermissionPrefix = string.Empty;
            }
        }
    }

    /// <summary>
    /// 获取项目根目录（包含 .sln 文件的目录）
    /// </summary>
    private static string GetProjectRootDirectory()
    {
        try
        {
            // 从当前程序集位置开始向上查找
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var directory = new DirectoryInfo(currentDir);

            // 向上查找，直到找到包含 .sln 文件的目录或包含 "src" 目录的父目录
            while (directory != null)
            {
                // 检查是否包含 .sln 文件
                if (directory.GetFiles("*.sln").Length > 0)
                {
                    return directory.FullName;
                }

                // 检查是否包含 "src" 目录（项目结构特征）
                if (directory.GetDirectories("src").Length > 0)
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            // 如果找不到，返回当前目录
            return currentDir;
        }
        catch
        {
            // 如果出错，返回当前目录
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }

    /// <summary>
    /// 新增字段
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCreateColumn))]
    private void CreateColumn()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            TaktMessageManager.ShowWarning("请先输入表名");
            return;
        }

        // 自动生成列名（ColumnName1, ColumnName2, ...）
        var columnNumber = Columns.Count + 1;
        var columnName = $"ColumnName{columnNumber}";

        var newColumn = new GenColumnDto
        {
            Id = 0,
            TableName = TableName,
            ColumnName = columnName,
            PropertyName = "字符串", // 默认值：字符串
            ColumnDescription = columnName, // 默认值与 ColumnName 相同
            DataType = "string", // 默认值：string
            ColumnDataType = "nvarchar", // 默认值：nvarchar
            IsPrimaryKey = 1,
            IsIdentity = 1,
            IsNullable = 0,
            Length = 64, // 默认值：64
            DecimalPlaces = 0, // 默认值：0
            DefaultValue = string.Empty, // 默认值：空字符串
            OrderNum = columnNumber,
            IsQuery = 1,
            QueryType = null,
            IsCreate = 0,
            IsUpdate = 0,
            IsList = 0,
            IsSort = 1,
            IsExport = 1,
            IsForm = 0,
            IsRequired = 1,
            FormControlType = null,
            DictType = null,
            Remarks = null
        };

        Columns.Add(newColumn);
        SelectedColumn = newColumn;
        IsUpdatingColumn = true;
    }

    private bool CanCreateColumn()
    {
        // 新建和编辑模式都可以添加字段，只要当前没有正在编辑的字段
        // 在新建模式下，允许表名为空时也添加字段（用户可以稍后输入表名）
        // 在编辑模式下，表名必须不为空
        if (IsUpdatingColumn)
        {
            return false;
        }
        
        // 新建模式下，允许添加字段（即使表名为空）
        if (IsCreate)
        {
            return true;
        }
        
        // 编辑模式下，表名必须不为空
        return !string.IsNullOrWhiteSpace(TableName);
    }

    /// <summary>
    /// 更新字段（进入编辑状态）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdateColumn))]
    private void UpdateColumn(GenColumnDto? column)
    {
        if (column == null)
        {
            column = SelectedColumn;
        }

        if (column == null)
        {
            return;
        }

        SelectedColumn = column;
        IsUpdatingColumn = true;
    }

    private bool CanUpdateColumn(GenColumnDto? column)
    {
        if (column == null)
        {
            column = SelectedColumn;
        }

        return !IsCreate && !IsUpdatingColumn && column != null && column.Id > 0;
    }

    /// <summary>
    /// 保存当前编辑的字段
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveColumn))]
    private async Task SaveColumnAsync(GenColumnDto? column)
    {
        if (column == null)
        {
            column = SelectedColumn;
        }

        if (column == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(column.ColumnName))
        {
            TaktMessageManager.ShowWarning("列名不能为空");
            return;
        }

        if (string.IsNullOrWhiteSpace(column.PropertyName))
        {
            TaktMessageManager.ShowWarning("属性名不能为空");
            return;
        }

        try
        {
            IsLoadingColumns = true;

            if (column.Id == 0)
            {
                // 新增
                var createDto = new GenColumnCreateDto
                {
                    TableName = column.TableName,
                    ColumnName = column.ColumnName,
                    PropertyName = column.PropertyName,
                    ColumnDescription = column.ColumnDescription,
                    DataType = column.DataType,
                    ColumnDataType = column.ColumnDataType,
                    IsPrimaryKey = column.IsPrimaryKey,
                    IsIdentity = column.IsIdentity,
                    IsNullable = column.IsNullable,
                    Length = column.Length,
                    DecimalPlaces = column.DecimalPlaces,
                    DefaultValue = column.DefaultValue,
                    OrderNum = column.OrderNum,
                    IsQuery = column.IsQuery,
                    QueryType = column.QueryType,
                    IsCreate = column.IsCreate,
                    IsUpdate = column.IsUpdate,
                    IsList = column.IsList,
                    IsSort = column.IsSort,
                    IsExport = column.IsExport,
                    IsForm = column.IsForm,
                    IsRequired = column.IsRequired,
                    FormControlType = column.FormControlType,
                    DictType = column.DictType,
                    Remarks = column.Remarks
                };

                var result = await _genColumnService.CreateAsync(createDto);
                if (result.Success && result.Data > 0)
                {
                    column.Id = result.Data;
                    TaktMessageManager.ShowSuccess("字段添加成功");
                    IsUpdatingColumn = false;
                    await LoadColumnsAsync();
                }
                else
                {
                    TaktMessageManager.ShowError(result.Message ?? "字段添加失败");
                }
            }
            else
            {
                // 更新
                var updateDto = new GenColumnUpdateDto
                {
                    Id = column.Id,
                    TableName = column.TableName,
                    ColumnName = column.ColumnName,
                    PropertyName = column.PropertyName,
                    ColumnDescription = column.ColumnDescription,
                    DataType = column.DataType,
                    ColumnDataType = column.ColumnDataType,
                    IsPrimaryKey = column.IsPrimaryKey,
                    IsIdentity = column.IsIdentity,
                    IsNullable = column.IsNullable,
                    Length = column.Length,
                    DecimalPlaces = column.DecimalPlaces,
                    DefaultValue = column.DefaultValue,
                    OrderNum = column.OrderNum,
                    IsQuery = column.IsQuery,
                    QueryType = column.QueryType,
                    IsCreate = column.IsCreate,
                    IsUpdate = column.IsUpdate,
                    IsList = column.IsList,
                    IsSort = column.IsSort,
                    IsExport = column.IsExport,
                    IsForm = column.IsForm,
                    IsRequired = column.IsRequired,
                    FormControlType = column.FormControlType,
                    DictType = column.DictType,
                    Remarks = column.Remarks
                };

                var result = await _genColumnService.UpdateAsync(updateDto);
                if (result.Success)
                {
                    TaktMessageManager.ShowSuccess("字段更新成功");
                    IsUpdatingColumn = false;
                    await LoadColumnsAsync();
                }
                else
                {
                    TaktMessageManager.ShowError(result.Message ?? "字段更新失败");
                }
            }
        }
        catch (Exception ex)
        {
            TaktMessageManager.ShowError($"保存字段失败: {ex.Message}");
        }
        finally
        {
            IsLoadingColumns = false;
        }
    }

    private bool CanSaveColumn(GenColumnDto? column)
    {
        if (column == null)
        {
            column = SelectedColumn;
        }

        return !IsCreate && IsUpdatingColumn && column != null;
    }

    /// <summary>
    /// 取消编辑
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelUpdate))]
    private async Task CancelUpdateAsync()
    {
        if (IsUpdatingColumn)
        {
            IsUpdatingColumn = false;
            await LoadColumnsAsync(); // 重新加载以恢复原始数据
        }
    }

    private bool CanCancelUpdate()
    {
        return !IsCreate && IsUpdatingColumn;
    }

    /// <summary>
    /// 导入表结构（从数据库导入列配置）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanImportTableStructure))]
    private async Task ImportTableStructureAsync()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            TaktMessageManager.ShowWarning("表名不能为空");
            return;
        }

        if (_codeGeneratorService == null)
        {
            TaktMessageManager.ShowError("代码生成服务未初始化");
            return;
        }

        try
        {
            IsLoadingColumns = true;
            _operLog?.Information("[CodeGenForm] 开始导入表结构，表名={TableName}", TableName);

            var result = await _codeGeneratorService.ImportFromTableAsync(TableName, Author);
            
            if (result.Success)
            {
                _operLog?.Information("[CodeGenForm] 表结构导入成功，表名={TableName}", TableName);
                TaktMessageManager.ShowSuccess($"表 {TableName} 的结构导入成功");
                
                // 重新加载列数据
                await LoadColumnsAsync();
            }
            else
            {
                _operLog?.Warning("[CodeGenForm] 表结构导入失败，表名={TableName}, 错误={Error}", TableName, result.Message);
                TaktMessageManager.ShowError($"导入失败：{result.Message}");
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[CodeGenForm] 导入表结构异常，表名={TableName}", TableName);
            TaktMessageManager.ShowError($"导入表结构失败：{ex.Message}");
        }
        finally
        {
            IsLoadingColumns = false;
        }
    }

    private bool CanImportTableStructure()
    {
        return !IsCreate && !string.IsNullOrWhiteSpace(TableName) && _codeGeneratorService != null;
    }

    /// <summary>
    /// 删除字段
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteColumn))]
    private async Task DeleteColumnAsync(GenColumnDto? column)
    {
        if (column == null)
        {
            column = SelectedColumn;
        }

        if (column == null || column.Id == 0)
        {
            return;
        }

        var confirmText = $"确定要删除字段 {column.ColumnName} 吗？";
        var owner = System.Windows.Application.Current?.MainWindow;
        if (owner == null || !TaktMessageManager.ShowDeleteConfirm(confirmText, owner))
        {
            return;
        }

        try
        {
            IsLoadingColumns = true;
            var result = await _genColumnService.DeleteAsync(column.Id);
            if (result.Success)
            {
                TaktMessageManager.ShowSuccess("字段删除成功");
                await LoadColumnsAsync();
            }
            else
            {
                TaktMessageManager.ShowError(result.Message ?? "字段删除失败");
            }
        }
        catch (Exception ex)
        {
            TaktMessageManager.ShowError($"删除字段失败: {ex.Message}");
        }
        finally
        {
            IsLoadingColumns = false;
        }
    }

    private bool CanDeleteColumn(GenColumnDto? column)
    {
        if (column == null)
        {
            column = SelectedColumn;
        }

        return !IsCreate && !IsUpdatingColumn && column != null && column.Id > 0;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearErrors()
    {
        TableNameError = string.Empty;
        Error = string.Empty;
    }

    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        ClearErrors();

        // 验证
        if (string.IsNullOrWhiteSpace(TableName))
        {
            TableNameError = _localizationManager.GetString("Generator.GenTable.Validation.TableNameRequired") ?? "表名不能为空";
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "System";
        var entityName = _localizationManager.GetString("Generator.GenTable.Keyword") ?? "代码生成配置";

        try
        {
            if (IsCreate)
            {
                _operLog?.Information("[CodeGenerator] 开始创建代码生成配置，操作人={Operator}, TableName={TableName}", operatorName, TableName);
                
                var createDto = new GenTableCreateDto
                {
                    TableName = TableName,
                    TableDescription = TableDescription,
                    ClassName = ClassName,
                    DetailTableName = DetailTableName,
                    DetailRelationField = DetailRelationField,
                    TreeCodeField = TreeCodeField,
                    TreeParentCodeField = TreeParentCodeField,
                    TreeNameField = TreeNameField,
                    Author = Author,
                    TemplateType = TemplateType,
                    GenNamespacePrefix = GenNamespacePrefix,
                    GenBusinessName = GenBusinessName,
                    GenModuleName = GenModuleName,
                    GenFunctionName = GenFunctionName,
                    GenType = GenType,
                    GenFunctions = GenFunctions,
                    GenPath = GenPath,
                    Options = Options,
                    ParentMenuName = ParentMenuName,
                    PermissionPrefix = PermissionPrefix,
                    IsDatabaseTable = IsDatabaseTable,
                    IsGenMenu = IsGenMenu,
                    IsGenTranslation = IsGenTranslation,
                    IsGenCode = IsGenCode,
                    DefaultSortField = DefaultSortField,
                    DefaultSortOrder = DefaultSortOrder,
                    Remarks = Remarks
                };

                var result = await _genTableService.CreateAsync(createDto);
                stopwatch.Stop();

                if (!result.Success)
                {
                    Error = result.Message ?? "创建失败";
                    TaktMessageManager.ShowError(Error);
                    _operLog?.Error("[CodeGenerator] 创建失败，操作人={Operator}, TableName={TableName}, Message={Message}", 
                        operatorName, TableName, result.Message ?? "未知错误");
                    return;
                }

                var requestParams = JsonSerializer.Serialize(new { TableName = TableName, ClassName = ClassName });
                var entityId = result.Data > 0 ? result.Data.ToString() : "0";
                _operLog?.Create(entityName, entityId, operatorName, "Generator.CodeGenForm", requestParams, (int)stopwatch.ElapsedMilliseconds);

                TaktMessageManager.ShowSuccess(_localizationManager.GetString("common.success.create") ?? "创建成功");
            }
            else
            {
                _operLog?.Information("[CodeGenerator] 开始更新代码生成配置，操作人={Operator}, Id={Id}, TableName={TableName}", 
                    operatorName, Id, TableName);
                
                var updateDto = new GenTableUpdateDto
                {
                    Id = Id,
                    TableName = TableName,
                    TableDescription = TableDescription,
                    ClassName = ClassName,
                    DetailTableName = DetailTableName,
                    DetailRelationField = DetailRelationField,
                    TreeCodeField = TreeCodeField,
                    TreeParentCodeField = TreeParentCodeField,
                    TreeNameField = TreeNameField,
                    Author = Author,
                    TemplateType = TemplateType,
                    GenNamespacePrefix = GenNamespacePrefix,
                    GenBusinessName = GenBusinessName,
                    GenModuleName = GenModuleName,
                    GenFunctionName = GenFunctionName,
                    GenType = GenType,
                    GenFunctions = GenFunctions,
                    GenPath = GenPath,
                    Options = Options,
                    ParentMenuName = ParentMenuName,
                    PermissionPrefix = PermissionPrefix,
                    IsDatabaseTable = IsDatabaseTable,
                    IsGenMenu = IsGenMenu,
                    IsGenTranslation = IsGenTranslation,
                    IsGenCode = IsGenCode,
                    DefaultSortField = DefaultSortField,
                    DefaultSortOrder = DefaultSortOrder,
                    Remarks = Remarks
                };

                var result = await _genTableService.UpdateAsync(updateDto);
                stopwatch.Stop();

                if (!result.Success)
                {
                    Error = result.Message ?? "更新失败";
                    TaktMessageManager.ShowError(Error);
                    _operLog?.Error("[CodeGenerator] 更新失败，操作人={Operator}, Id={Id}, TableName={TableName}, Message={Message}", 
                        operatorName, Id, TableName, result.Message ?? "未知错误");
                    return;
                }

                var changes = $"TableName: {TableName}, ClassName: {ClassName}";
                var requestParams = JsonSerializer.Serialize(new { Id = Id, TableName = TableName, ClassName = ClassName });
                _operLog?.Update(entityName, Id.ToString(), operatorName, changes, "Generator.CodeGenForm", requestParams, (int)stopwatch.ElapsedMilliseconds);

                TaktMessageManager.ShowSuccess(_localizationManager.GetString("common.success.update") ?? "更新成功");
            }

            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Error = ex.Message;
            TaktMessageManager.ShowError(Error);
            _operLog?.Error(ex, "[CodeGenerator] 保存失败，操作人={Operator}, IsCreate={IsCreate}, TableName={TableName}", 
                operatorName, IsCreate, TableName);
        }
    }
}


