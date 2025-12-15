// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views
// 文件名称：InitializationLogWindow.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：初始化日志窗口
// 
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险.
// ========================================

using System.Windows;
using System.Windows.Controls;

namespace Takt.Fluent.Views;

/// <summary>
/// 初始化日志窗口
/// </summary>
public partial class InitializationLogWindow : Window
{
    public InitializationLogWindow()
    {
        InitializeComponent();
    }

    private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // 设置光标到文本末尾
            textBox.CaretIndex = textBox.Text.Length;
            // 滚动到光标位置（即文本末尾）
            textBox.ScrollToEnd();
        }
    }
}

