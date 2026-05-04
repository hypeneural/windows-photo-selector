# Execucao 0026 - Validacao empacotada de single-instance e Launcher minimo

## Objetivo

Validar o `single-instance` com o app registrado/empacotado o suficiente para desbloquear o menu de contexto do Explorer. Nesta fatia tambem sera criado o `Evydencia.PhotoSelector.Launcher` minimo para receber caminhos do Explorer, normalizar/validar pasta e encaminhar para o app com `--folder`.

Esta fatia nao implementa `ShellExtension` C++/WinRT nem o menu moderno definitivo. O fallback de desenvolvimento por registro pode ser documentado/preparado, mas a extensao moderna fica para a proxima fatia.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/context-menu.md`
- `docs/execution-progress.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `src/Evydencia.PhotoSelector.App/Package.appxmanifest`
- `src/Evydencia.PhotoSelector.App/Evydencia.PhotoSelector.App.csproj`
- `src/Evydencia.PhotoSelector.App/Activation/AppInstanceCoordinator.cs`
- `src/Evydencia.PhotoSelector.Launcher/Program.cs`
- `src/Evydencia.PhotoSelector.Launcher/Evydencia.PhotoSelector.Launcher.csproj`
- `tools/*.ps1`

Documentacao oficial consultada:

- Microsoft Learn, `Create a single-instanced WinUI 3 app with C#`.
- Microsoft Learn, `Package your app using single-project MSIX`.
- Microsoft Learn, `Windows App SDK deployment guide for framework-dependent packaged apps`.
- Microsoft Learn, `Check for installed versions of the Windows App SDK runtime`.
- Microsoft Learn, `Integrate a packaged desktop app with File Explorer`.
- Microsoft Learn, `Integrate your desktop app with Windows using packaging extensions`.
- Microsoft Learn, `IExplorerCommand interface`.

## Camada afetada

- App
- Launcher
- ShellExtension
- Docs
- Tests

## Restricoes da tarefa

- V1 continua offline.
- Sem API Laravel.
- Sem PDV.
- Sem upload.
- Sem RAW.
- Sem Electron/WebView.
- Sem bloquear UI thread.
- Sem quebrar separacao de camadas.
- Sem excluir permanentemente por padrao.
- `Launcher` nao deve escanear pastas, decodificar imagens, mover arquivos ou escrever journal.

## Plano em passos pequenos

1. Validar o runtime Windows App SDK 2.0 disponivel no ambiente e instalar/registrar pacotes locais do NuGet se necessario.
2. Registrar o layout gerado do app com `Add-AppxPackage -Register` para validar package identity sem criar instalador final.
3. Executar smoke de duas ativacoes reais e confirmar que fica apenas uma instancia viva.
4. Implementar `Launcher` minimo com parser de argumentos, validacao de pasta e encaminhamento para o app.
5. Adicionar testes unitarios do parser/validacao do Launcher sem depender de Explorer.
6. Preparar scripts de contexto de desenvolvimento HKCU para `Directory` e `Directory\Background`, apontando para o Launcher.
7. Atualizar progresso e plano tecnico com o que foi validado e o que fica para `IExplorerCommand`/MSIX final.

## Testes necessarios

- Testes de Launcher para:
  - caminho por `--folder`;
  - caminho posicional;
  - caminho inexistente;
  - arquivo em vez de pasta;
  - argumentos gerados para o app.
- `tools/build.ps1`.
- `tools/test.ps1`.
- Smoke manual/automatizado:
  - app registrado;
  - primeira ativacao com `--folder`;
  - segunda ativacao com outra pasta;
  - no maximo um processo principal vivo.

## Criterio de aceite

- Runtime 2.0 e package identity sao verificados ou o bloqueio fica documentado.
- App registrado/empacotado inicia sem `REGDB_E_CLASSNOTREG`.
- Duas ativacoes nao deixam duas instancias principais vivas.
- `Launcher` aceita pasta valida e rejeita entradas invalidas sem tocar no pipeline de imagens.
- Scripts de menu de contexto dev existem e sao reversiveis.
- Build, format e testes passam.

## Riscos

- Instalar runtime MSIX pode depender de politicas do Windows. Mitigacao: usar os MSIX oficiais presentes no pacote NuGet e documentar falhas.
- `Add-AppxPackage -Register` pode exigir Developer Mode ou falhar por assinatura/dependencia. Mitigacao: registrar o erro e manter script de validacao repetivel.
- Validar Explorer moderno exige `ShellExtension`/`IExplorerCommand`; mitigacao: limitar esta fatia ao Launcher e fallback HKCU.
- O app UI pode exigir encerramento manual durante smoke. Mitigacao: script encerra processos do app ao final.

## Comandos a rodar

```powershell
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```

## Saida esperada no final

- Arquivos alterados.
- Camada alterada.
- Testes adicionados/atualizados.
- Comandos executados.
- Resultado do smoke registrado/empacotado.
- Impacto de performance.
- Riscos restantes.
- Desvios do plano ou de `AGENTS.md`, se houver.

## Resultado

- O Windows App Runtime 2.0.1 foi instalado a partir dos MSIX oficiais presentes no pacote NuGet local `Microsoft.WindowsAppSDK.Runtime`.
- O registro loose package com `Add-AppxPackage -Register` foi tentado, mas falhou por politica local de sideload/developer mode desabilitada (`0x80073CFF`).
- A sessao atual nao tem permissao para habilitar `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock`.
- O executavel direto continua falhando com `REGDB_E_CLASSNOTREG` no `DeploymentManager` enquanto o pacote nao esta registrado/instalado.
- `Evydencia.PhotoSelector.Launcher` foi implementado como broker leve para desenvolvimento e futura integracao Shell.
- Scripts HKCU de menu de contexto de desenvolvimento foram criados e validados com instalacao/remocao.

## Validacao

- `tools/format.ps1` passou.
- `tools/build.ps1` passou com 0 warnings.
- `tools/build.ps1 -Platform x64` passou com 0 warnings.
- `tools/test.ps1 -Filter "FullyQualifiedName~Launcher"` passou com 7 testes do Launcher.
- `tools/test.ps1` passou com 122 testes.
- `tools/install-context-menu-dev.ps1 -Platform x64` criou as chaves HKCU para pasta e fundo de pasta.
- `tools/uninstall-context-menu-dev.ps1` removeu as chaves HKCU.
- `Evydencia.PhotoSelector.Launcher.exe --help` retornou 0.
- `Evydencia.PhotoSelector.Launcher.exe --folder Z:\caminho-inexistente-evydencia` retornou 3.
- `Evydencia.PhotoSelector.Launcher.exe --folder %TEMP% --app Z:\app-inexistente.exe` retornou 4.

## Riscos remanescentes

- F0-19 continua pendente como validacao empacotada real, bloqueada por politica de sideload/developer mode ou por falta de instalador MSIX assinado confiavel.
- O menu moderno do Windows 11 ainda requer `ShellExtension` C++/WinRT com `IExplorerCommand` e manifest de extensao.
- O fallback HKCU e util para desenvolvimento, mas pode aparecer em `Mostrar mais opcoes` e nao substitui a integracao profissional.
