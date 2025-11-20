// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Routine
// 文件名称：TranslationKeyInfoDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：翻译键信息数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 翻译键信息数据传输对象
/// 用于主表显示翻译键列表
/// </summary>
public class TranslationKeyInfoDto
{
    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; } = string.Empty;

    /// <summary>
    /// 模块
    /// </summary>
    public string? Module { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

