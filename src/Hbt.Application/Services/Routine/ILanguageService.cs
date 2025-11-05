//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ILanguageService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 语言服务接口
//===================================================================

using Hbt.Application.Dtos.Routine;
using Hbt.Common.Results;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 语言服务接口
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// 分页查询语言列表
    /// </summary>
    Task<Result<PagedResult<LanguageDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);

    /// <summary>
    /// 高级查询语言列表
    /// </summary>
    Task<Result<PagedResult<LanguageDto>>> GetListAsync(LanguageQueryDto query);

    /// <summary>
    /// 根据ID获取语言
    /// </summary>
    Task<Result<LanguageDto>> GetByIdAsync(long id);

    /// <summary>
    /// 根据语言代码获取语言
    /// </summary>
    Task<Result<LanguageDto>> GetByCodeAsync(string languageCode);

    /// <summary>
    /// 创建语言
    /// </summary>
    Task<Result<long>> CreateAsync(LanguageCreateDto dto);

    /// <summary>
    /// 更新语言
    /// </summary>
    Task<Result> UpdateAsync(LanguageUpdateDto dto);

    /// <summary>
    /// 删除语言
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 修改语言状态
    /// </summary>
    Task<Result> StatusAsync(long id, int status);

    /// <summary>
    /// 获取语言选项列表（用于下拉列表）
    /// </summary>
    /// <param name="includeDisabled">是否包含已禁用的语言</param>
    /// <returns>语言选项列表</returns>
    Task<Result<List<LanguageOptionDto>>> OptionAsync(bool includeDisabled = false);
}
