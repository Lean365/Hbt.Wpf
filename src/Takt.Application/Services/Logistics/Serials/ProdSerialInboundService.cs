// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：ProdSerialInboundService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号入库服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Mapster;
using System.Linq.Expressions;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Serials;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Logistics.Serials;

/// <summary>
/// 产品序列号入库服务实现
/// </summary>
public class ProdSerialInboundService : IProdSerialInboundService
{
    private readonly IBaseRepository<ProdSerialInbound> _prodSerialInboundRepository;
    private readonly IProdSerialScanningService _prodSerialScanningService;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public ProdSerialInboundService(
        IBaseRepository<ProdSerialInbound> prodSerialInboundRepository,
        IProdSerialScanningService prodSerialScanningService,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _prodSerialInboundRepository = prodSerialInboundRepository;
        _prodSerialScanningService = prodSerialScanningService;
        _appLog = appLog;
        _operLog = operLog;
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

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.Equals("asc", StringComparison.OrdinalIgnoreCase))
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
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
            var response = result > 0 ? Result<long>.Ok(prodSerialInbound.Id) : Result<long>.Fail("创建产品序列号入库记录失败");

            // 自动创建入库扫描记录
            try
            {
                var scanningDto = new ProdSerialInboundScanningCreateDto
                {
                    InboundFullSerialNumber = prodSerialInbound.FullSerialNumber,
                    InboundDate = prodSerialInbound.InboundDate != default ? prodSerialInbound.InboundDate : DateTime.Now,
                    InboundClient = Takt.Common.Context.UserContext.Current.RealName ?? Takt.Common.Context.UserContext.Current.Username,
                    InboundIp = SystemInfoHelper.GetLocalIpAddress(),
                    InboundMachineName = SystemInfoHelper.GetMachineName(),
                    InboundLocation = null!, // 可以根据需要设置
                    InboundOs = $"{SystemInfoHelper.GetOsType()} {SystemInfoHelper.GetOsVersion()}"
                };

                // 调用扫描服务创建入库扫描记录
                await _prodSerialScanningService.CreateInboundScanningAsync(scanningDto);
                _appLog.Information("自动创建入库扫描记录成功，完整序列号={FullSerialNumber}", prodSerialInbound.FullSerialNumber);
            }
            catch (Exception ex)
            {
                // 扫描记录创建失败不影响入库操作，只记录日志
                _appLog.Warning("自动创建入库扫描记录失败，完整序列号={FullSerialNumber}, 错误={Error}",
                    prodSerialInbound.FullSerialNumber, ex.Message);
            }

            _operLog?.LogCreate("ProdSerialInbound", prodSerialInbound.Id.ToString(), "Logistics.Serials.ProdSerialInboundView",
                dto, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建产品序列号入库记录失败");
            return Result<long>.Fail($"创建产品序列号入库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(ProdSerialInboundUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialInbound = await _prodSerialInboundRepository.GetByIdAsync(dto.Id);
            if (prodSerialInbound == null)
                return Result.Fail("产品序列号入库记录不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldProdSerialInbound = prodSerialInbound.Adapt<ProdSerialInboundUpdateDto>();

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
            var result = await _prodSerialInboundRepository.UpdateAsync(prodSerialInbound);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldProdSerialInbound.FullSerialNumber != dto.FullSerialNumber) changeList.Add($"FullSerialNumber: {oldProdSerialInbound.FullSerialNumber} -> {dto.FullSerialNumber}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新产品序列号入库记录失败");

            _operLog?.LogUpdate("ProdSerialInbound", dto.Id.ToString(), "Logistics.Serials.ProdSerialInboundView", changes, dto, oldProdSerialInbound, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新产品序列号入库记录失败");
            return Result.Fail($"更新产品序列号入库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialInbound = await _prodSerialInboundRepository.GetByIdAsync(id);
            if (prodSerialInbound == null)
                return Result.Fail("产品序列号入库记录不存在");

            var result = await _prodSerialInboundRepository.DeleteAsync(prodSerialInbound);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除产品序列号入库记录失败");

            _operLog?.LogDelete("ProdSerialInbound", id.ToString(), "Logistics.Serials.ProdSerialInboundView",
                new { Id = id, FullSerialNumber = prodSerialInbound.FullSerialNumber }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除产品序列号入库记录失败");
            return Result.Fail($"删除产品序列号入库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || ids.Count == 0)
                return Result.Fail("ID列表为空");

            int successCount = 0;
            var deletedInfos = new List<object>();

            foreach (var id in ids)
            {
                var prodSerialInbound = await _prodSerialInboundRepository.GetByIdAsync(id);
                if (prodSerialInbound != null)
                {
                    var result = await _prodSerialInboundRepository.DeleteAsync(prodSerialInbound);
                    if (result > 0)
                    {
                        successCount++;
                        deletedInfos.Add(new { Id = id, FullSerialNumber = prodSerialInbound.FullSerialNumber });
                    }
                }
            }

            var response = Result.Ok($"成功删除 {successCount} 条记录");

            _operLog?.LogDelete("ProdSerialInbound", string.Join(",", ids), "Logistics.Serials.ProdSerialInboundView",
                new { Ids = ids, SuccessCount = successCount, DeletedInfos = deletedInfos }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
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
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialInboundQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
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
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "ProdSerialInbounds";
            var dtos = ExcelHelper.ImportFromExcel<ProdSerialInboundDto>(fileStream, sheetName);
            if (dtos == null || dtos.Count == 0)
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

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("ProdSerialInbound", success, "Logistics.Serials.ProdSerialInboundView",
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入产品序列号入库记录Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private static Expression<Func<ProdSerialInbound, bool>> QueryExpression(ProdSerialInboundQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdSerialInbound>()
            .And(psi => psi.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), psi => psi.InboundNo.Contains(query.Keywords!) ||
                                                                psi.SerialNumber.Contains(query.Keywords!) ||
                                                                psi.FullSerialNumber.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), psi => psi.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.InboundNo), psi => psi.InboundNo.Contains(query.InboundNo!))
            .AndIF(!string.IsNullOrEmpty(query.SerialNumber), psi => psi.SerialNumber.Contains(query.SerialNumber!))
            .AndIF(query.InboundDateFrom != default(DateTime), psi => psi.InboundDate >= query.InboundDateFrom)
            .AndIF(query.InboundDateTo != default(DateTime), psi => psi.InboundDate <= query.InboundDateTo)
            .ToExpression();
    }
}

