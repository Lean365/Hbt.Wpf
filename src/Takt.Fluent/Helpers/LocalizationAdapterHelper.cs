// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：LocalizationAdapterHelper.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：LocalizationNotifyProperty 辅助类，用于 XAML 绑定
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Fluent.Services;

namespace Takt.Fluent.Helpers;

/// <summary>
/// LocalizationNotifyProperty 辅助类，用于在 XAML 中绑定
/// </summary>
public static class LocalizationAdapterHelper
{
    /// <summary>
    /// 获取 LocalizationNotifyProperty 实例（用于 XAML 绑定）
    /// </summary>
    public static LocalizationNotifyProperty? Adapter
    {
        get
        {
            return App.Services?.GetService(typeof(LocalizationNotifyProperty)) as LocalizationNotifyProperty;
        }
    }
}

