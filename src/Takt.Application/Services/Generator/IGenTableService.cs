// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Generator
// 文件名称：IGenTableService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表配置服务接口
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Generator;
using Takt.Common.Results;
using System.IO;

namespace Takt.Application.Services.Generator;

/// <summary>
/// 代码生成表配置服务接口
/// </summary>
public interface IGenTableService
{
    /// <summary>
    /// 查询代码生成表配置列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象</param>
    /// <returns>分页列表</returns>
    Task<Result<PagedResult<GenTableDto>>> GetListAsync(GenTableQueryDto query);

    /// <summary>
    /// 根据ID获取代码生成表配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns>代码生成表配置信息</returns>
    Task<Result<GenTableDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建代码生成表配置
    /// </summary>
    /// <param name="dto">创建DTO</param>
    /// <returns>新记录ID</returns>
    Task<Result<long>> CreateAsync(GenTableCreateDto dto);

    /// <summary>
    /// 更新代码生成表配置
    /// </summary>
    /// <param name="dto">更新DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> UpdateAsync(GenTableUpdateDto dto);

    /// <summary>
    /// 删除代码生成表配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除代码生成表配置
    /// </summary>
    /// <param name="ids">代码生成表配置ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 导出代码生成表配置到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(GenTableQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出代码生成表配置 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入代码生成表配置
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}

