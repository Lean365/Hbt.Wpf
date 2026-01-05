// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：DictionaryDataService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：字典数据服务实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Mapster;
using System.Linq;
using System.Linq.Expressions;
using Takt.Application.Dtos.Routine;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 字典数据服务实现
/// 注意：DictionaryData 是子表，关联 DictionaryType（主表）
/// </summary>
public class DictionaryDataService : IDictionaryDataService
{
    private readonly IBaseRepository<DictionaryData> _dictionaryDataRepository;
    private readonly IBaseRepository<DictionaryType> _dictionaryTypeRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public DictionaryDataService(
        IBaseRepository<DictionaryData> dictionaryDataRepository,
        IBaseRepository<DictionaryType> dictionaryTypeRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _dictionaryDataRepository = dictionaryDataRepository;
        _dictionaryTypeRepository = dictionaryTypeRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询字典数据列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、类型代码等筛选条件</param>
    /// <returns>分页字典数据列表</returns>
    public async Task<Result<PagedResult<DictionaryDataDto>>> GetListAsync(DictionaryDataQueryDto query)
    {
        _appLog.Information("开始查询字典数据列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);

            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<DictionaryData, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "datalabel":
                        orderByExpression = d => d.DataLabel;
                        break;
                    case "sortorder":
                        orderByExpression = d => d.OrderNum;
                        break;
                    case "createdtime":
                        orderByExpression = d => d.CreatedTime;
                        break;
                    default:
                        orderByExpression = d => d.OrderNum; // 默认按排序顺序
                        break;
                }
            }
            else
            {
                orderByExpression = d => d.OrderNum; // 默认按排序顺序
            }

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }

            // 使用真实的数据库查询
            var result = await _dictionaryDataRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var dictionaryDataDtos = result.Items.Adapt<List<DictionaryDataDto>>();

            var pagedResult = new PagedResult<DictionaryDataDto>
            {
                Items = dictionaryDataDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<DictionaryDataDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询字典数据失败");
            return Result<PagedResult<DictionaryDataDto>>.Fail($"高级查询字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result<List<DictionaryDataDto>>> GetByTypeCodeAsync(string typeCode)
    {
        try
        {
            var result = await _dictionaryDataRepository.GetListAsync(
                dd => dd.TypeCode == typeCode && dd.IsDeleted == 0,
                1,
                int.MaxValue
            );

            var dictionaryDataDtos = result.Items.Adapt<List<DictionaryDataDto>>();
            return Result<List<DictionaryDataDto>>.Ok(dictionaryDataDtos);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取字典数据失败");
            return Result<List<DictionaryDataDto>>.Fail($"获取字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result<DictionaryDataDto>> GetByIdAsync(long id)
    {
        var dictionaryData = await _dictionaryDataRepository.GetByIdAsync(id);
        if (dictionaryData == null)
            return Result<DictionaryDataDto>.Fail("字典数据不存在");

        var dictionaryDataDto = dictionaryData.Adapt<DictionaryDataDto>();
        return Result<DictionaryDataDto>.Ok(dictionaryDataDto);
    }

    public async Task<Result<long>> CreateAsync(DictionaryDataCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // **主子表关系验证：检查主表是否存在（按代码）**
            var dictionaryType = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == dto.TypeCode && dt.IsDeleted == 0);
            if (dictionaryType == null)
                return Result<long>.Fail("关联的字典类型不存在");

            // 检查 TypeCode + DataLabel + DataValue 组合是否已存在
            var existsByCombination = await _dictionaryDataRepository.GetFirstAsync(
                dd => dd.TypeCode == dto.TypeCode && 
                      dd.DataLabel == dto.DataLabel && 
                      (dd.DataValue == dto.DataValue || (dd.DataValue == null && dto.DataValue == null)) &&
                      dd.IsDeleted == 0);
            if (existsByCombination != null)
                return Result<long>.Fail($"字典类型 {dto.TypeCode} 下已存在标签为 {dto.DataLabel}、值为 {dto.DataValue ?? "空"} 的字典数据");

            var dictionaryData = dto.Adapt<DictionaryData>();
            // 设置 TypeCode
            dictionaryData.TypeCode = dto.TypeCode;

            var result = await _dictionaryDataRepository.CreateAsync(dictionaryData);
            var response = result > 0 ? Result<long>.Ok(dictionaryData.Id) : Result<long>.Fail("创建字典数据失败");

            _operLog?.LogCreate("DictionaryData", dictionaryData.Id.ToString(), "Routine.DictionaryDataView", 
                dto, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("创建字典数据成功，ID: {Id}, 类型: {TypeCode}, 标签: {Label}",
                    dictionaryData.Id, dictionaryData.TypeCode, dictionaryData.DataLabel);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建字典数据失败");
            return Result<long>.Fail($"创建字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(DictionaryDataUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var dictionaryData = await _dictionaryDataRepository.GetByIdAsync(dto.Id);
            if (dictionaryData == null || dictionaryData.IsDeleted == 1)
                return Result.Fail("字典数据不存在");

            // **主子表关系验证：目标主表必须存在（按代码）**
            var targetType = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == dto.TypeCode && dt.IsDeleted == 0);
            if (targetType == null)
                return Result.Fail("关联的字典类型不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldDictionaryData = dictionaryData.Adapt<DictionaryDataUpdateDto>();

            // 检查 TypeCode + DataLabel + DataValue 组合是否被其他记录使用
            if (dictionaryData.TypeCode != dto.TypeCode || 
                dictionaryData.DataLabel != dto.DataLabel || 
                dictionaryData.DataValue != dto.DataValue)
            {
                var existsByCombination = await _dictionaryDataRepository.GetFirstAsync(
                    dd => dd.TypeCode == dto.TypeCode && 
                          dd.DataLabel == dto.DataLabel && 
                          (dd.DataValue == dto.DataValue || (dd.DataValue == null && dto.DataValue == null)) &&
                          dd.Id != dto.Id && 
                          dd.IsDeleted == 0);
                if (existsByCombination != null)
                    return Result.Fail($"字典类型 {dto.TypeCode} 下已存在标签为 {dto.DataLabel}、值为 {dto.DataValue ?? "空"} 的字典数据");
            }

            dto.Adapt(dictionaryData);
            // 确保 TypeCode 被正确更新
            dictionaryData.TypeCode = dto.TypeCode;

            var result = await _dictionaryDataRepository.UpdateAsync(dictionaryData);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldDictionaryData.TypeCode != dto.TypeCode) changeList.Add($"TypeCode: {oldDictionaryData.TypeCode} -> {dto.TypeCode}");
            if (oldDictionaryData.DataLabel != dto.DataLabel) changeList.Add($"DataLabel: {oldDictionaryData.DataLabel} -> {dto.DataLabel}");
            if (oldDictionaryData.DataValue != dto.DataValue) changeList.Add($"DataValue: {oldDictionaryData.DataValue ?? "null"} -> {dto.DataValue ?? "null"}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新字典数据失败");

            _operLog?.LogUpdate("DictionaryData", dto.Id.ToString(), "Routine.DictionaryDataView", changes, dto, oldDictionaryData, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("更新字典数据成功，ID: {Id}", dictionaryData.Id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新字典数据失败");
            return Result.Fail($"更新字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var dictionaryData = await _dictionaryDataRepository.GetByIdAsync(id);
            if (dictionaryData == null || dictionaryData.IsDeleted == 1)
                return Result.Fail("字典数据不存在");

            var result = await _dictionaryDataRepository.DeleteAsync(id);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除字典数据失败");

            _operLog?.LogDelete("DictionaryData", id.ToString(), "Routine.DictionaryDataView", 
                new { Id = id, TypeCode = dictionaryData.TypeCode, DataLabel = dictionaryData.DataLabel }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("删除字典数据成功，ID: {Id}", id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除字典数据失败");
            return Result.Fail($"删除字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            var result = await _dictionaryDataRepository.DeleteBatchAsync(ids.Cast<object>().ToList());
            var response = result > 0 ? Result.Ok($"成功删除 {result} 条记录") : Result.Fail("批量删除字典数据失败");
            
            _operLog?.LogDelete("DictionaryData", string.Join(",", ids), "Routine.DictionaryDataView", 
                new { Ids = ids, Count = ids.Count }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("批量删除字典数据成功，共删除 {Count} 条记录", result);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除字典数据失败");
            return Result.Fail($"批量删除字典数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出字典数据到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的字典数据</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(DictionaryDataQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<DictionaryData>().And(x => x.IsDeleted == 0).ToExpression();
            var dataList = await _dictionaryDataRepository.AsQueryable().Where(where).OrderBy(d => d.OrderNum).ToListAsync();
            var dtos = dataList.Adapt<List<DictionaryDataDto>>();
            sheetName ??= "DictionaryData";
            fileName ??= $"字典数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出字典数据Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出字典数据 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "DictionaryData";
        fileName ??= $"字典数据导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<DictionaryDataDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入字典数据
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "DictionaryData";
            var dtos = ExcelHelper.ImportFromExcel<DictionaryDataDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.TypeCode) || string.IsNullOrWhiteSpace(dto.DataLabel)) { fail++; continue; }
                    
                    // 检查 TypeCode + DataLabel + DataValue 组合是否已存在
                    var existing = await _dictionaryDataRepository.GetFirstAsync(
                        dd => dd.TypeCode == dto.TypeCode && 
                              dd.DataLabel == dto.DataLabel && 
                              (dd.DataValue == dto.DataValue || (dd.DataValue == null && dto.DataValue == null)) &&
                              dd.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<DictionaryData>();
                        await _dictionaryDataRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _dictionaryDataRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("DictionaryData", success, "Routine.DictionaryDataView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入字典数据Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<DictionaryData, bool>> QueryExpression(DictionaryDataQueryDto query)
    {
        return SqlSugar.Expressionable.Create<DictionaryData>()
            .And(x => x.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.TypeCode), x => x.TypeCode == query.TypeCode)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.DataLabel.Contains(query.Keywords!) ||
                                                              (x.DataValue != null && x.DataValue.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.DataLabel), x => x.DataLabel.Contains(query.DataLabel!))
            .ToExpression();
    }
}
