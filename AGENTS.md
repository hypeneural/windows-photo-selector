# AGENTS.md - Evydencia Escolher Fotos

## Projeto

Este repositorio contem o **Evydencia Escolher Fotos**, um aplicativo Windows nativo para selecao de fotos em estudio.

O produto nao e uma galeria generica. Ele e um visualizador JPEG de alta performance para o fluxo real de selecao por exclusao:

1. O operador abre uma pasta de sessao pelo Windows Explorer.
2. O menu de contexto mostra `Abrir Escolher Fotos`.
3. O app abre a pasta em um viewer limpo, preferencialmente fullscreen.
4. O cliente navega com setas.
5. O cliente pressiona `Delete` nas fotos que nao quer.
6. A foto some imediatamente da navegacao ativa.
7. `Ctrl+Z` restaura a ultima exclusao.
8. O app mantem contadores, journal local, logs e estado recuperavel.

## Escopo V1

V1 e local-first e offline.

Implementar na V1:

- abrir pasta por argumento de linha de comando;
- abrir pasta manualmente pelo app;
- listar apenas `.jpg` e `.jpeg`;
- ordenar inicialmente por nome;
- mostrar a primeira foto rapidamente;
- navegar com teclado;
- fullscreen limpo;
- overlay discreto;
- exclusao local recuperavel;
- `Ctrl+Z`;
- journal JSONL por sessao;
- logs estruturados;
- cache e prefetch;
- menu de contexto do Windows;
- testes e benchmarks.

Nao implementar na V1:

- API Laravel;
- login;
- PDV;
- venda;
- pedido;
- financeiro;
- upload;
- RAW;
- IA;
- sincronizacao remota;
- viewer baseado em Electron;
- viewer baseado em WebView;
- edicao de imagem.

Se uma tarefa pedir recursos de API, PDV ou segunda tela antes da base do viewer estar pronta, crie apenas contratos ou pontos de extensao documentados, sem implementacao remota.

## Anti-patterns

Nao faca:

- criar uma galeria generica de fotos;
- adicionar albums, tags, cloud sync, IA, ratings ou RAW na V1;
- usar `Directory.GetFiles` para pastas grandes;
- gerar thumbnails antes de mostrar a primeira foto;
- decodificar JPEG full-res no fit fullscreen normal;
- manter `FileStream` vivo na UI;
- colocar regras de delete/undo em ViewModels;
- fazer ShellExtension referenciar logica pesada de Core/Imaging;
- usar SQLite como unica fonte da verdade de estado das fotos;
- excluir permanentemente por padrao;
- renderizar filmstrip enquanto o cliente navega em fullscreen V1;
- adicionar chamadas de API dentro de Core, Imaging, DeleteManager ou UndoManager.

## Documentacao canonica

Antes de alterar arquitetura, leia o documento principal:

- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `PLANS.md` para tarefas nao triviais ou multi-etapas.

Quando existirem, mantenha estes documentos sincronizados:

- `docs/architecture.md`
- `docs/image-pipeline.md`
- `docs/performance-plan.md`
- `docs/context-menu.md`
- `docs/api-future-integration.md`
- `docs/release-checklist.md`
- `docs/adr/*.md`

Crie ou atualize ADRs quando uma decisao mudar stack, empacotamento, pipeline de imagem, estrategia de delete, cache, source of truth, menu de contexto, single-instance ou segunda tela.

## Planejamento obrigatorio

Antes de implementar qualquer tarefa nao trivial, produza um plano seguindo `PLANS.md`.

Use plano para trabalhos em:

- `FolderScanPolicy`, `IFolderScanner` ou `FileSystemFolderScanner`;
- `JpegDecodeService`;
- `PreviewCacheService`;
- `DeleteManager`;
- `UndoManager`;
- `AppActivationService`;
- single-instance;
- ShellExtension;
- benchmarks;
- mudancas de arquitetura.

O plano deve listar objetivo, arquivos a ler, camada afetada, restricoes, passos pequenos, testes, criterio de aceite, riscos e comandos.

## Stack

Fase 0 deve validar duas combinacoes antes da implementacao principal:

- Opcao A, conservadora: `.NET 8` + `Windows App SDK 1.8`.
- Opcao B, recomendada para produto novo em 2026: `.NET 10 LTS` + `Windows App SDK 2.0.1`.

Use a Opcao B se WinUI 3, empacotamento, MSIX, contexto do Explorer e ambiente local estiverem estaveis. Use a Opcao A apenas como fallback tecnico documentado.

Stack desejada:

- Windows App SDK;
- WinUI 3;
- C#/.NET;
- WIC/Windows Imaging Component para JPEG na V1;
- MetadataExtractor para EXIF/orientation/metadados basicos;
- Serilog para logs estruturados;
- Microsoft.Data.Sqlite para estado local derivado;
- JSONL para journal append-only;
- Win2D/Direct2D como evolucao atras de abstracoes, nao como dependencia obrigatoria da primeira fatia.

## Estrutura esperada

```text
/src
  /Evydencia.PhotoSelector.App
  /Evydencia.PhotoSelector.Application
  /Evydencia.PhotoSelector.Core
  /Evydencia.PhotoSelector.Imaging
  /Evydencia.PhotoSelector.Storage
  /Evydencia.PhotoSelector.Infrastructure
  /Evydencia.PhotoSelector.Contracts
  /Evydencia.PhotoSelector.ShellExtension
  /Evydencia.PhotoSelector.Launcher

/tests
  /Evydencia.PhotoSelector.Core.Tests
  /Evydencia.PhotoSelector.Application.Tests
  /Evydencia.PhotoSelector.Imaging.Tests
  /Evydencia.PhotoSelector.Storage.Tests
  /Evydencia.PhotoSelector.IntegrationTests
  /Evydencia.PhotoSelector.UiSmokeTests

/benchmarks
  /Evydencia.PhotoSelector.Benchmarks

/performance-smoke

/packaging
  /msix
  /certificates-dev
  /manifest-fragments
  /context-menu

/docs
  /adr
  architecture.md
  image-pipeline.md
  performance-plan.md
  context-menu.md
  api-future-integration.md
  release-checklist.md

/tools
```

## Limites entre camadas

`Application` contem casos de uso e orquestracao:

- `OpenSessionUseCase`;
- `NavigateNextPhotoUseCase`;
- `NavigatePreviousPhotoUseCase`;
- `DeleteCurrentPhotoUseCase`;
- `UndoLastDeleteUseCase`;
- `RecoverSessionUseCase`;
- `BuildLocalSelectionSummaryUseCase`;
- `PrepareViewerImageUseCase`;
- abstracoes para scanner, file move, journal, settings, preview e performance.

`Application` nao pode referenciar WinUI, XAML, `Window`, `Page` ou controles. Ela coordena `Core` com interfaces implementadas por `Storage`, `Imaging` e `Infrastructure`.

`Core` contem regras de dominio local:

- `PhotoSession`;
- `PhotoItem`;
- `PhotoStatus`;
- `NavigationController`;
- `DeleteManager`;
- `UndoManager`;
- contadores;
- regras de reconciliacao.

`Core` nao pode referenciar WinUI, Windows App SDK, WIC, Win2D, SQLite, Serilog, Shell, HTTP ou filesystem concreto.

`Imaging` contem:

- decode JPEG;
- leitura de EXIF orientation;
- calculo de tamanho de preview;
- cache em memoria;
- cache em disco;
- thumbnails;
- prefetch;
- cancelamento de decode obsoleto.

`Storage` contem:

- settings;
- recent sessions;
- journal JSONL;
- stores SQLite;
- file move service;
- cache path provider;
- limpeza de cache.

`Infrastructure` contem:

- dependency injection;
- logging;
- diagnostics;
- medicao de performance;
- tratamento global de excecoes.

`App` contem:

- WinUI pages/windows;
- bindings;
- keyboard input;
- fullscreen;
- overlay;
- activation;
- composicao de DI.

`ShellExtension` e `Launcher` devem ser minimos:

- receber caminho do Explorer;
- normalizar e validar caminho;
- ativar ou redirecionar para a instancia principal;
- sair.

Eles nao devem escanear pastas, decodificar imagens, alterar arquivos, escrever journal ou carregar dependencias pesadas.

Nao crie `Evydencia.PhotoSelector.Shell` na V1 enquanto nao houver codigo compartilhado real entre ShellExtension e Launcher.

`Contracts` contem apenas DTOs minimos na V1, como `LocalSelectionSummary` e `DeletedPhotoEventDto`. Interfaces remotas e contexto de pedido entram somente na fase futura aprovada por ADR.

## Single-instance e ativacao

O app deve ser single-instance por padrao.

Comportamento esperado:

- app fechado + `--folder`: abrir a sessao;
- app aberto sem sessao: carregar a pasta recebida;
- app aberto com sessao ativa: perguntar se deve abrir nova sessao ou manter a atual;
- ativacoes do Explorer devem redirecionar para a instancia principal;
- cliques repetidos no menu de contexto nao devem abrir multiplas janelas/processos.

Use APIs de lifecycle/instancing do Windows App SDK quando a stack validada suportar.

## App lifecycle requirement

Single-instance deve estar implementado antes do menu de contexto do Explorer ser considerado completo.

O redirecionamento de ativacao deve acontecer antes de criar a janela principal. Nao crie janelas antes de decidir se o processo atual e a instancia primaria ou secundaria.

## Regras de scan

O scan inicial deve ser progressivo.

- Use `Directory.EnumerateFiles`, nao `Directory.GetFiles`, para pastas grandes.
- Aceite apenas `.jpg` e `.jpeg`, case-insensitive.
- Ignore `_deletadas_evydencia`.
- Nao recursar subpastas na V1.
- Fase A deve capturar apenas dados baratos: caminho, nome, extensao, tamanho, last write time e sort index.
- Fase B deve rodar em background: dimensoes, EXIF orientation, capture date e cache key.

A primeira imagem nao pode depender da geracao de thumbnails nem da leitura completa de metadados de todas as fotos.

Teste obrigatorio de scan:

- uma pasta com 2.000 JPEGs deve mostrar a primeira imagem disponivel sem esperar metadados completos ou thumbnails.

## Pipeline de imagem

O viewer deve parecer instantaneo e manter qualidade alta.

Regras:

- nunca decodificar JPEG full-res no modo fit normal;
- calcular o tamanho fisico do monitor atual;
- aplicar DPI/rasterization scale;
- calcular area util do viewer;
- aplicar EXIF orientation antes do fit;
- usar fit-contain sem crop por padrao;
- definir decode target com margem de 1.15x a 1.35x;
- full-res somente para zoom 100% ou inspecao futura;
- cancelar decode obsoleto quando o usuario navega rapido;
- liberar streams imediatamente apos decode;
- nao manter file handle preso em `ImageSource`, `BitmapImage` ou pipeline equivalente.

Toda leitura de JPEG deve usar stream curto e compativel com move/delete:

```csharp
FileAccess.Read
FileShare.ReadWrite | FileShare.Delete
FileOptions.Asynchronous | FileOptions.SequentialScan
```

Crie e mantenha `DisplayContext` desde cedo para DPI, multi-monitor, fullscreen e segunda tela futura.

Aceite de decode:

- qualquer PR que exiba imagens deve explicar onde o target decode size e calculado;
- como DPI/rasterization scale e considerado;
- como EXIF orientation altera dimensoes;
- por que full-res decode nao e usado no fit normal;
- como decodes obsoletos sao cancelados.

## Performance budgets V1

Metas iniciais, a medir com JPEGs reais de estudio:

- 500 JPEGs: abertura da pasta mantem a UI responsiva;
- 2.000 JPEGs: app nao espera thumbnails para navegar;
- time to first image: alvo <= 1.5s em maquina SSD tipica;
- navegacao cached: percepcao alvo <= 50ms;
- resposta visual do `Delete`: imediata, antes do move terminar;
- fit normal nunca usa full-res decode;
- nenhum `FileStream` fica retido depois do decode.

## Cache e prefetch

Use cache sem comprometer qualidade visual.

Memory cache:

- LRU;
- prioridade para foto atual, proximas 3 e anteriores 2;
- limite configuravel;
- padrao dinamico quando possivel: minimo 512 MB, preferencialmente baseado em RAM disponivel com teto inicial razoavel.

Disk cache:

- em `%LOCALAPPDATA%\Evydencia\PhotoSelector\Cache`;
- thumbnails podem ser JPEG 85-90;
- previews fullscreen persistidos, se existirem, devem usar qualidade alta, preferencialmente 95+ ou formato que nao degrade perceptivelmente;
- nunca sobrescrever original;
- nunca usar preview recomprimido para zoom 100%;
- limpar por idade e tamanho;
- nao varrer todo cache no caminho critico de startup.

Cache key deve incluir caminho normalizado, tamanho, last write UTC, orientation, target size, versao do algoritmo e modo de qualidade/cor.

## Delete e undo

O fluxo principal e selecao por exclusao.

Default V1:

- mover para `<session-folder>\_deletadas_evydencia`;
- esconder imediatamente da navegacao ativa;
- navegar para a proxima foto;
- registrar journal;
- permitir `Ctrl+Z`;
- nunca excluir permanentemente por padrao.

Use estados explicitos:

- `Active`;
- `PendingDelete`;
- `Deleted`;
- `PendingRestore`;
- `Restored`;
- `Missing`;
- `DeleteFailed`.

Delete:

1. marcar item como `PendingDelete`;
2. esconder da lista ativa;
3. navegar para a proxima foto;
4. mover arquivo em background;
5. em sucesso, marcar `Deleted` e registrar journal;
6. em falha, marcar `DeleteFailed`, manter contadores consistentes e mostrar erro discreto.

Undo:

1. marcar item como `PendingRestore`;
2. mover de `_deletadas_evydencia` para o caminho original;
3. se houver colisao, aplicar politica segura;
4. recolocar no `SortIndex` original;
5. registrar journal;
6. preferir navegar para a foto restaurada.

## Fonte da verdade

Sistema de arquivos + journal JSONL sao a fonte da verdade da sessao.

SQLite e estado derivado para:

- sessoes recentes;
- indice de cache;
- configuracoes;
- metricas agregadas;
- aceleracao de abertura.

Ao reabrir sessao:

1. escanear pasta original;
2. escanear `_deletadas_evydencia`;
3. replay do journal;
4. reconciliar com o filesystem atual;
5. arquivos fora de ambos os locais viram `Missing`;
6. conflitos devem ser logados e resolvidos privilegiando a realidade do filesystem.

## Context menu

Separe tres modos:

1. Desenvolvimento rapido: registro HKCU classico, geralmente em `Mostrar mais opcoes`, para testar `--folder`.
2. Produto instalavel interno: MSIX assinado, package identity, `IExplorerCommand`, app abre por argumento/URI.
3. Produto profissional futuro: MSIX ou sparse package, certificado confiavel, auto-update, logs de ativacao Shell.

O item deve se chamar `Abrir Escolher Fotos`.

Tratar:

- clique sobre uma pasta;
- clique no fundo de uma pasta;
- caminhos invalidos;
- multiplos caminhos selecionados;
- instalacao e remocao segura do menu.

## UI e atalhos

Fullscreen V1:

- fundo preto ou cinza escuro;
- foto centralizada;
- sem miniaturas fixas;
- sem barras permanentes;
- overlay aparece apos mouse/teclado e some automaticamente;
- sem painel de venda, pacote, pedido ou API.

Atalhos obrigatorios:

- `Right`: proxima;
- `Left`: anterior;
- `Space`: proxima;
- `Delete`: excluir/remover atual;
- `Ctrl+Z`: desfazer;
- `F`: alternar fullscreen;
- `Esc`: sair do fullscreen;
- `Home`: primeira;
- `End`: ultima.

Nao dispare atalhos de viewer quando o foco estiver em input de texto.

## Cor e qualidade

V1:

- modo rapido assume sRGB;
- ler presenca de ICC quando barato;
- registrar no log se houver ICC diferente;
- nao transformar ICC por padrao.

V1.5:

- modo `Cor precisa`;
- avaliar perfil do monitor e transformacao ICC/WIC;
- documentar impacto de performance.

## Testes e benchmarks

Testes unitarios devem cobrir:

- sessao;
- scan JPEG;
- navegacao;
- delete em inicio/meio/fim;
- undo;
- contadores;
- estados pending/falha;
- journal replay;
- reconciliacao.

Testes de imaging devem cobrir:

- EXIF orientation;
- decode target sizing;
- cache key;
- invalidacao;
- LRU;
- cancelamento de decode obsoleto;
- file handle nao bloqueia move/delete.

Testes de integracao devem usar pasta temporaria e cobrir:

- mover para `_deletadas_evydencia`;
- restaurar com `Ctrl+Z`;
- colisao de nome;
- arquivo bloqueado;
- arquivo read-only;
- long path;
- crash/replay.

Benchmarks devem cobrir:

- scan 50, 500 e 2.000 JPEGs;
- decode preview de JPEG grande;
- navegacao cached/uncached;
- delete/undo em volume;
- cache hit/miss;
- uso de memoria.

Resultados de performance devem ser salvos em `/artifacts/performance` ou local documentado.

## Comandos

Use estes scripts como entrada padrao:

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Core"
.\tools\benchmarks.ps1
```

Scripts adicionais poderao existir para empacotamento e contexto:

```powershell
.\tools\package-msix.ps1
.\tools\install-context-menu-dev.ps1
.\tools\uninstall-context-menu-dev.ps1
.\tools\generate-test-jpegs.ps1
```

Quando os projetos existirem, prefira rodar o menor conjunto relevante:

```powershell
dotnet test .\tests\Evydencia.PhotoSelector.Core.Tests
dotnet test .\tests\Evydencia.PhotoSelector.Imaging.Tests
dotnet test .\tests\Evydencia.PhotoSelector.IntegrationTests
dotnet run -c Release --project .\benchmarks\Evydencia.PhotoSelector.Benchmarks
```

## Comportamento esperado do Codex

Antes de codar:

1. leia este arquivo;
2. leia a documentacao relevante em `/docs`;
3. use a skill local `.agents/skills/windows-photo-selector/SKILL.md` quando a tarefa tocar o app Windows;
4. identifique a camada afetada;
5. mantenha a mudanca pequena;
6. adicione ou atualize testes;
7. rode os testes relevantes;
8. reporte arquivos alterados, testes executados e riscos restantes.

Durante revisoes, priorize:

- bloqueio da UI thread;
- vazamento de file handles;
- delete/undo inconsistente;
- decodificacao full-res desnecessaria;
- `Core` dependendo de UI/infra;
- ShellExtension carregando logica pesada;
- V1 sendo contaminada por API/PDV/login;
- falta de cancelamento em tarefas de imagem;
- logs sem utilidade ou com dados sensiveis desnecessarios.

## Definition of Done

Uma tarefa so esta pronta quando:

1. a mudanca respeita limites de camada;
2. a V1 continua offline e JPEG-only;
3. nao ha API, PDV, upload, RAW, Electron ou WebView;
4. a UI thread nao executa scan, decode, cache, file move ou journal pesado;
5. testes relevantes foram adicionados ou atualizados;
6. os comandos minimos da tarefa foram rodados ou a impossibilidade foi explicada;
7. o Codex informou arquivos alterados, camada alterada, testes executados, riscos restantes, impacto de performance e desvios do `AGENTS.md`, se houver;
8. mudancas arquiteturais atualizaram ou criaram ADR.

Se uma solicitacao conflitar com este arquivo, explique o conflito antes de implementar.
