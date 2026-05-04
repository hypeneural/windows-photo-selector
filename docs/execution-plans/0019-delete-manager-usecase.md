# Execucao 0019 - DeleteManager e DeleteCurrentPhotoUseCase

## Objetivo

Implementar o fluxo de delete sem UI: `DeleteManager` no Core para transicoes `PendingDelete`, `Deleted` e `DeleteFailed`, e `DeleteCurrentPhotoUseCase` na Application para orquestrar o move via `IFileMoveService`. A fatia deve validar estado, contadores e navegacao antes de ligar a tecla `Delete` no viewer.

Esta fatia nao implementa `Ctrl+Z`, `UndoManager`, journal JSONL, overlay de erro, cache, menu de contexto, single-instance, API, PDV, RAW ou upload.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/delete-undo.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `src/Evydencia.PhotoSelector.Core/AGENTS.md`
- `src/Evydencia.PhotoSelector.Application/AGENTS.md`
- `tests/AGENTS.md`
- `docs/adr/0002-delete-mode.md`
- `docs/execution-progress.md`
- `src/Evydencia.PhotoSelector.Core/Photos/PhotoItem.cs`
- `src/Evydencia.PhotoSelector.Core/Photos/PhotoStatus.cs`
- `src/Evydencia.PhotoSelector.Core/Sessions/PhotoSession.cs`
- `src/Evydencia.PhotoSelector.Core/Navigation/NavigationController.cs`
- `src/Evydencia.PhotoSelector.Application/Abstractions/IFileMoveService.cs`
- `src/Evydencia.PhotoSelector.Application/Models/FileMoveResult.cs`
- `src/Evydencia.PhotoSelector.Storage/Filesystem/FileMoveService.cs`

Documentacao oficial consultada:

- Microsoft Learn, `File.Move`, para confirmar semantica de destino existente.
- Microsoft Learn, cancellation cooperativo em .NET e `CancellationToken.ThrowIfCancellationRequested`.

## Camada afetada

- Core
- Application
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

1. Criar `DeleteManager` em `Core/Deletion`.
2. Criar resultados de dominio para requisicao e conclusao do delete.
3. `RequestDeleteCurrent` deve marcar a foto atual como `PendingDelete`, remover da navegacao ativa e apontar a proxima foto.
4. `CompleteDelete` deve marcar `Deleted` e manter a foto atual coerente.
5. `FailDelete` deve marcar `DeleteFailed`, reinserir a foto como navegavel e preservar a foto preferida atual quando possivel.
6. Criar `DeleteCurrentPhotoUseCase` na Application usando `DeleteManager` e `IFileMoveService`.
7. Criar resultado de Application com status `NoCurrentPhoto`, `Deleted`, `DeleteFailed`.
8. Em sucesso do move, confirmar `Deleted`.
9. Em falha do move, confirmar `DeleteFailed` sem corromper contadores.
10. Em cancellation apos `PendingDelete`, restaurar estado navegavel e relancar `OperationCanceledException`.
11. Registrar `DeleteManager` e `DeleteCurrentPhotoUseCase` no DI.
12. Adicionar testes de Core e Application.
13. Adicionar um teste de integracao com `FileMoveService` real em pasta temporaria.
14. Atualizar `docs/execution-progress.md`, README e plano mestre.

## Testes necessarios

- Core: delete no inicio move para proxima e marca `PendingDelete`.
- Core: delete no fim move para anterior.
- Core: complete marca `Deleted` e contadores ficam corretos.
- Core: fail marca `DeleteFailed`, reconta ativo/deletado e preserva proxima foto atual.
- Core: sem foto ativa retorna `NoCurrentPhoto`.
- Application: move com sucesso marca `Deleted` e chama `IFileMoveService`.
- Application: move com falha marca `DeleteFailed` e preserva navegacao.
- Application: cancelamento apos pending nao deixa foto presa em `PendingDelete`.
- Integration: use case com `FileMoveService` real move arquivo para `_deletadas_evydencia`.

## Criterio de aceite

- Delete sem UI muda status e contadores de forma deterministica.
- Sucesso fisico do move deixa foto `Deleted`.
- Falha fisica do move deixa foto `DeleteFailed` e navegavel.
- Foto atual apos delete e a proxima, ou anterior se a excluida era a ultima.
- DI resolve `DeleteCurrentPhotoUseCase`.
- Build, format e testes passam.

## Riscos

- Falha de move pode reinserir a foto em posicao que altere `CurrentIndex`; mitigacao: `DeleteManager.FailDelete` preserva a foto preferida atual quando possivel.
- Cancellation no meio da operacao pode deixar `PendingDelete`; mitigacao: use case captura cancelamento apos request e chama `FailDelete` antes de relancar.
- O delete ainda nao grava journal; mitigacao: resultado do use case preserva `FileMoveResult` para a proxima fatia registrar `Deleted`/`DeleteFailed`.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Integration"
.\tools\build.ps1
.\tools\format.ps1
.\tools\test.ps1
.\tools\build.ps1
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

- Criado `DeleteManager` em Core para requisitar delete otimista e concluir como `Deleted` ou `DeleteFailed`.
- `RequestDeleteCurrent` marca a foto atual como `PendingDelete`, remove da navegacao ativa e posiciona a proxima foto, ou a anterior quando a atual era a ultima.
- `CompleteDelete` marca `Deleted` apos sucesso do move.
- `FailDelete` marca `DeleteFailed`, torna a foto navegavel de novo e preserva a proxima foto como atual quando possivel.
- Criado `DeleteCurrentPhotoUseCase` em Application, orquestrando `DeleteManager` com `IFileMoveService`.
- Cancelamento apos `PendingDelete` restaura estado navegavel antes de relancar `OperationCanceledException`.
- DI registra `DeleteManager` e `DeleteCurrentPhotoUseCase`.
- Teste de integracao valida delete real em pasta temporaria com `FileMoveService`.
- `F3-03` e `F3-04` foram marcados como concluidos em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Integration"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
.\tools\build.ps1
```

Resultado:

- Core: 27 testes aprovados.
- Application: 22 testes aprovados.
- Integration: 3 testes aprovados.
- Suite completa: 82 testes aprovados.
- Build completo: aprovado, 0 warnings.
- Format: aprovado apos normalizar CRLF dos arquivos alterados.

## Pendencias deliberadas

- UI ainda nao captura `Delete`.
- `UndoManager` e `UndoLastDeleteUseCase` ficam para a proxima fatia.
- Journal JSONL ainda nao registra `DeleteRequested`, `Deleted` ou `DeleteFailed`.
- Retry para arquivo travado fica para a fatia de fluxo de delete com erro de IO.
