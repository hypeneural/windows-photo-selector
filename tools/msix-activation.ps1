$ErrorActionPreference = "Stop"

function ConvertTo-CommandLineArgument {
  param([string]$Value)

  return '"' + $Value.Replace('"', '\"') + '"'
}

function Invoke-EvydenciaPackagedAppActivation {
  param(
    [string]$Arguments
  )

  $package = Get-AppxPackage -Name "Evydencia.PhotoSelector.App" -ErrorAction SilentlyContinue
  if ($null -eq $package) {
    throw "Pacote Evydencia.PhotoSelector.App nao instalado. Execute .\tools\install-msix-dev.ps1 -Reinstall."
  }

  $alias = Get-Command "EvydenciaPhotoSelector.exe" -ErrorAction SilentlyContinue
  if ($null -eq $alias) {
    throw "Alias EvydenciaPhotoSelector.exe nao encontrado. Reinstale o MSIX depois de gerar pacote com appExecutionAlias."
  }

  try {
    $process = Start-Process -FilePath $alias.Source -ArgumentList $Arguments -PassThru
    return $process.Id
  }
  catch {
    throw "Falha ao iniciar pelo appExecutionAlias $($alias.Source). O pacote esta instalado, mas o Windows nao executou o alias nesta sessao: $($_.Exception.Message)"
  }
}
