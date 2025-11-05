// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：DictionaryDataService.cs
// 创建时间：2025-01-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：字典数据服务实现（处理主子表关系）
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

using System.Linq.Expressions;
using Hbt.Application.Dtos.Routine;
using Hbt.Common.Logging;
using Hbt.Common.Results;
using Hbt.Domain.Entities.Routine;
using Hbt.Domain.Repositories;
using Mapster;

namespace Hbt.Application.Services.Routine;

/// <summary>
/// 字典数据服务实现
/// 注意：DictionaryData 是子表，关联 DictionaryType（主表）
/// </summary>
public class DictionaryDataService : IDictionaryDataService
{
    private readonly IBaseRepository<DictionaryData> _dictionaryDataRepository;
    private readonly IBaseRepository<DictionaryType> _dictionaryTypeRepository;
    private readonly AppLogManager _appLog;

    public DictionaryDataService(
        IBaseRepository<DictionaryData> dictionaryDataRepository,
        IBaseRepository<DictionaryType> dictionaryTypeRepository,
        AppLogManager appLog)
    {
        _dictionaryDataRepository = dictionaryDataRepository;
        _dictionaryTypeRepository = dictionaryTypeRepository;
        _appLog = appLog;
    }

    public async Task<Result<PagedResult<DictionaryDataDto>>> GetListAsync(int pageIndex, int pageSize, long? typeId = null, string? keyword = null)
    {
        _appLog.Information("开始查询字典数据列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, typeId={TypeId}, keyword='{Keyword}'",
            pageIndex, pageSize, typeId ?? 0, keyword ?? string.Empty);

        try
        {
            string? resolvedTypeCode = null;
            if (typeId.HasValue && typeId.Value > 0)
            {
                var dictType = await _dictionaryTypeRepository.GetByIdAsync(typeId.Value);
                if (dictType != null && dictType.IsDeleted == 0)
                {
                    resolvedTypeCode = dictType.TypeCode;
                }
            }

            var condition = SqlSugar.Expressionable.Create<DictionaryData>()
                .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
                .AndIF(!string.IsNullOrEmpty(resolvedTypeCode), x => x.TypeCode == resolvedTypeCode!)
                .AndIF(!string.IsNullOrEmpty(keyword), x => x.DataCode.Contains(keyword!) || x.DataName.Contains(keyword!))
                .ToExpression();
            var result = await _dictionaryDataRepository.GetListAsync(condition, pageIndex, pageSize);
            var dictionaryDataDtos = result.Items.Adapt<List<DictionaryDataDto>>();

            var pagedResult = new PagedResult<DictionaryDataDto>
            {
                Items = dictionaryDataDtos,
                TotalNum = result.TotalNum,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Result<PagedResult<DictionaryDataDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询字典数据失败");
            return Result<PagedResult<DictionaryDataDto>>.Fail($"查询字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<DictionaryDataDto>>> GetListAsync(DictionaryDataQueryDto query)
    {
        _appLog.Information("开始高级查询字典数据列表");

        try
        {
            var typeCode = query.TypeCode;
            var condition = SqlSugar.Expressionable.Create<DictionaryData>()
                .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
                .AndIF(!string.IsNullOrEmpty(typeCode), x => x.TypeCode == typeCode!)
                .AndIF(!string.IsNullOrEmpty(query.DataCode), x => x.DataCode.Contains(query.DataCode!))
                .AndIF(!string.IsNullOrEmpty(query.DataName), x => x.DataName.Contains(query.DataName!))
                .ToExpression();
            var result = await _dictionaryDataRepository.GetListAsync(condition, query.PageIndex, query.PageSize);
            var dictionaryDataDtos = result.Items.Adapt<List<DictionaryDataDto>>();

            var pagedResult = new PagedResult<DictionaryDataDto>
            {
                Items = dictionaryDataDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<DictionaryDataDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询字典数据失败");
            return Result<PagedResult<DictionaryDataDto>>.Fail($"高级查询字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result<List<DictionaryDataDto>>> GetByTypeCodeAsync(string typeCode)
    {
        try
        {
            var result = await _dictionaryDataRepository.GetListAsync(
                dd => dd.TypeCode == typeCode && dd.IsDeleted == 0,
                1,
                int.MaxValue
            );

            var dictionaryDataDtos = result.Items.Adapt<List<DictionaryDataDto>>();
            return Result<List<DictionaryDataDto>>.Ok(dictionaryDataDtos);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取字典数据失败");
            return Result<List<DictionaryDataDto>>.Fail($"获取字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result<DictionaryDataDto>> GetByIdAsync(long id)
    {
        var dictionaryData = await _dictionaryDataRepository.GetByIdAsync(id);
        if (dictionaryData == null)
            return Result<DictionaryDataDto>.Fail("字典数据不存在");

        var dictionaryDataDto = dictionaryData.Adapt<DictionaryDataDto>();
        return Result<DictionaryDataDto>.Ok(dictionaryDataDto);
    }

    public async Task<Result<long>> CreateAsync(DictionaryDataCreateDto dto)
    {
        try
        {
            // **主子表关系验证：检查主表是否存在（按代码）**
            var dictionaryType = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == dto.TypeCode && dt.IsDeleted == 0);
            if (dictionaryType == null)
                return Result<long>.Fail("关联的字典类型不存在");

            // 检查字典数据代码在同一个类型下是否唯一
            var exists = await _dictionaryDataRepository.GetFirstAsync(
                dd => dd.TypeCode == dto.TypeCode && dd.DataCode == dto.DataCode && dd.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"字典类型 {dto.TypeCode} 下已存在代码为 {dto.DataCode} 的字典数据");

            var dictionaryData = dto.Adapt<DictionaryData>();
            // 设置 TypeCode
            dictionaryData.TypeCode = dto.TypeCode;

            var result = await _dictionaryDataRepository.CreateAsync(dictionaryData);
            if (result > 0)
            {
                _appLog.Information("创建字典数据成功，ID: {Id}, 类型: {TypeCode}, 代码: {Code}",
                    dictionaryData.Id, dictionaryData.TypeCode, dictionaryData.DataCode);
                return Result<long>.Ok(dictionaryData.Id);
            }

            return Result<long>.Fail("创建字典数据失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建字典数据失败");
            return Result<long>.Fail($"创建字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(DictionaryDataUpdateDto dto)
    {
        try
        {
            var dictionaryData = await _dictionaryDataRepository.GetByIdAsync(dto.Id);
            if (dictionaryData == null || dictionaryData.IsDeleted == 1)
                return Result.Fail("字典数据不存在");

            // **主子表关系验证：目标主表必须存在（按代码）**
            var targetType = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == dto.TypeCode && dt.IsDeleted == 0);
            if (targetType == null)
                return Result.Fail("关联的字典类型不存在");

            // 检查字典数据代码在同一类型下是否被其他记录使用
            var exists = await _dictionaryDataRepository.GetFirstAsync(
                dd => dd.TypeCode == dto.TypeCode && dd.DataCode == dto.DataCode && dd.Id != dto.Id && dd.IsDeleted == 0);
            if (exists != null)
                return Result.Fail($"字典类型 {dto.TypeCode} 下已存在代码为 {dto.DataCode} 的字典数据");

            dto.Adapt(dictionaryData);
            // 确保 TypeCode 被正确更新
            dictionaryData.TypeCode = dto.TypeCode;

            var result = await _dictionaryDataRepository.UpdateAsync(dictionaryData);
            if (result > 0)
            {
                _appLog.Information("更新字典数据成功，ID: {Id}", dictionaryData.Id);
                return Result.Ok();
            }

            return Result.Fail("更新字典数据失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新字典数据失败");
            return Result.Fail($"更新字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var dictionaryData = await _dictionaryDataRepository.GetByIdAsync(id);
            if (dictionaryData == null || dictionaryData.IsDeleted == 1)
                return Result.Fail("字典数据不存在");

            var result = await _dictionaryDataRepository.DeleteAsync(id);
            if (result > 0)
            {
                _appLog.Information("删除字典数据成功，ID: {Id}", id);
                return Result.Ok();
            }

            return Result.Fail("删除字典数据失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除字典数据失败");
            return Result.Fail($"删除字典数据失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRangeAsync(List<long> ids)
    {
        try
        {
            var result = await _dictionaryDataRepository.DeleteRangeAsync(ids.Cast<object>().ToList());
            if (result > 0)
            {
                _appLog.Information("批量删除字典数据成功，共删除 {Count} 条记录", result);
                return Result.Ok();
            }

            return Result.Fail("批量删除字典数据失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "批量删除字典数据失败");
            return Result.Fail($"批量删除字典数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<DictionaryData, bool>> QueryExpression(long? typeId, string? keyword)
    {
        // 已弃用：实体使用 TypeCode，此方法仅保留签名，内部不使用 typeId。
        return SqlSugar.Expressionable.Create<DictionaryData>()
            .And(x => x.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(keyword), x => x.DataCode.Contains(keyword!) || x.DataName.Contains(keyword!))
            .ToExpression();
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<DictionaryData, bool>> QueryExpression(DictionaryDataQueryDto query)
    {
        // 已弃用：请在调用处将 TypeId 转为 TypeCode 后构造表达式
        return SqlSugar.Expressionable.Create<DictionaryData>()
            .And(x => x.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.DataCode), x => x.DataCode.Contains(query.DataCode!))
            .AndIF(!string.IsNullOrEmpty(query.DataName), x => x.DataName.Contains(query.DataName!))
            .ToExpression();
    }
}
