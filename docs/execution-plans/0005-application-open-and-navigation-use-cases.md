# Execucao 0005 - Use cases de abertura e navegacao

## Objetivo

Criar os primeiros casos de uso reais em `Application` para abrir uma sessao a partir de `IFolderScanner` e navegar entre fotos sem colocar orquestracao em ViewModels. Esta fatia ainda nao implementa delete, undo, UI, decode, cache ou journal.

## Arquivos que serao lidos

- `AGENTS.md`
- `src/Evydencia.PhotoSelector.Application/AGENTS.md`
- `src/Evydencia.PhotoSelector.Core/AGENTS.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `docs/execution-plans/0003-session-factory-and-folder-scanner.md`

## Camada afetada

- Core
- Application
- Tests
- Docs

## Restricoes da tarefa

- V1 continua offline.
- Sem API Laravel.
- Sem PDV.
- Sem upload.
- Sem RAW.
- Sem Electron/WebView.
- Sem WinUI nesta fatia.
- Sem filesystem concreto em `Application`.
- Sem delete/undo fisico.
- Sem decode/cache.

## Plano em passos pequenos

1. Ajustar `NavigationController` para respeitar `PhotoSession.CurrentIndex`.
2. Criar `OpenSessionCommand` e `OpenSessionResult`.
3. Criar `NavigationResult`.
4. Criar `OpenSessionUseCase`.
5. Criar `NavigateNextPhotoUseCase`.
6. Criar `NavigatePreviousPhotoUseCase`.
7. Criar fakes de scanner em `Application.Tests`.
8. Testar abertura de sessao e navegacao com fakes.
9. Rodar build, testes e format.

## Testes necessarios

- `OpenSessionUseCase` cria sessao com fotos retornadas pelo scanner.
- `OpenSessionUseCase` retorna primeira foto atual.
- `NavigateNextPhotoUseCase` atualiza `CurrentIndex`.
- `NavigatePreviousPhotoUseCase` respeita indice atual e limites.

## Criterio de aceite

- `Application` orquestra por interfaces e nao referencia WinUI nem filesystem.
- ViewModels futuros poderao chamar use cases sem conhecer scanner concreto.
- Build, testes e format passam sem avisos.

## Riscos

- Criar estado duplicado entre `PhotoSession` e `NavigationController`: `CurrentIndex` fica em `PhotoSession`, controlador apenas aplica regra.
- Fazer scanner concreto vazar para Application: usar apenas `IFolderScanner`.
- Implementar delete cedo: manter fora desta fatia.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Application"
.\tools\test.ps1 -Filter "FullyQualifiedName~Core"
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `NavigationController` ajustado para respeitar `PhotoSession.CurrentIndex`.
- [x] `OpenSessionCommand` criado.
- [x] `OpenSessionResult` criado.
- [x] `NavigationResult` criado.
- [x] `OpenSessionUseCase` criado.
- [x] `NavigateNextPhotoUseCase` criado.
- [x] `NavigatePreviousPhotoUseCase` criado.
- [x] Fake de `IFolderScanner` criado em `Application.Tests`.
- [x] Testes de abertura e navegacao criados.
- [x] Build, testes e format check executados com sucesso.

## Observacoes

- Esta fatia nao implementa delete, undo, filesystem direto em Application, decode, cache, UI ou journal.
- `F1-11` permanece aberto porque a issue completa exige tambem fluxos de delete e undo com fakes, que pertencem a uma fatia posterior.
