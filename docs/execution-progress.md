# Progresso de execucao

Ultima atualizacao: 2026-05-04

## Estado geral

- [x] Orientacoes Codex criadas (`AGENTS.md`, `PLANS.md`, skill local).
- [x] Plano tecnico refinado com camada Application e governanca de build.
- [x] ADRs iniciais criados.
- [x] Estrutura de governanca raiz iniciada.
- [x] SDK .NET 10 instalado e localizado pelos scripts.
- [x] Solution scaffoldada.
- [x] Projetos base criados.
- [x] Build validado.
- [x] Testes validado.

## Fase 0 - Provas tecnicas

- [x] F0-01 - ADR de stack Windows nativa criado.
- [x] F0-02 - Validar versao .NET e Windows App SDK.
- [x] F0-03 - Prototipar argumento `--folder`.
- [x] F0-04 - Prototipar fullscreen WinUI 3.
- [x] F0-05 - Prototipar decode JPEG dimensionado.
- [x] F0-06 - Validar EXIF orientation.
- [x] F0-07 - Validar move para `_deletadas_evydencia`.
- [ ] F0-08 - Investigar Lixeira com undo.
- [ ] F0-09 - Validar File Explorer context menu moderno.
- [ ] F0-10 - Definir metas de performance reais.
- [x] F0-11 - Validar .NET 10 + Windows App SDK 2.0.1.
- [ ] F0-12 - Validar single-instance com ativacao por pasta.
- [x] F0-13 - Validar decode sem prender file handle.
- [x] F0-14 - Criar `DisplayContext`.
- [ ] F0-15 - Validar preview cache sem perda visual.
- [x] F0-16 - ADR de fonte da verdade criado.
- [x] F0-17 - Governanca de build documentada.
- [x] F0-18 - Camada Application definida.

## Fase 1 - Skeleton, governanca e dominio minimo

- [x] F1-01 - Arquivos raiz de governanca criados.
- [x] F1-02 - Solucao e projetos base criados.
- [x] F1-03 - `PhotoItem` e `PhotoSession`.
- [x] F1-04 - Projeto `Application`.
- [x] F1-05 - `PhotoSessionFactory`.
- [x] F1-06 - `NavigationController`.
- [x] F1-07 - `FolderScanPolicy`.
- [x] F1-08 - Contratos de scan em Application.
- [x] F1-09 - `FileSystemFolderScanner`.
- [x] F1-10 - `DecodeTargetCalculator`.
- [x] F1-11 - `Application.Tests`.
- [x] F1-12 - DI.

## Fase 2 - Viewer vertical minimo

- [x] F2-01 - Abrir `PhotoSession` em background a partir de `--folder`.
- [x] F2-02 - Exibir estado da sessao aberta na UI inicial.
- [x] F2-03 - Mostrar primeira foto no viewer.
- [x] F2-04 - Navegacao visual por setas.
- [x] F2-05 - Fullscreen limpo inicial.

## Fase 3 - Delete e undo robusto

- [ ] F3-01 - Criar `DeleteMode` e settings base.
- [x] F3-02 - Implementar `FileMoveService` para move/restore local seguro.
- [x] F3-03 - Implementar `DeleteManager` de dominio com `PendingDelete`, `Deleted` e `DeleteFailed`.
- [x] F3-04 - Implementar `DeleteCurrentPhotoUseCase`.
- [x] F3-05 - Implementar `UndoManager` de dominio.
- [x] F3-06 - Implementar `UndoLastDeleteUseCase`.
- [x] F3-07 - Implementar `JsonlSessionJournalStore`.
- [x] F3-08 - Implementar replay basico de journal.
- [x] F3-09 - Tratar arquivo bloqueado no fluxo de delete.
- [x] F3-10 - Tratar arquivo ausente no fluxo de delete.
- [x] F3-11 - Atualizar contadores em todas as transicoes.
- [x] F3-15 - Ligar `Delete` e `Ctrl+Z` no viewer.

## Bloqueios atuais

- `ShellExtension` ainda nao tem projeto C++/WinRT. A decisao continua adiada para a fase de menu de contexto.
- Single-instance ainda nao foi implementado; deve vir antes do menu de contexto do Explorer.
- Conversao do resultado de decode para `ImageSource`/viewer WinUI ja existe para a primeira foto, com navegacao visual inicial por `Right`, `Left` e `Space`.
- Fullscreen inicial ja usa `AppWindow`/`FullScreenPresenter`, `F` alterna, `Esc` sai, e o decode recaptura `DisplayContext` com estado fullscreen.
- `FileMoveService` ja move para `_deletadas_evydencia`, restaura, resolve colisao e preserva `LastWriteTimeUtc`; ja esta ligado ao `DeleteCurrentPhotoUseCase`.
- `DeleteManager`, `DeleteCurrentPhotoUseCase`, `UndoManager` e `UndoLastDeleteUseCase` ja validam `PendingDelete`, `Deleted`, `PendingRestore`, `Restored`, `Missing`, `DeleteFailed`, contadores e navegacao; `JsonlSessionJournalStore` ja registra delete/restore em JSONL; `ReplaySessionJournalUseCase` ja faz replay basico e reconciliacao inicial; os atalhos `Delete` e `Ctrl+Z` ja chamam os use cases reais no viewer.
- Overlay temporizado, retry visual de falha e fila de comandos para deletes muito rapidos ainda ficam para UX/performance posterior.
- `WindowsDisplayContextService` captura `XamlRoot`, area util, escala de rasterizacao e estado fullscreen. Identificacao detalhada de monitor/display area fica para a fatia de segunda tela.

## Ultima validacao

- [x] Estrutura de orientacao validada por `.agents/skills/windows-photo-selector/scripts/validate-solution.ps1`.
- [x] `.NET SDK 10.0.203` instalado via `winget`.
- [x] `Microsoft.WindowsAppSDK 2.0.1` validado por restore/build do projeto WinUI.
- [x] `tools/build.ps1` executado com sucesso.
- [x] `tools/test.ps1` executado com sucesso.
- [x] `tools/format.ps1` executado com sucesso.
- [x] `tools/benchmarks.ps1` executado com BenchmarkDotNet real.
- [x] Build executado com sucesso.
- [x] Testes executados com sucesso.
- [x] Benchmarks executados com sucesso.
- [x] `DecodeTargetCalculatorBenchmarks`: EXIF 1 ~115,5 ns; EXIF 6 ~118,2 ns.
- [x] `FolderScanBenchmarks`: 500 JPEGs ~1,486 ms; 2.000 JPEGs ~5,492 ms.
- [x] `OpenSessionUseCaseBenchmarks`: 500 candidatos ~380,1 us; 2.000 candidatos ~1,937 ms.
- [x] Fatia 0012 foi documentacao apenas; nenhum teste automatizado adicional foi necessario.
- [x] Fatia 0013 validou `JpegDecodeService`, `JpegSignatureProbe` e decode sem prender file handle.
- [x] `tools/build.ps1` executado com sucesso apos 0013.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Imaging"` executado com sucesso apos 0013.
- [x] `tools/format.ps1` executado com sucesso apos 0013.
- [x] `tools/test.ps1` executado com sucesso apos 0013.
- [x] Fatia 0014 validou EXIF orientation 6/8 com fixtures sinteticas e sem dupla aplicacao de dimensoes.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Imaging"` executado com sucesso apos 0014: 13 testes.
- [x] `tools/build.ps1` executado com sucesso apos 0014.
- [x] `tools/format.ps1` executado com sucesso apos 0014.
- [x] `tools/test.ps1` executado com sucesso apos 0014: 60 testes.
- [x] Repositorio Git inicializado, commit inicial criado e branch `main` publicada em `https://github.com/hypeneural/windows-photo-selector.git`.
- [x] Fatia 0015 conectou decode JPEG dimensionado ao viewer WinUI para a primeira foto.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Imaging|FullyQualifiedName~UiSmoke"` executado com sucesso apos 0015: Imaging 15 testes, UiSmoke 3 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0015.
- [x] `tools/test.ps1` executado com sucesso apos 0015: 62 testes.
- [x] `tools/build.ps1` executado com sucesso apos 0015.
- [x] Fatia 0016 implementou navegacao visual por `Right`, `Left` e `Space`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~UiSmoke"` executado com sucesso apos 0016: Application 17 testes, UiSmoke 4 testes.
- [x] `tools/build.ps1` executado com sucesso apos 0016.
- [x] `tools/format.ps1` executado com sucesso apos 0016.
- [x] `tools/test.ps1` executado com sucesso apos 0016: 63 testes.
- [x] Fatia 0017 implementou fullscreen limpo inicial com `AppWindow`/`FullScreenPresenter`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~UiSmoke"` executado com sucesso apos 0017: UiSmoke 5 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0017.
- [x] `tools/build.ps1` executado com sucesso apos 0017: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0017: 64 testes.
- [x] Fatia 0018 implementou a fundacao de move/restore para Delete/Undo.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Storage|FullyQualifiedName~Integration"` executado com sucesso apos 0018: Storage 10 testes, Integration 2 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0018.
- [x] `tools/build.ps1` executado com sucesso apos 0018: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0018: 71 testes.
- [x] Fatia 0019 implementou `DeleteManager` e `DeleteCurrentPhotoUseCase`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Integration"` executado com sucesso apos 0019: Core 27 testes, Application 22 testes, Integration 3 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0019.
- [x] `tools/build.ps1` executado com sucesso apos 0019: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0019: 82 testes.
- [x] Fatia 0020 implementou `UndoManager` e `UndoLastDeleteUseCase`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Integration"` executado com sucesso apos 0020: Core 32 testes, Application 27 testes, Integration 4 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0020.
- [x] `tools/build.ps1` executado com sucesso apos 0020: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0020: 93 testes.
- [x] Fatia 0021 implementou `JsonlSessionJournalStore` e registro de eventos em delete/restore.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"` executado com sucesso apos 0021: Application 27 testes, Storage 12 testes, Integration 4 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0021.
- [x] `tools/build.ps1` executado com sucesso apos 0021: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0021: 95 testes.
- [x] Fatia 0022 implementou replay basico de journal e reconciliacao inicial por filesystem.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"` executado com sucesso apos 0022: Application 32 testes, Storage 14 testes, Integration 5 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0022.
- [x] `tools/build.ps1` executado com sucesso apos 0022: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0022: 103 testes.
- [x] Fatia 0023 fechou F3-09/F3-10: arquivo bloqueado retorna `FileLocked`/`DeleteFailed`, arquivo ausente vira `Missing`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Core|FullyQualifiedName~Application|FullyQualifiedName~Storage|FullyQualifiedName~Integration"` executado com sucesso apos 0023: Core 33 testes, Application 33 testes, Storage 16 testes, Integration 7 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0023.
- [x] `tools/build.ps1` executado com sucesso apos 0023: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0023: 109 testes.
- [x] Fatia 0024 ligou `Delete` e `Ctrl+Z` ao viewer WinUI, com estado otimista, status discreto e recarga da foto atual/restaurada.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~UiSmoke|FullyQualifiedName~Application|FullyQualifiedName~Integration"` executado com sucesso apos 0024: Application 33 testes, UiSmoke 10 testes, Integration 7 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0024.
- [x] `tools/build.ps1` executado com sucesso apos 0024: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0024: 114 testes.

## Ultima fatia concluida

- 0001 - Governanca e skeleton inicial.
- 0002 - Dominio minimo de sessao e navegacao.
- 0003 - Factory de sessao e scanner filesystem.
- 0004 - DisplayContext e DecodeTargetCalculator.
- 0005 - Use cases de abertura e navegacao.
- 0006 - Dependency injection e composition root.
- 0007 - Prototipo do argumento `--folder`.
- 0008 - Abrir sessao a partir do argumento `--folder`.
- 0009 - Estado inicial da sessao na UI.
- 0010 - Benchmarks iniciais e scanner com `FileInfo` pre-populado.
- 0011 - `WindowsDisplayContextService`.
- 0012 - Analise aprofundada de repositorios de referencia e ajustes do plano de implementacao.
- 0013 - Base de decode JPEG seguro com signature probe, taxonomia de erro e teste de file handle.
- 0014 - Validacao EXIF orientation 6/8 sem dupla aplicacao de rotacao/dimensoes.
- 0015 - Primeira foto no viewer WinUI usando `ImageDecodeResult` convertido para `SoftwareBitmapSource`.
- 0016 - Navegacao visual por setas/espaco no viewer WinUI com cancelamento de decode anterior.
- 0017 - Fullscreen limpo inicial com `F`, `Esc` e recaptura de `DisplayContext`.
- 0018 - Fundacao de move/restore para `_deletadas_evydencia` com colisao, timestamp e read-only.
- 0019 - `DeleteManager` e `DeleteCurrentPhotoUseCase` sem UI, com status, contadores, navegacao e move real em teste de integracao.
- 0020 - `UndoManager` e `UndoLastDeleteUseCase` sem UI, com pilha LIFO por sessao, restore real em teste de integracao e retry seguro em falha.
- 0021 - `JsonlSessionJournalStore` com eventos JSONL append-only para delete e restore, integrado aos use cases sem UI.
- 0022 - Replay basico de journal com `ReplaySessionJournalUseCase`, leitura JSONL, reconciliacao por filesystem e recuperacao de fotos deletadas nao escaneadas.
- 0023 - Tratamento de arquivo bloqueado/ausente no delete, com `FileLocked`, `Missing`, contadores consistentes e journal `DeleteFailed`.
- 0024 - Atalhos `Delete` e `Ctrl+Z` ligados ao viewer WinUI, chamando os use cases reais e preservando contadores/estado visual.

## Cobertura atual de testes

- `Core.Tests`: 33 testes.
- `Application.Tests`: 33 testes.
- `Imaging.Tests`: 15 testes.
- `Storage.Tests`: 16 testes.
- `IntegrationTests`: 7 testes.
- `UiSmokeTests`: 10 testes.
- Total atual: 114 testes.

## Toolchain validado

- .NET SDK: `10.0.203`
- Windows App SDK NuGet: `2.0.1`
- Windows SDK Build Tools NuGet: `10.0.26100.7705`
- WinUI template usado para scaffold: `Microsoft.WindowsAppSDK.WinUI.CSharp.Templates`
- Target principal Windows: `net10.0-windows10.0.19041.0`
- BenchmarkDotNet: `0.15.8`
