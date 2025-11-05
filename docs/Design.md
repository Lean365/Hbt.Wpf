# Hbt.Wpf 项目设计文档

## 1. 项目概述

### 1.1 项目简介

Hbt.Wpf 是基于 WPF（Windows Presentation Foundation）开发的企业级中后台管理系统，采用分层架构设计，支持多语言、多主题、RBAC权限管理等核心功能。

### 1.2 技术栈

- **框架**: .NET 9.0, WPF
- **ORM**: SqlSugar 5.1.4
- **依赖注入**: Autofac 8.4.0 + Microsoft.Extensions.DependencyInjection
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **UI组件**: AvalonDock 4.72.1（MDI标签页管理）
- **图标**: FontAwesome.Sharp 6.6.0
- **日志**: Serilog 4.3.0
- **配置**: Microsoft.Extensions.Configuration
- **映射**: Mapster 7.4.0

## 2. 架构设计

### 2.1 分层架构

项目采用经典的分层架构（Clean Architecture），分为以下层次：

```
┌─────────────────────────────────────────┐
│     Hbt.Fluent (表现层)                  │
│  - Views (XAML/C#)                     │
│  - ViewModels (MVVM)                   │
│  - Services (UI服务)                    │
│  - Helpers (UI辅助)                    │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│     Hbt.Application (应用层)             │
│  - Services (业务服务接口和实现)         │
│  - Dtos (数据传输对象)                  │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│     Hbt.Domain (领域层)                  │
│  - Entities (实体模型)                  │
│  - Repositories (仓储接口)              │
│  - Interfaces (领域接口)                │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│  Hbt.Infrastructure (基础设施层)          │
│  - Data (数据访问)                      │
│  - Repositories (仓储实现)              │
│  - DependencyInjection (依赖注入)        │
│  - Services (基础设施服务)              │
└─────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────┐
│     Hbt.Common (通用层)                  │
│  - Logging (日志管理)                   │
│  - Config (配置)                        │
│  - Helpers (工具类)                     │
│  - Results (结果封装)                   │
│  - Security (安全)                      │
└─────────────────────────────────────────┘
```

### 2.2 各层职责

#### 2.2.1 Hbt.Fluent (表现层)

- **职责**: UI展示、用户交互、视图逻辑
- **主要组件**:
  - `Views`: XAML视图文件
  - `ViewModels`: MVVM视图模型
  - `Services`: UI相关服务（ThemeService、LanguageService）
  - `Helpers`: UI辅助类（转换器、扩展方法）
  - `Models`: UI模型（DocumentTabItem、AvalonDocument）

#### 2.2.2 Hbt.Application (应用层)

- **职责**: 业务逻辑编排、应用服务
- **主要组件**:
  - `Services`: 业务服务接口和实现
    - Identity: 身份认证服务（UserService、MenuService、RoleService等）
    - Routine: 基础服务（LanguageService、TranslationService、DictionaryService等）
    - Logistics: 物流服务（Materials、Serials、Visitors等）
    - Logging: 日志服务
  - `Dtos`: 数据传输对象（DTO）

#### 2.2.3 Hbt.Domain (领域层)

- **职责**: 领域模型、业务规则
- **主要组件**:
  - `Entities`: 实体模型
    - Identity: 身份实体（User、Role、Menu、UserRole、RoleMenu、UserSession）
    - Logging: 日志实体（LoginLog、OperationLog、ExceptionLog、DiffLog）
    - Logistics: 物流实体（Materials、Serials、Visitors）
    - Routine: 基础实体（Language、Translation、DictionaryType、DictionaryData、Setting）
  - `Repositories`: 仓储接口（IBaseRepository<T>）
  - `Interfaces`: 领域接口（ILocalizationManager）

#### 2.2.4 Hbt.Infrastructure (基础设施层)

- **职责**: 数据访问、外部服务集成
- **主要组件**:
  - `Data`: 数据访问（DbContext、DbTableInitializer、种子数据）
  - `Repositories`: 仓储实现（BaseRepository<T>）
  - `DependencyInjection`: 依赖注入配置（AutofacModule）
  - `Services`: 基础设施服务（LocalizationManager）

#### 2.2.5 Hbt.Common (通用层)

- **职责**: 通用工具、共享组件
- **主要组件**:
  - `Logging`: 日志管理器（InitLogManager、AppLogManager、OperLogManager）
  - `Config`: 配置类（HbtDatabaseSettings）
  - `Helpers`: 工具类（ExcelHelper、FileHelper、SystemInfoHelper等）
  - `Results`: 结果封装（Result、PagedResult、PagedQuery）
  - `Security`: 安全工具（SecurityHelper）
  - `Constants`: 常量定义（AppConstants）
  - `Enums`: 枚举类型（UserEnum、MenuEnum、StatusEnum等）

## 3. 核心模块设计

### 3.1 身份认证模块 (Identity)

#### 3.1.1 实体设计

- **User**: 用户实体
  - 基本信息：用户名、密码、邮箱、手机号
  - 状态管理：用户状态、用户类型
  - 审计字段：创建时间、更新时间等（继承自BaseEntity）

- **Role**: 角色实体
  - 角色名称、角色编码
  - 角色状态、角色描述

- **Menu**: 菜单实体
  - 菜单信息：菜单名称、菜单编码、I18nKey
  - 菜单类型：目录、菜单、按钮、API
  - 菜单图标：Icon（FontAwesome图标）
  - 菜单层级：ParentId、SortOrder

- **UserRole**: 用户角色关联
- **RoleMenu**: 角色菜单关联
- **UserSession**: 用户会话

#### 3.1.2 服务设计

- **IUserService**: 用户服务
  - CRUD操作
  - 密码管理（修改密码、重置密码）
  - 状态管理
  - Excel导入导出

- **IRoleService**: 角色服务
  - 角色管理
  - 角色权限分配

- **IMenuService**: 菜单服务
  - 菜单树构建
  - 权限码获取
  - 菜单管理（CRUD、状态、排序）
  - Excel导入导出

- **ILoginService**: 登录服务
  - 用户认证
  - 会话管理

- **ISessionService**: 会话服务
  - 会话创建、更新、销毁

### 3.2 多语言模块 (Routine)

#### 3.2.1 实体设计

- **Language**: 语言实体
  - 语言代码、语言名称、状态

- **Translation**: 翻译实体
  - 翻译键、语言代码、翻译值

- **DictionaryType**: 字典类型
- **DictionaryData**: 字典数据
- **Setting**: 系统设置

#### 3.2.2 服务设计

- **ILanguageService**: 语言服务
  - 语言管理（CRUD、状态）
  - 语言选项列表

- **ITranslationService**: 翻译服务
  - 翻译管理
  - 翻译查询

- **IDictionaryTypeService**: 字典类型服务
- **IDictionaryDataService**: 字典数据服务
- **ISettingService**: 设置服务

### 3.3 日志模块 (Logging)

#### 3.3.1 实体设计

- **LoginLog**: 登录日志
- **OperationLog**: 操作日志
- **ExceptionLog**: 异常日志
- **DiffLog**: 差异日志（数据变更记录）

#### 3.3.2 日志管理器

- **InitLogManager**: 初始化日志
- **AppLogManager**: 应用程序日志
- **OperLogManager**: 操作日志

### 3.4 物流模块 (Logistics)

#### 3.4.1 实体设计

- **Materials**: 物料管理
  - ProdMaterial: 产品物料
  - ProdModel: 产品型号

- **Serials**: 序列号管理
  - ProdSerial: 产品序列号
  - ProdSerialInbound: 入库记录
  - ProdSerialOutbound: 出库记录

- **Visitors**: 访客管理
  - Visitor: 访客信息
  - VisitorDetail: 访客详情

## 4. UI设计

### 4.1 MVVM模式

项目采用MVVM（Model-View-ViewModel）模式：

- **View**: XAML视图文件，负责UI展示
- **ViewModel**: 视图模型，负责业务逻辑和状态管理
- **Model**: 数据模型（实体、DTO）

### 4.2 主窗口设计

**MainWindow.xaml** 主窗口结构：

```
┌─────────────────────────────────────────┐
│            TitleBar (标题栏)              │
│  - Logo、菜单、用户信息、窗口控制按钮      │
└─────────────────────────────────────────┘
┌──────────┬──────────────────────────────┐
│          │                              │
│  Menu    │    AvalonDock                │
│  Tree    │    (文档标签页区域)           │
│          │                              │
│          │                              │
└──────────┴──────────────────────────────┘
┌─────────────────────────────────────────┐
│            StatusBar (状态栏)             │
│  - 版权信息、当前文档标题                │
└─────────────────────────────────────────┘
```

### 4.3 标签页管理

使用 **AvalonDock** 实现MDI（多文档界面）：

- **AvalonDocument**: 继承自`LayoutDocument`，包装文档内容
- **DocumentTabItem**: 文档标签页数据模型
- **MainWindowViewModel**: 管理文档生命周期（创建、激活、关闭）

### 4.4 菜单树设计

- 使用 `TreeView` 控件展示菜单树
- 支持多级菜单（目录、菜单、按钮）
- 菜单项支持图标（FontAwesome）和本地化文本
- 点击菜单项在AvalonDock中创建/激活文档标签页

### 4.5 主题和本地化

- **ThemeService**: 主题管理服务（Light/Dark）
- **LanguageService**: 语言服务（WPF层）
- **LocExtension**: XAML本地化扩展标记
- **StringToTranslationConverter**: 字符串到翻译的转换器

## 5. 数据访问设计

### 5.1 仓储模式

采用仓储模式（Repository Pattern）抽象数据访问：

- **IBaseRepository<T>**: 通用仓储接口
- **BaseRepository<T>**: 通用仓储实现（基于SqlSugar）

### 5.2 数据库上下文

- **DbContext**: SqlSugar数据库上下文
- 单例模式，统一管理数据库连接

### 5.3 实体映射

- 使用 **SqlSugar** 特性（`[SugarTable]`、`[SugarColumn]`）进行ORM映射
- 使用 **Mapster** 进行DTO到实体的映射

### 5.4 数据库初始化

- **DbTableInitializer**: 数据库表初始化（Code First）
- **DbSeedRbac**: RBAC种子数据初始化
- **DbSeedRoutine**: Routine模块种子数据初始化
- **DbSeedMenu**: 菜单种子数据初始化

## 6. 依赖注入设计

### 6.1 容器选择

- **Autofac**: 主要依赖注入容器
- **Microsoft.Extensions.DependencyInjection**: 与.NET Host集成

### 6.2 注册策略

- **AutofacModule**: Autofac模块，统一注册所有服务
- **App.xaml.cs**: 注册UI相关服务（窗口、ViewModel、UI服务）

### 6.3 生命周期

- **Singleton**: 单例（DbContext、LogManager、Service）
- **Transient**: 瞬态（ViewModel、Window）
- **InstancePerLifetimeScope**: 作用域（Repository、Application Service）

## 7. 日志设计

### 7.1 日志架构

- **Serilog**: 结构化日志框架
- **日志目标**: 控制台、文件（logs/app-*.txt）

### 7.2 日志管理器

- **InitLogManager**: 初始化日志（应用启动、数据库初始化）
- **AppLogManager**: 应用程序日志（通用应用日志）
- **OperLogManager**: 操作日志（业务操作记录）

### 7.3 日志级别

- Debug: 调试信息
- Information: 一般信息
- Warning: 警告
- Error: 错误
- Fatal: 致命错误

## 8. 安全设计

### 8.1 密码加密

- 使用 **BouncyCastle** 进行密码加密/解密
- **SecurityHelper**: 安全工具类

### 8.2 权限控制

- **RBAC**: 基于角色的访问控制
- 用户-角色-菜单多对多关系
- 权限码（Permission Code）控制按钮和API权限

### 8.3 会话管理

- **UserSession**: 用户会话实体
- **SessionService**: 会话管理服务
- **OnlineUserManager**: 在线用户管理

## 9. 设计模式

### 9.1 使用的设计模式

- **分层架构**: Clean Architecture
- **仓储模式**: Repository Pattern
- **依赖注入**: Dependency Injection
- **MVVM模式**: Model-View-ViewModel
- **单例模式**: Singleton（DbContext、LogManager）
- **工厂模式**: ServiceLocator（服务定位）
- **策略模式**: 主题切换、语言切换

## 10. 数据流设计

### 10.1 用户操作流程

```
用户操作
    ↓
View (XAML)
    ↓
ViewModel (命令绑定)
    ↓
Application Service (业务逻辑)
    ↓
Repository (数据访问)
    ↓
Database
```

### 10.2 菜单导航流程

```
菜单点击
    ↓
MainWindow.xaml.cs (事件处理)
    ↓
MainWindowViewModel.NavigateToMenu()
    ↓
MainWindowViewModel.AddOrActivateDocument()
    ↓
创建 DocumentTabItem
    ↓
创建 AvalonDocument (LayoutDocument)
    ↓
添加到 Documents 集合
    ↓
AvalonDock 显示标签页
```

### 10.3 本地化流程

```
菜单 I18nKey
    ↓
StringToTranslationConverter
    ↓
LanguageService.GetTranslation()
    ↓
Translation Entity (数据库)
    ↓
返回翻译文本
    ↓
UI 显示
```

## 11. 扩展性设计

### 11.1 模块化设计

- 各业务模块独立（Identity、Logistics、Routine）
- 支持新增业务模块

### 11.2 插件化设计

- 菜单动态加载（从数据库读取）
- 页面动态创建（通过ViewTypeName反射创建）

### 11.3 配置化设计

- `appsettings.json` 配置文件
- 数据库配置、日志配置可配置

## 12. 性能优化

### 12.1 数据库优化

- 索引优化（实体类定义索引）
- 分页查询（避免全表扫描）
- 逻辑删除（软删除，保留数据）

### 12.2 UI优化

- 异步操作（async/await）
- 数据绑定（减少代码后置）
- 虚拟化（TreeView虚拟化）

### 12.3 缓存策略

- 菜单树缓存（可扩展）
- 翻译缓存（可扩展）

## 13. 错误处理

### 13.1 异常处理

- 统一异常处理（Result<T>封装）
- 异常日志记录（ExceptionLog）

### 13.2 用户提示

- MessageBox显示错误信息
- 日志记录详细错误

## 14. 测试策略

### 14.1 单元测试

- 服务层单元测试（可扩展）
- 仓储层单元测试（可扩展）

### 14.2 集成测试

- 数据库集成测试（可扩展）

## 15. 部署设计

### 15.1 构建配置

- Debug/Release配置
- 目标框架：.NET 9.0-windows

### 15.2 依赖管理

- NuGet包管理
- 项目引用管理

### 15.3 配置文件

- `appsettings.json`: 应用配置
- `app.manifest`: 应用程序清单

## 16. 版本管理

### 16.1 版本号规范

- 主版本号.次版本号.修订号
- 当前版本：1.0

### 16.2 变更记录

- 记录重大变更（可扩展）

