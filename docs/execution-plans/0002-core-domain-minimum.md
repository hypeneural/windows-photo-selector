# Execucao 0002 - Dominio minimo de sessao e navegacao

## Objetivo

Criar o dominio minimo para representar uma sessao local de selecao JPEG sem tocar em WinUI, decode, cache, delete fisico ou menu de contexto. Esta fatia entrega modelos e regras puras para fotos, sessao, navegacao e politica de scan, com testes unitarios em `Core.Tests`.

## Arquivos que serao lidos

- `AGENTS.md`
- `src/Evydencia.PhotoSelector.Core/AGENTS.md`
- `tests/AGENTS.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `docs/evydencia-escolher-fotos-plano-implementacao.md`

## Camada afetada

- Core
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
- Sem filesystem concreto no `Core`.
- Sem WinUI, WIC, SQLite, Serilog ou Shell no `Core`.

## Plano em passos pequenos

1. Criar `PhotoStatus`.
2. Criar `PhotoItem` com dados baratos de scan e metadados opcionais.
3. Criar `PhotoSession` com contadores derivados.
4. Criar `NavigationController` para primeira, ultima, proxima, anterior e remocao logica.
5. Criar `FolderScanPolicy` para aceitar `.jpg/.jpeg` e ignorar `_deletadas_evydencia`.
6. Remover arquivos `Class1.cs` gerados pelo template nos projetos tocados.
7. Criar testes unitarios de dominio.
8. Atualizar checklist.

## Testes necessarios

- `PhotoSession` calcula total inicial, ativos e deletadas.
- `NavigationController` navega para proxima/anterior respeitando fotos ativas.
- `NavigationController` trata inicio/fim sem estourar indice.
- `FolderScanPolicy` aceita `.jpg/.jpeg` case-insensitive.
- `FolderScanPolicy` rejeita `_deletadas_evydencia` e extensoes nao JPEG.

## Criterio de aceite

- `Core` continua sem dependencias de infra/UI.
- Testes de `Core.Tests` cobrem as regras adicionadas.
- `tools/build.ps1`, `tools/test.ps1` e `tools/format.ps1` passam.

## Riscos

- Modelar estado demais cedo: manter apenas o necessario para F1.
- Acoplar `Core` ao filesystem: usar strings/politicas puras, nao `Directory.EnumerateFiles`.
- Criar delete fisico cedo: deixar para fatia posterior.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Core"
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `PhotoStatus` criado com estados `PendingDelete`, `PendingRestore`, `DeleteFailed` e demais estados planejados.
- [x] `PhotoItem` criado com dados baratos de scan, metadados opcionais e status de navegacao.
- [x] `PhotoSession` criado com contadores derivados e ordenacao por `SortIndex`.
- [x] `NavigationController` criado para primeira, ultima, proxima, anterior e remocao logica da foto atual.
- [x] `FolderScanPolicy` criado para JPEG-only e ignorar `_deletadas_evydencia`.
- [x] Placeholders `Class1.cs` e `Test1.cs` removidos do Core/Core.Tests.
- [x] Testes unitarios de Core criados.
- [x] Build, testes e format check executados com sucesso.

## Observacoes

- Esta fatia nao implementa delete fisico, journal, filesystem scanner, decode, cache ou UI.
- `DeleteFailed` permanece disponivel para navegacao para permitir reinsercao visual segura quando o move fisico falhar em fatia futura.
