$ErrorActionPreference = "Stop"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "    LuminaDesktop Build & Package Script" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# 1. .NET Publish
Write-Host "[1/2] Publishing LuminaDesktop (Release)..." -ForegroundColor Yellow
dotnet publish -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed."
    exit 1
}

# 2. Compile Inno Setup Script
Write-Host ""
Write-Host "[2/2] Building Installer with Inno Setup..." -ForegroundColor Yellow

$innoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $innoSetupPath)) {
    $innoSetupPath = "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
}

if (-not (Test-Path $innoSetupPath)) {
    Write-Error "Inno Setup compiler (ISCC.exe) not found. Please install Inno Setup 6."
    exit 1
}

& $innoSetupPath "LuminaDesktop.iss"
if ($LASTEXITCODE -ne 0) {
    Write-Error "Inno Setup compilation failed."
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host " Build complete! Installer generated in:     " -ForegroundColor Green
Write-Host " .\Output\LuminaDesktopInstaller_v1.0.0.exe  " -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
