param(
  [string]$PackagePath = "",
  [switch]$Reinstall
)

$ErrorActionPreference = "Stop"

$packageName = "Evydencia.PhotoSelector.App"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$packageRoot = Join-Path $repoRoot "artifacts\packages"

if ($PackagePath -eq "") {
  $latestPackage = Get-ChildItem -Path $packageRoot -Recurse -Include *.msix,*.msixbundle |
    Where-Object { $_.Name -like "$packageName*" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

  if ($null -eq $latestPackage) {
    throw "Nenhum MSIX encontrado em $packageRoot. Execute .\tools\package-msix-dev.ps1 primeiro."
  }

  $PackagePath = $latestPackage.FullName
}

$PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path

if ($Reinstall) {
  Get-Process -Name $packageName -ErrorAction SilentlyContinue | Stop-Process -Force

  $existingPackage = Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue
  if ($null -ne $existingPackage) {
    Write-Host "Removendo pacote existente $($existingPackage.PackageFullName)..."
    Remove-AppxPackage -Package $existingPackage.PackageFullName
  }
}

try {
  Write-Host "Instalando MSIX dev: $PackagePath"
  Add-AppxPackage -Path $PackagePath -ForceApplicationShutdown
}
catch {
  $message = $_.Exception.Message
  if ($message -like "*0x800B0109*" -or $message -like "*root certificate*") {
    Write-Host ""
    Write-Host "O pacote esta assinado, mas o Windows ainda nao confia na raiz do certificado para instalacao AppX."
    Write-Host "Execute em PowerShell elevado:"
    Write-Host "  .\tools\trust-msix-dev-cert-admin.ps1"
    Write-Host "Depois rode novamente:"
    Write-Host "  .\tools\install-msix-dev.ps1 -Reinstall"
    Write-Host ""
  }

  throw
}

$installedPackage = Get-AppxPackage -Name $packageName -ErrorAction Stop
Write-Host "Pacote instalado: $($installedPackage.PackageFullName)"
