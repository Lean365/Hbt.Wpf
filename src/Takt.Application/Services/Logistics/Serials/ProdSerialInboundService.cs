// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：ProdSerialInboundService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号入库服务实现
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
/// 产品序列号入库服务实现
/// </summary>
public class ProdSerialInboundService : IProdSerialInboundService
{
    private readonly IBaseRepository<ProdSerialInbound> _prodSerialInboundRepository;
    private readonly AppLogManager _appLog;

    public ProdSerialInboundService(
        IBaseRepository<ProdSerialInbound> prodSerialInboundRepository,
        AppLogManager appLog)
    {
        _prodSerialInboundRepository = prodSerialInboundRepository;
        _appLog = appLog;
    }

    /// <summary>
    /// 查询产品序列号入库记录列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码、入库单号等筛选条件</param>
    /// <returns>分页产品序列号入库记录列表</returns>
    public async Task<Result<PagedResult<ProdSerialInboundDto>>> GetListAsync(ProdSerialInboundQueryDto query)
    {
        _appLog.Information("开始查询产品序列号入库记录列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<ProdSerialInbound, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "inboundno":
                        orderByExpression = psi => psi.InboundNo;
                        break;
                    case "inbounddate":
                        orderByExpression = psi => psi.InboundDate;
                        break;
                    case "createdtime":
                        orderByExpression = psi => psi.CreatedTime;
                        break;
                    default:
                        orderByExpression = psi => psi.InboundDate;
                        break;
                }
            }
            else
            {
                orderByExpression = psi => psi.InboundDate; // 默认按入库日期倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _prodSerialInboundRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodSerialInboundDtos = result.Items.Adapt<List<ProdSerialInboundDto>>();

            var pagedResult = new PagedResult<ProdSerialInboundDto>
            {
                Items = prodSerialInboundDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdSerialInboundDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询产品序列号入库记录数据失败");
            return Result<PagedResult<ProdSerialInboundDto>>.Fail($"高级查询产品序列号入库记录数据失败: {ex.Message}");
        }
    }

    public async Task<Result<ProdSerialInboundDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodSerialInbound = await _prodSerialInboundRepository.GetByIdAsync(id);
            if (prodSerialInbound == null)
                return Result<ProdSerialInboundDto>.Fail("产品序列号入库记录不存在");

            var prodSerialInboundDto = prodSerialInbound.Adapt<ProdSerialInboundDto>();
            return Result<ProdSerialInboundDto>.Ok(prodSerialInboundDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取产品序列号入库记录失败");
            return Result<ProdSerialInboundDto>.Fail($"获取产品序列号入库记录失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(ProdSerialInboundCreateDto dto)
    {
        try
        {
            // 检查完整序列号是否已存在
            if (!string.IsNullOrWhiteSpace(dto.FullSerialNumber))
            {
                var exists = await _prodSerialInboundRepository.GetFirstAsync(
                    psi => psi.FullSerialNumber == dto.FullSerialNumber && psi.IsDeleted == 0);
                if (exists != null)
                    return Result<long>.Fail($"完整序列号 {dto.FullSerialNumber} 已存在");
            }

            var prodSerialInbound = dto.Adapt<ProdSerialInbound>();
            var result = await _prodSerialInboundRepository.CreateAsync(prodSerialInbound);
            return Result<long>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建产品序列号入库记录失败");
            return Result<long>.Fail($"创建产品序列号入库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(ProdSerialInboundUpdateDto dto)
    {
        try
        {
            var prodSerialInbound = await _prodSerialInboundRepository.GetByIdAsync(dto.Id);
            if (prodSerialInbound == null)
                return Result.Fail("产品序列号入库记录不存在");

            // 检查完整序列号是否被其他记录使用
            if (prodSerialInbound.FullSerialNumber != dto.FullSerialNumber && !string.IsNullOrWhiteSpace(dto.FullSerialNumber))
            {
                var exists = await _prodSerialInboundRepository.GetFirstAsync(
                    psi => psi.FullSerialNumber == dto.FullSerialNumber && 
                           psi.Id != dto.Id && 
                           psi.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"完整序列号 {dto.FullSerialNumber} 已被其他入库记录使用");
            }

            dto.Adapt(prodSerialInbound);
            await _prodSerialInboundRepository.UpdateAsync(prodSerialInbound);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新产品序列号入库记录失败");
            return Result.Fail($"更新产品序列号入库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var prodSerialInbound = await _prodSerialInboundRepository.GetByIdAsync(id);
            if (prodSerialInbound == null)
                return Result.Fail("产品序列号入库记录不存在");

            await _prodSerialInboundRepository.DeleteAsync(prodSerialInbound);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除产品序列号入库记录失败");
            return Result.Fail($"删除产品序列号入库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteRangeAsync(List<long> ids)
    {
        try
        {
            foreach (var id in ids)
            {
                var prodSerialInbound = await _prodSerialInboundRepository.GetByIdAsync(id);
                if (prodSerialInbound != null)
                {
                    await _prodSerialInboundRepository.DeleteAsync(prodSerialInbound);
                }
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "批量删除产品序列号入库记录失败");
            return Result.Fail($"批量删除产品序列号入库记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品序列号入库记录到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号入库记录</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialInboundQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdSerialInbound>().And(x => x.IsDeleted == 0).ToExpression();
            var records = await _prodSerialInboundRepository.AsQueryable().Where(where).OrderBy(psi => psi.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = records.Adapt<List<ProdSerialInboundDto>>();
            sheetName ??= "ProdSerialInbounds";
            fileName ??= $"产品序列号入库记录导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出产品序列号入库记录Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品序列号入库记录 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "ProdSerialInbounds";
        fileName ??= $"产品序列号入库记录导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<ProdSerialInboundDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入产品序列号入库记录
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "ProdSerialInbounds";
            var dtos = ExcelHelper.ImportFromExcel<ProdSerialInboundDto>(fileStream, sheetName);
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
                    var existing = await _prodSerialInboundRepository.GetFirstAsync(
                        psi => psi.FullSerialNumber == dto.FullSerialNumber && psi.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<ProdSerialInbound>();
                        await _prodSerialInboundRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _prodSerialInboundRepository.UpdateAsync(existing);
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
            _appLog.Error(ex, "导入产品序列号入库记录Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<ProdSerialInbound, bool>> QueryExpression(ProdSerialInboundQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdSerialInbound>()
            .And(psi => psi.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), psi => psi.InboundNo.Contains(query.Keywords!) || 
                                                                psi.SerialNumber.Contains(query.Keywords!) ||
                                                                psi.FullSerialNumber.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), psi => psi.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.InboundNo), psi => psi.InboundNo.Contains(query.InboundNo!))
            .AndIF(!string.IsNullOrEmpty(query.SerialNumber), psi => psi.SerialNumber.Contains(query.SerialNumber!))
            .AndIF(query.InboundDateFrom.HasValue, psi => psi.InboundDate >= query.InboundDateFrom!.Value)
            .AndIF(query.InboundDateTo.HasValue, psi => psi.InboundDate <= query.InboundDateTo!.Value)
            .ToExpression();
    }
}

