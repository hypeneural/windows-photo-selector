# Viewer quality and performance gap analysis

Ultima atualizacao: 2026-05-04

## Objetivo

Este documento resume o estado atual do Evydencia Escolher Fotos, o que ja esta pronto e o que ainda precisa melhorar para o viewer chegar perto da experiencia do visualizador de imagens do Windows/Fotos: navegacao praticamente instantanea, zoom mantendo nitidez, tela sem piscada preta e fullscreen realmente limpo.

O foco continua sendo V1 local-first, offline e JPEG-only. Nao entram API Laravel, login, PDV, upload, RAW, IA, Electron ou WebView.

## Referencias oficiais consultadas

- Microsoft Learn, `Image` WinUI: recomenda controlar `DecodePixelWidth`/`DecodePixelHeight` para imagens grandes quando o tamanho de exibicao e menor que o original, reduzindo memoria e custo de decode.
  <https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.image>
- Microsoft Learn, otimizar imagens em apps Windows: alerta que decode maior que o tamanho exibido desperdicca memoria, ate 4 bytes por pixel.
  <https://learn.microsoft.com/en-us/windows/uwp/debug-test-perf/optimize-animations-and-media>
- Microsoft Learn, `BitmapTransform`: `ScaledWidth`/`ScaledHeight` sao definidos no espaco da imagem fonte, antes de rotacao/flip; com EXIF, deve-se considerar `OrientedPixelWidth`/`OrientedPixelHeight`.
  <https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmaptransform>
- Microsoft Learn, `BitmapFrame.GetPixelDataAsync`: ao usar `RespectExifOrientation`, as dimensoes corretas do resultado devem considerar `OrientedPixelWidth` e `OrientedPixelHeight`.
  <https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmapframe.getpixeldataasync>
- Microsoft Learn, `FullScreenPresenter`: fullscreen nao e maximizado; a janela ocupa a tela inteira e elementos como barra de titulo/taskbar ficam ocultos por padrao.
  <https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.fullscreenpresenter>
- Microsoft Learn, AppWindow/windowing: `FullScreenPresenter` configura uma janela sem borda/title bar e esconde a taskbar para experiencia fullscreen.
  <https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/windowing/windowing-overview>

## Stack atual

- `.NET SDK 10.0.203`
- Target principal: `net10.0-windows10.0.19041.0`
- Windows App SDK NuGet: `2.0.1`
- WinUI 3 para UI nativa.
- Windows Imaging Component via `Windows.Graphics.Imaging` para JPEG.
- `SoftwareBitmapSource` para exibir pixels decodificados no WinUI.
- `AppWindow` + `FullScreenPresenter` para fullscreen.
- `Microsoft.Data.Sqlite` planejado para estado derivado.
- JSONL ja usado para journal append-only de delete/restore.
- Cache em memoria LRU ja criado para previews e actual-size sob demanda.
- Prefetch leve ja criado para proximas 3 fotos e anteriores 2.
- BenchmarkDotNet ja existe no projeto.

## Arquitetura atual

Camadas existentes:

- `App`: WinUI, janela, atalhos, fullscreen, overlay, conversao de pixels para `ImageSource`.
- `Application`: use cases, abertura de sessao, navegacao, delete, undo, replay.
- `Core`: dominio puro, `PhotoSession`, `PhotoItem`, navegacao, delete/undo state machine.
- `Imaging`: decode JPEG, EXIF, sizing, cache em memoria, prefetch.
- `Storage`: filesystem, move/restore, journal JSONL.
- `Infrastructure`: DI e composicao tecnica.
- `Launcher`: fallback de ativacao por argumento/menu dev.

Principio atual correto: `Core` nao depende de WinUI, WIC, storage, shell, HTTP ou logs. O viewer ainda tem bastante logica em `MainPage.xaml.cs`; isso e aceitavel para o prototipo funcional, mas deve ser reduzido quando entrarmos em UX/performance mais fina.

## O que ja esta pronto

### Abertura e sessao

- Abre pasta por `--folder`.
- Lista apenas `.jpg`/`.jpeg`.
- Ignora `_deletadas_evydencia`.
- Ordena por nome/sort index.
- Scanner usa `Directory.EnumerateFiles`.
- Sessao, contadores e estado ficam no dominio.

### Viewer

- Mostra primeira foto no WinUI.
- Navega com `Right`, `Left`, `Space`, `Home`, `End`.
- `KeyboardAccelerator` evita depender do foco no `ViewerHost`.
- Tooltip fixo de acelerador foi removido.
- Overlay temporizado aparece apos atividade e some sozinho.

### Fullscreen

- `F` alterna fullscreen.
- `Esc` sai do fullscreen.
- Implementacao usa `FullScreenPresenter.Create()`.
- O decode recaptura `DisplayContext` quando muda fullscreen.

### Zoom e pan

- Roda do mouse faz zoom in/out.
- `+` e `-` fazem zoom.
- `0` volta para fit.
- Duplo clique volta para fit.
- Pan/arrastar funciona quando esta ampliado.
- `1` faz decode full-res sob demanda e calcula zoom 100% real em pixels fisicos.

### Decode e qualidade

- Decode fit normal usa tamanho calculado pelo `DecodeTargetCalculator`.
- O target considera area util, DPI/rasterization scale e EXIF orientation.
- Fit normal nao decodifica full-res.
- Full-res so acontece no atalho `1`.
- `GetPixelDataAsync` usa `ExifOrientationMode.RespectExifOrientation`.
- Streams sao curtos e abertos com compartilhamento compativel com move/delete.
- Testes cobrem EXIF orientation 6/8 e file handle.

### Delete/undo

- `Delete` esta ligado ao viewer.
- `Ctrl+Z` esta ligado ao viewer.
- Delete move para `_deletadas_evydencia`.
- Undo restaura.
- Estados `PendingDelete`, `Deleted`, `PendingRestore`, `Restored`, `Missing`, `DeleteFailed` existem.
- Journal JSONL registra delete/restore/falha.
- Replay basico/reconciliacao inicial existem.

### Cache/prefetch

- `MemoryImageCache` LRU guarda pixels decodificados.
- Cache key inclui path normalizado, tamanho, `LastWriteTimeUtc`, modo, target e versao de algoritmo.
- `PreviewCacheService` reutiliza preview fit e actual-size sob demanda.
- `PrefetchScheduler` decodifica proximas 3 e anteriores 2 em background.
- Prefetch nao faz full-res.

### Testes

Cobertura atual:

- `Core.Tests`: 33
- `Application.Tests`: 36
- `Imaging.Tests`: 25
- `Storage.Tests`: 16
- `IntegrationTests`: 7
- `Launcher.Tests`: 7
- `UiSmokeTests`: 19
- Total: 143 testes

## Gap principal contra o Fotos do Windows

O comportamento observado no Fotos do Windows para uma foto de `6000x4000px`, cerca de `6 MB`, e:

- fit na tela por volta de `24%`;
- navegacao entre imagens praticamente instantanea;
- zoom progressivo sem perder nitidez perceptivel;
- sem piscada preta perceptivel entre fotos;
- fullscreen 100%, com barra superior e taskbar ocultas.

Hoje nosso app ja tem a base, mas ainda nao deve ser considerado equivalente ao Fotos do Windows por estes motivos.

## Gaps tecnicos e melhorias necessarias

### 1. Zoom ainda nao tem re-decode progressivo automatico

Hoje:

- fit normal decodifica uma versao adequada para a tela com margem de qualidade;
- se o usuario da zoom com roda/`+`, o app escala o preview atual;
- se o usuario aperta `1`, o app troca para full-res 100%.

Problema:

- no Fotos do Windows, o usuario nao precisa apertar `1` para manter nitidez ao aproximar. A experiencia esperada e que o viewer va refinando a imagem conforme o zoom aumenta.
- escalar um preview fit alem do seu tamanho decodificado pode gerar suavidade/borrado. Isso e perceptivel em pele, cabelo, roupa, olhos, texto e detalhes finos.

Melhoria recomendada:

- implementar uma "zoom quality ladder":
  - fit preview: usado para abertura rapida;
  - high-quality tier 1: quando zoom passa de 125%-150% do preview;
  - high-quality tier 2: quando zoom passa de 200%-300%;
  - actual-size: quando chega perto de 100% real;
  - nunca decodificar full-res no fit normal.
- enquanto o re-decode acontece, manter a imagem atual na tela.
- trocar para a versao mais nitida de forma atomica quando o decode novo estiver pronto.
- usar hysteresis para nao ficar redecodificando a cada tick do mouse wheel.

### 2. Navegacao ainda pode mostrar tela preta em uncached load

Hoje:

- `LoadCurrentPhotoAsync` limpa `CurrentPhotoImage.Source = null` antes de decodificar a proxima imagem.
- se a proxima imagem ainda nao estiver em cache, aparece fundo preto/carregamento.

Problema:

- o Fotos do Windows praticamente nao deixa o usuario sentir esse vazio.
- em fluxo de escolha por exclusao, a navegacao deve parecer continua.

Melhoria recomendada:

- trocar para "atomic image swap":
  - manter a imagem atual visivel enquanto a proxima esta decodificando;
  - se a proxima ja esta em cache, trocar imediatamente;
  - se nao esta, mostrar status discreto, mas nao limpar a tela antes da nova imagem estar pronta;
  - aplicar um timeout curto para casos de erro, ai sim mostrar falha discreta.
- nao usar fade longo. O fluxo de selecao precisa ser seco e rapido.

### 3. Cache atual guarda pixels, mas nao prepara `ImageSource`

Hoje:

- cache em memoria guarda `ImageDecodeResult` com bytes BGRA.
- a cada exibicao, o app cria `SoftwareBitmapSource` a partir desses bytes.

Problema:

- isso evita re-decode, mas ainda existe custo na UI thread para criar a fonte visual.
- para navegacao "instantanea", principalmente segurando seta, precisamos reduzir tambem o custo de preparar a fonte.

Melhoria recomendada:

- criar um `ViewerImageSourceCache` no App, limitado e thread-safe no contexto da UI:
  - cachear `SoftwareBitmapSource` apenas para current/next/previous;
  - invalidar quando DisplayContext/target muda;
  - nao colocar esse cache em `Imaging`, porque `ImageSource` e WinUI/App.
- alternativa: precriar `SoftwareBitmapSource` das proximas imagens na UI dispatcher quando o app estiver ocioso.

### 4. Prefetch ainda e basico

Hoje:

- prefetch cancela fila antiga e busca proximas 3/anteriores 2.
- ha um gate de decode sequencial.

Problema:

- nao ha prioridade explicita por direcao de navegacao.
- nao ha telemetria de hit/miss.
- nao ha prefetch adaptativo se o usuario esta segurando seta.
- nao ha limite separado para actual-size vs preview.

Melhoria recomendada:

- `PrefetchScheduler` deve receber direcao da ultima navegacao.
- prioridade:
  - foto atual sempre top priority;
  - se navegou para direita, proximas primeiro;
  - se navegou para esquerda, anteriores primeiro.
- medir:
  - cache hit;
  - cache miss;
  - decode time;
  - image source creation time;
  - time to visible.
- nao fazer thumbnail/filmstrip no modo cliente fullscreen.

### 5. Fullscreen precisa de validacao visual real

Hoje:

- `FullscreenService` usa `FullScreenPresenter`.
- pela doc oficial, esse presenter deve ocultar barra de titulo e taskbar por padrao.

Problema:

- precisamos validar na maquina real se a barra superior some sempre:
  - unpackaged dev;
  - MSIX instalado;
  - monitor unico;
  - segundo monitor futuramente;
  - ao abrir via Explorer.

Melhoria recomendada:

- criar smoke manual/automatizado:
  - abrir app;
  - pressionar `F`;
  - validar `AppWindow.Presenter.Kind == FullScreen`;
  - capturar screenshot ou validar visualmente que nao ha titlebar/taskbar;
  - pressionar `Esc` e validar retorno.
- adicionar setting futura: abrir em fullscreen automaticamente quando vier de `--folder`.
- se ainda aparecer barra superior, investigar:
  - se a janela esta empacotada/unpackaged;
  - se existe custom titlebar;
  - se algum overlay do app esta ocupando topo;
  - se o presenter realmente entrou em `FullScreen`.

### 6. Qualidade de escala ainda depende de WinUI transform

Hoje:

- fit preview usa WIC com `BitmapInterpolationMode.Fant`.
- zoom visual usa `CompositeTransform` sobre `Image`.

Problema:

- quando o zoom amplia alem da resolucao do preview, a escala e feita pelo render do WinUI sobre o bitmap ja reduzido.
- isso nao e igual a um pipeline premium que redecodifica/renderiza no tamanho correto ou usa GPU com filtro de alta qualidade.

Melhoria recomendada:

- curto prazo: re-decode progressivo por zoom tier.
- medio prazo: spike de Win2D/Direct2D para:
  - render com `CanvasBitmap`;
  - controle de interpolacao;
  - pan/zoom com GPU;
  - possivel tile/pyramid no futuro.
- manter Win2D atras de abstracao para nao travar V1.

### 7. Configuracoes ainda nao existem

Falta implementar:

- modo de delete:
  - `_deletadas_evydencia`;
  - Lixeira;
  - permanente protegido, futuro.
- cache:
  - limite de memoria;
  - prefetch next/previous;
  - limpar cache;
  - ativar/desativar cache em disco futuro.
- qualidade:
  - rapido;
  - nitido;
  - cor precisa, futuro.
- fullscreen:
  - abrir em fullscreen automaticamente;
  - overlay timeout.
- zoom:
  - zoom wheel step;
  - re-decode automatico ligado/desligado;
  - manter 100% entre fotos, talvez futuro.

### 8. Cache em disco ainda nao existe

Hoje:

- cache e apenas memoria.

Melhoria recomendada:

- para V1.1/V1.5:
  - cache de thumbnails em disco;
  - cache de previews sem recompressao perceptivel;
  - nunca usar preview recomprimido para zoom 100%;
  - nao varrer cache inteiro no startup;
  - limpar por idade/tamanho em background.

Observacao:

- para qualidade de fullscreen, evitar JPEG 88-92. Se persistir preview, usar qualidade 95+ ou formato alternativo documentado. Para zoom real, sempre usar original/full-res ou tier redecodificado do original.

### 9. Color management ainda esta em modo rapido

Hoje:

- `ColorManagementMode.DoNotColorManage`.
- assumimos sRGB para performance.

Melhoria recomendada:

- V1: manter rapido, mas detectar ICC quando barato e logar.
- V1.5: setting `Cor precisa`, usando ICC/perfil de monitor quando necessario.
- documentar impacto de performance.

### 10. MSIX e Explorer moderno ainda pendentes

Pronto:

- launcher minimo.
- scripts HKCU de menu classico/dev.
- MSIX dev assinado e scripts de prereq/instalacao.

Pendente:

- validar smoke MSIX empacotado com duas ativacoes reais;
- resolver launch por alias/app activation no pacote instalado;
- criar `ShellExtension` moderno com `IExplorerCommand`;
- manter extensao minima: receber path, chamar launcher/app, sair;
- tratar clique em pasta e fundo da pasta.

## Comportamento alvo para foto 6000x4000

Exemplo: JPEG `6000x4000`, cerca de `6 MB`.

Em monitor 1920x1080:

- fit contain aproximado: algo perto de `1620x1080`.
- zoom visual reportado pode ficar por volta de `24%` a `27%`, dependendo da area util.
- preview fit deve decodificar algo como 1.15x-1.35x do fit, sem full-res.
- ao zoomar acima do preview, o viewer deve disparar re-decode em background.
- ao chegar em `100%`, deve usar full-res orientado.
- se o decode full-res gera `6000x4000`, memoria BGRA aproximada: `6000 * 4000 * 4 = 96 MB` por imagem.

Regra importante:

- nao e aceitavel decodificar full-res de todas as proximas fotos, porque 5 imagens full-res podem passar de `480 MB` so em pixels, sem contar overhead. Full-res deve ser atual/sob demanda, com LRU agressivo.

## Proximas issues recomendadas

Nota: a lista abaixo descreve os gaps. A ordem operacional atualizada para implementacao esta em `docs/viewer-implementation-backlog.md`. A sequencia aprovada e:

1. VQ-01 - Atomic image swap.
2. VQ-04 - Telemetria de time-to-visible.
3. VQ-03 - Cache de `ImageSource` no App.
4. VQ-02 - Zoom quality ladder.
5. VQ-05 - Prefetch direcional/adaptativo e smoke de navegacao rapida.
6. VQ-06 - Fullscreen real.
7. VQ-07 - Settings base.
8. VQ-09 - Spike Win2D/Direct2D.
9. VQ-08 - MSIX/ShellExtension moderno.

### VQ-01 - Remover tela preta na navegacao

Objetivo:

- nao limpar `CurrentPhotoImage.Source` antes da proxima imagem estar pronta.

Criterio:

- ao navegar para imagem uncached, a tela nao fica preta;
- se houver delay, imagem anterior permanece ate troca atomica;
- erro mostra overlay discreto.

### VQ-02 - Re-decode automatico por zoom tier

Objetivo:

- ao aumentar zoom, o viewer troca para decode mais nitido automaticamente.

Criterio:

- wheel zoom em foto `6000x4000` nao fica visivelmente borrado;
- `1` continua forcando 100% real;
- full-res nao e decodificado no fit normal.

### VQ-03 - Cache de `ImageSource` no App

Objetivo:

- reduzir custo de criar `SoftwareBitmapSource` a cada navegacao cached.

Criterio:

- current/next/previous podem ter fonte visual preparada;
- cache e invalidado ao mudar target/display;
- nao atravessa camada `Imaging`.

### VQ-04 - Telemetria de performance do viewer

Objetivo:

- medir tempo real de navegacao e decode.

Criterio:

- logar `navigation_requested`, `cache_hit`, `decode_started`, `decode_completed`, `source_created`, `image_visible`;
- medir tempo ate imagem visivel;
- separar cached vs uncached.

### VQ-05 - Smoke automatizado de navegacao rapida

Objetivo:

- validar segurar seta direita/esquerda em pasta real ou fixture grande.

Criterio:

- sem crash;
- sem fila antiga acumulada;
- sem memoria explodindo;
- tempo medio cached dentro do budget.

### VQ-06 - Validar fullscreen real

Objetivo:

- confirmar visualmente e por API que fullscreen oculta barra superior/taskbar.

Criterio:

- `Presenter.Kind == FullScreen`;
- screenshot/manual smoke sem titlebar/taskbar;
- `Esc` retorna;
- abertura por `--folder` pode entrar em fullscreen automatico quando setting existir.

### VQ-07 - Criar settings base

Objetivo:

- criar `AppSettingsService`/store local.

Criterio:

- delete mode;
- cache memory limit;
- prefetch counts;
- open fullscreen by default;
- quality mode.

### VQ-08 - Validar MSIX empacotado e ShellExtension moderno

Objetivo:

- fechar fluxo real do Explorer moderno.

Criterio:

- pacote instalado e confiavel;
- single-instance validado no pacote;
- `Abrir Escolher Fotos` aparece no menu moderno via `IExplorerCommand`;
- launcher recebe path e ativa app.

### VQ-09 - Spike Win2D/Direct2D

Objetivo:

- avaliar se Win2D melhora escala/pan/zoom em relacao ao `Image` + `CompositeTransform`.

Criterio:

- prototipo isolado;
- comparar nitidez e frame pacing;
- nao substituir pipeline atual sem benchmark/smoke visual.

## Criterios de aceite para o viewer ficar "nivel Fotos do Windows"

Para uma pasta real com JPEGs de estudio, incluindo fotos `6000x4000`:

- primeira imagem visivel rapidamente, alvo inicial <= 1.5s em SSD comum;
- navegacao cached parece instantanea, alvo perceptivo <= 50ms;
- navegacao uncached nao mostra tela preta;
- segurar seta direita nao acumula fila antiga;
- zoom por roda mantem nitidez progressiva sem exigir que o usuario aperte `1`;
- `1` mostra 100% real;
- `0` volta para fit rapidamente;
- fullscreen esconde barra superior e taskbar;
- `Delete` some imediatamente e `Ctrl+Z` restaura;
- nenhum decode prende file handle;
- memoria fica dentro do limite configurado.

## Decisao tecnica recomendada

Nao tentar resolver tudo com full-res sempre. Isso parece simples, mas quebra performance e memoria em pastas de 500-2.000 JPEGs.

A estrategia correta para V1 robusta e:

1. manter preview fit rapido;
2. manter cache/prefetch de previews;
3. nao limpar a imagem antes da proxima estar pronta;
4. implementar re-decode automatico por zoom tier;
5. usar full-res apenas quando o zoom realmente exige;
6. medir tudo com logs e smoke real.

Essa abordagem preserva o objetivo do produto: abrir rapido, navegar rapido, deletar rapido e manter qualidade visual alta sem transformar a V1 em um aplicativo pesado.
