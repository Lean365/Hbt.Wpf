// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Visitors
// 文件名称：VisitorDetailService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客详情服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using System.Linq.Expressions;
using Takt.Application.Dtos.Logistics.Visitors;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Visitors;
using Takt.Domain.Repositories;
using Mapster;

namespace Takt.Application.Services.Logistics.Visitors;

/// <summary>
/// 访客详情服务实现
/// </summary>
public class VisitorDetailService : IVisitorDetailService
{
    private readonly IBaseRepository<VisitorDetail> _visitorDetailRepository;
    private readonly IBaseRepository<Visitor> _visitorRepository;
    private readonly AppLogManager _appLog;

    public VisitorDetailService(
        IBaseRepository<VisitorDetail> visitorDetailRepository,
        IBaseRepository<Visitor> visitorRepository,
        AppLogManager appLog)
    {
        _visitorDetailRepository = visitorDetailRepository;
        _visitorRepository = visitorRepository;
        _appLog = appLog;
    }

    /// <summary>
    /// 查询访客详情列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、访客ID等筛选条件</param>
    /// <returns>分页访客详情列表</returns>
    public async Task<Result<PagedResult<VisitorDetailDto>>> GetListAsync(VisitorDetailQueryDto query)
    {
        _appLog.Information("开始查询访客详情列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<VisitorDetail, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "name":
                        orderByExpression = vd => vd.Name;
                        break;
                    case "department":
                        orderByExpression = vd => vd.Department;
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
            var result = await _visitorDetailRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var visitorDetailDtos = result.Items.Adapt<List<VisitorDetailDto>>();

            var pagedResult = new PagedResult<VisitorDetailDto>
            {
                Items = visitorDetailDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<VisitorDetailDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询访客详情数据失败");
            return Result<PagedResult<VisitorDetailDto>>.Fail($"高级查询访客详情数据失败: {ex.Message}");
        }
    }

    public async Task<Result<VisitorDetailDto>> GetByIdAsync(long id)
    {
        try
        {
            var visitorDetail = await _visitorDetailRepository.GetByIdAsync(id);
            if (visitorDetail == null)
                return Result<VisitorDetailDto>.Fail("访客详情不存在");

            var visitorDetailDto = visitorDetail.Adapt<VisitorDetailDto>();
            return Result<VisitorDetailDto>.Ok(visitorDetailDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取访客详情失败");
            return Result<VisitorDetailDto>.Fail($"获取访客详情失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(VisitorDetailCreateDto dto)
    {
        try
        {
            // 获取关联的访客信息（用于检查公司和时段）
            var visitor = await _visitorRepository.GetByIdAsync(dto.VisitorId);
            if (visitor == null || visitor.IsDeleted == 1)
                return Result<long>.Fail("关联的访客记录不存在");

            // 检查同一公司同一时段同一部门同一姓名同一职务是否已存在
            var exists = await _visitorDetailRepository.GetFirstAsync(
                vd => vd.VisitorId == dto.VisitorId &&
                      vd.Department == dto.Department &&
                      vd.Name == dto.Name &&
                      vd.Position == dto.Position &&
                      vd.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"公司 {visitor.CompanyName} 在 {visitor.StartTime:yyyy-MM-dd HH:mm:ss} 时段已存在部门 {dto.Department}、姓名 {dto.Name}、职务 {dto.Position} 的访客详情记录");

            var visitorDetail = dto.Adapt<VisitorDetail>();
            var result = await _visitorDetailRepository.CreateAsync(visitorDetail);
            return Result<long>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建访客详情失败");
            return Result<long>.Fail($"创建访客详情失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(VisitorDetailUpdateDto dto)
    {
        try
        {
            var visitorDetail = await _visitorDetailRepository.GetByIdAsync(dto.Id);
            if (visitorDetail == null)
                return Result.Fail("访客详情不存在");

            // 获取关联的访客信息（用于检查公司和时段）
            var visitor = await _visitorRepository.GetByIdAsync(dto.VisitorId);
            if (visitor == null || visitor.IsDeleted == 1)
                return Result.Fail("关联的访客记录不存在");

            // 检查同一公司同一时段同一部门同一姓名同一职务是否被其他记录使用
            if (visitorDetail.VisitorId != dto.VisitorId || 
                visitorDetail.Department != dto.Department || 
                visitorDetail.Name != dto.Name || 
                visitorDetail.Position != dto.Position)
            {
                var exists = await _visitorDetailRepository.GetFirstAsync(
                    vd => vd.VisitorId == dto.VisitorId &&
                          vd.Department == dto.Department &&
                          vd.Name == dto.Name &&
                          vd.Position == dto.Position &&
                          vd.Id != dto.Id &&
                          vd.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"公司 {visitor.CompanyName} 在 {visitor.StartTime:yyyy-MM-dd HH:mm:ss} 时段已存在部门 {dto.Department}、姓名 {dto.Name}、职务 {dto.Position} 的访客详情记录");
            }

            dto.Adapt(visitorDetail);
            await _visitorDetailRepository.UpdateAsync(visitorDetail);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新访客详情失败");
            return Result.Fail($"更新访客详情失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var visitorDetail = await _visitorDetailRepository.GetByIdAsync(id);
            if (visitorDetail == null)
                return Result.Fail("访客详情不存在");

            await _visitorDetailRepository.DeleteAsync(visitorDetail);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除访客详情失败");
            return Result.Fail($"删除访客详情失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRangeAsync(List<long> ids)
    {
        try
        {
            foreach (var id in ids)
            {
                var visitorDetail = await _visitorDetailRepository.GetByIdAsync(id);
                if (visitorDetail != null)
                {
                    await _visitorDetailRepository.DeleteAsync(visitorDetail);
                }
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "批量删除访客详情失败");
            return Result.Fail($"批量删除访客详情失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出访客详情到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的访客详情</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(VisitorDetailQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<VisitorDetail>().And(x => x.IsDeleted == 0).ToExpression();
            var visitorDetails = await _visitorDetailRepository.AsQueryable().Where(where).OrderBy(vd => vd.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = visitorDetails.Adapt<List<VisitorDetailDto>>();
            sheetName ??= "VisitorDetails";
            fileName ??= $"访客详情导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出访客详情Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出访客详情 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "VisitorDetails";
        fileName ??= $"访客详情导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<VisitorDetailDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入访客详情
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "VisitorDetails";
            var dtos = ExcelHelper.ImportFromExcel<VisitorDetailDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (dto.VisitorId <= 0 || string.IsNullOrWhiteSpace(dto.Name) || 
                        string.IsNullOrWhiteSpace(dto.Department) || string.IsNullOrWhiteSpace(dto.Position))
                    {
                        fail++;
                        continue;
                    }

                    // 获取关联的访客信息（用于检查公司和时段）
                    var visitor = await _visitorRepository.GetByIdAsync(dto.VisitorId);
                    if (visitor == null || visitor.IsDeleted == 1)
                    {
                        fail++;
                        continue;
                    }

                    // 检查同一公司同一时段同一部门同一姓名同一职务是否已存在
                    var existing = await _visitorDetailRepository.GetFirstAsync(
                        vd => vd.VisitorId == dto.VisitorId &&
                              vd.Department == dto.Department &&
                              vd.Name == dto.Name &&
                              vd.Position == dto.Position &&
                              vd.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<VisitorDetail>();
                        await _visitorDetailRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _visitorDetailRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            return Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导入访客详情Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<VisitorDetail, bool>> QueryExpression(VisitorDetailQueryDto query)
    {
        return SqlSugar.Expressionable.Create<VisitorDetail>()
            .And(vd => vd.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), vd => vd.Name.Contains(query.Keywords!) || 
                                                               vd.Department.Contains(query.Keywords!) ||
                                                               (vd.Position != null && vd.Position.Contains(query.Keywords!)))
            .AndIF(query.VisitorId.HasValue && query.VisitorId.Value > 0, vd => vd.VisitorId == query.VisitorId!.Value)
            .AndIF(!string.IsNullOrEmpty(query.Name), vd => vd.Name.Contains(query.Name!))
            .AndIF(!string.IsNullOrEmpty(query.Department), vd => vd.Department.Contains(query.Department!))
            .ToExpression();
    }
}

