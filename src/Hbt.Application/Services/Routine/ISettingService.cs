//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ISettingService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 系统设置服务接口
//===================================================================

using Hbt.Application.Dtos.Routine;
using Hbt.Common.Results;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 系统设置服务接口
/// </summary>
public interface ISettingService
{
    /// <summary>
    /// 分页查询系统设置列表
    /// </summary>
    Task<Result<PagedResult<SettingDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);

    /// <summary>
    /// 高级查询系统设置列表
    /// </summary>
    Task<Result<PagedResult<SettingDto>>> GetListAsync(SettingQueryDto query);

    /// <summary>
    /// 根据ID获取系统设置
    /// </summary>
    Task<Result<SettingDto>> GetByIdAsync(long id);

    /// <summary>
    /// 根据设置键获取系统设置
    /// </summary>
    Task<Result<SettingDto>> GetByKeyAsync(string settingKey);

    /// <summary>
    /// 根据分类获取系统设置列表
    /// </summary>
    Task<Result<List<SettingDto>>> GetByCategoryAsync(string category);

    /// <summary>
    /// 创建系统设置
    /// </summary>
    Task<Result<long>> CreateAsync(SettingCreateDto dto);

    /// <summary>
    /// 更新系统设置
    /// </summary>
    Task<Result> UpdateAsync(SettingUpdateDto dto);

    /// <summary>
    /// 删除系统设置
    /// </summary>
    Task<Result> DeleteAsync(long id);
}
