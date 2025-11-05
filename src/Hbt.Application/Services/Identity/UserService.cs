// ========================================
// 项目名称：黑冰台中后台管理
// 文件名称：UserService.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：用户服务实现
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

using System.Linq.Expressions;
using Hbt.Application.Dtos.Identity;
using Hbt.Common.Logging;
using Hbt.Common.Results;
using Hbt.Common.Helpers;
using Hbt.Common.Security;
using Hbt.Domain.Entities.Identity;
using Hbt.Domain.Repositories;
using Mapster;
using SqlSugar;

namespace Hbt.Application.Services.Identity;

/// <summary>
/// 用户服务实现
/// 实现用户相关的业务逻辑
/// </summary>
public class UserService : IUserService
{
    private readonly IBaseRepository<User> _userRepository;
    private readonly AppLogManager _appLog;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="appLog">应用程序日志管理器</param>
    public UserService(IBaseRepository<User> userRepository, AppLogManager appLog)
    {
        _userRepository = userRepository;
        _appLog = appLog;
    }

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键字</param>
    /// <returns>分页用户列表</returns>
    public async Task<Result<PagedResult<UserDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null)
    {
        _appLog.Information("开始查询用户列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'", 
            pageIndex, pageSize, keyword ?? string.Empty);
        
        try
        {
            // 构建查询条件
            System.Linq.Expressions.Expression<Func<User, bool>>? condition = null;
            if (!string.IsNullOrEmpty(keyword))
            {
                condition = u => u.Username.Contains(keyword) || 
                                (u.RealName != null && u.RealName.Contains(keyword)) ||
                                (u.Email != null && u.Email.Contains(keyword));
            }
            
            // 使用真实的数据库查询
            var result = await _userRepository.GetListAsync(condition, pageIndex, pageSize);
            var userDtos = result.Items.Adapt<List<UserDto>>();
            
            _appLog.Information("数据库查询完成，返回 {Count} 条用户记录，总数: {TotalNum}", 
                userDtos.Count, result.TotalNum);
            
            var pagedResult = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalNum = result.TotalNum,
                PageIndex = pageIndex,
                PageSize = pageSize
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
    /// 高级查询用户列表
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>分页用户列表</returns>
    public async Task<Result<PagedResult<UserDto>>> GetListAsync(UserQueryDto query)
    {
        _appLog.Information("开始高级查询用户列表，参数: {Query}", query);
        
        try
        {
            // 构建查询条件
            var whereExpression = BuildWhereExpression(query);
            
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
            
            _appLog.Information("高级查询完成，返回 {Count} 条用户记录，总数: {TotalNum}", 
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
            _appLog.Error(ex, "高级查询用户数据失败");
            return Result<PagedResult<UserDto>>.Fail($"高级查询用户数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询条件表达式
    /// </summary>
    /// <param name="query">查询条件</param>
    /// <returns>查询表达式</returns>
    private Expression<Func<User, bool>>? BuildWhereExpression(UserQueryDto query)
    {
        return SqlSugar.Expressionable.Create<User>()
            .And(x => x.IsDeleted == 0)  // 0=否（未删除），1=是（已删除）

            .AndIF(!string.IsNullOrEmpty(query.Username), x => x.Username.Contains(query.Username!))
            .AndIF(!string.IsNullOrEmpty(query.Email), x => x.Email != null && x.Email.Contains(query.Email!))
            .AndIF(!string.IsNullOrEmpty(query.Phone), x => x.Phone != null && x.Phone.Contains(query.Phone!))
            .AndIF(!string.IsNullOrEmpty(query.RealName), x => x.RealName != null && x.RealName.Contains(query.RealName!))
            .AndIF(query.UserType.HasValue, x => x.UserType == query.UserType!.Value)
            .AndIF(query.UserGender.HasValue, x => x.UserGender == query.UserGender!.Value)
            .AndIF(query.UserStatus.HasValue, x => x.UserStatus == query.UserStatus!.Value)

            .ToExpression();
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息</returns>
    public async Task<Result<UserDto>> GetByIdAsync(long id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return Result<UserDto>.Fail("用户不存在");

        var userDto = user.Adapt<UserDto>();
        return Result<UserDto>.Ok(userDto);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="dto">创建用户DTO</param>
    /// <returns>新用户ID</returns>
    public async Task<Result<long>> CreateAsync(UserCreateDto dto)
    {
        var user = dto.Adapt<User>();

        var result = await _userRepository.CreateAsync(user);
        if (result > 0)
            return Result<long>.Ok(user.Id);

        return Result<long>.Fail("创建用户失败");
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="dto">更新用户DTO</param>
    /// <returns>操作结果</returns>
    public async Task<Result> UpdateAsync(UserUpdateDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id);
        if (user == null)
            return Result.Fail("用户不存在");

        dto.Adapt(user);

        var result = await _userRepository.UpdateAsync(user);
        return result > 0 ? Result.Ok() : Result.Fail("更新用户失败");
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteAsync(long id)
    {
        var result = await _userRepository.DeleteAsync(id);
        return result > 0 ? Result.Ok() : Result.Fail("删除用户失败");
    }

    // 移除按原始参数的状态修改，仅保留 DTO 方式

    /// <summary>
    /// 修改密码（用户）
    /// </summary>
    public async Task<Result> ChangePasswordAsync(UserChangePasswordDto dto)
    {
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
            return rows > 0 ? Result.Ok("修改密码成功") : Result.Fail("修改密码失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "修改密码失败");
            return Result.Fail($"修改密码失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 重置密码（管理员）
    /// </summary>
    public async Task<Result> ResetPasswordAsync(UserResetPasswordDto dto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null) return Result.Fail("用户不存在");

            user.Password = SecurityHelper.HashPassword(dto.NewPassword);
            var rows = await _userRepository.UpdateAsync(user);
            return rows > 0 ? Result.Ok("重置密码成功") : Result.Fail("重置密码失败");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "重置密码失败");
            return Result.Fail($"重置密码失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 修改用户状态（DTO）
    /// </summary>
    public async Task<Result> StatusAsync(UserStatusDto dto)
    {
        var result = await _userRepository.StatusAsync(dto.Id, (int)dto.Status);
        return result > 0 ? Result.Ok("修改状态成功") : Result.Fail("修改状态失败");
    }

    /// <summary>
    /// 导出用户到 Excel（支持条件查询）
    /// </summary>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(UserQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? BuildWhereExpression(query) : SqlSugar.Expressionable.Create<User>().And(x => x.IsDeleted == 0).ToExpression();
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

    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "Users";
        fileName ??= $"用户导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<UserDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
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
                    var existing = await _userRepository.GetFirstAsync(u => u.Username == dto.Username && u.IsDeleted == 0);
                    if (existing == null)
                    {
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
                            Remarks = dto.Remarks
                        };
                        await _userRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        existing.Email = dto.Email;
                        existing.Phone = dto.Phone;
                        existing.RealName = dto.RealName;
                        existing.UserType = dto.UserType;
                        existing.UserGender = dto.UserGender;
                        existing.UserStatus = dto.UserStatus;
                        existing.Remarks = dto.Remarks;
                        await _userRepository.UpdateAsync(existing);
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
            _appLog.Error(ex, "从Excel导入用户失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }
}
