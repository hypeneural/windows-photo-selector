$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")
$required = @(
    "AGENTS.md",
    "PLANS.md",
    ".codex\config.toml",
    ".editorconfig",
    ".gitignore",
    ".agents\skills\windows-photo-selector\SKILL.md",
    "global.json",
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    "NuGet.config",
    "README.md",
    "CHANGELOG.md",
    "docs\evydencia-escolher-fotos-plano-implementacao.md",
    "docs\execution-progress.md",
    "tools\build.ps1",
    "tools\test.ps1",
    "tools\format.ps1",
    "tools\benchmarks.ps1"
)

$missing = @()
foreach ($path in $required) {
    $fullPath = Join-Path $repoRoot $path
    if (-not (Test-Path $fullPath)) {
        $missing += $path
    }
}

if ($missing.Count -gt 0) {
    Write-Error ("Missing required files: " + ($missing -join ", "))
}

$solution = Join-Path $repoRoot "Evydencia.PhotoSelector.sln"
if (Test-Path $solution) {
    Write-Host "Solution found: $solution"
} else {
    Write-Host "Solution not scaffolded yet. Orientation structure is valid."
}
