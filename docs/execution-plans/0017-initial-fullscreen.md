# Execucao 0017 - Fullscreen limpo inicial

## Objetivo

Implementar o primeiro modo fullscreen do viewer WinUI usando `AppWindow`/`FullScreenPresenter` do Windows App SDK. A fatia deve permitir `F` para alternar fullscreen e `Esc` para sair, mantendo a tela escura, o viewer focado e recalculando o `DisplayContext` para o decode dimensionado.

Esta fatia nao implementa Delete/Undo, cache/prefetch, menu de contexto, single-instance, API, PDV, RAW, upload ou segunda tela.

## Arquivos que serao lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/image-pipeline.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/accessibility.instructions.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/performance.instructions.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/winui-best-practices.instructions.md`
- `src/Evydencia.PhotoSelector.App/App.xaml.cs`
- `src/Evydencia.PhotoSelector.App/Composition/AppCompositionRoot.cs`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `src/Evydencia.PhotoSelector.App/ViewModels/MainPageViewModel.cs`
- `src/Evydencia.PhotoSelector.App/Display/WindowsDisplayContextService.cs`
- `tests/Evydencia.PhotoSelector.UiSmokeTests/ViewModels/MainPageViewModelTests.cs`
- `docs/execution-progress.md`

Documentacao oficial consultada:

- Microsoft Learn, `AppWindow` e gerenciamento de janelas no Windows App SDK.
- Microsoft Learn, `FullScreenPresenter`.
- Microsoft Learn, recuperacao de HWND em WinUI 3.

## Camada afetada

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
- Sem excluir permanentemente por padrao.

## Plano em passos pequenos

1. Criar `FullscreenService` em `App/Windowing`, encapsulando `AppWindow` e `FullScreenPresenter`.
2. Registrar `FullscreenService` no composition root do app.
3. Expor a janela principal no `App` apenas para adaptacao WinUI do `MainPage`.
4. Adicionar estado `IsFullscreen` ao `MainPageViewModel`, sem tipos WinUI.
5. Capturar `F` e `Esc` em `OnViewerKeyDown`.
6. Ao alternar fullscreen, atualizar o ViewModel, manter foco no viewer e recarregar a foto atual com `DisplayContext` recalculado.
7. Passar o estado fullscreen real para `WindowsDisplayContextService.Capture`.
8. Adicionar teste de ViewModel para o estado fullscreen.
9. Atualizar `docs/execution-progress.md` e plano mestre.

## Testes necessarios

- UI smoke test do ViewModel: `SetFullscreen` atualiza o estado e nao altera sessao/foto.
- Build valida `FullscreenService`, XAML e code-behind com Windows App SDK 2.0.1.
- Teste manual futuro ainda sera necessario para validar transicao visual fullscreen em maquina com UI interativa.

## Criterio de aceite

- `F` alterna entre fullscreen e janela normal quando o viewer esta focado.
- `Esc` sai do fullscreen quando ativo.
- Fullscreen usa `AppWindow`/Windows App SDK, nao hacks de borda manual.
- `DisplayContextSnapshot.IsFullscreen` reflete o estado atual usado no decode.
- O decode anterior continua sendo cancelado antes do reload.
- Build, format e testes passam.
- `F0-04` e `F2-05` ficam marcados como concluidos.

## Riscos

- API de windowing variar por versao do Windows App SDK: mitigacao com consulta a Microsoft Learn e build local.
- `Esc` pode ser interceptado pelo sistema em alguns cenarios fullscreen: mitigacao mantendo tambem `F` como toggle.
- Recarregar imagem ao alternar fullscreen pode custar decode adicional: aceitavel nesta fatia para garantir target correto; cache/prefetch entram depois.
- Teste automatizado nao valida presenter real sem UI automation: mitigacao com build e registro da pendencia de smoke manual.

## Comandos a rodar

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~UiSmoke"
.\tools\build.ps1
.\tools\format.ps1
.\tools\test.ps1
.\tools\build.ps1
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

- Criado `FullscreenService` na camada App para encapsular `AppWindow` e `FullScreenPresenter`.
- `F` alterna fullscreen quando o viewer esta focado.
- `Esc` sai do fullscreen quando o presenter atual esta em fullscreen.
- `MainPageViewModel` ganhou estado puro `IsFullscreen`, sem dependencia WinUI.
- O reload da foto atual apos alternar fullscreen recaptura `DisplayContext` com `IsFullscreen=true/false`, mantendo o decode dimensionado.
- `ViewerHost` recebeu `AutomationProperties.AutomationId` e nome acessivel para futura UI automation.
- `F0-04` e `F2-05` foram marcados como concluidos em `docs/execution-progress.md`.

## Validacao executada

```powershell
.\tools\test.ps1 -Filter "FullyQualifiedName~UiSmoke"
.\tools\build.ps1
.\tools\format.ps1
.\tools\test.ps1
.\tools\build.ps1
```

Resultados:

- UiSmoke: 5 testes aprovados.
- Suite completa: 64 testes aprovados.
- Build completo: aprovado, 0 warnings.
- Format: aprovado apos normalizar CRLF dos arquivos alterados.

## Pendencias deliberadas

- Teste visual/manual de fullscreen real fica para smoke interativo, porque o presenter do Windows App SDK nao e validado por teste unitario.
- `Home`, `End`, `Delete` e `Ctrl+Z` ficam fora desta fatia.
- Single-instance continua pendente e deve vir antes do menu de contexto do Explorer.
- Cache/prefetch ainda nao foram implementados; alternar fullscreen ainda recarrega a imagem atual para garantir target correto.
