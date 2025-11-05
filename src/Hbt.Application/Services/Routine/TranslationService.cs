// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：TranslationService.cs
// 创建时间：2025-01-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：翻译服务实现
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

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
/// 翻译服务实现
/// 实现翻译相关的业务逻辑
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly AppLogManager _appLog;

    public TranslationService(
        IBaseRepository<Translation> translationRepository,
        IBaseRepository<Language> languageRepository,
        AppLogManager appLog)
    {
        _translationRepository = translationRepository;
        _languageRepository = languageRepository;
        _appLog = appLog;
    }

    public async Task<Result<PagedResult<TranslationDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null)
    {
        _appLog.Information("开始查询翻译列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            pageIndex, pageSize, keyword ?? string.Empty);

        try
        {
            System.Linq.Expressions.Expression<Func<Translation, bool>>? condition = null;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = t => t.TranslationKey.Contains(keyword) || t.TranslationValue.Contains(keyword) ||
                               (t.Module != null && t.Module.Contains(keyword));
            }

            var result = await _translationRepository.GetListAsync(condition, pageIndex, pageSize);
            var translationDtos = result.Items.Adapt<List<TranslationDto>>();

            var pagedResult = new PagedResult<TranslationDto>
            {
                Items = translationDtos,
                TotalNum = result.TotalNum,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Result<PagedResult<TranslationDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询翻译数据失败");
            return Result<PagedResult<TranslationDto>>.Fail($"查询翻译数据失败: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<TranslationDto>>> GetListAsync(TranslationQueryDto query)
    {
        _appLog.Information("开始高级查询翻译列表");

        try
        {
            // 直接按 LanguageCode 过滤
            var condition = SqlSugar.Expressionable.Create<Translation>()
                .And(x => x.IsDeleted == 0)
                .AndIF(!string.IsNullOrEmpty(query.LanguageCode), x => x.LanguageCode == query.LanguageCode!)
                .AndIF(!string.IsNullOrEmpty(query.TranslationKey), x => x.TranslationKey.Contains(query.TranslationKey!))
                .AndIF(!string.IsNullOrEmpty(query.Module), x => x.Module == query.Module)
                .ToExpression();
            var result = await _translationRepository.GetListAsync(condition, query.PageIndex, query.PageSize);
            var translationDtos = result.Items.Adapt<List<TranslationDto>>();

            var pagedResult = new PagedResult<TranslationDto>
            {
                Items = translationDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<TranslationDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询翻译数据失败");
            return Result<PagedResult<TranslationDto>>.Fail($"高级查询翻译数据失败: {ex.Message}");
        }
    }

    public async Task<Result<TranslationDto>> GetByIdAsync(long id)
    {
        var translation = await _translationRepository.GetByIdAsync(id);
        if (translation == null)
            return Result<TranslationDto>.Fail("翻译不存在");

        var translationDto = translation.Adapt<TranslationDto>();
        return Result<TranslationDto>.Ok(translationDto);
    }

    public async Task<Result<string>> GetValueAsync(string languageCode, string translationKey)
    {
        try
        {
            // 按语言代码与翻译键获取翻译
            var translation = await _translationRepository.GetFirstAsync(
                t => t.LanguageCode == languageCode && t.TranslationKey == translationKey && t.IsDeleted == 0);
            if (translation == null)
                return Result<string>.Fail("翻译不存在");

            return Result<string>.Ok(translation.TranslationValue);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取翻译值失败");
            return Result<string>.Fail($"获取翻译值失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(TranslationCreateDto dto)
    {
        try
        {
            // 验证语言是否存在（按代码）
            var language = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.IsDeleted == 0);
            if (language == null)
                return Result<long>.Fail("关联的语言不存在");

            // 检查同一语言下翻译键是否唯一
            var exists = await _translationRepository.GetFirstAsync(
                t => t.LanguageCode == language.LanguageCode && t.TranslationKey == dto.TranslationKey && t.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"语言 {dto.LanguageCode} 下已存在翻译键 {dto.TranslationKey}");

            var translation = dto.Adapt<Translation>();
            // 设置语言代码
            translation.LanguageCode = dto.LanguageCode;

            var result = await _translationRepository.CreateAsync(translation);
            if (result > 0)
            {
                _appLog.Information("创建翻译成功，ID: {Id}, 语言: {LanguageCode}, 键: {Key}",
                    translation.Id, translation.LanguageCode, translation.TranslationKey);
                return Result<long>.Ok(translation.Id);
            }

            return Result<long>.Fail("创建翻译失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建翻译失败");
            return Result<long>.Fail($"创建翻译失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(TranslationUpdateDto dto)
    {
        try
        {
            var translation = await _translationRepository.GetByIdAsync(dto.Id);
            if (translation == null || translation.IsDeleted == 1)
                return Result.Fail("翻译不存在");

            // 目标语言（按代码）
            var targetLanguage = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.IsDeleted == 0);
            if (targetLanguage == null)
                return Result.Fail("关联的语言不存在");

            // 检查翻译键在同一语言下是否被其他记录使用
            var exists = await _translationRepository.GetFirstAsync(
                t => t.LanguageCode == targetLanguage.LanguageCode && t.TranslationKey == dto.TranslationKey && t.Id != dto.Id && t.IsDeleted == 0);
            if (exists != null)
                return Result.Fail($"语言 {dto.LanguageCode} 下已存在翻译键 {dto.TranslationKey}");

            dto.Adapt(translation);
            // 确保语言代码被正确更新
            translation.LanguageCode = dto.LanguageCode;

            var result = await _translationRepository.UpdateAsync(translation);
            if (result > 0)
            {
                _appLog.Information("更新翻译成功，ID: {Id}", translation.Id);
                return Result.Ok();
            }

            return Result.Fail("更新翻译失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新翻译失败");
            return Result.Fail($"更新翻译失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var result = await _translationRepository.DeleteAsync(id);
            if (result > 0)
            {
                _appLog.Information("删除翻译成功，ID: {Id}", id);
                return Result.Ok();
            }

            return Result.Fail("删除翻译失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除翻译失败");
            return Result.Fail($"删除翻译失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取模块的所有翻译（按翻译键转置，包含所有语言）
    /// 返回格式：{翻译键: {语言代码: 翻译值}}
    /// </summary>
    public async Task<Result<Dictionary<string, Dictionary<string, string>>>> GetTranslationsByModuleAsync(string? module = null)
    {
        try
        {
            _appLog.Information("开始获取模块翻译，模块: {Module}", module ?? "全部");
            Debug.WriteLine($"[TranslationService] 开始获取模块翻译，模块: {module ?? "全部"}");

            Debug.WriteLine("[TranslationService] 步骤1: 开始查询所有语言...");
            _appLog.Information("[TranslationService] 步骤1: 查询所有启用的语言");
            
            // 获取所有语言
            var languages = await _languageRepository.GetListAsync(
                l => l.IsDeleted == 0 && l.LanguageStatus == 0,
                1,
                int.MaxValue
            );

            Debug.WriteLine($"[TranslationService] 步骤1完成: 获取到 {languages.Items.Count} 种语言");
            _appLog.Information("[TranslationService] 获取到 {Count} 种语言", languages.Items.Count);

            Debug.WriteLine("[TranslationService] 步骤2: 开始构建翻译查询条件...");
            
            // 构建查询条件
        var condition = SqlSugar.Expressionable.Create<Translation>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
                .AndIF(!string.IsNullOrEmpty(module), x => x.Module == module)
                .ToExpression();

            Debug.WriteLine("[TranslationService] 步骤2完成: 查询条件已构建");
            
            Debug.WriteLine("[TranslationService] 步骤3: 开始查询所有翻译...");
            _appLog.Information("[TranslationService] 步骤3: 查询所有翻译数据");

            // 获取所有翻译
            var translations = await _translationRepository.GetListAsync(condition, 1, int.MaxValue);

            Debug.WriteLine($"[TranslationService] 步骤3完成: 获取到 {translations.Items.Count} 条翻译记录");
            _appLog.Information("[TranslationService] 获取到 {Count} 条翻译记录", translations.Items.Count);

            Debug.WriteLine("[TranslationService] 步骤4: 开始构建语言代码映射...");

            // 构建启用语言代码集合
            var enabledLanguageCodes = new HashSet<string>(languages.Items.Select(l => l.LanguageCode));

            Debug.WriteLine($"[TranslationService] 步骤4完成: 语言映射已构建，包含 {enabledLanguageCodes.Count} 个语言");

            Debug.WriteLine("[TranslationService] 步骤5: 开始转置翻译数据...");
            _appLog.Information("[TranslationService] 步骤5: 转置翻译数据");

            // 转置：按翻译键分组
            var result = translations.Items
                .GroupBy(t => t.TranslationKey)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(t => enabledLanguageCodes.Contains(t.LanguageCode))
                          .ToDictionary(
                              t => t.LanguageCode,
                              t => t.TranslationValue
                          )
                );

            Debug.WriteLine($"[TranslationService] 步骤5完成: 转置完成，共 {result.Count} 个翻译键");
            _appLog.Information("获取模块翻译完成，共 {Count} 个翻译键", result.Count);
            return Result<Dictionary<string, Dictionary<string, string>>>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取模块翻译失败");
            return Result<Dictionary<string, Dictionary<string, string>>>.Fail($"获取模块翻译失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取多个翻译键的翻译值（转置后）
    /// </summary>
    public async Task<Result<Dictionary<string, Dictionary<string, string>>>> GetTranslationsByKeysAsync(List<string> translationKeys)
    {
        try
        {
            _appLog.Information("开始获取翻译键翻译，共 {Count} 个键", translationKeys.Count);

            // 获取所有语言
            var languages = await _languageRepository.GetListAsync(
                l => l.IsDeleted == 0 && l.LanguageStatus == 0,
                1,
                int.MaxValue
            );

            // 获取指定的翻译
        var condition = SqlSugar.Expressionable.Create<Translation>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
                .And(x => translationKeys.Contains(x.TranslationKey))
                .ToExpression();

            var translations = await _translationRepository.GetListAsync(condition, 1, int.MaxValue);

            // 构建启用语言代码集合
            var enabledLanguageCodes = new HashSet<string>(languages.Items.Select(l => l.LanguageCode));

            // 转置：按翻译键分组
            var result = translations.Items
                .GroupBy(t => t.TranslationKey)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(t => enabledLanguageCodes.Contains(t.LanguageCode))
                          .ToDictionary(
                              t => t.LanguageCode,
                              t => t.TranslationValue
                          )
                );

            _appLog.Information("获取翻译键翻译完成，共 {Count} 个翻译键", result.Count);
            return Result<Dictionary<string, Dictionary<string, string>>>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取翻译键翻译失败");
            return Result<Dictionary<string, Dictionary<string, string>>>.Fail($"获取翻译键翻译失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量获取翻译键的所有语言翻译值
    /// 返回格式：{语言代码: 翻译值}
    /// </summary>
    public async Task<Result<Dictionary<string, string>>> GetTranslationValuesAsync(string translationKey)
    {
        try
        {
            _appLog.Information("开始获取翻译键的所有语言翻译，键: {Key}", translationKey);

            // 获取该翻译键的所有翻译
            var translations = await _translationRepository.GetListAsync(
                t => t.IsDeleted == 0 && t.TranslationKey == translationKey,
                1,
                int.MaxValue
            );

            if (!translations.Items.Any())
            {
                return Result<Dictionary<string, string>>.Ok(new Dictionary<string, string>());
            }

            // 启用语言代码集合
            var enabledLanguageCodes2 = new HashSet<string>((await _languageRepository.GetListAsync(
                l => l.IsDeleted == 0 && l.LanguageStatus == 0,
                1,
                int.MaxValue
            )).Items.Select(l => l.LanguageCode));

            // 构建结果：{语言代码: 翻译值}
            var result = translations.Items
                .Where(t => enabledLanguageCodes2.Contains(t.LanguageCode))
                .ToDictionary(t => t.LanguageCode, t => t.TranslationValue);

            _appLog.Information("获取翻译完成，共 {Count} 种语言", result.Count);
            return Result<Dictionary<string, string>>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取翻译失败");
            return Result<Dictionary<string, string>>.Fail($"获取翻译失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<Translation, bool>> QueryExpression(TranslationQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Translation>()
            .And(x => x.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.TranslationKey), x => x.TranslationKey.Contains(query.TranslationKey!))
            .AndIF(!string.IsNullOrEmpty(query.Module), x => x.Module == query.Module)
            .ToExpression();
    }
}
