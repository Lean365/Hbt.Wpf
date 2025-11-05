// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：UserDto.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：用户数据传输对象
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

using Hbt.Common.Results;
using Hbt.Common.Enums;

namespace Hbt.Application.Dtos.Identity;

/// <summary>
/// 用户数据传输对象
/// 用于传输用户信息
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;   

    
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string? RealName { get; set; }
    
    /// <summary>
    /// 用户类型（0=系统用户，1=普通用户）
    /// </summary>
    public UserTypeEnum UserType { get; set; }
    
    /// <summary>
    /// 用户性别（0=未知，1=男，2=女）
    /// </summary>
    public UserGenderEnum UserGender { get; set; }
    
    /// <summary>
    /// 用户状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum UserStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedBy { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }
    /// <summary>
    /// 是否删除
    /// </summary>
    public int IsDeleted { get; set; }
    /// <summary>
    /// 删除人
    /// </summary>
    public string? DeletedBy { get; set; }
    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedTime { get; set; }
}

/// <summary>
/// 用户查询数据传输对象
/// 用于查询用户信息
/// </summary>
public class UserQueryDto : PagedQuery
{
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string? RealName { get; set; }
    
    /// <summary>
    /// 用户类型（0=系统用户，1=普通用户）
    /// </summary>
    public UserTypeEnum? UserType { get; set; }
    
    /// <summary>
    /// 用户性别（0=未知，1=男，2=女）
    /// </summary>
    public UserGenderEnum? UserGender { get; set; }
    
    /// <summary>
    /// 用户状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum? UserStatus { get; set; }
    

}

/// <summary>
/// 创建用户数据传输对象
/// 用于创建新用户
/// </summary>
public class UserCreateDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string? RealName { get; set; }
    
    /// <summary>
    /// 用户类型（0=系统用户，1=普通用户）
    /// </summary>
    public UserTypeEnum UserType { get; set; } = 0;
    
    /// <summary>
    /// 用户性别（0=未知，1=男，2=女）
    /// </summary>
    public UserGenderEnum UserGender { get; set; } = 0;
    
    /// <summary>
    /// 用户状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum UserStatus { get; set; } = 0;
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// 更新用户数据传输对象
/// 用于更新用户信息
/// </summary>
public class UserUpdateDto : UserCreateDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 修改密码数据传输对象
/// 用于修改用户密码
/// </summary>
public class UserChangePasswordDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 旧密码
    /// </summary>
    public string OldPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 重置密码数据传输对象（管理员重置，不需要旧密码）
/// </summary>
public class UserResetPasswordDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 用户状态更新 DTO（启用/禁用）
/// </summary>
public class UserStatusDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 新状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum Status { get; set; }
}