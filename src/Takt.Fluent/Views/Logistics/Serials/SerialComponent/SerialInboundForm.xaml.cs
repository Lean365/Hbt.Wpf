// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Logistics.Serials.SerialComponent
// 文件名称：SerialInboundForm.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：序列号入库表单窗口（扫描入库）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;
using Takt.Fluent.ViewModels.Logistics.Serials;

namespace Takt.Fluent.Views.Logistics.Serials.SerialComponent;

/// <summary>
/// 序列号入库表单窗口（扫描入库）
/// </summary>
public partial class SerialInboundForm : Window
{
    private readonly ILocalizationManager? _localizationManager;
    private SerialInboundFormViewModel? _viewModel;

    /// <summary>
    /// 初始化序列号入库表单窗口
    /// </summary>
    /// <param name="vm">序列号入库表单视图模型</param>
    /// <param name="localizationManager">本地化管理器</param>
    public SerialInboundForm(SerialInboundFormViewModel vm, ILocalizationManager? localizationManager = null)
    {
        InitializeComponent();
        _localizationManager = localizationManager ?? App.Services?.GetService<ILocalizationManager>();
        _viewModel = vm;
        DataContext = vm;

        // 设置 Owner（如果还没有设置）
        if (Owner == null)
        {
            Owner = System.Windows.Application.Current?.MainWindow;
        }

        Loaded += (s, e) =>
        {
            // 延迟计算高度，确保 UI 已完全渲染
            _ = Dispatcher.BeginInvoke(new Action(() =>
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

                // 聚焦到 FullSerialNumber 输入框，方便扫描
                FocusFullSerialNumberInput();
                
                // 为输入框添加事件处理，确保扫描枪扫描后也能自动保存
                var textBox = this.FindName("FullSerialNumberTextBox") as System.Windows.Controls.TextBox;
                if (textBox != null)
                {
                    // TextInput 事件：捕获文本输入（推荐，更适合扫描枪输入）
                    textBox.TextInput += FullSerialNumberTextBox_TextInput;
                    // PreviewKeyDown 事件：捕获 Enter 键触发保存
                    textBox.PreviewKeyDown += FullSerialNumberTextBox_PreviewKeyDown;
                }
                
                // 监听 FullSerialNumber 属性变化，当它被清空时重新聚焦
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(SerialInboundFormViewModel.FullSerialNumber))
                        {
                            if (string.IsNullOrWhiteSpace(_viewModel.FullSerialNumber))
                            {
                                // 延迟聚焦，确保 UI 已更新
                                _ = Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    FocusFullSerialNumberInput();
                                }), System.Windows.Threading.DispatcherPriority.Input);
                            }
                        }
                    };
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
    }

    /// <summary>
    /// FullSerialNumber 输入框 TextInput 事件处理（推荐）
    /// 捕获文本输入，验证字母必须为大写，长度必须大于0且小于等于19位
    /// </summary>
    private void FullSerialNumberTextBox_TextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            // 获取输入的文本
            string inputText = e.Text;
            
            // 计算输入后的总长度
            int selectionStart = textBox.SelectionStart;
            int selectionLength = textBox.SelectionLength;
            string currentText = textBox.Text ?? string.Empty;
            string newText = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, inputText);
            int newLength = newText.Length;
            
            // 验证长度：必须大于0且小于等于19位
            if (newLength > 19)
            {
                // 超过19位，阻止输入
                e.Handled = true;
                return;
            }
            
            // 检查输入中是否包含小写字母
            bool hasLowercase = false;
            foreach (char c in inputText)
            {
                if (char.IsLetter(c) && char.IsLower(c))
                {
                    hasLowercase = true;
                    break;
                }
            }
            
            // 如果包含小写字母，自动转换为大写
            if (hasLowercase)
            {
                // 阻止原始输入
                e.Handled = true;
                
                // 转换为大写并插入
                string upperText = inputText.ToUpperInvariant();
                string finalText = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, upperText);
                
                // 再次检查转换后的长度（虽然理论上不会变化，但为了安全）
                if (finalText.Length <= 19)
                {
                    textBox.Text = finalText;
                    textBox.SelectionStart = selectionStart + upperText.Length;
                    textBox.SelectionLength = 0;
                    
                    // 如果长度达到19位，检查是否重复
                    if (finalText.Length == 19 && _viewModel != null)
                    {
                        _ = CheckDuplicateAsync(finalText);
                    }
                }
                // 如果转换后超过19位，不插入（已在前面检查过，这里理论上不会执行）
            }
            else
            {
                // 如果输入长度达到19位，检查是否重复
                if (newLength == 19 && _viewModel != null)
                {
                    _ = CheckDuplicateAsync(newText);
                }
            }
        }
    }

    /// <summary>
    /// 异步检查序列号是否重复
    /// </summary>
    private async Task CheckDuplicateAsync(string fullSerialNumber)
    {
        if (_viewModel == null || string.IsNullOrWhiteSpace(fullSerialNumber))
        {
            return;
        }

        try
        {
            // 异步检查重复
            bool isDuplicate = await _viewModel.CheckDuplicateAsync(fullSerialNumber);
            
            // 回到 UI 线程更新错误提示并弹出消息框
            if (System.Windows.Application.Current != null)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (isDuplicate && _viewModel != null)
                    {
                        var errorMessage = "该序列号已存在，不能重复录入";
                        _viewModel.FullSerialNumberError = errorMessage;
                        
                        // 弹出消息框提示重复
                        TaktMessageBox.Warning(errorMessage, "重复提示", this);
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            // 检查重复失败不影响输入，只记录日志
            System.Diagnostics.Debug.WriteLine($"[SerialInboundForm] 检查序列号重复异常：{ex.Message}");
        }
    }

    /// <summary>
    /// FullSerialNumber 输入框 PreviewKeyDown 事件处理
    /// 确保回车键和扫描枪扫描后都能自动保存
    /// PreviewKeyDown 在 KeyBinding 之前触发，确保扫描枪也能正常工作
    /// </summary>
    private void FullSerialNumberTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _viewModel != null)
        {
            // 确保输入框的值已更新到 ViewModel（扫描枪快速输入时可能需要强制更新）
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                var bindingExpression = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                bindingExpression?.UpdateSource();
                
                // 如果输入框有值，执行保存命令
                if (!string.IsNullOrWhiteSpace(_viewModel.FullSerialNumber))
                {
                    // 执行保存命令（异步方法会自动处理）
                    if (_viewModel.SaveCommand.CanExecute(null))
                    {
                        _viewModel.SaveCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }
    }

    /// <summary>
    /// 聚焦到完整序列号输入框
    /// </summary>
    private void FocusFullSerialNumberInput()
    {
        // 查找 FullSerialNumber 输入框并聚焦
        var textBox = this.FindName("FullSerialNumberTextBox") as System.Windows.Controls.TextBox;
        if (textBox == null)
        {
            // 如果找不到命名控件，尝试通过遍历查找
            textBox = FindVisualChild<System.Windows.Controls.TextBox>(this);
        }
        textBox?.Focus();
        textBox?.SelectAll();
    }

    /// <summary>
    /// 查找可视化子元素
    /// </summary>
    private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                return t;
            }
            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }
        return null;
    }

    /// <summary>
    /// 计算并设置最佳窗口高度
    /// </summary>
    private void CalculateAndSetOptimalHeight()
    {
        if (_viewModel == null) return;

        const double fieldHeight = 80; // 字段高度
        const double stackPanelMargin = 32; // StackPanel Margin="16"（上下各16，共32）
        const double buttonAreaHeight = 52; // 按钮区域高度
        const double buttonMargin = 20; // 按钮区域顶部 Margin="0,20,0,0"
        const double windowMargin = 48; // 窗口 Margin="24"（上下各24，共48）
        const double buffer = 24;

        double contentHeight = fieldHeight + stackPanelMargin + buffer;
        double optimalHeight = contentHeight + buttonAreaHeight + buttonMargin + windowMargin;

        const double minHeight = 250;
        const double maxHeight = 400;
        Height = Math.Max(minHeight, Math.Min(maxHeight, optimalHeight));
    }

    /// <summary>
    /// 居中窗口到父窗口或屏幕
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            // 相对于父窗口居中，默认大小为父窗口的95%
            Width = Owner.ActualWidth * 0.95;
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

            if (Width == 0 || double.IsNaN(Width))
            {
                Width = Math.Min(500, screenWidth * 0.4);
            }
            if (Height == 0 || double.IsNaN(Height))
            {
                Height = Math.Min(300, screenHeight * 0.4);
            }

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
    private double CalculateOptimalHeight()
    {
        if (_viewModel == null) return 300;

        const double fieldHeight = 80;
        const double stackPanelMargin = 32;
        const double buttonAreaHeight = 52;
        const double buttonMargin = 20;
        const double windowMargin = 48;
        const double buffer = 24;

        double contentHeight = fieldHeight + stackPanelMargin + buffer;
        double optimalHeight = contentHeight + buttonAreaHeight + buttonMargin + windowMargin;

        const double minHeight = 250;
        const double maxHeight = 400;
        return Math.Max(minHeight, Math.Min(maxHeight, optimalHeight));
    }
}

