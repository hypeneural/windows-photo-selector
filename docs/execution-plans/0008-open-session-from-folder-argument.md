# Execucao 0008 - Abrir sessao a partir do argumento --folder

## Objetivo

Usar o argumento `--folder` para abrir uma sessao real pela camada `Application`, ainda sem criar o viewer completo. A abertura deve ser assincrona, nao bloquear o `OnLaunched` do WinUI e retornar falhas como estado controlado para a UI futura.

## Arquivos lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `src/Evydencia.PhotoSelector.Application/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.Application/Activation/FolderLaunchArgumentsParser.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/OpenSessionUseCase.cs`
- `src/Evydencia.PhotoSelector.App/App.xaml.cs`
- `docs/execution-progress.md`

## Camada afetada

- Application
- App
- Infrastructure
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

1. Criar status e resultado para abertura por argumento.
2. Criar `OpenFolderFromArgumentsUseCase` em `Application`.
3. Fazer o use case retornar `NoFolderArgument`, `Opened` ou `Failed`.
4. Registrar o novo use case no DI.
5. Atualizar o `App.xaml.cs` para iniciar a abertura assincrona quando houver argumentos.
6. Adicionar testes unitarios para sem pasta, pasta valida e falha do scanner.
7. Atualizar o teste de integracao de DI.
8. Atualizar `execution-progress.md`.

## Testes necessarios

- `Application.Tests` para o novo use case.
- `IntegrationTests` para resolucao via DI.
- Build completo.
- Testes completos.
- Format check.

## Criterio de aceite

- `OpenFolderFromArgumentsUseCase` abre uma `PhotoSession` quando recebe `--folder`.
- Sem `--folder`, o scanner nao e chamado.
- Falha de scanner retorna `Failed`, sem excecao nao observada.
- `App.xaml.cs` inicia a abertura sem bloquear a criacao da janela.
- Build, testes e format check passam.

## Riscos

- Abrir pasta no UI thread: mitigar usando task assincrona iniciada no launch.
- Excecao perdida no startup: mitigar encapsulando falhas no resultado do use case.
- Colocar filesystem na UI: mitigar chamando apenas `Application`.
- Tornar o App dificil de testar: mitigar mantendo logica pesada no use case testado.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `OpenFolderFromArgumentsStatus` criado com estados `NoFolderArgument`, `Opened` e `Failed`.
- [x] `OpenFolderFromArgumentsResult` criado para transportar argumentos, sessao aberta e erro controlado.
- [x] `OpenFolderFromArgumentsUseCase` criado em `Application`.
- [x] Sem `--folder`, o use case retorna `NoFolderArgument` e nao chama o scanner.
- [x] Com `--folder`, o use case abre `PhotoSession` usando `OpenSessionUseCase`.
- [x] Falha de scanner retorna `Failed`, sem excecao nao observada no startup.
- [x] `App.xaml.cs` inicia a abertura em `InitialSessionOpenTask` via `Task.Run`, sem scan no UI thread antes de criar/ativar a janela.
- [x] DI atualizado para resolver o novo use case.
- [x] `Application.Tests` recebeu 3 testes para abertura por argumento.
- [x] `IntegrationTests` valida resolucao do novo use case pelo container.
- [x] `tools/build.ps1` executado com sucesso, sem warnings.
- [x] `tools/test.ps1` executado com sucesso, 50 testes passando.
- [x] `tools/format.ps1` executado com sucesso.

## Observacoes

- Esta fatia ainda nao mostra a foto na UI. Ela apenas garante que o fluxo de ativacao ja consegue criar a sessao local em background.
- A documentacao oficial do Windows App SDK continua relevante para esta area: `LaunchActivatedEventArgs.Arguments` nao deve ser usado em apps desktop WinUI; os argumentos sao lidos por `Environment.GetCommandLineArgs()`.
