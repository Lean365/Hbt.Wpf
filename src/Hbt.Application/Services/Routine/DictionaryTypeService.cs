// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：DictionaryTypeService.cs
// 创建时间：2025-01-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：字典类型服务实现（处理主子表关系）
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
/// 字典类型服务实现
/// 注意：DictionaryType 和 DictionaryData 是主子表关系
/// </summary>
public class DictionaryTypeService : IDictionaryTypeService
{
    private readonly IBaseRepository<DictionaryType> _dictionaryTypeRepository;
    private readonly IBaseRepository<DictionaryData> _dictionaryDataRepository;
    private readonly AppLogManager _appLog;

    public DictionaryTypeService(
        IBaseRepository<DictionaryType> dictionaryTypeRepository,
        IBaseRepository<DictionaryData> dictionaryDataRepository,
        AppLogManager appLog)
    {
        _dictionaryTypeRepository = dictionaryTypeRepository;
        _dictionaryDataRepository = dictionaryDataRepository;
        _appLog = appLog;
    }

    public async Task<Result<PagedResult<DictionaryTypeDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null)
    {
        _appLog.Information("开始查询字典类型列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            pageIndex, pageSize, keyword ?? string.Empty);

        try
        {
            System.Linq.Expressions.Expression<Func<DictionaryType, bool>>? condition = null;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = dt => dt.TypeCode.Contains(keyword) || dt.TypeName.Contains(keyword);
            }

            var result = await _dictionaryTypeRepository.GetListAsync(condition, pageIndex, pageSize);
            var dictionaryTypeDtos = result.Items.Adapt<List<DictionaryTypeDto>>();

            var pagedResult = new PagedResult<DictionaryTypeDto>
            {
                Items = dictionaryTypeDtos,
                TotalNum = result.TotalNum,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Result<PagedResult<DictionaryTypeDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询字典类型数据失败");
            return Result<PagedResult<DictionaryTypeDto>>.Fail($"查询字典类型数据失败: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<DictionaryTypeDto>>> GetListAsync(DictionaryTypeQueryDto query)
    {
        _appLog.Information("开始高级查询字典类型列表");

        try
        {
            var condition = QueryExpression(query);
            var result = await _dictionaryTypeRepository.GetListAsync(condition, query.PageIndex, query.PageSize);
            var dictionaryTypeDtos = result.Items.Adapt<List<DictionaryTypeDto>>();

            var pagedResult = new PagedResult<DictionaryTypeDto>
            {
                Items = dictionaryTypeDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<DictionaryTypeDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询字典类型数据失败");
            return Result<PagedResult<DictionaryTypeDto>>.Fail($"高级查询字典类型数据失败: {ex.Message}");
        }
    }

    public async Task<Result<DictionaryTypeDto>> GetByIdAsync(long id, bool includeData = false)
    {
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetByIdAsync(id);
            if (dictionaryType == null || dictionaryType.IsDeleted == 1)
                return Result<DictionaryTypeDto>.Fail("字典类型不存在");

            var dictionaryTypeDto = dictionaryType.Adapt<DictionaryTypeDto>();

            // 如果需要包含字典数据（主子表关联查询）
            if (includeData)
            {
                var dictionaryDataResult = await _dictionaryDataRepository.GetListAsync(
                    dd => dd.TypeCode == dictionaryType.TypeCode && dd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                // 注意：这里只是说明需要包含数据，但 DTO 需要扩展才能支持
                _appLog.Information("加载字典类型 {Id} 的字典数据，共 {Count} 条", id, dictionaryDataResult.Items.Count);
            }

            return Result<DictionaryTypeDto>.Ok(dictionaryTypeDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取字典类型失败");
            return Result<DictionaryTypeDto>.Fail($"获取字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result<DictionaryTypeDto>> GetByCodeAsync(string typeCode, bool includeData = false)
    {
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == typeCode && dt.IsDeleted == 0);
            if (dictionaryType == null)
                return Result<DictionaryTypeDto>.Fail("字典类型不存在");

            var dictionaryTypeDto = dictionaryType.Adapt<DictionaryTypeDto>();

            // 如果需要包含字典数据
            if (includeData)
            {
                var dictionaryDataResult = await _dictionaryDataRepository.GetListAsync(
                    dd => dd.TypeCode == dictionaryType.TypeCode && dd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                _appLog.Information("加载字典类型 {Code} 的字典数据，共 {Count} 条", typeCode, dictionaryDataResult.Items.Count);
            }

            return Result<DictionaryTypeDto>.Ok(dictionaryTypeDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取字典类型失败");
            return Result<DictionaryTypeDto>.Fail($"获取字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(DictionaryTypeCreateDto dto)
    {
        try
        {
            // 检查类型代码是否已存在
            var exists = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == dto.TypeCode && dt.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"字典类型代码 {dto.TypeCode} 已存在");

            var dictionaryType = dto.Adapt<DictionaryType>();

            var result = await _dictionaryTypeRepository.CreateAsync(dictionaryType);
            if (result > 0)
            {
                _appLog.Information("创建字典类型成功，ID: {Id}, 代码: {Code}", dictionaryType.Id, dictionaryType.TypeCode);
                return Result<long>.Ok(dictionaryType.Id);
            }

            return Result<long>.Fail("创建字典类型失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建字典类型失败");
            return Result<long>.Fail($"创建字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(DictionaryTypeUpdateDto dto)
    {
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetByIdAsync(dto.Id);
            if (dictionaryType == null || dictionaryType.IsDeleted == 1)
                return Result.Fail("字典类型不存在");

            // 检查类型代码是否已被其他记录使用
            var exists = await _dictionaryTypeRepository.GetFirstAsync(dt => dt.TypeCode == dto.TypeCode && dt.Id != dto.Id && dt.IsDeleted == 0);
            if (exists != null)
                return Result.Fail($"字典类型代码 {dto.TypeCode} 已被其他字典类型使用");

            dto.Adapt(dictionaryType);

            var result = await _dictionaryTypeRepository.UpdateAsync(dictionaryType);
            if (result > 0)
            {
                _appLog.Information("更新字典类型成功，ID: {Id}", dictionaryType.Id);
                return Result.Ok();
            }

            return Result.Fail("更新字典类型失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新字典类型失败");
            return Result.Fail($"更新字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var dictionaryType = await _dictionaryTypeRepository.GetByIdAsync(id);
            if (dictionaryType == null || dictionaryType.IsDeleted == 1)
                return Result.Fail("字典类型不存在");

            // 检查是否为内置类型（0=是，1=否）
            if (dictionaryType.IsBuiltin == 0)
                return Result.Fail("内置字典类型不允许删除");

            // **主子表关系处理：删除主表时，同时删除子表数据（级联删除）**
            var dictionaryDataList = await _dictionaryDataRepository.GetListAsync(
                dd => dd.TypeCode == dictionaryType.TypeCode && dd.IsDeleted == 0,
                1,
                int.MaxValue
            );

            // 先删除所有关联的字典数据
            foreach (var dictionaryData in dictionaryDataList.Items)
            {
                await _dictionaryDataRepository.DeleteAsync(dictionaryData.Id);
            }

            _appLog.Information("删除字典类型 {Id} 的关联字典数据，共 {Count} 条", id, dictionaryDataList.Items.Count);

            // 再删除字典类型
            var result = await _dictionaryTypeRepository.DeleteAsync(id);
            if (result > 0)
            {
                _appLog.Information("删除字典类型成功，ID: {Id}", id);
                return Result.Ok();
            }

            return Result.Fail("删除字典类型失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除字典类型失败");
            return Result.Fail($"删除字典类型失败: {ex.Message}");
        }
    }

    public async Task<Result> StatusAsync(long id, int status)
    {
        try
        {
            var result = await _dictionaryTypeRepository.StatusAsync(id, status);
            if (result > 0)
            {
                _appLog.Information("修改字典类型状态成功，ID: {Id}, 状态: {Status}", id, status);
                return Result.Ok();
            }

            return Result.Fail("修改字典类型状态失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "修改字典类型状态失败");
            return Result.Fail($"修改字典类型状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<DictionaryType, bool>> QueryExpression(DictionaryTypeQueryDto query)
    {
        return SqlSugar.Expressionable.Create<DictionaryType>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
            .AndIF(!string.IsNullOrEmpty(query.TypeCode), x => x.TypeCode.Contains(query.TypeCode!))
            .AndIF(!string.IsNullOrEmpty(query.TypeName), x => x.TypeName.Contains(query.TypeName!))
            .AndIF(query.TypeStatus.HasValue, x => x.TypeStatus == query.TypeStatus!.Value)
            .ToExpression();
    }
}
