// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Materials
// 文件名称：ProdMaterialService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：生产物料服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using System.Linq.Expressions;
using Takt.Application.Dtos.Logistics.Materials;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Materials;
using Takt.Domain.Repositories;
using Mapster;

namespace Takt.Application.Services.Logistics.Materials;

/// <summary>
/// 生产物料服务实现
/// </summary>
public class ProdMaterialService : IProdMaterialService
{
    private readonly IBaseRepository<ProdMaterial> _prodMaterialRepository;
    private readonly AppLogManager _appLog;

    public ProdMaterialService(
        IBaseRepository<ProdMaterial> prodMaterialRepository,
        AppLogManager appLog)
    {
        _prodMaterialRepository = prodMaterialRepository;
        _appLog = appLog;
    }

    /// <summary>
    /// 查询生产物料列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码等筛选条件</param>
    /// <returns>包含分页生产物料列表的结果对象，成功时返回物料列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在物料代码、物料描述中搜索）
    /// 支持按物料代码、创建时间排序，默认按创建时间倒序
    /// </remarks>
    public async Task<Result<PagedResult<ProdMaterialDto>>> GetListAsync(ProdMaterialQueryDto query)
    {
        _appLog.Information("开始查询生产物料列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<ProdMaterial, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "materialcode":
                        orderByExpression = pm => pm.MaterialCode;
                        break;
                    case "createdtime":
                        orderByExpression = pm => pm.CreatedTime;
                        break;
                    default:
                        orderByExpression = pm => pm.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = pm => pm.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _prodMaterialRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodMaterialDtos = result.Items.Adapt<List<ProdMaterialDto>>();

            var pagedResult = new PagedResult<ProdMaterialDto>
            {
                Items = prodMaterialDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdMaterialDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询生产物料数据失败");
            return Result<PagedResult<ProdMaterialDto>>.Fail($"高级查询生产物料数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取生产物料信息
    /// </summary>
    /// <param name="id">生产物料ID，必须大于0</param>
    /// <returns>包含生产物料信息的结果对象，成功时返回物料DTO，失败时返回错误信息（如物料不存在）</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// </remarks>
    public async Task<Result<ProdMaterialDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodMaterial = await _prodMaterialRepository.GetByIdAsync(id);
            if (prodMaterial == null)
                return Result<ProdMaterialDto>.Fail("生产物料不存在");

            var prodMaterialDto = prodMaterial.Adapt<ProdMaterialDto>();
            return Result<ProdMaterialDto>.Ok(prodMaterialDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取生产物料失败");
            return Result<ProdMaterialDto>.Fail($"获取生产物料失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新生产物料
    /// </summary>
    /// <param name="dto">创建生产物料数据传输对象，包含物料代码、物料描述等物料信息</param>
    /// <returns>包含新生产物料ID的结果对象，成功时返回物料ID，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result<long>> CreateAsync(ProdMaterialCreateDto dto)
    {
        try
        {
            // 检查物料编码是否已存在
            var exists = await _prodMaterialRepository.GetFirstAsync(pm => pm.MaterialCode == dto.MaterialCode && pm.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"物料编码 {dto.MaterialCode} 已存在");

            var prodMaterial = dto.Adapt<ProdMaterial>();
            var result = await _prodMaterialRepository.CreateAsync(prodMaterial);
            return Result<long>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建生产物料失败");
            return Result<long>.Fail($"创建生产物料失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新生产物料信息
    /// </summary>
    /// <param name="dto">更新生产物料数据传输对象，必须包含物料ID和要更新的字段信息</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如物料不存在）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、变更内容、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result> UpdateAsync(ProdMaterialUpdateDto dto)
    {
        try
        {
            var prodMaterial = await _prodMaterialRepository.GetByIdAsync(dto.Id);
            if (prodMaterial == null)
                return Result.Fail("生产物料不存在");

            // 检查物料编码是否被其他记录使用
            if (prodMaterial.MaterialCode != dto.MaterialCode)
            {
                var exists = await _prodMaterialRepository.GetFirstAsync(pm => pm.MaterialCode == dto.MaterialCode && pm.Id != dto.Id && pm.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"物料编码 {dto.MaterialCode} 已被其他记录使用");
            }

            dto.Adapt(prodMaterial);
            await _prodMaterialRepository.UpdateAsync(prodMaterial);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新生产物料失败");
            return Result.Fail($"更新生产物料失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var prodMaterial = await _prodMaterialRepository.GetByIdAsync(id);
            if (prodMaterial == null)
                return Result.Fail("生产物料不存在");

            await _prodMaterialRepository.DeleteAsync(prodMaterial);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除生产物料失败");
            return Result.Fail($"删除生产物料失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出生产物料到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的生产物料</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdMaterialQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdMaterial>().And(x => x.IsDeleted == 0).ToExpression();
            var materials = await _prodMaterialRepository.AsQueryable().Where(where).OrderBy(pm => pm.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = materials.Adapt<List<ProdMaterialDto>>();
            sheetName ??= "ProdMaterials";
            fileName ??= $"生产物料导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出生产物料Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出生产物料 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "ProdMaterials";
        fileName ??= $"生产物料导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<ProdMaterialDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入生产物料
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "ProdMaterials";
            var dtos = ExcelHelper.ImportFromExcel<ProdMaterialDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.MaterialCode)) { fail++; continue; }
                    var existing = await _prodMaterialRepository.GetFirstAsync(pm => pm.MaterialCode == dto.MaterialCode && pm.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<ProdMaterial>();
                        await _prodMaterialRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _prodMaterialRepository.UpdateAsync(existing);
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
            _appLog.Error(ex, "导入生产物料Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<ProdMaterial, bool>> QueryExpression(ProdMaterialQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdMaterial>()
            .And(pm => pm.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), pm => pm.MaterialCode.Contains(query.Keywords!) || 
                                                               (pm.MaterialDescription != null && pm.MaterialDescription.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), pm => pm.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.MaterialDescription), pm => pm.MaterialDescription != null && pm.MaterialDescription.Contains(query.MaterialDescription!))
            .AndIF(!string.IsNullOrEmpty(query.MaterialType), pm => pm.MaterialType == query.MaterialType!)
            .AndIF(!string.IsNullOrEmpty(query.Plant), pm => pm.Plant == query.Plant!)
            .ToExpression();
    }
}

