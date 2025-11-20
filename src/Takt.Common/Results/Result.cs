// ========================================
// 项目名称：Takt.Wpf
// 文件名称：Result.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：操作结果封装类
// 
// 版权信息：
// Copyright (c) 2025 Takt All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

namespace Takt.Common.Results;

/// <summary>
/// 操作结果封装类
/// 用于统一封装操作结果
/// </summary>
public class Result
{
    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 返回消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    /// <param name="message">成功消息</param>
    /// <returns>成功结果</returns>
    public static Result Ok(string message = "操作成功")
    {
        return new Result
        {
            Success = true,
            Message = message,
            Code = 200
        };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    /// <param name="message">失败消息</param>
    /// <param name="code">错误代码</param>
    /// <returns>失败结果</returns>
    public static Result Fail(string message = "操作失败", int code = 400)
    {
        return new Result
        {
            Success = false,
            Message = message,
            Code = code
        };
    }
}

/// <summary>
/// 带数据的操作结果封装类
/// 用于统一封装带返回数据的操作结果
/// </summary>
/// <typeparam name="T">返回数据类型</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// 返回数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    /// <param name="data">返回数据</param>
    /// <param name="message">成功消息</param>
    /// <returns>成功结果</returns>
    public static Result<T> Ok(T data, string message = "操作成功")
    {
        return new Result<T>
        {
            Success = true,
            Message = message,
            Code = 200,
            Data = data
        };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    /// <param name="message">失败消息</param>
    /// <param name="code">错误代码</param>
    /// <returns>失败结果</returns>
    public new static Result<T> Fail(string message = "操作失败", int code = 400)
    {
        return new Result<T>
        {
            Success = false,
            Message = message,
            Code = code
        };
    }
}
