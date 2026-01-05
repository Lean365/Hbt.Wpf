// ========================================
// 项目名称：Takt.Wpf
// 文件名称：FileHelper.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：文件操作辅助类
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

namespace Takt.Common.Helpers;

/// <summary>
/// 文件操作辅助类
/// 提供常用的文件操作方法
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件存在返回true</returns>
    public static bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path">文件路径</param>
    public static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 读取文件所有文本
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件内容</returns>
    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// 写入文件所有文本
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="contents">要写入的内容</param>
    public static void WriteAllText(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    /// <summary>
    /// 创建目录（如果不存在）
    /// </summary>
    /// <param name="path">目录路径</param>
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="sourceFileName">源文件路径</param>
    /// <param name="destFileName">目标文件路径</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    public static void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
    {
        File.Copy(sourceFileName, destFileName, overwrite);
    }
}
