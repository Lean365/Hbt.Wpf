//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : ISerialsManager.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 序列号拆分管理器接口（领域层）
//===================================================================

namespace Takt.Domain.Interfaces;

/// <summary>
/// 序列号拆分管理器接口
/// 提供出入库序列号拆分功能，将完整序列号拆分为物料代码、序列号、数量
/// </summary>
public interface ISerialsManager
{
    /// <summary>
    /// 初始化（异步预加载数据）
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 拆分完整序列号
    /// 支持两种格式：
    /// 1. 19位格式：前10位物料号 + 6位序列号起始 + 2位台数
    ///    例如：091AE4DG0024Z011105（物料号：091AE4DG00，序列号：24Z011，台数：05）
    /// 2. 连字符格式：MaterialCode-SerialNumber-Quantity
    ///    例如：09NT57TS04-NT-507T-1
    /// </summary>
    /// <param name="fullSerialNumber">完整序列号</param>
    /// <returns>拆分结果，包含物料代码、序列号、数量</returns>
    SerialNumberSplitResult? SplitFullSerialNumber(string? fullSerialNumber);

    /// <summary>
    /// 生成完整序列号
    /// 格式：MaterialCode-SerialNumber-Quantity
    /// </summary>
    /// <param name="materialCode">物料代码</param>
    /// <param name="serialNumber">序列号</param>
    /// <param name="quantity">数量（decimal类型）</param>
    /// <returns>完整序列号</returns>
    string GenerateFullSerialNumber(string? materialCode, string? serialNumber, decimal quantity = 1);

    /// <summary>
    /// 保存入库记录
    /// 拆分完整序列号并写入入库表，如果已存在则不重复添加
    /// </summary>
    /// <param name="fullSerialNumber">完整序列号</param>
    /// <param name="inboundNo">入库单号（可选）</param>
    /// <param name="inboundDate">入库日期（可选，默认为当前日期）</param>
    /// <returns>保存结果，包含是否成功、记录ID（如果新增）或已存在记录的ID</returns>
    Task<SerialNumberSaveResult> SaveInboundAsync(string? fullSerialNumber, string? inboundNo = null!, DateTime? inboundDate = null);

    /// <summary>
    /// 保存出库记录
    /// 拆分完整序列号并写入出库表，如果已存在则不重复添加
    /// 体积根据包装信息中的 BusinessVolume 自动计算
    /// </summary>
    /// <param name="fullSerialNumber">完整序列号</param>
    /// <param name="outboundNo">出库单号（可选）</param>
    /// <param name="outboundDate">出库日期（可选，默认为当前日期）</param>
    /// <param name="destCode">仕向编码（可选）</param>
    /// <param name="destPort">目的地港口（可选）</param>
    /// <param name="sizeDimension">大小量纲（已废弃，不再使用，体积从包装信息的 BusinessVolume 获取）</param>
    /// <returns>保存结果，包含是否成功、记录ID（如果新增）或已存在记录的ID</returns>
    Task<SerialNumberSaveResult> SaveOutboundAsync(string? fullSerialNumber, string? outboundNo = null!, DateTime? outboundDate = null!, string? destCode = null!, string? destPort = null!, string? sizeDimension = null);
}

/// <summary>
/// 序列号保存结果
/// </summary>
public class SerialNumberSaveResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 记录ID（新增或已存在的记录ID）
    /// </summary>
    public long? RecordId { get; set; }

    /// <summary>
    /// 是否已存在（true表示记录已存在，false表示新增）
    /// </summary>
    public bool IsExisting { get; set; }

    /// <summary>
    /// 错误消息（如果保存失败）
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 序列号拆分结果
/// </summary>
public class SerialNumberSplitResult
{
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 数量（decimal类型）
    /// </summary>
    public decimal Quantity { get; set; } = 0;

    /// <summary>
    /// 是否拆分成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息（如果拆分失败）
    /// </summary>
    public string? ErrorMessage { get; set; }
}

