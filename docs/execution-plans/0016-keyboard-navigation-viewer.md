# Execucao 0016 - Navegacao visual por setas

## Objetivo

Implementar navegacao visual inicial no viewer usando teclado: `Right`/`Space` para proxima foto e `Left` para foto anterior. A troca deve atualizar estado visual, contador e carregar a nova imagem pelo pipeline seguro de decode dimensionado, cancelando decode anterior quando o usuario navegar rapidamente.

Esta fatia nao implementa Delete/Undo, cache/prefetch completo, fullscreen, API, PDV, RAW, upload ou menu de contexto.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.Application/UseCases/NavigateNextPhotoUseCase.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/NavigatePreviousPhotoUseCase.cs`
- `src/Evydencia.PhotoSelector.Core/Navigation/NavigationController.cs`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `src/Evydencia.PhotoSelector.App/ViewModels/MainPageViewModel.cs`
- `tests/Evydencia.PhotoSelector.UiSmokeTests/ViewModels/MainPageViewModelTests.cs`
- `docs/execution-progress.md`

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

1. Fazer `MainPage` manter a sessao aberta atual e resolver use cases de navegacao via DI.
2. Tornar o host do viewer focavel para receber eventos de teclado.
3. Capturar `VirtualKey.Right`, `VirtualKey.Space` e `VirtualKey.Left`, marcando o evento como handled.
4. Ignorar atalhos quando nao houver sessao ativa.
5. Aplicar `NavigationResult` no `MainPageViewModel` sem tipos WinUI.
6. Chamar o carregamento da foto atual apos cada navegacao, reutilizando o cancelamento ja existente.
7. Atualizar testes de ViewModel para contador e foto atual apos navegacao.
8. Atualizar docs de progresso e plano principal.

## Testes necessarios

- ViewModel aplica resultado de navegacao e atualiza foto atual, contador, nome e status.
- Testes existentes de `NavigateNextPhotoUseCase`/`NavigatePreviousPhotoUseCase` continuam passando.
- Build valida XAML/code-behind e eventos de teclado.

## Criterio de aceite

- `Right` carrega a proxima foto.
- `Space` carrega a proxima foto.
- `Left` carrega a foto anterior.
- Contador muda junto com a foto.
- Decode anterior e cancelado antes de carregar a nova foto.
- Build, format e testes passam.
- `F2-04` fica marcado como concluido.

## Riscos

- Eventos de teclado podem nao disparar se o viewer nao estiver focado; mitigacao: `ViewerHost` focavel e `Focus(FocusState.Programmatic)` apos abrir sessao.
- Holding de seta pode gerar muitas tarefas; mitigacao: cancelamento da `CancellationTokenSource` antes de cada novo decode.
- ViewModel pode ficar acoplado ao WinUI; mitigacao: manter apenas estado puro no ViewModel e deixar `ImageSource`/`Visibility` no App.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~UiSmoke"
.\tools\build.ps1
.\tools\format.ps1
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

- `MainPage` agora mantem a `PhotoSession` atual, captura `Right`, `Space` e `Left` no `ViewerHost` e usa `NavigateNextPhotoUseCase`/`NavigatePreviousPhotoUseCase` via DI.
- A cada navegacao, a tela atualiza foto, contador e status pelo `MainPageViewModel`, limpa a imagem anterior e chama novamente o pipeline de decode dimensionado.
- O decode anterior e cancelado antes de iniciar o novo carregamento, reaproveitando a `CancellationTokenSource` da fatia 0015.
- `ViewerHost` e focavel e recebe foco programatico apos abrir sessao/navegar.
- `MainPageViewModel` ganhou `ApplyNavigation` mantendo estado puro, sem dependencia de tipos WinUI.
- `F2-04` foi marcado como concluido em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~UiSmoke"
.\tools\build.ps1
.\tools\format.ps1
.\tools\test.ps1
.\tools\build.ps1
```

Resultados:

- Application: 17 testes aprovados.
- UiSmoke: 4 testes aprovados.
- Suite completa: 63 testes aprovados.
- Build completo: aprovado, 0 warnings.
- Format: aprovado apos normalizar CRLF dos arquivos alterados.

## Pendencias deliberadas

- `Home`, `End`, `Delete` e `Ctrl+Z` ficam fora desta fatia.
- Fullscreen limpo continua em `F2-05`.
- Prefetch e cache LRU ainda nao foram implementados; a navegacao atual cancela decode anterior, mas ainda nao tem cache para resposta instantanea.
