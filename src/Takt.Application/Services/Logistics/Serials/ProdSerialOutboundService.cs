// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：ProdSerialOutboundService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号出库服务实现
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
/// 产品序列号出库服务实现
/// </summary>
public class ProdSerialOutboundService : IProdSerialOutboundService
{
    private readonly IBaseRepository<ProdSerialOutbound> _prodSerialOutboundRepository;
    private readonly IBaseRepository<ProdSerialInbound> _prodSerialInboundRepository;
    private readonly IProdSerialScanningService _prodSerialScanningService;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public ProdSerialOutboundService(
        IBaseRepository<ProdSerialOutbound> prodSerialOutboundRepository,
        IBaseRepository<ProdSerialInbound> prodSerialInboundRepository,
        IProdSerialScanningService prodSerialScanningService,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _prodSerialOutboundRepository = prodSerialOutboundRepository;
        _prodSerialInboundRepository = prodSerialInboundRepository;
        _prodSerialScanningService = prodSerialScanningService;
        _appLog = appLog;
        _operLog = operLog;
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

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.Equals("asc", StringComparison.OrdinalIgnoreCase))
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 验证：出库扫描要验证该序列号是否入库
            if (!string.IsNullOrWhiteSpace(dto.FullSerialNumber))
            {
                // 检查该序列号是否已入库
                var inboundRecord = await _prodSerialInboundRepository.GetFirstAsync(
                    psi => psi.FullSerialNumber == dto.FullSerialNumber && psi.IsDeleted == 0);
                if (inboundRecord == null)
                {
                    _appLog.Warning("出库扫描失败：序列号 {FullSerialNumber} 尚未入库", dto.FullSerialNumber);
                    return Result<long>.Fail("该序列号尚未入库，无法出库");
                }

                // 检查完整序列号是否已出库（避免重复出库）
                var exists = await _prodSerialOutboundRepository.GetFirstAsync(
                    pso => pso.FullSerialNumber == dto.FullSerialNumber && pso.IsDeleted == 0);
                if (exists != null)
                {
                    _appLog.Warning("出库扫描失败：序列号 {FullSerialNumber} 已存在", dto.FullSerialNumber);
                    return Result<long>.Fail($"完整序列号 {dto.FullSerialNumber} 已存在");
                }
            }

            var prodSerialOutbound = dto.Adapt<ProdSerialOutbound>();
            var result = await _prodSerialOutboundRepository.CreateAsync(prodSerialOutbound);
            var response = result > 0 ? Result<long>.Ok(prodSerialOutbound.Id) : Result<long>.Fail("创建产品序列号出库记录失败");

            _appLog.Information("出库扫描成功：序列号 {FullSerialNumber}, 出库单号={OutboundNo}",
                dto.FullSerialNumber, dto.OutboundNo);

            // 自动创建出库扫描记录
            try
            {
                var scanningDto = new ProdSerialOutboundScanningCreateDto
                {
                    OutboundNo = prodSerialOutbound.OutboundNo,
                    DestCode = prodSerialOutbound.DestCode!,
                    DestPort = prodSerialOutbound.DestPort!,
                    OutboundFullSerialNumber = prodSerialOutbound.FullSerialNumber,
                    OutboundDate = prodSerialOutbound.OutboundDate != default ? prodSerialOutbound.OutboundDate : DateTime.Now,
                    OutboundClient = Takt.Common.Context.UserContext.Current.RealName ?? Takt.Common.Context.UserContext.Current.Username,
                    OutboundIp = SystemInfoHelper.GetLocalIpAddress(),
                    OutboundMachineName = SystemInfoHelper.GetMachineName(),
                    OutboundLocation = null!, // 可以根据需要设置
                    OutboundOs = $"{SystemInfoHelper.GetOsType()} {SystemInfoHelper.GetOsVersion()}"
                };

                // 调用扫描服务创建出库扫描记录（服务内部会处理更新已有记录的逻辑）
                await _prodSerialScanningService.CreateOutboundScanningAsync(scanningDto);
                _appLog.Information("自动创建出库扫描记录成功，完整序列号={FullSerialNumber}, 出库单号={OutboundNo}",
                    prodSerialOutbound.FullSerialNumber, prodSerialOutbound.OutboundNo);
            }
            catch (Exception ex)
            {
                // 扫描记录创建失败不影响出库操作，只记录日志
                _appLog.Warning("自动创建出库扫描记录失败，完整序列号={FullSerialNumber}, 出库单号={OutboundNo}, 错误={Error}",
                    prodSerialOutbound.FullSerialNumber, prodSerialOutbound.OutboundNo, ex.Message);
            }

            _operLog?.LogCreate("ProdSerialOutbound", prodSerialOutbound.Id.ToString(), "Logistics.Serials.ProdSerialOutboundView",
                dto, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建产品序列号出库记录失败");
            return Result<long>.Fail($"创建产品序列号出库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(ProdSerialOutboundUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialOutbound = await _prodSerialOutboundRepository.GetByIdAsync(dto.Id);
            if (prodSerialOutbound == null)
                return Result.Fail("产品序列号出库记录不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldProdSerialOutbound = prodSerialOutbound.Adapt<ProdSerialOutboundUpdateDto>();

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
            var result = await _prodSerialOutboundRepository.UpdateAsync(prodSerialOutbound);

            // 构建变更信息
            var changeList = new List<string>();
            if (oldProdSerialOutbound.FullSerialNumber != dto.FullSerialNumber) changeList.Add($"FullSerialNumber: {oldProdSerialOutbound.FullSerialNumber} -> {dto.FullSerialNumber}");
            if (oldProdSerialOutbound.OutboundNo != dto.OutboundNo) changeList.Add($"OutboundNo: {oldProdSerialOutbound.OutboundNo ?? "null"} -> {dto.OutboundNo ?? "null"}");
            if (oldProdSerialOutbound.OutboundDate != dto.OutboundDate) changeList.Add($"OutboundDate: {oldProdSerialOutbound.OutboundDate} -> {dto.OutboundDate}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            var response = result > 0 ? Result.Ok() : Result.Fail("更新产品序列号出库记录失败");

            _operLog?.LogUpdate("ProdSerialOutbound", dto.Id.ToString(), "Logistics.Serials.ProdSerialOutboundView", changes, dto, oldProdSerialOutbound, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新产品序列号出库记录失败");
            return Result.Fail($"更新产品序列号出库记录失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialOutbound = await _prodSerialOutboundRepository.GetByIdAsync(id);
            if (prodSerialOutbound == null)
                return Result.Fail("产品序列号出库记录不存在");

            var result = await _prodSerialOutboundRepository.DeleteAsync(prodSerialOutbound);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除产品序列号出库记录失败");

            _operLog?.LogDelete("ProdSerialOutbound", id.ToString(), "Logistics.Serials.ProdSerialOutboundView",
                new { Id = id, FullSerialNumber = prodSerialOutbound.FullSerialNumber }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除产品序列号出库记录失败");
            return Result.Fail($"删除产品序列号出库记录失败: {ex.Message}");
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
                var prodSerialOutbound = await _prodSerialOutboundRepository.GetByIdAsync(id);
                if (prodSerialOutbound != null)
                {
                    var result = await _prodSerialOutboundRepository.DeleteAsync(prodSerialOutbound);
                    if (result > 0)
                    {
                        successCount++;
                        deletedInfos.Add(new { Id = id, FullSerialNumber = prodSerialOutbound.FullSerialNumber });
                    }
                }
            }

            var response = Result.Ok($"成功删除 {successCount} 条记录");

            _operLog?.LogDelete("ProdSerialOutbound", string.Join(",", ids), "Logistics.Serials.ProdSerialOutboundView",
                new { Ids = ids, SuccessCount = successCount, DeletedInfos = deletedInfos }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
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
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialOutboundQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
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
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "ProdSerialOutbounds";
            var dtos = ExcelHelper.ImportFromExcel<ProdSerialOutboundDto>(fileStream, sheetName);
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

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");

            _operLog?.LogImport("ProdSerialOutbound", success, "Logistics.Serials.ProdSerialOutboundView",
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入产品序列号出库记录Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 统计产品序列号出库记录（按 DestCode 和 DestPort 统计占比）
    /// </summary>
    /// <param name="query">统计查询参数</param>
    /// <returns>统计数据列表</returns>
    public async Task<Result<List<ProdSerialOutboundStatisticDto>>> GetStatisticsAsync(ProdSerialOutboundStatisticQueryDto query)
    {
        _appLog.Information("开始统计产品序列号出库记录，参数: StatisticType={StatisticType}, Dimension={Dimension}, StartDate={StartDate}, EndDate={EndDate}",
            query.StatisticType ?? string.Empty, query.Dimension ?? string.Empty, query.StartDate.ToString("yyyy-MM-dd"), query.EndDate.ToString("yyyy-MM-dd"));

        try
        {
            var baseQuery = _prodSerialOutboundRepository.AsQueryable()
                .Where(pso => pso.IsDeleted == 0);

            // 应用日期范围过滤
            if (query.StartDate != default(DateTime))
            {
                baseQuery = baseQuery.Where(pso => pso.OutboundDate >= query.StartDate);
            }
            if (query.EndDate != default(DateTime))
            {
                baseQuery = baseQuery.Where(pso => pso.OutboundDate <= query.EndDate);
            }

            var statistics = new List<ProdSerialOutboundStatisticDto>();

            // 查询所有符合条件的数据
            var allData = await baseQuery
                .Select(pso => new
                {
                    pso.DestCode,
                    pso.DestPort,
                    pso.Quantity,
                    pso.OutboundDate
                })
                .ToListAsync();

            // 根据统计维度进行分组统计
            if (query.Dimension == "DestCode" || query.Dimension == "Both")
            {
                // 按 DestCode 统计
                var destCodeGroups = allData
                    .GroupBy(d => new
                    {
                        DestCode = d.DestCode ?? "NULL",
                        Period = query.StatisticType == "Year"
                            ? d.OutboundDate.Year.ToString()
                            : $"{d.OutboundDate.Year}-{d.OutboundDate.Month:D2}"
                    })
                    .Select(g => new
                    {
                        g.Key.Period,
                        g.Key.DestCode,
                        TotalQuantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                // 计算总数和占比
                var totalQuantity = destCodeGroups.Sum(s => s.TotalQuantity);
                foreach (var stat in destCodeGroups)
                {
                    statistics.Add(new ProdSerialOutboundStatisticDto
                    {
                        Period = stat.Period,
                        DestCode = stat.DestCode == "NULL" ? null! : stat.DestCode,
                        DestPort = null!,
                        TotalQuantity = stat.TotalQuantity,
                        Percentage = totalQuantity > 0 ? (double)(stat.TotalQuantity / totalQuantity * 100) : 0
                    });
                }
            }

            if (query.Dimension == "DestPort" || query.Dimension == "Both")
            {
                // 按 DestPort 统计
                var destPortGroups = allData
                    .GroupBy(d => new
                    {
                        DestPort = d.DestPort ?? "NULL",
                        Period = query.StatisticType == "Year"
                            ? d.OutboundDate.Year.ToString()
                            : $"{d.OutboundDate.Year}-{d.OutboundDate.Month:D2}"
                    })
                    .Select(g => new
                    {
                        g.Key.Period,
                        g.Key.DestPort,
                        TotalQuantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                // 计算总数和占比
                var totalQuantity = destPortGroups.Sum(s => s.TotalQuantity);
                foreach (var stat in destPortGroups)
                {
                    statistics.Add(new ProdSerialOutboundStatisticDto
                    {
                        Period = stat.Period,
                        DestCode = null!,
                        DestPort = stat.DestPort == "NULL" ? null! : stat.DestPort,
                        TotalQuantity = stat.TotalQuantity,
                        Percentage = totalQuantity > 0 ? (double)(stat.TotalQuantity / totalQuantity * 100) : 0
                    });
                }
            }

            return Result<List<ProdSerialOutboundStatisticDto>>.Ok(statistics);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "统计产品序列号出库记录失败");
            return Result<List<ProdSerialOutboundStatisticDto>>.Fail($"统计失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 统计产品序列号出库记录（同时返回按年和按月统计清单）
    /// </summary>
    /// <param name="query">统计查询参数</param>
    /// <returns>统计数据结果（包含按年和按月统计清单）</returns>
    public async Task<Result<ProdSerialOutboundStatisticResultDto>> GetStatisticsWithBothAsync(ProdSerialOutboundStatisticQueryDto query)
    {
        _appLog.Information("开始统计产品序列号出库记录（按年和按月），参数: Dimension={Dimension}, StartDate={StartDate}, EndDate={EndDate}",
            query.Dimension ?? string.Empty, query.StartDate.ToString("yyyy-MM-dd"), query.EndDate.ToString("yyyy-MM-dd"));

        try
        {
            var baseQuery = _prodSerialOutboundRepository.AsQueryable()
                .Where(pso => pso.IsDeleted == 0);

            // 应用日期范围过滤
            if (query.StartDate != default(DateTime))
            {
                baseQuery = baseQuery.Where(pso => pso.OutboundDate >= query.StartDate);
            }
            if (query.EndDate != default(DateTime))
            {
                baseQuery = baseQuery.Where(pso => pso.OutboundDate <= query.EndDate);
            }

            // 查询所有符合条件的数据
            var allData = await baseQuery
                .Select(pso => new
                {
                    pso.DestCode,
                    pso.DestPort,
                    pso.Quantity,
                    pso.OutboundDate
                })
                .ToListAsync();

            var result = new ProdSerialOutboundStatisticResultDto();

            // 按年统计
            var yearStatistics = new List<ProdSerialOutboundStatisticDto>();
            if (query.Dimension == "DestCode" || query.Dimension == "Both")
            {
                // 按 DestCode 按年统计
                var destCodeYearGroups = allData
                    .GroupBy(d => new
                    {
                        DestCode = d.DestCode ?? "NULL",
                        Period = d.OutboundDate.Year.ToString()
                    })
                    .Select(g => new
                    {
                        g.Key.Period,
                        g.Key.DestCode,
                        TotalQuantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                var totalQuantity = destCodeYearGroups.Sum(s => s.TotalQuantity);
                foreach (var stat in destCodeYearGroups)
                {
                    yearStatistics.Add(new ProdSerialOutboundStatisticDto
                    {
                        Period = stat.Period,
                        DestCode = stat.DestCode == "NULL" ? null! : stat.DestCode,
                        DestPort = null!,
                        TotalQuantity = stat.TotalQuantity,
                        Percentage = totalQuantity > 0 ? (double)(stat.TotalQuantity / totalQuantity * 100) : 0
                    });
                }
            }

            if (query.Dimension == "DestPort" || query.Dimension == "Both")
            {
                // 按 DestPort 按年统计
                var destPortYearGroups = allData
                    .GroupBy(d => new
                    {
                        DestPort = d.DestPort ?? "NULL",
                        Period = d.OutboundDate.Year.ToString()
                    })
                    .Select(g => new
                    {
                        g.Key.Period,
                        g.Key.DestPort,
                        TotalQuantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                var totalQuantity = destPortYearGroups.Sum(s => s.TotalQuantity);
                foreach (var stat in destPortYearGroups)
                {
                    yearStatistics.Add(new ProdSerialOutboundStatisticDto
                    {
                        Period = stat.Period,
                        DestCode = null!,
                        DestPort = stat.DestPort == "NULL" ? null! : stat.DestPort,
                        TotalQuantity = stat.TotalQuantity,
                        Percentage = totalQuantity > 0 ? (double)(stat.TotalQuantity / totalQuantity * 100) : 0
                    });
                }
            }
            result.YearStatistics = yearStatistics.OrderByDescending(s => s.Period).ToList();

            // 按月统计
            var monthStatistics = new List<ProdSerialOutboundStatisticDto>();
            if (query.Dimension == "DestCode" || query.Dimension == "Both")
            {
                // 按 DestCode 按月统计
                var destCodeMonthGroups = allData
                    .GroupBy(d => new
                    {
                        DestCode = d.DestCode ?? "NULL",
                        Period = $"{d.OutboundDate.Year}-{d.OutboundDate.Month:D2}"
                    })
                    .Select(g => new
                    {
                        g.Key.Period,
                        g.Key.DestCode,
                        TotalQuantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                var totalQuantity = destCodeMonthGroups.Sum(s => s.TotalQuantity);
                foreach (var stat in destCodeMonthGroups)
                {
                    monthStatistics.Add(new ProdSerialOutboundStatisticDto
                    {
                        Period = stat.Period,
                        DestCode = stat.DestCode == "NULL" ? null! : stat.DestCode,
                        DestPort = null!,
                        TotalQuantity = stat.TotalQuantity,
                        Percentage = totalQuantity > 0 ? (double)(stat.TotalQuantity / totalQuantity * 100) : 0
                    });
                }
            }

            if (query.Dimension == "DestPort" || query.Dimension == "Both")
            {
                // 按 DestPort 按月统计
                var destPortMonthGroups = allData
                    .GroupBy(d => new
                    {
                        DestPort = d.DestPort ?? "NULL",
                        Period = $"{d.OutboundDate.Year}-{d.OutboundDate.Month:D2}"
                    })
                    .Select(g => new
                    {
                        g.Key.Period,
                        g.Key.DestPort,
                        TotalQuantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                var totalQuantity = destPortMonthGroups.Sum(s => s.TotalQuantity);
                foreach (var stat in destPortMonthGroups)
                {
                    monthStatistics.Add(new ProdSerialOutboundStatisticDto
                    {
                        Period = stat.Period,
                        DestCode = null!,
                        DestPort = stat.DestPort == "NULL" ? null! : stat.DestPort,
                        TotalQuantity = stat.TotalQuantity,
                        Percentage = totalQuantity > 0 ? (double)(stat.TotalQuantity / totalQuantity * 100) : 0
                    });
                }
            }
            result.MonthStatistics = monthStatistics.OrderByDescending(s => s.Period).ToList();

            return Result<ProdSerialOutboundStatisticResultDto>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "统计产品序列号出库记录（按年和按月）失败");
            return Result<ProdSerialOutboundStatisticResultDto>.Fail($"统计失败: {ex.Message}");
        }
    }

    private static Expression<Func<ProdSerialOutbound, bool>> QueryExpression(ProdSerialOutboundQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdSerialOutbound>()
            .And(pso => pso.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), pso => pso.OutboundNo.Contains(query.Keywords!) ||
                                                                pso.SerialNumber.Contains(query.Keywords!) ||
                                                                pso.FullSerialNumber.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), pso => pso.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.OutboundNo), pso => pso.OutboundNo.Contains(query.OutboundNo!))
            .AndIF(!string.IsNullOrEmpty(query.SerialNumber), pso => pso.SerialNumber.Contains(query.SerialNumber!))
            .AndIF(query.OutboundDateFrom != default(DateTime), pso => pso.OutboundDate >= query.OutboundDateFrom)
            .AndIF(query.OutboundDateTo != default(DateTime), pso => pso.OutboundDate <= query.OutboundDateTo)
            .ToExpression();
    }
}

