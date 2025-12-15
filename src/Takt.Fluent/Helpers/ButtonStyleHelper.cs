//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : ButtonStyleHelper.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-11-04
// 版本号 : 0.0.1
// 描述    : 按钮样式映射辅助类，根据按钮操作类型自动选择合适的样式
//===================================================================

using System;
using System.Collections.Generic;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 按钮样式映射辅助类
/// 根据44个通用按钮的操作类型自动选择合适的样式
/// </summary>
public static class ButtonStyleHelper
{
    /// <summary>
    /// 按钮样式名称常量（按功能分类）
    /// </summary>
    public static class Styles
    {
        // 创建类：创建新内容
        public const string Create = "CreateButtonStyle";
        
        // 提交确认类：提交和确认操作
        public const string Submit = "SubmitButtonStyle";
        
        // 发布通知类：发布和发送
        public const string Publish = "PublishButtonStyle";
        
        // 查看查询类：查看信息
        public const string View = "ViewButtonStyle";
        
        // 更新类：更新编辑
        public const string Update = "UpdateButtonStyle";
        
        // 删除拒绝类：删除和拒绝操作
        public const string Delete = "DeleteButtonStyle";
        
        // 启动启用类：启动和启用
        public const string Start = "StartButtonStyle";
        
        // 停止禁用类：停止和禁用
        public const string Stop = "StopButtonStyle";
        
        // 导入导出类：数据传输
        public const string Transfer = "TransferButtonStyle";
        
        // 打印类：打印
        public const string Print = "PrintButtonStyle";
        
        // 控制类：控制状态
        public const string Control = "ControlButtonStyle";
        
        // 复制类：复制和归档
        public const string Copy = "CopyButtonStyle";
        
        // 重置类：重置
        public const string Reset = "ResetButtonStyle";
        
        // 还原类：还原
        public const string Restore = "RestoreButtonStyle";
        
        // 社交互动类：社交功能
        public const string Social = "SocialButtonStyle";
    }

    /// <summary>
    /// 按钮代码到样式名称的映射字典（按功能分类）
    /// </summary>
    private static readonly Dictionary<string, string> ButtonStyleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // ==================== 创建类 ====================
        { "create", Styles.Create },        // 新增
        { "clone", Styles.Create },         // 克隆
        { "login", Styles.Create },         // 登录（主要入口操作，使用主题色）
        
        // ==================== 提交确认类 ====================
        { "submit", Styles.Submit },       // 提交
        { "approve", Styles.Submit },       // 通过
        { "grant", Styles.Submit },         // 授予
        { "authorize", Styles.Submit },     // 授权
        
        // ==================== 发布通知类 ====================
        { "publish", Styles.Publish },      // 发布
        { "send", Styles.Publish },         // 发送
        { "notify", Styles.Publish },       // 通知
        
        // ==================== 查看查询类 ====================
        { "read", Styles.View },           // 查看
        { "detail", Styles.View },          // 详情
        { "query", Styles.View },           // 查询
        { "preview", Styles.View },         // 预览
        
        // ==================== 更新类 ====================
        { "update", Styles.Update },          // 更新
        
        // ==================== 删除拒绝类 ====================
        { "delete", Styles.Delete },        // 删除
        { "reject", Styles.Delete },        // 驳回
        { "revoke", Styles.Delete },         // 收回
        { "recall", Styles.Delete },         // 撤回
        
        // ==================== 启动启用类 ====================
        { "start", Styles.Start },          // 启动
        { "run", Styles.Start },            // 运行
        { "enable", Styles.Start },          // 启用
        { "unlock", Styles.Start },         // 解锁
        
        // ==================== 停止禁用类 ====================
        { "stop", Styles.Stop },            // 停止
        { "disable", Styles.Stop },         // 禁用
        { "lock", Styles.Stop },            // 锁定
        
        // ==================== 导入导出类 ====================
        { "export", Styles.Transfer },      // 导出
        { "import", Styles.Transfer },       // 导入
        { "download", Styles.Transfer },    // 下载
        { "upload", Styles.Transfer },       // 上传
        { "attach", Styles.Transfer },       // 附件
        
        // ==================== 打印类 ====================
        { "print", Styles.Print },          // 打印
        
        // ==================== 控制类 ====================
        { "pause", Styles.Control },        // 暂停
        { "resume", Styles.Control },       // 恢复
        { "restart", Styles.Control },      // 重启
        
        // ==================== 复制类 ====================
        { "copy", Styles.Copy },            // 复制
        { "archive", Styles.Copy },         // 归档
        
        // ==================== 重置类 ====================
        { "reset", Styles.Reset },          // 重置
        
        // ==================== 还原类 ====================
        { "restore", Styles.Restore },      // 还原
        
        // ==================== 社交互动类 ====================
        { "refresh", Styles.Social },       // 刷新
        { "favorite", Styles.Social },      // 收藏
        { "like", Styles.Social },          // 点赞
        { "comment", Styles.Social },       // 评论
        { "share", Styles.Social },         // 分享
        { "subscribe", Styles.Social }       // 订阅
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
            return Styles.View; // 默认返回查看样式
        }

        // 尝试从映射字典中获取
        if (ButtonStyleMap.TryGetValue(buttonCode, out var styleName))
        {
            return styleName;
        }

        // 如果未找到，返回默认样式
        return Styles.View;
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

