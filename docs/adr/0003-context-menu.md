# ADR 0003 - Windows context menu

## Status

Proposed

## Context

O operador precisa abrir o app diretamente de uma pasta no Windows Explorer usando `Abrir Escolher Fotos`.

## Decision

Separar tres modos:

1. Desenvolvimento rapido: registro HKCU classico para testar `--folder`.
2. Produto instalavel interno: MSIX assinado, package identity e `IExplorerCommand`.
3. Produto profissional futuro: MSIX ou sparse package, certificado confiavel, auto-update e logs de ativacao Shell.

Adicionar `Evydencia.PhotoSelector.Launcher` como broker leve. ShellExtension/Launcher apenas valida caminho e ativa a instancia principal.

## Consequences

- ShellExtension nao depende de Imaging nem de regras pesadas.
- Menu moderno do Windows 11 fica ligado a package identity.
- HKCU classico e apenas fallback de desenvolvimento.

## Validation

- Clique sobre pasta passa a pasta correta.
- Clique no fundo da pasta passa a pasta atual.
- Multiplos caminhos sao tratados explicitamente.
- Ativacao repetida nao abre multiplas instancias acidentais.
