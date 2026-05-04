param(
  [string]$Configuration = "Debug",
  [string]$Platform = "x64",
  [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Get-DotNet.ps1"
$dotnet = Get-EvydenciaDotNet

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $repoRoot "src\Evydencia.PhotoSelector.App\Evydencia.PhotoSelector.App.csproj"

if ($OutputPath -eq "") {
  $OutputPath = Join-Path $repoRoot "artifacts\unpackaged-debug"
}

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

& $dotnet publish $project `
  -c $Configuration `
  -r "win-$Platform" `
  -p:Platform=$Platform `
  -p:WindowsPackageType=None `
  -p:SelfContained=true `
  -p:PublishReadyToRun=false `
  -p:PublishTrimmed=false `
  -o $OutputPath

if ($LASTEXITCODE -ne 0) {
  throw "Falha ao publicar app unpackaged dev. dotnet publish retornou $LASTEXITCODE."
}

Write-Host "App unpackaged dev publicado em: $OutputPath"
