# Execucao 0001 - Governanca e skeleton inicial

## Objetivo

Iniciar a execucao do plano sem implementar o viewer ainda. Esta fatia cria governanca raiz, estrutura de pastas, scripts padronizados e checklist de progresso para permitir que as proximas fatias criem a solucao e os projetos de forma controlada.

## Arquivos lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `docs/adr/0006-build-and-packaging.md`

## Camada afetada

- Docs
- Benchmarks
- Tests
- Packaging
- Tools

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

1. Verificar disponibilidade do SDK .NET local.
2. Criar arquivos raiz de governanca.
3. Criar diretorios planejados para packaging, artifacts, fixtures e performance smoke.
4. Atualizar scripts para comandos previsiveis.
5. Criar checklist de progresso.
6. Rodar validacoes possiveis.

## Testes necessarios

- Validacao de estrutura por script.
- `dotnet --info`, `build` e `test` quando o SDK estiver disponivel.

## Criterio de aceite

- Arquivos raiz obrigatorios existem.
- Diretorios planejados existem.
- Scripts existem.
- Checklist marca F1-01 como concluido.
- Bloqueio de SDK, se existir, fica documentado.

## Riscos

- SDK .NET ausente: registrar bloqueio e nao instalar sem aprovacao.
- Criar projeto WinUI quebrado sem template: adiar ate validar tooling.
- Repositorios locais serem versionados por acidente: ignorar `repositorios/`.

## Comandos a rodar

```powershell
.\.agents\skills\windows-photo-selector\scripts\validate-solution.ps1
.\tools\build.ps1
.\tools\test.ps1
.\tools\benchmarks.ps1
```

## Resultado

- [x] SDK .NET 10 instalado via `winget`.
- [x] `Microsoft.WindowsAppSDK` 2.0.1 validado por restore/build.
- [x] Template WinUI oficial instalado para scaffold do projeto App.
- [x] Solucao `Evydencia.PhotoSelector.sln` criada no formato classico `.sln`.
- [x] Projetos base criados em `/src`, `/tests` e `/benchmarks`.
- [x] Referencias entre camadas configuradas.
- [x] Central Package Management ajustado em `Directory.Packages.props`.
- [x] Scripts ajustados para localizar `dotnet.exe` mesmo quando o processo atual nao herdou o PATH.
- [x] Build, testes, format check e benchmark placeholder executados com sucesso.

## Observacoes

- O projeto `ShellExtension` permanece sem `.vcxproj` nesta fatia. Ele deve ser criado na fase de menu de contexto, quando a decisao C++/WinRT, MSIX e `IExplorerCommand` for executada.
- O projeto `Evydencia.PhotoSelector.Benchmarks` ainda e placeholder; benchmarks reais entram na fase de performance.
