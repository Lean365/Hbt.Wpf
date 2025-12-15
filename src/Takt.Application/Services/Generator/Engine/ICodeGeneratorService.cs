// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Generator.Engine
// 文件名称：ICodeGeneratorService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成服务接口
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Generator;
using Takt.Common.Results;

namespace Takt.Application.Services.Generator.Engine;

/// <summary>
/// 代码生成服务接口
/// 
/// 使用场景：
/// 1. 有数据表：先使用 ImportFromTableAsync 从数据库表导入到 GenTable 和 GenColumn，然后在 UI 中修改配置，最后使用 GenerateFromConfigAsync 生成代码
/// 2. 无数据表：先在 UI 中创建并保存 GenTable 和 GenColumn 配置，然后使用 GenerateFromConfigAsync 生成代码
/// 
/// 注意：所有代码生成都必须从 GenTable 和 GenColumn 配置获取，不能直接从数据库表生成
/// </summary>
public interface ICodeGeneratorService
{
    /// <summary>
    /// 从配置生成代码（从 GenTable 和 GenColumn 获取配置）
    /// </summary>
    /// <param name="tableConfig">表配置（GenTable）</param>
    /// <param name="columnConfigs">列配置列表（GenColumn）</param>
    /// <param name="options">生成选项</param>
    /// <returns>生成的代码文件字典（文件名 -> 文件内容）</returns>
    Task<Dictionary<string, string>> GenerateFromConfigAsync(GenTableDto tableConfig, List<GenColumnDto> columnConfigs, CodeGenerationOptions options);

    /// <summary>
    /// 从数据库表导入并保存到 GenTable 和 GenColumn
    /// 用于场景1：有数据表时，先导入配置，然后可以在 UI 中修改配置，最后生成代码
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="author">作者（可选）</param>
    /// <returns>操作结果</returns>
    Task<Result> ImportFromTableAsync(string tableName, string? author = null);
}

/// <summary>
/// 代码生成选项
/// </summary>
public class CodeGenerationOptions
{
    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; } = "Takt365(Cursor AI)";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否生成实体
    /// </summary>
    public bool GenerateEntity { get; set; } = true;

    /// <summary>
    /// 是否生成DTO
    /// </summary>
    public bool GenerateDto { get; set; } = true;

    /// <summary>
    /// 是否生成服务接口
    /// </summary>
    public bool GenerateIService { get; set; } = true;

    /// <summary>
    /// 是否生成服务实现
    /// </summary>
    public bool GenerateService { get; set; } = true;

    /// <summary>
    /// 是否生成ViewModel
    /// </summary>
    public bool GenerateViewModel { get; set; } = true;

    /// <summary>
    /// 是否生成FormViewModel
    /// </summary>
    public bool GenerateFormViewModel { get; set; } = true;

    /// <summary>
    /// 是否生成View
    /// </summary>
    public bool GenerateView { get; set; } = true;

    /// <summary>
    /// 是否生成FormView
    /// </summary>
    public bool GenerateFormView { get; set; } = true;

    /// <summary>
    /// 是否生成菜单SQL
    /// </summary>
    public bool GenerateMenuSql { get; set; } = true;

    /// <summary>
    /// 是否生成翻译SQL
    /// </summary>
    public bool GenerateTranslationSql { get; set; } = true;

    /// <summary>
    /// 输出目录
    /// </summary>
    public string? OutputDirectory { get; set; }
}

