# Evydencia Escolher Fotos

Aplicativo Windows nativo para selecao local de fotos por exclusao no fluxo do Estudio Evydencia.

O produto nao e uma galeria generica. A V1 esta focada em abrir uma pasta de sessao, mostrar JPEGs com alta qualidade, navegar rapidamente por teclado e remover da selecao as fotos que o cliente nao quer.

## Escopo V1

Implementar:

- local-first e offline;
- JPEG apenas (`.jpg` e `.jpeg`);
- abertura de pasta por argumento `--folder`;
- abertura manual de pasta pelo app;
- viewer limpo, preparado para fullscreen;
- navegacao por teclado;
- `Delete` com recuperacao local;
- `Ctrl+Z`;
- journal JSONL por sessao;
- logs estruturados;
- decode JPEG dimensionado;
- EXIF orientation;
- cache/prefetch;
- menu de contexto do Windows.

Fora da V1:

- API Laravel;
- login;
- PDV;
- venda/pedido/financeiro;
- upload;
- RAW;
- IA;
- Electron;
- WebView como base do viewer.

## Stack

- .NET 10
- Windows App SDK 2.0.1
- WinUI 3
- C#
- WIC/WinRT Imaging para decode JPEG na V1
- MetadataExtractor para metadados JPEG/EXIF
- Serilog para logs
- Microsoft.Data.Sqlite para estado local derivado
- JSONL para journal append-only

Win2D/Direct2D fica planejado como evolucao atras de abstracoes, nao como dependencia obrigatoria da primeira fatia do viewer.

## Estrutura

```text
/src
  /Evydencia.PhotoSelector.App
  /Evydencia.PhotoSelector.Application
  /Evydencia.PhotoSelector.Core
  /Evydencia.PhotoSelector.Imaging
  /Evydencia.PhotoSelector.Storage
  /Evydencia.PhotoSelector.Infrastructure
  /Evydencia.PhotoSelector.Contracts
  /Evydencia.PhotoSelector.Launcher
  /Evydencia.PhotoSelector.ShellExtension

/tests
  /Evydencia.PhotoSelector.Core.Tests
  /Evydencia.PhotoSelector.Application.Tests
  /Evydencia.PhotoSelector.Imaging.Tests
  /Evydencia.PhotoSelector.Storage.Tests
  /Evydencia.PhotoSelector.IntegrationTests
  /Evydencia.PhotoSelector.UiSmokeTests

/benchmarks
/docs
/tools
/packaging
```

## Comandos

Build:

```powershell
.\tools\build.ps1
```

Build x64:

```powershell
.\tools\build.ps1 -Platform x64
```

Testes:

```powershell
.\tools\test.ps1
```

Testes por filtro:

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging"
```

Formatacao:

```powershell
.\tools\format.ps1
```

Benchmarks:

```powershell
.\tools\benchmarks.ps1
```

Menu de contexto classico para desenvolvimento:

```powershell
.\tools\install-context-menu-dev.ps1 -Platform x64
.\tools\uninstall-context-menu-dev.ps1
```

## Status atual

Consulte o progresso de execucao em:

- `docs/execution-progress.md`

Estado ja validado:

- solution scaffoldada;
- camada `Application` criada;
- dominio minimo de sessao/navegacao criado;
- scanner progressivo com `Directory.EnumerateFiles`;
- `DisplayContext` e `DecodeTargetCalculator`;
- abertura por argumento `--folder`;
- estado inicial da sessao na UI;
- benchmarks iniciais;
- decode JPEG dimensionado;
- EXIF orientation 6/8 validado;
- decode sem prender file handle.
- primeira foto exibida no viewer WinUI via `SoftwareBitmapSource`.
- navegacao visual inicial por `Right`, `Left` e `Space`.
- fullscreen limpo inicial com `F` e `Esc`.
- move/restore local para `_deletadas_evydencia` com colisao segura.
- delete sem UI com `PendingDelete`, `Deleted`, `DeleteFailed`, contadores e navegacao validados.
- undo sem UI com `PendingRestore`, `Restored`, pilha por sessao, contadores e restore real validados.
- journal JSONL append-only para delete/restore.
- replay basico de journal e reconciliacao inicial com filesystem como fonte da verdade.
- delete de arquivo bloqueado/ausente tratado sem corromper contadores: `FileLocked`/`DeleteFailed` e `Missing`.
- `Delete` e `Ctrl+Z` ligados ao viewer WinUI, chamando os use cases reais e atualizando foto/contadores.
- single-instance inicial com `AppInstance`/`RedirectActivationToAsync` antes da criacao da janela.
- `Launcher` minimo para receber pasta do Explorer, validar caminho e encaminhar para o app.
- fallback HKCU de menu de contexto de desenvolvimento para clique em pasta e no fundo da pasta.

Proximas fatias planejadas:

- overlay temporizado, retry visual e polimento do fluxo de erro no viewer;
- validar single-instance com MSIX registrado/instalado; no ambiente atual o registro loose package esta bloqueado por sideload/developer mode desabilitado;
- menu moderno do Explorer com `ShellExtension`/`IExplorerCommand`;

## Documentacao principal

Leia primeiro:

- `AGENTS.md`
- `PLANS.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `docs/execution-progress.md`

ADRs ficam em:

- `docs/adr`

## Regras de desenvolvimento

- V1 deve continuar offline e JPEG-only.
- Nao adicionar API, login, PDV, upload, RAW, IA, Electron ou WebView.
- Nao bloquear a UI thread com scan, decode, cache, move de arquivo ou journal pesado.
- Nao decodificar JPEG full-res no modo fit normal.
- Nao manter `FileStream` vivo depois do decode.
- `Core` nao depende de WinUI, Storage, Imaging, Shell, logs ou HTTP.
- `Application` orquestra casos de uso e mantem ViewModels finos.
- `ShellExtension` e `Launcher` devem permanecer minimos.
