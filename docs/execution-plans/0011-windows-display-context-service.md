# Execucao 0011 - WindowsDisplayContextService

## Objetivo

Criar o servico WinUI que captura tamanho efetivo, area util, escala de rasterizacao/DPI e estado fullscreen em um `DisplayContextSnapshot`. Esta fatia prepara o decode JPEG dimensionado sem implementar ainda o viewer, cache pesado, Delete/Undo ou menu de contexto.

## Arquivos a ler

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.Application/Display/DisplayContextSnapshot.cs`
- `src/Evydencia.PhotoSelector.Imaging/Sizing/DecodeTargetCalculator.cs`
- `src/Evydencia.PhotoSelector.App/Composition/AppCompositionRoot.cs`

## Camada afetada

- App
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
- `Application` nao deve referenciar WinUI.

## Plano em passos pequenos

1. Criar `App/Display/WindowsDisplayContextService.cs`.
2. Capturar `XamlRoot.Size`, `XamlRoot.RasterizationScale` e `FrameworkElement.ActualWidth/ActualHeight`.
3. Retornar `DisplayContextSnapshot` com dimensoes em DIPs e pixels calculados pelo proprio snapshot.
4. Registrar o servico no composition root do App.
5. Validar build/test/format.
6. Atualizar progresso de execucao.

## Testes necessarios

- `tools/build.ps1`
- `tools/test.ps1`
- `tools/format.ps1`

## Criterio de aceite

- O servico existe no projeto `App`.
- `Application` continua sem referencia a WinUI.
- `DisplayContextSnapshot` continua sendo o contrato neutro usado por `DecodeTargetCalculator`.
- Build e testes passam.

## Riscos

- Captura de monitor fisico ainda nao identifica monitor real; a primeira versao usa `AppWindowId` como identificador pratico de janela. Monitor/display area detalhado deve entrar junto do fullscreen/segunda tela.
- Teste unitario direto de `XamlRoot` e `FrameworkElement` e limitado fora de UI automation. Nesta fatia, a validacao principal e build/test; smoke visual entra quando o viewer existir.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- `WindowsDisplayContextService` foi criado em `src/Evydencia.PhotoSelector.App/Display`.
- O servico captura `XamlRoot.Size`, `FrameworkElement.ActualWidth/ActualHeight`, `XamlRoot.RasterizationScale`, estado fullscreen recebido pelo chamador e `DisplayRole`.
- O resultado permanece como `DisplayContextSnapshot`, que vive em `Application` e nao depende de WinUI.
- O servico foi registrado no composition root do App.

Validacao executada:

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
```

Resultado da validacao:

- Build passou com 0 avisos e 0 erros.
- 52 testes passaram.
- Format check passou depois de normalizar finais de linha.

Decisao tecnica:

- A primeira versao identifica o contexto pela janela (`AppWindowId`). Monitor/display area detalhado deve entrar com fullscreen e segunda tela, quando o app tiver janela de cliente/operador e necessidade real de distinguir monitores.
- A proxima fatia tecnica recomendada e `IImagePreviewService`/decode JPEG dimensionado com stream curto e teste de file handle.
