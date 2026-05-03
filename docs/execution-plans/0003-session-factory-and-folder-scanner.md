# Execucao 0003 - Factory de sessao e scanner filesystem

## Objetivo

Criar a primeira ponte entre dominio puro e filesystem sem contaminar o `Core`. A fatia entrega `PhotoSessionFactory`, contrato `IFolderScanner` na camada `Application` e implementacao `FileSystemFolderScanner` em `Storage`, usando scan barato e JPEG-only.

## Arquivos que serao lidos

- `AGENTS.md`
- `src/Evydencia.PhotoSelector.Core/AGENTS.md`
- `src/Evydencia.PhotoSelector.Application/AGENTS.md`
- `src/Evydencia.PhotoSelector.Imaging/AGENTS.md`
- `tests/AGENTS.md`
- `.agents/skills/windows-photo-selector/references/layer-routing.md`
- `docs/execution-plans/0002-core-domain-minimum.md`

## Camada afetada

- Core
- Application
- Storage
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
- `Core` continua sem filesystem concreto.
- Scanner nao le EXIF, dimensoes, thumbnails, cache ou decode.

## Plano em passos pequenos

1. Criar `PhotoFileCandidate` em `Core/Scanning`.
2. Criar `PhotoSessionFactory` em `Core/Sessions`.
3. Criar `FolderOpenRequest` e `IFolderScanner` em `Application`.
4. Criar `FileSystemFolderScanner` em `Storage/Filesystem`.
5. Remover placeholder `Class1.cs` de `Application` e `Storage`.
6. Criar testes unitarios de `PhotoSessionFactory`.
7. Criar testes de filesystem em `Storage.Tests` com pasta temporaria.
8. Rodar build, testes e format.

## Testes necessarios

- Factory ordena fotos por nome inicialmente.
- Factory preserva dados baratos do candidate.
- Scanner lista `.jpg/.jpeg` case-insensitive.
- Scanner ignora arquivos nao JPEG.
- Scanner nao recursa subpastas.
- Scanner nao inclui `_deletadas_evydencia`.

## Criterio de aceite

- `Core` nao referencia `System.IO`, WinUI, Storage ou Application.
- `Storage` usa `Directory.EnumerateFiles`, nao `Directory.GetFiles`.
- Testes cobrem scanner e factory.
- `tools/build.ps1`, `tools/test.ps1` e `tools/format.ps1` passam.

## Riscos

- Perder progressividade ao ordenar por nome: manter scanner barato e deixar ordenacao inicial na factory.
- Introduzir metadata cedo: manter somente path, nome, extensao, tamanho, last write e sort index.
- Criar acoplamento de `Core` ao Windows: evitar chamadas concretas de IO no dominio.

## Comandos a rodar

```powershell
.\tools\build.ps1
.\tools\test.ps1 -Filter "FullyQualifiedName~Core"
.\tools\test.ps1 -Filter "FullyQualifiedName~Storage"
.\tools\test.ps1
.\tools\format.ps1
```

## Resultado

- [x] `PhotoFileCandidate` criado no `Core`.
- [x] `PhotoSessionFactory` criado no `Core`.
- [x] `FolderOpenRequest` criado em `Application`.
- [x] `IFolderScanner` criado em `Application`.
- [x] `FileSystemFolderScanner` criado em `Storage/Filesystem`.
- [x] Scanner usa `Directory.EnumerateFiles`.
- [x] Scanner lista apenas `.jpg/.jpeg`, case-insensitive.
- [x] Scanner nao recursa subpastas.
- [x] Scanner nao inclui `_deletadas_evydencia`.
- [x] Testes de Core e Storage adicionados.
- [x] Build, testes e format check executados com sucesso.

## Observacoes

- A ordenacao inicial por nome fica na `PhotoSessionFactory`, mantendo o scanner simples e barato.
- O scanner ainda nao le EXIF, dimensoes, thumbnails, ICC, cache key ou decode; isso fica para fases posteriores.
