# Execucao 0021 - Journal JSONL minimo de delete e restore

## Objetivo

Implementar `ISessionJournalStore` e `JsonlSessionJournalStore` para gravar eventos append-only em JSONL por sessao. A fatia deve registrar `DeleteRequested`, `Deleted`, `DeleteFailed`, `UndoRequested`, `Restored` e `RestoreFailed` nos use cases existentes, ainda sem ligar `Delete` e `Ctrl+Z` na UI.

Esta fatia nao implementa replay completo, reconciliacao de crash, UI, atalhos, menu de contexto, single-instance, API, PDV, RAW ou upload.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/delete-undo.md`
- `src/Evydencia.PhotoSelector.Application/AGENTS.md`
- `tests/AGENTS.md`
- `docs/adr/0004-source-of-truth.md`
- `docs/execution-progress.md`
- `src/Evydencia.PhotoSelector.Application/UseCases/DeleteCurrentPhotoUseCase.cs`
- `src/Evydencia.PhotoSelector.Application/UseCases/UndoLastDeleteUseCase.cs`
- `src/Evydencia.PhotoSelector.Storage/Filesystem/FileMoveService.cs`
- testes de Core, Application, Storage e Integration relacionados a delete/undo

Documentacao oficial consultada:

- Microsoft Learn, `File.AppendAllTextAsync`, para confirmar append assincrono, criacao do arquivo quando ausente e fechamento do handle.
- Microsoft Learn, `System.Text.Json.JsonSerializer.Serialize`, para serializacao JSON.
- Microsoft Learn, `Directory.CreateDirectory`, para criacao idempotente da pasta do journal.

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

1. Criar `ISessionJournalStore` em Application.
2. Criar modelos `SessionJournalEvent` e `SessionJournalEventType`.
3. Criar factories de evento para delete/restore.
4. Implementar `JsonlSessionJournalStore` em Storage.
5. Salvar journal em `<session-folder>\_deletadas_evydencia\evydencia-session-journal.jsonl`.
6. Usar UTF-8 sem BOM, uma linha JSON por evento.
7. Serializar em camelCase e sem indentacao.
8. Registrar journal no DI.
9. Injetar journal em `DeleteCurrentPhotoUseCase`.
10. Injetar journal em `UndoLastDeleteUseCase`.
11. Registrar `DeleteRequested`, `Deleted` e `DeleteFailed`.
12. Registrar `UndoRequested`, `Restored` e `RestoreFailed`.
13. Adicionar fake de journal para Application.Tests.
14. Adicionar testes de Storage para arquivo JSONL real.
15. Atualizar teste de integracao para validar linhas JSONL em delete + undo real.
16. Atualizar docs de progresso.

## Testes necessarios

- Storage: append cria diretoria e arquivo JSONL.
- Storage: dois appends geram duas linhas.
- Storage: JSON gerado e parseavel.
- Application: delete sucesso registra `DeleteRequested` e `Deleted`.
- Application: delete falha registra `DeleteRequested` e `DeleteFailed`.
- Application: undo sucesso registra `UndoRequested` e `Restored`.
- Application: undo falha registra `UndoRequested` e `RestoreFailed`.
- Integration: delete + undo real gera JSONL com eventos esperados.

## Criterio de aceite

- Eventos de delete/restore sao gravados em JSONL append-only.
- O journal fica fora da lista de JPEGs porque esta em `_deletadas_evydencia`.
- Nenhum use case referencia filesystem concreto.
- DI resolve `ISessionJournalStore` como `JsonlSessionJournalStore`.
- Build, format e testes passam.

## Riscos

- Falha de escrita do evento final depois de um move fisico pode deixar filesystem mais atualizado que o journal; mitigacao: ADR 0004 ja define filesystem como realidade em conflito e replay/reconciliacao vem na proxima fatia.
- `AppendAllTextAsync` abre e fecha o arquivo a cada append; mitigacao: volume de eventos e baixo na V1 e evita handle preso.
- Escritas simultaneas no mesmo processo podem intercalar linhas; mitigacao: `JsonlSessionJournalStore` usa `SemaphoreSlim`.
- Journal dentro da pasta `_deletadas_evydencia` pode sumir se o operador apagar a pasta; mitigacao futura: replay privilegia filesystem e logs tecnicos serao adicionados em fatias seguintes.

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

- Criado `ISessionJournalStore` em Application.
- Criado `SessionJournalEvent` e `SessionJournalEventType`.
- Criado `JsonlSessionJournalStore` em Storage.
- Journal e gravado em `<session-folder>\_deletadas_evydencia\evydencia-session-journal.jsonl`.
- Escrita usa UTF-8 sem BOM, JSON camelCase, uma linha por evento e `SemaphoreSlim` para serializar appends no processo.
- `DeleteCurrentPhotoUseCase` registra `DeleteRequested`, `Deleted` e `DeleteFailed`.
- `UndoLastDeleteUseCase` registra `UndoRequested`, `Restored` e `RestoreFailed`.
- DI registra `ISessionJournalStore` como `JsonlSessionJournalStore`.
- Teste de integracao valida delete + undo real com journal JSONL.
- `F3-07` foi marcado como concluido em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```

Resultado:

- Application: 27 testes aprovados.
- Storage: 12 testes aprovados.
- Integration: 4 testes aprovados.
- Suite completa: 95 testes aprovados.
- Build completo: aprovado, 0 warnings.
- Format: aprovado.

## Pendencias deliberadas

- Replay basico de journal fica para `F3-08`.
- Reconciliacao de crash ainda nao foi implementada.
- UI ainda nao captura `Delete` nem `Ctrl+Z`.
- Falha de escrita do evento final apos move fisico ainda precisa ser tratada na estrategia de recovery/replay.
