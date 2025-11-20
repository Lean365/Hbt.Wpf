// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：DiffLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：差异日志服务实现
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
/// 差异日志服务实现
/// </summary>
public class DiffLogService : IDiffLogService
{
    private readonly IBaseRepository<DiffLog> _diffLogRepository;
    private readonly AppLogManager _appLog;

    public DiffLogService(IBaseRepository<DiffLog> diffLogRepository, AppLogManager appLog)
    {
        _diffLogRepository = diffLogRepository ?? throw new ArgumentNullException(nameof(diffLogRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 查询差异日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、表名、差异类型等筛选条件</param>
    /// <returns>包含分页差异日志列表的结果对象，成功时返回日志列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在表名、差异类型、业务数据、用户名中搜索）
    /// 支持按表名、差异时间排序，默认按差异时间倒序
    /// </remarks>
    public async Task<Result<PagedResult<DiffLogDto>>> GetListAsync(DiffLogQueryDto query)
    {
        _appLog.Information("开始查询差异日志列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式（日志通常按时间倒序）
            System.Linq.Expressions.Expression<Func<DiffLog, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "tablename":
                        orderByExpression = log => log.TableName;
                        break;
                    case "difftime":
                        orderByExpression = log => log.DiffTime;
                        break;
                    default:
                        orderByExpression = log => log.DiffTime;
                        break;
                }
            }
            else
            {
                orderByExpression = log => log.DiffTime; // 默认按时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _diffLogRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var diffLogDtos = result.Items.Adapt<List<DiffLogDto>>();

            var pagedResult = new PagedResult<DiffLogDto>
            {
                Items = diffLogDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<DiffLogDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询差异日志数据失败");
            return Result<PagedResult<DiffLogDto>>.Fail($"查询差异日志数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<DiffLog, bool>> QueryExpression(DiffLogQueryDto query)
    {
        return SqlSugar.Expressionable.Create<DiffLog>()
            .And(log => log.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), log => log.TableName.Contains(query.Keywords!) ||
                                                                 log.DiffType.Contains(query.Keywords!) ||
                                                                 (log.BusinessData != null && log.BusinessData.Contains(query.Keywords!)) ||
                                                                 (log.Username != null && log.Username.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.TableName), log => log.TableName.Contains(query.TableName!))
            .AndIF(!string.IsNullOrEmpty(query.DiffType), log => log.DiffType.Contains(query.DiffType!))
            .AndIF(!string.IsNullOrEmpty(query.Username), log => log.Username != null && log.Username.Contains(query.Username!))
            .AndIF(query.DiffTimeFrom.HasValue, log => log.DiffTime >= query.DiffTimeFrom!.Value)
            .AndIF(query.DiffTimeTo.HasValue, log => log.DiffTime <= query.DiffTimeTo!.Value)
            .ToExpression();
    }
}

