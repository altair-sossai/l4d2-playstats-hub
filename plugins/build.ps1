#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$Spcomp = $env:SPCOMP,
    [string]$SourceModInclude = $env:SOURCEMOD_INCLUDE
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$scripting = Join-Path $root "scripting"
$output = Join-Path $root "compiled"

if (-not $Spcomp) {
    $candidates = @(
        (Get-Command "spcomp" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Source),
        (Join-Path $root ".sourcemod\addons\sourcemod\scripting\spcomp.exe"),
        (Join-Path $env:ProgramFiles "SourceMod\addons\sourcemod\scripting\spcomp.exe")
    )

    $Spcomp = $candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
}

if (-not $Spcomp -or -not (Test-Path $Spcomp)) {
    throw "spcomp not found. Pass -Spcomp or set SPCOMP to the SourceMod 1.12 compiler."
}

if (-not $SourceModInclude) {
    $SourceModInclude = Join-Path (Split-Path $Spcomp -Parent) "include"
}

if (-not (Test-Path $SourceModInclude)) {
    throw "SourceMod include folder not found: $SourceModInclude. Pass -SourceModInclude or set SOURCEMOD_INCLUDE."
}

New-Item -ItemType Directory -Force -Path $output | Out-Null

$sources = @(Get-ChildItem -Path $scripting -Filter "*.sp" | Sort-Object Name)
$failed = 0

foreach ($source in $sources) {
    $target = Join-Path $output ($source.BaseName + ".smx")

    Write-Host "Compiling $($source.BaseName)" -ForegroundColor Cyan

    & $Spcomp $source.FullName -i (Join-Path $scripting "include") -i $SourceModInclude -o $target

    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED: $($source.BaseName)" -ForegroundColor Red
        $failed++
    }
}

if ($failed -gt 0) {
    throw "$failed of $($sources.Count) plugins failed to compile."
}

Write-Host "All $($sources.Count) plugins compiled into $output" -ForegroundColor Green
