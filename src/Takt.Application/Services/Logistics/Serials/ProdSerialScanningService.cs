// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：ProdSerialScanningService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号扫描记录服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Mapster;
using Newtonsoft.Json;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Serials;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Logistics.Serials;

/// <summary>
/// 产品序列号扫描记录服务实现
/// </summary>
public class ProdSerialScanningService : IProdSerialScanningService
{
    private readonly IBaseRepository<ProdSerialScanning> _prodSerialScanningRepository;
    private readonly IProdSerialScanningExService _prodSerialScanningExService;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    /// <summary>
    /// 初始化产品序列号扫描记录服务
    /// </summary>
    /// <param name="prodSerialScanningRepository">产品序列号扫描记录仓储接口</param>
    /// <param name="prodSerialScanningExService">产品序列号扫描异常记录服务接口</param>
    /// <param name="appLog">应用程序日志管理器</param>
    /// <param name="operLog">操作日志管理器（可选）</param>
    public ProdSerialScanningService(
        IBaseRepository<ProdSerialScanning> prodSerialScanningRepository,
        IProdSerialScanningExService prodSerialScanningExService,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _prodSerialScanningRepository = prodSerialScanningRepository;
        _prodSerialScanningExService = prodSerialScanningExService;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询产品序列号扫描记录列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码等筛选条件</param>
    /// <returns>分页产品序列号扫描记录列表</returns>
    public async Task<Result<PagedResult<ProdSerialScanningDto>>> GetListAsync(ProdSerialScanningQueryDto query)
    {
        _appLog.Information("开始查询产品序列号扫描记录列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            Expression<Func<ProdSerialScanning, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "inbounddate":
                        orderByExpression = pss => pss.InboundDate ?? DateTime.MinValue;
                        break;
                    case "outbounddate":
                        orderByExpression = pss => pss.OutboundDate ?? DateTime.MinValue;
                        break;
                    case "createdtime":
                        orderByExpression = pss => pss.CreatedTime;
                        break;
                    default:
                        orderByExpression = pss => pss.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = pss => pss.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _prodSerialScanningRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodSerialScanningDtos = result.Items.Adapt<List<ProdSerialScanningDto>>();

            var pagedResult = new PagedResult<ProdSerialScanningDto>
            {
                Items = prodSerialScanningDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdSerialScanningDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询产品序列号扫描记录数据失败");
            return Result<PagedResult<ProdSerialScanningDto>>.Fail($"高级查询产品序列号扫描记录数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取产品序列号扫描记录
    /// </summary>
    /// <param name="id">产品序列号扫描记录ID</param>
    /// <returns>产品序列号扫描记录数据传输对象</returns>
    public async Task<Result<ProdSerialScanningDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodSerialScanning = await _prodSerialScanningRepository.GetByIdAsync(id);
            if (prodSerialScanning == null)
                return Result<ProdSerialScanningDto>.Fail("产品序列号扫描记录不存在");

            var prodSerialScanningDto = prodSerialScanning.Adapt<ProdSerialScanningDto>();
            return Result<ProdSerialScanningDto>.Ok(prodSerialScanningDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取产品序列号扫描记录失败");
            return Result<ProdSerialScanningDto>.Fail($"获取产品序列号扫描记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建入库扫描记录
    /// </summary>
    /// <param name="dto">入库扫描记录创建数据传输对象，包含入库完整序列号、入库日期、入库客户端信息等</param>
    /// <returns>创建成功的扫描记录ID</returns>
    public async Task<Result<long>> CreateInboundScanningAsync(ProdSerialInboundScanningCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查入库完整序列号是否已存在（避免重复入库）
            if (!string.IsNullOrWhiteSpace(dto.InboundFullSerialNumber))
            {
                var exists = await _prodSerialScanningRepository.GetFirstAsync(
                    pss => pss.InboundFullSerialNumber == dto.InboundFullSerialNumber && pss.IsDeleted == 0);
                if (exists != null)
                {
                    var errorMessage = $"入库完整序列号 {dto.InboundFullSerialNumber} 已存在，不能重复录入";
                    _appLog.Warning("创建入库扫描记录失败：{ErrorMessage}", errorMessage);
                    
                    // 记录异常到异常表
                    try
                    {
                        var exDto = new ProdSerialInboundScanningExCreateDto
                        {
                            InboundFullSerialNumber = dto.InboundFullSerialNumber,
                            InboundDate = dto.InboundDate,
                            InboundClient = dto.InboundClient,
                            InboundIp = dto.InboundIp,
                            InboundMachineName = dto.InboundMachineName,
                            InboundLocation = dto.InboundLocation,
                            InboundOs = dto.InboundOs,
                            InboundDesc = errorMessage
                        };
                        await _prodSerialScanningExService.CreateInboundScanningExAsync(exDto);
                    }
                    catch (Exception exEx)
                    {
                        _appLog.Error(exEx, "记录入库扫描异常失败，完整序列号={FullSerialNumber}", dto.InboundFullSerialNumber);
                    }
                    
                    return Result<long>.Fail(errorMessage);
                }
            }

            var prodSerialScanning = dto.Adapt<ProdSerialScanning>();
            var result = await _prodSerialScanningRepository.CreateAsync(prodSerialScanning);
            Result<long> response = Result<long>.Ok(prodSerialScanning.Id);
            
            _operLog?.LogCreate("ProdSerialScanning", prodSerialScanning.Id.ToString(), "Logistics.Serials.ProdSerialScanningView", 
                dto, response, stopwatch);
            
            _appLog.Information("创建入库扫描记录成功，ID={Id}, 完整序列号={FullSerialNumber}", result, dto.InboundFullSerialNumber);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMessage = $"创建入库扫描记录失败: {ex.Message}";
            _appLog.Error(ex, "创建入库扫描记录失败，完整序列号={FullSerialNumber}", dto.InboundFullSerialNumber);
            
            // 记录异常到异常表
            try
            {
                var exDto = new ProdSerialInboundScanningExCreateDto
                {
                    InboundFullSerialNumber = dto.InboundFullSerialNumber,
                    InboundDate = dto.InboundDate,
                    InboundClient = dto.InboundClient,
                    InboundIp = dto.InboundIp,
                    InboundMachineName = dto.InboundMachineName,
                    InboundLocation = dto.InboundLocation,
                    InboundOs = dto.InboundOs,
                    InboundDesc = errorMessage
                };
                await _prodSerialScanningExService.CreateInboundScanningExAsync(exDto);
            }
            catch (Exception exEx)
            {
                _appLog.Error(exEx, "记录入库扫描异常失败，完整序列号={FullSerialNumber}", dto.InboundFullSerialNumber);
            }
            
            return Result<long>.Fail(errorMessage);
        }
    }

    /// <summary>
    /// 创建出库扫描记录
    /// 先验证是否存在入库记录，再验证是否已重复出库
    /// 如果该序列号已有入库扫描记录且没有出库信息，则更新该记录添加出库信息；否则创建新的出库扫描记录
    /// </summary>
    /// <param name="dto">出库扫描记录创建数据传输对象，包含出库完整序列号、出库单号、仕向地、目的地港口、出库日期、出库客户端信息等</param>
    /// <returns>创建或更新成功的扫描记录ID</returns>
    public async Task<Result<long>> CreateOutboundScanningAsync(ProdSerialOutboundScanningCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 第一步：验证是否存在入库记录（通过入库完整序列号匹配）
            var existingScanning = await _prodSerialScanningRepository.GetFirstAsync(
                pss => pss.InboundFullSerialNumber == dto.OutboundFullSerialNumber && pss.IsDeleted == 0);
            
            if (existingScanning != null)
            {
                // 第二步：验证是否已重复出库（检查是否已有出库信息）
                if (!string.IsNullOrWhiteSpace(existingScanning.OutboundFullSerialNumber))
                {
                    var errorMessage = $"该序列号已有出库信息，不能重复录入";
                    _appLog.Warning("创建出库扫描记录失败：{ErrorMessage}，完整序列号={FullSerialNumber}", errorMessage, dto.OutboundFullSerialNumber);
                    
                    // 记录异常到异常表
                    try
                    {
                        var exDto = new ProdSerialOutboundScanningExCreateDto
                        {
                            OutboundNo = dto.OutboundNo,
                            DestCode = dto.DestCode,
                            DestPort = dto.DestPort,
                            OutboundFullSerialNumber = dto.OutboundFullSerialNumber,
                            OutboundDate = dto.OutboundDate,
                            OutboundClient = dto.OutboundClient,
                            OutboundIp = dto.OutboundIp,
                            OutboundMachineName = dto.OutboundMachineName,
                            OutboundLocation = dto.OutboundLocation,
                            OutboundOs = dto.OutboundOs,
                            OutboundDesc = errorMessage
                        };
                        await _prodSerialScanningExService.CreateOutboundScanningExAsync(exDto);
                    }
                    catch (Exception exEx)
                    {
                        _appLog.Error(exEx, "记录出库扫描异常失败，完整序列号={FullSerialNumber}", dto.OutboundFullSerialNumber);
                    }
                    
                    return Result<long>.Fail(errorMessage);
                }

                // 更新已有记录，添加出库信息
                existingScanning.OutboundNo = dto.OutboundNo;
                existingScanning.DestCode = dto.DestCode;
                existingScanning.DestPort = dto.DestPort;
                existingScanning.OutboundFullSerialNumber = dto.OutboundFullSerialNumber;
                existingScanning.OutboundDate = dto.OutboundDate;
                existingScanning.OutboundClient = dto.OutboundClient;
                existingScanning.OutboundIp = dto.OutboundIp;
                existingScanning.OutboundMachineName = dto.OutboundMachineName;
                existingScanning.OutboundLocation = dto.OutboundLocation;
                existingScanning.OutboundOs = dto.OutboundOs;
                
                await _prodSerialScanningRepository.UpdateAsync(existingScanning);
                Result<long> response = Result<long>.Ok(existingScanning.Id);
                var changes = $"添加出库信息：出库单号={dto.OutboundNo}";
                
                _operLog?.LogUpdate("ProdSerialScanning", existingScanning.Id.ToString(), "Logistics.Serials.ProdSerialScanningView", changes,
                    new { OutboundFullSerialNumber = dto.OutboundFullSerialNumber, OutboundNo = dto.OutboundNo }, response, stopwatch);
                
                _appLog.Information("更新出库扫描记录成功，ID={Id}, 完整序列号={FullSerialNumber}, 出库单号={OutboundNo}", 
                    existingScanning.Id, dto.OutboundFullSerialNumber, dto.OutboundNo);
                return response;
            }
            else
            {
                stopwatch.Stop();
                // 如果没有入库记录，先记录为异常（该序列号尚未入库）
                var errorMessage = $"该序列号尚未入库，无法出库";
                _appLog.Warning("创建出库扫描记录失败：{ErrorMessage}，完整序列号={FullSerialNumber}", errorMessage, dto.OutboundFullSerialNumber);
                
                // 记录异常到异常表
                try
                {
                    var exDto = new ProdSerialOutboundScanningExCreateDto
                    {
                        OutboundNo = dto.OutboundNo,
                        DestCode = dto.DestCode,
                        DestPort = dto.DestPort,
                        OutboundFullSerialNumber = dto.OutboundFullSerialNumber,
                        OutboundDate = dto.OutboundDate,
                        OutboundClient = dto.OutboundClient,
                        OutboundIp = dto.OutboundIp,
                        OutboundMachineName = dto.OutboundMachineName,
                        OutboundLocation = dto.OutboundLocation,
                        OutboundOs = dto.OutboundOs,
                        OutboundDesc = errorMessage
                    };
                    await _prodSerialScanningExService.CreateOutboundScanningExAsync(exDto);
                }
                catch (Exception exEx)
                {
                    _appLog.Error(exEx, "记录出库扫描异常失败，完整序列号={FullSerialNumber}", dto.OutboundFullSerialNumber);
                }
                
                return Result<long>.Fail(errorMessage);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMessage = $"创建出库扫描记录失败: {ex.Message}";
            _appLog.Error(ex, "创建出库扫描记录失败，完整序列号={FullSerialNumber}, 出库单号={OutboundNo}", 
                dto.OutboundFullSerialNumber, dto.OutboundNo);
            
            // 记录异常到异常表
            try
            {
                var exDto = new ProdSerialOutboundScanningExCreateDto
                {
                    OutboundNo = dto.OutboundNo,
                    DestCode = dto.DestCode,
                    DestPort = dto.DestPort,
                    OutboundFullSerialNumber = dto.OutboundFullSerialNumber,
                    OutboundDate = dto.OutboundDate,
                    OutboundClient = dto.OutboundClient,
                    OutboundIp = dto.OutboundIp,
                    OutboundMachineName = dto.OutboundMachineName,
                    OutboundLocation = dto.OutboundLocation,
                    OutboundOs = dto.OutboundOs,
                    OutboundDesc = errorMessage
                };
                await _prodSerialScanningExService.CreateOutboundScanningExAsync(exDto);
            }
            catch (Exception exEx)
            {
                _appLog.Error(exEx, "记录出库扫描异常失败，完整序列号={FullSerialNumber}", dto.OutboundFullSerialNumber);
            }
            
            return Result<long>.Fail(errorMessage);
        }
    }

    /// <summary>
    /// 更新产品序列号扫描记录
    /// </summary>
    /// <param name="dto">产品序列号扫描记录更新数据传输对象</param>
    /// <returns>更新操作结果</returns>
    public async Task<Result> UpdateAsync(ProdSerialScanningUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialScanning = await _prodSerialScanningRepository.GetByIdAsync(dto.Id);
            if (prodSerialScanning == null)
                return Result.Fail("产品序列号扫描记录不存在");

            dto.Adapt(prodSerialScanning);
            var result = await _prodSerialScanningRepository.UpdateAsync(prodSerialScanning);
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新产品序列号扫描记录失败");
            
            _operLog?.LogUpdate("ProdSerialScanning", dto.Id.ToString(), "Logistics.Serials.ProdSerialScanningView", "", dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新产品序列号扫描记录失败");
            return Result.Fail($"更新产品序列号扫描记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除产品序列号扫描记录（软删除）
    /// </summary>
    /// <param name="id">产品序列号扫描记录ID</param>
    /// <returns>删除操作结果</returns>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialScanning = await _prodSerialScanningRepository.GetByIdAsync(id);
            if (prodSerialScanning == null)
                return Result.Fail("产品序列号扫描记录不存在");

            var result = await _prodSerialScanningRepository.DeleteAsync(prodSerialScanning);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除产品序列号扫描记录失败");
            
            _operLog?.LogDelete("ProdSerialScanning", id.ToString(), "Logistics.Serials.ProdSerialScanningView", 
                new { Id = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除产品序列号扫描记录失败");
            return Result.Fail($"删除产品序列号扫描记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除产品序列号扫描记录
    /// </summary>
    /// <param name="ids">扫描记录ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            var result = await _prodSerialScanningRepository.DeleteBatchAsync(ids.Cast<object>().ToList());
            var response = result > 0 ? Result.Ok($"成功删除 {result} 条记录") : Result.Fail("批量删除产品序列号扫描记录失败");
            
            _operLog?.LogDelete("ProdSerialScanning", string.Join(",", ids), "Logistics.Serials.ProdSerialScanningView", 
                new { Ids = ids, Count = ids.Count }, response, stopwatch);

            if (result > 0)
            {
                _appLog.Information("批量删除产品序列号扫描记录成功，共删除 {Count} 条记录", result);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除产品序列号扫描记录失败");
            return Result.Fail($"批量删除产品序列号扫描记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品序列号扫描记录到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号扫描记录</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialScanningQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdSerialScanning>().And(x => x.IsDeleted == 0).ToExpression();
            var records = await _prodSerialScanningRepository.AsQueryable().Where(where).OrderBy(pss => pss.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = records.Adapt<List<ProdSerialScanningExportDto>>();
            sheetName ??= "ProdSerialScanning";
            fileName ??= $"产品序列号扫描记录导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出产品序列号扫描记录Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// 根据查询条件构建用于数据库查询的表达式树，支持关键字搜索、物料代码过滤、完整序列号匹配、日期范围查询等
    /// </summary>
    /// <param name="query">查询条件对象，包含关键字、物料代码、入库/出库完整序列号、入库/出库日期范围等筛选条件</param>
    /// <returns>查询表达式</returns>
    private Expression<Func<ProdSerialScanning, bool>> QueryExpression(ProdSerialScanningQueryDto query)
    {
        var expression = SqlSugar.Expressionable.Create<ProdSerialScanning>()
            .And(pss => pss.IsDeleted == 0);

        // 关键字搜索（在入库/出库完整序列号中搜索）
        if (!string.IsNullOrEmpty(query.Keywords))
        {
            var keyword = query.Keywords;
            expression = expression.And(pss => 
                (!string.IsNullOrEmpty(pss.InboundFullSerialNumber) && pss.InboundFullSerialNumber.Contains(keyword)) ||
                (!string.IsNullOrEmpty(pss.OutboundFullSerialNumber) && pss.OutboundFullSerialNumber.Contains(keyword)));
        }

        // 物料代码过滤（通过完整序列号解析，格式为 MaterialCode-SerialNumber-Quantity）
        if (!string.IsNullOrEmpty(query.MaterialCode))
        {
            var materialCode = query.MaterialCode;
            expression = expression.And(pss => 
                (!string.IsNullOrEmpty(pss.InboundFullSerialNumber) && pss.InboundFullSerialNumber.StartsWith(materialCode + "-")) ||
                (!string.IsNullOrEmpty(pss.OutboundFullSerialNumber) && pss.OutboundFullSerialNumber.StartsWith(materialCode + "-")));
        }

        // 入库完整序列号
        if (!string.IsNullOrEmpty(query.InboundFullSerialNumber))
        {
            expression = expression.And(pss => pss.InboundFullSerialNumber != null && pss.InboundFullSerialNumber.Contains(query.InboundFullSerialNumber));
        }

        // 出库完整序列号
        if (!string.IsNullOrEmpty(query.OutboundFullSerialNumber))
        {
            expression = expression.And(pss => pss.OutboundFullSerialNumber != null && pss.OutboundFullSerialNumber.Contains(query.OutboundFullSerialNumber));
        }

        // 入库日期范围
        if (query.InboundDateFrom != default(DateTime))
        {
            expression = expression.And(pss => pss.InboundDate != null && pss.InboundDate >= query.InboundDateFrom);
        }

        if (query.InboundDateTo != default(DateTime))
        {
            expression = expression.And(pss => pss.InboundDate != null && pss.InboundDate <= query.InboundDateTo);
        }

        // 出库日期范围
        if (query.OutboundDateFrom != default(DateTime))
        {
            expression = expression.And(pss => pss.OutboundDate != null && pss.OutboundDate >= query.OutboundDateFrom);
        }

        if (query.OutboundDateTo != default(DateTime))
        {
            expression = expression.And(pss => pss.OutboundDate != null && pss.OutboundDate <= query.OutboundDateTo);
        }

        return expression.ToExpression();
    }
}

