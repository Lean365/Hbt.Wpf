// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Helpers
// 文件名称：DictionaryValueConverter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：字典值转换器（用于从字典中获取值）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Globalization;
using System.Windows.Data;
using Takt.Application.Dtos.Routine;

namespace Takt.Fluent.Helpers;

/// <summary>
/// 字典值转换器
/// 用于从 TranslationTransposedDto 的 TranslationValues 字典中获取指定语言的翻译值
/// </summary>
public class DictionaryValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // value 应该是 DataGridRow 的 DataContext（即 TranslationTransposedDto）
        if (value is TranslationTransposedDto transposed && parameter is string languageCode)
        {
            return transposed.GetTranslationValue(languageCode);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // ConvertBack 不会直接使用，因为我们使用 LostFocus 事件来处理更新
        // 但为了支持双向绑定，我们可以返回 Binding.DoNothing
        return Binding.DoNothing;
    }
}

