// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Generator
// 文件名称：GenColumnService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成列配置服务实现
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Mapster;
using Newtonsoft.Json;
using SqlSugar;
using System.Linq.Expressions;
using Takt.Application.Dtos.Generator;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Generator;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Generator;

/// <summary>
/// 代码生成列配置服务实现
/// </summary>
public class GenColumnService : IGenColumnService
{
    private readonly IBaseRepository<GenColumn> _genColumnRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="genColumnRepository">代码生成列配置仓储</param>
    /// <param name="appLog">应用程序日志管理器</param>
    /// <param name="operLog">操作日志管理器（可选）</param>
    public GenColumnService(IBaseRepository<GenColumn> genColumnRepository, AppLogManager appLog, OperLogManager? operLog = null)
    {
        _genColumnRepository = genColumnRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询代码生成列配置列表
    /// </summary>
    public async Task<Result<PagedResult<GenColumnDto>>> GetListAsync(GenColumnQueryDto query)
    {
        _appLog.Information("开始查询代码生成列配置列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            var whereExpression = QueryExpression(query);

            Expression<Func<GenColumn, object>>? orderByExpression = null;
            OrderByType orderByType = OrderByType.Desc;

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "tablename":
                        orderByExpression = x => x.TableName;
                        break;
                    case "columnname":
                        orderByExpression = x => x.ColumnName;
                        break;
                    case "ordernum":
                        orderByExpression = x => x.OrderNum;
                        break;
                    case "createdtime":
                        orderByExpression = x => x.CreatedTime;
                        break;
                    default:
                        orderByExpression = x => x.OrderNum;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = OrderByType.Asc;
            }

            var result = await _genColumnRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var dtos = result.Items.Adapt<List<GenColumnDto>>();

            _appLog.Information("查询完成，返回 {Count} 条记录，总数: {TotalNum}", dtos.Count, result.TotalNum);

            var pagedResult = new PagedResult<GenColumnDto>
            {
                Items = dtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<GenColumnDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询代码生成列配置数据失败");
            return Result<PagedResult<GenColumnDto>>.Fail($"查询数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取代码生成列配置
    /// </summary>
    public async Task<Result<GenColumnDto>> GetByIdAsync(long id)
    {
        var entity = await _genColumnRepository.GetByIdAsync(id);
        if (entity == null)
            return Result<GenColumnDto>.Fail("记录不存在");

        var dto = entity.Adapt<GenColumnDto>();
        return Result<GenColumnDto>.Ok(dto);
    }

    /// <summary>
    /// 创建代码生成列配置
    /// </summary>
    public async Task<Result<long>> CreateAsync(GenColumnCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 验证表名+列名唯一性
            var exists = await _genColumnRepository.GetFirstAsync(x => x.TableName == dto.TableName && x.ColumnName == dto.ColumnName && x.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"表 {dto.TableName} 的列 {dto.ColumnName} 已存在");

            var entity = dto.Adapt<GenColumn>();

            var result = await _genColumnRepository.CreateAsync(entity);
            Result<long> response = result > 0 
                ? Result<long>.Ok(entity.Id) 
                : Result<long>.Fail("创建失败");
            
            _operLog?.LogCreate("GenColumn", entity.Id.ToString(), "Generator.GenColumnView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建代码生成列配置失败");
            return Result<long>.Fail($"创建失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量创建代码生成列配置
    /// </summary>
    public async Task<Result<(int success, int fail, List<string> failMessages)>> CreateBatchAsync(List<GenColumnCreateDto> dtos)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (dtos == null || !dtos.Any())
                return Result<(int success, int fail, List<string> failMessages)>.Fail("DTO列表为空");

            // 批量验证唯一性（与 CreateAsync 方法一致，但使用批量查询避免连接问题）
            // 1. 一次性查询所有可能已存在的列（精确匹配表名+列名组合）
            // 完全依赖 SqlSugar 的自动连接管理，不手动干预
            var checkPairs = dtos.Select(d => new { d.TableName, d.ColumnName }).Distinct().ToList();

            // 使用 Expressionable 构建 OR 条件
            var expressionable = SqlSugar.Expressionable.Create<GenColumn>();
            foreach (var pair in checkPairs)
            {
                var tableName = pair.TableName;
                var columnName = pair.ColumnName;
                expressionable = expressionable.Or(x => x.TableName == tableName && x.ColumnName == columnName);
            }

            // 批量查询已存在的列（SqlSugar 会自动管理连接，不需要手动干预）
            // 如果查询失败，跳过查重，由数据库唯一约束处理重复数据
            List<GenColumn> existingColumns;
            try
            {
                existingColumns = await _genColumnRepository.AsQueryable()
                    .Where(x => x.IsDeleted == 0)
                    .Where(expressionable.ToExpression())
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // 任何查询错误都跳过查重，允许继续创建（由数据库唯一约束处理）
                _appLog.Warning("查询已存在列时发生错误，跳过查重，直接创建（由数据库唯一约束处理）: {Error}", ex.Message);
                existingColumns = new List<GenColumn>();
            }

            // 2. 构建已存在的键集合（表名+列名）
            var existingKeys = existingColumns
                .Select(c => $"{c.TableName}_{c.ColumnName}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 3. 过滤出不存在的列进行创建
            var entitiesToCreate = new List<GenColumn>();
            var failMessages = new List<string>();

            foreach (var dto in dtos)
            {
                // 验证表名+列名唯一性（与 CreateAsync 方法一致的逻辑）
                var key = $"{dto.TableName}_{dto.ColumnName}";
                if (existingKeys.Contains(key))
                {
                    failMessages.Add($"表 {dto.TableName} 的列 {dto.ColumnName} 已存在");
                    continue;
                }

                var entity = dto.Adapt<GenColumn>();
                entitiesToCreate.Add(entity);
            }

            // 批量创建
            int successCount = 0;
            if (entitiesToCreate.Any())
            {
                var result = await _genColumnRepository.CreateBatchAsync(entitiesToCreate);
                successCount = result;

                // 记录操作日志（批量创建时，为每个实体记录一条日志）
                if (result > 0 && _operLog != null)
                {
                    foreach (var entity in entitiesToCreate)
                    {
                        var createResponse = Result<long>.Ok(entity.Id);
                        _operLog.LogCreate("GenColumn", entity.Id.ToString(), "Generator.GenColumnView", 
                            new { TableName = entity.TableName, ColumnName = entity.ColumnName }, createResponse, stopwatch);
                    }
                }
            }

            int failCount = dtos.Count - successCount;
            return Result<(int success, int fail, List<string> failMessages)>.Ok((successCount, failCount, failMessages));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量创建代码生成列配置失败");
            return Result<(int success, int fail, List<string> failMessages)>.Fail($"批量创建失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新代码生成列配置
    /// </summary>
    public async Task<Result> UpdateAsync(GenColumnUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var entity = await _genColumnRepository.GetByIdAsync(dto.Id);
            if (entity == null)
                return Result.Fail("记录不存在");

            var oldTableName = entity.TableName;
            var oldColumnName = entity.ColumnName;

            // 验证表名+列名唯一性（如果表名或列名有变化）
            if (entity.TableName != dto.TableName || entity.ColumnName != dto.ColumnName)
            {
                var exists = await _genColumnRepository.GetFirstAsync(x => x.TableName == dto.TableName && x.ColumnName == dto.ColumnName && x.Id != dto.Id && x.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"表 {dto.TableName} 的列 {dto.ColumnName} 已被其他记录使用");
            }

            dto.Adapt(entity);

            var result = await _genColumnRepository.UpdateAsync(entity);
            var changes = $"TableName: {oldTableName} -> {dto.TableName}, ColumnName: {oldColumnName} -> {dto.ColumnName}";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新失败");
            
            _operLog?.LogUpdate("GenColumn", dto.Id.ToString(), "Generator.GenColumnView", changes, 
                new { Id = dto.Id, TableName = dto.TableName, ColumnName = dto.ColumnName }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新代码生成列配置失败");
            return Result.Fail($"更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除代码生成列配置
    /// </summary>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var entity = await _genColumnRepository.GetByIdAsync(id);
            if (entity == null)
                return Result.Fail("记录不存在");

            var result = await _genColumnRepository.DeleteAsync(id);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除失败");
            
            _operLog?.LogDelete("GenColumn", id.ToString(), "Generator.GenColumnView", 
                new { Id = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除代码生成列配置失败");
            return Result.Fail($"删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除代码生成列配置
    /// </summary>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            int successCount = 0;
            foreach (var id in ids)
            {
                var result = await _genColumnRepository.DeleteAsync(id);
                if (result > 0)
                    successCount++;
            }

            var response = Result.Ok($"成功删除 {successCount} 条记录");
            
            _operLog?.LogDelete("GenColumn", string.Join(",", ids), "Generator.GenColumnView", 
                new { Ids = ids }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除代码生成列配置失败");
            return Result.Fail($"批量删除失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据表名获取列配置列表
    /// </summary>
    public async Task<Result<List<GenColumnDto>>> GetByTableNameAsync(string tableName)
    {
        try
        {
            _appLog.Information("开始根据表名获取列配置列表，表名={TableName}", tableName);

            // 使用一个很大的 pageSize 来获取所有记录，确保完整获取所有字段信息
            var entities = await _genColumnRepository.GetListAsync(
                condition: x => x.TableName == tableName && x.IsDeleted == 0,
                pageIndex: 1,
                pageSize: 10000, // 使用足够大的 pageSize 确保获取所有记录
                orderByExpression: x => x.OrderNum,
                orderByType: OrderByType.Asc);

            _appLog.Information("从数据库获取到 {Count} 条记录", entities.Items.Count);

            // 验证实体数据
            foreach (var entity in entities.Items.Take(3)) // 只记录前3条作为示例
            {
                _appLog.Debug("实体数据: Id={Id}, TableName={TableName}, ColumnName={ColumnName}, PropertyName={PropertyName}, DataType={DataType}, ColumnDataType={ColumnDataType}, OrderNum={OrderNum}, IsQuery={IsQuery}, QueryType={QueryType}, FormControlType={FormControlType}, DictType={DictType}",
                    entity.Id, entity.TableName, entity.ColumnName, entity.PropertyName ?? string.Empty, entity.DataType ?? string.Empty, entity.ColumnDataType ?? string.Empty, entity.OrderNum, entity.IsQuery, entity.QueryType ?? string.Empty, entity.FormControlType ?? string.Empty, entity.DictType ?? string.Empty);
            }

            var dtos = entities.Items.Adapt<List<GenColumnDto>>();

            _appLog.Information("映射后 DTO 数量: {Count}", dtos.Count);

            // 验证 DTO 数据
            foreach (var dto in dtos.Take(3)) // 只记录前3条作为示例
            {
                _appLog.Debug("DTO 数据: Id={Id}, TableName={TableName}, ColumnName={ColumnName}, PropertyName={PropertyName}, DataType={DataType}, ColumnDataType={ColumnDataType}, OrderNum={OrderNum}, IsQuery={IsQuery}, QueryType={QueryType}, FormControlType={FormControlType}, DictType={DictType}",
                    dto.Id, dto.TableName, dto.ColumnName, dto.PropertyName ?? string.Empty, dto.DataType ?? string.Empty, dto.ColumnDataType ?? string.Empty, dto.OrderNum, dto.IsQuery, dto.QueryType ?? string.Empty, dto.FormControlType ?? string.Empty, dto.DictType ?? string.Empty);
            }

            return Result<List<GenColumnDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "根据表名获取列配置列表失败，表名={TableName}", tableName);
            return Result<List<GenColumnDto>>.Fail($"查询失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出代码生成列配置到Excel
    /// </summary>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(GenColumnQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : Expressionable.Create<GenColumn>().And(x => x.IsDeleted == 0).ToExpression();
            var entities = await _genColumnRepository.AsQueryable().Where(where).OrderBy(x => x.OrderNum).OrderBy(x => x.CreatedTime, OrderByType.Desc).ToListAsync();
            var dtos = entities.Adapt<List<GenColumnDto>>();
            sheetName ??= "GenColumns";
            fileName ??= $"代码生成列配置导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出代码生成列配置Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取代码生成列配置导入模板
    /// </summary>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "GenColumns";
        fileName ??= $"代码生成列配置导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<GenColumnDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从Excel导入代码生成列配置
    /// </summary>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "GenColumns";
            var dtos = ExcelHelper.ImportFromExcel<GenColumnDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0, fail = 0;
            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.TableName) || string.IsNullOrWhiteSpace(dto.ColumnName)) { fail++; continue; }

                    var existing = await _genColumnRepository.GetFirstAsync(x => x.TableName == dto.TableName && x.ColumnName == dto.ColumnName && x.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<GenColumn>();
                        await _genColumnRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _genColumnRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
            
            _operLog?.LogImport("GenColumn", success, "Generator.GenColumnView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "从Excel导入代码生成列配置失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<GenColumn, bool>> QueryExpression(GenColumnQueryDto query)
    {
        return Expressionable.Create<GenColumn>()
            .And(x => x.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.ColumnName.Contains(query.Keywords!) ||
                                                              (x.ColumnDescription != null && x.ColumnDescription.Contains(query.Keywords!)) ||
                                                              x.TableName.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.TableName), x => x.TableName.Contains(query.TableName!))
            .AndIF(!string.IsNullOrEmpty(query.ColumnName), x => x.ColumnName.Contains(query.ColumnName!))
            .ToExpression();
    }
}

