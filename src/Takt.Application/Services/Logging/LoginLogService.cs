// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logging
// 文件名称：LoginLogService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：登录日志服务实现
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Linq.Expressions;
using Takt.Application.Dtos.Logging;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logging;
using Takt.Domain.Repositories;
using Mapster;
using SqlSugar;

namespace Takt.Application.Services.Logging;

/// <summary>
/// 登录日志服务实现
/// </summary>
public class LoginLogService : ILoginLogService
{
    private readonly IBaseRepository<LoginLog> _loginLogRepository;
    private readonly AppLogManager _appLog;

    public LoginLogService(IBaseRepository<LoginLog> loginLogRepository, AppLogManager appLog)
    {
        _loginLogRepository = loginLogRepository ?? throw new ArgumentNullException(nameof(loginLogRepository));
        _appLog = appLog ?? throw new ArgumentNullException(nameof(appLog));
    }

    /// <summary>
    /// 查询登录日志列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、用户名、登录IP等筛选条件</param>
    /// <returns>包含分页登录日志列表的结果对象，成功时返回日志列表和总数，失败时返回错误信息</returns>
    /// <remarks>
    /// 此方法仅用于查询，不会记录操作日志
    /// 支持关键字搜索（在用户名、登录IP、机器名中搜索）
    /// 支持按用户名、登录时间排序，默认按登录时间倒序
    /// </remarks>
    public async Task<Result<PagedResult<LoginLogDto>>> GetListAsync(LoginLogQueryDto query)
    {
        _appLog.Information("开始查询登录日志列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式（日志通常按时间倒序）
            System.Linq.Expressions.Expression<Func<LoginLog, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "username":
                        orderByExpression = log => log.Username;
                        break;
                    case "logintime":
                        orderByExpression = log => log.LoginTime;
                        break;
                    default:
                        orderByExpression = log => log.LoginTime;
                        break;
                }
            }
            else
            {
                orderByExpression = log => log.LoginTime; // 默认按时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _loginLogRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var loginLogDtos = result.Items.Adapt<List<LoginLogDto>>();

            var pagedResult = new PagedResult<LoginLogDto>
            {
                Items = loginLogDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<LoginLogDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询登录日志数据失败");
            return Result<PagedResult<LoginLogDto>>.Fail($"查询登录日志数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构建查询表达式
    /// </summary>
    private Expression<Func<LoginLog, bool>> QueryExpression(LoginLogQueryDto query)
    {
        return SqlSugar.Expressionable.Create<LoginLog>()
            .And(log => log.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), log => log.Username.Contains(query.Keywords!) ||
                                                                 (log.LoginIp != null && log.LoginIp.Contains(query.Keywords!)) ||
                                                                 (log.MachineName != null && log.MachineName.Contains(query.Keywords!)))
            .AndIF(!string.IsNullOrEmpty(query.Username), log => log.Username.Contains(query.Username!))
            .AndIF(!string.IsNullOrEmpty(query.LoginIp), log => log.LoginIp != null && log.LoginIp.Contains(query.LoginIp!))
            .AndIF(query.LoginStatus.HasValue, log => log.LoginStatus == query.LoginStatus!.Value)
            .AndIF(query.LoginTimeFrom.HasValue, log => log.LoginTime >= query.LoginTimeFrom!.Value)
            .AndIF(query.LoginTimeTo.HasValue, log => log.LoginTime <= query.LoginTimeTo!.Value)
            .ToExpression();
    }
}

