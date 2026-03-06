# Clean and restart script for MAUI
Write-Host "Cleaning MAUI project..." -ForegroundColor Cyan

# Navigate to project
$projectPath = "C:\Users\jroge\Desktop\Triply\Triply\Triply"
Set-Location $projectPath

# Stop any running instances
Get-Process -Name "Triply" -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean
Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "✅ Cleaned! Now rebuild in Visual Studio." -ForegroundColor Green
