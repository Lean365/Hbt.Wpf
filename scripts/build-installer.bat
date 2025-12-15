@echo off
chcp 65001 >nul 2>&1
REM ========================================
REM 项目名称：节拍（Takt）中小企业平台 · Takt SMEs Platform
REM 脚本名称：build-installer.bat
REM 创建时间：2025-01-20
REM 创建人：Takt365(Cursor AI)
REM 功能描述：构建 WPF 应用程序安装包（MSIX）- 快速脚本
REM
REM 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
REM 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
REM ========================================

setlocal enabledelayedexpansion

REM 获取脚本所在目录，并切换到项目根目录
set "SCRIPT_DIR=%~dp0"
set "ROOT_DIR=%SCRIPT_DIR%.."
cd /d "%ROOT_DIR%"

set "CONFIGURATION=Release"
set "OUTPUT_PATH=.\publish"
set "PROJECT_PATH=.\src\Takt.Fluent\Takt.Fluent.csproj"

echo ========================================
echo 节拍中小企业平台 - 安装包构建脚本
echo ========================================
echo.

echo [1/4] 检查 .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo 错误: 未找到 .NET SDK，请先安装 .NET 9.0 SDK
    exit /b 1
)
echo ✓ .NET SDK 已安装
echo.

echo [2/4] 清理之前的构建...
if exist "%OUTPUT_PATH%" (
    rmdir /s /q "%OUTPUT_PATH%"
    echo ✓ 已清理输出目录
)
echo.

echo [3/4] 还原 NuGet 包...
dotnet restore "%PROJECT_PATH%"
if errorlevel 1 (
    echo 错误: NuGet 包还原失败
    exit /b 1
)
echo ✓ NuGet 包还原完成
echo.

echo [4/4] 构建 MSIX 安装包...
echo 配置: %CONFIGURATION%
echo 输出路径: "%OUTPUT_PATH%"
echo.

REM 尝试使用 msbuild 构建 MSIX 包（需要 Windows 10 SDK）
where msbuild >nul 2>&1
if errorlevel 1 (
    echo 警告: 未找到 msbuild，尝试使用 dotnet publish...
    dotnet publish "%PROJECT_PATH%" ^
        --configuration %CONFIGURATION% ^
        --output "%OUTPUT_PATH%" ^
        --runtime win-x64 ^
        -p:WindowsPackageType=MSIX ^
        -p:GenerateAppInstallerFile=false ^
        -p:AppxPackageDir="%OUTPUT_PATH%\AppPackages"
    
    if errorlevel 1 (
        echo 错误: dotnet publish 构建失败
        echo.
        echo 提示: MSIX 打包可能需要 Windows 10 SDK
        echo 请尝试使用 Visual Studio 的"发布"功能，或安装 Windows 10 SDK
        exit /b 1
    )
) else (
    echo 使用 msbuild 构建 MSIX 包...
    msbuild "%PROJECT_PATH%" ^
        /t:Publish ^
        /p:Configuration=%CONFIGURATION% ^
        /p:RuntimeIdentifier=win-x64 ^
        /p:WindowsPackageType=MSIX ^
        /p:GenerateAppInstallerFile=false ^
        /p:AppxPackageDir="%OUTPUT_PATH%\AppPackages" ^
        /p:AppxPackageOutputDir="%OUTPUT_PATH%\AppPackages"
    
    if errorlevel 1 (
        echo 错误: msbuild 构建失败
        exit /b 1
    )
)

if errorlevel 1 (
    echo 错误: 构建失败
    exit /b 1
)

echo.
echo ========================================
echo ✓ 构建完成！
echo ========================================
echo.

REM 查找生成的 MSIX 文件
set "MSIX_FOUND=0"
if exist "%OUTPUT_PATH%\AppPackages\*.msix" (
    echo 找到 MSIX 安装包:
    dir /b "%OUTPUT_PATH%\AppPackages\*.msix"
    set "MSIX_FOUND=1"
)
if exist "%OUTPUT_PATH%\*.msix" (
    echo 找到 MSIX 安装包:
    dir /b "%OUTPUT_PATH%\*.msix"
    set "MSIX_FOUND=1"
)

if "%MSIX_FOUND%"=="0" (
    echo 警告: 未找到 .msix 文件
    echo.
    echo 可能的原因:
    echo 1. MSIX 打包需要 Windows 10 SDK
    echo 2. 请尝试使用 Visual Studio 的"发布"功能
    echo 3. 或检查构建日志中的错误信息
    echo.
    echo 普通发布文件位置: "%OUTPUT_PATH%"
) else (
    echo.
    echo 安装说明:
    echo 1. 双击 .msix 文件进行安装
    echo 2. 或在 PowerShell 中运行: Add-AppxPackage -Path "安装包路径"
)
echo.

pause

