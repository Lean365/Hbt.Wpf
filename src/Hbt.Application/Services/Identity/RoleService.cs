// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：RoleService.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：角色服务实现
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
using Hbt.Common.Helpers;
using Hbt.Common.Results;
using Hbt.Domain.Entities.Identity;
using Hbt.Domain.Repositories;
using Mapster;

namespace Hbt.Application.Services.Identity;

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
    /// 分页查询角色列表
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键字</param>
    /// <returns>分页角色列表</returns>
    public async Task<Result<PagedResult<RoleDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null)
    {
        var result = await _roleRepository.GetListAsync(null, pageIndex, pageSize);
        var roleDtos = result.Items.Adapt<List<RoleDto>>();
        
        return Result<PagedResult<RoleDto>>.Ok(result.Adapt<PagedResult<RoleDto>>());
    }

    /// <summary>
    /// 根据ID获取角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>角色信息</returns>
    public async Task<Result<RoleDto>> GetByIdAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return Result<RoleDto>.Fail("角色不存在");

        var roleDto = role.Adapt<RoleDto>();
        return Result<RoleDto>.Ok(roleDto);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="dto">创建角色DTO</param>
    /// <returns>新角色ID</returns>
    public async Task<Result<long>> CreateAsync(RoleCreateDto dto)
    {
        var role = dto.Adapt<Role>();
        
        var result = await _roleRepository.CreateAsync(role);
        if (result > 0)
            return Result<long>.Ok(role.Id);
        
        return Result<long>.Fail("创建角色失败");
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="dto">更新角色DTO</param>
    /// <returns>操作结果</returns>
    public async Task<Result> UpdateAsync(RoleUpdateDto dto)
    {
        var role = await _roleRepository.GetByIdAsync(dto.Id);
        if (role == null)
            return Result.Fail("角色不存在");

        dto.Adapt(role);
        
        var result = await _roleRepository.UpdateAsync(role);
        return result > 0 ? Result.Ok() : Result.Fail("更新角色失败");
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteAsync(long id)
    {
        var result = await _roleRepository.DeleteAsync(id);
        return result > 0 ? Result.Ok() : Result.Fail("删除角色失败");
    }

    /// <summary>
    /// 修改角色状态（DTO）
    /// </summary>
    public async Task<Result> StatusAsync(RoleStatusDto dto)
    {
        var result = await _roleRepository.StatusAsync(dto.Id, (int)dto.Status);
        return result > 0 ? Result.Ok("修改状态成功") : Result.Fail("修改状态失败");
    }

    /// <summary>
    /// 导出角色到Excel（支持条件查询导出）
    /// </summary>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(RoleQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            // TODO: 实现查询条件构建
            var roles = await _roleRepository.AsQueryable()
                .Where(r => r.IsDeleted == 0)
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
                        var createDto = dto.Adapt<RoleCreateDto>();
                        var entity = createDto.Adapt<Role>();
                        await _roleRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
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
}
