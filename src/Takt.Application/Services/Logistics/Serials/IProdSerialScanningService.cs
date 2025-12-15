// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：IProdSerialScanningService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号扫描记录服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logistics.Serials;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Serials;

/// <summary>
/// 产品序列号扫描记录服务接口
/// </summary>
public interface IProdSerialScanningService
{
    /// <summary>
    /// 查询产品序列号扫描记录列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码等筛选条件</param>
    /// <returns>分页产品序列号扫描记录列表</returns>
    Task<Result<PagedResult<ProdSerialScanningDto>>> GetListAsync(ProdSerialScanningQueryDto query);

    /// <summary>
    /// 根据ID获取产品序列号扫描记录
    /// </summary>
    Task<Result<ProdSerialScanningDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建入库扫描记录
    /// </summary>
    Task<Result<long>> CreateInboundScanningAsync(ProdSerialInboundScanningCreateDto dto);

    /// <summary>
    /// 创建出库扫描记录
    /// </summary>
    Task<Result<long>> CreateOutboundScanningAsync(ProdSerialOutboundScanningCreateDto dto);

    /// <summary>
    /// 更新产品序列号扫描记录
    /// </summary>
    /// <param name="dto">产品序列号扫描记录更新数据传输对象</param>
    /// <returns>更新操作结果</returns>
    Task<Result> UpdateAsync(ProdSerialScanningUpdateDto dto);

    /// <summary>
    /// 删除产品序列号扫描记录
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除产品序列号扫描记录
    /// </summary>
    /// <param name="ids">扫描记录ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 导出产品序列号扫描记录到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号扫描记录</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialScanningQueryDto? query = null!, string? sheetName = null!, string? fileName = null);
}

