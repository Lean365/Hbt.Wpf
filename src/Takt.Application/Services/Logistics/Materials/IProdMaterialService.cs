// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Materials
// 文件名称：IProdMaterialService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：生产物料服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Logistics.Materials;
using Takt.Common.Models;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Materials;

/// <summary>
/// 生产物料服务接口
/// </summary>
public interface IProdMaterialService
{
    /// <summary>
    /// 查询生产物料列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码等筛选条件</param>
    /// <returns>分页生产物料列表</returns>
    Task<Result<PagedResult<ProdMaterialDto>>> GetListAsync(ProdMaterialQueryDto query);

    /// <summary>
    /// 根据ID获取生产物料
    /// </summary>
    /// <param name="id">生产物料ID</param>
    /// <returns>生产物料信息</returns>
    Task<Result<ProdMaterialDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建生产物料
    /// </summary>
    /// <param name="dto">创建生产物料DTO</param>
    /// <returns>新生产物料ID</returns>
    Task<Result<long>> CreateAsync(ProdMaterialCreateDto dto);

    /// <summary>
    /// 更新生产物料
    /// </summary>
    /// <param name="dto">更新生产物料DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> UpdateAsync(ProdMaterialUpdateDto dto);

    /// <summary>
    /// 删除生产物料
    /// </summary>
    /// <param name="id">生产物料ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除生产物料
    /// </summary>
    /// <param name="ids">生产物料ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 导出生产物料到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的生产物料</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdMaterialQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出生产物料 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入生产物料
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);

    /// <summary>
    /// 获取物料选项列表
    /// 从生产物料表中获取所有未删除的物料，用于下拉列表等UI组件
    /// </summary>
    /// <returns>物料选项列表</returns>
    Task<Result<List<SelectOptionModel>>> GetOptionAsync();
}

