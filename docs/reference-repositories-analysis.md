# Analise de repositorios de referencia para o viewer

Data da analise: 2026-05-03.

Esta analise usa os repositorios baixados em `C:\Users\Usuario\Desktop\ideias` apenas como referencia. O objetivo nao e pegar um viewer pronto, nem copiar codigo. O objetivo e validar o pipeline certo para a proxima fatia:

```text
folder -> PhotoSession -> CurrentPhoto -> DisplayContext -> DecodeTarget -> WIC decode -> ImageSource seguro -> UI
```

## Resumo executivo

O nosso repositorio ja tem uma parte importante da base:

- `FileSystemFolderScanner` ja usa enumeracao progressiva, JPEG-only e ignora `_deletadas_evydencia`.
- `OpenSessionUseCase` ja abre uma `PhotoSession` e retorna `CurrentPhoto`.
- `PhotoSession`, `PhotoItem` e `NavigationController` ja modelam a lista ativa e a navegacao.
- `DisplayContextSnapshot` ja representa area util, escala de rasterizacao/DPI e fullscreen sem depender de WinUI.
- `DecodeTargetCalculator` ja calcula fit-contain com EXIF orientation e margem de qualidade.

O que ainda falta para F2-03:

- leitor de metadados barato para a foto atual quando `Width`, `Height` e `ExifOrientation` ainda nao existem;
- decoder JPEG WIC com target size calculado, stream curto e `FileShare.ReadWrite | FileShare.Delete`;
- conversao segura para uma fonte exibivel no WinUI sem manter file handle preso;
- estado no ViewModel para imagem atual, carregamento, erro discreto e token/versao de decode;
- superficie de viewer com fundo preto/cinza escuro, sem filmstrip e sem controles permanentes;
- cancelamento/descarte de decode obsoleto quando a foto atual muda.

Decisao recomendada para agora: implementar F2-03 com WinUI + WIC e `Image`/`SoftwareBitmapSource` seguro. Win2D deve continuar fora da primeira fatia visual, atras de abstracao futura, porque o projeto ja tem `DecodeTargetCalculator` e a prioridade e provar o fluxo correto de arquivo, DPI, orientation e handle liberado.

## Disponibilidade das referencias

Repositorios locais encontrados:

- `C:\Users\Usuario\Desktop\ideias\WindowsAppSDK-Samples-main`
- `C:\Users\Usuario\Desktop\ideias\FlyPhotos-main`
- `C:\Users\Usuario\Desktop\ideias\Win2D-winappsdk-main`
- `C:\Users\Usuario\Desktop\ideias\jpegview-master`
- `C:\Users\Usuario\Desktop\ideias\qimgv-master`
- `C:\Users\Usuario\Desktop\ideias\QuickRawPicker-main`
- `C:\Users\Usuario\Desktop\ideias\oculante-master`
- `C:\Users\Usuario\Desktop\ideias\ImageGlass-develop`

Referencias nao encontradas localmente:

- `microsoft/Windows-classic-samples / WicViewerD2D`
- `getcull.fyi / cull`

Para estas duas, a analise ficou em nivel de documentacao/site oficial:

- WicViewerD2D: `https://learn.microsoft.com/en-us/windows/win32/wic/-wic-sample-d2d-viewer` e `https://github.com/microsoft/Windows-classic-samples/tree/master/Samples/Win7Samples/multimedia/wic/wicviewerd2d`
- cull: `https://www.getcull.fyi/` e `https://github.com/jshph/cull`

## Referencias para F2-03 - primeira foto no viewer

### 1. WindowsAppSDK-Samples

Licenca: MIT.

O que vale trazer como ideia:

- Single-instance oficial via `AppInstance`, `GetCurrent`, `FindOrRegisterForKey`, evento `Activated` e `RedirectActivationToAsync`.
- Redirecionamento de ativacao antes de criar janela principal, usando `DISABLE_XAML_GENERATED_MAIN` e um `Main` customizado.
- Fullscreen oficial via `AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen)` e leitura de `Presenter.Kind`.
- Interop de janela/monitor quando necessario: `WindowNative.GetWindowHandle`, `Win32Interop.GetWindowIdFromWindow`, `AppWindow.GetFromWindowId`.
- Uso de `DispatcherQueue` para devolver eventos da instancia primaria para a UI sem bloquear startup.

O que nao serve agora:

- Nao precisamos copiar a estrutura de samples.
- Nao precisamos resolver menu de contexto nesta fatia.
- Nao devemos misturar lifecycle/single-instance com decode F2-03 alem de manter o desenho compativel.

Impacto no nosso repositorio:

- Futuro `AppActivationService` e `Program.cs` customizado.
- `App.xaml.cs` deve mudar antes do menu de contexto, porque hoje a janela e criada no fluxo normal do WinUI antes do single-instance real.
- Para F2-03, o impacto e indireto: a superficie do viewer deve ser preparada para receber ativacao por pasta sem assumir multiplas janelas.

Recomendacao:

- Usar WindowsAppSDK-Samples como referencia canonica para single-instance/fullscreen.
- Nao implementar single-instance dentro de F2-03 se o objetivo for somente mostrar primeira foto; registrar como dependencia para contexto do Explorer.

### 2. FlyPhotos

Licenca: GPL-3.0 para source code, com restricoes adicionais para binarios e marca. Nao copiar codigo.

O que vale trazer como ideia:

- Produto focado em viewer rapido, nao galeria pesada.
- A primeira foto tem caminho privilegiado: carregar preview primeiro, mostrar imediatamente e depois melhorar qualidade.
- A listagem/caching roda em paralelo ao primeiro display, em vez de bloquear a primeira imagem.
- Cache prioriza foto atual, vizinhas e descarte fora da janela de interesse.
- Durante navegacao segurando seta, o viewer evita gastar trabalho pesado em HQ; no `KeyUp`, faz "brake" e melhora qualidade.
- Overlay, cursor e controles aparecem por atividade e somem automaticamente.
- Fullscreen/window state e tratado como comportamento da janela, nao como regra de dominio.
- `DispatcherQueue.TryEnqueue` aparece como ponte clara para atualizacao de UI.
- WIC/WinRT decoder usa transform com target size e `ExifOrientationMode.RespectExifOrientation`.

O que nao serve agora:

- Win2D-first no viewer.
- RAW, HEIF, PNG, multipage, video e dependencias amplas.
- Thumbnail strip fixa e muitos botoes de viewer.
- Delecao pela lixeira/recycle bin como fluxo principal.
- Logica de arquivo e source of truth dentro de controller de UI.
- `Directory.GetFiles`/descoberta de Explorer como base da nossa V1.

Impacto no nosso repositorio:

- `PrepareViewerImageUseCase`: deve existir como caminho dedicado para foto atual.
- `JpegDecodeService`: deve ter decode preview dimensionado, cancellation/discard de obsoleto e stream curto.
- `PreviewCacheService`: futuro cache deve priorizar atual, proximas 3 e anteriores 2.
- `MainPageViewModel`: deve manter `CurrentPhoto`, estado de carregamento/erro e identificador de operacao.
- `MainPage.xaml`: deve virar viewer de foto, nao tela de status.

Recomendacao:

- Copiar a estrategia, nao a implementacao: primeira foto primeiro, qualidade dimensionada, upgrade futuro, vizinhos em cache e overlay discreto.
- Para F2-03, nao implementar ainda cache amplo nem Win2D; deixar a API pronta para prefetch depois.

### 3. Windows-classic-samples / WicViewerD2D

Licenca: referencia oficial Microsoft; se algum codigo for copiado no futuro, verificar cabecalho/licenca do arquivo especifico. Nesta analise, uso apenas conceitual.

O que vale trazer como ideia:

- Separacao clara: WIC decodifica/converte, Direct2D renderiza.
- Pipeline base: `IWICImagingFactory` -> `IWICBitmapDecoder` -> frame -> scaler/converter -> bitmap renderizavel.
- Conversao de pixel format antes do render, tipicamente para formato compativel com Direct2D.
- Render nao deve depender do arquivo original continuar aberto.

O que nao serve agora:

- Codigo C++ antigo Win32/Direct2D direto.
- Nenhuma regra de UI WinUI, DI, camada Application/Core ou cancellation moderna.
- Nao resolve `DisplayContextSnapshot` nem `DecodeTargetCalculator`.

Impacto no nosso repositorio:

- `JpegDecodeService` em `Imaging` deve ser explicitamente separado do presenter WinUI.
- `App` deve receber um objeto ja decodificado/copiado e criar `ImageSource` seguro.
- `Core` continua fora de WIC/WinUI.

Recomendacao:

- Usar WicViewerD2D para validar a arquitetura mental WIC -> pixels renderizaveis.
- Implementar em C# com Windows Imaging APIs/WinRT ou WIC wrapper controlado, mantendo stream fechado apos decode.

### 4. Win2D

Licenca: MIT.

O que vale trazer como ideia:

- Renderer imediato e GPU-friendly para zoom/pan futuro.
- `CanvasControl` tem `CreateResources`, `Draw`, `Invalidate` e ciclo de recursos bem definido.
- A documentacao reforca a regra DPI: pixels = DIPs * DPI / 96.
- Recursos dependentes de DPI precisam ser recriados quando o DPI muda.
- `Invalidate` e coalescido, ideia util para evitar redesenho excessivo.

O que nao serve agora:

- `CanvasBitmap.LoadAsync` pode esconder full-res decode e nao encaixa sozinho na regra de target decode.
- Win2D nao deve entrar como dependencia obrigatoria da primeira foto.
- A versao local cita Win2D-WinUI 1.4.0 com Windows App SDK 1.8, enquanto nosso repo usa Windows App SDK 2.0.1. Isso exige validacao/ADR antes de adotar.

Impacto no nosso repositorio:

- Futuro `IViewerRenderer` ou `IImagePresenter` atras de abstracao.
- `DisplayContextSnapshot` e `DecodeTargetCalculator` devem continuar sendo a fonte de tamanho, mesmo se Win2D entrar.

Recomendacao:

- Nao adicionar Win2D em F2-03.
- Criar a primeira fatia com `ImageSource` seguro; quando zoom/pan/cache visual exigirem, fazer prototipo/ADR de Win2D.

## Referencias para fullscreen e UX cliente

### FlyPhotos

Ideias uteis:

- Viewer abre direto na foto, com controles secundarios.
- Overlay e cursor aparecem por interacao e somem por inatividade.
- Fullscreen deve preservar restauracao da janela e evitar flicker.
- Durante navegacao rapida, o viewer favorece responsividade em vez de qualidade maxima imediata.

Nao trazer:

- Thumbnail strip fixa.
- Grid/album/painel lateral no modo cliente.
- Controles demais no primeiro viewport.

### JPEGView

Licenca: GPL-2.0-or-later. Nao copiar codigo.

Ideias uteis:

- Filosofia de UI minima e viewer muito rapido.
- Fullscreen pode ser automatico ou configurado, mas para nosso V1 o comportamento deve favorecer fullscreen limpo.
- Painel inferior/nav aparece sob demanda.
- Atalhos de navegacao sao centrais.
- Configuracoes mostram a importancia de evitar confirmacoes e janelas no fluxo rapido, mas com seguranca quando ha risco de perda.
- `MoveToNext` em comandos de arquivo confirma que, apos uma acao de rejeicao/delete, navegar adiante e comportamento esperado.

Nao trazer:

- Editor embutido, filtros, slideshow, processamento AVX/SSE, RAW e formatos multiplos.
- Configurabilidade extrema por INI.
- Delecao permanente por comando como parte do fluxo V1.

Impacto no nosso repositorio:

- `MainPage.xaml` deve ficar visualmente mais perto de um viewer fullscreen do que de uma tela administrativa.
- Atalhos devem ser implementados no `App`/UI e roteados para use cases, sem colocar regra no ViewModel pesado.
- Overlay deve conter no maximo contador, nome da foto, estado de delete/undo e mensagens discretas.

### qimgv

Licenca: GPL-3.0. Nao copiar codigo.

Ideias uteis:

- Atalhos padrao similares ao nosso plano: Right/Left, Home/End, F/F11, Esc, Delete, Shift+Delete.
- Separacao conceitual entre `ActionManager`, UI e operacoes de arquivo.
- Depois de remover a foto atual, tenta carregar a foto no mesmo indice; se nao houver, volta para anterior.
- Operacoes de arquivo retornam resultado especifico, nao apenas `bool`.
- Move/copy trata colisao e rollback.

Nao trazer:

- Qt/C++.
- Folder view, video, editor, scripts, quick copy/move como features V1.
- `Shift+Delete` permanente.
- Copy-then-delete como implementacao padrao de move se a API .NET/Windows resolver melhor.

Impacto no nosso repositorio:

- `DeleteManager`/`FileMoveService` devem retornar resultado rico para `DeleteFailed`.
- `NavigationController.HideCurrent()` e fluxo de delete devem escolher proxima foto antes de mover fisicamente.
- UI deve mostrar erro discreto quando a operacao falhar.

### ImageGlass

Licenca: o README local mostra badge GPLv3 e os arquivos de codigo tem cabecalho GPLv3; o `LICENSE` local tambem contem EULA do produto. Tratar como alto risco para copia. Usar apenas como referencia conceitual.

Ideias uteis:

- `ImageBooster` pre-carrega janela de vizinhos em ordem atual -> direita -> esquerda.
- Cache tem limite por quantidade/tamanho e libera itens fora da janela.
- Opcoes de leitura separam tamanho solicitado, orientation, color profile e first-frame.
- Disco cache usa chave hash e purge por tamanho, reduzindo para 50% para evitar purge constante.
- Tem servico proprio de single-instance por mutex/pipe, mas para nosso caso a referencia oficial deve ser Windows App SDK.

Nao trazer:

- Galeria rica, suporte a 90 formatos, WebView2 opcional, editor e Store/EULA.
- Dependencias amplas como Magick.NET/MagicScaler/WicNet na primeira fatia.
- Cache que varre todo diretorio no caminho critico de startup.

Impacto no nosso repositorio:

- Futuro `PreviewCacheService` deve ter janela de prefetch e limite dinamico.
- `CacheKey` deve incluir path normalizado, tamanho, last write UTC, orientation, target size, versao do algoritmo e qualidade/cor.
- F2-03 pode criar apenas contrato minimo de cache ou ficar sem cache, desde que nao bloqueie F2-04/F2-05.

## Referencias para Delete/Undo e culling

### QuickRawPicker

Licenca: LGPL-2.1. Nao copiar codigo.

Ideias uteis:

- Culling e orientado por teclado e por estado visual simples.
- Marca e rating sao discretos e visiveis sem poluir a area da foto.
- Workflow separa marcar/agrupar/exportar do momento de revisao.
- Atalhos de avaliacao (`1..5`) e mark ajudam a entender o dominio de culling, mesmo que nao entrem na V1.

Nao trazer:

- RAW, LibRaw, Godot, rating XMP/PP3 como fonte de verdade.
- Comparacao multipla de ate 100 fotos.
- Sidecar como mecanismo V1 de delete/undo.

Impacto no nosso repositorio:

- Nenhum impacto direto em F2-03.
- Para Delete/Undo, reforca que o estado visual precisa ser claro, mas nossa fonte da verdade continua filesystem + journal JSONL.

### qimgv

Ideias uteis para Delete/Undo:

- `Delete` como mover para lixeira e `Shift+Delete` como permanente sao convencoes conhecidas; para nosso V1, usar `Delete` para mover para `_deletadas_evydencia` e nao oferecer permanente por padrao.
- Resultado rico de operacao de arquivo ajuda a manter contadores consistentes.
- Apos remocao da foto atual, navegar para a proxima sem exigir acao extra.

Nao trazer:

- Lixeira como source of truth.
- Remocao permanente.
- Dialogos intrusivos no fluxo cliente.

### cull

Licenca: MIT no GitHub oficial. Nao estava local; analise em nivel README/site.

Ideias uteis:

- Produto extremamente focado: abrir pasta, revisar, marcar pick/reject, exportar.
- Sem catalogo, sem import, sem cloud e sem runtime pesado.
- Atalhos simples: `Left`/`Right`, `P`, `X`, `U`.
- Reject/pick e escrito imediatamente, sem esperar fase posterior.
- Exporta picks para subpasta, ideia util futura para resumo local.

Nao trazer:

- RAW embedded preview como estrategia principal, porque nossa V1 e JPEG-only.
- XMP sidecar como fonte de verdade.
- Pick/reject por label no lugar do delete por exclusao planejado.

Impacto no nosso repositorio:

- Futuro `BuildLocalSelectionSummaryUseCase` pode se inspirar na clareza de "picks/export", mas V1 nao deve escrever XMP.
- Delete/Undo deve continuar com `_deletadas_evydencia`, journal JSONL e `Ctrl+Z`.

### Oculante

Licenca: MIT.

Ideias uteis:

- "Zen mode" e bom equivalente conceitual ao nosso fullscreen limpo.
- Atalhos configuraveis, mas com early-out quando o teclado esta capturado por input.
- Player cancela load anterior antes de carregar nova imagem.
- Cache simples remove item mais antigo quando passa do limite.
- Delete remove a foto da lista/cache e limpa a imagem atual, o que reforca que a UI deve reagir imediatamente.

Nao trazer:

- Rust/notan/egui, editor, canais de cor, analise de pixel e formatos multiplos.
- Delete direto via trash como regra V1.
- Cache baseado em full decoded image sem target size.

Impacto no nosso repositorio:

- Implementar atalhos no App com escopo: nao disparar quando foco estiver em input de texto.
- O decode deve ter cancelamento/descarte de obsoleto antes de atualizar `ImageSource`.
- `PreviewCacheService` deve usar target size e limite de memoria, nao guardar imagens full-res do fit normal.

## Mapa de licencas e risco

| Referencia | Licenca observada | Risco para copia | Uso recomendado |
| --- | --- | --- | --- |
| WindowsAppSDK-Samples | MIT | Baixo | Pode usar como referencia oficial; copiar trechos pequenos ainda deve manter atribuicao se necessario. |
| Windows-classic-samples/WicViewerD2D | Oficial Microsoft/sample | Baixo a medio | Usar conceitualmente; verificar cabecalho antes de qualquer copia. |
| Win2D | MIT | Baixo | Pode virar dependencia futura apos ADR/prototipo de compatibilidade. |
| FlyPhotos | GPL-3.0 + restricoes de binario/marca | Alto | Ideias apenas; nao copiar codigo. |
| JPEGView | GPL-2.0-or-later | Alto | Ideias apenas; nao copiar codigo. |
| qimgv | GPL-3.0 | Alto | Ideias apenas; nao copiar codigo. |
| ImageGlass | GPLv3 nos arquivos + EULA local | Alto | Ideias apenas; nao copiar codigo. |
| QuickRawPicker | LGPL-2.1 | Medio | Ideias apenas; evitar incorporar codigo. |
| Oculante | MIT | Baixo | Ideias podem ser aproveitadas; stack nao entra. |
| cull | MIT no GitHub oficial | Baixo | Ideias podem ser aproveitadas; fonte local nao estava baixada. |

## O que realmente podemos trazer que ainda nao temos

### Pipeline first-photo dedicado

Ainda nao temos um caminho de "foto atual primeiro". O que trazer:

- `PrepareViewerImageUseCase` recebe `PhotoItem` atual e `DisplayContextSnapshot`.
- Se o `PhotoItem` ainda nao tem dimensoes/orientation, ler apenas os metadados da foto atual.
- Calcular `DecodeTarget` com `DecodeTargetCalculator`.
- Decodificar apenas no target size, nunca full-res no fit normal.
- Retornar resultado neutro suficiente para o App criar/exibir a imagem.

Classes provaveis:

- `src/Evydencia.PhotoSelector.Application/Viewer/PrepareViewerImageUseCase.cs`
- `src/Evydencia.PhotoSelector.Application/Viewer/PreparedViewerImage.cs`
- `src/Evydencia.PhotoSelector.Imaging/Jpeg/JpegMetadataReader.cs`
- `src/Evydencia.PhotoSelector.Imaging/Jpeg/JpegDecodeService.cs`
- `src/Evydencia.PhotoSelector.App/ViewModels/MainPageViewModel.cs`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml`

### Decode JPEG seguro

Ainda nao temos decoder. O que trazer:

- Abrir arquivo com:

```csharp
FileAccess.Read
FileShare.ReadWrite | FileShare.Delete
FileOptions.Asynchronous | FileOptions.SequentialScan
```

- Usar WIC/Windows Imaging para scaled decode.
- Aplicar EXIF orientation antes do fit, como o `DecodeTargetCalculator` ja espera.
- Produzir um objeto decodificado que nao dependa do stream original.
- Fechar stream antes de entregar `ImageSource` para a UI.
- Ter teste que tenta mover/deletar o arquivo depois do decode.

### Cancelamento e descarte de obsoleto

Ainda nao temos troca visual de imagens. O que trazer:

- Cada request de decode deve receber `CancellationToken`.
- O ViewModel deve manter um numero de operacao ou token atual.
- Quando uma navegacao nova acontecer, cancelar o decode anterior.
- Se uma resposta antiga chegar atrasada, descartar antes de atualizar a UI.

### UX fullscreen limpa

Ainda nao temos viewer. O que trazer:

- Fundo preto ou cinza escuro.
- Foto centralizada com `Stretch=Uniform`/fit-contain.
- Overlay pequeno com contador, nome de arquivo e mensagens transitorias.
- Sem thumbnail strip, grade, painel lateral ou botoes permanentes.
- Overlay aparece apos mouse/teclado e some por inatividade.
- `F` alterna fullscreen e `Esc` sai do fullscreen.

### Atalhos com escopo

Ainda nao temos atalhos visuais. O que trazer:

- `Right`, `Left`, `Space`, `Home`, `End`, `Delete`, `Ctrl+Z`, `F`, `Esc`.
- Nao disparar atalhos quando o foco estiver em input de texto.
- Para F2-03, implementar no minimo o necessario para mostrar primeira foto e preparar F2-04 sem acoplar regra no Core.

### Prefetch/caching orientado por vizinhos

Ainda nao temos cache real. O que trazer depois da primeira foto:

- Prioridade atual, proximas 3 e anteriores 2.
- Ao segurar seta, favorecer preview/cached e adiar upgrade pesado ate `KeyUp`.
- Cache key com path normalizado, tamanho, last write UTC, orientation, target size, algoritmo e qualidade/cor.
- Limite dinamico de memoria e purge sem varrer cache de disco no startup critico.

Nao implementar todo cache em F2-03; apenas nao bloquear o desenho.

### Delete/Undo com feedback imediato

Ainda nao implementado. O que trazer das referencias:

- Apos rejeitar/deletar, a foto some e a navegacao avanca.
- Erro deve ser discreto e especifico.
- Operacao de arquivo precisa retornar resultado rico.
- UI deve diferenciar "acao visual ja aplicada" de "move fisico ainda pendente".

Manter a nossa regra:

- `PendingDelete` -> esconder da lista ativa -> navegar -> mover em background -> `Deleted` + journal.
- Falha -> `DeleteFailed` com contadores consistentes e erro discreto.
- Undo -> `PendingRestore` -> mover de volta -> recolocar no `SortIndex` original -> journal -> navegar para restaurada.

## Complemento da analise aprofundada - 2026-05-03

Depois de ler pontos tecnicos especificos em FlyPhotos, ImageGlass, qimgv, Oculante e JPEGView, estas sao as melhorias que fazem sentido adicionar ao nosso plano. Elas sao ideias de arquitetura e comportamento, nao copia de codigo.

### 1. Fila de decode com prioridade, coalescing e limpeza de trabalho obsoleto

O plano ja falava em cancelamento de decode obsoleto, mas ainda estava generico. A implementacao deve ter uma fila explicita, por exemplo `ViewerDecodeWorkQueue` ou `DecodePriorityQueue`, com estas regras:

- prioridade 0: foto atual;
- prioridade 1: adjacentes imediatas;
- prioridade 2: proximas/anteriores restantes;
- prioridade 3: thumbnail/disk cache/idle work;
- coalescing por `ImageCacheKey`, para nao decodificar a mesma foto/tamanho duas vezes;
- cancelamento por foto/chave;
- ao navegar para nova foto atual, limpar ou cancelar prefetch antigo antes de colocar a atual na fila;
- concorrencia baixa e controlada, normalmente 1 decode atual e 1 a 2 trabalhos de prefetch/cache, evitando competir com o disco.

Inspiracao: qimgv limpa a fila antes de load prioritario; ImageGlass tem queue de vizinhos e cancelamento por item; FlyPhotos separa preview, HQ e cache de disco. No nosso app, isso deve ficar em `Imaging/Prefetch` e ser orquestrado por `Application`, nunca por ViewModel gordo.

### 2. Navigation burst mode para tecla segurada

FlyPhotos mostra uma ideia importante: quando o usuario segura seta, o viewer prioriza resposta visual e posterga trabalho pesado. No nosso plano, isso vira:

- criar `NavigationBurstController` ou regra equivalente em `KeyboardShortcutService`/`FullscreenViewerViewModel`;
- comandos repetidos de `Right`/`Left` entram em modo burst;
- durante burst, mostrar somente preview atual ou imagem ja em memoria;
- nao iniciar refinamento pesado/HQ enquanto a tecla esta sendo repetida;
- apos `KeyUp` ou 150-250 ms sem navegacao, acionar refinamento da foto parada;
- cancelar filas antigas quando a direcao muda rapidamente.

Isso evita a sensacao de travamento quando o cliente segura seta direita para passar por muitas fotos.

### 3. Cache por janela de navegacao, nao apenas LRU

O plano ja define LRU em memoria. As referencias reforcam que viewer sequencial tem uma janela conhecida. Entao o `MemoryImageCache` deve combinar:

- LRU por limite de memoria;
- `TrimToWindow(activeWindowKeys)` quando o indice atual muda;
- janela padrao: atual, proxima 1, anterior 1, proxima 2, anterior 2, proxima 3;
- descarte explicito de recursos decodificados fora da janela quando houver pressao de memoria;
- separacao futura entre preview de tela, thumbnail e full-res/zoom.

Essa regra e mais deterministica que LRU puro para o fluxo de estúdio.

### 4. Politica de admissao de cache para JPEGs muito grandes

ImageGlass evita colocar certos arquivos pesados na fila de cache quando excedem limite de dimensao/tamanho. No nosso plano:

- a foto atual sempre pode ser decodificada no tamanho alvo;
- prefetch/cache pode pular JPEGs acima de limite configuravel de dimensao ou tamanho em MB;
- quando pular cache, registrar metrica discreta (`CacheAdmissionSkipped`);
- thumbnails futuras podem usar limite diferente de preview fullscreen;
- nao ler metadata de todos os arquivos no caminho critico apenas para decidir cache; usar essa regra quando a metadata ja existir ou quando o item entrar na fila.

Isso protege o app de sessoes com JPEGs gigantes sem atrasar a primeira foto.

### 5. Guard contra EXIF orientation dupla

FlyPhotos usa `OrientedPixelWidth/Height` e tambem pede ao WIC para respeitar EXIF orientation no pixel data. Essa combinacao e util, mas perigosa se o nosso `DecodeTargetCalculator` tambem aplicar swap manual sem uma decisao unica.

Regra nova:

- `DecodeTargetCalculator` deve receber dimensoes ja normalizadas por orientation ou receber dimensoes brutas + orientation, mas isso precisa ser explicito no request;
- `JpegDecodeService` deve documentar se usa `PixelWidth/PixelHeight` ou `OrientedPixelWidth/OrientedPixelHeight`;
- se usar `ExifOrientationMode.RespectExifOrientation` no decode, os testes devem garantir que orientation 6/8 nao sofre swap duplo;
- criar teste com fixture EXIF orientation 6 e 8 validando tamanho final e aspect ratio.

### 6. Taxonomia de erro de imagem e arquivo

Hoje o plano fala em erro discreto, mas falta uma taxonomia objetiva. Adicionar:

`ImageDecodeErrorCode`:

- `None`;
- `FileMissing`;
- `AccessDenied`;
- `UnsupportedOrNotJpeg`;
- `CorruptJpeg`;
- `DecodeCanceled`;
- `FileLocked`;
- `Unknown`.

`FileOperationErrorCode`:

- `None`;
- `SourceMissing`;
- `DestinationExists`;
- `DestinationNotWritable`;
- `SourceNotWritable`;
- `SamePath`;
- `CollisionResolved`;
- `MoveFailed`;
- `RestoreFailed`;
- `Unknown`.

Esses codigos devem alimentar overlay, logs, journal quando relevante e testes.

### 7. JPEG signature probe no caminho de decode

Oculante reforca a diferenca entre extensao e conteudo real. Como V1 lista `.jpg/.jpeg`, ainda assim pode haver arquivo corrompido ou renomeado. O plano deve incluir:

- `JpegSignatureProbe` barato, opcional antes do decode atual ou no tratamento de falha;
- verificar SOI JPEG (`FF D8`) sem ler o arquivo inteiro;
- nao fazer signature probe em todos os arquivos durante scan inicial;
- se falhar, marcar erro visual como `UnsupportedOrNotJpeg` ou `CorruptJpeg`, sem travar a sessao.

### 8. Shortcuts como mapa de comandos, com repeticao controlada

Oculante tem atalhos como dados e diferencia comandos repetiveis de comandos de borda. No nosso app:

- criar `ViewerCommand` enum no App;
- `KeyboardShortcutMap` centraliza teclas padrao;
- `KeyboardShortcutService` retorna comando somente se foco nao estiver em `TextBox`, `PasswordBox`, `RichEditBox` ou input equivalente;
- comandos repetiveis: `Next`, `Previous`, futuro `ZoomIn`, `ZoomOut`, pan;
- comandos de borda: `Delete`, `Undo`, `ToggleFullscreen`, `ExitFullscreen`, `Home`, `End`;
- `Delete` e `Ctrl+Z` nao devem disparar varias vezes por auto-repeat sem controle.

### 9. FileMoveService com resultado rico, preservacao de timestamps e rollback

qimgv reforca que operacao de arquivo nao deve ser `bool`. Para nosso `FileMoveService`:

- retornar objeto rico com status, source, destination final, collision name, exception opcional e duracao;
- preservar `LastWriteTimeUtc` e, quando viavel, `LastAccessTimeUtc` ao mover/restaurar;
- no mesmo volume, preferir move atomico/rename;
- em cross-volume futuro, tratar como copy + verify + delete, com rollback quando possivel;
- colisao deve gerar nome seguro sem sobrescrever original;
- undo deve usar o `DeletedToPath` real registrado no journal.

### 10. Disk cache SQLite eficiente quando entrar

FlyPhotos mostra que cache em SQLite pode ficar rapido quando a parte critica e pensada. Para quando o cache persistente entrar:

- usar prepared statements para caminhos quentes;
- manter contador de itens/tamanho em memoria, evitando `COUNT(*)` repetido;
- fazer touch/update de ultimo acesso de forma serializada e, se necessario, em lote;
- purgar em lote quando passar do limite, por exemplo reduzir para 50-75% do teto, evitando limpar um item por vez;
- nao varrer todo cache no startup critico;
- qualquer blob/recurso retornado pelo cache precisa ter ownership/dispose claro.

Isso e Fase 4, nao F2-03.

### 11. SessionFolderWatcher opcional, nao no caminho critico

ImageGlass e qimgv lidam com alteracoes externas na pasta. Para o nosso fluxo, isso e util, mas nao deve bloquear V1 inicial:

- planejar `SessionFolderWatcher` debounced para V1.1/V2;
- detectar arquivo removido/renomeado externamente e marcar `Missing`;
- detectar novo JPEG e adicionar com `SortIndex` posterior ou fluxo explicito;
- watcher deve usar compartilhamento que nao bloqueie delete/move;
- nunca recalcular thumbnails nem rescan pesado durante navegacao fullscreen.

### 12. Ordem do Explorer como opcional futuro

ImageGlass tem suporte conceitual para ordem do Explorer. Para nosso produto:

- manter ordenacao por nome na V1, conforme requisito;
- considerar `ExplorerViewOrderProvider` apenas se operador pedir "usar ordem atual do Explorer";
- isso deve ficar atras de feature flag/ADR, porque aumenta acoplamento com Shell/COM e nao e necessario para a primeira entrega.

## Ajuste recomendado da ordem de implementacao apos esta leitura

1. F2-03a: criar `ImageDecodeErrorCode`, `JpegSignatureProbe` e metadados minimos da foto atual.
2. F2-03b: implementar `JpegDecodeService` com WIC/WinRT, target size, EXIF orientation e teste contra orientation dupla.
3. F2-03c: exibir primeira foto em UI escura, com operation id/token para descartar resultado obsoleto.
4. F2-04a: implementar `ViewerCommand`, `KeyboardShortcutMap` e repeticao controlada.
5. F2-04b: implementar navegacao visual por setas com cancelamento forte da imagem anterior.
6. F2-05: fullscreen/overlay auto-hide.
7. F4-01: criar `ViewerDecodeWorkQueue` e `PrefetchScheduler` com prioridade, coalescing e limpeza de trabalho antigo.
8. F4-02: implementar `MemoryImageCache` com LRU + `TrimToWindow`.
9. F4-03: adicionar cache admission policy e medicao de cache skipped/hit/miss.
10. F3: implementar Delete/Undo com `FileMoveService` rico, timestamps, colisao e rollback quando possivel.

## O que nao devemos trazer para a V1

- RAW, HEIF, AVIF, WebP, TIFF, PNG ou video como superficie de produto.
- Electron, WebView, Tauri, Wails ou UI web.
- IA, cloud sync, login, PDV, API Laravel, upload ou segunda tela implementada.
- Editor, crop, filtros, rotacao persistente, rating, tags, album, catalogo ou importacao.
- Filmstrip fixa, grade sincronizada, painel lateral permanente ou file manager embutido.
- Full-res decode no fit normal.
- `Directory.GetFiles` no scan inicial.
- Source of truth em SQLite, XMP ou sidecar.
- Delete permanente por padrao.
- Dependencias pesadas de imagem antes de provar WIC JPEG-only.

## Recomendacao para F2-03 - Mostrar primeira foto no viewer

Objetivo da fatia:

Mostrar a `CurrentPhoto` da `PhotoSession` aberta, com decode JPEG dimensionado pelo `DisplayContextSnapshot`, sem bloquear UI e sem segurar file handle.

Fluxo recomendado:

1. `OpenSessionUseCase` continua retornando `OpenSessionResult(Session, CurrentPhoto)`.
2. `MainPageViewModel` guarda `PhotoSession`, `CurrentPhoto`, contadores e estado visual.
3. `MainPage` captura `DisplayContextSnapshot` pelo servico do App quando tiver tamanho real da area do viewer.
4. `PrepareViewerImageUseCase` recebe `CurrentPhoto` + `DisplayContextSnapshot`.
5. `JpegMetadataReader` le dimensoes/orientation somente da foto atual se ainda nao estiverem no `PhotoItem`.
6. `DecodeTargetCalculator` calcula target com DPI, orientation, fit-contain e margem 1.15x-1.35x.
7. `JpegDecodeService` faz WIC decode no target, usando stream curto e file share correto.
8. O App cria `SoftwareBitmapSource` ou `ImageSource` a partir de dados ja decodificados.
9. A UI mostra a imagem centralizada em fundo escuro.
10. Ao trocar foto no futuro, token/operation id descarta decode antigo antes de atualizar a tela.

Aceite tecnico de F2-03:

- A primeira foto aparece sem esperar thumbnails.
- O target decode size e calculado por `DecodeTargetCalculator`, nao por heuristica no XAML.
- EXIF orientation altera dimensoes antes do fit.
- DPI/rasterization scale entra via `DisplayContextSnapshot`.
- Fit normal nao usa full-res decode.
- O arquivo pode ser movido/deletado apos decode, provando que o stream foi liberado.
- `Core` nao referencia WinUI, Windows App SDK, WIC, filesystem concreto, Serilog ou SQLite.
- `Application` nao referencia `Window`, `Page`, XAML ou controles.

Testes recomendados para F2-03:

- Unitario de `DecodeTargetCalculator` com orientation 1, 6 e 8.
- Unitario/integracao de `JpegDecodeService` com JPEG pequeno e JPEG grande.
- Teste de handle: decodificar e depois mover o arquivo temporario.
- Teste de cancelamento/descarte: request antigo nao atualiza resultado depois de novo request.
- Build completo pelo script padrao.

Comandos esperados:

```powershell
.\tools\build.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging"
.\tools\test.ps1 -Filter "FullyQualifiedName~Application"
```

Se a fatia mexer tambem na UI:

```powershell
.\tools\test.ps1
.\tools\format.ps1
```

## Ordem recomendada depois desta analise

1. F2-03: primeira foto no viewer com WIC decode seguro.
2. F2-04: navegacao visual por setas com cancelamento/descarte de decode obsoleto.
3. F2-05: fullscreen limpo inicial com overlay auto-hide.
4. Prefetch/cache inicial: atual, proximas 3, anteriores 2.
5. Delete/Undo: mover para `_deletadas_evydencia`, estados, journal e `Ctrl+Z`.
6. Single-instance antes de considerar menu de contexto completo.

## Decisoes registradas

- FlyPhotos, JPEGView, qimgv, ImageGlass e QuickRawPicker ficam como inspiracao conceitual por risco de licenca e/ou escopo.
- WindowsAppSDK-Samples e Win2D sao referencias tecnicas seguras, mas Win2D nao entra antes de ADR/prototipo.
- WicViewerD2D valida o desenho WIC -> render, mas nao deve guiar estrutura de codigo WinUI.
- cull reforca foco e simplicidade de culling, mas RAW/XMP nao entram na V1.
- A proxima implementacao deve priorizar prova de decode correto, file handle liberado e primeira foto visivel rapidamente.
