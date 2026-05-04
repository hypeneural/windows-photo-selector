$ErrorActionPreference = "Stop"

$menuKeyName = "EvydenciaPhotoSelector"
$targets = @(
  "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shell\$menuKeyName",
  "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\$menuKeyName"
)

foreach ($target in $targets) {
  if (Test-Path -LiteralPath $target) {
    Remove-Item -LiteralPath $target -Recurse -Force
  }
}

Write-Host "Menu de contexto dev removido de HKCU."
