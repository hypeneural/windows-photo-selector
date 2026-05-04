# Execucao 0018 - Fundacao de move/restore para Delete/Undo

## Objetivo

Implementar a base segura de filesystem para o fluxo de selecao por exclusao: mover JPEGs para `_deletadas_evydencia` e restaurar para o caminho original com tratamento de colisao. Esta fatia valida `F0-07` e prepara `DeleteCurrentPhotoUseCase`/`UndoLastDeleteUseCase`, sem ainda conectar `Delete`/`Ctrl+Z` na UI.

Esta fatia nao implementa API, PDV, RAW, upload, cache, menu de contexto, single-instance, journal JSONL nem delete visual no viewer.

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
- `src/Evydencia.PhotoSelector.Storage/Filesystem/FileSystemFolderScanner.cs`
- `src/Evydencia.PhotoSelector.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`
- `tests/Evydencia.PhotoSelector.Storage.Tests/TemporaryFolder.cs`
- `tests/Evydencia.PhotoSelector.Storage.Tests/Filesystem/FileSystemFolderScannerTests.cs`
- `tests/Evydencia.PhotoSelector.IntegrationTests/Composition/DependencyInjectionTests.cs`

Documentacao oficial consultada:

- Microsoft Learn, `File.Move`.
- Microsoft Learn, `Directory.CreateDirectory`.
- Microsoft Learn, excecoes de IO (`IOException`, `UnauthorizedAccessException`).

## Camada afetada

- Application
- Storage
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

1. Criar contrato `IFileMoveService` em `Application/Abstractions`.
2. Criar modelos de resultado e erro para move/restore em `Application/Models`.
3. Implementar `FileMoveService` em `Storage/Filesystem`.
4. Mover para `<session-folder>\_deletadas_evydencia` sem overwrite.
5. Resolver colisao de destino com sufixo unico, preservando o caminho real no resultado.
6. Restaurar para o caminho original ou caminho seguro se o original estiver ocupado.
7. Preservar `LastWriteTimeUtc` quando possivel.
8. Mapear erros comuns para uma taxonomia inicial: fonte ausente, acesso negado, path invalido, path longo e falha de IO.
9. Registrar `IFileMoveService` no DI.
10. Adicionar testes de Storage para move, restore, colisao, fonte ausente, timestamp e arquivo read-only.
11. Atualizar `docs/execution-progress.md`, README e plano mestre.

## Testes necessarios

- `MoveToDeletedFolderAsync` move arquivo comum para `_deletadas_evydencia`.
- Move resolve colisao sem sobrescrever arquivo existente.
- Restore volta ao caminho original.
- Restore resolve colisao no caminho original.
- Fonte ausente retorna erro sem criar falso sucesso.
- Move/restore preserva arquivo read-only o suficiente para nao falhar no fluxo.
- DI resolve `IFileMoveService`.

## Criterio de aceite

- Arquivo movido sai da pasta original e aparece em `_deletadas_evydencia`.
- Restore recoloca o arquivo no caminho original quando livre.
- Colisoes nunca sobrescrevem arquivos existentes.
- Resultado informa destino real usado.
- `F0-07` fica marcado como concluido.
- Build, format e testes passam.

## Riscos

- `File.Move` pode copiar e deletar entre volumes; mitigacao: V1 usa `_deletadas_evydencia` dentro da mesma pasta da sessao, normalmente no mesmo volume.
- Arquivo travado por outro processo pode gerar `IOException`; mitigacao: retornar erro detalhado para futura UI/journal, sem corromper estado.
- Arquivo read-only pode dificultar limpeza de teste; mitigacao: `TemporaryFolder.Dispose` limpa atributo read-only antes de excluir.
- Colisao por corrida entre check e move ainda pode acontecer; mitigacao futura: retry curto no `FileMoveService`.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Storage|FullyQualifiedName~Integration"
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

- Criado `IFileMoveService` em Application para manter IO concreto fora dos casos de uso.
- Criados `FileMoveResult` e `FileMoveErrorCode` com destino real, colisao resolvida, timestamp preservado e erro inicial.
- Implementado `FileMoveService` em Storage usando `_deletadas_evydencia` como destino padrao.
- Move e restore usam `File.Move(..., overwrite: false)` e geram nome unico quando o destino ja existe.
- `Directory.CreateDirectory` prepara a pasta de destino sem depender de preexistencia.
- `LastWriteTimeUtc` e preservado apos move/restore.
- Arquivo read-only foi validado no fluxo de move/restore.
- DI registra `IFileMoveService`.
- `F0-07` e `F3-02` foram marcados como concluidos em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Storage|FullyQualifiedName~Integration"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
.\tools\build.ps1
```

Resultado:

- Storage: 10 testes aprovados.
- Integration: 2 testes aprovados.
- Suite completa: 71 testes aprovados.
- Build completo: aprovado, 0 warnings.
- Format: aprovado apos normalizar CRLF dos arquivos alterados.

## Pendencias deliberadas

- `DeleteManager`, `DeleteCurrentPhotoUseCase`, `UndoManager` e `UndoLastDeleteUseCase` ficam para as proximas fatias.
- Journal JSONL ainda nao foi conectado.
- UI ainda nao captura `Delete`/`Ctrl+Z`.
- Arquivo travado por outro processo ainda retorna erro de IO; retry curto fica para a fatia do fluxo de delete.
