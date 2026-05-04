param(
  [string]$PackagePath = ""
)

$ErrorActionPreference = "Stop"

$packageName = "Evydencia.PhotoSelector.App"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$packageRoot = Join-Path $repoRoot "artifacts\packages"
$certificatePath = Join-Path $repoRoot "packaging\certificates-dev\EvydenciaPhotoSelectorDev.cer"

function Test-IsAdministrator {
  $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
  $principal = [Security.Principal.WindowsPrincipal]::new($identity)
  return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-CertificateInStore {
  param(
    [string]$Thumbprint,
    [System.Security.Cryptography.X509Certificates.StoreLocation]$StoreLocation,
    [System.Security.Cryptography.X509Certificates.StoreName]$StoreName
  )

  $store = [System.Security.Cryptography.X509Certificates.X509Store]::new($StoreName, $StoreLocation)
  $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)
  try {
    $matches = $store.Certificates.Find(
      [System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,
      $Thumbprint,
      $false)
    return $matches.Count -gt 0
  }
  finally {
    $store.Close()
  }
}

if ($PackagePath -eq "") {
  $latestPackage = Get-ChildItem -Path $packageRoot -Recurse -Include *.msix,*.msixbundle -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "$packageName*" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

  if ($null -ne $latestPackage) {
    $PackagePath = $latestPackage.FullName
  }
}

$signatureStatus = "MissingPackage"
$signatureMessage = ""
$signerThumbprint = ""
$signerSubject = ""

if ($PackagePath -ne "" -and (Test-Path -LiteralPath $PackagePath)) {
  $PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
  $signature = Get-AuthenticodeSignature -FilePath $PackagePath
  $signatureStatus = $signature.Status.ToString()
  $signatureMessage = $signature.StatusMessage
  if ($null -ne $signature.SignerCertificate) {
    $signerThumbprint = $signature.SignerCertificate.Thumbprint
    $signerSubject = $signature.SignerCertificate.Subject
  }
}

$currentUserMy = $false
$currentUserRoot = $false
$currentUserTrustedPeople = $false
$localMachineTrustedPeople = $false
$localMachineRoot = $false

if ($signerThumbprint -ne "") {
  $currentUserMy = Test-CertificateInStore -Thumbprint $signerThumbprint -StoreLocation CurrentUser -StoreName My
  $currentUserRoot = Test-CertificateInStore -Thumbprint $signerThumbprint -StoreLocation CurrentUser -StoreName Root
  $currentUserTrustedPeople = Test-CertificateInStore -Thumbprint $signerThumbprint -StoreLocation CurrentUser -StoreName TrustedPeople
  $localMachineTrustedPeople = Test-CertificateInStore -Thumbprint $signerThumbprint -StoreLocation LocalMachine -StoreName TrustedPeople
  $localMachineRoot = Test-CertificateInStore -Thumbprint $signerThumbprint -StoreLocation LocalMachine -StoreName Root
}

$installedPackage = Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue

$result = [pscustomobject]@{
  IsAdministrator = Test-IsAdministrator
  PackagePath = $PackagePath
  SignatureStatus = $signatureStatus
  SignatureMessage = $signatureMessage
  SignerSubject = $signerSubject
  SignerThumbprint = $signerThumbprint
  CurrentUserMy = $currentUserMy
  CurrentUserRoot = $currentUserRoot
  CurrentUserTrustedPeople = $currentUserTrustedPeople
  LocalMachineTrustedPeople = $localMachineTrustedPeople
  LocalMachineRoot = $localMachineRoot
  DevCertificateFileExists = Test-Path -LiteralPath $certificatePath
  PackageInstalled = $null -ne $installedPackage
  InstalledPackageFullName = if ($null -ne $installedPackage) { $installedPackage.PackageFullName } else { "" }
}

$result | Format-List

if ($result.SignatureStatus -ne "Valid") {
  Write-Host ""
  Write-Host "Acao: execute .\tools\package-msix-dev.ps1 para gerar um MSIX assinado valido."
}

if ($result.SignatureStatus -eq "Valid" -and -not $result.LocalMachineTrustedPeople) {
  Write-Host ""
  Write-Host "Acao: execute em PowerShell elevado:"
  Write-Host "  .\tools\trust-msix-dev-cert-admin.ps1"
}

if ($result.SignatureStatus -eq "Valid" -and $result.LocalMachineTrustedPeople -and -not $result.PackageInstalled) {
  Write-Host ""
  Write-Host "Acao: execute:"
  Write-Host "  .\tools\install-msix-dev.ps1 -Reinstall"
}

if ($result.PackageInstalled) {
  Write-Host ""
  Write-Host "Acao: execute:"
  Write-Host "  .\tools\smoke-msix-single-instance.ps1"
}
