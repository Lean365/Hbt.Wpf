// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：ProdSerialOutboundService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号出库服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using System.Linq.Expressions;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Serials;
using Takt.Domain.Repositories;
using Mapster;

namespace Takt.Application.Services.Logistics.Serials;

/// <summary>
/// 产品序列号出库服务实现
/// </summary>
public class ProdSerialOutboundService : IProdSerialOutboundService
{
    private readonly IBaseRepository<ProdSerialOutbound> _prodSerialOutboundRepository;
    private readonly AppLogManager _appLog;

    public ProdSerialOutboundService(
        IBaseRepository<ProdSerialOutbound> prodSerialOutboundRepository,
        AppLogManager appLog)
    {
        _prodSerialOutboundRepository = prodSerialOutboundRepository;
        _appLog = appLog;
    }

    /// <summary>
    /// 查询产品序列号出库记录列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码、出库单号等筛选条件</param>
    /// <returns>分页产品序列号出库记录列表</returns>
    public async Task<Result<PagedResult<ProdSerialOutboundDto>>> GetListAsync(ProdSerialOutboundQueryDto query)
    {
        _appLog.Information("开始查询产品序列号出库记录列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<ProdSerialOutbound, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "outboundno":
                        orderByExpression = pso => pso.OutboundNo;
                        break;
                    case "outbounddate":
                        orderByExpression = pso => pso.OutboundDate;
                        break;
                    case "createdtime":
                        orderByExpression = pso => pso.CreatedTime;
                        break;
                    default:
                        orderByExpression = pso => pso.OutboundDate;
                        break;
                }
            }
            else
            {
                orderByExpression = pso => pso.OutboundDate; // 默认按出库日期倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _prodSerialOutboundRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodSerialOutboundDtos = result.Items.Adapt<List<ProdSerialOutboundDto>>();

            var pagedResult = new PagedResult<ProdSerialOutboundDto>
            {
                Items = prodSerialOutboundDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdSerialOutboundDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询产品序列号出库记录数据失败");
            return Result<PagedResult<ProdSerialOutboundDto>>.Fail($"高级查询产品序列号出库记录数据失败: {ex.Message}");
        }
    }

    public async Task<Result<ProdSerialOutboundDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodSerialOutbound = await _prodSerialOutboundRepository.GetByIdAsync(id);
            if (prodSerialOutbound == null)
                return Result<ProdSerialOutboundDto>.Fail("产品序列号出库记录不存在");

            var prodSerialOutboundDto = prodSerialOutbound.Adapt<ProdSerialOutboundDto>();
            return Result<ProdSerialOutboundDto>.Ok(prodSerialOutboundDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取产品序列号出库记录失败");
            return Result<ProdSerialOutboundDto>.Fail($"获取产品序列号出库记录失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(ProdSerialOutboundCreateDto dto)
    {
        try
        {
            // 检查完整序列号是否已存在
            if (!string.IsNullOrWhiteSpace(dto.FullSerialNumber))
            {
                var exists = await _prodSerialOutboundRepository.GetFirstAsync(
                    pso => pso.FullSerialNumber == dto.FullSerialNumber && pso.IsDeleted == 0);
                if (exists != null)
                    return Result<long>.Fail($"完整序列号 {dto.FullSerialNumber} 已存在");
            }

            var prodSerialOutbound = dto.Adapt<ProdSerialOutbound>();
            var result = await _prodSerialOutboundRepository.CreateAsync(prodSerialOutbound);
            return Result<long>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建产品序列号出库记录失败");
            return Result<long>.Fail($"创建产品序列号出库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(ProdSerialOutboundUpdateDto dto)
    {
        try
        {
            var prodSerialOutbound = await _prodSerialOutboundRepository.GetByIdAsync(dto.Id);
            if (prodSerialOutbound == null)
                return Result.Fail("产品序列号出库记录不存在");

            // 检查完整序列号是否被其他记录使用
            if (prodSerialOutbound.FullSerialNumber != dto.FullSerialNumber && !string.IsNullOrWhiteSpace(dto.FullSerialNumber))
            {
                var exists = await _prodSerialOutboundRepository.GetFirstAsync(
                    pso => pso.FullSerialNumber == dto.FullSerialNumber && 
                           pso.Id != dto.Id && 
                           pso.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"完整序列号 {dto.FullSerialNumber} 已被其他出库记录使用");
            }

            dto.Adapt(prodSerialOutbound);
            await _prodSerialOutboundRepository.UpdateAsync(prodSerialOutbound);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新产品序列号出库记录失败");
            return Result.Fail($"更新产品序列号出库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var prodSerialOutbound = await _prodSerialOutboundRepository.GetByIdAsync(id);
            if (prodSerialOutbound == null)
                return Result.Fail("产品序列号出库记录不存在");

            await _prodSerialOutboundRepository.DeleteAsync(prodSerialOutbound);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除产品序列号出库记录失败");
            return Result.Fail($"删除产品序列号出库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRangeAsync(List<long> ids)
    {
        try
        {
            foreach (var id in ids)
            {
                var prodSerialOutbound = await _prodSerialOutboundRepository.GetByIdAsync(id);
                if (prodSerialOutbound != null)
                {
                    await _prodSerialOutboundRepository.DeleteAsync(prodSerialOutbound);
                }
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "批量删除产品序列号出库记录失败");
            return Result.Fail($"批量删除产品序列号出库记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品序列号出库记录到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号出库记录</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialOutboundQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdSerialOutbound>().And(x => x.IsDeleted == 0).ToExpression();
            var records = await _prodSerialOutboundRepository.AsQueryable().Where(where).OrderBy(pso => pso.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = records.Adapt<List<ProdSerialOutboundDto>>();
            sheetName ??= "ProdSerialOutbounds";
            fileName ??= $"产品序列号出库记录导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出产品序列号出库记录Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品序列号出库记录 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "ProdSerialOutbounds";
        fileName ??= $"产品序列号出库记录导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<ProdSerialOutboundDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入产品序列号出库记录
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "ProdSerialOutbounds";
            var dtos = ExcelHelper.ImportFromExcel<ProdSerialOutboundDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.FullSerialNumber)) { fail++; continue; }
                    
                    // 检查完整序列号是否已存在
                    var existing = await _prodSerialOutboundRepository.GetFirstAsync(
                        pso => pso.FullSerialNumber == dto.FullSerialNumber && pso.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<ProdSerialOutbound>();
                        await _prodSerialOutboundRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _prodSerialOutboundRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            return Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导入产品序列号出库记录Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<ProdSerialOutbound, bool>> QueryExpression(ProdSerialOutboundQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdSerialOutbound>()
            .And(pso => pso.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), pso => pso.OutboundNo.Contains(query.Keywords!) || 
                                                                pso.SerialNumber.Contains(query.Keywords!) ||
                                                                pso.FullSerialNumber.Contains(query.Keywords!) ||
                                                                (pso.Destination != null && pso.Destination.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), pso => pso.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.OutboundNo), pso => pso.OutboundNo.Contains(query.OutboundNo!))
            .AndIF(!string.IsNullOrEmpty(query.SerialNumber), pso => pso.SerialNumber.Contains(query.SerialNumber!))
            .AndIF(!string.IsNullOrEmpty(query.Destination), pso => pso.Destination != null && pso.Destination.Contains(query.Destination!))
            .AndIF(query.OutboundDateFrom.HasValue, pso => pso.OutboundDate >= query.OutboundDateFrom!.Value)
            .AndIF(query.OutboundDateTo.HasValue, pso => pso.OutboundDate <= query.OutboundDateTo!.Value)
            .ToExpression();
    }
}

