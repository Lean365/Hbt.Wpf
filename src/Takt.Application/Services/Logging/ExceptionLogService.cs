// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：ExceptionLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：异常日志服务实现
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
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
/// 异常日志服务实现
/// </summary>
public class ExceptionLogService : IExceptionLogService
{
    private readonly IBaseRepository<ExceptionLog> _exceptionLogRepository;
    private readonly AppLogManager _appLog;

    public ExceptionLogService(IBaseRepository<ExceptionLog> exceptionLogRepository, AppLogManager appLog)
    {
        _exceptionLogRepository = exceptionLogRepository ?? throw new ArgumentNullException(nameof(exceptionLogRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 查询异常日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、异常类型、用户名等筛选条件</param>
    /// <returns>包含分页异常日志列表的结果对象，成功时返回日志列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在异常类型、异常消息、用户名、请求路径中搜索）
    /// 支持按异常类型、异常时间排序，默认按异常时间倒序
    /// </remarks>
    public async Task<Result<PagedResult<ExceptionLogDto>>> GetListAsync(ExceptionLogQueryDto query)
    {
        _appLog.Information("开始查询异常日志列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式（日志通常按时间倒序）
            System.Linq.Expressions.Expression<Func<ExceptionLog, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "exceptiontype":
                        orderByExpression = log => log.ExceptionType;
                        break;
                    case "exceptiontime":
                        orderByExpression = log => log.ExceptionTime;
                        break;
                    default:
                        orderByExpression = log => log.ExceptionTime;
                        break;
                }
            }
            else
            {
                orderByExpression = log => log.ExceptionTime; // 默认按时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _exceptionLogRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var exceptionLogDtos = result.Items.Adapt<List<ExceptionLogDto>>();

            var pagedResult = new PagedResult<ExceptionLogDto>
            {
                Items = exceptionLogDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ExceptionLogDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询异常日志数据失败");
            return Result<PagedResult<ExceptionLogDto>>.Fail($"查询异常日志数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<ExceptionLog, bool>> QueryExpression(ExceptionLogQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ExceptionLog>()
            .And(log => log.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), log => log.ExceptionType.Contains(query.Keywords!) ||
                                                                   log.ExceptionMessage.Contains(query.Keywords!) ||
                                                                   (log.Username != null && log.Username.Contains(query.Keywords!)) ||
                                                                   (log.RequestPath != null && log.RequestPath.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.ExceptionType), log => log.ExceptionType.Contains(query.ExceptionType!))
            .AndIF(!string.IsNullOrEmpty(query.Username), log => log.Username != null && log.Username.Contains(query.Username!))
            .AndIF(!string.IsNullOrEmpty(query.RequestPath), log => log.RequestPath != null && log.RequestPath.Contains(query.RequestPath!))
            .AndIF(query.ExceptionTimeFrom.HasValue, log => log.ExceptionTime >= query.ExceptionTimeFrom!.Value)
            .AndIF(query.ExceptionTimeTo.HasValue, log => log.ExceptionTime <= query.ExceptionTimeTo!.Value)
            .ToExpression();
    }
}

