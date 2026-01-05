# WPF 可安装程序包构建指南

## 概述

本文档说明如何为节拍（Takt）中小企业平台构建可安装程序包。

## 打包方案

项目支持两种打包方式：

### 1. MSIX 包（推荐）

MSIX 是 Microsoft 推荐的现代 Windows 应用打包格式，具有以下优势：

- ✅ 自动更新支持
- ✅ 沙箱安全
- ✅ 易于分发
- ✅ 支持 Windows Store 发布
- ✅ .NET 9.0 原生支持

### 2. MSI 安装包（传统方式）

MSI 是传统的 Windows 安装包格式，适合企业内部分发。

## 快速开始

### 方式一：使用 PowerShell 脚本（推荐）

```powershell
# 构建 Release 版本的 MSIX 安装包
.\scripts\build-installer.ps1 -Configuration Release

# 构建 Debug 版本
.\scripts\build-installer.ps1 -Configuration Debug

# 指定输出路径
.\scripts\build-installer.ps1 -Configuration Release -OutputPath ".\dist"
```

### 方式二：使用 dotnet CLI

```bash
# 发布 MSIX 包
dotnet publish src/Takt.Fluent/Takt.Fluent.csproj `
    --configuration Release `
    --output ./publish `
    --runtime win-x64 `
    -p:WindowsPackageType=MSIX `
    -p:GenerateAppInstallerFile=false
```

### 方式三：使用 Visual Studio

1. 右键点击 `Takt.Fluent` 项目
2. 选择"发布"（Publish）
3. 选择"新建"（New）
4. 选择"MSIX" 或"MSIX 包"
5. 配置发布设置
6. 点击"发布"

## 安装包签名

### 生成自签名证书（仅用于测试）

```powershell
# 创建自签名证书
$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=Takt IT Co.,Ltd." `
    -KeyExportPolicy Exportable `
    -CertStoreLocation Cert:\CurrentUser\My `
    -KeyUsage DigitalSignature `
    -KeySpec Signature `
    -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(5)

# 导出证书（包含私钥）
$password = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath ".\takt-cert.pfx" -Password $password
```

### 使用证书签名安装包

```powershell
.\scripts\build-installer.ps1 `
    -Configuration Release `
    -SignPackage `
    -CertificatePath ".\takt-cert.pfx" `
    -CertificatePassword "YourPassword"
```

## 安装包安装

### 安装 MSIX 包

```powershell
# 方式一：双击 .msix 文件
# 方式二：使用 PowerShell
Add-AppxPackage -Path ".\publish\Takt.Fluent_0.0.2.0_x64.msix"
```

### 卸载 MSIX 包

```powershell
# 查看已安装的包
Get-AppxPackage | Where-Object {$_.Name -like "*Takt*"}

# 卸载
Remove-AppxPackage -Package "Takt SMEs_<版本号>"
```

## 配置说明

### Package.appxmanifest

MSIX 包的配置文件位于 `src/Takt.Fluent/Package.appxmanifest`，包含以下重要配置：

- **Identity**: 包标识（Name、Publisher、Version）
- **Properties**: 显示名称、描述、图标
- **Capabilities**: 应用权限（网络访问等）
- **Applications**: 应用程序入口点

### Takt.Fluent.csproj

项目文件中的打包相关配置：

```xml
<PropertyGroup>
  <!-- MSIX 打包配置 -->
  <WindowsPackageType>MSIX</WindowsPackageType>
  <AppxBundle>Always</AppxBundle>
  <AppxBundlePlatforms>x64</AppxBundlePlatforms>
  
  <!-- 发布配置 -->
  <PublishReadyToRun>true</PublishReadyToRun>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
</PropertyGroup>
```

## 常见问题

### 1. 安装时提示"无法安装此应用包"

**原因**: 可能缺少必要的依赖或证书问题

**解决方案**:
- 确保 Windows 版本 >= Windows 10 1809
- 检查证书是否有效
- 使用管理员权限运行 PowerShell

### 2. 应用无法访问网络

**原因**: MSIX 包默认在沙箱中运行，需要声明网络权限

**解决方案**: 在 `Package.appxmanifest` 中已配置 `internetClient` 和 `privateNetworkClientServer` 权限

### 3. 应用无法访问文件系统

**原因**: MSIX 包对文件系统访问有限制

**解决方案**: 
- 使用 `runFullTrust` 能力（已在配置中启用）
- 应用可以访问用户目录（AppData）和安装目录

### 4. 构建失败：找不到 Package.appxmanifest

**原因**: 文件路径或名称不正确

**解决方案**: 确保 `Package.appxmanifest` 文件位于 `src/Takt.Fluent/` 目录下

## 企业部署

### 使用组策略部署

1. 将 MSIX 包放到网络共享位置
2. 在组策略中配置应用安装
3. 用户登录时自动安装

### 使用 Intune 部署

1. 上传 MSIX 包到 Microsoft Intune
2. 配置应用分配策略
3. 推送到目标设备

## 更新机制

### 自动更新（App Installer）

1. 配置 `AppInstallerUri` 指向更新服务器
2. 用户安装时选择"启用自动更新"
3. 应用会自动检查并下载更新

### 手动更新

```powershell
# 更新已安装的包
Add-AppxPackage -Path ".\publish\Takt.Fluent_0.0.3.0_x64.msix" -ForceUpdateFromAnyVersion
```

## 相关文件

- **项目配置**: `src/Takt.Fluent/Takt.Fluent.csproj`
- **清单文件**: `src/Takt.Fluent/Package.appxmanifest`
- **构建脚本**: `scripts/build-installer.ps1`
- **版本信息**: `Directory.Build.props`

## 参考文档

- [MSIX 打包文档](https://docs.microsoft.com/zh-cn/windows/msix/)
- [.NET 应用打包](https://docs.microsoft.com/zh-cn/dotnet/core/deploying/)
- [Windows 应用打包工具](https://docs.microsoft.com/zh-cn/windows/msix/packaging-tool/)

