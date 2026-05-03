# ADR 0001 - Stack version

## Status

Proposed

## Context

Evydencia Escolher Fotos sera um app Windows nativo novo, com expectativa de evolucao por anos. A V1 precisa validar WinUI 3, Windows App SDK, empacotamento, MSIX, single-instance e menu de contexto.

## Decision

Fase 0 deve validar:

- Opcao A, fallback conservador: `.NET 8` + `Windows App SDK 1.8`.
- Opcao B, preferida para produto novo em 2026: `.NET 10 LTS` + `Windows App SDK 2.0.1`.

Adotar a Opcao B se compilar, empacotar, rodar, suportar WinUI 3, AppLifecycle/AppInstance e estrategia Shell sem bloqueios relevantes no ambiente.

## Consequences

- A decisao final so deve ser fechada apos spike tecnico.
- Se a Opcao A for usada, registrar o bloqueio da Opcao B e a data da decisao.
- O viewer deve ficar isolado o suficiente para reduzir custo de upgrade de stack.

## Validation

- WinUI app compila.
- App roda empacotado e, se necessario, unpackaged.
- MSIX funciona.
- Single-instance redireciona ativacao.
- Context menu consegue passar pasta para o app.
