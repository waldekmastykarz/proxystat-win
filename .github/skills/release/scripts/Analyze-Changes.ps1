#Requires -Version 7.0
<#
.SYNOPSIS
    Analyzes changes since the last release tag.
.DESCRIPTION
    Lists commits and file changes since the last git tag to help determine
    the appropriate version bump (major, minor, or patch).
.EXAMPLE
    ./Analyze-Changes.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# Get the latest tag
$latestTag = git describe --tags --abbrev=0 2>$null

if (-not $latestTag) {
    Write-Host "No previous releases found. This will be the initial release." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "All commits:"
    git log --oneline
    exit 0
}

Write-Host "Latest release: " -NoNewline
Write-Host $latestTag -ForegroundColor Cyan
Write-Host ""

Write-Host "Commits since ${latestTag}:" -ForegroundColor Green
Write-Host "==========================" -ForegroundColor Green
git log "${latestTag}..HEAD" --oneline

Write-Host ""
Write-Host "Files changed:" -ForegroundColor Green
Write-Host "==============" -ForegroundColor Green
git diff --stat "${latestTag}..HEAD"

Write-Host ""
Write-Host "Change summary:" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green

$commits = (git log "${latestTag}..HEAD" --oneline | Measure-Object -Line).Lines
Write-Host "Total commits: $commits"

if ($commits -eq 0) {
    Write-Host ""
    Write-Host "⚠️  No changes since last release. Nothing to release." -ForegroundColor Yellow
    exit 1
}
