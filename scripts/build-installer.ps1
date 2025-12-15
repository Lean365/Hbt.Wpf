# ========================================
# 项目名称：节拍（Takt）中小企业平台 · Takt SMEs Platform
# 脚本名称：build-installer.ps1
# 创建时间：2025-01-20
# 创建人：Takt365(Cursor AI)
# 功能描述：构建 WPF 应用程序安装包（MSIX）
#
# 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
# 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\publish",
    
    [Parameter(Mandatory=$false)]
    [switch]$SignPackage = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$CertificatePath = "",
    
    [Parameter(Mandatory=$false)]
    [string]$CertificatePassword = ""
)

$ErrorActionPreference = "Stop"

# 获取脚本所在目录，并切换到项目根目录
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
Set-Location $RootDir

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "节拍中小企业平台 - 安装包构建脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "工作目录: $RootDir" -ForegroundColor Gray
Write-Host ""

# 检查 .NET SDK
Write-Host "[1/5] 检查 .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if (-not $dotnetVersion) {
    Write-Host "错误: 未找到 .NET SDK，请先安装 .NET 9.0 SDK" -ForegroundColor Red
    exit 1
}
Write-Host "✓ .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# 检查项目文件
Write-Host "[2/5] 检查项目文件..." -ForegroundColor Yellow
$projectPath = Join-Path $RootDir "src\Takt.Fluent\Takt.Fluent.csproj"
if (-not (Test-Path $projectPath)) {
    Write-Host "错误: 未找到项目文件: $projectPath" -ForegroundColor Red
    exit 1
}
Write-Host "✓ 项目文件: $projectPath" -ForegroundColor Green
Write-Host ""

# 清理之前的构建
Write-Host "[3/5] 清理之前的构建..." -ForegroundColor Yellow
$fullOutputPath = Join-Path $RootDir $OutputPath
if (Test-Path $fullOutputPath) {
    Remove-Item -Path $fullOutputPath -Recurse -Force
    Write-Host "✓ 已清理输出目录" -ForegroundColor Green
}
Write-Host ""

# 还原依赖
Write-Host "[4/5] 还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: NuGet 包还原失败" -ForegroundColor Red
    exit 1
}
Write-Host "✓ NuGet 包还原完成" -ForegroundColor Green
Write-Host ""

# 发布应用（MSIX 包）
Write-Host "[5/5] 构建 MSIX 安装包..." -ForegroundColor Yellow
Write-Host "配置: $Configuration" -ForegroundColor Gray
Write-Host "输出路径: $OutputPath" -ForegroundColor Gray
Write-Host ""

$fullOutputPath = Join-Path $RootDir $OutputPath
$publishArgs = @(
    "publish",
    $projectPath,
    "--configuration", $Configuration,
    "--output", $fullOutputPath,
    "--runtime", "win-x64",
    "-p:PublishProfile=FolderProfile",
    "-p:WindowsPackageType=MSIX",
    "-p:GenerateAppInstallerFile=false"
)

if ($SignPackage -and $CertificatePath -and (Test-Path $CertificatePath)) {
    Write-Host "使用证书签名: $CertificatePath" -ForegroundColor Gray
    $publishArgs += "-p:AppxPackageSigningEnabled=true"
    $publishArgs += "-p:AppxPackageKeyFile=$CertificatePath"
    if ($CertificatePassword) {
        $publishArgs += "-p:AppxPackageKeyPassword=$CertificatePassword"
    }
} else {
    Write-Host "警告: 未签名安装包（仅用于测试）" -ForegroundColor Yellow
}

dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 构建失败" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ 构建完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "安装包位置: $fullOutputPath" -ForegroundColor Cyan
Write-Host ""

# 查找生成的 MSIX 文件
$msixFiles = Get-ChildItem -Path $fullOutputPath -Filter "*.msix" -Recurse
if ($msixFiles) {
    Write-Host "生成的安装包:" -ForegroundColor Yellow
    foreach ($file in $msixFiles) {
        $fileSize = [math]::Round($file.Length / 1MB, 2)
        Write-Host "  - $($file.FullName) ($fileSize MB)" -ForegroundColor Green
    }
} else {
    Write-Host "警告: 未找到 .msix 文件" -ForegroundColor Yellow
    Write-Host "请检查构建输出目录: $fullOutputPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "安装说明:" -ForegroundColor Cyan
Write-Host "1. 双击 .msix 文件进行安装" -ForegroundColor White
Write-Host "2. 或在 PowerShell 中运行: Add-AppxPackage -Path `"<msix文件路径>`"" -ForegroundColor White
Write-Host ""

