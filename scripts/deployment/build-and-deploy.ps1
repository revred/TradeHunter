# TradeHunter Build and Deployment Script
# PowerShell script for Windows deployment

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "development",
    
    [Parameter(Mandatory=$false)]
    [string]$BuildConfiguration = "Release",
    
    [Parameter(Mandatory=$false)]
    [switch]$RunTests = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$PublishSelfContained = $false
)

Write-Host "🚀 TradeHunter Deployment Script" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Configuration: $BuildConfiguration" -ForegroundColor Yellow

# Set error handling
$ErrorActionPreference = "Stop"

# Define paths
$RootPath = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$SrcPath = Join-Path $RootPath "src"
$ProjectFile = Join-Path $SrcPath "TradeHunter.csproj"
$ConfigPath = Join-Path $RootPath "config"
$OutputPath = Join-Path $RootPath "bin" $Environment

Write-Host "📁 Project Structure:" -ForegroundColor Cyan
Write-Host "  Root: $RootPath"
Write-Host "  Source: $SrcPath"
Write-Host "  Config: $ConfigPath"
Write-Host "  Output: $OutputPath"

# Step 1: Clean previous builds
Write-Host "`n🧹 Cleaning previous builds..." -ForegroundColor Blue
if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

dotnet clean $ProjectFile --configuration $BuildConfiguration

# Step 2: Restore dependencies
Write-Host "`n📦 Restoring NuGet packages..." -ForegroundColor Blue
dotnet restore $ProjectFile

# Step 3: Build the project
Write-Host "`n🔨 Building project..." -ForegroundColor Blue
dotnet build $ProjectFile --configuration $BuildConfiguration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Build failed!"
    exit 1
}

# Step 4: Run tests (if enabled)
if ($RunTests) {
    Write-Host "`n🧪 Running tests..." -ForegroundColor Blue
    $TestPath = Join-Path $RootPath "tests"
    
    if (Test-Path $TestPath) {
        dotnet test $TestPath --configuration $BuildConfiguration --no-build --verbosity normal
        
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "⚠️ Some tests failed, but continuing with deployment..."
        }
    } else {
        Write-Host "ℹ️ No tests found, skipping test execution" -ForegroundColor Yellow
    }
}

# Step 5: Publish application
Write-Host "`n📤 Publishing application..." -ForegroundColor Blue

$PublishArgs = @(
    "publish"
    $ProjectFile
    "--configuration", $BuildConfiguration
    "--output", $OutputPath
    "--no-build"
)

if ($PublishSelfContained) {
    $PublishArgs += @("--self-contained", "true", "--runtime", "win-x64")
    Write-Host "  Publishing as self-contained executable" -ForegroundColor Cyan
}

& dotnet $PublishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Publish failed!"
    exit 1
}

# Step 6: Copy configuration files
Write-Host "`n⚙️ Copying configuration files..." -ForegroundColor Blue

$ConfigDestination = Join-Path $OutputPath "config"
Copy-Item -Recurse -Force $ConfigPath $ConfigDestination

# Copy environment-specific config
$EnvConfigSource = Join-Path $ConfigPath "environments" "$Environment.yml"
$EnvConfigDest = Join-Path $ConfigDestination "environment.yml"

if (Test-Path $EnvConfigSource) {
    Copy-Item $EnvConfigSource $EnvConfigDest
    Write-Host "  Copied environment config: $Environment.yml" -ForegroundColor Cyan
} else {
    Write-Warning "⚠️ Environment config not found: $Environment.yml"
}

# Step 7: Create startup scripts
Write-Host "`n📝 Creating startup scripts..." -ForegroundColor Blue

# Windows batch file
$BatchScript = @"
@echo off
echo Starting TradeHunter...
cd /d "%~dp0"
TradeHunter.exe %*
pause
"@

$BatchScript | Out-File -FilePath (Join-Path $OutputPath "start-tradehunter.bat") -Encoding ASCII

# PowerShell script
$PowerShellScript = @"
# TradeHunter Startup Script
param(
    [Parameter(ValueFromRemainingArguments=`$true)]
    [string[]]`$Arguments
)

Write-Host "🎯 Starting TradeHunter..." -ForegroundColor Green
Set-Location `$PSScriptRoot

if (`$Arguments) {
    & .\TradeHunter.exe `$Arguments
} else {
    & .\TradeHunter.exe hunt --demo
}
"@

$PowerShellScript | Out-File -FilePath (Join-Path $OutputPath "Start-TradeHunter.ps1") -Encoding UTF8

# Step 8: Create service installation script (optional)
Write-Host "`n🔧 Creating service installation script..." -ForegroundColor Blue

$ServiceScript = @"
# Install TradeHunter as Windows Service
param(
    [Parameter(Mandatory=`$true)]
    [ValidateSet("Install", "Uninstall", "Start", "Stop")]
    [string]`$Action,
    
    [Parameter(Mandatory=`$false)]
    [string]`$ServiceName = "TradeHunter",
    
    [Parameter(Mandatory=`$false)]
    [int]`$Port = 3000
)

`$ExePath = Join-Path `$PSScriptRoot "TradeHunter.exe"
`$ServiceArgs = "mcp --port `$Port"

switch (`$Action) {
    "Install" {
        Write-Host "Installing TradeHunter service..." -ForegroundColor Blue
        sc.exe create `$ServiceName binPath= "`$ExePath `$ServiceArgs" start= auto
        Write-Host "Service installed. Use 'Start' action to start it." -ForegroundColor Green
    }
    "Uninstall" {
        Write-Host "Uninstalling TradeHunter service..." -ForegroundColor Blue
        sc.exe stop `$ServiceName
        sc.exe delete `$ServiceName
        Write-Host "Service uninstalled." -ForegroundColor Green
    }
    "Start" {
        Write-Host "Starting TradeHunter service..." -ForegroundColor Blue
        sc.exe start `$ServiceName
        Write-Host "Service started." -ForegroundColor Green
    }
    "Stop" {
        Write-Host "Stopping TradeHunter service..." -ForegroundColor Blue
        sc.exe stop `$ServiceName
        Write-Host "Service stopped." -ForegroundColor Green
    }
}
"@

$ServiceScript | Out-File -FilePath (Join-Path $OutputPath "Manage-Service.ps1") -Encoding UTF8

# Step 9: Create deployment summary
Write-Host "`n📋 Creating deployment summary..." -ForegroundColor Blue

$DeploymentInfo = @{
    "DeploymentDate" = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "Environment" = $Environment
    "BuildConfiguration" = $BuildConfiguration
    "OutputPath" = $OutputPath
    "TestsRun" = $RunTests.ToString()
    "SelfContained" = $PublishSelfContained.ToString()
}

$DeploymentInfo | ConvertTo-Json -Depth 2 | Out-File -FilePath (Join-Path $OutputPath "deployment-info.json") -Encoding UTF8

# Step 10: Display completion summary
Write-Host "`n✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host "`n📊 Deployment Summary:" -ForegroundColor Cyan
Write-Host "  📁 Output Location: $OutputPath"
Write-Host "  ⚙️ Environment: $Environment"
Write-Host "  🔧 Configuration: $BuildConfiguration"
Write-Host "  🧪 Tests Run: $RunTests"

Write-Host "`n🚀 Quick Start Commands:" -ForegroundColor Yellow
Write-Host "  List strategies:    .\TradeHunter.exe list"
Write-Host "  Start hunting:      .\TradeHunter.exe hunt --demo"
Write-Host "  Start MCP server:   .\TradeHunter.exe mcp --port 3000"
Write-Host "  Get help:           .\TradeHunter.exe --help"

Write-Host "`n💡 Next Steps:" -ForegroundColor Magenta
Write-Host "  1. Review configuration in: $ConfigDestination"
Write-Host "  2. Customize strategies in: $ConfigDestination\strategies"
Write-Host "  3. Test with: .\start-tradehunter.bat"
Write-Host "  4. For production, consider running as service: .\Manage-Service.ps1 Install"

Write-Host "`n🎯 TradeHunter is ready to hunt!" -ForegroundColor Green
"@