# Execucao 0010 - Benchmarks iniciais e scanner com FileInfo pre-populado

## Objetivo

Transformar os benchmarks de placeholder em medicoes reais e aplicar uma melhoria segura no scanner antes de implementar o viewer de imagem. A fatia tambem registra a decisao de nao refatorar abertura progressiva completa antes de medir o custo real do scan ordenado por nome.

## Arquivos lidos

- `AGENTS.md`
- `PLANS.md`
- `.agents/skills/windows-photo-selector/SKILL.md`
- `.agents/skills/windows-photo-selector/references/benchmarks.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `docs/execution-progress.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`
- `src/Evydencia.PhotoSelector.Storage/Filesystem/FileSystemFolderScanner.cs`
- `benchmarks/Evydencia.PhotoSelector.Benchmarks/Program.cs`
- `tools/benchmarks.ps1`

## Camada afetada

- Storage
- Benchmarks
- Tools
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

1. Trocar `FileSystemFolderScanner` para `DirectoryInfo.EnumerateFiles`, aproveitando `FileInfo` pre-populado.
2. Adicionar BenchmarkDotNet ao projeto de benchmarks via Central Package Management.
3. Criar benchmarks para scan 500/2000 JPEGs.
4. Criar benchmark para `OpenSessionUseCase` com scanner em memoria 500/2000 itens.
5. Criar benchmark para `DecodeTargetCalculator`.
6. Atualizar `tools/benchmarks.ps1` para repassar argumentos ao BenchmarkDotNet.
7. Atualizar plano tecnico e progresso.
8. Rodar build, testes, format check e benchmarks filtrados de validacao.

## Testes necessarios

- `tools/build.ps1`
- `tools/test.ps1`
- `tools/format.ps1`
- benchmark filtrado para `DecodeTargetCalculatorBenchmarks`
- benchmark filtrado para `FolderScanBenchmarks`
- benchmark filtrado para `OpenSessionUseCaseBenchmarks`

## Criterio de aceite

- Scanner continua listando somente JPEGs de topo e ignorando `_deletadas_evydencia`.
- Benchmarks reais existem e podem ser listados/executados.
- BenchmarkDotNet salva artefatos em `artifacts/performance`.
- Build e testes passam sem warnings.

## Riscos

- Benchmark de IO ficar lento demais para a rotina normal: mitigar deixando argumentos filtraveis no script.
- Refatorar scanner e alterar sem querer semantica de filtros: mitigar mantendo testes de Storage existentes.
- Abrir progressivo quebrar ordenacao por nome: adiar refatoracao ate benchmark indicar necessidade e criar ADR/issue especifica.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
.\tools\benchmarks.ps1 '--filter' '*DecodeTargetCalculatorBenchmarks*' '--join'
```

## Resultado

- `FileSystemFolderScanner` passou a usar `DirectoryInfo.EnumerateFiles`, aproveitando `FileInfo` pre-populado para reduzir chamadas extras de filesystem no caminho de scan.
- `BenchmarkDotNet` foi adicionado ao projeto de benchmarks via Central Package Management.
- `tools/benchmarks.ps1` agora repassa argumentos ao BenchmarkDotNet, permitindo execucoes filtradas.
- Artefatos sao exportados em `artifacts/performance/results`.

Benchmarks executados em 2026-05-03:

| Benchmark | Cenario | Media | Alocacao |
|---|---:|---:|---:|
| `DecodeTargetCalculatorBenchmarks` | EXIF 1 | 115,5 ns | 112 B |
| `DecodeTargetCalculatorBenchmarks` | EXIF 6 | 118,2 ns | 112 B |
| `FolderScanBenchmarks` | 500 JPEGs | 1,486 ms | 591,58 KB |
| `FolderScanBenchmarks` | 2.000 JPEGs | 5,492 ms | 2.361,12 KB |
| `OpenSessionUseCaseBenchmarks` | 500 candidatos em memoria | 380,1 us | 174,12 KB |
| `OpenSessionUseCaseBenchmarks` | 2.000 candidatos em memoria | 1,937 ms | 685,23 KB |

Validacao executada:

```powershell
.\tools\build.ps1
.\tools\test.ps1
.\tools\format.ps1
.\tools\benchmarks.ps1 '--filter' '*DecodeTargetCalculatorBenchmarks*' '--join'
.\tools\benchmarks.ps1 '--filter' '*FolderScanBenchmarks*' '--join'
.\tools\benchmarks.ps1 '--filter' '*OpenSessionUseCaseBenchmarks*' '--join'
```

Decisao tecnica:

- Abertura progressiva completa ainda nao deve ser implementada nesta fatia. Nos cenarios sinteticos, scan e criacao de sessao para 2.000 itens ficaram abaixo de 10 ms somados. A proxima prioridade real do viewer e `DisplayContext` WinUI, decode JPEG dimensionado, EXIF/file handle e exibicao da primeira foto.
- A decisao deve ser reavaliada com JPEGs reais de estudio e disco mais lento. Se time-to-first-image ficar ruim depois do decode real, criar uma fatia propria para `SessionOpenHandle` ou abertura em batches.
