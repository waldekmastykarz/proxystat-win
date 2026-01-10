#Requires -Version 7.0
<#
.SYNOPSIS
    Builds and packages a release of ProxyStat.
.DESCRIPTION
    Publishes a self-contained Windows executable and packages it into a zip file
    ready for distribution.
.PARAMETER Version
    The semantic version number for this release (e.g., 1.0.0, 1.1.0, 2.0.0)
.EXAMPLE
    ./Build-Release.ps1 -Version 1.0.0
.EXAMPLE
    ./Build-Release.ps1 1.2.0
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$scriptDir = $PSScriptRoot
$projectRoot = Resolve-Path (Join-Path $scriptDir "../../..")
$publishDir = Join-Path $projectRoot "publish"
$projectDir = Join-Path $projectRoot "ProxyStat"
$zipName = "ProxyStat-v${Version}-win-x64.zip"
$zipPath = Join-Path $projectRoot $zipName

Write-Host "Building ProxyStat v${Version}..." -ForegroundColor Cyan
Write-Host ""

# Clean previous builds
if (Test-Path $publishDir) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $publishDir
}
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

# Build and publish
Write-Host "Publishing self-contained release..." -ForegroundColor Green
Push-Location $projectDir
try {
    dotnet publish -c Release -r win-x64 --self-contained true -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

# Create zip
Write-Host ""
Write-Host "Creating release package..." -ForegroundColor Green
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "✅ Release package created: $zipName" -ForegroundColor Green
Write-Host ""

# Show contents summary
Write-Host "Package contents:" -ForegroundColor Cyan
$zipInfo = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entries = $zipInfo.Entries | Select-Object -First 15
    foreach ($entry in $entries) {
        $size = "{0:N0}" -f $entry.Length
        Write-Host "  $($entry.Name) ($size bytes)"
    }
    $totalEntries = $zipInfo.Entries.Count
    if ($totalEntries -gt 15) {
        Write-Host "  ... and $($totalEntries - 15) more files"
    }
}
finally {
    $zipInfo.Dispose()
}

# Show file size
$fileSize = (Get-Item $zipPath).Length
$fileSizeMB = [math]::Round($fileSize / 1MB, 2)
Write-Host ""
Write-Host "Package size: ${fileSizeMB} MB" -ForegroundColor Cyan
Write-Host "Location: $zipPath" -ForegroundColor Cyan
