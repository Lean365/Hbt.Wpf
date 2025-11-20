// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：IOperationLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：操作日志服务接口
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logging;
using Takt.Common.Results;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 操作日志服务接口
/// </summary>
public interface IOperationLogService
{
    /// <summary>
    /// 查询操作日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、用户名、操作类型等筛选条件</param>
    /// <returns>分页操作日志列表</returns>
    Task<Result<PagedResult<OperationLogDto>>> GetListAsync(OperationLogQueryDto query);
}

