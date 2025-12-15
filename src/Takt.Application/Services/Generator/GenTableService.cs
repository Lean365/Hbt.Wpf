// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Generator
// 文件名称：GenTableService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表配置服务实现
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Mapster;
using Newtonsoft.Json;
using SqlSugar;
using Takt.Application.Dtos.Generator;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Generator;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Generator;

/// <summary>
/// 代码生成表配置服务实现
/// </summary>
public class GenTableService : IGenTableService
{
    private readonly IBaseRepository<GenTable> _genTableRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="genTableRepository">代码生成表配置仓储</param>
    /// <param name="appLog">应用程序日志管理器</param>
    /// <param name="operLog">操作日志管理器（可选）</param>
    public GenTableService(IBaseRepository<GenTable> genTableRepository, AppLogManager appLog, OperLogManager? operLog = null)
    {
        _genTableRepository = genTableRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询代码生成表配置列表
    /// </summary>
    public async Task<Result<PagedResult<GenTableDto>>> GetListAsync(GenTableQueryDto query)
    {
        _appLog.Information("开始查询代码生成表配置列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            var whereExpression = QueryExpression(query);

            Expression<Func<GenTable, object>>? orderByExpression = null;
            OrderByType orderByType = OrderByType.Desc;

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "tablename":
                        orderByExpression = x => x.TableName;
                        break;
                    case "tabledescription":
                        orderByExpression = x => x.TableDescription ?? string.Empty;
                        break;
                    case "createdtime":
                        orderByExpression = x => x.CreatedTime;
                        break;
                    default:
                        orderByExpression = x => x.CreatedTime;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = OrderByType.Asc;
            }

            var result = await _genTableRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var dtos = result.Items.Adapt<List<GenTableDto>>();

            _appLog.Information("查询完成，返回 {Count} 条记录，总数: {TotalNum}", dtos.Count, result.TotalNum);

            var pagedResult = new PagedResult<GenTableDto>
            {
                Items = dtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<GenTableDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询代码生成表配置数据失败");
            return Result<PagedResult<GenTableDto>>.Fail($"查询数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取代码生成表配置
    /// </summary>
    public async Task<Result<GenTableDto>> GetByIdAsync(long id)
    {
        var entity = await _genTableRepository.GetByIdAsync(id);
        if (entity == null)
            return Result<GenTableDto>.Fail("记录不存在");

        var dto = entity.Adapt<GenTableDto>();
        return Result<GenTableDto>.Ok(dto);
    }

    /// <summary>
    /// 创建代码生成表配置
    /// </summary>
    public async Task<Result<long>> CreateAsync(GenTableCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 验证表名唯一性
            var exists = await _genTableRepository.GetFirstAsync(x => x.TableName == dto.TableName && x.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"表名 {dto.TableName} 已存在");

            var entity = dto.Adapt<GenTable>();

            var result = await _genTableRepository.CreateAsync(entity);
            Result<long> response = result > 0 
                ? Result<long>.Ok(entity.Id) 
                : Result<long>.Fail("创建失败");
            
            _operLog?.LogCreate("GenTable", entity.Id.ToString(), "Generator.GenTableView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建代码生成表配置失败");
            return Result<long>.Fail($"创建失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新代码生成表配置
    /// </summary>
    public async Task<Result> UpdateAsync(GenTableUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var entity = await _genTableRepository.GetByIdAsync(dto.Id);
            if (entity == null)
                return Result.Fail("记录不存在");

            var oldTableName = entity.TableName;

            // 验证表名唯一性（如果表名有变化）
            if (entity.TableName != dto.TableName)
            {
                var exists = await _genTableRepository.GetFirstAsync(x => x.TableName == dto.TableName && x.Id != dto.Id && x.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"表名 {dto.TableName} 已被其他记录使用");
            }

            dto.Adapt(entity);

            var result = await _genTableRepository.UpdateAsync(entity);
            var changes = $"TableName: {oldTableName} -> {dto.TableName}";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新失败");
            
            _operLog?.LogUpdate("GenTable", dto.Id.ToString(), "Generator.GenTableView", changes, 
                new { Id = dto.Id, TableName = dto.TableName }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新代码生成表配置失败");
            return Result.Fail($"更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除代码生成表配置
    /// </summary>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var entity = await _genTableRepository.GetByIdAsync(id);
            if (entity == null)
                return Result.Fail("记录不存在");

            var result = await _genTableRepository.DeleteAsync(id);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除失败");
            
            _operLog?.LogDelete("GenTable", id.ToString(), "Generator.GenTableView", 
                new { Id = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除代码生成表配置失败");
            return Result.Fail($"删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除代码生成表配置
    /// </summary>
    /// <param name="ids">代码生成表配置ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            var result = await _genTableRepository.DeleteBatchAsync(ids.Cast<object>().ToList());
            var response = result > 0 ? Result.Ok($"成功删除 {result} 条记录") : Result.Fail("批量删除代码生成表配置失败");
            
            _operLog?.LogDelete("GenTable", string.Join(",", ids), "Generator.GenTableView", 
                new { Ids = ids, Count = ids.Count }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("批量删除代码生成表配置成功，共删除 {Count} 条记录", result);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除代码生成表配置失败");
            return Result.Fail($"批量删除代码生成表配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出代码生成表配置到Excel
    /// </summary>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(GenTableQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : Expressionable.Create<GenTable>().And(x => x.IsDeleted == 0).ToExpression();
            var entities = await _genTableRepository.AsQueryable().Where(where).OrderBy(x => x.CreatedTime, OrderByType.Desc).ToListAsync();
            var dtos = entities.Adapt<List<GenTableDto>>();
            sheetName ??= "GenTables";
            fileName ??= $"代码生成表配置导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出代码生成表配置Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取代码生成表配置导入模板
    /// </summary>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "GenTables";
        fileName ??= $"代码生成表配置导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<GenTableDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从Excel导入代码生成表配置
    /// </summary>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "GenTables";
            var dtos = ExcelHelper.ImportFromExcel<GenTableDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0, fail = 0;
            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.TableName)) { fail++; continue; }

                    var existing = await _genTableRepository.GetFirstAsync(x => x.TableName == dto.TableName && x.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<GenTable>();
                        await _genTableRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _genTableRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
            
            _operLog?.LogImport("GenTable", success, "Generator.GenTableView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "从Excel导入代码生成表配置失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<GenTable, bool>> QueryExpression(GenTableQueryDto query)
    {
        return Expressionable.Create<GenTable>()
            .And(x => x.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.TableName.Contains(query.Keywords!) ||
                                                               (x.TableDescription != null && x.TableDescription.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.TableName), x => x.TableName.Contains(query.TableName!))
            .AndIF(!string.IsNullOrEmpty(query.TableDescription), x => x.TableDescription != null && x.TableDescription.Contains(query.TableDescription!))
            .AndIF(!string.IsNullOrEmpty(query.TemplateType), x => x.TemplateType != null && x.TemplateType.Contains(query.TemplateType!))
            .ToExpression();
    }
}

