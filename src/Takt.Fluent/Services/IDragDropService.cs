// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Services
// 文件名称：IDragDropService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：拖拽辅助线服务接口，提供元素拖拽和对齐辅助线功能
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Takt.Fluent.Services;

/// <summary>
/// 拖拽辅助线服务接口
/// 提供统一的元素拖拽和对齐辅助线功能
/// </summary>
public interface IDragDropService
{
    /// <summary>
    /// 启用元素的拖拽功能
    /// </summary>
    /// <param name="element">要启用拖拽的元素</param>
    /// <param name="container">容器元素（用于计算相对位置）</param>
    /// <param name="snapDistance">吸附距离（像素），默认10</param>
    /// <param name="snapThreshold">自动吸附阈值（像素），默认3</param>
    /// <param name="stepSize">步进移动大小（像素），默认20。如果为0则不使用步进</param>
    /// <param name="otherElements">用于对齐检测的其他元素列表，如果为null则自动查找容器内所有可拖动元素</param>
    /// <param name="onDragStarted">拖拽开始回调</param>
    /// <param name="onDragEnded">拖拽结束回调</param>
    /// <param name="isEditMode">是否为编辑模式，只有编辑模式才显示辅助线</param>
    void EnableDrag(
        FrameworkElement element,
        Panel container,
        double snapDistance = 10.0,
        double snapThreshold = 3.0,
        double stepSize = 20.0,
        IEnumerable<FrameworkElement>? otherElements = null!, 
        Action<FrameworkElement, Point>? onDragStarted = null!, 
        Action<FrameworkElement, Point>? onDragEnded = null!, 
        Func<bool>? isEditMode = null);

    /// <summary>
    /// 禁用元素的拖拽功能
    /// </summary>
    /// <param name="element">要禁用拖拽的元素</param>
    void DisableDrag(FrameworkElement element);

    /// <summary>
    /// 清除所有辅助线
    /// </summary>
    void ClearGuideLines();

    /// <summary>
    /// 获取元素的当前位置（相对于容器）
    /// </summary>
    /// <param name="element">元素</param>
    /// <param name="container">容器</param>
    /// <returns>位置坐标</returns>
    Point GetElementPosition(FrameworkElement element, Panel container);

    /// <summary>
    /// 设置元素的当前位置（相对于容器）
    /// </summary>
    /// <param name="element">元素</param>
    /// <param name="container">容器</param>
    /// <param name="position">位置坐标</param>
    void SetElementPosition(FrameworkElement element, Panel container, Point position);
}

