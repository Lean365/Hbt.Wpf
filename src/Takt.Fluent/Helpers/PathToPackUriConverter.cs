// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：PathToPackUriConverter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：将相对路径转换为 Pack URI 的值转换器
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 将相对路径转换为 Pack URI 的值转换器
/// 例如：将 "assets/avatar.png" 转换为 "pack://application:,,,/Assets/avatar.png"
/// </summary>
public class PathToPackUriConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
        {
            // 如果路径为空，返回默认头像
            return new Uri("pack://application:,,,/Assets/avatar.png", UriKind.Absolute);
        }

        // 将相对路径转换为 Pack URI
        // 例如：assets/avatar.png -> pack://application:,,,/Assets/avatar.png
        // 注意：路径中的首字母需要大写（Assets 而不是 assets）
        var normalizedPath = path.Replace('\\', '/');
        if (!normalizedPath.StartsWith("/"))
        {
            normalizedPath = "/" + normalizedPath;
        }

        // 将路径首字母大写（Assets 而不是 assets）
        var parts = normalizedPath.Split('/');
        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
        {
            parts[1] = char.ToUpperInvariant(parts[1][0]) + (parts[1].Length > 1 ? parts[1].Substring(1) : string.Empty);
            normalizedPath = string.Join("/", parts);
        }

        try
        {
            return new Uri($"pack://application:,,,{normalizedPath}", UriKind.Absolute);
        }
        catch
        {
            // 如果转换失败，返回默认头像
            return new Uri("pack://application:,,,/Assets/avatar.png", UriKind.Absolute);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

