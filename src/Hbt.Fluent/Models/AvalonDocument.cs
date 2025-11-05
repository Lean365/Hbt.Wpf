//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : AvalonDocument.cs
// 创建者 : AI Assistant
// 创建时间: 2025-11-03
// 版本号 : 1.0
// 描述    : AvalonDock LayoutDocument 包装类，用于 MDI 文档管理
//===================================================================

using AvalonDock.Layout;
using Hbt.Application.Dtos.Identity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using FontAwesome.Sharp;
using System;
using System.Windows;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Hbt.Fluent.Models;

/// <summary>
/// AvalonDock 文档包装类
/// 包装 DocumentTabItem 以适配 AvalonDock 的 LayoutDocument
/// </summary>
public class AvalonDocument : LayoutDocument
{
    
    /// <summary>
    /// 原始的 DocumentTabItem 数据
    /// </summary>
    public new DocumentTabItem TabItem { get; }

    /// <summary>
    /// 菜单信息
    /// </summary>
    public MenuDto MenuItem => TabItem.MenuItem;

    /// <summary>
    /// 图标字符串（用于 XAML 绑定）
    /// </summary>
    public string? Icon => TabItem.Icon;

    /// <summary>
    /// 视图类型名称（用于判断是否已打开）
    /// 基于 Component 或 RoutePath 生成
    /// </summary>
    public string ViewTypeName => TabItem.ViewTypeName;

    /// <summary>
    /// 是否已修改
    /// </summary>
    public bool IsModified
    {
        get => TabItem.IsModified;
        set
        {
            if (TabItem.IsModified != value)
            {
                TabItem.IsModified = value;
                UpdateTitle();
            }
        }
    }

    public AvalonDocument(DocumentTabItem tabItem)
    {
        TabItem = tabItem ?? throw new ArgumentNullException(nameof(tabItem));
        
        // 根据 AvalonDock 官方文档，按顺序设置属性
        // 1. 设置 ContentId
        this.ContentId = TabItem.ViewTypeName ?? Guid.NewGuid().ToString();
        
        // 2. 设置标题（AvalonDock 会自动显示 Title 属性）
        string title = tabItem.Title;
        if (string.IsNullOrWhiteSpace(title))
        {
            title = TabItem.MenuItem?.MenuName ?? TabItem.MenuItem?.MenuCode ?? "未命名";
            tabItem.Title = title;
        }
        
        // 使用 SetValue 设置依赖属性 Title（确保 AvalonDock 正确绑定）
        SetValue(TitleProperty, title);
        
        // 验证 Title 已设置
        if (string.IsNullOrWhiteSpace(this.Title))
        {
            throw new InvalidOperationException($"无法设置文档标题：TabItem.Title={tabItem.Title}, MenuName={TabItem.MenuItem?.MenuName}, MenuCode={TabItem.MenuItem?.MenuCode}");
        }
        
        // 3. 设置 CanClose
        this.CanClose = tabItem.CanClose;
        
        // 4. 设置 Content（最后设置，确保不影响 Title）
        if (tabItem.Content is System.Windows.UIElement contentElement)
        {
            // 直接设置 Content 属性（LayoutContent.Content 是普通属性，不是依赖属性）
            this.Content = contentElement;
        }
        else
        {
            throw new ArgumentException($"DocumentTabItem.Content 必须是 UIElement，实际类型：{tabItem.Content?.GetType().FullName ?? "null"}", nameof(tabItem));
        }
        
        // 5. 设置其他属性
        this.ToolTip = $"{title} (路由: {MenuItem.RoutePath ?? "无"})";
        
        // 设置图标（如果有）
        // 根据 AvalonDock 官方文档，LayoutDocumentItem 会自动从 Model.IconSource 读取图标
        // 直接设置 IconSource 属性即可
        if (!string.IsNullOrEmpty(tabItem.Icon))
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AvalonDocument] 开始转换图标: Icon={tabItem.Icon}");
                var iconSource = ConvertIconToImageSource(tabItem.Icon);
                if (iconSource != null)
                {
                    // 直接设置 IconSource 属性（AvalonDock 会自动处理）
                    this.IconSource = iconSource;
                    System.Diagnostics.Debug.WriteLine($"[AvalonDocument] 图标设置成功: Icon={tabItem.Icon}, IconSource={iconSource.GetType().Name}, Width={iconSource.Width}, Height={iconSource.Height}, IconSourceIsNull={this.IconSource == null}");
                    
                    // 验证 IconSource 是否已设置
                    if (this.IconSource == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AvalonDocument] 警告：IconSource 设置后仍为 null！");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AvalonDocument] 图标转换返回 null: Icon={tabItem.Icon}");
                }
            }
            catch (Exception ex)
            {
                // 图标设置失败，记录异常但不抛出（避免影响文档创建）
                System.Diagnostics.Debug.WriteLine($"[AvalonDocument] 图标设置失败: Icon={tabItem.Icon}, Exception={ex.Message}, StackTrace={ex.StackTrace}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[AvalonDocument] 图标为空: TabItem.Icon=null");
        }
    }

    /// <summary>
    /// 更新标题（当 IsModified 改变时）
    /// </summary>
    private void UpdateTitle()
    {
        string newTitle = TabItem.IsModified ? $"{TabItem.Title} *" : TabItem.Title;
        SetValue(TitleProperty, newTitle);
    }
    
    public override string ToString()
    {
        return this.Title ?? TabItem?.Title ?? string.Empty;
    }
    
    /// <summary>
    /// 将 FontAwesome 图标字符串转换为 ImageSource
    /// 使用 FormattedText 渲染图标字符
    /// </summary>
    public static ImageSource? ConvertIconToImageSourceStatic(string iconString)
    {
        return ConvertIconToImageSource(iconString);
    }
    
    /// <summary>
    /// 将 FontAwesome 图标字符串转换为 ImageSource
    /// 使用 FontAwesome.Sharp 的 ToImageSource 扩展方法（如果可用）或 FormattedText 渲染
    /// </summary>
    private static ImageSource? ConvertIconToImageSource(string iconString)
    {
        if (string.IsNullOrEmpty(iconString))
        {
            System.Diagnostics.Debug.WriteLine($"[AvalonDocument] ConvertIconToImageSource: iconString 为空");
            return null;
        }
            
        try
        {
            // 解析 IconChar
            if (!Enum.TryParse<IconChar>(iconString, true, out var iconChar))
            {
                System.Diagnostics.Debug.WriteLine($"[AvalonDocument] ConvertIconToImageSource: 无法解析 IconChar: {iconString}");
                return null;
            }
            
            System.Diagnostics.Debug.WriteLine($"[AvalonDocument] ConvertIconToImageSource: 成功解析 IconChar: {iconString} -> {iconChar} ({(char)iconChar})");
            
            // 获取主题颜色
            var foregroundBrush = System.Windows.Application.Current?.TryFindResource("TextFillColorPrimaryBrush") as System.Windows.Media.Brush
                ?? System.Windows.Media.Brushes.Black;
            
            // 尝试使用 FontAwesome.Sharp 的 ToImageSource 扩展方法（如果可用）
            // 注意：FontAwesome.Sharp 6.x 可能不直接提供此方法，需要检查
            try
            {
                // 使用反射检查是否有 ToImageSource 扩展方法
                var iconCharType = typeof(IconChar);
                var extensionMethods = iconCharType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Where(m => m.Name == "ToImageSource" && m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
                    .ToList();
                
                if (extensionMethods.Any())
                {
                    var method = extensionMethods.First();
                    var parameters = method.GetParameters();
                    if (parameters.Length >= 2 && parameters[0].ParameterType == typeof(IconChar))
                    {
                        // 调用 ToImageSource(IconChar icon, Brush brush, double size)
                        var result = method.Invoke(null, new object[] { iconChar, foregroundBrush, 16.0 });
                        if (result is ImageSource imageSource)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AvalonDocument] ConvertIconToImageSource: 使用 ToImageSource 扩展方法成功: {iconString}, Size={imageSource.Width}x{imageSource.Height}");
                            return imageSource;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AvalonDocument] ConvertIconToImageSource: ToImageSource 扩展方法不可用，使用 FormattedText 方式: {ex.Message}");
            }
            
            // 回退方案：使用 FormattedText 渲染图标字符
            var fontFamily = new FontFamily("pack://application:,,,/FontAwesome.Sharp;component/#Font Awesome 6 Free Solid");
            
            var formattedText = new FormattedText(
                ((char)iconChar).ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                16.0, // FontSize
                foregroundBrush, // 使用主题颜色
                96.0); // PixelsPerDip
            
            // 创建一个 DrawingVisual 来渲染文本
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawText(formattedText, new Point(0, 0));
            }
            
            // 创建 RenderTargetBitmap 来捕获渲染结果
            var bitmap = new RenderTargetBitmap(16, 16, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            bitmap.Freeze();
            
            System.Diagnostics.Debug.WriteLine($"[AvalonDocument] ConvertIconToImageSource: 使用 FormattedText 成功创建 ImageSource: {iconString}, Size={bitmap.Width}x{bitmap.Height}");
            return bitmap;
        }
        catch (Exception ex)
        {
            // 转换失败，返回 null
            System.Diagnostics.Debug.WriteLine($"[AvalonDocument] ConvertIconToImageSource: 转换失败: Icon={iconString}, Exception={ex.Message}, StackTrace={ex.StackTrace}");
            return null;
        }
    }
}

