param(
  [string]$Configuration = "Debug",
  [string]$Platform = "x64",
  [string]$LauncherPath = "",
  [string]$AppPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

if ($LauncherPath -eq "") {
  $launcherCandidates = @(
    (Join-Path $repoRoot "src\Evydencia.PhotoSelector.Launcher\bin\$Platform\$Configuration\net10.0-windows10.0.19041.0\Evydencia.PhotoSelector.Launcher.exe"),
    (Join-Path $repoRoot "src\Evydencia.PhotoSelector.Launcher\bin\$Configuration\net10.0-windows10.0.19041.0\Evydencia.PhotoSelector.Launcher.exe")
  )
  $LauncherPath = $launcherCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if ($AppPath -eq "") {
  $AppPath = Join-Path $repoRoot "src\Evydencia.PhotoSelector.App\bin\$Platform\$Configuration\net10.0-windows10.0.19041.0\win-$Platform\Evydencia.PhotoSelector.App.exe"
}

if ($null -eq $LauncherPath -or -not (Test-Path -LiteralPath $LauncherPath)) {
  throw "Launcher nao encontrado. Execute .\tools\build.ps1 ou informe -LauncherPath. Caminho: $LauncherPath"
}

if (-not (Test-Path -LiteralPath $AppPath)) {
  throw "App nao encontrado. Execute build para a plataforma $Platform ou informe -AppPath. Caminho: $AppPath"
}

$menuKeyName = "EvydenciaPhotoSelector"
$targets = @(
  @{
    Path = "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shell\$menuKeyName"
    Token = "%1"
  },
  @{
    Path = "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\$menuKeyName"
    Token = "%V"
  }
)

foreach ($target in $targets) {
  $keyPath = $target.Path
  $commandPath = Join-Path $keyPath "command"
  New-Item -Path $commandPath -Force | Out-Null
  New-ItemProperty -Path $keyPath -Name "MUIVerb" -PropertyType String -Value "Abrir Escolher Fotos" -Force | Out-Null
  New-ItemProperty -Path $keyPath -Name "Icon" -PropertyType String -Value $AppPath -Force | Out-Null
  Set-Item -Path $commandPath -Value "`"$LauncherPath`" --folder `"$($target.Token)`" --source explorer --app `"$AppPath`""
}

Write-Host "Menu de contexto dev instalado em HKCU:"
Write-Host "- Clique em pasta: Abrir Escolher Fotos"
Write-Host "- Clique no fundo da pasta: Abrir Escolher Fotos"
