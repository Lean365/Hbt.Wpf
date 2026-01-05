// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Generator
// 文件名称：CodeGenerator.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成视图
// 
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险.
// ========================================

using System;
using System.Windows.Controls;
using Takt.Fluent.ViewModels.Generator;

namespace Takt.Fluent.Views.Generator;

public partial class CodeGeneratorView : UserControl
{
    public CodeGeneratorViewModel ViewModel { get; }

    public CodeGeneratorView(CodeGeneratorViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
    }
}

