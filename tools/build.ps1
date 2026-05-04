param(
    [string]$Configuration = "Debug",
    [string]$Platform = ""
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Get-DotNet.ps1"
$dotnet = Get-EvydenciaDotNet

$solution = Join-Path $PSScriptRoot "..\Evydencia.PhotoSelector.sln"

if (-not (Test-Path $solution)) {
    Write-Host "Solution not found yet: $solution"
    Write-Host "No app code has been scaffolded. Build script is ready for the implementation phase."
    exit 0
}

& $dotnet restore $solution

$buildArgs = @($solution, "-c", $Configuration, "--no-restore")
if ($Platform -ne "") {
    $buildArgs += "-p:Platform=$Platform"
}

& $dotnet build @buildArgs
