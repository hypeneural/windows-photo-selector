# Execucao 0014 - Validacao EXIF orientation 6/8

## Objetivo

Validar que o pipeline de decode JPEG respeita EXIF orientation sem aplicar rotacao duas vezes. A fatia cria fixtures sinteticas em tempo de teste com orientation 6 e 8, decodifica com `JpegDecodeService` e valida dimensoes e posicao dos cantos coloridos.

Esta fatia continua sem UI, fullscreen, cache, prefetch, Delete/Undo, API, PDV, RAW ou menu de contexto.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `src/Evydencia.PhotoSelector.Imaging/AGENTS.md`
- `tests/AGENTS.md`
- `docs/execution-progress.md`
- `docs/execution-plans/0013-jpeg-decode-foundation.md`
- `src/Evydencia.PhotoSelector.Imaging/Decode/JpegDecodeService.cs`
- `src/Evydencia.PhotoSelector.Imaging/Sizing/DecodeTargetCalculator.cs`
- `tests/Evydencia.PhotoSelector.Imaging.Tests/Decode/JpegTestImage.cs`
- `tests/Evydencia.PhotoSelector.Imaging.Tests/Decode/JpegDecodeServiceTests.cs`

## Camada afetada

- Imaging
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

1. Atualizar `JpegTestImage` para gravar `System.Photo.Orientation` como `UInt16`, conforme documentacao oficial.
2. Criar fixture sintetica com quadrantes coloridos para detectar rotacao real.
3. Adicionar testes para orientation 6 e 8.
4. Validar que `DecodeTargetCalculator` recebe dimensoes brutas + orientation e que o decode retorna pixels orientados.
5. Rodar testes de Imaging, build, format e suite completa.
6. Atualizar `docs/execution-progress.md`.

## Testes necessarios

- Orientation 6: canto superior esquerdo da saida deve vir do canto inferior esquerdo bruto.
- Orientation 8: canto superior esquerdo da saida deve vir do canto superior direito bruto.
- Dimensoes finais devem ser `OrientedTargetWidth`/`OrientedTargetHeight`.

## Criterio de aceite

- Testes automatizados provam que orientation 6 e 8 rotacionam a imagem na direcao esperada.
- Resultado nao usa full-res quando o target e menor que o original.
- `F0-06` fica marcado como concluido.
- Build e testes passam.

## Riscos

- JPEG e lossy e pode alterar cores exatas; mitigar validando canal dominante em amostras longe das bordas.
- Alguns codecs podem nao aceitar metadata de orientation; mitigar usando `System.Photo.Orientation` com `PropertyType.UInt16`, conforme Microsoft Learn.
- Se `BitmapEncoder.BitmapProperties.SetPropertiesAsync` falhar, registrar o bloqueio e usar fixture binaria minima na proxima tentativa.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging"
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

- `JpegTestImage` agora gera fixtures JPEG sinteticas com quadrantes coloridos e metadata `System.Photo.Orientation` em `UInt16`.
- `JpegDecodeServiceTests` cobre EXIF orientation 6 e 8, validando dimensoes finais orientadas e a posicao dos quadrantes apos `RespectExifOrientation`.
- A validacao confirma que o `DecodeTargetCalculator` continua recebendo dimensoes brutas + orientation e que o decode retorna pixels orientados sem aplicar rotacao/dimensoes duas vezes.
- `F0-06` foi marcado como concluido em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging"
.\tools\build.ps1
.\tools\format.ps1
.\tools\test.ps1
```

Resultados:

- Imaging: 13 testes aprovados.
- Suite completa: 60 testes aprovados.
- Build: aprovado.
- Format: aprovado apos normalizar os arquivos modificados para CRLF.

## Pendencias deliberadas

- Conversao do `ImageDecodeResult` para `ImageSource` no viewer WinUI continua fora desta fatia.
- Cancelamento de decode obsoleto entra na fatia de viewer/prefetch.
- Fixtures persistidas em `/tests/fixtures/jpeg/orientation` continuam opcionais; por enquanto os testes geram JPEGs em pasta temporaria para evitar binarios no repositorio.
