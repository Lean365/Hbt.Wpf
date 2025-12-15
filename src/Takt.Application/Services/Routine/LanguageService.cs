// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：LanguageService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：语言服务实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Mapster;
using System.Diagnostics;
using System.Linq.Expressions;
using Takt.Application.Dtos.Routine;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 语言服务实现
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    /// <summary>
    /// 初始化语言服务实例
    /// </summary>
    /// <param name="languageRepository">语言仓储接口</param>
    /// <param name="appLog">应用日志管理器</param>
    /// <param name="operLog">操作日志管理器（可选）</param>
    public LanguageService(IBaseRepository<Language> languageRepository, AppLogManager appLog, OperLogManager? operLog = null)
    {
        _languageRepository = languageRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询语言列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、语言代码等筛选条件</param>
    /// <returns>分页语言列表</returns>
    public async Task<Result<PagedResult<LanguageDto>>> GetListAsync(LanguageQueryDto query)
    {
        _appLog.Information("开始查询语言列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);

            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<Language, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "languagecode":
                        orderByExpression = l => l.LanguageCode;
                        break;
                    case "languagename":
                        orderByExpression = l => l.LanguageName;
                        break;
                    case "createdtime":
                        orderByExpression = l => l.CreatedTime;
                        break;
                    default:
                        orderByExpression = l => l.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = l => l.CreatedTime; // 默认按创建时间倒序
            }

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }

            // 使用真实的数据库查询
            var result = await _languageRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var languageDtos = result.Items.Adapt<List<LanguageDto>>();

            var pagedResult = new PagedResult<LanguageDto>
            {
                Items = languageDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<LanguageDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询语言数据失败");
            return Result<PagedResult<LanguageDto>>.Fail($"高级查询语言数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取语言信息
    /// </summary>
    /// <param name="id">语言ID</param>
    /// <returns>返回语言信息，如果不存在则返回失败结果</returns>
    public async Task<Result<LanguageDto>> GetByIdAsync(long id)
    {
        var language = await _languageRepository.GetByIdAsync(id);
        if (language == null)
            return Result<LanguageDto>.Fail("语言不存在");

        var languageDto = language.Adapt<LanguageDto>();
        return Result<LanguageDto>.Ok(languageDto);
    }

    /// <summary>
    /// 根据语言代码获取语言信息
    /// </summary>
    /// <param name="languageCode">语言代码（如：zh-CN, en-US）</param>
    /// <returns>返回语言信息，如果不存在则返回失败结果</returns>
    public async Task<Result<LanguageDto>> GetByCodeAsync(string languageCode)
    {
        var language = await _languageRepository.GetFirstAsync(l => l.LanguageCode == languageCode);
        if (language == null)
            return Result<LanguageDto>.Fail("语言不存在");

        var languageDto = language.Adapt<LanguageDto>();
        return Result<LanguageDto>.Ok(languageDto);
    }

    /// <summary>
    /// 创建新语言
    /// </summary>
    /// <param name="dto">语言创建数据传输对象</param>
    /// <returns>返回创建成功的语言ID，如果语言代码已存在或创建失败则返回失败结果</returns>
    public async Task<Result<long>> CreateAsync(LanguageCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查语言代码是否已存在
            var exists = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"语言代码 {dto.LanguageCode} 已存在");

            var language = dto.Adapt<Language>();

            var result = await _languageRepository.CreateAsync(language);
            var response = result > 0 ? Result<long>.Ok(language.Id) : Result<long>.Fail("创建语言失败");

            _operLog?.LogCreate("Language", language.Id.ToString(), "Routine.LanguageView", 
                dto, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("创建语言成功，ID: {Id}, 代码: {Code}", language.Id, language.LanguageCode);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建语言失败");
            return Result<long>.Fail($"创建语言失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新语言信息
    /// </summary>
    /// <param name="dto">语言更新数据传输对象</param>
    /// <returns>返回操作结果，如果语言不存在、语言代码已被其他语言使用或更新失败则返回失败结果</returns>
    public async Task<Result> UpdateAsync(LanguageUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var language = await _languageRepository.GetByIdAsync(dto.Id);
            if (language == null || language.IsDeleted == 1)
                return Result.Fail("语言不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldLanguage = language.Adapt<LanguageUpdateDto>();

            // 检查语言代码是否已被其他记录使用
            var exists = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.Id != dto.Id && l.IsDeleted == 0);
            if (exists != null)
                return Result.Fail($"语言代码 {dto.LanguageCode} 已被其他语言使用");

            dto.Adapt(language);

            var result = await _languageRepository.UpdateAsync(language);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldLanguage.LanguageCode != dto.LanguageCode) changeList.Add($"LanguageCode: {oldLanguage.LanguageCode} -> {dto.LanguageCode}");
            if (oldLanguage.LanguageName != dto.LanguageName) changeList.Add($"LanguageName: {oldLanguage.LanguageName} -> {dto.LanguageName}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新语言失败");

            _operLog?.LogUpdate("Language", dto.Id.ToString(), "Routine.LanguageView", changes, dto, oldLanguage, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("更新语言成功，ID: {Id}", language.Id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新语言失败");
            return Result.Fail($"更新语言失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除语言（软删除）
    /// </summary>
    /// <param name="id">语言ID</param>
    /// <returns>返回操作结果，如果语言不存在、是内置语言或删除失败则返回失败结果</returns>
    /// <remarks>内置语言（IsBuiltin = 0）不允许删除</remarks>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var language = await _languageRepository.GetByIdAsync(id);
            if (language == null || language.IsDeleted == 1)
                return Result.Fail("语言不存在");

            // 检查是否为内置语言（0=是，1=否）
            if (language.IsBuiltin == 0)
                return Result.Fail("内置语言不允许删除");

            var result = await _languageRepository.DeleteAsync(id);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除语言失败");

            _operLog?.LogDelete("Language", id.ToString(), "Routine.LanguageView", 
                new { Id = id, LanguageCode = language.LanguageCode }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("删除语言成功，ID: {Id}", id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除语言失败");
            return Result.Fail($"删除语言失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除语言
    /// </summary>
    /// <param name="ids">语言ID列表</param>
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
                var language = await _languageRepository.GetByIdAsync(id);
                if (language == null || language.IsDeleted == 1 || language.IsBuiltin == 0)
                {
                    failCount++;
                    continue;
                }

                var result = await _languageRepository.DeleteAsync(id);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("Language", id.ToString(), "Routine.LanguageView", 
                        new { Id = id, LanguageCode = language.LanguageCode }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除语言完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除语言失败");
            return Result.Fail($"批量删除语言失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 修改语言状态
    /// </summary>
    /// <param name="id">语言ID</param>
    /// <param name="status">状态值（0=启用，1=禁用）</param>
    /// <returns>返回操作结果，如果修改失败则返回失败结果</returns>
    public async Task<Result> StatusAsync(long id, int status)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = await _languageRepository.StatusAsync(id, status);
            var response = result > 0 ? Result.Ok() : Result.Fail("修改语言状态失败");

            _operLog?.LogUpdate("Language", id.ToString(), "Routine.LanguageView", $"修改状态为 {status}",
                new { Id = id, Status = status }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("修改语言状态成功，ID: {Id}, 状态: {Status}", id, status);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "修改语言状态失败");
            return Result.Fail($"修改语言状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取语言选项列表（用于下拉列表）
    /// </summary>
    /// <param name="includeDisabled">是否包含已禁用的语言</param>
    /// <returns>语言选项列表</returns>
    public async Task<Result<List<LanguageOptionDto>>> OptionAsync(bool includeDisabled = false)
    {
        _appLog.Information("开始获取语言选项列表，包含已禁用: {IncludeDisabled}", includeDisabled);
        Debug.WriteLine($"[LanguageService] 开始获取语言选项列表，包含已禁用: {includeDisabled}");

        try
        {
            // 构建查询条件
            Expression<Func<Language, bool>>? condition = l => l.IsDeleted == 0;

            if (!includeDisabled)
            {
                // 如果不包含已禁用，则只查询启用的语言（LanguageStatus = 0）
                condition = l => l.IsDeleted == 0 && l.LanguageStatus == 0;
            }

            Debug.WriteLine("[LanguageService] 开始调用 GetListAsync..");

            // 查询所有符合条件的语言
            var result = await _languageRepository.GetListAsync(condition, 1, int.MaxValue);

            Debug.WriteLine($"[LanguageService] GetListAsync 返回成功，数量: {result.Items.Count}");
            _appLog.Information("数据库查询成功，获取 {Count} 条语言记录", result.Items.Count);

            // 映射为选项DTO
            var options = result.Items.Select(l => new LanguageOptionDto
            {
                Id = l.Id,
                Code = l.LanguageCode,
                Name = l.LanguageName,
                DataValue = l.LanguageCode,
                DataLabel = l.LanguageName,
                OrderNum = l.OrderNum
            }).OrderBy(l => l.OrderNum).ThenBy(l => l.Code).ToList();

            Debug.WriteLine($"[LanguageService] 成功映射 {options.Count} 个语言选项");
            _appLog.Information("成功获取 {Count} 个语言选项", options.Count);
            return Result<List<LanguageOptionDto>>.Ok(options);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LanguageService] 获取语言选项列表失败: {ex.Message}");
            Debug.WriteLine($"[LanguageService] 异常堆栈: {ex.StackTrace}");
            _appLog.Error(ex, "获取语言选项列表失败");
            return Result<List<LanguageOptionDto>>.Fail($"获取语言选项列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出语言到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的语言</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(LanguageQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<Language>().And(x => x.IsDeleted == 0).ToExpression();
            var languages = await _languageRepository.AsQueryable().Where(where).OrderBy(l => l.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = languages.Adapt<List<LanguageDto>>();
            sheetName ??= "Languages";
            fileName ??= $"语言导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出语言Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出语言 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "Languages";
        fileName ??= $"语言导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<LanguageDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入语言
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "Languages";
            var dtos = ExcelHelper.ImportFromExcel<LanguageDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any())
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.LanguageCode)) { fail++; continue; }
                    var existing = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<Language>();
                        await _languageRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _languageRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("Language", success, "Routine.LanguageView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入语言Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>返回查询表达式</returns>
    private Expression<Func<Language, bool>> QueryExpression(LanguageQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Language>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.LanguageCode.Contains(query.Keywords!) ||
                                                              x.LanguageName.Contains(query.Keywords!) ||
                                                              (x.NativeName != null && x.NativeName.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.LanguageCode), x => x.LanguageCode.Contains(query.LanguageCode!))
            .AndIF(!string.IsNullOrEmpty(query.LanguageName), x => x.LanguageName.Contains(query.LanguageName!))
            .AndIF(query.LanguageStatus.HasValue, x => x.LanguageStatus == query.LanguageStatus!.Value)
            .ToExpression();
    }
}
