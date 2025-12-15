// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedVisit.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：随行人员种子数据初始化
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;
using Takt.Common.Logging;
using Takt.Domain.Entities.Logistics.Visits;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Logistics 模块随行人员种子初始化器
/// </summary>
public class DbSeedVisit
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<VisitingCompany> _visitorRepository;
    private readonly IBaseRepository<VisitingEntourage> _visitorDetailRepository;

    public DbSeedVisit(
        InitLogManager initLog,
        IBaseRepository<VisitingCompany> visitorRepository,
        IBaseRepository<VisitingEntourage> visitorDetailRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _visitorRepository = visitorRepository ?? throw new ArgumentNullException(nameof(visitorRepository));
        _visitorDetailRepository = visitorDetailRepository ?? throw new ArgumentNullException(nameof(visitorDetailRepository));
    }

    /// <summary>
    /// 执行随行人员种子数据初始化（创建或更新）
    /// </summary>
    public void Run()
    {
        var seeds = BuildVisitSeeds();
        
        foreach (var seed in seeds)
        {
            // 查询未删除的记录（根据公司名称和访问时间组合查询）
            var existing = _visitorRepository.GetFirst(v => 
                v.VisitingCompanyName == seed.Entourage.VisitingCompanyName && 
                v.VisitStartTime == seed.Entourage.VisitStartTime &&
                v.IsDeleted == 0);

            long visitorId;

            if (existing == null)
            {
                // 检查是否存在已删除的记录，如果存在则恢复
                var deleted = _visitorRepository.GetFirst(v => 
                    v.VisitingCompanyName == seed.Entourage.VisitingCompanyName && 
                    v.VisitStartTime == seed.Entourage.VisitStartTime &&
                    v.IsDeleted == 1);
                
                if (deleted != null)
                {
                    // 恢复已删除的记录，更新所有字段
                    deleted.VisitEndTime = seed.Entourage.VisitEndTime;
                    deleted.ReservationsDept = seed.Entourage.ReservationsDept;
                    deleted.Contact = seed.Entourage.Contact;
                    deleted.Purpose = seed.Entourage.Purpose;
                    deleted.Duration = seed.Entourage.Duration;
                    deleted.Industry = seed.Entourage.Industry;
                    deleted.VehiclePlate = seed.Entourage.VehiclePlate;
                    deleted.IsWelcomeSign = seed.Entourage.IsWelcomeSign;
                    deleted.IsVehicleNeeded = seed.Entourage.IsVehicleNeeded;
                    deleted.IsDeleted = 0; // 恢复删除标记
                    _visitorRepository.Update(deleted, "Takt365");
                    visitorId = deleted.Id;
                    _initLog.Information("✅ 恢复随行人员记录：{CompanyName}, {StartTime}", 
                        seed.Entourage.VisitingCompanyName, seed.Entourage.VisitStartTime);
                }
                else
                {
                    // 创建新随行人员记录
                    _visitorRepository.Create(seed.Entourage, "Takt365");
                    visitorId = seed.Entourage.Id;
                    _initLog.Information("✅ 创建随行人员记录：{CompanyName}, {StartTime}", 
                        seed.Entourage.VisitingCompanyName, seed.Entourage.VisitStartTime);
                }
            }
            else
            {
                // 检查是否需要更新（比较所有字段）
                bool needsUpdate = false;
                
                if (existing.VisitEndTime != seed.Entourage.VisitEndTime)
                {
                    existing.VisitEndTime = seed.Entourage.VisitEndTime;
                    needsUpdate = true;
                }
                if (existing.ReservationsDept != seed.Entourage.ReservationsDept)
                {
                    existing.ReservationsDept = seed.Entourage.ReservationsDept;
                    needsUpdate = true;
                }
                if (existing.Contact != seed.Entourage.Contact)
                {
                    existing.Contact = seed.Entourage.Contact;
                    needsUpdate = true;
                }
                if (existing.Purpose != seed.Entourage.Purpose)
                {
                    existing.Purpose = seed.Entourage.Purpose;
                    needsUpdate = true;
                }
                if (existing.Duration != seed.Entourage.Duration)
                {
                    existing.Duration = seed.Entourage.Duration;
                    needsUpdate = true;
                }
                if (existing.Industry != seed.Entourage.Industry)
                {
                    existing.Industry = seed.Entourage.Industry;
                    needsUpdate = true;
                }
                if (existing.VehiclePlate != seed.Entourage.VehiclePlate)
                {
                    existing.VehiclePlate = seed.Entourage.VehiclePlate;
                    needsUpdate = true;
                }
                if (existing.IsWelcomeSign != seed.Entourage.IsWelcomeSign)
                {
                    existing.IsWelcomeSign = seed.Entourage.IsWelcomeSign;
                    needsUpdate = true;
                }
                if (existing.IsVehicleNeeded != seed.Entourage.IsVehicleNeeded)
                {
                    existing.IsVehicleNeeded = seed.Entourage.IsVehicleNeeded;
                    needsUpdate = true;
                }
                
                if (needsUpdate)
                {
                    _visitorRepository.Update(existing, "Takt365");
                    _initLog.Information("✅ 更新随行人员记录：{CompanyName}, {StartTime}", 
                        seed.Entourage.VisitingCompanyName, seed.Entourage.VisitStartTime);
                }
                else
                {
                    _initLog.Information("✅ 随行人员记录已存在且无需更新：{CompanyName}, {StartTime}", 
                        seed.Entourage.VisitingCompanyName, seed.Entourage.VisitStartTime);
                }
                
                visitorId = existing.Id;
            }

            // 处理随行人员详情
            foreach (var detailSeed in seed.Details)
            {
                detailSeed.VisitingCompanyId = visitorId;
                
                // 查询是否存在相同的随行人员详情（根据随行人员ID、部门、姓名、职务组合查询）
                var existingDetail = _visitorDetailRepository.GetFirst(d => 
                    d.VisitingCompanyId == visitorId && 
                    d.VisitDept == detailSeed.VisitDept &&
                    d.VisitingMembers == detailSeed.VisitingMembers &&
                    d.VisitPost == detailSeed.VisitPost &&
                    d.IsDeleted == 0);

                if (existingDetail == null)
                {
                    // 检查是否存在已删除的记录
                    var deletedDetail = _visitorDetailRepository.GetFirst(d => 
                        d.VisitingCompanyId == visitorId && 
                        d.VisitDept == detailSeed.VisitDept &&
                        d.VisitingMembers == detailSeed.VisitingMembers &&
                        d.VisitPost == detailSeed.VisitPost &&
                        d.IsDeleted == 1);
                    
                    if (deletedDetail != null)
                    {
                        // 恢复已删除的记录，更新所有字段
                        deletedDetail.VisitDept = detailSeed.VisitDept;
                        deletedDetail.VisitingMembers = detailSeed.VisitingMembers;
                        deletedDetail.VisitPost = detailSeed.VisitPost;
                        deletedDetail.IsDeleted = 0; // 恢复删除标记
                        _visitorDetailRepository.Update(deletedDetail, "Takt365");
                        _initLog.Information("✅ 恢复随行人员详情：{Name}, {Post}, {Dept}", 
                            detailSeed.VisitingMembers, detailSeed.VisitPost, detailSeed.VisitDept);
                    }
                    else
                    {
                        // 创建新随行人员详情记录
                        _visitorDetailRepository.Create(detailSeed, "Takt365");
                        _initLog.Information("✅ 创建随行人员详情：{Name}, {Post}, {Dept}", 
                            detailSeed.VisitingMembers, detailSeed.VisitPost, detailSeed.VisitDept);
                    }
                }
                else
                {
                    // 检查是否需要更新（比较所有字段）
                    bool needsUpdate = false;
                    
                    // 注意：VisitingCompanyId 不应该更新，因为它是关联字段
                    // VisitDept、VisitingMembers、VisitPost 是查询条件，理论上不应该变化
                    // 但如果种子数据有变化，也应该更新
                    if (existingDetail.VisitDept != detailSeed.VisitDept)
                    {
                        existingDetail.VisitDept = detailSeed.VisitDept;
                        needsUpdate = true;
                    }
                    if (existingDetail.VisitingMembers != detailSeed.VisitingMembers)
                    {
                        existingDetail.VisitingMembers = detailSeed.VisitingMembers;
                        needsUpdate = true;
                    }
                    if (existingDetail.VisitPost != detailSeed.VisitPost)
                    {
                        existingDetail.VisitPost = detailSeed.VisitPost;
                        needsUpdate = true;
                    }
                    
                    if (needsUpdate)
                    {
                        _visitorDetailRepository.Update(existingDetail, "Takt365");
                        _initLog.Information("✅ 更新随行人员详情：{Name}, {Post}, {Dept}", 
                            detailSeed.VisitingMembers, detailSeed.VisitPost, detailSeed.VisitDept);
                    }
                    else
                    {
                        _initLog.Information("✅ 随行人员详情已存在且无需更新：{Name}, {Post}, {Dept}", 
                            detailSeed.VisitingMembers, detailSeed.VisitPost, detailSeed.VisitDept);
                    }
                }
            }
        }

        _initLog.Information("✅ 随行人员种子数据初始化完成");
    }

    private static List<VisitSeedData> BuildVisitSeeds()
    {
        // 设置访问时间为今天，开始时间为上午9点，结束时间为2026/12/31（测试用）
        var today = DateTime.Today;
        var visitStartTime = new DateTime(today.Year, today.Month, today.Day, 9, 0, 0);
        var visitEndTime = new DateTime(2026, 12, 31, 18, 0, 0);

        return new List<VisitSeedData>
        {
            new VisitSeedData
            {
                Entourage = new VisitingCompany
                {
                    VisitingCompanyName = "株式会社 カイジョー",
                    VisitStartTime = visitStartTime,
                    VisitEndTime = visitEndTime
                },
                Details = new List<VisitingEntourage>
                {
                    // 常务取缔役（无部门）
                    new VisitingEntourage
                    {
                        VisitDept = "",
                        VisitingMembers = "久保　覚　様",
                        VisitPost = "常務取締役"
                    },
                    new VisitingEntourage
                    {
                        VisitDept = "",  // 同一职务可省略部门
                        VisitingMembers = "青木 達也　様",
                        VisitPost = "常務取締役"
                    },
                    // 生産本部
                    new VisitingEntourage
                    {
                        VisitDept = "生産本部",
                        VisitingMembers = "増澤 保雄　様",
                        VisitPost = "生産技術部 部長代理"
                    },
                    new VisitingEntourage
                    {
                        VisitDept = "生産本部　超音波機器製造部",  // 下级部门需要完整路径
                        VisitingMembers = "増田 秀喜　様",
                        VisitPost = "生産管理課 課長"
                    }
                }
            }
        };
    }

    /// <summary>
    /// 随行人员种子数据结构
    /// </summary>
    private class VisitSeedData
    {
        public VisitingCompany Entourage { get; set; } = null!;
        public List<VisitingEntourage> Details { get; set; } = new();
    }
}

