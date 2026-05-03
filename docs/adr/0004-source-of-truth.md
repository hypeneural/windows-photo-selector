# ADR 0004 - Source of truth

## Status

Proposed

## Context

O operador pode mexer nos arquivos fora do app. O app tambem precisa recuperar estado depois de crash e manter auditoria de exclusoes/restauracoes.

## Decision

Filesystem + journal JSONL sao a fonte da verdade.

SQLite e estado derivado para:

- sessoes recentes;
- cache index;
- settings;
- metricas;
- aceleracao de abertura.

Ao reabrir sessao:

1. escanear pasta original;
2. escanear `_deletadas_evydencia`;
3. replay do journal;
4. reconciliar com filesystem;
5. marcar arquivos ausentes como `Missing`.

## Consequences

- SQLite pode ser reconstruido.
- Journal deve ser append-only.
- Conflitos devem ser logados e resolvidos privilegiando realidade do filesystem.

## Validation

- Sessao antiga reabre com estado correto.
- Arquivo movido fora do app vira `Missing` ou estado reconciliado.
- SQLite divergente nao corrompe sessao.
