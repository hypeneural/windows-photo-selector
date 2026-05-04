param(
  [string]$Configuration = "Release",
  [string]$Platform = "x64",
  [string]$CertificatePassword = "evydencia-dev-local",
  [string]$CertificateSubject = "CN=EvydenciaDev"
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Get-DotNet.ps1"
$dotnet = Get-EvydenciaDotNet

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $repoRoot "src\Evydencia.PhotoSelector.App\Evydencia.PhotoSelector.App.csproj"
$certificateDir = Join-Path $repoRoot "packaging\certificates-dev"
$packageDir = Join-Path $repoRoot "artifacts\packages"
$pfxPath = Join-Path $certificateDir "EvydenciaPhotoSelectorDev.pfx"
$cerPath = Join-Path $certificateDir "EvydenciaPhotoSelectorDev.cer"

New-Item -ItemType Directory -Force -Path $certificateDir | Out-Null
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

$securePassword = ConvertTo-SecureString -String $CertificatePassword -Force -AsPlainText

function Add-CertificateToCurrentUserStore {
  param(
    [string]$Path,
    [System.Security.Cryptography.X509Certificates.StoreName]$StoreName
  )

  $certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($Path)
  $store = [System.Security.Cryptography.X509Certificates.X509Store]::new(
    $StoreName,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser)
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

function Import-PfxCertificateToCurrentUserMy {
  param(
    [string]$Path,
    [string]$Password
  )

  $flags = [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable `
    -bor [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet `
    -bor [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::UserKeySet
  $certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new(
    $Path,
    $Password,
    $flags)

  $store = [System.Security.Cryptography.X509Certificates.X509Store]::new(
    [System.Security.Cryptography.X509Certificates.StoreName]::My,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser)
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

  return $certificate.Thumbprint
}

if (-not (Test-Path -LiteralPath $pfxPath)) {
  Write-Host "Criando certificado dev MSIX em CurrentUser\\My..."
  $certificate = New-SelfSignedCertificate `
    -Type Custom `
    -Subject $CertificateSubject `
    -FriendlyName "Evydencia Escolher Fotos Dev MSIX" `
    -KeyUsage DigitalSignature `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")

  Export-PfxCertificate -Cert $certificate -FilePath $pfxPath -Password $securePassword | Out-Null
  Export-Certificate -Cert $certificate -FilePath $cerPath | Out-Null
} elseif (-not (Test-Path -LiteralPath $cerPath)) {
  Write-Host "Exportando certificado publico dev a partir do PFX..."
  $certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
    $pfxPath,
    $CertificatePassword,
    [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
  [IO.File]::WriteAllBytes($cerPath, $certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
}

$certificateThumbprint = Import-PfxCertificateToCurrentUserMy -Path $pfxPath -Password $CertificatePassword

Write-Host "Confiando certificado dev em CurrentUser\\TrustedPeople..."
Add-CertificateToCurrentUserStore -Path $cerPath -StoreName TrustedPeople
Write-Host "Confiando certificado dev em CurrentUser\\Root..."
Add-CertificateToCurrentUserStore -Path $cerPath -StoreName Root
Write-Host "Assinando MSIX com certificado CurrentUser\\My thumbprint $certificateThumbprint."

$runtimeIdentifier = "win-$Platform"
$msbuildArgs = @(
  "publish",
  $project,
  "-c",
  $Configuration,
  "-p:Platform=$Platform",
  "-p:RuntimeIdentifier=$runtimeIdentifier",
  "-p:GenerateAppxPackageOnBuild=true",
  "-p:AppxBundle=Never",
  "-p:AppxPackageDir=$packageDir\",
  "-p:AppxPackageSigningEnabled=true",
  "-p:PackageCertificateThumbprint=$certificateThumbprint",
  "-p:PublishReadyToRun=false",
  "-p:PublishTrimmed=false"
)

Write-Host "Gerando MSIX dev assinado..."
& $dotnet @msbuildArgs
if ($LASTEXITCODE -ne 0) {
  throw "Falha ao gerar MSIX dev. dotnet publish retornou $LASTEXITCODE."
}

Get-ChildItem -Path $packageDir -Recurse -Include *.msix,*.msixbundle,*.appx,*.appxbundle |
  Sort-Object LastWriteTime -Descending |
  Select-Object -First 10 FullName, Length, LastWriteTime
