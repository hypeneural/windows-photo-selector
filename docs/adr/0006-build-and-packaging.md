# ADR 0006 - Build and packaging governance

## Status

Proposed

## Context

Evydencia Escolher Fotos precisa validar duas stacks, manter versões consistentes, preparar MSIX/package identity e evitar comandos improvisados durante a implementação.

## Decision

Criar governança de solução na raiz:

- `global.json`;
- `Directory.Build.props`;
- `Directory.Build.targets`;
- `Directory.Packages.props`;
- `NuGet.config`;
- `.editorconfig`;
- `.gitignore`;
- `README.md`;
- `CHANGELOG.md`;
- scripts fixos em `/tools`.

Criar `/packaging` para:

- MSIX;
- manifest fragments;
- context menu;
- certificados de desenvolvimento.

Criar `/artifacts` para saídas geradas e ignorar no Git.

## Consequences

- A stack escolhida fica explícita e reproduzível.
- Versões NuGet são centralizadas.
- Propriedades MSBuild ficam iguais entre projetos.
- Empacotamento não polui `src`.
- Benchmarks, logs e pacotes não entram por acidente no repositório.

## Validation

- `dotnet --info` usa SDK esperado pelo `global.json`.
- `dotnet restore` respeita `Directory.Packages.props`.
- `dotnet build` aplica propriedades comuns.
- `dotnet format --verify-no-changes` funciona quando a solução existir.
- Pacotes gerados ficam em `/artifacts/packages`.
