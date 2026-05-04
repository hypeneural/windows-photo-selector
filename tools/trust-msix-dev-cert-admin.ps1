param(
  [string]$CertificatePath = ""
)

$ErrorActionPreference = "Stop"

function Test-IsAdministrator {
  $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
  $principal = [Security.Principal.WindowsPrincipal]::new($identity)
  return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Add-CertificateToLocalMachineStore {
  param(
    [string]$Path,
    [System.Security.Cryptography.X509Certificates.StoreName]$StoreName
  )

  $certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($Path)
  $store = [System.Security.Cryptography.X509Certificates.X509Store]::new(
    $StoreName,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
  $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
  try {
    $existing = $store.Certificates.Find(
      [System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,
      $certificate.Thumbprint,
      $false)
    if ($existing.Count -eq 0) {
      $store.Add($certificate)
    }
  }
  finally {
    $store.Close()
  }
}

if (-not (Test-IsAdministrator)) {
  throw "Execute este script em um PowerShell elevado. O Windows exige confianca em LocalMachine para instalar o MSIX dev assinado."
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if ($CertificatePath -eq "") {
  $CertificatePath = Join-Path $repoRoot "packaging\certificates-dev\EvydenciaPhotoSelectorDev.cer"
}

if (-not (Test-Path -LiteralPath $CertificatePath)) {
  throw "Certificado nao encontrado: $CertificatePath. Execute .\tools\package-msix-dev.ps1 primeiro."
}

Add-CertificateToLocalMachineStore -Path $CertificatePath -StoreName Root
Add-CertificateToLocalMachineStore -Path $CertificatePath -StoreName TrustedPeople

Write-Host "Certificado dev confiado em LocalMachine\\Root e LocalMachine\\TrustedPeople."
