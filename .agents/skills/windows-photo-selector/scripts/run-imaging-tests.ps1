$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")
$testScript = Join-Path $repoRoot "tools\test.ps1"

& $testScript -Filter "FullyQualifiedName~Imaging"
