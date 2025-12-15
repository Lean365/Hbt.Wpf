// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：DictionaryTypeService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：字典类型服务实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using System.Linq.Expressions;
using Takt.Application.Dtos.Routine;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Models;
using Takt.Common.Results;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;
using Mapster;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 字典类型服务实现
/// 注意：DictionaryType 和 DictionaryData 是主子表关系
/// </summary>
public class DictionaryTypeService : IDictionaryTypeService
{
    private readonly IBaseRepository<DictionaryType> _dictionaryTypeRepository;
    private readonly IBaseRepository<DictionaryData> _dictionaryDataRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public DictionaryTypeService(
        IBaseRepository<DictionaryType> dictionaryTypeRepository,
        IBaseRepository<DictionaryData> dictionaryDataRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _dictionaryTypeRepository = dictionaryTypeRepository;
        _dictionaryDataRepository = dictionaryDataRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询字典类型列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、类型代码等筛选条件</param>
    /// <returns>分页字典类型列表</returns>
    public async Task<Result<PagedResult<DictionaryTypeDto>>> GetListAsync(DictionaryTypeQueryDto query)
    {
        _appLog.Information("开始查询字典类型列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<DictionaryType, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "typecode":
                        orderByExpression = dt => dt.TypeCode;
                        break;
                    case "typename":
                        orderByExpression = dt => dt.TypeName;
                        break;
                    case "createdtime":
                        orderByExpression = dt => dt.CreatedTime;
                        break;
                    default:
                        orderByExpression = dt => dt.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = dt => dt.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _dictionaryTypeRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var dictionaryTypeDtos = result.Items.Adapt<List<DictionaryTypeDto>>();

            var pagedResult = new PagedResult<DictionaryTypeDto>
            {
                Items = dictionaryTypeDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<DictionaryTypeDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询字典类型数据失败");
            return Result<PagedResult<DictionaryTypeDto>>.Fail($"高级查询字典类型数据失败: {ex.Message}");
        }
    }

    public async Task<Result<DictionaryTypeDto>> GetByIdAsync(long id, bool includeData = false)
    {
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetByIdAsync(id);
            if (dictionaryType == null || dictionaryType.IsDeleted == 1)
                return Result<DictionaryTypeDto>.Fail("字典类型不存在");

            var dictionaryTypeDto = dictionaryType.Adapt<DictionaryTypeDto>();

            // 如果需要包含字典数据（主子表关联查询）
            if (includeData)
            {
                var dictionaryDataResult = await _dictionaryDataRepository.GetListAsync(
                    dd => dd.TypeCode == dictionaryType.TypeCode && dd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                // 注意：这里只是说明需要包含数据，但 DTO 需要扩展才能支持
                _appLog.Information("加载字典类型 {Id} 的字典数据，共 {Count} 条", id, dictionaryDataResult.Items.Count);
            }

            return Result<DictionaryTypeDto>.Ok(dictionaryTypeDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取字典类型失败");
            return Result<DictionaryTypeDto>.Fail($"获取字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result<DictionaryTypeDto>> GetByCodeAsync(string typeCode, bool includeData = false)
    {
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == typeCode && dt.IsDeleted == 0);
            if (dictionaryType == null)
                return Result<DictionaryTypeDto>.Fail("字典类型不存在");

            var dictionaryTypeDto = dictionaryType.Adapt<DictionaryTypeDto>();

            // 如果需要包含字典数据
            if (includeData)
            {
                var dictionaryDataResult = await _dictionaryDataRepository.GetListAsync(
                    dd => dd.TypeCode == dictionaryType.TypeCode && dd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                _appLog.Information("加载字典类型 {Code} 的字典数据，共 {Count} 条", typeCode, dictionaryDataResult.Items.Count);
            }

            return Result<DictionaryTypeDto>.Ok(dictionaryTypeDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取字典类型失败");
            return Result<DictionaryTypeDto>.Fail($"获取字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(DictionaryTypeCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查同一数据源下类型代码是否已存在
            var exists = await _dictionaryTypeRepository.GetFirstAsync(
                dt => dt.TypeCode == dto.TypeCode && 
                      dt.DataSource == dto.DataSource && 
                      dt.IsDeleted == 0);
            if (exists != null)
            {
                var dataSourceInfo = dto.DataSource == 0 ? "系统" : "SQL脚本";
                return Result<long>.Fail($"{dataSourceInfo}下字典类型代码 {dto.TypeCode} 已存在");
            }

            var dictionaryType = dto.Adapt<DictionaryType>();

            var result = await _dictionaryTypeRepository.CreateAsync(dictionaryType);
            var response = result > 0 ? Result<long>.Ok(dictionaryType.Id) : Result<long>.Fail("创建字典类型失败");

            _operLog?.LogCreate("DictionaryType", dictionaryType.Id.ToString(), "Routine.DictionaryTypeView", 
                dto, response, stopwatch);

            if (result > 0)
            {
                var dataSourceInfo = dictionaryType.DataSource == 0 ? "系统" : "SQL脚本";
                _appLog.Information("创建字典类型成功，ID: {Id}, 代码: {Code}, 数据源: {DataSource}", 
                    dictionaryType.Id, dictionaryType.TypeCode, dataSourceInfo);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建字典类型失败");
            return Result<long>.Fail($"创建字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(DictionaryTypeUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetByIdAsync(dto.Id);
            if (dictionaryType == null || dictionaryType.IsDeleted == 1)
                return Result.Fail("字典类型不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldDictionaryType = dictionaryType.Adapt<DictionaryTypeUpdateDto>();

            // 检查同一数据源下类型代码是否已被其他记录使用
            if (dictionaryType.TypeCode != dto.TypeCode || dictionaryType.DataSource != dto.DataSource)
            {
                var exists = await _dictionaryTypeRepository.GetFirstAsync(
                    dt => dt.TypeCode == dto.TypeCode && 
                          dt.DataSource == dto.DataSource && 
                          dt.Id != dto.Id && 
                          dt.IsDeleted == 0);
                if (exists != null)
                {
                    var dataSourceInfo = dto.DataSource == 0 ? "系统" : "SQL脚本";
                    return Result.Fail($"{dataSourceInfo}下字典类型代码 {dto.TypeCode} 已被其他字典类型使用");
                }
            }

            dto.Adapt(dictionaryType);

            var result = await _dictionaryTypeRepository.UpdateAsync(dictionaryType);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldDictionaryType.TypeCode != dto.TypeCode) changeList.Add($"TypeCode: {oldDictionaryType.TypeCode} -> {dto.TypeCode}");
            if (oldDictionaryType.TypeName != dto.TypeName) changeList.Add($"TypeName: {oldDictionaryType.TypeName} -> {dto.TypeName}");
            if (oldDictionaryType.DataSource != dto.DataSource) changeList.Add($"DataSource: {oldDictionaryType.DataSource} -> {dto.DataSource}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新字典类型失败");

            _operLog?.LogUpdate("DictionaryType", dto.Id.ToString(), "Routine.DictionaryTypeView", changes, dto, oldDictionaryType, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("更新字典类型成功，ID: {Id}", dictionaryType.Id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新字典类型失败");
            return Result.Fail($"更新字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetByIdAsync(id);
            if (dictionaryType == null || dictionaryType.IsDeleted == 1)
                return Result.Fail("字典类型不存在");

            // 检查是否为内置类型（0=是，1=否）
            if (dictionaryType.IsBuiltin == 0)
                return Result.Fail("内置字典类型不允许删除");

            // **主子表关系处理：删除主表时，同时删除子表数据（级联删除）**
            var dictionaryDataList = await _dictionaryDataRepository.GetListAsync(
                dd => dd.TypeCode == dictionaryType.TypeCode && dd.IsDeleted == 0,
                1,
                int.MaxValue
            );

            // 先删除所有关联的字典数据
            foreach (var dictionaryData in dictionaryDataList.Items)
            {
                await _dictionaryDataRepository.DeleteAsync(dictionaryData.Id);
            }

            _appLog.Information("删除字典类型 {Id} 的关联字典数据，共 {Count} 条", id, dictionaryDataList.Items.Count);

            // 再删除字典类型
            var result = await _dictionaryTypeRepository.DeleteAsync(id);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除字典类型失败");

            _operLog?.LogDelete("DictionaryType", id.ToString(), "Routine.DictionaryTypeView", 
                new { Id = id, TypeCode = dictionaryType.TypeCode, TypeName = dictionaryType.TypeName }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("删除字典类型成功，ID: {Id}", id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除字典类型失败");
            return Result.Fail($"删除字典类型失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除字典类型（同时删除关联的字典数据）
    /// </summary>
    /// <param name="ids">字典类型ID列表</param>
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
                var dictionaryType = await _dictionaryTypeRepository.GetByIdAsync(id);
                if (dictionaryType == null || dictionaryType.IsDeleted == 1)
                {
                    failCount++;
                    continue;
                }

                if (dictionaryType.IsBuiltin == 0)
                {
                    failCount++;
                    continue;
                }

                // 级联删除关联的字典数据
                var dictionaryDataList = await _dictionaryDataRepository.GetListAsync(
                    dd => dd.TypeCode == dictionaryType.TypeCode && dd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                foreach (var dictionaryData in dictionaryDataList.Items)
                {
                    await _dictionaryDataRepository.DeleteAsync(dictionaryData.Id);
                }

                var result = await _dictionaryTypeRepository.DeleteAsync(id);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("DictionaryType", id.ToString(), "Routine.DictionaryTypeView", 
                        new { Id = id, TypeCode = dictionaryType.TypeCode }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除字典类型完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除字典类型失败");
            return Result.Fail($"批量删除字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result> StatusAsync(long id, int status)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = await _dictionaryTypeRepository.StatusAsync(id, status);
            var response = result > 0 ? Result.Ok() : Result.Fail("修改字典类型状态失败");

            _operLog?.LogUpdate("DictionaryType", id.ToString(), "Routine.DictionaryTypeView", $"修改状态为 {status}",
                new { Id = id, Status = status }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("修改字典类型状态成功，ID: {Id}, 状态: {Status}", id, status);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "修改字典类型状态失败");
            return Result.Fail($"修改字典类型状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出字典类型到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的字典类型</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(DictionaryTypeQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<DictionaryType>().And(x => x.IsDeleted == 0).ToExpression();
            var types = await _dictionaryTypeRepository.AsQueryable().Where(where).OrderBy(dt => dt.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = types.Adapt<List<DictionaryTypeDto>>();
            sheetName ??= "DictionaryTypes";
            fileName ??= $"字典类型导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出字典类型Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出字典类型 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "DictionaryTypes";
        fileName ??= $"字典类型导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<DictionaryTypeDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入字典类型
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "DictionaryTypes";
            var dtos = ExcelHelper.ImportFromExcel<DictionaryTypeDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.TypeCode)) { fail++; continue; }
                    
                    // 检查同一数据源下类型代码是否已存在
                    var existing = await _dictionaryTypeRepository.GetFirstAsync(
                        dt => dt.TypeCode == dto.TypeCode && 
                              dt.DataSource == dto.DataSource && 
                              dt.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<DictionaryType>();
                        await _dictionaryTypeRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _dictionaryTypeRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("DictionaryType", success, "Routine.DictionaryTypeView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入字典类型Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<DictionaryType, bool>> QueryExpression(DictionaryTypeQueryDto query)
    {
        return SqlSugar.Expressionable.Create<DictionaryType>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.TypeCode.Contains(query.Keywords!) || 
                                                               x.TypeName.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.TypeCode), x => x.TypeCode.Contains(query.TypeCode!))
            .AndIF(!string.IsNullOrEmpty(query.TypeName), x => x.TypeName.Contains(query.TypeName!))
            .AndIF(query.TypeStatus.HasValue, x => x.TypeStatus == query.TypeStatus!.Value)
            .ToExpression();
    }

    /// <summary>
    /// 获取字典选项列表
    /// 根据字典类型代码获取选项列表，支持系统字典和SQL脚本字典
    /// </summary>
    /// <param name="typeCode">字典类型代码</param>
    /// <returns>选项列表</returns>
    public async Task<Result<List<SelectOptionModel>>> GetOptionsAsync(string typeCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(typeCode))
                return Result<List<SelectOptionModel>>.Fail("字典类型代码不能为空");

            // 获取字典类型
            var dictionaryType = await _dictionaryTypeRepository.GetFirstAsync(
                dt => dt.TypeCode == typeCode && dt.IsDeleted == 0);
            if (dictionaryType == null)
                return Result<List<SelectOptionModel>>.Fail($"字典类型 {typeCode} 不存在");

            // 检查状态
            if (dictionaryType.TypeStatus != 0)
                return Result<List<SelectOptionModel>>.Fail($"字典类型 {typeCode} 已禁用");

            List<SelectOptionModel> options;

            // 根据数据源类型处理
            if (dictionaryType.DataSource == 0)
            {
                // 系统字典：从 DictionaryData 表查询
                var dictionaryDataResult = await _dictionaryDataRepository.GetListAsync(
                    dd => dd.TypeCode == typeCode && dd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                options = dictionaryDataResult.Items.Select(dd => new SelectOptionModel
                {
                    DataValue = dd.DataValue ?? string.Empty,
                    DataLabel = dd.DataLabel,
                    ExtLabel = dd.ExtLabel,
                    ExtValue = dd.ExtValue,
                    CssClass = dd.CssClass,
                    ListClass = dd.ListClass,
                    OrderNum = dd.OrderNum
                }).OrderBy(o => o.OrderNum).ThenBy(o => o.DataValue).ToList();
            }
            else if (dictionaryType.DataSource == 1)
            {
                // SQL脚本字典：执行SQL脚本
                if (string.IsNullOrWhiteSpace(dictionaryType.SqlScript))
                    return Result<List<SelectOptionModel>>.Fail($"字典类型 {typeCode} 的SQL脚本为空");

                try
                {
                    // 执行SQL查询，期望返回至少 Value 和 Label 列
                    // SQL脚本应该返回格式：Value, Label, OrderNum(可选), ExtLabel(可选), ExtValue(可选), CssClass(可选), ListClass(可选)
                    var sqlResult = await _dictionaryTypeRepository.ExecuteSqlAsync(dictionaryType.SqlScript);
                    
                    options = sqlResult.Select(row =>
                    {
                        var dict = (IDictionary<string, object>)row;
                        
                        // 大小写不敏感的键查找辅助方法
                        string? GetValue(string key)
                        {
                            var keyLower = key.ToLower();
                            var matchedKey = dict.Keys.FirstOrDefault(k => k.ToLower() == keyLower);
                            return matchedKey != null ? dict[matchedKey]?.ToString() : null;
                        }
                        
                        int GetIntValue(string key, int defaultValue = 0)
                        {
                            var value = GetValue(key);
                            return int.TryParse(value, out var intValue) ? intValue : defaultValue;
                        }
                        
                        return new SelectOptionModel
                        {
                            DataValue = GetValue("Value") ?? GetValue("DataValue") ?? string.Empty,
                            DataLabel = GetValue("Label") ?? GetValue("DataLabel") ?? string.Empty,
                            ExtLabel = GetValue("ExtLabel"),
                            ExtValue = GetValue("ExtValue"),
                            CssClass = GetValue("CssClass"),
                            ListClass = GetValue("ListClass"),
                            OrderNum = GetIntValue("OrderNum", 0)
                        };
                    }).Where(o => !string.IsNullOrWhiteSpace(o.DataValue) && !string.IsNullOrWhiteSpace(o.DataLabel))
                      .OrderBy(o => o.OrderNum)
                      .ThenBy(o => o.DataValue)
                      .ToList();
                }
                catch (Exception sqlEx)
                {
                    _appLog.Error(sqlEx, "执行字典类型 {TypeCode} 的SQL脚本失败", typeCode);
                    return Result<List<SelectOptionModel>>.Fail($"执行SQL脚本失败: {sqlEx.Message}");
                }
            }
            else
            {
                return Result<List<SelectOptionModel>>.Fail($"字典类型 {typeCode} 的数据源类型不支持");
            }

            _appLog.Information("获取字典类型 {TypeCode} 的选项列表成功，共 {Count} 条", typeCode, options.Count);
            return Result<List<SelectOptionModel>>.Ok(options);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取字典选项列表失败");
            return Result<List<SelectOptionModel>>.Fail($"获取字典选项列表失败: {ex.Message}");
        }
    }
}
