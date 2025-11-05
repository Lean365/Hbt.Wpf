# Hbt.Wpf 项目说明文档

## 1. 项目简介

Hbt.Wpf 是基于 WPF（Windows Presentation Foundation）开发的企业级中后台管理系统，采用分层架构设计，支持多语言、多主题、RBAC权限管理等核心功能。

### 1.1 项目名称

- **中文名称**: 黑冰台WPF管理系统
- **英文名称**: Hbt.Wpf

### 1.2 项目特点

- ✅ 分层架构设计（Clean Architecture）
- ✅ MVVM模式
- ✅ 依赖注入（Autofac + DI）
- ✅ 多语言支持（国际化）
- ✅ 多主题支持（Light/Dark）
- ✅ RBAC权限管理
- ✅ MDI标签页管理（AvalonDock）
- ✅ 完整的日志系统（Serilog）
- ✅ Excel导入导出
- ✅ 代码优先（Code First）数据库设计

## 2. 技术栈

### 2.1 核心技术

- **.NET**: 9.0
- **框架**: WPF (Windows Presentation Foundation)
- **ORM**: SqlSugar 5.1.4
- **依赖注入**: Autofac 8.4.0, Microsoft.Extensions.DependencyInjection 9.0.10
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **UI组件**: AvalonDock 4.72.1
- **图标**: FontAwesome.Sharp 6.6.0
- **日志**: Serilog 4.3.0
- **映射**: Mapster 7.4.0
- **加密**: BouncyCastle.Cryptography 2.6.2
- **Excel**: EPPlus 8.2.1

### 2.2 开发工具

- **IDE**: Visual Studio 2025
- **.NET SDK**: 9.0
- **数据库**: SQL Server

## 3. 项目结构

```
Hbt.Wpf/
├── src/
│   ├── Hbt.Fluent/          # 表现层（WPF UI）
│   │   ├── Views/           # 视图（XAML）
│   │   ├── ViewModels/      # 视图模型
│   │   ├── Services/        # UI服务
│   │   ├── Helpers/         # UI辅助类
│   │   ├── Models/          # UI模型
│   │   ├── Localization/    # 本地化
│   │   └── Resources/       # 资源文件
│   ├── Hbt.Application/     # 应用层（业务逻辑）
│   │   ├── Services/        # 业务服务
│   │   └── Dtos/            # 数据传输对象
│   ├── Hbt.Domain/          # 领域层（领域模型）
│   │   ├── Entities/        # 实体
│   │   ├── Repositories/    # 仓储接口
│   │   └── Interfaces/      # 领域接口
│   ├── Hbt.Infrastructure/  # 基础设施层（数据访问）
│   │   ├── Data/            # 数据访问
│   │   ├── Repositories/    # 仓储实现
│   │   └── DependencyInjection/  # 依赖注入
│   └── Hbt.Common/          # 通用层（共享组件）
│       ├── Logging/         # 日志管理
│       ├── Config/          # 配置
│       ├── Helpers/         # 工具类
│       ├── Results/         # 结果封装
│       └── Security/        # 安全工具
├── docs/                    # 文档
├── scripts/                 # 脚本
└── logs/                    # 日志文件
```

## 4. 快速开始

### 4.1 环境要求

- Windows 10/11 或 Windows Server 2016+
- .NET 9.0 SDK
- Visual Studio 2025 或更高版本
- SQL Server 2019 或更高版本

### 4.2 克隆项目

```bash
git clone <repository-url>
cd Hbt.Wpf
```

### 4.3 配置数据库

1. 打开 `src/Hbt.Fluent/appsettings.json`
2. 修改数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=Hbt_Wpf_Dev;User Id=sa;Password=your-password;TrustServerCertificate=true;"
  },
  "DatabaseSettings": {
    "EnableCodeFirst": true,
    "EnableSeedData": true
  }
}
```

### 4.4 构建项目

```bash
dotnet build
```

### 4.5 运行项目

```bash
cd src/Hbt.Fluent
dotnet run
```

或在 Visual Studio 中按 `F5` 运行。

## 5. 配置说明

### 5.1 应用配置 (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "DatabaseSettings": {
    "EnableCodeFirst": false,
    "EnableSeedData": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=fs03;Database=Hbt_Wpf_Dev;User Id=sa;Password=Tac26901333.;TrustServerCertificate=true;"
  }
}
```

**配置项说明**:

- `Logging`: 日志级别配置
- `DatabaseSettings.EnableCodeFirst`: 是否启用代码优先（自动建表）
- `DatabaseSettings.EnableSeedData`: 是否启用种子数据
- `ConnectionStrings.DefaultConnection`: 数据库连接字符串

### 5.2 数据库配置

- **数据库类型**: SQL Server
- **连接字符串格式**: `Server=服务器;Database=数据库名;User Id=用户名;Password=密码;TrustServerCertificate=true;`
- **Code First**: 支持自动建表（通过 `DbTableInitializer`）
- **种子数据**: 支持自动初始化种子数据（RBAC、Routine模块）

## 6. 功能模块

### 6.1 身份认证模块 (Identity)

- **用户管理**: 用户CRUD、密码管理、状态管理、Excel导入导出
- **角色管理**: 角色CRUD、状态管理
- **菜单管理**: 菜单树管理、权限管理、菜单CRUD、Excel导入导出
- **登录认证**: 用户登录、登出、会话管理
- **权限控制**: RBAC权限模型、权限码控制

### 6.2 基础模块 (Routine)

- **语言管理**: 多语言支持、语言CRUD
- **翻译管理**: 翻译键值对管理、翻译查询
- **字典管理**: 字典类型、字典数据管理
- **系统设置**: 系统配置管理

### 6.3 物流模块 (Logistics)

- **物料管理**: 产品物料、产品型号管理
- **序列号管理**: 产品序列号、入库/出库记录
- **访客管理**: 访客信息、访客详情管理

### 6.4 日志模块 (Logging)

- **登录日志**: 用户登录记录
- **操作日志**: 业务操作记录
- **异常日志**: 系统异常记录
- **差异日志**: 数据变更记录

### 6.5 UI功能

- **MDI标签页**: 多文档界面，支持标签页创建、激活、关闭
- **菜单树**: 动态菜单树，支持多级菜单、图标显示、本地化
- **主题切换**: 支持明亮/暗黑主题切换
- **多语言**: 支持界面多语言切换

## 7. 开发规范

### 7.1 命名规范

#### 后端命名规范

- **类名**: 必须以 `Hbt` 开头，使用 PascalCase
- **接口**: 必须以 `IHbt` 开头，使用 PascalCase
- **方法**: 使用 PascalCase，异步方法以 `Async` 结尾
- **属性**: 使用 PascalCase
- **变量**: 使用 camelCase
- **常量**: 全大写，下划线分隔

#### 前端命名规范

- **组件名**: 使用 PascalCase
- **文件名**: 使用 kebab-case
- **变量名**: 使用 camelCase
- **常量**: 全大写，下划线分隔
- **CSS类名**: 使用 kebab-case

### 7.2 代码组织

遵循分层架构原则：

- **表现层**: 只负责UI展示和用户交互
- **应用层**: 负责业务逻辑编排
- **领域层**: 负责领域模型和业务规则
- **基础设施层**: 负责数据访问和外部服务
- **通用层**: 负责共享组件和工具

### 7.3 代码注释

所有类、方法、属性都应该添加 XML 注释：

```csharp
/// <summary>
/// 类描述
/// </summary>
/// <remarks>
/// 详细说明
/// </remarks>
public class Example
{
    /// <summary>
    /// 属性描述
    /// </summary>
    public string Property { get; set; }

    /// <summary>
    /// 方法描述
    /// </summary>
    /// <param name="param">参数描述</param>
    /// <returns>返回值描述</returns>
    public async Task<string> MethodAsync(string param)
    {
        // 实现代码
    }
}
```

### 7.4 编码规范

- 使用异步编程模式（`async/await`）
- 使用依赖注入
- 统一异常处理（`Result<T>`）
- 统一日志记录（`OperLogManager`）
- 遵循 SOLID 原则
- 使用设计模式解决常见问题

## 8. 数据库设计

### 8.1 实体命名规范

- 表名: `hbt_模块名_实体名`（如：`hbt_oidc_user`）
- 字段名: 小写，下划线分隔（如：`user_name`）
- 主键: `id`（bigint，雪花ID）
- 审计字段: `created_by`, `created_time`, `updated_by`, `updated_time`, `is_deleted`, `deleted_by`, `deleted_time`

### 8.2 实体类型

#### Identity 模块

- `hbt_oidc_user`: 用户表
- `hbt_oidc_role`: 角色表
- `hbt_oidc_menu`: 菜单表
- `hbt_oidc_user_role`: 用户角色关联表
- `hbt_oidc_role_menu`: 角色菜单关联表
- `hbt_oidc_user_session`: 用户会话表

#### Routine 模块

- `hbt_routine_language`: 语言表
- `hbt_routine_translation`: 翻译表
- `hbt_routine_dictionary_type`: 字典类型表
- `hbt_routine_dictionary_data`: 字典数据表
- `hbt_routine_setting`: 系统设置表

#### Logging 模块

- `hbt_logging_login_log`: 登录日志表
- `hbt_logging_operation_log`: 操作日志表
- `hbt_logging_exception_log`: 异常日志表
- `hbt_logging_diff_log`: 差异日志表

#### Logistics 模块

- `hbt_logistics_prod_material`: 产品物料表
- `hbt_logistics_prod_model`: 产品型号表
- `hbt_logistics_prod_serial`: 产品序列号表
- `hbt_logistics_prod_serial_inbound`: 序列号入库表
- `hbt_logistics_prod_serial_outbound`: 序列号出库表
- `hbt_logistics_visitor`: 访客表
- `hbt_logistics_visitor_detail`: 访客详情表

## 9. 日志系统

### 9.1 日志文件位置

- **应用日志**: `logs/app-YYYY-MM-DD.txt`
- **初始化日志**: 通过 `InitLogManager` 记录
- **操作日志**: 通过 `OperLogManager` 记录

### 9.2 日志级别

- **Debug**: 调试信息
- **Information**: 一般信息
- **Warning**: 警告
- **Error**: 错误
- **Fatal**: 致命错误

### 9.3 日志记录示例

```csharp
var operLog = App.Services?.GetService<OperLogManager>();
operLog?.Information("用户 {Username} 执行了 {Action}", username, action);
operLog?.Error(ex, "操作失败: {Message}", message);
```

## 10. 常见问题

### 10.1 数据库连接失败

**问题**: 无法连接到数据库

**解决方案**:
1. 检查 `appsettings.json` 中的连接字符串
2. 确认 SQL Server 服务已启动
3. 检查防火墙设置
4. 确认数据库用户权限

### 10.2 标签页标题空白

**问题**: AvalonDock 标签页标题显示空白

**解决方案**:
1. 检查菜单项的 `I18nKey` 是否正确设置
2. 检查翻译表中是否有对应的翻译
3. 检查 `LanguageService` 是否正常工作
4. 查看日志文件排查问题

### 10.3 菜单树不展开

**问题**: 子菜单目录点击后不展开/收缩

**解决方案**:
1. 检查 `TreeViewItem` 的 `IsExpanded` 属性
2. 检查 `VisualTreeHelper` 是否正确找到 `TreeViewItem`
3. 查看 `MainWindow.xaml.cs` 中的菜单处理逻辑

## 11. 开发指南

### 11.1 添加新模块

1. 在 `Hbt.Domain/Entities` 中创建实体
2. 在 `Hbt.Application/Services` 中创建服务接口和实现
3. 在 `Hbt.Application/Dtos` 中创建DTO
4. 在 `Hbt.Fluent/Views` 中创建视图
5. 在 `Hbt.Fluent/ViewModels` 中创建视图模型
6. 在 `AutofacModule` 中注册服务（自动注册）
7. 在 `App.xaml.cs` 中注册 ViewModel 和 View

### 11.2 添加新菜单

1. 在数据库 `hbt_oidc_menu` 表中添加菜单记录
2. 设置 `I18nKey` 用于本地化
3. 设置 `Icon` 用于图标显示
4. 设置 `ViewTypeName` 用于视图类型
5. 在 `Translation` 表中添加翻译

### 11.3 添加新语言

1. 在 `Language` 表中添加语言记录
2. 在 `Translation` 表中添加对应语言的翻译
3. 在 `LanguageService` 中切换语言

## 12. 版本信息

- **当前版本**: 1.0
- **最后更新**: 2025-01-03
- **.NET版本**: 9.0
- **目标框架**: net9.0-windows

## 13. 许可证

MIT License

## 14. 贡献指南

欢迎提交 Issue 和 Pull Request。

## 15. 联系方式

- **项目维护者**: 黑冰台
- **邮箱**: [待补充]
- **项目地址**: [待补充]

## 16. 更新日志

### v1.0 (2025-01-03)

- ✅ 初始版本发布
- ✅ 完成身份认证模块
- ✅ 完成基础模块（Routine）
- ✅ 完成物流模块（Logistics）
- ✅ 完成日志模块
- ✅ 完成MDI标签页管理
- ✅ 完成多语言支持
- ✅ 完成多主题支持
- ✅ 完成Excel导入导出

## 17. 相关文档

- [设计文档](./Design.md)
- [接口文档](./Interface.md)

