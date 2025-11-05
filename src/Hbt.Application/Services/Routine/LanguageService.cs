//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : LanguageService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 语言服务实现
//===================================================================

using System.Diagnostics;
using System.Linq.Expressions;
using Hbt.Application.Dtos.Routine;
using Hbt.Common.Logging;
using Hbt.Common.Results;
using Hbt.Domain.Entities.Routine;
using Hbt.Domain.Repositories;
using Mapster;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 语言服务实现
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly AppLogManager _appLog;

    public LanguageService(IBaseRepository<Language> languageRepository, AppLogManager appLog)
    {
        _languageRepository = languageRepository;
        _appLog = appLog;
    }

    public async Task<Result<PagedResult<LanguageDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null)
    {
        _appLog.Information("开始查询语言列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            pageIndex, pageSize, keyword ?? string.Empty);

        try
        {
            System.Linq.Expressions.Expression<Func<Language, bool>>? condition = null;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = l => l.LanguageCode.Contains(keyword) || l.LanguageName.Contains(keyword) ||
                               (l.NativeName != null && l.NativeName.Contains(keyword));
            }

            var result = await _languageRepository.GetListAsync(condition, pageIndex, pageSize);
            var languageDtos = result.Items.Adapt<List<LanguageDto>>();

            _appLog.Information("数据库查询完成，返回 {Count} 条语言记录，总数: {TotalNum}",
                languageDtos.Count, result.TotalNum);

            var pagedResult = new PagedResult<LanguageDto>
            {
                Items = languageDtos,
                TotalNum = result.TotalNum,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Result<PagedResult<LanguageDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询语言数据失败");
            return Result<PagedResult<LanguageDto>>.Fail($"查询语言数据失败: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<LanguageDto>>> GetListAsync(LanguageQueryDto query)
    {
        _appLog.Information("开始高级查询语言列表");

        try
        {
            var condition = QueryExpression(query);
            var result = await _languageRepository.GetListAsync(condition, query.PageIndex, query.PageSize);
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

    public async Task<Result<LanguageDto>> GetByIdAsync(long id)
    {
        var language = await _languageRepository.GetByIdAsync(id);
        if (language == null)
            return Result<LanguageDto>.Fail("语言不存在");

        var languageDto = language.Adapt<LanguageDto>();
        return Result<LanguageDto>.Ok(languageDto);
    }

    public async Task<Result<LanguageDto>> GetByCodeAsync(string languageCode)
    {
        var language = await _languageRepository.GetFirstAsync(l => l.LanguageCode == languageCode);
        if (language == null)
            return Result<LanguageDto>.Fail("语言不存在");

        var languageDto = language.Adapt<LanguageDto>();
        return Result<LanguageDto>.Ok(languageDto);
    }

    public async Task<Result<long>> CreateAsync(LanguageCreateDto dto)
    {
        try
        {
            // 检查语言代码是否已存在
            var exists = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"语言代码 {dto.LanguageCode} 已存在");

            var language = dto.Adapt<Language>();

            var result = await _languageRepository.CreateAsync(language);
            if (result > 0)
            {
                _appLog.Information("创建语言成功，ID: {Id}, 代码: {Code}", language.Id, language.LanguageCode);
                return Result<long>.Ok(language.Id);
            }

            return Result<long>.Fail("创建语言失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建语言失败");
            return Result<long>.Fail($"创建语言失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(LanguageUpdateDto dto)
    {
        try
        {
            var language = await _languageRepository.GetByIdAsync(dto.Id);
            if (language == null || language.IsDeleted == 1)
                return Result.Fail("语言不存在");

            // 检查语言代码是否已被其他记录使用
            var exists = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.Id != dto.Id && l.IsDeleted == 0);
            if (exists != null)
                return Result.Fail($"语言代码 {dto.LanguageCode} 已被其他语言使用");

            dto.Adapt(language);

            var result = await _languageRepository.UpdateAsync(language);
            if (result > 0)
            {
                _appLog.Information("更新语言成功，ID: {Id}", language.Id);
                return Result.Ok();
            }

            return Result.Fail("更新语言失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新语言失败");
            return Result.Fail($"更新语言失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var language = await _languageRepository.GetByIdAsync(id);
            if (language == null || language.IsDeleted == 1)
                return Result.Fail("语言不存在");

            // 检查是否为内置语言（0=是，1=否）
            if (language.IsBuiltin == 0)
                return Result.Fail("内置语言不允许删除");

            var result = await _languageRepository.DeleteAsync(id);
            if (result > 0)
            {
                _appLog.Information("删除语言成功，ID: {Id}", id);
                return Result.Ok();
            }

            return Result.Fail("删除语言失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除语言失败");
            return Result.Fail($"删除语言失败: {ex.Message}");
        }
    }

    public async Task<Result> StatusAsync(long id, int status)
    {
        try
        {
            var result = await _languageRepository.StatusAsync(id, status);
            if (result > 0)
            {
                _appLog.Information("修改语言状态成功，ID: {Id}, 状态: {Status}", id, status);
                return Result.Ok();
            }

            return Result.Fail("修改语言状态失败");
        }
        catch (Exception ex)
        {
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

            Debug.WriteLine("[LanguageService] 开始调用 GetListAsync...");
            
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
                Value = l.LanguageCode,
                Label = l.LanguageName,
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
    /// 构建查询表达式
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>返回查询表达式</returns>
    private Expression<Func<Language, bool>> QueryExpression(LanguageQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Language>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
            .AndIF(!string.IsNullOrEmpty(query.LanguageCode), x => x.LanguageCode.Contains(query.LanguageCode!))
            .AndIF(!string.IsNullOrEmpty(query.LanguageName), x => x.LanguageName.Contains(query.LanguageName!))
            .AndIF(query.LanguageStatus.HasValue, x => x.LanguageStatus == query.LanguageStatus!.Value)
            .ToExpression();
    }
}
