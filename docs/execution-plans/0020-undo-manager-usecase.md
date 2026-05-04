# Execucao 0020 - UndoManager e UndoLastDeleteUseCase

## Objetivo

Implementar o ciclo de undo sem UI: `UndoManager` no Core para registrar deletes bem-sucedidos, marcar `PendingRestore`, concluir como `Restored` ou voltar para `Deleted` em falha, e `UndoLastDeleteUseCase` na Application para orquestrar restore via `IFileMoveService`.

Esta fatia fecha o ciclo seguro antes de ligar `Delete` e `Ctrl+Z` no viewer. Nao implementa UI, atalhos, journal JSONL, menu de contexto, single-instance, API, PDV, RAW ou upload.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/delete-undo.md`
- `src/Evydencia.PhotoSelector.Core/AGENTS.md`
- `src/Evydencia.PhotoSelector.Application/AGENTS.md`
- `tests/AGENTS.md`
- `docs/adr/0002-delete-mode.md`
- `docs/execution-progress.md`
- `src/Evydencia.PhotoSelector.Core/Photos/PhotoItem.cs`
- `src/Evydencia.PhotoSelector.Core/Photos/PhotoStatus.cs`
- `src/Evydencia.PhotoSelector.Core/Sessions/PhotoSession.cs`
- `src/Evydencia.PhotoSelector.Core/Deletion/DeleteManager.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/DeleteCurrentPhotoUseCase.cs`
- `src/Evydencia.PhotoSelector.Application/Abstractions/IFileMoveService.cs`
- `src/Evydencia.PhotoSelector.Application/Models/FileMoveResult.cs`
- `src/Evydencia.PhotoSelector.Storage/Filesystem/FileMoveService.cs`

Documentacao oficial consultada:

- Microsoft Learn, `File.Move`, para confirmar comportamento de move sem overwrite e variante com `overwrite`.
- Microsoft Learn, `CancellationToken.ThrowIfCancellationRequested`, para manter cancelamento cooperativo.
- Microsoft Learn, `Stack<T>`, para confirmar semantica LIFO de `Push`, `Peek` e `Pop`.

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

1. Criar modelos de undo em `Core/Undo`.
2. Criar `UndoManager` com pilha LIFO por `PhotoSession.Id`.
3. Registrar undo somente para foto com status `Deleted` e caminho de destino deletado conhecido.
4. `RequestRestoreLast` deve marcar a foto como `PendingRestore` e preservar a foto atual antes do restore.
5. `CompleteRestore` deve marcar `Restored`, remover a operacao da pilha e navegar para a foto restaurada.
6. `FailRestore` deve voltar a foto para `Deleted`, manter a operacao na pilha e preservar a foto atual anterior quando possivel.
7. Criar `UndoLastDeleteUseCase` usando `UndoManager` e `IFileMoveService.RestoreAsync`.
8. Atualizar `DeleteCurrentPhotoUseCase` para registrar undo apos move bem-sucedido.
9. Tratar cancellation apos `PendingRestore` sem deixar a foto nesse estado intermediario.
10. Registrar `UndoManager` e `UndoLastDeleteUseCase` no DI.
11. Adicionar testes de Core e Application.
12. Adicionar teste de integracao com delete + undo usando `FileMoveService` real.
13. Atualizar `docs/execution-progress.md`, README e plano mestre.

## Testes necessarios

- Core: undo sem operacao retorna `NoUndoAvailable`.
- Core: register + request marca `PendingRestore`.
- Core: complete marca `Restored`, remove da pilha e navega para a foto restaurada.
- Core: fail volta para `Deleted`, mantem undo disponivel e preserva a foto atual.
- Application: delete bem-sucedido registra undo.
- Application: undo bem-sucedido restaura status, contadores e navegacao.
- Application: restore falho volta para `Deleted` e mantem undo disponivel.
- Application: cancelamento apos pending nao deixa `PendingRestore`.
- Integration: delete + undo real em pasta temporaria move para `_deletadas_evydencia` e restaura para o caminho original.

## Criterio de aceite

- Delete seguido de undo sem UI e deterministico.
- Foto restaurada volta para navegacao ativa.
- `DeletedCount` volta para zero apos restore bem-sucedido.
- Falha de restore nao corrompe contadores nem perde a possibilidade de tentar undo novamente.
- DI resolve `UndoLastDeleteUseCase`.
- Build, format e testes passam.

## Riscos

- Colisao no restore pode restaurar para nome unico em vez do caminho original; mitigacao: `FileMoveResult.ActualDestinationPath` e propagado e a foto passa a apontar para o caminho real restaurado.
- Cancellation no meio do restore pode deixar `PendingRestore`; mitigacao: use case chama `FailRestore` antes de relancar `OperationCanceledException`.
- Pilha global de undo pode misturar sessoes; mitigacao: `UndoManager` mantem pilha por `PhotoSession.Id`.
- Journal ainda nao existe; mitigacao: resultados carregam dados suficientes para a proxima fatia registrar eventos JSONL.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Integration"
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

- Criado `UndoManager` em Core com pilha LIFO por `PhotoSession.Id`.
- `RegisterDeletedPhoto` registra operacoes de undo somente apos delete fisico bem-sucedido.
- `RequestRestoreLast` marca a foto como `PendingRestore` e preserva a foto atual antes do restore.
- `CompleteRestore` marca `Restored`, remove a operacao da pilha e navega para a foto restaurada.
- `FailRestore` volta a foto para `Deleted`, preserva a foto atual anterior quando possivel e mantem a operacao disponivel para retry.
- Criado `UndoLastDeleteUseCase` em Application, orquestrando `UndoManager` com `IFileMoveService.RestoreAsync`.
- `DeleteCurrentPhotoUseCase` passou a registrar undo em sucesso.
- Restore com colisao atualiza a localizacao do `PhotoItem` para o caminho real retornado por `FileMoveResult.ActualDestinationPath`.
- DI registra `UndoManager` e `UndoLastDeleteUseCase`.
- Teste de integracao valida delete + undo real em pasta temporaria com `FileMoveService`.
- `F3-05` e `F3-06` foram marcados como concluidos em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Integration"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```

Resultado:

- Core: 32 testes aprovados.
- Application: 27 testes aprovados.
- Integration: 4 testes aprovados.
- Suite completa: 93 testes aprovados.
- Build completo: aprovado, 0 warnings.
- Format: aprovado apos normalizar CRLF dos arquivos alterados.

## Pendencias deliberadas

- UI ainda nao captura `Delete` nem `Ctrl+Z`.
- Journal JSONL ainda nao registra `UndoRequested`, `Restored` ou `RestoreFailed`.
- Retry visual para falha de restore fica para a fatia de UI/erro.
- Replay de journal e reconciliacao de crash ficam para fatias seguintes.
