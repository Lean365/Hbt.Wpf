// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Visits
// 文件名称：VisitingEntourageService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员详情服务实现
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
/// 来访成员详情服务实现
/// </summary>
public class VisitingEntourageService : IVisitingEntourageService
{
    private readonly IBaseRepository<VisitingEntourage> _visitingEntourageRepository;
    private readonly IBaseRepository<VisitingCompany> _visitingCompanyRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public VisitingEntourageService(
        IBaseRepository<VisitingEntourage> visitingEntourageRepository,
        IBaseRepository<VisitingCompany> visitingCompanyRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _visitingEntourageRepository = visitingEntourageRepository;
        _visitingCompanyRepository = visitingCompanyRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询随行人员详情列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、随行人员ID等筛选条件</param>
    /// <returns>分页随行人员详情列表</returns>
    public async Task<Result<PagedResult<VisitingEntourageDto>>> GetListAsync(VisitingEntourageQueryDto query)
    {
        _appLog.Information("开始查询随行人员详情列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);

            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<VisitingEntourage, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "name":
                        orderByExpression = vd => vd.VisitingMembers;
                        break;
                    case "department":
                        orderByExpression = vd => vd.VisitDept;
                        break;
                    case "createdtime":
                        orderByExpression = vd => vd.CreatedTime;
                        break;
                    default:
                        orderByExpression = vd => vd.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = vd => vd.CreatedTime; // 默认按创建时间倒序
            }

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }

            // 使用真实的数据库查询
            var result = await _visitingEntourageRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var entourageDetailDtos = result.Items.Adapt<List<VisitingEntourageDto>>();

            var pagedResult = new PagedResult<VisitingEntourageDto>
            {
                Items = entourageDetailDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<VisitingEntourageDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询随行人员详情数据失败");
            return Result<PagedResult<VisitingEntourageDto>>.Fail($"高级查询随行人员详情数据失败: {ex.Message}");
        }
    }

    public async Task<Result<VisitingEntourageDto>> GetByIdAsync(long id)
    {
        try
        {
            var entourageDetail = await _visitingEntourageRepository.GetByIdAsync(id);
            if (entourageDetail == null)
                return Result<VisitingEntourageDto>.Fail("随行人员详情不存在");

            var entourageDetailDto = entourageDetail.Adapt<VisitingEntourageDto>();
            return Result<VisitingEntourageDto>.Ok(entourageDetailDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取随行人员详情失败");
            return Result<VisitingEntourageDto>.Fail($"获取随行人员详情失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(VisitingEntourageCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 获取关联的随行人员信息（用于检查公司和时段）
            var visitor = await _visitingCompanyRepository.GetByIdAsync(dto.VisitingCompanyId);
            if (visitor == null || visitor.IsDeleted == 1)
                return Result<long>.Fail("关联的来访公司记录不存在");

            // 检查同一公司同一时段同一部门同一姓名同一职务是否已存在
            var exists = await _visitingEntourageRepository.GetFirstAsync(
                vd => vd.VisitingCompanyId == dto.VisitingCompanyId &&
                      vd.VisitDept == dto.VisitDept &&
                      vd.VisitingMembers == dto.VisitingMembers &&
                      vd.VisitPost == dto.VisitPost &&
                      vd.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"公司 {visitor.VisitingCompanyName} 在 {visitor.VisitStartTime:yyyy-MM-dd HH:mm:ss} 时段已存在部门 {dto.VisitDept}、姓名 {dto.VisitingMembers}、职务 {dto.VisitPost} 的来访成员详情记录");

            var visitorDetail = dto.Adapt<VisitingEntourage>();
            var result = await _visitingEntourageRepository.CreateAsync(visitorDetail);
            var response = result > 0 ? Result<long>.Ok(visitorDetail.Id) : Result<long>.Fail("创建随行人员详情失败");

            _operLog?.LogCreate("VisitingEntourage", visitorDetail.Id.ToString(), "Logistics.Entourage.VisitingEntourageView",
                dto, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建随行人员详情失败");
            return Result<long>.Fail($"创建随行人员详情失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(VisitingEntourageUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var visitorDetail = await _visitingEntourageRepository.GetByIdAsync(dto.Id);
            if (visitorDetail == null)
                return Result.Fail("随行人员详情不存在");

            // 获取关联的来访公司信息（用于检查公司和时段）
            var visitor = await _visitingCompanyRepository.GetByIdAsync(dto.VisitingCompanyId);
            if (visitor == null || visitor.IsDeleted == 1)
                return Result.Fail("关联的随行人员记录不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldVisitingEntourage = visitorDetail.Adapt<VisitingEntourageUpdateDto>();

            // 检查同一公司同一时段同一部门同一姓名同一职务是否被其他记录使用
            if (visitorDetail.VisitingCompanyId != dto.VisitingCompanyId ||
                visitorDetail.VisitDept != dto.VisitDept ||
                visitorDetail.VisitingMembers != dto.VisitingMembers ||
                visitorDetail.VisitPost != dto.VisitPost)
            {
                var exists = await _visitingEntourageRepository.GetFirstAsync(
                    vd => vd.VisitingCompanyId == dto.VisitingCompanyId &&
                          vd.VisitDept == dto.VisitDept &&
                          vd.VisitingMembers == dto.VisitingMembers &&
                          vd.VisitPost == dto.VisitPost &&
                          vd.Id != dto.Id &&
                          vd.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"公司 {visitor.VisitingCompanyName} 在 {visitor.VisitStartTime:yyyy-MM-dd HH:mm:ss} 时段已存在部门 {dto.VisitDept}、姓名 {dto.VisitingMembers}、职务 {dto.VisitPost} 的来访成员详情记录");
            }

            dto.Adapt(visitorDetail);
            var result = await _visitingEntourageRepository.UpdateAsync(visitorDetail);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldVisitingEntourage.VisitingMembers != dto.VisitingMembers) changeList.Add($"VisitingMembers: {oldVisitingEntourage.VisitingMembers} -> {dto.VisitingMembers}");
            if (oldVisitingEntourage.VisitDept != dto.VisitDept) changeList.Add($"VisitDept: {oldVisitingEntourage.VisitDept} -> {dto.VisitDept}");
            if (oldVisitingEntourage.VisitPost != dto.VisitPost) changeList.Add($"VisitPost: {oldVisitingEntourage.VisitPost ?? "null"} -> {dto.VisitPost ?? "null"}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新随行人员详情失败");

            _operLog?.LogUpdate("VisitingEntourage", dto.Id.ToString(), "Logistics.Entourage.VisitingEntourageView", changes, dto, oldVisitingEntourage, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新随行人员详情失败");
            return Result.Fail($"更新随行人员详情失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var visitorDetail = await _visitingEntourageRepository.GetByIdAsync(id);
            if (visitorDetail == null)
                return Result.Fail("随行人员详情不存在");

            var result = await _visitingEntourageRepository.DeleteAsync(visitorDetail);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除随行人员详情失败");

            _operLog?.LogDelete("VisitingEntourage", id.ToString(), "Logistics.Entourage.VisitingEntourageView",
                new { Id = id, VisitingMembers = visitorDetail.VisitingMembers }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除随行人员详情失败");
            return Result.Fail($"删除随行人员详情失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            int successCount = 0;
            var deletedInfos = new List<object>();

            foreach (var id in ids)
            {
                var visitorDetail = await _visitingEntourageRepository.GetByIdAsync(id);
                if (visitorDetail != null)
                {
                    var result = await _visitingEntourageRepository.DeleteAsync(visitorDetail);
                    if (result > 0)
                    {
                        successCount++;
                        deletedInfos.Add(new { Id = id, VisitingCompanyId = visitorDetail.VisitingCompanyId });
                    }
                }
            }

            var response = Result.Ok($"成功删除 {successCount} 条记录");

            _operLog?.LogDelete("VisitingEntourage", string.Join(",", ids), "Logistics.Entourage.VisitingEntourageView",
                new { Ids = ids, SuccessCount = successCount, DeletedInfos = deletedInfos }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除随行人员详情失败");
            return Result.Fail($"批量删除随行人员详情失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出随行人员详情到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的随行人员详情</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(VisitingEntourageQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<VisitingEntourage>().And(x => x.IsDeleted == 0).ToExpression();
            var visitorDetails = await _visitingEntourageRepository.AsQueryable().Where(where).OrderBy(vd => vd.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = visitorDetails.Adapt<List<VisitingEntourageDto>>();
            sheetName ??= "VisitingEntourages";
            fileName ??= $"随行人员详情导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出随行人员详情Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出随行人员详情 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "VisitingEntourages";
        fileName ??= $"随行人员详情导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<VisitingEntourageDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入随行人员详情
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "VisitingEntourages";
            var dtos = ExcelHelper.ImportFromExcel<VisitingEntourageDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any())
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (dto.VisitingCompanyId <= 0 || string.IsNullOrWhiteSpace(dto.VisitingMembers) ||
                        string.IsNullOrWhiteSpace(dto.VisitDept) || string.IsNullOrWhiteSpace(dto.VisitPost))
                    {
                        fail++;
                        continue;
                    }

                    // 获取关联的来访公司信息（用于检查公司和时段）
                    var visitor = await _visitingCompanyRepository.GetByIdAsync(dto.VisitingCompanyId);
                    if (visitor == null || visitor.IsDeleted == 1)
                    {
                        fail++;
                        continue;
                    }

                    // 检查同一公司同一时段同一部门同一姓名同一职务是否已存在
                    var existing = await _visitingEntourageRepository.GetFirstAsync(
                        vd => vd.VisitingCompanyId == dto.VisitingCompanyId &&
                              vd.VisitDept == dto.VisitDept &&
                              vd.VisitingMembers == dto.VisitingMembers &&
                              vd.VisitPost == dto.VisitPost &&
                              vd.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<VisitingEntourage>();
                        await _visitingEntourageRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _visitingEntourageRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("VisitingEntourage", success, "Logistics.Entourage.VisitingEntourageView",
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入随行人员详情Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<VisitingEntourage, bool>> QueryExpression(VisitingEntourageQueryDto query)
    {
        return SqlSugar.Expressionable.Create<VisitingEntourage>()
            .And(vd => vd.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), vd => vd.VisitingMembers.Contains(query.Keywords!) ||
                                                               vd.VisitDept.Contains(query.Keywords!) ||
                                                               (vd.VisitPost != null && vd.VisitPost.Contains(query.Keywords!)))
            .AndIF(query.VisitingCompanyId.HasValue && query.VisitingCompanyId.Value > 0, vd => vd.VisitingCompanyId == query.VisitingCompanyId!.Value)
            .AndIF(!string.IsNullOrEmpty(query.VisitingMembers), vd => vd.VisitingMembers.Contains(query.VisitingMembers!))
            .AndIF(!string.IsNullOrEmpty(query.VisitDept), vd => vd.VisitDept.Contains(query.VisitDept!))
            .ToExpression();
    }
}

