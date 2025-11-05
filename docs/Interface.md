# Hbt.Wpf 项目接口文档

## 1. 概述

本文档描述 Hbt.Wpf 项目的所有接口定义，包括应用层服务接口、领域层仓储接口、以及数据传输对象（DTO）。

## 2. 应用层服务接口

### 2.1 身份认证模块 (Identity)

#### 2.1.1 IUserService - 用户服务接口

**命名空间**: `Hbt.Application.Services.Identity`

**接口定义**:

```csharp
public interface IUserService
{
    // 查询操作
    Task<Result<PagedResult<UserDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    Task<Result<PagedResult<UserDto>>> GetListAsync(UserQueryDto query);
    Task<Result<UserDto>> GetByIdAsync(long id);
    
    // 创建操作
    Task<Result<long>> CreateAsync(UserCreateDto dto);
    
    // 更新操作
    Task<Result> UpdateAsync(UserUpdateDto dto);
    Task<Result> ChangePasswordAsync(UserChangePasswordDto dto);
    Task<Result> ResetPasswordAsync(UserResetPasswordDto dto);
    Task<Result> StatusAsync(UserStatusDto dto);
    
    // 删除操作
    Task<Result> DeleteAsync(long id);
    
    // 导入导出
    Task<Result<(string fileName, byte[] content)>> ExportAsync(UserQueryDto? query = null, string? sheetName = null, string? fileName = null);
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null);
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}
```

**主要方法说明**:

- `GetListAsync`: 分页查询用户列表，支持关键字搜索
- `GetByIdAsync`: 根据ID获取用户详情
- `CreateAsync`: 创建新用户
- `UpdateAsync`: 更新用户信息
- `ChangePasswordAsync`: 用户修改密码（自助）
- `ResetPasswordAsync`: 管理员重置密码
- `StatusAsync`: 修改用户状态（启用/禁用）
- `DeleteAsync`: 删除用户（逻辑删除）
- `ExportAsync`: 导出用户到Excel
- `ImportAsync`: 从Excel导入用户

#### 2.1.2 IRoleService - 角色服务接口

**命名空间**: `Hbt.Application.Services.Identity`

**接口定义**:

```csharp
public interface IRoleService
{
    // 查询操作
    Task<Result<PagedResult<RoleDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    Task<Result<RoleDto>> GetByIdAsync(long id);
    
    // 创建操作
    Task<Result<long>> CreateAsync(RoleCreateDto dto);
    
    // 更新操作
    Task<Result> UpdateAsync(RoleUpdateDto dto);
    Task<Result> StatusAsync(RoleStatusDto dto);
    
    // 删除操作
    Task<Result> DeleteAsync(long id);
}
```

#### 2.1.3 IMenuService - 菜单服务接口

**命名空间**: `Hbt.Application.Services.Identity`

**接口定义**:

```csharp
public interface IMenuService
{
    // 查询操作
    Task<Result<UserMenuTreeDto>> GetUserMenuTreeAsync(long userId);
    Task<Result<List<MenuDto>>> GetMenuTreeByRolesAsync(List<long> roleIds);
    Task<Result<List<MenuDto>>> GetAllMenuTreeAsync();
    Task<Result<PagedResult<MenuDto>>> GetMenuPagedListAsync(int pageIndex, int pageSize, string? keyword = null);
    Task<Result<MenuDto>> GetMenuByIdAsync(long menuId);
    Task<Result<List<string>>> GetUserPermissionsAsync(long userId);
    
    // 创建操作
    Task<Result<MenuDto>> CreateMenuAsync(MenuCreateDto dto);
    Task<Result<List<MenuDto>>> CreateMenuBatchAsync(List<MenuCreateDto> dtos);
    
    // 更新操作
    Task<Result<MenuDto>> UpdateMenuAsync(long menuId, MenuUpdateDto dto);
    Task<Result> UpdateMenuStatusAsync(MenuStatusDto dto);
    Task<Result> UpdateMenuOrderAsync(MenuOrderDto dto);
    
    // 删除操作
    Task<Result> DeleteMenuAsync(long menuId);
    Task<Result> DeleteMenuBatchAsync(List<long> menuIds);
    
    // 导入导出
    Task<Result<(string fileName, byte[] content)>> ExportAsync(List<long>? menuIds = null, string? sheetName = null, string? fileName = null);
    Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null);
    Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null);
}
```

**主要方法说明**:

- `GetUserMenuTreeAsync`: 根据用户ID获取菜单树（权限过滤）
- `GetMenuTreeByRolesAsync`: 根据角色ID列表获取菜单树
- `GetAllMenuTreeAsync`: 获取所有菜单树（管理员）
- `GetUserPermissionsAsync`: 获取用户权限码列表
- `CreateMenuAsync`: 创建菜单
- `UpdateMenuAsync`: 更新菜单
- `UpdateMenuStatusAsync`: 更新菜单状态
- `UpdateMenuOrderAsync`: 调整菜单排序
- `DeleteMenuAsync`: 删除菜单
- `ExportAsync`: 导出菜单到Excel
- `ImportAsync`: 从Excel导入菜单

#### 2.1.4 ILoginService - 登录服务接口

**命名空间**: `Hbt.Application.Services.Identity`

**接口定义**:

```csharp
public interface ILoginService
{
    Task<Result<LoginResultDto>> LoginAsync(LoginDto dto);
    Task<Result> LogoutAsync(long userId);
    Task<Result> ValidateTokenAsync(string token);
}
```

**主要方法说明**:

- `LoginAsync`: 用户登录，返回登录结果（包含Token、用户信息）
- `LogoutAsync`: 用户登出
- `ValidateTokenAsync`: 验证Token有效性

#### 2.1.5 ISessionService - 会话服务接口

**命名空间**: `Hbt.Application.Services.Identity`

**接口定义**:

```csharp
public interface ISessionService
{
    Task<Result<UserSessionDto>> CreateSessionAsync(long userId);
    Task<Result> UpdateSessionAsync(long sessionId);
    Task<Result> DestroySessionAsync(long sessionId);
    Task<Result<UserSessionDto>> GetSessionAsync(long userId);
}
```

**主要方法说明**:

- `CreateSessionAsync`: 创建用户会话
- `UpdateSessionAsync`: 更新会话（刷新最后活动时间）
- `DestroySessionAsync`: 销毁会话
- `GetSessionAsync`: 获取用户会话

### 2.2 基础模块 (Routine)

#### 2.2.1 ILanguageService - 语言服务接口

**命名空间**: `Hbt.Application.Services.Routine`

**接口定义**:

```csharp
public interface ILanguageService
{
    Task<Result<PagedResult<LanguageDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    Task<Result<PagedResult<LanguageDto>>> GetListAsync(LanguageQueryDto query);
    Task<Result<LanguageDto>> GetByIdAsync(long id);
    Task<Result<LanguageDto>> GetByCodeAsync(string languageCode);
    Task<Result<long>> CreateAsync(LanguageCreateDto dto);
    Task<Result> UpdateAsync(LanguageUpdateDto dto);
    Task<Result> DeleteAsync(long id);
    Task<Result> StatusAsync(long id, int status);
    Task<Result<List<LanguageOptionDto>>> OptionAsync(bool includeDisabled = false);
}
```

**主要方法说明**:

- `GetListAsync`: 分页查询语言列表
- `GetByCodeAsync`: 根据语言代码获取语言
- `CreateAsync`: 创建语言
- `UpdateAsync`: 更新语言
- `DeleteAsync`: 删除语言
- `StatusAsync`: 修改语言状态
- `OptionAsync`: 获取语言选项列表（用于下拉框）

#### 2.2.2 ITranslationService - 翻译服务接口

**命名空间**: `Hbt.Application.Services.Routine`

**接口定义**:

```csharp
public interface ITranslationService
{
    Task<Result<PagedResult<TranslationDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    Task<Result<TranslationDto>> GetByIdAsync(long id);
    Task<Result<string>> GetTranslationAsync(string key, string languageCode, string? defaultValue = null);
    Task<Result<long>> CreateAsync(TranslationCreateDto dto);
    Task<Result> UpdateAsync(TranslationUpdateDto dto);
    Task<Result> DeleteAsync(long id);
    Task<Result> BatchCreateAsync(List<TranslationCreateDto> dtos);
}
```

**主要方法说明**:

- `GetListAsync`: 分页查询翻译列表
- `GetTranslationAsync`: 获取翻译文本（核心方法）
- `CreateAsync`: 创建翻译
- `UpdateAsync`: 更新翻译
- `DeleteAsync`: 删除翻译
- `BatchCreateAsync`: 批量创建翻译

#### 2.2.3 IDictionaryTypeService - 字典类型服务接口

**命名空间**: `Hbt.Application.Services.Routine`

**接口定义**:

```csharp
public interface IDictionaryTypeService
{
    Task<Result<PagedResult<DictionaryTypeDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    Task<Result<DictionaryTypeDto>> GetByIdAsync(long id);
    Task<Result<long>> CreateAsync(DictionaryTypeCreateDto dto);
    Task<Result> UpdateAsync(DictionaryTypeUpdateDto dto);
    Task<Result> DeleteAsync(long id);
    Task<Result> StatusAsync(long id, int status);
}
```

#### 2.2.4 IDictionaryDataService - 字典数据服务接口

**命名空间**: `Hbt.Application.Services.Routine`

**接口定义**:

```csharp
public interface IDictionaryDataService
{
    Task<Result<List<DictionaryDataDto>>> GetListByTypeIdAsync(long typeId);
    Task<Result<DictionaryDataDto>> GetByIdAsync(long id);
    Task<Result<long>> CreateAsync(DictionaryDataCreateDto dto);
    Task<Result> UpdateAsync(DictionaryDataUpdateDto dto);
    Task<Result> DeleteAsync(long id);
    Task<Result> StatusAsync(long id, int status);
}
```

#### 2.2.5 ISettingService - 设置服务接口

**命名空间**: `Hbt.Application.Services.Routine`

**接口定义**:

```csharp
public interface ISettingService
{
    Task<Result<PagedResult<SettingDto>>> GetListAsync(int pageIndex, int pageSize, string? keyword = null);
    Task<Result<SettingDto>> GetByKeyAsync(string key);
    Task<Result<string>> GetValueAsync(string key, string? defaultValue = null);
    Task<Result<long>> CreateAsync(SettingCreateDto dto);
    Task<Result> UpdateAsync(SettingUpdateDto dto);
    Task<Result> DeleteAsync(long id);
}
```

### 2.3 物流模块 (Logistics)

物流模块的服务接口（Materials、Serials、Visitors）在对应的子目录中定义，遵循相同的CRUD模式。

## 3. 领域层仓储接口

### 3.1 IBaseRepository<T> - 通用仓储接口

**命名空间**: `Hbt.Domain.Repositories`

**接口定义**:

```csharp
public interface IBaseRepository<TEntity> where TEntity : class, new()
{
    // 属性
    ISqlSugarClient SqlSugarClient { get; }
    SimpleClient<TEntity> SimpleClient { get; }
    
    // 查询操作
    ISugarQueryable<TEntity> AsQueryable();
    Task<PagedResult<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? condition = null,
        int pageIndex = 1,
        int pageSize = 20,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        OrderByType orderByType = OrderByType.Desc);
    Task<TEntity?> GetByIdAsync(object id);
    Task<TEntity?> GetFirstAsync(Expression<Func<TEntity, bool>> condition);
    TEntity? GetFirst(Expression<Func<TEntity, bool>> condition);
    Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? condition = null);
    
    // 新增操作
    Task<int> CreateAsync(TEntity entity);
    Task<int> CreateAsync(TEntity entity, string? userName);
    int Create(TEntity entity, string? userName);
    Task<int> CreateRangeAsync(List<TEntity> entities);
    
    // 更新操作
    Task<int> UpdateAsync(TEntity entity);
    Task<int> UpdateAsync(TEntity entity, string? userName);
    int Update(TEntity entity, string? userName);
    Task<int> UpdateRangeAsync(List<TEntity> entities);
    
    // 删除操作
    Task<int> DeleteAsync(object id);
    Task<int> DeleteAsync(TEntity entity);
    Task<int> DeleteAsync(Expression<Func<TEntity, bool>> condition);
    Task<int> DeleteRangeAsync(List<object> ids);
    
    // 状态操作
    Task<int> StatusAsync(object id, int status);
}
```

**主要方法说明**:

- `AsQueryable`: 获取查询对象（用于复杂查询）
- `GetListAsync`: 分页查询列表
- `GetByIdAsync`: 根据ID获取实体
- `GetFirstAsync`: 获取第一个符合条件的实体
- `CreateAsync`: 新增实体
- `UpdateAsync`: 更新实体
- `DeleteAsync`: 删除实体（逻辑删除）
- `StatusAsync`: 修改实体状态

## 4. 数据传输对象 (DTO)

### 4.1 用户相关DTO

#### UserDto - 用户DTO

```csharp
public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string RealName { get; set; }
    public int UserType { get; set; }
    public int UserStatus { get; set; }
    public DateTime CreatedTime { get; set; }
    // ... 其他字段
}
```

#### UserCreateDto - 创建用户DTO

```csharp
public class UserCreateDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string RealName { get; set; }
    public int UserType { get; set; }
    // ... 其他字段
}
```

#### UserUpdateDto - 更新用户DTO

```csharp
public class UserUpdateDto
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string RealName { get; set; }
    // ... 其他字段
}
```

#### UserQueryDto - 用户查询DTO

```csharp
public class UserQueryDto
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public int? UserType { get; set; }
    public int? UserStatus { get; set; }
    // ... 其他查询条件
}
```

### 4.2 菜单相关DTO

#### MenuDto - 菜单DTO

```csharp
public class MenuDto
{
    public long Id { get; set; }
    public string MenuName { get; set; }
    public string MenuCode { get; set; }
    public string? I18nKey { get; set; }
    public string? Icon { get; set; }
    public int MenuType { get; set; }
    public long? ParentId { get; set; }
    public int SortOrder { get; set; }
    public string? ViewTypeName { get; set; }
    public int MenuStatus { get; set; }
    public List<MenuDto>? Children { get; set; }
    // ... 其他字段
}
```

#### UserMenuTreeDto - 用户菜单树DTO

```csharp
public class UserMenuTreeDto
{
    public long UserId { get; set; }
    public string Username { get; set; }
    public List<MenuDto> MenuTree { get; set; }
    public List<string> Permissions { get; set; }
}
```

#### MenuCreateDto - 创建菜单DTO

```csharp
public class MenuCreateDto
{
    public string MenuName { get; set; }
    public string MenuCode { get; set; }
    public string? I18nKey { get; set; }
    public string? Icon { get; set; }
    public int MenuType { get; set; }
    public long? ParentId { get; set; }
    public int SortOrder { get; set; }
    public string? ViewTypeName { get; set; }
    // ... 其他字段
}
```

### 4.3 语言相关DTO

#### LanguageDto - 语言DTO

```csharp
public class LanguageDto
{
    public long Id { get; set; }
    public string LanguageCode { get; set; }
    public string LanguageName { get; set; }
    public string? NativeName { get; set; }
    public int Status { get; set; }
    public DateTime CreatedTime { get; set; }
}
```

#### LanguageOptionDto - 语言选项DTO

```csharp
public class LanguageOptionDto
{
    public long Id { get; set; }
    public string LanguageCode { get; set; }
    public string LanguageName { get; set; }
}
```

### 4.4 翻译相关DTO

#### TranslationDto - 翻译DTO

```csharp
public class TranslationDto
{
    public long Id { get; set; }
    public string TranslationKey { get; set; }
    public string LanguageCode { get; set; }
    public string TranslationValue { get; set; }
}
```

## 5. 结果封装

### 5.1 Result<T> - 通用结果

**命名空间**: `Hbt.Common.Results`

```csharp
public class Result<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public int Code { get; set; }
}
```

### 5.2 Result - 无数据结果

```csharp
public class Result
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int Code { get; set; }
}
```

### 5.3 PagedResult<T> - 分页结果

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
```

## 6. UI层接口

### 6.1 ViewModel接口

#### ViewModelBase - ViewModel基类

**命名空间**: `Hbt.Fluent.ViewModels`

```csharp
public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null);
}
```

### 6.2 UI服务接口

#### LanguageService - 语言服务（UI层）

**命名空间**: `Hbt.Fluent.Services`

```csharp
public class LanguageService
{
    public string CurrentLanguage { get; set; }
    public event EventHandler? LanguageChanged;
    public string GetTranslation(string key, string? defaultValue = null);
    public void SetLanguage(string languageCode);
}
```

#### ThemeService - 主题服务

**命名空间**: `Hbt.Fluent.Services`

```csharp
public class ThemeService
{
    public string CurrentTheme { get; set; }
    public event EventHandler? ThemeChanged;
    public void SetTheme(string theme);
    public void InitializeTheme();
}
```

## 7. 扩展接口

### 7.1 ILocalizationManager - 本地化管理器接口

**命名空间**: `Hbt.Domain.Interfaces`

```csharp
public interface ILocalizationManager
{
    string GetTranslation(string key, string languageCode, string? defaultValue = null);
    Task<string> GetTranslationAsync(string key, string languageCode, string? defaultValue = null);
}
```

## 8. 接口使用示例

### 8.1 用户服务使用示例

```csharp
// 获取用户服务
var userService = App.Services.GetService<IUserService>();

// 分页查询用户
var result = await userService.GetListAsync(pageIndex: 1, pageSize: 20, keyword: "admin");

if (result.Success && result.Data != null)
{
    var users = result.Data.Items;
    var totalCount = result.Data.TotalCount;
}

// 创建用户
var createDto = new UserCreateDto
{
    Username = "newuser",
    Password = "password123",
    Email = "newuser@example.com"
};
var createResult = await userService.CreateAsync(createDto);
```

### 8.2 菜单服务使用示例

```csharp
// 获取菜单服务
var menuService = App.Services.GetService<IMenuService>();

// 获取用户菜单树
var result = await menuService.GetUserMenuTreeAsync(userId: 1);

if (result.Success && result.Data != null)
{
    var menuTree = result.Data.MenuTree;
    var permissions = result.Data.Permissions;
}
```

### 8.3 仓储使用示例

```csharp
// 获取仓储
var userRepository = App.Services.GetService<IBaseRepository<User>>();

// 查询用户
var user = await userRepository.GetByIdAsync(id: 1);

// 分页查询
var pagedResult = await userRepository.GetListAsync(
    condition: u => u.UserStatus == 1,
    pageIndex: 1,
    pageSize: 20,
    orderByExpression: u => u.CreatedTime,
    orderByType: OrderByType.Desc
);
```

## 9. 接口版本说明

- **当前版本**: 1.0
- **接口稳定性**: 稳定
- **向后兼容**: 是

## 10. 注意事项

1. **异步操作**: 所有服务接口方法都是异步的，使用 `async/await` 模式
2. **结果封装**: 所有服务方法返回 `Result<T>` 或 `Result`，统一错误处理
3. **依赖注入**: 所有服务通过依赖注入容器获取，不要直接实例化
4. **空值检查**: 使用DTO时注意空值检查，特别是可选字段
5. **事务管理**: 仓储操作在服务层管理事务，不要跨服务事务

