// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：IDictionaryTypeService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：字典类型服务接口
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using Takt.Application.Dtos.Routine;
using Takt.Common.Models;
using Takt.Common.Results;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 字典类型服务接口
/// </summary>
public interface IDictionaryTypeService
{
    /// <summary>
    /// 查询字典类型列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、类型代码等筛选条件</param>
    /// <returns>分页字典类型列表</returns>
    Task<Result<PagedResult<DictionaryTypeDto>>> GetListAsync(DictionaryTypeQueryDto query);

    /// <summary>
    /// 根据ID获取字典类型（包含字典数据）
    /// </summary>
    Task<Result<DictionaryTypeDto>> GetByIdAsync(long id, bool includeData = false);

    /// <summary>
    /// 根据类型代码获取字典类型（包含字典数据）
    /// </summary>
    Task<Result<DictionaryTypeDto>> GetByCodeAsync(string typeCode, bool includeData = false);

    /// <summary>
    /// 创建字典类型
    /// </summary>
    Task<Result<long>> CreateAsync(DictionaryTypeCreateDto dto);

    /// <summary>
    /// 更新字典类型
    /// </summary>
    Task<Result> UpdateAsync(DictionaryTypeUpdateDto dto);

    /// <summary>
    /// 删除字典类型（同时删除关联的字典数据）
    /// </summary>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除字典类型（同时删除关联的字典数据）
    /// </summary>
    /// <param name="ids">字典类型ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 修改字典类型状态
    /// </summary>
    Task<Result> StatusAsync(long id, int status);

    /// <summary>
    /// 导出字典类型到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的字典类型</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(DictionaryTypeQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出字典类型 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入字典类型
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);

    /// <summary>
    /// 获取字典选项列表
    /// 根据字典类型代码获取选项列表，支持系统字典和SQL脚本字典
    /// </summary>
    /// <param name="typeCode">字典类型代码</param>
    /// <returns>选项列表</returns>
    Task<Result<List<SelectOptionModel>>> GetOptionsAsync(string typeCode);
}
