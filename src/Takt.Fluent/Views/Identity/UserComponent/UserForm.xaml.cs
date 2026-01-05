// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Identity
// 文件名称：UserForm.xaml.cs
// 创建时间：2025-11-13
// 创建人：Takt365(Cursor AI)
// 功能描述：用户表单窗口（新建/编辑用户）
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.Views.Identity.UserComponent;

/// <summary>
/// 用户表单窗口（新建/编辑用户）
/// </summary>
/// <remarks>
/// 提供用户信息的创建和编辑功能，包含三个 TabItem：
/// 1. 基本信息：用户名、真实姓名、邮箱、手机号、头像、密码等
/// 2. 状态信息：用户类型、性别、状态
/// 3. 备注信息：备注内容
/// 窗口高度根据内容动态计算，确保最佳显示效果
/// </remarks>
public partial class UserForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private Takt.Fluent.ViewModels.Identity.UserFormViewModel? _viewModel;

    /// <summary>
    /// 初始化用户表单窗口
    /// </summary>
    /// <param name="vm">用户表单视图模型</param>
    /// <param name="languageService">语言服务</param>
    public UserForm(Takt.Fluent.ViewModels.Identity.UserFormViewModel vm, ILocalizationManager? localizationManager = null)
    {
        InitializeComponent();
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _viewModel = vm;
        DataContext = vm;
        
        // 设置 Owner（如果还没有设置）
        if (Owner == null)
        {
            Owner = System.Windows.Application.Current.MainWindow;
        }
        
        // ILocalizationManager 初始化在应用启动时完成，无需在此初始化
        Loaded += (s, e) =>
        {
            
            // 延迟执行，确保UI已完全渲染
            _ = Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // 访问文本字段控件，直接从控件读取值（类似密码的方式）
                if (UsernameTextBox != null && RealNameTextBox != null && NicknameTextBox != null && EmailTextBox != null && PhoneTextBox != null)
                {
                    // 订阅所有文本框变更事件，进行实时验证（统一使用事件处理方式）
                    UsernameTextBox.TextChanged += (s, e) =>
                    {
                        if (s is TextBox textBox)
                        {
                            vm.ValidateUsername(textBox.Text ?? string.Empty);
                        }
                    };
                    
                    RealNameTextBox.TextChanged += (s, e) =>
                    {
                        if (s is TextBox textBox)
                        {
                            vm.ValidateRealName(textBox.Text ?? string.Empty);
                        }
                    };
                    
                    NicknameTextBox.TextChanged += (s, e) =>
                    {
                        if (s is TextBox textBox)
                        {
                            vm.ValidateNickname(textBox.Text ?? string.Empty);
                        }
                    };
                    
                    EmailTextBox.TextChanged += (s, e) =>
                    {
                        if (s is TextBox textBox)
                        {
                            vm.ValidateEmail(textBox.Text ?? string.Empty);
                        }
                    };
                    
                    PhoneTextBox.TextChanged += (s, e) =>
                    {
                        if (s is TextBox textBox)
                        {
                            vm.ValidatePhone(textBox.Text ?? string.Empty);
                        }
                    };
                    
                    // 查找备注控件
                    var remarksTextBox = FindName("RemarksTextBox") as TextBox;
                    
                    // 备注字段不需要验证，但需要读取值
                    // 注意：AvatarTextBox 现在通过绑定获取值，不需要从 TextBox 读取
                    vm.AttachTextFieldsAccess(() => (
                        UsernameTextBox.Text ?? string.Empty,
                        RealNameTextBox.Text ?? string.Empty,
                        NicknameTextBox.Text ?? string.Empty,
                        EmailTextBox.Text ?? string.Empty,
                        PhoneTextBox.Text ?? string.Empty,
                        vm.Avatar ?? string.Empty, // 从 ViewModel 获取，因为 AvatarTextBox 是只读的
                        remarksTextBox?.Text ?? string.Empty
                    ));
                }
                
                // 访问密码框，确保它们已初始化（仅在创建模式下）
                if (vm.IsCreate && Pwd != null && Pwd2 != null)
                {
                    vm.AttachPasswordAccess(() => (Pwd.Password, Pwd2.Password));
                    
                    // 订阅密码框变更事件，进行实时验证
                    Pwd.PasswordChanged += (s, e) =>
                    {
                        vm.ValidatePassword(Pwd.Password, Pwd2?.Password ?? string.Empty);
                    };
                    
                    Pwd2.PasswordChanged += (s, e) =>
                    {
                        vm.ValidatePasswordConfirm(Pwd?.Password ?? string.Empty, Pwd2.Password);
                    };
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);

            // 延迟计算高度，确保 IsCreate 属性已正确设置
            _ = Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // 计算并设置最佳窗口高度
                CalculateAndSetOptimalHeight();

                // 居中窗口
                CenterWindow();
                
                if (Owner != null)
                {
                    Owner.SizeChanged += Owner_SizeChanged;
                    Owner.LocationChanged += Owner_LocationChanged;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        };

        Unloaded += (_, _) =>
        {
            if (Owner != null)
            {
                Owner.SizeChanged -= Owner_SizeChanged;
                Owner.LocationChanged -= Owner_LocationChanged;
            }
        };
        
        // 订阅语言变化事件（通过 ILocalizationManager）
        if (_localizationManager != null)
        {
            _localizationManager.LanguageChanged += (sender, langCode) =>
            {
                // ViewModel 会处理标题更新，这里不需要额外处理
            };
        }

        // 订阅 IsCreate 变化，重新计算高度
        vm.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(vm.IsCreate))
            {
                // 延迟计算，确保 UI 已更新
                Dispatcher.BeginInvoke(new System.Action(() => CalculateAndSetOptimalHeight()), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        };
    }

    /// <summary>
    /// 计算并设置最佳窗口高度
    /// </summary>
    /// <remarks>
    /// 统计各 TabItem 的字段数量，计算各 TabItem 的实际高度，取最大值
    /// 确保所有字段都能显示
    /// </remarks>
    private void CalculateAndSetOptimalHeight()
    {
        if (_viewModel == null) return;

        // 计算各 TabItem 的实际高度
        double basicInfoHeight = CalculateBasicInfoHeight();
        double statusInfoHeight = CalculateStatusInfoHeight();
        double remarksInfoHeight = CalculateRemarksInfoHeight();

        // 取最大高度作为 TabControl 内容高度
        double maxTabContentHeight = Math.Max(Math.Max(basicInfoHeight, statusInfoHeight), remarksInfoHeight);

        // 窗口总高度 = TabControl 内容高度 + TabControl 头部 + 按钮区域 + 各种 Margin
        const double tabControlHeaderHeight = 52; // TabControl 头部高度（TabItem 标签区域，包含边框和内边距）
        const double buttonAreaHeight = 52; // 按钮区域高度（按钮高度约 36px + 按钮间距 + StackPanel 内边距）
        const double windowMargin = 48; // 窗口 Margin="24"（上下各24，共48）
        const double tabControlMargin = 16; // TabControl 底部 Margin="0,0,0,16"
        const double buttonMargin = 20; // 按钮区域顶部 Margin="0,20,0,0"

        double optimalHeight = maxTabContentHeight + tabControlHeaderHeight + buttonAreaHeight + windowMargin + tabControlMargin + buttonMargin;

        // 添加额外的缓冲空间，确保所有内容都能完整显示（考虑实际渲染、边框、内边距、标题栏等）
        const double extraBuffer = 48;

        // 设置最小和最大高度限制，确保所有内容都能显示
        const double minHeight = 500;
        const double maxHeight = 1000;
        Height = Math.Max(minHeight, Math.Min(maxHeight, optimalHeight + extraBuffer));
    }

    /// <summary>
    /// 计算基本信息 TabItem 的高度
    /// </summary>
    /// <returns>基本信息 TabItem 的内容高度（像素）</returns>
    private double CalculateBasicInfoHeight()
    {
        int fieldCount = GetBasicInfoFieldCount();
        return CalculateHeightByFieldCount(fieldCount);
    }

    /// <summary>
    /// 计算状态信息 TabItem 的高度
    /// </summary>
    /// <returns>状态信息 TabItem 的内容高度（像素）</returns>
    private double CalculateStatusInfoHeight()
    {
        int fieldCount = GetStatusInfoFieldCount();
        return CalculateHeightByFieldCount(fieldCount);
    }

    /// <summary>
    /// 计算备注信息 TabItem 的高度
    /// </summary>
    /// <returns>备注信息 TabItem 的内容高度（像素）</returns>
    /// <remarks>
    /// 备注信息 TabItem 是特殊字段，MinHeight=200，不能按普通字段计算
    /// StackPanel（Margin="16"）包含 Grid
    /// Grid 包含：
    ///   - 标签 TextBlock（左列，VerticalAlignment="Top"，不影响 Grid 高度）
    ///   - StackPanel（右列）包含：
    ///     - TaktTextBox（MinHeight="200"）
    ///     - 错误 TextBlock（MinHeight=20, Margin="112,4,0,0"）
    /// Grid 高度由右列 StackPanel 决定：200 + 24 = 224px
    /// 外层 StackPanel 高度 = Grid 高度 + Margin（32）= 256px
    /// </remarks>
    private double CalculateRemarksInfoHeight()
    {
        const double remarksTextBoxHeight = 200; // 备注文本框的 MinHeight
        const double errorTextHeight = 24; // 错误文本区域（MinHeight=20 + Margin Top=4）
        const double rightColumnStackPanelHeight = remarksTextBoxHeight + errorTextHeight; // 右列 StackPanel 高度
        const double gridHeight = rightColumnStackPanelHeight; // Grid 高度由右列决定
        const double stackPanelMargin = 32; // 外层 StackPanel Margin="16"（上下各16，共32）

        // 添加一些缓冲空间，确保所有内容都能完整显示
        const double buffer = 8;

        return gridHeight + stackPanelMargin + buffer;
    }

    /// <summary>
    /// 统计基本信息 TabItem 的字段数量
    /// </summary>
    /// <returns>字段数量</returns>
    /// <remarks>
    /// 固定字段：用户名、真实姓名、邮箱、手机号、头像 = 5个
    /// 密码字段（始终计算高度，无论是否显示）：密码、确认密码 = 2个
    /// 总字段数：5 + 2 = 7个字段（始终计算，确保高度足够）
    /// </remarks>
    private int GetBasicInfoFieldCount()
    {
        // 始终返回 7 个字段，包括密码和确认密码（即使不显示也要计算高度）
        const int totalFieldCount = 7; // 用户名、真实姓名、邮箱、手机号、头像、密码、确认密码

        return totalFieldCount;
    }

    /// <summary>
    /// 统计状态信息 TabItem 的字段数量
    /// </summary>
    /// <returns>字段数量</returns>
    /// <remarks>
    /// 字段：用户类型、性别、状态 = 3个
    /// </remarks>
    private int GetStatusInfoFieldCount()
    {
        return 3; // 用户类型、性别、状态
    }

    /// <summary>
    /// 统计备注信息 TabItem 的字段数量
    /// </summary>
    /// <returns>字段数量</returns>
    /// <remarks>
    /// 字段：备注 = 1个（特殊字段，MinHeight=200）
    /// </remarks>
    private int GetRemarksInfoFieldCount()
    {
        return 1; // 备注
    }

    /// <summary>
    /// 根据字段数量计算 TabItem 内容高度
    /// </summary>
    /// <param name="fieldCount">字段数量</param>
    /// <returns>TabItem 内容高度（像素）</returns>
    /// <remarks>
    /// 使用统一的字段高度和间距计算逻辑
    /// 每个字段 StackPanel：MinHeight=56（Grid 32px + 错误文本区域 24px）
    /// 字段间距：每个字段 StackPanel 的 Margin="0,0,0,8"
    /// StackPanel Margin="16"（上下各16，共32）
    /// </remarks>
    private double CalculateHeightByFieldCount(int fieldCount)
    {
        const double fieldHeight = 56; // 每个字段 StackPanel 的 MinHeight（Grid 32 + 错误文本 24）
        const double fieldSpacing = 8; // 字段之间的间距（每个字段 StackPanel 的 Margin="0,0,0,8"）
        const double stackPanelMargin = 32; // StackPanel Margin="16"（上下各16，共32）

        if (fieldCount <= 0)
        {
            return stackPanelMargin; // 至少返回 StackPanel 的 Margin
        }

        // 字段高度总和
        double fieldsHeight = fieldCount * fieldHeight;

        // 字段间距总和（n个字段有 n-1 个间距）
        double fieldsSpacing = (fieldCount - 1) * fieldSpacing;

        // 添加更多的缓冲空间，确保所有字段都能完整显示（考虑实际渲染可能需要的额外空间）
        const double buffer = 24;

        return fieldsHeight + fieldsSpacing + stackPanelMargin + buffer;
    }


    /// <summary>
    /// 居中窗口到父窗口或屏幕
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            // 相对于父窗口居中
            // 窗口宽度限制：最小为父窗口的40%，最大为父窗口的60%
            var minWidth = Owner.ActualWidth * 0.4;
            var maxWidth = Owner.ActualWidth * 0.6;
            // 使用50%作为默认宽度，但限制在40%-60%之间
            var calculatedWidth = Owner.ActualWidth * 0.5;
            Width = Math.Max(minWidth, Math.Min(calculatedWidth, maxWidth));
            
            // 窗口高度设置为父窗口的95%
            Height = Owner.ActualHeight * 0.95;
            
            // 计算居中位置
            Left = Owner.Left + (Owner.ActualWidth - Width) / 2;
            Top = Owner.Top + (Owner.ActualHeight - Height) / 2;
        }
        else
        {
            // 相对于屏幕居中
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            
            // 设置默认大小
            if (Width == 0 || double.IsNaN(Width))
            {
                Width = Math.Min(800, screenWidth * 0.6);
            }
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Math.Min(600, screenHeight * 0.7);
            }
            
            // 居中到屏幕
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }

    /// <summary>
    /// 父窗口大小变化事件处理
    /// </summary>
    private void Owner_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        CenterWindow();
    }

    /// <summary>
    /// 父窗口位置变化事件处理
    /// </summary>
    private void Owner_LocationChanged(object? sender, EventArgs e)
    {
        CenterWindow();
    }

    /// <summary>
    /// 计算最佳高度（不包含父窗口限制）
    /// </summary>
    /// <returns>计算出的最佳窗口高度（像素）</returns>
    /// <remarks>
    /// 计算逻辑与 CalculateAndSetOptimalHeight 相同，但不设置窗口高度
    /// 用于在父窗口大小变化时获取计算值，然后与父窗口限制进行比较
    /// </remarks>
    private double CalculateOptimalHeight()
    {
        if (_viewModel == null) return 600;

        // 计算各 TabItem 的实际高度
        double basicInfoHeight = CalculateBasicInfoHeight();
        double statusInfoHeight = CalculateStatusInfoHeight();
        double remarksInfoHeight = CalculateRemarksInfoHeight();

        // 取最大高度作为 TabControl 内容高度
        double maxTabContentHeight = Math.Max(Math.Max(basicInfoHeight, statusInfoHeight), remarksInfoHeight);

        const double tabControlHeaderHeight = 52;
        const double buttonAreaHeight = 52;
        const double windowMargin = 48;
        const double tabControlMargin = 16;
        const double buttonMargin = 20;

        double optimalHeight = maxTabContentHeight + tabControlHeaderHeight + buttonAreaHeight + windowMargin + tabControlMargin + buttonMargin;

        // 添加额外的缓冲空间，确保所有内容都能完整显示（考虑实际渲染、边框、内边距、标题栏等）
        const double extraBuffer = 48;

        const double minHeight = 500;
        const double maxHeight = 1000;
        return Math.Max(minHeight, Math.Min(maxHeight, optimalHeight + extraBuffer));
    }
}


