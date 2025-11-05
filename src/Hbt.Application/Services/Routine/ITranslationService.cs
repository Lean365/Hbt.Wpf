//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ITranslationService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-01-20
// 版本号 : 1.0
// 描述    : 翻译服务接口
//===================================================================

using Hbt.Application.Dtos.Routine;
using Hbt.Common.Results;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 翻译服务接口
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// 分页查询翻译列表
    /// </summary>
    Task<Result<PagedResult<TranslationDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);

    /// <summary>
    /// 高级查询翻译列表
    /// </summary>
    Task<Result<PagedResult<TranslationDto>>> GetListAsync(TranslationQueryDto query);

    /// <summary>
    /// 根据ID获取翻译
    /// </summary>
    Task<Result<TranslationDto>> GetByIdAsync(long id);

    /// <summary>
    /// 根据语言代码和翻译键获取翻译值
    /// </summary>
    Task<Result<string>> GetValueAsync(string languageCode, string translationKey);

    /// <summary>
    /// 创建翻译
    /// </summary>
    Task<Result<long>> CreateAsync(TranslationCreateDto dto);

    /// <summary>
    /// 更新翻译
    /// </summary>
    Task<Result> UpdateAsync(TranslationUpdateDto dto);

    /// <summary>
    /// 删除翻译
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 获取模块的所有翻译（按翻译键转置，包含所有语言）
    /// 返回格式：{翻译键: {语言代码: 翻译值}}
    /// </summary>
    Task<Result<Dictionary<string, Dictionary<string, string>>>> GetTranslationsByModuleAsync(string? module = null);

    /// <summary>
    /// 获取多个翻译键的翻译值（转置后）
    /// </summary>
    Task<Result<Dictionary<string, Dictionary<string, string>>>> GetTranslationsByKeysAsync(List<string> translationKeys);

    /// <summary>
    /// 批量获取翻译键的所有语言翻译值
    /// </summary>
    Task<Result<Dictionary<string, string>>> GetTranslationValuesAsync(string translationKey);
}
