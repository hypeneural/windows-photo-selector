# Plano 0035 - Atomic image swap

## Objetivo

Remover a piscada/tela preta durante navegacao do viewer. A imagem atual deve permanecer visivel enquanto a proxima imagem decodifica ou cria `ImageSource`; a troca deve acontecer somente quando a nova fonte visual estiver pronta e ainda corresponder ao request atual.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `docs/viewer-quality-performance-gap-analysis.md`
- `docs/viewer-implementation-backlog.md`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `src/Evydencia.PhotoSelector.App/Imaging/ViewerImageSourceFactory.cs`
- `src/Evydencia.PhotoSelector.Imaging/Cache/PreviewCacheService.cs`
- `tests/Evydencia.PhotoSelector.UiSmokeTests/MainPageShortcutSourceTests.cs`

## Camadas afetadas

- App
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
- Sem alterar delete/undo.
- Sem implementar cache em disco.
- Sem implementar Win2D.

## Plano em passos pequenos

1. Identificar todos os pontos em `MainPage.xaml.cs` que limpam `CurrentPhotoImage.Source` antes do novo source estar pronto.
2. Adicionar request/version id para carregamento visual de imagem.
3. Alterar `LoadCurrentPhotoAsync` para manter a imagem anterior durante decode/cache miss.
4. Trocar `CurrentPhotoImage.Source` somente depois de `ViewerImageSourceFactory.CreateAsync` terminar.
5. Conferir request id e `PhotoItem.Id` antes do swap.
6. Se o resultado ficou obsoleto, descartar/ignorar sem trocar UI.
7. Em falha de decode, manter imagem anterior e exibir status discreto.
8. Manter comportamento especial de delete/undo onde a imagem pode precisar sumir ou mudar imediatamente.
9. Atualizar `UiSmoke` para bloquear regressao de `CurrentPhotoImage.Source = null` no caminho normal de navegacao.
10. Fazer smoke manual em pasta real com navegacao uncached.

## Testes necessarios

- `UiSmoke` para confirmar atomic swap no caminho normal.
- `UiSmoke` para confirmar request/version id.
- Build completo.
- Testes completos.
- Smoke real em pasta de fotos.

## Criterio de aceite

- Navegacao para imagem uncached nao deixa tela preta.
- Imagem anterior permanece ate a proxima estar pronta.
- Decode antigo nao substitui imagem nova quando o usuario segura seta.
- Erro de decode nao apaga a imagem anterior.
- `Delete` e `Ctrl+Z` continuam funcionando.

## Riscos

- UX: manter imagem anterior pode confundir se o overlay nao indicar carregamento. Mitigacao: status discreto enquanto carrega.
- Stale decode: resultado antigo pode trocar imagem errada. Mitigacao: request/version id e checagem de `PhotoItem.Id`.
- Delete/undo: alguns caminhos realmente precisam limpar a imagem. Mitigacao: separar caminho normal de navegacao do caminho de comando de arquivo.
- Memoria: manter imagem anterior e nova por curto periodo aumenta uso momentaneo. Mitigacao: swap rapido e cache LRU.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~UiSmoke|FullyQualifiedName~Application|FullyQualifiedName~Imaging"
.\tools\format.ps1
.\tools\build.ps1
.\tools\test.ps1
```

## Saida esperada

- `MainPage.xaml.cs` com troca atomica no caminho de navegacao.
- Testes de smoke atualizados.
- `docs/execution-progress.md` atualizado com fatia 0035.
