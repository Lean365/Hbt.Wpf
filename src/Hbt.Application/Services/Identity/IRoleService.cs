// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：IRoleService.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：角色服务接口
// 
// 版权信息：
// Copyright (c) 2025 黑冰台. All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

using Hbt.Application.Dtos.Identity;
using Hbt.Common.Results;

namespace Hbt.Application.Services.Identity;

/// <summary>
/// 角色服务接口
/// 定义角色相关的业务操作
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 分页查询角色列表
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键字</param>
    /// <returns>分页角色列表</returns>
    Task<Result<PagedResult<RoleDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    
    /// <summary>
    /// 根据ID获取角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>角色信息</returns>
    Task<Result<RoleDto>> GetByIdAsync(long id);
    
    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="dto">创建角色DTO</param>
    /// <returns>新角色ID</returns>
    Task<Result<long>> CreateAsync(RoleCreateDto dto);
    
    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="dto">更新角色DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> UpdateAsync(RoleUpdateDto dto);
    
    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(long id);
    
    /// <summary>
    /// 修改角色状态（DTO方式）
    /// </summary>
    Task<Result> StatusAsync(RoleStatusDto dto);

    /// <summary>
    /// 导出角色到Excel（支持条件查询导出）
    /// </summary>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(RoleQueryDto? query = null, string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 导出角色 Excel 模板
    /// </summary>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入角色
    /// </summary>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}
