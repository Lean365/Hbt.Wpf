//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : IDictionaryTypeService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 字典类型服务接口
//===================================================================

using Hbt.Application.Dtos.Routine;
using Hbt.Common.Results;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 字典类型服务接口
/// </summary>
public interface IDictionaryTypeService
{
    /// <summary>
    /// 分页查询字典类型列表
    /// </summary>
    Task<Result<PagedResult<DictionaryTypeDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);

    /// <summary>
    /// 高级查询字典类型列表
    /// </summary>
    Task<Result<PagedResult<DictionaryTypeDto>>> GetListAsync(DictionaryTypeQueryDto query);

    /// <summary>
    /// 根据ID获取字典类型（包含字典数据）
    /// </summary>
    Task<Result<DictionaryTypeDto>> GetByIdAsync(long id, bool includeData = false);

    /// <summary>
    /// 根据类型代码获取字典类型（包含字典数据）
    /// </summary>
    Task<Result<DictionaryTypeDto>> GetByCodeAsync(string typeCode, bool includeData = false);

    /// <summary>
    /// 创建字典类型
    /// </summary>
    Task<Result<long>> CreateAsync(DictionaryTypeCreateDto dto);

    /// <summary>
    /// 更新字典类型
    /// </summary>
    Task<Result> UpdateAsync(DictionaryTypeUpdateDto dto);

    /// <summary>
    /// 删除字典类型（同时删除关联的字典数据）
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 修改字典类型状态
    /// </summary>
    Task<Result> StatusAsync(long id, int status);
}
