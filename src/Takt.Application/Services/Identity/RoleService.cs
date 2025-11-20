// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Identity
// 文件名称：RoleService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：角色服务实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Takt.Application.Dtos.Identity;
using Takt.Common.Helpers;
using Takt.Common.Results;
using Takt.Domain.Entities.Identity;
using Takt.Domain.Repositories;
using Mapster;
using SqlSugar;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 角色服务实现
/// 实现角色相关的业务逻辑
/// </summary>
public class RoleService : IRoleService
{
    private readonly IBaseRepository<Role> _roleRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public RoleService(IBaseRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 查询角色列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、角色名称、角色编码等筛选条件</param>
    /// <returns>包含分页角色列表的结果对象，成功时返回角色列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在角色名称、角色编码、描述中搜索）
    /// 支持按角色名称、角色编码、创建时间排序
    /// </remarks>
    public async Task<Result<PagedResult<RoleDto>>> GetListAsync(RoleQueryDto query)
    {
        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<Role, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                // 根据排序字段名构建排序表达式
                switch (query.OrderBy.ToLower())
                {
                    case "rolename":
                        orderByExpression = r => r.RoleName;
                        break;
                    case "rolecode":
                        orderByExpression = r => r.RoleCode;
                        break;
                    case "createdtime":
                        orderByExpression = r => r.CreatedTime;
                        break;
                    default:
                        orderByExpression = r => r.CreatedTime;
                        break;
                }
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _roleRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var roleDtos = result.Items.Adapt<List<RoleDto>>();

            var pagedResult = new PagedResult<RoleDto>
            {
                Items = roleDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<RoleDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<RoleDto>>.Fail($"查询角色数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取角色信息
    /// </summary>
    /// <param name="id">角色ID，必须大于0</param>
    /// <returns>包含角色信息的结果对象，成功时返回角色DTO，失败时返回错误信息（如角色不存在）</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// </remarks>
    public async Task<Result<RoleDto>> GetByIdAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return Result<RoleDto>.Fail("角色不存在");

        var roleDto = role.Adapt<RoleDto>();
        return Result<RoleDto>.Ok(roleDto);
    }

    /// <summary>
    /// 创建新角色
    /// </summary>
    /// <param name="dto">创建角色数据传输对象，包含角色名称、角色编码、描述等角色信息</param>
    /// <returns>包含新角色ID的结果对象，成功时返回角色ID，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result<long>> CreateAsync(RoleCreateDto dto)
    {
        // 验证角色编码唯一性
        var existsByCode = await _roleRepository.GetFirstAsync(r => r.RoleCode == dto.RoleCode && r.IsDeleted == 0);
        if (existsByCode != null)
            return Result<long>.Fail($"角色编码 {dto.RoleCode} 已存在");

        // 验证角色名称唯一性
        var existsByName = await _roleRepository.GetFirstAsync(r => r.RoleName == dto.RoleName && r.IsDeleted == 0);
        if (existsByName != null)
            return Result<long>.Fail($"角色名称 {dto.RoleName} 已存在");

        var role = dto.Adapt<Role>();
        
        var result = await _roleRepository.CreateAsync(role);
        if (result > 0)
            return Result<long>.Ok(role.Id);
        
        return Result<long>.Fail("创建角色失败");
    }

    /// <summary>
    /// 更新角色信息
    /// </summary>
    /// <param name="dto">更新角色数据传输对象，必须包含角色ID和要更新的字段信息</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如角色不存在、超级角色不允许更新）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、变更内容、操作时间、请求参数、执行耗时等信息
    /// 注意：超级角色（角色编码为 "super"）不允许更新
    /// </remarks>
    public async Task<Result> UpdateAsync(RoleUpdateDto dto)
    {
        var role = await _roleRepository.GetByIdAsync(dto.Id);
        if (role == null)
            return Result.Fail("角色不存在");

        // 检查是否为超级角色，超级角色不允许更新
        if (role.RoleCode == "super")
            return Result.Fail("超级角色不允许更新");

        // 验证角色编码唯一性（如果角色编码有变化）
        if (role.RoleCode != dto.RoleCode)
        {
            var existsByCode = await _roleRepository.GetFirstAsync(r => r.RoleCode == dto.RoleCode && r.Id != dto.Id && r.IsDeleted == 0);
            if (existsByCode != null)
                return Result.Fail($"角色编码 {dto.RoleCode} 已被其他角色使用");
        }

        // 验证角色名称唯一性（如果角色名称有变化）
        if (role.RoleName != dto.RoleName)
        {
            var existsByName = await _roleRepository.GetFirstAsync(r => r.RoleName == dto.RoleName && r.Id != dto.Id && r.IsDeleted == 0);
            if (existsByName != null)
                return Result.Fail($"角色名称 {dto.RoleName} 已被其他角色使用");
        }

        dto.Adapt(role);
        
        var result = await _roleRepository.UpdateAsync(role);
        return result > 0 ? Result.Ok() : Result.Fail("更新角色失败");
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id">角色ID，必须大于0</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如角色不存在、超级角色不允许删除）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 注意：超级角色（角色编码为 "super"）不允许删除
    /// </remarks>
    public async Task<Result> DeleteAsync(long id)
    {
        // 检查是否为超级角色，超级角色不允许删除
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return Result.Fail("角色不存在");
        
        if (role.RoleCode == "super")
            return Result.Fail("超级角色不允许删除");

        var result = await _roleRepository.DeleteAsync(id);
        return result > 0 ? Result.Ok() : Result.Fail("删除角色失败");
    }

    /// <summary>
    /// 修改角色状态（DTO方式）
    /// </summary>
    /// <param name="dto">角色状态数据传输对象，包含角色ID和状态值</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result> StatusAsync(RoleStatusDto dto)
    {
        var result = await _roleRepository.StatusAsync(dto.Id, (int)dto.Status);
        return result > 0 ? Result.Ok("修改状态成功") : Result.Fail("修改状态失败");
    }

    /// <summary>
    /// 导出角色到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的角色</param>
    /// <param name="sheetName">工作表名称，可选，默认为 "Roles"</param>
    /// <param name="fileName">文件名，可选，默认为 "角色导出_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容的结果对象，成功时返回文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于导出，不会记录操作日志
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(RoleQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var condition = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<Role>().And(r => r.IsDeleted == 0).ToExpression();
            var roles = await _roleRepository.AsQueryable()
                .Where(condition)
                .OrderBy(r => r.CreatedTime)
                .ToListAsync();
            
            var roleDtos = roles.Adapt<List<RoleDto>>();
            sheetName ??= "Roles";
            fileName ??= $"角色导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(roleDtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {roleDtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出角色 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选，默认为 "Roles"</param>
    /// <param name="fileName">文件名，可选，默认为 "角色导入模板_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容的结果对象，成功时返回文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于导出模板，不会记录操作日志
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "Roles";
        fileName ??= $"角色导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<RoleDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入角色
    /// </summary>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        try
        {
            sheetName ??= "Roles";
            var roleDtos = ExcelHelper.ImportFromExcel<RoleDto>(fileStream, sheetName);
            if (roleDtos == null || !roleDtos.Any()) return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0, fail = 0;
            foreach (var dto in roleDtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.RoleCode)) { fail++; continue; }
                    var existing = await _roleRepository.GetFirstAsync(r => r.RoleCode == dto.RoleCode && r.IsDeleted == 0);
                    if (existing == null)
                    {
                        // 验证角色名称唯一性
                        if (!string.IsNullOrWhiteSpace(dto.RoleName))
                        {
                            var existsByName = await _roleRepository.GetFirstAsync(r => r.RoleName == dto.RoleName && r.IsDeleted == 0);
                            if (existsByName != null) { fail++; continue; }
                        }

                        var createDto = dto.Adapt<RoleCreateDto>();
                        var entity = createDto.Adapt<Role>();
                        await _roleRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        // 验证角色名称唯一性（如果角色名称有变化）
                        if (existing.RoleName != dto.RoleName && !string.IsNullOrWhiteSpace(dto.RoleName))
                        {
                            var existsByName = await _roleRepository.GetFirstAsync(r => r.RoleName == dto.RoleName && r.Id != existing.Id && r.IsDeleted == 0);
                            if (existsByName != null) { fail++; continue; }
                        }

                        existing.RoleName = dto.RoleName;
                        existing.RoleCode = dto.RoleCode;
                        existing.Remarks = dto.Remarks;
                        existing.RoleStatus = dto.RoleStatus;
                        await _roleRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }
            return Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
        }
        catch (Exception ex)
        {
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<Role, bool>> QueryExpression(RoleQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Role>()
            .And(r => r.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), r => r.RoleName.Contains(query.Keywords!) || 
                                                              r.RoleCode.Contains(query.Keywords!) ||
                                                              (r.Description != null && r.Description.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.RoleName), r => r.RoleName.Contains(query.RoleName!))
            .AndIF(!string.IsNullOrEmpty(query.RoleCode), r => r.RoleCode.Contains(query.RoleCode!))
            .AndIF(!string.IsNullOrEmpty(query.Description), r => r.Description != null && r.Description.Contains(query.Description!))
            .AndIF(query.DataScope.HasValue, r => r.DataScope == query.DataScope!.Value)
            .AndIF(query.RoleStatus.HasValue, r => r.RoleStatus == query.RoleStatus!.Value)
            .ToExpression();
    }
}
