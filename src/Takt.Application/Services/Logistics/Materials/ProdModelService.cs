// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Materials
// 文件名称：ProdModelService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品机种服务实现
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
/// 产品机种服务实现
/// </summary>
public class ProdModelService : IProdModelService
{
    private readonly IBaseRepository<ProdModel> _prodModelRepository;
    private readonly AppLogManager _appLog;

    public ProdModelService(
        IBaseRepository<ProdModel> prodModelRepository,
        AppLogManager appLog)
    {
        _prodModelRepository = prodModelRepository;
        _appLog = appLog;
    }

    /// <summary>
    /// 查询产品机种列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、机种代码等筛选条件</param>
    /// <returns>分页产品机种列表</returns>
    public async Task<Result<PagedResult<ProdModelDto>>> GetListAsync(ProdModelQueryDto query)
    {
        _appLog.Information("开始查询产品机种列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<ProdModel, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "modelcode":
                        orderByExpression = pm => pm.ModelCode;
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
            var result = await _prodModelRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodModelDtos = result.Items.Adapt<List<ProdModelDto>>();

            var pagedResult = new PagedResult<ProdModelDto>
            {
                Items = prodModelDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdModelDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询产品机种数据失败");
            return Result<PagedResult<ProdModelDto>>.Fail($"高级查询产品机种数据失败: {ex.Message}");
        }
    }

    public async Task<Result<ProdModelDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodModel = await _prodModelRepository.GetByIdAsync(id);
            if (prodModel == null)
                return Result<ProdModelDto>.Fail("产品机种不存在");

            var prodModelDto = prodModel.Adapt<ProdModelDto>();
            return Result<ProdModelDto>.Ok(prodModelDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取产品机种失败");
            return Result<ProdModelDto>.Fail($"获取产品机种失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(ProdModelCreateDto dto)
    {
        try
        {
            // 检查机种编码是否已存在
            var exists = await _prodModelRepository.GetFirstAsync(pm => pm.ModelCode == dto.ModelCode && pm.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"机种编码 {dto.ModelCode} 已存在");

            var prodModel = dto.Adapt<ProdModel>();
            var result = await _prodModelRepository.CreateAsync(prodModel);
            return Result<long>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建产品机种失败");
            return Result<long>.Fail($"创建产品机种失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(ProdModelUpdateDto dto)
    {
        try
        {
            var prodModel = await _prodModelRepository.GetByIdAsync(dto.Id);
            if (prodModel == null)
                return Result.Fail("产品机种不存在");

            // 检查机种编码是否被其他记录使用
            if (prodModel.ModelCode != dto.ModelCode)
            {
                var exists = await _prodModelRepository.GetFirstAsync(pm => pm.ModelCode == dto.ModelCode && pm.Id != dto.Id && pm.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"机种编码 {dto.ModelCode} 已被其他记录使用");
            }

            dto.Adapt(prodModel);
            await _prodModelRepository.UpdateAsync(prodModel);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新产品机种失败");
            return Result.Fail($"更新产品机种失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var prodModel = await _prodModelRepository.GetByIdAsync(id);
            if (prodModel == null)
                return Result.Fail("产品机种不存在");

            await _prodModelRepository.DeleteAsync(prodModel);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除产品机种失败");
            return Result.Fail($"删除产品机种失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品机种到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品机种</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdModelQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdModel>().And(x => x.IsDeleted == 0).ToExpression();
            var models = await _prodModelRepository.AsQueryable().Where(where).OrderBy(pm => pm.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = models.Adapt<List<ProdModelDto>>();
            sheetName ??= "ProdModels";
            fileName ??= $"产品机种导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出产品机种Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品机种 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "ProdModels";
        fileName ??= $"产品机种导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<ProdModelDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入产品机种
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "ProdModels";
            var dtos = ExcelHelper.ImportFromExcel<ProdModelDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.ModelCode)) { fail++; continue; }
                    var existing = await _prodModelRepository.GetFirstAsync(pm => pm.ModelCode == dto.ModelCode && pm.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<ProdModel>();
                        await _prodModelRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _prodModelRepository.UpdateAsync(existing);
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
            _appLog.Error(ex, "导入产品机种Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<ProdModel, bool>> QueryExpression(ProdModelQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdModel>()
            .And(pm => pm.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), pm => pm.MaterialCode.Contains(query.Keywords!) || 
                                                               pm.ModelCode.Contains(query.Keywords!) || 
                                                               pm.DestCode.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), pm => pm.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.ModelCode), pm => pm.ModelCode.Contains(query.ModelCode!))
            .AndIF(!string.IsNullOrEmpty(query.DestCode), pm => pm.DestCode.Contains(query.DestCode!))
            .ToExpression();
    }
}

