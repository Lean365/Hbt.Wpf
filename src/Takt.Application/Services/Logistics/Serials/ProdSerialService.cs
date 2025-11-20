// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：ProdSerialService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号服务实现
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
/// 产品序列号服务实现
/// </summary>
public class ProdSerialService : IProdSerialService
{
    private readonly IBaseRepository<ProdSerial> _prodSerialRepository;
    private readonly AppLogManager _appLog;

    public ProdSerialService(
        IBaseRepository<ProdSerial> prodSerialRepository,
        AppLogManager appLog)
    {
        _prodSerialRepository = prodSerialRepository;
        _appLog = appLog;
    }

    /// <summary>
    /// 查询产品序列号列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码等筛选条件</param>
    /// <returns>分页产品序列号列表</returns>
    public async Task<Result<PagedResult<ProdSerialDto>>> GetListAsync(ProdSerialQueryDto query)
    {
        _appLog.Information("开始查询产品序列号列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<ProdSerial, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "materialcode":
                        orderByExpression = ps => ps.MaterialCode;
                        break;
                    case "modelcode":
                        orderByExpression = ps => ps.ModelCode;
                        break;
                    case "createdtime":
                        orderByExpression = ps => ps.CreatedTime;
                        break;
                    default:
                        orderByExpression = ps => ps.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = ps => ps.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _prodSerialRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var prodSerialDtos = result.Items.Adapt<List<ProdSerialDto>>();

            var pagedResult = new PagedResult<ProdSerialDto>
            {
                Items = prodSerialDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<ProdSerialDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询产品序列号数据失败");
            return Result<PagedResult<ProdSerialDto>>.Fail($"高级查询产品序列号数据失败: {ex.Message}");
        }
    }

    public async Task<Result<ProdSerialDto>> GetByIdAsync(long id)
    {
        try
        {
            var prodSerial = await _prodSerialRepository.GetByIdAsync(id);
            if (prodSerial == null)
                return Result<ProdSerialDto>.Fail("产品序列号不存在");

            var prodSerialDto = prodSerial.Adapt<ProdSerialDto>();
            return Result<ProdSerialDto>.Ok(prodSerialDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取产品序列号失败");
            return Result<ProdSerialDto>.Fail($"获取产品序列号失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(ProdSerialCreateDto dto)
    {
        try
        {
            // 检查物料编码是否已存在
            var exists = await _prodSerialRepository.GetFirstAsync(ps => ps.MaterialCode == dto.MaterialCode && ps.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"物料编码 {dto.MaterialCode} 已存在");

            var prodSerial = dto.Adapt<ProdSerial>();
            var result = await _prodSerialRepository.CreateAsync(prodSerial);
            return Result<long>.Ok(result);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "创建产品序列号失败");
            return Result<long>.Fail($"创建产品序列号失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(ProdSerialUpdateDto dto)
    {
        try
        {
            var prodSerial = await _prodSerialRepository.GetByIdAsync(dto.Id);
            if (prodSerial == null)
                return Result.Fail("产品序列号不存在");

            // 检查物料编码是否被其他记录使用
            if (prodSerial.MaterialCode != dto.MaterialCode)
            {
                var exists = await _prodSerialRepository.GetFirstAsync(ps => ps.MaterialCode == dto.MaterialCode && ps.Id != dto.Id && ps.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"物料编码 {dto.MaterialCode} 已被其他记录使用");
            }

            dto.Adapt(prodSerial);
            await _prodSerialRepository.UpdateAsync(prodSerial);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "更新产品序列号失败");
            return Result.Fail($"更新产品序列号失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var prodSerial = await _prodSerialRepository.GetByIdAsync(id);
            if (prodSerial == null)
                return Result.Fail("产品序列号不存在");

            await _prodSerialRepository.DeleteAsync(prodSerial);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "删除产品序列号失败");
            return Result.Fail($"删除产品序列号失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品序列号到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<ProdSerial>().And(x => x.IsDeleted == 0).ToExpression();
            var serials = await _prodSerialRepository.AsQueryable().Where(where).OrderBy(ps => ps.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = serials.Adapt<List<ProdSerialDto>>();
            sheetName ??= "ProdSerials";
            fileName ??= $"产品序列号导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出产品序列号Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出产品序列号 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "ProdSerials";
        fileName ??= $"产品序列号导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<ProdSerialDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入产品序列号
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "ProdSerials";
            var dtos = ExcelHelper.ImportFromExcel<ProdSerialDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.MaterialCode)) { fail++; continue; }
                    var existing = await _prodSerialRepository.GetFirstAsync(ps => ps.MaterialCode == dto.MaterialCode && ps.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<ProdSerial>();
                        await _prodSerialRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _prodSerialRepository.UpdateAsync(existing);
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
            _appLog.Error(ex, "导入产品序列号Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<ProdSerial, bool>> QueryExpression(ProdSerialQueryDto query)
    {
        return SqlSugar.Expressionable.Create<ProdSerial>()
            .And(ps => ps.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), ps => ps.MaterialCode.Contains(query.Keywords!) || 
                                                               ps.ModelCode.Contains(query.Keywords!) || 
                                                               (ps.DestCode != null && ps.DestCode.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.MaterialCode), ps => ps.MaterialCode.Contains(query.MaterialCode!))
            .AndIF(!string.IsNullOrEmpty(query.ModelCode), ps => ps.ModelCode.Contains(query.ModelCode!))
            .AndIF(!string.IsNullOrEmpty(query.DestCode), ps => ps.DestCode != null && ps.DestCode.Contains(query.DestCode!))
            .ToExpression();
    }
}

