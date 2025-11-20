# WPF 依赖注入注册逻辑指南

## 当前架构

项目使用 **Autofac + Microsoft.Extensions.DependencyInjection** 混合模式：

- **Autofac**：注册应用层服务、基础设施层服务（通过 `AutofacModule`）
- **Microsoft.Extensions.DependencyInjection**：注册表现层组件（View、ViewModel）

## 注册层次结构

```
App.xaml.cs (ConfigureServices)
├── 表现层服务（Singleton）
│   ├── ThemeService
│   └── LocalizationAdapter
├── 窗口（Singleton）
│   ├── LoginWindow
│   └── MainWindow
└── ViewModel/View（Transient）
    ├── LoginViewModel
    ├── MainWindowViewModel
    └── 其他业务 ViewModel/View

AutofacModule (ConfigureContainer)
├── 基础设施层（SingleInstance）
│   ├── DbContext
│   ├── DbTableInitializer
│   └── 日志管理器
├── 应用层服务（InstancePerLifetimeScope）
│   ├── IUserService -> UserService
│   ├── ILoginService -> LoginService
│   └── 其他 *Service
└── 领域层接口实现（SingleInstance）
    └── ILocalizationManager -> LocalizationManager
```

## 生命周期选择原则

### Singleton（单例）
**适用场景**：
- 全局共享的服务（如 `ThemeService`、`LocalizationAdapter`）
- 主窗口（`MainWindow`、`LoginWindow`）- 应用生命周期内只有一个实例
- 配置服务、日志管理器

**示例**：
```csharp
services.AddSingleton<ThemeService>();
services.AddSingleton<MainWindow>();
services.AddSingleton<MainWindowViewModel>();
```

### Transient（瞬态）
**适用场景**：
- ViewModel 和 View（每次创建新实例）
- 表单 ViewModel（每次打开表单都需要新实例）
- 业务组件（用户管理、角色管理等）

**示例**：
```csharp
services.AddTransient<UserViewModel>();
services.AddTransient<UserView>();
services.AddTransient<UserFormViewModel>();
```

### InstancePerLifetimeScope（作用域）
**适用场景**：
- 应用层服务（`IUserService`、`ILoginService` 等）
- 仓储（Repository）
- 数据库上下文相关的服务

**注意**：在 Autofac 中使用 `InstancePerLifetimeScope`，在 DI 容器中相当于 `Scoped`

## 正确的注册顺序

### 1. 基础设施层服务（AutofacModule）

```csharp
// @src/Takt.Infrastructure/DependencyInjection/AutofacModule.cs
protected override void Load(ContainerBuilder builder)
{
    // 1. 数据库上下文（SingleInstance）
    builder.Register(c => new DbContext(_connectionString, logger, _databaseSettings))
        .AsSelf()
        .SingleInstance();

    // 2. 日志管理器（SingleInstance）
    builder.RegisterType<OperLogManager>()
        .AsSelf()
        .SingleInstance();

    // 3. 仓储（InstancePerLifetimeScope）
    builder.RegisterGeneric(typeof(BaseRepository<>))
        .As(typeof(IBaseRepository<>))
        .InstancePerLifetimeScope();

    // 4. 应用层服务（InstancePerLifetimeScope）
    builder.RegisterAssemblyTypes(typeof(IUserService).Assembly)
        .Where(t => t.Name.EndsWith("Service"))
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();

    // 5. 领域层接口实现（SingleInstance）
    builder.RegisterType<LocalizationManager>()
        .As<ILocalizationManager>()
        .SingleInstance();
}
```

### 2. 表现层服务（App.xaml.cs - ConfigureServices）

```csharp
// @src/Takt.Fluent/App.xaml.cs
.ConfigureServices((context, services) =>
{
    // ========== 1. 全局服务（Singleton）==========
    services.AddSingleton<ThemeService>();
    
    services.AddSingleton<LocalizationAdapter>(sp =>
    {
        var localizationManager = sp.GetRequiredService<ILocalizationManager>();
        return new LocalizationAdapter(localizationManager);
    });

    // ========== 2. 主窗口和主窗口 ViewModel（Singleton）==========
    services.AddSingleton<MainWindowViewModel>();
    services.AddSingleton<MainWindow>();

    // ========== 3. 登录窗口和登录 ViewModel ==========
    services.AddSingleton<LoginWindow>();
    services.AddTransient<LoginViewModel>(); // 每次登录可能需要新实例

    // ========== 4. 业务 ViewModel 和 View（Transient）==========
    // 用户管理
    services.AddTransient<UserViewModel>();
    services.AddTransient<UserView>();
    services.AddTransient<UserFormViewModel>();
    services.AddTransient<UserForm>();

    // 角色管理
    services.AddTransient<RoleViewModel>();
    services.AddTransient<RoleView>();
    // ... 其他业务组件
})
```

## 常见问题和解决方案

### 问题1：循环依赖

**症状**：`CircularDependencyException`

**原因**：ViewModel A 依赖 ViewModel B，ViewModel B 依赖 ViewModel A

**解决**：
- 使用 `IServiceProvider` 延迟解析（当前项目已采用）
- 重构依赖关系，提取公共逻辑到服务层

**示例**（当前项目做法）：
```csharp
// @src/Takt.Fluent/ViewModels/Identity/UserViewModel.cs:33
private readonly IServiceProvider _serviceProvider;

// 延迟解析，避免循环依赖
var formViewModel = _serviceProvider.GetRequiredService<UserFormViewModel>();
```

### 问题2：服务未注册

**症状**：`InvalidOperationException: No service for type 'X' has been registered`

**原因**：
- 忘记注册服务
- 注册顺序错误（依赖的服务未先注册）
- 接口和实现不匹配

**解决**：
1. 检查 `AutofacModule` 中是否注册了应用层服务
2. 检查 `App.xaml.cs` 中是否注册了 ViewModel/View
3. 确保接口和实现类型匹配

### 问题3：生命周期不匹配

**症状**：服务状态异常、数据不同步

**原因**：
- Singleton 服务中注入了 Transient 依赖
- Transient 服务中注入了 Scoped 依赖（在 Autofac 中为 InstancePerLifetimeScope）

**解决**：
- **规则**：依赖的生命周期不能短于被依赖的服务
- Singleton 可以依赖 Singleton
- Transient 可以依赖 Singleton、Transient
- InstancePerLifetimeScope 可以依赖 Singleton、InstancePerLifetimeScope

### 问题4：IServiceProvider 在 View 中使用

**症状**：在 View 的代码后台直接使用 `App.Services`

**当前做法**（不推荐，但可接受）：
```csharp
// @src/Takt.Fluent/Views/MainWindow.xaml.cs:31
_themeService = App.Services.GetRequiredService<ThemeService>();
```

**推荐做法**：
```csharp
// View 构造函数注入
public MainWindow(MainWindowViewModel viewModel, ThemeService themeService)
{
    ViewModel = viewModel;
    _themeService = themeService;
    // ...
}
```

## 最佳实践

### ✅ 推荐做法

1. **构造函数注入**（优先）
   ```csharp
   public class UserViewModel : ObservableObject
   {
       private readonly IUserService _userService;
       
       public UserViewModel(IUserService userService)
       {
           _userService = userService ?? throw new ArgumentNullException(nameof(userService));
       }
   }
   ```

2. **使用接口注册**
   ```csharp
   // AutofacModule 中自动注册
   builder.RegisterAssemblyTypes(typeof(IUserService).Assembly)
       .Where(t => t.Name.EndsWith("Service"))
       .AsImplementedInterfaces()
       .InstancePerLifetimeScope();
   ```

3. **生命周期明确**
   - 全局服务：Singleton
   - 业务服务：InstancePerLifetimeScope
   - ViewModel/View：Transient

### ❌ 避免做法

1. **Service Locator 反模式**（除非必要）
   ```csharp
   // ❌ 不推荐
   var service = App.Services.GetService<IService>();
   
   // ✅ 推荐：构造函数注入
   public MyViewModel(IService service) { }
   ```

2. **在 View 中直接调用服务**
   ```csharp
   // ❌ 不推荐
   private void Button_Click(object sender, RoutedEventArgs e)
   {
       var service = App.Services.GetService<IUserService>();
       service.GetUsersAsync();
   }
   
   // ✅ 推荐：通过 ViewModel 调用
   private void Button_Click(object sender, RoutedEventArgs e)
   {
       ViewModel.LoadUsersCommand.Execute(null);
   }
   ```

3. **混合使用不同的 DI 容器**
   - 当前项目使用 Autofac + MS DI 混合模式是合理的
   - 但应明确分工：Autofac 负责业务层，MS DI 负责表现层

## 验证注册逻辑

### 检查清单

- [ ] 所有应用层服务接口都已注册实现
- [ ] 所有 ViewModel 都已注册
- [ ] 所有 View 都已注册
- [ ] 生命周期选择正确（Singleton/Transient/Scoped）
- [ ] 没有循环依赖
- [ ] 依赖的服务都已先注册

### 调试技巧

1. **启动时验证**
   ```csharp
   // 在 App.xaml.cs 的 InitializeApplicationAsync 中
   var userService = _host.Services.GetRequiredService<IUserService>();
   // 如果未注册，会抛出异常
   ```

2. **日志记录**
   ```csharp
   // 记录已注册的服务
   var services = _host.Services.GetServices<IUserService>();
   operLog?.Information("已注册 IUserService 实现: {Count}", services.Count());
   ```

## 参考文件

- **注册入口**：`@src/Takt.Fluent/App.xaml.cs:42-125`
- **Autofac 模块**：`@src/Takt.Infrastructure/DependencyInjection/AutofacModule.cs`
- **ViewModel 示例**：`@src/Takt.Fluent/ViewModels/Identity/UserViewModel.cs:81-95`
- **View 示例**：`@src/Takt.Fluent/Views/MainWindow.xaml.cs:21-46`

