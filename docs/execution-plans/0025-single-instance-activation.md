# Execucao 0025 - Single-instance e reativacao por pasta

## Objetivo

Implementar a primeira fatia de single-instance para o app WinUI 3. Uma segunda ativacao deve ser redirecionada para a instancia principal antes de criar nova janela, e a instancia principal deve receber os argumentos de pasta para abrir/substituir a sessao.

Esta fatia nao implementa `ShellExtension`, registro de menu de contexto, MSIX final, API, PDV, upload, RAW, cache pesado ou segunda tela.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/context-menu.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/App.xaml.cs`
- `src/Evydencia.PhotoSelector.App/Evydencia.PhotoSelector.App.csproj`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/OpenFolderFromArgumentsUseCase.cs`
- `src/Evydencia.PhotoSelector.Application/Activation/FolderLaunchArgumentsParser.cs`
- `docs/execution-progress.md`

Documentacao oficial consultada:

- Microsoft Learn, `Create a single-instanced WinUI 3 app with C#`.
- Microsoft Learn, `Application lifecycle functionality migration`.
- Microsoft Learn, `Rich activation with the app lifecycle API`.
- Microsoft Learn, `AppInstance` e `RedirectActivationToAsync`.

## Camada afetada

- App
- Application
- Tests
- Docs

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

## Plano em passos pequenos

1. Adicionar `DISABLE_XAML_GENERATED_MAIN` ao projeto WinUI.
2. Criar `Program.cs` customizado com `WinRT.ComWrappersSupport.InitializeComWrappers`, `DispatcherQueueSynchronizationContext` e redirecionamento por `AppInstance`.
3. Criar `AppInstanceCoordinator` para registrar a chave da instancia principal e redirecionar ativacoes secundarias.
4. Ler argumentos de ativacao `Launch` por `AppInstance.GetActivatedEventArgs`/`ILaunchActivatedEventArgs`.
5. Adicionar overload raw em `OpenFolderFromArgumentsUseCase` para reaproveitar o parser ja existente.
6. Fazer `App` assinar `AppInstance.Activated`, ativar a janela principal e emitir evento interno com os argumentos recebidos.
7. Fazer `MainPage` tratar nova pasta: se nao houver sessao, abrir; se houver sessao ativa, perguntar antes de substituir.
8. Atualizar progresso e plano tecnico.

## Testes necessarios

- `Application.Tests`: `OpenFolderFromArgumentsUseCase.ExecuteRawAsync` abre caminho com espacos.
- Build do app valida o `Program.cs` customizado e o simbolo `DISABLE_XAML_GENERATED_MAIN`.
- Testes completos garantem que abertura inicial, viewer e delete/undo nao regrediram.

## Criterio de aceite

- O app compila com `Program.cs` customizado.
- A decisao de redirecionamento acontece antes de criar a janela.
- Segunda ativacao usa `RedirectActivationToAsync`.
- Instancia principal recebe argumentos de launch redirecionados.
- `--folder` continua funcionando na abertura inicial.
- Build, format e testes passam.

## Riscos

- App lifecycle APIs podem variar por empacotamento/arquitetura; mitigacao: seguir Microsoft Learn e validar build local com Windows App SDK 2.0.1.
- Teste automatizado de duas instancias reais e dificil sem app empacotado; mitigacao: deixar smoke manual documentado e cobrir parser/use case por teste unitario.
- Dialogo de substituicao ainda e simples; refinamento visual fica para UX premium.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~OpenFolderFromArgumentsUseCase"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```

## Saida esperada no final

- Arquivos alterados.
- Camada alterada.
- Testes adicionados/atualizados.
- Comandos executados.
- Impacto de performance.
- Riscos restantes.
- Desvios do plano ou de `AGENTS.md`, se houver.

## Resultado

- `Program.cs` customizado inicia WinUI com `DISABLE_XAML_GENERATED_MAIN`.
- `AppInstanceCoordinator` registra a chave `Evydencia.PhotoSelector.Main`.
- Instancias secundarias chamam `RedirectActivationToAsync` e encerram antes de criar janela.
- A instancia principal assina `AppInstance.Activated`, reativa a janela e envia argumentos `Launch` para a `MainPage`.
- `MainPage` abre a pasta recebida se nao houver sessao, ou pergunta antes de substituir uma sessao ativa.
- Reativacoes repetidas enquanto existe comando/dialogo em andamento ficam bloqueadas com status discreto, evitando multiplos dialogos de substituicao.
- `OpenFolderFromArgumentsUseCase` ganhou `ExecuteRawAsync` para reaproveitar `FolderLaunchArgumentsParser.ParseRaw`.

## Validacao

- `tools/test.ps1 -Filter "FullyQualifiedName~OpenFolderFromArgumentsUseCase"` passou: Application 4 testes.
- `tools/format.ps1` passou.
- `tools/build.ps1` passou com 0 warnings.
- `tools/test.ps1` passou com 115 testes.

## Smoke runtime

- Tentativa de smoke por executavel direto foi inconclusiva: o processo encerrou com `0xE0434352` e `REGDB_E_CLASSNOTREG`.
- `Get-AppxPackage Microsoft.WindowsAppRuntime*` mostrou runtimes ate 1.8 no ambiente.
- `winget search WindowsAppRuntime` tambem listou runtime ate 1.8.
- A validacao real de duas ativacoes precisa ser repetida com Windows App Runtime 2.0 registrado ou com o app empacotado/registrado.

## Riscos remanescentes

- Validacao real de duas instancias ainda depende de runtime/packaging.
- A confirmacao visual de substituicao de sessao usa `ContentDialog` simples; refinamento de UX fica para etapa posterior.
- Menu de contexto moderno continua pendente de `ShellExtension`/MSIX.
