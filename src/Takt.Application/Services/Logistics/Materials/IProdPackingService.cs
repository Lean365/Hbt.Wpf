// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Materials
// 文件名称：IProdPackingService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：包装信息服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Logistics.Materials;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Materials;

/// <summary>
/// 包装信息服务接口
/// </summary>
public interface IProdPackingService
{
    /// <summary>
    /// 查询包装信息列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料编码等筛选条件</param>
    /// <returns>分页包装信息列表</returns>
    Task<Result<Takt.Common.Results.PagedResult<ProdPackingDto>>> GetListAsync(ProdPackingQueryDto query);

    /// <summary>
    /// 根据ID获取包装信息
    /// </summary>
    /// <param name="id">包装信息ID</param>
    /// <returns>包装信息</returns>
    Task<Result<ProdPackingDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建包装信息
    /// </summary>
    /// <param name="dto">创建包装信息DTO</param>
    /// <returns>新包装信息ID</returns>
    Task<Result<long>> CreateAsync(ProdPackingCreateDto dto);

    /// <summary>
    /// 更新包装信息
    /// </summary>
    /// <param name="dto">更新包装信息DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> UpdateAsync(ProdPackingUpdateDto dto);

    /// <summary>
    /// 删除包装信息
    /// </summary>
    /// <param name="id">包装信息ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除包装信息
    /// </summary>
    /// <param name="ids">包装信息ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 导出包装信息到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的包装信息</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdPackingQueryDto? query = null!, string? sheetName = null!, string? fileName = null);
}

