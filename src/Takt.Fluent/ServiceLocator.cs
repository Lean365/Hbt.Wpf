//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : ServiceLocator.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 服务定位器（用于WPF中的依赖注入）
//===================================================================

using Autofac;

namespace Takt.Fluent;

/// <summary>
/// 服务定位器
/// 提供全局访问依赖注入容器的方式
/// </summary>
public static class ServiceLocator
{
    private static IContainer? _container;

    /// <summary>
    /// 设置容器
    /// </summary>
    public static void SetContainer(IContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    public static T Resolve<T>() where T : notnull
    {
        if (_container == null)
            throw new InvalidOperationException("容器未初始化，请先调用SetContainer方法");

        return _container.Resolve<T>();
    }

    /// <summary>
    /// 尝试获取服务实例
    /// </summary>
    public static T? ResolveOptional<T>() where T : class
    {
        if (_container == null)
            return null;

        return _container.ResolveOptional<T>();
    }
}
