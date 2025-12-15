// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Services
// 文件名称：DragDropService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：拖拽辅助线服务实现，提供统一的元素拖拽和对齐辅助线功能
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Takt.Fluent.Adorners;

namespace Takt.Fluent.Services;

/// <summary>
/// 拖拽辅助线服务实现
/// </summary>
public class DragDropService : IDragDropService
{
    private readonly Dictionary<FrameworkElement, DragContext> _dragContexts = new();
    private GuideLineAdorner? _currentGuideLineAdorner;
    private DragPreviewAdorner? _currentDragPreviewAdorner;
    private UIElement? _adornerContainer;

    public void EnableDrag(
        FrameworkElement element,
        Panel container,
        double snapDistance = 10.0,
        double snapThreshold = 3.0,
        double stepSize = 20.0,
        IEnumerable<FrameworkElement>? otherElements = null!, 
        Action<FrameworkElement, Point>? onDragStarted = null!, 
        Action<FrameworkElement, Point>? onDragEnded = null!, 
        Func<bool>? isEditMode = null)
    {
        if (element == null) throw new ArgumentNullException(nameof(element));
        if (container == null) throw new ArgumentNullException(nameof(container));

        // 如果已启用，先禁用
        if (_dragContexts.ContainsKey(element))
        {
            DisableDrag(element);
        }

        var context = new DragContext(this)
        {
            Element = element,
            Container = container,
            SnapDistance = snapDistance,
            SnapThreshold = snapThreshold,
            StepSize = stepSize,
            OtherElements = otherElements,
            OnDragStarted = onDragStarted,
            OnDragEnded = onDragEnded,
            IsEditMode = isEditMode ?? (() => true)
        };

        _dragContexts[element] = context;

        // 注册鼠标事件
        element.MouseLeftButtonDown += context.OnMouseLeftButtonDown;
        element.MouseMove += context.OnMouseMove;
        element.MouseLeftButtonUp += context.OnMouseLeftButtonUp;

        // 设置容器（用于AdornerLayer）
        if (_adornerContainer == null)
        {
            _adornerContainer = container;
        }
    }

    public void DisableDrag(FrameworkElement element)
    {
        if (element == null) return;

        if (_dragContexts.TryGetValue(element, out var context))
        {
            // 取消事件订阅
            element.MouseLeftButtonDown -= context.OnMouseLeftButtonDown;
            element.MouseMove -= context.OnMouseMove;
            element.MouseLeftButtonUp -= context.OnMouseLeftButtonUp;

            // 释放鼠标捕获
            if (element.IsMouseCaptured)
            {
                element.ReleaseMouseCapture();
            }

            _dragContexts.Remove(element);
        }
    }

    public void ClearGuideLines()
    {
        if (_adornerContainer == null) return;

        var adornerLayer = AdornerLayer.GetAdornerLayer(_adornerContainer);
        if (adornerLayer == null) return;

        if (_currentGuideLineAdorner != null)
        {
            adornerLayer.Remove(_currentGuideLineAdorner);
            _currentGuideLineAdorner = null;
        }

        if (_currentDragPreviewAdorner != null)
        {
            adornerLayer.Remove(_currentDragPreviewAdorner);
            _currentDragPreviewAdorner = null;
        }
    }

    public Point GetElementPosition(FrameworkElement element, Panel container)
    {
        if (element == null || container == null)
            return new Point(0, 0);

        var position = element.TransformToAncestor(container).Transform(new Point(0, 0));
        return position;
    }

    public void SetElementPosition(FrameworkElement element, Panel container, Point position)
    {
        if (element == null || container == null) return;

        // 根据容器类型选择不同的定位方式
        if (container is Canvas canvas)
        {
            // Canvas 使用 SetLeft/SetTop（参考 WPF-Samples）
            Canvas.SetLeft(element, position.X);
            Canvas.SetTop(element, position.Y);
        }
        else
        {
            // Grid 或其他容器使用 Margin
            var margin = element.Margin;
            element.Margin = new Thickness(position.X, position.Y, margin.Right, margin.Bottom);
        }
    }

    private class DragContext
    {
        private readonly DragDropService _service;

        public FrameworkElement Element { get; set; } = null!;
        public Panel Container { get; set; } = null!;
        public double SnapDistance { get; set; }
        public double SnapThreshold { get; set; }
        public double StepSize { get; set; }
        public IEnumerable<FrameworkElement>? OtherElements { get; set; }
        public Action<FrameworkElement, Point>? OnDragStarted { get; set; }
        public Action<FrameworkElement, Point>? OnDragEnded { get; set; }
        public Func<bool> IsEditMode { get; set; } = () => true;

        private bool _isDragging;
        private Point _dragStartPoint;
        private Point _initialElementPosition;
        private FrameworkElement? _draggedElement;
        private TranslateTransform? _dragTransform; // 拖拽时的临时变换

        public DragContext(DragDropService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEditMode() || e.ChangedButton != MouseButton.Left) return;

            _draggedElement = sender as FrameworkElement;
            if (_draggedElement == null) return;

            _dragStartPoint = e.GetPosition(Container);
            _initialElementPosition = _service.GetElementPosition(_draggedElement, Container);
            _isDragging = false;

            // 初始化拖拽变换（参考 WPF-Samples：使用变换而非直接修改布局，避免频繁布局更新）
            _dragTransform = null;
            
            if (_draggedElement.RenderTransform is TranslateTransform translateTransform)
            {
                _dragTransform = translateTransform;
            }
            else if (_draggedElement.RenderTransform is TransformGroup transformGroup)
            {
                // 如果有 TransformGroup，查找其中的 TranslateTransform
                _dragTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
                if (_dragTransform == null)
                {
                    _dragTransform = new TranslateTransform();
                    transformGroup.Children.Add(_dragTransform);
                }
            }
            else
            {
                // 创建新的 TranslateTransform
                _dragTransform = new TranslateTransform();
                _draggedElement.RenderTransform = _dragTransform;
            }
            
            // 重置变换偏移
            _dragTransform.X = 0;
            _dragTransform.Y = 0;

            _draggedElement.CaptureMouse();
            e.Handled = true;

            OnDragStarted?.Invoke(_draggedElement, _initialElementPosition);
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsEditMode() || _draggedElement == null || !_draggedElement.IsMouseCaptured) return;

            var currentPoint = e.GetPosition(Container);
            var deltaX = currentPoint.X - _dragStartPoint.X;
            var deltaY = currentPoint.Y - _dragStartPoint.Y;

            if (!_isDragging &&
                (Math.Abs(deltaX) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(deltaY) > SystemParameters.MinimumVerticalDragDistance))
            {
                _isDragging = true;
            }

            if (!_isDragging) return;

            // 计算新位置
            var newX = _initialElementPosition.X + deltaX;
            var newY = _initialElementPosition.Y + deltaY;

            // 步进移动
            if (StepSize > 0)
            {
                newX = Math.Round(newX / StepSize) * StepSize;
                newY = Math.Round(newY / StepSize) * StepSize;
            }

            // 边界检查
            var elementWidth = _draggedElement.ActualWidth > 0 ? _draggedElement.ActualWidth : 100;
            var elementHeight = _draggedElement.ActualHeight > 0 ? _draggedElement.ActualHeight : 30;

            newX = Math.Max(0, Math.Min(newX, Container.ActualWidth - elementWidth));
            newY = Math.Max(0, Math.Min(newY, Container.ActualHeight - elementHeight));

            // 检测对齐并应用吸附
            var (snappedX, snappedY, horizontalGuides, verticalGuides) = 
                CalculateSnap(_draggedElement, newX, newY, elementWidth, elementHeight);

            // 计算变换偏移量（相对于初始位置）
            var offsetX = snappedX - _initialElementPosition.X;
            var offsetY = snappedY - _initialElementPosition.Y;

            // 使用 TranslateTransform 临时移动元素（参考 WPF-Samples，避免频繁布局更新）
            if (_dragTransform != null)
            {
                _dragTransform.X = offsetX;
                _dragTransform.Y = offsetY;
            }
            else
            {
                // 如果变换未初始化，使用传统方式
                _service.SetElementPosition(_draggedElement, Container, new Point(snappedX, snappedY));
            }

            // 显示辅助线（使用实际显示位置 = 初始位置 + Transform偏移）
            var actualX = _initialElementPosition.X + (_dragTransform?.X ?? 0);
            var actualY = _initialElementPosition.Y + (_dragTransform?.Y ?? 0);
            ShowGuideLines(_draggedElement, actualX, actualY, elementWidth, elementHeight, horizontalGuides, verticalGuides);

            // 显示拖拽预览（直接附加到元素上，自动跟随 Transform）
            ShowDragPreview(_draggedElement, actualX, actualY, elementWidth, elementHeight);
        }

        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedElement == null || !_draggedElement.IsMouseCaptured) return;

            // 计算最终位置（初始位置 + 变换偏移）
            Point finalPosition;
            if (_dragTransform != null && _isDragging)
            {
                finalPosition = new Point(
                    _initialElementPosition.X + _dragTransform.X,
                    _initialElementPosition.Y + _dragTransform.Y);
                
                // 将变换偏移转换为实际的布局位置（参考 WPF-Samples：拖拽结束后应用位置）
                _service.SetElementPosition(_draggedElement, Container, finalPosition);
                
                // 清除变换偏移（恢复原始变换状态）
                _dragTransform.X = 0;
                _dragTransform.Y = 0;
            }
            else
            {
                finalPosition = _service.GetElementPosition(_draggedElement, Container);
            }

            _draggedElement.ReleaseMouseCapture();
            _isDragging = false;
            _dragTransform = null;

            // 清除辅助线和预览
            ClearGuideLines();

            OnDragEnded?.Invoke(_draggedElement, finalPosition);

            _draggedElement = null;
        }

        private (double x, double y, HashSet<double> horizontalGuides, HashSet<double> verticalGuides)
            CalculateSnap(FrameworkElement element, double newX, double newY, double elementWidth, double elementHeight)
        {
            var horizontalGuides = new HashSet<double>();
            var verticalGuides = new HashSet<double>();

            double? snapToX = null;
            double? snapToY = null;
            double minDistanceX = SnapThreshold;
            double minDistanceY = SnapThreshold;

            var elementLeft = newX;
            var elementTop = newY;
            var elementRight = elementLeft + elementWidth;
            var elementBottom = elementTop + elementHeight;
            var elementCenterX = elementLeft + elementWidth / 2;
            var elementCenterY = elementTop + elementHeight / 2;

            // 获取其他元素列表
            var others = OtherElements ?? Container.Children.OfType<FrameworkElement>()
                .Where(el => el != element && el.IsVisible);

            foreach (var other in others)
            {
                var otherPosition = other.TransformToAncestor(Container).Transform(new Point(0, 0));
                var otherWidth = other.ActualWidth > 0 ? other.ActualWidth : other.RenderSize.Width;
                var otherHeight = other.ActualHeight > 0 ? other.ActualHeight : other.RenderSize.Height;

                var otherLeft = otherPosition.X;
                var otherTop = otherPosition.Y;
                var otherRight = otherLeft + otherWidth;
                var otherBottom = otherTop + otherHeight;
                var otherCenterX = otherLeft + otherWidth / 2;
                var otherCenterY = otherTop + otherHeight / 2;

                // 垂直对齐检测
                CheckAlignment(elementLeft, otherLeft, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementLeft, otherRight, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementLeft, otherCenterX, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementCenterX, otherLeft, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementCenterX, otherRight, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementCenterX, otherCenterX, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementRight, otherLeft, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementRight, otherRight, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);
                CheckAlignment(elementRight, otherCenterX, SnapDistance, SnapThreshold, ref snapToX, ref minDistanceX, ref verticalGuides);

                // 水平对齐检测
                CheckAlignment(elementTop, otherTop, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementTop, otherBottom, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementTop, otherCenterY, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementCenterY, otherTop, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementCenterY, otherBottom, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementCenterY, otherCenterY, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementBottom, otherTop, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementBottom, otherBottom, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
                CheckAlignment(elementBottom, otherCenterY, SnapDistance, SnapThreshold, ref snapToY, ref minDistanceY, ref horizontalGuides);
            }

            var finalX = snapToX ?? newX;
            var finalY = snapToY ?? newY;

            return (finalX, finalY, horizontalGuides, verticalGuides);
        }

        private static void CheckAlignment(
            double elementPos,
            double otherPos,
            double snapDistance,
            double snapThreshold,
            ref double? snapTo,
            ref double minDistance,
            ref HashSet<double> guidePositions)
        {
            var distance = Math.Abs(elementPos - otherPos);

            if (distance < snapDistance)
            {
                guidePositions.Add(otherPos);

                if (distance < snapThreshold && distance < minDistance)
                {
                    snapTo = otherPos;
                    minDistance = distance;
                }
            }
        }

        private void ShowGuideLines(
            FrameworkElement element,
            double left,
            double top,
            double width,
            double height,
            HashSet<double> horizontalGuides,
            HashSet<double> verticalGuides)
        {
            if (!IsEditMode()) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(Container);
            if (adornerLayer == null) return;

            var containerWidth = Container.ActualWidth > 0 ? Container.ActualWidth : Container.RenderSize.Width;
            var containerHeight = Container.ActualHeight > 0 ? Container.ActualHeight : Container.RenderSize.Height;

            // 移除旧的
            _service.ClearGuideLines();

            // 创建新的
            var guideAdorner = new GuideLineAdorner(
                Container,
                left,
                top,
                width,
                height,
                containerWidth,
                containerHeight,
                horizontalGuides,
                verticalGuides);

            adornerLayer.Add(guideAdorner);

            _service._currentGuideLineAdorner = guideAdorner;
            _service._adornerContainer = Container;
        }

        private void ShowDragPreview(
            FrameworkElement element,
            double left,
            double top,
            double width,
            double height)
        {
            if (!IsEditMode()) return;

            // 始终在元素本身上添加 Adorner（这样会自动跟随 Transform 移动）
            var elementAdornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (elementAdornerLayer == null)
            {
                // 如果元素上没有 AdornerLayer，尝试在容器上创建
                var containerAdornerLayer = AdornerLayer.GetAdornerLayer(Container);
                if (containerAdornerLayer == null) return;

                // 移除旧的容器 Adorner
                if (_service._currentDragPreviewAdorner != null)
                {
                    containerAdornerLayer.Remove(_service._currentDragPreviewAdorner);
                }

                // 在容器上创建 Adorner（使用实际显示位置）
                var containerPreviewAdorner = new DragPreviewAdorner(Container, left, top, width, height, attachToElement: false);
                containerAdornerLayer.Add(containerPreviewAdorner);
                _service._currentDragPreviewAdorner = containerPreviewAdorner;
                return;
            }

            // 移除旧的元素 Adorner
            if (_service._currentDragPreviewAdorner != null && _service._currentDragPreviewAdorner.AdornedElement == element)
            {
                elementAdornerLayer.Remove(_service._currentDragPreviewAdorner);
            }

            // 在元素本身上创建 Adorner（attachToElement = true，会自动跟随 Transform）
            var elementPreviewAdorner = new DragPreviewAdorner(element, 0, 0, 0, 0, attachToElement: true);
            elementAdornerLayer.Add(elementPreviewAdorner);
            _service._currentDragPreviewAdorner = elementPreviewAdorner;
        }

        private void ClearGuideLines()
        {
            _service.ClearGuideLines();
        }
    }
}

