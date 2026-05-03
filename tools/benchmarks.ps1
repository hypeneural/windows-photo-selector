param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$BenchmarkArgs = @()
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Get-DotNet.ps1"
$dotnet = Get-EvydenciaDotNet

$project = Join-Path $PSScriptRoot "..\benchmarks\Evydencia.PhotoSelector.Benchmarks\Evydencia.PhotoSelector.Benchmarks.csproj"

if (-not (Test-Path $project)) {
    Write-Host "Benchmark project not found yet: $project"
    Write-Host "No app code has been scaffolded. Benchmark script is ready for the implementation phase."
    exit 0
}

$dotnetArgs = @("run", "-c", "Release", "--project", $project, "--") + $BenchmarkArgs
& $dotnet @dotnetArgs
