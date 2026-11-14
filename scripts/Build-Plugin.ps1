param(
    [Parameter(Mandatory = $true)]
    [string]$Gta5Dir,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$resolvedGameDir = (Resolve-Path -LiteralPath $Gta5Dir).Path
$rageSdk = Join-Path $resolvedGameDir "SDK\RagePluginHook.dll"
$lspdfrApi = Join-Path $resolvedGameDir "Plugins\LSPD First Response.dll"

if (-not (Test-Path -LiteralPath $rageSdk -PathType Leaf)) {
    throw "RAGE Plugin Hook SDK assembly not found: $rageSdk"
}
if (-not (Test-Path -LiteralPath $lspdfrApi -PathType Leaf)) {
    throw "LSPDFR API assembly not found: $lspdfrApi"
}

$project = Join-Path $PSScriptRoot "..\src\ProtestCallout.Plugin\ProtestCallout.Plugin.csproj"
$project = (Resolve-Path -LiteralPath $project).Path

Write-Host "Building GTA Protest Callout against the local game SDK..."
dotnet build $project --configuration $Configuration "-p:Gta5Dir=$resolvedGameDir"

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

$buildDir = Join-Path (Split-Path $project -Parent) "bin\$Configuration\net472"
$pluginDll = Join-Path $buildDir "GTAProtestCallout.dll"
$coreDll = Join-Path $buildDir "ProtestCallout.Core.dll"
$settingsIni = Join-Path $buildDir "GTAProtestCallout.ini"

foreach ($file in @($pluginDll, $coreDll, $settingsIni)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Build output is missing: $file"
    }
}

Write-Host ""
Write-Host "Build succeeded."
Write-Host "Plugin:   $pluginDll"
Write-Host "Core:     $coreDll"
Write-Host "Settings: $settingsIni"
Write-Host "Copy all three files to the GTA V Plugins folder for a local test."
