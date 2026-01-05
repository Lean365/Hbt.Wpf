// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
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
using Mapster;
using Newtonsoft.Json;
using Takt.Application.Dtos.Logistics.Materials;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Models;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Materials;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Logistics.Materials;

/// <summary>
/// 产品机种服务实现
/// </summary>
public class ProdModelService : IProdModelService
{
    private readonly IBaseRepository<ProdModel> _prodModelRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public ProdModelService(
        IBaseRepository<ProdModel> prodModelRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _prodModelRepository = prodModelRepository;
        _appLog = appLog;
        _operLog = operLog;
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查机种编码是否已存在
            var exists = await _prodModelRepository.GetFirstAsync(pm => pm.ModelCode == dto.ModelCode && pm.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"机种编码 {dto.ModelCode} 已存在");

            var prodModel = dto.Adapt<ProdModel>();
            var result = await _prodModelRepository.CreateAsync(prodModel);
            Result<long> response = Result<long>.Ok(prodModel.Id);
            
            _operLog?.LogCreate("ProdModel", prodModel.Id.ToString(), "Logistics.Materials.ProdModelView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建产品机种失败");
            return Result<long>.Fail($"创建产品机种失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(ProdModelUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodModel = await _prodModelRepository.GetByIdAsync(dto.Id);
            if (prodModel == null)
                return Result.Fail("产品机种不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldProdModel = prodModel.Adapt<ProdModelUpdateDto>();

            // 检查机种编码是否被其他记录使用
            if (prodModel.ModelCode != dto.ModelCode)
            {
                var exists = await _prodModelRepository.GetFirstAsync(pm => pm.ModelCode == dto.ModelCode && pm.Id != dto.Id && pm.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"机种编码 {dto.ModelCode} 已被其他记录使用");
            }

            dto.Adapt(prodModel);
            var result = await _prodModelRepository.UpdateAsync(prodModel);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldProdModel.ModelCode != dto.ModelCode) changeList.Add($"ModelCode: {oldProdModel.ModelCode} -> {dto.ModelCode}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新产品机种失败");
            
            _operLog?.LogUpdate("ProdModel", dto.Id.ToString(), "Logistics.Materials.ProdModelView", changes, dto, oldProdModel, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新产品机种失败");
            return Result.Fail($"更新产品机种失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodModel = await _prodModelRepository.GetByIdAsync(id);
            if (prodModel == null)
                return Result.Fail("产品机种不存在");

            var result = await _prodModelRepository.DeleteAsync(prodModel);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除产品机种失败");
            
            _operLog?.LogDelete("ProdModel", id.ToString(), "Logistics.Materials.ProdModelView", 
                new { Id = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除产品机种失败");
            return Result.Fail($"删除产品机种失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除产品机种
    /// </summary>
    /// <param name="ids">产品机种ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            var result = await _prodModelRepository.DeleteBatchAsync(ids.Cast<object>().ToList());
            var response = result > 0 ? Result.Ok($"成功删除 {result} 条记录") : Result.Fail("批量删除产品机种失败");
            
            _operLog?.LogDelete("ProdModel", string.Join(",", ids), "Logistics.Materials.ProdModelView", 
                new { Ids = ids, Count = ids.Count }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("批量删除产品机种成功，共删除 {Count} 条记录", result);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除产品机种失败");
            return Result.Fail($"批量删除产品机种失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品机种到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品机种</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdModelQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
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
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
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

            stopwatch.Stop();
            var operatorName = Takt.Common.Context.UserContext.Current.IsAuthenticated
                ? Takt.Common.Context.UserContext.Current.Username
                : "Takt365";
            var requestParams = JsonConvert.SerializeObject(new { Total = dtos.Count, Success = success, Fail = fail });
            _operLog?.Import("ProdModel", success, operatorName);

            // 序列化响应结果
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.Default
            };
            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
            
            _operLog?.LogImport("ProdModel", success, "Logistics.Materials.ProdModelView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入产品机种Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取机种选项列表
    /// 从产品机种表中获取所有不重复的机种编码，用于下拉列表等UI组件
    /// </summary>
    /// <returns>机种选项列表</returns>
    public async Task<Result<List<SelectOptionModel>>> GetModelOptionAsync()
    {
        try
        {
            _appLog.Information("开始获取机种选项列表");

            // 查询所有未删除的记录，获取不重复的机种编码
            var modelCodes = await _prodModelRepository.AsQueryable()
                .Where(pm => pm.IsDeleted == 0)
                .Select(pm => pm.ModelCode)
                .Distinct()
                .OrderBy(it => it)
                .ToListAsync();

            var options = modelCodes.Select((code, index) => new SelectOptionModel
            {
                DataValue = code,
                DataLabel = code,
                OrderNum = index + 1
            }).ToList();

            _appLog.Information("成功获取 {Count} 个机种选项", options.Count);
            return Result<List<SelectOptionModel>>.Ok(options);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取机种选项列表失败");
            return Result<List<SelectOptionModel>>.Fail($"获取机种选项列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取仕向地选项列表
    /// 从产品机种表中获取所有不重复的仕向编码，用于下拉列表等UI组件
    /// </summary>
    /// <returns>仕向地选项列表</returns>
    public async Task<Result<List<SelectOptionModel>>> GetDestOptionAsync()
    {
        try
        {
            _appLog.Information("开始获取仕向地选项列表");

            // 先获取所有符合条件的记录，然后在内存中去重和排序（避免 SQL DISTINCT + ORDER BY 语法问题）
            var allRecords = await _prodModelRepository.AsQueryable()
                .Where(pm => pm.IsDeleted == 0 && pm.DestCode != null && pm.DestCode != string.Empty)
                .Select(pm => pm.DestCode)
                .ToListAsync();

            // 在内存中去重和排序
            var destCodes = allRecords
                .Where(code => !string.IsNullOrEmpty(code))
                .Distinct()
                .OrderBy(code => code)
                .ToList();

            var options = destCodes.Select((code, index) => new SelectOptionModel
            {
                DataValue = code ?? string.Empty,
                DataLabel = code ?? string.Empty,
                OrderNum = index + 1
            }).ToList();

            _appLog.Information("成功获取 {Count} 个仕向地选项", options.Count);
            if (options.Count > 0)
            {
                _appLog.Information("第一个仕向地选项：{Value} - {Label}", options[0].DataValue, options[0].DataLabel);
            }
            return Result<List<SelectOptionModel>>.Ok(options);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取仕向地选项列表失败");
            return Result<List<SelectOptionModel>>.Fail($"获取仕向地选项列表失败: {ex.Message}");
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

