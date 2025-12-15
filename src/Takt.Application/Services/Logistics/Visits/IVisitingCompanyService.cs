// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Visits
// 文件名称：IVisitingCompanyService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Logistics.Visits;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Visits;

/// <summary>
/// 来访公司服务接口
/// </summary>
public interface IVisitingCompanyService
{
    /// <summary>
    /// 查询随行人员列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、公司名称等筛选条件</param>
    /// <returns>分页随行人员列表</returns>
    Task<Result<PagedResult<VisitingCompanyDto>>> GetListAsync(VisitingCompanyQueryDto query);

    /// <summary>
    /// 根据ID获取随行人员
    /// </summary>
    Task<Result<VisitingCompanyDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建随行人员
    /// </summary>
    Task<Result<long>> CreateAsync(VisitingCompanyCreateDto dto);

    /// <summary>
    /// 更新随行人员
    /// </summary>
    Task<Result> UpdateAsync(VisitingCompanyUpdateDto dto);

    /// <summary>
    /// 删除随行人员（同时删除关联的随行人员详情）
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除随行人员（同时删除关联的随行人员详情）
    /// </summary>
    /// <param name="ids">随行人员ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 导出随行人员到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的随行人员</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(VisitingCompanyQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出随行人员 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入随行人员
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}

