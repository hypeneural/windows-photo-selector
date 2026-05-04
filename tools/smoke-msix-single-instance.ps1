param(
  [int]$StartupDelayMilliseconds = 2500
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\msix-activation.ps1"

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

$sessionA = New-SmokeSession -Name "a"
$sessionB = New-SmokeSession -Name "b"

try {
  Get-Process -Name $packageName -ErrorAction SilentlyContinue | Stop-Process -Force

  Write-Host "Iniciando primeira ativacao instalada..."
  $firstArguments = "--folder $(ConvertTo-CommandLineArgument $sessionA) --source $(ConvertTo-CommandLineArgument "msix-smoke-a")"
  $firstProcessId = Invoke-EvydenciaPackagedAppActivation -Arguments $firstArguments
  Write-Host "PID retornado na primeira ativacao: $firstProcessId"
  Start-Sleep -Milliseconds $StartupDelayMilliseconds

  $countAfterFirst = Get-AppProcessCount
  Write-Host "Processos apos primeira ativacao: $countAfterFirst"
  if ($countAfterFirst -ne 1) {
    throw "Esperava exatamente 1 processo apos a primeira ativacao, mas encontrei $countAfterFirst."
  }

  Write-Host "Iniciando segunda ativacao instalada..."
  $secondArguments = "--folder $(ConvertTo-CommandLineArgument $sessionB) --source $(ConvertTo-CommandLineArgument "msix-smoke-b")"
  $secondProcessId = Invoke-EvydenciaPackagedAppActivation -Arguments $secondArguments
  Write-Host "PID retornado na segunda ativacao: $secondProcessId"
  Start-Sleep -Milliseconds $StartupDelayMilliseconds

  $countAfterSecond = Get-AppProcessCount
  Write-Host "Processos apos segunda ativacao: $countAfterSecond"
  if ($countAfterSecond -ne 1) {
    throw "Single-instance falhou: esperava 1 processo vivo apos duas ativacoes, mas encontrei $countAfterSecond."
  }

  Write-Host "Smoke MSIX single-instance aprovado."
}
finally {
  Get-Process -Name $packageName -ErrorAction SilentlyContinue | Stop-Process -Force
  Remove-Item -LiteralPath $sessionA -Recurse -Force -ErrorAction SilentlyContinue
  Remove-Item -LiteralPath $sessionB -Recurse -Force -ErrorAction SilentlyContinue
}
