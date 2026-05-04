param(
  [Parameter(Mandatory = $true)]
  [string]$Folder,
  [string]$Source = "unpackaged-dev",
  [switch]$Publish
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$outputPath = Join-Path $repoRoot "artifacts\unpackaged-debug"
$exePath = Join-Path $outputPath "Evydencia.PhotoSelector.App.exe"

function ConvertTo-CommandLineArgument {
  param([string]$Value)

  return '"' + $Value.Replace('"', '\"') + '"'
}

$resolvedFolder = (Resolve-Path -LiteralPath $Folder).Path
if (-not (Test-Path -LiteralPath $resolvedFolder -PathType Container)) {
  throw "Pasta nao encontrada: $Folder"
}

if ($Publish -or -not (Test-Path -LiteralPath $exePath)) {
  & (Join-Path $PSScriptRoot "publish-unpackaged-dev.ps1") -OutputPath $outputPath
}

$arguments = "--folder $(ConvertTo-CommandLineArgument $resolvedFolder) --source $(ConvertTo-CommandLineArgument $Source)"
$process = Start-Process -FilePath $exePath -ArgumentList $arguments -PassThru
Write-Host "App unpackaged dev iniciado. PID: $($process.Id)"
