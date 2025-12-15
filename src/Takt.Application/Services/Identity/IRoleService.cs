// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Identity
// 文件名称：IRoleService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：角色服务接口
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Identity;
using Takt.Common.Results;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 角色服务接口
/// 定义角色相关的业务操作
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 查询角色列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、角色名称、角色编码等筛选条件</param>
    /// <returns>分页角色列表</returns>
    Task<Result<PagedResult<RoleDto>>> GetListAsync(RoleQueryDto query);
    
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
    /// 批量删除角色
    /// </summary>
    /// <param name="ids">角色ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
    /// <summary>
    /// 修改角色状态（DTO方式）
    /// </summary>
    /// <param name="dto">角色状态DTO，包含角色ID和状态值</param>
    /// <returns>操作结果</returns>
    Task<Result> StatusAsync(RoleStatusDto dto);

    /// <summary>
    /// 导出角色到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的角色</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(RoleQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出角色 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入角色
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);

    /// <summary>
    /// 获取角色已分配的菜单ID列表
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>菜单ID列表</returns>
    Task<Result<List<long>>> GetRoleMenusAsync(long roleId);

    /// <summary>
    /// 分配菜单给角色
    /// </summary>
    /// <param name="roleId">角色ID，必须大于0</param>
    /// <param name="menuIds">要分配的菜单ID列表，可以为空列表（表示移除所有菜单）</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 分配逻辑：先删除不在新列表中的菜单关联，再添加新列表中的菜单关联
    /// 如果菜单ID列表为空，将移除角色的所有菜单
    /// </remarks>
    Task<Result> AssignMenusAsync(long roleId, List<long> menuIds);
}
