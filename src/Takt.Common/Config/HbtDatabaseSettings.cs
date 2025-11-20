//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : HbtDatabaseSettings.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 版本号 : 0.0.1
// 描述    : 数据库配置
//===================================================================

namespace Takt.Common.Config;

/// <summary>
/// 数据库配置
/// </summary>
public class HbtDatabaseSettings
{
    /// <summary>
    /// 是否启用CodeFirst
    /// </summary>
    public bool EnableCodeFirst { get; set; } = true;

    /// <summary>
    /// 是否启用种子数据
    /// </summary>
    public bool EnableSeedData { get; set; } = true;

    /// <summary>
    /// 是否启用雪花ID
    /// </summary>
    public bool EnableSnowflakeId { get; set; } = true;

    /// <summary>
    /// 雪花ID工作机器ID
    /// </summary>
    /// <remarks>
    /// 取值范围：0-31
    /// </remarks>
    public int SnowflakeWorkerId { get; set; } = 1;

    /// <summary>
    /// 雪花ID数据中心ID
    /// </summary>
    /// <remarks>
    /// 取值范围：0-31（暂未使用）
    /// </remarks>
    public int SnowflakeDatacenterId { get; set; } = 1;

    /// <summary>
    /// 是否启用审计日志
    /// </summary>
    /// <remarks>
    /// 自动填充创建人、创建时间、更新人、更新时间
    /// </remarks>
    public bool EnableAuditLog { get; set; } = true;

    /// <summary>
    /// 是否启用差异日志
    /// </summary>
    /// <remarks>
    /// 记录数据变更前后的差异
    /// </remarks>
    public bool EnableDiffLog { get; set; } = true;

    /// <summary>
    /// 是否启用SQL日志
    /// </summary>
    /// <remarks>
    /// 记录SQL执行日志
    /// </remarks>
    public bool EnableSqlLog { get; set; } = true;

    /// <summary>
    /// 慢查询阈值（毫秒）
    /// </summary>
    /// <remarks>
    /// 超过此阈值的SQL会被记录为慢查询
    /// </remarks>
    public int SlowQueryThreshold { get; set; } = 1000;
}

