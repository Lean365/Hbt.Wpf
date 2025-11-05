// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：RoleDto.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：角色数据传输对象
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
/// 角色数据传输对象
/// 用于传输角色信息
/// </summary>
public class RoleDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 数据范围（1=全部数据, 2=本部门及以下, 3=本部门, 4=仅本人, 5=自定义）
    /// </summary>
    public DataScopeEnum DataScope { get; set; }
    
    /// <summary>
    /// 角色用户数
    /// </summary>
    public int UserCount { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 角色状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum RoleStatus { get; set; }
    
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
/// 角色查询数据传输对象
/// 用于查询角色信息
/// </summary>
public class RoleQueryDto : PagedQuery
{
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string? RoleName { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string? RoleCode { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 数据范围（1=全部数据, 2=本部门及以下, 3=本部门, 4=仅本人, 5=自定义）
    /// </summary>
    public DataScopeEnum? DataScope { get; set; }
    
    /// <summary>
    /// 角色状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum? RoleStatus { get; set; }
}

/// <summary>
/// 创建角色数据传输对象
/// 用于创建新角色
/// </summary>
public class RoleCreateDto
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 数据范围（1=全部数据, 2=本部门及以下, 3=本部门, 4=仅本人, 5=自定义）
    /// </summary>
    public DataScopeEnum DataScope { get; set; } = DataScopeEnum.Self;
    
    /// <summary>
    /// 角色用户数
    /// </summary>
    public int UserCount { get; set; } = 0;
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; } = 0;
    
    /// <summary>
    /// 角色状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum RoleStatus { get; set; } = 0;
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// 更新角色数据传输对象
/// 用于更新角色信息
/// </summary>
public class RoleUpdateDto : RoleCreateDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 角色状态更新 DTO（启用/禁用）
/// </summary>
public class RoleStatusDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 新状态（0=启用，1=禁用）
    /// </summary>
    public StatusEnum Status { get; set; }
}