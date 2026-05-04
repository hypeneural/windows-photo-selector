# Execucao 0029 - Preview dev do viewer e investigacao de alias MSIX

## Objetivo

Abrir o viewer para validacao visual com uma pasta real de JPEGs e continuar a validacao F0-19 sem iniciar `ShellExtension`. Como a ativacao empacotada pelo `.exe` em `WindowsApps` falhou com acesso negado, a fatia tambem registra o caminho oficial de `appExecutionAlias` e uma rota unpackaged dev para preview visual.

## Arquivos que serao lidos

- `docs/execution-progress.md`
- `src/Evydencia.PhotoSelector.App/Package.appxmanifest`
- `tools/package-msix-dev.ps1`
- `tools/install-msix-dev.ps1`
- `tools/smoke-msix-single-instance.ps1`

Documentacao oficial consultada:

- Microsoft Learn, `IApplicationActivationManager::ActivateApplication`: https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-iapplicationactivationmanager-activateapplication
- Microsoft Learn, `AppExecutionAlias`: https://learn.microsoft.com/uwp/schemas/appxpackage/uapmanifestschema/element-uap5-appexecutionalias
- Microsoft Learn, `Prepare to package a desktop application`: https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-prepare
- Microsoft Learn, `Invoke-CommandInDesktopPackage`: https://learn.microsoft.com/powershell/module/appx/invoke-commandindesktoppackage

## Camada afetada

- App manifest
- Tools
- Packaging
- Docs

## Restricoes

- V1 continua offline e JPEG-only.
- Sem API, PDV, upload, RAW, Electron ou WebView.
- Nao implementar `ShellExtension` ainda.

## Resultado

- Pasta real validada: `C:\Users\Usuario\Desktop\bkp geral\BKP 1712\SD 3\DCIM\100CAROL`.
- Viewer aberto para preview usando publish unpackaged dev em `artifacts\unpackaged-debug`.
- MSIX dev instalado com sucesso depois de confiar certificado em `LocalMachine\TrustedPeople`.
- `IApplicationActivationManager.ActivateApplication` retornou `0x80270254` para o pacote full-trust.
- `Invoke-CommandInDesktopPackage` falhou no ambiente atual com `AccessViolationException` em `daxexec.dll`.
- `appExecutionAlias` foi adicionado ao manifest para preparar o caminho de launch por alias, mas o stub `WindowsApps` ainda nao executou corretamente a partir da sessao Codex.

## Decisao

F0-19 ainda so deve ser marcado como concluido quando o smoke de duas ativacoes do app empacotado passar. O preview unpackaged e apenas uma rota de desenvolvimento para avaliar UX e imagem enquanto o launch empacotado e investigado.
