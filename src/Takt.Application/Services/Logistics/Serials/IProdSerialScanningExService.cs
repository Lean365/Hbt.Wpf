// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：IProdSerialScanningExService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号扫描异常记录服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logistics.Serials;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Serials;

/// <summary>
/// 产品序列号扫描异常记录服务接口
/// </summary>
public interface IProdSerialScanningExService
{
    /// <summary>
    /// 查询产品序列号扫描异常记录列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码等筛选条件</param>
    /// <returns>分页产品序列号扫描异常记录列表</returns>
    Task<Result<PagedResult<ProdSerialScanningExDto>>> GetListAsync(ProdSerialScanningExQueryDto query);

    /// <summary>
    /// 根据ID获取产品序列号扫描异常记录
    /// </summary>
    /// <param name="id">产品序列号扫描异常记录ID</param>
    /// <returns>产品序列号扫描异常记录数据传输对象</returns>
    Task<Result<ProdSerialScanningExDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建入库扫描异常记录
    /// </summary>
    /// <param name="dto">入库扫描异常记录创建数据传输对象，包含入库完整序列号、入库日期、入库客户端信息、异常描述等</param>
    /// <returns>创建成功的扫描异常记录ID</returns>
    Task<Result<long>> CreateInboundScanningExAsync(ProdSerialInboundScanningExCreateDto dto);

    /// <summary>
    /// 创建出库扫描异常记录
    /// </summary>
    /// <param name="dto">出库扫描异常记录创建数据传输对象，包含出库完整序列号、出库单号、仕向地、目的地港口、出库日期、出库客户端信息、异常描述等</param>
    /// <returns>创建成功的扫描异常记录ID</returns>
    Task<Result<long>> CreateOutboundScanningExAsync(ProdSerialOutboundScanningExCreateDto dto);

    /// <summary>
    /// 更新产品序列号扫描异常记录
    /// </summary>
    /// <param name="dto">产品序列号扫描异常记录更新数据传输对象</param>
    /// <returns>更新操作结果</returns>
    Task<Result> UpdateAsync(ProdSerialScanningExUpdateDto dto);

    /// <summary>
    /// 删除产品序列号扫描异常记录（软删除）
    /// </summary>
    /// <param name="id">产品序列号扫描异常记录ID</param>
    /// <returns>删除操作结果</returns>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除产品序列号扫描异常记录（软删除）
    /// </summary>
    /// <param name="ids">产品序列号扫描异常记录ID列表</param>
    /// <returns>删除操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);

    /// <summary>
    /// 导出产品序列号扫描异常记录到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号扫描异常记录</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialScanningExQueryDto? query = null!, string? sheetName = null!, string? fileName = null);
}

