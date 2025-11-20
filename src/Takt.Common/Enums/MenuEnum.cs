//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : MenuEnum.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 版本号 : 0.0.1
// 描述    : 菜单相关枚举集合
//===================================================================

namespace Takt.Common.Enums;

/// <summary>
/// 菜单类型枚举
/// </summary>
/// <remarks>
/// 0=目录, 1=菜单, 2=按钮, 3=API
/// </remarks>
public enum MenuTypeEnum
{
    /// <summary>
    /// 目录
    /// </summary>
    Directory = 0,

    /// <summary>
    /// 菜单
    /// </summary>
    Menu = 1,

    /// <summary>
    /// 按钮
    /// </summary>
    Button = 2,

    /// <summary>
    /// API
    /// </summary>
    Api = 3
}

/// <summary>
/// 可见性枚举
/// </summary>
/// <remarks>
/// 0=可见, 1=不可见
/// </remarks>
public enum VisibilityEnum
{
    /// <summary>
    /// 可见
    /// </summary>
    Visible = 0,

    /// <summary>
    /// 不可见
    /// </summary>
    Invisible = 1
}

/// <summary>
/// 外链枚举
/// </summary>
/// <remarks>
/// 0=外链, 1=不是外链
/// </remarks>
public enum ExternalEnum
{
    /// <summary>
    /// 外链
    /// </summary>
    External = 0,

    /// <summary>
    /// 不是外链
    /// </summary>
    NotExternal = 1
}

/// <summary>
/// 缓存枚举
/// </summary>
/// <remarks>
/// 0=缓存, 1=不缓存
/// </remarks>
public enum CacheEnum
{
    /// <summary>
    /// 缓存
    /// </summary>
    Cache = 0,

    /// <summary>
    /// 不缓存
    /// </summary>
    NoCache = 1
}

