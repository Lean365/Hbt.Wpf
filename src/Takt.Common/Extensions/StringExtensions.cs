// ========================================
// 项目名称：Takt.Wpf
// 文件名称：StringExtensions.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：字符串扩展方法
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

namespace Takt.Common.Extensions;

/// <summary>
/// 字符串扩展方法
/// 提供字符串处理的便捷扩展方法
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 判断字符串是否为空
    /// </summary>
    /// <param name="str">要检查的字符串</param>
    /// <returns>如果字符串为null或空，返回true</returns>
    public static bool IsNullOrEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// 判断字符串是否为空或仅包含空白字符
    /// </summary>
    /// <param name="str">要检查的字符串</param>
    /// <returns>如果字符串为null、空或仅包含空白字符，返回true</returns>
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    /// 截取字符串
    /// </summary>
    /// <param name="str">要截取的字符串</param>
    /// <param name="maxLength">最大长度</param>
    /// <returns>截取后的字符串</returns>
    public static string Truncate(this string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;
        
        return str.Substring(0, maxLength);
    }
}
