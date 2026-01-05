// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedLogisticsProdModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品机种种子数据初始化
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;
using Takt.Common.Logging;
using Takt.Domain.Entities.Logistics.Materials;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Logistics 模块产品机种种子初始化器
/// </summary>
public class DbSeedProdModel
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<ProdModel> _prodModelRepository;

    public DbSeedProdModel(
        InitLogManager initLog,
        IBaseRepository<ProdModel> prodModelRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _prodModelRepository = prodModelRepository ?? throw new ArgumentNullException(nameof(prodModelRepository));
    }

    /// <summary>
    /// 执行产品机种种子数据初始化（创建或更新）
    /// </summary>
    public void Run()
    {
        foreach (var seed in BuildProdModelSeeds())
        {
            // 查询未删除的记录（根据物料编码、机种编码、仕向编码组合查询）
            var existing = _prodModelRepository.GetFirst(p => 
                p.MaterialCode == seed.MaterialCode && 
                p.ModelCode == seed.ModelCode && 
                p.DestCode == seed.DestCode && 
                p.IsDeleted == 0);

            if (existing == null)
            {
                // 检查是否存在已删除的记录，如果存在则恢复
                var deleted = _prodModelRepository.GetFirst(p => 
                    p.MaterialCode == seed.MaterialCode && 
                    p.ModelCode == seed.ModelCode && 
                    p.DestCode == seed.DestCode && 
                    p.IsDeleted == 1);
                if (deleted != null)
                {
                    // 恢复已删除的记录
                    deleted.IsDeleted = 0;
                    _prodModelRepository.Update(deleted, "Takt365");
                    _initLog.Information("✅ 恢复产品机种：MaterialCode={MaterialCode}, ModelCode={ModelCode}, DestCode={DestCode}",
                        seed.MaterialCode, seed.ModelCode, seed.DestCode);
                }
                else
                {
                    // 创建新记录
                    _prodModelRepository.Create(seed, "Takt365");
                    _initLog.Information("✅ 创建产品机种：MaterialCode={MaterialCode}, ModelCode={ModelCode}, DestCode={DestCode}",
                        seed.MaterialCode, seed.ModelCode, seed.DestCode);
                }
            }
            else
            {
                // 记录已存在，跳过
                _initLog.Information("✅ 产品机种已存在：MaterialCode={MaterialCode}, ModelCode={ModelCode}, DestCode={DestCode}",
                    seed.MaterialCode, seed.ModelCode, seed.DestCode);
            }
        }

        _initLog.Information("✅ 产品机种种子数据初始化完成");
    }

    private static List<ProdModel> BuildProdModelSeeds()
    {
        return new List<ProdModel>
        {
            new ProdModel
            {
                MaterialCode = "09NT57TS04",
                ModelCode = "NT-507T",
                DestCode = "KOR"
            }
        };
    }
}
