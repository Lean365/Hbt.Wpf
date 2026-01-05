// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Generator.Engine
// 文件名称：CodeGeneratorService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成服务实现
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Scriban;
using Scriban.Runtime;
using System.Text;
using Takt.Application.Dtos.Generator;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;

namespace Takt.Application.Services.Generator.Engine;

/// <summary>
/// 代码生成服务实现
/// 
/// 使用场景：
/// 1. 有表：先使用 ImportFromTableAsync 从数据库表导入到 GenTable 和 GenColumn，然后在 UI 中修改配置，最后使用 GenerateFromConfigAsync 生成代码
/// 2. 无表：先在 UI 中创建并保存 GenTable 和 GenColumn 配置，然后使用 GenerateFromConfigAsync 生成代码
/// 
/// 注意：所有代码生成都必须从 GenTable 和 GenColumn 配置获取，不能直接从数据库表生成
/// </summary>
public class CodeGeneratorService : ICodeGeneratorService
{
    private readonly IDatabaseMetadataService _metadataService;
    private readonly IGenTableService _genTableService;
    private readonly IGenColumnService _genColumnService;
    private readonly AppLogManager _appLog;
    private readonly string _templateDirectory;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="metadataService">数据库元数据服务</param>
    /// <param name="genTableService">代码生成表配置服务</param>
    /// <param name="genColumnService">代码生成列配置服务</param>
    /// <param name="appLog">应用程序日志管理器</param>
    public CodeGeneratorService(
        IDatabaseMetadataService metadataService,
        IGenTableService genTableService,
        IGenColumnService genColumnService,
        AppLogManager appLog)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _genTableService = genTableService ?? throw new ArgumentNullException(nameof(genTableService));
        _genColumnService = genColumnService ?? throw new ArgumentNullException(nameof(genColumnService));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));

        // 使用符合 Windows 规范的模板目录
        // 优先使用用户自定义模板，如果不存在则使用默认模板（应用目录）
        _templateDirectory = Takt.Common.Helpers.PathHelper.GetDefaultTemplateDirectory();
    }

    /// <summary>
    /// 从配置生成代码（从 GenTable 和 GenColumn 获取配置）
    /// 
    /// 使用场景：
    /// - 场景1：有表时，先使用 ImportFromTableAsync 导入，修改配置后调用此方法生成代码
    /// - 场景2：无表时，先在 UI 中创建 GenTable 和 GenColumn 配置，然后调用此方法生成代码
    /// </summary>
    /// <param name="tableConfig">表配置（GenTable）</param>
    /// <param name="columnConfigs">列配置列表（GenColumn）</param>
    /// <param name="options">生成选项</param>
    /// <returns>生成的代码文件字典（文件名 -> 文件内容）</returns>
    public async Task<Dictionary<string, string>> GenerateFromConfigAsync(GenTableDto tableConfig, List<GenColumnDto> columnConfigs, CodeGenerationOptions options)
    {
        var result = new Dictionary<string, string>();

        // 确定模板类型（CRUD、MasterDetail、Tree）
        var templateType = DetermineTemplateType(tableConfig);

        // 准备模板数据
        var templateData = PrepareTemplateData(tableConfig, columnConfigs, options);
        templateData["TemplateType"] = templateType;

        // 生成实体
        if (options.GenerateEntity)
        {
            var entityCode = await RenderTemplateAsync("Entity.sbn", templateType, templateData);
            var entityFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}.cs";
            result[entityFileName] = entityCode;
        }

        // 生成DTO
        if (options.GenerateDto)
        {
            var dtoCode = await RenderTemplateAsync("Dto.sbn", templateType, templateData);
            var dtoFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}Dto.cs";
            result[dtoFileName] = dtoCode;
        }

        // 生成服务接口
        if (options.GenerateIService)
        {
            var iServiceCode = await RenderTemplateAsync("IService.sbn", templateType, templateData);
            var iServiceFileName = $"I{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}Service.cs";
            result[iServiceFileName] = iServiceCode;
        }

        // 生成服务实现
        if (options.GenerateService)
        {
            var serviceCode = await RenderTemplateAsync("Service.sbn", templateType, templateData);
            var serviceFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}Service.cs";
            result[serviceFileName] = serviceCode;
        }

        // 生成ViewModel
        if (options.GenerateViewModel)
        {
            var viewModelCode = await RenderTemplateAsync("ViewModel.sbn", templateType, templateData);
            var viewModelFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}ViewModel.cs";
            result[viewModelFileName] = viewModelCode;
        }

        // 生成FormViewModel
        if (options.GenerateFormViewModel)
        {
            var formViewModelCode = await RenderTemplateAsync("FormViewModel.sbn", templateType, templateData);
            var formViewModelFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}FormViewModel.cs";
            result[formViewModelFileName] = formViewModelCode;
        }

        // 生成View
        if (options.GenerateView)
        {
            var viewCode = await RenderTemplateAsync("View.xaml.sbn", templateType, templateData);
            var viewFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}View.xaml";
            result[viewFileName] = viewCode;
        }

        // 生成FormView
        if (options.GenerateFormView)
        {
            var formViewCode = await RenderTemplateAsync("FormView.xaml.sbn", templateType, templateData);
            var formViewFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}Form.xaml";
            result[formViewFileName] = formViewCode;
        }

        // 生成菜单SQL
        if (options.GenerateMenuSql)
        {
            var menuSqlCode = await RenderTemplateAsync("Menu.sql.sbn", templateType, templateData);
            var menuSqlFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}_Menu.sql";
            result[menuSqlFileName] = menuSqlCode;
        }

        // 生成翻译SQL
        if (options.GenerateTranslationSql)
        {
            var translationSqlCode = await RenderTemplateAsync("Translation.sql.sbn", templateType, templateData);
            var translationSqlFileName = $"{tableConfig.ClassName ?? GetClassName(tableConfig.TableName)}_Translation.sql";
            result[translationSqlFileName] = translationSqlCode;
        }

        return result;
    }

    /// <summary>
    /// 确定模板类型
    /// </summary>
    private static string DetermineTemplateType(GenTableDto tableConfig)
    {
        // 优先使用配置的模板类型
        if (!string.IsNullOrWhiteSpace(tableConfig.TemplateType))
        {
            return tableConfig.TemplateType;
        }

        // 根据表配置自动判断
        if (!string.IsNullOrWhiteSpace(tableConfig.TreeCodeField) &&
            !string.IsNullOrWhiteSpace(tableConfig.TreeParentCodeField))
        {
            return "Tree";
        }

        if (!string.IsNullOrWhiteSpace(tableConfig.DetailTableName) &&
            !string.IsNullOrWhiteSpace(tableConfig.DetailRelationField))
        {
            return "MasterDetail";
        }

        // 默认使用 CRUD
        return "CRUD";
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    private async Task<string> RenderTemplateAsync(string templateName, string templateType, ScriptObject templateData)
    {
        // 尝试加载特定类型的模板（如 Entity_CRUD.sbn, Entity_MasterDetail.sbn, Entity_Tree.sbn）
        var specificTemplateName = $"{Path.GetFileNameWithoutExtension(templateName)}_{templateType}{Path.GetExtension(templateName)}";

        // 优先查找用户自定义模板，如果不存在则使用默认模板
        string? templatePath = null;

        // 1. 优先查找用户自定义的特定类型模板
        templatePath = Takt.Common.Helpers.PathHelper.GetTemplateFilePath(specificTemplateName, templateType);

        // 2. 如果不存在，查找用户自定义的通用模板
        if (templatePath == null)
        {
            templatePath = Takt.Common.Helpers.PathHelper.GetTemplateFilePath(templateName, templateType);
        }

        // 3. 如果用户模板不存在，查找默认模板目录的特定类型模板
        if (templatePath == null)
        {
            var defaultTemplateDir = Takt.Common.Helpers.PathHelper.GetDefaultTemplateDirectory();
            var specificTemplatePath = Path.Combine(defaultTemplateDir, templateType, specificTemplateName);
            if (File.Exists(specificTemplatePath))
            {
                templatePath = specificTemplatePath;
            }
        }

        // 4. 如果不存在，查找默认模板目录的通用模板
        if (templatePath == null)
        {
            var defaultTemplateDir = Takt.Common.Helpers.PathHelper.GetDefaultTemplateDirectory();
            var genericTemplatePath = Path.Combine(defaultTemplateDir, templateType, templateName);
            if (File.Exists(genericTemplatePath))
            {
                templatePath = genericTemplatePath;
            }
            else
            {
                // 5. 最后尝试根目录
                var rootTemplatePath = Path.Combine(defaultTemplateDir, templateName);
                if (File.Exists(rootTemplatePath))
                {
                    templatePath = rootTemplatePath;
                }
            }
        }

        if (templatePath == null || !File.Exists(templatePath))
        {
            throw new FileNotFoundException($"模板文件不存在: {templateName} (类型: {templateType})");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            throw new InvalidOperationException($"模板解析错误: {string.Join(", ", template.Messages)}");
        }

        var context = new TemplateContext();
        context.PushGlobal(templateData);

        var result = await template.RenderAsync(context);
        return result;
    }

    /// <summary>
    /// 准备模板数据
    /// </summary>
    private ScriptObject PrepareTemplateData(GenTableDto tableConfig, List<GenColumnDto> columnConfigs, CodeGenerationOptions options)
    {
        var scriptObject = new ScriptObject();

        var className = tableConfig.ClassName ?? GetClassName(tableConfig.TableName);
        // 命名空间由命名空间前缀动态生成（不再使用 ModuleCode）
        var namespacePrefix = !string.IsNullOrWhiteSpace(tableConfig.GenNamespacePrefix) ? tableConfig.GenNamespacePrefix : "Takt";
        var namespaceName = namespacePrefix; // 命名空间就是命名空间前缀

        // 表信息
        scriptObject["TableName"] = tableConfig.TableName;
        scriptObject["table_name"] = tableConfig.TableName; // 小写版本
        scriptObject["TableDescription"] = tableConfig.TableDescription ?? string.Empty;
        scriptObject["ClassName"] = className;
        scriptObject["class_name"] = className; // 小写版本
        scriptObject["Namespace"] = namespaceName;
        scriptObject["namespace"] = namespaceName; // 小写版本
        scriptObject["ModuleName"] = tableConfig.GenModuleName ?? string.Empty;
        scriptObject["BusinessName"] = tableConfig.GenBusinessName ?? tableConfig.TableDescription ?? string.Empty;
        scriptObject["FunctionName"] = tableConfig.GenFunctionName ?? tableConfig.TableDescription ?? string.Empty;
        scriptObject["Author"] = options.Author;
        scriptObject["author"] = options.Author; // 小写版本
        scriptObject["CreatedTime"] = options.CreatedTime.ToString("yyyy-MM-dd");
        scriptObject["created_time"] = options.CreatedTime.ToString("yyyy-MM-dd"); // 小写版本

        // 列信息（按 OrderNum 排序）
        var columns = columnConfigs.OrderBy(c => c.OrderNum).ToList();
        scriptObject["Columns"] = columns;
        scriptObject["columns"] = columns; // 小写版本
        scriptObject["BusinessColumns"] = columns.Where(c => !IsAuditColumn(c.ColumnName) && !IsPrimaryKeyColumn(c)).ToList();
        scriptObject["business_columns"] = columns.Where(c => !IsAuditColumn(c.ColumnName) && !IsPrimaryKeyColumn(c)).ToList(); // 小写版本
        scriptObject["QueryColumns"] = columns.Where(c => c.IsQuery == 0).ToList();
        scriptObject["FormColumns"] = columns.Where(c => c.IsForm == 0 && !IsAuditColumn(c.ColumnName) && !IsPrimaryKeyColumn(c)).ToList();
        scriptObject["ListColumns"] = columns.Where(c => c.IsList == 0).ToList();

        // 检查是否有枚举类型
        var hasEnum = columns.Any(c => c.DataType != null && (c.DataType.Contains("Enum") || c.DataType.Contains("Status") || c.DataType.Contains("Type")));
        scriptObject["HasEnum"] = hasEnum;
        scriptObject["has_enum"] = hasEnum; // 小写版本

        // 主键列
        var primaryKeyColumn = columns.FirstOrDefault(c => c.IsPrimaryKey == 0);
        scriptObject["PrimaryKeyColumn"] = primaryKeyColumn;
        scriptObject["PrimaryKeyProperty"] = primaryKeyColumn?.PropertyName ?? "Id";
        scriptObject["primary_key_property"] = primaryKeyColumn?.PropertyName ?? "Id"; // 小写版本
        scriptObject["PrimaryKeyType"] = primaryKeyColumn?.DataType ?? "long";
        scriptObject["primary_key_type"] = primaryKeyColumn?.DataType ?? "long"; // 小写版本

        return scriptObject;
    }


    /// <summary>
    /// 获取类名（从表名转换）
    /// </summary>
    private static string GetClassName(string tableName)
    {
        // 移除前缀（如 takt_oidc_）
        var name = tableName;
        if (name.StartsWith("takt_"))
        {
            name = name.Substring(5);
        }

        // 转换为 PascalCase
        var parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
    }

    /// <summary>
    /// 转换为属性名（从列名转换）
    /// </summary>
    private static string ConvertToPropertyName(string columnName)
    {
        var parts = columnName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
    }

    /// <summary>
    /// 获取命名空间
    /// </summary>
    private static string GetNamespace(string? moduleCode)
    {
        if (string.IsNullOrWhiteSpace(moduleCode))
            return "Takt.Domain.Entities";

        return $"Takt.Domain.Entities.{moduleCode}";
    }

    /// <summary>
    /// 转换 SQL 类型到 C# 类型
    /// </summary>
    private static string ConvertSqlTypeToCSharpType(string sqlType, bool isNullable)
    {
        var csharpType = sqlType.ToLower() switch
        {
            "int" => "int",
            "bigint" => "long",
            "smallint" => "short",
            "tinyint" => "byte",
            "bit" => "bool",
            "decimal" or "numeric" => "decimal",
            "float" => "double",
            "real" => "float",
            "money" or "smallmoney" => "decimal",
            "datetime" or "datetime2" or "smalldatetime" => "DateTime",
            "date" => "DateTime",
            "time" => "TimeSpan",
            "char" or "varchar" or "nchar" or "nvarchar" or "text" or "ntext" => "string",
            "uniqueidentifier" => "Guid",
            "binary" or "varbinary" or "image" => "byte[]",
            "xml" => "string",
            _ => "string"
        };

        // 如果是可空类型且不是 string，添加 ?
        if (isNullable && csharpType != "string" && csharpType != "byte[]")
        {
            return $"{csharpType}?";
        }

        return csharpType;
    }

    /// <summary>
    /// 判断是否为审计字段
    /// </summary>
    private static bool IsAuditColumn(string columnName)
    {
        var auditColumns = new[] { "created_by", "created_time", "updated_by", "updated_time", "is_deleted", "deleted_by", "deleted_time" };
        return auditColumns.Contains(columnName.ToLower());
    }

    /// <summary>
    /// 从表名提取命名空间前缀（第一个下划线之前的字符，首字母大写）
    /// 例如：takt_logging_exception_log -> Takt
    /// </summary>
    private static string ExtractNamespacePrefix(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return "Takt";

        var firstUnderscoreIndex = tableName.IndexOf('_');
        if (firstUnderscoreIndex <= 0)
            return "Takt"; // 如果没有下划线或下划线在开头，返回默认值

        var prefix = tableName.Substring(0, firstUnderscoreIndex);
        // 首字母大写，其余保持原样
        if (prefix.Length > 0)
        {
            return char.ToUpper(prefix[0]) + (prefix.Length > 1 ? prefix.Substring(1) : string.Empty);
        }

        return "Takt";
    }

    /// <summary>
    /// 从表名提取权限前缀（第一个下划线之后的所有字符，下划线转换为冒号）
    /// 例如：takt_logging_exception_log -> logging:exception:log
    /// </summary>
    private static string ExtractPermissionPrefix(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return string.Empty;

        var firstUnderscoreIndex = tableName.IndexOf('_');
        if (firstUnderscoreIndex < 0 || firstUnderscoreIndex >= tableName.Length - 1)
            return string.Empty; // 如果没有下划线或下划线在末尾，返回空

        var suffix = tableName.Substring(firstUnderscoreIndex + 1);
        // 将下划线替换为冒号
        return suffix.Replace('_', ':');
    }

    /// <summary>
    /// 从表名提取模块名（第二个下划线之前的部分）
    /// 例如：takt_logging_exception_log -> logging
    /// </summary>
    private static string ExtractModuleName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return string.Empty;

        var parts = tableName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            // 返回第二个部分（索引为1）
            return parts[1];
        }

        return string.Empty;
    }

    /// <summary>
    /// 从表名和表描述提取业务名称
    /// 优先使用表描述，如果没有则使用类名
    /// </summary>
    private static string ExtractBusinessName(string tableName, string? tableDescription)
    {
        if (!string.IsNullOrWhiteSpace(tableDescription))
            return tableDescription;

        // 如果没有表描述，使用类名
        var className = GetClassName(tableName);
        return className;
    }

    /// <summary>
    /// 从表名和表描述提取功能名称
    /// 优先使用表描述，如果没有则使用类名
    /// </summary>
    private static string ExtractFunctionName(string tableName, string? tableDescription)
    {
        if (!string.IsNullOrWhiteSpace(tableDescription))
            return tableDescription;

        // 如果没有表描述，使用类名
        var className = GetClassName(tableName);
        return className;
    }

    /// <summary>
    /// 判断是否为主键列
    /// </summary>
    private static bool IsPrimaryKeyColumn(GenColumnDto column)
    {
        return column.IsPrimaryKey == 0;
    }

    /// <summary>
    /// 从数据库表导入并保存到 GenTable 和 GenColumn
    /// 
    /// 使用场景：有表时，先调用此方法导入配置，然后可以在 UI 中修改配置，最后使用 GenerateFromConfigAsync 生成代码
    /// 
    /// 流程：
    /// 1. 从数据库获取表信息和列信息
    /// 2. 创建 GenTable 配置
    /// 3. 创建 GenColumn 配置（使用新的规则：0=是，1=否）
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="author">作者（可选）</param>
    /// <returns>操作结果</returns>
    public async Task<Result> ImportFromTableAsync(string tableName, string? author = null)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return Result.Fail("表名不能为空");

        try
        {
            // 获取表信息
            var tableInfo = _metadataService.GetTableInfoList().FirstOrDefault(t => t.TableName == tableName);
            if (tableInfo == null)
                return Result.Fail($"表 {tableName} 不存在");

            // 获取列信息（禁用缓存，确保获取最新的列信息）
            var columns = _metadataService.GetColumnsByTableName(tableName, isCache: false);
            if (columns == null || columns.Count == 0)
                return Result.Fail($"表 {tableName} 没有列信息");

            _appLog.Information("从数据库获取表 {TableName} 的列信息，共 {Count} 个列: {Columns}",
                tableName, columns.Count, string.Join(", ", columns.Select(c => c.ColumnName)));

            // 生成类名
            var className = GetClassName(tableName);

            // 从表名提取命名空间前缀和权限前缀
            var namespacePrefix = ExtractNamespacePrefix(tableName);
            var permissionPrefix = ExtractPermissionPrefix(tableName);

            // 从表名提取模块名、业务名和功能名
            var moduleName = ExtractModuleName(tableName);
            var businessName = ExtractBusinessName(tableName, tableInfo.Description);
            var functionName = ExtractFunctionName(tableName, tableInfo.Description);

            // 处理列配置：先准备所有列配置数据（在创建 GenTable 之前，避免连接关闭问题）
            int orderNum = 1;
            var createColumnDtos = new List<GenColumnCreateDto>();

            _appLog.Information("开始导入表 {TableName} 的列配置，共 {Count} 个列",
                tableName, columns.Count);

            // 准备所有列配置数据
            foreach (var column in columns)
            {
                var propertyName = ConvertToPropertyName(column.ColumnName);
                var dataType = ConvertSqlTypeToCSharpType(column.DataType, column.IsNullable);

                // 判断是否为审计字段（除ID和备注以外）
                var isAuditColumn = IsAuditColumn(column.ColumnName);
                var isPrimaryKey = column.IsPrimaryKey;
                var isRemarks = column.ColumnName.Equals("remarks", StringComparison.OrdinalIgnoreCase);
                var isAuditButNotIdOrRemarks = isAuditColumn && !isPrimaryKey && !isRemarks;

                // 准备列配置DTO
                var createColumnDto = new GenColumnCreateDto
                {
                    // ========== 基本信息 ==========
                    TableName = tableName,
                    ColumnName = column.ColumnName,
                    PropertyName = propertyName,
                    ColumnDescription = column.Description!,

                    // ========== 数据类型 ==========
                    DataType = dataType,
                    ColumnDataType = column.DataType,

                    // ========== 列属性 ==========
                    IsNullable = column.IsNullable ? 0 : 1,
                    IsPrimaryKey = column.IsPrimaryKey ? 0 : 1,
                    IsIdentity = column.IsIdentity ? 0 : 1,
                    Length = column.Length,
                    DecimalPlaces = column.DecimalPlaces,
                    DefaultValue = column.DefaultValue!,

                    // ========== 排序 ==========
                    OrderNum = orderNum++,

                    // ========== 生成控制 ==========
                    // 审计字段（除ID和备注以外）：查询、新增、更新、列表、排序、导出、前端都应该是1（不需要）
                    // 其他字段使用默认值：IsQuery=1, IsCreate=0, IsUpdate=0, IsList=0, IsSort=1, IsExport=1, IsForm=0
                    IsQuery = isAuditButNotIdOrRemarks ? 1 : 1,
                    QueryType = null!, // 查询方式（可选，导入时不设置）
                    IsCreate = isAuditButNotIdOrRemarks ? 1 : 0,
                    IsUpdate = isAuditButNotIdOrRemarks ? 1 : 0,
                    IsList = isAuditButNotIdOrRemarks ? 1 : 0,
                    IsSort = isAuditButNotIdOrRemarks ? 1 : 1,
                    IsExport = isAuditButNotIdOrRemarks ? 1 : 1,
                    IsForm = isAuditButNotIdOrRemarks ? 1 : 0,
                    IsRequired = column.IsNullable ? 1 : 0,

                    // ========== UI相关 ==========
                    FormControlType = null!, // 表单显示类型（可选，导入时不设置）
                    DictType = null!, // 字典类型（可选，导入时不设置）

                    // ========== 备注 ==========
                    Remarks = null! // 备注（可选，导入时不设置）
                };

                createColumnDtos.Add(createColumnDto);
            }

            // 先创建 GenTable 配置（在批量创建列之前，确保连接状态正常）
            var createTableDto = new GenTableCreateDto
            {
                // ========== 基本信息 ==========
                TableName = tableInfo.TableName,
                TableDescription = tableInfo.Description!,
                ClassName = className,
                Author = author ?? "Takt365(Cursor AI)",

                // ========== 生成信息 ==========
                TemplateType = "CRUD", // 导入时统一默认为CRUD，用户可在UI中修改为MasterDetail或Tree
                GenNamespacePrefix = namespacePrefix ?? "Takt", // 如果提取失败，使用默认值
                GenBusinessName = businessName,
                GenModuleName = moduleName,
                GenFunctionName = functionName,
                GenFunctions = "List,Query,Create,Update,Delete,Import,Export", // 默认生成功能
                PermissionPrefix = permissionPrefix ?? string.Empty, // 如果提取失败，使用空字符串

                // ========== 主子表和树表字段（导入时不自动判断，统一设置为null，用户可在UI中手动配置）==========
                DetailTableName = null!,
                DetailRelationField = null!,
                TreeCodeField = null!,
                TreeParentCodeField = null!,
                TreeNameField = null!,

                // ========== 生成控制 ==========
                IsDatabaseTable = 0, // 有表（0=有表，1=无表）
                IsGenMenu = 1, // 不生成菜单（0=是，1=否）
                IsGenTranslation = 1, // 不生成翻译（0=是，1=否）
                IsGenCode = 1, // 不生成代码（0=是，1=否）

                // ========== 其他字段（导入时不设置，用户可在UI中手动配置）==========
                GenType = null!, // 生成方式（zip=压缩包，path=自定义路径）
                GenPath = null!, // 代码生成路径
                Options = null!, // 其它生成选项
                ParentMenuName = null!, // 上级菜单名称
                DefaultSortField = null!, // 默认排序字段
                DefaultSortOrder = "ASC", // 默认排序（ASC=正序，DESC=倒序）
                Remarks = null! // 备注
            };
            var createTableResult = await _genTableService.CreateAsync(createTableDto);
            if (!createTableResult.Success)
                return Result.Fail($"创建表配置失败: {createTableResult.Message}");

            _appLog.Information("表 {TableName} 配置创建成功 (ID: {Id})", tableName, createTableResult.Data);

            // 批量创建列配置（在创建 GenTable 之后，但连接应该已经重新打开）
            var batchResult = await _genColumnService.CreateBatchAsync(createColumnDtos);
            int createCount = 0;
            int failCount = 0;
            var failMessages = new List<string>();

            if (batchResult.Success)
            {
                createCount = batchResult.Data.success;
                failCount = batchResult.Data.fail;
                failMessages = batchResult.Data.failMessages;
            }
            else
            {
                failCount = createColumnDtos.Count;
                failMessages.Add(batchResult.Message ?? "批量创建失败");
            }

            _appLog.Information("导入表 {TableName} 完成: 成功创建 {CreateCount} 个，失败 {FailCount} 个，总列数 {TotalCount}",
                tableName, createCount, failCount, columns.Count);

            if (failCount > 0)
            {
                var message = $"成功创建 {createCount} 个列，失败 {failCount} 个列。失败详情:\n" + string.Join("\n", failMessages);
                return Result.Fail(message);
            }

            return Result.Ok($"成功导入表 {tableName}，共创建 {createCount} 个列");
        }
        catch (Exception ex)
        {
            return Result.Fail($"导入失败: {ex.Message}");
        }
    }
}


