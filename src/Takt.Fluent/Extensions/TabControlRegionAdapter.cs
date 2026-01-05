// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Extensions
// 文件名称：TabControlRegionAdapter.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：TabControl Region Adapter，使 Prism RegionManager 能够管理 TabControl
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using Prism.Navigation.Regions;
using Prism.Navigation.Regions.Behaviors;
using Takt.Application.Dtos.Identity;
using Takt.Common.Logging;
using Takt.Fluent.Models;
using Takt.Fluent.ViewModels;

namespace Takt.Fluent.Extensions;

/// <summary>
/// TabControl Region Adapter
/// 将 TabControl 适配为 Prism Region，支持标签页导航
/// 同时与 MainWindowViewModel 的 DocumentTabs 集合协同工作
/// </summary>
public class TabControlRegionAdapter : RegionAdapterBase<TabControl>
{
    private readonly OperLogManager? _operLog;
    private MainWindowViewModel? _mainWindowViewModel;

    public TabControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory, OperLogManager? operLog = null)
        : base(regionBehaviorFactory)
    {
        _operLog = operLog;
    }

    protected override void Adapt(IRegion region, TabControl regionTarget)
    {
        if (region == null) throw new ArgumentNullException(nameof(region));
        if (regionTarget == null) throw new ArgumentNullException(nameof(regionTarget));

        // 尝试获取 MainWindowViewModel（通过 DataContext 或查找父元素）
        _mainWindowViewModel = GetMainWindowViewModel(regionTarget);

        region.Views.CollectionChanged += (s, e) =>
        {
            OnViewsCollectionChanged(region, regionTarget, e);
        };

        region.ActiveViews.CollectionChanged += (s, e) =>
        {
            OnActiveViewsCollectionChanged(regionTarget, e);
        };

        // 处理用户手动切换标签页（通过 DocumentTabs 绑定）
        // 注意：由于使用了 ItemsSource 绑定，这里不直接处理 SelectionChanged
    }

    private MainWindowViewModel? GetMainWindowViewModel(DependencyObject element)
    {
        var current = element;
        while (current != null)
        {
            if (current is FrameworkElement fe && fe.DataContext is MainWindowViewModel vm)
            {
                return vm;
            }
            current = LogicalTreeHelper.GetParent(current) ?? VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    protected override IRegion CreateRegion()
    {
        return new AllActiveRegion();
    }

    private void OnViewsCollectionChanged(IRegion region, TabControl regionTarget, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // 由于 TabControl 使用 ItemsSource 绑定到 DocumentTabs，这里不需要直接操作 Items
        // 而是通过 NavigationService 更新 DocumentTabs 集合
        _operLog?.Debug("[TabControlRegion] 视图集合变更：Action={Action}, Count={Count}", 
            e.Action, e.NewItems?.Count ?? e.OldItems?.Count ?? 0);
    }

    private void OnActiveViewsCollectionChanged(TabControl regionTarget, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && _mainWindowViewModel != null)
        {
            foreach (var view in e.NewItems!)
            {
                // 通过 DocumentTabs 集合激活对应的标签页
                var documentTab = _mainWindowViewModel.DocumentTabs
                    .FirstOrDefault(t => t.Content == view);
                if (documentTab != null)
                {
                    _mainWindowViewModel.SelectedTab = documentTab;
                    _operLog?.Debug("[TabControlRegion] 激活标签页：{ViewType}", view.GetType().Name);
                }
            }
        }
    }
}

