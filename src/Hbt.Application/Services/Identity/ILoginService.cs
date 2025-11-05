// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：ILoginService.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：登录服务接口
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
