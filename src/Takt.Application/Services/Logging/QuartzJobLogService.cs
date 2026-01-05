// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：QuartzJobLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务日志服务实现
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Takt.Application.Dtos.Logging;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Repositories;
using Mapster;
using SqlSugar;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 任务日志服务实现
/// </summary>
public class QuartzJobLogService : IQuartzJobLogService
{
    private readonly IBaseRepository<QuartzJobLog> _quartzLogRepository;
    private readonly AppLogManager _appLog;

    public QuartzJobLogService(
        IBaseRepository<QuartzJobLog> quartzLogRepository,
        AppLogManager appLog)
    {
        _quartzLogRepository = quartzLogRepository ?? throw new ArgumentNullException(nameof(quartzLogRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 查询任务日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字等筛选条件</param>
    /// <returns>包含分页任务日志列表的结果对象，成功时返回日志列表和总数，失败时返回错误信息</returns>
    public async Task<Result<PagedResult<QuartzJobLogDto>>> GetListAsync(QuartzJobLogQueryDto query)
    {
        _appLog.Information("开始查询任务日志列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式（日志通常按时间倒序）
            Expression<Func<QuartzJobLog, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "jobname":
                        orderByExpression = log => log.JobName;
                        break;
                    case "starttime":
                        orderByExpression = log => log.StartTime;
                        break;
                    case "executeresult":
                        orderByExpression = log => log.ExecuteResult;
                        break;
                    default:
                        orderByExpression = log => log.StartTime;
                        break;
                }
            }
            else
            {
                orderByExpression = log => log.StartTime; // 默认按开始时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _quartzLogRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var quartzLogDtos = result.Items.Adapt<List<QuartzJobLogDto>>();

            var pagedResult = new PagedResult<QuartzJobLogDto>
            {
                Items = quartzLogDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<QuartzJobLogDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询任务日志数据失败");
            return Result<PagedResult<QuartzJobLogDto>>.Fail($"查询任务日志数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取任务日志
    /// </summary>
    /// <param name="id">日志ID</param>
    /// <returns>包含任务日志信息的结果对象，成功时返回日志DTO，失败时返回错误信息</returns>
    public async Task<Result<QuartzJobLogDto>> GetByIdAsync(long id)
    {
        try
        {
            var quartzLog = await _quartzLogRepository.GetByIdAsync(id);
            if (quartzLog == null || quartzLog.IsDeleted == 1)
                return Result<QuartzJobLogDto>.Fail("任务日志不存在");

            var quartzLogDto = quartzLog.Adapt<QuartzJobLogDto>();
            return Result<QuartzJobLogDto>.Ok(quartzLogDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取任务日志失败");
            return Result<QuartzJobLogDto>.Fail($"获取任务日志失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出任务日志到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的日志</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(QuartzJobLogQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<QuartzJobLog>().And(x => x.IsDeleted == 0).ToExpression();
            var logs = await _quartzLogRepository.AsQueryable().Where(where).OrderBy(log => log.StartTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = logs.Adapt<List<QuartzJobLogExportDto>>();
            sheetName ??= "QuartzJobLogs";
            fileName ??= $"任务日志导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出任务日志Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<QuartzJobLog, bool>> QueryExpression(QuartzJobLogQueryDto query)
    {
        return SqlSugar.Expressionable.Create<QuartzJobLog>()
            .And(log => log.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), log => log.JobName.Contains(query.Keywords!) ||
                                                                  (log.ErrorMessage != null && log.ErrorMessage.Contains(query.Keywords!)))
            .AndIF(query.QuartzId.HasValue, log => log.QuartzId == query.QuartzId!.Value)
            .AndIF(!string.IsNullOrEmpty(query.JobName), log => log.JobName.Contains(query.JobName!))
            .AndIF(!string.IsNullOrEmpty(query.JobGroup), log => log.JobGroup == query.JobGroup!)
            .AndIF(!string.IsNullOrEmpty(query.ExecuteResult), log => log.ExecuteResult == query.ExecuteResult!)
            .AndIF(query.StartTimeFrom != default(DateTime), log => log.StartTime >= query.StartTimeFrom)
            .AndIF(query.StartTimeTo != default(DateTime), log => log.StartTime <= query.StartTimeTo)
            .ToExpression();
    }
}

