# FlowRight API Documentation Build Script
param(
    [switch]$Serve = $false,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "🔨 Building FlowRight API Documentation" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan

# Check if DocFX is installed
try {
    $docfxVersion = docfx --version 2>$null
    Write-Host "✅ DocFX found: $docfxVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ DocFX not found. Installing DocFX globally..." -ForegroundColor Yellow
    dotnet tool install -g docfx
    Write-Host "✅ DocFX installed successfully" -ForegroundColor Green
}

# Build the solution to generate XML documentation
Write-Host ""
Write-Host "📦 Building FlowRight solution ($Configuration)..." -ForegroundColor Blue
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionDir = Split-Path -Parent $ScriptDir
Push-Location $SolutionDir
try {
    dotnet build FlowRight.sln --configuration $Configuration --no-restore
    Write-Host "✅ Solution built successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Solution build failed" -ForegroundColor Red
    Pop-Location
    exit 1
} finally {
    Pop-Location
}

# Generate API documentation
Write-Host ""
Write-Host "📖 Generating API documentation with DocFX..." -ForegroundColor Blue
Push-Location $ScriptDir
try {
    docfx
    
    # Check if generation was successful
    if (Test-Path "_site") {
        Write-Host ""
        Write-Host "✅ Documentation generated successfully!" -ForegroundColor Green
        Write-Host "📂 Output directory: docs\_site" -ForegroundColor White
        Write-Host "🌐 Open docs\_site\index.html to view the documentation" -ForegroundColor White
        
        Write-Host ""
        Write-Host "📊 Documentation Statistics:" -ForegroundColor Yellow
        $htmlFiles = (Get-ChildItem -Recurse -Filter "*.html" -Path "_site").Count
        $apiFiles = (Get-ChildItem -Recurse -Filter "*.html" -Path "_site\api").Count
        $articleFiles = if (Test-Path "_site\articles") { (Get-ChildItem -Recurse -Filter "*.html" -Path "_site\articles").Count } else { 0 }
        
        Write-Host "   - HTML files: $htmlFiles" -ForegroundColor White
        Write-Host "   - API pages: $apiFiles" -ForegroundColor White
        Write-Host "   - Article pages: $articleFiles" -ForegroundColor White
        
        if ($Serve) {
            Write-Host ""
            Write-Host "🚀 Starting local development server..." -ForegroundColor Cyan
            docfx serve _site
        } else {
            Write-Host ""
            Write-Host "🚀 To serve documentation locally, run:" -ForegroundColor Yellow
            Write-Host "   docfx serve docs\_site" -ForegroundColor White
        }
    } else {
        Write-Host "❌ Documentation generation failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ DocFX generation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}