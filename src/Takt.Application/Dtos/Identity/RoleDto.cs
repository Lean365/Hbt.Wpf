// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Dtos.Identity
// 文件名称：RoleDto.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：角色数据传输对象
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================
using Takt.Common.Results;
using Takt.Common.Enums;

namespace Takt.Application.Dtos.Identity;

/// <summary>
/// 角色数据传输对象
/// 用于传输角色信息
/// </summary>
public class RoleDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public RoleDto()
    {
        RoleName = string.Empty;
        RoleCode = string.Empty;
        Description = string.Empty;
        Remarks = string.Empty;
        CreatedBy = string.Empty;
        UpdatedBy = string.Empty;
        DeletedBy = string.Empty;
        CreatedTime = DateTime.Now;
        UpdatedTime = DateTime.Now;
        DeletedTime = DateTime.Now;
    }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string Description { get; set; }
    
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
    /// 是否删除
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
/// 角色查询数据传输对象
/// 用于查询角色信息
/// </summary>
public class RoleQueryDto : PagedQuery
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public RoleQueryDto()
    {
        Keywords = string.Empty;
        RoleName = string.Empty;
        RoleCode = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// 搜索关键词（支持在角色名称、角色编码、描述中搜索）
    /// </summary>
    public string Keywords { get; set; }
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string Description { get; set; }
    
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
    /// 构造函数：初始化默认值
    /// </summary>
    public RoleCreateDto()
    {
        RoleName = string.Empty;
        RoleCode = string.Empty;
        Description = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string Description { get; set; }
    
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
    public string Remarks { get; set; }
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

/// <summary>
/// 角色导出数据传输对象
/// </summary>
public class RoleExportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public RoleExportDto()
    {
        RoleName = string.Empty;
        RoleCode = string.Empty;
        Description = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string Description { get; set; }
    
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
    public string Remarks { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 角色导入模板数据传输对象
/// </summary>
public class RoleTemplateDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public RoleTemplateDto()
    {
        RoleName = string.Empty;
        RoleCode = string.Empty;
        Description = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 数据范围（1=全部数据, 2=本部门及以下, 3=本部门, 4=仅本人, 5=自定义）
    /// </summary>
    public int DataScope { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 角色状态（0=启用，1=禁用）
    /// </summary>
    public int RoleStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}

/// <summary>
/// 角色导入数据传输对象
/// </summary>
public class RoleImportDto
{
    /// <summary>
    /// 构造函数：初始化默认值
    /// </summary>
    public RoleImportDto()
    {
        RoleName = string.Empty;
        RoleCode = string.Empty;
        Description = string.Empty;
        Remarks = string.Empty;
    }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string RoleCode { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 数据范围（1=全部数据, 2=本部门及以下, 3=本部门, 4=仅本人, 5=自定义）
    /// </summary>
    public int DataScope { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 角色状态（0=启用，1=禁用）
    /// </summary>
    public int RoleStatus { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
}