// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Dtos.Identity
// 文件名称：UserDto.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：用户数据传输对象
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Common.Results;
using Takt.Common.Enums;

namespace Takt.Application.Dtos.Identity;

/// <summary>
/// 用户数据传输对象
/// 用于传输用户信息
/// </summary>
public class UserDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public UserDto()
    {
        Username = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Avatar = string.Empty;
        RealName = string.Empty;
        Nickname = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; }
    
    /// <summary>
    /// 用户头像
    /// </summary>
    public string Avatar { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }
    
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
    public string Remarks { get; set; }


    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdatedBy { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }
    
    /// <summary>
    /// 是否删除（0=否，1=是）
    /// </summary>
    public int IsDeleted { get; set; }
    
    /// <summary>
    /// 删除人
    /// </summary>
    public string DeletedBy { get; set; }
    
    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime DeletedTime { get; set; }
}

/// <summary>
/// 用户查询数据传输对象
/// 用于查询用户信息
/// </summary>
public class UserQueryDto : PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public UserQueryDto()
    {
        Keywords = string.Empty;
        Username = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        RealName = string.Empty;
        Nickname = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在用户名、真实姓名、邮箱、电话中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }
    
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
    /// 构造函数：初始化默认值
    /// </summary>
    public UserCreateDto()
    {
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        RealName = string.Empty;
        Nickname = string.Empty;
        Avatar = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }
    
    /// <summary>
    /// 用户类型（0=系统用户，1=普通用户）
    /// </summary>
    public UserTypeEnum UserType { get; set; } = 0;
    
    /// <summary>
    /// 用户性别（0=未知，1=男，2=女）
    /// </summary>
    public UserGenderEnum UserGender { get; set; } = 0;
    
    /// <summary>
    /// 用户头像
    /// </summary>
    public string Avatar { get; set; }
    
    /// <summary>
    /// 用户状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum UserStatus { get; set; } = 0;
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
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
    /// 构造函数：初始化默认值
    /// </summary>
    public UserChangePasswordDto()
    {
        OldPassword = string.Empty;
        NewPassword = string.Empty;
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 旧密码
    /// </summary>
    public string OldPassword { get; set; }
    
    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; }
}

/// <summary>
/// 重置密码数据传输对象（管理员重置，不需要旧密码）
/// </summary>
public class UserResetPasswordDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public UserResetPasswordDto()
    {
        NewPassword = string.Empty;
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; }
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

/// <summary>
/// 用户导出数据传输对象
/// </summary>
public class UserExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public UserExportDto()
    {
        Username = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        RealName = string.Empty;
        Nickname = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }
    
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
    public string Remarks { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 用户导入模板数据传输对象
/// </summary>
public class UserTemplateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public UserTemplateDto()
    {
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        RealName = string.Empty;
        Nickname = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }
    
    /// <summary>
    /// 用户类型（0=系统用户，1=普通用户）
    /// </summary>
    public int UserType { get; set; }
    
    /// <summary>
    /// 用户性别（0=未知，1=男，2=女）
    /// </summary>
    public int UserGender { get; set; }
    
    /// <summary>
    /// 用户状态（0=启用，1=禁用）
    /// </summary>
    public int UserStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 用户导入数据传输对象
/// </summary>
public class UserImportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public UserImportDto()
    {
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        RealName = string.Empty;
        Nickname = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// 手机号码
    /// </summary>
    public string Phone { get; set; }
    
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; }
    
    /// <summary>
    /// 用户类型（0=系统用户，1=普通用户）
    /// </summary>
    public int UserType { get; set; }
    
    /// <summary>
    /// 用户性别（0=未知，1=男，2=女）
    /// </summary>
    public int UserGender { get; set; }
    
    /// <summary>
    /// 用户状态（0=启用，1=禁用）
    /// </summary>
    public int UserStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}