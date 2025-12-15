// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：ProdSerialScanningExService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号扫描异常记录服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq;
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
/// 产品序列号扫描异常记录服务实现
/// </summary>
public class ProdSerialScanningExService : IProdSerialScanningExService
{
    private readonly IBaseRepository<ProdSerialScanningEx> _prodSerialScanningExRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    /// <summary>
    /// 初始化产品序列号扫描异常记录服务
    /// </summary>
    /// <param name="prodSerialScanningExRepository">产品序列号扫描异常记录仓储接口</param>
    /// <param name="appLog">应用程序日志管理器</param>
    /// <param name="operLog">操作日志管理器（可选）</param>
    public ProdSerialScanningExService(
        IBaseRepository<ProdSerialScanningEx> prodSerialScanningExRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _prodSerialScanningExRepository = prodSerialScanningExRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询产品序列号扫描异常记录列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码等筛选条件</param>
    /// <returns>分页产品序列号扫描异常记录列表</returns>
    public async Task<Result<PagedResult<ProdSerialScanningExDto>>> GetListAsync(ProdSerialScanningExQueryDto query)
    {
        _appLog.Information("开始查询产品序列号扫描异常记录列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            Expression<Func<ProdSerialScanningEx, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "inbounddate":
                        orderByExpression = psse => psse.InboundDate ?? DateTime.MinValue;
                        break;
                    case "outbounddate":
                        orderByExpression = psse => psse.OutboundDate ?? DateTime.MinValue;
                        break;
                    case "createdtime":
                        orderByExpression = psse => psse.CreatedTime;
                        break;
                    default:
                        orderByExpression = psse => psse.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = psse => psse.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _prodSerialScanningExRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodSerialScanningExDtos = result.Items.Adapt<List<ProdSerialScanningExDto>>();

            var pagedResult = new PagedResult<ProdSerialScanningExDto>
            {
                Items = prodSerialScanningExDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdSerialScanningExDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询产品序列号扫描异常记录数据失败");
            return Result<PagedResult<ProdSerialScanningExDto>>.Fail($"高级查询产品序列号扫描异常记录数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取产品序列号扫描异常记录
    /// </summary>
    /// <param name="id">产品序列号扫描异常记录ID</param>
    /// <returns>产品序列号扫描异常记录数据传输对象</returns>
    public async Task<Result<ProdSerialScanningExDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodSerialScanningEx = await _prodSerialScanningExRepository.GetByIdAsync(id);
            if (prodSerialScanningEx == null)
                return Result<ProdSerialScanningExDto>.Fail("产品序列号扫描异常记录不存在");

            var prodSerialScanningExDto = prodSerialScanningEx.Adapt<ProdSerialScanningExDto>();
            return Result<ProdSerialScanningExDto>.Ok(prodSerialScanningExDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取产品序列号扫描异常记录失败");
            return Result<ProdSerialScanningExDto>.Fail($"获取产品序列号扫描异常记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建入库扫描异常记录
    /// 注意：此方法不做任何验证，只管新增数据，允许重复记录
    /// </summary>
    /// <param name="dto">入库扫描异常记录创建数据传输对象，包含入库完整序列号、入库日期、入库客户端信息、异常描述等</param>
    /// <returns>创建成功的扫描异常记录ID</returns>
    public async Task<Result<long>> CreateInboundScanningExAsync(ProdSerialInboundScanningExCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 不做任何验证，直接创建异常记录
            var prodSerialScanningEx = dto.Adapt<ProdSerialScanningEx>();
            var result = await _prodSerialScanningExRepository.CreateAsync(prodSerialScanningEx);
            var response = result > 0 ? Result<long>.Ok(prodSerialScanningEx.Id) : Result<long>.Fail("创建入库扫描异常记录失败");
            
            _operLog?.LogCreate("ProdSerialScanningEx", prodSerialScanningEx.Id.ToString(), "Logistics.Serials.ProdSerialScanningExView", 
                dto, response, stopwatch);
            
            _appLog.Information("创建入库扫描异常记录成功，ID={Id}, 完整序列号={FullSerialNumber}, 异常描述={Desc}", 
                result, dto.InboundFullSerialNumber, dto.InboundDesc ?? "无");
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建入库扫描异常记录失败，完整序列号={FullSerialNumber}", dto.InboundFullSerialNumber);
            return Result<long>.Fail($"创建入库扫描异常记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建出库扫描异常记录
    /// 注意：此方法不做任何验证，只管新增数据，允许重复记录
    /// </summary>
    /// <param name="dto">出库扫描异常记录创建数据传输对象，包含出库完整序列号、出库单号、仕向地、目的地港口、出库日期、出库客户端信息、异常描述等</param>
    /// <returns>创建成功的扫描异常记录ID</returns>
    public async Task<Result<long>> CreateOutboundScanningExAsync(ProdSerialOutboundScanningExCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 不做任何验证，直接创建异常记录
            var prodSerialScanningEx = dto.Adapt<ProdSerialScanningEx>();
            var result = await _prodSerialScanningExRepository.CreateAsync(prodSerialScanningEx);
            var response = result > 0 ? Result<long>.Ok(prodSerialScanningEx.Id) : Result<long>.Fail("创建出库扫描异常记录失败");
            
            _operLog?.LogCreate("ProdSerialScanningEx", prodSerialScanningEx.Id.ToString(), "Logistics.Serials.ProdSerialScanningExView", 
                dto, response, stopwatch);
            
            _appLog.Information("创建出库扫描异常记录成功，ID={Id}, 完整序列号={FullSerialNumber}, 出库单号={OutboundNo}, 异常描述={Desc}", 
                result, dto.OutboundFullSerialNumber, dto.OutboundNo ?? "无", dto.OutboundDesc ?? "无");
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建出库扫描异常记录失败，完整序列号={FullSerialNumber}, 出库单号={OutboundNo}", 
                dto.OutboundFullSerialNumber, dto.OutboundNo ?? "无");
            return Result<long>.Fail($"创建出库扫描异常记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新产品序列号扫描异常记录
    /// </summary>
    /// <param name="dto">产品序列号扫描异常记录更新数据传输对象</param>
    /// <returns>更新操作结果</returns>
    public async Task<Result> UpdateAsync(ProdSerialScanningExUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialScanningEx = await _prodSerialScanningExRepository.GetByIdAsync(dto.Id);
            if (prodSerialScanningEx == null)
                return Result.Fail("产品序列号扫描异常记录不存在");

            dto.Adapt(prodSerialScanningEx);
            var result = await _prodSerialScanningExRepository.UpdateAsync(prodSerialScanningEx);
            var response = result > 0 ? Result.Ok() : Result.Fail("更新产品序列号扫描异常记录失败");

            _operLog?.LogUpdate("ProdSerialScanningEx", dto.Id.ToString(), "Logistics.Serials.ProdSerialScanningExView", "", dto, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新产品序列号扫描异常记录失败");
            return Result.Fail($"更新产品序列号扫描异常记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除产品序列号扫描异常记录（软删除）
    /// </summary>
    /// <param name="id">产品序列号扫描异常记录ID</param>
    /// <returns>删除操作结果</returns>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var prodSerialScanningEx = await _prodSerialScanningExRepository.GetByIdAsync(id);
            if (prodSerialScanningEx == null)
                return Result.Fail("产品序列号扫描异常记录不存在");

            var result = await _prodSerialScanningExRepository.DeleteAsync(prodSerialScanningEx);
            var response = result > 0 ? Result.Ok() : Result.Fail("删除产品序列号扫描异常记录失败");

            _operLog?.LogDelete("ProdSerialScanningEx", id.ToString(), "Logistics.Serials.ProdSerialScanningExView", 
                new { Id = id }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除产品序列号扫描异常记录失败");
            return Result.Fail($"删除产品序列号扫描异常记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除产品序列号扫描异常记录（软删除）
    /// </summary>
    /// <param name="ids">产品序列号扫描异常记录ID列表</param>
    /// <returns>删除操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            var prodSerialScanningExList = await _prodSerialScanningExRepository.GetListAsync(
                psse => ids.Contains(psse.Id) && psse.IsDeleted == 0);
            
            if (prodSerialScanningExList == null || prodSerialScanningExList.Items == null || prodSerialScanningExList.Items.Count == 0)
                return Result.Fail("未找到要删除的产品序列号扫描异常记录");

            int successCount = 0;
            var deletedInfos = new List<object>();
            
            foreach (var item in prodSerialScanningExList.Items)
            {
                var result = await _prodSerialScanningExRepository.DeleteAsync(item);
                if (result > 0)
                {
                    successCount++;
                    deletedInfos.Add(new { Id = item.Id, InboundFullSerialNumber = item.InboundFullSerialNumber, OutboundFullSerialNumber = item.OutboundFullSerialNumber });
                }
            }

            var response = Result.Ok($"成功删除 {successCount} 条记录");
            
            _operLog?.LogDelete("ProdSerialScanningEx", string.Join(",", ids), "Logistics.Serials.ProdSerialScanningExView", 
                new { Ids = ids, SuccessCount = successCount, DeletedInfos = deletedInfos }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除产品序列号扫描异常记录失败");
            return Result.Fail($"批量删除产品序列号扫描异常记录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// 根据查询条件构建用于数据库查询的表达式树，支持关键字搜索、物料代码过滤、完整序列号匹配、日期范围查询等
    /// </summary>
    /// <param name="query">查询条件对象，包含关键字、物料代码、入库/出库完整序列号、入库/出库日期范围等筛选条件</param>
    /// <returns>查询表达式</returns>
    private Expression<Func<ProdSerialScanningEx, bool>> QueryExpression(ProdSerialScanningExQueryDto query)
    {
        var expression = SqlSugar.Expressionable.Create<ProdSerialScanningEx>()
            .And(psse => psse.IsDeleted == 0);

        // 关键字搜索（在入库/出库完整序列号、异常描述中搜索）
        if (!string.IsNullOrEmpty(query.Keywords))
        {
            var keyword = query.Keywords;
            expression = expression.And(psse => 
                (!string.IsNullOrEmpty(psse.InboundFullSerialNumber) && psse.InboundFullSerialNumber.Contains(keyword)) ||
                (!string.IsNullOrEmpty(psse.OutboundFullSerialNumber) && psse.OutboundFullSerialNumber.Contains(keyword)) ||
                (!string.IsNullOrEmpty(psse.InboundDesc) && psse.InboundDesc.Contains(keyword)) ||
                (!string.IsNullOrEmpty(psse.OutboundDesc) && psse.OutboundDesc.Contains(keyword)));
        }

        // 物料代码过滤（通过完整序列号解析，格式为 MaterialCode-SerialNumber-Quantity）
        if (!string.IsNullOrEmpty(query.MaterialCode))
        {
            var materialCode = query.MaterialCode;
            expression = expression.And(psse => 
                (!string.IsNullOrEmpty(psse.InboundFullSerialNumber) && psse.InboundFullSerialNumber.StartsWith(materialCode + "-")) ||
                (!string.IsNullOrEmpty(psse.OutboundFullSerialNumber) && psse.OutboundFullSerialNumber.StartsWith(materialCode + "-")));
        }

        // 入库完整序列号
        if (!string.IsNullOrEmpty(query.InboundFullSerialNumber))
        {
            expression = expression.And(psse => psse.InboundFullSerialNumber != null && psse.InboundFullSerialNumber.Contains(query.InboundFullSerialNumber));
        }

        // 出库完整序列号
        if (!string.IsNullOrEmpty(query.OutboundFullSerialNumber))
        {
            expression = expression.And(psse => psse.OutboundFullSerialNumber != null && psse.OutboundFullSerialNumber.Contains(query.OutboundFullSerialNumber));
        }

        // 入库日期范围
        if (query.InboundDateFrom != default(DateTime))
        {
            expression = expression.And(psse => psse.InboundDate != null && psse.InboundDate >= query.InboundDateFrom);
        }

        if (query.InboundDateTo != default(DateTime))
        {
            expression = expression.And(psse => psse.InboundDate != null && psse.InboundDate <= query.InboundDateTo);
        }

        // 出库日期范围
        if (query.OutboundDateFrom != default(DateTime))
        {
            expression = expression.And(psse => psse.OutboundDate != null && psse.OutboundDate >= query.OutboundDateFrom);
        }

        if (query.OutboundDateTo != default(DateTime))
        {
            expression = expression.And(psse => psse.OutboundDate != null && psse.OutboundDate <= query.OutboundDateTo);
        }

        return expression.ToExpression();
    }

    /// <summary>
    /// 导出产品序列号扫描异常记录到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号扫描异常记录</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialScanningExQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdSerialScanningEx>().And(x => x.IsDeleted == 0).ToExpression();
            var records = await _prodSerialScanningExRepository.AsQueryable().Where(where).OrderBy(psse => psse.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = records.Adapt<List<ProdSerialScanningExExportDto>>();
            sheetName ??= "ProdSerialScanningEx";
            fileName ??= $"产品序列号扫描异常记录导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出产品序列号扫描异常记录Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }
}

