# Viewer implementation backlog

Ultima atualizacao: 2026-05-04

## Objetivo

Esta e a lista operacional de implementacao para continuar evoluindo o viewer ate uma experiencia mais proxima do Fotos/Visualizador do Windows: sem tela preta, zoom nitido, fullscreen real, navegacao instantanea e memoria controlada.

Documento base: `docs/viewer-quality-performance-gap-analysis.md`.

## Decisao principal

Nao decodificar full-res sempre.

Para uma foto `6000x4000`, o bitmap BGRA ocupa aproximadamente:

```text
6000 * 4000 * 4 = 96.000.000 bytes ~= 91,6 MiB
```

Se o app mantiver full-res para atual + proximas 3 + anteriores 2, a memoria so de pixels pode passar de `500 MB`, sem contar `SoftwareBitmap`, `SoftwareBitmapSource`, cache, overhead WinUI e estruturas auxiliares.

Estrategia correta:

1. preview fit rapido;
2. cache/prefetch de preview;
3. troca atomica sem tela preta;
4. re-decode por tiers de zoom;
5. full-res apenas quando o zoom exige ou quando o usuario aperta `1`;
6. telemetria para saber onde esta o custo real.

## Fontes oficiais relevantes

- `BitmapFrame.GetPixelDataAsync`: permite controlar formato, transform, EXIF orientation e color management; com `RespectExifOrientation`, as dimensoes devem considerar `OrientedPixelWidth`/`OrientedPixelHeight`.
  <https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmapframe.getpixeldataasync>
- `BitmapTransform`: `ScaledWidth`/`ScaledHeight` ficam no espaco da imagem fonte, antes de rotacao/flip. Isso confirma que o calculo de sizing precisa continuar centralizado e testado.
  <https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmaptransform>
- `SoftwareBitmapSource`: e `ImageSource`, implementa `IDisposable/IClosable` e pode aumentar uso de memoria por manter dados descomprimidos; portanto, cache de `ImageSource` precisa de LRU e descarte explicito.
  <https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.imaging.softwarebitmapsource>
- `FullScreenPresenter`: fullscreen deve esconder elementos do sistema como title bar e taskbar por padrao.
  <https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.windowing.fullscreenpresenter>
- Win2D overview e interpolacao: Win2D e candidato para spike futuro de renderizacao, mas nao deve bloquear a proxima entrega.
  <https://learn.microsoft.com/en-us/windows/apps/develop/win2d/features>
  <https://microsoft.github.io/Win2D/WinUI3/html/T_Microsoft_Graphics_Canvas_CanvasImageInterpolation.htm>

## Ordem aprovada de implementacao

### 1. VQ-01 - Atomic image swap

Prioridade: P0

Motivo:

- E o maior ganho perceptivo imediato.
- Remove a tela preta/piscada ao navegar para imagem uncached.
- Nao altera delete/undo nem estrategia de decode.

Comportamento esperado:

```text
imagem atual continua visivel
proxima imagem inicia carregamento
cache hit: troca imediata
cache miss: mantem anterior + overlay discreto
quando proxima fonte estiver pronta: troca atomica
erro: mantem anterior ou mostra erro discreto
resultado obsoleto: ignorado por request/version id
```

Implementacao sugerida:

- criar contador de request/version em `MainPage` ou `ViewerImageLoadCoordinator`;
- nao chamar `CurrentPhotoImage.Source = null` no inicio de `LoadCurrentPhotoAsync`;
- manter status discreto sem esconder a imagem anterior;
- trocar `CurrentPhotoImage.Source` apenas quando `ImageSource` novo estiver pronto;
- conferir `requestId` e `CurrentPhoto.Id` antes do swap;
- descartar fonte nova se ela ficou obsoleta.

Criterio de aceite:

- ao navegar para imagem uncached, a tela nao fica preta;
- se o decode atrasar, a imagem anterior permanece visivel;
- segurando seta, decode antigo nao substitui imagem nova;
- erro de decode nao apaga a imagem anterior.

Testes:

- `UiSmoke`/source test para garantir que nao ha clear antecipado;
- teste manual/smoke em pasta real;
- idealmente teste automatizado com decode atrasado/fake no futuro.

### 2. VQ-04 - Telemetria de time-to-visible

Prioridade: P0

Motivo:

- Sem medicao, nao da para saber se o gargalo esta em decode, cache, criacao de `SoftwareBitmapSource`, dispatcher/UI thread ou swap visual.
- Deve entrar antes do zoom ladder completo e antes de otimizacoes mais caras.

Eventos padronizados:

- `viewer_navigation_requested`
- `viewer_cache_hit`
- `viewer_cache_miss`
- `viewer_decode_started`
- `viewer_decode_completed`
- `viewer_source_create_started`
- `viewer_source_create_completed`
- `viewer_image_visible`
- `viewer_decode_canceled_stale`
- `viewer_zoom_tier_requested`
- `viewer_zoom_tier_swapped`

Tempos minimos:

- `time_to_visible = ImageVisibleAt - NavigationRequestedAt`
- `decode_time = DecodeCompletedAt - DecodeStartedAt`
- `source_creation_time = ImageSourceCreationCompletedAt - ImageSourceCreationStartedAt`
- `cache_lookup_time = CacheLookupCompletedAt - NavigationRequestedAt`

Classificacao:

- `cached_memory_pixels`
- `cached_image_source`
- `uncached_decode`
- `zoom_tier_decode`
- `actual_size_decode`
- `prefetch_hit`
- `prefetch_miss`

Criterio de aceite:

- cada navegacao registra hit/miss e tempo ate imagem visivel;
- logs nao bloqueiam UI thread;
- logs nao incluem dado sensivel desnecessario.

### 3. VQ-03 - ViewerImageSourceCache no App

Prioridade: P1

Motivo:

- O cache atual evita re-decode, mas ainda recria `SoftwareBitmapSource` no App.
- `SoftwareBitmapSource` e WinUI/App, nao deve entrar em `Imaging`.

Escopo:

- criar `ViewerImageSourceCache` no projeto `App`;
- guardar fonte visual preparada para:
  - atual;
  - proxima 1 ou 2;
  - anterior 1;
- invalidar por:
  - `ImageCacheKey`;
  - `DisplayContext`;
  - target/tier;
  - alteracao do arquivo;
- descartar explicitamente fontes removidas do LRU quando possivel.

Nao fazer:

- nao cachear `SoftwareBitmapSource` para todas as proximas 3/anteriores 2;
- nao cachear varias imagens full-res;
- nao mover `ImageSource` para `Imaging`.

Criterio de aceite:

- cache hit de preview nao paga sempre o custo completo de `SoftwareBitmapSource`;
- memoria fica limitada;
- source obsoleta e descartada.

### 4. VQ-02 - Zoom quality ladder

Prioridade: P1

Motivo:

- Hoje wheel/`+` escala o preview atual.
- Isso e rapido, mas pode ficar suave/borrado quando o zoom passa da resolucao real do preview.
- O Fotos do Windows refina automaticamente; o usuario nao precisa apertar `1`.

Tiers propostos:

```text
Tier 0 - Fit preview
Uso: abertura e navegacao rapida
Long edge: fit * 1.15 a 1.35

Tier 1 - Zoom leve
Dispara em ~125% a 150% do preview util
Long edge: fit * 2

Tier 2 - Zoom medio
Dispara em ~200% a 250%
Long edge: fit * 3 ou * 4, limitado ao original

Tier 3 - Actual-size / 100%
Dispara perto de 100% real ou tecla `1`
Long edge: original
```

Regras:

- manter imagem atual visivel enquanto o tier novo decodifica;
- trocar atomically quando o tier novo estiver pronto;
- manter mesmo ponto central de zoom/pan no swap;
- usar hysteresis para nao redecodificar a cada tick do mouse;
- full-res apenas quando necessario.

Classes sugeridas:

- `ZoomQualityPolicy`
- `ZoomQualityTier`
- `ZoomDecodeRequest`

Criterio de aceite:

- zoom em JPEG `6000x4000` nao fica preso ao preview fit borrado;
- `1` continua forcando 100% real;
- fit normal nao usa full-res;
- memoria nao cresce sem limite.

### 5. VQ-05 - Prefetch direcional/adaptativo e smoke de navegacao rapida

Prioridade: P1

Motivo:

- O prefetch atual ja faz proximas 3 e anteriores 2, mas nao conhece direcao.
- Em selecao real, o usuario normalmente segue por varios segundos na mesma direcao.

Novo comportamento:

- se ultima navegacao foi direita:
  - atual: maxima prioridade;
  - proximas 1, 2, 3: alta;
  - anteriores 1, 2: baixa.
- se ultima navegacao foi esquerda:
  - atual: maxima prioridade;
  - anteriores 1, 2, 3: alta;
  - proximas 1, 2: baixa.

Quando segurar seta:

- cancelar decodes obsoletos;
- nao acumular fila antiga;
- reduzir trabalho de refinamento;
- pausar thumbnail/filmstrip;
- pausar tier de zoom se o usuario estiver navegando rapidamente.

Criterio de aceite:

- segurando seta direita/esquerda em pasta grande, fila nao cresce indefinidamente;
- memoria nao explode;
- navegacao cached continua fluida.

### 6. VQ-06 - Fullscreen real

Prioridade: P1

Motivo:

- A implementacao com `FullScreenPresenter` esta correta, mas precisa smoke real em varios cenarios.

Cenarios:

- unpackaged/debug;
- MSIX instalado;
- abrir por `--folder`;
- abrir pelo menu dev HKCU;
- monitor unico;
- segundo monitor futuro.

Teste minimo:

- pressionar `F`;
- validar `Presenter.Kind == FullScreen`;
- verificar visualmente sem titlebar/taskbar;
- pressionar `Esc`;
- validar retorno ao modo anterior.

Se barra superior aparecer, investigar:

- `AppTitleBar.Visibility`;
- `ExtendsContentIntoTitleBar`;
- `SetTitleBar`;
- presenter atual;
- packaged vs unpackaged;
- overlay ocupando topo.

### 7. VQ-07 - Settings base

Prioridade: P2

Motivo:

- Depois que o motor do viewer estiver mais estavel, settings permitem ajustar comportamento sem recompilar.

Settings iniciais:

- delete mode;
- cache memory limit;
- prefetch next/previous;
- abrir fullscreen por padrao;
- quality mode;
- zoom wheel step;
- overlay timeout.

Criterio de aceite:

- settings persistem localmente;
- V1 continua offline;
- defaults conservadores.

### 8. VQ-09 - Spike Win2D/Direct2D

Prioridade: P2

Motivo:

- Win2D pode melhorar renderizacao/pan/zoom, mas nao deve bloquear atomic swap nem zoom ladder.

Escopo:

- prototipo isolado;
- comparar nitidez, frame pacing e memoria;
- avaliar `CanvasBitmap`, DPI e interpolacao;
- manter atras de `IImageRenderer` se for adotado.

Criterio:

- so substituir pipeline atual com benchmark e smoke visual.

### 9. VQ-08 - MSIX/ShellExtension moderno

Prioridade: P2 para motor do viewer; P0 para instalador/produto quando o viewer estiver aceitavel.

Motivo:

- Importante para fluxo real do Explorer, mas nao deve competir com o motor do viewer agora.

Escopo:

- validar single-instance empacotado;
- resolver launch por alias/app activation;
- criar `ShellExtension` moderno `IExplorerCommand`;
- manter extensao minima;
- tratar clique em pasta/fundo da pasta.

## Melhorias estruturais recomendadas

### ViewerImageLoadCoordinator

Criar um coordenador para reduzir `MainPage.xaml.cs`.

Responsabilidades:

- receber `PhotoItem` e `DisplayContext`;
- controlar request/version id;
- consultar cache de pixels e cache de source;
- criar `ImageSource`;
- fazer atomic swap;
- registrar telemetria;
- disparar prefetch;
- ignorar resultado obsoleto.

Nao deve:

- conhecer delete/undo;
- chamar API remota;
- depender de `Core`.

### Viewer load states

Estados recomendados:

- `Idle`
- `UsingCachedSource`
- `DecodingPreview`
- `DecodingZoomTier`
- `CreatingImageSource`
- `Ready`
- `Failed`
- `CanceledBecauseStale`

Uso:

- logs;
- testes;
- UX discreta;
- diagnostico de performance.

### ZoomQualityPolicy

Nao espalhar thresholds no code-behind.

Responsabilidades:

- `GetRequiredTier(photo, displayContext, zoom, currentTier)`;
- hysteresis;
- estimar memoria;
- decidir quando actual-size e permitido.

### ViewerPerformanceEvents

Centralizar nomes e payloads de eventos para evitar logs inconsistentes.

## O que nao implementar agora

- cache em disco de preview;
- thumbnails/filmstrip;
- Win2D como renderer principal;
- color management preciso;
- API/PDV/login/upload/RAW;
- multiplas janelas/segunda tela.

Esses itens continuam importantes, mas depois de:

1. atomic swap;
2. telemetria;
3. cache de `ImageSource`;
4. zoom quality ladder.

## Proxima fatia recomendada

Implementar `VQ-01 - Atomic image swap`.

Motivo:

- menor risco;
- alto impacto visual;
- prepara a base para cache de source e zoom tier;
- resolve a principal sensacao de lentidao percebida hoje.

Plano detalhado: `docs/execution-plans/0035-atomic-image-swap.md`.
