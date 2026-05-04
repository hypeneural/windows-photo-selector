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
- [x] F0-12 - Implementar single-instance inicial com ativacao por pasta.
- [ ] F0-19 - Validar single-instance em app registrado/empacotado com duas ativacoes reais. MSIX dev assinado ja e gerado, mas instalacao ainda depende de confiar o certificado em `LocalMachine\TrustedPeople` ou usar certificado confiavel.
- [x] F0-13 - Validar decode sem prender file handle.
- [x] F0-14 - Criar `DisplayContext`.
- [x] F0-15 - Validar preview cache sem perda visual perceptivel na primeira fatia: cache em memoria guarda pixels decodificados, sem recompressao em JPEG.
- [x] F0-16 - ADR de fonte da verdade criado.
- [x] F0-17 - Governanca de build documentada.
- [x] F0-18 - Camada Application definida.
- [x] F0-20 - Implementar `Launcher` minimo e fallback HKCU de menu de contexto para desenvolvimento.

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
- [x] F2-06 - Corrigir atalhos do viewer para nao dependerem de foco no `ViewerHost`.
- [x] F2-07 - Esconder tooltip de aceleradores e adicionar zoom por roda do mouse.
- [x] F2-08 - Adicionar pan/arrastar quando a imagem esta com zoom, reset por duplo clique e atalhos `+`, `-`, `0`.
- [x] F2-09 - Adicionar overlay temporizado no viewer.
- [x] F2-10 - Adicionar atalho `1` com decode full-res sob demanda para zoom 100% real.
- [x] F2-11 - Adicionar cache em memoria LRU e prefetch de previews proximos/anteriores.

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

- `ShellExtension` ainda nao tem projeto C++/WinRT. A decisao continua adiada para a fase de menu de contexto moderno.
- `Launcher` minimo ja recebe pasta, valida caminho e encaminha para o app com `--folder`; os scripts HKCU de desenvolvimento instalam/removem `Abrir Escolher Fotos` para clique em pasta e no fundo da pasta.
- Single-instance inicial ja usa `AppInstance.FindOrRegisterForKey` e `RedirectActivationToAsync` antes da criacao da janela. A validacao com app registrado/empacotado ainda precisa acontecer antes do menu de contexto do Explorer.
- Empacotamento dev assinado ja tem scripts para gerar MSIX, confiar certificado em ambiente elevado, instalar/reinstalar e rodar smoke single-instance instalado.
- Conversao do resultado de decode para `ImageSource`/viewer WinUI ja existe para a primeira foto, com navegacao visual por `Right`, `Left`, `Space`, `Home` e `End`; atalhos do viewer agora usam `KeyboardAccelerator` na pagina e `KeyDown` fica como fallback do `ViewerHost`.
- Fullscreen inicial ja usa `AppWindow`/`FullScreenPresenter`, `F` alterna, `Esc` sai, e o decode recaptura `DisplayContext` com estado fullscreen.
- Zoom optico sobre o preview atual ja existe por roda do mouse e atalhos `+`/`-`; `0` e duplo clique retornam para fit; pan/arrastar funciona quando a imagem esta ampliada; a imagem ficou `IsHitTestVisible=false` para manter os eventos de ponteiro no `ViewerHost`. O atalho `1` agora faz decode full-res sob demanda, respeita EXIF e calcula escala para 100% real em pixels fisicos; o resultado pode ser reutilizado pelo cache LRU, mas prefetch continua restrito a previews fit.
- `FileMoveService` ja move para `_deletadas_evydencia`, restaura, resolve colisao e preserva `LastWriteTimeUtc`; ja esta ligado ao `DeleteCurrentPhotoUseCase`.
- `DeleteManager`, `DeleteCurrentPhotoUseCase`, `UndoManager` e `UndoLastDeleteUseCase` ja validam `PendingDelete`, `Deleted`, `PendingRestore`, `Restored`, `Missing`, `DeleteFailed`, contadores e navegacao; `JsonlSessionJournalStore` ja registra delete/restore em JSONL; `ReplaySessionJournalUseCase` ja faz replay basico e reconciliacao inicial; os atalhos `Delete` e `Ctrl+Z` ja chamam os use cases reais no viewer.
- Overlay temporizado ja esconde status/contador apos atividade. Retry visual de falha e fila de comandos para deletes muito rapidos ainda ficam para UX/performance posterior.
- Cache em memoria LRU e prefetch leve de previews ja existem. Cache em disco, thumbnail/filmstrip, configuracoes, ultimas sessoes, shell extension moderno `IExplorerCommand`, smoke MSIX empacotado e segunda tela seguem pendentes.
- `WindowsDisplayContextService` captura `XamlRoot`, area util, escala de rasterizacao e estado fullscreen. Identificacao detalhada de monitor/display area fica para a fatia de segunda tela.

## Fila de implementacao do viewer

- [ ] VQ-01 - Atomic image swap para remover tela preta na navegacao.
- [ ] VQ-04 - Telemetria de `time_to_visible`, cache hit/miss, decode e source creation.
- [ ] VQ-03 - `ViewerImageSourceCache` no App para reduzir custo de `SoftwareBitmapSource`.
- [ ] VQ-02 - `ZoomQualityPolicy` e re-decode automatico por tiers de zoom.
- [ ] VQ-05 - Prefetch direcional/adaptativo e smoke de navegacao rapida.
- [ ] VQ-06 - Validar fullscreen real em unpackaged, MSIX, `--folder` e menu dev.
- [ ] VQ-07 - Settings base para delete mode, cache, prefetch, fullscreen e quality mode.
- [ ] VQ-09 - Spike Win2D/Direct2D isolado.
- [ ] VQ-08 - Validar MSIX empacotado e implementar ShellExtension/IExplorerCommand moderno.

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
- [x] Fatia 0025 implementou single-instance inicial com `Program.cs` customizado, `DISABLE_XAML_GENERATED_MAIN`, `AppInstanceCoordinator` e reativacao por argumentos `Launch`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~OpenFolderFromArgumentsUseCase"` executado com sucesso apos 0025: Application 4 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0025.
- [x] `tools/build.ps1` executado com sucesso apos 0025: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0025: 115 testes.
- [ ] Smoke real por executavel direto na 0025 ficou inconclusivo: o processo encerrou com `0xE0434352`/`REGDB_E_CLASSNOTREG` antes da instalacao local do Windows App Runtime 2.0.1.
- [x] Fatia 0026 instalou Windows App Runtime 2.0.1 x86/x64 a partir dos MSIX oficiais do NuGet local.
- [ ] Registro loose package com `Add-AppxPackage -Register` falhou com `0x80073CFF`: sideload/developer mode desabilitado. A sessao atual nao tem permissao para habilitar `AppModelUnlock` em HKLM.
- [ ] Smoke por executavel direto apos runtime 2.0 continua falhando com `REGDB_E_CLASSNOTREG` no `DeploymentManager` enquanto o pacote nao esta registrado/instalado.
- [x] Fatia 0026 implementou `Evydencia.PhotoSelector.Launcher` minimo e `tools/install-context-menu-dev.ps1`/`tools/uninstall-context-menu-dev.ps1`.
- [x] `tools/install-context-menu-dev.ps1 -Platform x64` validado: criou chaves HKCU para `Directory` e `Directory\Background`.
- [x] `tools/uninstall-context-menu-dev.ps1` validado: removeu chaves HKCU.
- [x] `tools/format.ps1` executado com sucesso apos 0026.
- [x] `tools/build.ps1` executado com sucesso apos 0026: 0 warnings.
- [x] `tools/build.ps1 -Platform x64` executado com sucesso apos 0026: 0 warnings.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Launcher"` executado com sucesso apos 0026: Launcher 7 testes.
- [x] `tools/test.ps1` executado com sucesso apos 0026: 122 testes.
- [x] Fatia 0027 criou script de MSIX dev assinado e gerou pacote x64 em `artifacts/packages`.
- [x] `tools/package-msix-dev.ps1` passou a assinar por `PackageCertificateThumbprint`; o publish ficou sem warnings de importacao de PFX, restando apenas aviso de symbols por ausencia de `mspdbcmf.exe`.
- [x] `Get-AuthenticodeSignature` validou assinatura do MSIX depois de confiar o certificado em `CurrentUser\Root`.
- [ ] `Add-AppxPackage` do MSIX dev ainda falhou com `0x800B0109`; a correcao exige confiar o certificado em `LocalMachine` via PowerShell elevado ou usar certificado confiavel.
- [x] `tools/trust-msix-dev-cert-admin.ps1` validado em sessao nao elevada: falha com instrucao clara para PowerShell elevado.
- [x] `tools/smoke-msix-single-instance.ps1` validado sem pacote instalado: falha com instrucao clara para package/install.
- [x] `tools/format.ps1` executado com sucesso apos 0027.
- [x] `tools/build.ps1` executado com sucesso apos 0027: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0027: 122 testes.
- [ ] `tools/smoke-msix-single-instance.ps1` ainda depende do pacote instalado para validar duas ativacoes reais.
- [x] Fatia 0028 ajustou `trust-msix-dev-cert-admin.ps1` para priorizar `LocalMachine\TrustedPeople`, mantendo `LocalMachine\Root` apenas por `-TrustRoot`.
- [x] `tools/check-msix-dev-prereqs.ps1` criado e executado: assinatura valida, certificado em `CurrentUser`, ausente em `LocalMachine\TrustedPeople`, pacote nao instalado.
- [x] Sintaxe PowerShell validada para `check-msix-dev-prereqs`, `trust-msix-dev-cert-admin`, `install-msix-dev`, `package-msix-dev` e `smoke-msix-single-instance`.
- [x] `tools/trust-msix-dev-cert-admin.ps1` validado em sessao nao elevada apos 0028: falha com instrucao clara para `LocalMachine\TrustedPeople`.
- [x] `tools/format.ps1` executado com sucesso apos 0028.
- [x] `tools/build.ps1` executado com sucesso apos 0028: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0028: 122 testes.
- [ ] `Add-AppxPackage` e smoke empacotado seguem bloqueados ate executar trust em PowerShell elevado.
- [x] Fatia 0029 instalou o MSIX dev apos confiar certificado em `LocalMachine\TrustedPeople`.
- [x] `appExecutionAlias` adicionado ao manifest para `EvydenciaPhotoSelector.exe` e `AbrirEscolherFotos.exe`.
- [x] Viewer aberto para preview visual da pasta `C:\Users\Usuario\Desktop\bkp geral\BKP 1712\SD 3\DCIM\100CAROL` via publish unpackaged dev.
- [ ] Smoke MSIX empacotado ainda nao passou: `Start-Process` pelo alias retornou acesso/execucao negados nesta sessao, `IApplicationActivationManager` retornou `0x80270254` e `Invoke-CommandInDesktopPackage` falhou com `AccessViolationException`.
- [x] `tools/format.ps1` executado com sucesso apos 0029.
- [x] `tools/build.ps1` executado com sucesso apos 0029: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0029: 122 testes.
- [x] Fatia 0030 corrigiu a entrada de teclado do viewer: `Right`, `Left`, `Space`, `Delete`, `Ctrl+Z`, `F`, `Esc`, `Home` e `End` foram registrados como `KeyboardAccelerator` de pagina; `ViewerHost.KeyDown` permanece como fallback; falhas assincronas de atalho agora viram status discreto em vez de derrubar o app.
- [x] Smoke manual automatizado validou `Right`/`Left` em pasta real por UI Automation e `Delete`/`Ctrl+Z` em pasta temporaria com copias de JPEGs; a pasta real foi reaberta para validacao manual.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Application|FullyQualifiedName~UiSmoke"` executado com sucesso apos 0030: Application 36 testes, UiSmoke 12 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0030.
- [x] `tools/build.ps1` executado com sucesso apos 0030: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0030: 126 testes.
- [x] Fatia 0031 removeu o tooltip fixo de acelerador com `KeyboardAcceleratorPlacementMode="Hidden"` e adicionou zoom por roda do mouse com `PointerWheelChanged` + `ScaleTransform`.
- [x] Smoke manual automatizado validou ausencia de tooltip `Direita` apos `Right` e `Zoom 120%` apos wheel up na pasta real.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~UiSmoke"` executado com sucesso apos 0031: UiSmoke 14 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0031.
- [x] `tools/build.ps1` executado com sucesso apos 0031: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0031: 128 testes.
- [x] Fatia 0032 adicionou `CompositeTransform` com pan/drag, reset por duplo clique, atalhos `+`/`-`/`0`, imagem sem hit-test e overlay temporizado por `DispatcherTimer`.
- [x] Smoke real automatizado validou zoom por teclado `+`, reset por duplo clique e ocultacao temporizada do overlay na pasta real `C:\Users\Usuario\Desktop\bkp geral\BKP 1712\SD 3\DCIM\100CAROL`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~UiSmoke"` executado com sucesso apos 0032: UiSmoke 17 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0032.
- [x] `tools/build.ps1` executado com sucesso apos 0032: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0032: 131 testes.
- [x] Fatia 0033 implementou `DecodeActualSizeAsync`, atalho `1` para 100% real, reset `0` recarregando preview fit e testes de full-res/orientacao/file handle.
- [x] Smoke real automatizado validou `1`: `Carregando 100%` -> `Zoom 100%`, e `0`: `Ajustado a tela`, na pasta real `C:\Users\Usuario\Desktop\bkp geral\BKP 1712\SD 3\DCIM\100CAROL`.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Imaging|FullyQualifiedName~UiSmoke"` executado com sucesso apos 0033: Imaging 18 testes, UiSmoke 18 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0033.
- [x] `tools/build.ps1` executado com sucesso apos 0033: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0033: 135 testes.
- [x] Fatia 0034 implementou `MemoryImageCache` LRU, `PreviewCacheService` e `PrefetchScheduler` para previews fit, com actual-size reutilizavel por cache e prefetch restrito a proximas/anteriores.
- [x] `tools/test.ps1 -Filter "FullyQualifiedName~Imaging|FullyQualifiedName~UiSmoke"` executado com sucesso apos 0034: Imaging 25 testes, UiSmoke 19 testes.
- [x] `tools/format.ps1` executado com sucesso apos 0034.
- [x] `tools/build.ps1` executado com sucesso apos 0034: 0 warnings.
- [x] `tools/test.ps1` executado com sucesso apos 0034: 143 testes.
- [x] Smoke real unpackaged dev abriu a pasta `C:\Users\Usuario\Desktop\bkp geral\BKP 1712\SD 3\DCIM\100CAROL` apos 0034.

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
- 0025 - Single-instance inicial com redirecionamento antes da janela e reativacao por pasta na instancia principal.
- 0026 - Validacao de runtime/registro empacotado, `Launcher` minimo e fallback HKCU de menu de contexto de desenvolvimento.
- 0027 - MSIX dev assinado e scripts de instalacao/smoke para desbloquear validacao single-instance empacotada; F0-19 permanece pendente ate instalar o pacote e passar o smoke.
- 0028 - Diagnostico de pre-requisitos MSIX dev e ajuste do trust de certificado para `LocalMachine\TrustedPeople`.
- 0029 - Preview unpackaged do viewer com pasta real e investigacao do launch empacotado por alias MSIX.
- 0030 - Atalhos do viewer corrigidos com `KeyboardAccelerator` de pagina, fallback `KeyDown`, `Home`/`End` e smoke manual automatizado de navegacao/delete/undo.
- 0031 - Tooltip fixo de acelerador ocultado e zoom por roda do mouse implementado no viewer.
- 0032 - Pan/arrastar em zoom, reset por duplo clique, atalhos `+`/`-`/`0` e overlay temporizado no viewer.
- 0033 - Atalho `1` com decode full-res sob demanda para zoom 100% real, mantendo fit normal com preview dimensionado.
- 0034 - Cache em memoria LRU para previews/actual-size sob demanda e prefetch de previews das proximas 3 fotos e anteriores 2.

## Cobertura atual de testes

- `Core.Tests`: 33 testes.
- `Application.Tests`: 36 testes.
- `Imaging.Tests`: 25 testes.
- `Storage.Tests`: 16 testes.
- `IntegrationTests`: 7 testes.
- `Launcher.Tests`: 7 testes.
- `UiSmokeTests`: 19 testes.
- Total atual: 143 testes.

## Toolchain validado

- .NET SDK: `10.0.203`
- Windows App SDK NuGet: `2.0.1`
- Windows App Runtime local: `2.0.1` instalado por MSIX do NuGet para validacao, mas app ainda precisa de registro/instalacao MSIX por politica de sideload.
- Windows SDK Build Tools NuGet: `10.0.26100.7705`
- WinUI template usado para scaffold: `Microsoft.WindowsAppSDK.WinUI.CSharp.Templates`
- Target principal Windows: `net10.0-windows10.0.19041.0`
- BenchmarkDotNet: `0.15.8`
