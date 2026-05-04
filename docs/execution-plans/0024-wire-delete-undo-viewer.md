# Execucao 0024 - Ligar Delete e Ctrl+Z no viewer

## Objetivo

Conectar os atalhos `Delete` e `Ctrl+Z` ao viewer WinUI usando os use cases ja validados. A UI deve atualizar a foto atual, contadores e mensagens discretas sem mover regra de delete/undo para ViewModel ou code-behind.

Esta fatia nao implementa cache/prefetch completo, overlay temporizado, retry visual, single-instance, menu de contexto, API, PDV, upload, RAW ou segunda tela.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/delete-undo.md`
- `docs/execution-progress.md`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `src/Evydencia.PhotoSelector.App/ViewModels/MainPageViewModel.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/DeleteCurrentPhotoUseCase.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/UndoLastDeleteUseCase.cs`
- `tests/Evydencia.PhotoSelector.UiSmokeTests/ViewModels/MainPageViewModelTests.cs`

Documentacao oficial consultada:

- Microsoft Learn, `Keyboard events`, para foco, `KeyDown`, `Handled` e atalhos customizados.
- Microsoft Learn, `Keyboard accelerators`, para padrao de atalhos como `Ctrl+Z`.
- Microsoft Learn, `InputKeyboardSource.GetKeyStateForCurrentThread`, para detectar `Control` no thread atual em WinUI/Windows App SDK.

Observacao: `src/Evydencia.PhotoSelector.App/AGENTS.md` aponta para `.github/instructions`, mas essa pasta nao existe neste repositorio. A fatia segue as regras locais disponiveis.

## Camada afetada

- App
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

1. Adicionar metodos de ViewModel para aplicar resultado de delete/undo e status transitorio.
2. No `MainPage`, detectar `Delete` e `Ctrl+Z` no `OnViewerKeyDown`.
3. Para `Delete`, chamar `DeleteCurrentPhotoUseCase`, atualizar visualmente para a proxima foto assim que a sessao entra em `PendingDelete`, carregar a proxima imagem e depois aplicar o resultado final.
4. Para `Ctrl+Z`, chamar `UndoLastDeleteUseCase`, aplicar resultado e carregar a foto restaurada quando existir.
5. Evitar comandos de arquivo concorrentes nesta primeira ligacao.
6. Adicionar testes de ViewModel para estados de delete, missing, falha e undo.
7. Atualizar docs de progresso.

## Testes necessarios

- UiSmoke: `ApplyDeletePending` atualiza foto/counter.
- UiSmoke: delete final preserva imagem atual quando a foto atual nao muda.
- UiSmoke: missing mostra status discreto e contadores corretos.
- UiSmoke: undo restaurado navega para foto restaurada.
- UiSmoke: sem undo mostra mensagem sem limpar foto atual.

## Criterio de aceite

- `Delete` chama o fluxo real de delete e mostra a proxima foto.
- `Ctrl+Z` chama o fluxo real de undo e mostra a foto restaurada.
- Falhas de delete/undo aparecem como mensagem discreta no overlay.
- Build, format e testes passam.

## Riscos

- Operacoes concorrentes de delete/undo podem gerar corrida de estado; mitigacao: bloquear um comando de arquivo por vez nesta fatia.
- A remocao visual imediata depende do estado `PendingDelete` mutado pelo use case antes do primeiro await; mitigacao: teste de Application ja valida que o use case remove da navegacao antes do move, e esta fatia atualiza a UI logo apos iniciar a task.
- Overlay ainda nao auto-oculta; fica para UX premium.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~UiSmoke|FullyQualifiedName~Application|FullyQualifiedName~Integration"
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

- `MainPage` detecta `Delete` e `Ctrl+Z` no viewer.
- `Delete` chama `DeleteCurrentPhotoUseCase`, aplica estado visual otimista e carrega a proxima foto enquanto o move termina.
- `Ctrl+Z` chama `UndoLastDeleteUseCase` e carrega a foto restaurada quando existir.
- A UI bloqueia um comando de arquivo por vez nesta primeira ligacao para evitar corrida entre move/restore.
- `MainPageViewModel` aplica resultados de delete/undo sem mover regra de dominio para a UI.

## Validacao

- `tools/test.ps1 -Filter "FullyQualifiedName~UiSmoke|FullyQualifiedName~Application|FullyQualifiedName~Integration"` passou: Application 33, UiSmoke 10, Integration 7.
- `tools/format.ps1` passou.
- `tools/build.ps1` passou com 0 warnings.
- `tools/test.ps1` passou com 114 testes.

## Riscos remanescentes

- Overlay temporizado e retry visual de falha ainda nao foram implementados.
- Deletes muito rapidos continuam serializados por `_fileCommandInProgress`; burst mode/queue fica para a fase de performance/UX.
- Single-instance segue pendente e continua bloqueando o menu de contexto profissional do Explorer.
