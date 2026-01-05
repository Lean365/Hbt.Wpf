// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Generator
// 文件名称：CodeGenFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表单视图模型（新建/编辑）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Newtonsoft.Json;
using System.IO;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;

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

    partial void OnDetailTableNameChanged(string? value)
    {
        // 当子表名称改变时，重新加载该表的字段列表
        _ = LoadDetailTableColumnsAsync(value);
    }

    [ObservableProperty]
    private string? _detailRelationField;

    /// <summary>
    /// 子表关联字段列表（根据 DetailTableName 动态加载）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _detailRelationFieldNames = new();

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

    /// <summary>
    /// 当前正在编辑的列（用于控制单行编辑状态）
    /// </summary>
    [ObservableProperty]
    private GenColumnDto? _editingColumn;

    /// <summary>
    /// 当 EditingColumn 改变时，通知所有相关命令重新评估 CanExecute
    /// </summary>
    partial void OnEditingColumnChanged(GenColumnDto? value)
    {
        // 通知所有行的按钮命令重新评估 CanExecute
        SaveColumnCommand.NotifyCanExecuteChanged();
        CancelUpdateCommand.NotifyCanExecuteChanged();
        UpdateColumnCommand.NotifyCanExecuteChanged();
        DeleteColumnCommand.NotifyCanExecuteChanged();

        // 触发全局命令重新评估，确保所有行的按钮状态都能正确更新
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

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
        ICodeGeneratorService? codeGeneratorService = null!,
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
        // 初始化列数据类型列表（按字母排序）
        ColumnDataTypes.Clear();
        ColumnDataTypes.Add("bigint"); // 64 位整数
        ColumnDataTypes.Add("bit"); // 布尔值
        ColumnDataTypes.Add("datetime"); // 日期时间
        ColumnDataTypes.Add("decimal"); // 精确数值类型
        ColumnDataTypes.Add("int"); // 32 位整数
        ColumnDataTypes.Add("nvarchar"); // 可变长度 Unicode 字符串
        ColumnDataTypes.Add("text"); // 文本类型
        ColumnDataTypes.Add("uniqueidentifier"); // 全局唯一标识符（GUID）
        ColumnDataTypes.Add("varchar"); // 可变长度字符串

        // 初始化数据类型列表（C#类型，按字母排序）
        DataTypes.Clear();
        DataTypes.Add("bool"); // bit
        DataTypes.Add("DateTime"); // datetime
        DataTypes.Add("decimal"); // decimal
        DataTypes.Add("Guid"); // uniqueidentifier
        DataTypes.Add("int"); // int
        DataTypes.Add("long"); // bigint
        DataTypes.Add("string"); // nvarchar, varchar, text

        // 初始化查询类型列表（按字母排序）
        QueryTypes.Clear();
        QueryTypes.Add("Between");   // 范围查询
        QueryTypes.Add("Equal");     // 精确查询
        QueryTypes.Add("GreaterThan"); // 大于查询
        QueryTypes.Add("GreaterThanOrEqual"); // 大于等于查询
        QueryTypes.Add("In");        // 包含查询
        QueryTypes.Add("LessThan");  // 小于查询
        QueryTypes.Add("LessThanOrEqual"); // 小于等于查询
        QueryTypes.Add("Like");      // 模糊查询
        QueryTypes.Add("NotEqual");  // 不等于查询

        // 初始化表单控件类型列表（MaterialDesign 控件类型，按字母排序）
        FormControlTypes.Clear();
        FormControlTypes.Add("CheckBox");          // 复选框
        FormControlTypes.Add("ComboBox");         // 下拉框
        FormControlTypes.Add("DatePicker");       // 日期选择器
        FormControlTypes.Add("DateTimePicker");   // 日期时间选择器
        FormControlTypes.Add("NumericUpDown");   // 数字输入框
        FormControlTypes.Add("PasswordBox");      // 密码框
        FormControlTypes.Add("RadioButton");      // 单选框
        FormControlTypes.Add("RichTextBox");      // 富文本编辑器
        FormControlTypes.Add("TextArea");         // 文本域（多行文本框）
        FormControlTypes.Add("TextBox");          // 文本框

        // 初始化字典类型列表（这里可以根据实际需求添加）
        DictTypes.Clear();
        // 字典类型通常是从系统字典表中动态加载的，这里先留空，后续可以从字典服务加载
        // 如果需要预设一些常用字典类型，可以在这里添加
    }

    /// <summary>
    /// 将列名转换为 C# 属性名（PascalCase）
    /// 例如：update_by -> UpdateBy
    /// </summary>
    public static string ConvertToPropertyName(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return string.Empty;
        }

        var parts = columnName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
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

        // 根据 ColumnName 自动生成 PropertyName
        if (!string.IsNullOrWhiteSpace(column.ColumnName))
        {
            column.PropertyName = ConvertToPropertyName(column.ColumnName);
        }

        // 根据 ColumnDataType 设置其他字段（只保留：nvarchar, varchar, int, bigint, bit, decimal, datetime, uniqueidentifier, text）
        switch (columnDataType)
        {
            case "nvarchar":
            case "varchar":
                column.DataType = "string";
                column.IsNullable = 0;  // 否
                column.IsPrimaryKey = 0;  // 否
                column.IsIdentity = 0;  // 否
                column.Length = 64;
                column.DecimalPlaces = 0;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;  // 是
                column.QueryType = "In";  // 包含
                column.IsCreate = 0;  // 可
                column.IsUpdate = 0;  // 可
                column.IsDelete = 0;  // 可
                column.IsList = 0;  // 可
                column.IsExport = 0;  // 可
                column.IsSort = 0;  // 可
                column.IsRequired = 0;  // 必填（0表示是）
                column.IsForm = 0;  // 可
                column.FormControlType = "TextBox";
                column.DictType = string.Empty;
                break;

            case "text":
                column.DataType = "string";
                column.IsNullable = 0;  // 否
                column.IsPrimaryKey = 0;  // 否
                column.IsIdentity = 0;  // 否
                column.Length = 64;
                column.DecimalPlaces = 0;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;  // 是
                column.QueryType = "In";  // 包含
                column.IsCreate = 0;  // 可
                column.IsUpdate = 0;  // 可
                column.IsDelete = 0;  // 可
                column.IsList = 0;  // 可
                column.IsExport = 0;  // 可
                column.IsSort = 0;  // 可
                column.IsRequired = 0;  // 必填（0表示是）
                column.IsForm = 0;  // 可
                column.FormControlType = "TextBox";
                column.DictType = string.Empty;
                break;

            case "decimal":
                column.DataType = "decimal";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 18;
                column.DecimalPlaces = 5;
                column.DefaultValue = "0";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "int":
                column.DataType = "int";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 11;
                column.DecimalPlaces = 0;
                column.DefaultValue = "0";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "bigint":
                column.DataType = "long";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 20;
                column.DecimalPlaces = 0;
                column.DefaultValue = "0";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "bit":
                column.DataType = "bool";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "CheckBox";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = "false";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "datetime":
                column.DataType = "DateTime";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Between";
                column.FormControlType = "DateTimePicker";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "uniqueidentifier":
                column.DataType = "Guid";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "TextBox";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            default:
                // 默认字符串类型（text）
                column.DataType = "string";
                if (string.IsNullOrWhiteSpace(column.ColumnDataType))
                {
                    column.ColumnDataType = "text";
                }
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "In";
                column.FormControlType = "TextBox";
                column.Length = 64;
                column.DecimalPlaces = 0;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;
        }
    }


    /// <summary>
    /// 根据 ColumnDataType 获取对应的 DataType 选项列表
    /// </summary>
    public ObservableCollection<string> GetDataTypeOptionsByColumnDataType(string? columnDataType)
    {
        var options = new ObservableCollection<string>();

        if (string.IsNullOrWhiteSpace(columnDataType))
        {
            return DataTypes; // 如果没有 ColumnDataType，返回所有选项
        }

        var dataType = columnDataType.ToLower();

        // 根据 ColumnDataType 返回对应的 DataType 选项（只保留：nvarchar, varchar, int, bigint, bit, decimal, datetime, uniqueidentifier, text）
        switch (dataType)
        {
            case "nvarchar":
            case "varchar":
            case "text":
                options.Add("string");
                break;

            case "decimal":
                options.Add("decimal");
                break;

            case "int":
                options.Add("int");
                break;

            case "bigint":
                options.Add("long");
                break;

            case "bit":
                options.Add("bool");
                break;

            case "datetime":
                options.Add("DateTime");
                break;

            case "uniqueidentifier":
                options.Add("Guid");
                break;

            default:
                // 默认返回所有选项
                foreach (var type in DataTypes)
                {
                    options.Add(type);
                }
                break;
        }

        return options;
    }

    /// <summary>
    /// 根据 DataType 获取对应的 ColumnDataType 选项列表
    /// </summary>
    public ObservableCollection<string> GetColumnDataTypeOptionsByDataType(string? dataType)
    {
        var options = new ObservableCollection<string>();

        if (string.IsNullOrWhiteSpace(dataType))
        {
            return ColumnDataTypes; // 如果没有 DataType，返回所有选项
        }

        // 根据 DataType 返回对应的 ColumnDataType 选项（只保留：nvarchar, varchar, int, bigint, bit, decimal, datetime, uniqueidentifier, text）
        switch (dataType)
        {
            case "string":
                options.Add("nvarchar");
                options.Add("varchar");
                options.Add("text");
                break;

            case "decimal":
                options.Add("decimal");
                break;

            case "int":
                options.Add("int");
                break;

            case "long":
                options.Add("bigint");
                break;

            case "bool":
                options.Add("bit");
                break;

            case "DateTime":
                options.Add("datetime");
                break;

            case "Guid":
                options.Add("uniqueidentifier");
                break;

            default:
                // 默认返回所有选项
                foreach (var type in ColumnDataTypes)
                {
                    options.Add(type);
                }
                break;
        }

        return options;
    }


    /// <summary>
    /// 根据数据类型（C#类型）自动同步其他字段
    /// </summary>
    public void SyncColumnFieldsByCSharpDataType(GenColumnDto column)
    {
        if (column == null || string.IsNullOrWhiteSpace(column.DataType))
        {
            return;
        }

        // 根据 ColumnName 自动生成 PropertyName
        if (!string.IsNullOrWhiteSpace(column.ColumnName))
        {
            column.PropertyName = ConvertToPropertyName(column.ColumnName);
        }

        var dataType = column.DataType;

        // 根据 DataType 设置其他字段
        switch (dataType)
        {
            case "string":
                // 根据 ColumnName 自动生成 PropertyName
                if (!string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    column.PropertyName = ConvertToPropertyName(column.ColumnName);
                }
                column.ColumnDataType = "text";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.Length = 64;
                column.DecimalPlaces = 0;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;
                column.QueryType = "In";
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.FormControlType = "TextBox";
                column.DictType = string.Empty;
                break;

            case "int":
                column.ColumnDataType = "int";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 11;
                column.DecimalPlaces = 0;
                column.DefaultValue = "0";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "long":
                column.ColumnDataType = "bigint";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 20;
                column.DecimalPlaces = 0;
                column.DefaultValue = "0";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "decimal":
                column.ColumnDataType = "decimal";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "NumericUpDown";
                column.Length = 18;
                column.DecimalPlaces = 5;
                column.DefaultValue = "0";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;


            case "bool":
                column.ColumnDataType = "bit";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "CheckBox";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = "false";
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "DateTime":
                column.ColumnDataType = "datetime";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Between";
                column.FormControlType = "DateTimePicker";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;

            case "Guid":
                column.ColumnDataType = "uniqueidentifier";
                column.IsNullable = 0;
                column.IsPrimaryKey = 0;
                column.IsIdentity = 0;
                column.QueryType = "Equal";
                column.FormControlType = "TextBox";
                column.Length = null;
                column.DecimalPlaces = null;
                column.DefaultValue = string.Empty;
                column.IsQuery = 0;
                column.IsCreate = 0;
                column.IsUpdate = 0;
                column.IsDelete = 0;
                column.IsList = 0;
                column.IsExport = 0;
                column.IsSort = 0;
                column.IsRequired = 0;
                column.IsForm = 0;
                column.DictType = string.Empty;
                break;
        }
    }

    /// <summary>
    /// 初始化创建模式（针对无数据表的情景，手动配置代码生成）
    /// </summary>
    public void ForCreate()
    {
        IsCreate = true;
        // 拼接标题：新增 + 代码生成配置
        var createText = _localizationManager.GetString("common.button.create");
        var entityText = _localizationManager.GetString("generator.gentable.entity");
        Title = $"{createText}{entityText}";
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
        // 拼接标题：更新 + 代码生成配置
        var updateText = _localizationManager.GetString("common.button.update");
        var entityText = _localizationManager.GetString("generator.gentable.entity");
        Title = $"{updateText}{entityText}";
        Id = dto.Id;
        TableName = dto.TableName;
        TableDescription = dto.TableDescription;
        ClassName = dto.ClassName;
        DetailTableName = dto.DetailTableName;
        // OnDetailTableNameChanged 会自动触发加载字段列表（异步）
        // DetailRelationField 会在字段列表加载完成后自动显示在 ComboBox 中
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
    /// 加载子表的字段列表（用于 DetailRelationField 联动）
    /// </summary>
    private async Task LoadDetailTableColumnsAsync(string? detailTableName)
    {
        if (string.IsNullOrWhiteSpace(detailTableName))
        {
            DetailRelationFieldNames.Clear();
            return;
        }

        try
        {
            await Task.Run(() =>
            {
                var columns = _metadataService.GetColumnsByTableName(detailTableName, isCache: false);
                DetailRelationFieldNames.Clear();
                if (columns != null && columns.Count > 0)
                {
                    foreach (var column in columns.OrderBy(c => c.ColumnName))
                    {
                        if (!string.IsNullOrWhiteSpace(column.ColumnName))
                        {
                            DetailRelationFieldNames.Add(column.ColumnName);
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[CodeGenForm] 加载子表字段列表失败，表名={DetailTableName}", detailTableName);
            DetailRelationFieldNames.Clear();
        }
    }

    /// <summary>
    /// 扁平化菜单树
    /// </summary>
    private static List<MenuDto> FlattenMenuTree(List<MenuDto> menuTree)
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
            var fieldName = _localizationManager.GetString("generator.gentable.tablename");
            TaktMessageManager.ShowWarning(string.Format(_localizationManager.GetString("common.validation.required"), fieldName));
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
            firstPart = char.ToLower(firstPart[0]) + (firstPart.Length > 1 ? firstPart.Substring(1) : string.Empty);
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
            if (PermissionPrefix != null && PermissionPrefix.StartsWith(':'))
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
            var fieldName = _localizationManager.GetString("generator.gentable.tablename");
            TaktMessageManager.ShowWarning(string.Format(_localizationManager.GetString("common.placeholder.input"), fieldName));
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
            PropertyName = ConvertToPropertyName(columnName), // 根据 ColumnName 自动生成
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
            QueryType = null!,
            IsCreate = 0,
            IsUpdate = 0,
            IsList = 0,
            IsSort = 1,
            IsExport = 1,
            IsForm = 0,
            IsRequired = 1,
            FormControlType = null!,
            DictType = null!,
            Remarks = null!
        };

        Columns.Add(newColumn);
        SelectedColumn = newColumn;

        // 只有在编辑模式下才设置 EditingColumn
        // 新建模式下允许连续添加多行，不需要设置编辑状态
        if (!IsCreate)
        {
            EditingColumn = newColumn;
        }

        // 记录日志
        _operLog?.Information("[CodeGenForm] 新增字段行，表名={TableName}, 列名={ColumnName}, 属性名={PropertyName}, 数据类型={DataType}, 库列类型={ColumnDataType}, 序号={OrderNum}",
            TableName, columnName, newColumn.PropertyName, newColumn.DataType, newColumn.ColumnDataType, columnNumber);

        // 通知命令重新评估 CanExecute
        CreateColumnCommand.NotifyCanExecuteChanged();
    }

    private bool CanCreateColumn()
    {
        // 在新建模式下，允许连续添加多行，即使当前有行正在编辑
        // 在编辑模式下，需要先保存当前编辑的行，才能继续添加
        if (IsCreate)
        {
            // 新建模式下，允许添加字段（即使表名为空，即使有行正在编辑）
            return true;
        }

        // 编辑模式下，如果当前有行正在编辑，不允许添加新行
        if (EditingColumn != null)
        {
            return false;
        }

        // 编辑模式下，只有当 IsDatabaseTable = 1（无数据表）时，才允许添加新行
        // IsDatabaseTable = 0（有数据表）时，不允许手动添加行，只能从数据库导入
        if (IsDatabaseTable == 0)
        {
            return false;
        }

        // IsDatabaseTable = 1 时，允许添加新行，但表名必须不为空
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
        EditingColumn = column;

        // 通知命令重新评估 CanExecute
        SaveColumnCommand.NotifyCanExecuteChanged();
        CancelUpdateCommand.NotifyCanExecuteChanged();

        // 记录日志
        _operLog?.Information("[CodeGenForm] 进入编辑字段状态，表名={TableName}, 字段Id={Id}, 列名={ColumnName}, 属性名={PropertyName}",
            TableName, column.Id, column.ColumnName, column.PropertyName ?? string.Empty);
    }

    private bool CanUpdateColumn(GenColumnDto? column)
    {
        if (column == null)
        {
            column = SelectedColumn;
        }

        return !IsCreate && EditingColumn == null && column != null && column.Id > 0;
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
            var fieldName = _localizationManager.GetString("generator.gencolumn.columnname");
            TaktMessageManager.ShowWarning(string.Format(_localizationManager.GetString("common.validation.required"), fieldName));
            return;
        }

        if (string.IsNullOrWhiteSpace(column.PropertyName))
        {
            var fieldName = _localizationManager.GetString("generator.gencolumn.propertyname");
            TaktMessageManager.ShowWarning(string.Format(_localizationManager.GetString("common.validation.required"), fieldName));
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
                    EditingColumn = null;

                    // 通知命令重新评估 CanExecute
                    SaveColumnCommand.NotifyCanExecuteChanged();
                    CancelUpdateCommand.NotifyCanExecuteChanged();

                    // 记录日志
                    _operLog?.Information("[CodeGenForm] 字段添加成功，表名={TableName}, 字段Id={Id}, 列名={ColumnName}, 属性名={PropertyName}, 数据类型={DataType}, 库列类型={ColumnDataType}",
                        TableName, column.Id, column.ColumnName, column.PropertyName ?? string.Empty, column.DataType ?? string.Empty, column.ColumnDataType ?? string.Empty);

                    await LoadColumnsAsync();
                }
                else
                {
                    var errorMessage = result.Message ?? "字段添加失败";
                    TaktMessageManager.ShowError(errorMessage);

                    // 记录错误日志
                    _operLog?.Error("[CodeGenForm] 字段添加失败，表名={TableName}, 列名={ColumnName}, 错误信息={ErrorMessage}",
                        TableName, column.ColumnName, errorMessage);
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
                    EditingColumn = null;

                    // 通知命令重新评估 CanExecute
                    SaveColumnCommand.NotifyCanExecuteChanged();
                    CancelUpdateCommand.NotifyCanExecuteChanged();

                    // 记录日志
                    _operLog?.Information("[CodeGenForm] 字段更新成功，表名={TableName}, 字段Id={Id}, 列名={ColumnName}, 属性名={PropertyName}, 数据类型={DataType}, 库列类型={ColumnDataType}",
                        TableName, column.Id, column.ColumnName, column.PropertyName ?? string.Empty, column.DataType ?? string.Empty, column.ColumnDataType ?? string.Empty);

                    await LoadColumnsAsync();
                }
                else
                {
                    var errorMessage = result.Message ?? "字段更新失败";
                    TaktMessageManager.ShowError(errorMessage);

                    // 记录错误日志
                    _operLog?.Error("[CodeGenForm] 字段更新失败，表名={TableName}, 字段Id={Id}, 列名={ColumnName}, 错误信息={ErrorMessage}",
                        TableName, column.Id, column.ColumnName, errorMessage);
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

        // 只要有正在编辑的列，且当前列是正在编辑的列，就可以保存
        // 新建模式下也可以保存（用于保存新添加的列）
        return EditingColumn != null && EditingColumn == column;
    }

    /// <summary>
    /// 取消编辑
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelUpdate))]
    private async Task CancelUpdateAsync()
    {
        if (EditingColumn != null)
        {
            EditingColumn = null;

            // 通知命令重新评估 CanExecute
            SaveColumnCommand.NotifyCanExecuteChanged();
            CancelUpdateCommand.NotifyCanExecuteChanged();

            await LoadColumnsAsync(); // 重新加载以恢复原始数据
        }
    }

    private bool CanCancelUpdate()
    {
        // 只要有正在编辑的列，就可以取消
        // 新建模式下也可以取消（用于取消新添加的列的编辑）
        return EditingColumn != null;
    }

    /// <summary>
    /// 导入表结构（从数据库导入列配置）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanImportTableStructure))]
    private async Task ImportTableStructureAsync()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            var fieldName = _localizationManager.GetString("generator.gentable.tablename");
            TaktMessageManager.ShowWarning(string.Format(_localizationManager.GetString("common.validation.required"), fieldName));
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

            // 记录删除前的信息
            var columnId = column.Id;
            var columnName = column.ColumnName;
            var propertyName = column.PropertyName;

            var result = await _genColumnService.DeleteAsync(column.Id);
            if (result.Success)
            {
                TaktMessageManager.ShowSuccess("字段删除成功");

                // 记录日志
                _operLog?.Information("[CodeGenForm] 字段删除成功，表名={TableName}, 字段Id={Id}, 列名={ColumnName}, 属性名={PropertyName}",
                    TableName, columnId, columnName ?? string.Empty, propertyName ?? string.Empty);

                await LoadColumnsAsync();
            }
            else
            {
                var errorMessage = result.Message ?? "字段删除失败";
                TaktMessageManager.ShowError(errorMessage);

                // 记录错误日志
                _operLog?.Error("[CodeGenForm] 字段删除失败，表名={TableName}, 字段Id={Id}, 列名={ColumnName}, 错误信息={ErrorMessage}",
                    TableName, columnId, columnName, errorMessage);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"删除字段失败: {ex.Message}";
            TaktMessageManager.ShowError(errorMessage);

            // 记录异常日志
            _operLog?.Error(ex, "[CodeGenForm] 字段删除异常，表名={TableName}, 字段Id={Id}, 列名={ColumnName}",
                TableName, column.Id, column.ColumnName);
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

        // 编辑模式下，如果 IsDatabaseTable = 0（有数据表），不允许删除字段
        if (!IsCreate && IsDatabaseTable == 0)
        {
            return false;
        }

        return !IsCreate && EditingColumn == null && column != null && column.Id > 0;
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
            TableNameError = _localizationManager.GetString("generator.gentable.validation.tablenamerequired");
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var entityName = _localizationManager.GetString("generator.gentable.entity");

        try
        {
            if (IsCreate)
            {
                _operLog?.Information("[CodeGenerator] 开始创建代码生成配置，操作人={Operator}, TableName={TableName}", operatorName, TableName);

                var createDto = new GenTableCreateDto
                {
                    TableName = TableName,
                    TableDescription = TableDescription!,
                    ClassName = ClassName!,
                    DetailTableName = DetailTableName!,
                    DetailRelationField = DetailRelationField!,
                    TreeCodeField = TreeCodeField!,
                    TreeParentCodeField = TreeParentCodeField!,
                    TreeNameField = TreeNameField!,
                    Author = Author!,
                    TemplateType = TemplateType!,
                    GenNamespacePrefix = GenNamespacePrefix!,
                    GenBusinessName = GenBusinessName!,
                    GenModuleName = GenModuleName!,
                    GenFunctionName = GenFunctionName!,
                    GenType = GenType!,
                    GenFunctions = GenFunctions!,
                    GenPath = GenPath!,
                    Options = Options!,
                    ParentMenuName = ParentMenuName!,
                    PermissionPrefix = PermissionPrefix!,
                    IsDatabaseTable = IsDatabaseTable,
                    IsGenMenu = IsGenMenu,
                    IsGenTranslation = IsGenTranslation,
                    IsGenCode = IsGenCode,
                    DefaultSortField = DefaultSortField!,
                    DefaultSortOrder = DefaultSortOrder!,
                    Remarks = Remarks!
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

                var requestParams = JsonConvert.SerializeObject(new { TableName = TableName, ClassName = ClassName });
                var entityId = result.Data > 0 ? result.Data.ToString() : "0";
                // 确保 ElapsedTime 正确计算（毫秒），处理溢出和精度问题
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var elapsedTime = elapsedMs > int.MaxValue ? int.MaxValue : (int)elapsedMs;
                _operLog?.Create(entityName, entityId, operatorName, "Generator.CodeGenForm", requestParams, null, elapsedTime);

                TaktMessageManager.ShowSuccess(string.Format(_localizationManager.GetString("common.success.create"), entityName));
            }
            else
            {
                _operLog?.Information("[CodeGenerator] 开始更新代码生成配置，操作人={Operator}, Id={Id}, TableName={TableName}",
                    operatorName, Id, TableName);

                var updateDto = new GenTableUpdateDto
                {
                    Id = Id,
                    TableName = TableName,
                    TableDescription = TableDescription!,
                    ClassName = ClassName!,
                    DetailTableName = DetailTableName!,
                    DetailRelationField = DetailRelationField!,
                    TreeCodeField = TreeCodeField!,
                    TreeParentCodeField = TreeParentCodeField!,
                    TreeNameField = TreeNameField!,
                    Author = Author!,
                    TemplateType = TemplateType!,
                    GenNamespacePrefix = GenNamespacePrefix!,
                    GenBusinessName = GenBusinessName!,
                    GenModuleName = GenModuleName!,
                    GenFunctionName = GenFunctionName!,
                    GenType = GenType!,
                    GenFunctions = GenFunctions!,
                    GenPath = GenPath!,
                    Options = Options!,
                    ParentMenuName = ParentMenuName!,
                    PermissionPrefix = PermissionPrefix!,
                    IsDatabaseTable = IsDatabaseTable,
                    IsGenMenu = IsGenMenu,
                    IsGenTranslation = IsGenTranslation,
                    IsGenCode = IsGenCode,
                    DefaultSortField = DefaultSortField!,
                    DefaultSortOrder = DefaultSortOrder!,
                    Remarks = Remarks!
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
                var requestParams = JsonConvert.SerializeObject(new { Id = Id, TableName = TableName, ClassName = ClassName });
                // 确保 ElapsedTime 正确计算（毫秒），处理溢出和精度问题
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var elapsedTime = elapsedMs > int.MaxValue ? int.MaxValue : (int)elapsedMs;
                _operLog?.Update(entityName, Id.ToString(), operatorName, changes, "Generator.CodeGenForm", requestParams, null, elapsedTime);

                TaktMessageManager.ShowSuccess(string.Format(_localizationManager.GetString("common.success.update"), entityName));
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


