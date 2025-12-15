// ========================================
// 项目名称：Takt.Wpf
// 文件名称：AppConstants.cs
// 创建时间：2025-10-17
// 创建人：Hbt365(Cursor AI)
// 功能描述：应用程序常量
// 
// 版权信息：
// Copyright (c) 2025 Takt All rights reserved.
// 
// 开源许可：MIT License
// 
// 免责声明：
// 本软件是"按原样"提供的，没有任何形式的明示或暗示的保证，包括但不限于
// 对适销性、特定用途的适用性和不侵权的保证。在任何情况下，作者或版权持有人
// 都不对任何索赔、损害或其他责任负责，无论这些追责来自合同、侵权或其它行为中，
// 还是产生于、源于或有关于本软件以及本软件的使用或其它处置。
// ========================================

namespace Takt.Common.Constants;

/// <summary>
/// 应用程序常量
/// 定义全局使用的常量值
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// 默认文化
    /// </summary>
    public const string DefaultCulture = "zh-CN";
    
    /// <summary>
    /// 英文文化
    /// </summary>
    public const string EnglishCulture = "en-US";
    
    /// <summary>
    /// 主题常量
    /// </summary>
    public static class Theme
    {
        /// <summary>
        /// 明亮主题
        /// </summary>
        public const string Light = "Light";
        
        /// <summary>
        /// 暗黑主题
        /// </summary>
        public const string Dark = "Dark";
        
        /// <summary>
        /// 默认主题
        /// </summary>
        public const string Default = Light;
    }
    
    /// <summary>
    /// 日期格式常量
    /// </summary>
    public static class DateFormat
    {
        /// <summary>
        /// 标准日期时间格式
        /// </summary>
        public const string Standard = "yyyy-MM-dd HH:mm:ss";
        
        /// <summary>
        /// 短日期格式
        /// </summary>
        public const string ShortDate = "yyyy-MM-dd";
        
        /// <summary>
        /// 短时间格式
        /// </summary>
        public const string ShortTime = "HH:mm:ss";
    }
}
