// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Serials
// 文件名称：IProdSerialOutboundService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号出库服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Logistics.Serials;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Serials;

/// <summary>
/// 产品序列号出库服务接口
/// </summary>
public interface IProdSerialOutboundService
{
    /// <summary>
    /// 查询产品序列号出库记录列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、物料代码、出库单号等筛选条件</param>
    /// <returns>分页产品序列号出库记录列表</returns>
    Task<Result<PagedResult<ProdSerialOutboundDto>>> GetListAsync(ProdSerialOutboundQueryDto query);

    /// <summary>
    /// 根据ID获取产品序列号出库记录
    /// </summary>
    Task<Result<ProdSerialOutboundDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建产品序列号出库记录
    /// </summary>
    Task<Result<long>> CreateAsync(ProdSerialOutboundCreateDto dto);

    /// <summary>
    /// 更新产品序列号出库记录
    /// </summary>
    Task<Result> UpdateAsync(ProdSerialOutboundUpdateDto dto);

    /// <summary>
    /// 删除产品序列号出库记录
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除产品序列号出库记录
    /// </summary>
    Task<Result> DeleteRangeAsync(List<long> ids);

    /// <summary>
    /// 导出产品序列号出库记录到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品序列号出库记录</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdSerialOutboundQueryDto? query = null, string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 导出产品序列号出库记录 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入产品序列号出库记录
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}

