// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Visits
// 文件名称：VisitingCompanyService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Mapster;
using System.Linq.Expressions;
using Takt.Application.Dtos.Logistics.Visits;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Visits;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Logistics.Visits;

/// <summary>
/// 来访公司服务实现
/// </summary>
public class VisitingCompanyService : IVisitingCompanyService
{
    private readonly IBaseRepository<VisitingCompany> _visitingCompanyRepository;
    private readonly IBaseRepository<VisitingEntourage> _visitingEntourageRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public VisitingCompanyService(
        IBaseRepository<VisitingCompany> visitingCompanyRepository,
        IBaseRepository<VisitingEntourage> visitingEntourageRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _visitingCompanyRepository = visitingCompanyRepository;
        _visitingEntourageRepository = visitingEntourageRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询随行人员列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、公司名称等筛选条件</param>
    /// <returns>分页随行人员列表</returns>
    public async Task<Result<PagedResult<VisitingCompanyDto>>> GetListAsync(VisitingCompanyQueryDto query)
    {
        _appLog.Information("开始查询随行人员列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);

            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<VisitingCompany, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "companyname":
                        orderByExpression = v => v.VisitingCompanyName;
                        break;
                    case "starttime":
                        orderByExpression = v => v.VisitStartTime;
                        break;
                    case "createdtime":
                        orderByExpression = v => v.CreatedTime;
                        break;
                    default:
                        orderByExpression = v => v.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = v => v.CreatedTime; // 默认按创建时间倒序
            }

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }

            // 使用真实的数据库查询
            var result = await _visitingCompanyRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var entourageDtos = result.Items.Adapt<List<VisitingCompanyDto>>();

            var pagedResult = new PagedResult<VisitingCompanyDto>
            {
                Items = entourageDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<VisitingCompanyDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询随行人员数据失败");
            return Result<PagedResult<VisitingCompanyDto>>.Fail($"高级查询随行人员数据失败: {ex.Message}");
        }
    }

    public async Task<Result<VisitingCompanyDto>> GetByIdAsync(long id)
    {
        try
        {
            var entourage = await _visitingCompanyRepository.GetByIdAsync(id);
            if (entourage == null)
                return Result<VisitingCompanyDto>.Fail("随行人员不存在");

            var entourageDto = entourage.Adapt<VisitingCompanyDto>();
            return Result<VisitingCompanyDto>.Ok(entourageDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取随行人员失败");
            return Result<VisitingCompanyDto>.Fail($"获取随行人员失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(VisitingCompanyCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 验证时间段有效性
            if (dto.VisitStartTime >= dto.VisitEndTime)
            {
                return Result<long>.Fail($"开始时间 {dto.VisitStartTime:yyyy-MM-dd HH:mm:ss} 必须早于结束时间 {dto.VisitEndTime:yyyy-MM-dd HH:mm:ss}");
            }

            // 验证时间段间隔：结束时间必须比开始时间至少大1小时
            var timeDiff = dto.VisitEndTime - dto.VisitStartTime;
            if (timeDiff.TotalHours < 1.0)
            {
                return Result<long>.Fail($"结束时间必须比开始时间至少大1小时。当前间隔：{timeDiff.TotalMinutes:F0} 分钟");
            }

            // 检查时间段和公司是否重复（同一时段、同一公司不允许重复记录）
            // 时间段重叠的判断：两个时间段 [start1, end1] 和 [start2, end2] 重叠的条件是：start1 < end2 && start2 < end1
            // 同时需要检查公司名称是否相同
            var overlappingEntourage = await _visitingCompanyRepository.GetListAsync(
                v => v.IsDeleted == 0 &&
                     v.VisitingCompanyName == dto.VisitingCompanyName &&
                     v.VisitStartTime < dto.VisitEndTime &&
                     v.VisitEndTime > dto.VisitStartTime,
                1,
                int.MaxValue
            );

            if (overlappingEntourage.Items.Any())
            {
                var conflictEntourage = overlappingEntourage.Items.First();
                _appLog.Warning("创建随行人员失败：时间段和公司冲突。新记录：公司={Company}，时间段={StartTime} ~ {EndTime}，冲突记录ID：{Id}，时间段：{ConflictStartTime} ~ {ConflictEndTime}",
                    dto.VisitingCompanyName, dto.VisitStartTime, dto.VisitEndTime, conflictEntourage.Id,
                    conflictEntourage.VisitStartTime, conflictEntourage.VisitEndTime);

                return Result<long>.Fail($"时间段冲突：公司 {dto.VisitingCompanyName} 在时间段 {dto.VisitStartTime:yyyy-MM-dd HH:mm:ss} ~ {dto.VisitEndTime:yyyy-MM-dd HH:mm:ss} 与现有记录重叠（现有记录ID：{conflictEntourage.Id}，时间段：{conflictEntourage.VisitStartTime:yyyy-MM-dd HH:mm:ss} ~ {conflictEntourage.VisitEndTime:yyyy-MM-dd HH:mm:ss}）。请选择更新现有记录或调整时间段。");
            }

            var entourage = dto.Adapt<VisitingCompany>();
            var result = await _visitingCompanyRepository.CreateAsync(entourage);
            Result<long> response = result > 0
                ? Result<long>.Ok(entourage.Id)
                : Result<long>.Fail("创建随行人员失败");

            _operLog?.LogCreate("Entourage", entourage.Id.ToString(), "Logistics.Entourage.EntourageView",
                dto, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建随行人员失败");
            return Result<long>.Fail($"创建随行人员失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(VisitingCompanyUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var entourage = await _visitingCompanyRepository.GetByIdAsync(dto.Id);
            if (entourage == null)
                return Result.Fail("随行人员不存在");

            // 验证时间段有效性
            if (dto.VisitStartTime >= dto.VisitEndTime)
            {
                return Result.Fail($"开始时间 {dto.VisitStartTime:yyyy-MM-dd HH:mm:ss} 必须早于结束时间 {dto.VisitEndTime:yyyy-MM-dd HH:mm:ss}");
            }

            // 验证时间段间隔：结束时间必须比开始时间至少大1小时
            var timeDiff = dto.VisitEndTime - dto.VisitStartTime;
            if (timeDiff.TotalHours < 1.0)
            {
                return Result.Fail($"结束时间必须比开始时间至少大1小时。当前间隔：{timeDiff.TotalMinutes:F0} 分钟");
            }

            // 保存旧值用于记录变更（完整对象）
            var oldEntourage = entourage.Adapt<VisitingCompanyUpdateDto>();

            // 检查时间段和公司是否重复（排除自身，同一时段、同一公司不允许重复记录）
            // 时间段重叠的判断：两个时间段 [start1, end1] 和 [start2, end2] 重叠的条件是：start1 < end2 && start2 < end1
            // 同时需要检查公司名称是否相同
            if (entourage.VisitStartTime != dto.VisitStartTime || entourage.VisitEndTime != dto.VisitEndTime || entourage.VisitingCompanyName != dto.VisitingCompanyName)
            {
                var overlappingEntourage = await _visitingCompanyRepository.GetListAsync(
                    v => v.IsDeleted == 0 &&
                         v.Id != dto.Id &&
                         v.VisitingCompanyName == dto.VisitingCompanyName &&
                         v.VisitStartTime < dto.VisitEndTime &&
                         v.VisitEndTime > dto.VisitStartTime,
                    1,
                    int.MaxValue
                );

                if (overlappingEntourage.Items.Any())
                {
                    var conflictEntourage = overlappingEntourage.Items.First();
                    _appLog.Warning("更新随行人员失败：时间段和公司冲突。更新记录ID：{Id}，公司={Company}，新时间段：{StartTime} ~ {EndTime}，冲突记录ID：{ConflictId}，时间段：{ConflictStartTime} ~ {ConflictEndTime}",
                        dto.Id, dto.VisitingCompanyName, dto.VisitStartTime, dto.VisitEndTime, conflictEntourage.Id,
                        conflictEntourage.VisitStartTime, conflictEntourage.VisitEndTime);

                    return Result.Fail($"时间段冲突：公司 {dto.VisitingCompanyName} 在时间段 {dto.VisitStartTime:yyyy-MM-dd HH:mm:ss} ~ {dto.VisitEndTime:yyyy-MM-dd HH:mm:ss} 与现有记录重叠（现有记录ID：{conflictEntourage.Id}，时间段：{conflictEntourage.VisitStartTime:yyyy-MM-dd HH:mm:ss} ~ {conflictEntourage.VisitEndTime:yyyy-MM-dd HH:mm:ss}）。请调整时间段。");
                }
            }

            dto.Adapt(entourage);
            var result = await _visitingCompanyRepository.UpdateAsync(entourage);

            // 构建完整的变更信息，包含所有变更的字段
            var changeList = new List<string>();
            if (oldEntourage.VisitingCompanyName != dto.VisitingCompanyName) changeList.Add($"VisitingCompanyName: {oldEntourage.VisitingCompanyName} -> {dto.VisitingCompanyName}");
            if (oldEntourage.VisitStartTime != dto.VisitStartTime) changeList.Add($"VisitStartTime: {oldEntourage.VisitStartTime} -> {dto.VisitStartTime}");
            if (oldEntourage.VisitEndTime != dto.VisitEndTime) changeList.Add($"VisitEndTime: {oldEntourage.VisitEndTime} -> {dto.VisitEndTime}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新随行人员失败");

            _operLog?.LogUpdate("Entourage", dto.Id.ToString(), "Logistics.Entourage.EntourageView", changes, dto, oldEntourage, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新随行人员失败");
            return Result.Fail($"更新随行人员失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var entourage = await _visitingCompanyRepository.GetByIdAsync(id);
            if (entourage == null)
                return Result.Fail("随行人员不存在");

            // 删除关联的随行人员详情
            var details = await _visitingEntourageRepository.GetListAsync(
                vd => vd.VisitingCompanyId == id && vd.IsDeleted == 0,
                1,
                int.MaxValue
            );

            foreach (var detail in details.Items)
            {
                await _visitingEntourageRepository.DeleteAsync(detail);
            }

            // 删除随行人员
            var result = await _visitingCompanyRepository.DeleteAsync(entourage);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除随行人员失败");

            _operLog?.LogDelete("Entourage", id.ToString(), "Logistics.Entourage.EntourageView",
                new { EntourageId = id }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除随行人员失败");
            return Result.Fail($"删除随行人员失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除随行人员（同时删除关联的随行人员详情）
    /// </summary>
    /// <param name="ids">随行人员ID列表</param>
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
                var entourage = await _visitingCompanyRepository.GetByIdAsync(id);
                if (entourage == null)
                {
                    failCount++;
                    continue;
                }

                // 删除关联的随行人员详情
                var details = await _visitingEntourageRepository.GetListAsync(
                    vd => vd.VisitingCompanyId == id && vd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                foreach (var detail in details.Items)
                {
                    await _visitingEntourageRepository.DeleteAsync(detail);
                }

                var result = await _visitingCompanyRepository.DeleteAsync(entourage);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("Entourage", id.ToString(), "Logistics.Entourage.EntourageView",
                        new { EntourageId = id }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除随行人员完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除随行人员失败");
            return Result.Fail($"批量删除随行人员失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出随行人员到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的随行人员</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(VisitingCompanyQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<VisitingCompany>().And(x => x.IsDeleted == 0).ToExpression();
            var entourages = await _visitingCompanyRepository.AsQueryable().Where(where).OrderBy(v => v.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = entourages.Adapt<List<VisitingCompanyDto>>();
            sheetName ??= "Entourage";
            fileName ??= $"随行人员导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出随行人员Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出随行人员 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "Entourage";
        fileName ??= $"随行人员导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<VisitingCompanyDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入随行人员
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "Entourage";
            var dtos = ExcelHelper.ImportFromExcel<VisitingCompanyDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any())
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.VisitingCompanyName)) { fail++; continue; }

                    // 检查同一公司同一时段是否已存在
                    var existing = await _visitingCompanyRepository.GetFirstAsync(
                        v => v.VisitingCompanyName == dto.VisitingCompanyName &&
                             v.VisitStartTime == dto.VisitStartTime &&
                             v.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<VisitingCompany>();
                        await _visitingCompanyRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _visitingCompanyRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("Entourage", success, "Logistics.Entourage.EntourageView",
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入随行人员Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<VisitingCompany, bool>> QueryExpression(VisitingCompanyQueryDto query)
    {
        return SqlSugar.Expressionable.Create<VisitingCompany>()
            .And(v => v.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), v => v.VisitingCompanyName.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.VisitingCompanyName), v => v.VisitingCompanyName.Contains(query.VisitingCompanyName!))
            .AndIF(query.VisitStartTimeFrom != default(DateTime), v => v.VisitStartTime >= query.VisitStartTimeFrom)
            .AndIF(query.VisitStartTimeTo != default(DateTime), v => v.VisitStartTime <= query.VisitStartTimeTo)
            .ToExpression();
    }
}

