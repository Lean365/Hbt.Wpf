// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Adorners
// 文件名称：DragPreviewAdorner.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：拖拽预览装饰器，显示虚线边框预览
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Takt.Fluent.Adorners;

/// <summary>
/// 拖拽预览装饰器（显示虚线边框）
/// </summary>
public class DragPreviewAdorner : Adorner
{
    private readonly double _left;
    private readonly double _top;
    private readonly double _width;
    private readonly double _height;
    private readonly Pen _borderPen;
    private readonly bool _attachToElement; // 是否附加到元素本身

    /// <summary>
    /// 创建拖拽预览装饰器
    /// </summary>
    /// <param name="adornedElement">被装饰的元素（容器或拖拽的元素本身）</param>
    /// <param name="left">元素左侧位置（相对于容器）</param>
    /// <param name="top">元素顶部位置（相对于容器）</param>
    /// <param name="width">元素宽度</param>
    /// <param name="height">元素高度</param>
    /// <param name="attachToElement">如果为 true，边框直接附加到元素上；如果为 false，使用绝对坐标</param>
    public DragPreviewAdorner(UIElement adornedElement, double left, double top, double width, double height, bool attachToElement = false)
        : base(adornedElement)
    {
        _left = left;
        _top = top;
        _width = width;
        _height = height;
        _attachToElement = attachToElement;

        // 创建虚线边框画笔（青色，1px宽度，更明显）
        _borderPen = new Pen(
            new SolidColorBrush(Color.FromRgb(0, 162, 232)),
            1.0) // 1px 宽度，更明显
        {
            DashStyle = new DashStyle(new double[] { 4, 2 }, 0)
        };
        _borderPen.Freeze();

        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        Rect rect;
        if (_attachToElement)
        {
            // 如果附加到元素本身，使用元素的实际边界（考虑 Margin 和 Padding）
            rect = new Rect(
                -1, // 稍微向外扩展，确保边框可见
                -1,
                AdornedElement.RenderSize.Width + 2,
                AdornedElement.RenderSize.Height + 2);
        }
        else
        {
            // 使用绝对坐标（相对于容器）
            rect = new Rect(_left, _top, _width, _height);
        }

        // 绘制虚线边框矩形
        drawingContext.DrawRectangle(null, _borderPen, rect);
    }

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
    {
        // 返回原始变换，预览位置通过 OnRender 中的坐标控制
        return base.GetDesiredTransform(transform);
    }
}

