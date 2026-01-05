// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：IQuartzJobLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务日志服务接口
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logging;
using Takt.Common.Results;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 任务日志服务接口
/// </summary>
public interface IQuartzJobLogService
{
    /// <summary>
    /// 查询任务日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字等筛选条件</param>
    /// <returns>分页任务日志列表</returns>
    Task<Result<PagedResult<QuartzJobLogDto>>> GetListAsync(QuartzJobLogQueryDto query);

    /// <summary>
    /// 根据ID获取任务日志
    /// </summary>
    /// <param name="id">日志ID</param>
    /// <returns>任务日志信息</returns>
    Task<Result<QuartzJobLogDto>> GetByIdAsync(long id);

    /// <summary>
    /// 导出任务日志到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的日志</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(QuartzJobLogQueryDto? query = null!, string? sheetName = null!, string? fileName = null);
}

