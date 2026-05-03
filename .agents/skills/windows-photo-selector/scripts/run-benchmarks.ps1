$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")
$benchmarkScript = Join-Path $repoRoot "tools\benchmarks.ps1"

& $benchmarkScript
