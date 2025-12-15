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
using Mapster;
using Newtonsoft.Json;
using SqlSugar;
using Takt.Application.Dtos.Identity;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Identity;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 角色服务实现
/// 实现角色相关的业务逻辑
/// </summary>
public class RoleService : IRoleService
{
    private readonly IBaseRepository<Role> _roleRepository;
    private readonly IBaseRepository<RoleMenu> _roleMenuRepository;
    private static readonly Serilog.ILogger _appLog = Serilog.Log.ForContext<RoleService>();
    private readonly Takt.Common.Logging.OperLogManager? _operLog;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    /// <param name="roleMenuRepository">角色菜单关联仓储</param>
    /// <param name="operLog">操作日志管理器</param>
    public RoleService(
        IBaseRepository<Role> roleRepository,
        IBaseRepository<RoleMenu> roleMenuRepository,
        Takt.Common.Logging.OperLogManager? operLog = null)
    {
        _roleRepository = roleRepository;
        _roleMenuRepository = roleMenuRepository;
        _operLog = operLog;
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
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
            Result<long> response = result > 0 
                ? Result<long>.Ok(role.Id) 
                : Result<long>.Fail("创建角色失败");
            
            _operLog?.LogCreate("Role", role.Id.ToString(), "Identity.RoleView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建角色失败");
            return Result<long>.Fail($"创建角色失败: {ex.Message}");
        }
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var role = await _roleRepository.GetByIdAsync(dto.Id);
            if (role == null)
                return Result.Fail("角色不存在");

            // 检查是否为超级角色，超级角色不允许更新
            if (role.RoleCode == "super")
                return Result.Fail("超级角色不允许更新");

            var oldRole = role.Adapt<RoleUpdateDto>(); // 保存旧值用于记录变更

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

            // 构建变更信息
            var changeList = new List<string>();
            if (oldRole.RoleName != dto.RoleName) changeList.Add($"RoleName: {oldRole.RoleName} -> {dto.RoleName}");
            if (oldRole.RoleCode != dto.RoleCode) changeList.Add($"RoleCode: {oldRole.RoleCode} -> {dto.RoleCode}");
            if (oldRole.Description != dto.Description) changeList.Add($"Description: {oldRole.Description ?? "null"} -> {dto.Description ?? "null"}");
            if (oldRole.RoleStatus != dto.RoleStatus) changeList.Add($"RoleStatus: {oldRole.RoleStatus} -> {dto.RoleStatus}");
            if (oldRole.Remarks != dto.Remarks) changeList.Add($"Remarks: {oldRole.Remarks ?? "null"} -> {dto.Remarks ?? "null"}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新角色失败");
            
            _operLog?.LogUpdate("Role", dto.Id.ToString(), "Identity.RoleView", changes, dto, oldRole, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新角色失败");
            return Result.Fail($"更新角色失败: {ex.Message}");
        }
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查是否为超级角色，超级角色不允许删除
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return Result.Fail("角色不存在");
            
            if (role.RoleCode == "super")
                return Result.Fail("超级角色不允许删除");

            var result = await _roleRepository.DeleteAsync(id);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除角色失败");
            
            _operLog?.LogDelete("Role", id.ToString(), "Identity.RoleView", 
                new { RoleId = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除角色失败");
            return Result.Fail($"删除角色失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除角色
    /// </summary>
    /// <param name="ids">角色ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            int successCount = 0;
            int failCount = 0;

            foreach (var id in ids)
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null || role.RoleCode == "super")
                {
                    failCount++;
                    continue;
                }

                var result = await _roleRepository.DeleteAsync(id);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("Role", id.ToString(), "Identity.RoleView", 
                        new { RoleId = id }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除角色完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除角色失败");
            return Result.Fail($"批量删除角色失败: {ex.Message}");
        }
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = await _roleRepository.StatusAsync(dto.Id, (int)dto.Status);
            var response = result > 0 ? Result.Ok("修改状态成功") : Result.Fail("修改状态失败");

            _operLog?.LogUpdate("Role", dto.Id.ToString(), "Identity.RoleView", $"修改状态为 {dto.Status}",
                new { RoleId = dto.Id, Status = dto.Status }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "修改角色状态失败，角色ID: {RoleId}", dto.Id);
            return Result.Fail($"修改状态失败: {ex.Message}");
        }
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
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(RoleQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
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
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
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

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
            
            _operLog?.LogImport("Role", success, "Identity.RoleView", 
                new { Total = roleDtos.Count, Success = success, Fail = fail, Items = roleDtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "从Excel导入角色失败");
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

    /// <summary>
    /// 获取角色已分配的菜单ID列表
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>菜单ID列表</returns>
    public async Task<Result<List<long>>> GetRoleMenusAsync(long roleId)
    {
        try
        {
            if (roleId <= 0)
                return Result<List<long>>.Fail("角色ID必须大于0");

            var roleMenusResult = await _roleMenuRepository.GetListAsync(rm => rm.RoleId == roleId && rm.IsDeleted == 0);
            var menuIds = roleMenusResult.Items.Select(rm => rm.MenuId).ToList();

            return Result<List<long>>.Ok(menuIds);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取角色菜单失败，角色ID: {RoleId}", roleId);
            return Result<List<long>>.Fail($"获取角色菜单失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 分配菜单给角色
    /// </summary>
    /// <param name="roleId">角色ID，必须大于0</param>
    /// <param name="menuIds">要分配的菜单ID列表，可以为空列表（表示移除所有菜单）</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 分配逻辑：先删除不在新列表中的菜单关联，再添加新列表中的菜单关联
    /// 如果菜单ID列表为空，将移除角色的所有菜单
    /// </remarks>
    public async Task<Result> AssignMenusAsync(long roleId, List<long> menuIds)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _appLog.Information("开始分配菜单给角色，角色ID: {RoleId}, 菜单数量: {Count}", roleId, menuIds.Count);

        try
        {
            if (roleId <= 0)
                return Result.Fail("角色ID必须大于0");

            // 1. 获取角色当前的菜单列表
            var existingRoleMenusResult = await _roleMenuRepository.GetListAsync(rm => rm.RoleId == roleId && rm.IsDeleted == 0);
            var existingRoleMenus = existingRoleMenusResult.Items;
            var existingMenuIds = existingRoleMenus.Select(rm => rm.MenuId).ToList();

            // 2. 计算需要删除的菜单（存在于数据库但不在新列表中）
            var menusToDelete = existingRoleMenus.Where(rm => !menuIds.Contains(rm.MenuId)).ToList();

            // 3. 计算需要添加的菜单（在新列表中但不在数据库）
            var menusToAdd = menuIds.Where(menuId => !existingMenuIds.Contains(menuId)).ToList();

            // 4. 删除不需要的菜单（软删除）
            foreach (var roleMenu in menusToDelete)
            {
                await _roleMenuRepository.DeleteAsync(roleMenu.Id);
            }

            // 5. 添加新菜单
            foreach (var menuId in menusToAdd)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = roleId,
                    MenuId = menuId
                };
                await _roleMenuRepository.CreateAsync(roleMenu);
            }

            _appLog.Information("分配菜单完成，角色ID: {RoleId}, 删除: {DeleteCount}, 添加: {AddCount}",
                roleId, menusToDelete.Count, menusToAdd.Count);

            var changes = $"删除 {menusToDelete.Count} 个菜单，添加 {menusToAdd.Count} 个菜单";
            var response = Result.Ok("分配菜单成功");
            
            _operLog?.LogUpdate("Role", roleId.ToString(), "Identity.RoleView", $"分配菜单：{changes}",
                new { RoleId = roleId, MenuIds = menuIds, DeletedCount = menusToDelete.Count, AddedCount = menusToAdd.Count }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "分配菜单失败，角色ID: {RoleId}", roleId);
            return Result.Fail($"分配菜单失败: {ex.Message}");
        }
    }
}
