#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]$Spcomp = $env:SPCOMP,
    [string]$SourceModInclude = $env:SOURCEMOD_INCLUDE,
    [switch]$Check
)

$ErrorActionPreference = "Stop"

$expectedCompiler = "1.12.0.7260"
$buildDate = '__DATE__=\"reproducible\"'
$buildTime = '__TIME__=\"build\"'

$root = $PSScriptRoot
$scripting = Join-Path $root "left4dead2\addons\sourcemod\scripting"
$plugins = Join-Path $root "left4dead2\addons\sourcemod\plugins"
$optionalPlugins = @("l4d2_playstats_sync", "l4d2_ranking", "l4d2_show_patent_icon")

function Get-PluginPath {
    param([string]$Name)

    if ($optionalPlugins -contains $Name) {
        return "optional/$Name.smx"
    }

    return "$Name.smx"
}

function Invoke-Spcomp {
    param([string]$Compiler, [string]$Directory, [string]$Arguments)

    $start = New-Object System.Diagnostics.ProcessStartInfo
    $start.FileName = $Compiler
    $start.Arguments = $Arguments
    $start.WorkingDirectory = $Directory
    $start.UseShellExecute = $false

    $process = [System.Diagnostics.Process]::Start($start)
    $process.WaitForExit()

    return $process.ExitCode
}

if (-not $Spcomp) {
    $candidates = @(
        (Get-Command "spcomp" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Source),
        (Join-Path $root ".sourcemod\addons\sourcemod\scripting\spcomp.exe"),
        (Join-Path $env:ProgramFiles "SourceMod\addons\sourcemod\scripting\spcomp.exe")
    )

    $Spcomp = $candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
}

if (-not $Spcomp -or -not (Test-Path $Spcomp)) {
    throw "spcomp not found. Pass -Spcomp, set SPCOMP, or unpack SourceMod $expectedCompiler into plugins\.sourcemod."
}

$Spcomp = (Resolve-Path $Spcomp).Path

$versionStart = New-Object System.Diagnostics.ProcessStartInfo
$versionStart.FileName = $Spcomp
$versionStart.UseShellExecute = $false
$versionStart.RedirectStandardOutput = $true
$versionStart.RedirectStandardError = $true

$versionProcess = [System.Diagnostics.Process]::Start($versionStart)
$banner = $versionProcess.StandardOutput.ReadToEnd() + $versionProcess.StandardError.ReadToEnd()
$versionProcess.WaitForExit()

$compiler = (($banner -split "\r?\n" | Where-Object { $_.Trim() } | Select-Object -First 1) -split "\s+")[-1]

if ($compiler -ne $expectedCompiler) {
    $message = "spcomp is version '$compiler', but the committed plugins are built with $expectedCompiler."

    if ($Check) {
        throw $message
    }

    Write-Warning "$message The .smx files will differ from the committed ones."
}

if (-not $SourceModInclude) {
    $SourceModInclude = Join-Path (Split-Path $Spcomp -Parent) "include"
}

if (-not (Test-Path (Join-Path $SourceModInclude "sourcemod.inc"))) {
    throw "SourceMod include folder not found: $SourceModInclude. Pass -SourceModInclude or set SOURCEMOD_INCLUDE."
}

$stage = Join-Path ([System.IO.Path]::GetTempPath()) ("l4d2-playstats-hub-plugins-" + [System.Guid]::NewGuid().ToString("N"))

try {
    New-Item -ItemType Directory -Force -Path (Join-Path $stage "out") | Out-Null
    Copy-Item -Path (Join-Path $scripting "*.sp") -Destination $stage
    Copy-Item -Path (Join-Path $scripting "include") -Destination (Join-Path $stage "include") -Recurse
    Copy-Item -Path $SourceModInclude -Destination (Join-Path $stage "sourcemod") -Recurse

    $sources = @(Get-ChildItem -Path $stage -Filter "*.sp" | Sort-Object Name)
    $failed = 0

    foreach ($source in $sources) {
        $name = $source.BaseName

        Write-Host "Compiling $name" -ForegroundColor Cyan

        $syntax = Invoke-Spcomp $Spcomp $stage "`"$($source.Name)`" -i include/ -i sourcemod/ --syntax-only -E -v0"
        if ($syntax -ne 0) {
            Write-Host "FAILED: $name has errors or warnings" -ForegroundColor Red
            $failed++
            continue
        }

        $build = Invoke-Spcomp $Spcomp $stage "`"$($source.Name)`" -i include/ -i sourcemod/ -o `"out/$name.smx`" -z0 -w201 -v0 $buildDate $buildTime"
        if ($build -ne 0) {
            Write-Host "FAILED: $name" -ForegroundColor Red
            $failed++
        }
    }

    if ($failed -gt 0) {
        throw "$failed of $($sources.Count) plugins failed to compile."
    }

    $compiled = @(Get-ChildItem -Path (Join-Path $stage "out") -Filter "*.smx" | Sort-Object Name)

    if ($Check) {
        $different = 0

        foreach ($file in $compiled) {
            $target = Get-PluginPath $file.BaseName
            $committed = Join-Path $plugins $target

            if (-not (Test-Path $committed)) {
                Write-Host "MISSING: left4dead2/addons/sourcemod/plugins/$target is not committed." -ForegroundColor Red
                $different++
            }
            elseif ((Get-FileHash $file.FullName -Algorithm SHA256).Hash -ne (Get-FileHash $committed -Algorithm SHA256).Hash) {
                Write-Host "OUTDATED: left4dead2/addons/sourcemod/plugins/$target is not the result of compiling the committed source." -ForegroundColor Red
                $different++
            }
        }

        $committedFiles = @(Get-ChildItem -Path $plugins -Filter "*.smx" -ErrorAction SilentlyContinue | ForEach-Object { @{ File = $_; Path = $_.Name } })
        $committedFiles += @(Get-ChildItem -Path (Join-Path $plugins "optional") -Filter "*.smx" -ErrorAction SilentlyContinue | ForEach-Object { @{ File = $_; Path = "optional/$($_.Name)" } })

        foreach ($entry in $committedFiles) {
            $name = $entry.File.BaseName

            if (-not (Test-Path (Join-Path $scripting ($name + ".sp")))) {
                Write-Host "ORPHAN: left4dead2/addons/sourcemod/plugins/$($entry.Path) has no source in scripting/." -ForegroundColor Red
                $different++
            }
            elseif ($entry.Path -ne (Get-PluginPath $name)) {
                Write-Host "MISPLACED: left4dead2/addons/sourcemod/plugins/$($entry.Path) belongs in left4dead2/addons/sourcemod/plugins/$(Get-PluginPath $name)." -ForegroundColor Red
                $different++
            }
        }

        if ($different -gt 0) {
            throw "Run plugins/build.ps1 (or build.sh) with SourceMod $expectedCompiler and commit the .smx files."
        }

        Write-Host "All $($sources.Count) committed plugins match their sources." -ForegroundColor Green
        return
    }

    New-Item -ItemType Directory -Force -Path (Join-Path $plugins "optional") | Out-Null

    foreach ($file in $compiled) {
        Copy-Item -Path $file.FullName -Destination (Join-Path $plugins (Get-PluginPath $file.BaseName)) -Force
    }

    Write-Host "All $($sources.Count) plugins compiled into left4dead2/addons/sourcemod/plugins and its optional folder" -ForegroundColor Green
}
finally {
    Remove-Item -Recurse -Force -Path $stage -ErrorAction SilentlyContinue
}
