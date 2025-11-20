// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Visitors
// 文件名称：IVisitorDetailService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客详情服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Logistics.Visitors;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Visitors;

/// <summary>
/// 访客详情服务接口
/// </summary>
public interface IVisitorDetailService
{
    /// <summary>
    /// 查询访客详情列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、访客ID等筛选条件</param>
    /// <returns>分页访客详情列表</returns>
    Task<Result<PagedResult<VisitorDetailDto>>> GetListAsync(VisitorDetailQueryDto query);

    /// <summary>
    /// 根据ID获取访客详情
    /// </summary>
    Task<Result<VisitorDetailDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建访客详情
    /// </summary>
    Task<Result<long>> CreateAsync(VisitorDetailCreateDto dto);

    /// <summary>
    /// 更新访客详情
    /// </summary>
    Task<Result> UpdateAsync(VisitorDetailUpdateDto dto);

    /// <summary>
    /// 删除访客详情
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除访客详情
    /// </summary>
    Task<Result> DeleteRangeAsync(List<long> ids);

    /// <summary>
    /// 导出访客详情到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的访客详情</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(VisitorDetailQueryDto? query = null, string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 导出访客详情 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入访客详情
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}

