# Triply Build Fix Script
Write-Host "🔧 Triply Build Fix Script" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

Write-Host "⚠️  WARNING: This script will:" -ForegroundColor Yellow
Write-Host "   1. Close all Visual Studio instances" -ForegroundColor Yellow
Write-Host "   2. Clean build artifacts" -ForegroundColor Yellow
Write-Host "   3. Clear NuGet caches" -ForegroundColor Yellow
Write-Host "   4. Restore packages" -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "Continue? (Y/N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "Cancelled." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Step 1: Closing Visual Studio..." -ForegroundColor Green
Get-Process devenv -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

Write-Host "Step 2: Cleaning build artifacts..." -ForegroundColor Green
$projectPath = "C:\Users\jroge\Desktop\Triply\Triply\Triply"
Remove-Item "$projectPath\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$projectPath\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$projectPath\.vs" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Step 3: Cleaning NuGet local caches..." -ForegroundColor Green
dotnet nuget locals http-cache --clear
dotnet nuget locals temp --clear

Write-Host "Step 4: Restoring NuGet packages..." -ForegroundColor Green
Set-Location $projectPath
dotnet restore "Triply.csproj" --force

Write-Host ""
Write-Host "✅ Done! Try building again in Visual Studio." -ForegroundColor Green
Write-Host ""
Write-Host "If build still fails, you may need to:" -ForegroundColor Yellow
Write-Host "  1. Restart your computer to release all file locks" -ForegroundColor Yellow
Write-Host "  2. Or run: taskkill /F /IM MSBuild.exe /T" -ForegroundColor Yellow
Write-Host ""
