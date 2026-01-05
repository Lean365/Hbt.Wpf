// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：ITranslationService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：翻译服务接口
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Routine;
using Takt.Common.Results;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 翻译服务接口
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// 查询翻译列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、翻译键、语言代码等筛选条件</param>
    /// <returns>分页翻译列表</returns>
    Task<Result<PagedResult<TranslationDto>>> GetListAsync(TranslationQueryDto query);

    /// <summary>
    /// 根据ID获取翻译
    /// </summary>
    Task<Result<TranslationDto>> GetByIdAsync(long id);

    /// <summary>
    /// 根据语言代码和翻译键获取翻译值
    /// </summary>
    Task<Result<string>> GetValueAsync(string languageCode, string translationKey);

    /// <summary>
    /// 创建翻译
    /// </summary>
    Task<Result<long>> CreateAsync(TranslationCreateDto dto);

    /// <summary>
    /// 更新翻译
    /// </summary>
    Task<Result> UpdateAsync(TranslationUpdateDto dto);

    /// <summary>
    /// 删除翻译
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除翻译
    /// </summary>
    /// <param name="ids">翻译ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);

    /// <summary>
    /// 获取模块的所有翻译（按翻译键转置，包含所有语言）
    /// 返回格式：{翻译键: {语言代码: 翻译值}}
    /// </summary>
    Task<Result<Dictionary<string, Dictionary<string, string>>>> GetTranslationsByModuleAsync(string? module = null);

    /// <summary>
    /// 获取多个翻译键的翻译值（转置后）
    /// </summary>
    Task<Result<Dictionary<string, Dictionary<string, string>>>> GetTranslationsByKeysAsync(List<string> translationKeys);

    /// <summary>
    /// 批量获取翻译键的所有语言翻译值
    /// </summary>
    Task<Result<Dictionary<string, string>>> GetTranslationValuesAsync(string translationKey);

    /// <summary>
    /// 分页获取唯一的翻译键列表
    /// </summary>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="keyword">关键词（可选，用于搜索翻译键）</param>
    /// <param name="module">模块过滤（可选）</param>
    /// <returns>返回分页结果，包含翻译键列表和总数</returns>
    Task<Result<PagedResult<string>>> GetTranslationKeysAsync(int pageIndex, int pageSize, string? keyword = null!, string? module = null);

    /// <summary>
    /// 导出翻译到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的翻译</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(TranslationQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出翻译 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入翻译
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}
