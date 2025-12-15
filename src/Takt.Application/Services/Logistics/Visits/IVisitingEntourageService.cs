// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Visits
// 文件名称：IVisitingEntourageService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员详情服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logistics.Visits;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Visits;

/// <summary>
/// 来访成员详情服务接口
/// </summary>
public interface IVisitingEntourageService
{
    /// <summary>
    /// 查询随行人员详情列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、随行人员ID等筛选条件</param>
    /// <returns>分页随行人员详情列表</returns>
    Task<Result<PagedResult<VisitingEntourageDto>>> GetListAsync(VisitingEntourageQueryDto query);

    /// <summary>
    /// 根据ID获取随行人员详情
    /// </summary>
    Task<Result<VisitingEntourageDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建随行人员详情
    /// </summary>
    Task<Result<long>> CreateAsync(VisitingEntourageCreateDto dto);

    /// <summary>
    /// 更新随行人员详情
    /// </summary>
    Task<Result> UpdateAsync(VisitingEntourageUpdateDto dto);

    /// <summary>
    /// 删除随行人员详情
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除随行人员详情
    /// </summary>
    Task<Result> DeleteBatchAsync(List<long> ids);

    /// <summary>
    /// 导出随行人员详情到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的随行人员详情</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(VisitingEntourageQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出随行人员详情 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入随行人员详情
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}

