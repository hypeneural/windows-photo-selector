# Execucao 0004 - DisplayContext e DecodeTargetCalculator

## Objetivo

Criar a base testavel para decode JPEG dimensionado, sem implementar ainda o `JpegDecodeService` nem exibir imagens. Esta fatia entrega um snapshot de display/DPI na camada `Application` e um `DecodeTargetCalculator` na camada `Imaging`, garantindo `fit contain`, EXIF orientation, margem de qualidade e clamp ao tamanho original.

## Arquivos que serao lidos

- `AGENTS.md`
- `src/Evydencia.PhotoSelector.Imaging/AGENTS.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- Documentacao oficial Microsoft sobre `Image`, `BitmapImage.DecodePixelWidth` e `DecodePixelType`.

## Camada afetada

- Application
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
- Sem decode real nesta fatia.
- Sem WIC/WinUI dentro do calculador.
- Sem file IO.
- Sem bloquear UI thread.

## Plano em passos pequenos

1. Criar `DisplayRole` e `DisplayContextSnapshot` em `Application`.
2. Criar modelos de sizing em `Imaging/Sizing`.
3. Implementar `DecodeTargetCalculator`.
4. Considerar EXIF orientation 5, 6, 7 e 8 como troca de dimensoes.
5. Calcular `fit contain` em pixels fisicos.
6. Aplicar margem padrao dentro do intervalo 1.15x a 1.35x.
7. Clamp ao tamanho original para nao upscaling de decode.
8. Remover placeholders de Imaging.
9. Criar testes de `DisplayContextSnapshot` e `DecodeTargetCalculator`.
10. Rodar build, testes e format.

## Testes necessarios

- DIPs e rasterization scale geram pixels fisicos.
- Foto landscape grande em 1920x1080 calcula fit 1620x1080 e target com margem.
- EXIF orientation 6 troca dimensoes e mapeia target de volta para dimensoes de decode.
- Imagem menor que viewport nao e ampliada no decode.
- Margem fora do intervalo aceito falha.

## Criterio de aceite

- `DecodeTargetCalculator` existe e esta testado.
- Nao ha decode real nem dependencia de WinUI/WIC no calculador.
- Build, testes e format passam sem avisos.

## Riscos

- Confundir pixels logicos com fisicos: `DisplayContextSnapshot` expoe ambos explicitamente.
- Aplicar EXIF depois do fit: calculador aplica orientation antes do `fit contain`.
- Criar full-res desnecessario: calculador sempre clampa ao necessario e ao original.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Application"
.\tools\test.ps1 -Filter "FullyQualifiedName~Imaging"
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `DisplayRole` criado.
- [x] `DisplayContextSnapshot` criado com DIPs, pixels fisicos, rasterization scale, fullscreen e role.
- [x] `DecodeTargetRequest` criado.
- [x] `DecodeTarget` criado.
- [x] `DecodeTargetCalculator` criado.
- [x] EXIF orientation 5, 6, 7 e 8 tratam troca de dimensoes.
- [x] `fit contain` calculado em pixels fisicos.
- [x] Margem padrao `4/3` aplicada dentro do intervalo 1.15x a 1.35x.
- [x] Target clampa ao tamanho original para evitar upscaling de decode.
- [x] Testes de Application e Imaging criados.
- [x] Build, testes e format check executados com sucesso.

## Observacoes

- Esta fatia nao decodifica JPEG e nao referencia WinUI/WIC no calculador.
- O servico WinUI que cria `DisplayContextSnapshot` a partir de janela/monitor real ainda fica pendente para a fatia de App.
- A documentacao oficial Microsoft reforca que `DecodePixelWidth`/`DecodePixelHeight` devem ser definidos para imagens grandes exibidas em regioes menores e que, no WinUI, esses valores sao fisicos por padrao.
