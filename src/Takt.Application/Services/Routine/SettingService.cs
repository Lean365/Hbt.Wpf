// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：SettingService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：系统设置服务实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Takt.Application.Dtos.Routine;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;
using Mapster;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 系统设置服务实现
/// </summary>
public class SettingService : ISettingService
{
    private readonly IBaseRepository<Setting> _settingRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public SettingService(IBaseRepository<Setting> settingRepository, AppLogManager appLog, OperLogManager? operLog = null)
    {
        _settingRepository = settingRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询系统设置列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、设置键等筛选条件</param>
    /// <returns>包含分页系统设置列表的结果对象，成功时返回设置列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在设置键、设置值、描述中搜索）
    /// 支持按设置键、创建时间排序，默认按创建时间倒序
    /// </remarks>
    public async Task<Result<PagedResult<SettingDto>>> GetListAsync(SettingQueryDto query)
    {
        _appLog.Information("开始查询系统设置列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<Setting, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "settingkey":
                        orderByExpression = s => s.SettingKey;
                        break;
                    case "createdtime":
                        orderByExpression = s => s.CreatedTime;
                        break;
                    default:
                        orderByExpression = s => s.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = s => s.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _settingRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var settingDtos = result.Items.Adapt<List<SettingDto>>();

            var pagedResult = new PagedResult<SettingDto>
            {
                Items = settingDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<SettingDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询系统设置数据失败");
            return Result<PagedResult<SettingDto>>.Fail($"高级查询系统设置数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取系统设置信息
    /// </summary>
    /// <param name="id">系统设置ID，必须大于0</param>
    /// <returns>包含系统设置信息的结果对象，成功时返回设置DTO，失败时返回错误信息（如设置不存在）</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// </remarks>
    public async Task<Result<SettingDto>> GetByIdAsync(long id)
    {
        var setting = await _settingRepository.GetByIdAsync(id);
        if (setting == null)
            return Result<SettingDto>.Fail("系统设置不存在");

        var settingDto = setting.Adapt<SettingDto>();
        return Result<SettingDto>.Ok(settingDto);
    }

    /// <summary>
    /// 根据设置键获取系统设置信息
    /// </summary>
    /// <param name="settingKey">设置键，不能为空</param>
    /// <returns>包含系统设置信息的结果对象，成功时返回设置DTO，失败时返回错误信息（如设置不存在）</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// </remarks>
    public async Task<Result<SettingDto>> GetByKeyAsync(string settingKey)
    {
        var setting = await _settingRepository.GetFirstAsync(s => s.SettingKey == settingKey && s.IsDeleted == 0);
        if (setting == null)
            return Result<SettingDto>.Fail("系统设置不存在");

        var settingDto = setting.Adapt<SettingDto>();
        return Result<SettingDto>.Ok(settingDto);
    }

    /// <summary>
    /// 根据分类获取系统设置列表
    /// </summary>
    /// <param name="category">分类名称，不能为空</param>
    /// <returns>包含系统设置列表的结果对象，成功时返回设置列表，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// </remarks>
    public async Task<Result<List<SettingDto>>> GetByCategoryAsync(string category)
    {
        var settings = await _settingRepository.GetListAsync(s => s.Category == category && s.IsDeleted == 0, 1, int.MaxValue);
        var settingDtos = settings.Items.Adapt<List<SettingDto>>();
        return Result<List<SettingDto>>.Ok(settingDtos);
    }

    /// <summary>
    /// 创建新系统设置
    /// </summary>
    /// <param name="dto">创建系统设置数据传输对象，包含设置键、设置值、描述等设置信息</param>
    /// <returns>包含新系统设置ID的结果对象，成功时返回设置ID，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result<long>> CreateAsync(SettingCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查设置键是否已存在
            var exists = await _settingRepository.GetFirstAsync(s => s.SettingKey == dto.SettingKey && s.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"设置键 {dto.SettingKey} 已存在");

            var setting = dto.Adapt<Setting>();

            var result = await _settingRepository.CreateAsync(setting);
            var response = result > 0 ? Result<long>.Ok(setting.Id) : Result<long>.Fail("创建系统设置失败");

            _operLog?.LogCreate("Setting", setting.Id.ToString(), "Routine.SettingView", 
                dto, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("创建系统设置成功，ID: {Id}, 键: {Key}", setting.Id, setting.SettingKey);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建系统设置失败");
            return Result<long>.Fail($"创建系统设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新系统设置信息
    /// </summary>
    /// <param name="dto">更新系统设置数据传输对象，必须包含设置ID和要更新的字段信息</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如设置不存在、设置不允许修改）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、变更内容、操作时间、请求参数、执行耗时等信息
    /// 注意：不可编辑的设置（IsEditable != 0）不允许修改
    /// </remarks>
    public async Task<Result> UpdateAsync(SettingUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var setting = await _settingRepository.GetByIdAsync(dto.Id);
            if (setting == null || setting.IsDeleted == 1)
                return Result.Fail("系统设置不存在");

            // 检查是否可编辑（0=是，1=否）
            if (setting.IsEditable != 0)
                return Result.Fail("该设置不允许修改");

            // 保存旧值用于记录变更（完整对象）
            var oldSetting = setting.Adapt<SettingUpdateDto>();

            // 检查设置键是否被其他记录使用
            if (setting.SettingKey != dto.SettingKey)
            {
                var exists = await _settingRepository.GetFirstAsync(s => s.SettingKey == dto.SettingKey && s.Id != dto.Id && s.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"设置键 {dto.SettingKey} 已被其他设置使用");
            }

            dto.Adapt(setting);

            var result = await _settingRepository.UpdateAsync(setting);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldSetting.SettingKey != dto.SettingKey) changeList.Add($"SettingKey: {oldSetting.SettingKey} -> {dto.SettingKey}");
            if (oldSetting.SettingValue != dto.SettingValue) changeList.Add($"SettingValue: {oldSetting.SettingValue ?? "null"} -> {dto.SettingValue ?? "null"}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新系统设置失败");

            _operLog?.LogUpdate("Setting", dto.Id.ToString(), "Routine.SettingView", changes, dto, oldSetting, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("更新系统设置成功，ID: {Id}", setting.Id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新系统设置失败");
            return Result.Fail($"更新系统设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除系统设置
    /// </summary>
    /// <param name="id">系统设置ID，必须大于0</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如设置不存在、内置设置不允许删除）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 注意：内置设置（IsBuiltin == 0）不允许删除
    /// </remarks>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var setting = await _settingRepository.GetByIdAsync(id);
            if (setting == null || setting.IsDeleted == 1)
                return Result.Fail("系统设置不存在");

            // 检查是否为内置设置（0=是，1=否）
            if (setting.IsBuiltin == 0)
                return Result.Fail("内置设置不允许删除");

            var result = await _settingRepository.DeleteAsync(id);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除系统设置失败");

            _operLog?.LogDelete("Setting", id.ToString(), "Routine.SettingView", 
                new { Id = id, SettingKey = setting.SettingKey }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("删除系统设置成功，ID: {Id}", id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除系统设置失败");
            return Result.Fail($"删除系统设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除系统设置
    /// </summary>
    /// <param name="ids">系统设置ID列表</param>
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
                var setting = await _settingRepository.GetByIdAsync(id);
                if (setting == null || setting.IsDeleted == 1 || setting.IsBuiltin == 0)
                {
                    failCount++;
                    continue;
                }

                var result = await _settingRepository.DeleteAsync(id);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("Setting", id.ToString(), "Routine.SettingView", 
                        new { Id = id, SettingKey = setting.SettingKey }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除系统设置完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除系统设置失败");
            return Result.Fail($"批量删除系统设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出系统设置到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的系统设置</param>
    /// <param name="sheetName">工作表名称，可选，默认为 "Settings"</param>
    /// <param name="fileName">文件名，可选，默认为 "系统设置导出_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容的结果对象，成功时返回文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于导出，不会记录操作日志
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(SettingQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var condition = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<Setting>().And(x => x.IsDeleted == 0).ToExpression();
            var settings = await _settingRepository.AsQueryable().Where(condition).OrderBy(s => s.OrderNum).ToListAsync();
            var settingDtos = settings.Adapt<List<SettingDto>>();
            sheetName ??= "Settings";
            fileName ??= $"系统设置导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(settingDtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {settingDtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出系统设置Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出系统设置 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选，默认为 "Settings"</param>
    /// <param name="fileName">文件名，可选，默认为 "系统设置导入模板_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容的结果对象，成功时返回文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于导出模板，不会记录操作日志
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "Settings";
        fileName ??= $"系统设置导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<SettingDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入系统设置
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选，默认为 "Settings"</param>
    /// <returns>包含成功和失败数量的结果对象，成功时返回导入统计信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "Settings";
            var dtos = ExcelHelper.ImportFromExcel<SettingDto>(fileStream, sheetName);
            
            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.SettingKey)) { fail++; continue; }
                    
                    // 检查设置键是否已存在
                    var existing = await _settingRepository.GetFirstAsync(s => s.SettingKey == dto.SettingKey && s.IsDeleted == 0);
                    if (existing == null)
                    {
                        var setting = dto.Adapt<Setting>();
                        var result = await _settingRepository.CreateAsync(setting);
                        if (result > 0)
                        {
                            success++;
                            _appLog.Information("导入系统设置成功，键: {Key}", setting.SettingKey);
                        }
                        else
                        {
                            fail++;
                        }
                    }
                    else
                    {
                        // 如果存在则更新（但需要检查是否可编辑）
                        if (existing.IsEditable == 0)
                        {
                            dto.Adapt(existing);
                            var result = await _settingRepository.UpdateAsync(existing);
                            if (result > 0)
                            {
                                success++;
                                _appLog.Information("更新系统设置成功，键: {Key}", existing.SettingKey);
                            }
                            else
                            {
                                fail++;
                            }
                        }
                        else
                        {
                            fail++;
                            _appLog.Warning("导入系统设置失败：设置键 {Key} 已存在且不可编辑", dto.SettingKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    fail++;
                    _appLog.Error(ex, "导入系统设置失败，键: {Key}", dto.SettingKey);
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("Setting", success, "Routine.SettingView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入系统设置Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<Setting, bool>> QueryExpression(SettingQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Setting>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.SettingKey.Contains(query.Keywords!) || 
                                                              (x.SettingValue != null && x.SettingValue.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.SettingKey), x => x.SettingKey.Contains(query.SettingKey!))
            .AndIF(!string.IsNullOrEmpty(query.Category), x => x.Category == query.Category)
            .ToExpression();
    }
}
