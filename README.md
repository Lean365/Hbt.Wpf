<div align="center">

<img src="https://github.com/Lean365/Takt.Wpf/raw/master/src/Takt.Fluent/Assets/takt.png" width="64" alt="Takt Logo">

# Takt SMEs Platform

基于 WPF 开发的企业级中后台管理系统，采用分层架构设计，支持多语言、多主题、RBAC 权限管理等核心功能。

> ⚠️ **重要说明**: 本项目使用 Cursor AI 辅助开发完成，**不接受任何 Issues 提交**。

</div>

## 技术栈

### 核心框架
- **.NET 9.0** + **WPF** - 现代化桌面应用框架
- **Prism** (9.0.537) - 模块化 MVVM 框架，支持 Region 管理和导航
- **CommunityToolkit.Mvvm** (8.4.0) - 现代化 MVVM 工具包

### UI 组件
- **MaterialDesignThemes** (5.3.0) - Material Design 风格 UI 组件库
- **FontAwesome.Sharp** (6.6.0) - 图标库

### 架构模式
- **Clean Architecture** - 分层架构设计
- **MVVM Pattern** - Model-View-ViewModel 模式

### 依赖注入
- **Autofac** (8.4.0) - 高性能 IoC 容器
- **Microsoft.Extensions.DependencyInjection** (9.0.10) - 微软官方 DI 容器

### 数据访问
- **SqlSugar** - 轻量级 ORM 框架

### 日志系统
- **Serilog** (4.3.0) - 结构化日志框架
- **Serilog.Sinks.Console** (6.1.1) - 控制台输出
- **Serilog.Sinks.File** (7.0.0) - 文件输出

### 模板引擎
- **Scriban** (6.5.2) - 高性能模板引擎，用于代码生成

### JSON 处理
- **Newtonsoft.Json** (13.0.4) - JSON 序列化/反序列化

### 媒体播放
- **LibVLCSharp.WPF** (3.9.4) - VLC 媒体播放器 WPF 集成
- **VideoLAN.LibVLC.Windows** (3.0.21) - VLC 核心库

### 其他工具
- **Quartz** (3.15.1) - 任务调度框架
- **Mapster** (7.4.0) - 高性能对象映射

## 快速开始

### 环境要求

- Windows 10/11
- .NET 9.0 SDK
- SQL Server 2019+

### 安装步骤

```bash
# 克隆项目
git clone https://github.com/Lean365/Takt.Wpf.git
cd Takt.Wpf

# 配置数据库（编辑 appsettings.json）
# 修改 ConnectionStrings.DefaultConnection

# 构建项目
dotnet build

# 运行
cd src/Takt.Fluent
dotnet run
```

### 构建安装包

```bash
# 方式一：批处理脚本
.\scripts\build-installer.bat

# 方式二：PowerShell
.\scripts\build-installer.ps1 -Configuration Release

# 方式三：dotnet CLI
dotnet publish src/Takt.Fluent/Takt.Fluent.csproj `
    --configuration Release `
    --output ./publish `
    --runtime win-x64 `
    -p:WindowsPackageType=MSIX
```

输出文件位于 `publish/` 目录。

## 配置说明

编辑 `src/Takt.Fluent/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Takt_Wpf_Dev;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
  },
  "DatabaseSettings": {
    "EnableCodeFirst": false,
    "EnableSeedData": false
  }
}
```

## 功能模块

### 身份认证 (Identity)
- 用户管理：CRUD、密码管理、状态管理
- 角色管理：角色配置、权限分配
- 菜单管理：动态菜单树、权限控制
- RBAC 权限模型

### 基础模块 (Routine)
- 多语言管理：中文、英文、日文
- 翻译管理：翻译键值对
- 字典管理：系统字典
- 系统设置：应用配置

### 后勤模块 (Logistics)
- 物料管理：产品物料、型号
- 序列号管理：入库/出库记录
- 访客管理：访客信息管理

### 日志模块 (Logging)
- 登录日志、操作日志、差异日志
- 自动清理：每月1号0点执行，保留最近7天

### 代码生成 (Generator)
- 表配置管理：从数据库导入表结构
- 代码生成：基于模板自动生成 Entity、DTO、Service、ViewModel、View
- 支持 CRUD、MasterDetail、Tree 模板类型

## 项目结构

```
Takt.Wpf/
├── src/
│   ├── Takt.Fluent/         # 表现层（WPF UI）
│   ├── Takt.Application/    # 应用层（业务逻辑）
│   ├── Takt.Domain/         # 领域层（领域模型）
│   ├── Takt.Infrastructure/ # 基础设施层（数据访问）
│   └── Takt.Common/         # 通用层（共享组件）
├── docs/                    # 文档
└── scripts/                 # 构建脚本
```

## 开发规范

### 命名规范

- **类名**: 以 `Takt` 开头，PascalCase
- **接口**: 以 `ITakt` 开头，PascalCase
- **异步方法**: 以 `Async` 结尾
- **变量**: camelCase

### 架构原则

- 分层架构：Fluent → Application → Domain → Infrastructure → Common
- 依赖方向只能向下
- MVVM 模式：View 只负责 UI，ViewModel 处理逻辑
- 依赖注入：通过构造函数注入

## 多语言使用

### XAML 中使用

```xml
<TextBlock Text="{local:Loc Key=Identity.User.Title}"/>
<Button Content="{local:Loc Key=Button.Save}"/>
```

### C# 中使用

```csharp
var title = _localizationManager.GetString("Identity.User.Title");
```

翻译数据存储在数据库 `takt_routine_translation` 表中。

## 路径管理

使用 `PathHelper` 统一管理路径：

- **日志**: `AppData\Local\Takt\Takt SMEs\Logs`
- **配置**: `AppData\Roaming\Takt\Takt SMEs`
- **模板**: `AppData\Roaming\Takt\Takt SMEs\Templates`

## 数据库

### 主要表

- **Identity**: `takt_oidc_user`, `takt_oidc_role`, `takt_oidc_menu`
- **Routine**: `takt_routine_language`, `takt_routine_translation`, `takt_routine_setting`
- **Logging**: `takt_logging_login_log`, `takt_logging_operation_log`
- **Logistics**: `takt_logistics_prod_material`, `takt_logistics_prod_serial_inbound`

### 实体规范

- 表名: `takt_模块名_实体名`
- 主键: `id` (bigint, 雪花ID)
- 审计字段: `created_by`, `created_time`, `updated_by`, `updated_time`, `is_deleted`

## 常见问题

**数据库连接失败**
- 检查 `appsettings.json` 连接字符串
- 确认 SQL Server 服务已启动

**菜单不显示**
- 检查数据库 `takt_oidc_menu` 表
- 确认用户角色权限配置

**翻译不生效**
- 检查 `takt_routine_translation` 表
- 确认语言代码正确（zh-CN, en-US, ja-JP）

## 版本信息

- **当前版本**: 0.0.2
- **.NET 版本**: 9.0
- **最后更新**: 2025-12-15

## 完整技术栈

本项目采用完整的企业级技术栈，涵盖前端 UI、架构模式、依赖注入、数据访问、日志系统、模板引擎等各个方面：

```
WPF + Prism + CommunityToolkit + MaterialDesignThemes + 
Autofac + SqlSugar + Scriban + Serilog + Newtonsoft + 
FontAwesome.Sharp + LibVLCSharp
```

### 技术栈详细列表

| 类别 | 技术 | 版本 | 用途 |
|------|------|------|------|
| 核心框架 | .NET | 9.0 | 运行平台 |
| UI 框架 | WPF | 9.0 | 桌面应用 UI |
| MVVM 框架 | Prism | 9.0.537 | 模块化 MVVM |
| MVVM 工具 | CommunityToolkit.Mvvm | 8.4.0 | MVVM 辅助工具 |
| UI 组件 | MaterialDesignThemes | 5.3.0 | Material Design UI |
| 图标库 | FontAwesome.Sharp | 6.6.0 | 图标支持 |
| 依赖注入 | Autofac | 8.4.0 | IoC 容器 |
| ORM | SqlSugar | - | 数据访问层 |
| 日志 | Serilog | 4.3.0 | 结构化日志 |
| 模板引擎 | Scriban | 6.5.2 | 代码生成模板 |
| JSON | Newtonsoft.Json | 13.0.4 | JSON 处理 |
| 媒体播放 | LibVLCSharp.WPF | 3.9.4 | 视频播放 |
| 任务调度 | Quartz | 3.15.1 | 定时任务 |
| 对象映射 | Mapster | 7.4.0 | DTO 映射 |

## 许可证

MIT License

**免责声明**: 此软件使用 MIT License，作者不承担任何使用风险。

## 相关链接

- **项目地址**: https://github.com/Lean365/Takt.Wpf
- **安装包构建**: [docs/INSTALLER.md](./docs/INSTALLER.md)
- **架构规范**: [.cursor/rules/architecture.mdc](.cursor/rules/architecture.mdc)

## 更新日志

### v0.0.2 (2025-12-15)
- 🏗️ **架构优化**
  - 使用 Prism.Wpf (9.0.537) 和 Prism.DryIoc (9.0.537) 实现模块化 MVVM 架构
  - 创建 `NavigationService` 统一导航管理
  - 实现模块化设计：IdentityModule、LogisticsModule、GeneratorModule、LoggingModule、RoutineModule
- 🎨 **初始化视图优化**
  - 新增 `InitializationStatusManager` 初始化状态管理器
  - 新增 `InitializationLogWindow` 初始化日志窗口，实时显示初始化进度
  - 优化启动流程：初始化期间禁用登录，完成后自动显示登录窗口

### v0.0.1 (2025-11-01)
- 🎉 **初始版本发布**
- ✨ **核心功能模块**
  - 身份认证模块：用户管理、角色管理、菜单管理、RBAC 权限模型
  - 后勤模块：物料管理、序列号管理（入库/出库/扫描）、访客管理、欢迎标识
  - 代码生成模块：表配置管理、代码生成（Entity/DTO/Service/ViewModel/View）
  - 日志模块：登录日志、操作日志、差异日志、自动清理
  - 基础模块：多语言管理（中文/英文/日文）、翻译管理、字典管理、系统设置
- 🌍 **多语言支持**
  - 支持中文（zh-CN）、英文（en-US）、日文（ja-JP）
  - 数据库驱动的翻译系统，支持动态切换语言
- 🎨 **UI/UX**
  - Material Design 风格界面
  - 多主题支持（亮色/暗色）
  - 响应式布局设计
