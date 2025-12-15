// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Materials
// 文件名称：ProdPackingService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：包装信息服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Mapster;
using Newtonsoft.Json;
using Takt.Application.Dtos.Logistics.Materials;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Materials;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Logistics.Materials;

/// <summary>
/// 包装信息服务实现
/// </summary>
public class ProdPackingService : IProdPackingService
{
    private readonly IBaseRepository<ProdPacking> _prodPackingRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public ProdPackingService(
        IBaseRepository<ProdPacking> prodPackingRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _prodPackingRepository = prodPackingRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询包装信息列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料编码等筛选条件</param>
    /// <returns>包含分页包装信息列表的结果对象，成功时返回包装信息列表和总数，失败时返回错误信息</returns>
    public async Task<Result<PagedResult<ProdPackingDto>>> GetListAsync(ProdPackingQueryDto query)
    {
        _appLog.Information("开始查询包装信息列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            Expression<Func<ProdPacking, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "materialcode":
                        orderByExpression = pp => pp.MaterialCode;
                        break;
                    case "createdtime":
                        orderByExpression = pp => pp.CreatedTime;
                        break;
                    default:
                        orderByExpression = pp => pp.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = pp => pp.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _prodPackingRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodPackingDtos = result.Items.Adapt<List<ProdPackingDto>>();

            var pagedResult = new PagedResult<ProdPackingDto>
            {
                Items = prodPackingDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdPackingDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询包装信息数据失败");
            return Result<PagedResult<ProdPackingDto>>.Fail($"高级查询包装信息数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取包装信息
    /// </summary>
    /// <param name="id">包装信息ID，必须大于0</param>
    /// <returns>包含包装信息的结果对象，成功时返回包装信息DTO，失败时返回错误信息（如包装信息不存在）</returns>
    public async Task<Result<ProdPackingDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodPacking = await _prodPackingRepository.GetByIdAsync(id);
            if (prodPacking == null)
                return Result<ProdPackingDto>.Fail("包装信息不存在");

            var prodPackingDto = prodPacking.Adapt<ProdPackingDto>();
            return Result<ProdPackingDto>.Ok(prodPackingDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取包装信息失败");
            return Result<ProdPackingDto>.Fail($"获取包装信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新包装信息
    /// </summary>
    /// <param name="dto">创建包装信息数据传输对象，包含物料编码、包装类型等包装信息</param>
    /// <returns>包含新包装信息ID的结果对象，成功时返回包装信息ID，失败时返回错误信息</returns>
    public async Task<Result<long>> CreateAsync(ProdPackingCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查物料编码是否已存在
            var exists = await _prodPackingRepository.GetFirstAsync(
                pp => pp.MaterialCode == dto.MaterialCode && pp.IsDeleted == 0);
            if (exists != null)
            {
                return Result<long>.Fail($"物料编码 {dto.MaterialCode} 的包装信息已存在");
            }

            var prodPacking = dto.Adapt<ProdPacking>();
            var result = await _prodPackingRepository.CreateAsync(prodPacking);
            Result<long> response = result > 0 
                ? Result<long>.Ok(prodPacking.Id) 
                : Result<long>.Fail("创建包装信息失败");
            
            _operLog?.LogCreate("ProdPacking", prodPacking.Id.ToString(), "Logistics.Materials.ProdPackingView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建包装信息失败，物料编码={MaterialCode}", dto.MaterialCode);
            return Result<long>.Fail($"创建包装信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新包装信息
    /// </summary>
    /// <param name="dto">更新包装信息数据传输对象，包含ID和要更新的字段</param>
    /// <returns>包含操作结果的结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    public async Task<Result> UpdateAsync(ProdPackingUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodPacking = await _prodPackingRepository.GetByIdAsync(dto.Id);
            if (prodPacking == null)
                return Result.Fail("包装信息不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldProdPacking = prodPacking.Adapt<ProdPackingUpdateDto>();

            // 检查物料编码是否被其他记录使用
            if (prodPacking.MaterialCode != dto.MaterialCode)
            {
                var exists = await _prodPackingRepository.GetFirstAsync(
                    pp => pp.MaterialCode == dto.MaterialCode && pp.Id != dto.Id && pp.IsDeleted == 0);
                if (exists != null)
                {
                    return Result.Fail($"物料编码 {dto.MaterialCode} 已被其他包装信息使用");
                }
            }

            dto.Adapt(prodPacking);
            var result = await _prodPackingRepository.UpdateAsync(prodPacking);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldProdPacking.MaterialCode != dto.MaterialCode) changeList.Add($"MaterialCode: {oldProdPacking.MaterialCode} -> {dto.MaterialCode}");
            if (oldProdPacking.PackingType != dto.PackingType) changeList.Add($"PackingType: {oldProdPacking.PackingType ?? "null"} -> {dto.PackingType ?? "null"}");
            if (oldProdPacking.PackingUnit != dto.PackingUnit) changeList.Add($"PackingUnit: {oldProdPacking.PackingUnit ?? "null"} -> {dto.PackingUnit ?? "null"}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新包装信息失败");
            
            _operLog?.LogUpdate("ProdPacking", dto.Id.ToString(), "Logistics.Materials.ProdPackingView", changes, dto, oldProdPacking, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新包装信息失败，ID={Id}", dto.Id);
            return Result.Fail($"更新包装信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除包装信息（软删除）
    /// </summary>
    /// <param name="id">包装信息ID，必须大于0</param>
    /// <returns>包含操作结果的结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodPacking = await _prodPackingRepository.GetByIdAsync(id);
            if (prodPacking == null)
                return Result.Fail("包装信息不存在");

            var result = await _prodPackingRepository.DeleteAsync(prodPacking);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除包装信息失败");
            
            _operLog?.LogDelete("ProdPacking", id.ToString(), "Logistics.Materials.ProdPackingView", 
                new { Id = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除包装信息失败，ID={Id}", id);
            return Result.Fail($"删除包装信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除包装信息
    /// </summary>
    /// <param name="ids">包装信息ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            var result = await _prodPackingRepository.DeleteBatchAsync(ids.Cast<object>().ToList());
            var response = result > 0 ? Result.Ok($"成功删除 {result} 条记录") : Result.Fail("批量删除包装信息失败");
            
            _operLog?.LogDelete("ProdPacking", string.Join(",", ids), "Logistics.Materials.ProdPackingView", 
                new { Ids = ids, Count = ids.Count }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("批量删除包装信息成功，共删除 {Count} 条记录", result);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除包装信息失败");
            return Result.Fail($"批量删除包装信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出包装信息到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的包装信息</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdPackingQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdPacking>().And(x => x.IsDeleted == 0).ToExpression();
            var packings = await _prodPackingRepository.AsQueryable().Where(where).OrderBy(pp => pp.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = packings.Adapt<List<ProdPackingExportDto>>();
            sheetName ??= "ProdPackings";
            fileName ??= $"包装信息导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出包装信息Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    private Expression<Func<ProdPacking, bool>> QueryExpression(ProdPackingQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdPacking>()
            .And(pp => pp.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), pp => pp.MaterialCode.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), pp => pp.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.PackingType), pp => pp.PackingType == query.PackingType!)
            .AndIF(!string.IsNullOrEmpty(query.PackingUnit), pp => pp.PackingUnit == query.PackingUnit!)
            .ToExpression();
    }
}

