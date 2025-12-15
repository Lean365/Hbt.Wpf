// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Routine
// 文件名称：LocalizationView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：本地化管理视图（主子表容器）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Takt.Application.Dtos.Routine;
using Takt.Common.Logging;
using Takt.Fluent.Controls;
using Takt.Fluent.ViewModels.Routine;

namespace Takt.Fluent.Views.Routine;

/// <summary>
/// 本地化管理视图（转置表格）
/// </summary>
public partial class LocalizationView : UserControl
{
    public LocalizationViewModel ViewModel { get; }

    public LocalizationView(LocalizationViewModel viewModel)
    {
        var operLog = App.Services?.GetService<OperLogManager>();
        operLog?.Debug("[LocalizationView] 构造函数开始执行");
        
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        
        InitializeComponent();
        operLog?.Debug("[LocalizationView] InitializeComponent 完成，DataContext 已设置");
    }

    /// <summary>
    /// 翻译值文本框失去焦点事件
    /// 当用户编辑翻译值后，保存到数据库
    /// </summary>
    private void TranslationValueTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is TranslationTransposedDto item && ViewModel != null)
        {
            // 通过 DataGridCell 获取当前列
            var cell = FindAncestor<DataGridCell>(textBox);
            if (cell != null && cell.Column is DataGridTemplateColumn column && column.Header?.ToString() is string header)
            {
                var languageCode = ExtractLanguageCode(header);
                if (!string.IsNullOrEmpty(languageCode))
                {
                    // 获取最新的值
                    var newValue = textBox.Text ?? string.Empty;
                    var oldValue = item.GetTranslationValue(languageCode);
                    
                    // 如果值发生变化，更新并保存
                    if (newValue != oldValue)
                    {
                        item.SetTranslationValue(languageCode, newValue);
                        
                        // 获取翻译ID
                        var translationId = item.TranslationIds.TryGetValue(languageCode, out var id) ? id : (long?)null;
                        
                        // 异步保存
                        _ = SaveTranslationValueAsync(item.TranslationKey, languageCode, newValue, translationId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 保存翻译值到数据库
    /// </summary>
    private async System.Threading.Tasks.Task SaveTranslationValueAsync(string translationKey, string languageCode, string translationValue, long? translationId)
    {
        if (ViewModel == null)
        {
            return;
        }

        try
        {
            var success = await ViewModel.SaveTranslationValueAsync(translationKey, languageCode, translationValue, translationId);
            // ID 已在 ViewModel 中更新，不需要额外操作
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<OperLogManager>();
            operLog?.Error(ex, "[LocalizationView] 保存翻译值失败");
        }
    }

    /// <summary>
    /// 在视觉树中向上查找指定类型的父元素
    /// </summary>
    private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T t)
            {
                return t;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    /// <summary>
    /// 从列头提取语言代码
    /// 格式：语言名称 (语言代码)
    /// </summary>
    private string? ExtractLanguageCode(string header)
    {
        var startIndex = header.IndexOf('(');
        var endIndex = header.IndexOf(')');
        if (startIndex >= 0 && endIndex > startIndex)
        {
            return header.Substring(startIndex + 1, endIndex - startIndex - 1);
        }
        return null;
    }
}

