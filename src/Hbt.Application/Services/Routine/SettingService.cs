//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : SettingService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 系统设置服务实现
//===================================================================

using System.Linq.Expressions;
using Hbt.Application.Dtos.Routine;
using Hbt.Common.Logging;
using Hbt.Common.Results;
using Hbt.Domain.Entities.Routine;
using Hbt.Domain.Repositories;
using Mapster;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 系统设置服务实现
/// </summary>
public class SettingService : ISettingService
{
    private readonly IBaseRepository<Setting> _settingRepository;
    private readonly AppLogManager _appLog;

    public SettingService(IBaseRepository<Setting> settingRepository, AppLogManager appLog)
    {
        _settingRepository = settingRepository;
        _appLog = appLog;
    }

    public async Task<Result<PagedResult<SettingDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null)
    {
        _appLog.Information("开始查询系统设置列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            pageIndex, pageSize, keyword ?? string.Empty);

        try
        {
            System.Linq.Expressions.Expression<Func<Setting, bool>>? condition = null;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = s => s.SettingKey.Contains(keyword) || 
                               s.SettingValue.Contains(keyword) ||
                               (s.SettingDescription != null && s.SettingDescription.Contains(keyword));
            }

            var result = await _settingRepository.GetListAsync(condition, pageIndex, pageSize);
            var settingDtos = result.Items.Adapt<List<SettingDto>>();

            var pagedResult = new PagedResult<SettingDto>
            {
                Items = settingDtos,
                TotalNum = result.TotalNum,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Result<PagedResult<SettingDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询系统设置数据失败");
            return Result<PagedResult<SettingDto>>.Fail($"查询系统设置数据失败: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<SettingDto>>> GetListAsync(SettingQueryDto query)
    {
        _appLog.Information("开始高级查询系统设置列表");

        try
        {
            var condition = QueryExpression(query);
            var result = await _settingRepository.GetListAsync(condition, query.PageIndex, query.PageSize);
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

    public async Task<Result<SettingDto>> GetByIdAsync(long id)
    {
        var setting = await _settingRepository.GetByIdAsync(id);
        if (setting == null)
            return Result<SettingDto>.Fail("系统设置不存在");

        var settingDto = setting.Adapt<SettingDto>();
        return Result<SettingDto>.Ok(settingDto);
    }

    public async Task<Result<SettingDto>> GetByKeyAsync(string settingKey)
    {
        var setting = await _settingRepository.GetFirstAsync(s => s.SettingKey == settingKey && s.IsDeleted == 0);
        if (setting == null)
            return Result<SettingDto>.Fail("系统设置不存在");

        var settingDto = setting.Adapt<SettingDto>();
        return Result<SettingDto>.Ok(settingDto);
    }

    public async Task<Result<List<SettingDto>>> GetByCategoryAsync(string category)
    {
        var settings = await _settingRepository.GetListAsync(s => s.Category == category && s.IsDeleted == 0, 1, int.MaxValue);
        var settingDtos = settings.Items.Adapt<List<SettingDto>>();
        return Result<List<SettingDto>>.Ok(settingDtos);
    }

    public async Task<Result<long>> CreateAsync(SettingCreateDto dto)
    {
        try
        {
            // 检查设置键是否已存在
            var exists = await _settingRepository.GetFirstAsync(s => s.SettingKey == dto.SettingKey && s.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"设置键 {dto.SettingKey} 已存在");

            var setting = dto.Adapt<Setting>();

            var result = await _settingRepository.CreateAsync(setting);
            if (result > 0)
            {
                _appLog.Information("创建系统设置成功，ID: {Id}, 键: {Key}", setting.Id, setting.SettingKey);
                return Result<long>.Ok(setting.Id);
            }

            return Result<long>.Fail("创建系统设置失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建系统设置失败");
            return Result<long>.Fail($"创建系统设置失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(SettingUpdateDto dto)
    {
        try
        {
            var setting = await _settingRepository.GetByIdAsync(dto.Id);
            if (setting == null || setting.IsDeleted == 1)
                return Result.Fail("系统设置不存在");

            // 检查是否可编辑（0=是，1=否）
            if (setting.IsEditable != 0)
                return Result.Fail("该设置不允许修改");

            dto.Adapt(setting);

            var result = await _settingRepository.UpdateAsync(setting);
            if (result > 0)
            {
                _appLog.Information("更新系统设置成功，ID: {Id}", setting.Id);
                return Result.Ok();
            }

            return Result.Fail("更新系统设置失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新系统设置失败");
            return Result.Fail($"更新系统设置失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var setting = await _settingRepository.GetByIdAsync(id);
            if (setting == null || setting.IsDeleted == 1)
                return Result.Fail("系统设置不存在");

            // 检查是否为内置设置（0=是，1=否）
            if (setting.IsBuiltin == 0)
                return Result.Fail("内置设置不允许删除");

            var result = await _settingRepository.DeleteAsync(id);
            if (result > 0)
            {
                _appLog.Information("删除系统设置成功，ID: {Id}", id);
                return Result.Ok();
            }

            return Result.Fail("删除系统设置失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除系统设置失败");
            return Result.Fail($"删除系统设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<Setting, bool>> QueryExpression(SettingQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Setting>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
            .AndIF(!string.IsNullOrEmpty(query.SettingKey), x => x.SettingKey.Contains(query.SettingKey!))
            .AndIF(!string.IsNullOrEmpty(query.Category), x => x.Category == query.Category)
            .ToExpression();
    }
}
