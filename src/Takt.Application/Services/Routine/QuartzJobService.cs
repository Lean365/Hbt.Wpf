// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Routine
// 文件名称：QuartzJobService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：任务服务实现
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Mapster;
using Newtonsoft.Json;
using Takt.Application.Dtos.Routine;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 任务服务实现
/// </summary>
public class QuartzJobService : IQuartzJobService
{
    private readonly IBaseRepository<QuartzJob> _quartzRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public QuartzJobService(
        IBaseRepository<QuartzJob> quartzRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _quartzRepository = quartzRepository ?? throw new ArgumentNullException(nameof(quartzRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
        _operLog = operLog;
    }

    /// <summary>
    /// 查询任务列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字等筛选条件</param>
    /// <returns>包含分页任务列表的结果对象，成功时返回任务列表和总数，失败时返回错误信息</returns>
    public async Task<Result<PagedResult<QuartzJobDto>>> GetListAsync(QuartzJobQueryDto query)
    {
        _appLog.Information("开始查询任务列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            Expression<Func<QuartzJob, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "jobname":
                        orderByExpression = q => q.JobName;
                        break;
                    case "jobgroup":
                        orderByExpression = q => q.JobGroup;
                        break;
                    case "status":
                        orderByExpression = q => q.Status;
                        break;
                    case "createdtime":
                        orderByExpression = q => q.CreatedTime;
                        break;
                    default:
                        orderByExpression = q => q.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = q => q.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _quartzRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var quartzDtos = result.Items.Adapt<List<QuartzJobDto>>();

            var pagedResult = new PagedResult<QuartzJobDto>
            {
                Items = quartzDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<QuartzJobDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询任务数据失败");
            return Result<PagedResult<QuartzJobDto>>.Fail($"高级查询任务数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取任务
    /// </summary>
    /// <param name="id">任务ID，必须大于0</param>
    /// <returns>包含任务信息的结果对象，成功时返回任务DTO，失败时返回错误信息</returns>
    public async Task<Result<QuartzJobDto>> GetByIdAsync(long id)
    {
        try
        {
            var quartz = await _quartzRepository.GetByIdAsync(id);
            if (quartz == null || quartz.IsDeleted == 1)
                return Result<QuartzJobDto>.Fail("任务不存在");

            var quartzDto = quartz.Adapt<QuartzJobDto>();
            return Result<QuartzJobDto>.Ok(quartzDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取任务失败");
            return Result<QuartzJobDto>.Fail($"获取任务失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建任务
    /// </summary>
    /// <param name="dto">创建数据传输对象，包含任务名称、Cron表达式等信息</param>
    /// <returns>包含新任务ID的结果对象，成功时返回任务ID，失败时返回错误信息</returns>
    public async Task<Result<long>> CreateAsync(QuartzJobCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查任务名称和任务组是否已存在
            var exists = await _quartzRepository.GetFirstAsync(
                q => q.JobName == dto.JobName && q.JobGroup == dto.JobGroup && q.IsDeleted == 0);
            if (exists != null)
            {
                return Result<long>.Fail($"任务名称 {dto.JobName} 在任务组 {dto.JobGroup} 中已存在");
            }

            // 检查触发器名称和触发器组是否已存在
            var triggerExists = await _quartzRepository.GetFirstAsync(
                q => q.TriggerName == dto.TriggerName && q.TriggerGroup == dto.TriggerGroup && q.IsDeleted == 0);
            if (triggerExists != null)
            {
                return Result<long>.Fail($"触发器名称 {dto.TriggerName} 在触发器组 {dto.TriggerGroup} 中已存在");
            }

            var quartz = dto.Adapt<QuartzJob>();
            var result = await _quartzRepository.CreateAsync(quartz);
            Result<long> response = result > 0 
                ? Result<long>.Ok(quartz.Id) 
                : Result<long>.Fail("创建任务失败");
            
            _operLog?.LogCreate("QuartzJob", quartz.Id.ToString(), "Routine.QuartzJobView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建任务失败，任务名称={JobName}", dto.JobName);
            return Result<long>.Fail($"创建任务失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新任务
    /// </summary>
    /// <param name="dto">更新数据传输对象，包含ID和要更新的字段</param>
    /// <returns>包含操作结果的结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    public async Task<Result> UpdateAsync(QuartzJobUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var quartz = await _quartzRepository.GetByIdAsync(dto.Id);
            if (quartz == null || quartz.IsDeleted == 1)
                return Result.Fail("任务不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldQuartz = quartz.Adapt<QuartzJobUpdateDto>();

            // 检查任务名称和任务组是否被其他记录使用
            if (quartz.JobName != dto.JobName || quartz.JobGroup != dto.JobGroup)
            {
                var exists = await _quartzRepository.GetFirstAsync(
                    q => q.JobName == dto.JobName && q.JobGroup == dto.JobGroup && q.Id != dto.Id && q.IsDeleted == 0);
                if (exists != null)
                {
                    return Result.Fail($"任务名称 {dto.JobName} 在任务组 {dto.JobGroup} 中已被其他任务使用");
                }
            }

            // 检查触发器名称和触发器组是否被其他记录使用
            if (quartz.TriggerName != dto.TriggerName || quartz.TriggerGroup != dto.TriggerGroup)
            {
                var triggerExists = await _quartzRepository.GetFirstAsync(
                    q => q.TriggerName == dto.TriggerName && q.TriggerGroup == dto.TriggerGroup && q.Id != dto.Id && q.IsDeleted == 0);
                if (triggerExists != null)
                {
                    return Result.Fail($"触发器名称 {dto.TriggerName} 在触发器组 {dto.TriggerGroup} 中已被其他任务使用");
                }
            }

            dto.Adapt(quartz);
            var result = await _quartzRepository.UpdateAsync(quartz);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldQuartz.JobName != dto.JobName) changeList.Add($"JobName: {oldQuartz.JobName} -> {dto.JobName}");
            if (oldQuartz.JobGroup != dto.JobGroup) changeList.Add($"JobGroup: {oldQuartz.JobGroup} -> {dto.JobGroup}");
            if (oldQuartz.TriggerName != dto.TriggerName) changeList.Add($"TriggerName: {oldQuartz.TriggerName} -> {dto.TriggerName}");
            if (oldQuartz.TriggerGroup != dto.TriggerGroup) changeList.Add($"TriggerGroup: {oldQuartz.TriggerGroup} -> {dto.TriggerGroup}");
            if (oldQuartz.CronExpression != dto.CronExpression) changeList.Add($"CronExpression: {oldQuartz.CronExpression} -> {dto.CronExpression}");
            if (oldQuartz.Status != dto.Status) changeList.Add($"Status: {oldQuartz.Status} -> {dto.Status}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新任务失败");
            
            _operLog?.LogUpdate("QuartzJob", dto.Id.ToString(), "Routine.QuartzJobView", changes, dto, oldQuartz, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新任务失败，ID={Id}", dto.Id);
            return Result.Fail($"更新任务失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除任务（逻辑删除）
    /// </summary>
    /// <param name="id">任务ID，必须大于0</param>
    /// <returns>包含操作结果的结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var quartz = await _quartzRepository.GetByIdAsync(id);
            if (quartz == null || quartz.IsDeleted == 1)
                return Result.Fail("任务不存在");

            var result = await _quartzRepository.DeleteAsync(quartz);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除任务失败");
            
            _operLog?.LogDelete("QuartzJob", id.ToString(), "Routine.QuartzJobView", 
                new { Id = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除任务失败，ID={Id}", id);
            return Result.Fail($"删除任务失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除任务（逻辑删除）
    /// </summary>
    /// <param name="ids">任务ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            int successCount = 0;
            int failCount = 0;

            foreach (var id in ids)
            {
                var quartz = await _quartzRepository.GetByIdAsync(id);
                if (quartz == null || quartz.IsDeleted == 1)
                {
                    failCount++;
                    continue;
                }

                var result = await _quartzRepository.DeleteAsync(quartz);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("QuartzJob", id.ToString(), "Routine.QuartzJobView", 
                        new { Id = id }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除任务完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除任务失败");
            return Result.Fail($"批量删除任务失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 修改任务状态（0=启用，1=禁用）
    /// </summary>
    /// <param name="dto">状态DTO</param>
    /// <returns>操作结果</returns>
    public async Task<Result> StatusAsync(QuartzJobStatusDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = await _quartzRepository.StatusAsync(dto.Id, dto.Status);
            var response = result > 0 ? Result.Ok() : Result.Fail("修改任务状态失败");

            _operLog?.LogUpdate("QuartzJob", dto.Id.ToString(), "Routine.QuartzJobView", $"修改状态为 {dto.Status}",
                new { Id = dto.Id, Status = dto.Status }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("修改任务状态成功，ID: {Id}, 状态: {Status}", dto.Id, dto.Status);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "修改任务状态失败，ID: {Id}, 状态: {Status}", dto.Id, dto.Status);
            return Result.Fail($"修改任务状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出任务到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的任务</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(QuartzJobQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<QuartzJob>().And(x => x.IsDeleted == 0).ToExpression();
            var quartzes = await _quartzRepository.AsQueryable().Where(where).OrderBy(q => q.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = quartzes.Adapt<List<QuartzJobExportDto>>();
            sheetName ??= "QuartzJobs";
            fileName ??= $"任务导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出任务Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<QuartzJob, bool>> QueryExpression(QuartzJobQueryDto query)
    {
        return SqlSugar.Expressionable.Create<QuartzJob>()
            .And(q => q.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), q => q.JobName.Contains(query.Keywords!) ||
                                                               (q.JobDescription != null && q.JobDescription.Contains(query.Keywords!)) ||
                                                               q.JobClassName.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.JobName), q => q.JobName.Contains(query.JobName!))
            .AndIF(!string.IsNullOrEmpty(query.JobGroup), q => q.JobGroup == query.JobGroup!)
            .AndIF(query.Status.HasValue, q => q.Status == query.Status!.Value)
            .ToExpression();
    }
}

