# Execucao 0009 - Estado inicial da sessao na UI

## Objetivo

Exibir na tela inicial o estado da abertura por `--folder`, sem renderizar imagem ainda. Esta fatia cria um ViewModel fino no projeto WinUI, usa `x:Bind` e mostra se a sessao foi carregada, se nao houve pasta ou se ocorreu falha.

## Arquivos lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/design-principles.instructions.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/accessibility.instructions.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/performance.instructions.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/code-quality.instructions.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/winui-best-practices.instructions.md`
- `src/Evydencia.PhotoSelector.App/.github/instructions/testing.instructions.md`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml`
- `src/Evydencia.PhotoSelector.App/MainPage.xaml.cs`
- `docs/execution-progress.md`

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

1. Criar `MainPageViewModel` com estado de carregamento inicial.
2. Atualizar `MainPage.xaml` para usar `x:Bind` e tema WinUI.
3. Atualizar `MainPage.xaml.cs` para aguardar `InitialSessionOpenTask` sem bloquear o construtor.
4. Referenciar o projeto App em `UiSmokeTests`.
5. Trocar o teste placeholder por testes do ViewModel.
6. Atualizar `execution-progress.md`.

## Testes necessarios

- Testes de ViewModel para sem pasta, sessao aberta e falha.
- Build completo.
- Testes completos.
- Format check.

## Criterio de aceite

- UI inicial compila e usa `x:Bind`.
- O ViewModel nao abre pasta, nao faz scan e nao referencia WinUI.
- A pagina atualiza estado a partir de `InitialSessionOpenTask`.
- Build, testes e format check passam.

## Riscos

- Bloquear UI aguardando scan: mitigar aguardando task no evento `Loaded`.
- Criar ViewModel gordo: mitigar deixando ele apenas mapear resultado para texto de UI.
- Teste WinUI instanciar controles em ambiente unitario: mitigar testando somente o ViewModel.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `MainPageViewModel` criado no projeto App.
- [x] `MainPage.xaml` substituido por tela inicial simples com `x:Bind`.
- [x] `MainPage.xaml.cs` aguarda `InitialSessionOpenTask` no evento `Loaded`, sem bloquear o construtor.
- [x] UI mostra estado sem pasta, sessao carregada, contador de JPEGs e falha de abertura.
- [x] Teste placeholder de `UiSmokeTests` removido.
- [x] `UiSmokeTests` agora valida o ViewModel em 3 cenarios.
- [x] `tools/build.ps1` executado com sucesso, sem warnings.
- [x] `tools/test.ps1` executado com sucesso, 52 testes passando.
- [x] `tools/format.ps1` executado com sucesso.

## Observacoes

- A primeira tentativa de testar referenciando diretamente o projeto WinUI falhou por carregamento do assembly empacotado. A correcao foi compilar o arquivo do ViewModel como link no projeto de teste, mantendo a logica testada sem depender do executavel MSIX.
- Esta fatia ainda nao renderiza JPEG. Ela apenas fecha o caminho de argumento `--folder` ate uma tela inicial que sabe informar o estado da sessao.
