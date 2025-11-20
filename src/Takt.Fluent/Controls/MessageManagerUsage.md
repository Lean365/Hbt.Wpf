# 消息管理器使用规范

## 概述

`TaktMessageManager` 提供了统一的消息显示接口，根据操作类型自动选择合适的显示方式：

- **自动消失提示框（Toast）**：适用于常规操作的结果提示
- **手动确认框（MessageBox）**：适用于需要用户确认的重要操作

## 使用规范

### 1. 自动消失提示框（Toast）- 5秒自动消失，顶部居中显示

**适用于以下场景：**
- ✅ 登录成功/失败
- ✅ 创建操作成功/失败
- ✅ 更新操作成功/失败
- ✅ 导入操作成功/失败
- ✅ 导出操作成功/失败
- ✅ 删除操作成功/失败（删除执行后）
- ✅ 其他常规操作的结果提示

**颜色区分：**
- 成功：绿色边框（RGB: 76, 175, 80）
- 失败/错误：红色边框（RGB: 244, 67, 54）
- 警告：橙色边框（RGB: 255, 152, 0）
- 信息：蓝色边框（RGB: 33, 150, 243）

**使用示例：**

```csharp
// 方式1：直接显示成功/错误消息
TaktMessageManager.ShowSuccess("操作成功！");
TaktMessageManager.ShowError("操作失败！");

// 方式2：根据 Result 对象自动显示
var result = await _userService.CreateAsync(dto);
TaktMessageManager.ShowResult(result); // 成功显示绿色，失败显示红色

// 方式3：带返回值的 Result
var result = await _userService.GetByIdAsync(id);
if (TaktMessageManager.ShowResult(result)) // 成功返回 true，失败返回 false
{
    // 处理成功逻辑
}
```

### 2. 手动确认框（MessageBox）- 需要点击确定/取消，视口居中显示

**适用于以下场景：**
- ⚠️ 删除操作前的确认
- ⚠️ 重要操作前的确认（如批量删除、数据清空等）
- ⚠️ 需要用户明确选择的操作

**使用示例：**

```csharp
// 方式1：通用确认消息
var result = TaktMessageManager.ShowQuestion("确定要执行此操作吗？");
if (result == MessageBoxResult.Yes)
{
    // 用户点击了"是"
}

// 方式2：删除确认（推荐）
var result = TaktMessageManager.ShowDeleteConfirm("用户：张三");
if (result == MessageBoxResult.Yes)
{
    // 执行删除操作
    var deleteResult = await _userService.DeleteAsync(id);
    TaktMessageManager.ShowResult(deleteResult); // 删除结果用 Toast 显示
}
```

## 完整示例

### 登录操作

```csharp
public async Task LoginAsync()
{
    var result = await _loginService.LoginAsync(dto);
    
    // 登录成功/失败都用 Toast 显示（自动消失）
    TaktMessageManager.ShowResult(result);
    
    if (result.Success)
    {
        // 跳转到主界面
    }
}
```

### 创建操作

```csharp
public async Task CreateAsync()
{
    var result = await _userService.CreateAsync(dto);
    
    // 创建成功/失败都用 Toast 显示（自动消失）
    TaktMessageManager.ShowResult(result);
    
    if (result.Success)
    {
        // 刷新列表
        await LoadDataAsync();
    }
}
```

### 删除操作

```csharp
public async Task DeleteAsync()
{
    // 1. 删除前：使用手动确认框
    var confirmResult = TaktMessageManager.ShowDeleteConfirm(selectedItem?.Name);
    if (confirmResult != MessageBoxResult.Yes)
        return;
    
    // 2. 执行删除
    var result = await _userService.DeleteAsync(selectedItem.Id);
    
    // 3. 删除结果：使用 Toast 显示（自动消失）
    TaktMessageManager.ShowResult(result);
    
    if (result.Success)
    {
        // 刷新列表
        await LoadDataAsync();
    }
}
```

### 导入/导出操作

```csharp
public async Task ImportAsync()
{
    var result = await _userService.ImportAsync(fileStream);
    
    // 导入结果用 Toast 显示（自动消失）
    if (result.Success)
    {
        TaktMessageManager.ShowSuccess($"导入完成：成功 {result.Data.success} 条，失败 {result.Data.fail} 条");
    }
    else
    {
        TaktMessageManager.ShowError(result.Message);
    }
    
    if (result.Success)
    {
        // 刷新列表
        await LoadDataAsync();
    }
}
```

## 注意事项

1. **自动消失提示框（Toast）**：
   - 显示时长固定为 5 秒
   - 位置在视图顶部居中
   - 成功和失败通过边框颜色区分（绿色/红色）
   - 不阻塞用户操作

2. **手动确认框（MessageBox）**：
   - 位置在视口居中（CenterScreen）
   - 会阻塞用户操作，直到用户点击按钮
   - 仅用于需要用户明确确认的操作

3. **统一规范**：
   - 所有 CURD 操作的结果提示都使用 Toast（自动消失）
   - 所有删除操作前都使用 MessageBox（手动确认）
   - 登录成功/失败都使用 Toast（自动消失）

## 方法总结

| 方法 | 显示方式 | 适用场景 |
|------|---------|---------|
| `ShowSuccess()` | Toast（自动消失） | 操作成功提示 |
| `ShowError()` | Toast（自动消失） | 操作失败提示 |
| `ShowResult()` | Toast（自动消失） | 根据 Result 自动显示 |
| `ShowQuestion()` | MessageBox（手动确认） | 通用确认消息 |
| `ShowDeleteConfirm()` | MessageBox（手动确认） | 删除操作确认 |
