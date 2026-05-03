# Execucao 0007 - Prototipo do argumento --folder

## Objetivo

Implementar a primeira fatia de ativacao por pasta sem integrar ainda com Explorer, single-instance ou abertura automatica de sessao. A entrega cria um parser testavel para argumentos de linha de comando e faz o projeto WinUI capturar o resultado de `--folder`, preparando o caminho para o futuro launcher/context menu.

## Arquivos lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `src/Evydencia.PhotoSelector.Application/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/AGENTS.md`
- `src/Evydencia.PhotoSelector.App/App.xaml.cs`
- `src/Evydencia.PhotoSelector.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`
- `docs/execution-progress.md`

## Camada afetada

- App
- Application
- Infrastructure
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

1. Criar modelo imutavel para argumentos de launch com `FolderPath` e `Source`.
2. Criar parser em `Application` que suporte `--folder "C:\path"`, `--folder=C:\path`, `/folder "C:\path"` e `--source explorer`.
3. Criar tokenizador simples para string raw de launch com suporte a aspas.
4. Registrar o parser no DI.
5. Atualizar `App.xaml.cs` para usar `Environment.GetCommandLineArgs()` e armazenar os argumentos parseados em propriedade publica.
6. Adicionar testes unitarios em `Application.Tests`.
7. Atualizar `execution-progress.md`.

## Testes necessarios

- Testes unitarios do parser para:
  - argumento vazio;
  - `--folder` separado;
  - `--folder=` inline;
  - caminho com espaco entre aspas;
  - `/folder`;
  - `--source explorer`;
  - argumento desconhecido ignorado.
- Build completo.
- Testes completos.
- Format check.

## Criterio de aceite

- O parser retorna `FolderPath` normalizado para a string recebida, sem acessar filesystem.
- O parser nao fica na camada WinUI.
- O App captura `Environment.GetCommandLineArgs()` e guarda o resultado sem abrir sessao ainda.
- O DI consegue resolver o parser.
- Build, testes e format check passam.

## Riscos

- Parsing de comando Windows completo e complexo: mitigar cobrindo o formato que o Launcher/context menu vai emitir e deixando comentarios/documentacao para evoluir.
- `LaunchActivatedEventArgs.Arguments` nao funcionar em app desktop WinUI: mitigar usando `Environment.GetCommandLineArgs()`, conforme documentacao oficial do Windows App SDK.
- Iniciar abertura de sessao cedo demais no `OnLaunched`: mitigar apenas armazenando a ativacao nesta fatia.
- Acoplar `Application` a WinUI: mitigar usando string e tipos simples.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `FolderLaunchArguments` criado como modelo simples de ativacao.
- [x] `FolderLaunchArgumentsParser` criado em `Application`, sem dependencia de WinUI ou filesystem.
- [x] Parser cobre `--folder`, `--folder=`, `/folder`, `/folder=`, `/folder:` e `--source`.
- [x] `App.xaml.cs` usa `Environment.GetCommandLineArgs().Skip(1)` para capturar argumentos de app desktop WinUI.
- [x] Parser registrado no DI.
- [x] Teste de integracao atualizado para validar resolucao do parser.
- [x] `Application.Tests` recebeu 8 testes do parser.
- [x] `tools/build.ps1` executado com sucesso, sem warnings.
- [x] `tools/test.ps1` executado com sucesso, 47 testes passando.
- [x] `tools/format.ps1` executado com sucesso.

## Observacoes

- A documentacao oficial do Windows App SDK indica que `LaunchActivatedEventArgs.Arguments` nao e suportado para apps desktop e retorna string vazia. Por isso a captura real ficou em `Environment.GetCommandLineArgs()`.
- Esta fatia nao abre sessao automaticamente. Abertura real por argumento entra na proxima fatia do fluxo do viewer/ativacao.
