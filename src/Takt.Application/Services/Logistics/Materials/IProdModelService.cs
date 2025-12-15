// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Materials
// 文件名称：IProdModelService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品机种服务接口
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Logistics.Materials;
using Takt.Common.Models;
using Takt.Common.Results;

namespace Takt.Application.Services.Logistics.Materials;

/// <summary>
/// 产品机种服务接口
/// </summary>
public interface IProdModelService
{
    /// <summary>
    /// 查询产品机种列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、机种代码等筛选条件</param>
    /// <returns>分页产品机种列表</returns>
    Task<Result<PagedResult<ProdModelDto>>> GetListAsync(ProdModelQueryDto query);

    /// <summary>
    /// 根据ID获取产品机种
    /// </summary>
    Task<Result<ProdModelDto>> GetByIdAsync(long id);

    /// <summary>
    /// 创建产品机种
    /// </summary>
    Task<Result<long>> CreateAsync(ProdModelCreateDto dto);

    /// <summary>
    /// 更新产品机种
    /// </summary>
    Task<Result> UpdateAsync(ProdModelUpdateDto dto);

    /// <summary>
    /// 删除产品机种
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除产品机种
    /// </summary>
    /// <param name="ids">产品机种ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 导出产品机种到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的产品机种</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(ProdModelQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出产品机种 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入产品机种
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);

    /// <summary>
    /// 获取机种选项列表
    /// 从产品机种表中获取所有不重复的机种编码，用于下拉列表等UI组件
    /// </summary>
    /// <returns>机种选项列表</returns>
    Task<Result<List<SelectOptionModel>>> GetModelOptionAsync();

    /// <summary>
    /// 获取仕向地选项列表
    /// 从产品机种表中获取所有不重复的仕向编码，用于下拉列表等UI组件
    /// </summary>
    /// <returns>仕向地选项列表</returns>
    Task<Result<List<SelectOptionModel>>> GetDestOptionAsync();
}

