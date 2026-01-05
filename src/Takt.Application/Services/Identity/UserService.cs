// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Application.Services.Identity
// 文件名称：UserService.cs
// 创建时间：2025-10-17
// 创建人：Takt365(Cursor AI)
// 功能描述：用户服务实现
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Mapster;
using Newtonsoft.Json;
using SqlSugar;
using System.Linq.Expressions;
using Takt.Application.Dtos.Identity;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Common.Security;
using Takt.Domain.Entities.Identity;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Identity;

/// <summary>
/// 用户服务实现
/// 实现用户相关的业务逻辑
/// </summary>
public class UserService : IUserService
{
    private readonly IBaseRepository<User> _userRepository;
    private readonly IBaseRepository<UserRole> _userRoleRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="userRoleRepository">用户角色仓储</param>
    /// <param name="appLog">应用程序日志管理器</param>
    /// <param name="operLog">操作日志管理器（可选）</param>
    public UserService(IBaseRepository<User> userRepository, IBaseRepository<UserRole> userRoleRepository, AppLogManager appLog, OperLogManager? operLog = null)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询用户列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、用户名、邮箱、电话、真实姓名、用户类型、用户性别、用户状态等筛选条件</param>
    /// <returns>包含分页用户列表的结果对象，成功时返回用户列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在用户名、真实姓名、邮箱、电话中搜索）
    /// 支持按用户名、真实姓名、邮箱、创建时间排序
    /// </remarks>
    public async Task<Result<PagedResult<UserDto>>> GetListAsync(UserQueryDto query)
    {
        _appLog.Information("开始查询用户列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);

            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<User, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                // 根据排序字段名构建排序表达式
                switch (query.OrderBy.ToLower())
                {
                    case "username":
                        orderByExpression = u => u.Username;
                        break;
                    case "realname":
                        orderByExpression = u => u.RealName ?? string.Empty;
                        break;
                    case "email":
                        orderByExpression = u => u.Email ?? string.Empty;
                        break;
                    case "createdtime":
                        orderByExpression = u => u.CreatedTime;
                        break;
                    default:
                        orderByExpression = u => u.CreatedTime;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }

            // 使用真实的数据库查询
            var result = await _userRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var userDtos = result.Items.Adapt<List<UserDto>>();

            _appLog.Information("查询完成，返回 {Count} 条用户记录，总数: {TotalNum}",
                userDtos.Count, result.TotalNum);

            var pagedResult = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<UserDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "查询用户数据失败");
            return Result<PagedResult<UserDto>>.Fail($"查询用户数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取用户信息
    /// </summary>
    /// <param name="id">用户ID，必须大于0</param>
    /// <returns>包含用户信息的结果对象，成功时返回用户DTO，失败时返回错误信息（如用户不存在）</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// </remarks>
    public async Task<Result<UserDto>> GetByIdAsync(long id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return Result<UserDto>.Fail("用户不存在");

        var userDto = user.Adapt<UserDto>();
        return Result<UserDto>.Ok(userDto);
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    /// <param name="dto">创建用户数据传输对象，包含用户名、密码、真实姓名、邮箱、电话等用户信息</param>
    /// <returns>包含新用户ID的结果对象，成功时返回用户ID，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// </remarks>
    public async Task<Result<long>> CreateAsync(UserCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 验证用户名唯一性
            var existsByUsername = await _userRepository.GetFirstAsync(u => u.Username == dto.Username && u.IsDeleted == 0);
            if (existsByUsername != null)
                return Result<long>.Fail($"用户名 {dto.Username} 已存在");

            // 验证邮箱唯一性（如果提供了邮箱）
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existsByEmail = await _userRepository.GetFirstAsync(u => u.Email == dto.Email && u.IsDeleted == 0);
                if (existsByEmail != null)
                    return Result<long>.Fail($"邮箱 {dto.Email} 已被其他用户使用");
            }

            // 验证手机号唯一性（如果提供了手机号）
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existsByPhone = await _userRepository.GetFirstAsync(u => u.Phone == dto.Phone && u.IsDeleted == 0);
                if (existsByPhone != null)
                    return Result<long>.Fail($"手机号 {dto.Phone} 已被其他用户使用");
            }

            var user = dto.Adapt<User>();

            var result = await _userRepository.CreateAsync(user);
            Result<long> response = result > 0 
                ? Result<long>.Ok(user.Id) 
                : Result<long>.Fail("创建用户失败");
            
            // 记录完整的创建信息（包含所有字段）
            _operLog?.LogCreate("User", user.Id.ToString(), "Identity.UserView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建用户失败");
            return Result<long>.Fail($"创建用户失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="dto">更新用户数据传输对象，必须包含用户ID和要更新的字段信息</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如用户不存在、超级用户不允许更新）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、变更内容、操作时间、请求参数、执行耗时等信息
    /// 注意：更新操作不会修改密码，如需修改密码请使用 ChangePasswordAsync 或 ResetPasswordAsync
    /// 注意：超级用户（用户名为 "admin"）不允许更新
    /// </remarks>
    public async Task<Result> UpdateAsync(UserUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null)
                return Result.Fail("用户不存在");

            // 检查是否为超级用户，超级用户不允许更新
            if (user.Username == "admin")
                return Result.Fail("超级用户不允许更新");

            var oldUser = user.Adapt<UserUpdateDto>(); // 保存旧值用于记录变更
            var originalPassword = user.Password; // 保存原始密码，防止被覆盖

            dto.Adapt(user);

            // 更新操作不修改密码和用户名，恢复原始值
            user.Password = originalPassword; // 恢复原始密码
            user.Username = oldUser.Username; // 恢复原始用户名（虽然 UI 已经是只读，但这里双重保险）

            // 验证邮箱唯一性（如果邮箱有变化）
            if (user.Email != dto.Email && !string.IsNullOrWhiteSpace(dto.Email))
            {
                var existsByEmail = await _userRepository.GetFirstAsync(u => u.Email == dto.Email && u.Id != dto.Id && u.IsDeleted == 0);
                if (existsByEmail != null)
                    return Result.Fail($"邮箱 {dto.Email} 已被其他用户使用");
            }

            // 验证手机号唯一性（如果手机号有变化）
            if (user.Phone != dto.Phone && !string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existsByPhone = await _userRepository.GetFirstAsync(u => u.Phone == dto.Phone && u.Id != dto.Id && u.IsDeleted == 0);
                if (existsByPhone != null)
                    return Result.Fail($"手机号 {dto.Phone} 已被其他用户使用");
            }

            var result = await _userRepository.UpdateAsync(user);

            // 构建完整的变更信息，包含所有变更的字段
            var changeList = new List<string>();
            if (oldUser.Username != dto.Username) changeList.Add($"Username: {oldUser.Username} -> {dto.Username}");
            if (oldUser.RealName != dto.RealName) changeList.Add($"RealName: {oldUser.RealName ?? "null"} -> {dto.RealName ?? "null"}");
            if (oldUser.Email != dto.Email) changeList.Add($"Email: {oldUser.Email ?? "null"} -> {dto.Email ?? "null"}");
            if (oldUser.Phone != dto.Phone) changeList.Add($"Phone: {oldUser.Phone ?? "null"} -> {dto.Phone ?? "null"}");
            if (oldUser.Nickname != dto.Nickname) changeList.Add($"Nickname: {oldUser.Nickname} -> {dto.Nickname}");
            if (oldUser.Avatar != dto.Avatar) changeList.Add($"Avatar: {oldUser.Avatar ?? "null"} -> {dto.Avatar ?? "null"}");
            if (oldUser.UserType != dto.UserType) changeList.Add($"UserType: {oldUser.UserType} -> {dto.UserType}");
            if (oldUser.UserGender != dto.UserGender) changeList.Add($"UserGender: {oldUser.UserGender} -> {dto.UserGender}");
            if (oldUser.UserStatus != dto.UserStatus) changeList.Add($"UserStatus: {oldUser.UserStatus} -> {dto.UserStatus}");
            if (oldUser.Remarks != dto.Remarks) changeList.Add($"Remarks: {oldUser.Remarks ?? "null"} -> {dto.Remarks ?? "null"}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新用户失败");
            
            // 记录完整的修改前后信息（旧值和新值）
            _operLog?.LogUpdate("User", dto.Id.ToString(), "Identity.UserView", changes, dto, oldUser, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新用户失败");
            return Result.Fail($"更新用户失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    /// <param name="id">要删除的用户ID，必须大于0</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如用户不存在、超级用户不允许删除）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 删除操作采用软删除方式，不会真正从数据库中移除数据，只是标记为已删除状态
    /// 注意：超级用户（用户名为 "admin"）不允许删除
    /// </remarks>
    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查是否为超级用户，超级用户不允许删除
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return Result.Fail("用户不存在");

            if (user.Username == "admin")
                return Result.Fail("超级用户不允许删除");

            var result = await _userRepository.DeleteAsync(id);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除用户失败");
            
            _operLog?.LogDelete("User", id.ToString(), "Identity.UserView", 
                new { UserId = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除用户失败");
            return Result.Fail($"删除用户失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除用户
    /// </summary>
    /// <param name="ids">用户ID列表</param>
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
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.Username == "admin")
                {
                    failCount++;
                    continue;
                }

                var result = await _userRepository.DeleteAsync(id);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("User", id.ToString(), "Identity.UserView", 
                        new { UserId = id }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除用户完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除用户失败");
            return Result.Fail($"批量删除用户失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 修改密码（用户自主修改）
    /// </summary>
    /// <param name="dto">修改密码数据传输对象，包含用户ID、旧密码和新密码</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如用户不存在、旧密码不正确）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 用户必须提供正确的旧密码才能修改密码
    /// </remarks>
    public async Task<Result> ChangePasswordAsync(UserChangePasswordDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null) return Result.Fail("用户不存在");

            if (!SecurityHelper.VerifyPassword(dto.OldPassword, user.Password))
            {
                return Result.Fail("旧密码不正确");
            }

            user.Password = SecurityHelper.HashPassword(dto.NewPassword);
            var rows = await _userRepository.UpdateAsync(user);
            Result response = rows > 0 ? Result.Ok("修改密码成功") : Result.Fail("修改密码失败");
            
            _operLog?.LogUpdate("User", dto.UserId.ToString(), "Identity.UserView", "修改密码",
                new { UserId = dto.UserId }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "修改密码失败");
            return Result.Fail($"修改密码失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 重置密码（管理员操作）
    /// </summary>
    /// <param name="dto">重置密码数据传输对象，包含用户ID和新密码</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如用户不存在）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 管理员重置密码时不需要提供旧密码，直接设置新密码
    /// </remarks>
    public async Task<Result> ResetPasswordAsync(UserResetPasswordDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null) return Result.Fail("用户不存在");

            user.Password = SecurityHelper.HashPassword(dto.NewPassword);
            var rows = await _userRepository.UpdateAsync(user);
            Result response = rows > 0 ? Result.Ok("重置密码成功") : Result.Fail("重置密码失败");
            
            _operLog?.LogUpdate("User", dto.UserId.ToString(), "Identity.UserView", "重置密码",
                new { UserId = dto.UserId }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "重置密码失败");
            return Result.Fail($"重置密码失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 修改用户状态
    /// </summary>
    /// <param name="dto">用户状态数据传输对象，包含用户ID和目标状态（启用/禁用）</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息（如用户不存在）</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 状态修改会影响用户的登录权限
    /// </remarks>
    public async Task<Result> StatusAsync(UserStatusDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查是否为超级用户，超级用户不允许修改状态
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null)
                return Result.Fail("用户不存在");

            if (user.Username == "admin")
                return Result.Fail("超级用户不允许修改状态");

            var result = await _userRepository.StatusAsync(dto.Id, (int)dto.Status);
            Result response = result > 0 ? Result.Ok("修改状态成功") : Result.Fail("修改状态失败");
            
            _operLog?.LogUpdate("User", dto.Id.ToString(), "Identity.UserView", $"修改状态为 {dto.Status}",
                new { UserId = dto.Id, Status = dto.Status }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "修改用户状态失败");
            return Result.Fail($"修改状态失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出用户数据到 Excel 文件
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的用户数据</param>
    /// <param name="sheetName">Excel 工作表名称，默认为 "Users"</param>
    /// <param name="fileName">导出文件名，默认为 "用户导出_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容字节数组的结果对象，成功时返回文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作不会记录操作日志
    /// 导出的 Excel 文件包含所有用户字段信息
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(UserQueryDto? query = null!, string? sheetName = null!, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<User>().And(x => x.IsDeleted == 0).ToExpression();
            var users = await _userRepository.AsQueryable().Where(where).OrderBy(u => u.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = users.Adapt<List<UserDto>>();
            sheetName ??= "Users";
            fileName ??= $"用户导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出用户Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户导入模板 Excel 文件
    /// </summary>
    /// <param name="sheetName">Excel 工作表名称，默认为 "Users"</param>
    /// <param name="fileName">模板文件名，默认为 "用户导入模板_yyyyMMddHHmmss.xlsx"</param>
    /// <returns>包含文件名和文件内容字节数组的结果对象，成功时返回模板文件信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作不会记录操作日志
    /// 模板文件包含用户字段的列头，用于指导用户填写导入数据
    /// </remarks>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null!, string? fileName = null)
    {
        sheetName ??= "Users";
        fileName ??= $"用户导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<UserDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 文件导入用户数据
    /// </summary>
    /// <param name="fileStream">Excel 文件流，必须包含有效的用户数据</param>
    /// <param name="sheetName">Excel 工作表名称，默认为 "Users"</param>
    /// <returns>包含成功和失败数量的结果对象，成功时返回导入统计信息（成功数量、失败数量），失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 导入逻辑：如果用户名已存在则更新用户信息，否则创建新用户
    /// 用户名不能为空，空用户名的记录会被跳过并计入失败数量
    /// </remarks>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "Users";
            var userDtos = ExcelHelper.ImportFromExcel<UserDto>(fileStream, sheetName);
            if (userDtos == null || !userDtos.Any()) return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0, fail = 0;
            foreach (var dto in userDtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.Username)) { fail++; continue; }

                    // 验证用户名唯一性
                    var existing = await _userRepository.GetFirstAsync(u => u.Username == dto.Username && u.IsDeleted == 0);
                    if (existing == null)
                    {
                        // 验证邮箱唯一性（如果提供了邮箱）
                        if (!string.IsNullOrWhiteSpace(dto.Email))
                        {
                            var existsByEmail = await _userRepository.GetFirstAsync(u => u.Email == dto.Email && u.IsDeleted == 0);
                            if (existsByEmail != null) { fail++; continue; }
                        }

                        // 验证手机号唯一性（如果提供了手机号）
                        if (!string.IsNullOrWhiteSpace(dto.Phone))
                        {
                            var existsByPhone = await _userRepository.GetFirstAsync(u => u.Phone == dto.Phone && u.IsDeleted == 0);
                            if (existsByPhone != null) { fail++; continue; }
                        }

                        var entity = new User
                        {
                            Username = dto.Username,
                            Password = string.Empty,
                            Email = dto.Email,
                            Phone = dto.Phone,
                            RealName = dto.RealName,
                            UserType = dto.UserType,
                            UserGender = dto.UserGender,
                            UserStatus = dto.UserStatus,
                            Remarks = dto.Remarks,
                            Avatar = dto.Avatar
                        };
                        await _userRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        // 验证邮箱唯一性（如果邮箱有变化）
                        if (existing.Email != dto.Email && !string.IsNullOrWhiteSpace(dto.Email))
                        {
                            var existsByEmail = await _userRepository.GetFirstAsync(u => u.Email == dto.Email && u.Id != existing.Id && u.IsDeleted == 0);
                            if (existsByEmail != null) { fail++; continue; }
                        }

                        // 验证手机号唯一性（如果手机号有变化）
                        if (existing.Phone != dto.Phone && !string.IsNullOrWhiteSpace(dto.Phone))
                        {
                            var existsByPhone = await _userRepository.GetFirstAsync(u => u.Phone == dto.Phone && u.Id != existing.Id && u.IsDeleted == 0);
                            if (existsByPhone != null) { fail++; continue; }
                        }

                        existing.Email = dto.Email;
                        existing.Phone = dto.Phone;
                        existing.RealName = dto.RealName;
                        existing.UserType = dto.UserType;
                        existing.UserGender = dto.UserGender;
                        existing.UserStatus = dto.UserStatus;
                        existing.Remarks = dto.Remarks;
                        existing.Avatar = dto.Avatar;
                        await _userRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
            
            _operLog?.LogImport("User", success, "Identity.UserView", 
                new { Total = userDtos.Count, Success = success, Fail = fail, Items = userDtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "从Excel导入用户失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户已分配的角色ID列表
    /// </summary>
    /// <param name="userId">用户ID，必须大于0</param>
    /// <returns>包含角色ID列表的结果对象，成功时返回角色ID列表，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 返回的角色ID列表不包含已删除的角色关联
    /// </remarks>
    public async Task<Result<List<long>>> GetUserRolesAsync(long userId)
    {
        _appLog.Information("开始获取用户角色列表，用户ID: {UserId}", userId);

        try
        {
            var userRolesResult = await _userRoleRepository.GetListAsync(ur => ur.UserId == userId && ur.IsDeleted == 0);
            var roleIds = userRolesResult.Items.Select(ur => ur.RoleId).ToList();

            _appLog.Information("获取用户角色列表完成，用户ID: {UserId}, 角色数量: {Count}", userId, roleIds.Count);

            return Result<List<long>>.Ok(roleIds);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取用户角色列表失败，用户ID: {UserId}", userId);
            return Result<List<long>>.Fail($"获取用户角色列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 分配角色给用户
    /// </summary>
    /// <param name="userId">用户ID，必须大于0</param>
    /// <param name="roleIds">要分配的角色ID列表，可以为空列表（表示移除所有角色）</param>
    /// <returns>操作结果对象，成功时返回成功信息，失败时返回错误信息</returns>
    /// <remarks>
    /// 此操作会记录操作日志到数据库，包含操作人、操作时间、请求参数、执行耗时等信息
    /// 分配逻辑：先删除不在新列表中的角色关联，再添加新列表中的角色关联
    /// 如果角色ID列表为空，将移除用户的所有角色
    /// </remarks>
    public async Task<Result> AssignRolesAsync(long userId, List<long> roleIds)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _appLog.Information("开始分配角色给用户，用户ID: {UserId}, 角色数量: {Count}", userId, roleIds.Count);

        try
        {
            // 1. 获取用户当前的角色列表
            var existingUserRolesResult = await _userRoleRepository.GetListAsync(ur => ur.UserId == userId && ur.IsDeleted == 0);
            var existingUserRoles = existingUserRolesResult.Items;
            var existingRoleIds = existingUserRoles.Select(ur => ur.RoleId).ToList();

            // 2. 计算需要删除的角色（存在于数据库但不在新列表中）
            var rolesToDelete = existingUserRoles.Where(ur => !roleIds.Contains(ur.RoleId)).ToList();

            // 3. 计算需要添加的角色（在新列表中但不在数据库）
            var rolesToAdd = roleIds.Where(roleId => !existingRoleIds.Contains(roleId)).ToList();

            // 4. 删除不需要的角色（软删除）
            foreach (var userRole in rolesToDelete)
            {
                await _userRoleRepository.DeleteAsync(userRole.Id);
            }

            // 5. 添加新角色
            foreach (var roleId in rolesToAdd)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                };
                await _userRoleRepository.CreateAsync(userRole);
            }

            stopwatch.Stop();
            _appLog.Information("分配角色完成，用户ID: {UserId}, 删除: {DeleteCount}, 添加: {AddCount}",
                userId, rolesToDelete.Count, rolesToAdd.Count);

            var changes = $"删除 {rolesToDelete.Count} 个角色，添加 {rolesToAdd.Count} 个角色";
            var response = Result.Ok("分配角色成功");
            
            _operLog?.LogUpdate("User", userId.ToString(), "Identity.UserView", $"分配角色：{changes}",
                new { UserId = userId, RoleIds = roleIds, DeletedCount = rolesToDelete.Count, AddedCount = rolesToAdd.Count }, response, stopwatch);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "分配角色失败，用户ID: {UserId}", userId);
            return Result.Fail($"分配角色失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建用户查询表达式
    /// </summary>
    /// <param name="query">用户查询数据传输对象，包含各种筛选条件</param>
    /// <returns>用于筛选用户的 LINQ 表达式，自动排除已删除的用户（IsDeleted == 0）</returns>
    /// <remarks>
    /// 此方法用于将查询DTO转换为数据库查询表达式
    /// 支持的关键字搜索字段：用户名、真实姓名、邮箱、电话
    /// 支持的条件筛选：用户名、邮箱、电话、真实姓名、用户类型、用户性别、用户状态
    /// </remarks>
    private Expression<Func<User, bool>> QueryExpression(UserQueryDto query)
    {
        return SqlSugar.Expressionable.Create<User>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）
            .AndIF(!string.IsNullOrEmpty(query.Keywords), x => x.Username.Contains(query.Keywords!) ||
                                                              (x.RealName != null && x.RealName.Contains(query.Keywords!)) ||
                                                              (x.Email != null && x.Email.Contains(query.Keywords!)) ||
                                                              (x.Phone != null && x.Phone.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.Username), x => x.Username.Contains(query.Username!))
            .AndIF(!string.IsNullOrEmpty(query.Email), x => x.Email != null && x.Email.Contains(query.Email!))
            .AndIF(!string.IsNullOrEmpty(query.Phone), x => x.Phone != null && x.Phone.Contains(query.Phone!))
            .AndIF(!string.IsNullOrEmpty(query.RealName), x => x.RealName != null && x.RealName.Contains(query.RealName!))
            .AndIF(query.UserType.HasValue, x => x.UserType == query.UserType!.Value)
            .AndIF(query.UserGender.HasValue, x => x.UserGender == query.UserGender!.Value)
            .AndIF(query.UserStatus.HasValue, x => x.UserStatus == query.UserStatus!.Value)
            .ToExpression();
    }
}
