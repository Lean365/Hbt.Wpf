// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visits
// 文件名称：VisitingDisplayItem.cs
// 创建时间：2025-01-25
// 创建人：Takt365(Cursor AI)
// 功能描述：来访成员详情显示项（用于数字标牌显示）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logistics.Visits;

namespace Takt.Fluent.ViewModels.Logistics.Visits;

/// <summary>
/// 来访成员详情显示项
/// 用于数字标牌显示，支持分组和合并相同部门
/// </summary>
public class VisitingDisplayItem
{
    /// <summary>
    /// 公司名称（用于按公司分组显示）
    /// </summary>
    public string? VisitingCompany { get; set; }

    /// <summary>
    /// 来访公司ID（用于关联公司名称）
    /// </summary>
    public long VisitingCompanyId { get; set; }

    /// <summary>
    /// 部门名称（为空表示没有部门的人员）
    /// </summary>
    public string? VisitDept { get; set; }

    /// <summary>
    /// 职务（合并多个人员的职务，用逗号分隔）
    /// </summary>
    public string VisitPost { get; set; } = string.Empty;

    /// <summary>
    /// 来访成员名称（合并多个人员，用逗号分隔）
    /// </summary>
    public string VisitingMembers { get; set; } = string.Empty;

    /// <summary>
    /// 原始来访成员详情列表（用于编辑功能）
    /// </summary>
    public List<VisitingEntourageDto> SourceDetails { get; set; } = new();

    /// <summary>
    /// 是否显示部门名称（用于标识部门名称行）
    /// 部门名称行：有部门名称，但职务和人员都为空
    /// </summary>
    public bool ShowDept => !string.IsNullOrWhiteSpace(VisitDept) && string.IsNullOrWhiteSpace(VisitPost) && string.IsNullOrWhiteSpace(VisitingMembers);

    /// <summary>
    /// 是否显示公司名称（用于标识公司名称行）
    /// 公司名称行：有公司名称，但部门、职务和人员都为空
    /// </summary>
    public bool ShowCompany => !string.IsNullOrWhiteSpace(VisitingCompany) && string.IsNullOrWhiteSpace(VisitDept) && string.IsNullOrWhiteSpace(VisitPost) && string.IsNullOrWhiteSpace(VisitingMembers);

    /// <summary>
    /// 是否显示人员信息（有职务或人员时显示）
    /// 人员行：有职务或人员，可能有部门（但不显示部门名称，只在部门名称行显示）
    /// </summary>
    public bool ShowPerson => !string.IsNullOrWhiteSpace(VisitPost) || !string.IsNullOrWhiteSpace(VisitingMembers);

    /// <summary>
    /// 从原始详情列表创建显示项列表（按公司分组）
    /// 规则：
    /// 1. 按公司分组显示
    /// 2. 每个公司下，没有部门的人员优先显示，每条显示一行（职务+人员）
    /// 3. 相同部门合并为一条，格式：部门名称 + 该部门下的所有职务+人员
    /// </summary>
    /// <param name="details">来访成员详情列表</param>
    /// <param name="visitorIdToCompanyMap">来访公司ID到公司名称的映射</param>
    public static List<VisitingDisplayItem> CreateDisplayItems(IEnumerable<VisitingEntourageDto> details, Dictionary<long, string> visitorIdToCompanyMap)
    {
        var displayItems = new List<VisitingDisplayItem>();
        var detailsList = details.ToList();

        if (!detailsList.Any())
            return displayItems;

        // **修复：按公司名称分组，而不是按随行人员ID分组**
        // 如果多个随行人员记录属于同一公司（公司名称相同），应该合并显示
        var companyGroups = detailsList
            .GroupBy(d =>
            {
                var visitingCompanyId = d.VisitingCompanyId;
                return visitorIdToCompanyMap.ContainsKey(visitingCompanyId)
                    ? visitorIdToCompanyMap[visitingCompanyId] ?? string.Empty
                    : string.Empty;
            })
            .Where(g => !string.IsNullOrWhiteSpace(g.Key)) // 过滤掉公司名称为空的记录
            .OrderBy(g => g.Key); // 按公司名称排序

        foreach (var companyGroup in companyGroups)
        {
            var companyName = companyGroup.Key;
            var groupDetails = companyGroup.ToList();

            // **获取该公司的所有来访公司ID（用于后续关联）**
            var companyVisitingCompanyIds = groupDetails.Select(d => d.VisitingCompanyId).Distinct().ToList();

            // 添加公司名称行（只显示一次，即使有多个来访公司ID属于同一公司）
            displayItems.Add(new VisitingDisplayItem
            {
                VisitingCompany = companyName,
                VisitingCompanyId = companyVisitingCompanyIds.FirstOrDefault(), // 使用第一个来访公司ID作为代表
                VisitDept = null,
                VisitPost = string.Empty,
                VisitingMembers = string.Empty,
                SourceDetails = new List<VisitingEntourageDto>() // 公司名称行不关联具体详情
            });

            // 分离有部门和无部门的记录
            var noDeptDetails = groupDetails
                .Where(d => string.IsNullOrWhiteSpace(d.VisitDept))
                .ToList();
            var withDeptDetails = groupDetails
                .Where(d => !string.IsNullOrWhiteSpace(d.VisitDept))
                .ToList();

            // 1. 优先显示没有部门的人员（每条显示一行：职务+人员）
            foreach (var detail in noDeptDetails)
            {
                displayItems.Add(new VisitingDisplayItem
                {
                    VisitingCompany = companyName,
                    VisitingCompanyId = detail.VisitingCompanyId, // 保留原始来访公司ID
                    VisitDept = null,
                    VisitPost = detail.VisitPost ?? string.Empty,
                    VisitingMembers = detail.VisitingMembers ?? string.Empty,
                    SourceDetails = new List<VisitingEntourageDto> { detail }
                });
            }

            // 2. 按部门分组，相同部门下的每个职务+人员组合显示为一行
            var deptGroups = withDeptDetails
                .GroupBy(d => d.VisitDept ?? string.Empty)
                .OrderBy(g => g.Key);

            foreach (var deptGroup in deptGroups)
            {
                var deptName = deptGroup.Key;
                var deptGroupDetails = deptGroup.ToList();

                // 先添加部门名称行（作为单独一行显示，只有部门名称，职务和人员为空）
                displayItems.Add(new VisitingDisplayItem
                {
                    VisitingCompany = companyName,
                    VisitingCompanyId = companyVisitingCompanyIds.FirstOrDefault(), // 使用第一个来访公司ID作为代表
                    VisitDept = deptName,
                    VisitPost = string.Empty, // 部门名称行，职务和人员为空
                    VisitingMembers = string.Empty,
                    SourceDetails = new List<VisitingEntourageDto>() // 部门名称行不关联具体详情
                });

                // 然后为该部门下的每个唯一的职务+人员组合添加一行
                // 注意：人员行的部门字段设为 null，因为部门名称已经在部门名称行显示了
                var postPersonPairs = deptGroupDetails
                    .Select(d => new { Post = d.VisitPost ?? string.Empty, Person = d.VisitingMembers ?? string.Empty })
                    .Distinct()
                    .OrderBy(p => p.Post)
                    .ThenBy(p => p.Person)
                    .ToList();

                foreach (var pair in postPersonPairs)
                {
                    // **合并同一公司、同一部门、同一职务+人员组合的所有记录**
                    var matchingDetails = deptGroupDetails.Where(d =>
                        (d.VisitPost ?? string.Empty) == pair.Post &&
                        (d.VisitingMembers ?? string.Empty) == pair.Person).ToList();

                    displayItems.Add(new VisitingDisplayItem
                    {
                        VisitingCompany = companyName,
                        VisitingCompanyId = matchingDetails.FirstOrDefault()?.VisitingCompanyId ?? companyVisitingCompanyIds.FirstOrDefault(), // 保留第一个匹配记录的来访公司ID
                        VisitDept = deptName, // 保留部门信息（用于UI判断缩进），但ShowDept会返回false（因为不是部门名称行）
                        VisitPost = pair.Post,
                        VisitingMembers = pair.Person,
                        SourceDetails = matchingDetails
                    });
                }
            }
        }

        return displayItems;
    }
}

