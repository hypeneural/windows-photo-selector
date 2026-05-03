# Execucao 0015 - Primeira foto no viewer WinUI

## Objetivo

Conectar o pipeline de decode JPEG dimensionado ao WinUI para mostrar a primeira foto da sessao aberta por `--folder`. A imagem deve vir de pixels decodificados pelo `JpegDecodeService`, nao de URI direto para arquivo, evitando decode full-res sem controle e evitando file handle preso.

Esta fatia nao implementa Delete/Undo, cache pesado, prefetch, fullscreen completo, API, PDV, RAW, upload ou menu de contexto.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.Imaging/AGENTS.md`
- `docs/execution-progress.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `src/Evydencia.PhotoSelector.App/ViewModels/MainPageViewModel.cs`
- `src/Evydencia.PhotoSelector.Imaging/Decode/JpegDecodeService.cs`
- `src/Evydencia.PhotoSelector.Imaging/Sizing/DecodeTargetCalculator.cs`
- `tests/Evydencia.PhotoSelector.UiSmokeTests/ViewModels/MainPageViewModelTests.cs`
- `tests/Evydencia.PhotoSelector.Imaging.Tests/Decode/JpegDecodeServiceTests.cs`

## Camada afetada

- App
- Imaging
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

1. Adicionar metodo de decode para display que leia dimensoes reais pelo decoder e calcule o `DecodeTarget` a partir de `DisplayContextSnapshot`.
2. Manter `FileShare.ReadWrite | FileShare.Delete` e `ExifOrientationMode.RespectExifOrientation`.
3. Criar conversor App-side de `ImageDecodeResult` para `SoftwareBitmapSource`.
4. Registrar servicos necessarios no composition root.
5. Expandir `MainPageViewModel` para expor foto atual, imagem atual, contador e estado de carregamento.
6. Atualizar `MainPage.xaml` para exibir viewer escuro com `Image Stretch=Uniform` e overlay minimo.
7. Carregar a primeira foto apos a sessao inicial usando cancellation token para descartar trabalho antigo.
8. Atualizar testes de ViewModel e Imaging.
9. Rodar build, format e testes.
10. Atualizar `docs/execution-progress.md`.

## Testes necessarios

- Imaging: decode para display calcula target a partir de dimensoes reais do JPEG e nao retorna full-res quando a area de viewer e menor.
- Imaging: decode para display respeita EXIF orientation 6/8 sem dupla aplicacao.
- UI smoke/ViewModel: sessao aberta expoe foto atual, contador e estado do viewer.

## Criterio de aceite

- App continua abrindo por `--folder`.
- Quando a sessao tem JPEG, a primeira foto tem caminho, contador e `ImageSource` carregavel.
- A UI usa `ImageSource` criado de pixel buffer decodificado, nao URI direto para arquivo.
- O decode usa tamanho alvo calculado por `DisplayContext`.
- O arquivo exibido continua liberado apos o decode.
- `F2-03` fica marcado como concluido.
- Build, format e testes passam.

## Riscos

- `SoftwareBitmapSource.SetBitmapAsync` exige BGRA com alpha premultiplicado; mitigacao: o decode ja retorna `BitmapPixelFormat.Bgra8` e `BitmapAlphaMode.Premultiplied`.
- Criar `SoftwareBitmapSource` fora da UI thread pode falhar; mitigacao: conversao para `ImageSource` acontece no App apos o await voltar para a UI thread.
- Decodes antigos podem chegar atrasados; mitigacao: `CancellationTokenSource` por carregamento.
- Dimensoes sem metadata no `PhotoItem`; mitigacao: metodo de display consulta o `BitmapDecoder` diretamente.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging|FullyQualifiedName~UiSmoke"
.\tools\build.ps1
.\tools\format.ps1
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

- `JpegDecodeService` ganhou `DecodeForDisplayAsync`, que consulta `BitmapDecoder.PixelWidth`, `PixelHeight`, `OrientedPixelWidth` e `OrientedPixelHeight`, calcula o `DecodeTarget` pelo `DisplayContextSnapshot` e decodifica com `ExifOrientationMode.RespectExifOrientation`.
- `ViewerImageSourceFactory` converte o pixel buffer BGRA premultiplicado para `SoftwareBitmapSource` no App, mantendo WinUI fora de Imaging.
- `MainPage` agora troca da tela inicial para um viewer escuro quando uma sessao abre, carrega a primeira foto com cancelamento e usa `Image.Stretch=Uniform`.
- `MainPageViewModel` continua testavel sem tipos WinUI e expoe estado de viewer, foto atual, contador e status de carregamento.
- `F2-03` foi marcado como concluido em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging|FullyQualifiedName~UiSmoke"
.\tools\build.ps1
.\tools\format.ps1
.\tools\test.ps1
.\tools\build.ps1
```

Resultados:

- Imaging: 15 testes aprovados.
- UiSmoke: 3 testes aprovados.
- Suite completa: 62 testes aprovados.
- Build completo: aprovado, 0 warnings.
- Format: aprovado apos normalizar CRLF dos arquivos alterados.

## Pendencias deliberadas

- Navegacao visual por setas fica para F2-04.
- Fullscreen limpo fica para F2-05.
- Prefetch/cancelamento agressivo por burst de navegacao fica para a fase de cache/prefetch.
- Ainda nao ha teste visual automatizado de screenshot do viewer; a validacao atual cobre build XAML, ViewModel e pipeline de decode.
