//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : ButtonStyleHelper.cs
// 创建者 : AI Assistant
// 创建时间: 2025-11-04
// 版本号 : 1.0
// 描述    : 按钮样式映射辅助类，根据按钮操作类型自动选择合适的样式
//===================================================================

using System;
using System.Collections.Generic;

namespace Hbt.Fluent.Helpers;

/// <summary>
/// 按钮样式映射辅助类
/// 根据44个通用按钮的操作类型自动选择合适的样式
/// </summary>
public static class ButtonStyleHelper
{
    /// <summary>
    /// 按钮样式名称常量
    /// </summary>
    public static class Styles
    {
        public const string Primary = "PrimaryButtonStyle";
        public const string Secondary = "SecondaryButtonStyle";
        public const string Danger = "DangerButtonStyle";
        public const string Text = "TextButtonStyle";
        public const string Icon = "IconButtonStyle";
    }

    /// <summary>
    /// 按钮代码到样式名称的映射字典
    /// </summary>
    private static readonly Dictionary<string, string> ButtonStyleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // ==================== Primary（主要操作） ====================
        // 创建、提交、通过、启动等正向操作
        { "create", Styles.Primary },      // 新增
        { "submit", Styles.Primary },      // 提交
        { "approve", Styles.Primary },     // 通过
        { "start", Styles.Primary },        // 启动
        { "run", Styles.Primary },         // 运行
        { "enable", Styles.Primary },       // 启用
        { "unlock", Styles.Primary },       // 解锁
        { "grant", Styles.Primary },        // 授予
        { "authorize", Styles.Primary },    // 授权
        { "publish", Styles.Primary },     // 发布
        { "send", Styles.Primary },         // 发送
        { "notify", Styles.Primary },       // 通知
        { "restore", Styles.Primary },      // 还原
        
        // ==================== Danger（危险操作） ====================
        // 删除、拒绝、停止、禁用等破坏性操作
        { "delete", Styles.Danger },        // 删除
        { "reject", Styles.Danger },        // 驳回
        { "stop", Styles.Danger },          // 停止
        { "disable", Styles.Danger },       // 禁用
        { "lock", Styles.Danger },          // 锁定
        { "revoke", Styles.Danger },        // 收回
        { "recall", Styles.Danger },        // 撤回
        
        // ==================== Secondary（次要操作） ====================
        // 更新、编辑、导入、导出、查看等常规操作
        { "update", Styles.Secondary },    // 更新
        { "read", Styles.Secondary },       // 查看
        { "detail", Styles.Secondary },     // 详情
        { "query", Styles.Secondary },      // 查询
        { "export", Styles.Secondary },     // 导出
        { "import", Styles.Secondary },    // 导入
        { "print", Styles.Secondary },      // 打印
        { "preview", Styles.Secondary },    // 预览
        { "download", Styles.Secondary },   // 下载
        { "upload", Styles.Secondary },     // 上传
        { "attach", Styles.Secondary },     // 附件
        { "pause", Styles.Secondary },      // 暂停
        { "resume", Styles.Secondary },     // 恢复
        { "restart", Styles.Secondary },    // 重启
        { "copy", Styles.Secondary },       // 复制
        { "clone", Styles.Secondary },      // 克隆
        { "archive", Styles.Secondary },    // 归档
        
        // ==================== Text（文本按钮） ====================
        // 次要操作，通常用于分页、重置等
        { "reset", Styles.Text },          // 重置
        
        // ==================== Icon（图标按钮） ====================
        // 刷新、收藏、点赞等辅助功能
        { "refresh", Styles.Icon },         // 刷新
        { "favorite", Styles.Icon },        // 收藏
        { "like", Styles.Icon },            // 点赞
        { "comment", Styles.Icon },         // 评论
        { "share", Styles.Icon },           // 分享
        { "subscribe", Styles.Icon }        // 订阅
    };

    /// <summary>
    /// 根据按钮代码获取样式名称
    /// </summary>
    /// <param name="buttonCode">按钮代码（如 "create", "delete"）</param>
    /// <returns>样式名称（如 "PrimaryButtonStyle"），如果未找到则返回 SecondaryButtonStyle</returns>
    public static string GetStyleName(string? buttonCode)
    {
        if (string.IsNullOrWhiteSpace(buttonCode))
        {
            return Styles.Secondary; // 默认返回次要样式
        }

        // 尝试从映射字典中获取
        if (ButtonStyleMap.TryGetValue(buttonCode, out var styleName))
        {
            return styleName;
        }

        // 如果未找到，返回默认样式
        return Styles.Secondary;
    }

    /// <summary>
    /// 根据按钮代码获取样式资源键（用于 XAML 绑定）
    /// </summary>
    /// <param name="buttonCode">按钮代码</param>
    /// <returns>样式资源键（如 "PrimaryButtonStyle"）</returns>
    public static string GetStyleResourceKey(string? buttonCode)
    {
        return GetStyleName(buttonCode);
    }

    /// <summary>
    /// 检查按钮代码是否已映射
    /// </summary>
    /// <param name="buttonCode">按钮代码</param>
    /// <returns>如果已映射返回 true，否则返回 false</returns>
    public static bool IsMapped(string? buttonCode)
    {
        if (string.IsNullOrWhiteSpace(buttonCode))
        {
            return false;
        }

        return ButtonStyleMap.ContainsKey(buttonCode);
    }

    /// <summary>
    /// 获取所有已映射的按钮代码
    /// </summary>
    /// <returns>按钮代码集合</returns>
    public static IEnumerable<string> GetMappedButtonCodes()
    {
        return ButtonStyleMap.Keys;
    }
}

