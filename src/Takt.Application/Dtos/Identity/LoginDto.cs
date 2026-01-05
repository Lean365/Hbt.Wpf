// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Dtos.Identity
// 文件名称：LoginDto.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：登录数据传输对象
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Identity;

/// <summary>
/// 登录请求数据传输对象
/// 用于用户登录验证
/// </summary>
public class LoginDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public LoginDto()
    {
        Username = string.Empty;
        Password = string.Empty;
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
    /// 是否记住密码
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// 登录结果数据传输对象
/// 用于返回登录成功后的用户信息
/// </summary>
public class LoginResultDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public LoginResultDto()
    {
        Username = string.Empty;
        RealName = string.Empty;
        RoleName = string.Empty;
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        Menus = new List<MenuDto>();
        Permissions = new List<string>();
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long? RoleId { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 菜单树
    /// </summary>
    public List<MenuDto> Menus { get; set; }

    /// <summary>
    /// 权限码列表
    /// </summary>
    public List<string> Permissions { get; set; }
}

