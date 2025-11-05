// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：IUserService.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：用户服务接口
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
/// 用户服务接口
/// 定义用户相关的业务操作
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键字</param>
    /// <returns>分页用户列表</returns>
    Task<Result<PagedResult<UserDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    
    /// <summary>
    /// 高级查询用户列表
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>分页用户列表</returns>
    Task<Result<PagedResult<UserDto>>> GetListAsync(UserQueryDto query);
    
    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息</returns>
    Task<Result<UserDto>> GetByIdAsync(long id);
    
    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="dto">创建用户DTO</param>
    /// <returns>新用户ID</returns>
    Task<Result<long>> CreateAsync(UserCreateDto dto);
    
    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="dto">更新用户DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> UpdateAsync(UserUpdateDto dto);
    
    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(long id);
    
    /// <summary>
    /// 修改密码（用户自助）
    /// </summary>
    /// <param name="dto">修改密码DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> ChangePasswordAsync(UserChangePasswordDto dto);

    /// <summary>
    /// 重置密码（管理员）
    /// </summary>
    /// <param name="dto">重置密码DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> ResetPasswordAsync(UserResetPasswordDto dto);

    /// <summary>
    /// 修改用户状态（DTO 方式）
    /// </summary>
    /// <param name="dto">状态DTO</param>
    /// <returns>操作结果</returns>
    Task<Result> StatusAsync(UserStatusDto dto);

    /// <summary>
    /// 导出用户到Excel（支持条件查询导出）
    /// </summary>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(UserQueryDto? query = null, string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 导出用户 Excel 模板
    /// </summary>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入用户
    /// </summary>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}
