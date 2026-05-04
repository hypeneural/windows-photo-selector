# Execucao 0022 - Replay basico de journal e reconciliacao inicial

## Objetivo

Implementar replay basico do journal JSONL para aplicar eventos de delete/restore em uma `PhotoSession` ja aberta. A fatia tambem cria uma reconciliacao inicial com filesystem: quando o journal e o disco divergem, a realidade do filesystem prevalece para marcar fotos como `Restored`, `Deleted` ou `Missing`.

Esta fatia nao implementa UI, atalhos `Delete`/`Ctrl+Z`, replay completo de crash, SQLite, logs estruturados, menu de contexto, single-instance, API, PDV, RAW ou upload.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/delete-undo.md`
- `docs/adr/0004-source-of-truth.md`
- `docs/execution-progress.md`
- `src/Evydencia.PhotoSelector.Application/Abstractions/ISessionJournalStore.cs`
- `src/Evydencia.PhotoSelector.Application/Models/SessionJournalEvent.cs`
- `src/Evydencia.PhotoSelector.Storage/Journal/JsonlSessionJournalStore.cs`
- `src/Evydencia.PhotoSelector.Core/Sessions/PhotoSession.cs`
- `src/Evydencia.PhotoSelector.Core/Photos/PhotoItem.cs`
- testes de Application/Storage/Integration de journal, delete e undo

Documentacao oficial consultada:

- Microsoft Learn, `File.ReadLinesAsync`, para leitura linha-a-linha sem carregar todo o journal.
- Microsoft Learn, `JsonSerializer.Deserialize`, para parse dos eventos JSON.
- Microsoft Learn, `Directory.EnumerateFiles`, para manter scan/reconciliacao compativel com a estrategia progressiva.

## Camada afetada

- Core
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

1. Adicionar leitura de eventos em `ISessionJournalStore`.
2. Implementar leitura JSONL em `JsonlSessionJournalStore`.
3. Tornar `SessionJournalEvent` desserializavel pelo `System.Text.Json`.
4. Criar abstracao `IFileExistenceService`.
5. Implementar `FileSystemFileExistenceService` em Storage.
6. Criar `ReplaySessionJournalUseCase` em Application.
7. Aplicar eventos em ordem e resolver foto por caminho, nao por `PhotoId`, porque uma sessao reaberta gera novos ids.
8. Para evento `Deleted`, marcar `Deleted` quando o arquivo existe em `_deletadas_evydencia`; se existe no original, marcar `Restored`; se nao existe em nenhum, marcar `Missing`.
9. Para evento `Restored`, marcar `Restored` quando o arquivo existe no destino; se existe apenas em `_deletadas_evydencia`, marcar `Deleted`; se nao existe, marcar `Missing`.
10. Adicionar foto recuperada a sessao quando o journal aponta para uma foto ausente da lista escaneada.
11. Registrar DI.
12. Adicionar testes de Storage e Application.
13. Atualizar docs de progresso.

## Testes necessarios

- Storage: `ReadEventsAsync` le eventos gravados em JSONL.
- Storage: journal ausente retorna vazio.
- Application: replay de `Deleted` adiciona foto ausente e marca `Deleted` quando arquivo deletado existe.
- Application: filesystem vence journal `Deleted` quando arquivo original existe.
- Application: `Restored` marca foto escaneada como `Restored`.
- Application: evento apontando para arquivo inexistente marca `Missing`.
- Integration: delete real + nova sessao escaneada + replay reconstroi deleted count.

## Criterio de aceite

- Replay basico aplica eventos em ordem.
- Arquivo em `_deletadas_evydencia` pode voltar a contar como `Deleted` mesmo se nao foi escaneado na pasta original.
- Arquivo existente na pasta original prevalece sobre evento antigo de delete.
- Arquivo ausente vira `Missing`.
- Build, format e testes passam.

## Riscos

- SortIndex original de foto deletada nao existe no journal atual; mitigacao: fotos recuperadas como `Deleted` entram no final, sem afetar navegacao ativa.
- Replay ainda nao recria pilha de undo; mitigacao: fatia futura pode reconstruir `UndoManager` a partir de eventos finais.
- Eventos JSONL corrompidos sao ignorados nesta fatia; mitigacao: logs tecnicos entram em fatia posterior.
- Reconciliacao ainda e minima e por caminho; renames externos complexos ficam para recovery avancado.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"
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

- `ISessionJournalStore` agora expoe `ReadEventsAsync`.
- `JsonlSessionJournalStore` le JSONL linha a linha e ignora linhas vazias/corrompidas.
- `SessionJournalEvent` passou a ser desserializavel por `System.Text.Json`.
- `ReplaySessionJournalUseCase` aplica eventos finais (`Deleted`, `Restored`, `DeleteFailed`, `RestoreFailed`) em ordem.
- A reconciliacao basica usa `IFileExistenceService`: arquivo original existente vence evento antigo de delete; arquivo em `_deletadas_evydencia` vira `Deleted`; arquivo ausente vira `Missing`.
- Fotos deletadas que nao aparecem no scan da pasta original sao reinseridas na sessao como recuperadas para preservar contadores.
- DI registra `IFileExistenceService` e `ReplaySessionJournalUseCase`.

## Validacao

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```

Resultado:

- Application: 32 testes.
- Storage: 14 testes.
- Integration: 5 testes.
- Suite completa: 103 testes.
- Build: 0 warnings, 0 erros.

## Riscos remanescentes

- Replay ainda nao reconstrui a pilha de undo para sessoes reabertas.
- Reconciliacao ainda nao trata renames externos complexos.
- Logs tecnicos para linhas corrompidas ficam para a fatia de diagnostics/Serilog.
