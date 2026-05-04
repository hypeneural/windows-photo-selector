param(
  [Parameter(Mandatory = $true)]
  [string]$Folder,
  [string]$Source = "manual-msix-launch"
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\msix-activation.ps1"

$resolvedFolder = (Resolve-Path -LiteralPath $Folder).Path
if (-not (Test-Path -LiteralPath $resolvedFolder -PathType Container)) {
  throw "Pasta nao encontrada: $Folder"
}

$arguments = "--folder $(ConvertTo-CommandLineArgument $resolvedFolder) --source $(ConvertTo-CommandLineArgument $Source)"
$processId = Invoke-EvydenciaPackagedAppActivation -Arguments $arguments
Write-Host "App ativado via MSIX. PID retornado: $processId"
