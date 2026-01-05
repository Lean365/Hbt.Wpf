// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：ILoginLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：登录日志服务接口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logging;
using Takt.Common.Results;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 登录日志服务接口
/// </summary>
public interface ILoginLogService
{
    /// <summary>
    /// 查询登录日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、用户名、登录IP等筛选条件</param>
    /// <returns>分页登录日志列表</returns>
    Task<Result<PagedResult<LoginLogDto>>> GetListAsync(LoginLogQueryDto query);

    /// <summary>
    /// 导出登录日志到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的日志</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(LoginLogQueryDto? query = null!, string? sheetName = null!, string? fileName = null);
}

