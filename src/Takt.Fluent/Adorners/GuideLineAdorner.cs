// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Adorners
// 文件名称：GuideLineAdorner.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：辅助线装饰器，使用 AdornerLayer 实现辅助线显示
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Takt.Fluent.Adorners;

/// <summary>
/// 辅助线装饰器（使用 AdornerLayer 实现，参考 AutoCAD 方式）
/// 优点：不占用布局空间，不影响鼠标事件，性能更好
/// </summary>
public class GuideLineAdorner : Adorner
{
    private readonly double _elementLeft;
    private readonly double _elementTop;
    private readonly double _canvasWidth;
    private readonly double _canvasHeight;
    private readonly HashSet<double> _horizontalAlignY;
    private readonly HashSet<double> _verticalAlignX;
    private readonly Pen _guideLinePen;
    private readonly Typeface _labelTypeface;
    private readonly Brush _labelForeground;
    private readonly Brush _labelBackground;
    private readonly double _elementWidth;
    private readonly double _elementHeight;
    private readonly double _snapDistance; // 对齐虚线显示距离（像素）

    public GuideLineAdorner(
        UIElement adornedElement,
        double elementLeft,
        double elementTop,
        double elementWidth,
        double elementHeight,
        double canvasWidth,
        double canvasHeight,
        HashSet<double> horizontalAlignY,
        HashSet<double> verticalAlignX,
        double snapDistance = 20.0) // 对齐虚线显示距离，默认200px
        : base(adornedElement)
    {
        _elementLeft = elementLeft;
        _elementTop = elementTop;
        _elementWidth = elementWidth;
        _elementHeight = elementHeight;
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
        _horizontalAlignY = horizontalAlignY ?? new HashSet<double>();
        _verticalAlignX = verticalAlignX ?? new HashSet<double>();
        _snapDistance = snapDistance;

        // 现代流程图风格：细虚线，蓝色，半透明
        var guideLineBrush = new SolidColorBrush(Color.FromArgb(200, 0, 120, 215)); // 半透明蓝色
        guideLineBrush.Freeze();
        _guideLinePen = new Pen(guideLineBrush, 1.0)
        {
            DashStyle = new DashStyle(new double[] { 3, 3 }, 0) // 更细的虚线
        };
        _guideLinePen.Freeze();

        // 创建标签字体和画刷
        _labelTypeface = new Typeface(
            new FontFamily("Consolas, Courier New"),
            FontStyles.Normal,
            FontWeights.Bold,
            FontStretches.Normal);
        // 现代流程图风格：标签使用更柔和的颜色
        _labelForeground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        _labelForeground.Freeze();
        _labelBackground = new SolidColorBrush(Color.FromArgb(220, 0, 120, 215)); // 半透明蓝色背景
        _labelBackground.Freeze();

        // 确保不阻挡鼠标事件
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // 现代流程图风格：不显示元素自身的位置线，只显示对齐线

        // 现代流程图风格：只显示对齐线，显示距离信息（如 "+5px"）
        var displayThreshold = _snapDistance;
        
        var elementRight = _elementLeft + _elementWidth;
        var elementBottom = _elementTop + _elementHeight;
        var elementCenterX = _elementLeft + _elementWidth / 2;
        var elementCenterY = _elementTop + _elementHeight / 2;

        // 水平对齐辅助线（显示距离信息）
        var horizontalCandidates = new List<(double y, double distance, string alignmentType)>();
        foreach (var alignY in _horizontalAlignY)
        {
            var distToTop = Math.Abs(_elementTop - alignY);
            var distToCenter = Math.Abs(elementCenterY - alignY);
            var distToBottom = Math.Abs(elementBottom - alignY);
            var minDist = Math.Min(Math.Min(distToTop, distToCenter), distToBottom);
            
            if (minDist < displayThreshold && minDist > 0.1)
            {
                string alignmentType = "center";
                if (distToTop == minDist) alignmentType = "top";
                else if (distToBottom == minDist) alignmentType = "bottom";
                
                horizontalCandidates.Add((alignY, minDist, alignmentType));
            }
        }
        
        // 按距离排序，只显示最近的2条
        var sortedHorizontal = horizontalCandidates
            .OrderBy(c => c.distance)
            .Take(2);
        
        foreach (var (alignY, distance, _) in sortedHorizontal)
        {
            var clampedY = Math.Max(0, Math.Min(alignY, _canvasHeight));
            drawingContext.DrawLine(_guideLinePen, new Point(0, clampedY), new Point(_canvasWidth, clampedY));
            
            // 显示距离信息（现代流程图风格：显示相对距离）
            var distanceText = distance < 1.0 ? "对齐" : $"{distance:F0}px";
            DrawLabel(drawingContext, distanceText, new Point(8, clampedY - 12), isHorizontal: true);
        }

        // 垂直对齐辅助线（显示距离信息）
        var verticalCandidates = new List<(double x, double distance, string alignmentType)>();
        foreach (var alignX in _verticalAlignX)
        {
            var distToLeft = Math.Abs(_elementLeft - alignX);
            var distToCenter = Math.Abs(elementCenterX - alignX);
            var distToRight = Math.Abs(elementRight - alignX);
            var minDist = Math.Min(Math.Min(distToLeft, distToCenter), distToRight);
            
            if (minDist < displayThreshold && minDist > 0.1)
            {
                string alignmentType = "center";
                if (distToLeft == minDist) alignmentType = "left";
                else if (distToRight == minDist) alignmentType = "right";
                
                verticalCandidates.Add((alignX, minDist, alignmentType));
            }
        }
        
        // 按距离排序，只显示最近的2条
        var sortedVertical = verticalCandidates
            .OrderBy(c => c.distance)
            .Take(2);
        
        foreach (var (alignX, distance, _) in sortedVertical)
        {
            var clampedX = Math.Max(0, Math.Min(alignX, _canvasWidth));
            drawingContext.DrawLine(_guideLinePen, new Point(clampedX, 0), new Point(clampedX, _canvasHeight));
            
            // 显示距离信息（现代流程图风格：显示相对距离）
            var distanceText = distance < 1.0 ? "对齐" : $"{distance:F0}px";
            DrawLabel(drawingContext, distanceText, new Point(clampedX - 30, 8), isHorizontal: false);
        }
    }

    private void DrawLabel(DrawingContext drawingContext, string text, Point position, bool isHorizontal)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _labelTypeface,
            11,
            _labelForeground,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        var labelSize = new Size(formattedText.Width + 12, formattedText.Height + 6);
        var labelRect = new Rect(position, labelSize);

        // 绘制背景
        drawingContext.DrawRoundedRectangle(_labelBackground, null, labelRect, 3, 3);

        // 绘制文字（居中）
        drawingContext.DrawText(formattedText, new Point(position.X + 6, position.Y + 3));
    }
}

