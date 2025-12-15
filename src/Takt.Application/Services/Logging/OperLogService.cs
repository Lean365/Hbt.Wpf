// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：OperLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：操作日志服务实现
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Takt.Application.Dtos.Logging;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Repositories;
using Mapster;
using SqlSugar;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 操作日志服务实现
/// </summary>
public class OperLogService : IOperLogService
{
    private readonly IBaseRepository<OperLog> _operationLogRepository;
    private readonly AppLogManager _appLog;

    public OperLogService(IBaseRepository<OperLog> operationLogRepository, AppLogManager appLog)
    {
        _operationLogRepository = operationLogRepository ?? throw new ArgumentNullException(nameof(operationLogRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 查询操作日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、用户名、操作类型等筛选条件</param>
    /// <returns>包含分页操作日志列表的结果对象，成功时返回日志列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在用户名、操作类型、操作模块、操作描述中搜索）
    /// 支持按用户名、操作类型、操作时间排序，默认按操作时间倒序
    /// </remarks>
    public async Task<Result<PagedResult<OperLogDto>>> GetListAsync(OperLogQueryDto query)
    {
        _appLog.Information("开始查询操作日志列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式（日志通常按时间倒序）
            System.Linq.Expressions.Expression<Func<OperLog, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "username":
                        orderByExpression = log => log.Username;
                        break;
                    case "operationtype":
                        orderByExpression = log => log.OperationType;
                        break;
                    case "operationtime":
                        orderByExpression = log => log.OperationTime;
                        break;
                    default:
                        orderByExpression = log => log.OperationTime;
                        break;
                }
            }
            else
            {
                orderByExpression = log => log.OperationTime; // 默认按时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _operationLogRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var operationLogDtos = result.Items.Adapt<List<OperLogDto>>();

            var pagedResult = new PagedResult<OperLogDto>
            {
                Items = operationLogDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<OperLogDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询操作日志数据失败");
            return Result<PagedResult<OperLogDto>>.Fail($"查询操作日志数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<OperLog, bool>> QueryExpression(OperLogQueryDto query)
    {
        return SqlSugar.Expressionable.Create<OperLog>()
            .And(log => log.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), log => log.Username.Contains(query.Keywords!) ||
                                                                   log.OperationType.Contains(query.Keywords!) ||
                                                                   log.OperationModule.Contains(query.Keywords!) ||
                                                                   (log.OperationDesc != null && log.OperationDesc.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.Username), log => log.Username.Contains(query.Username!))
            .AndIF(!string.IsNullOrEmpty(query.OperationType), log => log.OperationType.Contains(query.OperationType!))
            .AndIF(!string.IsNullOrEmpty(query.OperationModule), log => log.OperationModule.Contains(query.OperationModule!))
            .AndIF(query.OperationTimeFrom != default(DateTime), log => log.OperationTime >= query.OperationTimeFrom)
            .AndIF(query.OperationTimeTo != default(DateTime), log => log.OperationTime <= query.OperationTimeTo)
            .ToExpression();
    }

    /// <summary>
    /// 导出操作日志到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的日志</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(OperLogQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<OperLog>().And(x => x.IsDeleted == 0).ToExpression();
            var logs = await _operationLogRepository.AsQueryable().Where(where).OrderBy(log => log.OperationTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = logs.Adapt<List<OperLogDto>>();
            sheetName ??= "OperLogs";
            fileName ??= $"操作日志导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = Takt.Common.Helpers.ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出操作日志Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }
}

