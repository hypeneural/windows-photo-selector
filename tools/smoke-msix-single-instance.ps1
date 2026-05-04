param(
  [int]$StartupDelayMilliseconds = 2500
)

$ErrorActionPreference = "Stop"

$packageName = "Evydencia.PhotoSelector.App"
$base64Jpeg =
  "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAP//////////////////////////////////////////////////////////////////////////////////////2wBDAf//////////////////////////////////////////////////////////////////////////////////////wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAX/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIQAxAAAAH/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/9oACAEBAAEFAqf/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oACAEDAQE/ASP/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oACAECAQE/ASP/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/9oACAEBAAY/Asf/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/9oACAEBAAE/IV//2gAMAwEAAgADAAAAEP/EFBQRAQAAAAAAAAAAAAAAAAAAABD/2gAIAQMBAT8QH//EFBQRAQAAAAAAAAAAAAAAAAAAABD/2gAIAQIBAT8QH//EFBQBAQAAAAAAAAAAAAAAAAAAABD/2gAIAQEAAT8QH//Z"

function New-SmokeSession {
  param([string]$Name)

  $folder = Join-Path ([IO.Path]::GetTempPath()) "evydencia-msix-smoke-$Name-$([Guid]::NewGuid().ToString("N"))"
  New-Item -ItemType Directory -Force -Path $folder | Out-Null
  [IO.File]::WriteAllBytes((Join-Path $folder "IMG_0001.jpg"), [Convert]::FromBase64String($base64Jpeg))
  return $folder
}

function Get-AppProcessCount {
  return @(Get-Process -Name $packageName -ErrorAction SilentlyContinue).Count
}

$package = Get-AppxPackage -Name $packageName -ErrorAction SilentlyContinue
if ($null -eq $package) {
  throw "Pacote $packageName nao instalado. Execute .\tools\package-msix-dev.ps1 e .\tools\install-msix-dev.ps1 -Reinstall."
}

$appExe = Join-Path $package.InstallLocation "$packageName.exe"
if (-not (Test-Path -LiteralPath $appExe)) {
  throw "Executavel instalado nao encontrado: $appExe"
}

$sessionA = New-SmokeSession -Name "a"
$sessionB = New-SmokeSession -Name "b"

try {
  Get-Process -Name $packageName -ErrorAction SilentlyContinue | Stop-Process -Force

  Write-Host "Iniciando primeira ativacao instalada..."
  $first = Start-Process -FilePath $appExe -ArgumentList @("--folder", "`"$sessionA`"", "--source", "msix-smoke-a") -PassThru
  Start-Sleep -Milliseconds $StartupDelayMilliseconds

  $countAfterFirst = Get-AppProcessCount
  Write-Host "Processos apos primeira ativacao: $countAfterFirst"
  if ($countAfterFirst -ne 1) {
    throw "Esperava exatamente 1 processo apos a primeira ativacao, mas encontrei $countAfterFirst."
  }

  Write-Host "Iniciando segunda ativacao instalada..."
  $second = Start-Process -FilePath $appExe -ArgumentList @("--folder", "`"$sessionB`"", "--source", "msix-smoke-b") -PassThru
  Start-Sleep -Milliseconds $StartupDelayMilliseconds

  $countAfterSecond = Get-AppProcessCount
  Write-Host "Processos apos segunda ativacao: $countAfterSecond"
  if ($countAfterSecond -ne 1) {
    throw "Single-instance falhou: esperava 1 processo vivo apos duas ativacoes, mas encontrei $countAfterSecond."
  }

  $second.Refresh()
  if (-not $second.HasExited) {
    Write-Host "A segunda ativacao foi redirecionada, mas o processo secundario ainda esta vivo; aguardando encerramento curto..."
    $second.WaitForExit(3000) | Out-Null
    $second.Refresh()
  }

  if (-not $second.HasExited) {
    throw "O processo secundario nao encerrou apos redirecionar a ativacao."
  }

  Write-Host "Smoke MSIX single-instance aprovado."
}
finally {
  Get-Process -Name $packageName -ErrorAction SilentlyContinue | Stop-Process -Force
  Remove-Item -LiteralPath $sessionA -Recurse -Force -ErrorAction SilentlyContinue
  Remove-Item -LiteralPath $sessionB -Recurse -Force -ErrorAction SilentlyContinue
}
