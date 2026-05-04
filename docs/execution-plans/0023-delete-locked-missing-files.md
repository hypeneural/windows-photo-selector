# Execucao 0023 - Arquivo bloqueado e ausente no fluxo de delete

## Objetivo

Fechar F3-09 e F3-10 antes de ligar `Delete`/`Ctrl+Z` no viewer. A fatia garante que arquivo bloqueado vira falha recuperavel (`DeleteFailed` + erro especifico) e que arquivo ausente vira `Missing`, mantendo contadores, navegacao e journal consistentes.

Nao implementa UI, atalhos no viewer, retry visual, logs Serilog, API, PDV, upload, RAW, menu de contexto ou single-instance.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/delete-undo.md`
- `docs/execution-progress.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `src/Evydencia.PhotoSelector.Storage/Filesystem/FileMoveService.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/DeleteCurrentPhotoUseCase.cs`
- `src/Evydencia.PhotoSelector.Core/Deletion/DeleteManager.cs`
- testes de Core/Application/Storage/Integration relacionados a delete

Documentacao oficial consultada:

- Microsoft Learn, `File.Move`, para excecoes esperadas em move.
- Microsoft Learn, `Handling I/O errors in .NET`, para classificar `IOException.HResult` e `ERROR_SHARING_VIOLATION`.
- Microsoft Learn, `FileShare`, para simular arquivo bloqueado com `FileShare.None` em teste.

## Camada afetada

- Core
- Application
- Storage
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

1. Adicionar erro especifico `FileLocked` em `FileMoveErrorCode`.
2. Mapear `IOException` com Win32 `ERROR_SHARING_VIOLATION`/`ERROR_LOCK_VIOLATION` para `FileLocked`.
3. Adicionar transicao de dominio para arquivo ausente no delete: `PendingDelete` -> `Missing`.
4. Fazer `DeleteCurrentPhotoUseCase` retornar resultado `Missing` quando `IFileMoveService` retornar `SourceMissing`.
5. Manter `DeleteFailed` para arquivo bloqueado e demais falhas recuperaveis.
6. Garantir que `DeleteFailed` e `Missing` escrevem evento `DeleteFailed` no journal com o erro real.
7. Adicionar testes unitarios de Core/Application.
8. Adicionar testes reais de Storage/Integration para arquivo bloqueado e ausente.
9. Atualizar docs de progresso.

## Testes necessarios

- Core: source missing marca foto como `Missing`, remove da navegacao ativa e preserva proxima foto.
- Application: `SourceMissing` retorna `DeleteCurrentPhotoStatus.Missing`, foto `Missing`, sem undo.
- Storage: arquivo aberto com `FileShare.None` retorna `FileMoveErrorCode.FileLocked`.
- Integration: delete de arquivo bloqueado deixa arquivo no local, marca `DeleteFailed`, journal registra `DeleteFailed`.
- Integration: delete de arquivo ausente marca `Missing`, app continua na proxima foto, journal registra `DeleteFailed`.

## Criterio de aceite

- F3-09 marcado como concluido: arquivo bloqueado nao corrompe estado e retorna erro especifico.
- F3-10 marcado como concluido: arquivo ausente vira `Missing` e app continua.
- Build, format e testes passam.

## Riscos

- Lock pode produzir codigo Win32 diferente dependendo do filesystem/antivirus; mitigacao: cobrir `ERROR_SHARING_VIOLATION` e `ERROR_LOCK_VIOLATION`, mantendo fallback `IoFailure`.
- `Missing` nao deve entrar em navegacao ativa nem contar como deletada; mitigacao: teste de contadores.
- Journal usa evento `DeleteFailed` para `Missing`; mitigacao: `ErrorCode=SourceMissing` diferencia o caso ate existir evento especifico futuro.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"
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

- `FileMoveErrorCode` ganhou `FileLocked`.
- `FileMoveService` mapeia `ERROR_SHARING_VIOLATION` e `ERROR_LOCK_VIOLATION` para `FileLocked`.
- `DeleteManager` ganhou transicao `PendingDelete` -> `Missing`.
- `DeleteCurrentPhotoUseCase` retorna `Missing` quando o move falha com `SourceMissing`.
- Arquivo bloqueado continua como `DeleteFailed`, sem entrar na pilha de undo.
- Arquivo ausente sai da navegacao ativa como `Missing`, sem contar como deletado e sem entrar na pilha de undo.
- Journal continua registrando `DeleteFailed` com `ErrorCode` real (`FileLocked` ou `SourceMissing`).

## Validacao

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```

Resultado:

- Core: 33 testes.
- Application: 33 testes.
- Storage: 16 testes.
- Integration: 7 testes.
- Suite completa: 109 testes.
- Build: 0 warnings, 0 erros.

## Riscos remanescentes

- Cross-volume com arquivo em uso pode deixar copia parcial conforme comportamento documentado do `File.Move`; rollback cross-volume continua reservado para F3-14.
- Overlay/UX de erro ainda nao existe porque `Delete` ainda nao foi ligado no viewer.
