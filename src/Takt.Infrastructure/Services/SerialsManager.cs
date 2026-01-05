//=======================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.Services
// 文件名 : SerialsManager.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 序列号拆分管理器实现（基础设施层）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//=======================================

using Takt.Common.Logging;
using Takt.Domain.Entities.Logistics.Materials;
using Takt.Domain.Entities.Logistics.Serials;
using Takt.Domain.Interfaces;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Services;

/// <summary>
/// 序列号拆分管理器
/// 提供出入库序列号拆分功能，将完整序列号拆分为物料代码、序列号、数量
/// </summary>
public class SerialsManager : ISerialsManager
{
    private readonly AppLogManager _appLog;
    private readonly IBaseRepository<ProdSerialInbound> _prodSerialInboundRepository;
    private readonly IBaseRepository<ProdSerialOutbound> _prodSerialOutboundRepository;
    private readonly IBaseRepository<ProdPacking> _prodPackingRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="appLog">应用程序日志管理器</param>
    /// <param name="prodSerialInboundRepository">产品序列号入库仓储</param>
    /// <param name="prodSerialOutboundRepository">产品序列号出库仓储</param>
    /// <param name="prodPackingRepository">包装信息仓储</param>
    public SerialsManager(
        AppLogManager appLog,
        IBaseRepository<ProdSerialInbound> prodSerialInboundRepository,
        IBaseRepository<ProdSerialOutbound> prodSerialOutboundRepository,
        IBaseRepository<ProdPacking> prodPackingRepository)
    {
        _appLog = appLog;
        _prodSerialInboundRepository = prodSerialInboundRepository;
        _prodSerialOutboundRepository = prodSerialOutboundRepository;
        _prodPackingRepository = prodPackingRepository;
        _appLog.Information("SerialsManager 构造函数完成");
    }

    /// <summary>
    /// 初始化（异步预加载数据）
    /// </summary>
    public Task InitializeAsync()
    {
        try
        {
            _appLog.Information("[SerialsManager] 开始初始化序列号拆分管理器");
            
            // 可以在这里预加载一些配置数据，如序列号格式规则等
            // 目前暂时不需要预加载数据
            
            _appLog.Information("[SerialsManager] 序列号拆分管理器初始化完成");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[SerialsManager] 初始化失败");
        }
        
        return Task.CompletedTask;
    }

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
    public SerialNumberSplitResult? SplitFullSerialNumber(string? fullSerialNumber)
    {
        var result = new SerialNumberSplitResult
        {
            IsSuccess = false
        };

        if (string.IsNullOrWhiteSpace(fullSerialNumber))
        {
            result.ErrorMessage = "完整序列号不能为空";
            _appLog.Warning("[SerialsManager] 拆分失败：完整序列号为空");
            return result;
        }

        try
        {
            // 去除首尾空格
            var trimmedSerialNumber = fullSerialNumber.Trim();
            
            // 判断是否为19位格式（无连字符）
            if (trimmedSerialNumber.Length == 19 && !trimmedSerialNumber.Contains('-'))
            {
                // 19位格式：前10位物料号 + 6位序列号起始 + 2位台数
                // 例如：091AE4DG0024Z011105
                result.MaterialCode = trimmedSerialNumber.AsSpan(0, 10).ToString(); // 前10位：物料号
                result.SerialNumber = trimmedSerialNumber.AsSpan(10, 6).ToString(); // 第11-16位：序列号起始
                
                // 第17-19位：台数（2位）
                if (decimal.TryParse(trimmedSerialNumber.AsSpan(16, 2), out decimal quantity))
                {
                    result.Quantity = quantity;
                }
                else
                {
                    result.ErrorMessage = $"台数部分格式不正确：{trimmedSerialNumber.Substring(16, 2)}";
                    _appLog.Warning("[SerialsManager] 拆分失败：台数部分格式不正确，完整序列号={FullSerialNumber}，台数部分={QuantityPart}", 
                        trimmedSerialNumber, trimmedSerialNumber.Substring(16, 2));
                    return result;
                }

                result.IsSuccess = true;
                _appLog.Information("[SerialsManager] 拆分成功（19位格式）：完整序列号={FullSerialNumber}，物料代码={MaterialCode}，序列号={SerialNumber}，数量={Quantity}", 
                    trimmedSerialNumber, result.MaterialCode, result.SerialNumber, result.Quantity);
                return result;
            }
            
            // 连字符格式：MaterialCode-SerialNumber-Quantity
            var parts = trimmedSerialNumber.Split('-', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 2)
            {
                result.ErrorMessage = "完整序列号格式不正确，支持格式：1) 19位无连字符格式（前10位物料号+6位序列号+2位台数） 2) 连字符格式（MaterialCode-SerialNumber-Quantity）";
                _appLog.Warning("[SerialsManager] 拆分失败：格式不正确，完整序列号={FullSerialNumber}，长度={Length}，分割后部分数={Count}", 
                    trimmedSerialNumber, trimmedSerialNumber.Length, parts.Length);
                return result;
            }

            // 第一部分：物料代码
            result.MaterialCode = parts[0].Trim();
            
            // 第二部分：序列号
            result.SerialNumber = parts[1].Trim();
            
            // 第三部分：数量（可选，默认为1）
            if (parts.Length >= 3)
            {
                if (decimal.TryParse(parts[2].Trim(), out decimal quantity))
                {
                    result.Quantity = quantity;
                }
                else
                {
                    result.ErrorMessage = $"数量部分格式不正确：{parts[2]}";
                    _appLog.Warning("[SerialsManager] 拆分失败：数量部分格式不正确，完整序列号={FullSerialNumber}，数量部分={QuantityPart}", 
                        trimmedSerialNumber, parts[2]);
                    return result;
                }
            }
            else
            {
                result.Quantity = 1; // 默认数量为1
            }

            result.IsSuccess = true;
            _appLog.Information("[SerialsManager] 拆分成功（连字符格式）：完整序列号={FullSerialNumber}，物料代码={MaterialCode}，序列号={SerialNumber}，数量={Quantity}", 
                trimmedSerialNumber, result.MaterialCode, result.SerialNumber, result.Quantity);
            return result;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[SerialsManager] 拆分完整序列号异常，完整序列号={FullSerialNumber}", fullSerialNumber);
            result.ErrorMessage = $"拆分失败: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// 生成完整序列号
    /// 根据物料代码长度决定格式：
    /// - 如果物料代码为10位且序列号为6位，生成19位格式（前10位物料号+6位序列号+2位台数）
    /// - 否则生成连字符格式（MaterialCode-SerialNumber-Quantity）
    /// </summary>
    /// <param name="materialCode">物料代码</param>
    /// <param name="serialNumber">序列号</param>
    /// <param name="quantity">数量（decimal类型）</param>
    /// <returns>完整序列号</returns>
    public string GenerateFullSerialNumber(string? materialCode, string? serialNumber, decimal quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(materialCode) || string.IsNullOrWhiteSpace(serialNumber))
        {
            _appLog.Warning("[SerialsManager] 生成完整序列号失败：物料代码或序列号为空");
            return string.Empty;
        }

        if (quantity <= 0)
        {
            quantity = 1;
        }

        var trimmedMaterialCode = materialCode.Trim();
        var trimmedSerialNumber = serialNumber.Trim();
        
        // 如果物料代码为10位且序列号为6位，生成19位格式
        if (trimmedMaterialCode.Length == 10 && trimmedSerialNumber.Length == 6 && quantity <= 99)
        {
            // 19位格式：前10位物料号 + 6位序列号 + 2位台数
            var quantityStr = ((int)quantity).ToString("D2"); // 格式化为2位数字
            var fullSerialNumber = $"{trimmedMaterialCode}{trimmedSerialNumber}{quantityStr}";
            _appLog.Information("[SerialsManager] 生成完整序列号成功（19位格式）：{FullSerialNumber}", fullSerialNumber);
            return fullSerialNumber;
        }
        
        // 连字符格式
        var fullSerialNumberWithDash = $"{trimmedMaterialCode}-{trimmedSerialNumber}-{quantity}";
        _appLog.Information("[SerialsManager] 生成完整序列号成功（连字符格式）：{FullSerialNumber}", fullSerialNumberWithDash);
        return fullSerialNumberWithDash;
    }

    /// <summary>
    /// 保存入库记录
    /// 拆分完整序列号并写入入库表，如果已存在则不重复添加
    /// </summary>
    /// <param name="fullSerialNumber">完整序列号</param>
    /// <param name="inboundNo">入库单号（可选）</param>
    /// <param name="inboundDate">入库日期（可选，默认为当前日期）</param>
    /// <returns>保存结果，包含是否成功、记录ID（如果新增）或已存在记录的ID</returns>
    public async Task<SerialNumberSaveResult> SaveInboundAsync(string? fullSerialNumber, string? inboundNo = null!, DateTime? inboundDate = null)
    {
        var result = new SerialNumberSaveResult
        {
            IsSuccess = false,
            IsExisting = false
        };

        if (string.IsNullOrWhiteSpace(fullSerialNumber))
        {
            result.ErrorMessage = "完整序列号不能为空";
            _appLog.Warning("[SerialsManager] 保存入库记录失败：完整序列号为空");
            return result;
        }

        try
        {
            var trimmedFullSerialNumber = fullSerialNumber.Trim();

            // 检查是否已存在
            var existing = await _prodSerialInboundRepository.GetFirstAsync(
                psi => psi.FullSerialNumber == trimmedFullSerialNumber && psi.IsDeleted == 0);

            if (existing != null)
            {
                result.IsSuccess = true;
                result.IsExisting = true;
                result.RecordId = existing.Id;
                _appLog.Information("[SerialsManager] 入库记录已存在，不重复添加，完整序列号={FullSerialNumber}，记录ID={RecordId}", 
                    trimmedFullSerialNumber, existing.Id);
                return result;
            }

            // 拆分完整序列号
            var splitResult = SplitFullSerialNumber(trimmedFullSerialNumber);
            if (splitResult == null || !splitResult.IsSuccess)
            {
                result.ErrorMessage = splitResult?.ErrorMessage ?? "拆分序列号失败";
                _appLog.Warning("[SerialsManager] 保存入库记录失败：{ErrorMessage}，完整序列号={FullSerialNumber}", 
                    result.ErrorMessage, trimmedFullSerialNumber);
                return result;
            }

            // 创建新记录
            var prodSerialInbound = new ProdSerialInbound
            {
                FullSerialNumber = trimmedFullSerialNumber,
                MaterialCode = splitResult.MaterialCode,
                SerialNumber = splitResult.SerialNumber,
                Quantity = splitResult.Quantity,
                InboundNo = inboundNo ?? string.Empty,
                InboundDate = inboundDate ?? DateTime.Now
            };

            var recordId = await _prodSerialInboundRepository.CreateAsync(prodSerialInbound);
            
            result.IsSuccess = true;
            result.IsExisting = false;
            result.RecordId = recordId;
            _appLog.Information("[SerialsManager] 保存入库记录成功，完整序列号={FullSerialNumber}，物料代码={MaterialCode}，序列号={SerialNumber}，数量={Quantity}，记录ID={RecordId}", 
                trimmedFullSerialNumber, splitResult.MaterialCode, splitResult.SerialNumber, splitResult.Quantity, recordId);
            return result;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[SerialsManager] 保存入库记录异常，完整序列号={FullSerialNumber}", fullSerialNumber);
            result.ErrorMessage = $"保存入库记录失败: {ex.Message}";
            return result;
        }
    }

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
    public async Task<SerialNumberSaveResult> SaveOutboundAsync(string? fullSerialNumber, string? outboundNo = null!, DateTime? outboundDate = null!, string? destCode = null!, string? destPort = null!, string? sizeDimension = null)
    {
        var result = new SerialNumberSaveResult
        {
            IsSuccess = false,
            IsExisting = false
        };

        if (string.IsNullOrWhiteSpace(fullSerialNumber))
        {
            result.ErrorMessage = "完整序列号不能为空";
            _appLog.Warning("[SerialsManager] 保存出库记录失败：完整序列号为空");
            return result;
        }

        try
        {
            var trimmedFullSerialNumber = fullSerialNumber.Trim();

            // 检查是否已存在
            var existing = await _prodSerialOutboundRepository.GetFirstAsync(
                pso => pso.FullSerialNumber == trimmedFullSerialNumber && pso.IsDeleted == 0);

            if (existing != null)
            {
                result.IsSuccess = true;
                result.IsExisting = true;
                result.RecordId = existing.Id;
                _appLog.Information("[SerialsManager] 出库记录已存在，不重复添加，完整序列号={FullSerialNumber}，记录ID={RecordId}", 
                    trimmedFullSerialNumber, existing.Id);
                return result;
            }

            // 拆分完整序列号
            var splitResult = SplitFullSerialNumber(trimmedFullSerialNumber);
            if (splitResult == null || !splitResult.IsSuccess)
            {
                result.ErrorMessage = splitResult?.ErrorMessage ?? "拆分序列号失败";
                _appLog.Warning("[SerialsManager] 保存出库记录失败：{ErrorMessage}，完整序列号={FullSerialNumber}", 
                    result.ErrorMessage, trimmedFullSerialNumber);
                return result;
            }

            // 查询包装信息
            // 注意：所有包装物料都有三位前缀，所以需要处理前缀匹配
            // 先尝试精确匹配（以防万一）
            var packing = await _prodPackingRepository.GetFirstAsync(
                pp => pp.MaterialCode == splitResult.MaterialCode && pp.IsDeleted == 0);
            
            // 如果精确匹配失败，尝试匹配以实际物料编码结尾的记录（处理三位前缀）
            // 包装物料的格式：前缀(3位) + 实际物料编码
            if (packing == null && !string.IsNullOrWhiteSpace(splitResult.MaterialCode))
            {
                _appLog.Information("[SerialsManager] 精确匹配失败，尝试后缀匹配：序列号中的物料编码={MaterialCodeFromSerial}", 
                    splitResult.MaterialCode);
                
                packing = await _prodPackingRepository.GetFirstAsync(
                    pp => pp.MaterialCode.EndsWith(splitResult.MaterialCode) && pp.IsDeleted == 0);
                
                if (packing != null)
                {
                    var prefix = packing.MaterialCode.Length > splitResult.MaterialCode.Length 
                        ? packing.MaterialCode[..(packing.MaterialCode.Length - splitResult.MaterialCode.Length)] 
                        : "未知";
                    _appLog.Information("[SerialsManager] 通过后缀匹配找到包装信息：序列号中的物料编码={MaterialCodeFromSerial}，包装信息中的物料编码={MaterialCodeInPacking}，前缀={Prefix}", 
                        splitResult.MaterialCode, packing.MaterialCode, prefix);
                }
                else
                {
                    _appLog.Warning("[SerialsManager] 未找到物料编码={MaterialCode}的包装信息（精确匹配和后缀匹配均失败）", splitResult.MaterialCode);
                }
            }
            else if (packing != null)
            {
                _appLog.Information("[SerialsManager] 通过精确匹配找到包装信息：物料编码={MaterialCode}", splitResult.MaterialCode);
            }

            // 计算重量、体积、箱数
            decimal? calculatedWeight = null;
            decimal? calculatedVolume = null;
            // 一个完整序列号就是一箱，所以箱数直接设置为 1
            int calculatedCarQuantity = 1;

            if (packing != null)
            {
                // 计算重量：使用毛重（GrossWeight），如果毛重为空则使用净重（NetWeight）
                // 重量 = 单个包装重量 × 数量
                if (packing.GrossWeight.HasValue && packing.GrossWeight.Value > 0)
                {
                    calculatedWeight = packing.GrossWeight.Value * splitResult.Quantity;
                    _appLog.Information("[SerialsManager] 根据包装信息计算重量：单个包装毛重={GrossWeight} {WeightUnit}，数量={Quantity}，总重量={TotalWeight} {WeightUnit}", 
                        packing.GrossWeight.Value, packing.WeightUnit, splitResult.Quantity, calculatedWeight.Value, packing.WeightUnit);
                }
                else if (packing.NetWeight.HasValue && packing.NetWeight.Value > 0)
                {
                    calculatedWeight = packing.NetWeight.Value * splitResult.Quantity;
                    _appLog.Information("[SerialsManager] 根据包装信息计算重量：单个包装净重={NetWeight} {WeightUnit}，数量={Quantity}，总重量={TotalWeight} {WeightUnit}", 
                        packing.NetWeight.Value, packing.WeightUnit, splitResult.Quantity, calculatedWeight.Value, packing.WeightUnit);
                }

                // 计算体积：使用业务量（一个包装单位的体积）
                // BusinessVolume 是包装信息中直接提供的体积数据，不需要根据大小量纲计算
                if (packing.BusinessVolume.HasValue && packing.BusinessVolume.Value > 0)
                {
                    // 体积 = 单个包装体积 × 数量
                    calculatedVolume = packing.BusinessVolume.Value * splitResult.Quantity;
                    _appLog.Information("[SerialsManager] 根据包装信息中的业务量计算体积：单个包装体积={BusinessVolume} {VolumeUnit}，数量={Quantity}，总体积={TotalVolume} {VolumeUnit}", 
                        packing.BusinessVolume.Value, packing.VolumeUnit, splitResult.Quantity, calculatedVolume.Value, packing.VolumeUnit);
                }

                // 箱数（卡通箱数量）：一个完整序列号就是一箱，所以箱数固定为 1
                // Car 表示 Carton（卡通箱），CarQuantity 表示卡通箱数量
                _appLog.Information("[SerialsManager] 箱数（卡通箱数量）：一个完整序列号对应一箱，箱数={CarQuantity}", calculatedCarQuantity);
            }
            else
            {
                // 如果没有找到包装信息，无法计算重量、体积、箱数
                _appLog.Warning("[SerialsManager] 未找到物料编码={MaterialCode}的包装信息，无法自动计算重量、体积、箱数", splitResult.MaterialCode);
            }

            // 创建新记录
            var prodSerialOutbound = new ProdSerialOutbound
            {
                FullSerialNumber = trimmedFullSerialNumber,
                MaterialCode = splitResult.MaterialCode,
                SerialNumber = splitResult.SerialNumber,
                Quantity = splitResult.Quantity,
                OutboundNo = outboundNo ?? string.Empty,
                OutboundDate = outboundDate ?? DateTime.Now,
                DestCode = destCode,
                DestPort = destPort,
                Weight = calculatedWeight,
                Volume = calculatedVolume,
                CarQuantity = calculatedCarQuantity
            };

            var recordId = await _prodSerialOutboundRepository.CreateAsync(prodSerialOutbound);
            
            result.IsSuccess = true;
            result.IsExisting = false;
            result.RecordId = recordId;
            _appLog.Information("[SerialsManager] 保存出库记录成功，完整序列号={FullSerialNumber}，物料代码={MaterialCode}，序列号={SerialNumber}，数量={Quantity}，出库单号={OutboundNo}，记录ID={RecordId}", 
                trimmedFullSerialNumber, splitResult.MaterialCode, splitResult.SerialNumber, splitResult.Quantity, outboundNo ?? "无", recordId);
            return result;
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "[SerialsManager] 保存出库记录异常，完整序列号={FullSerialNumber}", fullSerialNumber);
            result.ErrorMessage = $"保存出库记录失败: {ex.Message}";
            return result;
        }
    }
}

