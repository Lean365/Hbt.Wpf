// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Routine
// 文件名称：IQuartzJobService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务服务接口
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Routine;
using Takt.Common.Results;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 任务服务接口
/// </summary>
public interface IQuartzJobService
{
    /// <summary>
    /// 查询任务列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字等筛选条件</param>
    /// <returns>分页任务列表</returns>
    Task<Result<PagedResult<QuartzJobDto>>> GetListAsync(QuartzJobQueryDto query);

    /// <summary>
    /// 根据ID获取任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <returns>任务信息</returns>
    Task<Result<QuartzJobDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建任务
    /// </summary>
    /// <param name="dto">创建数据传输对象</param>
    /// <returns>创建的任务ID</returns>
    Task<Result<long>> CreateAsync(QuartzJobCreateDto dto);

    /// <summary>
    /// 更新任务
    /// </summary>
    /// <param name="dto">更新数据传输对象</param>
    /// <returns>操作结果</returns>
    Task<Result> UpdateAsync(QuartzJobUpdateDto dto);

    /// <summary>
    /// 删除任务（逻辑删除）
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除任务（逻辑删除）
    /// </summary>
    /// <param name="ids">任务ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 修改任务状态（启用/禁用）
    /// </summary>
    /// <param name="dto">状态DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> StatusAsync(QuartzJobStatusDto dto);

    /// <summary>
    /// 导出任务到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的任务</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(QuartzJobQueryDto? query = null!, string? sheetName = null!, string? fileName = null);
}

