// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：TranslationService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：翻译服务实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Diagnostics;
using System.IO;
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
/// 翻译服务实现
/// 实现翻译相关的业务逻辑
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public TranslationService(
        IBaseRepository<Translation> translationRepository,
        IBaseRepository<Language> languageRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _translationRepository = translationRepository;
        _languageRepository = languageRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询翻译列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、翻译键、语言代码等筛选条件</param>
    /// <returns>分页翻译列表</returns>
    public async Task<Result<PagedResult<TranslationDto>>> GetListAsync(TranslationQueryDto query)
    {
        _appLog.Information("开始查询翻译列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<Translation, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "translationkey":
                        orderByExpression = t => t.TranslationKey;
                        break;
                    case "createdtime":
                        orderByExpression = t => t.CreatedTime;
                        break;
                    default:
                        orderByExpression = t => t.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = t => t.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _translationRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
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
            var response = result > 0 ? Result<long>.Ok(translation.Id) : Result<long>.Fail("创建翻译失败");

            _operLog?.LogCreate("Translation", translation.Id.ToString(), "Routine.TranslationView", 
                dto, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("创建翻译成功，ID: {Id}, 语言: {LanguageCode}, 键: {Key}",
                    translation.Id, translation.LanguageCode, translation.TranslationKey);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建翻译失败");
            return Result<long>.Fail($"创建翻译失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(TranslationUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var translation = await _translationRepository.GetByIdAsync(dto.Id);
            if (translation == null || translation.IsDeleted == 1)
                return Result.Fail("翻译不存在");

            // 目标语言（按代码）
            var targetLanguage = await _languageRepository.GetFirstAsync(l => l.LanguageCode == dto.LanguageCode && l.IsDeleted == 0);
            if (targetLanguage == null)
                return Result.Fail("关联的语言不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldTranslation = translation.Adapt<TranslationUpdateDto>();

            // 检查翻译键在同一语言下是否被其他记录使用
            var exists = await _translationRepository.GetFirstAsync(
                t => t.LanguageCode == targetLanguage.LanguageCode && t.TranslationKey == dto.TranslationKey && t.Id != dto.Id && t.IsDeleted == 0);
            if (exists != null)
                return Result.Fail($"语言 {dto.LanguageCode} 下已存在翻译键 {dto.TranslationKey}");

            dto.Adapt(translation);
            // 确保语言代码被正确更新
            translation.LanguageCode = dto.LanguageCode;

            var result = await _translationRepository.UpdateAsync(translation);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldTranslation.LanguageCode != dto.LanguageCode) changeList.Add($"LanguageCode: {oldTranslation.LanguageCode} -> {dto.LanguageCode}");
            if (oldTranslation.TranslationKey != dto.TranslationKey) changeList.Add($"TranslationKey: {oldTranslation.TranslationKey} -> {dto.TranslationKey}");
            if (oldTranslation.TranslationValue != dto.TranslationValue) changeList.Add($"TranslationValue: {oldTranslation.TranslationValue ?? "null"} -> {dto.TranslationValue ?? "null"}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新翻译失败");

            _operLog?.LogUpdate("Translation", dto.Id.ToString(), "Routine.TranslationView", changes, dto, oldTranslation, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("更新翻译成功，ID: {Id}", translation.Id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新翻译失败");
            return Result.Fail($"更新翻译失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var translation = await _translationRepository.GetByIdAsync(id);
            if (translation == null || translation.IsDeleted == 1)
                return Result.Fail("翻译不存在");

            var result = await _translationRepository.DeleteAsync(id);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除翻译失败");

            _operLog?.LogDelete("Translation", id.ToString(), "Routine.TranslationView", 
                new { Id = id, TranslationKey = translation.TranslationKey, LanguageCode = translation.LanguageCode }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("删除翻译成功，ID: {Id}", id);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除翻译失败");
            return Result.Fail($"删除翻译失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除翻译
    /// </summary>
    /// <param name="ids">翻译ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            var result = await _translationRepository.DeleteBatchAsync(ids.Cast<object>().ToList());
            var response = result > 0 ? Result.Ok($"成功删除 {result} 条记录") : Result.Fail("批量删除翻译失败");
            
            _operLog?.LogDelete("Translation", string.Join(",", ids), "Routine.TranslationView", 
                new { Ids = ids, Count = ids.Count }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("批量删除翻译成功，共删除 {Count} 条记录", result);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除翻译失败");
            return Result.Fail($"批量删除翻译失败: {ex.Message}");
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

            Debug.WriteLine("[TranslationService] 步骤1: 开始查询所有语言..");
            _appLog.Information("[TranslationService] 步骤1: 查询所有启用的语言");
            
            // 获取所有语言
            var languages = await _languageRepository.GetListAsync(
                l => l.IsDeleted == 0 && l.LanguageStatus == 0,
                1,
                int.MaxValue
            );

            Debug.WriteLine($"[TranslationService] 步骤1完成: 获取到 {languages.Items.Count} 种语言");
            _appLog.Information("[TranslationService] 获取到 {Count} 种语言", languages.Items.Count);

            Debug.WriteLine("[TranslationService] 步骤2: 开始构建翻译查询条件..");
            
            // 构建查询条件
        var condition = SqlSugar.Expressionable.Create<Translation>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
                .AndIF(!string.IsNullOrEmpty(module), x => x.Module == module)
                .ToExpression();

            Debug.WriteLine("[TranslationService] 步骤2完成: 查询条件已构建");
            
            Debug.WriteLine("[TranslationService] 步骤3: 开始查询所有翻译..");
            _appLog.Information("[TranslationService] 步骤3: 查询所有翻译数据");

            // 获取所有翻译
            var translations = await _translationRepository.GetListAsync(condition, 1, int.MaxValue);

            Debug.WriteLine($"[TranslationService] 步骤3完成: 获取到 {translations.Items.Count} 条翻译记录");
            _appLog.Information("[TranslationService] 获取到 {Count} 条翻译记录", translations.Items.Count);

            Debug.WriteLine("[TranslationService] 步骤4: 开始构建语言代码映射..");

            // 构建启用语言代码集合
            var enabledLanguageCodes = new HashSet<string>(languages.Items.Select(l => l.LanguageCode));

            Debug.WriteLine($"[TranslationService] 步骤4完成: 语言映射已构建，包含 {enabledLanguageCodes.Count} 个语言");

            Debug.WriteLine("[TranslationService] 步骤5: 开始转置翻译数据..");
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
    /// 分页获取唯一的翻译键列表
    /// </summary>
    /// <param name="pageIndex">页码（从1开始）</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="keyword">关键词（可选，用于搜索翻译键）</param>
    /// <param name="module">模块过滤（可选）</param>
    /// <returns>返回分页结果，包含翻译键列表和总数</returns>
    public async Task<Result<PagedResult<string>>> GetTranslationKeysAsync(int pageIndex, int pageSize, string? keyword = null!, string? module = null)
    {
        try
        {
            _appLog.Information("开始获取翻译键列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}', module='{Module}'",
                pageIndex, pageSize, keyword ?? string.Empty, module ?? string.Empty);

            // 使用数据库层面的 DISTINCT 查询，更高效
            var query = _translationRepository.AsQueryable()
                .Where(x => x.IsDeleted == 0);

            // 应用过滤条件
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.TranslationKey.Contains(keyword));
                _appLog.Information("应用关键词过滤: {Keyword}", keyword);
            }

            if (!string.IsNullOrEmpty(module))
            {
                query = query.Where(x => x.Module == module);
                _appLog.Information("应用模块过滤: {Module}", module);
            }

            // 先获取所有符合条件的记录数量（用于调试）
            var allCount = await query.CountAsync();
            _appLog.Information("符合条件的翻译记录总数: {Count}", allCount);

            // 获取所有翻译记录，然后在内存中去重（避免 SQL DISTINCT 语法问题）
            var allRecords = await query.Select(x => x.TranslationKey).ToListAsync();
            var allKeys = allRecords.Distinct().ToList();
            var totalCount = allKeys.Count;
            _appLog.Information("唯一翻译键总数: {TotalCount}", totalCount);

            if (totalCount == 0)
            {
                _appLog.Warning("未找到任何翻译键，可能原因：1) 数据库中没有数据 2) 查询条件过严 3) 所有数据都被标记为已删除");
                
                var emptyResult = new PagedResult<string>
                {
                    Items = new List<string>(),
                    TotalNum = 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                return Result<PagedResult<string>>.Ok(emptyResult);
            }

            // 在内存中排序和分页
            var sortedKeys = allKeys.OrderBy(x => x).ToList();
            var skip = (pageIndex - 1) * pageSize;
            var pagedKeys = sortedKeys.Skip(skip).Take(pageSize).ToList();

            _appLog.Information("获取翻译键列表完成，返回 {Count} 条，总数: {TotalNum}", pagedKeys.Count, totalCount);

            var pagedResult = new PagedResult<string>
            {
                Items = pagedKeys,
                TotalNum = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Result<PagedResult<string>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取翻译键列表失败，异常详情: {Exception}", ex.ToString());
            return Result<PagedResult<string>>.Fail($"获取翻译键列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出翻译到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的翻译</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(TranslationQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<Translation>().And(x => x.IsDeleted == 0).ToExpression();
            var translations = await _translationRepository.AsQueryable().Where(where).OrderBy(t => t.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = translations.Adapt<List<TranslationDto>>();
            sheetName ??= "Translations";
            fileName ??= $"翻译导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出翻译Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出翻译 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "Translations";
        fileName ??= $"翻译导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<TranslationDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入翻译
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "Translations";
            var dtos = ExcelHelper.ImportFromExcel<TranslationDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.TranslationKey) || string.IsNullOrWhiteSpace(dto.LanguageCode)) { fail++; continue; }
                    var existing = await _translationRepository.GetFirstAsync(t => t.TranslationKey == dto.TranslationKey && t.LanguageCode == dto.LanguageCode && t.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<Translation>();
                        await _translationRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _translationRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("Translation", success, "Routine.TranslationView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入翻译Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<Translation, bool>> QueryExpression(TranslationQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Translation>()
            .And(x => x.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.TranslationKey.Contains(query.Keywords!) || 
                                                              x.TranslationValue.Contains(query.Keywords!) ||
                                                              (x.Module != null && x.Module.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.TranslationKey), x => x.TranslationKey.Contains(query.TranslationKey!))
            .AndIF(!string.IsNullOrEmpty(query.Module), x => x.Module == query.Module)
            .ToExpression();
    }
}
