# ════════════════════════════════════════════════════════════════════════════
# deploy.ps1 — publish the site and copy it to the Synology NAS.
#
# Usage (in PowerShell, in the project folder):
#     .\deploy.ps1
#
# What it does:
#   1. "dotnet publish" builds a Release version of the site into a temp folder.
#   2. Copies it to \\192.168.50.2\docker\personalsite\app (only changed files;
#      files no longer in the build are removed).
#   3. Copies docker-compose.yml next to it.
# Afterwards, restart the container in DSM: Container Manager -> Project ->
# personalsite -> Action -> Restart (the site is then down for a few seconds).
#
# The .env file with the database password is NOT touched; it only exists on
# the NAS (see deploy/.env.example).
# ════════════════════════════════════════════════════════════════════════════

param(
    # The NAS folder that holds docker-compose.yml, .env, app/ and keys/
    [string]$Target = '\\192.168.50.2\docker\personalsite'
)

# Stop at the first error instead of continuing with a half-finished deploy
$ErrorActionPreference = 'Stop'

$publish = Join-Path $env:TEMP 'personalsite-publish'

Write-Host '1/3  Publishing (Release)...'
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }
dotnet publish -c Release -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

# Development-only and Windows-only files are not needed on the NAS
Remove-Item (Join-Path $publish 'appsettings.Development.json'), (Join-Path $publish 'PersonalSite.exe'), (Join-Path $publish 'web.config') -ErrorAction SilentlyContinue

Write-Host "2/3  Copying to $Target\app ..."
New-Item -ItemType Directory -Force (Join-Path $Target 'app'), (Join-Path $Target 'keys'), (Join-Path $Target 'showcase') | Out-Null
# robocopy /MIR = make the target an exact mirror of the source.
# /NFL /NDL /NJH /NP = less output. Exit codes below 8 mean success.
robocopy $publish (Join-Path $Target 'app') /MIR /NFL /NDL /NJH /NP /R:2 /W:2 | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy failed with exit code $LASTEXITCODE." }

Write-Host '3/3  Copying docker-compose.yml ...'
Copy-Item (Join-Path $PSScriptRoot 'deploy\docker-compose.yml') (Join-Path $Target 'docker-compose.yml') -Force

if (-not (Test-Path (Join-Path $Target '.env'))) {
    Write-Warning "No .env file in $Target yet. Copy deploy\.env.example there as .env and fill in the password."
}

Write-Host ''
Write-Host 'Done. Now restart the container in DSM: Container Manager -> Project -> personalsite -> Action -> Restart.'
