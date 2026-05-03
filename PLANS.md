# PLANS.md - Template de execucao Codex

Use este template antes de implementar qualquer tarefa nao trivial.

Tarefas triviais podem seguir direto quando forem leitura, ajuste de texto pequeno ou correcao mecanica sem risco de arquitetura. Para `FolderScanner`, `DeleteManager`, `JpegDecodeService`, `AppInstanceCoordinator`, ShellExtension, cache, prefetch, benchmarks e mudancas de camada, planeje primeiro.

## Objetivo

Explique em 2-4 frases o que sera entregue e qual comportamento observavel vai mudar.

## Arquivos que serao lidos

Liste os arquivos que precisam ser consultados antes da alteracao.

Inclua:

- `AGENTS.md`
- AGENTS especifico da subpasta, se existir
- documentacao em `docs/`
- testes relacionados
- codigo da camada afetada

## Camada afetada

Marque uma ou mais:

- App
- Core
- Imaging
- Storage
- Infrastructure
- Shell
- ShellExtension
- Launcher
- Contracts
- Tests
- Benchmarks
- Docs

## Restricoes da tarefa

Confirme:

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

Liste passos executaveis em ordem.

Cada passo deve ser pequeno o bastante para ser revisado isoladamente.

## Testes necessarios

Liste:

- testes unitarios;
- testes de integracao;
- UI smoke tests;
- benchmarks;
- casos manuais inevitaveis.

## Criterio de aceite

Defina exatamente quando a tarefa estara pronta.

Use criterios observaveis, como:

- comando executado com sucesso;
- teste novo falha antes e passa depois;
- benchmark gerado;
- journal contem evento esperado;
- arquivo movido/restaurado corretamente;
- UI nao bloqueia no caminho critico.

## Riscos

Liste riscos de:

- performance;
- file handle;
- threading;
- Windows App SDK;
- WinUI;
- Explorer/Shell;
- cache;
- perda de dados;
- UX.

Para cada risco, diga a mitigacao.

## Comandos a rodar

Liste build/test/benchmark relevantes.

Exemplos:

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Core"
.\tools\benchmarks.ps1
```

## Saida esperada no final

Ao finalizar a tarefa, reporte:

- arquivos alterados;
- camada alterada;
- testes adicionados/atualizados;
- comandos executados;
- impacto de performance;
- riscos restantes;
- desvios do plano ou de `AGENTS.md`.
