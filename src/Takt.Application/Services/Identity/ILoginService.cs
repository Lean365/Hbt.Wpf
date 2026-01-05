// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Identity
// 文件名称：ILoginService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：登录服务接口
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Identity;
using Takt.Common.Results;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 登录服务接口
/// 定义用户认证相关的业务操作
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="dto">登录DTO</param>
    /// <returns>登录结果</returns>
    Task<Result<LoginResultDto>> LoginAsync(LoginDto dto);
    
    /// <summary>
    /// 用户登出
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>操作结果</returns>
    Task<Result> LogoutAsync(long userId);
}
