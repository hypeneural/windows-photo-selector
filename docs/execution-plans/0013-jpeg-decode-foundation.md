# Execucao 0013 - Base de decode JPEG seguro

## Objetivo

Implementar a primeira parte tecnica de F2-03: taxonomia de erro de decode, prova barata de assinatura JPEG e um servico de decode JPEG dimensionado em `Imaging`. A entrega deve provar que o arquivo e aberto com stream curto, `FileShare.ReadWrite | FileShare.Delete`, target size calculado e resultado independente do `FileStream`.

Esta fatia nao implementa fullscreen, cache, prefetch, Delete/Undo, API, PDV, RAW ou menu de contexto.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `src/Evydencia.PhotoSelector.Imaging/AGENTS.md`
- `tests/AGENTS.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `docs/execution-progress.md`
- `src/Evydencia.PhotoSelector.Imaging/Sizing/*.cs`
- `tests/Evydencia.PhotoSelector.Imaging.Tests/Sizing/*.cs`

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

1. Criar `ImageDecodeErrorCode`.
2. Criar `JpegSignatureProbe` para validar SOI JPEG sem ler o arquivo inteiro.
3. Criar modelos `ImageDecodeRequest` e `ImageDecodeResult`.
4. Criar `JpegDecodeService` usando Windows Imaging APIs com `BitmapTransform` e `ExifOrientationMode.RespectExifOrientation`.
5. Abrir arquivos com `FileAccess.Read`, `FileShare.ReadWrite | FileShare.Delete`, `FileOptions.Asynchronous | FileOptions.SequentialScan`.
6. Garantir que o resultado carrega pixels em memoria e fecha o stream antes de retornar.
7. Criar testes de signature probe, decode dimensionado e handle liberado.
8. Rodar build e testes de Imaging.
9. Atualizar `docs/execution-progress.md`.

## Testes necessarios

- `JpegSignatureProbeTests`
- `JpegDecodeServiceTests`
- Teste com JPEG valido pequeno.
- Teste com arquivo que tem extensao `.jpg`, mas assinatura invalida.
- Teste de file handle: decodificar e mover o arquivo logo depois.

## Criterio de aceite

- `JpegDecodeService` retorna pixels decodificados e dimensoes finais sem expor `FileStream`.
- Decode usa o target calculado, sem full-res no fit normal.
- Arquivo decodificado pode ser movido imediatamente apos o decode.
- Arquivo nao-JPEG retorna erro controlado.
- Build passa.
- Testes de Imaging passam.

## Riscos

- WinRT stream interop: mitigar usando `FileStream.AsRandomAccessStream`, conforme documentacao oficial .NET para converter streams.
- EXIF orientation dupla: mitigar mantendo o target em dimensoes brutas para `BitmapTransform` e deixando WIC aplicar orientation na saida; testes EXIF completos entram na proxima fatia com fixtures orientadas.
- Pixel buffer grande: mitigar usando `DecodeTargetCalculator` antes de chamar o decoder.
- Cancelamento real do WinRT async: nesta fatia o token e checado antes/depois da operacao; cancelamento agressivo da fila entra em F4.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging"
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

- Criados `ImageDecodeErrorCode`, `ImageDecodeRequest`, `ImageDecodeResult`, `JpegSignatureProbe` e `JpegDecodeService`.
- `JpegDecodeService` usa Windows Imaging APIs com `BitmapTransform`, `ExifOrientationMode.RespectExifOrientation` e `ColorManagementMode.DoNotColorManage`.
- Leitura de JPEG usa stream curto com `FileShare.ReadWrite | FileShare.Delete` e `FileOptions.Asynchronous | FileOptions.SequentialScan`.
- Resultado de decode retorna pixels BGRA em memoria, sem expor `FileStream` ou `ImageSource`.
- Criados testes para assinatura JPEG valida, conteudo invalido, arquivo ausente, decode dimensionado, erro controlado e move imediato depois do decode.
- `tools/build.ps1` passou.
- `tools/test.ps1 -Filter "FullyQualifiedName~Imaging"` passou.
- `tools/format.ps1` passou apos normalizar fim de linha dos arquivos novos.
- `tools/test.ps1` passou com 58 testes.

## Pendencias deliberadas

- A conversao dos pixels para `SoftwareBitmapSource`/`ImageSource` fica para a proxima fatia de UI.
- Fixtures EXIF orientation 6/8 ainda precisam ser adicionadas para validar ausencia de orientation dupla.
- Cancelamento agressivo de fila e `ViewerDecodeWorkQueue` ficam para F4.
