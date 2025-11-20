# 节拍(Takt)中小企业管理平台 · Takt SMEs Platform 项目说明文档

## 1. 项目简介

节拍(Takt)中小企业管理平台（Takt SMEs Platform）是基于 WPF（Windows Presentation Foundation）开发的企业级中后台管理系统，采用分层架构设计，支持多语言、多主题、RBAC 权限管理等核心功能。

### 1.1 项目名称

- **中文名称**: 节拍(Takt)中小企业管理平台
- **英文名称**: Takt SMEs Platform

### 1.2 项目特点

- ✅ 分层架构设计（Clean Architecture）
- ✅ MVVM模式
- ✅ 依赖注入（Autofac + DI）
- ✅ 多语言支持（国际化）
- ✅ 多主题支持（Light/Dark）
- ✅ RBAC权限管理
- ✅ 多标签页工作区
- ✅ 完整的日志系统（Serilog）
- ✅ Excel导入导出
- ✅ 代码优先（Code First）数据库设计
- ✅ 代码生成器（基于数据库表自动生成CRUD代码）
- ✅ 日志自动清理（每月1号0点自动清理，保留最近7天）
- ✅ 符合 Windows 规范的路径管理（AppData目录）

## 2. 技术栈

### 2.1 核心技术

- **.NET**: 9.0
- **框架**: WPF (Windows Presentation Foundation)
- **UI**: MaterialDesignThemes 5.3.0、MaterialDesignColors 5.3.0、FontAwesome.Sharp 6.6.0
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **依赖注入**: Autofac 8.4.0、Autofac.Extensions.DependencyInjection 10.0.0、Microsoft.Extensions.DependencyInjection 9.0.10
- **配置**: Microsoft.Extensions.Configuration 9.0.10、Microsoft.Extensions.Configuration.Json 9.0.10、Microsoft.Extensions.Hosting 9.0.10
- **ORM**: SqlSugarCore 5.1.4.208
- **日志**: Serilog 4.3.0、Serilog.Extensions.Hosting 9.0.0、Serilog.Settings.Configuration 9.0.0、Serilog.Sinks.Console 6.1.1、Serilog.Sinks.File 7.0.0
- **映射**: Mapster 7.4.0
- **模板引擎**: Scriban 6.5.1
- **加密**: BouncyCastle.Cryptography 2.6.2
- **Excel**: EPPlus 8.2.1
- **JSON**: Newtonsoft.Json 13.0.4
- **硬件信息**: Hardware.Info 101.1.0.1

### 2.2 开发工具

- **IDE**: Visual Studio 2025
- **.NET SDK**: 9.0
- **数据库**: SQL Server

## 3. 项目结构

```
Takt.Wpf/
├── src/
│   ├── Takt.Fluent/         # 表现层（WPF UI）
│   │   ├── Views/           # 视图（XAML）
│   │   ├── ViewModels/      # 视图模型
│   │   ├── Services/        # UI服务
│   │   ├── Helpers/         # UI辅助类
│   │   ├── Models/          # UI模型
│   │   ├── Localization/    # 本地化
│   │   └── Resources/       # 资源文件
│   ├── Takt.Application/    # 应用层（业务逻辑）
│   │   ├── Services/        # 业务服务
│   │   └── Dtos/            # 数据传输对象
│   ├── Takt.Domain/         # 领域层（领域模型）
│   │   ├── Entities/        # 实体
│   │   ├── Repositories/    # 仓储接口
│   │   └── Interfaces/      # 领域接口
│   ├── Takt.Infrastructure/ # 基础设施层（数据访问）
│   │   ├── Data/            # 数据访问
│   │   ├── Repositories/    # 仓储实现
│   │   └── DependencyInjection/  # 依赖注入
│   └── Takt.Common/         # 通用层（共享组件）
│       ├── Logging/         # 日志管理
│       ├── Config/          # 配置
│       ├── Helpers/         # 工具类
│       ├── Results/         # 结果封装
│       └── Security/        # 安全工具
├── docs/                    # 文档
├── scripts/                 # 脚本
└── .cursor/                 # Cursor AI 规则配置
    └── rules/               # 项目规范文档
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
cd "Takt.Wpf"
```

### 4.3 配置数据库

1. 打开 `src/Takt.Fluent/appsettings.json`
2. 修改数据库连接字符串中的服务器地址、数据库名、用户名和密码
3. 详细配置说明请参考 [5. 配置说明](#5-配置说明) 章节

### 4.4 构建项目

```bash
dotnet build
```

### 4.5 运行项目

```bash
cd src/Takt.Fluent
dotnet run
```

或在 Visual Studio 中按 `F5` 运行。

### 4.6 构建安装包

项目支持 MSIX 打包，可以生成可安装程序包。构建过程会将所有输出文件生成到项目根目录下的 `publish/` 目录。

**构建方式**：

**方式一：使用批处理脚本（推荐）**
```bash
.\scripts\build-installer.bat
```

**方式二：使用 PowerShell 脚本**
```powershell
.\scripts\build-installer.ps1 -Configuration Release
```

**方式三：使用 dotnet CLI**
```bash
dotnet publish src/Takt.Fluent/Takt.Fluent.csproj `
    --configuration Release `
    --output ./publish `
    --runtime win-x64 `
    -p:WindowsPackageType=MSIX
```

**方式四：使用 Visual Studio**
1. 右键点击 `Takt.Fluent` 项目
2. 选择"发布"（Publish）
3. 选择"MSIX" 或"MSIX 包"
4. 配置发布设置并发布

**构建输出**：
- 构建完成后，所有输出文件（包括 MSIX 安装包、DLL、配置文件等）将生成到 `publish/` 目录
- MSIX 安装包文件通常位于 `publish/` 目录下，文件名为 `Takt.Fluent_*.msix` 或类似格式

**重要说明**：
- `publish/` 目录是构建输出目录，包含编译后的二进制文件和安装包
- 该目录已配置在 `.gitignore` 中，不会被 Git 跟踪，同步到远程仓库时会被自动排除
- 每次构建都会重新生成该目录内容，无需提交到版本控制系统
- 如需分发应用，只需将 `publish/` 目录中的 MSIX 安装包文件分发给用户即可

详细说明请参考 [安装包构建指南](./docs/INSTALLER.md)。

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
    "DefaultConnection": "Server=localhost;Database=Takt_Wpf_Dev;User Id=sa;Password=YourPassword;TrustServerCertificate=true;MultipleActiveResultSets=true;"
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

### 6.3 后勤模块 (Logistics)

- **物料管理**: 产品物料、产品型号管理
- **序列号管理**: 产品序列号、入库/出库记录
- **访客管理**: 访客信息、访客详情管理

### 6.4 日志模块 (Logging)

- **登录日志**: 用户登录记录
- **操作日志**: 业务操作记录
- **异常日志**: 系统异常记录
- **差异日志**: 数据变更记录
- **日志清理**: 自动清理过期日志（每月1号0点执行，保留最近7天）

### 6.5 代码生成模块 (Generator)

- **表配置管理**: 代码生成表配置（GenTable）的CRUD操作
- **列配置管理**: 代码生成列配置（GenColumn）的CRUD操作
- **表结构导入**: 从数据库表自动导入表结构和列信息
- **代码生成**: 基于配置自动生成实体、DTO、Service、ViewModel、View等代码
- **模板管理**: 支持CRUD、MasterDetail、Tree三种模板类型
- **自定义模板**: 支持用户自定义代码生成模板（存储在AppData\Roaming）

### 6.6 UI功能

- **MDI标签页**: 多文档界面，支持标签页创建、激活、关闭
- **菜单树**: 动态菜单树，支持多级菜单、图标显示、本地化
- **主题切换**: 支持明亮/暗黑主题切换
- **多语言**: 支持界面多语言切换

## 7. 开发规范

### 7.1 命名规范

#### 后端命名规范

- **类名**: 必须以 `Takt` 开头，使用 PascalCase
- **接口**: 必须以 `ITakt` 开头，使用 PascalCase
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

- 表名: `takt_模块名_实体名`（如：`takt_oidc_user`）
- 字段名: 小写，下划线分隔（如：`user_name`）
- 主键: `id`（bigint，雪花ID）
- 审计字段: `created_by`, `created_time`, `updated_by`, `updated_time`, `is_deleted`, `deleted_by`, `deleted_time`

### 8.2 实体类型

#### Identity 模块

- `takt_oidc_user`: 用户表
- `takt_oidc_role`: 角色表
- `takt_oidc_menu`: 菜单表
- `takt_oidc_user_role`: 用户角色关联表
- `takt_oidc_role_menu`: 角色菜单关联表
- `takt_oidc_user_session`: 用户会话表

#### Routine 模块

- `takt_routine_language`: 语言表
- `takt_routine_translation`: 翻译表
- `takt_routine_dictionary_type`: 字典类型表
- `takt_routine_dictionary_data`: 字典数据表
- `takt_routine_setting`: 系统设置表

#### Logging 模块

- `takt_logging_login_log`: 登录日志表
- `takt_logging_operation_log`: 操作日志表
- `takt_logging_exception_log`: 异常日志表
- `takt_logging_diff_log`: 差异日志表

#### Logistics 模块

- `takt_logistics_prod_material`: 产品物料表
- `takt_logistics_prod_model`: 产品型号表
- `takt_logistics_prod_serial`: 产品序列号表
- `takt_logistics_prod_serial_inbound`: 序列号入库表
- `takt_logistics_prod_serial_outbound`: 序列号出库表
- `takt_logistics_visitor`: 访客表
- `takt_logistics_visitor_detail`: 访客详情表

#### Generator 模块

- `takt_generator_table`: 代码生成表配置表
- `takt_generator_column`: 代码生成列配置表

## 9. 日志系统

### 9.1 日志文件位置

所有日志文件存储在符合 Windows 规范的 `AppData\Local` 目录下：

- **应用日志**: `C:\Users\{UserName}\AppData\Local\Takt\TaktSMEsPlatform\Logs\app-YYYYMMDD.txt`
- **操作日志**: `C:\Users\{UserName}\AppData\Local\Takt\TaktSMEsPlatform\Logs\oper-YYYYMMDD.txt`
- **初始化日志**: `C:\Users\{UserName}\AppData\Local\Takt\TaktSMEsPlatform\Logs\init-YYYYMMDD.txt`

**注意**: 日志目录使用 `PathHelper.GetLogDirectory()` 获取，符合 Windows 应用规范。

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

### 9.4 日志自动清理

系统实现了自动日志清理功能：

- **清理时间**: 每月1号0点自动执行
- **保留天数**: 保留最近7天的日志
- **清理范围**: 
  - 文本日志文件（app-*.txt, oper-*.txt, init-*.txt）
  - 数据库日志记录（OperationLog, LoginLog, DiffLog, ExceptionLog）
- **实现方式**: 通过 `LogCleanupBackgroundService` 后台服务实现

## 10. 常见问题

### 10.1 数据库连接失败

**问题**: 无法连接到数据库

**解决方案**:
1. 检查 `appsettings.json` 中的连接字符串
2. 确认 SQL Server 服务已启动
3. 检查防火墙设置
4. 确认数据库用户权限

### 10.2 仪表盘欢迎语不刷新

**问题**: 登录后仪表盘顶部欢迎语仍显示默认文案

**解决方案**:
1. 确认 `DashboardViewModel.UpdateGreeting()` 是否被调用
2. 检查 `LanguageService` 翻译缓存是否加载最新键值
3. 核对 `UserContext` 中是否存在当前登录用户信息
4. 查看应用日志确保没有异常输出

### 10.3 菜单树不展开

**问题**: 子菜单目录点击后不展开/收缩

**解决方案**:
1. 检查 `TreeViewItem` 的 `IsExpanded` 属性
2. 检查 `VisualTreeHelper` 是否正确找到 `TreeViewItem`
3. 查看 `MainWindow.xaml.cs` 中的菜单处理逻辑

## 11. 开发指南

### 11.1 添加新模块

1. 在 `Takt.Domain/Entities` 中创建实体
2. 在 `Takt.Application/Services` 中创建服务接口和实现
3. 在 `Takt.Application/Dtos` 中创建 DTO
4. 在 `Takt.Fluent/Views` 中创建视图
5. 在 `Takt.Fluent/ViewModels` 中创建视图模型
6. 在 `AutofacModule` 中注册服务（自动注册）
7. 在 `App.xaml.cs` 中注册 ViewModel 和 View

### 11.2 添加新菜单

1. 在数据库 `takt_oidc_menu` 表中添加菜单记录
2. 设置 `I18nKey` 用于本地化
3. 设置 `Icon` 用于图标显示
4. 设置 `ViewTypeName` 用于视图类型
5. 在 `Translation` 表中添加翻译

### 11.3 添加新语言

1. 在 `Language` 表中添加语言记录
2. 在 `Translation` 表中添加对应语言的翻译
3. 在 `LanguageService` 中切换语言

## 12. 路径管理规范

### 12.1 路径辅助类 (PathHelper)

项目使用 `Takt.Common.Helpers.PathHelper` 统一管理所有应用程序路径，确保符合 Windows 应用规范。

### 12.2 日志路径

- **位置**: `AppData\Local\Takt\TaktSMEsPlatform\Logs`
- **获取方法**: `PathHelper.GetLogDirectory()`
- **用途**: 存储应用日志、操作日志、初始化日志
- **特点**: 本地存储，不会在域网络中漫游

### 12.3 配置路径

#### 用户漫游配置 (AppData\Roaming)

- **位置**: `AppData\Roaming\Takt\TaktSMEsPlatform`
- **获取方法**: `PathHelper.GetRoamingConfigDirectory()`
- **用途**: 存储用户个性化配置（主题、语言、窗口位置等）
- **特点**: 会跟随用户账户在域网络中漫游
- **配置文件**: `taktsettings.json`（用户设置）

#### 用户本地配置 (AppData\Local)

- **位置**: `AppData\Local\Takt\TaktSMEsPlatform`
- **获取方法**: `PathHelper.GetLocalConfigDirectory()`
- **用途**: 存储本地缓存配置、临时设置等
- **特点**: 不会在域网络中漫游

### 12.4 代码模板路径

#### 默认模板目录

- **位置**: `{AppDirectory}\Assets\Generator`
- **获取方法**: `PathHelper.GetDefaultTemplateDirectory()`
- **用途**: 存储应用自带的代码生成模板（只读）
- **特点**: 应用更新时会被覆盖

#### 用户自定义模板目录

- **位置**: `AppData\Roaming\Takt\TaktSMEsPlatform\Templates`
- **获取方法**: `PathHelper.GetUserTemplateDirectory()`
- **用途**: 存储用户自定义的代码生成模板
- **特点**: 会跟随用户账户漫游，用户可编辑

#### 智能模板查找

系统会按以下优先级查找模板：

1. 用户自定义的特定类型模板（如 `Entity_CRUD.sbn`）
2. 用户自定义的通用模板（如 `Entity.sbn`）
3. 默认模板目录的特定类型模板
4. 默认模板目录的通用模板

**获取方法**: `PathHelper.GetTemplateFilePath(templateFileName, templateType)`

### 12.5 路径使用示例

```csharp
using Takt.Common.Helpers;

// 获取日志目录
var logDir = PathHelper.GetLogDirectory();

// 获取用户配置目录
var configDir = PathHelper.GetRoamingConfigDirectory();

// 获取配置文件路径
var configFile = PathHelper.GetRoamingConfigFilePath("taktsettings.json");

// 获取模板文件路径（智能查找）
var templatePath = PathHelper.GetTemplateFilePath("Entity.sbn", "CRUD");
```

## 13. 版本信息

- **当前版本**: 0.0.2
- **最后更新**: 2025-01-20
- **.NET版本**: 9.0
- **目标框架**: net9.0-windows

## 14. 许可证

MIT License

## 15. 贡献指南

欢迎提交 Issue 和 Pull Request。

## 16. 联系方式

- **项目维护者**: Takt365(Cursor AI)
- **邮箱**: [待补充]
- **项目地址**: [待补充]

## 17. 更新日志

### v0.0.2 (2025-01-20)

- ✅ 新增代码生成模块（Generator）
  - 表配置管理（GenTable）
  - 列配置管理（GenColumn）
  - 从数据库表导入表结构
  - 基于配置自动生成代码（Entity、DTO、Service、ViewModel、View等）
  - 支持CRUD、MasterDetail、Tree三种模板类型
- ✅ 实现日志自动清理功能
  - 每月1号0点自动执行
  - 保留最近7天的日志（文本日志和数据库日志）
  - 并行清理，提高效率
- ✅ 路径管理规范化
  - 统一使用 `PathHelper` 管理所有路径
  - 日志目录迁移到 `AppData\Local`
  - 配置文件迁移到 `AppData\Roaming`
  - 支持用户自定义代码生成模板
- ✅ 优化代码生成表单
  - 列顺序规范化
  - ComboBox联动功能（ColumnDataType联动PropertyName等字段）
  - 新增IsDelete字段支持
  - 字段描述简化

### v0.0.1 (2025-01-03)

- ✅ 初始版本发布
- ✅ 完成身份认证模块
- ✅ 完成基础模块（Routine）
- ✅ 完成后勤模块（Logistics）
- ✅ 完成日志模块
- ✅ 完成MDI标签页管理
- ✅ 完成多语言支持
- ✅ 完成多主题支持
- ✅ 完成Excel导入导出

## 18. 相关文档

- [设计文档](./Design.md)
- [接口文档](./Interface.md)
- [安装包构建指南](./docs/INSTALLER.md)
- [架构规范](.cursor/rules/architecture.mdc)
- [命名规范](.cursor/rules/naming.mdc)
- [MVVM规范](.cursor/rules/mvvm.mdc)
- [依赖注入规范](.cursor/rules/dependency-injection.mdc)
- [异步编程规范](.cursor/rules/async.mdc)

