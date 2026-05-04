# Menu de contexto Windows

## Status atual

A V1 ainda nao tem `ShellExtension` C++/WinRT nem `IExplorerCommand` moderno. A fatia atual prepara o caminho de desenvolvimento:

- `Evydencia.PhotoSelector.Launcher` recebe a pasta do Explorer;
- valida se o caminho e uma pasta existente;
- inicia `Evydencia.PhotoSelector.App.exe` com `--folder "<pasta>" --source explorer`;
- nao escaneia fotos, nao decodifica imagens, nao move arquivos e nao escreve journal.

## Fallback de desenvolvimento

Instalar:

```powershell
.\tools\install-context-menu-dev.ps1 -Platform x64
```

Remover:

```powershell
.\tools\uninstall-context-menu-dev.ps1
```

O script registra chaves em `HKCU`:

- `Software\Classes\Directory\shell\EvydenciaPhotoSelector`
- `Software\Classes\Directory\Background\shell\EvydenciaPhotoSelector`

Esse fallback pode aparecer no menu classico ou em `Mostrar mais opcoes`, dependendo da versao/configuracao do Windows.

## Caminho profissional

Para o menu moderno do Windows 11, o plano continua sendo:

1. app com package identity;
2. MSIX ou sparse package;
3. extensao COM leve com `IExplorerCommand`;
4. manifest com extensoes de File Explorer;
5. instalador assinado;
6. logs minimos de ativacao Shell.

O `ShellExtension` deve permanecer minimo e chamar o Launcher/AppActivation. Ele nao deve carregar `Core`, `Imaging`, `Storage` nem pipeline de viewer.

## Validacao pendente

O registro loose package com `Add-AppxPackage -Register` falhou no ambiente atual por politica de sideload/developer mode desabilitada. A validacao final do menu moderno depende de:

- habilitar Developer Mode/sideload ou usar instalador MSIX assinado confiavel;
- registrar/instalar o pacote;
- validar duas ativacoes reais;
- implementar `IExplorerCommand`.
