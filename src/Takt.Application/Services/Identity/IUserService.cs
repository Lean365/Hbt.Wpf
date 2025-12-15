// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Identity
// 文件名称：IUserService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：用户服务接口
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.   
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Identity;
using Takt.Common.Results;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 用户服务接口
/// 定义用户相关的业务操作
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 查询用户列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、用户名、邮箱、电话、真实姓名、用户类型、用户性别、用户状态等筛选条件</param>
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
    /// 批量删除用户
    /// </summary>
    /// <param name="ids">用户ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteBatchAsync(List<long> ids);
    
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
    /// <param name="query">查询条件对象，可选，用于筛选要导出的用户</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> ExportAsync(UserQueryDto? query = null!, string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 导出用户 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null);

    /// <summary>
    /// 从 Excel 导入用户
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);

    /// <summary>
    /// 获取用户的角色列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>角色ID列表</returns>
    Task<Result<List<long>>> GetUserRolesAsync(long userId);

    /// <summary>
    /// 分配角色给用户
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="roleIds">角色ID列表</param>
    /// <returns>操作结果</returns>
    Task<Result> AssignRolesAsync(long userId, List<long> roleIds);
}
