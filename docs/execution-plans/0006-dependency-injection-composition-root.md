# Execucao 0006 - Dependency injection e composition root

## Objetivo

Criar a primeira composition root do app sem implementar UI real do viewer. Esta fatia registra os servicos ja existentes de Core, Application, Storage e Imaging em um `IServiceProvider` unico, para que o projeto WinUI consiga resolver os casos de uso sem colocar orquestracao nos ViewModels.

## Arquivos lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `docs/execution-progress.md`
- `docs/execution-plans/0001-governance-and-skeleton.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/App.xaml.cs`
- `src/Evydencia.PhotoSelector.Infrastructure/Evydencia.PhotoSelector.Infrastructure.csproj`
- `Directory.Packages.props`

## Camada afetada

- App
- Infrastructure
- Storage
- Imaging
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
- Sem bloquear UI thread.
- Sem quebrar separacao de camadas.
- Sem excluir permanentemente por padrao.

## Plano em passos pequenos

1. Adicionar o pacote oficial `Microsoft.Extensions.DependencyInjection` via Central Package Management.
2. Dar ao projeto `Infrastructure` as referencias necessarias para registrar implementacoes de `Storage` e `Imaging`.
3. Criar extensoes de DI em `Infrastructure/DependencyInjection`.
4. Criar uma factory de `IServiceProvider` validando escopos no build.
5. Criar `AppCompositionRoot` no projeto WinUI.
6. Inicializar `Services` no `App.xaml.cs` sem criar nova funcionalidade de UI.
7. Adicionar teste de integracao garantindo que os principais servicos resolvem.
8. Atualizar `execution-progress.md`.

## Testes necessarios

- Teste de integracao para resolver:
  - `OpenSessionUseCase`;
  - `NavigateNextPhotoUseCase`;
  - `NavigatePreviousPhotoUseCase`;
  - `IFolderScanner`;
  - `DecodeTargetCalculator`.
- Build completo.
- Testes completos.
- Format check.

## Criterio de aceite

- `Infrastructure` expoe registro de DI sem depender de WinUI.
- `App` cria um `IServiceProvider` centralizado.
- `IFolderScanner` resolve para `FileSystemFolderScanner`.
- Use cases de abertura e navegacao resolvem pelo container.
- `DecodeTargetCalculator` resolve pelo container.
- Build, testes e format check passam.

## Riscos

- Dependencia circular entre projetos: mitigar mantendo `Application` sem referencia a `Infrastructure` e registrando implementacoes somente para fora.
- Composition root instanciar servicos pesados no startup: mitigar usando registros transientes/singletons leves sem executar scan/decode.
- Criar dependencia de WinUI em `Infrastructure`: mitigar deixando WinUI apenas no projeto `App`.
- Versao de pacote desatualizada: mitigar consultando NuGet oficial antes de fixar a versao central.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `Microsoft.Extensions.DependencyInjection` fixado via Central Package Management.
- [x] `Infrastructure` ajustado para `net10.0-windows10.0.19041.0`, porque registra o projeto `Imaging`, que e Windows-targeted.
- [x] `ServiceCollectionExtensions` criado para registrar servicos da fatia atual.
- [x] `EvydenciaServiceProviderFactory` criado com validacao de build e escopos.
- [x] `AppCompositionRoot` criado no projeto WinUI.
- [x] `App.xaml.cs` inicializa `Services` sem abrir sessao, scan ou decode.
- [x] Teste de integracao valida resolucao de use cases, scanner e `DecodeTargetCalculator`.
- [x] `tools/build.ps1` executado com sucesso, sem warnings.
- [x] `tools/test.ps1` executado com sucesso, 39 testes passando.
- [x] `tools/format.ps1` executado com sucesso.

## Observacoes

- A primeira tentativa de build revelou incompatibilidade de TFM: `Infrastructure` estava `net10.0` e passou a referenciar `Imaging`, que e `net10.0-windows10.0.19041.0`. A correcao manteve `Core` e `Application` puros em `net10.0`.
- A composition root nao instancia trabalho pesado no startup; ela apenas registra servicos leves e cria o provider.
