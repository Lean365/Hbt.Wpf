//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : IDictionaryDataService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 字典数据服务接口
//===================================================================

using Hbt.Application.Dtos.Routine;
using Hbt.Common.Results;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 字典数据服务接口
/// </summary>
public interface IDictionaryDataService
{
    /// <summary>
    /// 分页查询字典数据列表
    /// </summary>
    Task<Result<PagedResult<DictionaryDataDto>>> GetListAsync(int pageIndex, int pageSize, long? typeId = null, string? keyword = null);

    /// <summary>
    /// 高级查询字典数据列表
    /// </summary>
    Task<Result<PagedResult<DictionaryDataDto>>> GetListAsync(DictionaryDataQueryDto query);

    /// <summary>
    /// 根据字典类型代码获取字典数据列表
    /// </summary>
    Task<Result<List<DictionaryDataDto>>> GetByTypeCodeAsync(string typeCode);

    /// <summary>
    /// 根据ID获取字典数据
    /// </summary>
    Task<Result<DictionaryDataDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建字典数据
    /// </summary>
    Task<Result<long>> CreateAsync(DictionaryDataCreateDto dto);

    /// <summary>
    /// 更新字典数据
    /// </summary>
    Task<Result> UpdateAsync(DictionaryDataUpdateDto dto);

    /// <summary>
    /// 删除字典数据
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除字典数据
    /// </summary>
    Task<Result> DeleteRangeAsync(List<long> ids);
}
