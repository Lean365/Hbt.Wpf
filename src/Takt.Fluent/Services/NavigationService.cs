// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Services
// 文件名称：NavigationService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：导航服务，封装 Prism RegionManager 导航逻辑
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using Prism.Navigation.Regions;
using System;
using Takt.Application.Dtos.Identity;
using Takt.Common.Logging;

namespace Takt.Fluent.Services;

/// <summary>
/// 导航服务接口
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 导航到指定菜单
    /// </summary>
    void NavigateToMenu(MenuDto menuItem);
}

/// <summary>
/// 导航服务实现
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IRegionManager _regionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IContainerProvider _containerProvider;
    private readonly OperLogManager? _operLog;

    public NavigationService(IRegionManager regionManager, IServiceProvider serviceProvider, IContainerProvider containerProvider, OperLogManager? operLog = null)
    {
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _containerProvider = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));
        _operLog = operLog;
    }

    public void NavigateToMenu(MenuDto menuItem)
    {
        if (menuItem == null)
        {
            return;
        }

        _operLog?.Debug("[NavigationService] NavigateToMenu 开始：菜单={MenuCode} ({MenuName})", menuItem.MenuCode, menuItem.MenuName);

        // 获取视图类型名称
        string? viewTypeName = null;

        if (!string.IsNullOrEmpty(menuItem.Component))
        {
            viewTypeName = menuItem.Component;
            _operLog?.Debug("[NavigationService] 使用 Component：{Component}", viewTypeName);
        }
        else if (!string.IsNullOrEmpty(menuItem.RoutePath))
        {
            viewTypeName = $"Takt.Fluent.{menuItem.RoutePath.Replace('/', '.')}";
            _operLog?.Debug("[NavigationService] 使用 RoutePath 生成类型名称：{TypeName}", viewTypeName);
        }
        else
        {
            _operLog?.Warning("[NavigationService] 菜单 RoutePath 和 Component 都为空：{MenuCode}", menuItem.MenuCode);
            return;
        }

        if (string.IsNullOrEmpty(viewTypeName))
        {
            return;
        }

        try
        {
            // 查找视图类型
            Type? viewType = Type.GetType(viewTypeName);
            if (viewType == null)
            {
                viewType = typeof(Views.MainWindow).Assembly.GetType(viewTypeName);
            }

            if (viewType == null)
            {
                _operLog?.Error("[NavigationService] 找不到视图类型：{TypeName}", viewTypeName);
                return;
            }

            // 创建视图实例：优先使用 Prism 容器解析（如果已通过 RegisterForNavigation 注册）
            // 如果 Prism 容器解析失败，记录详细错误信息，但不回退到 ServiceProvider
            // 因为通过 RegisterForNavigation 注册的视图应该能够从 Prism 容器正确解析
            object? viewInstance = null;
            try
            {
                // 尝试从 Prism 容器解析（如果已通过 RegisterForNavigation 注册）
                viewInstance = _containerProvider.Resolve(viewType);
                _operLog?.Debug("[NavigationService] 从 Prism 容器解析视图成功：{TypeName}", viewTypeName);
            }
            catch (Exception prismEx)
            {
                // Prism 容器解析失败，记录详细错误信息
                _operLog?.Error(prismEx, "[NavigationService] 从 Prism 容器解析视图失败：{TypeName}, 错误类型：{ExceptionType}, 错误消息：{Error}", 
                    viewTypeName, prismEx.GetType().Name, prismEx.Message);
                
                // 检查是否是 ContainerResolutionException，记录更详细的信息
                if (prismEx is Prism.Ioc.ContainerResolutionException containerEx)
                {
                    _operLog?.Error(containerEx, "[NavigationService] ContainerResolutionException 详情 - 服务类型：{ServiceType}, 内部异常：{InnerException}",
                        containerEx.ServiceType?.FullName ?? "未知", containerEx.InnerException?.Message ?? "无");
                }
                
                // 如果 Prism 容器中未注册，尝试使用 ServiceProvider（用于非 Prism 注册的视图）
                // 但这种情况应该很少见，因为大部分视图都应该通过 RegisterForNavigation 注册
                try
                {
                    viewInstance = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
                    _operLog?.Warning("[NavigationService] Prism 容器解析失败，回退到 ServiceProvider 创建视图成功：{TypeName}", viewTypeName);
                }
                catch (Exception createEx)
                {
                    _operLog?.Error(createEx, "[NavigationService] 从 ServiceProvider 创建视图也失败：{TypeName}, 错误类型：{ExceptionType}, 错误消息：{Error}", 
                        viewTypeName, createEx.GetType().Name, createEx.Message);
                    
                    // 提供更详细的错误信息
                    var errorMessage = $"无法创建视图 '{viewTypeName}'。\n\n" +
                        $"Prism 容器错误：{prismEx.GetType().Name}: {prismEx.Message}\n" +
                        $"ServiceProvider 错误：{createEx.GetType().Name}: {createEx.Message}\n\n" +
                        $"请确保该视图已通过 RegisterForNavigation 正确注册，或者其依赖的服务都已注册。";
                    
                    _operLog?.Error(errorMessage);
                    return;
                }
            }
            
            if (viewInstance == null)
            {
                _operLog?.Error("[NavigationService] 创建视图实例失败：{TypeName}", viewTypeName);
                return;
            }

            // 获取 MainWindowViewModel 以更新 DocumentTabs
            var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
            var mainWindowViewModel = mainWindow?.ViewModel;

            if (mainWindowViewModel == null)
            {
                _operLog?.Error("[NavigationService] MainWindowViewModel 未找到");
                return;
            }

            // 检查 DocumentTabs 中是否已存在相同的视图
            var existingTab = mainWindowViewModel.DocumentTabs
                .FirstOrDefault(t => t.ViewTypeName == viewTypeName);

            if (existingTab != null)
            {
                // 已存在，激活它
                mainWindowViewModel.SelectedTab = existingTab;
                _operLog?.Information("[NavigationService] 激活已存在的视图：{TypeName}", viewTypeName);
            }
            else
            {
                // 不存在，创建 DocumentTabItem 并添加到集合
                var titleKey = menuItem.I18nKey ?? menuItem.MenuCode ?? string.Empty;
                var localizationManager = _serviceProvider.GetService<Takt.Domain.Interfaces.ILocalizationManager>();
                var title = !string.IsNullOrWhiteSpace(titleKey) && localizationManager != null
                    ? localizationManager.GetString(titleKey)
                    : menuItem.MenuName ?? "未命名";

                var documentTab = new Takt.Fluent.Models.DocumentTabItem(menuItem, title, viewInstance, viewTypeName);
                mainWindowViewModel.DocumentTabs.Add(documentTab);
                mainWindowViewModel.SelectedTab = documentTab;

                // 同时添加到 Region（如果 Region 存在）
                var region = _regionManager.Regions.ContainsRegionWithName("DocumentTabRegion")
                    ? _regionManager.Regions["DocumentTabRegion"]
                    : null;
                if (region != null)
                {
                    region.Add(viewInstance);
                    region.Activate(viewInstance);
                }

                _operLog?.Information("[NavigationService] 添加并激活新视图：{TypeName}", viewTypeName);
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[NavigationService] 导航失败：{TypeName}", viewTypeName);
        }
    }
}

