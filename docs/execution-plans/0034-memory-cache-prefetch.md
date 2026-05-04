# Plano 0034 - Cache em memoria LRU e prefetch de preview

## Objetivo

Adicionar uma primeira camada real de cache/prefetch para o viewer sem alterar o fluxo de negocio. O viewer deve reutilizar previews decodificados em memoria, manter o fit normal sem full-res e iniciar prefetch das proximas 3 fotos e anteriores 2 apos carregar a foto atual.

## Arquivos lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `src/Evydencia.PhotoSelector.Imaging/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.Imaging/Decode/JpegDecodeService.cs`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `docs/execution-progress.md`

## Camadas afetadas

- Imaging
- App
- Infrastructure
- Tests
- Docs

## Restricoes

- V1 continua offline.
- Sem API Laravel.
- Sem PDV.
- Sem upload.
- Sem RAW.
- Sem Electron/WebView.
- Sem bloquear UI thread.
- Sem quebrar separacao de camadas.
- Sem excluir permanentemente por padrao.

## Plano

1. Criar `MemoryImageCache` LRU em Imaging.
2. Criar `ImageCacheKey` com versao de arquivo, modo e target de preview.
3. Criar `PreviewCacheService` para preview fit e actual-size sob demanda.
4. Criar `PrefetchScheduler` com cancelamento de fila antiga e ordem next 3/previous 2.
5. Ligar `MainPage` ao cache para load atual e ao scheduler apos imagem carregada.
6. Registrar servicos no DI.
7. Cobrir cache, key, prefetch e wiring do viewer com testes.

## Testes necessarios

- `MemoryImageCache` deve evictar LRU.
- Cache deve ignorar item maior que limite.
- Key deve invalidar por tamanho/last write.
- `PreviewCacheService` deve devolver cache hit sem reabrir arquivo.
- Actual-size deve usar entrada separada do preview.
- `PrefetchScheduler` deve priorizar proximas 3 e anteriores 2.
- UI smoke deve confirmar wiring em `MainPage`.

## Criterio de aceite

- Build passa.
- Testes de Imaging e UiSmoke passam.
- Teste completo passa.
- Fit normal usa cache/prefetch de preview e continua sem full-res.
- Actual-size `1` pode reutilizar cache, mas prefetch nao decodifica full-res.

## Riscos

- Memoria: cache full-res pode crescer. Mitigacao: LRU com limite de bytes e entrada maior que limite nao e armazenada.
- Stale decode: usuario segurando seta pode criar fila antiga. Mitigacao: `PrefetchScheduler.Schedule` cancela run anterior.
- UI thread: prefetch roda em background e armazena bytes, nao `ImageSource`.
- File handle: cache guarda bytes decodificados; decode continua usando streams curtos existentes.

## Comandos

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging|FullyQualifiedName~UiSmoke"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```
