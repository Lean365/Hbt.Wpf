//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : UserEnum.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 版本号 : 0.0.1
// 描述    : 用户相关枚举集合
//===================================================================

namespace Takt.Common.Enums;

/// <summary>
/// 用户类型枚举
/// </summary>
/// <remarks>
/// 0=系统用户, 1=普通用户
/// </remarks>
public enum UserTypeEnum
{
    /// <summary>
    /// 系统用户
    /// </summary>
    System = 0,

    /// <summary>
    /// 普通用户
    /// </summary>
    Normal = 1
}

/// <summary>
/// 用户性别枚举
/// </summary>
/// <remarks>
/// 0=未知, 1=男, 2=女
/// </remarks>
public enum UserGenderEnum
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 男
    /// </summary>
    Male = 1,

    /// <summary>
    /// 女
    /// </summary>
    Female = 2
}

/// <summary>
/// 数据权限范围枚举
/// </summary>
/// <remarks>
/// 1=全部数据, 2=自定义数据, 3=本部门数据, 4=本部门及以下数据, 5=仅本人数据
/// </remarks>
public enum DataScopeEnum
{
    /// <summary>
    /// 全部数据
    /// </summary>
    All = 1,

    /// <summary>
    /// 自定义数据
    /// </summary>
    Custom = 2,

    /// <summary>
    /// 本部门数据
    /// </summary>
    Department = 3,

    /// <summary>
    /// 本部门及以下数据
    /// </summary>
    DepartmentAndBelow = 4,

    /// <summary>
    /// 仅本人数据
    /// </summary>
    Self = 5
}
