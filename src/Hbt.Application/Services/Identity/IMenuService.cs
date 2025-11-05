//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : IMenuService.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-21
// 版本号 : 1.0
// 描述    : 菜单服务接口
//===================================================================

using Hbt.Application.Dtos.Identity;
using Hbt.Common.Results;

namespace Hbt.Application.Services.Identity;

/// <summary>
/// 菜单服务接口
/// </summary>
public interface IMenuService
{
    #region 查询操作

    /// <summary>
    /// 根据用户ID获取菜单树
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>菜单树</returns>
    Task<Result<UserMenuTreeDto>> GetUserMenuTreeAsync(long userId);

    /// <summary>
    /// 根据角色ID列表获取菜单树
    /// </summary>
    /// <param name="roleIds">角色ID列表</param>
    /// <returns>菜单树</returns>
    Task<Result<List<MenuDto>>> GetMenuTreeByRolesAsync(List<long> roleIds);

    /// <summary>
    /// 获取所有菜单树（管理员）
    /// </summary>
    /// <returns>完整菜单树</returns>
    Task<Result<List<MenuDto>>> GetAllMenuTreeAsync();

    /// <summary>
    /// 获取菜单列表（分页）
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>分页菜单列表</returns>
    Task<Result<PagedResult<MenuDto>>> GetMenuPagedListAsync(int pageIndex, int pageSize, string? keyword = null);

    /// <summary>
    /// 根据ID获取菜单详情
    /// </summary>
    /// <param name="menuId">菜单ID</param>
    /// <returns>菜单详情</returns>
    Task<Result<MenuDto>> GetMenuByIdAsync(long menuId);

    /// <summary>
    /// 根据用户ID获取权限码列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>权限码列表</returns>
    Task<Result<List<string>>> GetUserPermissionsAsync(long userId);

    #endregion

    #region 创建操作

    /// <summary>
    /// 创建菜单
    /// </summary>
    /// <param name="dto">菜单DTO</param>
    /// <returns>创建结果</returns>
    Task<Result<MenuDto>> CreateMenuAsync(MenuCreateDto dto);

    /// <summary>
    /// 批量创建菜单
    /// </summary>
    /// <param name="dtos">菜单DTO列表</param>
    /// <returns>创建结果</returns>
    Task<Result<List<MenuDto>>> CreateMenuBatchAsync(List<MenuCreateDto> dtos);

    #endregion

    #region 更新操作

    /// <summary>
    /// 更新菜单
    /// </summary>
    /// <param name="menuId">菜单ID</param>
    /// <param name="dto">菜单DTO</param>
    /// <returns>更新结果</returns>
    Task<Result<MenuDto>> UpdateMenuAsync(long menuId, MenuUpdateDto dto);

    /// <summary>
    /// 更新菜单状态（DTO方式）
    /// </summary>
    Task<Result> UpdateMenuStatusAsync(MenuStatusDto dto);

    /// <summary>
    /// 调整菜单排序（DTO方式）
    /// </summary>
    Task<Result> UpdateMenuOrderAsync(MenuOrderDto dto);

    #endregion

    #region 删除操作

    /// <summary>
    /// 删除菜单
    /// </summary>
    /// <param name="menuId">菜单ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteMenuAsync(long menuId);

    /// <summary>
    /// 批量删除菜单
    /// </summary>
    /// <param name="menuIds">菜单ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteMenuBatchAsync(List<long> menuIds);

    #endregion

    #region 导入导出（仅 Excel）

    /// <summary>
    /// 导出菜单到Excel
    /// </summary>
    /// <param name="menuIds">菜单ID列表（为空则导出全部）</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="fileName">文件名</param>
    /// <returns>文件名和文件内容</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(List<long>? menuIds = null, string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 导出菜单 Excel 模板（仅表头，双行表头）
    /// </summary>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入菜单
    /// </summary>
    /// <param name="fileStream">Excel 文件流</param>
    /// <param name="sheetName">工作表名称</param>
    /// <returns>成功和失败数量</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);

    #endregion
}

