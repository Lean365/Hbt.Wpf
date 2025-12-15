// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Routine
// 文件名称：ISettingService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：系统设置服务接口
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Routine;
using Takt.Common.Results;

namespace Takt.Application.Services.Routine;

/// <summary>
/// 系统设置服务接口
/// </summary>
public interface ISettingService
{
    /// <summary>
    /// 查询系统设置列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、设置键等筛选条件</param>
    /// <returns>分页系统设置列表</returns>
    Task<Result<PagedResult<SettingDto>>> GetListAsync(SettingQueryDto query);

    /// <summary>
    /// 根据ID获取系统设置
    /// </summary>
    /// <param name="id">系统设置ID</param>
    /// <returns>系统设置信息</returns>
    Task<Result<SettingDto>> GetByIdAsync(long id);

    /// <summary>
    /// 根据设置键获取系统设置
    /// </summary>
    /// <param name="settingKey">设置键</param>
    /// <returns>系统设置信息</returns>
    Task<Result<SettingDto>> GetByKeyAsync(string settingKey);

    /// <summary>
    /// 根据分类获取系统设置列表
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>系统设置列表</returns>
    Task<Result<List<SettingDto>>> GetByCategoryAsync(string category);

    /// <summary>
    /// 创建系统设置
    /// </summary>
    /// <param name="dto">创建系统设置DTO</param>
    /// <returns>新系统设置ID</returns>
    Task<Result<long>> CreateAsync(SettingCreateDto dto);

    /// <summary>
    /// 更新系统设置
    /// </summary>
    /// <param name="dto">更新系统设置DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> UpdateAsync(SettingUpdateDto dto);

    /// <summary>
    /// 删除系统设置
    /// </summary>
    /// <param name="id">系统设置ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(long id);

    /// <summary>
    /// 批量删除系统设置
    /// </summary>
    /// <param name="ids">系统设置ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 导出系统设置到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的系统设置</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(SettingQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出系统设置 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入系统设置
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}
