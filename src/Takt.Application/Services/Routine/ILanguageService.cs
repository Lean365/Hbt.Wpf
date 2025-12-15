// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：ILanguageService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：语言服务接口
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
/// 语言服务接口
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// 查询语言列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、语言代码等筛选条件</param>
    /// <returns>分页语言列表</returns>
    Task<Result<PagedResult<LanguageDto>>> GetListAsync(LanguageQueryDto query);

    /// <summary>
    /// 根据ID获取语言
    /// </summary>
    Task<Result<LanguageDto>> GetByIdAsync(long id);

    /// <summary>
    /// 根据语言代码获取语言
    /// </summary>
    Task<Result<LanguageDto>> GetByCodeAsync(string languageCode);

    /// <summary>
    /// 创建语言
    /// </summary>
    Task<Result<long>> CreateAsync(LanguageCreateDto dto);

    /// <summary>
    /// 更新语言
    /// </summary>
    Task<Result> UpdateAsync(LanguageUpdateDto dto);

    /// <summary>
    /// 删除语言
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除语言
    /// </summary>
    /// <param name="ids">语言ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 修改语言状态
    /// </summary>
    Task<Result> StatusAsync(long id, int status);

    /// <summary>
    /// 获取语言选项列表（用于下拉列表）
    /// </summary>
    /// <param name="includeDisabled">是否包含已禁用的语言</param>
    /// <returns>语言选项列表</returns>
    Task<Result<List<LanguageOptionDto>>> OptionAsync(bool includeDisabled = false);

    /// <summary>
    /// 导出语言到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的语言</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(LanguageQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出语言 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入语言
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}
